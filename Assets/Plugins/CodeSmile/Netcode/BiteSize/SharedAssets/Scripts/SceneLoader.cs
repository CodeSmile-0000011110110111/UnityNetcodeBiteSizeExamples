// Copyright (C) 2021-2022 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CodeSmile.Netcode.BiteSize.SceneManagement
{
	public sealed class SceneLoader : MonoBehaviour
	{
		private string _sceneName;

		private void Update()
		{
			Debug.Log($"Try load scene: {_sceneName}");
			SceneManager.LoadScene(_sceneName);
		}

#if UNITY_EDITOR
		[SerializeField] private SceneAsset _sceneToLoad;
		private void OnValidate() => _sceneName = _sceneToLoad != null ? _sceneToLoad.name : null;
#endif
	}
}