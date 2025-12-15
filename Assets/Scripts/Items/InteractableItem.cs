using UnityEngine;

public class InteractableItem : MonoBehaviour
{
    [Header("Item Identification")]
    public string ItemName = "Lidar Scanner";

    [Header("Hold Position (Local to HandHolder)")]
    public Vector3 LocalPosition = new Vector3(0.5f, -0.35f, 0.7f);
    public Vector3 LocalRotation = new Vector3(-90f, 180f, 10f);

    [Header("Debug / Tuning")]
    [Tooltip("When enabled, the item will continuously re-apply LocalPosition/LocalRotation while held.")]
    public bool livePoseTuning = true;

    // --- Runtime state ---
    private bool isHeld = false;
    private ItemHolder currentHolder;

    public string GetInteractionText()
    {
        return "Press E to pick up " + ItemName;
    }

    void Update()
    {
        // 🔧 LIVE TUNING (turn off when you're done)
        if (livePoseTuning && isHeld && currentHolder != null)
        {
            currentHolder.EquipItem(gameObject, LocalPosition, LocalRotation);
        }
    }

    public void PickUp(ItemHolder holder)
    {
        currentHolder = holder;
        isHeld = true;

        holder.EquipItem(gameObject, LocalPosition, LocalRotation);

        // Disable physics/collider
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;
    }

    public void Drop(ItemHolder holder, Vector3 playerForward)
    {
        isHeld = false;
        currentHolder = null;

        holder.DropItem(gameObject);

        // Re-enable physics/collider
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = true;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.linearVelocity = playerForward * 3f + Vector3.up * 2f;
        }
    }
}
