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

    [Header("UI ANIMATIONS")]
    [Tooltip("Animators for Canvas UI elements that slide out during dialogue and slide back in after.")]
    public Animator[] uiElementAnimators;

    [Header("PORTRAIT SETTINGS")]
    [Tooltip("The Image UI component on the LEFT (for NPCs)")]
    public Image leftCharacterIcon;

    [Tooltip("The Image UI component on the RIGHT (for Main Character)")]
    public Image rightCharacterIcon;

    [Header("FULLSCREEN IMAGE UI")]
    public GameObject fullscreenImageContainer;
    public Image fullscreenImageDisplay;

    [Header("TEXT SETTINGS")]
    public TextMeshProUGUI characterName;
    public TextMeshProUGUI dialogueArea;
    public GameObject continuePrompt;

    private Queue<DialogueLine> lines;

    public bool isDialogueActive = false;

    public float typingSpeed = 0.02f;

    public Animator animator;

    private DialogueTrigger currentTrigger;

    public bool isTimelineControllingPlayer = false;

    public event System.Action OnDialogueEnded;

    // NEW:
    // True only when the current dialogue line has completely finished typing
    private bool isTypingFinished = false;


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
        // Space can only advance dialogue AFTER the current line
        // has completely finished typing
        if (isDialogueActive &&
            isTypingFinished &&
            Keyboard.current != null &&
            Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            DisplayNextDialogueLine();
        }
    }


    public void StartDialogue(Dialogue dialogue, DialogueTrigger trigger = null)
    {
        currentTrigger = trigger;

        DialogueBox.SetActive(true);
        isDialogueActive = true;

        // Reset typing state
        isTypingFinished = false;

        // Hide continue prompt when dialogue starts
        if (continuePrompt != null)
        {
            continuePrompt.SetActive(false);
        }

        // Slide out canvas elements when dialogue starts
        TriggerUIAnimations("SlideOut");

        if (animator != null)
        {
            animator.Play("show");
        }

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

        // New dialogue line is now being typed
        isTypingFinished = false;

        // Hide Continue prompt while typing
        if (continuePrompt != null)
        {
            continuePrompt.SetActive(false);
        }

        DialogueLine currentLine = lines.Dequeue();


        // 1. Handle Fullscreen Image per line
        if (currentLine.hasImage && currentLine.fullscreenImage != null)
        {
            if (fullscreenImageDisplay != null)
            {
                fullscreenImageDisplay.sprite = currentLine.fullscreenImage;
            }

            if (fullscreenImageContainer != null)
            {
                fullscreenImageContainer.SetActive(true);
            }
        }
        else
        {
            if (fullscreenImageContainer != null)
            {
                fullscreenImageContainer.SetActive(false);
            }
        }


        // 2. Handle Speaker Portraits
        if (currentLine.character.isPlayer)
        {
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
        // Make sure Continue is hidden while typing
        isTypingFinished = false;

        if (continuePrompt != null)
        {
            continuePrompt.SetActive(false);
        }

        dialogueArea.text = "";

        foreach (char letter in dialogueLine.line.ToCharArray())
        {
            dialogueArea.text += letter;

            yield return new WaitForSeconds(typingSpeed);
        }


        // IMPORTANT:
        // This happens ONLY after the entire sentence has finished typing
        isTypingFinished = true;

        if (continuePrompt != null)
        {
            continuePrompt.SetActive(true);
        }
    }


    public void EndDialogue()
    {
        StartCoroutine(EndDialogueRoutine());
    }


    private IEnumerator EndDialogueRoutine()
    {
        isDialogueActive = false;

        isTypingFinished = false;

        if (continuePrompt != null)
        {
            continuePrompt.SetActive(false);
        }

        if (fullscreenImageContainer != null)
        {
            fullscreenImageContainer.SetActive(false);
        }

        if (animator != null)
        {
            animator.Play("hide");
        }

        // Slide canvas elements back in when dialogue ends
        TriggerUIAnimations("SlideIn");

        yield return new WaitForSeconds(0.5f);

        if (leftCharacterIcon != null)
            leftCharacterIcon.gameObject.SetActive(false);

        if (rightCharacterIcon != null)
            rightCharacterIcon.gameObject.SetActive(false);

        DialogueBox.SetActive(false);

        if (currentTrigger != null)
        {
            currentTrigger.OnDialogueComplete();
        }

        DisableDialogue();

        OnDialogueEnded?.Invoke();
    }


    public void DisableDialogue()
    {
        currentTrigger = null;
    }


    private void TriggerUIAnimations(string stateName)
    {
        if (uiElementAnimators == null)
            return;

        foreach (Animator elemAnimator in uiElementAnimators)
        {
            if (elemAnimator != null)
            {
                elemAnimator.Play(stateName);
            }
        }
    }
}
