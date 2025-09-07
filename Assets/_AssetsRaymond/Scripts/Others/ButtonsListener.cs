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
}

public class ButtonsListener : MonoBehaviour
{
    [Header("Button Target Pairs")]
    public List<ButtonTargetPair> buttonTargetPairs = new List<ButtonTargetPair>();
    
    [Header("Debug Settings")]
    public bool showDebugInfo = true;
    
    // Start is called before the first frame update
    void Start()
    {
        SetupButtonListeners();
        //HideAllTargets();
        ShowTargetByName("Mascot_Idle");
    }

    // Update is called once per frame
    void Update()
    {
        
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
    
    #region Button Event Handlers
    
    /// <summary>
    /// Called when any button is pressed
    /// </summary>
    /// <param name="pressedPair">The button-target pair that was pressed</param>
    private void OnButtonPressed(ButtonTargetPair pressedPair)
    {
        if (showDebugInfo)
        {
            Debug.Log($"Button pressed: {pressedPair.buttonName}");
        }
        
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
}
