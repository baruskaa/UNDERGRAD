using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Inventory.Model;

public class UnlockHallwayDoor : MonoBehaviour
{
    [Header("SAVE SYSTEM")]
    public SaveableWorldObject saveableObject;

    [Header("KEY SETTINGS")]
    public ItemSO requiredKey;
    public InventorySO playerInventory;

    [Header("POST-UNLOCK OBJECT TOGGLES")]
    public List<GameObject> objectsToEnable = new List<GameObject>();
    public List<GameObject> objectsToDisable = new List<GameObject>();

    [Header("ALERT GAMEOBJECTS")]
    public GameObject regularAlert;
    public GameObject keyAlert;

    [Header("LOCKED DOOR DIALOGUE")]
    public Dialogue lockedDialogue;

    private bool isPlayerInRange = false;
    private bool isUnlocked = false;

    public bool IsUnlocked => isUnlocked;

    private void Awake()
    {
        if (saveableObject == null)
            saveableObject = GetComponent<SaveableWorldObject>();
    }

    private void Start()
    {
        // If state was already marked changed upon scene load, apply it immediately
        if (saveableObject != null && saveableObject.IsStateChanged)
        {
            SetUnlockedState(true);
        }
    }

    private void Update()
    {
        if (isUnlocked || !isPlayerInRange) return;

        if (Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame)
        {
            InteractWithDoor();
        }
    }

    private void InteractWithDoor()
    {
        // Check both direct reference and asset name for maximum reliability
        bool hasKey = playerInventory != null && requiredKey != null &&
            (playerInventory.HasItem(requiredKey) || playerInventory.HasItemByName(requiredKey.name));

        if (hasKey)
        {
            UnlockDoor();
        }
        else
        {
            TriggerLockedDialogue();
        }
    }

    private void UnlockDoor()
    {
        // Mark state as changed so SaveSystem saves this door as unlocked
        if (saveableObject != null)
        {
            saveableObject.SetStateChanged(true);
        }

        SetUnlockedState(true);
    }

    public void SetUnlockedState(bool unlocked)
    {
        isUnlocked = unlocked;

        if (unlocked)
        {
            HideAlerts();

            if (objectsToEnable != null)
            {
                foreach (GameObject obj in objectsToEnable) if (obj != null) obj.SetActive(true);
            }

            if (objectsToDisable != null)
            {
                foreach (GameObject obj in objectsToDisable) if (obj != null) obj.SetActive(false);
            }

            gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Called directly by SaveableWorldObject when SaveSystem loads the save file
    /// </summary>
    public void ApplyLoadedState(bool isStateChanged)
    {
        if (saveableObject != null)
        {
            saveableObject.SetStateChanged(isStateChanged);
        }

        SetUnlockedState(isStateChanged);
    }

    private void TriggerLockedDialogue()
    {
        if (DialogueManager.Instance != null && !DialogueManager.Instance.isDialogueActive)
        {
            HideAlerts();
            DialogueManager.Instance.StartDialogue(lockedDialogue, null);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isUnlocked) return;

        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = true;
            UpdateAlertState();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = false;
            HideAlerts();
        }
    }

    private void UpdateAlertState()
    {
        bool hasKey = playerInventory != null && requiredKey != null &&
            (playerInventory.HasItem(requiredKey) || playerInventory.HasItemByName(requiredKey.name));

        if (hasKey)
        {
            if (regularAlert != null) regularAlert.SetActive(false);
            if (keyAlert != null) keyAlert.SetActive(true);
        }
        else
        {
            if (keyAlert != null) keyAlert.SetActive(false);
            if (regularAlert != null) regularAlert.SetActive(true);
        }
    }

    private void HideAlerts()
    {
        if (regularAlert != null) regularAlert.SetActive(false);
        if (keyAlert != null) keyAlert.SetActive(false);
    }
}