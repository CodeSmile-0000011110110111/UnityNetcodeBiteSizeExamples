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

		private void Update()
		{
			// HACK for when user clicks Disconnect button since NetworkManager does not have a OnShutdown event
			// and since client disconnects intentionally, Netcode won't call OnClientDisconnect either assuming user code handles that
			// well, this handles it without resorting to adding an event handler for the Disconnect button:
			if (IsClientDisconnected())
				LoadSceneWhenDisconnected();
		}

		public override void OnDestroy()
		{
			//Debug.Log($"{name} - OnDestroy()");
			RemoveNetworkSceneManagerCallbacks();
			base.OnDestroy();
		}

		private void OnApplicationQuit() => _quitting = true;

		private void AddNetworkSceneManagerCallbacks()
		{
			if (NetworkManager?.SceneManager != null)
			{
				// ensure that we never register twice in case this method is called more than once 
				RemoveNetworkSceneManagerCallbacks();
				NetworkManager.SceneManager.OnSceneEvent += OnSceneEvent;
			}
		}

		private void RemoveNetworkSceneManagerCallbacks()
		{
			if (NetworkObject != null && NetworkManager?.SceneManager != null)
				NetworkManager.SceneManager.OnSceneEvent -= OnSceneEvent;
		}

		private bool IsClientDisconnected() => IsClient && IsServer == false && NetworkManager.IsConnectedClient == false;

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
			AddNetworkSceneManagerCallbacks();
			if (_loadEvent == LoadEvent.OnNetworkSpawn)
				TryLoadScene();
		}

		public override void OnNetworkDespawn()
		{
			if (_loadEvent == LoadEvent.OnNetworkDespawn)
				TryLoadScene();
		}

		public void LoadSceneAdditive(int sceneIndex)
		{
			if (IsServer && NetworkManager.IsListening)
			{
				if (_additiveSceneNames == null || _additiveSceneNames.Length == 0)
				{
					NetworkLog.LogError("NetworkSceneLoader has not additive scenes assigned");
					return;
				}

				if (sceneIndex < 0 || sceneIndex >= _additiveSceneNames.Length)
				{
					NetworkLog.LogError($"NetworkSceneLoader should load scene {sceneIndex} but has only {_additiveSceneNames.Length} scenes");
					return;
				}

				if (SceneManager.sceneCount > 9)
					NetworkManager.SceneManager.UnloadScene(SceneManager.GetSceneAt(1));

				var sceneName = _additiveSceneNames[sceneIndex];
				NetworkLog.LogInfo($"Try load scene additive: {sceneName}");

				var status = NetworkManager.SceneManager?.LoadScene(sceneName, LoadSceneMode.Additive);
				if (status != SceneEventProgressStatus.Started)
					NetworkLog.LogWarning($"Failed to add Scene '{_onEventSceneName}' => {status}");
			}
		}

		public void LoadSceneSingleOnDemand()
		{
			if (IsServer && NetworkManager.IsListening)
			{
				if (_onDemandSceneName == null)
				{
					NetworkLog.LogError("NetworkSceneLoader has no on-demand scene assigned");
					return;
				}

				NetworkLog.LogInfo($"Try load scene single: {_onDemandSceneName}");
				var status = NetworkManager.SceneManager?.LoadScene(_onDemandSceneName, LoadSceneMode.Single);
				if (status != SceneEventProgressStatus.Started)
					NetworkLog.LogWarning($"Failed to load Scene '{_onDemandSceneName}' => {status}");
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

		private void LoadSceneWhenDisconnected() => SceneManager.LoadScene(_onEventSceneName);

		private void OnEventLoadScene()
		{
			NetworkLog.LogInfo($"NetworkSceneLoader - load scene: {_onEventSceneName}");

			// if despawned due to server shutting down we must load the scene using non-networked SceneManager
			if (NetworkManager.ShutdownInProgress || NetworkManager.IsListening == false)
				LoadSceneWhenDisconnected();
			else
			{
				var status = NetworkManager.SceneManager?.LoadScene(_onEventSceneName, LoadSceneMode.Single);
				if (status != SceneEventProgressStatus.Started)
				{
					NetworkLog.LogWarning($"Failed to load Scene '{_onEventSceneName}' => {status} - will try again ...");
					// try again soon
					StartCoroutine(OnEventLoadSceneAfterDelay(0.2f));
				}
			}
		}

		private IEnumerator OnEventLoadSceneAfterDelay(float delay)
		{
			yield return new WaitForSeconds(delay);

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