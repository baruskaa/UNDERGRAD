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
    public GameObject fullscreenImageContainer; // Parent panel or GameObject holding the Image
    public Image fullscreenImageDisplay;        // UI Image component showing the sprite

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

        // 2. Handle Speaker Portraits (Left vs Right)
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

    private void TriggerUIAnimations(string stateName)
    {
        if (uiElementAnimators == null) return;

        foreach (Animator elemAnimator in uiElementAnimators)
        {
            if (elemAnimator != null)
            {
                elemAnimator.Play(stateName);
            }
        }
    }
}