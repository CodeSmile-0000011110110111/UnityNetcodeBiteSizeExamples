using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace CodeSmile.Netcode.BiteSize.Lobby
{
	public sealed class LobbyPlayerSpotManager : NetworkBehaviour
	{
		[SerializeField] private GameObject[] _playerPrefabs = new GameObject[4];

		private Transform[] _spawnLocations;
		private ulong[] _assignedLocations;

		private void Start()
		{
			AddNetworkManagerCallbacks();
			InitSpawnLocations();
			SpawnExistingPlayers();
		}

		public override void OnDestroy()
		{
			RemoveNetworkManagerCallbacks();
			base.OnDestroy();
		}

		private void AddNetworkManagerCallbacks()
		{
			var netMan = NetworkManager.Singleton;
			if (netMan != null)
			{
				RemoveNetworkManagerCallbacks();
				netMan.OnClientConnectedCallback += OnClientConnected;
				netMan.OnClientDisconnectCallback += OnClientDisconnect;
				netMan.SceneManager.OnSynchronizeComplete += OnClientSynchronizeComplete;
			}
		}

		private void RemoveNetworkManagerCallbacks()
		{
			var netMan = NetworkManager.Singleton;
			if (netMan != null)
			{
				netMan.OnClientConnectedCallback -= OnClientConnected;
				netMan.OnClientDisconnectCallback -= OnClientDisconnect;
				netMan.SceneManager.OnSynchronizeComplete -= OnClientSynchronizeComplete;
			}
		}

		private void OnClientSynchronizeComplete(ulong clientId)
		{
			Net.LogInfo("OnClientSynchronizeComplete");
			//SpawnPlayerObject(clientId);
		}

		private void OnClientConnected(ulong clientId)
		{
			Net.LogInfoServer("OnClientConnected");
			//SpawnPlayerObject(clientId);
		}

		private void OnClientDisconnect(ulong clientId)
		{
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
				Debug.Log($"spawn player object #{clientLobbyIndex} for clientId {clientId}");

				var playerObject = Instantiate(_playerPrefabs[clientLobbyIndex], Vector3.zero, Quaternion.identity);
				var playerNetworkObject = playerObject.GetComponent<NetworkObject>();
				playerNetworkObject.SpawnAsPlayerObject(clientId);
				playerNetworkObject.TrySetParent(_spawnLocations[clientLobbyIndex], false);

				// prevent it from moving / toppling over in the lobby
				playerObject.GetComponent<Rigidbody>().isKinematic = true;
			}
		}

		private void DespawnPlayerObject(ulong clientId)
		{
			if (IsServer)
				UnassignSpawnLocation(GetClientLobbyIndex(clientId));
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
		private int GetClientLobbyIndex(ulong clientId)
		{
			for (int i = 0; i < _assignedLocations.Length; i++)
			{
				if (_assignedLocations[i] == clientId)
					return i;
			}

			return -1;
		}
		
	}
}