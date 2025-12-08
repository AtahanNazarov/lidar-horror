using UnityEngine;

public class ViewmodelSway : MonoBehaviour
{
    [Header("Mouse sway")]
    public float swayAmount = 0.04f;
    public float swayMaxAmount = 0.08f;
    public float swaySmooth = 8f;

    [Header("Walk bob (item only)")]
    public CharacterController playerController;  // drag [Player] here
    public float bobAmount = 0.04f;
    public float bobFrequency = 9f;
    public float moveThreshold = 0.05f;

    [Header("Idle breathing")]
    public float idleAmount = 0.01f;
    public float idleFrequency = 1.2f;

    private Vector3 initialLocalPos;
    private float bobTimer;
    private float idleTimer;

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

        // ---------- 2. Walk bob ----------
        Vector3 bobOffset = Vector3.zero;

        if (playerController != null &&
            playerController.isGrounded &&
            playerController.velocity.magnitude > moveThreshold)
        {
            bobTimer += Time.deltaTime * bobFrequency;
            bobOffset.y = Mathf.Sin(bobTimer) * bobAmount;
            bobOffset.x = Mathf.Cos(bobTimer * 0.5f) * bobAmount * 0.5f;
        }
        else
        {
            bobTimer = 0f;
        }

        // ---------- 3. Idle breathing (always on) ----------
        idleTimer += Time.deltaTime * idleFrequency;
        Vector3 idleOffset = new Vector3(
            0f,
            Mathf.Sin(idleTimer) * idleAmount,
            0f
        );

        // ---------- 4. Apply ----------
        Vector3 targetPos = initialLocalPos + swayOffset + bobOffset + idleOffset;

        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            targetPos,
            Time.deltaTime * swaySmooth
        );
    }
}
