using UnityEngine;
using TMPro;

public class PlayerController : MonoBehaviour
{
    public float interactionDistance = 3f;
    public float interactionSphereRadius = 0.5f;
    public LayerMask interactionLayer;
    public GameObject interactionPanel;
    public TextMeshProUGUI interactionText;
    public Camera playerCamera;
    private ItemHolder _itemHolder;
    public InteractableItem currentHeldItem;

    void Start()
    {
        _itemHolder = GetComponentInChildren<ItemHolder>();
    }

    void Update()
    {
        if (currentHeldItem != null)
        {
            if (interactionPanel != null) interactionPanel.SetActive(false);
            if (Input.GetKeyDown(KeyCode.G)) DropItem();
            return;
        }

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.SphereCast(ray, interactionSphereRadius, out hit, interactionDistance, interactionLayer))
        {
            InteractableItem item = hit.collider.GetComponent<InteractableItem>();
            if (item != null)
            {
                interactionPanel.SetActive(true);
                interactionText.text = item.GetInteractionText();
                if (Input.GetKeyDown(KeyCode.E)) PickUpItem(item);
            }
            else { interactionPanel.SetActive(false); }
        }
        else { interactionPanel.SetActive(false); }
    }

    void PickUpItem(InteractableItem item)
    {
        currentHeldItem = item;
        item.PickUp(_itemHolder);
        
        // CRITICAL FIX: Force Unity to refresh all components like Inspector does
        StartCoroutine(ForceComponentRefresh());
    }

    void DropItem()
    {
        currentHeldItem.Drop(_itemHolder, transform.forward);
        currentHeldItem = null;
        
        // CRITICAL FIX: Force refresh on drop too
        StartCoroutine(ForceComponentRefresh());
    }
    
    private System.Collections.IEnumerator ForceComponentRefresh()
    {
        // Wait only 1 fixed update instead of 3
        yield return new WaitForFixedUpdate();
        
        // Force complete transform hierarchy update
        Physics.SyncTransforms();
        
        // Reset Character Controller (mimics Inspector refresh)
        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false;
            cc.enabled = true;
        }
        
        // Force camera refresh
        if (playerCamera != null)
        {
            playerCamera.enabled = false;
            playerCamera.enabled = true;
        }
        
        // Final sync
        Physics.SyncTransforms();
    }
}