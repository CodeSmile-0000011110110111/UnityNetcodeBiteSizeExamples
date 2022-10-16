// Copyright (C) 2021-2022 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using TMPro;
using UnityEngine;

namespace CodeSmile.Netcode.BiteSize.QuickStart
{
	public sealed class JoinGameMenu : MenuSystemBase
	{
		[SerializeField] private TMP_InputField _hostAddressInput;
		[SerializeField] private TMP_InputField _hostPortInput;

		private void OnEnable()
		{
			_hostAddressInput.text = MenuPrefs.ServerAddress;
			_hostPortInput.text = MenuPrefs.ServerPort;
		}

		public void OnAddressChanged(string value) => MenuPrefs.ServerAddress = value;
		public void OnPortChanged(string value) => MenuPrefs.ServerPort = value;

		public void OnOkayButtonClicked()
		{
			NetcodeQuickStart.ConnectionAddressData = MenuPrefs.GetConnectionAddressData();
			NetcodeQuickStart.ConnectionPayloadData = MenuPrefs.GetConnectionPayloadData();
			NetcodeQuickStart.StartClient();
			GoToMenu(typeof(ConnectingMenu));
		}
	}
}