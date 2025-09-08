using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioFYP : MonoBehaviour
{
    [Header("Audio Manager Reference")]
    [SerializeField] private AudioManager audioManager;
    
    // Start is called before the first frame update
    void Start()
    {
        // Find AudioManager if not assigned
        if (audioManager == null)
        {
            audioManager = FindObjectOfType<AudioManager>();
            if (audioManager == null)
            {
                Debug.LogError("AudioManager not found! Please assign it in the inspector or ensure there's an AudioManager in the scene.");
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    /// <summary>
    /// Plays the Pickup audio effect
    /// </summary>
    public void PlayAudioWinning()
    {
        if (audioManager != null)
        {
            audioManager.PlaySFX("Winning");
        }
        else
        {
            Debug.LogWarning("AudioManager is not assigned or found!");
        }
    }
    
    /// <summary>
    /// Plays the Pop audio effect
    /// </summary>
    public void PlayAudioPop()
    {
        if (audioManager != null)
        {
            audioManager.PlaySFX("Pop");
        }
        else
        {
            Debug.LogWarning("AudioManager is not assigned or found!");
        }
    }
    
    /// <summary>
    /// Plays the Stretch audio effect
    /// </summary>
    public void PlayAudioStretch()
    {
        if (audioManager != null)
        {
            audioManager.PlaySFX("Stretch");
        }
        else
        {
            Debug.LogWarning("AudioManager is not assigned or found!");
        }
    }
}
