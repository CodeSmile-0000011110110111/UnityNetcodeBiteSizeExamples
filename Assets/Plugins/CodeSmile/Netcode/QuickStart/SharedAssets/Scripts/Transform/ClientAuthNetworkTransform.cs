// Copyright (C) 2021-2022 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using Unity.Netcode.Components;
using UnityEngine;

namespace CodeSmile.Netcode
{
	/// <summary>
	/// Use this instead of NetworkTransform to make client transform changes client-authoritative.
	/// </summary>
	[DisallowMultipleComponent]
	public class ClientAuthNetworkTransform : NetworkTransform
	{
		protected override bool OnIsServerAuthoritative() => false;
	}
}