using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [SerializeField] private ItemSO itemData;

    [Header("ALERT SETTINGS")]
    [Tooltip("Optional: Local alert icon attached to this item object.")]
    [SerializeField] private GameObject itemAlertIcon;

    public ItemSO GetItem() => itemData;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Option A: Toggle an alert icon floating above this specific item
            if (itemAlertIcon != null)
            {
                itemAlertIcon.SetActive(true);
            }

            // Option B: If your Player has an Alert GameObject, toggle it directly via PlayerManager
            PlayerManager player = collision.GetComponent<PlayerManager>();
            if (player != null && itemAlertIcon != null)
            {
                itemAlertIcon.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            HideAlert(collision.gameObject);
        }
    }

    public void OnPickedUp()
    {
        // Make sure the alert is hidden before destroying the item
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            HideAlert(playerObj);
        }

        Destroy(gameObject);
    }

    private void HideAlert(GameObject playerObj)
    {
        if (itemAlertIcon != null)
        {
            itemAlertIcon.SetActive(false);
        }

        if (playerObj != null)
        {
            PlayerManager player = playerObj.GetComponent<PlayerManager>();
            if (player != null && itemAlertIcon != null)
            {
                itemAlertIcon.SetActive(false);
            }
        }
    }
}