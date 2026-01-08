using UnityEngine;
using System.Collections;

public class InteractableItem : MonoBehaviour
{
    public string ItemName = "Item";
    public Vector3 LocalPosition = new Vector3(0.5f, -0.35f, 0.7f);
    public Vector3 LocalRotation = new Vector3(-90f, 180f, 10f);
    public bool livePoseTuning = true;

    private bool isHeld = false;
    private ItemHolder currentHolder;

    public string GetInteractionText()
    {
        return "Press E to pick up " + ItemName;
    }

    void Update()
    {
        if (livePoseTuning && isHeld && currentHolder != null)
        {
            currentHolder.EquipItem(gameObject, LocalPosition, LocalRotation);
        }
    }

    public void PickUp(ItemHolder holder)
    {
        StartCoroutine(PickUpRoutine(holder));
    }

    private IEnumerator PickUpRoutine(ItemHolder holder)
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        Collider col = GetComponent<Collider>();

        // CRITICAL: Disable ALL colliders on this object and children
        Collider[] allColliders = GetComponentsInChildren<Collider>();
        foreach (Collider c in allColliders)
        {
            c.enabled = false;
        }

        if (rb != null) 
        { 
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true; 
            rb.detectCollisions = false;
            rb.useGravity = false;
        }

        // Reduced to just 1 wait for faster pickup
        yield return new WaitForFixedUpdate();

        Physics.SyncTransforms();

        currentHolder = holder;
        isHeld = true;
        holder.EquipItem(gameObject, LocalPosition, LocalRotation);
        
        Physics.SyncTransforms();
    }

    public void Drop(ItemHolder holder, Vector3 playerForward)
    {
        isHeld = false;
        currentHolder = null;
        holder.DropItem(gameObject);

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = true;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.detectCollisions = true;
            rb.linearVelocity = playerForward * 3f + Vector3.up * 2f;
        }
    }
}