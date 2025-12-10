using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace PortalSystem
{
    [HelpURL("https://assetstore.unity.com/packages/slug/225954")]
    public class FireButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [SerializeField]
        private Gun gun;

        [SerializeField]
        private CameraRotation cameraRotation;

        /// <summary>
        /// Pressing the firing button
        /// </summary>
        /// <param name="data"></param>
        public void OnPointerDown(PointerEventData data)
        {
            GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.2f);
            gun.Shoot();
            cameraRotation.OnPointerDown(data);
        }

        /// <summary>
        /// Simultaneous pressing of the button and rotation of the camera
        /// </summary>
        /// <param name="data"></param>
        public void OnDrag(PointerEventData data)
        {
            cameraRotation.OnDrag(data);
        }

        /// <summary>
        /// Releasing the firing button
        /// </summary>
        /// <param name="data"></param>
        public void OnPointerUp(PointerEventData data)
        {
            GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.2f);
            gun.StopShoot();
            cameraRotation.OnPointerUp(data);
        }
    }
}