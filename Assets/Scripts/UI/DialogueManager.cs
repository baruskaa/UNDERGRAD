using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;
    public GameObject DialogueBox;

    public PlayerManager playerManager;

    [Header("PORTRAIT SETTINGS")]
    [Tooltip("The Image UI component on the LEFT (for NPCs)")]
    public Image leftCharacterIcon;

    [Tooltip("The Image UI component on the RIGHT (for Main Character)")]
    public Image rightCharacterIcon;

    [Header("TEXT SETTINGS")]
    public TextMeshProUGUI characterName;
    public TextMeshProUGUI dialogueArea;

    private Queue<DialogueLine> lines;

    public bool isDialogueActive = false;

    public float typingSpeed = 0.02f;

    public Animator animator;

    private DialogueTrigger currentTrigger;

    public bool isTimelineControllingPlayer = false;

    public event System.Action OnDialogueEnded;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        lines = new Queue<DialogueLine>();
    }

    private void Update()
    {
        // Press Space to advance to the next dialogue line
        if (isDialogueActive && Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            DisplayNextDialogueLine();
        }
    }

    public void StartDialogue(Dialogue dialogue, DialogueTrigger trigger = null)
    {
        currentTrigger = trigger;

        DialogueBox.SetActive(true);
        isDialogueActive = true;

        animator.Play("show");

        lines.Clear();

        foreach (DialogueLine dialogueLine in dialogue.dialogueLines)
        {
            lines.Enqueue(dialogueLine);
        }

        DisplayNextDialogueLine();
    }

    public void DisplayNextDialogueLine()
    {
        if (lines.Count == 0)
        {
            EndDialogue();
            return;
        }

        DialogueLine currentLine = lines.Dequeue();

        // Handle Speaker Portraits (Left vs Right)
        if (currentLine.character.isPlayer)
        {
            // Show Right Portrait (Player), Hide Left
            if (rightCharacterIcon != null)
            {
                rightCharacterIcon.sprite = currentLine.character.icon;
                rightCharacterIcon.gameObject.SetActive(true);
            }
            if (leftCharacterIcon != null)
            {
                leftCharacterIcon.gameObject.SetActive(false);
            }
        }
        else
        {
            // Show Left Portrait (NPC), Hide Right
            if (leftCharacterIcon != null)
            {
                leftCharacterIcon.sprite = currentLine.character.icon;
                leftCharacterIcon.gameObject.SetActive(true);
            }
            if (rightCharacterIcon != null)
            {
                rightCharacterIcon.gameObject.SetActive(false);
            }
        }

        characterName.text = currentLine.character.name;

        StopAllCoroutines();
        StartCoroutine(TypeSentence(currentLine));
    }

    IEnumerator TypeSentence(DialogueLine dialogueLine)
    {
        dialogueArea.text = "";
        foreach (char letter in dialogueLine.line.ToCharArray())
        {
            dialogueArea.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    public void EndDialogue()
    {
        StartCoroutine(EndDialogueRoutine());
    }

    private IEnumerator EndDialogueRoutine()
    {
        isDialogueActive = false;

        // 1. Play the slide-down animation while portraits are still visible
        if (animator != null)
        {
            animator.Play("hide");
        }

        // 2. Wait for the slide-down animation to complete
        yield return new WaitForSeconds(0.5f);

        // 3. Hide both portraits AFTER the animation finishes sliding down
        if (leftCharacterIcon != null) leftCharacterIcon.gameObject.SetActive(false);
        if (rightCharacterIcon != null) rightCharacterIcon.gameObject.SetActive(false);

        DialogueBox.SetActive(false);

        if (currentTrigger != null) currentTrigger.OnDialogueComplete();

        DisableDialogue();

        OnDialogueEnded?.Invoke();
    }

    public void DisableDialogue()
    {
        currentTrigger = null;
    }
}