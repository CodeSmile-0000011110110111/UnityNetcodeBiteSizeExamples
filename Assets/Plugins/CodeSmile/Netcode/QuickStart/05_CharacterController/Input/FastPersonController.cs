// Copyright (C) 2021-2022 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using Cinemachine;
using System;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;

namespace CodeSmile.Netcode.QuickStart
{
	/// <summary>
	/// First person character controller that enables skillful movement, rather than today's FPS controllers which,
	/// frankly, are closer to being "I took an arrow to my knee I'm glad I can move AT ALL".
	/// As John Romero puts it, they are "grandpa's shooters" to make feel people cool "ducking at the right time". :)
	/// Watch this for around 90 seconds: https://www.youtube.com/watch?v=9gs5r_4eY0k&t=232s
	///
	/// This is losely based on several (public domain) Quake-style character controllers for Unity,
	/// rewritten from scratch to
	/// a) understand it all and
	/// b) use only modern 2021.3 Unity tech and
	/// c) make it compatible with Unity Netcode networking and
	/// d) because it's fun fixing all their issues and inefficiencies and wonder how that even worked to begin with.
	/// </summary>
	[RequireComponent(typeof(PlayerInputReceiver))]
	public sealed class FastPersonController : MonoBehaviour
	{
		[SerializeField] private CharacterController _characterController;
		[SerializeField] private Animator _animator;

		[Header("Camera")]
		[SerializeField] private CinemachineVirtualCamera _virtualCamera;
		[SerializeField] private PlayerMouseLook _mouseLook;

		[Header("Settings")]
		[SerializeField] private MotionSettings _motionSettings;
		[SerializeField] private CollisionSettings _collisionSettings;
		[SerializeField] private AnimationSettings _animationSettings;

		[Header("Debug")]
		[SerializeField] private MotionState _motionState;

		private AnimationId _animId;
		private float _animationBlendSpeed;
		private IPlayerInputReceiver _inputReceiver;
		private PlayerInputState _playerInputState;
		private Vector3 _groundHitNormal;
		private bool _isJumpQueued;

		private void Start()
		{
			if (_virtualCamera == null)
				throw new ArgumentNullException("a CinemachineVirtualCamera must be assigned in the Inspector");
			if (_characterController == null)
				throw new ArgumentNullException("a CharacterController must be assigned in the Inspector");
			if (_animator == null)
				throw new ArgumentNullException("a Animator must be assigned in the Inspector");

			var localInputReceiver = GetComponent<PlayerInputReceiver>();
			var netInputReceiver = GetComponent<NetworkPlayerInputReceiver>();
			_inputReceiver = netInputReceiver.enabled ? netInputReceiver : localInputReceiver;
			
			// for convenience (and because it wouldn't make any sense) always remove our own layer from the collision mask
			//_collisionSettings.groundLayers &= ~gameObject.layer;

			_mouseLook = new PlayerMouseLook();
			_mouseLook.Init(_characterController.transform, _virtualCamera.transform, _inputReceiver);
			
			// if server authoritative (character controller is not enabled) disable local move and camera
			enabled = _characterController.enabled;
			
			SetCharacterControllerCenterYFromHeight();
			AssignAnimationIds();
		}

		private void Update()
		{
			_playerInputState = _inputReceiver.CurrentState;

			DetermineGroundedState();
			SetGroundedAnimationState();

			if (_motionState.IsGrounded)
			{
				SlopeSlide();
				GroundMove();
			}
			else
				AirMove();

			_mouseLook.UpdateCursorLockState();
			_mouseLook.LookRotation(_characterController.transform, _virtualCamera.transform, _playerInputState.LookDir);
			_characterController.Move(_motionState.Velocity * Time.deltaTime);
		}

		private void OnDisable() => _mouseLook.SetShouldLockCursor(false);

		private void OnControllerColliderHit(ControllerColliderHit hit) => _groundHitNormal = hit.normal;

		private void OnDrawGizmosSelected()
		{
			if (_characterController == null)
				_characterController = GetComponent<CharacterController>();

			var green = new Color(0.0f, 1.0f, 0.0f, 0.35f);
			var red = new Color(1.0f, 0.0f, 0.0f, 0.35f);
			Gizmos.color = _motionState.IsGrounded ? green : red;

			var groundSpherePos = GetGroundedSpherePosition();
			var radius = _characterController.radius + 0.03f;
			Gizmos.DrawSphere(groundSpherePos, radius);
		}

		private void AssignAnimationIds() => _animId = new AnimationId
		{
			Speed = Animator.StringToHash("Speed"),
			Grounded = Animator.StringToHash("Grounded"),
			Jump = Animator.StringToHash("Jump"),
			FreeFall = Animator.StringToHash("FreeFall"),
			MotionSpeed = Animator.StringToHash("MotionSpeed"),
		};

		private void SetCharacterControllerCenterYFromHeight()
		{
			var center = _characterController.center;
			center.y = _characterController.height * 0.5f;
			_characterController.center = center;
		}

		private void SlopeSlide()
		{
			// FIXME: slope-sliding ... it works but it doesn't feel good
			var isOnSteepSlope = Vector3.Angle(Vector3.up, _groundHitNormal) > _characterController.slopeLimit;
			if (isOnSteepSlope)
			{
				var invGroundUpNormal = 1f - _groundHitNormal.y;
				_motionState.Velocity.x += invGroundUpNormal * _groundHitNormal.x * _motionSettings.SlopeSlideSpeed;
				_motionState.Velocity.z += invGroundUpNormal * _groundHitNormal.z * _motionSettings.SlopeSlideSpeed;
				_motionState.IsGrounded = false;
			}
		}

		private void GroundMove()
		{
			if (_playerInputState.JumpPressed == false)
				ApplySurfaceFriction();

			var moveDir = _playerInputState.GetMoveDir();
			var targetDir = _characterController.transform.TransformDirection(moveDir).normalized;
			var targetSpeed = targetDir.magnitude * _motionSettings.Ground.MaxSpeed;
			Accelerate(targetDir, targetSpeed, _motionSettings.Ground.Acceleration);

			if (_playerInputState.JumpPressed)
			{
				_playerInputState.JumpPressed = false;
				_motionState.Velocity.y = _motionSettings.JumpForce;
				SetJumpFallAnimationState(true, false);
			}
			else
			{
				_motionState.Velocity.y = 0f;
				ApplyGravity();

				PlayWalkRunAnimation(moveDir, targetSpeed);
			}
		}

		private void AirMove()
		{
			var inputDir = _playerInputState.GetMoveDir();
			var targetDir = _characterController.transform.TransformDirection(inputDir);
			var moveSpeed = targetDir.magnitude * _motionSettings.Air.MaxSpeed;
			var airControlSpeed = moveSpeed;
			targetDir.Normalize();

			var acceleration = 0f;
			if (inputDir.x != 0f && inputDir.z == 0f)
			{
				moveSpeed = Mathf.Min(moveSpeed, _motionSettings.Strafe.MaxSpeed);
				acceleration = _motionSettings.Strafe.Acceleration;
			}
			else
			{
				var currentSpeed = Vector3.Dot(_motionState.Velocity, targetDir);
				acceleration = currentSpeed < 0f ? _motionSettings.Air.Deceleration : _motionSettings.Air.Acceleration;
			}

			Accelerate(targetDir, moveSpeed, acceleration);

			if (_motionSettings.AirControlRate > 0f)
				AirControl(targetDir, airControlSpeed);

			ApplyGravity();

			if (_motionState.Velocity.y < 0f)
				SetJumpFallAnimationState(false, true);
		}

		// Air control occurs when the player is in the air, it allows players to move side 
		// to side much faster rather than being 'sluggish' when it comes to cornering.
		private void AirControl(Vector3 targetDir, float targetSpeed)
		{
			// no air control when not moving
			if (Mathf.Approximately(Mathf.Abs(_playerInputState.MoveDir.y), 0f) || Mathf.Approximately(Mathf.Abs(targetSpeed), 0f))
				return;

			var velocity = _motionState.Velocity;
			var verticalSpeed = velocity.y;
			velocity.y = 0f;

			var speed = velocity.magnitude;
			velocity.Normalize();

			var currentSpeed = Vector3.Dot(velocity, targetDir);
			var airControlFactor = _motionSettings.AirControlRate * currentSpeed * currentSpeed * Time.deltaTime;

			// allow change of direction in mid-air
			if (currentSpeed > 0f)
			{
				velocity.x *= speed + targetDir.x * airControlFactor;
				velocity.y *= speed + targetDir.y * airControlFactor;
				velocity.z *= speed + targetDir.z * airControlFactor;
				velocity.Normalize();
			}

			_motionState.Velocity = new Vector3(velocity.x * speed, verticalSpeed, velocity.z * speed);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void ApplyGravity() => _motionState.Velocity.y -= _motionSettings.Gravity * Time.deltaTime;

		private float Accelerate(Vector3 targetDir, float targetSpeed, float acceleration)
		{
			if (targetSpeed <= 0f)
				return 0f;

			var currentSpeed = Vector3.Dot(_motionState.Velocity, targetDir);
			var maxAcceleration = targetSpeed - currentSpeed;
			if (maxAcceleration <= 0f)
				return 0f;

			var finalAcceleration = Mathf.Min(acceleration * targetSpeed * Time.deltaTime, maxAcceleration);
			_motionState.Velocity.x += finalAcceleration * targetDir.x;
			_motionState.Velocity.z += finalAcceleration * targetDir.z;
			return finalAcceleration;
		}

		private void ApplySurfaceFriction()
		{
			var velocity = _motionState.Velocity;
			var currentSpeed = velocity.magnitude;
			var currentOrMinSpeed = currentSpeed < _motionSettings.Ground.Deceleration ? _motionSettings.Ground.Deceleration : currentSpeed;
			var speedReduction = currentOrMinSpeed * _motionSettings.SurfaceFriction * Time.deltaTime;

			var speed = Mathf.Max(currentSpeed - speedReduction, 0f);
			if (currentSpeed > 0f)
				speed /= currentSpeed;

			_motionState.Velocity = new Vector3(velocity.x * speed, velocity.y, velocity.z * speed);
		}

		private Vector3 GetGroundedSpherePosition()
		{
			var pos = _characterController.transform.position;
			pos.y += _characterController.radius - _collisionSettings.GroundedRelaxation;
			return pos;
		}

		private void DetermineGroundedState()
		{
			var groundSpherePos = GetGroundedSpherePosition();
			var radius = _characterController.radius;
			var layers = _collisionSettings.GroundLayers;
			_motionState.IsGrounded = Physics.CheckSphere(groundSpherePos, radius, layers, QueryTriggerInteraction.Ignore);
		}

		private void SetGroundedAnimationState() => _animator.SetBool(_animId.Grounded, _motionState.IsGrounded);

		private void SetJumpFallAnimationState(bool jump, bool fall)
		{
			_animator.SetBool(_animId.Jump, jump);
			_animator.SetBool(_animId.FreeFall, fall);
		}

		private void PlayWalkRunAnimation(Vector3 moveDir, float targetSpeed)
		{
			_animationBlendSpeed = Mathf.Lerp(_animationBlendSpeed, targetSpeed, Time.deltaTime * _animationSettings.MaxWalkBlendSpeed);
			if (_animationBlendSpeed < _animationSettings.MinWalkBlendSpeed)
				_animationBlendSpeed = 0f;

			var backward = moveDir.z < 0f;
			var motionSpeed = _motionState.Velocity.magnitude / _animationSettings.RunSpeed * (backward ? -1f : 1f);
			//if (_animationBlendSpeed > 0f || motionSpeed > 0f) Debug.Log($"anim speed: {_animationBlendSpeed}, motionSpeed: {motionSpeed}");

			_animator.SetFloat(_animId.Speed, _animationBlendSpeed);
			_animator.SetFloat(_animId.MotionSpeed, motionSpeed);
			SetJumpFallAnimationState(false, false);
		}

		private struct AnimationId
		{
			public int Speed;
			public int Grounded;
			public int Jump;
			public int FreeFall;
			public int MotionSpeed;
		}

		[Serializable]
		public struct CollisionSettings
		{
			[Tooltip("Jump more easily on rough surfaces, but makes player seem to sink slightly into ground after landing.")]
			[Range(0.02f, .2f)] public float GroundedRelaxation;
			[Tooltip("Layers the controller considers as ground surfaces.")]
			public LayerMask GroundLayers;
		}

		[Serializable]
		public struct AnimationSettings
		{
			public float MinWalkBlendSpeed;
			public float MaxWalkBlendSpeed;
			[Tooltip("lower values lead to faster animation")]
			public float RunSpeed;
		}

		[Serializable]
		public struct MotionSettings
		{
			[Tooltip("Gravity. The force that keeps everyone grounded but no one truly understands.")]
			[Range(0f, 50f)] public float Gravity;
			[Range(0f, 50f)] public float SurfaceFriction;
			[Range(0f, 50f)] public float JumpForce;
			[Range(0f, 50f)] public float AirControlRate;
			[Range(0f, 10f)] public float SlopeSlideSpeed;
			public KinematicSettings Ground;
			public KinematicSettings Air;
			public KinematicSettings Strafe;
		}

		[Serializable]
		public struct KinematicSettings
		{
			[Range(0f, 50f)] public float MaxSpeed;
			[Range(0f, 50f)] public float Acceleration;
			[Range(0f, 50f)] public float Deceleration;
		}

		[Serializable]
		public struct MotionState
		{
			public Vector3 Velocity;
			public bool IsGrounded;
		}
	}
}