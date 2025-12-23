using UnityEngine;

public class MouseLook : MonoBehaviour
{
    // Start with a small value, we'll tweak in Inspector
    public float mouseSensitivity = 0.08f;
    public Transform playerBody;

    private float xRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // --- FIX: STOP CAMERA IF PAUSED ---
        // If the PauseMenu says we are paused, stop reading mouse input instantly.
        if (PauseMenu.isPaused) return; 

        // Raw mouse input (no smoothing)
        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Up/down rotation (camera)
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        // Left/right rotation (body)
        playerBody.Rotate(Vector3.up * mouseX);
    }
}