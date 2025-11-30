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

    [Header("Fading")]
    public float fadeSpeed = 4f; // How fast it disappears/reappears

    [Header("Shake Settings")]
    public float shakeAmount = 4f;
    public float shakeDuration = 0.4f;

    // Internal variables
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup; // Controls visibility
    private Vector2 originalPos;
    private float currentShakeTimer = 0f;

    void Start()
    {
        if (staminaSlider == null) staminaSlider = GetComponent<Slider>();
        if (fillImage == null && staminaSlider != null)
            fillImage = staminaSlider.fillRect.GetComponent<Image>();

        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>(); // Auto-find the group
        
        if (rectTransform != null)
            originalPos = rectTransform.anchoredPosition;
    }

    void Update()
    {
        if (playerMovement == null || staminaSlider == null) return;

        // --- PART 1: SLIDER VALUE ---
        float v = playerMovement.Stamina01;
        if (v < 0.01f) v = 0f;
        else if (v > 0.99f) v = 1f;
        staminaSlider.value = v;

        // --- PART 2: COLOR ---
        if (fillImage != null)
        {
            Color targetColor = playerMovement.IsStaminaLocked ? exhaustedColor : defaultColor;
            fillImage.color = Color.Lerp(fillImage.color, targetColor, Time.deltaTime * colorLerpSpeed);
        }

        // --- PART 3: AUTO-FADE ---
        // If Stamina is full (1) AND we are not locked (recharging from 0) -> Invisible (0)
        // Otherwise -> Visible (1)
        float targetAlpha = (v >= 1f && !playerMovement.IsStaminaLocked) ? 0f : 1f;
        
        // Move alpha smoothly
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