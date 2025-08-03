using UnityEngine;

public class MainMenuMusic : MonoBehaviour
{
    [Header("Audio Settings")]
    [Tooltip("The audio clip to play as background music")]
    public AudioClip backgroundMusic;
    
    [Tooltip("The AudioSource component (leave empty to auto-create)")]
    public AudioSource audioSource;
    
    [Header("Music Properties")]
    [Tooltip("Volume of the background music")]
    [Range(0f, 1f)]
    public float volume = 0.5f;
    
    [Tooltip("Should the music loop continuously?")]
    public bool loop = true;
    
    [Tooltip("Should music start playing automatically when enabled?")]
    public bool playOnStart = true;
    
    [Tooltip("Fade in duration when starting music")]
    [Range(0f, 5f)]
    public float fadeInDuration = 1f;
    
    [Tooltip("Fade out duration when stopping music")]
    [Range(0f, 5f)]
    public float fadeOutDuration = 1f;
    
    [Header("Persistence")]
    [Tooltip("Should this music persist when loading other scenes?")]
    public bool dontDestroyOnLoad = false;
    
    private float targetVolume;
    private bool isFading = false;
    private float fadeTimer = 0f;
    private float fadeDuration = 0f;
    private float startVolume = 0f;
    
    void Awake()
    {
        // Setup AudioSource if not assigned
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        // Configure AudioSource
        SetupAudioSource();
        
        // Handle persistence
        if (dontDestroyOnLoad)
        {
            DontDestroyOnLoad(gameObject);
        }
    }
    
    void Start()
    {
        if (playOnStart && backgroundMusic != null)
        {
            PlayMusic();
        }
    }
    
    void Update()
    {
        // Handle fading
        if (isFading)
        {
            fadeTimer += Time.deltaTime;
            float t = fadeTimer / fadeDuration;
            
            if (t >= 1f)
            {
                // Fade complete
                audioSource.volume = targetVolume;
                isFading = false;
                
                // Stop audio if we were fading out to silence
                if (targetVolume <= 0f)
                {
                    audioSource.Stop();
                }
            }
            else
            {
                // Continue fading
                audioSource.volume = Mathf.Lerp(startVolume, targetVolume, t);
            }
        }
    }
    
    private void SetupAudioSource()
    {
        if (audioSource != null)
        {
            audioSource.clip = backgroundMusic;
            audioSource.loop = loop;
            audioSource.volume = volume;
            audioSource.playOnAwake = false;
            
            // Set 3D audio properties for UI music
            audioSource.spatialBlend = 0f; // 2D audio
            audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        }
    }
    
    public void PlayMusic()
    {
        if (backgroundMusic == null)
        {
            Debug.LogWarning("MainMenuMusic: No background music clip assigned!");
            return;
        }
        
        if (audioSource == null)
        {
            Debug.LogError("MainMenuMusic: No AudioSource component found!");
            return;
        }
        
        // Setup the audio source
        SetupAudioSource();
        
        // Start playing
        audioSource.Play();
        
        // Fade in if specified
        if (fadeInDuration > 0f)
        {
            FadeIn();
        }
        else
        {
            audioSource.volume = volume;
        }
    }
    
    public void StopMusic()
    {
        if (audioSource == null || !audioSource.isPlaying)
            return;
        
        // Fade out if specified
        if (fadeOutDuration > 0f)
        {
            FadeOut();
        }
        else
        {
            audioSource.Stop();
        }
    }
    
    public void PauseMusic()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Pause();
        }
    }
    
    public void ResumeMusic()
    {
        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.UnPause();
        }
    }
    
    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        
        if (audioSource != null && !isFading)
        {
            audioSource.volume = volume;
        }
    }
    
    public void FadeIn()
    {
        if (audioSource == null)
            return;
        
        startVolume = 0f;
        targetVolume = volume;
        audioSource.volume = startVolume;
        
        StartFade(fadeInDuration);
    }
    
    public void FadeOut()
    {
        if (audioSource == null)
            return;
        
        startVolume = audioSource.volume;
        targetVolume = 0f;
        
        StartFade(fadeOutDuration);
    }
    
    private void StartFade(float duration)
    {
        fadeDuration = duration;
        fadeTimer = 0f;
        isFading = true;
    }
    
    public void ChangeMusic(AudioClip newClip)
    {
        if (newClip == null)
            return;
        
        bool wasPlaying = audioSource != null && audioSource.isPlaying;
        
        if (wasPlaying)
        {
            StopMusic();
        }
        
        backgroundMusic = newClip;
        SetupAudioSource();
        
        if (wasPlaying)
        {
            PlayMusic();
        }
    }
    
    // Utility methods for external scripts
    public bool IsPlaying()
    {
        return audioSource != null && audioSource.isPlaying;
    }
    
    public float GetCurrentVolume()
    {
        return audioSource != null ? audioSource.volume : 0f;
    }
    
    public float GetMusicLength()
    {
        return backgroundMusic != null ? backgroundMusic.length : 0f;
    }
    
    public float GetCurrentTime()
    {
        return audioSource != null ? audioSource.time : 0f;
    }
    
    void OnValidate()
    {
        // Update volume in real-time during inspector changes
        if (audioSource != null && !isFading)
        {
            audioSource.volume = volume;
        }
    }
}