using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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
    public bool isDialogueOnInteract;
    public bool isDisableAlert;
    public GameObject Alert;

    [Header("DIALOGUE TO QUEST")]
    public bool startQuestAfterDialogue;
    public int questNumberToStart;

    //[Header("MOVING NPC SETTINGS")]
    //public bool isMovingNPC;
    //public NPCWaypointMovement npcMovement;

    //[Header("ENEMY SETTINGS")]
    //public bool activateEnemyAI;
    //public GameObject enemyObject; // Assign enemy GameObject here
    //public MonoBehaviour enemyAIScript; // Assign the script like EnemyAI.cs

    [Header("POST-DIALOGUE OBJECT TOGGLES")]
    //public bool useObjectToggles;
    public GameObject[] objectsToEnable;
    public GameObject[] objectsToDisable;

    [Header("PLAYER & UI")]
    public bool openPanelAfterDialogue;
    public PlayerManager playerManager;

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
        // if (isMovingNPC && npcMovement != null)
        //{
        //     npcMovement.PauseMovement();
        // }
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

            // Hide the alert prompt when walking away
            if (Alert != null)
            {
                Alert.SetActive(false);
            }
        }
    }
    public void TriggerDialogueInteract()
    {
        if (isDialogueOnInteract)
        {
            TriggerDialogue();
        }
    }

    public void OnDialogueComplete()
    {
        // Handle enemy activation
        //if (activateEnemyAI && enemyAIScript != null)
        //{
        //    enemyAIScript.enabled = true;

        //    if (enemyObject != null)
        //    {
        //        BoxCollider2D[] colliders = enemyObject.GetComponents<BoxCollider2D>();
        //        foreach (BoxCollider2D col in colliders)
        //        {
        //            if (col.isTrigger)
        //            {
        //                col.enabled = true;
        //            }
        //        }
        //    }
        //}

        //// Toggle objects
        //if (useObjectToggles)
        //{
        //    foreach (GameObject go in objectsToEnable)
        //        if (go != null) go.SetActive(true);

        //    foreach (GameObject go in objectsToDisable)
        //        if (go != null) go.SetActive(false);
        //}

        // Resume NPC
        //if (isMovingNPC && npcMovement != null)
        //{
        //    npcMovement.ResumeMovement();
        //}

        // Disable self if needed
        if (disableAfterDialogue)
        {
            gameObject.SetActive(false);
        }

        // Only show panel if toggle is enabled
        // if (openPanelAfterDialogue && playerManager != null)
        //{
        //    if (playerManager.hasMagicSpellBook)
        //    {
        //        if (withSpellBookPanel != null)
        //            withSpellBookPanel.SetActive(true);
        //    }
        //    else
        //    {
        //        if (withoutSpellBookPanel != null)
        //            withoutSpellBookPanel.SetActive(true);
        //    }
        //}
    }
}

