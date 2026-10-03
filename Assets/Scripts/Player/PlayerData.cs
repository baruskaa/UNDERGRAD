using System;
using System.Collections.Generic;
using UnityEngine;
using Inventory.Model;

[Serializable]
public class PlayerData
{
    // Health and Hunger Stats
    public int health;
    public int hunger;

    // Transform Position
    public float[] position;

    // Inventory Data (Slot Index -> Item Name)
    public List<int> inventoryIndices = new List<int>();
    public List<string> inventoryItemNames = new List<string>();

    // Quest System Data
    public bool[] questCompleted;
    public string itemCollected;

    public PlayerData(PlayerManager player, InventorySO inventorySO, QuestManager questManager)
    {
        // Save Player Stats
        health = player.currentHealth;
        hunger = player.currentHunger;

        // Save Position
        position = new float[3];
        position[0] = player.transform.position.x;
        position[1] = player.transform.position.y;
        position[2] = player.transform.position.z;

        // Save Inventory State
        if (inventorySO != null)
        {
            var currentState = inventorySO.GetCurrentInventoryState();
            foreach (var kvp in currentState)
            {
                if (!kvp.Value.IsEmpty && kvp.Value.item != null)
                {
                    inventoryIndices.Add(kvp.Key);

                    // Save item by Name property or ScriptableObject asset name
                    string nameToSave = !string.IsNullOrEmpty(kvp.Value.item.Name)
                        ? kvp.Value.item.Name
                        : kvp.Value.item.name;

                    inventoryItemNames.Add(nameToSave);
                }
            }
        }

        // Save Quest State
        if (questManager != null)
        {
            if (questManager.questCompleted != null)
            {
                questCompleted = (bool[])questManager.questCompleted.Clone();
            }
            itemCollected = questManager.itemCollected;
        }
    }
}