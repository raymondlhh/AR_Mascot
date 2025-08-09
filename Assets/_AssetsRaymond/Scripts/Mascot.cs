using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Mascot Animation Controller
/// 
/// This script manages the mascot's dancing animations and states.
/// It controls switching between different dance animations and the default "Look Around" animation.
/// 
/// Features:
/// - 8 different dance animations
/// - Animation state management (isDancing parameter)
/// - Automatic return to "Look Around" when animation ends
/// - Support for restarting animations when the same button is pressed
/// - Coroutine-based animation duration tracking
/// 
/// Animation List:
/// 0 - Chicken Dance
/// 1 - House Dancing  
/// 2 - Locking Hip Hop Dance
/// 3 - Northern Soul Spin Combo
/// 4 - Robot Hip Hop Dance
/// 5 - Swing Dancing
/// 6 - Tut Hip Hop Dance
/// 7 - Wave Hip Hop Dance
/// </summary>
public class Mascot : MonoBehaviour
{
    [Header("Animation Components")]
    public Animator animator;
    
    [Header("Animation Settings")]
    public float defaultAnimationDuration = 5f; // Default duration if we can't get actual length
    public bool debugMode = false;
    
    [Header("Animation Names")]
    public string[] danceAnimationNames = new string[]
    {
        "Chicken Dance",
        "House Dancing", 
        "Locking Hip Hop Dance",
        "Northern Soul Spin Combo",
        "Robot Hip Hop Dance",
        "Swing Dancing",
        "Tut Hip Hop Dance",
        "Wave Hip Hop Dance"
    };
    
    // Private variables
    private bool isDancing = false;
    private int currentDanceIndex = -1;
    private Coroutine currentDanceCoroutine;
    
    // Animation state hashes for performance
    private int isDancingHash;
    private Dictionary<string, int> animationHashes;
    
    void Start()
    {
        InitializeMascot();
    }
    
    void InitializeMascot()
    {
        // Auto-find animator if not assigned
        if (animator == null)
            animator = GetComponent<Animator>();
            
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
        
        if (animator == null)
        {
            Debug.LogError("Mascot: No Animator component found! Please assign an Animator.");
            return;
        }
        
        // Cache animation parameter hashes for performance
        isDancingHash = Animator.StringToHash("isDancing");
        
        // Initialize animation hashes
        animationHashes = new Dictionary<string, int>();
        foreach (string animName in danceAnimationNames)
        {
            animationHashes[animName] = Animator.StringToHash(animName);
        }
        
        // Ensure we start in the default state
        SetDancingState(false);
        
        if (debugMode)
            Debug.Log("Mascot initialized with " + danceAnimationNames.Length + " dance animations");
    }
    
    /// <summary>
    /// Plays a specific dance animation by index (0-7)
    /// </summary>
    /// <param name="danceIndex">Index of the dance animation (0-7)</param>
    public void PlayDanceAnimation(int danceIndex)
    {
        if (danceIndex < 0 || danceIndex >= danceAnimationNames.Length)
        {
            Debug.LogError($"Mascot: Invalid dance index {danceIndex}. Must be between 0 and {danceAnimationNames.Length - 1}");
            return;
        }
        
        if (animator == null)
        {
            Debug.LogError("Mascot: Animator is null!");
            return;
        }
        
        // If the same animation is already playing, restart it
        if (isDancing && currentDanceIndex == danceIndex)
        {
            if (debugMode)
                Debug.Log($"Mascot: Restarting {danceAnimationNames[danceIndex]} animation");
            RestartCurrentAnimation();
            return;
        }
        
        // Stop current animation if any
        if (currentDanceCoroutine != null)
        {
            StopCoroutine(currentDanceCoroutine);
            currentDanceCoroutine = null;
        }
        
        // Play the new animation
        currentDanceIndex = danceIndex;
        string animationName = danceAnimationNames[danceIndex];
        
        if (debugMode)
            Debug.Log($"Mascot: Playing {animationName} animation (index: {danceIndex})");
        
        // Set dancing state and play animation
        SetDancingState(true);
        PlaySpecificAnimation(animationName);
        
        // Start coroutine to handle animation completion
        currentDanceCoroutine = StartCoroutine(HandleAnimationCompletion(animationName));
    }
    
    /// <summary>
    /// Plays a specific dance animation by name
    /// </summary>
    /// <param name="animationName">Name of the animation to play</param>
    public void PlayDanceAnimation(string animationName)
    {
        int index = System.Array.IndexOf(danceAnimationNames, animationName);
        if (index >= 0)
        {
            PlayDanceAnimation(index);
        }
        else
        {
            Debug.LogError($"Mascot: Animation '{animationName}' not found in dance animation list");
        }
    }
    
    /// <summary>
    /// Stops the current dance and returns to Look Around
    /// </summary>
    public void StopDancing()
    {
        if (currentDanceCoroutine != null)
        {
            StopCoroutine(currentDanceCoroutine);
            currentDanceCoroutine = null;
        }
        
        SetDancingState(false);
        currentDanceIndex = -1;
        
        if (debugMode)
            Debug.Log("Mascot: Stopped dancing, returning to Look Around");
    }
    
    /// <summary>
    /// Restarts the currently playing animation
    /// </summary>
    public void RestartCurrentAnimation()
    {
        if (currentDanceIndex >= 0)
        {
            // Stop current coroutine
            if (currentDanceCoroutine != null)
            {
                StopCoroutine(currentDanceCoroutine);
                currentDanceCoroutine = null;
            }
            
            // Restart the animation
            string animationName = danceAnimationNames[currentDanceIndex];
            PlaySpecificAnimation(animationName);
            
            // Start new completion handler
            currentDanceCoroutine = StartCoroutine(HandleAnimationCompletion(animationName));
        }
    }
    
    /// <summary>
    /// Sets the isDancing parameter in the animator
    /// </summary>
    /// <param name="dancing">Whether the mascot should be dancing</param>
    private void SetDancingState(bool dancing)
    {
        if (animator != null)
        {
            animator.SetBool(isDancingHash, dancing);
            isDancing = dancing;
        }
    }
    
    /// <summary>
    /// Plays a specific animation state
    /// </summary>
    /// <param name="animationName">Name of the animation to play</param>
    private void PlaySpecificAnimation(string animationName)
    {
        if (animator != null && animationHashes.ContainsKey(animationName))
        {
            // Use CrossFadeInFixedTime for smooth transitions
            animator.CrossFadeInFixedTime(animationName, 0.25f);
        }
        else if (animator != null)
        {
            // Fallback to Play method
            animator.Play(animationName);
        }
    }
    
    /// <summary>
    /// Handles animation completion and returns to Look Around
    /// </summary>
    /// <param name="animationName">Name of the animation being handled</param>
    /// <returns></returns>
    private IEnumerator HandleAnimationCompletion(string animationName)
    {
        // Wait for animation to start playing
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        
        // Try to get the actual animation length
        float animationLength = GetAnimationLength(animationName);
        
        if (debugMode)
            Debug.Log($"Mascot: {animationName} will play for {animationLength} seconds");
        
        // Wait for animation to complete
        yield return new WaitForSeconds(animationLength);
        
        // Return to Look Around
        SetDancingState(false);
        currentDanceIndex = -1;
        currentDanceCoroutine = null;
        
        if (debugMode)
            Debug.Log($"Mascot: {animationName} completed, returning to Look Around");
    }
    
    /// <summary>
    /// Gets the length of a specific animation clip
    /// </summary>
    /// <param name="animationName">Name of the animation</param>
    /// <returns>Length in seconds</returns>
    private float GetAnimationLength(string animationName)
    {
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
            
            foreach (AnimationClip clip in clips)
            {
                if (clip.name.Contains(animationName) || animationName.Contains(clip.name))
                {
                    return clip.length;
                }
            }
        }
        
        // Fallback to default duration
        return defaultAnimationDuration;
    }
    
    /// <summary>
    /// Gets the current dancing state
    /// </summary>
    /// <returns>True if currently dancing</returns>
    public bool IsDancing()
    {
        return isDancing;
    }
    
    /// <summary>
    /// Gets the current dance index
    /// </summary>
    /// <returns>Current dance index (-1 if not dancing)</returns>
    public int GetCurrentDanceIndex()
    {
        return currentDanceIndex;
    }
    
    /// <summary>
    /// Gets the name of the currently playing dance
    /// </summary>
    /// <returns>Current dance name (null if not dancing)</returns>
    public string GetCurrentDanceName()
    {
        if (currentDanceIndex >= 0 && currentDanceIndex < danceAnimationNames.Length)
            return danceAnimationNames[currentDanceIndex];
        return null;
    }
    
    void Update()
    {
        // Optional: Add any update logic here if needed
        // For example, checking animation states or handling input
    }
    
    void OnDestroy()
    {
        // Clean up coroutines
        if (currentDanceCoroutine != null)
        {
            StopCoroutine(currentDanceCoroutine);
            currentDanceCoroutine = null;
        }
    }
}
