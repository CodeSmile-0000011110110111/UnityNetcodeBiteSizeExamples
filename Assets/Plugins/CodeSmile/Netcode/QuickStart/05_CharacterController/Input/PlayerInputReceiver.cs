using UnityEngine;
using UnityEngine.InputSystem;

namespace CodeSmile.Netcode.QuickStart
{
	[RequireComponent(typeof(PlayerInput))]
	public sealed class PlayerInputReceiver : MonoBehaviour, IPlayerInputReceiver
	{
		[SerializeField] private PlayerInputState _currentState;

		private void OnApplicationFocus(bool hasFocus) => CancelPressed = !hasFocus;

		public bool CancelPressed { get; private set; }

		public PlayerInputState CurrentState => _currentState;

		public void OnMove(InputValue value) => _currentState.MoveDir = value.Get<Vector2>();
		public void OnLook(InputValue value) => _currentState.LookDir = value.Get<Vector2>();
		public void OnJump(InputValue value) => _currentState.JumpPressed = value.isPressed;
		public void OnAttack(InputValue value) => _currentState.AttackPressed = value.isPressed;
		public void OnCancel(InputValue value) => CancelPressed = value.isPressed;
	}
}