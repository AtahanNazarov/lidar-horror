using UnityEngine;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Settings")]
    public float interactionDistance = 3f;
    public LayerMask interactionLayer;
    public Transform handPosition;

    [Header("UI References")]
    public GameObject interactionPanel;      // Drag your panel here
    public TextMeshProUGUI interactionText;  // Drag the TMP text here

    [Header("References")]
    public Camera playerCamera;

    [Header("Current State")]
    public PickupableItem currentHeldItem;

    void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        if (interactionPanel != null)
            interactionPanel.SetActive(false);
    }

    void Update()
    {
        HandleInteraction();
    }

    void HandleInteraction()
    {
        // 1. If holding an item: keep UI hidden but still allow dropping with G
        if (currentHeldItem != null)
        {
            if (interactionPanel != null)
                interactionPanel.SetActive(false);

            if (Input.GetKeyDown(KeyCode.G))
            {
                DropItem();
            }
            return; // don’t raycast, hand is full
        }

        // 2. Hand empty → default: hide UI
        if (interactionPanel != null)
            interactionPanel.SetActive(false);

        if (playerCamera == null) return;

        // 3. Raycast / SphereCast for interactables
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        float rayRadius = 0.2f;

        if (Physics.SphereCast(ray, rayRadius, out hit, interactionDistance, interactionLayer))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

            if (interactable != null)
            {
                // Show ONLY pickup text from the interactable
                if (interactionText != null && interactionPanel != null)
                {
                    interactionText.text = interactable.GetDescription(); // e.g. "Press E to Pick Up LiDAR Scanner"
                    interactionPanel.SetActive(true);
                }

                if (Input.GetKeyDown(KeyCode.E))
                {
                    PickupableItem item = hit.collider.GetComponent<PickupableItem>();
                    if (item != null)
                    {
                        PickUpItem(item);
                    }
                    else
                    {
                        interactable.Interact();
                    }
                }
            }
        }
    }

    void PickUpItem(PickupableItem item)
    {
        currentHeldItem = item;
        item.OnPickUp(handPosition);
    }

    void DropItem()
    {
        if (currentHeldItem != null)
        {
            currentHeldItem.OnDrop();
            currentHeldItem = null;
        }
    }
}
