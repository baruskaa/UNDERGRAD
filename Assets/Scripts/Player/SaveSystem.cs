using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Inventory.Model;

public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance { get; private set; }

    /// <summary>
    /// Static flag that prevents triggers (like start dialogues) from firing while a save file is loading.
    /// </summary>
    public static bool IsLoadingSave { get; private set; } = false;

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

        // Initialize Level 1 inventory baseline if starting fresh and not restoring a save file
        if (playerInventory != null && !IsLoadingSave)
        {
            playerInventory.InitializeLevel1Inventory(flashlightItem);
        }
    }

    /// <summary>
    /// Utility method to check if save JSON exists for Main Menu UI validation
    /// </summary>
    public bool HasSaveFile()
    {
        return File.Exists(saveFilePath);
    }

    /// <summary>
    /// Call this from Main Menu Load Button
    /// </summary>
    public void LoadGameFromMainMenu()
    {
        if (!HasSaveFile())
        {
            Debug.LogWarning("[SaveSystem] No save file found!");
            return;
        }

        StartCoroutine(LoadSceneAndApplySaveRoutine());
    }

    private IEnumerator LoadSceneAndApplySaveRoutine()
    {
        Debug.Log("<color=yellow>[SaveSystem] --- STARTING MAIN MENU LOAD ---</color>");
        IsLoadingSave = true; // Lock triggers during loading

        string json = File.ReadAllText(saveFilePath);
        PlayerData data = JsonUtility.FromJson<PlayerData>(json);

        if (data == null || string.IsNullOrEmpty(data.sceneName))
        {
            Debug.LogError("[SaveSystem] Save data corrupt or missing sceneName.");
            IsLoadingSave = false;
            yield break;
        }

        // 1. Load target level asynchronously
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(data.sceneName);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // Wait 1 frame to ensure Scene Awake/Start runs completely
        yield return null;

        // 2. Re-bind scene objects & restore saved states
        EnsureReferences();
        ApplyDataToScene(data);

        IsLoadingSave = false; // Unlock triggers after restoration finishes
        Debug.Log("<color=green>[SaveSystem] --- LOAD & SCENE TRANSITION COMPLETE ---</color>");
    }

    private void EnsureReferences()
    {
        if (playerManager == null)
            playerManager = PlayerManager.Instance ?? FindObjectOfType<PlayerManager>();

        if (questManager == null)
            questManager = FindObjectOfType<QuestManager>();
    }

    private void ApplyDataToScene(PlayerData data)
    {
        // 0. Instantly kill any dialogue UI that auto-triggered during scene load
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.ForceCloseDialogue();
        }

        // 1. Player Stats & Position
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

        // 2. Quests
        if (questManager != null && data.questCompleted != null)
        {
            questManager.questCompleted = (bool[])data.questCompleted.Clone();
            questManager.itemCollected = data.itemCollected;
        }

        // 3. Inventory
        if (playerInventory != null)
        {
            playerInventory.RestoreFromSaveData(data.inventoryIndices, data.inventoryItemNames, itemDatabase, flashlightItem);
        }

        // 4. Restore World Objects & Dialogue Triggers
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
                    worldObj.RestoreState(savedState);
                }
            }
        }
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
        if (!HasSaveFile()) return;

        EnsureReferences();

        try
        {
            IsLoadingSave = true;
            string json = File.ReadAllText(saveFilePath);
            PlayerData data = JsonUtility.FromJson<PlayerData>(json);
            if (data != null) ApplyDataToScene(data);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"<color=red>[SaveSystem] LOAD EXCEPTION:</color> {e.Message}");
        }
        finally
        {
            IsLoadingSave = false;
        }
    }
}