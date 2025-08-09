using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Mascot Animation Controller
/// 
/// This script manages the mascot's dancing animations, getHit animation, and states.
/// It controls switching between different dance animations, the getHit reaction, and the default "Look Around" animation.
/// 
/// Features:
/// - 8 different dance animations
/// - getHit animation triggered by double-tap
/// - Animation state management (isDancing, getHit parameters)
/// - Automatic return to "Look Around" when animation ends
/// - Support for restarting animations when the same button is pressed
/// - Coroutine-based animation duration tracking
/// - Animation interruption system
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
/// getHit - Special reaction animation triggered by double-tap
/// </summary>
public class MascotAnimations : MonoBehaviour
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
    
    [Header("Special Animations")]
    public string getHitAnimationName = "getHit";
    public float getHitAnimationDuration = 3f; // Default duration for getHit animation
    
    // Private variables
    private bool isDancing = false;
    private bool isGettingHit = false;
    private int currentDanceIndex = -1;
    private Coroutine currentAnimationCoroutine;
    private AnimationType currentAnimationType = AnimationType.LookAround;
    
    // Animation state hashes for performance
    private int isDancingHash;
    private int getHitHash;
    private Dictionary<string, int> animationHashes;
    
    // Animation type enum
    public enum AnimationType
    {
        LookAround,
        Dancing,
        GetHit
    }
    
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
            Debug.LogError("MascotAnimations: No Animator component found! Please assign an Animator.");
            return;
        }
        
        // Cache animation parameter hashes for performance
        isDancingHash = Animator.StringToHash("isDancing");
        getHitHash = Animator.StringToHash("getHit");
        
        // Initialize animation hashes
        animationHashes = new Dictionary<string, int>();
        foreach (string animName in danceAnimationNames)
        {
            animationHashes[animName] = Animator.StringToHash(animName);
        }
        animationHashes[getHitAnimationName] = Animator.StringToHash(getHitAnimationName);
        
        // Ensure we start in the default state
        SetAnimationState(AnimationType.LookAround);
        
        if (debugMode)
            Debug.Log("MascotAnimations initialized with " + danceAnimationNames.Length + " dance animations + getHit animation");
    }
    
    /// <summary>
    /// Plays a specific dance animation by index (0-7)
    /// </summary>
    /// <param name="danceIndex">Index of the dance animation (0-7)</param>
    public void PlayDanceAnimation(int danceIndex)
    {
        if (danceIndex < 0 || danceIndex >= danceAnimationNames.Length)
        {
            Debug.LogError($"MascotAnimations: Invalid dance index {danceIndex}. Must be between 0 and {danceAnimationNames.Length - 1}");
            return;
        }
        
        if (animator == null)
        {
            Debug.LogError("MascotAnimations: Animator is null!");
            return;
        }
        
        // If the same dance animation is already playing, restart it
        if (currentAnimationType == AnimationType.Dancing && currentDanceIndex == danceIndex)
        {
            if (debugMode)
                Debug.Log($"MascotAnimations: Restarting {danceAnimationNames[danceIndex]} animation");
            RestartCurrentAnimation();
            return;
        }
        
        // Stop current animation if any
        StopCurrentAnimation();
        
        // Play the new dance animation
        currentDanceIndex = danceIndex;
        currentAnimationType = AnimationType.Dancing;
        string animationName = danceAnimationNames[danceIndex];
        
        if (debugMode)
            Debug.Log($"MascotAnimations: Playing {animationName} animation (index: {danceIndex})");
        
        // Set dancing state and play animation
        SetAnimationState(AnimationType.Dancing);
        PlaySpecificAnimation(animationName);
        
        // Start coroutine to handle animation completion
        currentAnimationCoroutine = StartCoroutine(HandleAnimationCompletion(animationName, AnimationType.Dancing));
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
            Debug.LogError($"MascotAnimations: Animation '{animationName}' not found in dance animation list");
        }
    }
    
    /// <summary>
    /// Plays the getHit animation (triggered by double-tap)
    /// </summary>
    public void PlayGetHitAnimation()
    {
        if (animator == null)
        {
            Debug.LogError("MascotAnimations: Animator is null!");
            return;
        }
        
        // If getHit animation is already playing, restart it
        if (currentAnimationType == AnimationType.GetHit)
        {
            if (debugMode)
                Debug.Log("MascotAnimations: Restarting getHit animation");
            RestartCurrentAnimation();
            return;
        }
        
        // Stop current animation (dance or other)
        StopCurrentAnimation();
        
        // Play getHit animation
        currentAnimationType = AnimationType.GetHit;
        currentDanceIndex = -1; // Reset dance index since we're not dancing
        
        if (debugMode)
            Debug.Log("MascotAnimations: Playing getHit animation");
        
        // Set getHit state and play animation
        SetAnimationState(AnimationType.GetHit);
        PlaySpecificAnimation(getHitAnimationName);
        
        // Start coroutine to handle animation completion
        currentAnimationCoroutine = StartCoroutine(HandleAnimationCompletion(getHitAnimationName, AnimationType.GetHit));
    }
    

    
    /// <summary>
    /// Stops the current animation and returns to Look Around
    /// </summary>
    public void StopCurrentAnimation()
    {
        if (currentAnimationCoroutine != null)
        {
            StopCoroutine(currentAnimationCoroutine);
            currentAnimationCoroutine = null;
        }
        
        SetAnimationState(AnimationType.LookAround);
        currentDanceIndex = -1;
        currentAnimationType = AnimationType.LookAround;
        
        if (debugMode)
            Debug.Log("MascotAnimations: Stopped current animation, returning to Look Around");
    }
    
    /// <summary>
    /// Stops dancing and returns to Look Around (for backward compatibility)
    /// </summary>
    public void StopDancing()
    {
        StopCurrentAnimation();
    }
    
    /// <summary>
    /// Restarts the currently playing animation
    /// </summary>
    public void RestartCurrentAnimation()
    {
        if (currentAnimationType == AnimationType.Dancing && currentDanceIndex >= 0)
        {
            // Stop current coroutine
            if (currentAnimationCoroutine != null)
            {
                StopCoroutine(currentAnimationCoroutine);
                currentAnimationCoroutine = null;
            }
            
            // Restart the dance animation
            string animationName = danceAnimationNames[currentDanceIndex];
            PlaySpecificAnimation(animationName);
            
            // Start new completion handler
            currentAnimationCoroutine = StartCoroutine(HandleAnimationCompletion(animationName, AnimationType.Dancing));
        }
        else if (currentAnimationType == AnimationType.GetHit)
        {
            // Stop current coroutine
            if (currentAnimationCoroutine != null)
            {
                StopCoroutine(currentAnimationCoroutine);
                currentAnimationCoroutine = null;
            }
            
            // Restart the getHit animation
            PlaySpecificAnimation(getHitAnimationName);
            
            // Start new completion handler
            currentAnimationCoroutine = StartCoroutine(HandleAnimationCompletion(getHitAnimationName, AnimationType.GetHit));
        }

    }
    
    /// <summary>
    /// Sets the animation state parameters in the animator
    /// </summary>
    /// <param name="animationType">Type of animation to set</param>
    private void SetAnimationState(AnimationType animationType)
    {
        if (animator != null)
        {
            switch (animationType)
            {
                case AnimationType.LookAround:
                    animator.SetBool(isDancingHash, false);
                    animator.SetBool(getHitHash, false);
                    isDancing = false;
                    isGettingHit = false;
                    break;
                    
                case AnimationType.Dancing:
                    animator.SetBool(isDancingHash, true);
                    animator.SetBool(getHitHash, false);
                    isDancing = true;
                    isGettingHit = false;
                    break;
                    
                case AnimationType.GetHit:
                    animator.SetBool(isDancingHash, false);
                    animator.SetBool(getHitHash, true);
                    isDancing = false;
                    isGettingHit = true;
                    break;
            }
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
    /// <param name="animationType">Type of animation being handled</param>
    /// <returns></returns>
    private IEnumerator HandleAnimationCompletion(string animationName, AnimationType animationType)
    {
        // Wait for animation to start playing
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        
        // Get the appropriate animation length
        float animationLength;
        if (animationType == AnimationType.GetHit)
        {
            animationLength = GetAnimationLength(animationName);
            if (animationLength <= 0) animationLength = getHitAnimationDuration;
        }

        else
        {
            animationLength = GetAnimationLength(animationName);
        }
        
        if (debugMode)
            Debug.Log($"MascotAnimations: {animationName} ({animationType}) will play for {animationLength} seconds");
        
        // Wait for animation to complete
        yield return new WaitForSeconds(animationLength);
        
        // Return to Look Around
        SetAnimationState(AnimationType.LookAround);
        currentDanceIndex = -1;
        currentAnimationType = AnimationType.LookAround;
        currentAnimationCoroutine = null;
        
        if (debugMode)
            Debug.Log($"MascotAnimations: {animationName} completed, returning to Look Around");
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
    /// Gets the current getHit state
    /// </summary>
    /// <returns>True if currently playing getHit animation</returns>
    public bool IsGettingHit()
    {
        return isGettingHit;
    }
    

    
    /// <summary>
    /// Gets the current animation type
    /// </summary>
    /// <returns>Current animation type</returns>
    public AnimationType GetCurrentAnimationType()
    {
        return currentAnimationType;
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
    
    /// <summary>
    /// Checks if any animation is currently playing (dance or getHit)
    /// </summary>
    /// <returns>True if any animation is playing</returns>
    public bool IsPlayingAnyAnimation()
    {
        return currentAnimationType != AnimationType.LookAround;
    }
    
    void Update()
    {
        // Optional: Add any update logic here if needed
        // For example, checking animation states or handling input
    }
    
    void OnDestroy()
    {
        // Clean up coroutines
        if (currentAnimationCoroutine != null)
        {
            StopCoroutine(currentAnimationCoroutine);
            currentAnimationCoroutine = null;
        }
    }
}