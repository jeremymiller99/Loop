using System.Collections;
using UnityEngine;
using TMPro;

public class TextColorChanger : MonoBehaviour
{
    [Header("Text Settings")]
    [Tooltip("The TextMeshPro component to change colors (leave empty to auto-find)")]
    public TextMeshProUGUI textComponent;
    
    [Header("Color Settings")]
    [Tooltip("Time between color changes in seconds")]
    [Range(0.1f, 5f)]
    public float colorChangeInterval = 1f;
    
    [Tooltip("Should colors fade smoothly or change instantly?")]
    public bool smoothTransition = true;
    
    [Tooltip("Duration of smooth transition (only used if smoothTransition is true)")]
    [Range(0.1f, 2f)]
    public float transitionDuration = 0.5f;
    
    [Header("Color Options")]
    [Tooltip("Colors to randomly choose from")]
    public Color[] colors = new Color[]
    {
        Color.red,
        Color.green,
        Color.blue,
        Color.yellow,
        Color.magenta,
        Color.cyan,
        Color.white
    };
    
    private Coroutine colorChangeCoroutine;
    
    void Start()
    {
        // Auto-find TextMeshPro component if not assigned
        if (textComponent == null)
        {
            textComponent = GetComponentInChildren<TextMeshProUGUI>();
            
            if (textComponent == null)
            {
                Debug.LogError("TextColorChanger: No TextMeshProUGUI component found! Please assign one in the inspector or make sure there's one in children.");
                return;
            }
        }
        
        // Validate colors array
        if (colors == null || colors.Length == 0)
        {
            Debug.LogWarning("TextColorChanger: No colors defined! Using default colors.");
            SetupDefaultColors();
        }
        
        // Start the color changing coroutine
        StartColorChanging();
    }
    
    void OnEnable()
    {
        if (textComponent != null)
        {
            StartColorChanging();
        }
    }
    
    void OnDisable()
    {
        StopColorChanging();
    }
    
    public void StartColorChanging()
    {
        if (colorChangeCoroutine != null)
        {
            StopCoroutine(colorChangeCoroutine);
        }
        
        colorChangeCoroutine = StartCoroutine(ChangeColorsCoroutine());
    }
    
    public void StopColorChanging()
    {
        if (colorChangeCoroutine != null)
        {
            StopCoroutine(colorChangeCoroutine);
            colorChangeCoroutine = null;
        }
    }
    
    private IEnumerator ChangeColorsCoroutine()
    {
        while (true)
        {
            // Get a random color
            Color newColor = GetRandomColor();
            
            if (smoothTransition && textComponent != null)
            {
                // Smooth color transition
                Color startColor = textComponent.color;
                float elapsedTime = 0f;
                
                while (elapsedTime < transitionDuration)
                {
                    elapsedTime += Time.deltaTime;
                    float t = elapsedTime / transitionDuration;
                    textComponent.color = Color.Lerp(startColor, newColor, t);
                    yield return null;
                }
                
                textComponent.color = newColor;
            }
            else if (textComponent != null)
            {
                // Instant color change
                textComponent.color = newColor;
            }
            
            yield return new WaitForSeconds(colorChangeInterval);
        }
    }
    
    private Color GetRandomColor()
    {
        if (colors == null || colors.Length == 0)
        {
            return Color.white;
        }
        
        int randomIndex = Random.Range(0, colors.Length);
        return colors[randomIndex];
    }
    
    private void SetupDefaultColors()
    {
        colors = new Color[]
        {
            Color.red,
            Color.green,
            Color.blue,
            Color.yellow,
            Color.magenta,
            Color.cyan,
            Color.white
        };
    }
    
    // Public method to manually change color (useful for testing or special events)
    public void ChangeColorNow()
    {
        if (textComponent != null)
        {
            textComponent.color = GetRandomColor();
        }
    }
    
    // Public method to set specific color
    public void SetColor(Color color)
    {
        if (textComponent != null)
        {
            textComponent.color = color;
        }
    }
}