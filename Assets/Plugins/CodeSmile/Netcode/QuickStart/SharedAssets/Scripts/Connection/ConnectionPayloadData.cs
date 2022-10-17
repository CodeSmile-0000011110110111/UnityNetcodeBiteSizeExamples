// Copyright (C) 2021-2022 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;

namespace CodeSmile.Netcode
{
	[Serializable]
	public struct ConnectionPayloadData
	{
		public NetworkPlayerData PlayerData;
		public string PasswordHash;

		public override string ToString() => $"Payload({PlayerData}, PasswordHash: {PasswordHash})";
	}
}