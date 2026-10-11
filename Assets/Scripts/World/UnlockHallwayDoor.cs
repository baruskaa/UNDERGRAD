using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Inventory.Model;

public class UnlockHallwayDoor : MonoBehaviour
{
    [Header("SAVE SYSTEM")]
    public SaveableWorldObject saveableObject;

    [Header("INSPECTION UI")]
    [Tooltip("Single GameObject reference for this door's inspection UI image panel.")]
    public GameObject doorInspectUI;

    [Header("BLACK FADE ANIMATOR")]
    public Animator fadeAnimator;
    public GameObject fader;                    // Drag the 'Black' GameObject here
    public string fadeOutAnimation = "Fadeout"; // State/Clip name that fades screen to black
    public string fadeInAnimation = "FadeIn";   // State/Clip name that fades screen back in
    public float fadeDuration = 1f;             // Duration of the fade animation in seconds
    public float fadeHoldDuration = 0.2f;       // Default hold duration on full black screen

    [Header("KEY SETTINGS")]
    public ItemSO requiredKey;
    public InventorySO playerInventory;

    [Header("POST-UNLOCK OBJECT TOGGLES")]
    public List<GameObject> objectsToEnable = new List<GameObject>();
    public List<GameObject> objectsToDisable = new List<GameObject>();

    [Header("ALERT GAMEOBJECTS")]
    public GameObject regularAlert;
    public GameObject keyAlert;
    public GameObject doorAlert; // Alert shown during inspection when player HAS key

    [Header("LOCKED DOOR DIALOGUE")]
    public Dialogue lockedDialogue;

    [Header("AUDIO & TRANSITION SETTINGS")]
    public AudioSource audioSource;
    public AudioClip unlockSound;
    public bool movePlayerOnUnlock = true;
    public Transform transitionPoint;
    public Vector2 newMinBounds;
    public Vector2 newMaxBounds;

    [Space(5)]
    [Tooltip("Check if unlocking this door should transition to another scene instead of moving locally.")]
    public bool loadNextScene = false;
    [Tooltip("The build index number of the scene to load.")]
    public int targetSceneBuildIndex = 0;

    private bool isPlayerInRange = false;
    private bool isUnlocked = false;
    private bool isInspecting = false;
    private bool isTransitioning = false;
    private GameObject playerRef;

    public bool IsUnlocked => isUnlocked;

    private void Awake()
    {
        if (saveableObject == null)
            saveableObject = GetComponent<SaveableWorldObject>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        EnsureFadeReferences();
    }

    private void Start()
    {
        if (saveableObject != null && saveableObject.IsStateChanged)
        {
            SetUnlockedState(true);
        }

        EnsureFadeReferences();
        if (fader != null) fader.SetActive(false);

        HideAllAlerts();

        // Safely set inspect UI inactive on start without calling DisableInspectUI
        if (doorInspectUI != null)
        {
            doorInspectUI.SetActive(false);
        }
    }

    private void OnDisable()
    {
        isInspecting = false;
        isTransitioning = false;
        SetPlayerPauseState(false);
        if (fader != null) fader.SetActive(false);
    }

    private void Update()
    {
        if (isUnlocked || !isPlayerInRange || isTransitioning) return;

        if (DialogueManager.Instance != null && DialogueManager.Instance.isDialogueActive) return;

        if (Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame)
        {
            InteractWithDoor();
        }
    }

    private void EnsureFadeReferences()
    {
        if (fader == null && fadeAnimator != null) fader = fadeAnimator.gameObject;
        if (fadeAnimator == null && fader != null) fadeAnimator = fader.GetComponent<Animator>();
    }

    private InventorySO GetActiveInventory()
    {
        InventorySO inv = playerInventory;
        if (inv == null && SaveSystem.Instance != null)
            inv = SaveSystem.Instance.playerInventory;

        return inv;
    }

    private bool CheckHasKey()
    {
        InventorySO inv = GetActiveInventory();
        if (inv == null || requiredKey == null) return false;

        return inv.HasItem(requiredKey) || inv.HasItemByName(requiredKey.name);
    }

    private void RemoveKeyFromInventory()
    {
        if (requiredKey == null) return;

        InventorySO inv = GetActiveInventory();
        if (inv != null)
        {
            inv.RemoveItemSO(requiredKey);
        }
    }

    private void SetPlayerPauseState(bool isPaused)
    {
        GameObject player = playerRef != null ? playerRef : GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        PlayerMovement movement = player.GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.isInspecting = isPaused;
        }
    }

    private void InteractWithDoor()
    {
        bool hasKey = CheckHasKey();

        if (isInspecting)
        {
            if (hasKey)
            {
                if (loadNextScene)
                {
                    _ = PerformUnlockAndTransitionNextScene();
                }
                else
                {
                    _ = PerformUnlockAndTransition();
                }
            }
            else
            {
                _ = CloseInspectionWithFade();
            }
        }
        else
        {
            _ = OpenInspectionWithFade();
        }
    }

    private void EnableInspectUI()
    {
        SetPlayerPauseState(true);

        if (doorInspectUI != null)
        {
            doorInspectUI.SetActive(true);
        }

        isInspecting = true;
    }

    private void DisableInspectUI()
    {
        if (doorInspectUI != null)
        {
            doorInspectUI.SetActive(false);
        }

        isInspecting = false;
        SetPlayerPauseState(false);
    }

    private static readonly int FadeInStateHash = Animator.StringToHash("FadeIn");
    private static readonly int FadeOutStateHash = Animator.StringToHash("FadeOut");

    private async Task PlayFadeToBlack()
    {
        EnsureFadeReferences();

        if (fader != null)
            fader.SetActive(true);

        if (fadeAnimator != null)
        {
            fadeAnimator.CrossFade(FadeInStateHash, 0f);
            await Task.Delay(Mathf.RoundToInt(fadeDuration * 1000f));
        }
    }

    private async Task PlayFadeFromBlack()
    {
        EnsureFadeReferences();

        if (fadeAnimator != null)
        {
            fadeAnimator.CrossFade(FadeOutStateHash, 0f);
            await Task.Delay(Mathf.RoundToInt(fadeDuration * 1000f));
        }

        if (fader != null)
            fader.SetActive(false);
    }

    private async Task OpenInspectionWithFade()
    {
        isTransitioning = true;
        HideProximityAlerts();

        await PlayFadeToBlack();
        EnableInspectUI();

        if (CheckHasKey() && doorAlert != null)
        {
            doorAlert.SetActive(true);
        }

        if (fadeHoldDuration > 0f)
            await Task.Delay(Mathf.RoundToInt(fadeHoldDuration * 1000f));

        await PlayFadeFromBlack();

        if (!CheckHasKey())
        {
            TriggerLockedDialogue();
        }

        isTransitioning = false;
    }

    private async Task CloseInspectionWithFade()
    {
        isTransitioning = true;

        if (doorAlert != null)
            doorAlert.SetActive(false);

        await PlayFadeToBlack();
        DisableInspectUI();

        if (fadeHoldDuration > 0f)
            await Task.Delay(Mathf.RoundToInt(fadeHoldDuration * 1000f));

        await PlayFadeFromBlack();

        if (isPlayerInRange)
            UpdateAlertState();

        isTransitioning = false;
    }

    private async Task PerformUnlockAndTransition()
    {
        isTransitioning = true;
        HideAllAlerts();

        await PlayFadeToBlack();
        DisableInspectUI();

        if (audioSource != null && unlockSound != null)
        {
            audioSource.PlayOneShot(unlockSound);
        }

        GameObject playerToMove = playerRef;

        if (playerToMove == null)
            playerToMove = GameObject.FindGameObjectWithTag("Player");

        if (movePlayerOnUnlock && playerToMove != null && transitionPoint != null)
        {
            playerToMove.transform.position = transitionPoint.position;

            CameraFollow cam = FindAnyObjectByType<CameraFollow>();

            if (cam != null)
                cam.SetBounds(newMinBounds, newMaxBounds);
        }

        await Task.Delay(Mathf.RoundToInt(2f * 1000f));

        await PlayFadeFromBlack();
        RemoveKeyFromInventory();
        UnlockDoor();

        

        isTransitioning = false;
    }

    private async Task PerformUnlockAndTransitionNextScene()
    {
        isTransitioning = true;
        HideAllAlerts();

        await PlayFadeToBlack();
        DisableInspectUI();

        if (audioSource != null && unlockSound != null)
        {
            audioSource.PlayOneShot(unlockSound);
        }

        await Task.Delay(Mathf.RoundToInt(2f * 1000f));

        RemoveKeyFromInventory();
        UnlockDoor();

        AsyncOperation sceneLoad = SceneManager.LoadSceneAsync(targetSceneBuildIndex);
        while (!sceneLoad.isDone)
        {
            await Task.Yield();
        }

        isTransitioning = false;
    }

    private void UnlockDoor()
    {
        if (saveableObject != null)
        {
            saveableObject.SetStateChanged(true);
        }

        SetUnlockedState(true);
    }

    public void SetUnlockedState(bool unlocked)
    {
        isUnlocked = unlocked;

        if (unlocked)
        {
            HideAllAlerts();
            DisableInspectUI();

            if (objectsToEnable != null)
            {
                foreach (GameObject obj in objectsToEnable) if (obj != null) obj.SetActive(true);
            }

            if (objectsToDisable != null)
            {
                foreach (GameObject obj in objectsToDisable) if (obj != null) obj.SetActive(false);
            }

            gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(true);
        }
    }

    public void ApplyLoadedState(bool isStateChanged)
    {
        if (saveableObject != null)
        {
            saveableObject.SetStateChanged(isStateChanged);
        }

        SetUnlockedState(isStateChanged);
    }

    private void TriggerLockedDialogue()
    {
        if (DialogueManager.Instance != null && !DialogueManager.Instance.isDialogueActive)
        {
            HideProximityAlerts();

            DialogueManager.Instance.OnDialogueEnded += OnLockedDialogueEnded;
            DialogueManager.Instance.StartDialogue(lockedDialogue, null);
        }
    }

    private void OnLockedDialogueEnded()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueEnded -= OnLockedDialogueEnded;
        }

        _ = CloseInspectionWithFade();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isUnlocked) return;

        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = true;
            playerRef = collision.gameObject;
            UpdateAlertState();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = false;
            playerRef = null;
            HideAllAlerts();
            DisableInspectUI();
        }
    }

    private void UpdateAlertState()
    {
        if (isInspecting) return;

        bool hasKey = CheckHasKey();

        if (hasKey)
        {
            if (regularAlert != null) regularAlert.SetActive(false);
            if (keyAlert != null) keyAlert.SetActive(true);
        }
        else
        {
            if (keyAlert != null) keyAlert.SetActive(false);
            if (regularAlert != null) regularAlert.SetActive(true);
        }
    }

    private void HideProximityAlerts()
    {
        if (regularAlert != null) regularAlert.SetActive(false);
        if (keyAlert != null) keyAlert.SetActive(false);
    }

    private void HideAllAlerts()
    {
        HideProximityAlerts();
        if (doorAlert != null) doorAlert.SetActive(false);
    }
}