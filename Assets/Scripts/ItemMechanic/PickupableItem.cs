using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PickupableItem : MonoBehaviour, IInteractable
{
    [Header("Settings")]
    public string itemName = "LiDAR Scanner";

    private Rigidbody rb;
    private Collider itemCollider;
    private bool isHeld = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        itemCollider = GetComponent<Collider>();
    }

    // Always returns the pickup message (UI is handled by PlayerInteraction)
    public string GetDescription()
    {
        return $"Press E to Pick Up {itemName}";
    }

    // Interaction is handled in PlayerInteraction, so this stays empty
    public void Interact() {}

    public void OnPickUp(Transform handTransform)
    {
        isHeld = true;
        rb.isKinematic = true;
        rb.useGravity = false;
        if (itemCollider) itemCollider.enabled = false;

        transform.SetParent(handTransform);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    public void OnDrop()
    {
        isHeld = false;

        transform.SetParent(null);

        rb.isKinematic = false;
        rb.useGravity = true;
        if (itemCollider) itemCollider.enabled = true;

        rb.AddForce(transform.forward * 2f, ForceMode.Impulse);
    }
}
