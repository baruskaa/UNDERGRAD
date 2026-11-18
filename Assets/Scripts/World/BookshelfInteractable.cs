using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Collider2D))]
public class BookshelfInteractable : MonoBehaviour
{
    public GameObject turnOffSelf;

    [Header("Alert Settings")]
    [Tooltip("The alert icon/GameObject positioned above the player's head.")]
    public GameObject playerHeadAlert;

    [Header("UI Panels")]
    [Tooltip("The main Bookshelf UI GameObject.")]
    public GameObject bookshelfUI;
    [Tooltip("The Book UI GameObject (child of Bookshelf UI).")]
    public GameObject bookUI;

    [Header("UI Buttons")]
    [Tooltip("If checked, enables and binds the Open Book Button inside the Bookshelf UI.")]
    public bool useOpenBookButton = true;
    [Tooltip("Optional button inside Bookshelf UI that opens the Book child object.")]
    public Button openBookButton;

    [Tooltip("If checked, enables and binds the Reveal Objects Button inside the Book UI.")]
    public bool useRevealObjectsButton = true;
    [Tooltip("Optional button inside Book UI that closes the Bookshelf UI and reveals the target objects.")]
    public Button revealObjectsButton;

    [Header("Objects to Reveal")]
    [Tooltip("List of GameObjects that will become active when the Book button is pressed.")]
    public List<GameObject> objectsToReveal;

    [Header("Player Control")]
    [Tooltip("Freeze player movement while reading the bookshelf/book.")]
    public bool freezePlayerWhileOpen = true;

    private bool isPlayerInRange = false;
    private bool isBookshelfOpen = false;

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
        // Ensure alert and UI panels start hidden
        if (playerHeadAlert != null) playerHeadAlert.SetActive(false);
        if (bookshelfUI != null) bookshelfUI.SetActive(false);
        if (bookUI != null) bookUI.SetActive(false);

        // Configure optional Open Book button based on checkbox
        if (openBookButton != null)
        {
            openBookButton.gameObject.SetActive(useOpenBookButton);

            if (useOpenBookButton)
            {
                openBookButton.onClick.AddListener(OpenBook);
            }
        }

        // Configure optional Reveal Objects button based on checkbox
        if (revealObjectsButton != null)
        {
            revealObjectsButton.gameObject.SetActive(useRevealObjectsButton);

            if (useRevealObjectsButton)
            {
                revealObjectsButton.onClick.AddListener(OnBookActionButtonPressed);
            }
        }
    }

    private void Update()
    {
        if (!isPlayerInRange) return;

        // Toggle Bookshelf UI when pressing Q
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (!isBookshelfOpen)
            {
                OpenBookshelf();
            }
            else
            {
                CloseBookshelf();
            }
        }
    }

    public void OpenBookshelf()
    {
        isBookshelfOpen = true;

        // Hide alert prompt while reading
        if (playerHeadAlert != null) playerHeadAlert.SetActive(false);

        // Show Bookshelf UI, ensure Book child starts closed
        if (bookshelfUI != null) bookshelfUI.SetActive(true);
        if (bookUI != null) bookUI.SetActive(false);

        // Ensure Open Book button visibility matches checkbox
        if (openBookButton != null)
        {
            openBookButton.gameObject.SetActive(useOpenBookButton);
        }

        if (freezePlayerWhileOpen)
        {
            SetPlayerMovementEnabled(false);
        }
    }

    public void CloseBookshelf()
    {
        isBookshelfOpen = false;

        if (bookUI != null) bookUI.SetActive(false);
        if (bookshelfUI != null) bookshelfUI.SetActive(false);

        // Re-show head alert if player is still standing in the trigger area
        if (isPlayerInRange && playerHeadAlert != null)
        {
            playerHeadAlert.SetActive(true);
        }

        if (freezePlayerWhileOpen)
        {
            SetPlayerMovementEnabled(true);
        }
    }

    /// <summary>
    /// Called by the button inside Bookshelf UI to open the child Book GameObject.
    /// </summary>
    public void OpenBook()
    {
        if (!useOpenBookButton) return;

        if (bookUI != null)
        {
            bookUI.SetActive(true);
        }

        // Ensure Reveal Objects button visibility matches checkbox whenever the book opens
        if (revealObjectsButton != null)
        {
            revealObjectsButton.gameObject.SetActive(useRevealObjectsButton);
        }
    }

    /// <summary>
    /// Called by the button inside Book UI to hide the Bookshelf UI and show the list of GameObjects.
    /// </summary>
    public void OnBookActionButtonPressed()
    {
        if (!useRevealObjectsButton) return;

        // 1. Hide Bookshelf UI (and its child Book UI)
        CloseBookshelf();

        // 2. Activate all GameObjects in the reveal list
        if (objectsToReveal != null)
        {
            foreach (GameObject obj in objectsToReveal)
            {
                if (obj != null)
                {
                    obj.SetActive(true);
                }
            }

            turnOffSelf.SetActive(false);
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

            if (!isBookshelfOpen && playerHeadAlert != null)
            {
                playerHeadAlert.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = false;

            if (playerHeadAlert != null)
            {
                playerHeadAlert.SetActive(false);
            }

            if (isBookshelfOpen)
            {
                CloseBookshelf();
            }
        }
    }
}