// Copyright (C) 2021-2022 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System.Linq;
using TMPro;
using UnityEngine;

namespace CodeSmile.Netcode.BiteSize.QuickStart
{
	public sealed class HostGameMenu : MenuSystemBase
	{
		[SerializeField] private TMP_Text _publicIpLabel;
		[SerializeField] private TMP_Text _localIpLabel;
		[SerializeField] private TMP_InputField _portInput;

		private void OnEnable()
		{
			_publicIpLabel.text = NetcodeHelper.GetPublicIPv4();
			_localIpLabel.text = NetcodeHelper.GetLocalIPv4()?.FirstOrDefault();
			_portInput.text = MenuPrefs.HostPort;
		}

		public void OnPortChanged(string value) => MenuPrefs.HostPort = value;

		public void OnOkayButtonClicked()
		{
			if (NetcodeQuickStart.StartHost(MenuPrefs.LocalPlayerName, MenuPrefs.HostPort))
				GoToMenu(typeof(ConnectedMenu));
			else
				Debug.LogError("could not start host, see log for details");
		}
	}
}