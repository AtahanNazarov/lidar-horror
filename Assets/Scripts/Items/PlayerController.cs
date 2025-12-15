using UnityEngine;
using TMPro;

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
    private ItemHolder _itemHolder;

    [Header("Current State")]
    public InteractableItem currentHeldItem;

    public KeyCode pickupKey = KeyCode.E;
    public KeyCode dropKey = KeyCode.G;

    void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        if (interactionPanel != null)
            interactionPanel.SetActive(false);

        _itemHolder = GetComponentInChildren<ItemHolder>();

        if (_itemHolder == null)
        {
            Debug.LogError(
                "PlayerController: No ItemHolder found. " +
                "Add ItemHolder to Player/Camera/ViewModelRoot and assign HandHolder."
            );
        }
    }

    void Update()
    {
        HandleInteraction();
    }

    void HandleInteraction()
    {
        // --- HELD ITEM ---
        if (currentHeldItem != null)
        {
            if (interactionPanel != null)
                interactionPanel.SetActive(false);

            if (Input.GetKeyDown(dropKey))
            {
                DropItem();
            }
            return;
        }

        // --- PICKUP ---
        if (interactionPanel != null)
            interactionPanel.SetActive(false);

        if (playerCamera == null)
            return;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.SphereCast(ray, interactionSphereRadius, out hit, interactionDistance, interactionLayer))
        {
            InteractableItem item = hit.collider.GetComponent<InteractableItem>();

            if (item != null)
            {
                if (interactionText != null && interactionPanel != null)
                {
                    interactionText.text = item.GetInteractionText();
                    interactionPanel.SetActive(true);
                }

                if (Input.GetKeyDown(pickupKey))
                {
                    PickUpItem(item);
                }
            }
        }
    }

    void PickUpItem(InteractableItem item)
    {
        if (_itemHolder == null)
            return;

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
