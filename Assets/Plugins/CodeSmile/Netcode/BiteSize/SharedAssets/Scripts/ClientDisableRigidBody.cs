// Copyright (C) 2021-2022 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace CodeSmile.Netcode.BiteSize.SceneManagement
{
	public sealed class ClientDisableRigidBody : NetworkBehaviour
	{
		public override void OnNetworkSpawn()
		{
			if (IsServer == false)
			{
				// NetworkRigidBody depends on RigidBody thus it must be removed first.
				var netBody = GetComponent<NetworkRigidbody>();
				if (netBody != null)
					Destroy(netBody);
				
				var body = GetComponent<Rigidbody>();
				if (body != null)
					Destroy(body);
			}
		}
	}
}