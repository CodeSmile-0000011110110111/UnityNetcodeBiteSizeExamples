// Copyright (C) 2021-2022 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using Unity.Netcode.Components;
using UnityEngine;

namespace Plugins.CodeSmile.Netcode.QuickStart
{
	public class ClientAuthNetworkAnimator : NetworkAnimator
	{
		private void OnValidate()
		{
			// prevent adding this script from throwing nullrefs until Animator is assigned
			// I mean, the NetworkAnimator REQUIRES Animator to be present, so why is it null?
			if (Animator == null)
				Animator = GetComponent<Animator>();
		}

		protected override bool OnIsServerAuthoritative() => false;
	}
}