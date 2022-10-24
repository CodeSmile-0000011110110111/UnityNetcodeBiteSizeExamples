// Copyright (C) 2021-2022 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEngine;

namespace CodeSmile.Netcode.QuickStart
{
	/// <summary>
	/// This is the state of the input that could be recorded or transferred over network to replicate the player's actions.
	/// </summary>
	[Serializable]
	public struct PlayerInputState
	{
		public Vector2 MoveDir;
		public Vector2 LookDir;
		public bool JumpPressed;
		public bool AttackPressed;

		public Vector3 GetMoveDir() => new(MoveDir.x, 0f, MoveDir.y);
		public Vector3 GetLookDir() => new(LookDir.x, 0f, LookDir.y);
	}
}