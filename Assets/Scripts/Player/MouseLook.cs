using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public float mouseSensitivity = 0.08f;
    public Transform playerBody;

    private float xRotation = 0f;

    void Start()
    {
        // Initial lock when the scene starts
        LockCursor();
    }

    void Update()
    {
        // --- FIX: CURSOR STATE MANAGEMENT ---
        // If the game is not paused, but the cursor has somehow become unlocked 
        // (common when switching scenes), force it back to locked.
        if (!PauseMenu.isPaused && Cursor.lockState != CursorLockMode.Locked)
        {
            LockCursor();
        }

        // --- STOP CAMERA IF PAUSED ---
        if (PauseMenu.isPaused) return; 

        // Raw mouse input
        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Up/down rotation (camera)
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        // Left/right rotation (body)
        playerBody.Rotate(Vector3.up * mouseX);
    }

    // Helper method to ensure cursor is hidden and locked
    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}