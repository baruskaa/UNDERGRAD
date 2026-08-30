using UnityEngine;

public class ObjectOn : MonoBehaviour
{
    [Header("OBJECT TO TURN ON")]
    public GameObject targetObject;

    public string requiredTag = "Player";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag)) return;

        if (targetObject != null)
        {
            targetObject.SetActive(true);
        }
    }
}
