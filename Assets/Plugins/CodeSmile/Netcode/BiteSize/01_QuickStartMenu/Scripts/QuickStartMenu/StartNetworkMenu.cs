// Copyright (C) 2021-2022 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using TMPro;
using UnityEngine;

namespace CodeSmile.Netcode.BiteSize.QuickStart
{
	public sealed class StartNetworkMenu : MenuSystemBase
	{
		[SerializeField] private TMP_InputField _playerNameInputField;

		private void OnEnable() => SetPlayerName(MenuPrefs.LocalPlayerName);

		public void OnPlayerNameChanged(string name)
		{
			const int MaxNameLength = 16;
			if (name.Length > MaxNameLength)
			{
				name = name.Substring(0, MaxNameLength);
				SetPlayerName(name);
			}

			MenuPrefs.LocalPlayerName = name;
		}

		public void SetPlayerName(string name) => _playerNameInputField.text = name;

		public void OnHostGameClicked() => GoToMenu(typeof(HostGameMenu));

		public void OnHostAgainClicked()
		{
			// try hosting with previous (or default) settings
			if (NetcodeQuickStart.StartHost(MenuPrefs.LocalPlayerName, MenuPrefs.HostPort))
				GoToMenu(typeof(ConnectedMenu));
			else
				Debug.LogError("could not start host, see log for details");
		}

		public void OnJoinGameClicked() => GoToMenu(typeof(JoinGameMenu));

		public void OnJoinAgainClicked()
		{
			// try joining the same server again
			NetcodeQuickStart.StartClient(MenuPrefs.LocalPlayerName, MenuPrefs.JoinAddress, MenuPrefs.JoinPort);
			GoToMenu(typeof(ConnectingMenu));
		}
	}
}