using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Inventory.Model
{
    [CreateAssetMenu]
    public class InventorySO : ScriptableObject
    {
        [SerializeField]
        private List<InventoryItem> inventoryItems;

        [field: SerializeField]
        public int Size { get; private set; } = 10;

        public event Action<Dictionary<int, InventoryItem>> OnInventoryUpdated;

        public void Initialize()
        {
            if (inventoryItems == null)
            {
                inventoryItems = new List<InventoryItem>();
            }

            while (inventoryItems.Count < Size)
            {
                inventoryItems.Add(InventoryItem.GetEmptyItem());
            }

            if (inventoryItems.Count > Size)
            {
                inventoryItems = inventoryItems.Take(Size).ToList();
            }

            InformAboutChange();
        }

        public bool AddItem(ItemSO item)
        {
            if (IsInventoryFull())
                return false;

            for (int i = 0; i < inventoryItems.Count; i++)
            {
                if (inventoryItems[i].IsEmpty)
                {
                    inventoryItems[i] = new InventoryItem { item = item };
                    InformAboutChange();
                    return true;
                }
            }
            return false;
        }

        private bool IsInventoryFull()
            => inventoryItems.Where(item => item.IsEmpty).Any() == false;

        public void RemoveItem(int itemIndex)
        {
            if (inventoryItems.Count > itemIndex)
            {
                if (inventoryItems[itemIndex].IsEmpty)
                    return;

                if (inventoryItems[itemIndex].item != null &&
                    inventoryItems[itemIndex].item.itemType == ItemType.NonConsumable)
                {
                    Debug.Log($"[Inventory] '{inventoryItems[itemIndex].item.Name}' is NonConsumable and will not be removed.");
                    return;
                }

                inventoryItems[itemIndex] = InventoryItem.GetEmptyItem();
                InformAboutChange();
            }
        }

        public bool HasItem(ItemSO item)
        {
            if (item == null) return false;
            return inventoryItems.Any(slot => !slot.IsEmpty && slot.item == item);
        }

        public bool HasItemByName(string itemName)
        {
            if (string.IsNullOrEmpty(itemName)) return false;
            return inventoryItems.Any(slot => !slot.IsEmpty && slot.item != null &&
                (slot.item.Name.Equals(itemName, StringComparison.OrdinalIgnoreCase) ||
                 slot.item.name.Equals(itemName, StringComparison.OrdinalIgnoreCase)));
        }

        public Dictionary<int, InventoryItem> GetCurrentInventoryState()
        {
            Dictionary<int, InventoryItem> returnValue = new Dictionary<int, InventoryItem>();

            for (int i = 0; i < inventoryItems.Count; i++)
            {
                if (inventoryItems[i].IsEmpty)
                    continue;
                returnValue[i] = inventoryItems[i];
            }
            return returnValue;
        }

        public InventoryItem GetItemAt(int itemIndex)
        {
            return inventoryItems[itemIndex];
        }

        public void ClearInventory()
        {
            if (inventoryItems == null) return;
            for (int i = 0; i < inventoryItems.Count; i++)
            {
                inventoryItems[i] = InventoryItem.GetEmptyItem();
            }
            InformAboutChange();
        }

        public void LoadSavedItemsWithIndices(List<int> indices, List<ItemSO> savedItems)
        {
            ClearInventory();
            if (indices == null || savedItems == null) return;

            for (int i = 0; i < indices.Count && i < savedItems.Count; i++)
            {
                int slotIndex = indices[i];
                if (slotIndex >= 0 && slotIndex < Size && savedItems[i] != null)
                {
                    inventoryItems[slotIndex] = new InventoryItem { item = savedItems[i] };
                }
            }
            InformAboutChange();
        }

        private void InformAboutChange()
        {
            OnInventoryUpdated?.Invoke(GetCurrentInventoryState());
        }

        // --- LEVEL 1 & SAVE SYSTEM INTEGRATION METHODS ---

        public void InitializeLevel1Inventory(ItemSO flashlightItem)
        {
            Initialize();
            ClearInventory();

            if (flashlightItem != null)
            {
                AddItem(flashlightItem);
            }
        }

        /// <summary>
        /// Saves the Unity Asset File Name (e.g., "Key 402") instead of display Name ("Key")
        /// </summary>
        public void GetSaveData(out List<int> savedIndices, out List<string> savedNames)
        {
            savedIndices = new List<int>();
            savedNames = new List<string>();

            if (inventoryItems == null) return;

            for (int i = 0; i < inventoryItems.Count; i++)
            {
                if (!inventoryItems[i].IsEmpty && inventoryItems[i].item != null)
                {
                    savedIndices.Add(i);
                    // CHANGED: Uses item.name (Asset file name, e.g. "Key 402")
                    savedNames.Add(inventoryItems[i].item.name);
                }
            }
        }

        /// <summary>
        /// Restores inventory by matching JSON asset names against itemDatabase entries (item.name)
        /// </summary>
        public void RestoreFromSaveData(List<int> savedIndices, List<string> savedNames, List<ItemSO> database, ItemSO flashlightItem)
        {
            Initialize();

            List<ItemSO> matchedItems = new List<ItemSO>();
            List<int> validIndices = new List<int>();

            if (savedIndices != null && savedNames != null && database != null)
            {
                for (int i = 0; i < savedIndices.Count && i < savedNames.Count; i++)
                {
                    string targetAssetName = savedNames[i];

                    // CHANGED: Matches strictly against x.name (Asset File Name)
                    ItemSO itemAsset = database.Find(x => x != null &&
                        x.name.Equals(targetAssetName, StringComparison.OrdinalIgnoreCase));

                    if (itemAsset != null)
                    {
                        validIndices.Add(savedIndices[i]);
                        matchedItems.Add(itemAsset);
                    }
                    else
                    {
                        Debug.LogWarning($"[InventorySO] Could not find ItemSO asset named '{targetAssetName}' in itemDatabase!");
                    }
                }
            }

            // Load matches into exact slot indices and trigger UI refresh
            LoadSavedItemsWithIndices(validIndices, matchedItems);

            // Mandatory Level 1 check: ensure Flashlight is present
            if (flashlightItem != null && !HasItem(flashlightItem))
            {
                AddItem(flashlightItem);
            }
        }
    }

    [Serializable]
    public struct InventoryItem
    {
        public ItemSO item;
        public bool IsEmpty => item == null;

        public static InventoryItem GetEmptyItem()
            => new InventoryItem
            {
                item = null
            };
    }
}