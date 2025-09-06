using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonPressed : MonoBehaviour
{
    [Header("Animator Settings")]
    public Animator targetAnimator;
    
    [Header("Target Object Settings")]
    public GameObject targetGameObject;
    public GameObject objectToActivate;
    
    [Header("Timer Settings")]
    public float countdownDuration = 5f;
    
    [Header("Debug Settings")]
    public bool showDebugInfo = true;
    
    // Private variables for timer functionality
    private Coroutine countdownCoroutine;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    #region Assignable Functions (Can be assigned in Unity Inspector)
    
    /// <summary>
    /// Restarts the assigned target animator
    /// </summary>
    public void RestartAssignedAnimator()
    {
        if (targetAnimator != null)
        {
            // Disable and re-enable the animator to restart it
            targetAnimator.enabled = false;
            targetAnimator.enabled = true;
            
            if (showDebugInfo)
            {
                Debug.Log($"Assigned animator restarted: {targetAnimator.name}");
            }
        }
        else
        {
            Debug.LogWarning("RestartAssignedAnimator: No animator assigned! Please assign an animator in the Inspector.");
        }
    }
    
    /// <summary>
    /// Starts a countdown timer that will set the assigned target object to inactive after the specified duration
    /// </summary>
    public void StartCountdownToDeactivateAssigned()
    {
        if (targetGameObject != null)
        {
            StartCountdownToDeactivate(targetGameObject);
        }
        else
        {
            Debug.LogWarning("StartCountdownToDeactivateAssigned: No target GameObject assigned! Please assign a GameObject in the Inspector.");
        }
    }
    
    /// <summary>
    /// Starts a countdown timer that will set the assigned target object to inactive after the specified duration
    /// </summary>
    /// <param name="duration">Custom countdown duration in seconds</param>
    public void StartCountdownToDeactivateAssigned(float duration)
    {
        if (targetGameObject != null)
        {
            StartCountdownToDeactivate(targetGameObject, duration);
        }
        else
        {
            Debug.LogWarning("StartCountdownToDeactivateAssigned: No target GameObject assigned! Please assign a GameObject in the Inspector.");
        }
    }
    
    /// <summary>
    /// Stops the current countdown timer
    /// </summary>
    public void StopCountdown()
    {
        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
            countdownCoroutine = null;
            
            if (showDebugInfo)
            {
                Debug.Log("Countdown timer stopped");
            }
        }
    }
    
    #endregion
    
    #region Parameter-based Functions (For code usage)
    
    /// <summary>
    /// Restarts the animator by rebooting it
    /// </summary>
    /// <param name="targetAnimator">The Animator component to restart</param>
    public void RestartAnimator(Animator targetAnimator)
    {
        if (targetAnimator != null)
        {
            // Disable and re-enable the animator to restart it
            targetAnimator.enabled = false;
            targetAnimator.enabled = true;
            
            if (showDebugInfo)
            {
                Debug.Log($"Animator restarted: {targetAnimator.name}");
            }
        }
        else
        {
            Debug.LogWarning("RestartAnimator: Target animator is null!");
        }
    }
    
    /// <summary>
    /// Restarts an animator by finding it by name
    /// </summary>
    /// <param name="animatorName">The name of the GameObject with the Animator component</param>
    public void RestartAnimator(string animatorName)
    {
        GameObject targetObject = GameObject.Find(animatorName);
        
        if (targetObject != null)
        {
            Animator targetAnimator = targetObject.GetComponent<Animator>();
            
            if (targetAnimator != null)
            {
                // Disable and re-enable the animator to restart it
                targetAnimator.enabled = false;
                targetAnimator.enabled = true;
                
                if (showDebugInfo)
                {
                    Debug.Log($"Animator restarted: {animatorName}");
                }
            }
            else
            {
                Debug.LogWarning($"RestartAnimator: No Animator component found on '{animatorName}'");
            }
        }
        else
        {
            Debug.LogWarning($"RestartAnimator: Could not find GameObject with name '{animatorName}'");
        }
    }
    
    /// <summary>
    /// Restarts the assigned target animator
    /// </summary>
    // public void RestartAssignedAnimator()
    // {
    //     if (targetAnimator != null)
    //     {
    //         // Disable and re-enable the animator to restart it
    //         targetAnimator.enabled = false;
    //         targetAnimator.enabled = true;
            
    //         if (showDebugInfo)
    //         {
    //             Debug.Log($"Assigned animator restarted: {targetAnimator.name}");
    //         }
    //     }
    //     else
    //     {
    //         Debug.LogWarning("RestartAssignedAnimator: No animator assigned! Please assign an animator in the Inspector.");
    //     }
    // }
    
    /// <summary>
    /// Restarts the animator on this GameObject
    /// </summary>
    public void RestartThisAnimator()
    {
        Animator thisAnimator = GetComponent<Animator>();
        
        if (thisAnimator != null)
        {
            // Disable and re-enable the animator to restart it
            thisAnimator.enabled = false;
            thisAnimator.enabled = true;
            
            if (showDebugInfo)
            {
                Debug.Log($"This animator restarted: {gameObject.name}");
            }
        }
        else
        {
            Debug.LogWarning("RestartThisAnimator: No Animator component found on this GameObject!");
        }
    }
    
    #endregion
    
    #region Timer Functions (Parameter-based)
    
    /// <summary>
    /// Starts a countdown timer that will set the target object to inactive after the specified duration
    /// </summary>
    /// <param name="targetObject">The GameObject to set inactive after countdown</param>
    public void StartCountdownToDeactivate(GameObject targetObject)
    {
        if (targetObject != null)
        {
            // Stop any existing countdown
            if (countdownCoroutine != null)
            {
                StopCoroutine(countdownCoroutine);
            }
            
            // Start new countdown
            countdownCoroutine = StartCoroutine(CountdownToDeactivate(targetObject, countdownDuration));
            
            if (showDebugInfo)
            {
                Debug.Log($"Started countdown to deactivate '{targetObject.name}' in {countdownDuration} seconds");
            }
        }
        else
        {
            Debug.LogWarning("StartCountdownToDeactivate: Target object is null!");
        }
    }
    
    /// <summary>
    /// Starts a countdown timer that will set the target object to inactive after the specified duration
    /// </summary>
    /// <param name="targetObject">The GameObject to set inactive after countdown</param>
    /// <param name="duration">Custom countdown duration in seconds</param>
    public void StartCountdownToDeactivate(GameObject targetObject, float duration)
    {
        if (targetObject != null)
        {
            // Stop any existing countdown
            if (countdownCoroutine != null)
            {
                StopCoroutine(countdownCoroutine);
            }
            
            // Start new countdown with custom duration
            countdownCoroutine = StartCoroutine(CountdownToDeactivate(targetObject, duration));
            
            if (showDebugInfo)
            {
                Debug.Log($"Started countdown to deactivate '{targetObject.name}' in {duration} seconds");
            }
        }
        else
        {
            Debug.LogWarning("StartCountdownToDeactivate: Target object is null!");
        }
    }
    
    /// <summary>
    /// Starts a countdown timer that will set the target object (found by name) to inactive after the specified duration
    /// </summary>
    /// <param name="objectName">The name of the GameObject to find and set inactive after countdown</param>
    public void StartCountdownToDeactivate(string objectName)
    {
        GameObject targetObject = GameObject.Find(objectName);
        
        if (targetObject != null)
        {
            StartCountdownToDeactivate(targetObject);
        }
        else
        {
            Debug.LogWarning($"StartCountdownToDeactivate: Could not find GameObject with name '{objectName}'");
        }
    }
    
    /// <summary>
    /// Starts a countdown timer that will set the target object (found by name) to inactive after the specified duration
    /// </summary>
    /// <param name="objectName">The name of the GameObject to find and set inactive after countdown</param>
    /// <param name="duration">Custom countdown duration in seconds</param>
    public void StartCountdownToDeactivate(string objectName, float duration)
    {
        GameObject targetObject = GameObject.Find(objectName);
        
        if (targetObject != null)
        {
            StartCountdownToDeactivate(targetObject, duration);
        }
        else
        {
            Debug.LogWarning($"StartCountdownToDeactivate: Could not find GameObject with name '{objectName}'");
        }
    }
    
    #endregion
    
    #region Internal Helper Functions (Private/Internal use only)
    
    /// <summary>
    /// Coroutine that handles the countdown and deactivates the object
    /// </summary>
    /// <param name="targetObject">The object to deactivate</param>
    /// <param name="duration">Countdown duration in seconds</param>
    /// <returns></returns>
    private IEnumerator CountdownToDeactivate(GameObject targetObject, float duration)
    {
        float timeRemaining = duration;
        
        while (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            
            if (showDebugInfo)
            {
                Debug.Log($"Countdown: {timeRemaining:F1} seconds remaining for '{targetObject.name}'");
            }
            
            yield return null;
        }
        
        // Time's up - deactivate the target object
        if (targetObject != null)
        {
            targetObject.SetActive(false);
            
            if (showDebugInfo)
            {
                Debug.Log($"Countdown finished: '{targetObject.name}' has been deactivated");
            }
        }
        
        // Activate the other object
        if (objectToActivate != null)
        {
            objectToActivate.SetActive(true);
            
            if (showDebugInfo)
            {
                Debug.Log($"Object activated: '{objectToActivate.name}'");
            }
        }
        
        countdownCoroutine = null;
    }
    
    #endregion
}
