using UnityEngine;

public class ObjectOff : MonoBehaviour
{
    [Header("OBJECT TO TURN OFF")]
    public GameObject targetObject;

    [Header("Trigger Settings")]
    public string requiredTag = "Player";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag)) return;

        TurnOffTarget();
    }

    /// <summary>
    /// Parameterless method you can select directly in a UI Button's OnClick() event.
    /// </summary>
    public void TurnOffTarget()
    {
        if (targetObject != null)
        {
            targetObject.SetActive(false);
        }
    }

    /// <summary>
    /// Optional button method if you want to pass a specific GameObject to disable directly in OnClick().
    /// </summary>
    public void TurnOffObject(GameObject objectToDisable)
    {
        if (objectToDisable != null)
        {
            objectToDisable.SetActive(false);
        }
    }
}