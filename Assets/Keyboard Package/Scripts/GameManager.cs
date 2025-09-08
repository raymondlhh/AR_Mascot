using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    [SerializeField] TextMeshProUGUI textBox;
    [SerializeField] TextMeshProUGUI printBox;
    
    private VirtualKeyboardManager virtualKeyboardManager;

    private void Start()
    {
        Instance = this;
        printBox.text = "";
        textBox.text = "";
    }
    
    /// <summary>
    /// Set the virtual keyboard manager reference
    /// </summary>
    /// <param name="manager">The virtual keyboard manager</param>
    public void SetVirtualKeyboardManager(VirtualKeyboardManager manager)
    {
        virtualKeyboardManager = manager;
    }

    public void DeleteLetter()
    {
        if (virtualKeyboardManager != null)
        {
            // Use the new virtual keyboard manager
            virtualKeyboardManager.DeleteCharacter();
        }
        else
        {
            // Fallback to old behavior for backward compatibility
            if(textBox != null && textBox.text.Length != 0) {
                textBox.text = textBox.text.Remove(textBox.text.Length - 1, 1);
            }
        }
    }

    public void AddLetter(string letter)
    {
        if (virtualKeyboardManager != null)
        {
            // Use the new virtual keyboard manager
            virtualKeyboardManager.AddCharacter(letter);
        }
        else
        {
            // Fallback to old behavior for backward compatibility
            if(textBox != null) {
                textBox.text = textBox.text + letter;
            }
        }
    }

    public void SubmitWord()
    {
        if (virtualKeyboardManager != null)
        {
            // Use the new virtual keyboard manager
            virtualKeyboardManager.SubmitInput();
        }
        else
        {
            // Fallback to old behavior for backward compatibility
            if(printBox != null && textBox != null) {
                printBox.text = textBox.text;
                textBox.text = "";
            }
        }
        // Debug.Log("Text submitted successfully!");
    }
}
