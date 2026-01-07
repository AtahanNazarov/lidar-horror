using UnityEngine;

public class GunFollower : MonoBehaviour
{
    [Header("References")]
    public Transform playerTransform;
    public Transform cameraTransform;

    [Header("Gun Offset")]
    public Vector3 offset = new Vector3(0.5f, -0.3f, 0.8f);

    void LateUpdate()
    {
        if (playerTransform == null || cameraTransform == null)
            return;

        transform.position = cameraTransform.position
                           + cameraTransform.right * offset.x
                           + cameraTransform.up * offset.y
                           + cameraTransform.forward * offset.z;

        transform.rotation = cameraTransform.rotation;
    }
}
