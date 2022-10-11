// Copyright (C) 2021-2022 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using UnityEngine;
using UnityEngine.EventSystems;

namespace CodeSmile.Netcode.QuickStart
{
	public sealed class InitiallyVisibleMenu : MonoBehaviour
	{
		[Tooltip("Child object that should be set active while all others will be inactive. If null, will set the first child object active.")]
		[SerializeField] private GameObject _initiallyVisibleMenu;

		private void OnEnable()
		{
			foreach (Transform child in transform)
			{
				if (child.GetComponent<EventSystem>() == null)
					child.gameObject.SetActive(false);
			}

			if (_initiallyVisibleMenu != null)
				_initiallyVisibleMenu.SetActive(true);
			else
				transform.GetChild(0)?.gameObject.SetActive(true);
		}
	}
}