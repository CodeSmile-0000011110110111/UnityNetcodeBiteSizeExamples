// Copyright (C) 2021-2022 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using System.IO;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace CodeSmile.Netcode.FileTransfer
{
	[DisallowMultipleComponent]
	public class NetworkFileTransfer : NetworkDataTransfer
	{
		public void SendToServer(Texture2D texture)
		{
			if (texture == null) throw new ArgumentNullException(nameof(texture));
			
			SendToServer(texture.EncodeToPNG());
		}

		public void SendToClient(Texture2D texture, ulong clientId)
		{
			if (texture == null) throw new ArgumentNullException(nameof(texture));

			SendToClient(texture.EncodeToPNG(), clientId);
		}

		public void SendFileToServer(string path)
		{
			if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException(nameof(path));
			if (File.Exists(path) == false) throw new FileNotFoundException(path);

			SendToServer(File.ReadAllBytes(path));
		}

		public void SendFileToClient(string path, ulong clientId)
		{
			if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException(nameof(path));
			if (File.Exists(path) == false) throw new FileNotFoundException(path);

			SendToClient(File.ReadAllBytes(path), clientId);
		}

		public static new NetworkFileTransfer Singleton => _singleton as NetworkFileTransfer;

	}
}