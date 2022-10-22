// Copyright (C) 2021-2022 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using Unity.Netcode.Components;
using UnityEngine;

namespace CodeSmile.Netcode.QuickStart
{
	/// <summary>
	/// NetworkAnimator that allows you to set in Inspector who is the authoritah.
	/// </summary>
	[AddComponentMenu("Netcode/Network Animator Authoritah")]
	public class NetworkAnimatorAuthoritah : NetworkAnimator
	{
		public Authoritah Authoritah = Authoritah.Server;

#if UNITY_EDITOR
		private void OnValidate()
		{
			// Workaround: auto-assign Animator to prevent this script from throwing nullrefs until Animator is assigned
			// I mean, the NetworkAnimator REQUIRES Animator to be present, so why is its reference not auto-assigned?
			if (Animator == null)
				Animator = GetComponent<Animator>();
		}
#endif

		protected override bool OnIsServerAuthoritative() => Authoritah == Authoritah.Server;
	}
}