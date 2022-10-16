// Copyright (C) 2021-2022 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Netcode.BiteSize.SceneManagement;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace CodeSmile.Netcode.BiteSize.Lobby
{
	public class LobbyGuiController : MonoBehaviour
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

		public void OnButtonServerStartGame()
		{
			var playersReady = FindObjectOfType<NetworkLobbyReadyState>().ArePlayersReady();
			if (playersReady)
			{
				// re-enable rigidbody after lobby
				foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
					client.PlayerObject.GetComponent<Rigidbody>().isKinematic = false;

				FindObjectOfType<NetworkSceneLoader>().LoadSceneSingleOnDemand();
			}
			else
				Debug.LogWarning("couldn't start game: not all players ready");
		}

		public void OnButtonServerKickClient(int clientLobbyIndex)
		{
			Debug.Log("kick client " + clientLobbyIndex);
			var clientId = FindObjectOfType<NetworkLobbyPlayerManager>().GetClientId(clientLobbyIndex);
			FindObjectOfType<LocalDisconnectManager>().KickRemoteClient(clientId);
		}

		public void OnButtonClientReadyChanged(Toggle toggle) =>
			FindObjectOfType<NetworkLobbyReadyState>().OnClientReadyStateChanged(toggle.isOn);
	}
}