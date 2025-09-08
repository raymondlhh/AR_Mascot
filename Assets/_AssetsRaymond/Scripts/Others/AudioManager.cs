using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("SFX Audio Elements")]
    [SerializeField] private List<AudioElement> sfxElements = new List<AudioElement>();
    
    [Header("BGM Audio Elements")]
    [SerializeField] private List<AudioElement> bgmElements = new List<AudioElement>();
    
    [Header("Narration Audio Elements")]
    [SerializeField] private List<AudioElement> narrationElements = new List<AudioElement>();
    
    [Header("Voiceovers Audio Elements")]
    [SerializeField] private List<AudioElement> voiceoverElements = new List<AudioElement>();
    
    [Header("UI Audio Elements")]
    [SerializeField] private List<AudioElement> uiElements = new List<AudioElement>();
    
    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource narrationSource;
    [SerializeField] private AudioSource voiceoverSource;
    [SerializeField] private AudioSource uiSource;
    
    [Header("Global Volume Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float masterVolume = 1f;
    [Range(0f, 1f)]
    [SerializeField] private float sfxVolume = 1f;
    [Range(0f, 1f)]
    [SerializeField] private float bgmVolume = 1f;
    [Range(0f, 1f)]
    [SerializeField] private float narrationVolume = 1f;
    [Range(0f, 1f)]
    [SerializeField] private float voiceoverVolume = 1f;
    [Range(0f, 1f)]
    [SerializeField] private float uiVolume = 1f;

    #region Unity Lifecycle
    void Start()
    {
        InitializeAudioSources();
    }
    #endregion

    #region Initialization
    void InitializeAudioSources()
    {
        // Get or create SFX AudioSource
        if (sfxSource == null)
        {
            // First try to find SFX AudioSource in children
            Transform sfxChild = transform.Find("SFX");
            if (sfxChild != null)
            {
                sfxSource = sfxChild.GetComponent<AudioSource>();
            }
            
            // If not found in children, try the main GameObject
            if (sfxSource == null)
            {
                sfxSource = GetComponent<AudioSource>();
            }
            
            // If still not found, create one
            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        // Get or create BGM AudioSource
        if (bgmSource == null)
        {
            // Look for BGM AudioSource in children
            Transform bgmChild = transform.Find("BGM");
            if (bgmChild != null)
            {
                bgmSource = bgmChild.GetComponent<AudioSource>();
            }
            
            // If not found, try to find any AudioSource that's not the SFX source
            if (bgmSource == null)
            {
                AudioSource[] allSources = GetComponentsInChildren<AudioSource>();
                foreach (AudioSource source in allSources)
                {
                    if (source != sfxSource)
                    {
                        bgmSource = source;
                        break;
                    }
                }
            }
            
            // If still not found, create a new GameObject for BGM
            if (bgmSource == null)
            {
                GameObject bgmObject = new GameObject("BGM AudioSource");
                bgmObject.transform.SetParent(transform);
                bgmSource = bgmObject.AddComponent<AudioSource>();
            }
        }
        
        // Get or create Narration AudioSource
        if (narrationSource == null)
        {
            // Look for Narration AudioSource in children
            Transform narrationChild = transform.Find("Narration");
            if (narrationChild != null)
            {
                narrationSource = narrationChild.GetComponent<AudioSource>();
            }
            
            // If not found, try to find any AudioSource that's not SFX or BGM
            if (narrationSource == null)
            {
                AudioSource[] allSources = GetComponentsInChildren<AudioSource>();
                foreach (AudioSource source in allSources)
                {
                    if (source != sfxSource && source != bgmSource)
                    {
                        narrationSource = source;
                        break;
                    }
                }
            }
            
            if (narrationSource == null)
            {
                // Create a new GameObject for Narration
                GameObject narrationObject = new GameObject("Narration AudioSource");
                narrationObject.transform.SetParent(transform);
                narrationSource = narrationObject.AddComponent<AudioSource>();
            }
        }
        
        // Get or create Voiceover AudioSource
        if (voiceoverSource == null)
        {
            // Look for Voiceover AudioSource in children
            Transform voiceoverChild = transform.Find("Voiceovers");
            if (voiceoverChild != null)
            {
                voiceoverSource = voiceoverChild.GetComponent<AudioSource>();
            }
            
            // If not found, try to find any AudioSource that's not SFX, BGM, or Narration
            if (voiceoverSource == null)
            {
                AudioSource[] allSources = GetComponentsInChildren<AudioSource>();
                foreach (AudioSource source in allSources)
                {
                    if (source != sfxSource && source != bgmSource && source != narrationSource)
                    {
                        voiceoverSource = source;
                        break;
                    }
                }
            }
            
            if (voiceoverSource == null)
            {
                // Create a new GameObject for Voiceover
                GameObject voiceoverObject = new GameObject("Voiceover AudioSource");
                voiceoverObject.transform.SetParent(transform);
                voiceoverSource = voiceoverObject.AddComponent<AudioSource>();
            }
        }
        
        // Get or create UI AudioSource
        if (uiSource == null)
        {
            // Look for UI AudioSource in children
            Transform uiChild = transform.Find("UI");
            if (uiChild != null)
            {
                uiSource = uiChild.GetComponent<AudioSource>();
            }
            
            // If not found, try to find any AudioSource that's not SFX, BGM, Narration, or Voiceover
            if (uiSource == null)
            {
                AudioSource[] allSources = GetComponentsInChildren<AudioSource>();
                foreach (AudioSource source in allSources)
                {
                    if (source != sfxSource && source != bgmSource && source != narrationSource && source != voiceoverSource)
                    {
                        uiSource = source;
                        break;
                    }
                }
            }
            
            if (uiSource == null)
            {
                // Create a new GameObject for UI
                GameObject uiObject = new GameObject("UI AudioSource");
                uiObject.transform.SetParent(transform);
                uiSource = uiObject.AddComponent<AudioSource>();
            }
        }
        
        // Configure BGM source for looping
        bgmSource.loop = true;
        
        // Configure Narration source (no looping by default)
        narrationSource.loop = false;
        
        // Configure Voiceover source (no looping by default)
        voiceoverSource.loop = false;
        
        // Configure UI source (no looping by default)
        uiSource.loop = false;
        
        // Apply initial volume settings
        UpdateVolumeSettings();
        
        // Debug: Log which AudioSources were found
        Debug.Log($"AudioManager: SFX Source = {(sfxSource != null ? sfxSource.name : "NULL")}");
        Debug.Log($"AudioManager: BGM Source = {(bgmSource != null ? bgmSource.name : "NULL")}");
        Debug.Log($"AudioManager: Narration Source = {(narrationSource != null ? narrationSource.name : "NULL")}");
        Debug.Log($"AudioManager: Voiceover Source = {(voiceoverSource != null ? voiceoverSource.name : "NULL")}");
        Debug.Log($"AudioManager: UI Source = {(uiSource != null ? uiSource.name : "NULL")}");
    }
    #endregion

    #region Volume Management
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
        if (narrationSource != null)
        {
            narrationSource.volume = masterVolume * narrationVolume;
        }
        if (voiceoverSource != null)
        {
            voiceoverSource.volume = masterVolume * voiceoverVolume;
        }
        if (uiSource != null)
        {
            uiSource.volume = masterVolume * uiVolume;
        }
    }
    #endregion

    #region SFX Methods
    public void PlaySFX(string audioName)
    {
        AudioElement element = sfxElements.Find(x => x.audioName == audioName);
        if (element != null && element.audioFile != null && sfxSource != null)
        {
            ApplyAudioElementSettings(sfxSource, element);
            sfxSource.clip = element.audioFile;
            sfxSource.volume = element.volume;
            sfxSource.Play();
            Debug.Log($"Playing SFX: {audioName} on {sfxSource.name} with volume {element.volume} - Clip assigned: {element.audioFile.name}");
        }
        else
        {
            Debug.LogWarning($"SFX '{audioName}' not found or has no audio file assigned. Element: {(element != null ? "Found" : "NULL")}, AudioFile: {(element != null && element.audioFile != null ? element.audioFile.name : "NULL")}, SFXSource: {(sfxSource != null ? sfxSource.name : "NULL")}");
        }
    }
    
    public void PlaySFXOneShot(string audioName)
    {
        AudioElement element = sfxElements.Find(x => x.audioName == audioName);
        if (element != null && element.audioFile != null && sfxSource != null)
        {
            ApplyAudioElementSettings(sfxSource, element);
            sfxSource.PlayOneShot(element.audioFile, element.volume);
            Debug.Log($"Playing SFX OneShot: {audioName} on {sfxSource.name} with volume {element.volume}");
        }
        else
        {
            Debug.LogWarning($"SFX '{audioName}' not found or has no audio file assigned.");
        }
    }
    
    public void StopSFX()
    {
        if (sfxSource != null)
        {
            sfxSource.Stop();
            Debug.Log("SFX stopped");
        }
    }
    
    public bool IsSFXPlaying()
    {
        return sfxSource != null && sfxSource.isPlaying;
    }
    
    
    #endregion

    #region BGM Methods
    public void PlayBGM(string audioName)
    {
        AudioElement element = bgmElements.Find(x => x.audioName == audioName);
        if (element != null && element.audioFile != null && bgmSource != null)
        {
            ApplyAudioElementSettings(bgmSource, element);
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
    #endregion

    #region Narration Methods
    public void PlayNarration(string audioName)
    {
        AudioElement element = narrationElements.Find(x => x.audioName == audioName);
        if (element != null && element.audioFile != null && narrationSource != null)
        {
            ApplyAudioElementSettings(narrationSource, element);
            narrationSource.clip = element.audioFile;
            narrationSource.volume = element.volume;
            narrationSource.Play();
            Debug.Log($"Playing Narration: {audioName} on {narrationSource.name} with volume {element.volume} - Clip assigned: {element.audioFile.name}");
        }
        else
        {
            Debug.LogWarning($"Narration '{audioName}' not found or has no audio file assigned. Element: {(element != null ? "Found" : "NULL")}, AudioFile: {(element != null && element.audioFile != null ? element.audioFile.name : "NULL")}, NarrationSource: {(narrationSource != null ? narrationSource.name : "NULL")}");
        }
    }
    
    public void StopNarration()
    {
        if (narrationSource != null)
        {
            narrationSource.Stop();
        }
    }
    
    public void PauseNarration()
    {
        if (narrationSource != null)
        {
            narrationSource.Pause();
        }
    }
    
    public void ResumeNarration()
    {
        if (narrationSource != null)
        {
            narrationSource.UnPause();
        }
    }
    
    public bool IsNarrationPlaying()
    {
        return narrationSource != null && narrationSource.isPlaying;
    }
    
    public void PlayNarrationElement(AudioElement element)
    {
        if (element != null && element.audioFile != null && narrationSource != null)
        {
            ApplyAudioElementSettings(narrationSource, element);
            narrationSource.clip = element.audioFile;
            narrationSource.volume = element.volume;
            narrationSource.Play();
        }
        else
        {
            Debug.LogWarning($"Narration element is null or has no audio file assigned.");
        }
    }
    #endregion

    #region Voiceover Methods
    public void PlayVoiceover(string audioName)
    {
        AudioElement element = voiceoverElements.Find(x => x.audioName == audioName);
        if (element != null && element.audioFile != null && voiceoverSource != null)
        {
            ApplyAudioElementSettings(voiceoverSource, element);
            voiceoverSource.Play();
        }
        else
        {
            Debug.LogWarning($"Voiceover '{audioName}' not found or has no audio file assigned.");
        }
    }
    
    public void StopVoiceover()
    {
        if (voiceoverSource != null)
        {
            voiceoverSource.Stop();
        }
    }
    
    public void PauseVoiceover()
    {
        if (voiceoverSource != null)
        {
            voiceoverSource.Pause();
        }
    }
    
    public void ResumeVoiceover()
    {
        if (voiceoverSource != null)
        {
            voiceoverSource.UnPause();
        }
    }
    
    public bool IsVoiceoverPlaying()
    {
        return voiceoverSource != null && voiceoverSource.isPlaying;
    }
    #endregion

    #region UI Methods
    public void PlayUI(string audioName)
    {
        AudioElement element = uiElements.Find(x => x.audioName == audioName);
        if (element != null && element.audioFile != null && uiSource != null)
        {
            ApplyAudioElementSettings(uiSource, element);
            uiSource.clip = element.audioFile;
            uiSource.volume = element.volume;
            uiSource.Play();
            Debug.Log($"Playing UI: {audioName} on {uiSource.name} with volume {element.volume} - Clip assigned: {element.audioFile.name}");
        }
        else
        {
            Debug.LogWarning($"UI '{audioName}' not found or has no audio file assigned. Element: {(element != null ? "Found" : "NULL")}, AudioFile: {(element != null && element.audioFile != null ? element.audioFile.name : "NULL")}, UISource: {(uiSource != null ? uiSource.name : "NULL")}");
        }
    }
    
    public void PlayUIOneShot(string audioName)
    {
        AudioElement element = uiElements.Find(x => x.audioName == audioName);
        if (element != null && element.audioFile != null && uiSource != null)
        {
            ApplyAudioElementSettings(uiSource, element);
            uiSource.PlayOneShot(element.audioFile, element.volume);
            Debug.Log($"Playing UI OneShot: {audioName} on {uiSource.name} with volume {element.volume}");
        }
        else
        {
            Debug.LogWarning($"UI '{audioName}' not found or has no audio file assigned.");
        }
    }
    
    public void StopUI()
    {
        if (uiSource != null)
        {
            uiSource.Stop();
            Debug.Log("UI stopped");
        }
    }
    
    public void PauseUI()
    {
        if (uiSource != null)
        {
            uiSource.Pause();
        }
    }
    
    public void ResumeUI()
    {
        if (uiSource != null)
        {
            uiSource.UnPause();
        }
    }
    
    public bool IsUIPlaying()
    {
        return uiSource != null && uiSource.isPlaying;
    }
    #endregion

    #region Utility Methods
    public AudioElement GetSFXElement(string audioName)
    {
        return sfxElements.Find(x => x.audioName == audioName);
    }
    
    public AudioElement GetBGMElement(string audioName)
    {
        return bgmElements.Find(x => x.audioName == audioName);
    }
    
    public AudioElement GetNarrationElement(string audioName)
    {
        return narrationElements.Find(x => x.audioName == audioName);
    }
    
    public AudioElement GetNarrationElementByIndex(int index)
    {
        if (index >= 0 && index < narrationElements.Count)
        {
            return narrationElements[index];
        }
        return null;
    }
    
    public AudioElement GetVoiceoverElement(string audioName)
    {
        return voiceoverElements.Find(x => x.audioName == audioName);
    }
    
    public AudioElement GetUIElement(string audioName)
    {
        return uiElements.Find(x => x.audioName == audioName);
    }
    #endregion

    #region Audio Element Management
    // Method to apply audio element settings to an audio source
    private void ApplyAudioElementSettings(AudioSource source, AudioElement element)
    {
        if (source == null) return;
        
        source.spatialBlend = element.spatialBlend;
        
        // Apply 3D sound settings if spatial blend is greater than 0
        if (element.spatialBlend > 0f)
        {
            source.minDistance = element.minDistance;
            source.maxDistance = element.maxDistance;
            source.rolloffMode = element.rolloffMode;
        }
    }
    
    public void AddSFXElement(string name, AudioClip clip, float volume = 1f, float spatialBlend = 0f)
    {
        AudioElement newElement = new AudioElement
        {
            audioName = name,
            audioFile = clip,
            volume = Mathf.Clamp01(volume),
            spatialBlend = Mathf.Clamp01(spatialBlend)
        };
        sfxElements.Add(newElement);
    }
    
    public void AddBGMElement(string name, AudioClip clip, float volume = 1f, float spatialBlend = 0f)
    {
        AudioElement newElement = new AudioElement
        {
            audioName = name,
            audioFile = clip,
            volume = Mathf.Clamp01(volume),
            spatialBlend = Mathf.Clamp01(spatialBlend)
        };
        bgmElements.Add(newElement);
    }
    
    public void AddNarrationElement(string name, AudioClip clip, float volume = 1f, float spatialBlend = 0f)
    {
        AudioElement newElement = new AudioElement
        {
            audioName = name,
            audioFile = clip,
            volume = Mathf.Clamp01(volume),
            spatialBlend = Mathf.Clamp01(spatialBlend)
        };
        narrationElements.Add(newElement);
    }
    
    public void AddVoiceoverElement(string name, AudioClip clip, float volume = 1f, float spatialBlend = 0f)
    {
        AudioElement newElement = new AudioElement
        {
            audioName = name,
            audioFile = clip,
            volume = Mathf.Clamp01(volume),
            spatialBlend = Mathf.Clamp01(spatialBlend)
        };
        voiceoverElements.Add(newElement);
    }
    
    public void AddUIElement(string name, AudioClip clip, float volume = 1f, float spatialBlend = 0f)
    {
        AudioElement newElement = new AudioElement
        {
            audioName = name,
            audioFile = clip,
            volume = Mathf.Clamp01(volume),
            spatialBlend = Mathf.Clamp01(spatialBlend)
        };
        uiElements.Add(newElement);
    }
    #endregion

    #region Volume Control Methods
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
    
    public void SetNarrationVolume(float volume)
    {
        narrationVolume = Mathf.Clamp01(volume);
        UpdateVolumeSettings();
    }
    
    public void SetVoiceoverVolume(float volume)
    {
        voiceoverVolume = Mathf.Clamp01(volume);
        UpdateVolumeSettings();
    }
    
    public void SetUIVolume(float volume)
    {
        uiVolume = Mathf.Clamp01(volume);
        UpdateVolumeSettings();
    }
    
    public float GetMasterVolume() => masterVolume;
    public float GetSFXVolume() => sfxVolume;
    public float GetBGMVolume() => bgmVolume;
    public float GetNarrationVolume() => narrationVolume;
    public float GetVoiceoverVolume() => voiceoverVolume;
    public float GetUIVolume() => uiVolume;
    #endregion
}
