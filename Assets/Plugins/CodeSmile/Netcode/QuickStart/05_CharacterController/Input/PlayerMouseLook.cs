using System;
using UnityEngine;

namespace CodeSmile.Netcode.QuickStart
{
	/// <summary>
	/// Custom script based on the version from the Standard Assets.
	/// FIXME: replace this with Cinemachine POV aiming
	/// </summary>
	[Serializable]
	public sealed class PlayerMouseLook
	{
		[SerializeField] [Range(0.01f, 1f)] private float _horizontalSensitivity = 0.5f;
		[SerializeField] [Range(0.01f, 1f)] private float _verticalSensitivity = 0.5f;
		[SerializeField] private bool _clampVerticalRotation = true;
		[SerializeField] [Range(-90f, 0f)] private float _minimumVerticalLookAngle = -90f;
		[SerializeField] [Range(0f, 90f)] private float _maximumVerticalLookAngle = 90f;
		[SerializeField] private bool _applySmoothing;
		[SerializeField] [Range(0f, 90f)] private float _smoothTime = 15f;
		[SerializeField] private bool _allowCursorLockAndHide = true;

		private Quaternion _characterTargetRotation;
		private Quaternion _cameraTargetRotation;

		private PlayerInputReceiver _inputReceiver;
		private bool _isCursorLocked = true;
		private bool _initialized;

		public void Init(Transform character, Transform camera, PlayerInputReceiver inputReceiver)
		{
			_characterTargetRotation = character.localRotation;
			_cameraTargetRotation = camera.localRotation;
			_inputReceiver = inputReceiver;
			_initialized = true;
			SetCursorLockState(_allowCursorLockAndHide);
		}

		public void LookRotation(Transform character, Transform camera, Vector2 mouseDelta)
		{
			var xRot = mouseDelta.y * _verticalSensitivity;
			var yRot = mouseDelta.x * _horizontalSensitivity;

			_cameraTargetRotation *= Quaternion.Euler(-xRot, 0f, 0f);
			if (_clampVerticalRotation)
				_cameraTargetRotation = ClampRotationAroundXAxis(_cameraTargetRotation);

			_characterTargetRotation *= Quaternion.Euler(0f, yRot, 0f);
			if (_applySmoothing)
			{
				var time = _smoothTime * Time.deltaTime;
				character.localRotation = Quaternion.Slerp(character.localRotation, _characterTargetRotation, time);
				camera.localRotation = Quaternion.Slerp(camera.localRotation, _cameraTargetRotation, time);
			}
			else
			{
				character.localRotation = _characterTargetRotation;
				camera.localRotation = _cameraTargetRotation;
			}
		}

		public void SetShouldLockCursor(bool value)
		{
			_allowCursorLockAndHide = value;
			if (_allowCursorLockAndHide == false)
				SetCursorLockState(false);
		}

		public void UpdateCursorLockState()
		{
			if (_allowCursorLockAndHide)
			{
				if (_initialized && _inputReceiver.CancelPressed)
					SetCursorLockState(false);
				else if (_initialized && _inputReceiver.CurrentState.AttackPressed)
					SetCursorLockState(true);
				else
					SetCursorLockState(_isCursorLocked);
			}
		}

		private void SetCursorLockState(bool locked)
		{
			if (locked != _isCursorLocked)
				Debug.Log("Cursor locked: " + locked);

			if (locked)
			{
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
				_isCursorLocked = true;
			}
			else
			{
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
				_isCursorLocked = false;
			}
		}

		private Quaternion ClampRotationAroundXAxis(Quaternion q)
		{
			q.x /= q.w;
			q.y /= q.w;
			q.z /= q.w;
			q.w = 1f;

			var angleX = 2f * Mathf.Rad2Deg * Mathf.Atan(q.x);
			angleX = Mathf.Clamp(angleX, _minimumVerticalLookAngle, _maximumVerticalLookAngle);
			q.x = Mathf.Tan(.5f * Mathf.Deg2Rad * angleX);

			return q;
		}
	}
}