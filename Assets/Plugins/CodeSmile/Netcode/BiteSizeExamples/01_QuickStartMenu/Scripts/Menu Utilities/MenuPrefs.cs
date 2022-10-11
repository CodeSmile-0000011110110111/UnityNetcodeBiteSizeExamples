// Copyright (C) 2021-2022 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using UnityEngine;

namespace CodeSmile.Netcode.QuickStart
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

		public static string JoinAddress
		{
			get => PlayerPrefs.GetString(KeyPrefix + nameof(JoinAddress), "127.0.0.1");
			set => PlayerPrefs.SetString(KeyPrefix + nameof(JoinAddress), value);
		}

		public static string JoinPort
		{
			get => PlayerPrefs.GetString(KeyPrefix + nameof(JoinPort), "7777");
			set => PlayerPrefs.SetString(KeyPrefix + nameof(JoinPort), value);
		}
	}
}