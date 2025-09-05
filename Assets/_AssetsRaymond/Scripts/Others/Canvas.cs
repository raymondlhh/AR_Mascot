using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Canvas Controller for Horizontal Scrolling Panel and Mascot Animation Control
/// 
/// This script enables horizontal scrolling functionality for a panel containing buttons
/// and manages the connection between dance buttons and mascot animations.
/// 
/// Setup Instructions:
/// 1. Attach this script to the Canvas GameObject
/// 2. The script will automatically find and connect:
///    - The Scrollbar component in the scene
///    - The Panel child object containing the buttons
///    - The viewport (Canvas RectTransform)
///    - The Mascot component for animation control
/// 3. Assign the MascotProfile GameObject reference in the inspector
/// 4. Alternatively, you can manually assign the references in the inspector
/// 
/// Features:
/// - Automatic ScrollRect setup and configuration
/// - Scrollbar integration for visual feedback
/// - Mouse wheel scrolling support
/// - Touch scrolling support for mobile devices
/// - Dynamic content size calculation
/// - Public methods for programmatic scrolling
/// - 8 dance button integration with mascot animations
/// - Animation state management (isDancing, Look Around default)
/// - Button click sound effects
/// - MascotProfile activation requirement for button functionality
/// 
/// Button Layout (0-7):
/// 0 - FYP_Button (Chicken Action)
/// 1 - VRAR_Button (House Dancing)
/// 2 - VideoGame_Button (Locking Hip Hop Action)
/// 3 - BoardGame_Button (Northern Soul Spin Combo)
/// 4 - 3DModeling_Button (Robot Hip Hop Action)
/// 5 - 2D3DAnimation_Button (Swing Dancing)
/// 6 - GameEnvironment_Button (Tut Hip Hop Action)
/// 7 - VideoAudioProduction_Button (Wave Hip Hop Action)
/// 
/// Usage:
/// - Use the scrollbar to navigate through hidden buttons
/// - Buttons are only clickable when MascotProfile object is active
/// - Click any button to trigger its corresponding dance animation
/// - Click the same button again to restart the animation
/// - Animation automatically returns to "Look Around" when finished
/// - Button clicks will play sound effects
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
    
    [Header("Mascot Animation Control")]
    public MascotAnimations mascotController;
    public GameObject mascotProfile; // Reference to MascotProfile object
    
    [Header("Audio Management")]
    public AudioManager audioManager;
    [SerializeField] private string buttonClickSoundName = "ButtonClick";
    
    [Header("Action Buttons")]
    public Button[] actionButtons = new Button[8];
    
    [Header("Button Names (for auto-detection)")]
    public string[] buttonNames = new string[]
    {
        "FYP_Button",
        "VRAR_Button", 
        "VideoGame_Button",
        "BoardGame_Button",
        "3DModeling_Button",
        "2D3DAnimation_Button",
        "GameEnvironment_Button",
        "VideoAudioProduction_Button"
    };
    
    [Header("Debug Settings")]
    public bool debugButtonClicks = false;
    
    private ScrollRect scrollRect;
    private float contentWidth;
    private float viewportWidth;
    
    void Start()
    {
        SetupScrolling();
        SetupMascotControl();
        SetupAudioManager();
        SetupActionButtons();
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
    
    void SetupMascotControl()
    {
        // Auto-find mascot controller if not assigned
        if (mascotController == null)
        {
            mascotController = FindObjectOfType<MascotAnimations>();
        }
        
        if (mascotController == null)
        {
            Debug.LogWarning("Canvas: No Mascot component found! Action buttons will not function.");
        }
        else if (debugButtonClicks)
        {
            Debug.Log("Canvas: Mascot controller found and connected");
        }
    }
    
    void SetupAudioManager()
    {
        // Auto-find AudioManager if not assigned
        if (audioManager == null)
        {
            audioManager = FindObjectOfType<AudioManager>();
        }
        
        if (audioManager == null)
        {
            Debug.LogWarning("Canvas: No AudioManager found! Button click sounds will not play.");
        }
        else if (debugButtonClicks)
        {
            Debug.Log("Canvas: AudioManager found and connected");
        }
    }
    
    void SetupActionButtons()
    {
        // Auto-find buttons if not assigned
        bool foundAllButtons = true;
        
        for (int i = 0; i < actionButtons.Length; i++)
        {
            if (actionButtons[i] == null)
            {
                // Try to find button by name
                if (i < buttonNames.Length && !string.IsNullOrEmpty(buttonNames[i]))
                {
                    GameObject buttonObj = GameObject.Find(buttonNames[i]);
                    if (buttonObj != null)
                    {
                        actionButtons[i] = buttonObj.GetComponent<Button>();
                    }
                }
                
                if (actionButtons[i] == null)
                {
                    Debug.LogWarning($"Canvas: Action button {i} ({(i < buttonNames.Length ? buttonNames[i] : "Unknown")}) not found!");
                    foundAllButtons = false;
                }
            }
        }
        
        // Setup button click listeners
        for (int i = 0; i < actionButtons.Length; i++)
        {
            if (actionButtons[i] != null)
            {
                int buttonIndex = i; // Capture for closure
                actionButtons[i].onClick.AddListener(() => OnActionButtonClicked(buttonIndex));
                
                if (debugButtonClicks)
                    Debug.Log($"Canvas: Setup button {buttonIndex} ({(buttonIndex < buttonNames.Length ? buttonNames[buttonIndex] : "Unknown")})");
            }
        }
        
        if (foundAllButtons)
        {
            Debug.Log("Canvas: All 8 action buttons found and connected successfully!");
        }
        else
        {
            Debug.LogWarning("Canvas: Some action buttons were not found. Check button names and hierarchy.");
        }
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
    
    // ========== ACTION BUTTON HANDLERS ========== 
    
    /// <summary>
    /// Handles action button clicks
    /// </summary>
    /// <param name="buttonIndex">Index of the clicked button (0-7)</param>
    public void OnActionButtonClicked(int buttonIndex)
    {
        // Check if MascotProfile is active before allowing button clicks
        if (mascotProfile != null && !mascotProfile.activeInHierarchy)
        {
            if (debugButtonClicks)
            {
                Debug.Log("Canvas: Button click ignored - MascotProfile is not active!");
            }
            return;
        }
        
        if (mascotController == null)
        {
            Debug.LogError("Canvas: Cannot play action animation - Mascot controller is null!");
            return;
        }
        
        if (buttonIndex < 0 || buttonIndex >= actionButtons.Length)
        {
            Debug.LogError($"Canvas: Invalid button index {buttonIndex}");
            return;
        }
        
        if (debugButtonClicks)
        {
            string buttonName = buttonIndex < buttonNames.Length ? buttonNames[buttonIndex] : "Unknown";
            Debug.Log($"Canvas: Action button {buttonIndex} ({buttonName}) clicked!");
        }
        
        // Play button click sound
        PlayButtonClickSound();
        
        // Play the corresponding action animation
        mascotController.PlayActionAnimation(buttonIndex);
    }
    
    /// <summary>
    /// Plays the button click sound effect
    /// </summary>
    private void PlayButtonClickSound()
    {
        if (audioManager != null)
        {
            audioManager.PlaySFX(buttonClickSoundName);
        }
        else
        {
            Debug.LogWarning("Canvas: Cannot play button click sound - AudioManager is null!");
        }
    }
    
    /// <summary>
    /// Public method to trigger specific action animations (for external scripts)
    /// </summary>
    /// <param name="danceIndex">Index of the action to play (0-7)</param>
    public void PlayAction(int danceIndex)
    {
        OnActionButtonClicked(danceIndex);
    }
    
    /// <summary>
    /// Public method to stop current action and return to Look Around
    /// </summary>
    public void StopAction()
    {
        if (mascotController != null)
        {
            mascotController.StopAction();
        }
    }
    
    /// <summary>
    /// Gets the current acting state from the mascot
    /// </summary>
    /// <returns>True if mascot is currently acting</returns>
    public bool IsMascotActing()
    {
        return mascotController != null && mascotController.IsActing();
    }
    
    /// <summary>
    /// Gets the current action index from the mascot
    /// </summary>
    /// <returns>Current action index (-1 if not acting)</returns>
    public int GetCurrentActionIndex()
    {
        return mascotController != null ? mascotController.GetCurrentActionIndex() : -1;
    }
    
    /// <summary>
    /// Checks if buttons are currently clickable (MascotProfile is active)
    /// </summary>
    /// <returns>True if buttons can be clicked, false otherwise</returns>
    public bool AreButtonsClickable()
    {
        return mascotProfile == null || mascotProfile.activeInHierarchy;
    }
    
    void OnDestroy()
    {
        // Clean up event listeners
        if (horizontalScrollbar != null)
        {
            horizontalScrollbar.onValueChanged.RemoveListener(OnScrollbarValueChanged);
        }
        
        // Clean up button listeners
        for (int i = 0; i < actionButtons.Length; i++)
        {
            if (actionButtons[i] != null)
            {
                actionButtons[i].onClick.RemoveAllListeners();
            }
        }
    }
}
