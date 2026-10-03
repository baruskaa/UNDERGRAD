using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inventory.Model;

public class PlayerManager : MonoBehaviour
{
    [Header("Save / Load References")]
    public InventorySO inventoryData;
    public QuestManager questManager;
    public List<ItemSO> itemDatabase; // Assign all ItemSO assets here in the Inspector

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
    private const float STARVATION_INTERVAL = 3f;

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
        }
        if (Input.GetKeyDown(KeyCode.J))
        {
            TakeHunger(1);
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            LoadPlayer();
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

    public void SavePlayer()
    {
        SaveSystem.SavePlayer(this, inventoryData, questManager);
        Debug.Log("Game Saved!");
    }

    public void LoadPlayer()
    {
        PlayerData data = SaveSystem.LoadPlayer();

        if (data == null) return;

        // Restore Stats
        currentHealth = data.health;
        currentHunger = data.hunger;

        foreach (HealthBar bar in healthBars)
        {
            bar.SetHealth(currentHealth);
        }

        foreach (Hungerbar bar in hungerbars)
        {
            bar.SetHunger(currentHunger);
        }

        // Restore Position
        Vector3 position;
        position.x = data.position[0];
        position.y = data.position[1];
        position.z = data.position[2];
        transform.position = position;

        // Restore Inventory Data
        if (inventoryData != null && itemDatabase != null)
        {
            // Clear current inventory contents
            for (int i = 0; i < inventoryData.Size; i++)
            {
                inventoryData.RemoveItem(i);
            }

            // Repopulate from saved items
            for (int i = 0; i < data.inventoryItemNames.Count; i++)
            {
                string storedName = data.inventoryItemNames[i];

                ItemSO matchedItem = itemDatabase.Find(item =>
                    item != null && (item.Name == storedName || item.name == storedName));

                if (matchedItem != null)
                {
                    inventoryData.AddItem(matchedItem);
                }
            }
        }

        // Restore Quests
        if (questManager != null && data.questCompleted != null)
        {
            questManager.questCompleted = (bool[])data.questCompleted.Clone();
            questManager.itemCollected = data.itemCollected;
        }

        UpdateHungerPenalties();
        Debug.Log("Game Loaded!");
    }

    // --- EAT FOOD FUNCTION ---
    public void Eat(int hungerAmount, int healthAmount = 3)
    {
        RestoreHunger(hungerAmount);
        RestoreHealth(healthAmount);
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

        hungerTimer = 0f;
        UpdateHungerPenalties();
    }

    public void TakeDamage(int damage)
    {
        currentHealth = Mathf.Max(0, currentHealth - damage);

        foreach (HealthBar bar in healthBars)
        {
            bar.SetHealth(currentHealth);
        }

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