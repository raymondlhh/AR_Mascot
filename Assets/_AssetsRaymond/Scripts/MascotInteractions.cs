using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Mascot Interaction Controller
/// 
/// This script handles user interactions with the mascot, including double-tap detection,
/// rotation gestures, and scaling gestures.
/// 
/// Features:
/// - Double-tap detection with configurable timing
/// - Swipe left/right rotation on collider
/// - Two-finger pinch/zoom scaling with limits
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
/// - Swipe left/right on mascot to rotate around Y axis
/// - Use two fingers to pinch/zoom for scaling (limited between min and max scale)
/// - Works with both touch (mobile) and mouse input
/// - During getHit animation, double-tap again to restart the animation
/// - getHit animation interrupts any dancing animations
/// - Scaling is limited to minScale (original size) and maxScale multipliers
/// </summary>
public class MascotInteractions : MonoBehaviour
{
    [Header("Interaction Settings")]
    [Range(0.1f, 2.0f)]
    public float doubleTapMaxInterval = 0.8f; // Maximum time between taps for double-tap
    public bool enableTouchInput = true;
    public bool enableMouseInput = true;
    
    [Header("Gesture Settings")]
    public bool enableRotation = true;
    public bool enableScaling = true;
    [Range(10f, 200f)]
    public float rotationSensitivity = 50f; // Degrees per pixel swipe
    [Range(0.1f, 5f)]
    public float scaleSensitivity = 1f; // Scale factor sensitivity
    
    [Header("Scale Limits")]
    [Range(0.5f, 2f)]
    public float minScale = 1f; // Minimum scale (original size)
    [Range(2f, 10f)]
    public float maxScale = 3f; // Maximum scale
    

    
    [Header("Components")]
    public MascotAnimations mascotAnimations;
    public CapsuleCollider interactionCollider;
    
    [Header("Debug Settings")]
    public bool debugMode = false;
    public bool showDebugGizmos = false;
    
    // Private variables for double-tap detection
    private float lastTapTime = 0f;
    private int tapCount = 0;
    
    // Private variables for gesture detection
    private Vector3 originalScale;
    private bool isRotating = false;
    private bool isScaling = false;
    private Vector2 lastTouchPosition;
    private float lastPinchDistance = 0f;
    

    
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
        
        // Store original scale for scaling limits
        originalScale = transform.localScale;
        
        if (debugMode)
        {
            Debug.Log($"MascotInteractions initialized. Double-tap interval: {doubleTapMaxInterval}s, Original scale: {originalScale}");
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
            if (Input.touchCount == 1)
            {
                // Single touch - handle taps and rotation
                HandleSingleTouch();
            }
            else if (Input.touchCount == 2)
            {
                // Two finger touch - handle scaling
                HandleTwoFingerTouch();
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
    /// Handles single touch input for taps and rotation
    /// </summary>
    void HandleSingleTouch()
    {
        Touch touch = Input.GetTouch(0);
        
        switch (touch.phase)
        {
            case TouchPhase.Began:
                Vector2 touchPosition = touch.position;
                CheckTapOnMascot(touchPosition);
                
                // Start potential rotation
                if (enableRotation && IsTouchOverMascot(touchPosition))
                {
                    isRotating = true;
                    lastTouchPosition = touchPosition;
                }
                break;
                
            case TouchPhase.Moved:
                if (isRotating && enableRotation)
                {
                    HandleRotation(touch.position);
                }
                break;
                
            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                isRotating = false;
                break;
        }
    }
    
    /// <summary>
    /// Handles two finger touch input for scaling
    /// </summary>
    void HandleTwoFingerTouch()
    {
        Touch touch1 = Input.GetTouch(0);
        Touch touch2 = Input.GetTouch(1);
        
        if (touch1.phase == TouchPhase.Began || touch2.phase == TouchPhase.Began)
        {
            // Start scaling
            if (enableScaling)
            {
                isScaling = true;
                lastPinchDistance = Vector2.Distance(touch1.position, touch2.position);
            }
        }
        else if ((touch1.phase == TouchPhase.Moved || touch2.phase == TouchPhase.Moved) && isScaling)
        {
            // Handle scaling
            HandleScaling(touch1.position, touch2.position);
        }
        else if (touch1.phase == TouchPhase.Ended || touch2.phase == TouchPhase.Ended || 
                 touch1.phase == TouchPhase.Canceled || touch2.phase == TouchPhase.Canceled)
        {
            // End scaling
            isScaling = false;
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
    
    // ========== GESTURE DETECTION METHODS ==========
    
    /// <summary>
    /// Checks if the touch position is over the mascot collider
    /// </summary>
    /// <param name="screenPosition">Screen position of the touch</param>
    /// <returns>True if touch is over mascot</returns>
    bool IsTouchOverMascot(Vector2 screenPosition)
    {
        if (mainCamera == null || interactionCollider == null) return false;
        
        Ray ray = mainCamera.ScreenPointToRay(screenPosition);
        RaycastHit hit;
        return interactionCollider.Raycast(ray, out hit, Mathf.Infinity);
    }
    
    /// <summary>
    /// Handles rotation gesture
    /// </summary>
    /// <param name="currentTouchPosition">Current touch position</param>
    void HandleRotation(Vector2 currentTouchPosition)
    {
        Vector2 deltaPosition = currentTouchPosition - lastTouchPosition;
        
        // Only rotate based on horizontal movement (left/right swipe)
        float rotationAmount = deltaPosition.x * rotationSensitivity * Time.deltaTime;
        
        // Apply rotation around Y axis
        transform.Rotate(0, rotationAmount, 0, Space.World);
        
        lastTouchPosition = currentTouchPosition;
        
        if (debugMode)
            Debug.Log($"MascotInteractions: Rotating by {rotationAmount} degrees");
    }
    
    /// <summary>
    /// Handles scaling gesture (pinch to zoom)
    /// </summary>
    /// <param name="touch1Position">First finger position</param>
    /// <param name="touch2Position">Second finger position</param>
    void HandleScaling(Vector2 touch1Position, Vector2 touch2Position)
    {
        float currentPinchDistance = Vector2.Distance(touch1Position, touch2Position);
        float deltaDistance = currentPinchDistance - lastPinchDistance;
        
        // Calculate scale factor
        float scaleFactor = 1f + (deltaDistance * scaleSensitivity * 0.001f);
        
        // Apply scaling
        Vector3 newScale = transform.localScale * scaleFactor;
        
        // Clamp scale within limits (relative to original scale)
        newScale.x = Mathf.Clamp(newScale.x, originalScale.x * minScale, originalScale.x * maxScale);
        newScale.y = Mathf.Clamp(newScale.y, originalScale.y * minScale, originalScale.y * maxScale);
        newScale.z = Mathf.Clamp(newScale.z, originalScale.z * minScale, originalScale.z * maxScale);
        
        transform.localScale = newScale;
        
        lastPinchDistance = currentPinchDistance;
        
        if (debugMode)
            Debug.Log($"MascotInteractions: Scaling to {newScale} (factor: {scaleFactor})");
    }
    

    

    


    
    // ========== PUBLIC METHODS ==========
    
    /// <summary>
    /// Resets the mascot to original scale and rotation
    /// </summary>
    public void ResetTransform()
    {
        transform.localScale = originalScale;
        transform.rotation = Quaternion.identity;
        
        if (debugMode)
            Debug.Log("MascotInteractions: Transform reset to original state");
    }
    
    /// <summary>
    /// Sets the mascot's scale within limits
    /// </summary>
    /// <param name="scale">Scale multiplier (1 = original size)</param>
    public void SetScale(float scale)
    {
        scale = Mathf.Clamp(scale, minScale, maxScale);
        Vector3 newScale = originalScale * scale;
        transform.localScale = newScale;
        
        if (debugMode)
            Debug.Log($"MascotInteractions: Scale set to {scale} (actual scale: {newScale})");
    }
    
    /// <summary>
    /// Rotates the mascot by a specific angle
    /// </summary>
    /// <param name="angle">Rotation angle in degrees</param>
    public void RotateBy(float angle)
    {
        transform.Rotate(0, angle, 0, Space.World);
        
        if (debugMode)
            Debug.Log($"MascotInteractions: Rotated by {angle} degrees");
    }
    
    /// <summary>
    /// Gets the current scale relative to original size
    /// </summary>
    /// <returns>Scale factor (1 = original size)</returns>
    public float GetCurrentScaleFactor()
    {
        return transform.localScale.x / originalScale.x;
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
            if (isScaling) colliderColor = Color.red;
            else if (isRotating) colliderColor = Color.blue;
            else if (tapCount > 0) colliderColor = Color.cyan;
            
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
        isRotating = false;
        isScaling = false;
    }
}
