using UnityEngine;
using System.Collections.Generic; // Required for Lists

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 2.4f; 
    public float sprintMultiplier = 2f; 

    // --- CROUCH SETTINGS START ---
    [Header("Crouching")]
    public KeyCode crouchKey = KeyCode.LeftControl; // Hold to crouch
    public float standingHeight = 1.8f;     
    public float crouchHeight = 1.0f;       
    public float crouchSpeedMultiplier = 0.4f; 
    public float crouchTransitionSpeed = 8f;
    
    // For ceiling check safety:
    public float ceilingCheckRadius = 0.3f; // Radius of the check
    public LayerMask ceilingLayer;           // What layers to check against (e.g., Default, Walls)

    private bool isCrouching = false;
    private Transform playerCamera;
    private float originalCameraY; 
    // --- CROUCH SETTINGS END ---

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

    [Header("Animation")]
    public List<Animator> playerAnimators; 

    private float currentStamina;
    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private bool staminaLock = false;
    
    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        currentStamina = maxStamina;

        // CROUCH: Find Camera and set original height
        if (Camera.main != null)
        {
            playerCamera = Camera.main.transform;
            originalCameraY = playerCamera.localPosition.y;
            standingHeight = controller.height;
        }
    }

    void Update()
    {
        // Ground Check
        isGrounded = controller.isGrounded;
        if (isGrounded) coyoteTimeCounter = coyoteTime;
        else coyoteTimeCounter -= Time.deltaTime;

        // Jump Buffer
        if (Input.GetButtonDown("Jump")) jumpBufferCounter = jumpBufferTime;
        else jumpBufferCounter -= Time.deltaTime;
        
        // --- CROUCH FUNCTION CALL (Handles safety check) ---
        HandleCrouch();

        // Movement Inputs
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        float inputMagnitude = new Vector2(x, z).sqrMagnitude;
        bool hasMoveInput = (Mathf.Abs(x) > 0.01f || Mathf.Abs(z) > 0.01f);
        
        // Cannot sprint while crouching
        bool wantsToSprint = Input.GetKey(KeyCode.LeftShift) && hasMoveInput && !isCrouching; 

        // Stamina Logic
        if (currentStamina <= 0f) { currentStamina = 0f; staminaLock = true; }

        bool isSprintingNow = false;
        if (!staminaLock && wantsToSprint && currentStamina > 0f) isSprintingNow = true;

        // --- STAMINA REPLENISHMENT LOGIC (Fixes the Crouch Regen Bug) ---
        
        bool shouldDrainStamina = false;
        bool shouldRegenStamina = true;


        if (isSprintingNow)
        {
            shouldDrainStamina = true;
            shouldRegenStamina = false;
        }

        if (shouldDrainStamina)
        {
            currentStamina -= staminaDrainRate * Time.deltaTime;
            if (currentStamina <= 0f) { currentStamina = 0f; staminaLock = true; }
        }

        // Regen if not actively draining (this runs during Idle, Walk, and Crouch)
        if (shouldRegenStamina)
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
            if (staminaLock && currentStamina >= maxStamina) { currentStamina = maxStamina; staminaLock = false; }
        }
        
        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
        
        // --- END STAMINA LOGIC ---

        // --- SPEED CALCULATION ---
        float currentSpeed = speed;

        if (isCrouching) // Apply Crouch speed multiplier first
        {
            currentSpeed *= crouchSpeedMultiplier;
        }
        else if (isSprintingNow) // Apply Sprint speed multiplier (only if not crouching)
        {
            currentSpeed *= sprintMultiplier;
        }
        
        // Physics Move
        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * currentSpeed * Time.deltaTime);

        // Jump Logic (Cannot jump while crouching)
        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f && !isCrouching)
        {
            currentStamina -= jumpStaminaCost;
            if (currentStamina <= 0f) { currentStamina = 0f; staminaLock = true; }

            float effectiveUpGravity = upwardGravity; 
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * effectiveUpGravity);

            // Trigger Jump for ALL animators
            foreach (Animator anim in playerAnimators)
            {
                if(anim != null) anim.SetTrigger("Jump");
            }

            jumpBufferCounter = 0f;
            coyoteTimeCounter = 0f;
        }

        // Gravity
        if (isGrounded && velocity.y < 0f) velocity.y = -2f;
        if (velocity.y > 0f) velocity.y += upwardGravity * Time.deltaTime;
        else velocity.y += downwardGravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // Animation Logic (Walk/Run/Crouch)
        float targetAnim = 0f;

        if (inputMagnitude > 0.01f)
        {
            if (isCrouching) targetAnim = 0.25f; 
            else if (isSprintingNow) targetAnim = 1f; 
            else targetAnim = 0.5f; 
        }

        // Set Animation Parameters
        foreach (Animator anim in playerAnimators)
        {
            if (anim != null)
            {
                anim.SetFloat("Speed", Mathf.Lerp(anim.GetFloat("Speed"), targetAnim, Time.deltaTime * 10f));
                anim.SetBool("IsCrouching", isCrouching); 
            }
        }
    }
    
    // --- HOLD-TO-CROUCH FUNCTION WITH CEILING CHECK ---
    void HandleCrouch()
    {
        // Player's intention for this frame: are they holding the key?
        bool wantsToCrouch = Input.GetKey(crouchKey); 
        
        // --- CEILING CHECK LOGIC ---
        if (!wantsToCrouch && isCrouching) 
        {
            // Calculate the height needed to stand up (Height change + a small buffer)
            float checkHeight = standingHeight - controller.height + 0.05f; 
            
            // Calculate the position to start the check (from the top of the current crouched collider)
            Vector3 checkPosition = transform.position + Vector3.up * controller.height;

            // Perform the sphere check above the character
            if (Physics.SphereCast(checkPosition, ceilingCheckRadius, Vector3.up, out RaycastHit hit, checkHeight, ceilingLayer))
            {
                // If the SphereCast hits a ceiling, we are blocked.
                wantsToCrouch = true; // Force the state back to true
            }
        }
        // --- END CEILING CHECK ---

        // Set the state based on the input (or the override from the ceiling check)
        isCrouching = wantsToCrouch; 
        
        float targetHeight = isCrouching ? crouchHeight : standingHeight;
        // Calculation to drop the camera relative to the height change
        float targetCameraY = isCrouching ? originalCameraY - (standingHeight - crouchHeight) : originalCameraY; 

        // Smoothly change the controller height
        controller.height = Mathf.Lerp(controller.height, targetHeight, Time.deltaTime * crouchTransitionSpeed);
        
        // Adjust the center point so the character stays on the floor
        controller.center = new Vector3(0, controller.height / 2f, 0);

        // Smoothly move the camera's Y position
        if (playerCamera != null)
        {
            playerCamera.localPosition = Vector3.Lerp(playerCamera.localPosition, 
                                                    new Vector3(playerCamera.localPosition.x, targetCameraY, playerCamera.localPosition.z), 
                                                    Time.deltaTime * crouchTransitionSpeed);
        }
    }


    public float Stamina01 { get { if (maxStamina <= 0f) return 0f; return currentStamina / maxStamina; } }
    public bool IsStaminaLocked { get { return staminaLock; } }
}