using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AudioElement
{
    public string audioName;
    public AudioClip audioFile;
    [Range(0f, 1f)]
    public float volume = 1f;
}

public class MascotAudio : MonoBehaviour
{
    [Header("Audio Elements")]
    [SerializeField] private List<AudioElement> audioElements = new List<AudioElement>();
    
    [Header("Appear Sound Settings")]
    [SerializeField] private string appearSoundName = "Appear";
    [SerializeField] private bool playAppearOnEnable = true;
    
    private AudioSource audioSource;
    private bool hasPlayedAppearSound = false;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    void OnEnable()
    {
        if (playAppearOnEnable && !hasPlayedAppearSound)
        {
            PlayAppearSound();
        }
    }
    
    // Method to play appear sound
    private void PlayAppearSound()
    {
        if (audioSource != null && !hasPlayedAppearSound)
        {
            AudioElement appearElement = audioElements.Find(x => x.audioName == appearSoundName);
            if (appearElement != null && appearElement.audioFile != null)
            {
                audioSource.clip = appearElement.audioFile;
                audioSource.volume = appearElement.volume;
                audioSource.Play();
                hasPlayedAppearSound = true;
                
                // Reset flag after a delay to allow re-triggering
                StartCoroutine(ResetAppearSoundFlag());
            }
            else
            {
                Debug.LogWarning($"Appear sound '{appearSoundName}' not found or has no audio file assigned.");
            }
        }
    }
    
    // Reset the appear sound flag after a delay
    private IEnumerator ResetAppearSoundFlag()
    {
        yield return new WaitForSeconds(0.5f);
        hasPlayedAppearSound = false;
    }
    
    // Method to play audio by name
    public void PlayAudioByName(string audioName)
    {
        AudioElement element = audioElements.Find(x => x.audioName == audioName);
        if (element != null && element.audioFile != null)
        {
            audioSource.clip = element.audioFile;
            audioSource.volume = element.volume;
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning($"Audio '{audioName}' not found or has no audio file assigned.");
        }
    }
    
    // Method to get audio element by name
    public AudioElement GetAudioElement(string audioName)
    {
        return audioElements.Find(x => x.audioName == audioName);
    }
    
    // Method to add new audio element
    public void AddAudioElement(string name, AudioClip clip, float volume = 1f)
    {
        AudioElement newElement = new AudioElement
        {
            audioName = name,
            audioFile = clip,
            volume = Mathf.Clamp01(volume)
        };
        audioElements.Add(newElement);
    }
    
    // Method to reset the appear sound flag manually
    public void ResetAppearSound()
    {
        hasPlayedAppearSound = false;
    }
}
