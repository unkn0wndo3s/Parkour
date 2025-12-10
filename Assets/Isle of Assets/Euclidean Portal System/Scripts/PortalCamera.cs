using UnityEngine;

namespace PortalSystem
{
	[HelpURL("https://assetstore.unity.com/packages/slug/225954")]
	public class PortalCamera : MonoBehaviour
	{
		[SerializeField]
		private Transform playerCamera, portalA, portalB;

		[SerializeField]
		private CameraType cameraType;

		private enum CameraType { A, B }

		/// <summary>
		/// Placing a player in one of the places (A and B)
		/// </summary>
		private void Update()
		{
			transform.position = playerCamera.position;
			if (PortalTeleporter.isSecondWorld)
				transform.position += cameraType == CameraType.A ? (portalA.position - portalB.position) : Vector3.zero;
			else
				transform.position += cameraType == CameraType.A ? Vector3.zero : (portalB.position - portalA.position);
			transform.rotation = playerCamera.rotation;
		}
	}
}