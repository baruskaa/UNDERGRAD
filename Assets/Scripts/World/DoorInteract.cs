using UnityEngine;

public class DoorInteract : MonoBehaviour
{
    [Header("Teleport Target")]
    public Transform targetDestination;

    [Header("UI Prompt")]
    public GameObject interactPrompt;

    [Header("Camera Bounds Update")]
    public bool updateCameraBounds = true;
    public Vector2 roomMinBounds;
    public Vector2 roomMaxBounds;

    private bool isPlayerInRange = false;
    private Transform playerTransform;

    private void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.Q))
        {
            if (targetDestination != null && playerTransform != null)
            {
                // Teleport Player
                playerTransform.position = targetDestination.position;

                // Update Camera Bounds
                if (updateCameraBounds)
                {
                    CameraFollow cam = Camera.main.GetComponent<CameraFollow>();
                    if (cam != null)
                    {
                        cam.SetBounds(roomMinBounds, roomMaxBounds);
                    }
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = true;
            playerTransform = collision.transform;

            if (interactPrompt != null)
                interactPrompt.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = false;

            if (interactPrompt != null)
                interactPrompt.SetActive(false);
        }
    }
}