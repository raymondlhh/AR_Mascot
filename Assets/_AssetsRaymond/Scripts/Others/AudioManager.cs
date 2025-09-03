using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("SFX Audio Elements")]
    [SerializeField] private List<AudioElement> sfxElements = new List<AudioElement>();
    
    [Header("BGM Audio Elements")]
    [SerializeField] private List<AudioElement> bgmElements = new List<AudioElement>();
    
    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource bgmSource;
    
    [Header("Global Volume Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float masterVolume = 1f;
    [Range(0f, 1f)]
    [SerializeField] private float sfxVolume = 1f;
    [Range(0f, 1f)]
    [SerializeField] private float bgmVolume = 1f;

    void Start()
    {
        InitializeAudioSources();
    }

    void InitializeAudioSources()
    {
        // Get or create SFX AudioSource
        if (sfxSource == null)
        {
            sfxSource = GetComponent<AudioSource>();
            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        // Get or create BGM AudioSource
        if (bgmSource == null)
        {
            // Look for BGM AudioSource in children
            bgmSource = GetComponentInChildren<AudioSource>();
            if (bgmSource == null || bgmSource == sfxSource)
            {
                // Create a new GameObject for BGM
                GameObject bgmObject = new GameObject("BGM AudioSource");
                bgmObject.transform.SetParent(transform);
                bgmSource = bgmObject.AddComponent<AudioSource>();
            }
        }
        
        // Configure BGM source for looping
        bgmSource.loop = true;
        
        // Apply initial volume settings
        UpdateVolumeSettings();
    }

    void UpdateVolumeSettings()
    {
        if (sfxSource != null)
        {
            sfxSource.volume = masterVolume * sfxVolume;
        }
        if (bgmSource != null)
        {
            bgmSource.volume = masterVolume * bgmVolume;
        }
    }

    // SFX Methods
    public void PlaySFX(string audioName)
    {
        AudioElement element = sfxElements.Find(x => x.audioName == audioName);
        if (element != null && element.audioFile != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(element.audioFile, element.volume);
        }
        else
        {
            Debug.LogWarning($"SFX '{audioName}' not found or has no audio file assigned.");
        }
    }
    
    public void PlaySFXOneShot(string audioName)
    {
        AudioElement element = sfxElements.Find(x => x.audioName == audioName);
        if (element != null && element.audioFile != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(element.audioFile, element.volume);
        }
        else
        {
            Debug.LogWarning($"SFX '{audioName}' not found or has no audio file assigned.");
        }
    }

    // BGM Methods
    public void PlayBGM(string audioName)
    {
        AudioElement element = bgmElements.Find(x => x.audioName == audioName);
        if (element != null && element.audioFile != null && bgmSource != null)
        {
            bgmSource.clip = element.audioFile;
            bgmSource.volume = element.volume;
            bgmSource.Play();
        }
        else
        {
            Debug.LogWarning($"BGM '{audioName}' not found or has no audio file assigned.");
        }
    }
    
    public void StopBGM()
    {
        if (bgmSource != null)
        {
            bgmSource.Stop();
        }
    }
    
    public void PauseBGM()
    {
        if (bgmSource != null)
        {
            bgmSource.Pause();
        }
    }
    
    public void ResumeBGM()
    {
        if (bgmSource != null)
        {
            bgmSource.UnPause();
        }
    }

    // Utility Methods
    public AudioElement GetSFXElement(string audioName)
    {
        return sfxElements.Find(x => x.audioName == audioName);
    }
    
    public AudioElement GetBGMElement(string audioName)
    {
        return bgmElements.Find(x => x.audioName == audioName);
    }
    
    public void AddSFXElement(string name, AudioClip clip, float volume = 1f)
    {
        AudioElement newElement = new AudioElement
        {
            audioName = name,
            audioFile = clip,
            volume = Mathf.Clamp01(volume)
        };
        sfxElements.Add(newElement);
    }
    
    public void AddBGMElement(string name, AudioClip clip, float volume = 1f)
    {
        AudioElement newElement = new AudioElement
        {
            audioName = name,
            audioFile = clip,
            volume = Mathf.Clamp01(volume)
        };
        bgmElements.Add(newElement);
    }
    
    // Volume Control Methods
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateVolumeSettings();
    }
    
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        UpdateVolumeSettings();
    }
    
    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        UpdateVolumeSettings();
    }
    
    public float GetMasterVolume() => masterVolume;
    public float GetSFXVolume() => sfxVolume;
    public float GetBGMVolume() => bgmVolume;
}
