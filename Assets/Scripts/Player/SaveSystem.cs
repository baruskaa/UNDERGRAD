using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Inventory.Model;

public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance { get; private set; }

    [Header("References")]
    public PlayerManager playerManager;
    public InventorySO playerInventory;
    public QuestManager questManager;

    [Header("Level 1 Inventory Config")]
    [Tooltip("Drag your Flashlight ItemSO here to guarantee it in Level 1.")]
    public ItemSO flashlightItem;

    [Tooltip("Drag ALL ItemSO assets in your project here so SaveSystem can reconstruct inventory items by name.")]
    public List<ItemSO> itemDatabase;

    private string saveFilePath;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        saveFilePath = Path.Combine(Application.persistentDataPath, "savegame.json");
    }

    private void Start()
    {
        EnsureReferences();

        // Initialize Level 1 inventory baseline
        if (playerInventory != null)
        {
            playerInventory.InitializeLevel1Inventory(flashlightItem);
        }
    }

    private void Update()
    {
        // 1. Save Key (K)
        if (Input.GetKeyDown(KeyCode.K))
        {
            Debug.Log("<color=cyan>[SaveSystem] 'K' Key Pressed -> Triggering Save</color>");
            SaveGame();
        }

        // 2. Load Key (L)
        if (Input.GetKeyDown(KeyCode.L))
        {
            Debug.Log("<color=cyan>[SaveSystem] 'L' Key Pressed -> Triggering Load</color>");
            LoadGame();
        }
    }

    /// <summary>
    /// Re-binds scene references if they were destroyed or cleared during a scene reload.
    /// </summary>
    private void EnsureReferences()
    {
        if (playerManager == null)
            playerManager = PlayerManager.Instance ?? FindObjectOfType<PlayerManager>();

        if (questManager == null)
            questManager = FindObjectOfType<QuestManager>();
    }

    public void SaveGame()
    {
        EnsureReferences();

        try
        {
            PlayerData data = new PlayerData(playerManager, playerInventory, questManager);
            string json = JsonUtility.ToJson(data, true);

            File.WriteAllText(saveFilePath, json);
            Debug.Log($"<color=green>[SaveSystem] SAVED SUCCESSFULLY to: {saveFilePath}</color>");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"<color=red>[SaveSystem] SAVE ERROR:</color> {e.Message}");
        }
    }

    public void LoadGame()
    {
        Debug.Log("<color=yellow>[SaveSystem] --- STARTING LOAD ---</color>");

        if (!File.Exists(saveFilePath))
        {
            Debug.LogWarning($"[SaveSystem] No save file found at path: {saveFilePath}");
            return;
        }

        EnsureReferences();

        try
        {
            string json = File.ReadAllText(saveFilePath);
            PlayerData data = JsonUtility.FromJson<PlayerData>(json);

            if (data == null)
            {
                Debug.LogError("[SaveSystem] JSON deserialization resulted in null PlayerData.");
                return;
            }

            // 1. Restore Player Stats & Position
            if (playerManager != null)
            {
                playerManager.currentHealth = data.health;
                playerManager.currentHunger = data.hunger;

                if (data.position != null && data.position.Length == 3)
                {
                    playerManager.transform.position = new Vector3(data.position[0], data.position[1], data.position[2]);
                }

                playerManager.UpdateUI();
            }

            // 2. Restore Quests
            if (questManager != null && data.questCompleted != null)
            {
                questManager.questCompleted = (bool[])data.questCompleted.Clone();
                questManager.itemCollected = data.itemCollected;
            }

            // 3. Restore Inventory
            if (playerInventory != null)
            {
                playerInventory.RestoreFromSaveData(data.inventoryIndices, data.inventoryItemNames, itemDatabase, flashlightItem);
            }

            // Inside SaveSystem.cs -> LoadGame() -> Section 4:
            if (data.savedWorldStates != null)
            {
                SaveableWorldObject[] worldObjects = FindObjectsByType<SaveableWorldObject>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None
                );

                foreach (var worldObj in worldObjects)
                {
                    var savedState = data.savedWorldStates.Find(x => x.objectID == worldObj.UniqueID);
                    if (savedState != null)
                    {
                        worldObj.RestoreState(savedState); // Passes the full state including isActive
                    }
                }
            }

            Debug.Log("<color=green>[SaveSystem] --- LOAD COMPLETE ---</color>");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"<color=red>[SaveSystem] LOAD EXCEPTION:</color> {e.Message}");
        }
    }
}