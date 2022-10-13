using Unity.Netcode;
using UnityEngine;

namespace CodeSmile.Netcode.BiteSize.SceneManagement
{
	public sealed class SceneLoadGuiController : MonoBehaviour
	{
		[SerializeField] private GameObject _serverGui;
		[SerializeField] private GameObject _clientGui;

		private void Start() => UpdateGuiActiveState();

		private void UpdateGuiActiveState()
		{
			var netMan = NetworkManager.Singleton;
			var isNetworkActive = netMan != null && netMan.IsListening && netMan.ShutdownInProgress == false;
			_serverGui?.SetActive(isNetworkActive && netMan.IsServer);
			_clientGui?.SetActive(isNetworkActive && netMan.IsClient && netMan.IsConnectedClient);
		}

		public void OnButtonServerLoadSceneAdditive(int sceneIndex)
		{
			var sceneLoader = FindObjectOfType<NetworkSceneLoader>();
			if (sceneLoader != null)
				sceneLoader.LoadSceneAdditive(sceneIndex);
			else
				Debug.LogError("NetworkSceneLoader not in scene ...");
		}

		public void OnButtonServerLoadSceneSingle()
		{
			var sceneLoader = FindObjectOfType<NetworkSceneLoader>();
			if (sceneLoader != null)
				sceneLoader.LoadSceneSingleOnDemand();
			else
				Debug.LogError("NetworkSceneLoader not in scene ...");
		}
	}
}