using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inventory.UI;

namespace Inventory.Model
{
    public class InventoryController : MonoBehaviour
    {
        [SerializeField]
        private InventoryPage inventoryUI;

        [SerializeField]
        private InventorySO inventoryData;

        [SerializeField]
        private PlayerManager playerManager; // Reference to your PlayerManager

        public List<ItemSO> initialItems = new List<ItemSO>();

        // Tracks which slot is currently clicked/selected
        private int currentlySelectedIndex = -1;

        private void Start()
        {
            PrepareUI();
            PrepareInventoryData();
        }

        private void PrepareInventoryData()
        {
            inventoryData.Initialize();
            inventoryData.OnInventoryUpdated += UpdateInventoryUI;

            UpdateInventoryUI(inventoryData.GetCurrentInventoryState());
        }

        private void PrepareUI()
        {
            inventoryUI.InitializeInventoryUI(inventoryData.Size);
            inventoryUI.OnDescriptionRequested += HandleDescriptionRequest;
            inventoryUI.OnItemActionRequested += HandleItemActionRequest;
        }

        private void UpdateInventoryUI(Dictionary<int, InventoryItem> inventoryState)
        {
            inventoryUI.ResetAllItems();
            foreach (var item in inventoryState)
            {
                inventoryUI.UpdateData(item.Key, item.Value.item.ItemImage);
            }
        }

        private void HandleDescriptionRequest(int itemIndex)
        {
            InventoryItem inventoryItem = inventoryData.GetItemAt(itemIndex);
            if (inventoryItem.IsEmpty)
            {
                inventoryUI.ResetSelection();
                currentlySelectedIndex = -1;
                return;
            }

            currentlySelectedIndex = itemIndex; // Save selected index
            ItemSO item = inventoryItem.item;
            inventoryUI.UpdateDescription(itemIndex, item.ItemImage, item.Name, item.Description);
        }

        private void HandleItemActionRequest(int itemIndex)
        {
            InventoryItem inventoryItem = inventoryData.GetItemAt(itemIndex);
            if (inventoryItem.IsEmpty)
                return;

            ItemSO item = inventoryItem.item;

            // Perform action based on item type
            if (item.itemType == ItemType.Food)
            {
                item.Eat(playerManager);
            }
            else if (item.itemType == ItemType.Medkit)
            {
                item.Heal(playerManager);
            }

            inventoryData.RemoveItem(itemIndex);
            inventoryUI.ResetSelection();
            currentlySelectedIndex = -1;
        }

        private void Update()
        {
            // Toggle Inventory with E key
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (inventoryUI.IsOpen() == false)
                {
                    inventoryUI.Show();
                    UpdateInventoryUI(inventoryData.GetCurrentInventoryState());
                }
                else
                {
                    inventoryUI.Hide();
                    currentlySelectedIndex = -1;
                }
            }

            // Consume/Use item with Q key when UI is open and an item is selected
            if (inventoryUI.IsOpen() && Input.GetKeyDown(KeyCode.Q))
            {
                if (currentlySelectedIndex != -1)
                {
                    HandleItemActionRequest(currentlySelectedIndex);
                }
            }
        }
    }
}