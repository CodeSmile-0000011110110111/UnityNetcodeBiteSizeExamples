// Copyright (C) 2021-2022 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using UnityEngine;

namespace CodeSmile.Netcode.QuickStart
{
	/// <summary>
	/// Turns the object to always face the camera.
	/// Defaults to facing Camera.main if target is null.
	/// </summary>
	public sealed class LookAtTargetOrMainCamera : MonoBehaviour
	{
		[Tooltip("The target to face towards. If null, will face towards Camera.main.")]
		[SerializeField] private Transform _target;
		
		private void Start()
		{
			if (_target == null)
				_target = Camera.main.transform;
		}

		private void LateUpdate() => transform.LookAt(transform.position + _target.forward);
	}
}