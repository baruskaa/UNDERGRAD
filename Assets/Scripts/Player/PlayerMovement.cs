using UnityEngine;
using UnityEngine.InputSystem;
using Inventory.Model;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    [HideInInspector] public float speedPenalty = 0f;

    [Header("Inventory Reference")]
    public InventorySO inventoryData;
    public ItemSO flashlightItem; // Drag Flashlight ItemSO here (or leave null to search by name "Flashlight")

    [Header("Flashlight Settings")]
    public Transform flashlightPivot;
    public GameObject flashlightLight;
    public float lightRotationOffset = 0f;

    private Rigidbody2D rb;
    private float speedX;
    private float speedY;
    private Animator animator;
    private Vector2 lastFacingDir = Vector2.down;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        if (flashlightPivot != null)
        {
            UpdateFlashlightRotation();
        }
    }

    void Update()
    {
        if (DialogueManager.Instance != null && DialogueManager.Instance.isDialogueActive)
        {
            speedX = 0f;
            speedY = 0f;
            animator.SetBool("isWalking", false);
            return;
        }

        // Toggle Flashlight input check
        bool fKeyPressed = (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame) || Input.GetKeyDown(KeyCode.F);

        if (fKeyPressed && flashlightLight != null)
        {
            bool hasFlashlightInInventory = false;

            if (inventoryData != null)
            {
                // Check by ScriptableObject reference or by name fallback
                hasFlashlightInInventory = flashlightItem != null
                    ? inventoryData.HasItem(flashlightItem)
                    : inventoryData.HasItemByName("Flashlight");
            }

            if (hasFlashlightInInventory)
            {
                flashlightLight.SetActive(!flashlightLight.activeSelf);
            }
            else
            {
                Debug.Log("[Flashlight] You do not have a flashlight in your inventory!");
            }
        }

        speedX = 0f;
        speedY = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) speedX -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) speedX += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) speedY -= 1f;
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) speedY += 1f;
        }

        bool isWalking = speedX != 0f || speedY != 0f;
        animator.SetBool("isWalking", isWalking);

        if (isWalking)
        {
            lastFacingDir = new Vector2(speedX, speedY).normalized;

            animator.SetFloat("InputX", speedX);
            animator.SetFloat("InputY", speedY);
            animator.SetFloat("LastInputX", speedX);
            animator.SetFloat("LastInputY", speedY);
        }

        UpdateFlashlightRotation();
    }

    private void UpdateFlashlightRotation()
    {
        if (flashlightPivot == null) return;

        float angle = Mathf.Atan2(lastFacingDir.y, lastFacingDir.x) * Mathf.Rad2Deg;
        flashlightPivot.rotation = Quaternion.Euler(0f, 0f, angle + lightRotationOffset);
    }

    void FixedUpdate()
    {
        if (DialogueManager.Instance != null && DialogueManager.Instance.isDialogueActive)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        float currentSpeed = Mathf.Max(1f, moveSpeed - speedPenalty);
        rb.linearVelocity = new Vector2(speedX * currentSpeed, speedY * currentSpeed);
    }
}