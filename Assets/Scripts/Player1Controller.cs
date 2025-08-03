using UnityEngine;

public class Player1Controller : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 12f;
    [SerializeField] private float jumpForce = 16f;
    [SerializeField] private float airControl = 0.7f; // How much control you have in the air
    
    [Header("Audio")]
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip landSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioClip goalSound;
    
    private Rigidbody2D rb;
    private bool isGrounded;
    private bool wasGrounded; // Track previous ground state for landing detection
    private bool isActivePlayer = true; // Controls whether this player responds to input
    private AudioSource audioSource;
    private bool hasPlayedJumpSound = false; // Prevent jump sound spam when holding key

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Initialize audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        
        // Subscribe to game events for death and goal sounds
        GameEvents.OnPlayer1Died += PlayDeathSound;
        GameEvents.OnPlayer1ReachedGoal += PlayGoalSound;
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
        
        // Reset jump sound flag when jump keys are released or player is not grounded
        if ((!Input.GetKey(KeyCode.Space) && !Input.GetKey(KeyCode.UpArrow)) || !isGrounded)
        {
            hasPlayedJumpSound = false;
        }
    }
    
    private void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        PlayJumpSound();
    }
    
    // Method called by GroundCheck script
    public void SetGrounded(bool grounded)
    {
        wasGrounded = isGrounded;
        isGrounded = grounded;
        
        // Play landing sound when transitioning from not grounded to grounded
        if (!wasGrounded && isGrounded)
        {
            PlayLandSound();
        }
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
    
    #region Audio Methods
    
    private void PlayJumpSound()
    {
        if (jumpSound != null && audioSource != null && !hasPlayedJumpSound)
        {
            // Store original pitch and temporarily lower it
            float originalPitch = audioSource.pitch;
            audioSource.pitch = 0.4f; // Much lower pitch to make it deep and less ear-piercing
            audioSource.PlayOneShot(jumpSound, 0.5f); // Play at 50% volume to tone it down
            audioSource.pitch = originalPitch; // Restore original pitch
            hasPlayedJumpSound = true; // Prevent playing again until reset
        }
    }
    
    private void PlayLandSound()
    {
        if (landSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(landSound);
        }
    }
    
    private void PlayDeathSound()
    {
        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound);
        }
    }
    
    private void PlayGoalSound()
    {
        if (goalSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(goalSound);
        }
    }
    
    #endregion
    
    void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        GameEvents.OnPlayer1Died -= PlayDeathSound;
        GameEvents.OnPlayer1ReachedGoal -= PlayGoalSound;
    }
}
