using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Collider2D))]
public class SecretaryPasscode : MonoBehaviour
{
    [Header("Alert Settings")]
    [Tooltip("The alert icon/GameObject shown only when the player is within interaction range.")]
    public GameObject interactionAlert;

    [Header("Passcode Configuration")]
    [Tooltip("The exact passcode required to unlock.")]
    public string correctPasscode = "1234";

    [Header("UI References")]
    [Tooltip("The Passcode UI Canvas/Panel GameObject that starts disabled.")]
    public GameObject passcodeUI;
    [Tooltip("The TextMeshPro Input Field where the player types the passcode.")]
    public TMP_InputField passcodeInputField;
    [Tooltip("Optional text element to display feedback.")]
    public TMP_Text feedbackText;
    [Tooltip("Optional Submit button (pressing Enter also submits automatically).")]
    public Button submitButton;

    [Header("BLACK FADE ANIMATOR")]
    public Animator fadeAnimator;
    public GameObject fader;                    // Drag the 'Black' GameObject here
    public string fadeOutAnimation = "Fadeout"; // State/Clip name that fades screen to black
    public string fadeInAnimation = "FadeIn";   // State/Clip name that fades screen back in
    public float fadeDuration = 1f;             // Duration of the fade animation in seconds
    public float fadeHoldDuration = 0.2f;       // Default hold duration on full black screen

    [Header("Success Outcomes (Correct Passcode)")]
    [Tooltip("GameObjects to ENABLE when the player enters the correct passcode.")]
    public List<GameObject> objectsToEnableOnSuccess;
    [Tooltip("GameObjects to DISABLE when the player enters the correct passcode.")]
    public List<GameObject> objectsToDisableOnSuccess;

    private bool isPlayerInRange = false;
    private bool isUIOpen = false;
    private bool isUnlocked = false;
    private bool isTransitioning = false;

    private static readonly int FadeInStateHash = Animator.StringToHash("FadeIn");
    private static readonly int FadeOutStateHash = Animator.StringToHash("FadeOut");

    private void Awake()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null && !col.isTrigger)
        {
            col.isTrigger = true;
        }

        EnsureFadeReferences();
    }

    private void Start()
    {
        EnsureFadeReferences();
        if (fader != null)
        {
            fader.SetActive(false);
        }

        if (interactionAlert != null)
        {
            interactionAlert.SetActive(false);
        }

        if (passcodeUI != null)
        {
            passcodeUI.SetActive(false);
        }

        if (submitButton != null)
        {
            submitButton.onClick.AddListener(CheckPasscode);
        }

        if (passcodeInputField != null)
        {
            passcodeInputField.onSubmit.AddListener(delegate { CheckPasscode(); });
        }
    }

    private void OnDisable()
    {
        isUIOpen = false;
        isTransitioning = false;
        SetPlayerMovementEnabled(true);
        if (fader != null) fader.SetActive(false);
    }

    private void Update()
    {
        if (isUnlocked || !isPlayerInRange || isTransitioning) return;

        // Toggle Passcode UI on/off with Q while within trigger range
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (!isUIOpen)
            {
                _ = OpenUIWithFade();
            }
            else
            {
                if (passcodeInputField != null && passcodeInputField.text.EndsWith("q", System.StringComparison.OrdinalIgnoreCase))
                {
                    passcodeInputField.text = passcodeInputField.text.Substring(0, passcodeInputField.text.Length - 1);
                }

                _ = CloseUIWithFade();
            }
        }
        else if (isUIOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            _ = CloseUIWithFade();
        }
    }

    private void EnsureFadeReferences()
    {
        if (fader == null && fadeAnimator != null) fader = fadeAnimator.gameObject;
        if (fadeAnimator == null && fader != null) fadeAnimator = fader.GetComponent<Animator>();
    }

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

    public async Task OpenUIWithFade()
    {
        if (isTransitioning) return;
        isTransitioning = true;

        if (interactionAlert != null)
        {
            interactionAlert.SetActive(false);
        }

        await PlayFadeToBlack();
        OpenUI();

        if (fadeHoldDuration > 0f)
            await Task.Delay(Mathf.RoundToInt(fadeHoldDuration * 1000f));

        await PlayFadeFromBlack();

        // Re-focus input field after fade completes
        if (passcodeInputField != null)
        {
            passcodeInputField.ActivateInputField();
            passcodeInputField.Select();
        }

        isTransitioning = false;
    }

    public async Task CloseUIWithFade(bool isSuccess = false)
    {
        if (isTransitioning) return;
        isTransitioning = true;

        await PlayFadeToBlack();
        CloseUI();

        if (isSuccess)
        {
            SetObjectsActive(objectsToEnableOnSuccess, true);
            SetObjectsActive(objectsToDisableOnSuccess, false);
        }

        if (fadeHoldDuration > 0f)
            await Task.Delay(Mathf.RoundToInt(fadeHoldDuration * 1000f));

        await PlayFadeFromBlack();

        isTransitioning = false;
    }

    public void OpenUI()
    {
        isUIOpen = true;

        if (interactionAlert != null)
        {
            interactionAlert.SetActive(false);
        }

        if (passcodeUI != null)
        {
            passcodeUI.SetActive(true);
        }

        SetPlayerMovementEnabled(false);

        if (passcodeInputField != null)
        {
            passcodeInputField.text = "";
            passcodeInputField.ActivateInputField();
            passcodeInputField.Select();
        }

        UpdateFeedbackText("ENTER PASSCODE", Color.white);
    }

    public void CloseUI()
    {
        isUIOpen = false;

        if (passcodeInputField != null)
        {
            passcodeInputField.DeactivateInputField();
        }

        if (passcodeUI != null)
        {
            passcodeUI.SetActive(false);
        }

        if (interactionAlert != null)
        {
            bool canStillInteract = isPlayerInRange && !isUnlocked;
            interactionAlert.SetActive(canStillInteract);
        }

        SetPlayerMovementEnabled(true);
    }

    public void CheckPasscode()
    {
        if (!isUIOpen || isUnlocked || isTransitioning || passcodeInputField == null) return;

        string enteredPasscode = passcodeInputField.text.Trim();

        if (string.IsNullOrEmpty(enteredPasscode)) return;

        // --- CORRECT PASSCODE ---
        if (enteredPasscode.Equals(correctPasscode, System.StringComparison.OrdinalIgnoreCase))
        {
            isUnlocked = true;

            _ = CloseUIWithFade(true);
            Debug.Log("<color=green>[SecretaryPasscode] Access Granted!</color>");
        }
        // --- WRONG PASSCODE (Unlimited Tries) ---
        else
        {
            UpdateFeedbackText("INVALID PASSCODE. TRY AGAIN.", Color.red);

            passcodeInputField.text = "";
            passcodeInputField.ActivateInputField();
            passcodeInputField.Select();
        }
    }

    private void SetObjectsActive(List<GameObject> objects, bool state)
    {
        if (objects == null) return;

        foreach (GameObject obj in objects)
        {
            if (obj != null)
            {
                obj.SetActive(state);
            }
        }
    }

    private void UpdateFeedbackText(string message, Color textColor)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
            feedbackText.color = textColor;
        }
    }

    private void SetPlayerMovementEnabled(bool isEnabled)
    {
        if (PlayerManager.Instance != null)
        {
            PlayerMovement pm = PlayerManager.Instance.GetComponent<PlayerMovement>();
            if (pm != null)
            {
                pm.enabled = isEnabled;
            }

            Rigidbody2D rb = PlayerManager.Instance.GetComponent<Rigidbody2D>();
            if (rb != null && !isEnabled)
            {
                rb.linearVelocity = Vector2.zero;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = true;

            if (!isUIOpen && !isUnlocked && interactionAlert != null)
            {
                interactionAlert.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = false;

            if (interactionAlert != null)
            {
                interactionAlert.SetActive(false);
            }

            if (isUIOpen && !isTransitioning)
            {
                _ = CloseUIWithFade();
            }
        }
    }
}