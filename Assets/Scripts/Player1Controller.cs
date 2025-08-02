using UnityEngine;

public class Player1Controller : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 12f;
    [SerializeField] private float jumpForce = 16f;
    [SerializeField] private float airControl = 0.7f; // How much control you have in the air
    
    private Rigidbody2D rb;
    private bool isGrounded;
    private bool isActivePlayer = true; // Controls whether this player responds to input

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        // Only handle input if this is the active player
        if (!isActivePlayer) return;
        
        // Handle horizontal movement
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        
        // Apply different movement based on whether grounded or in air
        if (isGrounded)
        {
            // Full control when grounded
            Vector2 movement = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
            rb.linearVelocity = movement;
        }
        else
        {
            // Reduced air control for more precise platforming
            float currentXVelocity = rb.linearVelocity.x;
            float targetXVelocity = horizontalInput * moveSpeed;
            float newXVelocity = Mathf.Lerp(currentXVelocity, targetXVelocity, airControl * Time.deltaTime * 10f);
            
            rb.linearVelocity = new Vector2(newXVelocity, rb.linearVelocity.y);
        }
        
        // Handle jumping
        if ((Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.UpArrow)) && isGrounded)
        {
            Jump();
        }
    }
    
    private void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }
    
    // Method called by GroundCheck script
    public void SetGrounded(bool grounded)
    {
        isGrounded = grounded;
    }
    
    // Method to set whether this player is active
    public void SetActive(bool active)
    {
        isActivePlayer = active;
    }
    
    // Property to check if this player is active
    public bool IsActive => isActivePlayer;
    
    // Property to check if this player is grounded (useful for recording system)
    public bool IsGrounded => isGrounded;
}
