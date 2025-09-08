using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Audio2D3DAnimation : MonoBehaviour
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
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    // Play audio for props appearing
    public void PlayAudioPropsAppeared()
    {
        if (audioManager != null)
        {
            audioManager.PlaySFX("Appear");
        }
        else
        {
            Debug.LogWarning("AudioManager not found! Cannot play PropsAppeared sound.");
        }
    }
    
    
}
