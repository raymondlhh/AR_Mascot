using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropsAudio : MonoBehaviour
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

    // Play pen scribble sound
    public void PlayAudioPenScribble()
    {
        if (audioManager != null)
        {
            audioManager.PlaySFX("PenScribble");
        }
        else
        {
            Debug.LogWarning("AudioManager not found! Cannot play PenScribble sound.");
        }
    }

    public void PlayAudioSlime()
    {
        if (audioManager != null)
        {
            audioManager.PlaySFX("Slime");
        }
        else
        {
            Debug.LogWarning("AudioManager not found! Cannot play Slime sound.");
        }
    }

    public void PlayAudioItemPutDown()
    {
        if (audioManager != null)
        {
            audioManager.PlaySFX("ItemPutDown");
        }
        else
        {
            Debug.LogWarning("AudioManager not found! Cannot play ItemPutDown sound.");
        }
    }

    public void PlayAudioCorrect()
    {
        if (audioManager != null)
        {
            audioManager.PlaySFX("Correct");
        }
        else
        {
            Debug.LogWarning("AudioManager not found! Cannot play Correct sound.");
        }
    }

    public void PlayAudioWrong()
    {
        if (audioManager != null)
        {
            audioManager.PlaySFX("Wrong");
        }
        else
        {
            Debug.LogWarning("AudioManager not found! Cannot play Wrong sound.");
        }
    }

    public void PlayAudioMagicAttack()
    {
        if (audioManager != null)
        {
            audioManager.PlaySFX("MagicAttack");
        }
        else
        {
            Debug.LogWarning("AudioManager not found! Cannot play MaggicAttack sound.");
        }
    }

    public void PlayAudioChair()
    {
        if (audioManager != null)
        {
            audioManager.PlaySFX("Chair");
        }
        else
        {
            Debug.LogWarning("AudioManager not found! Cannot play Chair sound.");
        }
    }

    public void PlayBGMForScene()
    {
        if (audioManager != null)
        {
            audioManager.PlaySFX("Chair");
        }
        else
        {
            Debug.LogWarning("AudioManager not found! Cannot play Chair sound.");
        }
    }

    public void PlayGameMusic()
    {
        if (audioManager != null)
        {
            audioManager.PlayBGM("GameMusic");
        }
        else
        {
            Debug.LogWarning("AudioManager not found! Cannot play GameMusic sound.");
        }
    }

    public void DecreaseBGM()
    {
        if (audioManager != null)
        {
            audioManager.DecreaseBGM();
        }
        else
        {
            Debug.LogWarning("AudioManager not found! Cannot play GameMusic sound.");
        }
    }

    public void PlayAudioDing()
    {
        if (audioManager != null)
        {
            audioManager.PlaySFX("Ding");
        }
        else
        {
            Debug.LogWarning("AudioManager not found! Cannot play Ding sound.");
        }
    }

    public void PlayAudioThrow()
    {
        if (audioManager != null)
        {
            audioManager.PlaySFX("Throw");
        }
        else
        {
            Debug.LogWarning("AudioManager not found! Cannot play Throw sound.");
        }
    }

    public void PlayAudioRecord()
    {
        if (audioManager != null)
        {
            audioManager.PlaySFX("Record");
        }
        else
        {
            Debug.LogWarning("AudioManager not found! Cannot play Record sound.");
        }
    }
    
    // Stop pen scribble sound
    public void StopSFX()
    {
        if (audioManager != null)
        {
            audioManager.StopSFX();
        }
        else
        {
            Debug.LogWarning("AudioManager not found! Cannot stop SFX.");
        }
    }
}