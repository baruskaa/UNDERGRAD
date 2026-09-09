using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Inventory.Model;

public class BarricadeInteraction : MonoBehaviour
{
    [Header("BARRICADE GAMEOBJECTS")]
    [SerializeField] private GameObject barricadeEnabled;  // Active intact barricade
    [SerializeField] private GameObject barricadeDisabled; // Broken/cleared barricade visual (optional)

    [Header("CROWBAR SETTINGS")]
    public ItemSO crowbarSO;
    public InventorySO playerInventory;

    [Header("POST-BREAK OBJECTS")]
    public List<GameObject> objectsToEnable = new List<GameObject>();

    [Header("ALERT GAMEOBJECTS")]
    public GameObject regularAlert; // Alert shown when player DOES NOT have the crowbar
    public GameObject crowbarAlert; // Alert shown when player HAS the crowbar

    [Header("NO CROWBAR DIALOGUE")]
    public Dialogue noCrowbarDialogue;

    private bool isPlayerInRange = false;
    private bool isCleared = false;

    private void Start()
    {
        UpdateBarricadeVisuals();
    }

    private void Update()
    {
        if (isCleared || !isPlayerInRange) return;

        // Press 'Q' to interact with the barricade
        if (Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame)
        {
            InteractWithBarricade();
        }
    }

    private void InteractWithBarricade()
    {
        bool hasCrowbar = playerInventory != null && playerInventory.HasItem(crowbarSO);

        if (hasCrowbar)
        {
            RemoveBarricade();
        }
        else
        {
            TriggerNoCrowbarDialogue();
        }
    }

    private void RemoveBarricade()
    {
        isCleared = true;

        // Hide alert UI
        HideAlerts();

        // Swap barricade visual states
        UpdateBarricadeVisuals();

        // Enable post-break objects (e.g. pathways, new items, colliders)
        if (objectsToEnable != null)
        {
            foreach (GameObject obj in objectsToEnable)
            {
                if (obj != null) obj.SetActive(true);
            }
        }

        // Disable interaction trigger so it can't be reused
        gameObject.SetActive(false);
    }

    private void TriggerNoCrowbarDialogue()
    {
        if (DialogueManager.Instance != null && !DialogueManager.Instance.isDialogueActive)
        {
            HideAlerts();
            DialogueManager.Instance.StartDialogue(noCrowbarDialogue, null);
        }
    }

    private void UpdateBarricadeVisuals()
    {
        if (barricadeEnabled != null) barricadeEnabled.SetActive(!isCleared);
        if (barricadeDisabled != null) barricadeDisabled.SetActive(isCleared);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isCleared) return;

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
        bool hasCrowbar = playerInventory != null && playerInventory.HasItem(crowbarSO);

        if (hasCrowbar)
        {
            if (regularAlert != null) regularAlert.SetActive(false);
            if (crowbarAlert != null) crowbarAlert.SetActive(true);
        }
        else
        {
            if (crowbarAlert != null) crowbarAlert.SetActive(false);
            if (regularAlert != null) regularAlert.SetActive(true);
        }
    }

    private void HideAlerts()
    {
        if (regularAlert != null) regularAlert.SetActive(false);
        if (crowbarAlert != null) crowbarAlert.SetActive(false);
    }
}