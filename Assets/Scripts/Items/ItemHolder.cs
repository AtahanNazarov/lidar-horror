using UnityEngine;

public class ItemHolder : MonoBehaviour
{
    public Transform HandHolderTransform;
    public GameObject fpsArms; 
    private GameObject currentItem;

    void Start()
    {
        if (fpsArms != null)
            fpsArms.SetActive(false);
    }

    public void EquipItem(GameObject heldObject, Vector3 localPos, Vector3 localRot)
    {
        if (heldObject == null || HandHolderTransform == null)
            return;

        currentItem = heldObject;

        if (fpsArms != null)
            fpsArms.SetActive(true);

        heldObject.transform.SetParent(HandHolderTransform);
        heldObject.transform.localPosition = localPos;
        heldObject.transform.localRotation = Quaternion.Euler(localRot);

        Rigidbody rb = heldObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // ===== ADD THESE 2 LINES =====
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    public void DropItem(GameObject droppedObject)
    {
        if (droppedObject == null)
            return;

        droppedObject.transform.SetParent(null);

        Rigidbody rb = droppedObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        currentItem = null;

        if (fpsArms != null)
            fpsArms.SetActive(false);
    }
}