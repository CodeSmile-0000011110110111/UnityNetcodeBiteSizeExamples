// Copyright (C) 2021-2022 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using Cinemachine;
using Unity.Netcode;
using UnityEngine;

namespace Plugins.CodeSmile.Netcode.QuickStart
{
	public class SetCameraFollowForOwner : NetworkBehaviour
	{
		[SerializeField] private GameObject _virtualCameraPrefab;
		[SerializeField] private Transform _cinemachineCameraTarget;

		private CinemachineVirtualCamera _virtualFollowCamera;
		
		public override void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();

			if (IsOwner && _virtualCameraPrefab != null)
			{
				var cameraObject = Instantiate(_virtualCameraPrefab);
				_virtualFollowCamera = cameraObject.GetComponent<CinemachineVirtualCamera>();
				_virtualFollowCamera.Follow = _cinemachineCameraTarget;
			}
		}

		public override void OnNetworkDespawn()
		{
			if (IsOwner)
				Destroy(_virtualFollowCamera.gameObject);
			
			base.OnNetworkDespawn();
		}
	}
}