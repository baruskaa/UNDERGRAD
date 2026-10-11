using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using Inventory.Model;

public enum DoorType
{
    LockedDoor,
    BarricadedDoor,
    ChainedGate
}

public class UnlockHallwayDoor : MonoBehaviour
{
    [Header("SAVE SYSTEM")]
    public SaveableWorldObject saveableObject;

    [Header("DOOR TYPE & INSPECTION UI")]
    public DoorType doorType = DoorType.LockedDoor;
    public GameObject lockedDoorUI;
    public GameObject barricadedDoorUI;
    public GameObject chainedGateUI;

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
        DisableInspectUI();
    }

    private void OnDisable()
    {
        isInspecting = false;
        isTransitioning = false;
        if (fader != null) fader.SetActive(false);
    }

    private void Update()
    {
        if (isUnlocked || !isPlayerInRange || isTransitioning) return;

        // Ignore Q keypresses while dialogue is actively playing on screen
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

    private bool CheckHasKey()
    {
        InventorySO inv = playerInventory;
        if (inv == null && SaveSystem.Instance != null) inv = SaveSystem.Instance.playerInventory;

        if (inv == null || requiredKey == null) return false;

        return inv.HasItem(requiredKey) || inv.HasItemByName(requiredKey.name);
    }

    private void InteractWithDoor()
    {
        bool hasKey = CheckHasKey();

        if (isInspecting)
        {
            if (hasKey)
            {
                // Player has key -> Unlock & Teleport
                _ = PerformUnlockAndTransition();
            }
            else
            {
                // Player doesn't have key -> Close inspect UI with fade
                _ = CloseInspectionWithFade();
            }
        }
        else
        {
            // First press -> Open inspect UI with fade
            _ = OpenInspectionWithFade();
        }
    }

    private GameObject GetCurrentInspectUI()
    {
        return doorType switch
        {
            DoorType.LockedDoor => lockedDoorUI,
            DoorType.BarricadedDoor => barricadedDoorUI,
            DoorType.ChainedGate => chainedGateUI,
            _ => null
        };
    }

    private void EnableInspectUI()
    {
        GameObject activeUI = GetCurrentInspectUI();
        if (activeUI != null)
        {
            activeUI.SetActive(true);
        }

        isInspecting = true;
    }

    private void DisableInspectUI()
    {
        if (lockedDoorUI != null) lockedDoorUI.SetActive(false);
        if (barricadedDoorUI != null) barricadedDoorUI.SetActive(false);
        if (chainedGateUI != null) chainedGateUI.SetActive(false);

        isInspecting = false;
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
            // Transparency 0 -> 1, screen becomes black
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

        // 1. FadeIn: screen becomes black
        await PlayFadeToBlack();

        // 2. Enable the door inspection UI while the screen is black
        EnableInspectUI();

        // 3. Enable the door alert at the same time as the inspection UI
        if (CheckHasKey() && doorAlert != null)
        {
            doorAlert.SetActive(true);
        }

        // 4. Optional hold on full black
        if (fadeHoldDuration > 0f)
            await Task.Delay(Mathf.RoundToInt(fadeHoldDuration * 1000f));

        // 5. FadeOut: screen becomes visible again
        await PlayFadeFromBlack();

        // 6. Start dialogue if no key
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

        // 1. FadeIn: screen becomes black
        await PlayFadeToBlack();

        // 2. Disable the inspection UI while the screen is black
        DisableInspectUI();

        // 3. Optional hold on full black
        if (fadeHoldDuration > 0f)
            await Task.Delay(Mathf.RoundToInt(fadeHoldDuration * 1000f));

        // 4. FadeOut: return to normal gameplay view
        await PlayFadeFromBlack();

        if (isPlayerInRange)
            UpdateAlertState();

        isTransitioning = false;
    }

    private async Task PerformUnlockAndTransition()
    {
        isTransitioning = true;
        HideAllAlerts();

        // 1. FadeIn: screen becomes black
        await PlayFadeToBlack();

        // 2. Disable inspect UI
        DisableInspectUI();

        // 3. Play the unlock sound at the same time the 2-second black-screen hold begins
        if (audioSource != null && unlockSound != null)
        {
            audioSource.PlayOneShot(unlockSound);
        }

        // 4. Teleport player and adjust camera bounds while the screen is black
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

        

        // 6. Hold the screen black for exactly 2 seconds while the sound plays
        await Task.Delay(Mathf.RoundToInt(2f * 1000f));


        // 7. FadeOut: reveal the new area
        await PlayFadeFromBlack();

        isTransitioning = false;

        // 5. Save unlocked state
        UnlockDoor();

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