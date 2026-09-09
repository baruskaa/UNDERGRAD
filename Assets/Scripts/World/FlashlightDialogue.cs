using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Inventory.Model;

public class FlashlightDialogue : MonoBehaviour
{
    [Header("Inventory Reference")]
    public InventorySO inventoryData;
    public ItemSO flashlightItem;

    [Header("Flashlight Light Reference")]
    public GameObject flashlightLight;

    [Header("Dialogue Settings")]
    public Dialogue dialogue;
    [Tooltip("If checked, the dialogue and object activations will only happen the first time the player turns on the flashlight.")]
    public bool triggerOnlyOnce = true;

    [Header("OBJECT TOGGLES ON FLASHLIGHT ON")]
    public GameObject[] objectsToEnable;
    public GameObject[] objectsToDisable;

    private bool hasTriggered = false;
    private bool awaitingDialogueEnd = false;

    private void Update()
    {
        // Stop checking if already triggered and set to run only once
        if (triggerOnlyOnce && hasTriggered) return;

        // Check for F key press
        bool fKeyPressed = (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame) || Input.GetKeyDown(KeyCode.F);

        if (fKeyPressed)
        {
            // Don't trigger if dialogue is already running
            if (DialogueManager.Instance != null && DialogueManager.Instance.isDialogueActive) return;

            if (HasFlashlight())
            {
                StartCoroutine(CheckAndTriggerDialogue());
            }
        }
    }

    private bool HasFlashlight()
    {
        if (inventoryData == null) return false;

        if (flashlightItem != null && inventoryData.HasItem(flashlightItem))
        {
            return true;
        }

        // Fallback name search
        return inventoryData.HasItemByName("Flashlight");
    }

    private IEnumerator CheckAndTriggerDialogue()
    {
        // Wait one frame to allow PlayerMovement to update the flashlight active state
        yield return null;

        // Only trigger actions when turning the flashlight ON
        if (flashlightLight != null && flashlightLight.activeSelf)
        {
            hasTriggered = true;

            // Trigger Dialogue if available
            if (DialogueManager.Instance != null && dialogue != null && dialogue.dialogueLines != null && dialogue.dialogueLines.Count > 0)
            {
                awaitingDialogueEnd = true;
                DialogueManager.Instance.OnDialogueEnded += HandleDialogueEnded;
                DialogueManager.Instance.StartDialogue(dialogue);
            }
            else
            {
                // If no dialogue exists, enable/disable objects immediately
                ApplyObjectToggles();
            }
        }
    }

    private void HandleDialogueEnded()
    {
        if (!awaitingDialogueEnd) return;

        // Unsubscribe to avoid double-firing
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueEnded -= HandleDialogueEnded;
        }

        awaitingDialogueEnd = false;
        ApplyObjectToggles();
    }

    private void ApplyObjectToggles()
    {
        // 1. Enable specified GameObjects
        if (objectsToEnable != null)
        {
            foreach (GameObject go in objectsToEnable)
            {
                if (go != null) go.SetActive(true);
            }
        }

        // 2. Disable specified GameObjects
        if (objectsToDisable != null)
        {
            foreach (GameObject go in objectsToDisable)
            {
                if (go != null) go.SetActive(false);
            }
        }
    }

    private void OnDestroy()
    {
        // Clean up subscription if GameObject is destroyed during dialogue
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueEnded -= HandleDialogueEnded;
        }
    }
}