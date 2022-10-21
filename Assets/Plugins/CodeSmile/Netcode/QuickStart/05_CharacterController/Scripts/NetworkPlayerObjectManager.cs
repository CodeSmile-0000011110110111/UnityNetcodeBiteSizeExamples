using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace CodeSmile.Netcode.QuickStart
{
	/// <summary>
	/// Makes adjustments to player whether it is the local or a remote player.
	/// </summary>
	[RequireComponent(typeof(CharacterController))]
	public class NetworkPlayerObjectManager : NetworkBehaviour
	{
		[SerializeField] private GameObject _localPlayerRoot;
		[SerializeField] private GameObject _remotePlayerRoot;
		[SerializeField] private ClientPlayerNameLabel _nameTag;

		private readonly NetworkVariable<FixedString128Bytes> _playerNameVar = new();

		private void OnEnable() => _playerNameVar.OnValueChanged += OnPlayerNameChanged;

		private void OnDisable() => _playerNameVar.OnValueChanged -= OnPlayerNameChanged;

		public void ReSendPlayerName()
		{
			var playerName = _playerNameVar.Value;
			_playerNameVar.Value = "";
			_playerNameVar.Value = playerName;
		}

		public override void OnNetworkSpawn()
		{
			if (_localPlayerRoot == null)
				throw new ArgumentNullException(nameof(_localPlayerRoot));
			if (_remotePlayerRoot == null)
				throw new ArgumentNullException(nameof(_remotePlayerRoot));
			if (_nameTag == null)
				throw new ArgumentNullException(nameof(_nameTag));

			base.OnNetworkSpawn();

			name = $"[{(IsLocalPlayer ? "Local" : "Remote")}] {name}".Replace("(Clone)", "");
			//Debug.Log($"{name} - OnNetworkSpawn({OwnerClientId}): server {IsServer}, host {IsHost}, client {IsClient}, owner {IsOwner}, localPlayer {IsLocalPlayer}");

			if (IsServer)
			{
				var bootstrap = FindObjectOfType<NetcodeBootstrap>();
				//var playerInfo = bootstrap.GetPlayerInfo(OwnerClientId);
				//_playerNameVar.Value = playerInfo.Name;
				Debug.LogWarning("todo: get player name");
			}

			GetComponent<CharacterController>().enabled = IsLocalPlayer;
			_localPlayerRoot.SetActive(IsLocalPlayer);
			_remotePlayerRoot.SetActive(IsLocalPlayer == false);

			//_playerRenderer.material.color = IsOwner ? Color.green : Random.ColorHSV(0f, 1f, .2f, 1f, .5f, 1f);
		}

		private void OnPlayerNameChanged(FixedString128Bytes prevName, FixedString128Bytes newName) =>
			_nameTag.SetPlayerName(newName.ToString());
	}
}