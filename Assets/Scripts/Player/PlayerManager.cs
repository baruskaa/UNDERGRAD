using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] public int maxHealth = 20;
    [SerializeField] public int currentHealth;

    [Header("Hunger")]
    [SerializeField] public int maxHunger = 20;
    [SerializeField] public int currentHunger;

    [Header("Flash Feedback")]
    public SpriteRenderer spriteRenderer;
    public Material flashMaterial;
    public float flashDuration = 0.15f;

    [Header("Bars")]
    public HealthBar[] healthBars;
    public Hungerbar[] hungerbars;

    private PlayerMovement playerMovement;
    private Material originalMaterial;
    private Coroutine flashCoroutine;
    private float starvationTimer = 0f;
    private const float STARVATION_INTERVAL = 5f;

    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();

        // Cache original material and SpriteRenderer
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (spriteRenderer != null)
        {
            originalMaterial = spriteRenderer.material;
        }

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

        UpdateHungerPenalties();
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

        if (currentHunger <= 6 && currentHealth > 0)
        {
            starvationTimer += Time.deltaTime;
            if (starvationTimer >= STARVATION_INTERVAL)
            {
                TakeDamage(1);
                starvationTimer = 0f;
            }
        }
        else
        {
            starvationTimer = 0f;
        }
    }

    public void RestoreHealth(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);

        foreach (HealthBar bar in healthBars)
        {
            bar.SetHealth(currentHealth);
        }
    }

    public void RestoreHunger(int amount)
    {
        currentHunger = Mathf.Min(currentHunger + amount, maxHunger);

        foreach (Hungerbar bar in hungerbars)
        {
            bar.SetHunger(currentHunger);
        }

        UpdateHungerPenalties();
    }

    public void TakeDamage(int damage)
    {
        currentHealth = Mathf.Max(0, currentHealth - damage);

        foreach (HealthBar bar in healthBars)
        {
            bar.SetHealth(currentHealth);
        }

        // Trigger flash effect
        if (spriteRenderer != null && flashMaterial != null)
        {
            if (flashCoroutine != null) StopCoroutine(flashCoroutine);
            flashCoroutine = StartCoroutine(FlashWhite());
        }
    }

    private IEnumerator FlashWhite()
    {
        spriteRenderer.material = flashMaterial;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.material = originalMaterial;
        flashCoroutine = null;
    }

    public void TakeHunger(int hunger)
    {
        currentHunger = Mathf.Max(0, currentHunger - hunger);

        foreach (Hungerbar bar in hungerbars)
        {
            bar.SetHunger(currentHunger);
        }

        UpdateHungerPenalties();
    }

    private void UpdateHungerPenalties()
    {
        if (playerMovement == null) return;

        if (currentHunger <= 5)
        {
            playerMovement.speedPenalty = 2f;
        }
        else if (currentHunger <= 10)
        {
            playerMovement.speedPenalty = 1f;
        }
        else
        {
            playerMovement.speedPenalty = 0f;
        }
    }
}