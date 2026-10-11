using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    [Header("Health")]
    [SerializeField] public int maxHealth = 20;
    [SerializeField] public int currentHealth;

    [Header("Hunger")]
    [SerializeField] public int maxHunger = 20;
    [SerializeField] public int currentHunger;

    [Header("Hunger Timer Settings")]
    [Tooltip("Time in seconds before hunger decreases by 1 point.")]
    public float hungerInterval = 60f; // 1 Minute
    private float hungerTimer = 0f;

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

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (spriteRenderer != null)
        {
            originalMaterial = spriteRenderer.material;
        }

        // ONLY initialize default full health/hunger if we are NOT in the middle of loading a save file
        if (!SaveSystem.IsLoadingSave)
        {
            if (currentHealth <= 0) currentHealth = maxHealth;
            if (currentHunger <= 0) currentHunger = maxHunger;
        }

        InitializeBars();
        UpdateHungerPenalties();
    }

    void Update()
    {
        // --- PASSIVE HUNGER DECAY ---
        if (currentHunger > 0)
        {
            hungerTimer += Time.deltaTime;
            if (hungerTimer >= hungerInterval)
            {
                TakeHunger(1);
                hungerTimer = 0f;
            }
        }

        // --- DEBUG CONTROLS ---
        if (Input.GetKeyDown(KeyCode.H))
        {
            TakeDamage(1);
            Debug.Log($"[DEBUG] Pressed 'H' -> Health: {currentHealth}/{maxHealth}");
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            TakeHunger(1);
            Debug.Log($"[DEBUG] Pressed 'J' -> Hunger: {currentHunger}/{maxHunger}");
        }

        // --- STARVATION DAMAGE LOGIC ---
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

    public void InitializeBars()
    {
        if (healthBars != null)
        {
            foreach (HealthBar bar in healthBars)
            {
                if (bar != null)
                {
                    bar.SetmaxHealth(maxHealth);
                    bar.SetHealth(currentHealth);
                }
            }
        }

        if (hungerbars != null)
        {
            foreach (Hungerbar bar in hungerbars)
            {
                if (bar != null)
                {
                    bar.SetMaxHunger(maxHunger);
                    bar.SetHunger(currentHunger);
                }
            }
        }
    }

    public void UpdateUI()
    {
        if (healthBars != null)
        {
            foreach (HealthBar bar in healthBars)
            {
                if (bar != null) bar.SetHealth(currentHealth);
            }
        }

        if (hungerbars != null)
        {
            foreach (Hungerbar bar in hungerbars)
            {
                if (bar != null) bar.SetHunger(currentHunger);
            }
        }

        UpdateHungerPenalties();
    }

    public void RestoreHealth(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        UpdateUI();
    }

    public void Heal(int amount) => RestoreHealth(amount);

    public void RestoreHunger(int amount)
    {
        currentHunger = Mathf.Min(currentHunger + amount, maxHunger);
        hungerTimer = 0f;
        UpdateUI();
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return; // Prevent multiple death triggers

        currentHealth = Mathf.Max(0, currentHealth - damage);
        UpdateUI();

        // Trigger flash effect
        if (spriteRenderer != null && flashMaterial != null)
        {
            if (flashCoroutine != null) StopCoroutine(flashCoroutine);
            flashCoroutine = StartCoroutine(FlashWhite());
        }

        // Check for Game Over when health reaches 0
        if (currentHealth == 0)
        {
            OnPlayerDeath();
        }
    }

    private void OnPlayerDeath()
    {
        Debug.Log("[PlayerManager] Player health reached 0. Triggering Game Over!");

        if (GameOverManager.Instance != null)
        {
            GameOverManager.Instance.TriggerGameOver();
        }
        else
        {
            Debug.LogError("[PlayerManager] GameOverManager Instance not found in scene!");
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
        UpdateUI();
    }

    public void ConsumeHunger(int amount) => TakeHunger(amount);

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