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
			_hostAddressInput.text = MenuPrefs.ServerAddress;
			_hostPortInput.text = MenuPrefs.ServerPort;
			PreviousMenu = typeof(StartNetworkMenu);
		}

		public void OnAddressChanged(string value) => MenuPrefs.ServerAddress = value;
		public void OnPortChanged(string value) => MenuPrefs.ServerPort = value;

		public void OnOkayButtonClicked()
		{
			NetcodeBootstrap.ConnectionAddressData = MenuPrefs.GetConnectionAddressData();
			NetcodeBootstrap.ConnectionPayloadData = MenuPrefs.GetConnectionPayloadData();
			NetcodeBootstrap.StartClient();
			GoToMenu(typeof(ConnectingMenu));
		}
	}
}