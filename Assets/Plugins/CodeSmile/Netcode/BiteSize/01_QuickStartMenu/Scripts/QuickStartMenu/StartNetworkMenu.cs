// Copyright (C) 2021-2022 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CodeSmile.Netcode.BiteSize.QuickStart
{
	public sealed class StartNetworkMenu : MenuSystemBase
	{
		private const int MaxPlayerNameLength = 16;

		[SerializeField] private TMP_InputField _playerNameInputField;
		[SerializeField] private TMP_InputField _passwordInputField;
		[SerializeField] private Toggle _rememberPasswordToggle;
		[SerializeField] private Toggle _isServerToggle;
		[SerializeField] private TMP_Text _hostButtonLabel;
		[SerializeField] private TMP_Text _hostAgainButtonLabel;

		private void OnEnable()
		{
			SetPlayerName(MenuPrefs.LocalPlayerName);
			SetServerPassword(MenuPrefs.RememberPassword ? MenuPrefs.ServerPassword : null);
			SetRememberPasswordToggle(MenuPrefs.RememberPassword);
			UpdateServerHostState(MenuPrefs.IsHost);
		}

		public void OnPlayerNameChanged(string name) =>
			SetPlayerName(name.Length > MaxPlayerNameLength ? name.Substring(0, MaxPlayerNameLength) : name);

		public void OnServerPasswordChanged(string password) => SetServerPassword(password);

		public void OnRememberPasswordToggleChanged() => SetRememberPasswordToggle(_rememberPasswordToggle.isOn);

		public void OnServerToggleChanged() => UpdateServerHostState(_isServerToggle.isOn == false);

		private void SetPlayerName(string name) => MenuPrefs.LocalPlayerName = _playerNameInputField.text = name;

		private void SetRememberPasswordToggle(bool rememberPassword)
		{
			MenuPrefs.RememberPassword = _rememberPasswordToggle.isOn = rememberPassword;
			SetServerPassword(_passwordInputField.text);
		}

		private void SetServerPassword(string password)
		{
			MenuPrefs.ServerPassword = MenuPrefs.RememberPassword ? password : null;
			MenuPrefs.ServerPasswordHash = NetcodeHelper.GetSaltedSHA1Hash(password);
			_passwordInputField.text = password;
			//Debug.Log($"Server password stored in PlayerPrefs: {MenuPrefs.ServerPassword}");
		}

		private void UpdateServerHostState(bool isHost)
		{
			MenuPrefs.IsHost = isHost;
			_isServerToggle.isOn = isHost == false;
			_hostButtonLabel.text = isHost ? "Host" : "Server";
			_hostAgainButtonLabel.text = isHost ? "Host again" : "Serve again";
		}

		public void OnHostGameClicked() => GoToMenu(typeof(HostGameMenu));
		public void OnJoinGameClicked() => GoToMenu(typeof(JoinGameMenu));
		public void OnHostAgainClicked() => ConnectAgain(false);
		public void OnJoinAgainClicked() => ConnectAgain(true);

		private void ConnectAgain(bool asClient)
		{
			// try host/join with previous (or default) settings
			NetcodeQuickStart.ConnectionAddressData = MenuPrefs.GetConnectionAddressData();
			NetcodeQuickStart.ConnectionPayloadData = MenuPrefs.GetConnectionPayloadData();
			if (asClient)
				NetcodeQuickStart.StartClient();
			else
				NetcodeQuickStart.StartServer(MenuPrefs.IsHost);
			GoToMenu(typeof(ConnectedMenu));
		}
	}
}