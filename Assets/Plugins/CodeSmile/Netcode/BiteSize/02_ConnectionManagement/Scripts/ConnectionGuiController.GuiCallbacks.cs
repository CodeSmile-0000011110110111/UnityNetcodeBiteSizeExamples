// Copyright (C) 2021-2022 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using Unity.Netcode;

namespace CodeSmile.Netcode.BiteSize.ConnectionHandling
{
	public sealed partial class ConnectionGuiController
	{
		public void OnButtonServerShutdown() => ShutdownNetwork();
		public void OnButtonServerKickClient() => KickClients();
		public void OnButtonServerKickAllClients() => KickClients(NetworkManager.Singleton.ConnectedClientsIds.Count);
		public void OnButtonClientDisconnect() => ShutdownNetwork();
	}
}