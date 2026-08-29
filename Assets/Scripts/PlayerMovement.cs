using UnityEngine;

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
        speedX = Input.GetAxisRaw("Horizontal");
        speedY = Input.GetAxisRaw("Vertical");

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
        rb.linearVelocity = new Vector2(speedX * moveSpeed, speedY * moveSpeed);
    }
}