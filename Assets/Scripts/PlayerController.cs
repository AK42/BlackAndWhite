using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 15f;
    public float maxJumpHeight = 2f;
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;
    public BoxCollider2D groundCheck;
    public BoxCollider2D wallCheckHaut;
    public BoxCollider2D wallCheckBas;
    public LayerMask groundLayer;
    public float checkRadius = 0.2f;
    public float runVelocityThreshold = 0.1f;
    public float wallJumpForce = 15f;
    public float wallJumpHorizontalForce = 8f;
    public float wallJumpInputLockTime = 0.2f;
    public float sameWallJumpUpwardMultiplier = 0.6f;
    public float wallJumpReadyTime = 0.1f; // Time after grabbing wall before wall jump is allowed

    private int lastWallJumpDirection = 0; // -1 for left, 1 for right, 0 for none
    private float wallJumpInputLockCounter = 0f;
    private float lockedMoveInput = 0f;
    private Rigidbody2D rb;
    private Animator animator;
    private bool isGrounded;
    private bool isWallSliding;
    private int currentWallDirection = 0; // -1 for left, 1 for right, 0 for none
    private float moveInput;
    private bool facingRight = true;
    private float jumpStartY;
    private bool jumped = false;
    private float wallJumpReadyCounter = 0f;
    private bool wallJumpReady = false;
    private bool wasWallSliding = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        UpdateGroundedState();
        UpdateWallSlidingState();
        ReadInput();
        UpdateRunningAnimation();
        HandleJumpInput();
        ApplyVariableGravity();
        EnforceMaxJumpHeight();
        HandleSpriteFlip();
        UpdateJumpAndFallAnimations();
    }

    void FixedUpdate()
    {
        ApplyHorizontalMovement();
    }

    // --- Separation of Concerns Methods ---

    void UpdateGroundedState()
    {
        isGrounded = groundCheck.IsTouchingLayers(groundLayer);
        animator.SetBool("IsGrounded", isGrounded);
    }

    void UpdateWallSlidingState()
    {
        bool onLeftWall = wallCheckHaut.IsTouchingLayers(groundLayer) && wallCheckBas.IsTouchingLayers(groundLayer) && !isGrounded && moveInput < 0;
        bool onRightWall = wallCheckHaut.IsTouchingLayers(groundLayer) && wallCheckBas.IsTouchingLayers(groundLayer) && !isGrounded && moveInput > 0;
        bool touchingWall = onLeftWall || onRightWall;

        if ((onLeftWall && rb.linearVelocity.y < 1) || (onRightWall && rb.linearVelocity.y < 1))
        {
            isWallSliding = true;
            currentWallDirection = onLeftWall ? -1 : 1;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -1.2f);

            // Start wall jump ready timer only when first starting to wall slide
            if (!wasWallSliding)
            {
                wallJumpReady = false;
                wallJumpReadyCounter = wallJumpReadyTime;
            }
        }
        else
        {
            isWallSliding = false;
            currentWallDirection = 0;
            wallJumpReady = false;
            wallJumpReadyCounter = 0f;
        }

        // Update wall jump ready timer
        if (isWallSliding && !wallJumpReady)
        {
            wallJumpReadyCounter -= Time.deltaTime;
            if (wallJumpReadyCounter <= 0f)
            {
                wallJumpReady = true;
            }
        }

        wasWallSliding = isWallSliding;
        animator.SetBool("IsWallSliding", isWallSliding);
    }

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

    void UpdateRunningAnimation()
    {
        bool isRunning = Mathf.Abs(rb.linearVelocity.x) > runVelocityThreshold;
        animator.SetBool("IsRunning", isRunning);
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

                // Check if jumping from the same wall as last time
                float upwardForce = wallJumpForce;
                if (lastWallJumpDirection == currentWallDirection)
                {
                    upwardForce *= sameWallJumpUpwardMultiplier;
                }

                rb.linearVelocity = new Vector2(jumpDir * wallJumpHorizontalForce, upwardForce);

                if ((facingRight && jumpDir < 0) || (!facingRight && jumpDir > 0))
                {
                    Flip();
                }

                jumpStartY = transform.position.y;
                jumped = true;

                wallJumpInputLockCounter = wallJumpInputLockTime;
                lockedMoveInput = jumpDir;

                // Update last wall jump direction
                lastWallJumpDirection = currentWallDirection;

                wallJumpReady = false;
                wallJumpReadyCounter = 0f;
            }
        }
    }

    void ApplyVariableGravity()
    {
        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
        else if (rb.linearVelocity.y > 0 && !Input.GetButton("Jump"))
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }
    }

    void EnforceMaxJumpHeight()
    {
        if (jumped && transform.position.y >= jumpStartY + maxJumpHeight)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
            jumped = false;
        }
    }

    void HandleSpriteFlip()
    {
        if ((facingRight && moveInput < 0) || (!facingRight && moveInput > 0))
        {
            Flip();
        }
    }

    void UpdateJumpAndFallAnimations()
    {
        animator.SetBool("IsJumping", !isGrounded && rb.linearVelocity.y > 0);
        animator.SetBool("IsFalling", !isGrounded && rb.linearVelocity.y < -2);
    }

    void ApplyHorizontalMovement()
    {
        // Only apply normal movement if not in wall jump input lock
        if (wallJumpInputLockCounter <= 0f)
        {
            rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        }
        // else: do nothing, keep the wall jump velocity
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
    bool IsTouchingWall()
    {
        return wallCheckHaut.IsTouchingLayers(groundLayer) && wallCheckBas.IsTouchingLayers(groundLayer) && !isGrounded;
    }
}
