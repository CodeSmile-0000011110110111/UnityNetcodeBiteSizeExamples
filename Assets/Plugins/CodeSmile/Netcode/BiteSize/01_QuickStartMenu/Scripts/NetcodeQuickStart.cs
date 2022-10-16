using System.Text;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace CodeSmile.Netcode.BiteSize.QuickStart
{
	/// <summary>
	/// Class to display helper buttons and status labels on the GUI, as well as buttons to start host/client/server.
	/// Once a connection has been established to the server, the local player can be teleported to random positions via a GUI button.
	/// </summary>
	[RequireComponent(typeof(NetworkManager), typeof(UnityTransport))]
	public class NetcodeQuickStart : MonoBehaviour
	{
		[Tooltip("True = Player prefab of NetworkManager is spawned automatically. Otherwise server must manually spawn player objects.")]
		[SerializeField] private bool _autoSpawnPlayerObject = true;
		[Tooltip("How many clients can connect. Max. 256 clients seemed like a (very) reasonable limit.")]
		[SerializeField] [Range(2, 256)] private int _maxConnectedClients = 32;
		[Tooltip("Defaults to 0.0.0.0 - this means: listen to all incoming connections.")]
		[SerializeField] private string _serverListenAddress = ConnectionAddressData.DefaultListenAddress;

		private void Start() => AddNetworkManagerCallbacks();

		private void OnDestroy() => RemoveNetworkManagerCallbacks();

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
			SetTransportConnectionData(NetcodeHelper.GetFirstLocalIPv4());
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
			Destroy(GetComponent<ServerPlayerManager>());
			SetTransportConnectionData(NetcodeHelper.TryResolveHostname(ConnectionAddressData.Address));
			SetConnectionPayload();

			//Debug.Log($"StartClient with payload: {ConnectionPayloadData}");

			NetworkManager.Singleton.StartClient();
		}

		private void SetConnectionPayload() => NetworkManager.Singleton.NetworkConfig.ConnectionData =
			Encoding.ASCII.GetBytes(JsonUtility.ToJson(ConnectionPayloadData));

		private void SetTransportConnectionData(string ipAddress)
		{
			ushort.TryParse(ConnectionAddressData.Port, out var port);
			GetComponent<UnityTransport>().SetConnectionData(ipAddress, port, _serverListenAddress);
		}

		private void OnConnectionApproval(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
		{
			Net.LogInfo($"=> OnConnectionApproval({request.ClientNetworkId})");

			var clientId = request.ClientNetworkId;

			// Don't allow more players than max
			if (NetworkManager.Singleton.ConnectedClientsList.Count >= _maxConnectedClients)
			{
				Net.LogWarning($"Server rejected clientId {clientId}: server full ({_maxConnectedClients} connections)");
				response.Approved = false;
				return;
			}

			var payloadJson = Encoding.ASCII.GetString(request.Payload);
			var payload = JsonUtility.FromJson<ConnectionPayloadData>(payloadJson);

			// Always accept the host!
			var isHost = clientId == NetworkManager.ServerClientId;
			response.Approved = isHost || CheckMatchingPasswords(payload.PasswordHash);

			if (response.Approved)
			{
				response.CreatePlayerObject = _autoSpawnPlayerObject;
				GetComponent<ServerPlayerManager>().OnClientConnectionApproved(clientId, payload.PlayerData, ref response);
			}

			if (response.Approved == false)
				Net.LogWarning($"Server rejected clientId {clientId}: password mismatch or ServerPlayerManager rejection");
		}

		private bool CheckMatchingPasswords(string clientPasswordHash)
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