using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class StaminaUI : MonoBehaviour
{
    public PlayerMovement playerMovement;
    public Slider staminaSlider;

    [Header("Visuals")]
    public Image fillImage; 
    public Color defaultColor = Color.white;
    public Color exhaustedColor = Color.magenta;
    public float colorLerpSpeed = 5f;

    [Header("Smoothing")]
    public float barChangeSpeed = 10f; // Higher = Snappier, Lower = Smoother

    [Header("Fading")]
    public float fadeSpeed = 4f; 

    [Header("Shake Settings")]
    public float shakeAmount = 4f;
    public float shakeDuration = 0.4f;

    // Internal variables
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 originalPos;
    private float currentShakeTimer = 0f;

    void Start()
    {
        if (staminaSlider == null) staminaSlider = GetComponent<Slider>();
        if (fillImage == null && staminaSlider != null)
            fillImage = staminaSlider.fillRect.GetComponent<Image>();

        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>(); 
        
        if (rectTransform != null)
            originalPos = rectTransform.anchoredPosition;
    }

    void Update()
    {
        if (playerMovement == null || staminaSlider == null) return;

        // --- PART 1: SLIDER VALUE (UPDATED) ---
        // 1. Get the "Real" value from the player script
        float targetValue = playerMovement.Stamina01;

        // 2. Snap target to 0 or 1 to prevent floating point errors
        if (targetValue < 0.01f) targetValue = 0f;
        else if (targetValue > 0.99f) targetValue = 1f;

        // 3. SMOOTHLY move the slider towards the target value
        // Mathf.Lerp creates a nice "ease-out" effect (fast start, slow end)
        staminaSlider.value = Mathf.Lerp(staminaSlider.value, targetValue, Time.deltaTime * barChangeSpeed);

        // --- PART 2: COLOR ---
        if (fillImage != null)
        {
            Color targetColor = playerMovement.IsStaminaLocked ? exhaustedColor : defaultColor;
            fillImage.color = Color.Lerp(fillImage.color, targetColor, Time.deltaTime * colorLerpSpeed);
        }

        // --- PART 3: AUTO-FADE ---
        // Note: We check 'targetValue' for fading so it doesn't wait for the slow animation to finish before fading out
        float targetAlpha = (targetValue >= 1f && !playerMovement.IsStaminaLocked) ? 0f : 1f;
        
        if (canvasGroup != null)
        {
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, Time.deltaTime * fadeSpeed);
        }

        // --- PART 4: SHAKE LOGIC ---
        if (playerMovement.IsStaminaLocked && Input.GetKeyDown(KeyCode.LeftShift))
        {
            currentShakeTimer = shakeDuration;
        }

        if (currentShakeTimer > 0)
        {
            currentShakeTimer -= Time.deltaTime;
            Vector2 shakeOffset = Random.insideUnitCircle * shakeAmount;
            rectTransform.anchoredPosition = originalPos + shakeOffset;
        }
        else
        {
            if (rectTransform.anchoredPosition != originalPos)
            {
                rectTransform.anchoredPosition = originalPos;
            }
        }
    }
}