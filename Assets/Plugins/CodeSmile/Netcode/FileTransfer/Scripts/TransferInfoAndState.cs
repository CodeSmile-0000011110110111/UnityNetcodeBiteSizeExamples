// Copyright (C) 2021-2022 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace CodeSmile.Netcode.FileTransfer
{
	/// <summary>
	/// Indicates the various states of a single transfer.
	/// </summary>
	public enum TransferState
	{
		Uninitialized, // default state

		Initializing,
		Transferring,
		Completed,

		//Canceled, // canceled by user
		Aborted, // aborted by server
		ValidationFailed, // transfer complete but checksum failed
	}

	/// <summary>
	/// This is sent out when a transfer begins to let the receiver know how much data we're going to send,
	/// how many packets to expect and the SHA1 hash of the data being sent for validation after transfer completed.
	/// </summary>
	[Serializable]
	public sealed class VerifyData : INetworkSerializable
	{
		public FixedString128Bytes SHA1; // hashcode of data
		public int DataSize; // in bytes
		public int PacketCount;

		public static string GetSHA1Hash(IReadOnlyList<byte> data)
		{
			using (var sha1 = new SHA1CryptoServiceProvider())
				return string.Concat(sha1.ComputeHash(data.ToArray()).Select(x => x.ToString("X2")));
		}

		// required by NetworkManager
		public VerifyData() {}

		public VerifyData(in byte[] data, int packetCount)
		{
			SHA1 = GetSHA1Hash(data);
			DataSize = data.Length;
			PacketCount = packetCount;
		}

		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			serializer.SerializeValue(ref SHA1);
			serializer.SerializeValue(ref DataSize);
			serializer.SerializeValue(ref PacketCount);
		}
	}

	/// <summary>
	/// Contains state and data of a transfer in progress, as well as information to verify received data.
	/// </summary>
	[Serializable]
	public sealed class TransferInfo
	{
		public TransferState State;
		public ushort PacketSize;
		public int PacketCounter;
		public VerifyData VerifyData;
		public byte[] DataToSend;
		public List<byte> DataReceived;

		private byte[] _packetDataBuffer;

		// required by NetworkManager
		public TransferInfo() {}

		/// <summary>
		/// Constructor to be used by the sender only.
		/// </summary>
		/// <param name="dataToSend"></param>
		/// <param name="packetSize"></param>
		/// <param name="packetCount"></param>
		/// <param name="metaData"></param>
		public TransferInfo(in IReadOnlyList<byte> dataToSend, ushort packetSize)
		{
			var expectedPacketCount = (int)Mathf.Ceil(dataToSend.Count / (float)packetSize);
			State = TransferState.Initializing;
			PacketSize = packetSize;
			PacketCounter = 0;
			DataToSend = dataToSend.ToArray();
			DataReceived = null;
			VerifyData = new VerifyData(DataToSend, expectedPacketCount);
			_packetDataBuffer = null;
		}

		/// <summary>
		/// Constructor to be used by the receiver only.
		/// </summary>
		/// <param name="verifyData"></param>
		public TransferInfo(VerifyData verifyData)
		{
			State = TransferState.Initializing;
			PacketSize = 0;
			PacketCounter = 0;
			DataToSend = null;
			DataReceived = new List<byte>(verifyData.DataSize);
			VerifyData = verifyData;
			_packetDataBuffer = null;
		}

		/// <summary>
		/// Current transfer progress in percent (0.0f to 1.0f).
		/// </summary>
		/// <returns></returns>
		public float GetCompletionPercent() => PacketCounter / (float)VerifyData.PacketCount;

		/// <summary>
		/// Get the next slice of the data to send.
		/// Increments PacketCounter! Means: it returns the next data slice on the next call!
		/// </summary>
		/// <param name="packetData"></param>
		public void GetNextPacketData(out byte[] packetData)
		{
			packetData = null;

			var startIndex = PacketCounter * PacketSize;
			if (startIndex < VerifyData.DataSize)
			{
				PacketCounter++;

				// last packet is likely to be less than PacketSize, hence clamp it to DataSize
				var endIndex = Mathf.Min(startIndex + PacketSize, VerifyData.DataSize);
				var packetSize = endIndex - startIndex;

				// (re-)allocate the next packet array, if necessary (there's 2 re-allocations: the first and usually the last packet)
				if (_packetDataBuffer == null || _packetDataBuffer.Length != packetSize)
					_packetDataBuffer = new byte[packetSize];

				packetData = _packetDataBuffer;

				Array.Copy(DataToSend, startIndex, packetData, 0, packetSize);
			}
		}

		/// <summary>
		/// Appends received data to the data buffer.
		/// Must only be called by sender!
		/// </summary>
		/// <param name="packetData"></param>
		public void AddReceivedPacketData(byte[] packetData)
		{
			PacketCounter++;
			DataReceived.AddRange(packetData);
		}

		/// <summary>
		/// Compare hashes of the received data and the hash the sender has sent us.
		/// </summary>
		/// <returns>True if the hashes match, false if not which means received data is corrupt or incomplete.</returns>
		public bool CompareDataHashes() => VerifyData.SHA1.Equals(VerifyData.GetSHA1Hash(DataReceived));

		/// <summary>
		/// Are we currently transferring data?
		/// </summary>
		/// <returns>true if State equals Initializing or Transferring</returns>
		public bool IsTransferInProgress() => State == TransferState.Initializing || State == TransferState.Transferring;
	}
}