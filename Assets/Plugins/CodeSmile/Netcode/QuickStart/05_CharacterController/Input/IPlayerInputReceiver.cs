// Copyright (C) 2021-2022 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

namespace CodeSmile.Netcode.QuickStart
{
	public interface IPlayerInputReceiver
	{
		public PlayerInputState CurrentState { get; }
		public bool CancelPressed { get; }
	}
}