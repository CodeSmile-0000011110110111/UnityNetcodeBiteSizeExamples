// Copyright (C) 2021-2022 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using TMPro;
using UnityEngine;

namespace CodeSmile.Netcode.QuickStart
{
	public sealed class JoinGameMenu : MenuSystemBase
	{
		[SerializeField] private TMP_InputField _hostAddressInput;
		[SerializeField] private TMP_InputField _hostPortInput;

		private void OnEnable()
		{
			_hostAddressInput.text = MenuPrefs.JoinAddress;
			_hostPortInput.text = MenuPrefs.JoinPort;
		}

		public void OnAddressChanged(string value) => MenuPrefs.JoinAddress = value;
		public void OnPortChanged(string value) => MenuPrefs.JoinPort = value;

		public void OnOkayButtonClicked()
		{
			NetcodeQuickStart.StartClient(MenuPrefs.LocalPlayerName, MenuPrefs.JoinAddress, MenuPrefs.JoinPort);
			GoToMenu(typeof(ConnectingMenu));
		}
	}
}