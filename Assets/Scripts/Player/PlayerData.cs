using Inventory.Model;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // Added for SceneManager

[Serializable]
public class WorldObjectState
{
    public string objectID;
    public bool isInteractableStateChanged;
    public bool isActive;
}

[Serializable]
public class PlayerData
{
    public string sceneName; // <--- Stores saved scene name
    public int health;
    public int hunger;
    public float[] position = new float[3];
    public bool[] questCompleted;
    public string itemCollected;
    public List<WorldObjectState> savedWorldStates = new List<WorldObjectState>();

    // Inventory lists
    public List<int> inventoryIndices = new List<int>();
    public List<string> inventoryItemNames = new List<string>();

    public PlayerData(PlayerManager pm, InventorySO inv, QuestManager qm)
    {
        // Record currently active scene
        sceneName = SceneManager.GetActiveScene().name;

        if (pm != null)
        {
            health = pm.currentHealth;
            hunger = pm.currentHunger;
            position = new float[] { pm.transform.position.x, pm.transform.position.y, pm.transform.position.z };
        }

        if (qm != null)
        {
            if (qm.questCompleted != null)
            {
                questCompleted = (bool[])qm.questCompleted.Clone();
            }
            itemCollected = qm.itemCollected;
        }

        SaveableWorldObject[] worldObjects = UnityEngine.Object.FindObjectsByType<SaveableWorldObject>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        foreach (var worldObj in worldObjects)
        {
            if (!string.IsNullOrEmpty(worldObj.UniqueID))
            {
                savedWorldStates.Add(new WorldObjectState
                {
                    objectID = worldObj.UniqueID,
                    isInteractableStateChanged = worldObj.IsStateChanged,
                    isActive = worldObj.gameObject.activeSelf
                });
            }
        }

        if (inv != null)
        {
            inv.GetSaveData(out inventoryIndices, out inventoryItemNames);
        }
    }
}