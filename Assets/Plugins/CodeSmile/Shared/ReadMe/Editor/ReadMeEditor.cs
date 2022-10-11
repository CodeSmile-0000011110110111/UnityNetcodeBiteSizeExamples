// Copyright (C) 2021-2022 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using UnityEditor;
using UnityEngine;

namespace CodeSmile.ReadMe.Editor
{
	[CustomEditor(typeof(ReadMeScriptableObject))]
	[InitializeOnLoad]
	public class ReadmeEditor : UnityEditor.Editor
	{
		private static readonly string kShowedReadmeSessionStateName = "CodeSmileReadmeEditor.showedReadme";

		private static readonly float kSpace = 16f;
		[SerializeField] private GUIStyle m_LinkStyle;
		[SerializeField] private GUIStyle m_TitleStyle;
		[SerializeField] private GUIStyle m_HeadingStyle;
		[SerializeField] private GUIStyle m_BodyStyle;

		private bool m_Initialized;

		private static void SelectReadmeAutomatically()
		{
			if (!SessionState.GetBool(kShowedReadmeSessionStateName, false))
			{
				SelectReadme();
				SessionState.SetBool(kShowedReadmeSessionStateName, true);
			}
		}

		//[MenuItem("Tutorial/Show Tutorial Instructions")]
		private static ReadMeScriptableObject SelectReadme()
		{
			var ids = AssetDatabase.FindAssets("README t:ReadMeScriptableObject");
			if (ids.Length == 1)
			{
				var readmeObject = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(ids[0]));
				Selection.objects = new[] { readmeObject };
				return (ReadMeScriptableObject)readmeObject;
			}
			Debug.Log("Couldn't find a readme");
			return null;
		}

		static ReadmeEditor() => EditorApplication.delayCall += SelectReadmeAutomatically;

		private void Init()
		{
			if (m_Initialized)
				return;

			m_BodyStyle = new GUIStyle(EditorStyles.label);
			m_BodyStyle.wordWrap = true;
			m_BodyStyle.fontSize = 14;

			m_TitleStyle = new GUIStyle(m_BodyStyle);
			m_TitleStyle.fontSize = 26;

			m_HeadingStyle = new GUIStyle(m_BodyStyle);
			m_HeadingStyle.fontSize = 18;

			m_LinkStyle = new GUIStyle(m_BodyStyle);
			m_LinkStyle.wordWrap = false;
			// Match selection color which works nicely for both light and dark skins
			m_LinkStyle.normal.textColor = new Color(0x00 / 255f, 0x78 / 255f, 0xDA / 255f, 1f);
			m_LinkStyle.stretchWidth = false;

			m_Initialized = true;
		}

		private GUIStyle LinkStyle => m_LinkStyle;

		private GUIStyle TitleStyle => m_TitleStyle;

		private GUIStyle HeadingStyle => m_HeadingStyle;

		private GUIStyle BodyStyle => m_BodyStyle;

		protected override void OnHeaderGUI()
		{
			var readme = (ReadMeScriptableObject)target;
			Init();

			var iconWidth = Mathf.Min(EditorGUIUtility.currentViewWidth / 3f - 20f, 128f);

			GUILayout.BeginHorizontal("In BigTitle");
			{
				GUILayout.Label(readme.icon, GUILayout.Width(iconWidth), GUILayout.Height(iconWidth));
				GUILayout.Label(readme.title, TitleStyle);
			}
			GUILayout.EndHorizontal();
		}

		public override void OnInspectorGUI()
		{
			var readme = (ReadMeScriptableObject)target;
			Init();

			if (readme.editing)
			{
				DrawDefaultInspector();
				return;
			}

			if (readme.sections != null)
			{
				foreach (var section in readme.sections)
				{
					if (!string.IsNullOrEmpty(section.heading))
						GUILayout.Label(section.heading, HeadingStyle);
					if (!string.IsNullOrEmpty(section.text))
						GUILayout.Label(section.text, BodyStyle);
					if (!string.IsNullOrEmpty(section.linkText))
					{
						if (LinkLabel(new GUIContent(section.linkText)))
							Application.OpenURL(section.url);
					}
					GUILayout.Space(kSpace);
				}
			}
		}

		private bool LinkLabel(GUIContent label, params GUILayoutOption[] options)
		{
			var position = GUILayoutUtility.GetRect(label, LinkStyle, options);

			Handles.BeginGUI();
			Handles.color = LinkStyle.normal.textColor;
			Handles.DrawLine(new Vector3(position.xMin, position.yMax), new Vector3(position.xMax, position.yMax));
			Handles.color = Color.white;
			Handles.EndGUI();

			EditorGUIUtility.AddCursorRect(position, MouseCursor.Link);

			return GUI.Button(position, label, LinkStyle);
		}
	}
}