using UnityEngine;

// Renamed from ItemHoldingManager for simplicity.
public class ItemHolder : MonoBehaviour
{
    [Tooltip("Drag your PlayerCamera here to parent the item to.")]
    public Transform CameraTransform;
    
    // --- Public methods used by the item ---
    
    // Equips the item, reading its specific position data from the item itself.
    public void EquipItem(GameObject heldObject, Vector3 localPos, Vector3 localRot)
    {
        if (CameraTransform != null)
        {
            // 1. Parent the single object to the camera
            heldObject.transform.SetParent(CameraTransform);
            
            // 2. Set the custom local position and rotation
            heldObject.transform.localPosition = localPos;
            heldObject.transform.localRotation = Quaternion.Euler(localRot);
        }
    }

    // DropItem removes the parenting
    public void DropItem(GameObject droppedObject)
    {
        droppedObject.transform.SetParent(null);
    }
}