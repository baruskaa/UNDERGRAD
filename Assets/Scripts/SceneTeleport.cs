using UnityEngine;

public class TeleportToPosition : MonoBehaviour
{
    [SerializeField] private Transform targetSpawnPoint;
    private bool isPlayerInTrigger = false;
    private GameObject playerRef;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInTrigger = true;
            playerRef = other.gameObject;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInTrigger = false;
            playerRef = null;
        }
    }

    private void Update()
    {

        if (isPlayerInTrigger && Input.GetKeyDown(KeyCode.Q))
        {
            TeleportPlayer();
        }
    }

    private void TeleportPlayer()
    {
        if (playerRef != null && targetSpawnPoint != null)
        {
            playerRef.transform.position = targetSpawnPoint.position;
        }
    }
}