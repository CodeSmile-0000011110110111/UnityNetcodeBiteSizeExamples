// Copyright (C) 2021-2022 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEngine;

namespace CodeSmile.Netcode.QuickStart
{
	[DisallowMultipleComponent]
	public class MenuSystemBase : MonoBehaviour
	{
		public Type PreviousMenu { get; set; }
		public NetcodeBootstrap NetcodeBootstrap
		{
			get
			{
				var bootstrap = FindObjectOfType<NetcodeBootstrap>();
				if (bootstrap == null)
					throw new Exception($"{typeof(NetcodeBootstrap).Name} not found in scene");

				return bootstrap;
			}
		}

		public void OnBackButtonClicked() => GoToMenu(PreviousMenu);

		public virtual void GoToMenu(Type menuScriptType)
		{
			var targetMenuScript = transform.parent.GetComponentInChildren(menuScriptType, true) as MenuSystemBase;
			if (targetMenuScript)
			{
				targetMenuScript.PreviousMenu = GetType();
				targetMenuScript.gameObject.SetActive(true);
				gameObject.SetActive(false);
			}
			else
				Debug.LogError($"GoToMenu({menuScriptType.Name}) => no such menu in hierarchy of {transform.parent.name}");
		}
	}
}