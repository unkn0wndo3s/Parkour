using UnityEngine;

namespace PortalSystem
{
    [HelpURL("https://assetstore.unity.com/packages/slug/225954")]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField]
        private Transform cam, heldObjects;

        [SerializeField]
        private float followingSpeed;

        /// <summary>
        /// Animation of objects following the direction of the player's view in the player's hand
        /// </summary>
        private void Update()
        {
            heldObjects.rotation = Quaternion.Lerp(heldObjects.rotation, cam.rotation, followingSpeed * Time.deltaTime);
        }
    }
}