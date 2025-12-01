using UnityEngine;
using TMPro;

// Renamed from PlayerInteraction.cs, now uses ItemHolder.
public class PlayerController : MonoBehaviour
{
    [Header("Settings")]
    public float interactionDistance = 3f;
    public float interactionSphereRadius = 0.5f;
    public LayerMask interactionLayer;
    
    [Header("UI References")]
    public GameObject interactionPanel;
    public TextMeshProUGUI interactionText;

    [Header("References")]
    public Camera playerCamera;
    private ItemHolder _itemHolder; // Reference to the new holder script

    [Header("Current State")]
    public InteractableItem currentHeldItem; // Reference to the new item script

    void Start()
    {
        if (playerCamera == null) playerCamera = Camera.main;
        if (interactionPanel != null) interactionPanel.SetActive(false);

        // Get the new ItemHolder component
        _itemHolder = GetComponent<ItemHolder>();
    }

    void Update()
    {
        HandleInteraction();
    }

    void HandleInteraction()
    {
        // 1. Handle Dropping Held Item
        if (currentHeldItem != null)
        {
            if (interactionPanel != null)
                interactionPanel.SetActive(false);

            if (Input.GetKeyDown(KeyCode.G))
            {
                DropItem();
            }
            return; 
        }

        // 2. SphereCast for pickup detection
        if (interactionPanel != null) interactionPanel.SetActive(false);
        if (playerCamera == null) return;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.SphereCast(ray, interactionSphereRadius, out hit, interactionDistance, interactionLayer))
        {
            // Look for the new item script: InteractableItem
            InteractableItem item = hit.collider.GetComponent<InteractableItem>();

            if (item != null)
            {
                // Show UI text directly from the item
                if (interactionText != null && interactionPanel != null)
                {
                    interactionText.text = item.GetInteractionText();
                    interactionPanel.SetActive(true);
                }

                if (Input.GetKeyDown(KeyCode.E))
                {
                    PickUpItem(item);
                }
            }
        }
    }

    void PickUpItem(InteractableItem item)
    {
        if (_itemHolder == null) return;
        currentHeldItem = item; 
        item.PickUp(_itemHolder); 
    }

    void DropItem()
    {
        if (currentHeldItem != null)
        {
            currentHeldItem.Drop(_itemHolder, transform.forward); 
            currentHeldItem = null;
        }
    }
}