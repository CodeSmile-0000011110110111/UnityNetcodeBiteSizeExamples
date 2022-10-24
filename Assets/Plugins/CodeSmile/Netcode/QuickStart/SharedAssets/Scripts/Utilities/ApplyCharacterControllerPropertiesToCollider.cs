using UnityEngine;

namespace CodeSmile.Netcode.QuickStart
{
	[RequireComponent(typeof(CapsuleCollider))]
	public class ApplyCharacterControllerPropertiesToCollider : MonoBehaviour
	{
		[SerializeField] private CharacterController _characterController;

		private void OnEnable()
		{
			if (_characterController != null)
			{
				var collider = GetComponent<CapsuleCollider>();
				collider.radius = _characterController.radius;
				collider.height = _characterController.height;
				collider.center = _characterController.center;
				collider.material = _characterController.material;
			}
			
			if (Application.isPlaying)
				Destroy(this);
		}
	}
}