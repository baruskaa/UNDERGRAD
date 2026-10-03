using UnityEngine;

public class BathroomDoor : MonoBehaviour
{
    [Header("Door Models / Sprites")]
    public GameObject closedDoorObject;
    public GameObject openDoorObject;

    [Header("Objects to Enable when Door Opens")]
    [Tooltip("Drag any lights, triggers, or items inside the cubicle that should turn on when opened.")]
    public GameObject[] objectsToEnable;

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
        // Only allow interaction if door is NOT yet opened
        if (!isOpen && isPlayerInRange && Input.GetKeyDown(interactKey))
        {
            InteractWithDoor();
        }
    }

    // --- TRIGGER DETECTION ---
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInRange = true;
            // Only show alert prompt if door is still closed
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
        // Guard Clause: Prevent closing if door is already open
        if (isOpen) return;

        isOpen = true;

        if (saveableObject != null)
        {
            saveableObject.SetStateChanged(true);
        }

        // Hide alert UI permanently once opened
        if (playerAlert != null)
        {
            playerAlert.SetActive(false);
        }

        UpdateDoorState();
    }

    public void UpdateDoorState()
    {
        // 1. Swap door models
        if (closedDoorObject != null) closedDoorObject.SetActive(!isOpen);
        if (openDoorObject != null) openDoorObject.SetActive(isOpen);

        // 2. Enable objects inside cubicle
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

    /// <summary>
    /// Called by SaveableWorldObject during LoadGame()
    /// </summary>
    public void ApplyLoadedState(bool savedIsOpenState)
    {
        isOpen = savedIsOpenState;

        if (saveableObject != null)
        {
            saveableObject.SetStateChanged(isOpen);
        }

        // Ensure alert prompt is hidden if loading an already opened door
        if (isOpen && playerAlert != null)
        {
            playerAlert.SetActive(false);
        }

        UpdateDoorState();
    }
}