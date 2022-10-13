// Copyright (C) 2021-2022 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using Unity.Netcode.Components;
using UnityEngine;

namespace CodeSmile.Netcode.QuickStart
{
	public class ClientAuthNetworkTransform : MonoBehaviour
	{
		[DisallowMultipleComponent]
		public class ClientNetworkTransform : NetworkTransform
		{
			/// <summary>
			/// Make clients send the authority of the transform.
			/// </summary>
			/// <returns></returns>
			protected override bool OnIsServerAuthoritative() => false;
		}
	}
}