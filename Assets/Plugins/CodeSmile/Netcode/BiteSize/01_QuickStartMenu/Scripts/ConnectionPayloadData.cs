// Copyright (C) 2021-2022 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;

namespace CodeSmile.Netcode.BiteSize.QuickStart
{
	[Serializable]
	public struct ConnectionPayloadData
	{
		public string PasswordHash;
		public PlayerData PlayerData;

		public override string ToString() => $"ConnectionPayload - {PlayerData}, PwHash: {PasswordHash}";
	}
}