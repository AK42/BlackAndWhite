using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 15f;
    public float maxJumpHeight = 2f;

    [Header("Physics")]
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;

    [Header("Wall Interactions")]
    public float wallSlideSpeed = 1.2f;
    public float wallJumpForce = 15f;
    public float wallJumpHorizontalForce = 8f;
    public float wallJumpInputLockTime = 0.2f;
    public float wallJumpReadyTime = 0.1f;

    [Header("Checks & Thresholds")]
    public BoxCollider2D groundCheck;
    public BoxCollider2D wallCheckHaut;
    public BoxCollider2D wallCheckBas;
    public LayerMask groundLayer;
    public float runVelocityThreshold = 0.1f;
    public float fallVelocityThreshold = -2f;
    public float wallSlideVelocityThreshold = 0.5f;

    // Private state variables
    private Rigidbody2D rb;
    private Animator animator;

    private bool isGrounded;
    private bool isWallSliding;
    private int currentWallDirection; // -1 for left, 1 for right
    private float moveInput;
    private bool facingRight = true;
    private float jumpStartY;
    private bool jumped;

    // Wall jump state variables
    private float wallJumpInputLockCounter;
    private float lockedMoveInput;
    private float wallJumpReadyCounter;
    private bool wallJumpReady;
    private bool wasWallSliding;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Order of operations is important for state management
        UpdateGroundedState();
        ReadInput(); // Read input before using it for state checks
        UpdateWallSlidingState();
        
        HandleJumpInput();
        HandleSpriteFlip();
        UpdateAllAnimations();
    }

    void FixedUpdate()
    {
        // Physics-related updates
        ApplyHorizontalMovement();
        ApplyVariableGravity();
        EnforceMaxJumpHeight();
    }

    // --- State Update Methods ---

    void UpdateGroundedState()
    {
        isGrounded = groundCheck.IsTouchingLayers(groundLayer);
    }

    void UpdateWallSlidingState()
    {
        bool isTouchingWall = IsTouchingWall();
        bool canStartSliding = isTouchingWall && !isGrounded && rb.linearVelocity.y < wallSlideVelocityThreshold;

        if (canStartSliding)
        {
            // Determine wall direction based on which side has contact
            currentWallDirection = wallCheckHaut.transform.position.x > transform.position.x ? 1 : -1;
            
            if (!wasWallSliding) // Just started touching the wall
            {
                wallJumpReady = false;
                wallJumpReadyCounter = wallJumpReadyTime;
            }
            
            isWallSliding = true;
        }
        else
        {
            isWallSliding = false;
            currentWallDirection = 0;
            wallJumpReady = false;
        }

        // Handle the actual slide speed
        if (isWallSliding)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -wallSlideSpeed);
        }

        // Update the timer for when a wall jump becomes available
        if (isWallSliding && !wallJumpReady)
        {
            wallJumpReadyCounter -= Time.deltaTime;
            if (wallJumpReadyCounter <= 0f)
            {
                wallJumpReady = true;
            }
        }

        wasWallSliding = isWallSliding;
    }

    // --- Input & Movement Methods ---

    void ReadInput()
    {
        if (wallJumpInputLockCounter > 0f)
        {
            moveInput = lockedMoveInput;
            wallJumpInputLockCounter -= Time.deltaTime;
        }
        else
        {
            moveInput = Input.GetAxisRaw("Horizontal");
        }
    }

    void HandleJumpInput()
    {
        if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                jumpStartY = transform.position.y;
                jumped = true;
            }
            else if (IsTouchingWall() && currentWallDirection != 0 && wallJumpReady)
            {
                float jumpDir = -currentWallDirection;
                rb.linearVelocity = new Vector2(jumpDir * wallJumpHorizontalForce, wallJumpForce);
                
                jumpStartY = transform.position.y;
                jumped = true;
                
                wallJumpInputLockCounter = wallJumpInputLockTime;
                lockedMoveInput = jumpDir;
                
                wallJumpReady = false;
            }
        }
    }

    void ApplyHorizontalMovement()
    {
        if (wallJumpInputLockCounter <= 0f)
        {
            rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        }
    }

    void ApplyVariableGravity()
    {
        if (isWallSliding) return; // No extra gravity while sliding

        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        else if (rb.linearVelocity.y > 0 && !Input.GetButton("Jump"))
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }
    }

    void EnforceMaxJumpHeight()
    {
        if (jumped && rb.linearVelocity.y > 0 && transform.position.y >= jumpStartY + maxJumpHeight)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
            jumped = false;
        }
    }

    // --- Animation & Visuals ---

    void HandleSpriteFlip()
    {
        // Flip only if not locked by a wall jump
        if (wallJumpInputLockCounter <= 0f)
        {
            if (moveInput > 0 && !facingRight)
            {
                Flip();
            }
            else if (moveInput < 0 && facingRight)
            {
                Flip();
            }
        }
    }

    void UpdateAllAnimations()
    {
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetBool("IsWallSliding", isWallSliding);
        animator.SetBool("IsRunning", Mathf.Abs(rb.linearVelocity.x) > runVelocityThreshold);
        animator.SetBool("IsJumping", !isGrounded && !isWallSliding && rb.linearVelocity.y > 0);
        animator.SetBool("IsFalling", !isGrounded && !isWallSliding && rb.linearVelocity.y < fallVelocityThreshold);
    }

    void Flip()
    {
        facingRight = !facingRight;
        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
    }

    // --- Helper Methods ---

    bool IsTouchingWall()
    {
        return wallCheckHaut.IsTouchingLayers(groundLayer) && wallCheckBas.IsTouchingLayers(groundLayer) && !isGrounded;
    }
}
