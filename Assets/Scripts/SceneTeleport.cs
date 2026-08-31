using UnityEngine;

public class TeleportToPosition : MonoBehaviour
{
    [SerializeField] private Transform targetDestination;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.transform.position = targetDestination.position;
        }
        if (Camera.main != null)
        {
            Vector3 targetCamPos = targetDestination.position;
            targetCamPos.z = Camera.main.transform.position.z;
            Camera.main.transform.position = targetCamPos;
        }
    }
}