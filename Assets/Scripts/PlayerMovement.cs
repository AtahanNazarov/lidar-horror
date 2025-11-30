using UnityEngine;
using System.Collections.Generic; // Required for Lists

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 2.4f;          
    public float sprintMultiplier = 2f; 

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
    // This List supports both the Visual Guy and the Shadow Guy
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

        // Movement Inputs
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        bool hasMoveInput = (Mathf.Abs(x) > 0.01f || Mathf.Abs(z) > 0.01f);
        bool wantsToSprint = Input.GetKey(KeyCode.LeftShift) && hasMoveInput;

        // Stamina Logic
        if (currentStamina <= 0f) { currentStamina = 0f; staminaLock = true; }

        bool isSprintingNow = false;
        if (!staminaLock && wantsToSprint && currentStamina > 0f) isSprintingNow = true;

        float currentSpeed = speed;
        if (isSprintingNow)
        {
            currentSpeed *= sprintMultiplier;
            currentStamina -= staminaDrainRate * Time.deltaTime;
            if (currentStamina <= 0f) { currentStamina = 0f; staminaLock = true; }
        }
        else
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
            if (staminaLock && currentStamina >= maxStamina) { currentStamina = maxStamina; staminaLock = false; }
        }

        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
        
        // Physics Move
        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * currentSpeed * Time.deltaTime);

        // Jump Logic
        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
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

        // Animation Logic (Walk/Run)
        float inputMagnitude = new Vector2(x, z).sqrMagnitude;
        float targetAnim = 0f;

        if (inputMagnitude > 0.01f)
        {
            if (isSprintingNow) targetAnim = 1f; 
            else targetAnim = 0.5f; 
        }

        foreach (Animator anim in playerAnimators)
        {
            if (anim != null)
            {
                anim.SetFloat("Speed", Mathf.Lerp(anim.GetFloat("Speed"), targetAnim, Time.deltaTime * 10f));
            }
        }
    }

    public float Stamina01 { get { if (maxStamina <= 0f) return 0f; return currentStamina / maxStamina; } }
    public bool IsStaminaLocked { get { return staminaLock; } }
}