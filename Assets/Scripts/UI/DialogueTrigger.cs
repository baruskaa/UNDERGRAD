using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class DialogueCharacter
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
    public bool hasImage; // Boolean toggle in Inspector
    public Sprite fullscreenImage;
}

[System.Serializable]
public class Dialogue
{
    public List<DialogueLine> dialogueLines = new List<DialogueLine>();
}

public class DialogueTrigger : MonoBehaviour
{
    private bool isPlayerInRange = false;

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

    private void Update()
    {
        // Press Q to interact with the NPC
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
        if (Alert != null)
        {
            Alert.SetActive(false);
        }
        DialogueManager.Instance.StartDialogue(dialogue, this);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = true;

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
        // 1. Enable designated GameObjects
        if (objectsToEnable != null)
        {
            foreach (GameObject go in objectsToEnable)
            {
                if (go != null) go.SetActive(true);
            }
        }

        // 2. Disable designated GameObjects
        if (objectsToDisable != null)
        {
            foreach (GameObject go in objectsToDisable)
            {
                if (go != null) go.SetActive(false);
            }
        }

        // 3. Trigger the Quest
        if (startQuestAfterDialogue)
        {
            QuestManager qm = FindAnyObjectByType<QuestManager>();
            if (qm != null && questNumberToStart < qm.quests.Length)
            {
                if (!qm.questCompleted[questNumberToStart] && !qm.quests[questNumberToStart].gameObject.activeSelf)
                {
                    qm.quests[questNumberToStart].gameObject.SetActive(true);
                    qm.quests[questNumberToStart].StartQuest();
                }
            }
            else
            {
                Debug.LogError($"QuestManager missing or Quest Index {questNumberToStart} out of bounds.");
            }
        }

        // 4. Disable trigger object if checked
        if (disableAfterDialogue)
        {
            gameObject.SetActive(false);
        }
    }
}