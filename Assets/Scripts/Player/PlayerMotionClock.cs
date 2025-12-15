using UnityEngine;

public class PlayerMotionClock : MonoBehaviour
{
    public CharacterController controller;

    [Header("Frequencies (match your camera)")]
    public float idleFrequency = 1.2f;
    public float walkFrequency = 6f;
    public float runFrequency  = 9f;

    [Header("Input")]
    public float moveThreshold = 0.01f;

    public float Phase { get; private set; }

    void Reset()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (controller == null) return;

        float inputX = Input.GetAxisRaw("Horizontal");
        float inputZ = Input.GetAxisRaw("Vertical");
        bool hasInput = (inputX * inputX + inputZ * inputZ) > moveThreshold * moveThreshold;

        bool grounded = controller.isGrounded;
        bool moving = grounded && hasInput;

        float freq = moving
            ? (Input.GetKey(KeyCode.LeftShift) ? runFrequency : walkFrequency)
            : idleFrequency;

        Phase += Time.deltaTime * freq;
    }
}
