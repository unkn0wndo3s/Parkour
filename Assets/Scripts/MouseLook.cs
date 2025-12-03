using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [Header("Refs")]
    public Transform playerBody; // le Player (capsule)

    [Header("Settings")]
    public float mouseSensitivity = 200f;

    private float xRotation = 0f;

    private void Start()
    {
        // Au cas où, on s'assure que le curseur est lock
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {

        // Si le jeu est en pause (Time.timeScale == 0), on ne bouge plus la caméra
        if (Time.timeScale == 0f)
            return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.unscaledDeltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.unscaledDeltaTime;

        // Pitch (regarder haut/bas) sur la caméra
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -85f, 85f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Yaw (tourner gauche/droite) sur le corps du player
        if (playerBody != null)
        {
            playerBody.Rotate(Vector3.up * mouseX);
        }
    }
}
