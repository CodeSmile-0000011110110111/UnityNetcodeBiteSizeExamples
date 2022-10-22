// Copyright (C) 2021-2022 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using Unity.Netcode;
using UnityEngine;

namespace CodeSmile.Netcode.QuickStart
{
	/// <summary>
	/// This is a relay class for client disconnect / network shutdown events. It is part of the NetcodeBootstrap prefab.
	/// 
	/// The primary reason for this class is to ensure other systems can subscribe to shutdown/client disconnect/client kicked events
	/// that are issued locally, because Netcode will not call OnClientDisconnect callbacks when the local system "logically" knows
	/// about the event occuring.
	/// Similarly, there is currently no OnShutdown event in Netcode, so if a client wishes to disconnect, we also need to forward
	/// that message to subscribed systems.
	/// </summary>
	[RequireComponent(typeof(NetworkManager))]
	public class NetworkDisconnectManager : MonoBehaviour
	{
		/// <summary>
		/// Invoked locally when the local client disconnects or on the server/host when the server shuts down.
		/// </summary>
		public event Action OnNetworkShutdown;

		/// <summary>
		/// Invoked on the server when a client has been forcefully disconnected.
		/// </summary>
		public event Action<ulong, KickReason> OnServerKickedRemoteClient;

		/// <summary>
		/// Encodes common reasons for forcibly disconnecting a client.
		/// </summary>
		public enum KickReason
		{
			None,
			Vote,
			BadBehaviour,
			BadConnection,
			ServerAuthority,
		}

		private void OnDestroy()
		{
			OnNetworkShutdown = null;
			OnServerKickedRemoteClient = null;
		}

		/// <summary>
		/// Shut down network.
		/// For clients this means disconnecting from the server.
		/// For the server/host this means shutting down the server and disconnecting all clients.
		/// Invokes the OnNetworkShutdown event after calling NetworkManager.Shutdown().
		/// </summary>
		public void NetworkShutdown()
		{
			var netMan = NetworkManager.Singleton;
			if (netMan != null && netMan.ShutdownInProgress == false)
			{
				netMan.Shutdown();
				OnNetworkShutdown?.Invoke();
			}
		}

		/// <summary>
		/// Kicks a remote client. Must be issued by the Server.
		/// Note: the host cannot be kicked (will be ignored).
		/// Invokes the OnKickedRemoteClient event.
		/// </summary>
		/// <param name="clientId"></param>
		/// <param name="reason"></param>
		public void KickClient(ulong clientId, KickReason reason = KickReason.None)
		{
			var netMan = NetworkManager.Singleton;
			if (netMan != null && netMan.IsServer)
			{
				// don't kick the host
				if (clientId != NetworkManager.ServerClientId)
				{
					netMan.DisconnectClient(clientId);
					OnServerKickedRemoteClient?.Invoke(clientId, reason);
				}
			}
		}
	}
}