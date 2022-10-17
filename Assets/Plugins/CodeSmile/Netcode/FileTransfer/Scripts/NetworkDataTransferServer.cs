// Copyright (C) 2021-2022 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace CodeSmile.Netcode.FileTransfer
{
	[AddComponentMenu("")] // hides this script from "Add Component" list
	[DisallowMultipleComponent]
	internal class NetworkDataTransferServer : NetworkBehaviour
	{
		[NonSerialized] private readonly Dictionary<ulong, TransferInfo> _receivingTransfers = new Dictionary<ulong, TransferInfo>();
		[NonSerialized] private TransferInfo _sendingTransfer;
		[NonSerialized] private NetworkDataTransfer _controller;
		[NonSerialized] private NetworkDataTransferClient _client;

		private void Awake()
		{
			_controller = GetComponent<NetworkDataTransfer>();
			_client = GetComponent<NetworkDataTransferClient>();
		}

		public bool IsTransferInProgress => IsSendingTransferInProgress || IsReceivingTransferInProgress();
		public bool IsSendingTransferInProgress => _sendingTransfer != null && _sendingTransfer.IsTransferInProgress();

		public bool IsReceivingTransferInProgress()
		{
			foreach (var receivingTransfer in _receivingTransfers.Values)
			{
				if (receivingTransfer.IsTransferInProgress())
					return true;
			}
			return false;
		}

		public bool IsReceivingTransferInProgress(ulong clientId) => _receivingTransfers.ContainsKey(clientId);

		private void ResetSendingTransfer() => _sendingTransfer = null;
		private void AddReceivingTransfer(ulong clientId, in TransferInfo transferInfo) => _receivingTransfers.Add(clientId, transferInfo);
		private TransferInfo GetReceivingTransfer(ulong clientId) => _receivingTransfers[clientId];
		private void SetReceivingTransfer(ulong clientId, in TransferInfo transferInfo) => _receivingTransfers[clientId] = transferInfo;
		private void RemoveReceivingTransfer(ulong clientId) => _receivingTransfers.Remove(clientId);

		internal void InitiateTransferToClient(IReadOnlyList<byte> data, ulong clientId, ushort packetSize)
		{
			_sendingTransfer = new TransferInfo(data, packetSize);
			_client.RequestServerTransfer_ClientRpc(_sendingTransfer.VerifyData, NetcodeUtils.SendTo(clientId));
		}

		private void SendNextPacketToClient(ulong clientId)
		{
			_sendingTransfer.State = TransferState.Transferring;
			_sendingTransfer.GetNextPacketData(out var packetData);
			if (packetData != null)
				_client.SendServerData_ClientRpc(packetData, NetcodeUtils.SendTo(clientId));
			else
			{
				_sendingTransfer.State = TransferState.Completed;
				_client.FinishedServerTransfer_ClientRpc(NetcodeUtils.SendTo(clientId));
			}
		}

		/// <summary>
		/// Client responds to server transfer request
		/// </summary>
		/// <param name="transferState"></param>
		/// <param name="rpcParams"></param>
		[ServerRpc(RequireOwnership = false)]
		internal void AckRequestServerTransfer_ServerRpc(TransferState transferState, ServerRpcParams rpcParams = default)
		{
			_sendingTransfer.State = transferState;
			_controller.ServerSender?.OnServerSendRequested(_sendingTransfer);

			if (_sendingTransfer.State == TransferState.Transferring || _sendingTransfer.State == TransferState.Initializing)
				SendNextPacketToClient(rpcParams.Receive.SenderClientId);
			else
			{
				Debug.LogWarning("client denied data transfer request");
				ResetSendingTransfer();
			}
		}

		/// <summary>
		/// Client responds to server data packet
		/// </summary>
		/// <param name="transferState"></param>
		/// <param name="rpcParams"></param>
		[ServerRpc(RequireOwnership = false)]
		internal void AckSendServerData_ServerRpc(TransferState transferState, ServerRpcParams rpcParams = default)
		{
			_sendingTransfer.State = transferState;
			_controller.ServerSender?.OnServerSendProgress(_sendingTransfer);
			if (_sendingTransfer.State == TransferState.Transferring)
				SendNextPacketToClient(rpcParams.Receive.SenderClientId);
			else
			{
				Debug.LogWarning("client denied packet transfer in progress");
				ResetSendingTransfer();
			}
		}

		/// <summary>
		/// Client responds to server finished with transfer
		/// </summary>
		/// <param name="transferState"></param>
		/// <param name="rpcParams"></param>
		[ServerRpc(RequireOwnership = false)]
		internal void AckFinishedServerTransfer_ServerRpc(TransferState transferState, ServerRpcParams rpcParams = default)
		{
			_sendingTransfer.State = transferState;
			_controller.ServerSender?.OnServerSendFinished(_sendingTransfer);
			ResetSendingTransfer();
		}

		/// <summary>
		/// Client requests transfer to server => server responds
		/// </summary>
		/// <param name="verifyData"></param>
		/// <param name="rpcParams"></param>
		[ServerRpc(RequireOwnership = false)]
		internal void RequestClientTransfer_ServerRpc(VerifyData verifyData, ServerRpcParams rpcParams = default)
		{
			var clientId = rpcParams.Receive.SenderClientId;
			var receivingTransfer = new TransferInfo(verifyData);
			AddReceivingTransfer(clientId, receivingTransfer);
			
			_controller.ServerReceiver?.OnServerReceiveRequested(receivingTransfer);
			_client.AckRequestClientTransfer_ClientRpc(receivingTransfer.State, NetcodeUtils.SendTo(rpcParams));
			
			if (receivingTransfer.State == TransferState.Aborted)
				RemoveReceivingTransfer(clientId);
		}

		/// <summary>
		/// Client sent data packet => server responds
		/// </summary>
		/// <param name="packetData"></param>
		/// <param name="rpcParams"></param>
		[ServerRpc(RequireOwnership = false)]
		internal void SendClientData_ServerRpc(byte[] packetData, ServerRpcParams rpcParams = default)
		{
			var clientId = rpcParams.Receive.SenderClientId;
			var receivingTransfer = GetReceivingTransfer(clientId);
			receivingTransfer.State = TransferState.Transferring;
			receivingTransfer.AddReceivedPacketData(packetData);

			_controller.ServerReceiver?.OnServerReceiveProgress(receivingTransfer);
			_client.AckSendClientData_ClientRpc(receivingTransfer.State, NetcodeUtils.SendTo(rpcParams));
			
			if (receivingTransfer.State == TransferState.Aborted)
				RemoveReceivingTransfer(clientId);
		}

		/// <summary>
		/// Client finished data transfer => server responds
		/// </summary>
		/// <param name="rpcParams"></param>
		[ServerRpc(RequireOwnership = false)]
		internal void FinishedClientTransfer_ServerRpc(ServerRpcParams rpcParams = default)
		{
			var clientId = rpcParams.Receive.SenderClientId;
			var receivingTransfer = GetReceivingTransfer(clientId);
			var hashesMatch = receivingTransfer.CompareDataHashes();
			receivingTransfer.State = hashesMatch ? TransferState.Completed : TransferState.ValidationFailed;

			_controller.ServerReceiver?.OnServerReceiveFinished(receivingTransfer);
			_client.AckFinishedClientTransfer_ClientRpc(receivingTransfer.State, NetcodeUtils.SendTo(rpcParams));
			RemoveReceivingTransfer(clientId);
		}
	}
}