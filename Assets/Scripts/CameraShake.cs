using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    [Header("Shake Settings")]
    [SerializeField] private float landingShakeIntensity = 0.1f;
    [SerializeField] private float landingShakeDuration = 0.2f;
    [SerializeField] private float collisionShakeIntensity = 0.2f;
    [SerializeField] private float collisionShakeDuration = 0.3f;
    [SerializeField] private float shakeDecayRate = 2f; // How fast shake intensity decreases over time
    
    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    
    // Private variables
    private Vector3 originalPosition;
    private bool isShaking = false;
    private Coroutine shakeCoroutine;

    void Start()
    {
        // Get camera transform if not assigned
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main != null ? Camera.main.transform : transform;
        }
        
        originalPosition = cameraTransform.localPosition;
        
        // Subscribe to collision events
        SubscribeToEvents();
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        UnsubscribeFromEvents();
    }
    
    private void SubscribeToEvents()
    {
        // Subscribe to collision/death events
        GameEvents.OnPlayer1Died += TriggerCollisionShake;
        GameEvents.OnPlayer2Died += TriggerCollisionShake;
        GameEvents.OnPlayer2Victory += TriggerCollisionShake; // When player gets shot
    }
    
    private void UnsubscribeFromEvents()
    {
        // Unsubscribe from events
        GameEvents.OnPlayer1Died -= TriggerCollisionShake;
        GameEvents.OnPlayer2Died -= TriggerCollisionShake;
        GameEvents.OnPlayer2Victory -= TriggerCollisionShake;
    }
    
    public void TriggerLandingShake()
    {
        TriggerShake(landingShakeIntensity, landingShakeDuration);
    }
    
    public void TriggerCollisionShake()
    {
        TriggerShake(collisionShakeIntensity, collisionShakeDuration);
    }
    
    public void TriggerShake(float intensity, float duration)
    {
        // If already shaking, stop the current shake
        if (isShaking && shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }
        
        // Start new shake
        shakeCoroutine = StartCoroutine(ShakeCoroutine(intensity, duration));
    }
    
    private IEnumerator ShakeCoroutine(float intensity, float duration)
    {
        isShaking = true;
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            // Calculate current shake intensity (decreases over time)
            float currentIntensity = intensity * (1f - (elapsedTime / duration));
            currentIntensity = Mathf.Pow(currentIntensity, shakeDecayRate);
            
            // Generate random shake offset
            Vector3 shakeOffset = new Vector3(
                Random.Range(-currentIntensity, currentIntensity),
                Random.Range(-currentIntensity, currentIntensity),
                0f
            );
            
            // Apply shake to camera position
            cameraTransform.localPosition = originalPosition + shakeOffset;
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Reset camera to original position
        cameraTransform.localPosition = originalPosition;
        isShaking = false;
        shakeCoroutine = null;
    }
    
    // Public methods for external scripts to trigger shakes
    public void ShakeLanding()
    {
        TriggerLandingShake();
    }
    
    public void ShakeCollision()
    {
        TriggerCollisionShake();
    }
    
    public void ShakeCustom(float intensity, float duration)
    {
        TriggerShake(intensity, duration);
    }
}