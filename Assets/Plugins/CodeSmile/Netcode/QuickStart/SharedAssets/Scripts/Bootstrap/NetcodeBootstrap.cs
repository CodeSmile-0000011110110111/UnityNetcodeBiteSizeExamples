using System.Text;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace CodeSmile.Netcode
{
	/// <summary>
	/// Class to display helper buttons and status labels on the GUI, as well as buttons to start host/client/server.
	/// Once a connection has been established to the server, the local player can be teleported to random positions via a GUI button.
	/// </summary>
	[RequireComponent(typeof(NetworkManager), typeof(UnityTransport), typeof(NetworkDisconnectManager))]
	public class NetcodeBootstrap : MonoBehaviour
	{
		private void Start() => AddNetworkManagerCallbacks();

		private void OnDestroy() => RemoveNetworkManagerCallbacks();
		[field: Tooltip("If true NetworkManager's PlayerPrefab is spawned automatically. " +
		                "Set to false to control when and which prefab to spawn for each client.")]
		[field: SerializeField] public bool AutoSpawnPlayerPrefab { get; set; } = true;

		[field: Tooltip("How many clients can connect. Max. 256 clients seemed like a (very) reasonable limit.")]
		[field: SerializeField] [field: Range(2, 256)] public int MaxConnectedClients { get; set; } = 32;

		[field: Tooltip("Defaults to '0.0.0.0' which means: server listens to all incoming connections.")]
		[field: SerializeField] public string ServerListenAddress { get; set; } = ConnectionAddressData.DefaultListenAddress;

		[field: Tooltip("If true, disallows any further client connections. For Lobby-based games that do not allow late joins. " +
		                "Intended to be modified at runtime only, ie server toggles it on after Lobby when session starts. " +
		                "Automatically resets to false when StartServer() is called.")]
		[field: SerializeField] public bool IsSessionClosed { get; set; }

		public ConnectionAddressData ConnectionAddressData { get; set; }
		public ConnectionPayloadData ConnectionPayloadData { get; set; }

		private void AddNetworkManagerCallbacks()
		{
			var netMan = NetworkManager.Singleton;
			if (netMan != null)
			{
				// ensure we never register callbacks twice
				RemoveNetworkManagerCallbacks();

				netMan.ConnectionApprovalCallback += OnConnectionApproval;
				netMan.OnServerStarted += OnServerStarted;
				netMan.OnClientConnectedCallback += OnClientConnected;
				netMan.OnClientDisconnectCallback += OnClientDisconnect;
				netMan.OnTransportFailure += OnTransportFailure;
			}
		}

		private void RemoveNetworkManagerCallbacks()
		{
			var netMan = NetworkManager.Singleton;
			if (netMan != null)
			{
				netMan.ConnectionApprovalCallback -= OnConnectionApproval;
				netMan.OnServerStarted -= OnServerStarted;
				netMan.OnClientConnectedCallback -= OnClientConnected;
				netMan.OnClientDisconnectCallback -= OnClientDisconnect;
				netMan.OnTransportFailure -= OnTransportFailure;
			}
		}

		private void OnServerStarted() => Net.LogInfo($"=> OnServerStarted - ServerClientId: {NetworkManager.ServerClientId}");

		private void OnClientConnected(ulong clientId) => Net.LogInfoServer($"=> OnClientConnected({clientId})");

		private void OnClientDisconnect(ulong clientId) => Net.LogInfoServer($"=> OnClientDisonnect({clientId})");

		private void OnTransportFailure() => Net.LogErrorServer("=> OnTransportFailure");

		public bool StartServer(bool isHost = false)
		{
			// ensure clients can join when server starts
			IsSessionClosed = false;

			SetTransportConnectionData(NetcodeUtils.GetFirstLocalIPv4());
			SetConnectionPayload();

			//Debug.Log($"StartServer with payload: {ConnectionPayloadData}");

			var netMan = NetworkManager.Singleton;
			var didStart = isHost ? netMan.StartHost() : netMan.StartServer();
			if (didStart)
				GetComponent<ServerPlayerManager>().Init();

			return didStart;
		}

		public void StartClient()
		{
			SetTransportConnectionData(NetcodeUtils.TryResolveHostname(ConnectionAddressData.Address));
			SetConnectionPayload();

			//Debug.Log($"StartClient with payload: {ConnectionPayloadData}");

			NetworkManager.Singleton.StartClient();
		}

		private void SetConnectionPayload() => NetworkManager.Singleton.NetworkConfig.ConnectionData =
			Encoding.ASCII.GetBytes(JsonUtility.ToJson(ConnectionPayloadData));

		private void SetTransportConnectionData(string ipAddress)
		{
			ushort.TryParse(ConnectionAddressData.Port, out var port);
			GetComponent<UnityTransport>().SetConnectionData(ipAddress, port, ServerListenAddress);
		}

		private void OnConnectionApproval(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
		{
			var clientId = request.ClientNetworkId;
			Net.LogInfo($"=> OnConnectionApproval({clientId})");

			// isHost ensures the host always gets accepted (host is required to be accepted!)
			var isHost = clientId == NetworkManager.ServerClientId;

			response.Approved = isHost || IsSessionClosed == false;
			if (response.Approved == false)
			{
				Net.LogWarning($"Server rejected connection of clientId {clientId}: session is closed, joining mid-game disallowed");
				return;
			}

			// Don't allow more players than max
			response.Approved = isHost || IsAcceptingConnections();
			if (response.Approved == false)
			{
				Net.LogWarning($"Server rejected connection of clientId {clientId}: server full ({MaxConnectedClients} connections)");
				return;
			}

			// deserialize the payload data
			var payloadJson = Encoding.ASCII.GetString(request.Payload);
			var payload = JsonUtility.FromJson<ConnectionPayloadData>(payloadJson);

			response.Approved = isHost || IsPasswordMatching(payload.PasswordHash);
			if (response.Approved == false)
			{
				Net.LogWarning($"Server rejected connection of clientId {clientId}: password mismatch");
				return;
			}

			response.CreatePlayerObject = AutoSpawnPlayerPrefab;
			GetComponent<ServerPlayerManager>()?.OnClientConnectionApproved(clientId, payload.PlayerData, ref response);

			// ServerPlayerManager may have set Approved to false for some reason
			response.Approved = isHost || response.Approved;
			if (response.Approved == false)
			{
				Net.LogWarning($"Server rejected connection of clientId {clientId}: ServerPlayerManager rejection");
				return;
			}

			Net.LogInfo($"\tclient {clientId} connection accepted - {payload}");
		}

		private bool IsAcceptingConnections() => NetworkManager.Singleton.ConnectedClientsList.Count < MaxConnectedClients;

		private bool IsPasswordMatching(string clientPasswordHash)
		{
			// Check connection password hashes (unless server has no password set)
			var serverPasswordHash = ConnectionPayloadData.PasswordHash;
			if (string.IsNullOrEmpty(serverPasswordHash))
				return true;

			Net.LogInfo($"checking client password hash '{clientPasswordHash}' against server's '{serverPasswordHash}'");
			return serverPasswordHash.Equals(clientPasswordHash);
		}
	}
}