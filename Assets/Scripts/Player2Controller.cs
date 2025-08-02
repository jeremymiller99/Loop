using UnityEngine;

public class Player2Controller : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 12f;
    [SerializeField] private float jumpForce = 16f;
    [SerializeField] private float airControl = 0.7f; // How much control you have in the air
    [SerializeField] private GameObject bulletPrefab; // Bullet prefab to instantiate
    [SerializeField] private Transform firePoint; // Point where bullets spawn
    [SerializeField] private Transform gun; // Gun transform that rotates towards mouse
    [SerializeField] private float fireRate = 0.3f; // Time between shots (automatic fire)
    [SerializeField] private float gunDistance = 0.8f; // Distance gun orbits around player
    
    private Rigidbody2D rb;
    private bool isGrounded;
    private bool isActivePlayer = false; // Player 2 starts inactive
    private float nextFireTime = 0f; // Time when player can fire next bullet
    private bool facingRight = false; // Track which direction player is facing (starts facing left)
    private Camera playerCamera; // Reference to main camera
    private Vector3 originalGunScale; // Store original gun scale

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCamera = Camera.main;
        
        if (playerCamera == null)
        {
            playerCamera = FindFirstObjectByType<Camera>();
        }
        
        // Store original gun scale to preserve sprite sizing
        if (gun != null)
        {
            originalGunScale = gun.localScale;
        }
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
        
        // Handle gun aiming towards mouse and player facing
        if (gun != null && playerCamera != null)
        {
            AimGunAtMouse();
            FaceMouseDirection();
        }
        
        // Handle automatic shooting with mouse hold
        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
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
    
    // Property to check facing direction (useful for recording system)
    public bool IsFacingRight => facingRight;
    
    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }
    
    private void AimGunAtMouse()
    {
        // Get mouse position in world coordinates
        Vector3 mousePos = Input.mousePosition;
        mousePos = playerCamera.ScreenToWorldPoint(mousePos);
        mousePos.z = 0f; // Set z to 0 for 2D
        
        // Calculate direction from player to mouse
        Vector2 direction = (mousePos - transform.position).normalized;
        
        // Position gun around player at fixed distance
        Vector3 gunPosition = transform.position + (Vector3)(direction * gunDistance);
        gun.position = gunPosition;
        
        // Rotate gun to point toward mouse
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        // Check if gun should be flipped to stay upright
        Vector3 gunScale = originalGunScale; // Preserve original scale
        if (angle > 90f || angle < -90f)
        {
            // Flip gun vertically when pointing leftward (preserve original Y scale magnitude)
            gunScale.y = -Mathf.Abs(originalGunScale.y);
            // Adjust angle to compensate for the flip
            angle += 180f;
        }
        else
        {
            // Keep original Y scale when facing right
            gunScale.y = Mathf.Abs(originalGunScale.y);
        }
        
        gun.rotation = Quaternion.Euler(0, 0, angle);
        gun.localScale = gunScale;
        
        // Position fire point at the gun's position (or slightly ahead)
        if (firePoint != null)
        {
            firePoint.position = gunPosition + (Vector3)(direction * 0.3f); // Slightly ahead of gun
        }
    }
    
    private void FaceMouseDirection()
    {
        // Get mouse position in world coordinates
        Vector3 mousePos = Input.mousePosition;
        mousePos = playerCamera.ScreenToWorldPoint(mousePos);
        mousePos.z = 0f; // Set z to 0 for 2D
        
        // Determine if mouse is to the right or left of player
        bool mouseIsRight = mousePos.x > transform.position.x;
        
        // Flip player sprite if needed
        if (mouseIsRight && !facingRight)
        {
            Flip();
        }
        else if (!mouseIsRight && facingRight)
        {
            Flip();
        }
    }
    
    private void Shoot()
    {
        if (bulletPrefab == null || firePoint == null)
        {
            Debug.LogWarning("Bullet prefab or fire point not set on Player2Controller!");
            return;
        }
        
        if (playerCamera == null)
        {
            Debug.LogWarning("No camera found for mouse position calculation!");
            return;
        }
        
        // Create bullet at fire point
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Debug.Log($"Bullet spawned at position: {firePoint.position}");
        
        // Calculate shoot direction towards mouse
        Vector3 mousePos = Input.mousePosition;
        mousePos = playerCamera.ScreenToWorldPoint(mousePos);
        mousePos.z = 0f; // Set z to 0 for 2D
        
        Vector2 shootDirection = (mousePos - (Vector3)transform.position).normalized;
        Debug.Log($"Shoot direction: {shootDirection}, Mouse pos: {mousePos}, Player pos: {transform.position}");
        
        // Get bullet component and set its direction
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.SetDirection(shootDirection, gameObject);
            Debug.Log($"Bullet direction set successfully");
        }
        else
        {
            Debug.LogError("Bullet prefab doesn't have Bullet script attached!");
        }
    }
    
    // Public method for replay system to simulate shooting at a specific position
    public void SimulateShoot(Vector3 targetWorldPosition)
    {
        if (bulletPrefab == null || firePoint == null)
        {
            Debug.LogWarning("Cannot simulate shoot - bullet prefab or fire point not set!");
            return;
        }
        
        // Create bullet at fire point
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        
        // Calculate shoot direction towards target position
        Vector2 shootDirection = (targetWorldPosition - transform.position).normalized;
        
        // Get bullet component and set its direction
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.SetDirection(shootDirection, gameObject);
            Debug.Log($"Simulated shot towards {targetWorldPosition} from {gameObject.name}");
        }
        else
        {
            Debug.LogError("Bullet prefab doesn't have Bullet script attached!");
        }
    }
}