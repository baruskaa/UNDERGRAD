using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inventory.UI; // Replace with your UI namespace if different

namespace Inventory.Model
{
    public class InventoryController : MonoBehaviour
    {
        [SerializeField]
        private InventoryPage inventoryUI;

        [SerializeField]
        private InventorySO inventoryData;

        public List<ItemSO> initialItems = new List<ItemSO>();

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
                return;
            }

            ItemSO item = inventoryItem.item;
            inventoryUI.UpdateDescription(itemIndex, item.ItemImage, item.name, item.Description);
        }

        private void HandleItemActionRequest(int itemIndex)
        {
            InventoryItem inventoryItem = inventoryData.GetItemAt(itemIndex);
            if (inventoryItem.IsEmpty)
                return;

            inventoryData.RemoveItem(itemIndex);
            inventoryUI.ResetSelection();
        }

        private void Update()
        {
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
                }
            }
        }
    }
}