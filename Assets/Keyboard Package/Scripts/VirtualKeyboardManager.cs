using UnityEngine;
using TMPro;
using System.Collections;

public class VirtualKeyboardManager : MonoBehaviour
{
    [Header("Keyboard References")]
    [SerializeField] private GameObject virtualKeyboard;
    [SerializeField] private KeyboardController keyboardController;
    
    [Header("Settings")]
    [SerializeField] private bool hideKeyboardOnSubmit = true;
    [SerializeField] private bool hideKeyboardOnClickOutside = true;
    
    private TMP_InputField currentInputField;
    private bool isKeyboardVisible = false;
    
    public static VirtualKeyboardManager Instance { get; private set; }
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    private void Start()
    {
        // Hide keyboard initially
        if (virtualKeyboard != null)
        {
            virtualKeyboard.SetActive(false);
        }
        
        // Update GameManager reference if it exists
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetVirtualKeyboardManager(this);
        }
    }
    
    private void Update()
    {
        // Hide keyboard when clicking outside input field
        if (hideKeyboardOnClickOutside && isKeyboardVisible && Input.GetMouseButtonDown(0))
        {
            // Check if click is on input field or keyboard
            if (!IsClickOnInputFieldOrKeyboard())
            {
                HideKeyboard();
            }
        }
    }
    
    /// <summary>
    /// Show the virtual keyboard and connect it to the specified input field
    /// </summary>
    /// <param name="inputField">The input field to connect to</param>
    public void ShowKeyboard(TMP_InputField inputField)
    {
        if (inputField == null)
        {
            Debug.LogWarning("VirtualKeyboardManager: Input field is null!");
            return;
        }
        
        currentInputField = inputField;
        
        if (virtualKeyboard != null)
        {
            virtualKeyboard.SetActive(true);
            isKeyboardVisible = true;
            
            // Focus the input field
            inputField.ActivateInputField();
            
            Debug.Log("VirtualKeyboardManager: Keyboard shown for input field");
        }
        else
        {
            Debug.LogError("VirtualKeyboardManager: Virtual keyboard GameObject is not assigned!");
        }
    }
    
    /// <summary>
    /// Hide the virtual keyboard
    /// </summary>
    public void HideKeyboard()
    {
        if (virtualKeyboard != null)
        {
            virtualKeyboard.SetActive(false);
            isKeyboardVisible = false;
            
            // Deactivate current input field
            if (currentInputField != null)
            {
                currentInputField.DeactivateInputField();
                currentInputField = null;
            }
            
            Debug.Log("VirtualKeyboardManager: Keyboard hidden");
        }
    }
    
    /// <summary>
    /// Toggle keyboard visibility
    /// </summary>
    public void ToggleKeyboard(TMP_InputField inputField)
    {
        if (isKeyboardVisible && currentInputField == inputField)
        {
            HideKeyboard();
        }
        else
        {
            ShowKeyboard(inputField);
        }
    }
    
    /// <summary>
    /// Add a character to the current input field
    /// </summary>
    /// <param name="character">Character to add</param>
    public void AddCharacter(string character)
    {
        if (currentInputField != null)
        {
            // Temporarily disable the input field to prevent focus loss
            bool wasInteractable = currentInputField.interactable;
            currentInputField.interactable = false;
            
            int caretPosition = currentInputField.caretPosition;
            string currentText = currentInputField.text;
            
            // Insert character at caret position
            currentInputField.text = currentText.Insert(caretPosition, character);
            
            // Move caret position
            currentInputField.caretPosition = caretPosition + 1;
            
            // Update selection
            currentInputField.selectionAnchorPosition = currentInputField.caretPosition;
            currentInputField.selectionFocusPosition = currentInputField.caretPosition;
            
            // Re-enable the input field
            currentInputField.interactable = wasInteractable;
            
            // Ensure the input field stays focused
            StartCoroutine(RefocusInputField());
        }
    }
    
    /// <summary>
    /// Delete the last character from the current input field
    /// </summary>
    public void DeleteCharacter()
    {
        if (currentInputField != null && currentInputField.text.Length > 0)
        {
            // Temporarily disable the input field to prevent focus loss
            bool wasInteractable = currentInputField.interactable;
            currentInputField.interactable = false;
            
            int caretPosition = currentInputField.caretPosition;
            string currentText = currentInputField.text;
            
            if (caretPosition > 0)
            {
                // Delete character before caret
                currentInputField.text = currentText.Remove(caretPosition - 1, 1);
                currentInputField.caretPosition = caretPosition - 1;
            }
            else if (currentText.Length > 0)
            {
                // Delete last character if caret is at beginning
                currentInputField.text = currentText.Remove(currentText.Length - 1, 1);
            }
            
            // Update selection
            currentInputField.selectionAnchorPosition = currentInputField.caretPosition;
            currentInputField.selectionFocusPosition = currentInputField.caretPosition;
            
            // Re-enable the input field
            currentInputField.interactable = wasInteractable;
            
            // Ensure the input field stays focused
            StartCoroutine(RefocusInputField());
        }
    }
    
    /// <summary>
    /// Submit the current input field text
    /// </summary>
    public void SubmitInput()
    {
        if (currentInputField != null)
        {
            // Trigger the input field's onSubmit event
            currentInputField.onEndEdit?.Invoke(currentInputField.text);
            
            if (hideKeyboardOnSubmit)
            {
                HideKeyboard();
            }
        }
    }
    
    /// <summary>
    /// Check if the current click is on an input field or the keyboard
    /// </summary>
    /// <returns>True if click is on input field or keyboard</returns>
    private bool IsClickOnInputFieldOrKeyboard()
    {
        // Check if click is on the keyboard
        if (virtualKeyboard != null && virtualKeyboard.activeInHierarchy)
        {
            // Simple check - you might want to implement more sophisticated raycast checking
            return true;
        }
        
        // Check if click is on any input field
        TMP_InputField[] inputFields = FindObjectsOfType<TMP_InputField>();
        foreach (TMP_InputField inputField in inputFields)
        {
            if (inputField.gameObject.activeInHierarchy)
            {
                // You can implement more sophisticated checking here
                return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Get the current input field
    /// </summary>
    public TMP_InputField GetCurrentInputField()
    {
        return currentInputField;
    }
    
    /// <summary>
    /// Check if keyboard is currently visible
    /// </summary>
    public bool IsKeyboardVisible()
    {
        return isKeyboardVisible;
    }
    
    /// <summary>
    /// Coroutine to refocus the input field after a short delay
    /// </summary>
    private IEnumerator RefocusInputField()
    {
        yield return new WaitForEndOfFrame();
        
        if (currentInputField != null)
        {
            currentInputField.ActivateInputField();
        }
    }
}
