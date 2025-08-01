using UnityEngine;

public class Player1Controller : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 12f;
    [SerializeField] private float jumpForce = 16f;
    [SerializeField] private float wallJumpForce = 14f;
    [SerializeField] private float wallJumpHorizontalForce = 8f;
    [SerializeField] private float airControl = 0.7f;
    [SerializeField] private float wallSlideSpeed = 2f;
    [SerializeField] private float wallJumpCooldown = 0.1f;
    
    private Rigidbody2D rb;
    private bool isGrounded;
    private bool isTouchingLeftWall;
    private bool isTouchingRightWall;
    private bool isWallSliding;
    private float lastWallJumpTime;
    private bool hasUsedCoyoteJump;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        HandleMovement();
        HandleJumping();
    }
    
    private void HandleMovement()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        
        // Check if wall sliding
        isWallSliding = !isGrounded && (isTouchingLeftWall || isTouchingRightWall) && rb.linearVelocity.y < 0;
        
        if (isGrounded)
        {
            // Ground movement
            rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
        }
        else if (isWallSliding)
        {
            // Wall sliding
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -wallSlideSpeed);
        }
        else
        {
            // Air movement
            float targetXVelocity = horizontalInput * moveSpeed;
            float newXVelocity = Mathf.Lerp(rb.linearVelocity.x, targetXVelocity, airControl * Time.deltaTime * 10f);
            rb.linearVelocity = new Vector2(newXVelocity, rb.linearVelocity.y);
        }
    }
    
    private void HandleJumping()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (isGrounded)
            {
                // Ground jump
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                Debug.Log("Ground Jump");
            }
            else if (isWallSliding && Time.time > lastWallJumpTime + wallJumpCooldown)
            {
                // Wall jump
                float horizontalForce = isTouchingRightWall ? -wallJumpHorizontalForce : wallJumpHorizontalForce;
                rb.linearVelocity = new Vector2(horizontalForce, wallJumpForce);
                lastWallJumpTime = Time.time;
                hasUsedCoyoteJump = true;
                Debug.Log("Wall Jump");
            }
        }
    }
    
    public void SetGrounded(bool grounded)
    {
        if (grounded && !isGrounded)
        {
            Debug.Log("LANDED");
            hasUsedCoyoteJump = false;
        }
        else if (!grounded && isGrounded)
        {
            Debug.Log("LEFT GROUND");
        }
        
        isGrounded = grounded;
        Debug.Log($"Grounded: {isGrounded}");
    }
    
    public void SetWallContact(bool isRightWall, bool isTouching)
    {
        if (isRightWall)
        {
            isTouchingRightWall = isTouching;
        }
        else
        {
            isTouchingLeftWall = isTouching;
        }
        
        Debug.Log($"Wall Contact - Left: {isTouchingLeftWall}, Right: {isTouchingRightWall}");
    }
}
