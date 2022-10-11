// Copyright (C) 2021-2022 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace CodeSmile.Netcode.FileTransfer
{
	[RequireComponent(typeof(NetworkDataTransferServer), typeof(NetworkDataTransferClient))]
	[DisallowMultipleComponent]
	public class NetworkDataTransfer : NetworkBehaviour
	{
		[NonSerialized] protected static NetworkDataTransfer _singleton;

		[Tooltip("How many bytes to transfer in a single packet. Can be up to 32k but you should leave room for other data packets. " +
		         "This is also influenced by Transport settings. If you encounter connection issues while " +
		         "transferring files, lower this value until the issues go away. Be sure to test under maximum load.")]
		[SerializeField] [Range(512, short.MaxValue - 7)] private ushort _maxPacketSize = (short.MaxValue + 1) / 2;

		[NonSerialized] private NetworkDataTransferServer _server;
		[NonSerialized] private NetworkDataTransferClient _client;

		public void Awake()
		{
			_server = GetComponent<NetworkDataTransferServer>();
			_client = GetComponent<NetworkDataTransferClient>();
		}

		protected virtual void OnEnable()
		{
			if (_singleton == null)
				_singleton = this;
		}

		public override void OnDestroy()
		{
			if (_singleton == this)
				_singleton = null;
			base.OnDestroy();
		}

		public IClientSender ClientSender { get; set; }
		public IClientReceiver ClientReceiver { get; set; }
		public IServerSender ServerSender { get; set; }
		public IServerReceiver ServerReceiver { get; set; }

		public static NetworkDataTransfer Singleton { get => _singleton; private set => _singleton = value; }

		/// <summary>
		/// Start sending data to client.
		/// This can only be called on the server.
		/// </summary>
		/// <param name="data"></param>
		/// <param name="clientId">client Id to send data to - must be a connected client</param>
		public void SendToClient(IReadOnlyList<byte> data, ulong clientId)
		{
			if (IsServer == false)
				throw new InvalidOperationException("Clients cannot SendToClient(). Relay transfer through server instead.");
			if (_server.IsTransferInProgress)
				throw new InvalidOperationException("Server is already busy transferring data");

			_server.InitiateTransferToClient(data, clientId, _maxPacketSize);
		}

		/// <summary>
		/// Start sending data to the server.
		/// This can only be called on a client, and NOT on the host either (cannot RPC send to itself, this would cause stackoverflow).
		/// </summary>
		/// <param name="data"></param>
		public void SendToServer(IReadOnlyList<byte> data)
		{
			if (IsServer)
				throw new InvalidOperationException("Server cannot SendToServer()");
			if (IsHost)
				throw new InvalidOperationException("Host cannot SendToServer() - host IS the server!");
			if (_client.IsTransferInProgress)
				throw new InvalidOperationException("Client is already busy transferring data");

			_client.InitiateTransferToServer(data, _maxPacketSize);
		}

		public bool IsClientTransferInProgress => _client.IsTransferInProgress;
		public bool IsServerSendingTransferInProgress => _server.IsSendingTransferInProgress;
		public bool IsServerReceivingTransferInProgress => _server.IsReceivingTransferInProgress();
	}
}