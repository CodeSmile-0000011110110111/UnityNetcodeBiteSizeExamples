using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CodeSmile.Netcode.QuickStart
{
	[RequireComponent(typeof(PlayerInput))]
	public sealed class PlayerInputReceiver : MonoBehaviour
	{
		[SerializeField] private InputState _currentState;

		private void OnApplicationFocus(bool hasFocus) => CancelPressed = !hasFocus;

		public bool CancelPressed { get; private set; }

		public InputState CurrentState => _currentState;

		public void OnMove(InputValue value) => _currentState.MoveDir = value.Get<Vector2>();
		public void OnLook(InputValue value) => _currentState.LookDir = value.Get<Vector2>();
		public void OnJump(InputValue value) => _currentState.JumpPressed = value.isPressed;
		public void OnAttack(InputValue value) => _currentState.AttackPressed = value.isPressed;
		public void OnCancel(InputValue value) => CancelPressed = value.isPressed;

		/// <summary>
		/// This is the state of the input that could be recorded or transferred over network to replicate the player's actions.
		/// </summary>
		[Serializable]
		public struct InputState
		{
			public Vector2 MoveDir;
			public Vector2 LookDir;
			public bool JumpPressed;
			public bool AttackPressed;

			public Vector3 GetMoveDir() => new(MoveDir.x, 0f, MoveDir.y);
			public Vector3 GetLookDir() => new(LookDir.x, 0f, LookDir.y);
		}
	}
}