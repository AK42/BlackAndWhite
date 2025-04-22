using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 15f;
    public float maxJumpHeight = 2f; // Maximum height relative to the start of the jump
    public float fallMultiplier = 2.5f; // Multiplier for faster falling
    public float lowJumpMultiplier = 2f; // Multiplier for lower jump height
    public Transform groundCheck;
    public LayerMask groundLayer;
    public float checkRadius = 0.2f;
    public float runVelocityThreshold = 0.1f;

    private Rigidbody2D rb;
    private Animator animator;
    private bool isGrounded;
    private float moveInput;
    private bool facingRight = true;
    private float jumpStartY;
    private bool jumped = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Check if the player is grounded
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
        animator.SetBool("IsGrounded", isGrounded);

        // Get horizontal input
        moveInput = Input.GetAxisRaw("Horizontal");

        // Check velocity for running animation
        bool isRunning = Mathf.Abs(rb.linearVelocity.x) > runVelocityThreshold;
        animator.SetBool("IsRunning", isRunning);

        // Handle jumping
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpStartY = transform.position.y; // Store the initial Y position
            jumped = true;
        }

        // Modify gravity for faster fall
        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
        else if (rb.linearVelocity.y > 0 && !Input.GetButton("Jump"))
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }

        // Apply gravity when max jump height is reached
        if (jumped && transform.position.y >= jumpStartY + maxJumpHeight)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
            jumped = false; // Reset jump state
        }

        // Flip the sprite based on direction
        if ((facingRight && moveInput < 0) || (!facingRight && moveInput > 0))
        {
            Flip();
        }

        // Update animator parameters
        animator.SetBool("IsJumping", !isGrounded && rb.linearVelocity.y > 0);
        animator.SetBool("IsFalling", !isGrounded && rb.linearVelocity.y < -2);
    }

    void FixedUpdate()
    {
        // Apply horizontal movement
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
}
