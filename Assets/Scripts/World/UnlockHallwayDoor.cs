using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Inventory.Model;

public class UnlockHallwayDoor : MonoBehaviour
{
    [Header("KEY SETTINGS")]
    [Tooltip("The ScriptableObject item representing the key needed for this door.")]
    public ItemSO requiredKey;
    public InventorySO playerInventory;

    [Header("POST-UNLOCK OBJECT TOGGLES")]
    public List<GameObject> objectsToEnable = new List<GameObject>();
    public List<GameObject> objectsToDisable = new List<GameObject>();

    [Header("ALERT GAMEOBJECTS")]
    public GameObject regularAlert; // Alert shown when player DOES NOT have the key
    public GameObject keyAlert;     // Alert shown when player HAS the key

    [Header("LOCKED DOOR DIALOGUE")]
    public Dialogue lockedDialogue;

    private bool isPlayerInRange = false;
    private bool isUnlocked = false;

    private void Update()
    {
        if (isUnlocked || !isPlayerInRange) return;

        // Press 'Q' to interact with the door
        if (Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame)
        {
            InteractWithDoor();
        }
    }

    private void InteractWithDoor()
    {
        bool hasKey = playerInventory != null && playerInventory.HasItem(requiredKey);

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
        isUnlocked = true;

        // Disable both alerts upon unlocking
        HideAlerts();

        // Enable target GameObjects
        if (objectsToEnable != null)
        {
            foreach (GameObject obj in objectsToEnable)
            {
                if (obj != null) obj.SetActive(true);
            }
        }

        // Disable target GameObjects
        if (objectsToDisable != null)
        {
            foreach (GameObject obj in objectsToDisable)
            {
                if (obj != null) obj.SetActive(false);
            }
        }

        // Disable this interaction trigger so it can't be reused
        gameObject.SetActive(false);
    }

    private void TriggerLockedDialogue()
    {
        if (DialogueManager.Instance != null && !DialogueManager.Instance.isDialogueActive)
        {
            // Hide alerts while dialogue box is open
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
        bool hasKey = playerInventory != null && playerInventory.HasItem(requiredKey);

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