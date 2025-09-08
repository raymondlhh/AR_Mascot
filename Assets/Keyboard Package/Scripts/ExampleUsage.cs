using UnityEngine;
using TMPro;

/// <summary>
/// Example script showing how to use the virtual keyboard with input fields
/// This script demonstrates different ways to integrate the virtual keyboard
/// </summary>
public class ExampleUsage : MonoBehaviour
{
    [Header("Input Fields")]
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private TMP_InputField messageInput;
    
    [Header("UI References")]
    [SerializeField] private TMP_Text displayText;
    [SerializeField] private UnityEngine.UI.Button submitButton;
    [SerializeField] private UnityEngine.UI.Button clearButton;
    
    private void Start()
    {
        // Example 1: Automatic integration using InputFieldKeyboardHandler
        // Just add the InputFieldKeyboardHandler component to your input fields
        // and it will automatically show/hide the keyboard when tapped
        
        // Example 2: Manual keyboard control
        if (submitButton != null)
        {
            submitButton.onClick.AddListener(OnSubmitClicked);
        }
        
        if (clearButton != null)
        {
            clearButton.onClick.AddListener(OnClearClicked);
        }
        
        // Example 3: Custom input field event handling
        if (usernameInput != null)
        {
            usernameInput.onEndEdit.AddListener(OnUsernameSubmitted);
        }
        
        if (passwordInput != null)
        {
            passwordInput.onEndEdit.AddListener(OnPasswordSubmitted);
        }
        
        if (messageInput != null)
        {
            messageInput.onEndEdit.AddListener(OnMessageSubmitted);
        }
    }
    
    private void OnDestroy()
    {
        // Clean up event listeners
        if (submitButton != null)
        {
            submitButton.onClick.RemoveListener(OnSubmitClicked);
        }
        
        if (clearButton != null)
        {
            clearButton.onClick.RemoveListener(OnClearClicked);
        }
        
        if (usernameInput != null)
        {
            usernameInput.onEndEdit.RemoveListener(OnUsernameSubmitted);
        }
        
        if (passwordInput != null)
        {
            passwordInput.onEndEdit.RemoveListener(OnPasswordSubmitted);
        }
        
        if (messageInput != null)
        {
            messageInput.onEndEdit.RemoveListener(OnMessageSubmitted);
        }
    }
    
    /// <summary>
    /// Example of manual keyboard control
    /// </summary>
    public void ShowKeyboardForUsername()
    {
        if (VirtualKeyboardManager.Instance != null && usernameInput != null)
        {
            VirtualKeyboardManager.Instance.ShowKeyboard(usernameInput);
        }
    }
    
    /// <summary>
    /// Example of manual keyboard control
    /// </summary>
    public void ShowKeyboardForPassword()
    {
        if (VirtualKeyboardManager.Instance != null && passwordInput != null)
        {
            VirtualKeyboardManager.Instance.ShowKeyboard(passwordInput);
        }
    }
    
    /// <summary>
    /// Example of manual keyboard control
    /// </summary>
    public void ShowKeyboardForMessage()
    {
        if (VirtualKeyboardManager.Instance != null && messageInput != null)
        {
            VirtualKeyboardManager.Instance.ShowKeyboard(messageInput);
        }
    }
    
    /// <summary>
    /// Example of hiding keyboard manually
    /// </summary>
    public void HideKeyboard()
    {
        if (VirtualKeyboardManager.Instance != null)
        {
            VirtualKeyboardManager.Instance.HideKeyboard();
        }
    }
    
    /// <summary>
    /// Example of toggling keyboard visibility
    /// </summary>
    public void ToggleKeyboard()
    {
        if (VirtualKeyboardManager.Instance != null && messageInput != null)
        {
            VirtualKeyboardManager.Instance.ToggleKeyboard(messageInput);
        }
    }
    
    private void OnSubmitClicked()
    {
        string username = usernameInput != null ? usernameInput.text : "";
        string password = passwordInput != null ? passwordInput.text : "";
        string message = messageInput != null ? messageInput.text : "";
        
        string result = $"Username: {username}\nPassword: {password}\nMessage: {message}";
        
        if (displayText != null)
        {
            displayText.text = result;
        }
        
        Debug.Log("Form submitted: " + result);
        
        // Hide keyboard after submission
        HideKeyboard();
    }
    
    private void OnClearClicked()
    {
        if (usernameInput != null) usernameInput.text = "";
        if (passwordInput != null) passwordInput.text = "";
        if (messageInput != null) messageInput.text = "";
        if (displayText != null) displayText.text = "";
        
        Debug.Log("Form cleared");
    }
    
    private void OnUsernameSubmitted(string text)
    {
        Debug.Log("Username submitted: " + text);
    }
    
    private void OnPasswordSubmitted(string text)
    {
        Debug.Log("Password submitted: " + text);
    }
    
    private void OnMessageSubmitted(string text)
    {
        Debug.Log("Message submitted: " + text);
    }
}
