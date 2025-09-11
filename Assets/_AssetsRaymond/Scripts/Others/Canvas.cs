using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class ButtonTargetPair
{
    [Header("Button and Target Pair")]
    public Button button;
    public GameObject targetObject;
    public string buttonName;
    
    [Header("Audio Settings")]
    public string narrationName;

    
}

public class Canvas : MonoBehaviour
{
    [Header("Button Target Pairs")]
    public List<ButtonTargetPair> buttonTargetPairs = new List<ButtonTargetPair>();
    
    [Header("Cover Panels")]
    public GameObject CoverSwitchButton;
    public GameObject CoverSendButton;

    [Header("Debug Settings")]
    public bool showDebugInfo = true;
    
    [Header("Scroll Configuration")]
    public Scrollbar horizontalScrollbar;
    public RectTransform contentPanel;
    public RectTransform viewport;
    
    [Header("Scroll Settings")]
    public float scrollSensitivity = 1f;
    public bool enableTouchScrolling = true;
    
    [Header("Audio Management")]
    public AudioManager audioManager;
    
    [Header("Mascot Profile Control")]
    public GameObject MascotProfile;
    
    [Header("ChatGPT Reference")]
    public ChatGPT chatGPT;
    
    [Header("Text Fade Settings")]
    public TMPro.TextMeshProUGUI textToFade;
    public float fadeOutDuration = 1f;
    
    private ScrollRect scrollRect;
    private float contentWidth;
    private float viewportWidth;
    private bool wasMascotProfileActive = true;
    
    // Start is called before the first frame update
    void Start()
    {
        SetupButtonListeners();
        SetupScrolling();
        SetupAudioManager();
        ShowTargetByName("Mascot_Idle");
        
        // Initialize mascot profile state
        if (MascotProfile != null)
        {
            wasMascotProfileActive = MascotProfile.activeInHierarchy;
        }
    }

    // Update is called once per frame
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
        
        // Check if MascotProfile state has changed
        if (MascotProfile != null)
        {
            bool isCurrentlyActive = MascotProfile.activeInHierarchy;
            
            // If MascotProfile was active but is now inactive, stop audio and fade out text
            if (wasMascotProfileActive && !isCurrentlyActive)
            {
                StopAllAudio();
                
                // Fade out text if it's not already faded out
                if (textToFade != null && textToFade.color.a > 0f)
                {
                    StartCoroutine(FadeOutText());
                }
                
                // Destroy all dialogue symbols when MascotProfile becomes inactive
                SymbolManager.DestroyAllSymbols();
            }
            
            // Update the previous state
            wasMascotProfileActive = isCurrentlyActive;
        }
        
        // Handle cover buttons based on isDisplayTextFinished state
        if (chatGPT != null)
        {
            if (!chatGPT.isDisplayTextFinished)
            {
                ShowCoverButtons();
            }
            else
            {
                HideCoverButtons();
            }
        }
    }
    
    void OnDestroy()
    {
        // Clean up scrollbar event listeners
        if (horizontalScrollbar != null)
        {
            horizontalScrollbar.onValueChanged.RemoveListener(OnScrollbarValueChanged);
        }
    }
    
    #region Setup Functions
    
    /// <summary>
    /// Sets up button listeners for all assigned button-target pairs
    /// </summary>
    private void SetupButtonListeners()
    {
        foreach (ButtonTargetPair pair in buttonTargetPairs)
        {
            if (pair.button != null)
            {
                // Remove any existing listeners to avoid duplicates
                pair.button.onClick.RemoveAllListeners();
                
                // Add the listener for this specific pair
                pair.button.onClick.AddListener(() => OnButtonPressed(pair));
                
                if (showDebugInfo)
                {
                    Debug.Log($"Button listener set up for: {pair.buttonName}");
                }
            }
            else
            {
                Debug.LogWarning($"Button is null for pair: {pair.buttonName}");
            }
        }
    }
    
    #endregion
    
    #region Scroll Setup Functions
    
    /// <summary>
    /// Sets up scrolling functionality
    /// </summary>
    private void SetupScrolling()
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
    
    /// <summary>
    /// Configures the ScrollRect component
    /// </summary>
    private void ConfigureScrollRect()
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
    
    /// <summary>
    /// Connects the scrollbar to the ScrollRect
    /// </summary>
    private void ConnectScrollbar()
    {
        if (scrollRect != null && horizontalScrollbar != null)
        {
            scrollRect.horizontalScrollbar = horizontalScrollbar;
            horizontalScrollbar.onValueChanged.AddListener(OnScrollbarValueChanged);
        }
    }
    
    /// <summary>
    /// Calculates content dimensions and updates scrollbar size
    /// </summary>
    private void CalculateContentDimensions()
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
    
    /// <summary>
    /// Handles scrollbar value changes
    /// </summary>
    /// <param name="value">New scrollbar value</param>
    private void OnScrollbarValueChanged(float value)
    {
        if (scrollRect != null)
        {
            // Update scroll position based on scrollbar value
            scrollRect.horizontalNormalizedPosition = value;
        }
    }
    
    #endregion
    
    #region Scroll Input Handling
    
    /// <summary>
    /// Handles input scrolling (mouse wheel and touch)
    /// </summary>
    private void HandleInputScrolling()
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
    
    #endregion
    
    #region Public Scroll Methods
    
    /// <summary>
    /// Scrolls to a specific position (0-1 range)
    /// </summary>
    /// <param name="normalizedPosition">Position to scroll to (0-1)</param>
    public void ScrollToPosition(float normalizedPosition)
    {
        if (horizontalScrollbar != null)
        {
            horizontalScrollbar.value = Mathf.Clamp01(normalizedPosition);
        }
    }
    
    /// <summary>
    /// Scrolls by a specific amount
    /// </summary>
    /// <param name="amount">Amount to scroll by</param>
    public void ScrollByAmount(float amount)
    {
        if (horizontalScrollbar != null)
        {
            float newValue = horizontalScrollbar.value + amount;
            horizontalScrollbar.value = Mathf.Clamp01(newValue);
        }
    }
    
    #endregion
    
    #region Audio Setup
    
    /// <summary>
    /// Sets up the AudioManager reference
    /// </summary>
    private void SetupAudioManager()
    {
        // Auto-find AudioManager if not assigned
        if (audioManager == null)
        {
            audioManager = FindObjectOfType<AudioManager>();
        }
        
        if (audioManager == null)
        {
            Debug.LogWarning("Canvas2: No AudioManager found! Button click sounds will not play.");
        }
    }
    
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
            Debug.LogWarning("Canvas2: Cannot play button click sound - AudioManager is null!");
        }
    }
    
    /// <summary>
    /// Plays narration for a specific button using its narration name
    /// </summary>
    /// <param name="pressedPair">The button-target pair that was pressed</param>
    private void PlayNarrationForButton(ButtonTargetPair pressedPair)
    {
        Debug.Log($"Canvas2: PlayNarrationForButton called for button: {pressedPair.buttonName}");
        
        if (audioManager == null)
        {
            Debug.LogWarning("Canvas2: Cannot play narration - AudioManager is null!");
            return;
        }
        
        Debug.Log($"Canvas2: AudioManager found, checking narration name...");
        
        // Check if narration name is provided
        if (string.IsNullOrEmpty(pressedPair.narrationName))
        {
            Debug.Log($"Canvas2: No narration name provided for button: {pressedPair.buttonName} (narrationName is null or empty)");
            return;
        }
        
        Debug.Log($"Canvas2: Narration name found: '{pressedPair.narrationName}' for button: {pressedPair.buttonName}");
        Debug.Log($"Canvas2: Calling audioManager.PlayNarration('{pressedPair.narrationName}')");
        
        // Play narration using the narration name
        audioManager.PlayNarration(pressedPair.narrationName);
        
        Debug.Log($"Canvas2: PlayNarration call completed for '{pressedPair.narrationName}'");
    }
    
    /// <summary>
    /// Stops all audio when MascotProfile becomes inactive
    /// </summary>
    private void StopAllAudio()
    {
        if (audioManager != null)
        {
            audioManager.StopSFX();
            audioManager.StopNarration();
            audioManager.StopBGM();
            
            if (showDebugInfo)
            {
                Debug.Log("Canvas2: MascotProfile became inactive - stopped all audio");
            }
        }
        else
        {
            Debug.LogWarning("Canvas2: Cannot stop audio - AudioManager is null!");
        }
    }
    
    #endregion
    
    #region Button Event Handlers
    
    /// <summary>
    /// Called when any button is pressed
    /// </summary>
    /// <param name="pressedPair">The button-target pair that was pressed</param>
    private void OnButtonPressed(ButtonTargetPair pressedPair)
    {
        audioManager.StopBGM();
        audioManager.StopNarration();
        Debug.Log($"Canvas2: OnButtonPressed called for button: {pressedPair.buttonName}");
        
        // Start fade out coroutine when button is clicked (only if text is not already faded out)
        if (textToFade != null && textToFade.color.a > 0f)
        {
            StartCoroutine(FadeOutText());
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"Button pressed: {pressedPair.buttonName}");
        }
        
        
        // Check if MascotProfile is active
        if (MascotProfile != null && !MascotProfile.activeInHierarchy)
        {
            // If MascotProfile is not active, only show Mascot_Idle and don't play narration
            ShowTargetByName("Mascot_Idle");
            
            if (showDebugInfo)
            {
                Debug.Log("MascotProfile is not active - showing only Mascot_Idle, narration disabled");
            }
        }
        else
        {
            PlayButtonClickSound();
            // If MascotProfile is active or null, proceed with normal behavior
            // Play narration if narration name is provided
            Debug.Log($"Canvas2: About to call PlayNarrationForButton for: {pressedPair.buttonName}");
            PlayNarrationForButton(pressedPair);
            
            // Hide all target objects first
            HideAllTargets();
            
            // Show the specific target object for this button
            if (pressedPair.targetObject != null)
            {
                pressedPair.targetObject.SetActive(true);
                
                if (showDebugInfo)
                {
                    Debug.Log($"Activated target: {pressedPair.targetObject.name}");
                }
            }
            else
            {
                Debug.LogWarning($"Target object is null for button: {pressedPair.buttonName}");
            }
        }
    }
    
    #endregion
    
    #region Target Control Functions
    
    /// <summary>
    /// Hides all target objects except Mascot_Idle
    /// </summary>
    public void HideAllTargets()
    {
        foreach (ButtonTargetPair pair in buttonTargetPairs)
        {
            if (pair.targetObject != null)
            {
                pair.targetObject.SetActive(false);
            }
        }
        
        if (showDebugInfo)
        {
            Debug.Log("All target objects hidden (except Mascot_Idle)");
        }
    }
    
    /// <summary>
    /// Shows a specific target object by name
    /// </summary>
    /// <param name="targetName">Name of the target object to show</param>
    public void ShowTargetByName(string targetName)
    {
        // First hide all
        HideAllTargets();
        
        // Find and show the specific target
        foreach (ButtonTargetPair pair in buttonTargetPairs)
        {
            if (pair.targetObject != null && pair.targetObject.name == targetName)
            {
                pair.targetObject.SetActive(true);
                
                if (showDebugInfo)
                {
                    Debug.Log($"Target shown by name: {targetName}");
                }
                return;
            }
        }
        
        Debug.LogWarning($"Target object not found with name: {targetName}");
    }
    
    /// <summary>
    /// Shows a specific target object by button name
    /// </summary>
    /// <param name="buttonName">Name of the button whose target should be shown</param>
    public void ShowTargetByButtonName(string buttonName)
    {
        // First hide all
        HideAllTargets();
        
        // Find and show the specific target
        foreach (ButtonTargetPair pair in buttonTargetPairs)
        {
            if (pair.buttonName == buttonName && pair.targetObject != null)
            {
                pair.targetObject.SetActive(true);
                
                if (showDebugInfo)
                {
                    Debug.Log($"Target shown by button name: {buttonName}");
                }
                return;
            }
        }
        
        Debug.LogWarning($"Button not found with name: {buttonName}");
    }
    
    #endregion
    
    #region Utility Functions
    
    /// <summary>
    /// Adds a new button-target pair at runtime
    /// </summary>
    /// <param name="button">The button to add</param>
    /// <param name="targetObject">The target object to control</param>
    /// <param name="buttonName">Name identifier for the button</param>
    public void AddButtonTargetPair(Button button, GameObject targetObject, string buttonName)
    {
        ButtonTargetPair newPair = new ButtonTargetPair
        {
            button = button,
            targetObject = targetObject,
            buttonName = buttonName
        };
        
        buttonTargetPairs.Add(newPair);
        
        // Set up listener for the new button
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => OnButtonPressed(newPair));
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"Added new button-target pair: {buttonName}");
        }
    }
    
    /// <summary>
    /// Removes a button-target pair by button name
    /// </summary>
    /// <param name="buttonName">Name of the button to remove</param>
    public void RemoveButtonTargetPair(string buttonName)
    {
        for (int i = buttonTargetPairs.Count - 1; i >= 0; i--)
        {
            if (buttonTargetPairs[i].buttonName == buttonName)
            {
                // Remove listener before removing the pair
                if (buttonTargetPairs[i].button != null)
                {
                    buttonTargetPairs[i].button.onClick.RemoveAllListeners();
                }
                
                buttonTargetPairs.RemoveAt(i);
                
                if (showDebugInfo)
                {
                    Debug.Log($"Removed button-target pair: {buttonName}");
                }
                return;
            }
        }
        
        Debug.LogWarning($"Button-target pair not found with name: {buttonName}");
    }
    
    #endregion
    
    #region Text Fade Functions
    
    /// <summary>
    /// Fades out the specified text over time
    /// </summary>
    private IEnumerator FadeOutText()
    {
        if (textToFade == null)
        {
            Debug.LogWarning("Canvas: No text assigned to fade out!");
            yield break;
        }
        
        Color originalColor = textToFade.color;
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeOutDuration);
            textToFade.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }
        
        // Ensure text is completely transparent
        textToFade.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
    }
    
    #endregion

    public void ShowCoverButtons()
    {
        CoverSwitchButton.SetActive(true);
        CoverSendButton.SetActive(true);
    }

    public void HideCoverButtons()
    {
        CoverSwitchButton.SetActive(false);
        CoverSendButton.SetActive(false);
    }
}
