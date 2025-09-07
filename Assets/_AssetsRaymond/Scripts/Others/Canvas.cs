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
    
    [Header("Mascot Animation Control")]
    public MascotAnimations mascotController;
    public GameObject mascotProfile; // Reference to MascotProfile object
    public MascotProps mascotProps; // Reference to MascotProps component
    
    [Header("Audio Management")]
    public AudioManager audioManager;
    
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
    
    
    #region Unity Lifecycle
    void Start()
    {
        SetupMascotControl();
        SetupAudioManager();
        SetupActionButtons();
    }
    
    void OnDestroy()
    {
        // Clean up button listeners
        for (int i = 0; i < actionButtons.Length; i++)
        {
            if (actionButtons[i] != null)
            {
                actionButtons[i].onClick.RemoveAllListeners();
            }
        }
    }
    #endregion
    
    #region Initialization Methods
    #endregion
    
    #region Mascot Control Setup
    void SetupMascotControl()
    {
        // Auto-find mascot controller if not assigned
        if (mascotController == null)
        {
            mascotController = FindObjectOfType<MascotAnimations>();
        }
        
        // Auto-find mascot props if not assigned
        if (mascotProps == null)
        {
            mascotProps = FindObjectOfType<MascotProps>();
        }
        
        if (mascotController == null)
        {
            Debug.LogWarning("Canvas: No Mascot component found! Action buttons will not function.");
        }
        
        if (mascotProps == null)
        {
            Debug.LogWarning("Canvas: No MascotProps component found! Props will not be hidden on button click.");
        }
    }
    #endregion
    
    #region Audio Setup
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
    }
    #endregion
    
    #region Button Setup
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
                
            }
        }
        
        if (!foundAllButtons)
        {
            Debug.LogWarning("Canvas: Some action buttons were not found. Check button names and hierarchy.");
        }
    }
    #endregion
    
    
    
    #region Action Button Handlers
    /// <summary>
    /// Handles action button clicks
    /// </summary>
    /// <param name="buttonIndex">Index of the clicked button (0-7)</param>
    public void OnActionButtonClicked(int buttonIndex)
    {
        // Check if MascotProfile is active before allowing button clicks
        if (mascotProfile != null && !mascotProfile.activeInHierarchy)
        {
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
        
        
        // Play button click sound
        PlayButtonClickSound();
        
        // Stop any currently playing narration
        if (audioManager != null)
        {
            audioManager.StopNarration();
        }
        
        // Play the corresponding narration
        PlayNarrationForButton(buttonIndex);
        
        
        // Hide all props first
        if (mascotProps != null)
        {
            mascotProps.HideAllProps();
        }
        
        // Play the corresponding action animation
        mascotController.PlayActionAnimation(buttonIndex);
    }
    #endregion
    
    #region Audio Methods
    /// <summary>
    /// Plays the button click sound effect
    /// </summary>
    private void PlayButtonClickSound()
    {
        if (audioManager != null)
        {
            audioManager.PlayUI("ButtonClick");
        }
        else
        {
            Debug.LogWarning("Canvas: Cannot play button click sound - AudioManager is null!");
        }
    }
    
    /// <summary>
    /// Plays the corresponding narration for the button index
    /// </summary>
    /// <param name="buttonIndex">Index of the button (0-7)</param>
    private void PlayNarrationForButton(int buttonIndex)
    {
        if (audioManager == null)
        {
            Debug.LogWarning("Canvas: Cannot play narration - AudioManager is null!");
            return;
        }
        
        if (buttonIndex < 0 || buttonIndex >= buttonNames.Length)
        {
            Debug.LogError($"Canvas: Invalid button index {buttonIndex} for narration");
            return;
        }
        
        // Get the narration name based on button index
        string narrationName = buttonNames[buttonIndex];
        
        
        // Play narration by index instead of by name
        PlayNarrationByIndex(buttonIndex);
    }
    
    /// <summary>
    /// Plays narration by index - loads the audio file dynamically and plays it
    /// </summary>
    /// <param name="narrationIndex">Index of the narration (0-7)</param>
    private void PlayNarrationByIndex(int narrationIndex)
    {
        if (audioManager == null)
        {
            Debug.LogWarning("Canvas: Cannot play narration - AudioManager is null!");
            return;
        }
        
        // Get the narration element by index
        var narrationElement = audioManager.GetNarrationElementByIndex(narrationIndex);
        if (narrationElement == null)
        {
            Debug.LogError($"Canvas: Narration element at index {narrationIndex} not found!");
            return;
        }
        
        if (narrationElement.audioFile == null)
        {
            Debug.LogError($"Canvas: No audio file assigned to narration at index {narrationIndex}!");
            return;
        }
        
        
        // Play the narration using the element directly
        audioManager.PlayNarrationElement(narrationElement);
    }
    
    #endregion
    
    #region Public Methods
    /// <summary>
    /// Public method to trigger specific action animations (for external scripts)
    /// </summary>
    /// <param name="danceIndex">Index of the action to play (0-7)</param>
    public void PlayAction(int danceIndex)
    {
        OnActionButtonClicked(danceIndex);
    }
    
    /// <summary>
    /// Public method to play narration for a specific button index (for external scripts)
    /// </summary>
    /// <param name="buttonIndex">Index of the button (0-7)</param>
    public void PlayNarrationForButtonIndex(int buttonIndex)
    {
        PlayNarrationForButton(buttonIndex);
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
    #endregion
}
