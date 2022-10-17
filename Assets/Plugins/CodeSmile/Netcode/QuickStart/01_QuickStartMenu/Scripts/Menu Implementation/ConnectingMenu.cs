// Copyright (C) 2021-2022 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace CodeSmile.Netcode.QuickStart
{
	public sealed class ConnectingMenu : MenuSystemBase
	{
		[SerializeField] private TMP_Text _connectingLabel;

		private void OnEnable()
		{
			_connectingLabel.text = "Connecting ...";

			if (NetworkManager.Singleton != null)
			{
				NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
				NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
			}
		}

		private void OnDisable()
		{
			if (NetworkManager.Singleton != null)
			{
				NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
				NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
			}
		}

		private void OnClientConnected(ulong clientId) => GoToMenu(typeof(ConnectedMenu));

		private void OnClientDisconnected(ulong clientId)
		{
			_connectingLabel.text = "Failed to connect!";
			StartCoroutine(WaitThenGoBack());
		}

		private IEnumerator WaitThenGoBack()
		{
			yield return new WaitForSeconds(2f);

			OnBackButtonClicked();
		}
	}
}