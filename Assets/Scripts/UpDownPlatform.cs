using UnityEngine;

public class UpDownPlatform : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 2f;
    [SerializeField] private float distance = 3f;
    
    [Header("Movement Direction")]
    [SerializeField] private bool moveUpDown = true;
    [SerializeField] private bool moveLeftRight = false;
    
    private Vector3 startPosition;
    private float timeOffset;
    
    void Start()
    {
        startPosition = transform.position;
        timeOffset = Random.Range(0f, 2f * Mathf.PI); // Random start position
    }
    
    void Update()
    {
        float time = Time.time + timeOffset;
        
        if (moveUpDown)
        {
            // Move up and down using sine wave
            float newY = startPosition.y + Mathf.Sin(time * speed) * distance;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
        
        if (moveLeftRight)
        {
            // Move left and right using sine wave
            float newX = startPosition.x + Mathf.Sin(time * speed) * distance;
            transform.position = new Vector3(newX, transform.position.y, transform.position.z);
        }
    }
    
    // Optional: Visualize the movement range in the editor
    void OnDrawGizmosSelected()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            Vector3 currentPos = transform.position;
            
            if (moveUpDown)
            {
                Gizmos.DrawLine(currentPos + Vector3.up * distance, currentPos - Vector3.up * distance);
                Gizmos.DrawWireSphere(currentPos + Vector3.up * distance, 0.2f);
                Gizmos.DrawWireSphere(currentPos - Vector3.up * distance, 0.2f);
            }
            
            if (moveLeftRight)
            {
                Gizmos.DrawLine(currentPos + Vector3.right * distance, currentPos - Vector3.right * distance);
                Gizmos.DrawWireSphere(currentPos + Vector3.right * distance, 0.2f);
                Gizmos.DrawWireSphere(currentPos - Vector3.right * distance, 0.2f);
            }
        }
    }
} 