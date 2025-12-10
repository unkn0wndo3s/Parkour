using UnityEngine;

namespace PortalSystem
{
	[HelpURL("https://assetstore.unity.com/packages/slug/225954")]
	public class PortalTeleporter : MonoBehaviour
	{
		public static bool isSecondWorld;

		[SerializeField]
		private GameObject[] objs;

		[SerializeField]
		private BoxCollider[] borders;

		[SerializeField]
		private BoxCollider col;

		[SerializeField]
		private Camera cameraPlayer, cameraA, cameraB;

		[SerializeField]
		private Transform player, gun, portalA, portalB;

		private bool isCan;

		/// <summary>
		/// Tracking the player's entry into the portal
		/// </summary>
		/// <param name="other"></param>
		private void OnTriggerEnter(Collider other)
		{
			if (other.name != "Player")
				return;
			isCan = true;
			for (int i = 0; i < borders.Length; i++)
				borders[i].enabled = false;
			cameraA.nearClipPlane = 0.01f;
			cameraB.nearClipPlane = 0.01f;
		}

		/// <summary>
		/// Resetting static field data
		/// </summary>
		private void Awake()
		{
			isSecondWorld = false;
		}

		/// <summary>
		/// Controlling the correct display of worlds depending on where the player is located
		/// </summary>
		private void Update()
		{
			col.enabled = (transform.position.z - player.position.z) * (isSecondWorld ? -1f : 1f) > 0f;
			if (isCan && Vector3.Distance(new Vector3(player.position.x, 0, player.position.z), transform.position) > 1.5f)
			{
				isCan = false;
				for (int i = 0; i < borders.Length; i++)
					borders[i].enabled = true;
			}
			float distance = Vector3.Distance(cameraPlayer.transform.position + (isSecondWorld ? Vector3.zero : (portalB.position - portalA.position)), transform.position) - 1f;
			if (isSecondWorld && cameraA.enabled)
				cameraA.nearClipPlane = distance;
			else if (!isSecondWorld && cameraB.enabled)
				cameraB.nearClipPlane = distance;
			if (!isCan)
				return;
			if (Vector3.Dot(transform.forward, player.position - transform.position) > 0.2f)
			{
				for (int i = 0; i < objs.Length; i++)
					objs[i].SetActive(!objs[i].activeSelf);
				cameraA.enabled = !cameraA.enabled;
				cameraB.enabled = !cameraB.enabled;
				cameraPlayer.backgroundColor = isSecondWorld ? cameraA.backgroundColor : cameraB.backgroundColor;
				player.GetComponent<CharacterController>().enabled = false;
				player.position += isSecondWorld ? (portalA.position - portalB.position) : (portalB.position - portalA.position);
				player.GetComponent<CharacterController>().enabled = true;
				isSecondWorld = !isSecondWorld;
			}
		}
	}
}