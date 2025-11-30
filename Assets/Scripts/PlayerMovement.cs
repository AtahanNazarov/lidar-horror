using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 2.4f;          // your comfy walk speed
    public float sprintMultiplier = 2f; // how much faster when sprinting

    [Header("Jumping")]
    public float jumpHeight = 1.0f;

    // Gravity split: softer going up, stronger falling down
    public float upwardGravity = -20f;   // affects how long you "hang" at top
    public float downwardGravity = -50f; // stronger pull down for snappy fall

    // Coyote time = small window after leaving ground where you can still jump
    public float coyoteTime = 0.1f;
    // Jump buffer = press jump slightly early and still get the jump
    public float jumpBufferTime = 0.1f;

    [Header("Stamina")]
    public float maxStamina = 5f;          // seconds of sprint at full drain rate
    public float staminaDrainRate = 1f;    // per second when sprinting
    public float staminaRegenRate = 1.5f;  // per second when not sprinting
    
    // Cost to jump
    public float jumpStaminaCost = 1.0f; 

    private float currentStamina;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

    private float coyoteTimeCounter;
    private float jumpBufferCounter;

    // Becomes true when you fully drain stamina.
    // While true, you cannot sprint again until stamina is fully recharged.
    private bool staminaLock = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        currentStamina = maxStamina; // start full stamina
    }

    void Update()
    {
        // ----------- Ground check & coyote time -----------
        isGrounded = controller.isGrounded;

        if (isGrounded)
        {
            // Reset coyote timer when on ground
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            // Count down when in air
            coyoteTimeCounter -= Time.deltaTime;
        }

        // ----------- Jump buffer -----------
        if (Input.GetButtonDown("Jump"))
        {
            // Player pressed jump: start buffer timer
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            // Count down jump buffer
            jumpBufferCounter -= Time.deltaTime;
        }

        // ----------- Movement (walk + stamina-based sprint) -----------
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        bool hasMoveInput = (Mathf.Abs(x) > 0.01f || Mathf.Abs(z) > 0.01f);
        bool wantsToSprint = Input.GetKey(KeyCode.LeftShift) && hasMoveInput;

        // Hard rules: if stamina is empty, lock sprint
        if (currentStamina <= 0f)
        {
            currentStamina = 0f;
            staminaLock = true;
        }

        // Decide if we are sprinting this frame
        bool isSprintingNow = false;

        if (!staminaLock && wantsToSprint && currentStamina > 0f)
        {
            isSprintingNow = true;
        }

        float currentSpeed = speed;

        if (isSprintingNow)
        {
            // SPRINT
            currentSpeed *= sprintMultiplier;

            currentStamina -= staminaDrainRate * Time.deltaTime;

            // If we fully drained stamina this frame, lock sprint
            if (currentStamina <= 0f)
            {
                currentStamina = 0f;
                staminaLock = true;
            }
        }
        else
        {
            // NOT sprinting → walk + regen stamina
            currentStamina += staminaRegenRate * Time.deltaTime;

            // If we were locked and stamina is now full, unlock sprint
            if (staminaLock && currentStamina >= maxStamina)
            {
                currentStamina = maxStamina;
                staminaLock = false;
            }
        }

        // Clamp stamina just in case
        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);

        // Finally move
        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * currentSpeed * Time.deltaTime);

        // ----------- Jump logic using coyote + buffer -----------
        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
        {
            // UPDATED: We ALWAYS jump if the buttons were pressed
            // We do NOT check stamina level before jumping
            
            // 1. Pay the cost (even if it goes below 0 internally, we clamp it later)
            currentStamina -= jumpStaminaCost;

            // 2. Check if that jump killed our energy to trigger the UI "Pink Mode"
            if (currentStamina <= 0f)
            {
                currentStamina = 0f;
                staminaLock = true;
            }

            // 3. Perform jump physics
            float effectiveUpGravity = upwardGravity; 
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * effectiveUpGravity);

            // Consume timers
            jumpBufferCounter = 0f;
            coyoteTimeCounter = 0f;
        }

        // ----------- Gravity & falling with different up/down gravity -----------
        if (isGrounded && velocity.y < 0f)
        {
            // small downward force to stay grounded
            velocity.y = -2f;
        }

        // If moving upwards (velocity.y > 0) → use softer (less negative) gravity
        // If falling (velocity.y <= 0) → use stronger gravity for a snappy fall
        if (velocity.y > 0f)
        {
            velocity.y += upwardGravity * Time.deltaTime;
        }
        else
        {
            velocity.y += downwardGravity * Time.deltaTime;
        }

        controller.Move(velocity * Time.deltaTime);
    }

    // 0–1 stamina value for UI slider etc.
    public float Stamina01
    {
        get
        {
            if (maxStamina <= 0f) return 0f;
            return currentStamina / maxStamina;
        }
    }

    public bool IsStaminaLocked
    {
        get { return staminaLock; }
    }
}