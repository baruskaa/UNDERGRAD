using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [SerializeField] private ItemSO itemData;

    public ItemSO GetItem() => itemData;

    public void OnPickedUp()
    {
        Destroy(gameObject);
    }
}