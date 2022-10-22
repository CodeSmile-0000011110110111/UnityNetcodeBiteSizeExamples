// Copyright (C) 2021-2022 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace CodeSmile.Netcode.QuickStart
{
	/// <summary>
	/// Stores and gives access to NetworkPlayerData instances of clients on the server-side.
	/// </summary>
	[DisallowMultipleComponent]
	[RequireComponent(typeof(NetworkManager))]
	public class ServerPlayerManager : MonoBehaviour
	{
		private readonly Dictionary<ulong, NetworkPlayerData> _playerDatas = new();
		public void Init() => AddNetworkManagerCallbacks();

		private void OnEnable()
		{
			if (Singleton == null)
				Singleton = this;
		}

		private void OnDestroy() => RemoveNetworkManagerCallbacks();

		public static ServerPlayerManager Singleton { get; private set; }

		public NetworkPlayerData GetPlayerData(ulong clientId)
		{
			if (_playerDatas.ContainsKey(clientId))
				return _playerDatas[clientId];

			Debug.LogWarning($"No player info available for clientId {clientId}");
			return default;
		}

		public void SetPlayerData(ulong clientId, NetworkPlayerData networkPlayerData)
		{
			if (_playerDatas.ContainsKey(clientId))
				_playerDatas[clientId] = networkPlayerData;
			else
				_playerDatas.Add(clientId, networkPlayerData);
		}

		private void AddNetworkManagerCallbacks()
		{
			var netMan = NetworkManager.Singleton;
			if (netMan != null)
			{
				// ensure we never register callbacks twice
				RemoveNetworkManagerCallbacks();
				netMan.OnClientDisconnectCallback += OnClientDisconnect;
			}
		}

		private void RemoveNetworkManagerCallbacks()
		{
			var netMan = NetworkManager.Singleton;
			if (netMan != null)
				netMan.OnClientDisconnectCallback -= OnClientDisconnect;
		}

		private void OnClientDisconnect(ulong clientId)
		{
			if (NetworkManager.Singleton.IsServer)
			{
				Net.LogInfo($"=> ServerPlayerManager OnClientDisonnect({clientId})");

				var playerData = GetPlayerData(clientId);
				playerData.Connected = false;
				playerData.ClientId = ulong.MaxValue;
				SetPlayerData(clientId, playerData);
			}
		}

		public void OnClientConnectionApproved(ulong clientId, NetworkPlayerData networkPlayerData,
			ref NetworkManager.ConnectionApprovalResponse response)
		{
			Net.LogInfo($"=> ServerPlayerManager OnClientConnectionApproved({clientId})");

			// TODO: look for an appropriate spawn location, or identify player and use its last known position
			// or verify the provided position & rotation for validity
			response.Position = networkPlayerData.StartPosition;
			response.Rotation = networkPlayerData.StartRotation;

			networkPlayerData.Connected = response.Approved;
			SetPlayerData(clientId, networkPlayerData);
		}
	}
}