// Copyright (C) 2021-2022 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using UnityEngine;

namespace CodeSmile.Netcode.QuickStart
{
	[RequireComponent(typeof(CharacterController))]
	public class PlayerAnimationEvents : MonoBehaviour
	{
		public AudioClip _landingAudioClip;
		[Range(0f, 1f)] public float _landAudioVolume = 0.5f;
		[Range(0f, 1f)] public float _landMinBlendWeight = 0.3f;
		public AudioClip[] _footstepAudioClips;
		[Range(0f, 1f)] public float _footstepAudioVolume = 0.5f;
		[Range(0f, 1f)] public float _footstepMinBlendWeight = 0.5f;

		private CharacterController _characterController;

		private void Start() => _characterController = GetComponent<CharacterController>();

		private void OnLand(AnimationEvent animationEvent)
		{
			//Debug.Log(animationEvent.animatorClipInfo.weight);
			if (animationEvent.animatorClipInfo.weight >= _landMinBlendWeight)
			{
				var pos = transform.TransformPoint(_characterController.center);
				AudioSource.PlayClipAtPoint(_landingAudioClip, pos, _landAudioVolume);
			}
		}

		private void OnFootstep(AnimationEvent animationEvent)
		{
			if (animationEvent.animatorClipInfo.weight >= _footstepMinBlendWeight)
			{
				if (_footstepAudioClips.Length > 0)
				{
					var index = Random.Range(0, _footstepAudioClips.Length);
					var pos = transform.TransformPoint(_characterController.center);
					AudioSource.PlayClipAtPoint(_footstepAudioClips[index], pos, _footstepAudioVolume);
				}
			}
		}
	}
}