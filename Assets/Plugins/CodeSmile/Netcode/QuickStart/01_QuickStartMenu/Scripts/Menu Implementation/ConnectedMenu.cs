// Copyright (C) 2021-2022 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System.Collections;
using UnityEngine;

namespace CodeSmile.Netcode.QuickStart
{
	public sealed class ConnectedMenu : MenuSystemBase
	{
		private void OnEnable() => StartCoroutine(WaitThenClose());

		private IEnumerator WaitThenClose()
		{
			yield return new WaitForSeconds(2f);

			transform.parent.gameObject.SetActive(false);
		}
	}
}