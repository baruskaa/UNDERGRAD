using UnityEngine;
using Inventory.Model;

[RequireComponent(typeof(SaveableWorldObject))]
public class ItemPickup : MonoBehaviour
{
    [SerializeField] private ItemSO item;
    private SaveableWorldObject saveableObject;

    private void Awake()
    {
        saveableObject = GetComponent<SaveableWorldObject>();
    }

    public ItemSO GetItem() => item;

    public void OnPickedUp()
    {
        if (saveableObject != null)
        {
            saveableObject.SetStateChanged(true);
        }
        gameObject.SetActive(false);
    }
}