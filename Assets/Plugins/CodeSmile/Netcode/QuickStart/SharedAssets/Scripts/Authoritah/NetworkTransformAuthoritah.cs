// Copyright (C) 2021-2022 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using Unity.Netcode.Components;
using UnityEngine;

namespace CodeSmile.Netcode.QuickStart
{
	/// <summary>
	/// NetworkTransform that allows you to set in Inspector who is the authoritah.
	/// </summary>
	[DisallowMultipleComponent]
	[AddComponentMenu("Netcode/Network Transform Authoritah")]
	public class NetworkTransformAuthoritah : NetworkTransform
	{
		public Authoritah Authoritah = Authoritah.Server;

		protected override bool OnIsServerAuthoritative() => Authoritah == Authoritah.Server;
	}
}