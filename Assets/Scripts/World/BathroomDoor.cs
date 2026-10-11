using UnityEngine;
using Inventory.Model;

public class BathroomDoor : MonoBehaviour
{
    [Header("Door Models / Sprites")]
    public GameObject closedDoorObject;
    public GameObject openDoorObject;

    [Header("Objects to Enable when Door Opens")]
    [Tooltip("Drag any lights, triggers, or items inside the cubicle that should turn on when opened.")]
    public GameObject[] objectsToEnable;

    [Header("Key Requirement Settings")]
    public bool requiresKey = false;
    public ItemSO requiredKey;
    public InventorySO playerInventory;

    [Header("Locked Dialogue Settings")]
    public Dialogue lockedDialogue; // Dialogue to trigger when player lacks key

    [Header("Interaction & Alert Settings")]
    [Tooltip("UI prompt or alert object shown when player is near (e.g. 'Press Q to Open')")]
    public GameObject playerAlert;
    public KeyCode interactKey = KeyCode.Q;
    public string playerTag = "Player";

    [Header("Save System Reference")]
    public SaveableWorldObject saveableObject;

    [HideInInspector]
    public bool isOpen = false;
    private bool isPlayerInRange = false;

    private void Start()
    {
        if (saveableObject == null)
        {
            saveableObject = GetComponent<SaveableWorldObject>();
        }

        if (playerAlert != null)
        {
            playerAlert.SetActive(false);
        }

        // Sync visual state on start if save state was loaded
        if (saveableObject != null)
        {
            isOpen = saveableObject.IsStateChanged;
        }

        UpdateDoorState();
    }

    private void Update()
    {
        // Ignore interactions if door is open or if dialogue is actively playing
        if (isOpen || !isPlayerInRange) return;

        if (DialogueManager.Instance != null && DialogueManager.Instance.isDialogueActive) return;

        if (Input.GetKeyDown(interactKey))
        {
            InteractWithDoor();
        }
    }

    // --- KEY CHECK LOGIC ---
    private bool CheckHasKey()
    {
        if (!requiresKey) return true;

        InventorySO inv = playerInventory;
        if (inv == null && SaveSystem.Instance != null) inv = SaveSystem.Instance.playerInventory;

        if (inv == null || requiredKey == null) return false;

        return inv.HasItem(requiredKey) || inv.HasItemByName(requiredKey.name);
    }

    // --- TRIGGER DETECTION ---
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInRange = true;
            if (!isOpen && playerAlert != null) playerAlert.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInRange = false;
            if (playerAlert != null) playerAlert.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInRange = true;
            if (!isOpen && playerAlert != null) playerAlert.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInRange = false;
            if (playerAlert != null) playerAlert.SetActive(false);
        }
    }

    // --- DOOR LOGIC ---
    public void InteractWithDoor()
    {
        if (isOpen) return;

        if (requiresKey && !CheckHasKey())
        {
            TriggerLockedDialogue();
            return;
        }

        isOpen = true;

        if (saveableObject != null)
        {
            saveableObject.SetStateChanged(true);
        }

        if (playerAlert != null)
        {
            playerAlert.SetActive(false);
        }

        UpdateDoorState();
    }

    // --- DIALOGUE HANDLING (Reference: UnlockHallwayDoor) ---
    private void TriggerLockedDialogue()
    {
        if (lockedDialogue != null && DialogueManager.Instance != null && !DialogueManager.Instance.isDialogueActive)
        {
            if (playerAlert != null) playerAlert.SetActive(false);

            DialogueManager.Instance.OnDialogueEnded += OnLockedDialogueEnded;
            DialogueManager.Instance.StartDialogue(lockedDialogue, null);
        }
    }

    private void OnLockedDialogueEnded()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueEnded -= OnLockedDialogueEnded;
        }

        // Re-enable interaction prompt if player is still nearby
        if (isPlayerInRange && !isOpen && playerAlert != null)
        {
            playerAlert.SetActive(true);
        }
    }

    public void UpdateDoorState()
    {
        if (closedDoorObject != null) closedDoorObject.SetActive(!isOpen);
        if (openDoorObject != null) openDoorObject.SetActive(isOpen);

        if (objectsToEnable != null)
        {
            foreach (GameObject obj in objectsToEnable)
            {
                if (obj != null)
                {
                    obj.SetActive(isOpen);
                }
            }
        }
    }

    public void ApplyLoadedState(bool savedIsOpenState)
    {
        isOpen = savedIsOpenState;

        if (saveableObject != null)
        {
            saveableObject.SetStateChanged(isOpen);
        }

        if (isOpen && playerAlert != null)
        {
            playerAlert.SetActive(false);
        }

        UpdateDoorState();
    }
}