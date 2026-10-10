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
    public Image leftCharacterIcon;
    public Image rightCharacterIcon;

    [Header("FULLSCREEN IMAGE UI")]
    public GameObject fullscreenImageContainer;
    public Image fullscreenImageDisplay;

    [Header("TEXT SETTINGS")]
    public TextMeshProUGUI characterName;
    public TextMeshProUGUI dialogueArea;

    [Header("ANIMATORS")]
    public Animator playerStatsAnimator;
    public Animator controlsAnimator;
    public Animator questAnimator;
    public string playerStatsHideState = "SlideOut";
    public string playerStatsShowState = "SlideIn";


    private Queue<DialogueLine> lines;

    public bool isDialogueActive = false;
    public float typingSpeed = 0.02f;

    [Header("DIALOGUE BOX ANIMATOR")]
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
        // Press Space to advance dialogue
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

        if (animator != null)
        {
            animator.Play("show");
        }

        if (playerStatsAnimator != null)
        {
            playerStatsAnimator.Play(playerStatsHideState);
            controlsAnimator.Play("SlideOut");
            questAnimator.Play("SlideOut");
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

        // 1. Fullscreen Image Handling
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

        // 2. Portrait Handling
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
            // Use Realtime so typing doesn't freeze when Time.timeScale = 0
            yield return new WaitForSecondsRealtime(typingSpeed);
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

        if (playerStatsAnimator != null) playerStatsAnimator.Play(playerStatsShowState);
        if (controlsAnimator != null) controlsAnimator.Play("SlideIn");

        // Slide Quest UI back in
        QuestBoxManager qbm = FindFirstObjectByType<QuestBoxManager>();
        if (questAnimator != null && qbm != null && qbm.isQuestActive)
        {
            questAnimator.Play("SlideIn");
        }
        

        yield return new WaitForSeconds(0.5f);

        if (leftCharacterIcon != null) leftCharacterIcon.gameObject.SetActive(false);
        if (rightCharacterIcon != null) rightCharacterIcon.gameObject.SetActive(false);

        DialogueBox.SetActive(false);

        // Triggers OnDialogueComplete() which flags SaveableWorldObject state as changed
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

    /// <summary>
    /// Instantly closes and resets the dialogue UI (used when loading a save file)
    /// </summary>
    public void ForceCloseDialogue()
    {
        StopAllCoroutines();
        isDialogueActive = false;
        currentTrigger = null;

        if (DialogueBox != null) DialogueBox.SetActive(false);
        if (fullscreenImageContainer != null) fullscreenImageContainer.SetActive(false);
        if (leftCharacterIcon != null) leftCharacterIcon.gameObject.SetActive(false);
        if (rightCharacterIcon != null) rightCharacterIcon.gameObject.SetActive(false);

        if (playerStatsAnimator != null)
        {
            playerStatsAnimator.Play(playerStatsShowState);
            if (controlsAnimator != null) controlsAnimator.Play("SlideIn");
        }
    }
}