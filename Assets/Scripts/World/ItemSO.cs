using System;
using UnityEngine;

public enum ItemType
{
    Food,
    Medkit,
    NonConsumable
}

[CreateAssetMenu]
public class ItemSO : ScriptableObject
{
    public int ID => GetInstanceID();

    [field: SerializeField]
    public string Name { get; set; }

    [field: SerializeField]
    [field: TextArea]
    public string Description { get; set; }

    [field: SerializeField]
    public Sprite ItemImage { get; set; }

    
    public bool IsConsumable => itemType != ItemType.NonConsumable;

    [Header("Item Category")]
    public ItemType itemType;

    [Header("Consumable Values")]
    [SerializeField] private int restoreAmount = 5;

    public void Eat(PlayerManager player)
    {
        // Guard clause: ignore if not Food
        if (itemType != ItemType.Food) return;

        if (player != null)
        {
            player.RestoreHunger(restoreAmount);
        }
    }

    public void Heal(PlayerManager player)
    {
        // Guard clause: ignore if not Medkit
        if (itemType != ItemType.Medkit) return;

        if (player != null)
        {
            player.RestoreHealth(restoreAmount);
        }
    }

    // Unified action helper to handle usage from inventory UI
    public void Use(PlayerManager player)
    {
        switch (itemType)
        {
            case ItemType.Food:
                Eat(player);
                break;
            case ItemType.Medkit:
                Heal(player);
                break;
            case ItemType.NonConsumable:
                Debug.Log($"{Name} cannot be consumed.");
                break;
        }
    }
}