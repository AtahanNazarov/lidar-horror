using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Lidar Integration")]
    public LaserSystem lidar;

    [Header("Movement")]
    public float speed = 2.4f;
    public float sprintMultiplier = 2f;

    [Header("Crouching")]
    public KeyCode crouchKey = KeyCode.LeftControl;
    public float standingHeight = 1.8f;
    public float crouchHeight = 1.0f;
    public float crouchSpeedMultiplier = 0.4f;
    public float crouchTransitionSpeed = 8f;
    public float ceilingCheckRadius = 0.3f;
    public LayerMask ceilingLayer;

    [Header("Jumping")]
    public float jumpHeight = 1.0f;
    public float upwardGravity = -20f;
    public float downwardGravity = -50f;
    public float coyoteTime = 0.1f;
    public float jumpBufferTime = 0.1f;

    [Header("Stamina")]
    public float maxStamina = 5f;
    public float staminaDrainRate = 1f;
    public float staminaRegenRate = 1.5f;
    public float jumpStaminaCost = 1.0f;

    [Header("Audio Settings")]
    public AudioSource movementAudioSource; 
    public AudioSource vocalAudioSource;    
    public AudioClip walkClip;
    public AudioClip runClip;
    public AudioClip heavyBreathingClip;

    private float currentStamina;
    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private bool staminaLock = false;
    private bool isCrouching = false;
    private Transform playerCamera;
    private float originalCameraY;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        
        // Auto-assign or Add AudioSources
        if (movementAudioSource == null) movementAudioSource = GetComponent<AudioSource>();
        if (vocalAudioSource == null) 
        {
            // Create a second one if not assigned so breathing doesn't stop footsteps
            vocalAudioSource = gameObject.AddComponent<AudioSource>();
        }
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        currentStamina = maxStamina;

        if (Camera.main != null)
        {
            playerCamera = Camera.main.transform;
            originalCameraY = playerCamera.localPosition.y;
            standingHeight = controller.height;
        }
    }

    void Update()
    {
        isGrounded = controller.isGrounded;

        if (isGrounded) coyoteTimeCounter = coyoteTime;
        else coyoteTimeCounter -= Time.deltaTime;

        if (Input.GetButtonDown("Jump")) jumpBufferCounter = jumpBufferTime;
        else jumpBufferCounter -= Time.deltaTime;

        HandleCrouch();

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        bool hasMoveInput = (Mathf.Abs(x) > 0.1f || Mathf.Abs(z) > 0.1f);
        bool wantsToSprint = Input.GetKey(KeyCode.LeftShift) && hasMoveInput && !isCrouching;

        // Stamina logic
        if (currentStamina <= 0f)
        {
            currentStamina = 0f;
            staminaLock = true;
        }

        bool isSprintingNow = (!staminaLock && wantsToSprint && currentStamina > 0f);

        if (isSprintingNow) currentStamina -= staminaDrainRate * Time.deltaTime;
        else
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
            if (staminaLock && currentStamina >= maxStamina) staminaLock = false;
        }

        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);

        HandleLidarInput(isSprintingNow);
        HandleAudio(hasMoveInput, isSprintingNow);

        float currentSpeed = speed;
        if (isCrouching) currentSpeed *= crouchSpeedMultiplier;
        else if (isSprintingNow) currentSpeed *= sprintMultiplier;

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * currentSpeed * Time.deltaTime);

        // Jumping (Logic only, no audio)
        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f && !isCrouching)
        {
            currentStamina -= jumpStaminaCost;
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * upwardGravity);
            jumpBufferCounter = 0f;
            coyoteTimeCounter = 0f;
        }

        if (isGrounded && velocity.y < 0f) velocity.y = -2f;
        velocity.y += (velocity.y > 0f ? upwardGravity : downwardGravity) * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void HandleAudio(bool hasMoveInput, bool isSprinting)
    {
        // 1. Footsteps Logic
        if (hasMoveInput && isGrounded)
        {
            AudioClip desiredClip = isSprinting ? runClip : walkClip;
            
            if (movementAudioSource.clip != desiredClip || !movementAudioSource.isPlaying)
            {
                movementAudioSource.clip = desiredClip;
                movementAudioSource.loop = true;
                movementAudioSource.Play();
            }
        }
        else
        {
            if (movementAudioSource.isPlaying && movementAudioSource.loop) 
                movementAudioSource.Stop();
        }

        // 2. Heavy Breathing Logic (Triggers below 30% stamina)
        if (currentStamina < (maxStamina * 0.3f))
        {
            if (!vocalAudioSource.isPlaying || vocalAudioSource.clip != heavyBreathingClip)
            {
                vocalAudioSource.clip = heavyBreathingClip;
                vocalAudioSource.loop = true;
                vocalAudioSource.Play();
            }
        }
        else if (currentStamina > (maxStamina * 0.6f) && vocalAudioSource.clip == heavyBreathingClip)
        {
            vocalAudioSource.Stop();
        }
    }

    void HandleLidarInput(bool isSprinting)
    {
        if (lidar == null) return;
        bool isHeld = lidar.transform.IsChildOf(this.transform);
        lidar.scanning = (Input.GetMouseButton(0) && !isSprinting && isHeld);
    }

    void HandleCrouch()
    {
        bool wantsToCrouch = Input.GetKey(crouchKey);
        if (!wantsToCrouch && isCrouching)
        {
            float checkHeight = standingHeight - controller.height + 0.05f;
            Vector3 checkPosition = transform.position + Vector3.up * controller.height;
            if (Physics.SphereCast(checkPosition, ceilingCheckRadius, Vector3.up, out _, checkHeight, ceilingLayer))
                wantsToCrouch = true;
        }

        isCrouching = wantsToCrouch;
        float targetHeight = isCrouching ? crouchHeight : standingHeight;
        float targetCameraY = isCrouching ? originalCameraY - (standingHeight - crouchHeight) : originalCameraY;

        controller.height = Mathf.Lerp(controller.height, targetHeight, Time.deltaTime * crouchTransitionSpeed);
        controller.center = new Vector3(0, controller.height / 2f, 0);

        if (playerCamera != null)
        {
            playerCamera.localPosition = Vector3.Lerp(playerCamera.localPosition,
                new Vector3(playerCamera.localPosition.x, targetCameraY, playerCamera.localPosition.z),
                Time.deltaTime * crouchTransitionSpeed);
        }
    }

    public float Stamina01 => maxStamina <= 0f ? 0f : currentStamina / maxStamina;
    public bool IsStaminaLocked => staminaLock;
}