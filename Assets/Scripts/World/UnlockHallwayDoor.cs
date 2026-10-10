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
    public string fadeOutAnimation = "fadeout"; // State/Clip name that fades screen to black
    public string fadeInAnimation = "fadein";   // State/Clip name that fades screen back in
    public float fadeDuration = 0.5f;           // Duration of the fade animation in seconds
    public float fadeHoldDuration = 0.2f;       // Hold duration on full black screen

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
        if (isInspecting)
        {
            Time.timeScale = 1f;
            isInspecting = false;
        }

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

        Time.timeScale = 1f; // Restore game speed
        isInspecting = false;
    }

    private async Task PlayFadeToBlack()
    {
        EnsureFadeReferences();

        // 1. Force black panel ON before running the animation
        if (fader != null) fader.SetActive(true);

        if (fadeAnimator != null)
        {
            // Play from normalized time 0 to guarantee fresh start
            fadeAnimator.Play(fadeOutAnimation, 0, 0f);
            await Task.Delay((int)(fadeDuration * 1000));
        }
    }

    private async Task PlayFadeFromBlack()
    {
        EnsureFadeReferences();

        if (fadeAnimator != null)
        {
            // Play from normalized time 0 to guarantee fresh start
            fadeAnimator.Play(fadeInAnimation, 0, 0f);
            await Task.Delay((int)(fadeDuration * 1000));
        }

        // 2. Turn OFF black panel ONLY after fade-in finishes
        if (fader != null) fader.SetActive(false);
    }

    private async Task OpenInspectionWithFade()
    {
        isTransitioning = true;
        HideProximityAlerts();

        // 1. Fade Out to Black
        await PlayFadeToBlack();

        // 2. Enable Inspect UI while screen is covered in black
        EnableInspectUI();

        // 3. Hold black screen briefly
        if (fadeHoldDuration > 0f)
        {
            await Task.Delay((int)(fadeHoldDuration * 1000));
        }

        // 4. Fade back in to reveal Inspect UI
        await PlayFadeFromBlack();

        // 5. Pause game time AFTER inspect UI is fully visible
        Time.timeScale = 0f;

        bool hasKey = CheckHasKey();

        if (hasKey)
        {
            if (doorAlert != null)
            {
                doorAlert.SetActive(true);
            }
        }
        else
        {
            TriggerLockedDialogue();
        }

        isTransitioning = false;
    }

    private async Task CloseInspectionWithFade()
    {
        isTransitioning = true;

        // Restore game speed before fading
        Time.timeScale = 1f;
        if (doorAlert != null) doorAlert.SetActive(false);

        // 1. Fade Out to Black
        await PlayFadeToBlack();

        // 2. Disable UI while screen is black
        DisableInspectUI();

        // 3. Hold black screen
        if (fadeHoldDuration > 0f)
        {
            await Task.Delay((int)(fadeHoldDuration * 1000));
        }

        // 4. Fade back in
        await PlayFadeFromBlack();

        if (isPlayerInRange)
        {
            UpdateAlertState();
        }

        isTransitioning = false;
    }

    private async Task PerformUnlockAndTransition()
    {
        isTransitioning = true;

        // Restore game speed before fading
        Time.timeScale = 1f;
        HideAllAlerts();

        // 1. Fade Out to Black
        await PlayFadeToBlack();

        // 2. Disable inspect UI
        DisableInspectUI();

        // 3. Play SFX
        if (audioSource != null && unlockSound != null)
        {
            audioSource.PlayOneShot(unlockSound);
        }

        // 4. Teleport Player & Adjust Camera
        GameObject playerToMove = playerRef;
        if (playerToMove == null) playerToMove = GameObject.FindGameObjectWithTag("Player");

        if (movePlayerOnUnlock && playerToMove != null && transitionPoint != null)
        {
            playerToMove.transform.position = transitionPoint.position;

            CameraFollow cam = FindAnyObjectByType<CameraFollow>();
            if (cam != null)
            {
                cam.SetBounds(newMinBounds, newMaxBounds);
            }
        }

        // 5. Save Unlocked State
        UnlockDoor();

        if (fadeHoldDuration > 0f)
        {
            await Task.Delay((int)(fadeHoldDuration * 1000));
        }

        // 6. Fade back in
        await PlayFadeFromBlack();

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