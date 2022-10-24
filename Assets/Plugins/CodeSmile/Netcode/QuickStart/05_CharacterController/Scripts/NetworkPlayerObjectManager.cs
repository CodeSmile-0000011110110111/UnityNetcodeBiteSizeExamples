using Cinemachine;
using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace CodeSmile.Netcode.QuickStart
{
	/// <summary>
	/// Makes adjustments to player whether it is the local or a remote player.
	/// </summary>
	[RequireComponent(typeof(CharacterController), typeof(NetworkTransformAuthoritah))]
	public class NetworkPlayerObjectManager : NetworkBehaviour
	{
		[SerializeField] private GameObject _localPlayerRoot;
		[SerializeField] private GameObject _remotePlayerRoot;
		[SerializeField] private ClientPlayerNameLabel _nameTag;
		[SerializeField] private CinemachineVirtualCamera _virtualCamera;
		[SerializeField] private PlayerInputReceiver _inputReceiver;
		[SerializeField] private NetworkPlayerInputReceiver _netInputReceiver;

		private readonly NetworkVariable<FixedString128Bytes> _playerNameVar = new();

		private void OnEnable() => _playerNameVar.OnValueChanged += OnPlayerNameChanged;

		private void OnDisable() => _playerNameVar.OnValueChanged -= OnPlayerNameChanged;

		public override void OnNetworkSpawn()
		{
			CheckReferencesNotNull();

			base.OnNetworkSpawn();

			name = $"[{(IsLocalPlayer ? "Local" : "Remote")}] {name}".Replace("(Clone)", "");
			//Debug.Log($"{name} - OnNetworkSpawn({OwnerClientId}): server {IsServer}, host {IsHost}, client {IsClient}, owner {IsOwner}, localPlayer {IsLocalPlayer}");

			if (IsServer)
			{
				var playerData = ServerPlayerManager.Singleton.GetPlayerData(OwnerClientId);
				_playerNameVar.Value = playerData.Name;
			}

			_nameTag.SetPlayerName(_playerNameVar.Value.ToString());

			var characterController = GetComponent<CharacterController>();
			var netTransform = GetComponent<NetworkTransformAuthoritah>();

			var isServerAuthoritative = netTransform.Authoritah == Authoritah.Server;
			characterController.enabled = isServerAuthoritative ? IsServer : IsLocalPlayer;
			_localPlayerRoot.SetActive(IsLocalPlayer || isServerAuthoritative && IsServer);
			_remotePlayerRoot.SetActive(IsLocalPlayer == false);
			_virtualCamera.enabled = IsLocalPlayer;

			var localInputEnabled = IsLocalPlayer;
			var netInputEnabled = isServerAuthoritative && (IsLocalPlayer && IsServer == false || IsServer && IsLocalPlayer == false);
			//Debug.Log($"{name} - local input: {localInputEnabled}, network input: {netInputEnabled}");

			_inputReceiver.enabled = localInputEnabled;
			_netInputReceiver.enabled = netInputEnabled;
			//Debug.Log($"{name} - local input: {_inputReceiver.enabled}, network input: {_netInputReceiver.enabled} (CHECK)");
			
			if (netInputEnabled)
				Debug.LogWarning("Server authoritative player movement is enabled. While technically working, it will cause clients " +
				                 "(not the host) to feel input lag and player animations aren't supported either.");

			//_playerRenderer.material.color = IsOwner ? Color.green : Random.ColorHSV(0f, 1f, .2f, 1f, .5f, 1f);
		}

		private void CheckReferencesNotNull()
		{
			if (_localPlayerRoot == null) throw new ArgumentNullException();
			if (_remotePlayerRoot == null) throw new ArgumentNullException();
			if (_nameTag == null) throw new ArgumentNullException();
			if (_virtualCamera == null) throw new ArgumentNullException();
			if (_inputReceiver == null) throw new ArgumentNullException();
			if (_netInputReceiver == null) throw new ArgumentNullException();
		}
		
		private void OnPlayerNameChanged(FixedString128Bytes prevName, FixedString128Bytes newName) =>
			_nameTag.SetPlayerName(newName.ToString());
	}
}