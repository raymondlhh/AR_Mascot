using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

[RequireComponent(typeof(TMP_InputField))]
public class InputFieldKeyboardHandler : MonoBehaviour
{
    [Header("Keyboard Settings")]
    [SerializeField] private bool showKeyboardOnSelect = true;
    [SerializeField] private bool hideKeyboardOnDeselect = true;
    [SerializeField] private bool hideKeyboardOnSubmit = true;
    
    private TMP_InputField inputField;
    private VirtualKeyboardManager keyboardManager;
    
    private void Awake()
    {
        inputField = GetComponent<TMP_InputField>();
        keyboardManager = VirtualKeyboardManager.Instance;
    }
    
    private void Start()
    {
        if (keyboardManager == null)
        {
            Debug.LogError("InputFieldKeyboardHandler: VirtualKeyboardManager not found! Make sure it exists in the scene.");
            return;
        }
        
        // Subscribe to input field events
        if (showKeyboardOnSelect)
        {
            inputField.onSelect.AddListener(OnInputFieldSelected);
        }
        
        if (hideKeyboardOnDeselect)
        {
            inputField.onDeselect.AddListener(OnInputFieldDeselected);
        }
        
        if (hideKeyboardOnSubmit)
        {
            inputField.onEndEdit.AddListener(OnInputFieldSubmit);
        }
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        if (inputField != null)
        {
            inputField.onSelect.RemoveListener(OnInputFieldSelected);
            inputField.onDeselect.RemoveListener(OnInputFieldDeselected);
            inputField.onEndEdit.RemoveListener(OnInputFieldSubmit);
        }
    }
    
    /// <summary>
    /// Called when the input field is selected (clicked/tapped)
    /// </summary>
    private void OnInputFieldSelected(string text)
    {
        if (keyboardManager != null)
        {
            keyboardManager.ShowKeyboard(inputField);
            Debug.Log("InputFieldKeyboardHandler: Input field selected, showing keyboard");
        }
    }
    
    /// <summary>
    /// Called when the input field is deselected
    /// </summary>
    private void OnInputFieldDeselected(string text)
    {
        if (keyboardManager != null && hideKeyboardOnDeselect)
        {
            // Only hide if this is the current input field
            if (keyboardManager.GetCurrentInputField() == inputField)
            {
                // Add a small delay to prevent keyboard from hiding when clicking keyboard buttons
                StartCoroutine(DelayedHideKeyboard());
                Debug.Log("InputFieldKeyboardHandler: Input field deselected, hiding keyboard");
            }
        }
    }
    
    /// <summary>
    /// Called when the input field text is submitted (Enter key pressed)
    /// </summary>
    private void OnInputFieldSubmit(string text)
    {
        if (keyboardManager != null && hideKeyboardOnSubmit)
        {
            // Only hide if this is the current input field
            if (keyboardManager.GetCurrentInputField() == inputField)
            {
                keyboardManager.HideKeyboard();
                Debug.Log("InputFieldKeyboardHandler: Input field submitted, hiding keyboard");
            }
        }
    }
    
    /// <summary>
    /// Manually show keyboard for this input field
    /// </summary>
    public void ShowKeyboard()
    {
        if (keyboardManager != null)
        {
            keyboardManager.ShowKeyboard(inputField);
        }
    }
    
    /// <summary>
    /// Manually hide keyboard
    /// </summary>
    public void HideKeyboard()
    {
        if (keyboardManager != null)
        {
            keyboardManager.HideKeyboard();
        }
    }
    
    /// <summary>
    /// Toggle keyboard visibility for this input field
    /// </summary>
    public void ToggleKeyboard()
    {
        if (keyboardManager != null)
        {
            keyboardManager.ToggleKeyboard(inputField);
        }
    }
    
    /// <summary>
    /// Get the associated input field
    /// </summary>
    public TMP_InputField GetInputField()
    {
        return inputField;
    }
    
    /// <summary>
    /// Coroutine to delay hiding the keyboard to prevent conflicts with button clicks
    /// </summary>
    private IEnumerator DelayedHideKeyboard()
    {
        yield return new WaitForSeconds(0.1f);
        
        // Check if the input field is still not focused
        if (keyboardManager != null && !inputField.isFocused)
        {
            keyboardManager.HideKeyboard();
        }
    }
}
