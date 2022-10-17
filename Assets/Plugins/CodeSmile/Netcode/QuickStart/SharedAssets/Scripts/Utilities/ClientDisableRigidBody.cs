// Copyright (C) 2021-2022 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace CodeSmile.Netcode
{
	/// <summary>
	/// Put this on any network object with (Network)RigidBody on it to disable the rigid body physics on the client-side.
	/// This is required for network physics to behave as expected, otherwise the server and client may both simulate physics
	/// movement of the object, resulting in conflicting motion.
	/// </summary>
	[DisallowMultipleComponent]
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