using CodeSmile.Netcode.BiteSize.QuickStart;
using Unity.Netcode;
using UnityEngine;

namespace CodeSmile.Netcode.BiteSize.Connection
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

		private void OnDestroy() => RemoveNetworkManagerCallbacks();

		private void ShutdownNetwork()
		{
			var netMan = NetworkManager.Singleton;
			if (netMan != null)
			{
				NetworkLog.LogInfo(netMan.IsServer
					? $"Shutting down server, disconnecting {netMan.ConnectedClientsList.Count} clients .."
					: "Shutting down client, disconnecting from server (if still connected) ..");

				FindObjectOfType<ConnectionManager>().NetworkShutdown();
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

		private void KickClients(int kickThisManyClients)
		{
			var netMan = NetworkManager.Singleton;
			var clientIds = netMan.ConnectedClientsIds;
			var clientCount = clientIds.Count;
			var connMan = FindObjectOfType<ConnectionManager>();

			// DisconnectClient() modifies ConnectedClientsIds hence the reverse enumeration
			for (var i = clientCount - 1; i >= 0; i--)
			{
				var clientId = clientIds[i];

				// don't kick the host - note this is also ensured by KickRemoteClient
				if (clientId == NetworkManager.ServerClientId)
					continue;

				NetworkLog.LogInfo($"Kicking client {clientId} for no good reason ..");
				connMan.KickRemoteClient(clientId, ConnectionManager.KickReason.ByAuthorityOfTheServer);

				kickThisManyClients--;
				if (kickThisManyClients <= 0)
					break;
			}
		}
	}
}