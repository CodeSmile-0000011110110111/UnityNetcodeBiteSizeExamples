using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace CodeSmile.Netcode.BiteSize.Lobby
{
	public sealed class NetworkLobbyPlayerManager : NetworkBehaviour
	{
		[SerializeField] private GameObject[] _playerPrefabs = new GameObject[4];

		private Transform[] _spawnLocations;
		private ulong[] _assignedLocations;
		private ConnectionManager _connectionManager;
		private NetworkLobbyReadyState _readyState;

		public override void OnDestroy()
		{
			RemoveNetworkManagerCallbacks();
			_connectionManager = null;

			base.OnDestroy();
		}

		public override void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();

			_connectionManager = FindObjectOfType<ConnectionManager>();
			_readyState = GetComponent<NetworkLobbyReadyState>();

			AddNetworkManagerCallbacks();
			InitSpawnLocations();
			SpawnExistingPlayers();
		}

		private void AddNetworkManagerCallbacks()
		{
			var netMan = NetworkManager.Singleton;
			if (netMan != null)
			{
				RemoveNetworkManagerCallbacks();
				netMan.OnClientConnectedCallback += OnClientConnected;
				netMan.OnClientDisconnectCallback += OnClientDisconnect;
				_connectionManager.OnServerKickedRemoteClient += OnServerKickedRemoteClient;
			}
		}

		private void RemoveNetworkManagerCallbacks()
		{
			var netMan = NetworkManager.Singleton;
			if (netMan != null)
			{
				netMan.OnClientConnectedCallback -= OnClientConnected;
				netMan.OnClientDisconnectCallback -= OnClientDisconnect;
				_connectionManager.OnServerKickedRemoteClient -= OnServerKickedRemoteClient;
			}
		}

		private void OnClientConnected(ulong clientId)
		{
			Net.LogInfoServer($"OnClientConnected({clientId})");

			SpawnPlayerObject(clientId);
		}

		private void OnClientDisconnect(ulong clientId)
		{
			Net.LogInfoServer($"OnClientDisconnect({clientId})");

			DespawnPlayerObject(clientId);
		}

		private void OnServerKickedRemoteClient(ulong clientId, ConnectionManager.KickReason reason)
		{
			Net.LogInfo($"OnKickedRemoteClient({clientId}, {reason})");
			DespawnPlayerObject(clientId);
		}

		private void InitSpawnLocations()
		{
			// Assumption: spawn locations are first child of this object's children
			var locations = new List<Transform>();
			foreach (Transform child in transform)
				locations.Add(child.GetChild(0));

			_spawnLocations = locations.ToArray();

			_assignedLocations = new ulong[_spawnLocations.Length];
			for (var i = 0; i < _assignedLocations.Length; i++)
				_assignedLocations[i] = ulong.MaxValue;
		}

		private void SpawnExistingPlayers()
		{
			if (IsServer)
			{
				// since connection happens in the other scene, host will already be connected
				var netMan = NetworkManager.Singleton;
				foreach (var client in netMan.ConnectedClientsList)
					SpawnPlayerObject(client.ClientId);
			}
		}

		private void SpawnPlayerObject(ulong clientId)
		{
			if (IsServer)
			{
				var clientLobbyIndex = GetNextClientLobbyIndex();
				AssignSpawnLocation(clientLobbyIndex, clientId);
				var spawnLocation = _spawnLocations[clientLobbyIndex];
				Net.LogInfo($"spawn player object #{clientLobbyIndex} for clientId {clientId} at {spawnLocation.position}");

				var playerObject = Instantiate(_playerPrefabs[clientLobbyIndex], spawnLocation.position, spawnLocation.rotation);
				playerObject.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);

				// prevent it from moving / toppling over in the lobby
				playerObject.GetComponent<Rigidbody>().isKinematic = true;

				_readyState.BroadcastPlayerReadyStates();
			}
		}

		private void DespawnPlayerObject(ulong clientId)
		{
			if (IsServer)
			{
				_readyState.SetPlayerReadyState(clientId, false);

				var clientLobbyIndex = GetClientLobbyIndex(clientId);
				Net.LogInfo($"Despawn player {clientId} with lobby index {clientLobbyIndex}");

				if (clientLobbyIndex < 0)
					Net.LogWarning($"Client {clientId} has no lobby index (probably was not spawned)");
				else
					UnassignSpawnLocation(clientLobbyIndex);
			}
		}

		private void AssignSpawnLocation(int index, ulong clientId) => _assignedLocations[index] = clientId;

		private void UnassignSpawnLocation(int index) => _assignedLocations[index] = ulong.MaxValue;

		private int GetNextClientLobbyIndex()
		{
			var index = 0;
			while (_assignedLocations[index] < ulong.MaxValue)
			{
				index++;
				if (index >= _assignedLocations.Length)
					throw new ArgumentOutOfRangeException("all locations occupied, this is likely a bug");
			}

			return index;
		}

		public int GetClientLobbyIndex(ulong clientId)
		{
			for (var i = 0; i < _assignedLocations.Length; i++)
				if (_assignedLocations[i] == clientId)
					return i;

			return -1;
		}

		public ulong GetClientId(int lobbyIndex) => _assignedLocations[lobbyIndex];
	}
}