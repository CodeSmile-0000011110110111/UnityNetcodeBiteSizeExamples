// Copyright (C) 2021-2022 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;

namespace CodeSmile.Netcode.QuickStart
{
	/// <summary>
	/// Custom data for players.
	/// </summary>
	[Serializable]
	public struct NetworkPlayerData
	{
		public static Encoding NameEncoding = Encoding.UTF8;
		public static int NameMaxLength = 32;

		public bool Connected;
		public ulong ClientId;
		//public Guid PlayerId; 
		public string Name;
		public Color32 Color;
		[FormerlySerializedAs("Position")] public Vector3 StartPosition;
		[FormerlySerializedAs("Rotation")] public Quaternion StartRotation;

		public override string ToString() => $"PlayerData(Name:{Name}, StartPosition:{StartPosition})";
	}
}