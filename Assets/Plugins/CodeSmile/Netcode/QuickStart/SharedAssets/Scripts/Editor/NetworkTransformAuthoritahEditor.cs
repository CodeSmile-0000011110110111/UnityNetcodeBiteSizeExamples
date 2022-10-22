// Copyright (C) 2021-2022 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using Unity.Netcode.Editor;
using UnityEditor;

namespace CodeSmile.Netcode.QuickStart.Editor
{
	[CustomEditor(typeof(NetworkTransformAuthoritah))]
	public class NetworkTransformAuthoritahEditor : NetworkTransformEditor
	{
		private SerializedProperty m_Authoritah;

		public override void OnInspectorGUI()
		{
			if (m_Authoritah == null)
				m_Authoritah = serializedObject.FindProperty(nameof(NetworkTransformAuthoritah.Authoritah));
			
			EditorGUILayout.PropertyField(m_Authoritah);
			EditorGUILayout.Space();
			
			base.OnInspectorGUI();
		}
	}
}