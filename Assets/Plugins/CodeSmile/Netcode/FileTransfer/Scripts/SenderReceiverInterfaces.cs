// Copyright (C) 2021-2022 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

namespace CodeSmile.Netcode.FileTransfer
{
	public interface IClientSender
	{
		public void OnClientSendRequested(TransferInfo transferInfo);
		public void OnClientSendProgress(TransferInfo transferInfo);
		public void OnClientSendFinished(TransferInfo transferInfo);
	}
	
	public interface IClientReceiver
	{
		public void OnClientReceiveRequested(TransferInfo transferInfo);
		public void OnClientReceiveProgress(TransferInfo transferInfo);
		public void OnClientReceiveFinished(TransferInfo transferInfo);
	}

	public interface IServerSender
	{
		public void OnServerSendRequested(TransferInfo transferInfo);
		public void OnServerSendProgress(TransferInfo transferInfo);
		public void OnServerSendFinished(TransferInfo transferInfo);
	}

	public interface IServerReceiver
	{
		public void OnServerReceiveRequested(TransferInfo transferInfo);
		public void OnServerReceiveProgress(TransferInfo transferInfo);
		public void OnServerReceiveFinished(TransferInfo transferInfo);
	}

	public interface IClientSenderReceiver : IClientSender, IClientReceiver{}
	public interface IServerSenderReceiver : IServerSender, IServerReceiver{}
}