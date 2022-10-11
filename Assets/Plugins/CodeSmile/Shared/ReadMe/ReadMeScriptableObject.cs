// Copyright (C) 2021-2022 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEngine;

namespace CodeSmile.ReadMe
{
	[CreateAssetMenu(fileName = "README", menuName = "Readme", order = 0)]
	public class ReadMeScriptableObject : ScriptableObject
	{
		public bool editing = true;
		public Texture2D icon;
		public string title;
		public Section[] sections;
	
		[Serializable]
		public class Section 
		{
			public string heading, text, linkText, url;
		}
	}
}
