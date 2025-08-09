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
/// - Hover over mascot for 2 seconds to start grabbing mode
/// - Drag mascot around within sphere boundary while grabbing
/// - Release or move outside boundary to stop grabbing and return to initial position
/// - Works with both touch (mobile) and mouse input
/// - During getHit animation, double-tap again to restart the animation
/// - getHit and grabbing animations interrupt any dancing animations
/// </summary>
public class MascotInteractions : MonoBehaviour
{
    [Header("Interaction Settings")]
    [Range(0.1f, 2.0f)]
    public float doubleTapMaxInterval = 0.8f; // Maximum time between taps for double-tap
    public bool enableTouchInput = true;
    public bool enableMouseInput = true;
    
    [Header("Hover & Grab Settings")]
    [Range(0.5f, 5.0f)]
    public float hoverTimeToGrab = 2.0f; // Time to hover before grabbing starts
    public bool enableHoverGrab = true;
    
    [Header("Movement Constraints")]
    public Vector3 grabCenter = Vector3.zero; // Center of the grab sphere (relative to initial position)
    [Range(1.0f, 20.0f)]
    public float grabSphereRadius = 5.0f; // Maximum distance from center
    public Transform parentTransform; // The transform to move when grabbing
    public bool returnToInitialPosition = true; // Return to initial position when grabbing stops
    public bool smoothReturn = true; // Smoothly animate return to initial position
    [Range(0.1f, 2.0f)]
    public float returnDuration = 0.5f; // Duration of smooth return animation
    
    [Header("Components")]
    public MascotAnimations mascotAnimations;
    public CapsuleCollider interactionCollider;
    
    [Header("Debug Settings")]
    public bool debugMode = false;
    public bool showDebugGizmos = false;
    
    // Private variables for double-tap detection
    private float lastTapTime = 0f;
    private int tapCount = 0;
    
    // Private variables for hover and grab detection
    private bool isHovering = false;
    private float hoverStartTime = 0f;
    private bool isGrabbing = false;
    private Vector3 initialGrabPosition;
    private Vector3 grabOffset;
    private Vector3 lastValidPosition;
    private Coroutine hoverCoroutine;
    private Coroutine returnCoroutine;
    
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
        
        // Auto-find parent transform if not assigned
        if (parentTransform == null)
        {
            parentTransform = transform.parent;
            if (parentTransform == null)
                parentTransform = transform;
        }
        
        // Store initial position for grab center calculation
        initialGrabPosition = parentTransform.position;
        lastValidPosition = parentTransform.position;
        
        if (debugMode)
        {
            Debug.Log($"MascotInteractions initialized. Double-tap interval: {doubleTapMaxInterval}s, Hover time: {hoverTimeToGrab}s, Grab radius: {grabSphereRadius}");
        }
    }
    
    void Update()
    {
        HandleInput();
    }
    
    void HandleInput()
    {
        // Handle grabbing movement
        if (isGrabbing)
        {
            HandleGrabMovement();
            return; // Skip other input handling while grabbing
        }
        
        // Handle hover detection (only when not grabbing)
        if (enableHoverGrab)
        {
            HandleHoverDetection();
        }
        
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
    
    // ========== HOVER & GRAB DETECTION ==========
    
    /// <summary>
    /// Handles hover detection for grab activation
    /// </summary>
    void HandleHoverDetection()
    {
        bool currentlyOverMascot = IsPointerOverMascot();
        
        if (currentlyOverMascot && !isHovering)
        {
            // Start hovering
            StartHover();
        }
        else if (!currentlyOverMascot && isHovering)
        {
            // Stop hovering
            StopHover();
        }
    }
    
    /// <summary>
    /// Checks if the pointer is currently over the mascot
    /// </summary>
    /// <returns>True if pointer is over mascot</returns>
    bool IsPointerOverMascot()
    {
        if (mainCamera == null || interactionCollider == null) return false;
        
        Vector2 pointerPosition;
        
        // Get pointer position based on input type
        if (enableTouchInput && Input.touchCount > 0)
        {
            pointerPosition = Input.GetTouch(0).position;
        }
        else if (enableMouseInput)
        {
            pointerPosition = Input.mousePosition;
        }
        else
        {
            return false;
        }
        
        // Create ray and check collision
        Ray ray = mainCamera.ScreenPointToRay(pointerPosition);
        RaycastHit hit;
        return interactionCollider.Raycast(ray, out hit, Mathf.Infinity);
    }
    
    /// <summary>
    /// Starts the hover timer
    /// </summary>
    void StartHover()
    {
        isHovering = true;
        hoverStartTime = Time.time;
        
        if (debugMode)
            Debug.Log("MascotInteractions: Started hovering, waiting for grab activation...");
        
        // Start hover coroutine
        if (hoverCoroutine != null)
            StopCoroutine(hoverCoroutine);
        
        hoverCoroutine = StartCoroutine(HoverTimer());
    }
    
    /// <summary>
    /// Stops the hover timer
    /// </summary>
    void StopHover()
    {
        isHovering = false;
        
        if (hoverCoroutine != null)
        {
            StopCoroutine(hoverCoroutine);
            hoverCoroutine = null;
        }
        
        if (debugMode)
            Debug.Log("MascotInteractions: Stopped hovering");
    }
    
    /// <summary>
    /// Coroutine that handles hover timing
    /// </summary>
    /// <returns></returns>
    IEnumerator HoverTimer()
    {
        yield return new WaitForSeconds(hoverTimeToGrab);
        
        // Hover time reached, start grabbing
        StartGrabbing();
    }
    
    /// <summary>
    /// Starts the grabbing state
    /// </summary>
    void StartGrabbing()
    {
        if (isGrabbing) return; // Already grabbing
        
        isGrabbing = true;
        isHovering = false;
        
        // Get current pointer position for offset calculation
        Vector2 pointerPosition = enableTouchInput && Input.touchCount > 0 
            ? Input.GetTouch(0).position 
            : (Vector2)Input.mousePosition;
        
        Vector3 worldPointerPos = GetWorldPositionFromScreen(pointerPosition);
        if (parentTransform != null)
        {
            grabOffset = parentTransform.position - worldPointerPos;
        }
        
        // Trigger grabbing animation
        if (mascotAnimations != null)
        {
            mascotAnimations.StartGrabbing();
        }
        
        if (debugMode)
            Debug.Log("MascotInteractions: *** GRABBING STARTED! *** Mascot can now be moved.");
    }
    
    /// <summary>
    /// Stops the grabbing state and returns mascot to initial position
    /// </summary>
    void StopGrabbing()
    {
        if (!isGrabbing) return; // Not grabbing
        
        isGrabbing = false;
        
        // Return to initial position (if enabled)
        if (returnToInitialPosition && parentTransform != null)
        {
            if (smoothReturn)
            {
                // Start smooth return animation
                if (returnCoroutine != null)
                    StopCoroutine(returnCoroutine);
                
                returnCoroutine = StartCoroutine(SmoothReturnToInitialPosition());
            }
            else
            {
                // Instant return
                parentTransform.position = initialGrabPosition;
                lastValidPosition = initialGrabPosition;
                
                if (debugMode)
                    Debug.Log($"MascotInteractions: Instantly returned mascot to initial position: {initialGrabPosition}");
            }
        }
        
        // Stop grabbing animation
        if (mascotAnimations != null)
        {
            mascotAnimations.StopGrabbing();
        }
        
        if (debugMode)
            Debug.Log("MascotInteractions: Grabbing stopped, returning to Look Around");
    }
    
    /// <summary>
    /// Handles movement while grabbing
    /// </summary>
    void HandleGrabMovement()
    {
        if (!isGrabbing || parentTransform == null) return;
        
        // Check if user is still holding/touching
        bool stillHolding = false;
        Vector2 currentPointerPos = Vector2.zero;
        
        if (enableTouchInput && Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            stillHolding = touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary;
            currentPointerPos = touch.position;
        }
        else if (enableMouseInput && Input.GetMouseButton(0))
        {
            stillHolding = true;
            currentPointerPos = Input.mousePosition;
        }
        
        if (!stillHolding)
        {
            // User released, stop grabbing
            StopGrabbing();
            return;
        }
        
        // Calculate new position
        Vector3 worldPointerPos = GetWorldPositionFromScreen(currentPointerPos);
        Vector3 targetPosition = worldPointerPos + grabOffset;
        
        // Check sphere boundary constraint
        Vector3 grabCenterWorld = initialGrabPosition + grabCenter;
        float distanceFromCenter = Vector3.Distance(targetPosition, grabCenterWorld);
        
        if (distanceFromCenter <= grabSphereRadius)
        {
            // Within bounds, move to target position
            parentTransform.position = targetPosition;
            lastValidPosition = targetPosition;
        }
        else
        {
            // Outside bounds, stop grabbing
            if (debugMode)
                Debug.Log($"MascotInteractions: Outside grab sphere ({distanceFromCenter:F2} > {grabSphereRadius}), stopping grab");
            
            // Return to last valid position
            parentTransform.position = lastValidPosition;
            StopGrabbing();
        }
    }
    
    /// <summary>
    /// Converts screen position to world position on a plane
    /// </summary>
    /// <param name="screenPosition">Screen position</param>
    /// <returns>World position</returns>
    Vector3 GetWorldPositionFromScreen(Vector2 screenPosition)
    {
        if (mainCamera == null) return Vector3.zero;
        
        // Create a plane at the mascot's Y position
        float planeY = parentTransform != null ? parentTransform.position.y : 0f;
        Plane plane = new Plane(Vector3.up, new Vector3(0, planeY, 0));
        
        Ray ray = mainCamera.ScreenPointToRay(screenPosition);
        
        if (plane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }
        
        // Fallback: use camera's forward direction
        return mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 10f));
    }
    
    /// <summary>
    /// Smoothly returns the mascot to its initial position
    /// </summary>
    /// <returns></returns>
    IEnumerator SmoothReturnToInitialPosition()
    {
        if (parentTransform == null) yield break;
        
        Vector3 startPosition = parentTransform.position;
        Vector3 targetPosition = initialGrabPosition;
        float elapsedTime = 0f;
        
        if (debugMode)
            Debug.Log($"MascotInteractions: Starting smooth return from {startPosition} to {targetPosition} over {returnDuration}s");
        
        while (elapsedTime < returnDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / returnDuration;
            
            // Use smooth ease-out curve for natural movement
            float smoothProgress = 1f - (1f - progress) * (1f - progress);
            
            Vector3 currentPosition = Vector3.Lerp(startPosition, targetPosition, smoothProgress);
            parentTransform.position = currentPosition;
            
            yield return null;
        }
        
        // Ensure we end exactly at the target position
        parentTransform.position = targetPosition;
        lastValidPosition = targetPosition;
        returnCoroutine = null;
        
        if (debugMode)
            Debug.Log("MascotInteractions: Smooth return completed");
    }

    
    /// <summary>
    /// Public method to manually trigger getHit animation (for testing)
    /// </summary>
    public void TriggerGetHitAnimation()
    {
        OnDoubleTapDetected();
    }
    
    /// <summary>
    /// Public method to manually return mascot to initial position
    /// </summary>
    public void ReturnToInitialPosition(bool useSmooth = true)
    {
        if (parentTransform == null) return;
        
        // Stop any grabbing if active
        if (isGrabbing)
        {
            StopGrabbing();
            return; // StopGrabbing will handle the return
        }
        
        if (useSmooth && smoothReturn)
        {
            // Start smooth return animation
            if (returnCoroutine != null)
                StopCoroutine(returnCoroutine);
            
            returnCoroutine = StartCoroutine(SmoothReturnToInitialPosition());
        }
        else
        {
            // Instant return
            parentTransform.position = initialGrabPosition;
            lastValidPosition = initialGrabPosition;
            
            if (debugMode)
                Debug.Log($"MascotInteractions: Manually returned mascot to initial position: {initialGrabPosition}");
        }
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
            if (isGrabbing) colliderColor = Color.red;
            else if (isHovering) colliderColor = Color.yellow;
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
        
        // Draw grab sphere boundary
        Vector3 grabCenterWorld = initialGrabPosition + grabCenter;
        Gizmos.color = isGrabbing ? Color.red : Color.blue;
        Gizmos.matrix = Matrix4x4.identity;
        Gizmos.DrawWireSphere(grabCenterWorld, grabSphereRadius);
        
        // Draw grab center point
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(grabCenterWorld, Vector3.one * 0.2f);
        
        // Draw current position if grabbing
        if (isGrabbing && parentTransform != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(parentTransform.position, Vector3.one * 0.1f);
            
            // Draw line from center to current position
            Gizmos.color = Color.white;
            Gizmos.DrawLine(grabCenterWorld, parentTransform.position);
        }
    }
    
    void OnDestroy()
    {
        // Clean up any resources if needed
        tapCount = 0;
        lastTapTime = 0f;
        
        // Clean up hover coroutine
        if (hoverCoroutine != null)
        {
            StopCoroutine(hoverCoroutine);
            hoverCoroutine = null;
        }
        
        // Clean up return coroutine
        if (returnCoroutine != null)
        {
            StopCoroutine(returnCoroutine);
            returnCoroutine = null;
        }
        
        isHovering = false;
        isGrabbing = false;
    }
}
