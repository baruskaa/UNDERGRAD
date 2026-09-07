using System;
using UnityEngine;

public enum ItemType
{
    Food,
    Medkit
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

    [Header("Item Category")]
    public ItemType itemType;

    [Header("Consumable Values")]
    [SerializeField] private int restoreAmount = 5;

    public void Eat(PlayerManager player)
    {
        if (player != null)
        {
            player.RestoreHunger(restoreAmount);
        }
    }

    public void Heal(PlayerManager player)
    {
        if (player != null)
        {
            player.RestoreHealth(restoreAmount);
        }
    }
}