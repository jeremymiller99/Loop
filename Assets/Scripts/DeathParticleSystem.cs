using UnityEngine;

public class DeathParticleSystem : MonoBehaviour
{
    [Header("Particle System Settings")]
    [SerializeField] private ParticleSystem deathParticles;
    [SerializeField] private Material particleMaterial; // White material for the particles
    [SerializeField] private Texture2D whiteTexture; // Optional: assign white.jpg texture, or will create runtime
    
    [Header("Effect Settings")]
    [SerializeField] private int particleCount = 50;
    [SerializeField] private float explosionForce = 8f;
    [SerializeField] private float particleLifetime = 1.5f;
    [SerializeField] private float particleSize = 0.1f;
    [SerializeField] private float particleSizeVariation = 0.05f;
    
    // Player references to get death positions
    private Player1Controller player1;
    private Player2Controller player2;
    
    // Prevent multiple plays
    private bool isPlayingEffect = false;
    
    void Start()
    {
        // Find player references
        player1 = FindFirstObjectByType<Player1Controller>();
        player2 = FindFirstObjectByType<Player2Controller>();
        
        // Create particle system if not assigned
        if (deathParticles == null)
        {
            CreateParticleSystem();
        }
        
        // Create material if not assigned
        if (particleMaterial == null)
        {
            CreateParticleMaterial();
        }
        
        // Configure the particle system
        ConfigureParticleSystem();
        
        // Subscribe to death events
        GameEvents.OnPlayer1Died += OnPlayer1Death;
        GameEvents.OnPlayer2Died += OnPlayer2Death;
        GameEvents.OnPlayer1Shot += OnPlayer1Shot;
        
        // Subscribe to goal events
        GameEvents.OnPlayer1ReachedGoal += OnPlayer1ReachedGoal;
        GameEvents.OnPlayer1GhostReachedGoal += OnPlayer1GhostReachedGoal;
        
        // Subscribe to phase events to reset the system
        GameEvents.OnPhaseStarted += OnPhaseStarted;
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        GameEvents.OnPlayer1Died -= OnPlayer1Death;
        GameEvents.OnPlayer2Died -= OnPlayer2Death;
        GameEvents.OnPlayer1Shot -= OnPlayer1Shot;
        GameEvents.OnPlayer1ReachedGoal -= OnPlayer1ReachedGoal;
        GameEvents.OnPlayer1GhostReachedGoal -= OnPlayer1GhostReachedGoal;
        GameEvents.OnPhaseStarted -= OnPhaseStarted;
    }
    
    private void CreateParticleSystem()
    {
        // Create a new GameObject for the particle system
        GameObject particleGO = new GameObject("DeathParticles");
        particleGO.transform.SetParent(transform);
        
        // Add ParticleSystem component
        deathParticles = particleGO.AddComponent<ParticleSystem>();
    }
    
    private void CreateParticleMaterial()
    {
        // Create the material with appropriate shader
        // Try Universal Render Pipeline shader first, then fallback to Sprites/Default
        Shader particleShader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
        if (particleShader == null)
        {
            particleShader = Shader.Find("Sprites/Default");
        }
        
        particleMaterial = new Material(particleShader);
        
        if (whiteTexture != null)
        {
            particleMaterial.mainTexture = whiteTexture;
        }
        else
        {
            // Create a simple white texture at runtime
            Texture2D runtimeWhiteTexture = new Texture2D(1, 1);
            runtimeWhiteTexture.SetPixel(0, 0, Color.white);
            runtimeWhiteTexture.Apply();
            particleMaterial.mainTexture = runtimeWhiteTexture;
            Debug.Log("Created runtime white texture for death particles");
        }
        
        particleMaterial.color = Color.white;
        Debug.Log("Created particle material for death effects");
    }
    
    private void ConfigureParticleSystem()
    {
        if (deathParticles == null) return;
        
        var main = deathParticles.main;
        main.startLifetime = particleLifetime;
        main.startSpeed = new ParticleSystem.MinMaxCurve(explosionForce * 0.5f, explosionForce * 1.5f); // Random speed variation
        main.startSize = new ParticleSystem.MinMaxCurve(particleSize * 0.5f, particleSize * 1.5f); // Random size variation
        main.startColor = Color.white;
        main.maxParticles = particleCount;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = false;
        main.loop = false; // Ensure no looping
        main.prewarm = false;
        
        // Shape - emit from a small area with randomness
        var shape = deathParticles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.2f; // Slightly larger area for more spread
        shape.radiusThickness = 0.5f; // Emit from edges and interior
        shape.arc = 360f; // Full circle
        shape.arcMode = ParticleSystemShapeMultiModeValue.Random; // Random positions around circle
        shape.randomDirectionAmount = 0.3f; // Add randomness to initial direction
        
        // Limit velocity over lifetime to slow down particles (damping effect)
        var limitVelocityOverLifetime = deathParticles.limitVelocityOverLifetime;
        limitVelocityOverLifetime.enabled = true;
        limitVelocityOverLifetime.limit = 5f; // Maximum speed particles can have
        limitVelocityOverLifetime.dampen = 0.3f; // How much to slow down particles (0-1)
        limitVelocityOverLifetime.separateAxes = false;
        limitVelocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
        
        // Add velocity noise for more chaotic movement
        var velocityOverLifetime = deathParticles.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
        // Add some orbital velocity for swirling effect - all curves must be same mode
        velocityOverLifetime.orbitalX = new ParticleSystem.MinMaxCurve(-1f, 1f);
        velocityOverLifetime.orbitalY = new ParticleSystem.MinMaxCurve(-1f, 1f);
        velocityOverLifetime.orbitalZ = new ParticleSystem.MinMaxCurve(-1f, 1f);
        velocityOverLifetime.radial = new ParticleSystem.MinMaxCurve(-0.5f, 0.5f);
        
        // Size over lifetime - start normal, then shrink
        var sizeOverLifetime = deathParticles.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 1f);
        sizeCurve.AddKey(0.8f, 1f);
        sizeCurve.AddKey(1f, 0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
        
        // Color over lifetime - fade out
        var colorOverLifetime = deathParticles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(Color.white, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
        );
        colorOverLifetime.color = gradient;
        
        // Emission - one-time burst only
        var emission = deathParticles.emission;
        emission.enabled = true;
        emission.rateOverTime = 0; // No continuous emission
        emission.SetBursts(new ParticleSystem.Burst[0]); // Clear any existing bursts first
        
        // Add some randomness to particle sizes
        var sizeBySpeed = deathParticles.sizeBySpeed;
        sizeBySpeed.enabled = true;
        sizeBySpeed.range = new Vector2(0.5f, 2f);
        AnimationCurve speedSizeCurve = new AnimationCurve();
        speedSizeCurve.AddKey(0f, 0.8f);
        speedSizeCurve.AddKey(1f, 1.2f);
        sizeBySpeed.size = new ParticleSystem.MinMaxCurve(1f, speedSizeCurve);
        
        // Configure renderer for square particles
        var renderer = deathParticles.GetComponent<ParticleSystemRenderer>();
        renderer.material = particleMaterial;
        renderer.alignment = ParticleSystemRenderSpace.View;
        
        Debug.Log("Death particle system configured successfully");
    }
    
    private void OnPlayer1Death()
    {
        if (player1 != null && !isPlayingEffect)
        {
            PlayParticleEffect(player1.transform.position, Color.white);
            HidePlayerSprite(player1.gameObject);
            Debug.Log($"Playing death particles for Player 1 at position: {player1.transform.position}");
        }
        else if (isPlayingEffect)
        {
            Debug.Log("Death particles already playing - ignoring duplicate death event");
        }
    }
    
    private void OnPlayer2Death()
    {
        if (player2 != null && !isPlayingEffect)
        {
            PlayParticleEffect(player2.transform.position, Color.white);
            HidePlayerSprite(player2.gameObject);
            Debug.Log($"Playing death particles for Player 2 at position: {player2.transform.position}");
        }
        else if (isPlayingEffect)
        {
            Debug.Log("Death particles already playing - ignoring duplicate death event");
        }
    }
    
    private void OnPlayer1Shot()
    {
        if (player1 != null && !isPlayingEffect)
        {
            // Use ghost tint for shot death particles
            Color ghostTint = new Color(0.4f, 0.4f, 0.4f, 1f); // Same as inactive player tint
            PlayParticleEffect(player1.transform.position, ghostTint);
            HidePlayerSprite(player1.gameObject);
            Debug.Log($"Playing shot death particles for Player 1 at position: {player1.transform.position}");
        }
        else if (isPlayingEffect)
        {
            Debug.Log("Shot particles already playing - ignoring duplicate shot event");
        }
    }
    
    private void OnPlayer1ReachedGoal()
    {
        if (player1 != null && !isPlayingEffect)
        {
            PlayParticleEffect(player1.transform.position, Color.white);
            // Don't hide the sprite for goal - player should remain visible for celebration
            Debug.Log($"Playing goal particles for Player 1 at position: {player1.transform.position}");
        }
        else if (isPlayingEffect)
        {
            Debug.Log("Goal particles already playing - ignoring duplicate goal event");
        }
    }
    
    private void OnPlayer1GhostReachedGoal()
    {
        if (player1 != null && !isPlayingEffect)
        {
            // Use a golden/yellow tint for ghost victory particles
            Color ghostVictoryTint = new Color(1f, 1f, 0.5f, 1f); // Golden yellow
            PlayParticleEffect(player1.transform.position, ghostVictoryTint);
            HidePlayerSprite(player1.gameObject);
            Debug.Log($"Playing ghost victory particles for Player 1 at position: {player1.transform.position}");
        }
        else if (isPlayingEffect)
        {
            Debug.Log("Ghost victory particles already playing - ignoring duplicate ghost goal event");
        }
    }
    
    private void OnPhaseStarted(GamePhase phase)
    {
        // Reset the particle system when a new phase starts
        ResetParticleSystem();
        Debug.Log($"Particle system reset for phase: {phase}");
    }
    
    private void PlayParticleEffect(Vector3 position, Color particleColor)
    {
        if (deathParticles == null || isPlayingEffect) return;
        
        // Set flag to prevent multiple plays
        isPlayingEffect = true;
        
        // Stop any existing particles first
        deathParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        
        // Clear the particle system completely
        deathParticles.Clear();
        
        // Move particle system to death position
        deathParticles.transform.position = position;
        
        // Set particle color
        var main = deathParticles.main;
        main.startColor = particleColor;
        
        // Configure single burst emission with random particle count variation
        var emission = deathParticles.emission;
        emission.SetBursts(new ParticleSystem.Burst[0]); // Clear existing bursts first
        int randomParticleCount = Random.Range(particleCount - 10, particleCount + 10); // Add some variation
        emission.SetBursts(new ParticleSystem.Burst[]
        {
            new ParticleSystem.Burst(0.0f, randomParticleCount)
        });
        
        // Play the effect (it will automatically stop after the burst due to main.loop = false)
        deathParticles.Play();
        
        // Reset the flag after the particle lifetime + a small buffer
        Invoke(nameof(ResetPlayingFlag), particleLifetime + 0.5f);
        
        Debug.Log($"Particle effect played at position: {position} with {randomParticleCount} particles (color: {particleColor})");
    }
    
    private void HidePlayerSprite(GameObject player)
    {
        if (player == null) return;
        
        // Get the main sprite renderer
        SpriteRenderer mainRenderer = player.GetComponent<SpriteRenderer>();
        if (mainRenderer != null)
        {
            Color color = mainRenderer.color;
            color.a = 0f; // Set alpha to 0 (fully transparent)
            mainRenderer.color = color;
        }
        
        // Also hide all child sprite renderers (weapons, accessories, etc.)
        SpriteRenderer[] childRenderers = player.GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer renderer in childRenderers)
        {
            Color color = renderer.color;
            color.a = 0f; // Set alpha to 0 (fully transparent)
            renderer.color = color;
        }
        
        Debug.Log($"Player sprite hidden: {player.name}");
    }
    
    private void ResetPlayingFlag()
    {
        isPlayingEffect = false;
        Debug.Log("Particle effect flag reset - ready for next death");
    }
    
    private void ResetParticleSystem()
    {
        if (deathParticles != null)
        {
            // Stop and clear any existing particles
            deathParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            deathParticles.Clear();
            
            // Clear any pending bursts
            var emission = deathParticles.emission;
            emission.SetBursts(new ParticleSystem.Burst[0]);
        }
        
        // Reset the playing flag
        isPlayingEffect = false;
        
        // Cancel any pending ResetPlayingFlag invokes
        CancelInvoke(nameof(ResetPlayingFlag));
        
        Debug.Log("Particle system fully reset and cleaned");
    }
}