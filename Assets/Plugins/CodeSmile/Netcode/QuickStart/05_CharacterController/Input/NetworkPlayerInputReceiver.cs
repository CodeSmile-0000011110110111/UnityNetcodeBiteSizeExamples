using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CodeSmile.Netcode.QuickStart
{
	[RequireComponent(typeof(PlayerInput))]
	public sealed class NetworkPlayerInputReceiver : NetworkBehaviour, IPlayerInputReceiver
	{
		[SerializeField] private PlayerInputState _currentState;

		private readonly NetworkVariable<Vector2> _moveDir =
			new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
		private readonly NetworkVariable<Vector2> _lookDir =
			new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
		private readonly NetworkVariable<bool> _jump =
			new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
		private readonly NetworkVariable<bool> _attack =
			new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

		private void OnApplicationFocus(bool hasFocus) => CancelPressed = !hasFocus;

		public override void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();
			if (IsServer)
			{
				_moveDir.OnValueChanged += OnMoveDirChanged;
				_lookDir.OnValueChanged += OnLookDirChanged;
				_jump.OnValueChanged += OnJumpChanged;
				_attack.OnValueChanged += OnAttackChanged;
			}
		}

		public override void OnNetworkDespawn()
		{
			if (IsServer)
			{
				_moveDir.OnValueChanged -= OnMoveDirChanged;
				_lookDir.OnValueChanged -= OnLookDirChanged;
				_jump.OnValueChanged -= OnJumpChanged;
				_attack.OnValueChanged -= OnAttackChanged;
			}
			base.OnNetworkDespawn();
		}

		private void OnMoveDirChanged(Vector2 prevValue, Vector2 value)
		{
			if (enabled)
				_currentState.MoveDir = value;
		}

		private void OnLookDirChanged(Vector2 prevValue, Vector2 value)
		{
			if (enabled)
				_currentState.LookDir = value;
		}

		private void OnJumpChanged(bool prevValue, bool value)
		{
			if (enabled)
				_currentState.JumpPressed = value;
		}

		private void OnAttackChanged(bool prevValue, bool value)
		{
			if (enabled)
				_currentState.AttackPressed = value;
		}

		public bool CancelPressed { get; private set; }

		public PlayerInputState CurrentState => _currentState;

		public void OnMove(InputValue value)
		{
			if (enabled)
				_moveDir.Value = value.Get<Vector2>();
		}

		public void OnLook(InputValue value)
		{
			var lookDir = value.Get<Vector2>();
			if (enabled)
				_lookDir.Value = lookDir;
		}

		public void OnJump(InputValue value)
		{
			if (enabled)
				_jump.Value = value.isPressed;
		}

		public void OnAttack(InputValue value)
		{
			if (enabled)
				_attack.Value = value.isPressed;
		}

		public void OnCancel(InputValue value) => CancelPressed = value.isPressed;
	}
}