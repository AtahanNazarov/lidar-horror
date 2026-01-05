using UnityEngine;

public class PlayerCameraMotion : MonoBehaviour
{
    [Header("Required references")]
    public CharacterController controller;   // drag [Player] here
    public PlayerMovement playerMovement;    // drag [Player] here

    [Header("Shared motion clock")]
    public PlayerMotionClock motionClock;    // drag [Player] here

    [Header("Head bob amplitudes")]
    public float idleAmplitude = 0.01f;
    public float walkAmplitude = 0.04f;
    public float runAmplitude  = 0.07f;

    [Header("Mouse positional sway")]
    public float swayHorizontalAmount = 0.03f;
    public float swayVerticalAmount   = 0.02f;
    public float swayMaxOffset        = 0.06f;

    [Header("Smoothing")]
    public float bobSmooth  = 10f;
    public float swaySmooth = 12f;
    public float moveThreshold = 0.01f;

    [Header("Jump / Land camera kick")]
    public float jumpKickAmount   = 0.04f;
    public float jumpKickDuration = 0.15f;

    public float landKickAmount   = 0.06f;
    public float landKickDuration = 0.12f;

    private Vector3 currentBobOffset;
    private Vector3 currentSwayOffset;
    private Vector3 lastTotalOffset;

    private bool wasGrounded = true;
    private float jumpKickTimer;
    private float landKickTimer;

    void Start()
    {
        if (controller != null)
            wasGrounded = controller.isGrounded;
    }

    void LateUpdate()
    {
        if (controller == null)
            return;

        bool grounded = controller.isGrounded;
        float verticalVel = controller.velocity.y;

        if (!grounded && wasGrounded && verticalVel > 0.1f)
            jumpKickTimer = jumpKickDuration;

        if (grounded && !wasGrounded && verticalVel < -0.1f)
            landKickTimer = landKickDuration;

        wasGrounded = grounded;

        Vector3 basePos = transform.localPosition - lastTotalOffset;

        Vector3 targetBob  = CalculateBobOffset();
        Vector3 targetSway = CalculateSwayOffset();
        Vector3 jumpLandOffset = CalculateJumpLandOffset();

        currentBobOffset  = Vector3.Lerp(currentBobOffset,  targetBob,  Time.deltaTime * bobSmooth);
        currentSwayOffset = Vector3.Lerp(currentSwayOffset, targetSway, Time.deltaTime * swaySmooth);

        Vector3 totalOffset = currentBobOffset + currentSwayOffset + jumpLandOffset;

        transform.localPosition = basePos + totalOffset;
        lastTotalOffset = totalOffset;
    }

    Vector3 CalculateBobOffset()
    {
        float phase = (motionClock != null) ? motionClock.Phase : 0f;

        float inputX = Input.GetAxisRaw("Horizontal");
        float inputZ = Input.GetAxisRaw("Vertical");
        bool hasInput = (inputX * inputX + inputZ * inputZ) > moveThreshold * moveThreshold;

        bool isGrounded = controller.isGrounded;
        bool isMoving = hasInput && isGrounded;

        float amp;
        if (!isMoving)
            amp = idleAmplitude;
        else
            amp = Input.GetKey(KeyCode.LeftShift) ? runAmplitude : walkAmplitude;

        float bobX = Mathf.Cos(phase) * amp * 0.4f;
        float bobY = Mathf.Sin(phase * 2f) * amp;

        return new Vector3(bobX, bobY, 0f);
    }

    Vector3 CalculateSwayOffset()
    {
        float mouseX = Input.GetAxisRaw("Mouse X");
        float mouseY = Input.GetAxisRaw("Mouse Y");

        float offsetX = -mouseX * swayHorizontalAmount;
        float offsetY = -mouseY * swayVerticalAmount;

        offsetX = Mathf.Clamp(offsetX, -swayMaxOffset, swayMaxOffset);
        offsetY = Mathf.Clamp(offsetY, -swayMaxOffset, swayMaxOffset);

        return new Vector3(offsetX, offsetY, 0f);
    }

    Vector3 CalculateJumpLandOffset()
    {
        float y = 0f;

        if (jumpKickTimer > 0f)
        {
            float t = 1f - (jumpKickTimer / Mathf.Max(jumpKickDuration, 0.0001f));
            y += Mathf.Sin(t * Mathf.PI) * jumpKickAmount;
            jumpKickTimer -= Time.deltaTime;
            if (jumpKickTimer < 0f) jumpKickTimer = 0f;
        }

        if (landKickTimer > 0f)
        {
            float t = 1f - (landKickTimer / Mathf.Max(landKickDuration, 0.0001f));
            y -= Mathf.Sin(t * Mathf.PI) * landKickAmount;
            landKickTimer -= Time.deltaTime;
            if (landKickTimer < 0f) landKickTimer = 0f;
        }

        return new Vector3(0f, y, 0f);
    }
}
