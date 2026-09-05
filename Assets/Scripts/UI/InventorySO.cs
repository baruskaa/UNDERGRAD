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
            // 1. If the list is totally empty/null, create it
            if (inventoryItems == null)
            {
                inventoryItems = new List<InventoryItem>();
            }

            // 2. Expand the list to match 'Size' without clearing existing elements
            while (inventoryItems.Count < Size)
            {
                inventoryItems.Add(InventoryItem.GetEmptyItem());
            }

            // 3. Trim if size was lowered in inspector
            if (inventoryItems.Count > Size)
            {
                inventoryItems = inventoryItems.Take(Size).ToList();
            }

            // Push state to UI without re-instantiating or overwriting items
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

                inventoryItems[itemIndex] = InventoryItem.GetEmptyItem();
                InformAboutChange();
            }
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

        private void InformAboutChange()
        {
            OnInventoryUpdated?.Invoke(GetCurrentInventoryState());
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