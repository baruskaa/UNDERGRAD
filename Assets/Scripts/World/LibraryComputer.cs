using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Collider2D))]
public class LibraryComputer : MonoBehaviour
{
    [Header("Alert Settings")]
    [Tooltip("The alert icon/GameObject shown only when the player is within interaction range.")]
    public GameObject interactionAlert;

    [Header("Student ID Configuration")]
    [Tooltip("The exact Student ID required to unlock the library.")]
    public string correctStudentID = "2023-10492";
    [Tooltip("Maximum wrong attempts before librarian spawns and attacks.")]
    public int maxAttempts = 3;

    [Header("UI References")]
    [Tooltip("The Computer UI Canvas/Panel GameObject that starts disabled.")]
    public GameObject computerUI;
    [Tooltip("The TextMeshPro Input Field where the player types their Student ID.")]
    public TMP_InputField studentIdInputField;
    [Tooltip("Optional text element to display remaining attempts or feedback.")]
    public TMP_Text feedbackText;
    [Tooltip("Optional Submit button (pressing Enter also submits automatically).")]
    public Button submitButton;

    [Header("Librarian Reference")]
    [Tooltip("Drag your Librarian enemy GameObject (with LibrarianAI script) here. Starts disabled until 3 wrong attempts.")]
    public LibrarianAI librarianEnemy;

    [Header("Success Outcomes (Correct ID)")]
    [Tooltip("GameObjects to ENABLE when the player logs in successfully.")]
    public List<GameObject> objectsToEnableOnSuccess;
    [Tooltip("GameObjects to DISABLE when the player logs in successfully.")]
    public List<GameObject> objectsToDisableOnSuccess;

    [Header("Failure Outcomes (3 Wrong Attempts)")]
    [Tooltip("GameObjects to ENABLE when the player fails to log in 3 times.")]
    public List<GameObject> objectsToEnableOnFail;
    [Tooltip("GameObjects to DISABLE when the player fails to log in 3 times.")]
    public List<GameObject> objectsToDisableOnFail;

    [Header("Audio Feedback (Optional)")]
    public AudioSource audioSource;
    public AudioClip keyPressOrOpenSound;
    public AudioClip wrongIdSound;
    public AudioClip accessGrantedSound;

    private bool isPlayerInRange = false;
    private bool isComputerOpen = false;
    private bool isUnlocked = false;
    private bool isLockedOut = false;
    private int currentWrongAttempts = 0;

    private void Awake()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null && !col.isTrigger)
        {
            col.isTrigger = true;
        }
    }

    private void Start()
    {
        if (interactionAlert != null)
        {
            interactionAlert.SetActive(false);
        }

        if (computerUI != null)
        {
            computerUI.SetActive(false);
        }

        // Ensure Librarian starts disabled until 3 failed attempts occur
        if (librarianEnemy != null)
        {
            librarianEnemy.gameObject.SetActive(false);
        }

        if (submitButton != null)
        {
            submitButton.onClick.AddListener(CheckStudentID);
        }

        if (studentIdInputField != null)
        {
            studentIdInputField.onSubmit.AddListener(delegate { CheckStudentID(); });
        }
    }

    private void Update()
    {
        if (isUnlocked || isLockedOut || !isPlayerInRange) return;

        if (!isComputerOpen && Input.GetKeyDown(KeyCode.Q))
        {
            OpenComputer();
        }
        else if (isComputerOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseComputer();
        }
    }

    public void OpenComputer()
    {
        isComputerOpen = true;

        if (interactionAlert != null)
        {
            interactionAlert.SetActive(false);
        }

        if (computerUI != null)
        {
            computerUI.SetActive(true);
        }

        if (audioSource != null && keyPressOrOpenSound != null)
        {
            audioSource.PlayOneShot(keyPressOrOpenSound);
        }

        SetPlayerMovementEnabled(false);

        if (studentIdInputField != null)
        {
            studentIdInputField.text = "";
            studentIdInputField.ActivateInputField();
            studentIdInputField.Select();
        }

        UpdateFeedbackText($"ENTER STUDENT ID ({maxAttempts - currentWrongAttempts} ATTEMPTS LEFT)", Color.white);
    }

    public void CloseComputer()
    {
        isComputerOpen = false;

        if (computerUI != null)
        {
            computerUI.SetActive(false);
        }

        if (interactionAlert != null)
        {
            bool canStillInteract = isPlayerInRange && !isUnlocked && !isLockedOut;
            interactionAlert.SetActive(canStillInteract);
        }

        SetPlayerMovementEnabled(true);
    }

    public void CheckStudentID()
    {
        if (!isComputerOpen || isUnlocked || isLockedOut || studentIdInputField == null) return;

        string enteredID = studentIdInputField.text.Trim();

        if (string.IsNullOrEmpty(enteredID)) return;

        // --- CORRECT STUDENT ID ---
        if (enteredID.Equals(correctStudentID, System.StringComparison.OrdinalIgnoreCase))
        {
            isUnlocked = true;

            if (audioSource != null && accessGrantedSound != null)
            {
                audioSource.PlayOneShot(accessGrantedSound);
            }

            if (librarianEnemy != null)
            {
                librarianEnemy.DisableLibrarian();
            }

            SetObjectsActive(objectsToEnableOnSuccess, true);
            SetObjectsActive(objectsToDisableOnSuccess, false);

            CloseComputer();
            Debug.Log("<color=green>[LibraryComputer] Access Granted! Library unlocked safely.</color>");
        }
        // --- WRONG STUDENT ID ---
        else
        {
            currentWrongAttempts++;

            if (audioSource != null && wrongIdSound != null)
            {
                audioSource.PlayOneShot(wrongIdSound);
            }

            int remaining = maxAttempts - currentWrongAttempts;

            if (remaining <= 0)
            {
                // 3 Wrong Tries -> Lock out, close UI, enable Librarian and trigger attack
                isLockedOut = true;
                CloseComputer();

                SetObjectsActive(objectsToEnableOnFail, true);
                SetObjectsActive(objectsToDisableOnFail, false);

                Debug.Log("<color=red>[LibraryComputer] Security Breach! Enabling Librarian to attack.</color>");

                if (librarianEnemy != null)
                {
                    librarianEnemy.TriggerSecurityAlertChase();
                }
            }
            else
            {
                UpdateFeedbackText($"INVALID ID. {remaining} {(remaining == 1 ? "ATTEMPT" : "ATTEMPTS")} REMAINING.", Color.red);

                studentIdInputField.text = "";
                studentIdInputField.ActivateInputField();
                studentIdInputField.Select();
            }
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

            if (!isComputerOpen && !isUnlocked && !isLockedOut && interactionAlert != null)
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

            if (isComputerOpen)
            {
                CloseComputer();
            }
        }
    }
}