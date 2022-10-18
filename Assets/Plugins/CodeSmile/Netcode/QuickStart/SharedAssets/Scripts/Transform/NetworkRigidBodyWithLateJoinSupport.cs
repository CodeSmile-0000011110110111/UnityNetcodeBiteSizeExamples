// Copyright (C) 2021-2022 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using Unity.Netcode.Components;
using UnityEngine;

namespace CodeSmile.Netcode
{
	/// <summary>
	/// Use this instead of NetworkRigidBody to ensure that sleeping body's transform is sent to
	/// late-joining clients by calling NetworkTransform.Teleport() with current transform values.
	/// </summary>
	[DisallowMultipleComponent]
	public sealed class NetworkRigidBodyWithLateJoinSupport : NetworkRigidbody
	{
		public override void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();
			if (IsServer)
				NetworkManager.OnClientConnectedCallback += OnClientConnected;
		}

		public override void OnNetworkDespawn()
		{
			base.OnNetworkDespawn();
			if (IsServer)
				NetworkManager.OnClientConnectedCallback -= OnClientConnected;
		}

		/// <summary>
		/// Server-only callback
		/// </summary>
		/// <param name="clientId"></param>
		private void OnClientConnected(ulong clientId) => SendSleepingBodyTransform();

		/// <summary>
		/// ensures that late-joining clients get the latest position of sleeping bodies
		/// </summary>
		private void SendSleepingBodyTransform()
		{
			var rigidbody = GetComponent<Rigidbody>();
			if (rigidbody.IsSleeping())
			{
				//Net.LogInfo($"waking up rigidbody of: {name}");
				var networkTransform = GetComponent<NetworkTransform>();
				networkTransform.Teleport(rigidbody.position, rigidbody.rotation, transform.localScale);
			}
		}
	}
}