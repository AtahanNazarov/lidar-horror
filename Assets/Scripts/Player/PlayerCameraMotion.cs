using UnityEngine;

/// <summary>
/// Smooth FPS camera bob + positional sway + jump/land kick,
/// layered on top of whatever PlayerMovement does (including crouch).
/// Attach to PlayerCamera. It ONLY changes localPosition.
/// </summary>
public class PlayerCameraMotion : MonoBehaviour
{
    [Header("Required references")]
    public CharacterController controller;   // drag [Player] here
    public PlayerMovement playerMovement;    // drag [Player] here (for speed refs)

    [Header("Head bob amplitudes")]
    public float idleAmplitude = 0.01f;
    public float walkAmplitude = 0.04f;
    public float runAmplitude  = 0.07f;

    [Header("Head bob frequencies")]
    public float idleFrequency = 1.2f;
    public float walkFrequency = 6f;
    public float runFrequency  = 9f;

    [Header("Mouse positional sway")]
    public float swayHorizontalAmount = 0.03f;
    public float swayVerticalAmount   = 0.02f;
    public float swayMaxOffset        = 0.06f;

    [Header("Smoothing")]
    public float bobSmooth  = 10f;
    public float swaySmooth = 12f;
    public float moveThreshold = 0.01f;  // how much input counts as "moving"

    [Header("Jump / Land camera kick")]
    public float jumpKickAmount   = 0.04f;  // how much camera bumps up on jump
    public float jumpKickDuration = 0.15f;  // duration of jump kick

    public float landKickAmount   = 0.06f;  // how much camera dips on landing
    public float landKickDuration = 0.12f;  // duration of landing kick

    private float walkSpeedRef = 2.4f;
    private float runSpeedRef  = 4.8f;

    private float bobTimer;
    private Vector3 currentBobOffset;
    private Vector3 currentSwayOffset;
    private Vector3 lastTotalOffset;

    // jump/land state
    private bool wasGrounded = true;
    private float jumpKickTimer;
    private float landKickTimer;

    void Start()
    {
        if (playerMovement != null)
        {
            walkSpeedRef = playerMovement.speed;
            runSpeedRef  = playerMovement.speed * playerMovement.sprintMultiplier;
        }

        if (controller != null)
            wasGrounded = controller.isGrounded;
    }

    // LateUpdate so movement & crouch already updated camera position
    void LateUpdate()
    {
        if (controller == null)
            return;

        // --- detect jump / landing events ---
        bool grounded = controller.isGrounded;
        float verticalVel = controller.velocity.y;

        // Just left the ground and going up → jump
        if (!grounded && wasGrounded && verticalVel > 0.1f)
        {
            jumpKickTimer = jumpKickDuration;
        }

        // Just hit the ground coming down → landing
        if (grounded && !wasGrounded && verticalVel < -0.1f)
        {
            landKickTimer = landKickDuration;
        }

        wasGrounded = grounded;

        // --- remove last frame's offsets to get the "base" position ---
        Vector3 basePos = transform.localPosition - lastTotalOffset;

        // --- calculate offsets ---
        Vector3 targetBob  = CalculateBobOffset();
        Vector3 targetSway = CalculateSwayOffset();
        Vector3 jumpLandOffset = CalculateJumpLandOffset();

        currentBobOffset = Vector3.Lerp(currentBobOffset,  targetBob,  Time.deltaTime * bobSmooth);
        currentSwayOffset = Vector3.Lerp(currentSwayOffset, targetSway, Time.deltaTime * swaySmooth);

        Vector3 totalOffset = currentBobOffset + currentSwayOffset + jumpLandOffset;

        // --- apply final position ---
        transform.localPosition = basePos + totalOffset;
        lastTotalOffset = totalOffset;
    }

    Vector3 CalculateBobOffset()
    {
        // Movement based on WASD input, not only velocity
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputZ = Input.GetAxisRaw("Vertical");
        Vector2 input = new Vector2(inputX, inputZ);
        bool hasInput = input.sqrMagnitude > moveThreshold * moveThreshold;

        bool isGrounded = controller.isGrounded;
        bool isMoving = hasInput && isGrounded;

        float amp;
        float freq;

        if (!isMoving)
        {
            amp = idleAmplitude;
            freq = idleFrequency;
        }
        else
        {
            bool wantsSprint = Input.GetKey(KeyCode.LeftShift);
            amp  = wantsSprint ? runAmplitude  : walkAmplitude;
            freq = wantsSprint ? runFrequency  : walkFrequency;
        }

        bobTimer += Time.deltaTime * freq;

        // Classic FPS pattern: little X + stronger Y
        float bobX = Mathf.Cos(bobTimer) * amp * 0.4f;
        float bobY = Mathf.Sin(bobTimer * 2f) * amp;

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

        // Upward kick on jump (fast up, ease back)
        if (jumpKickTimer > 0f)
        {
            float t = 1f - (jumpKickTimer / Mathf.Max(jumpKickDuration, 0.0001f)); // 0→1
            y += Mathf.Sin(t * Mathf.PI) * jumpKickAmount;
            jumpKickTimer -= Time.deltaTime;
            if (jumpKickTimer < 0f) jumpKickTimer = 0f;
        }

        // Downward dip on landing (fast down, ease back)
        if (landKickTimer > 0f)
        {
            float t = 1f - (landKickTimer / Mathf.Max(landKickDuration, 0.0001f)); // 0→1
            y -= Mathf.Sin(t * Mathf.PI) * landKickAmount;
            landKickTimer -= Time.deltaTime;
            if (landKickTimer < 0f) landKickTimer = 0f;
        }

        return new Vector3(0f, y, 0f);
    }
}
