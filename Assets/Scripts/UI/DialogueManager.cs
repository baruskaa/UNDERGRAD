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

    [Header("FULLSCREEN IMAGE UI")]
    public GameObject fullscreenImageContainer; // Parent panel or GameObject holding the Image
    public Image fullscreenImageDisplay;        // UI Image component showing the sprite

    [Header("TEXT SETTINGS")]
    public TextMeshProUGUI characterName;
    public TextMeshProUGUI dialogueArea;

    [Header("PLAYER STATS ANIMATION")]
    public Animator playerStatsAnimator;
    public string playerStatsHideState = "SlideOut"; // Animation to hide stats during dialogue
    public string playerStatsShowState = "SlideIn";  // Animation to bring stats back after dialogue

    [Header("CONTROLS UI ANIMATION")]
    public Animator controlsAnimator;

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

        // 1. Play Dialogue Box show animation
        if (animator != null)
        {
            animator.Play("show");
        }

        // 2. Play Player Stats hide animation (SlideOut)
        if (playerStatsAnimator != null)
        {
            playerStatsAnimator.Play(playerStatsHideState);
            controlsAnimator.Play("SlideOut");
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

        // Hide full-screen cutscene panel immediately when dialogue concludes
        if (fullscreenImageContainer != null)
        {
            fullscreenImageContainer.SetActive(false);
        }

        // 1. Play the dialogue box hide animation & slide player stats back in
        if (animator != null)
        {
            animator.Play("hide");
        }

        if (playerStatsAnimator != null)
        {
            playerStatsAnimator.Play(playerStatsShowState);
            controlsAnimator.Play("SlideIn");
        }

        // 2. Wait for the animation transition to complete
        yield return new WaitForSeconds(0.5f);

        // 3. Hide both portraits AFTER the animation finishes
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