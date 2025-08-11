using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Mascot Interaction Controller
/// 
/// This script handles user interactions with the mascot, specifically double-tap detection
/// on the capsule collider to trigger the getHit animation.
/// 
/// Features:
/// - Double-tap detection with configurable timing
/// - Touch and mouse input support
/// - Integration with MascotAnimations for getHit animation
/// - Collision detection with capsule collider
/// - Debug visualization and logging
/// 
/// Setup Instructions:
/// 1. Attach this script to the same GameObject as the CapsuleCollider
/// 2. Ensure the GameObject has a CapsuleCollider with "Is Trigger" enabled
/// 3. The script will automatically find the MascotAnimations component
/// 4. Configure double-tap timing and debug settings in the inspector
/// 
/// Usage:
/// - Double-tap the mascot's capsule collider to trigger getHit animation
/// - Works with both touch (mobile) and mouse input
/// - During getHit animation, double-tap again to restart the animation
/// - getHit animation interrupts any dancing animations
/// </summary>
public class MascotInteractions : MonoBehaviour
{
    [Header("Interaction Settings")]
    [Range(0.1f, 2.0f)]
    public float doubleTapMaxInterval = 0.8f; // Maximum time between taps for double-tap
    public bool enableTouchInput = true;
    public bool enableMouseInput = true;
    

    
    [Header("Components")]
    public MascotAnimations mascotAnimations;
    public CapsuleCollider interactionCollider;
    
    [Header("Debug Settings")]
    public bool debugMode = false;
    public bool showDebugGizmos = false;
    
    // Private variables for double-tap detection
    private float lastTapTime = 0f;
    private int tapCount = 0;
    

    
    // Input handling
    private Camera mainCamera;
    private bool isOverCollider = false;
    
    void Start()
    {
        InitializeInteractions();
    }
    
    void InitializeInteractions()
    {
        // Auto-find components if not assigned
        if (mascotAnimations == null)
        {
            mascotAnimations = GetComponent<MascotAnimations>();
            if (mascotAnimations == null)
                mascotAnimations = GetComponentInParent<MascotAnimations>();
            if (mascotAnimations == null)
                mascotAnimations = GetComponentInChildren<MascotAnimations>();
        }
        
        if (interactionCollider == null)
        {
            interactionCollider = GetComponent<CapsuleCollider>();
            if (interactionCollider == null)
                interactionCollider = GetComponentInChildren<CapsuleCollider>();
        }
        
        // Get main camera for raycasting
        if (mainCamera == null)
            mainCamera = Camera.main;
        if (mainCamera == null)
            mainCamera = FindObjectOfType<Camera>();
        
        // Validation
        if (mascotAnimations == null)
        {
            Debug.LogError("MascotInteractions: No MascotAnimations component found! Double-tap functionality will not work.");
        }
        
        if (interactionCollider == null)
        {
            Debug.LogError("MascotInteractions: No CapsuleCollider found! Please add a CapsuleCollider with 'Is Trigger' enabled.");
        }
        else
        {
            // Ensure the collider is set as trigger
            if (!interactionCollider.isTrigger)
            {
                Debug.LogWarning("MascotInteractions: CapsuleCollider should have 'Is Trigger' enabled for proper interaction.");
            }
        }
        
        if (mainCamera == null)
        {
            Debug.LogError("MascotInteractions: No Camera found! Touch/mouse input will not work properly.");
        }
        
        if (debugMode)
        {
            Debug.Log($"MascotInteractions initialized. Double-tap interval: {doubleTapMaxInterval}s");
        }
    }
    
    void Update()
    {
        HandleInput();
    }
    
    void HandleInput()
    {
        // Handle touch input (mobile)
        if (enableTouchInput && Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                Vector2 touchPosition = touch.position;
                CheckTapOnMascot(touchPosition);
            }
        }
        // Handle mouse input (desktop)
        else if (enableMouseInput && Input.GetMouseButtonDown(0))
        {
            Vector2 mousePosition = Input.mousePosition;
            CheckTapOnMascot(mousePosition);
        }
    }
    

    
    /// <summary>
    /// Checks if the tap/click position hits the mascot's collider
    /// </summary>
    /// <param name="screenPosition">Screen position of the tap/click</param>
    void CheckTapOnMascot(Vector2 screenPosition)
    {
        if (mainCamera == null || interactionCollider == null) return;
        
        // Create ray from camera through the tap position
        Ray ray = mainCamera.ScreenPointToRay(screenPosition);
        RaycastHit hit;
        
        // Check if ray hits the interaction collider
        if (interactionCollider.Raycast(ray, out hit, Mathf.Infinity))
        {
            if (debugMode)
                Debug.Log($"MascotInteractions: Tap detected on mascot at {hit.point}");
            
            OnMascotTapped();
        }
        else if (debugMode)
        {
            Debug.Log("MascotInteractions: Tap missed mascot collider");
        }
    }
    
    /// <summary>
    /// Handles when the mascot is tapped
    /// </summary>
    void OnMascotTapped()
    {
        float currentTime = Time.time;
        float timeSinceLastTap = currentTime - lastTapTime;
        
        if (debugMode)
            Debug.Log($"MascotInteractions: Tap detected. Time since last tap: {timeSinceLastTap:F2}s, Current tap count: {tapCount}");
        
        if (tapCount == 0)
        {
            // First tap
            tapCount = 1;
            lastTapTime = currentTime;
            
            if (debugMode)
                Debug.Log("MascotInteractions: First tap recorded, waiting for second tap...");
        }
        else if (tapCount == 1)
        {
            // Potential second tap
            if (timeSinceLastTap <= doubleTapMaxInterval)
            {
                // Valid double-tap!
                OnDoubleTapDetected();
                
                // Reset immediately for next double-tap sequence
                tapCount = 0;
                lastTapTime = 0f;
                
                if (debugMode)
                    Debug.Log("MascotInteractions: Double-tap completed! Ready for next sequence.");
            }
            else
            {
                // Too slow, treat as new first tap
                tapCount = 1;
                lastTapTime = currentTime;
                
                if (debugMode)
                    Debug.Log($"MascotInteractions: Too slow ({timeSinceLastTap:F2}s > {doubleTapMaxInterval:F2}s). Starting new sequence.");
            }
        }
    }
    

    
    /// <summary>
    /// Handles when a valid double-tap is detected
    /// </summary>
    void OnDoubleTapDetected()
    {
        if (debugMode)
            Debug.Log("MascotInteractions: *** DOUBLE-TAP DETECTED! *** Triggering getHit animation.");
        
        // Trigger the getHit animation
        if (mascotAnimations != null)
        {
            mascotAnimations.PlayGetHitAnimation();
            
            if (debugMode)
                Debug.Log("MascotInteractions: getHit animation triggered successfully.");
        }
        else
        {
            Debug.LogError("MascotInteractions: Cannot play getHit animation - MascotAnimations component is null!");
        }
    }
    

    

    


    
    /// <summary>
    /// Public method to manually trigger getHit animation (for testing)
    /// </summary>
    public void TriggerGetHitAnimation()
    {
        OnDoubleTapDetected();
    }
    

    

    

    
    /// <summary>
    /// Gets the current tap count
    /// </summary>
    /// <returns>Current tap count (0 or 1)</returns>
    public int GetCurrentTapCount()
    {
        return tapCount;
    }
    
    /// <summary>
    /// Gets the time since last tap
    /// </summary>
    /// <returns>Time since last tap in seconds</returns>
    public float GetTimeSinceLastTap()
    {
        return Time.time - lastTapTime;
    }
    
    // Unity Events for trigger detection (alternative method)
    void OnTriggerEnter(Collider other)
    {
        // Optional: Handle when something enters the trigger
        if (debugMode)
            Debug.Log($"MascotInteractions: Trigger entered by {other.name}");
    }
    
    void OnTriggerExit(Collider other)
    {
        // Optional: Handle when something exits the trigger
        if (debugMode)
            Debug.Log($"MascotInteractions: Trigger exited by {other.name}");
    }
    
    // Debug visualization
    void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;
        
        // Draw the interaction collider bounds
        if (interactionCollider != null)
        {
            Color colliderColor = Color.green;
            if (tapCount > 0) colliderColor = Color.cyan;
            
            Gizmos.color = colliderColor;
            Gizmos.matrix = transform.localToWorldMatrix;
            
            // Draw capsule wireframe
            Vector3 center = interactionCollider.center;
            float radius = interactionCollider.radius;
            float height = interactionCollider.height;
            
            // Draw the capsule outline
            Gizmos.DrawWireCube(center, new Vector3(radius * 2, height, radius * 2));
            Gizmos.DrawWireSphere(center + Vector3.up * (height * 0.5f - radius), radius);
            Gizmos.DrawWireSphere(center + Vector3.down * (height * 0.5f - radius), radius);
        }
    }
    
    void OnDestroy()
    {
        // Clean up any resources if needed
        tapCount = 0;
        lastTapTime = 0f;
    }
}
