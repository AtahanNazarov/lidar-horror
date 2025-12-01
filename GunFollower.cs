using UnityEngine;

public class GunFollower : MonoBehaviour {
    [Tooltip("Drag the Player GameObject (with CharacterController) here.")]
    public Transform playerTransform;

    [Header("Gun Offset")]
    [Tooltip("The position relative to the player where the gun is held.")]
    public Vector3 offset = new Vector3(0.5f, 0f, 0f);

    void Update() {
        if (playerTransform != null) {
            // 1. Position the gun relative to the player's position and the offset
            transform.position = playerTransform.position + offset;

            // 2. Make the gun face the same direction as the player
            // This ensures the laser always shoots in the player's forward direction.
            transform.rotation = playerTransform.rotation;
        } else {
            Debug.LogWarning("Player Transform not assigned in GunFollower script.");
        }
    }
}