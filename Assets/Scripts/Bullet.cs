using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float bulletSpeed = 20f;
    [SerializeField] private float lifetime = 5f; // Bullet will destroy itself after this many seconds
    [SerializeField] private float spinSpeed = 360f; // Degrees per second of spinning
    
    private Rigidbody2D rb;
    private Vector2 direction;
    private bool directionSet = false;
    private GameObject shooter; // Reference to the player who shot this bullet
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        Debug.Log($"Bullet Awake called at position: {transform.position}");
        
        // Ensure we have a Rigidbody2D component
        if (rb == null)
        {
            Debug.LogError("Bullet prefab is missing Rigidbody2D component!");
            Destroy(gameObject);
            return;
        }
        
        // Configure Rigidbody2D for bullet behavior
        rb.gravityScale = 0f; // Disable gravity so bullet doesn't drop
        rb.linearDamping = 0f; // No air resistance
        rb.angularDamping = 0f; // No rotational resistance
        
        // Apply spinning
        rb.angularVelocity = spinSpeed;
        Debug.Log($"Bullet Rigidbody2D configured: gravity={rb.gravityScale}, drag={rb.linearDamping}, spin={rb.angularVelocity}°/s");
    }
    
    void Start()
    {
        Debug.Log($"Bullet Start called. Direction set: {directionSet}, Direction: {direction}");
        
        // Apply direction if it was set before Start was called
        if (directionSet)
        {
            rb.linearVelocity = direction * bulletSpeed;
            Debug.Log($"Bullet velocity set to: {rb.linearVelocity}");
        }
        else
        {
            Debug.LogWarning("Bullet direction not set! Bullet will not move.");
        }
        
        // Ensure bullet is spinning (backup in case it wasn't set earlier)
        rb.angularVelocity = spinSpeed;
        Debug.Log($"Bullet spin ensured: {rb.angularVelocity}°/s");
        
        // Safety check - if velocity is still zero, something went wrong
        if (rb.linearVelocity.magnitude < 0.1f)
        {
            Debug.LogError($"Bullet has no velocity! Current velocity: {rb.linearVelocity}");
        }
        
        // Destroy bullet after lifetime to prevent memory leaks
        Destroy(gameObject, lifetime);
        Debug.Log($"Bullet will be destroyed in {lifetime} seconds");
    }
    
    public void SetDirection(Vector2 shootDirection, GameObject shooterObject = null)
    {
        direction = shootDirection.normalized;
        directionSet = true;
        shooter = shooterObject;
        Debug.Log($"SetDirection called with direction: {shootDirection}, normalized: {direction}, shooter: {(shooterObject != null ? shooterObject.name : "null")}");
        
        // If rb is available, set velocity and spin immediately
        if (rb != null)
        {
            rb.linearVelocity = direction * bulletSpeed;
            rb.angularVelocity = spinSpeed; // Make sure bullet spins
            Debug.Log($"Bullet velocity immediately set to: {rb.linearVelocity}, spin: {rb.angularVelocity}°/s");
        }
        else
        {
            Debug.Log("Rigidbody2D not available yet, will set velocity in Start()");
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"Bullet OnTriggerEnter2D with: {other.name} (tag: {other.tag})");
        
        // Ignore collision with the shooter
        if (shooter != null && other.gameObject == shooter)
        {
            Debug.Log($"Ignoring collision with shooter: {shooter.name}");
            return;
        }
        
        // Check if bullet hit a player
        if (other.CompareTag("Player"))
        {
            Debug.Log($"Bullet hit player: {other.name}");
            
            // Check if this is Player 1 being hit (victory condition for Player 2)
            Player1Controller player1 = other.GetComponent<Player1Controller>();
            if (player1 != null)
            {
                Debug.Log("Player 1 was shot! Player 2 wins!");
                
                // Trigger event - much more efficient than FindFirstObjectByType
                GameEvents.TriggerPlayer2Victory();
            }
        }
        
        Debug.Log($"Destroying bullet due to collision with: {other.name}");
        // Destroy bullet on any collision
        Destroy(gameObject);
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"Bullet OnCollisionEnter2D with: {collision.gameObject.name} (tag: {collision.gameObject.tag})");
        
        // Ignore collision with the shooter
        if (shooter != null && collision.gameObject == shooter)
        {
            Debug.Log($"Ignoring collision with shooter: {shooter.name}");
            return;
        }
        
        // Check if bullet hit a player
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log($"Bullet hit player: {collision.gameObject.name}");
            
            // Check if this is Player 1 being hit (victory condition for Player 2)
            Player1Controller player1 = collision.gameObject.GetComponent<Player1Controller>();
            if (player1 != null)
            {
                Debug.Log("Player 1 was shot! Player 2 wins!");
                
                // Trigger event - much more efficient than FindFirstObjectByType
                GameEvents.TriggerPlayer2Victory();
            }
        }
        
        Debug.Log($"Destroying bullet due to collision with: {collision.gameObject.name}");
        // Destroy bullet on any collision
        Destroy(gameObject);
    }
}