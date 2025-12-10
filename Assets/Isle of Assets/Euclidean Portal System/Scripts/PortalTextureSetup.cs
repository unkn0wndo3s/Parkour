using UnityEngine;

namespace PortalSystem
{
	[HelpURL("https://assetstore.unity.com/packages/slug/225954")]
	public class PortalTextureSetup : MonoBehaviour
	{
		[SerializeField]
		private Camera[] cameras;

		[SerializeField]
		private Material[] materials;

		private int saveScreenWidth, saveScreenHeight;

		/// <summary>
		/// Disabling Camera B
		/// </summary>
		private void Awake()
		{
			cameras[1].enabled = true;
		}

		/// <summary>
		/// Configuring RenderTexture
		/// </summary>
		private void Update()
		{
			if (saveScreenWidth == Screen.width && saveScreenHeight == Screen.height)
				return;
			for (int i = 0; i < 2; i++)
			{
				if (cameras[i].targetTexture != null)
					cameras[i].targetTexture.Release();
				saveScreenWidth = Screen.width;
				saveScreenHeight = Screen.height;
				cameras[i].targetTexture = new RenderTexture(saveScreenWidth, saveScreenHeight, 16);
				materials[i].mainTexture = cameras[i].targetTexture;
			}
		}
	}
}