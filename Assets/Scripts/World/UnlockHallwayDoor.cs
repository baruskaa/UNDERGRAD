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

    [Header("FADE SETTINGS")]
    public bool useFade = true;

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
    [Tooltip("Optional list of GameObjects to enable once after the locked dialogue plays.")]
    public List<GameObject> objectsToEnableAfterDialogue = new List<GameObject>();
    [Tooltip("Optional list of GameObjects to disable once after the locked dialogue plays.")]
    public List<GameObject> objectsToDisableAfterDialogue = new List<GameObject>();

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
    private bool isWaitingForDialogue = false;
    private bool hasTriggeredPostDialogueToggles = false;
    private int ignoreInputUntilFrame = -1;
    private GameObject playerRef;

    public bool IsUnlocked => isUnlocked;

    private void Awake()
    {
        if (saveableObject == null)
            saveableObject = GetComponent<SaveableWorldObject>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        if (saveableObject != null && saveableObject.IsStateChanged)
        {
            SetUnlockedState(true);
        }

        HideAllAlerts();

        // Safely set inspect UI inactive on start without calling DisableInspectUI
        if (doorInspectUI != null)
        {
            doorInspectUI.SetActive(false);
        }
    }

    private void OnDisable()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueEnded -= OnLockedDialogueEnded;
        }

        isInspecting = false;
        isTransitioning = false;
        isWaitingForDialogue = false;
        SetPlayerPauseState(false);
    }

    private void Update()
    {
        if (isUnlocked || !isPlayerInRange || isTransitioning || isWaitingForDialogue) return;
        if (Time.frameCount <= ignoreInputUntilFrame) return;

        if (DialogueManager.Instance != null && DialogueManager.Instance.isDialogueActive) return;

        if (Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame)
        {
            InteractWithDoor();
        }
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

    private async Task OpenInspectionWithFade()
    {
        if (isTransitioning) return;
        isTransitioning = true;
        HideProximityAlerts();

        if (useFade && ScreenFader.Instance != null)
        {
            await ScreenFader.Instance.FadeOut();
        }

        EnableInspectUI();

        bool hasKey = CheckHasKey();
        if (hasKey && doorAlert != null)
        {
            doorAlert.SetActive(true);
        }

        if (useFade && ScreenFader.Instance != null)
        {
            await ScreenFader.Instance.FadeIn();
        }

        isTransitioning = false;

        if (!hasKey)
        {
            TriggerLockedDialogue();
        }
    }

    private async Task CloseInspectionWithFade(bool applyPostDialogueToggles = false)
    {
        if (isTransitioning) return;
        isTransitioning = true;

        if (doorAlert != null)
            doorAlert.SetActive(false);

        if (useFade && ScreenFader.Instance != null)
        {
            await ScreenFader.Instance.FadeOut();
        }

        DisableInspectUI();

        // First pass: apply enable/disable toggles while screen is faded out
        if (applyPostDialogueToggles && !hasTriggeredPostDialogueToggles)
        {
            EnablePostDialogueObjects();
            DisablePostDialogueObjects();
        }

        if (useFade && ScreenFader.Instance != null)
        {
            await ScreenFader.Instance.FadeIn();
        }

        // Second pass (double call): re-apply enable list after fade completes
        if (applyPostDialogueToggles && !hasTriggeredPostDialogueToggles)
        {
            EnablePostDialogueObjects();
            hasTriggeredPostDialogueToggles = true;
        }

        if (isPlayerInRange && !isUnlocked)
            UpdateAlertState();

        ignoreInputUntilFrame = Time.frameCount + 1;
        isTransitioning = false;
    }

    private async Task PerformUnlockAndTransition()
    {
        if (isTransitioning) return;
        isTransitioning = true;
        HideAllAlerts();

        if (useFade && ScreenFader.Instance != null)
        {
            await ScreenFader.Instance.FadeOut();
        }

        DisableInspectUI();

        if (audioSource != null && unlockSound != null)
        {
            audioSource.PlayOneShot(unlockSound);
        }

        GameObject playerToMove = playerRef != null ? playerRef : GameObject.FindGameObjectWithTag("Player");

        if (movePlayerOnUnlock && playerToMove != null && transitionPoint != null)
        {
            playerToMove.transform.position = transitionPoint.position;

            CameraFollow cam = FindAnyObjectByType<CameraFollow>();
            if (cam != null)
                cam.SetBounds(newMinBounds, newMaxBounds);
        }

        RemoveKeyFromInventory();
        UnlockDoor();

        if (useFade && ScreenFader.Instance != null)
        {
            await ScreenFader.Instance.FadeIn();
        }

        isTransitioning = false;
    }

    private async Task PerformUnlockAndTransitionNextScene()
    {
        if (isTransitioning) return;
        isTransitioning = true;
        HideAllAlerts();

        if (useFade && ScreenFader.Instance != null)
        {
            await ScreenFader.Instance.FadeOut();
        }

        DisableInspectUI();

        if (audioSource != null && unlockSound != null)
        {
            audioSource.PlayOneShot(unlockSound);
        }

        RemoveKeyFromInventory();
        UnlockDoor();

        AsyncOperation sceneLoad = SceneManager.LoadSceneAsync(targetSceneBuildIndex);
        while (!sceneLoad.isDone)
        {
            await Task.Yield();
        }

        if (useFade && ScreenFader.Instance != null)
        {
            await ScreenFader.Instance.FadeIn();
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
        if (DialogueManager.Instance != null && lockedDialogue != null && !DialogueManager.Instance.isDialogueActive)
        {
            HideProximityAlerts();
            isWaitingForDialogue = true;

            DialogueManager.Instance.OnDialogueEnded -= OnLockedDialogueEnded;
            DialogueManager.Instance.OnDialogueEnded += OnLockedDialogueEnded;
            DialogueManager.Instance.StartDialogue(lockedDialogue, null);
        }
        else
        {
            _ = CloseInspectionWithFade(true);
        }
    }

    private void OnLockedDialogueEnded()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueEnded -= OnLockedDialogueEnded;
        }

        isWaitingForDialogue = false;
        ignoreInputUntilFrame = Time.frameCount + 1;

        // Immediate call on dialogue end before fade starts
        if (!hasTriggeredPostDialogueToggles)
        {
            EnablePostDialogueObjects();
        }

        _ = CloseInspectionWithFade(true);
    }

    private void EnablePostDialogueObjects()
    {
        if (objectsToEnableAfterDialogue == null) return;

        foreach (GameObject obj in objectsToEnableAfterDialogue)
        {
            if (obj == null) continue;

            // Ensure any inactive parent in the hierarchy is also enabled so the object actually becomes visible
            Transform parent = obj.transform.parent;
            while (parent != null)
            {
                if (!parent.gameObject.activeSelf)
                {
                    parent.gameObject.SetActive(true);
                }
                parent = parent.parent;
            }

            obj.SetActive(true);
        }
    }

    private void DisablePostDialogueObjects()
    {
        if (objectsToDisableAfterDialogue == null) return;

        foreach (GameObject obj in objectsToDisableAfterDialogue)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }
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
            if (!isTransitioning)
            {
                DisableInspectUI();
            }
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