using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody2D rb;
    private float speedX;
    private float speedY;
    private Animator animator;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Freeze input reading while dialogue is active
        if (DialogueManager.Instance != null && DialogueManager.Instance.isDialogueActive)
        {
            speedX = 0f;
            speedY = 0f;
            animator.SetBool("isWalking", false);
            return;
        }

        if (Keyboard.current == null) return;

        speedX = 0f;
        speedY = 0f;

        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) speedX -= 1f;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) speedX += 1f;
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) speedY -= 1f;
        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) speedY += 1f;

        bool isWalking = speedX != 0f || speedY != 0f;
        animator.SetBool("isWalking", isWalking);

        if (isWalking)
        {
            animator.SetFloat("InputX", speedX);
            animator.SetFloat("InputY", speedY);
            animator.SetFloat("LastInputX", speedX);
            animator.SetFloat("LastInputY", speedY);
        }
    }

    void FixedUpdate()
    {
        if (DialogueManager.Instance != null && DialogueManager.Instance.isDialogueActive)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        rb.linearVelocity = new Vector2(speedX * moveSpeed, speedY * moveSpeed);
    }
}