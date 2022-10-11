using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace CodeSmile.Netcode.FileTransfer.Demo
{
	public class FileTransferDemo : MonoBehaviour
	{
		[SerializeField] private Texture2D _transferTexture;
		[SerializeField] private RawImage _uiImage;

		private void OnEnable()
		{
			// register to receive callbacks
			var fileTransfer = NetworkFileTransfer.Singleton;
			fileTransfer.ClientSender = new ClientSender(this);
			fileTransfer.ClientReceiver = new ClientReceiver(this);
			fileTransfer.ServerSender = new ServerSender(this);
			fileTransfer.ServerReceiver = new ServerReceiver(this);
		}

		private void OnDisable()
		{
			var fileTransfer = NetworkFileTransfer.Singleton;
			fileTransfer.ClientSender = null;
			fileTransfer.ClientReceiver = null;
			fileTransfer.ServerSender = null;
			fileTransfer.ServerReceiver = null;
		}

		public void OnSendButtonClicked()
		{
			var networkManager = NetworkManager.Singleton;
			if (networkManager == null || networkManager.IsListening == false || networkManager.ShutdownInProgress)
			{
				Debug.LogError("NetworkManager not initialized, not listening, or shutting down");
				return;
			}

			var fileTransfer = NetworkFileTransfer.Singleton;

			if (networkManager.IsServer)
			{
				var didSend = false;
				foreach (var clientId in networkManager.ConnectedClientsIds)
				{
					// send to first client that isn't us (in particular: don't try sending from host-server to host-client)
					if (clientId != networkManager.LocalClientId)
					{
						fileTransfer.SendToClient(_transferTexture, clientId);
						didSend = true;
						break;
					}
				}

				if (didSend == false)
					Debug.LogWarning("couldn't send: no connected clients");
			}
			else
			{
				// send from client to server
				fileTransfer.SendToServer(_transferTexture);
			}
		}

		private void OnClientToServerTransferFinished(TransferInfo transferInfo) =>
			//if (NetworkManager.Singleton.IsServer)
			LoadAndShowReceivedTexture(transferInfo);

		private void OnServerToClientTransferFinished(TransferInfo transferInfo) =>
			//if (NetworkManager.Singleton.IsServer == false)
			LoadAndShowReceivedTexture(transferInfo);

		private void LoadAndShowReceivedTexture(TransferInfo transferInfo)
		{
			var tex = new Texture2D(512, 512);
			tex.LoadImage(transferInfo.DataReceived.ToArray());
			_uiImage.texture = tex;
		}

		/// <summary>
		/// Client sending to Server
		/// </summary>
		private class ClientSender : IClientSender
		{
			private FileTransferDemo _demo;
			public ClientSender(FileTransferDemo demo) => _demo = demo;

			public void OnClientSendRequested(TransferInfo transferInfo) =>
				Debug.Log($"Client requested transfer to server, state: {transferInfo.State}");

			public void OnClientSendProgress(TransferInfo transferInfo) =>
				Debug.Log($"Client sent {transferInfo.GetCompletionPercent() * 100f}% data, state: {transferInfo.State}");

			public void OnClientSendFinished(TransferInfo transferInfo) =>
				Debug.Log($"Client finished transfer, state: {transferInfo.State}");
		}

		/// <summary>
		/// Client receiving from Server
		/// </summary>
		private class ClientReceiver : IClientReceiver
		{
			private readonly FileTransferDemo _demo;
			public ClientReceiver(FileTransferDemo demo) => _demo = demo;

			public void OnClientReceiveRequested(TransferInfo transferInfo) =>
				Debug.Log($"Client got transfer request from server, state: {transferInfo.State}");

			public void OnClientReceiveProgress(TransferInfo transferInfo) =>
				Debug.Log($"Client received {transferInfo.GetCompletionPercent() * 100f}% data, state: {transferInfo.State}");

			public void OnClientReceiveFinished(TransferInfo transferInfo)
			{
				Debug.Log($"Client got transfer finished from server, state: {transferInfo.State}");
				_demo.OnServerToClientTransferFinished(transferInfo);
			}
		}

		/// <summary>
		/// Server sending to Client
		/// </summary>
		private class ServerSender : IServerSender
		{
			private FileTransferDemo _demo;
			public ServerSender(FileTransferDemo demo) => _demo = demo;

			public void OnServerSendRequested(TransferInfo transferInfo) =>
				Debug.Log($"Server requested transfer to client, state: {transferInfo.State}");

			public void OnServerSendProgress(TransferInfo transferInfo) =>
				Debug.Log($"Server sent {transferInfo.GetCompletionPercent() * 100f}% data, state: {transferInfo.State}");

			public void OnServerSendFinished(TransferInfo transferInfo) =>
				Debug.Log($"Server finished transfer, state: {transferInfo.State}");
		}

		private class ServerReceiver : IServerReceiver
		{
			private readonly FileTransferDemo _demo;
			public ServerReceiver(FileTransferDemo demo) => _demo = demo;

			public void OnServerReceiveRequested(TransferInfo transferInfo) =>
				Debug.Log($"Server got transfer request from client, state: {transferInfo.State}");

			public void OnServerReceiveProgress(TransferInfo transferInfo) =>
				Debug.Log($"Server received {transferInfo.GetCompletionPercent() * 100f}% data, state: {transferInfo.State}");

			public void OnServerReceiveFinished(TransferInfo transferInfo)
			{
				Debug.Log($"Server got transfer finished from client, state: {transferInfo.State}");
				_demo.OnClientToServerTransferFinished(transferInfo);
			}
		}
	}
}