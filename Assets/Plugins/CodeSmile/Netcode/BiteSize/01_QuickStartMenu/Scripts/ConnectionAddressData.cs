// Copyright (C) 2021-2022 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;

namespace CodeSmile.Netcode.BiteSize.QuickStart
{
	[Serializable]
	public struct ConnectionAddressData
	{
		public const string DefaultListenAddress = "0.0.0.0";

		public string Address;
		public string Port;
	}
}