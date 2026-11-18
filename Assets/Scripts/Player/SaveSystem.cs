using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Inventory.Model;

public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance { get; private set; }

    public static bool IsLoadingSave { get; private set; } = false;

    [Header("References")]
    public PlayerManager playerManager;
    public InventorySO playerInventory;
    public QuestManager questManager;
    public CameraFollow cameraFollow;

    [Header("Level 1 Inventory Config")]
    public ItemSO flashlightItem;
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

        if (playerInventory != null && !IsLoadingSave)
        {
            playerInventory.InitializeLevel1Inventory(flashlightItem);
        }
    }

    public bool HasSaveFile()
    {
        return File.Exists(saveFilePath);
    }

    public void DeleteSaveFile()
    {
        if (HasSaveFile())
        {
            File.Delete(saveFilePath);
            Debug.Log("[SaveSystem] Save file deleted.");
        }
    }

    public void LoadGameFromMainMenu()
    {
        if (!HasSaveFile())
        {
            Debug.LogWarning("[SaveSystem] No save file found!");
            return;
        }

        Time.timeScale = 1f;
        StartCoroutine(LoadSceneAndApplySaveRoutine());
    }

    private IEnumerator LoadSceneAndApplySaveRoutine()
    {
        Debug.Log("<color=yellow>[SaveSystem] --- STARTING LOAD ROUTINE ---</color>");
        IsLoadingSave = true;

        string json = File.ReadAllText(saveFilePath);
        PlayerData data = JsonUtility.FromJson<PlayerData>(json);

        if (data == null || string.IsNullOrEmpty(data.sceneName))
        {
            Debug.LogError("[SaveSystem] Save data corrupt or missing sceneName.");
            IsLoadingSave = false;
            yield break;
        }

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(data.sceneName);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // Wait 2 frames to ensure all scene Start() and Awake() methods finish execution first
        yield return null;
        yield return null;

        EnsureReferences();
        ApplyDataToScene(data);

        IsLoadingSave = false;
        Debug.Log("<color=green>[SaveSystem] --- LOAD & SCENE TRANSITION COMPLETE ---</color>");
    }

    private void EnsureReferences()
    {
        playerManager = PlayerManager.Instance ?? FindFirstObjectByType<PlayerManager>();
        questManager = FindFirstObjectByType<QuestManager>();
        if (cameraFollow == null)
            cameraFollow = FindFirstObjectByType<CameraFollow>();
    }

    private void ApplyDataToScene(PlayerData data)
    {
        // 0. Close active dialogue
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.ForceCloseDialogue();
        }

        // 1. Restore Player Stats & Position directly
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

        // 1.5 Restore Camera Bounds
        if (cameraFollow != null && data.hasCameraBounds &&
            data.cameraMinBounds != null && data.cameraMinBounds.Length == 2 &&
            data.cameraMaxBounds != null && data.cameraMaxBounds.Length == 2)
        {
            cameraFollow.useBounds = data.useCameraBounds;
            Vector2 loadedMin = new Vector2(data.cameraMinBounds[0], data.cameraMinBounds[1]);
            Vector2 loadedMax = new Vector2(data.cameraMaxBounds[0], data.cameraMaxBounds[1]);
            cameraFollow.SetBounds(loadedMin, loadedMax);

            if (playerManager != null)
            {
                Vector3 targetPos = playerManager.transform.position + cameraFollow.offset;
                if (cameraFollow.useBounds)
                {
                    targetPos.x = Mathf.Clamp(targetPos.x, loadedMin.x, loadedMax.x);
                    targetPos.y = Mathf.Clamp(targetPos.y, loadedMin.y, loadedMax.y);
                }
                cameraFollow.transform.position = targetPos;
            }
        }

        // 2. Restore Inventory
        if (playerInventory != null)
        {
            playerInventory.RestoreFromSaveData(data.inventoryIndices, data.inventoryItemNames, itemDatabase, flashlightItem);
        }

        // 3. Restore World Objects, Doors, QuestObjects & QuestTriggers
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

                    // If this object is a door, force refresh its door state
                    UnlockHallwayDoor door = worldObj.GetComponent<UnlockHallwayDoor>();
                    if (door != null)
                    {
                        door.ApplyLoadedState(savedState.isInteractableStateChanged);
                    }
                }
            }
        }

        // 4. Restore Quests & Sync Quest UI after world states are restored
        if (questManager != null)
        {
            questManager.ApplyLoadedQuests(data.questCompleted, data.itemCollected);
        }
    }

    public void SaveGame()
    {
        EnsureReferences();

        try
        {
            PlayerData data = new PlayerData(playerManager, playerInventory, questManager, cameraFollow);
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
            Time.timeScale = 1f;
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