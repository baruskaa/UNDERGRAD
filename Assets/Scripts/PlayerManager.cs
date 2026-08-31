using System;
using UnityEngine;

/// <summary>
/// Manages core player stats: health and hunger.
/// Attach to the Player GameObject.
/// </summary>
public class PlayerManager : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] public int maxHealth = 20;
    [SerializeField] public int currentHealth;

    [Header("Hunger")]
    [SerializeField] public int maxHunger = 20;
    [SerializeField] public int currentHunger;

    [Header("Bars")]
    public HealthBar healthBar;
    public Hungerbar hungerbar;

    void Start()
    {
        currentHealth = maxHealth;
        healthBar.SetmaxHealth(maxHealth);

        currentHunger = maxHunger;
        hungerbar.SetMaxHunger(maxHunger);

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            TakeDamage(1);
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            TakeHunger(1);
        }
    }

    void TakeDamage(int damage)
    {
        currentHealth -= damage;
        healthBar.SetHealth(currentHealth);
    }

    void TakeHunger(int hunger)
    {
        currentHunger -= hunger;
        hungerbar.SetHunger(currentHunger);
    }
}