// Copyright (C) 2021-2022 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEngine;

namespace CodeSmile.Netcode.BiteSize.QuickStart
{
	[Serializable]
	public struct PlayerData
	{
		public bool Connected;
		public ulong ClientId;
		//public Guid PlayerId; 
		public string Name;
		public Color32 Color;
		public Vector3 Position;
		public Quaternion Rotation;
		
		public override string ToString() => $"PlayerData - Name: {Name}";
	}
}