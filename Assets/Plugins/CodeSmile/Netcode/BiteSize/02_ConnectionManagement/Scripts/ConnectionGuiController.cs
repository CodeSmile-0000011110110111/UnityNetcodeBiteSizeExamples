using CodeSmile.Netcode.QuickStart;
using Unity.Netcode;
using UnityEngine;

namespace CodeSmile.Netcode.BiteSize.ConnectionHandling
{
	public sealed partial class ConnectionGuiController : MonoBehaviour
	{
		[SerializeField] private GameObject _serverGui;
		[SerializeField] private GameObject _clientGui;

		private void Start()
		{
			AddNetworkManagerCallbacks();
			UpdateGuiActiveState();
		}

		private void OnDestroy()
		{
			RemoveNetworkManagerCallbacks();
			//ShutdownNetwork();
		}

		private void ShutdownNetwork()
		{
			var netMan = NetworkManager.Singleton;
			if (netMan != null)
			{
				NetworkLog.LogInfo(netMan.IsServer
					? $"Shutting down server, disconnecting {netMan.ConnectedClientsList.Count} clients .."
					: "Shutting down client, disconnecting from server (if still connected) ..");
				
				netMan.Shutdown();
				UpdateGuiActiveState();
			}
		}

		private void UpdateGuiActiveState()
		{
			var netMan = NetworkManager.Singleton;
			var isNetworkActive = netMan != null && netMan.IsListening && netMan.ShutdownInProgress == false;
			_serverGui?.SetActive(isNetworkActive && netMan.IsServer);
			_clientGui?.SetActive(isNetworkActive && netMan.IsClient && netMan.IsConnectedClient);

			if (_serverGui.activeSelf == false && _clientGui.activeSelf == false)
				EnableStartNetworkMenu();
		}

		private void EnableStartNetworkMenu()
		{
			var menu = FindObjectOfType<InitiallyVisibleMenu>(true);
			// ensure OnEnable runs again
			menu?.gameObject.SetActive(false);
			menu?.gameObject.SetActive(true);
		}

		private void KickClients(int kickThisManyClients = 1)
		{
			var netMan = NetworkManager.Singleton;
			var clientIds = netMan.ConnectedClientsIds;
			var clientCount = clientIds.Count;

			// DisconnectClient() modifies ConnectedClientsIds hence the reverse enumeration
			for (var i = clientCount - 1; i >= 0; i--)
			{
				var clientId = clientIds[i];

				// don't kick the host :)
				if (clientId == NetworkManager.ServerClientId)
					continue;

				NetworkLog.LogInfo($"Kicking client {clientId} for no reason ..");
				netMan.DisconnectClient(clientId);

				kickThisManyClients--;
				if (kickThisManyClients <= 0)
					break;
			}
		}
	}
}