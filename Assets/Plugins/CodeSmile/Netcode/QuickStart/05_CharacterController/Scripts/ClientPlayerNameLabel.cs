// Copyright (C) 2021-2022 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using TMPro;
using UnityEngine;

namespace CodeSmile.Netcode.QuickStart
{
	public class ClientPlayerNameLabel : MonoBehaviour
	{
		[SerializeField] private TMP_Text _label;

		public void SetPlayerName(string name) => _label.text = name;
	}
}