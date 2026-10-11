using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DamageTrigger2D : MonoBehaviour
{
    [Header("Damage Settings")]
    [Tooltip("Amount of damage dealt when inside the trigger.")]
    public int damageAmount = 1;

    [Tooltip("Check if the damage should destroy this hazard object on contact.")]
    public bool destroyOnContact = false;

    [Header("Continuous Damage Settings")]
    [Tooltip("Check if this object deals repeated damage while the player remains inside the trigger.")]
    public bool dealContinuousDamage = false;

    [Tooltip("Time delay in seconds between damage ticks when dealing continuous damage.")]
    public float damageInterval = 1f;

    private bool isPlayerInside = false;
    private float nextDamageTime = 0f;
    private GameObject playerObjRef;

    private void Awake()
    {
        // Ensure the Collider2D component is set to IsTrigger
        Collider2D col = GetComponent<Collider2D>();
        if (col != null && !col.isTrigger)
        {
            col.isTrigger = true;
            Debug.LogWarning($"[DamageTrigger2D] Collider2D on {gameObject.name} was automatically set to IsTrigger = true.");
        }
    }

    private void Update()
    {
        // Continuous damage tick timer running in Update (unaffected by Rigidbody 2D physics sleep)
        if (dealContinuousDamage && isPlayerInside)
        {
            if (Time.time >= nextDamageTime)
            {
                ApplyDamage();
                nextDamageTime = Time.time + damageInterval;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInside = true;
            playerObjRef = collision.gameObject;

            // Deal immediate damage upon entering
            ApplyDamage();
            nextDamageTime = Time.time + damageInterval;

            if (destroyOnContact)
            {
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInside = false;
            playerObjRef = null;
        }
    }

    private void ApplyDamage()
    {
        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.TakeDamage(damageAmount);
        }
        else if (playerObjRef != null)
        {
            PlayerManager pm = playerObjRef.GetComponent<PlayerManager>();
            if (pm != null)
            {
                pm.TakeDamage(damageAmount);
            }
        }
    }
}