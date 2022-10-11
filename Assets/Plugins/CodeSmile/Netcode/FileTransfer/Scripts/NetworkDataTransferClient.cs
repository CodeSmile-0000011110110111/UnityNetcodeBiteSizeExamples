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
	internal class NetworkDataTransferClient : NetworkBehaviour
	{
		[NonSerialized] private TransferInfo _sendingTransfer;
		[NonSerialized] private TransferInfo _receivingTransfer;
		[NonSerialized] private NetworkDataTransfer _controller;
		[NonSerialized] private NetworkDataTransferServer _server;

		private void Awake()
		{
			_controller = GetComponent<NetworkDataTransfer>();
			_server = GetComponent<NetworkDataTransferServer>();
		}

		public bool IsTransferInProgress => (_sendingTransfer != null && _sendingTransfer.IsTransferInProgress()) || 
		                                    (_receivingTransfer != null && _receivingTransfer.IsTransferInProgress());

		internal void InitiateTransferToServer(IReadOnlyList<byte> data, ushort packetSize)
		{
			_sendingTransfer = new TransferInfo(data, packetSize);
			_server.RequestClientTransfer_ServerRpc(_sendingTransfer.VerifyData);
		}

		private void SendNextPacketToServer()
		{
			_sendingTransfer.State = TransferState.Transferring;
			_sendingTransfer.GetNextPacketData(out var packetData);
			if (packetData != null)
				_server.SendClientData_ServerRpc(packetData);
			else
			{
				_sendingTransfer.State = TransferState.Completed;
				_server.FinishedClientTransfer_ServerRpc();
			}
		}

		private void ResetSendingTransfer() => _sendingTransfer = null;
		private void ResetReceivingTransfer() => _receivingTransfer = null;

		/// <summary>
		/// Server responds to client transfer request
		/// </summary>
		/// <param name="transferState"></param>
		/// <param name="rpcParams"></param>
		[ClientRpc]
		internal void AckRequestClientTransfer_ClientRpc(TransferState transferState, ClientRpcParams rpcParams = default)
		{
			_sendingTransfer.State = transferState;
			_controller.ClientSender?.OnClientSendRequested(_sendingTransfer);

			if (_sendingTransfer.State == TransferState.Transferring || _sendingTransfer.State == TransferState.Initializing)
				SendNextPacketToServer();
			else
			{
				Debug.LogWarning($"abort transfer, server changed transfer state to: {_sendingTransfer.State}");
				ResetSendingTransfer();
			}
		}

		/// <summary>
		/// Server responds to client's data packet
		/// </summary>
		/// <param name="transferState"></param>
		/// <param name="rpcParams"></param>
		[ClientRpc]
		internal void AckSendClientData_ClientRpc(TransferState transferState, ClientRpcParams rpcParams = default)
		{
			_sendingTransfer.State = transferState;
			_controller.ClientSender?.OnClientSendProgress(_sendingTransfer);

			if (_sendingTransfer.State == TransferState.Transferring)
				SendNextPacketToServer();
			else
			{
				Debug.LogWarning("server denied packet transfer in progress");
				ResetSendingTransfer();
			}
		}

		/// <summary>
		/// Server says the transfer is finished 
		/// </summary>
		/// <param name="transferState"></param>
		/// <param name="rpcParams"></param>
		[ClientRpc]
		internal void AckFinishedClientTransfer_ClientRpc(TransferState transferState, ClientRpcParams rpcParams = default)
		{
			_sendingTransfer.State = transferState;
			_controller.ClientSender?.OnClientSendFinished(_sendingTransfer);
			ResetSendingTransfer();
		}

		/// <summary>
		/// Server requests transfer => client responds
		/// </summary>
		/// <param name="verifyData"></param>
		/// <param name="rpcParams"></param>
		[ClientRpc]
		internal void RequestServerTransfer_ClientRpc(VerifyData verifyData, ClientRpcParams rpcParams = default)
		{
			_receivingTransfer = new TransferInfo(verifyData);
			_controller.ClientReceiver?.OnClientReceiveRequested(_receivingTransfer);
			_server.AckRequestServerTransfer_ServerRpc(_receivingTransfer.State);
			
			if (_receivingTransfer.State == TransferState.Aborted)
				ResetReceivingTransfer();
		}

		/// <summary>
		/// Server sent data => client responds
		/// </summary>
		/// <param name="packetData"></param>
		/// <param name="rpcParams"></param>
		[ClientRpc]
		internal void SendServerData_ClientRpc(byte[] packetData, ClientRpcParams rpcParams = default)
		{
			_receivingTransfer.State = TransferState.Transferring;
			_receivingTransfer.AddReceivedPacketData(packetData);
			_controller.ClientReceiver?.OnClientReceiveProgress(_receivingTransfer);
			_server.AckSendServerData_ServerRpc(_receivingTransfer.State);
			
			if (_receivingTransfer.State == TransferState.Aborted)
				ResetReceivingTransfer();
		}

		/// <summary>
		/// Server finished sending data => client responds
		/// </summary>
		/// <param name="rpcParams"></param>
		[ClientRpc]
		internal void FinishedServerTransfer_ClientRpc(ClientRpcParams rpcParams = default)
		{
			var hashesMatch = _receivingTransfer.CompareDataHashes();
			_receivingTransfer.State = hashesMatch ? TransferState.Completed : TransferState.ValidationFailed;
			_controller.ClientReceiver?.OnClientReceiveFinished(_receivingTransfer);
			_server.AckFinishedServerTransfer_ServerRpc(_receivingTransfer.State);
			ResetReceivingTransfer();
		}
	}
}