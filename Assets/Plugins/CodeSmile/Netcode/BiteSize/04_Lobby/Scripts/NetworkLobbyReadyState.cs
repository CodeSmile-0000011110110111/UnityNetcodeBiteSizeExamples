// Copyright (C) 2021-2022 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using Unity.Netcode;
using UnityEngine;

namespace CodeSmile.Netcode.BiteSize.Lobby
{
	[RequireComponent(typeof(NetworkLobbyPlayerManager))]
	public class NetworkLobbyReadyState : NetworkBehaviour
	{
		private const int MaxPlayers = 4;

		[SerializeField] private Material _notReadyMaterial;
		[SerializeField] private Material _readyMaterial;

		[Header("For debugging only")]
		[SerializeField] private bool[] _readyStates;

		private MeshRenderer[] _pedestals;
		private NetworkLobbyPlayerManager _playerManager;

		private void Awake()
		{
			_playerManager = GetComponent<NetworkLobbyPlayerManager>();
			_readyStates = new bool[MaxPlayers];

			// assumption: each child is a player pedestal with a mesh renderer
			_pedestals = new MeshRenderer[MaxPlayers];
			for (var i = 0; i < MaxPlayers; i++)
				_pedestals[i] = transform.GetChild(i).GetComponent<MeshRenderer>();
		}

		public override void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();

			// host is always "ready"
			SetPlayerReadyState(0, true);
		}

		private bool IsIndexInRange(int index) => index >= 0 && index < MaxPlayers;

		public bool GetPlayerReadyState(int index) => IsIndexInRange(index) ? _readyStates[index] : false;

		public bool ArePlayersReady()
		{
			var readyCount = 0;
			for (var i = 0; i < MaxPlayers; i++)
				if (_readyStates[i])
					readyCount++;

			var clientCount = NetworkManager.ConnectedClientsList.Count;
			return readyCount == clientCount && clientCount > 1;
		}

		public void SetPlayerReadyState(ulong clientId, bool readyState)
		{
			var lobbyIndex = _playerManager.GetClientLobbyIndex(clientId);

			if (IsIndexInRange(lobbyIndex))
			{
				_readyStates[lobbyIndex] = readyState;
				UpdatePedestalMaterials();

				if (IsServer)
					BroadcastPlayerReadyStates();
			}
		}

		private void UpdatePedestalMaterials()
		{
			for (var i = 0; i < MaxPlayers; i++)
				_pedestals[i].sharedMaterial = _readyStates[i] ? _readyMaterial : _notReadyMaterial;
		}

		public void BroadcastPlayerReadyStates()
		{
			var stateBits = 0;
			for (var i = 0; i < MaxPlayers; i++)
				if (_readyStates[i])
					stateBits |= 1 << i;

			UpdatePlayerReadyStatesClientRpc((byte)stateBits);
		}

		[ClientRpc]
		private void UpdatePlayerReadyStatesClientRpc(byte stateBits)
		{
			if (IsHost == false)
			{
				for (var i = 0; i < MaxPlayers; i++)
					_readyStates[i] = (stateBits & 1 << i) != 0;

				UpdatePedestalMaterials();
			}
		}

		public void OnClientReadyStateChanged(bool ready) => ClientReadyStateChangedServerRpc((byte)(ready ? 1 : 0));

		[ServerRpc(RequireOwnership = false)]
		private void ClientReadyStateChangedServerRpc(byte readyBit, ServerRpcParams rpcParams = default) =>
			SetPlayerReadyState(rpcParams.Receive.SenderClientId, readyBit != 0);
	}
}