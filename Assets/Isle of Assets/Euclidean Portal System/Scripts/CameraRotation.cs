using UnityEngine;
using UnityEngine.EventSystems;

namespace PortalSystem
{
    [HelpURL("https://assetstore.unity.com/packages/slug/225954")]
    public class CameraRotation : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [SerializeField]
        private Transform cam, player;

        [SerializeField]
        private float sensitivity;

        private Vector2 lastPos, pos, savePos;

        /// <summary>
        /// Saving the touch position
        /// </summary>
        /// <param name="data"></param>
        public void OnPointerDown(PointerEventData data)
        {
            savePos = data.position;
        }

        /// <summary>
        /// Camera rotation
        /// </summary>
        /// <param name="data"></param>
        public void OnDrag(PointerEventData data)
        {
            pos = lastPos + (data.position - savePos) * sensitivity;
            Quaternion rotate = Quaternion.AngleAxis(pos.x, Vector3.up) *
                                Quaternion.AngleAxis(Mathf.Clamp(pos.y, -80f, 80f), -Vector3.right);
            player.eulerAngles = new Vector3(0, rotate.eulerAngles.y, 0f);
            cam.eulerAngles = new Vector3(rotate.eulerAngles.x, cam.eulerAngles.y, 0f);
        }

        /// <summary>
        /// Saving the last touch position
        /// </summary>
        /// <param name="data"></param>
        public void OnPointerUp(PointerEventData data)
        {
            lastPos = pos;
        }
    }
}