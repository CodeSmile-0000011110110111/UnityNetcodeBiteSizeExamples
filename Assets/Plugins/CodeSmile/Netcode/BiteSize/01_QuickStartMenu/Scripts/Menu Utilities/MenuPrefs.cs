// Copyright (C) 2021-2022 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using UnityEngine;

namespace CodeSmile.Netcode.BiteSize.QuickStart
{
	public static class MenuPrefs
	{
		private const string KeyPrefix = "Menu_";

		public static string LocalPlayerName
		{
			get => PlayerPrefs.GetString(KeyPrefix + nameof(LocalPlayerName), "Player");
			set => PlayerPrefs.SetString(KeyPrefix + nameof(LocalPlayerName), value);
		}

		public static string HostPort
		{
			get => PlayerPrefs.GetString(KeyPrefix + nameof(HostPort), "7777");
			set => PlayerPrefs.SetString(KeyPrefix + nameof(HostPort), value);
		}

		public static string ServerAddress
		{
			get => PlayerPrefs.GetString(KeyPrefix + nameof(ServerAddress), "127.0.0.1");
			set => PlayerPrefs.SetString(KeyPrefix + nameof(ServerAddress), value);
		}

		public static string ServerPort
		{
			get => PlayerPrefs.GetString(KeyPrefix + nameof(ServerPort), "7777");
			set => PlayerPrefs.SetString(KeyPrefix + nameof(ServerPort), value);
		}

		public static bool IsHost
		{
			get => PlayerPrefs.GetInt(KeyPrefix + nameof(IsHost), 0) != 0;
			set => PlayerPrefs.SetInt(KeyPrefix + nameof(IsHost), value ? 1 : 0);
		}

		public static bool RememberPassword
		{
			get => PlayerPrefs.GetInt(KeyPrefix + nameof(RememberPassword), 0) != 0;
			set => PlayerPrefs.SetInt(KeyPrefix + nameof(RememberPassword), value ? 1 : 0);
		}

		public static string ServerPassword
		{
			get => PlayerPrefs.GetString(KeyPrefix + nameof(ServerPassword), null);
			set => PlayerPrefs.SetString(KeyPrefix + nameof(ServerPassword), value);
		}

		public static string ServerPasswordHash
		{
			get => PlayerPrefs.GetString(KeyPrefix + nameof(ServerPasswordHash), null);
			set => PlayerPrefs.SetString(KeyPrefix + nameof(ServerPasswordHash), value);
		}

		public static ConnectionAddressData GetConnectionAddressData() => new()
		{
			Address = ServerAddress,
			Port = ServerPort,
		};

		public static ConnectionPayloadData GetConnectionPayloadData() => new()
		{
			PlayerData = new PlayerData { Name = LocalPlayerName, Position = Vector3.up},
			PasswordHash = ServerPasswordHash,
		};
	}
}