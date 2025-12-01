using UnityEngine;

// Replaces PickupableItem.cs and incorporates position data.
public class InteractableItem : MonoBehaviour
{
    [Header("Item Identification")]
    public string ItemName = "Lidar Scanner";
    
    [Header("Hold Position (Local to Camera)")]
    // These values define the unique hold pose for THIS item
    public Vector3 LocalPosition = new Vector3(0.5f, -0.35f, 0.7f);
    public Vector3 LocalRotation = new Vector3(-90f, 180f, 10f);

    // --- Interaction Methods (used by PlayerInteraction) ---

    public string GetInteractionText()
    {
        return "Press E to pick up " + ItemName;
    }
    
    public void PickUp(ItemHolder holder)
    {
        // Tell the holder to parent and position this GameObject
        holder.EquipItem(gameObject, LocalPosition, LocalRotation); 
        
        // Disable physics/collider
        Collider col = GetComponent<Collider>();
        if(col != null) col.enabled = false;
        
        Rigidbody rb = GetComponent<Rigidbody>();
        if(rb != null) rb.isKinematic = true;
    }

    public void Drop(ItemHolder holder, Vector3 playerForward)
    {
        // Tell the holder to remove parenting
        holder.DropItem(gameObject); 
        
        // Re-enable physics/collider
        Collider col = GetComponent<Collider>();
        if(col != null) col.enabled = true;
        
        Rigidbody rb = GetComponent<Rigidbody>();
        if(rb != null) 
        {
            rb.isKinematic = false;
            // Throw the item slightly forward
            rb.linearVelocity = playerForward * 3f + Vector3.up * 2f; 
        }
    }
}