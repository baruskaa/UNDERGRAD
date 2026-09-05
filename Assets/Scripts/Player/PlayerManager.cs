using System;
using UnityEngine;
public class PlayerManager : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] public int maxHealth = 20;
    [SerializeField] public int currentHealth;
    [Header("Hunger")]
    [SerializeField] public int maxHunger = 20;
    [SerializeField] public int currentHunger;

    [Header("Bars")]
    public HealthBar[] healthBars;
    public Hungerbar[] hungerbars;

    void Start()
    {
        currentHealth = maxHealth;
        currentHunger = maxHunger;

        foreach (HealthBar bar in healthBars)
        {
            bar.SetmaxHealth(maxHealth);
            bar.SetHealth(currentHealth);
        }

        foreach (Hungerbar bar in hungerbars)
        {
            bar.SetMaxHunger(maxHunger);
            bar.SetHunger(currentHunger);
        }
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

        foreach (HealthBar bar in healthBars)
        {
            bar.SetHealth(currentHealth);
        }
    }
    void TakeHunger(int hunger)
    {
        currentHunger -= hunger;

        foreach (Hungerbar bar in hungerbars)
        {
            bar.SetHunger(currentHunger);
        }
    }
}