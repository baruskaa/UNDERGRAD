using UnityEngine;
using Inventory.Model;

public class PlayerItemInteract : MonoBehaviour
{
    [Header("Inventory Reference")]
    [SerializeField] private InventorySO inventoryData;

    [Header("Alert UI")]
    [SerializeField] private GameObject alertIcon;

    private ItemPickup currentNearbyItem;

    private void Start()
    {
        if (alertIcon != null)
            alertIcon.SetActive(false);
    }

    private void Update()
    {
        if (currentNearbyItem != null && Input.GetKeyDown(KeyCode.Q))
        {
            PickUpItem();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<ItemPickup>(out ItemPickup item))
        {
            currentNearbyItem = item;
            if (alertIcon != null)
                alertIcon.SetActive(true);

            Debug.Log("Player entered item trigger zone.");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent<ItemPickup>(out ItemPickup item) && item == currentNearbyItem)
        {
            currentNearbyItem = null;
            if (alertIcon != null)
                alertIcon.SetActive(false);

            Debug.Log("Player left item trigger zone.");
        }
    }

    private void PickUpItem()
    {
        if (currentNearbyItem == null) return;

        ItemSO itemAsset = currentNearbyItem.GetItem();

        // BUG CHECK 1: Is the scriptable object missing on the ground item?
        if (itemAsset == null)
        {
            Debug.LogError("BUG: The item on the ground has no ItemSO assigned in its ItemPickup script!");
            return;
        }

        // BUG CHECK 2: Is the inventory missing on the player?
        if (inventoryData == null)
        {
            Debug.LogError("BUG: Inventory Data is missing on PlayerItemInteract script!");
            return;
        }

        bool result = inventoryData.AddItem(itemAsset);

        if (result)
        {
            Debug.Log($"Successfully picked up: {itemAsset.Name}");
            currentNearbyItem.OnPickedUp();
            currentNearbyItem = null;
            if (alertIcon != null)
                alertIcon.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Cannot pick up item: Inventory is full!");
        }
    }
}