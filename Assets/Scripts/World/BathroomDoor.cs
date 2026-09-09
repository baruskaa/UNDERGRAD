using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BathroomDoor : MonoBehaviour
{
    [Header("Door GameObjects")]
    [SerializeField] private GameObject closedDoor;
    [SerializeField] private GameObject openedDoor;

    [Header("Door State")]
    [SerializeField] private bool isClosed = true;
    [SerializeField] private bool isOpened = false;

    [Header("Post-Open Objects")]
    [SerializeField] private List<GameObject> objectsToEnable = new List<GameObject>();

    [Header("Player Feedback")]
    [SerializeField] private GameObject playerAlert;

    private bool isPlayerInRange = false;

    private void Start()
    {
        // Set initial visual states
        UpdateDoorStateVisuals();
    }

    private void Update()
    {
        // If the door is already opened, ignore interaction
        if (isOpened) return;

        // Press 'E' to open the door (or change to Keyboard.current.qKey for 'Q')
        if (isPlayerInRange && Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame)
        {
            OpenDoor();
        }
    }

    private void OpenDoor()
    {
        isClosed = false;
        isOpened = true;

        // Hide alert icon
        if (playerAlert != null)
        {
            playerAlert.SetActive(false);
        }

        // Toggle door objects
        UpdateDoorStateVisuals();

        // Enable secondary objects
        if (objectsToEnable != null)
        {
            foreach (GameObject obj in objectsToEnable)
            {
                if (obj != null)
                {
                    obj.SetActive(true);
                }
            }
        }
    }

    private void UpdateDoorStateVisuals()
    {
        if (closedDoor != null) closedDoor.SetActive(isClosed);
        if (openedDoor != null) openedDoor.SetActive(isOpened);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Only show alert if door is still closed
        if (isClosed && collision.CompareTag("Player"))
        {
            isPlayerInRange = true;

            if (playerAlert != null)
            {
                playerAlert.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = false;

            if (playerAlert != null)
            {
                playerAlert.SetActive(false);
            }
        }
    }
}