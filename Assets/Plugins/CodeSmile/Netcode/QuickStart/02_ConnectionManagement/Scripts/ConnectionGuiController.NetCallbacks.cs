// Copyright (C) 2021-2022 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using Unity.Netcode;

namespace CodeSmile.Netcode.QuickStart
{
	public sealed partial class ConnectionGuiController
	{
		private void AddNetworkManagerCallbacks()
		{
			var netMan = NetworkManager.Singleton;
			if (netMan != null)
			{
				// ensure that we never register twice in case this method is called more than once 
				RemoveNetworkManagerCallbacks();

				netMan.OnServerStarted += OnServerStarted;
				netMan.OnClientConnectedCallback += OnClientConnected;
				netMan.OnClientDisconnectCallback += OnClientDisconnected;
			}
		}

		private void RemoveNetworkManagerCallbacks()
		{
			var netMan = NetworkManager.Singleton;
			if (netMan != null)
			{
				netMan.OnServerStarted -= OnServerStarted;
				netMan.OnClientConnectedCallback -= OnClientConnected;
				netMan.OnClientDisconnectCallback -= OnClientDisconnected;
			}
		}

		private void OnServerStarted()
		{
			NetworkLog.LogInfo($"OnServerStarted - server clientId: {NetworkManager.ServerClientId}");
			UpdateGuiActiveState();
		}

		private void OnClientConnected(ulong clientId)
		{
			NetworkLog.LogInfoServer($"OnClientConnected({clientId})");
			UpdateGuiActiveState();
		}

		private void OnClientDisconnected(ulong clientId)
		{
			NetworkLog.LogInfoServer($"OnClientDisconnected({clientId})");

			if (clientId == NetworkManager.ServerClientId)
			{
				NetworkLog.LogWarning("\tDisconnected by Server (kicked or shutdown) => shutting down network ..");
				ShutdownNetwork();
			}

			UpdateGuiActiveState();
		}
	}
}