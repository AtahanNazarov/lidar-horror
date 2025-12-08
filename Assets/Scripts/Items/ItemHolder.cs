using UnityEngine;

// Renamed from ItemHoldingManager for simplicity.
public class ItemHolder : MonoBehaviour
{
    [Tooltip("Drag your HandHolder (child of ViewModelRoot) here.")]
    public Transform HandHolderTransform;
    
    // --- Public methods used by the item ---
    
    // Equips the item, reading its specific position data from the item itself.
    public void EquipItem(GameObject heldObject, Vector3 localPos, Vector3 localRot)
    {
        if (HandHolderTransform != null)
        {
            // 1. Parent the object to the hand holder (which is under the swaying ViewModelRoot)
            heldObject.transform.SetParent(HandHolderTransform);
            
            // 2. Set the custom local position and rotation
            heldObject.transform.localPosition = localPos;
            heldObject.transform.localRotation = Quaternion.Euler(localRot);
        }
        else
        {
            Debug.LogWarning("ItemHolder: HandHolderTransform is not assigned.");
        }
    }

    // DropItem removes the parenting
    public void DropItem(GameObject droppedObject)
    {
        if (droppedObject != null)
            droppedObject.transform.SetParent(null);
    }
}
