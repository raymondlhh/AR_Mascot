using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Canvas Controller for Horizontal Scrolling Panel
/// 
/// This script enables horizontal scrolling functionality for a panel containing buttons.
/// It automatically connects a scrollbar to scroll through content that extends beyond the viewport.
/// 
/// Setup Instructions:
/// 1. Attach this script to the Canvas GameObject
/// 2. The script will automatically find and connect:
///    - The Scrollbar component in the scene
///    - The Panel child object containing the buttons
///    - The viewport (Canvas RectTransform)
/// 3. Alternatively, you can manually assign the references in the inspector
/// 
/// Features:
/// - Automatic ScrollRect setup and configuration
/// - Scrollbar integration for visual feedback
/// - Mouse wheel scrolling support
/// - Touch scrolling support for mobile devices
/// - Dynamic content size calculation
/// - Public methods for programmatic scrolling
/// 
/// Usage:
/// - Use the scrollbar to navigate through hidden buttons
/// - Scroll with mouse wheel when hovering over the content
/// - On mobile, swipe horizontally to scroll
/// - Call ScrollToPosition(float) to scroll to specific positions programmatically
/// - Call ScrollByAmount(float) to scroll by relative amounts
/// </summary>
public class Canvas : MonoBehaviour
{
    [Header("Scroll Configuration")]
    public Scrollbar horizontalScrollbar;
    public RectTransform contentPanel;
    public RectTransform viewport;
    
    [Header("Scroll Settings")]
    public float scrollSensitivity = 1f;
    public bool enableTouchScrolling = true;
    
    private ScrollRect scrollRect;
    private float contentWidth;
    private float viewportWidth;
    
    void Start()
    {
        SetupScrolling();
    }
    
    void SetupScrolling()
    {
        // Auto-find components if not assigned
        if (horizontalScrollbar == null)
            horizontalScrollbar = FindObjectOfType<Scrollbar>();
            
        if (contentPanel == null)
            contentPanel = transform.Find("Panel")?.GetComponent<RectTransform>();
            
        if (viewport == null)
            viewport = GetComponent<RectTransform>();
        
        // Get or add ScrollRect component
        scrollRect = GetComponent<ScrollRect>();
        if (scrollRect == null)
        {
            scrollRect = gameObject.AddComponent<ScrollRect>();
        }
        
        // Configure ScrollRect
        ConfigureScrollRect();
        
        // Setup scrollbar connection
        ConnectScrollbar();
        
        // Calculate content dimensions
        CalculateContentDimensions();
    }
    
    void ConfigureScrollRect()
    {
        if (scrollRect != null && contentPanel != null)
        {
            scrollRect.content = contentPanel;
            scrollRect.horizontal = true;
            scrollRect.vertical = false;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.scrollSensitivity = scrollSensitivity;
            
            if (viewport != null)
                scrollRect.viewport = viewport;
        }
    }
    
    void ConnectScrollbar()
    {
        if (scrollRect != null && horizontalScrollbar != null)
        {
            scrollRect.horizontalScrollbar = horizontalScrollbar;
            horizontalScrollbar.onValueChanged.AddListener(OnScrollbarValueChanged);
        }
    }
    
    void CalculateContentDimensions()
    {
        if (contentPanel != null && viewport != null)
        {
            contentWidth = contentPanel.rect.width;
            viewportWidth = viewport.rect.width;
            
            // Update scrollbar size based on content
            if (horizontalScrollbar != null)
            {
                float scrollbarSize = Mathf.Clamp01(viewportWidth / contentWidth);
                horizontalScrollbar.size = scrollbarSize;
            }
        }
    }
    
    void OnScrollbarValueChanged(float value)
    {
        if (scrollRect != null)
        {
            // Update scroll position based on scrollbar value
            scrollRect.horizontalNormalizedPosition = value;
        }
    }
    
    // Public method to scroll to a specific position (0-1 range)
    public void ScrollToPosition(float normalizedPosition)
    {
        if (horizontalScrollbar != null)
        {
            horizontalScrollbar.value = Mathf.Clamp01(normalizedPosition);
        }
    }
    
    // Public method to scroll by a specific amount
    public void ScrollByAmount(float amount)
    {
        if (horizontalScrollbar != null)
        {
            float newValue = horizontalScrollbar.value + amount;
            horizontalScrollbar.value = Mathf.Clamp01(newValue);
        }
    }
    
    void Update()
    {
        // Handle touch/mouse wheel scrolling if enabled
        if (enableTouchScrolling)
        {
            HandleInputScrolling();
        }
        
        // Update content dimensions if they change
        if (contentPanel != null)
        {
            float currentContentWidth = contentPanel.rect.width;
            if (Mathf.Abs(currentContentWidth - contentWidth) > 0.1f)
            {
                CalculateContentDimensions();
            }
        }
    }
    
    void HandleInputScrolling()
    {
        // Mouse wheel scrolling
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            ScrollByAmount(-scroll * 0.1f); // Negative for natural scrolling
        }
        
        // Touch scrolling (basic implementation)
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Moved)
            {
                float deltaX = touch.deltaPosition.x;
                ScrollByAmount(-deltaX * 0.001f); // Adjust sensitivity as needed
            }
        }
    }
    
    void OnDestroy()
    {
        // Clean up event listeners
        if (horizontalScrollbar != null)
        {
            horizontalScrollbar.onValueChanged.RemoveListener(OnScrollbarValueChanged);
        }
    }
}
