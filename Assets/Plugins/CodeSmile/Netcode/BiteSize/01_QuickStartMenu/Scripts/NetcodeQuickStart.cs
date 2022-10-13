using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace CodeSmile.Netcode.BiteSize.QuickStart
{
	[Serializable]
	public struct PlayerInfo
	{
		public string Name;
	}

	[Serializable]
	public struct ConnectionPayload
	{
		public string PlayerName;
	}

	/// <summary>
	/// Class to display helper buttons and status labels on the GUI, as well as buttons to start host/client/server.
	/// Once a connection has been established to the server, the local player can be teleported to random positions via a GUI button.
	/// </summary>
	[RequireComponent(typeof(NetworkManager), typeof(UnityTransport))]
	public class NetcodeQuickStart : MonoBehaviour
	{
		[Tooltip("Arbitrary player name string.")]
		[SerializeField] private string _playerName = "Player";
		[Tooltip("Server IP address or hostname. If a public IP or hostname is used the server is in theory accessible from the Internet, " +
		         "provided that port forwarding has been set up and no firewall is blocking connection attempts.")]
		[SerializeField] private string _serverAddress = "localhost";
		[Tooltip("Port the server accepts connections on.")]
		[SerializeField] private string _serverPort = "7777";
		[SerializeField] private bool _createPlayerObject = true;
		[SerializeField] private int _maxConnectedClients = 32;

		private readonly string _serverListenAddress = "0.0.0.0"; // 0.0.0.0 means: listen to all

		private Dictionary<ulong, PlayerInfo> _playerInfos;

		private void Start() => AddNetworkManagerCallbacks();

		private void OnDestroy() => RemoveNetworkManagerCallbacks();

		private void AddNetworkManagerCallbacks()
		{
			var netMan = NetworkManager.Singleton;
			if (netMan != null)
			{
				// ensure we never register callbacks twice
				RemoveNetworkManagerCallbacks();

				netMan.ConnectionApprovalCallback += ConnectionApproval;
				netMan.OnServerStarted += OnServerStarted;
				netMan.OnClientConnectedCallback += OnClientConnected;
				netMan.OnClientDisconnectCallback += OnClientDisconnected;
				netMan.OnTransportFailure += OnTransportFailure;
			}
		}

		private void RemoveNetworkManagerCallbacks()
		{
			var netMan = NetworkManager.Singleton;
			if (netMan != null)
			{
				netMan.ConnectionApprovalCallback -= ConnectionApproval;
				netMan.OnServerStarted -= OnServerStarted;
				netMan.OnClientConnectedCallback -= OnClientConnected;
				netMan.OnClientDisconnectCallback -= OnClientDisconnected;
				netMan.OnTransportFailure -= OnTransportFailure;
			}
		}

		private void OnServerStarted()
		{
			// if we're hosting we're not getting client connected event, so call it manually
			var netMan = NetworkManager.Singleton;
			if (netMan.IsHost)
				OnClientConnected(netMan.LocalClientId);
		}

		private void OnClientConnected(ulong clientId)
		{
			var netMan = NetworkManager.Singleton;

			// FIXME: hack for late joins
			if (netMan.IsServer)
				StartCoroutine(UpdatePlayerNames());
		}

		private void OnClientDisconnected(ulong clientId)
		{
			var netMan = NetworkManager.Singleton;
			if (netMan.IsServer)
				_playerInfos.Remove(clientId);
			else if (clientId == netMan.LocalClientId)
			{
				// perform any cleanup of local player here ...
			}
		}

		private void OnTransportFailure() => NetworkLog.LogErrorServer("OnTransportFailure");

		public bool StartHost(string playerName, string port)
		{
			_playerName = playerName;
			_serverPort = port;
			return StartHost();
		}

		private void StartServer()
		{
			_playerInfos = new Dictionary<ulong, PlayerInfo>();
			SetServerTransportConnectionData();
			NetworkManager.Singleton.StartServer();
		}

		private bool StartHost()
		{
			_playerInfos = new Dictionary<ulong, PlayerInfo>();
			SetServerTransportConnectionData();
			SetConnectionPayload();
			return NetworkManager.Singleton.StartHost();
		}

		public void StartClient(string playerName, string hostAddress, string port)
		{
			_playerName = playerName;
			_serverAddress = hostAddress;
			_serverPort = port;
			SetClientTransportConnectionData();
			SetConnectionPayload();
			NetworkManager.Singleton.StartClient();
		}

		private void SetConnectionPayload()
		{
			var payload = JsonUtility.ToJson(new ConnectionPayload { PlayerName = _playerName });
			NetworkManager.Singleton.NetworkConfig.ConnectionData = Encoding.ASCII.GetBytes(payload);
		}

		private void SetServerTransportConnectionData() =>
			SetTransportConnectionData(NetcodeHelper.GetLocalIPv4()?.FirstOrDefault());

		private void SetClientTransportConnectionData() =>
			SetTransportConnectionData(NetcodeHelper.ResolveHostname(_serverAddress)?.FirstOrDefault()?.ToString());

		private void SetTransportConnectionData(string ip)
		{
			ushort.TryParse(_serverPort, out var port);
			GetComponent<UnityTransport>().SetConnectionData(ip, port, _serverListenAddress);
		}

		private void ConnectionApproval(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
		{
			// don't allow more players than max
			if (NetworkManager.Singleton.ConnectedClientsList.Count >= _maxConnectedClients)
			{
				NetworkLog.LogWarningServer($"Server rejected connection request: server full ({_maxConnectedClients} clients)");
				response.Approved = false;
				return;
			}
			
			var payload = JsonUtility.FromJson<ConnectionPayload>(Encoding.ASCII.GetString(request.Payload));
			_playerInfos.Add(request.ClientNetworkId, new PlayerInfo { Name = payload.PlayerName });

			response.CreatePlayerObject = _createPlayerObject;
			response.Rotation = Quaternion.identity;
			response.Position = Vector3.one;
			response.Approved = true;
		}

		public PlayerInfo GetPlayerInfo(ulong clientId)
		{
			if (_playerInfos.ContainsKey(clientId))
				return _playerInfos[clientId];

			return default;
		}

		private IEnumerator UpdatePlayerNames()
		{
			yield return new WaitForSeconds(.3f);

			/*
			var players = FindObjectsOfType<PlayerNetworkBehaviour>();
			foreach (var player in players)
				player.ReSendPlayerName();
			*/
		}

		[ServerRpc]
		public void GetPlayerNameViaServerRpc(ServerRpcParams serverRpcParams = default)
		{
			/*
			var clientId = serverRpcParams.Receive.SenderClientId;
			if (NetworkManager.ConnectedClients.ContainsKey(clientId))
			{
				//var client = NetworkManager.ConnectedClients[clientId];

				if (_playerInfos.ContainsKey(clientId) == false)
					Debug.LogError($"no player info for {clientId}");
				else
					return _playerInfos[clientId].Name;
			}
			return clientId.ToString();
			*/
		}
	}
}