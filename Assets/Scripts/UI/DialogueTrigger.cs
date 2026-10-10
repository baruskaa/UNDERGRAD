using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public struct DialogueCharacter
{
    public string name;
    public Sprite icon;
    public bool isPlayer;
}

[System.Serializable]
public class DialogueLine
{
    public DialogueCharacter character;
    [TextArea(3, 10)]
    public string line;

    [Header("FULLSCREEN IMAGE SETTINGS")]
    public bool hasImage;
    public Sprite fullscreenImage;
}

[System.Serializable]
public class Dialogue
{
    [NonReorderable]
    public List<DialogueLine> dialogueLines = new List<DialogueLine>();
}

public class DialogueTrigger : MonoBehaviour
{
    private bool isPlayerInRange = false;
    private bool isInitialized = false;

    [Header("DIALOGUE SETTINGS")]
    public Dialogue dialogue;
    public bool disableAfterDialogue;
    public bool isDialogueOnInteract = true;
    public GameObject Alert;

    [Header("DIALOGUE TO QUEST")]
    public bool startQuestAfterDialogue;
    public int questNumberToStart;

    [Header("POST-DIALOGUE OBJECT TOGGLES")]
    public GameObject[] objectsToEnable;
    public GameObject[] objectsToDisable;

    private SaveableWorldObject saveableObject;

    private void Awake()
    {
        saveableObject = GetComponent<SaveableWorldObject>();
    }

    private IEnumerator Start()
    {
        // Wait 1 frame so SaveSystem can restore save data first if loading
        yield return null;

        // If SaveSystem loaded this object as already completed, cancel execution
        if (saveableObject != null && saveableObject.IsStateChanged)
        {
            if (disableAfterDialogue) gameObject.SetActive(false);
            yield break;
        }

        isInitialized = true;

        // NEW GAME AUTO-TRIGGER: If this dialogue triggers automatically (!isDialogueOnInteract)
        // and we are NOT loading a save file, trigger the intro dialogue!
        if (!isDialogueOnInteract && !SaveSystem.IsLoadingSave)
        {
            if (isPlayerInRange || IsPlayerOverlapping())
            {
                TriggerDialogue();
            }
        }
    }

    private bool IsPlayerOverlapping()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col == null) return false;

        ContactFilter2D filter = new ContactFilter2D().NoFilter();
        List<Collider2D> results = new List<Collider2D>();
        col.Overlap(filter, results);

        foreach (var c in results)
        {
            if (c.CompareTag("Player")) return true;
        }
        return false;
    }

    private void Update()
    {
        if (!isInitialized) return;
        if (saveableObject != null && saveableObject.IsStateChanged) return;

        // Press Q to interact with NPC
        if (isPlayerInRange && isDialogueOnInteract && Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame)
        {
            if (DialogueManager.Instance != null && !DialogueManager.Instance.isDialogueActive)
            {
                TriggerDialogue();
            }
        }
    }

    public void TriggerDialogue()
    {
        if (saveableObject != null && saveableObject.IsStateChanged)
        {
            if (disableAfterDialogue) gameObject.SetActive(false);
            return;
        }

        if (Alert != null)
        {
            Alert.SetActive(false);
        }

        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(dialogue, this);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = true;

            if (!isInitialized || SaveSystem.IsLoadingSave) return;
            if (saveableObject != null && saveableObject.IsStateChanged) return;

            if (isDialogueOnInteract)
            {
                if (Alert != null) Alert.SetActive(true);
            }
            else
            {
                if (DialogueManager.Instance != null && !DialogueManager.Instance.isDialogueActive)
                {
                    TriggerDialogue();
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = false;

            if (Alert != null)
            {
                Alert.SetActive(false);
            }
        }
    }

    public void OnDialogueComplete()
    {
        if (saveableObject != null)
        {
            saveableObject.SetStateChanged(true);
        }

        if (objectsToEnable != null)
        {
            foreach (GameObject go in objectsToEnable)
            {
                if (go != null) go.SetActive(true);
            }
        }

        if (objectsToDisable != null)
        {
            foreach (GameObject go in objectsToDisable)
            {
                if (go != null) go.SetActive(false);
            }
        }

        if (startQuestAfterDialogue)
        {
            QuestManager qm = FindFirstObjectByType<QuestManager>();
            if (qm != null && questNumberToStart < qm.quests.Length)
            {
                if (!qm.questCompleted[questNumberToStart] && !qm.quests[questNumberToStart].gameObject.activeSelf)
                {
                    qm.quests[questNumberToStart].gameObject.SetActive(true);
                    qm.quests[questNumberToStart].StartQuest();
                }
            }
        }

        if (disableAfterDialogue)
        {
            gameObject.SetActive(false);
        }
    }

    public void ApplyLoadedState(bool isCompleted, bool isActive)
    {
        if (saveableObject != null)
        {
            saveableObject.SetStateChanged(isCompleted);
        }

        if (isCompleted && disableAfterDialogue)
        {
            gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(isActive);
        }
    }
}