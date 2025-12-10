using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace PortalSystem
{
    [HelpURL("https://assetstore.unity.com/packages/slug/225954")]
    [RequireComponent(typeof(AudioSource))]
    public class Joystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [SerializeField]
        private Transform cam, heldObjects;

        [SerializeField]
        private CharacterController characterController;

        [SerializeField]
        private RectTransform stick, center;

        [SerializeField]
        private AudioClip[] footsteps;

        [SerializeField]
        private float radius, movingSpeed, gravity, maxPlayerYPos, minxPlayerYPos, inaccuracy, steppingSpeed, steppingRatio;

        private AudioSource walk;
        private Vector2 joystickPos;
        private float distance;
        private int stepNumber;
        private bool isClick, isCame;

        /// <summary>
        /// Pressing the joystick
        /// </summary>
        /// <param name="data"></param>
        public void OnPointerDown(PointerEventData data)
        {
            isClick = true;
            SetJoystickUI(1.1f, 1f);
        }

        /// <summary>
        /// Holding the joystick in the circle area
        /// </summary>
        /// <param name="data"></param>
        public void OnDrag(PointerEventData data)
        {
            if (Vector2.Distance(center.position, data.position) < distance)
                stick.position = data.position;
            else
                stick.position = center.position + (new Vector3(data.position.x, data.position.y, 0f) - center.position).normalized * distance;
            joystickPos = (stick.position - center.position) / distance * radius;
        }

        /// <summary>
        /// Releasing the joystick
        /// </summary>
        /// <param name="data"></param>
        public void OnPointerUp(PointerEventData data)
        {
            isClick = false;
            stick.position = center.position;
            joystickPos = Vector2.zero;
            SetJoystickUI(1, 0.5f);
        }

        /// <summary>
        /// Setting up the joystick display
        /// </summary>
        /// <param name="scale"></param>
        /// <param name="opacity"></param>
        private void SetJoystickUI(float scale, float opacity)
        {
            stick.localScale = Vector3.one * scale;
            stick.GetComponent<Image>().color = new Color(1f, 1f, 1f, opacity);
            GetComponent<Image>().color = new Color(1f, 1f, 1f, opacity * 0.8f);
        }

        /// <summary>
        /// Preparing the joystick for operation
        /// </summary>
        private void Start()
        {
            isClick = false;
            walk = GetComponent<AudioSource>();
            distance = Screen.width * Screen.width / Screen.height * 0.06f * ((float)Screen.height / Screen.width);
        }

        /// <summary>
        /// Movement of the player depending on the position of the joystick
        /// </summary>
        private void Update()
        {
            if (isClick)
            {
                characterController.Move(((characterController.transform.forward * joystickPos.y + characterController.transform.right * joystickPos.x) * movingSpeed + Vector3.up * gravity) * Time.deltaTime);
                if (isCame)
                {
                    cam.localPosition = Vector3.Lerp(cam.localPosition, Vector3.up * maxPlayerYPos, Time.deltaTime * steppingSpeed * characterController.velocity.magnitude);
                    if (cam.localPosition.y > maxPlayerYPos - inaccuracy)
                        isCame = false;
                }
                else
                {
                    cam.localPosition = Vector3.Lerp(cam.localPosition, Vector3.up * minxPlayerYPos, Time.deltaTime * steppingSpeed * steppingRatio * characterController.velocity.magnitude);
                    if (cam.localPosition.y < minxPlayerYPos + inaccuracy)
                    {
                        isCame = true;
                        walk.PlayOneShot(footsteps[stepNumber++ % footsteps.Length]);
                    }
                }
            }
        }
    }
}