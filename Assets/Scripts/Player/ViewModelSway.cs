using UnityEngine;

public class ViewmodelSway : MonoBehaviour
{
    [Header("Mouse sway (viewmodel only)")]
    public float swayAmount = 0.04f;
    public float swayMaxAmount = 0.08f;
    public float swaySmooth = 8f;

    [Header("Shared motion clock")]
    public PlayerMotionClock motionClock; // drag [Player] here

    [Header("Walk bob (synced)")]
    public CharacterController playerController;  // drag [Player] here
    public float bobAmount = 0.04f;
    public float moveThreshold = 0.05f;

    [Header("Idle breathing (synced)")]
    public float idleAmount = 0.01f;

    private Vector3 initialLocalPos;
    private Vector3 currentOffset;

    void Start()
    {
        initialLocalPos = transform.localPosition;
    }

    void Update()
    {
        // ---------- 1. Mouse sway ----------
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        Vector3 swayOffset = new Vector3(
            -mouseX * swayAmount,
            -mouseY * swayAmount,
            0f
        );

        swayOffset.x = Mathf.Clamp(swayOffset.x, -swayMaxAmount, swayMaxAmount);
        swayOffset.y = Mathf.Clamp(swayOffset.y, -swayMaxAmount, swayMaxAmount);

        // ---------- 2. Synced bob + idle (shared phase) ----------
        float phase = (motionClock != null) ? motionClock.Phase : 0f;

        bool moving =
            playerController != null &&
            playerController.isGrounded &&
            playerController.velocity.magnitude > moveThreshold;

        Vector3 bobOffset = Vector3.zero;
        if (moving)
        {
            bobOffset.y = Mathf.Sin(phase * 2f) * bobAmount;
            bobOffset.x = Mathf.Cos(phase) * bobAmount * 0.5f;
        }

        Vector3 idleOffset = new Vector3(0f, Mathf.Sin(phase) * idleAmount, 0f);

        Vector3 targetOffset = swayOffset + bobOffset + idleOffset;

        // ---------- 3. Apply ----------
        currentOffset = Vector3.Lerp(currentOffset, targetOffset, Time.deltaTime * swaySmooth);
        transform.localPosition = initialLocalPos + currentOffset;
    }
}
