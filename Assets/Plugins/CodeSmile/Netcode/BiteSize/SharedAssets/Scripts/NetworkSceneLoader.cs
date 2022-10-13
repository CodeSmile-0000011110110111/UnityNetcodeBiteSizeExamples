using System;
using System.Collections;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CodeSmile.Netcode.BiteSize.SceneManagement
{
	public class NetworkSceneLoader : NetworkBehaviour
	{
		public enum LoadEvent
		{
			OnNetworkSpawn,
			OnNetworkDespawn,
		}

		[Tooltip("Determines on which Unity event method the scene will be loaded.")]
		[SerializeField] private LoadEvent _loadEvent;
		[Tooltip("Time in seconds to wait before loading the scene.")]
		[SerializeField] private float _loadSceneDelay;

		private string _onEventSceneName;
		private string _onDemandSceneName;
		private string[] _additiveSceneNames;
		private bool _quitting;

		private ConnectionManager _connectionManager;

		private void Awake()
		{
			_connectionManager = FindObjectOfType<ConnectionManager>();
		}

		public override void OnDestroy()
		{
			RemoveNetworkCallbacks();
			base.OnDestroy();
		}

		private void OnApplicationQuit() => _quitting = true;

		private void AddNetworkCallbacks()
		{
			if (NetworkManager?.SceneManager != null)
			{
				// ensure that we never register twice in case this method is called more than once 
				RemoveNetworkCallbacks();
				NetworkManager.SceneManager.OnSceneEvent += OnSceneEvent;
				_connectionManager.OnNetworkShutdown += OnNetworkShutdown;
			}
		}

		private void RemoveNetworkCallbacks()
		{
			if (NetworkObject != null && NetworkManager?.SceneManager != null)
			{
				NetworkManager.SceneManager.OnSceneEvent -= OnSceneEvent;
				_connectionManager.OnNetworkShutdown -= OnNetworkShutdown;
			}
		}

		/// <summary>
		/// Receives all scene loading events. This just logs what's happening.
		/// </summary>
		/// <param name="ev"></param>
		private void OnSceneEvent(SceneEvent ev)
		{
			Debug.Log($"OnSceneEvent: {ev.SceneName}, {ev.SceneEventType}, {ev.LoadSceneMode}, clientId: {ev.ClientId}");
			if (ev.ClientsThatCompleted != null)
			{
				foreach (var clientId in ev.ClientsThatCompleted)
					Debug.Log($"\tclient {clientId} completed scene load");
			}
			if (ev.ClientsThatTimedOut != null)
			{
				foreach (var clientId in ev.ClientsThatTimedOut)
					Debug.LogWarning($"\tclient {clientId} timed out");
			}
		}

		public override void OnNetworkSpawn()
		{
			AddNetworkCallbacks();
			if (_loadEvent == LoadEvent.OnNetworkSpawn)
				TryLoadScene();
		}

		public override void OnNetworkDespawn()
		{
			if (_loadEvent == LoadEvent.OnNetworkDespawn)
				TryLoadScene();
		}

		private void OnNetworkShutdown()
		{
			if (IsServer == false)
			{
				Debug.Log($"Load scene on client shutdown: {_onEventSceneName}");
				if (_loadEvent == LoadEvent.OnNetworkDespawn)
					OnEventLoadSceneNonNetworked();
			}
		}

		public void LoadSceneAdditive(int sceneIndex)
		{
			if (IsServer && NetworkManager.IsListening)
			{
				if (_additiveSceneNames == null || _additiveSceneNames.Length == 0)
				{
					Net.LogError("NetworkSceneLoader has not additive scenes assigned");
					return;
				}

				if (sceneIndex < 0 || sceneIndex >= _additiveSceneNames.Length)
				{
					Net.LogError($"NetworkSceneLoader should load scene {sceneIndex} but has only {_additiveSceneNames.Length} scenes");
					return;
				}

				if (SceneManager.sceneCount > 9)
					NetworkManager.SceneManager.UnloadScene(SceneManager.GetSceneAt(1));

				var sceneName = _additiveSceneNames[sceneIndex];
				Net.LogInfo($"Try load scene additive: {sceneName}");

				var status = NetworkManager.SceneManager?.LoadScene(sceneName, LoadSceneMode.Additive);
				if (status != SceneEventProgressStatus.Started)
					Net.LogWarning($"Failed to add Scene '{_onEventSceneName}' => {status}");
			}
		}

		public void LoadSceneSingleOnDemand()
		{
			if (IsServer && NetworkManager.IsListening)
			{
				if (_onDemandSceneName == null)
				{
					Net.LogError("NetworkSceneLoader has no on-demand scene assigned");
					return;
				}

				Net.LogInfo($"Try load scene on demand: {_onDemandSceneName}");
				var status = NetworkManager.SceneManager?.LoadScene(_onDemandSceneName, LoadSceneMode.Single);
				if (status != SceneEventProgressStatus.Started)
					Net.LogWarning($"Failed to load Scene '{_onDemandSceneName}' => {status}");
			}
		}

		private void TryLoadScene()
		{
			if (IsServer && _quitting == false)
			{
				if (string.IsNullOrWhiteSpace(_onEventSceneName))
					throw new Exception("invalid scene name");

				StartCoroutine(OnEventLoadSceneAfterDelay(_loadSceneDelay));
			}
		}

		private void OnEventLoadSceneNonNetworked()
		{
			RemoveNetworkCallbacks();
			SceneManager.LoadScene(_onEventSceneName);
		}

		private void OnEventLoadScene()
		{
			Net.LogInfo($"NetworkSceneLoader - OnEventLoadScene: {_onEventSceneName}");

			// if despawned due to server shutting down we must load the scene using non-networked SceneManager
			if (NetworkManager.ShutdownInProgress || NetworkManager.IsListening == false)
				OnEventLoadSceneNonNetworked();
			else
			{
				var status = NetworkManager.SceneManager?.LoadScene(_onEventSceneName, LoadSceneMode.Single);
				if (status != SceneEventProgressStatus.Started)
				{
					Net.LogWarning($"Failed to load Scene '{_onEventSceneName}' => {status} - will try again ...");
					// single scene load should not fail, thus try again soon:
					StartCoroutine(OnEventLoadSceneAfterDelay(0.2f));
				}
			}
		}

		private IEnumerator OnEventLoadSceneAfterDelay(float delay)
		{
			if (delay > 0f)
				yield return new WaitForSeconds(delay);
			else
				yield return new WaitForEndOfFrame();

			OnEventLoadScene();
		}

#if UNITY_EDITOR
		[SerializeField] private SceneAsset _sceneToLoadOnEvent;
		[SerializeField] private SceneAsset _sceneToLoadOnDemand;
		[SerializeField] private SceneAsset[] _additiveScenesToLoad;
		private void OnValidate()
		{
			_onEventSceneName = _sceneToLoadOnEvent != null ? _sceneToLoadOnEvent.name : null;
			_onDemandSceneName = _sceneToLoadOnDemand != null ? _sceneToLoadOnDemand.name : null;

			if (_additiveScenesToLoad != null)
			{
				if (_additiveSceneNames == null || _additiveSceneNames.Length != _additiveScenesToLoad.Length)
					_additiveSceneNames = new string[_additiveScenesToLoad.Length];

				for (var i = 0; i < _additiveScenesToLoad.Length; i++)
					if (_additiveScenesToLoad[i] != null)
						_additiveSceneNames[i] = _additiveScenesToLoad[i].name;
			}
		}
#endif
	}
}