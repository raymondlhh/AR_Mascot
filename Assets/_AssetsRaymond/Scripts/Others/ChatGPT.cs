using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenAI;
using OpenAI.Chat;
using TMPro;

public class ChatGPT : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private Button button;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TMP_Dropdown dropDownText;
    [SerializeField] private TMP_Text displayText;
    [SerializeField] private string aiIdentity = "Act as an AI that responds to questions";

    [Header("Mascot Dialogue")]
    [SerializeField] private MascotController mascotController;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private Canvas canvas;
    [SerializeField] private Dialogue3DText mascotDialogue;
    [SerializeField] private float interval;
    [SerializeField] public bool isDisplayTextFinished = true;
    [SerializeField] public bool is3DText = true;
    
    [Header("Mascot Profile Control")]
    [SerializeField] private GameObject MascotProfile;
    
    
    
    
    [Header("API Configuration")]
    public string apiKey;
    private string userInput;
    private string chatHistory;
    private Coroutine displayCoroutine;
   
    private OpenAIClient api;
    

    private void Start()
    {
        chatHistory += aiIdentity + "\n";
        api = new OpenAIClient(new OpenAIAuthentication(apiKey));
        button.onClick.AddListener(AskAI);
        
        // Initialize displayText state based on is3DText
        if (displayText != null)
        {
            displayText.gameObject.SetActive(!is3DText);
        }
    }

    public void Set3DTextMode(bool enable3D)
    {
        is3DText = enable3D;
        
        // Control displayText GameObject based on 3D text mode
        if (displayText != null)
        {
            displayText.gameObject.SetActive(!enable3D);
        }
    }

    private void Update()
    {
        // Check if mascot controller's lookOnCamera is false (no mascots active)
        if (mascotController != null && !mascotController.lookOnCamera && displayCoroutine != null)
        {
            StopCoroutine(displayCoroutine);
            displayCoroutine = null;
            audioManager.StopVoiceover(); // Stop any playing voiceover
            isDisplayTextFinished = true; // Mark as finished when stopped
            
            // Re-enable button and input field when stopped
            button.enabled = true;
            inputField.enabled = true;
        }
        
        // Check if MascotProfile is inactive and stop displayCoroutine if it's running
        if (MascotProfile != null && !MascotProfile.activeInHierarchy && displayCoroutine != null)
        {
            StopCoroutine(displayCoroutine);
            displayCoroutine = null;
            audioManager.StopVoiceover(); // Stop any playing voiceover
            isDisplayTextFinished = true; // Mark as finished when stopped
            
            // Re-enable button and input field when stopped
            button.enabled = true;
            inputField.enabled = true;
            
            Debug.Log("MascotProfile is inactive - stopped displayCoroutine");
        }
    }

    private async void AskAI()
    {
        audioManager.StopBGM();
        // Check if display text is still being processed
        if (!isDisplayTextFinished)
        {
            Debug.Log("Display text is still being processed - AskAI blocked");
            return;
        }

        // Check if MascotProfile is active before proceeding
        if (MascotProfile != null && !MascotProfile.activeInHierarchy)
        {
            Debug.Log("MascotProfile is inactive - AskAI blocked");
            return;
        }

        // Disable button and input field immediately to prevent multiple clicks
        button.enabled = false;
        inputField.enabled = false;
        isDisplayTextFinished = false; // Set to false immediately when button is clicked

        audioManager.PlaySFX("ButtonClick2");
        canvas.ShowTargetByName("Mascot_Idle");
        
        // Check if mascot controller's lookOnCamera is false (no mascots active)
        // if (mascotController == null || !mascotController.lookOnCamera)
        // {
        //     return;
        // }

        userInput = dropDownText.options[dropDownText.value].text;
        chatHistory += $"{userInput}\n";

        //displayText.text = "Thinking...";
        //StartCoroutine(DisplayTextInChunks(displayText.text));
        inputField.text = "";

        var chatMessages = new List<Message>
        {
            new Message(Role.System, aiIdentity),
            new Message(Role.User, userInput)
        };

        var chatRequest = new ChatRequest(chatMessages);
        var result = await api.ChatEndpoint.GetCompletionAsync(chatRequest);

        displayText.text = result.FirstChoice.Message.Content.ToString();
        
        
        // Split text into chunks of 18 characters or less and call MascotDialogue for each
        string fullText = result.FirstChoice.Message.Content.ToString();
        
        // Only start DisplayTextInChunks coroutine if is3DText is true
        if (is3DText)
        {
            displayCoroutine = StartCoroutine(DisplayTextInChunks(fullText));
        }
        else
        {
            // If 3D text is disabled, use regular text display with typing effect
            // Make sure displayText is active before using it
            if (displayText != null)
            {
                displayText.gameObject.SetActive(true);
            }
            displayCoroutine = StartCoroutine(DisplayTextInChunksRegular(fullText));
        }
        
        chatHistory += $"{result.FirstChoice.Message.Content.ToString()}\n";
        
    }


    private IEnumerator DisplayTextInChunks(string fullText)
    {
        int i = 0;
        while (i < fullText.Length)
        {
            
            // Check if mascot controller's lookOnCamera is false (no mascots active)
            if (mascotController != null && !mascotController.lookOnCamera)
            {
                Debug.Log("No mascots are active. Stopping DisplayTextInChunks.");
                audioManager.StopVoiceover(); // Stop any playing voiceover
                isDisplayTextFinished = true; // Mark as finished when stopped
                
                // Re-enable button and input field when stopped early
                button.enabled = true;
                inputField.enabled = true;
                
                yield break; // Exit the coroutine immediately
            }

            int chunkLength = Mathf.Min(15, fullText.Length - i);
            string textChunk = fullText.Substring(i, chunkLength);
            
            // If this is not the last chunk and the chunk doesn't end with a space,
            // try to find the last space to avoid cutting words
            if (i + chunkLength < fullText.Length && !textChunk.EndsWith(" "))
            {
                int lastSpaceIndex = textChunk.LastIndexOf(' ');
                if (lastSpaceIndex > 0) // Only adjust if we found a space and it's not at the beginning
                {
                    chunkLength = lastSpaceIndex;
                    textChunk = fullText.Substring(i, chunkLength);
                }
            }
            
            // Play voiceover for this chunk
            audioManager.PlayVoiceover("Typing");
            
            // Display the text chunk
            mascotDialogue.SetDialogueInfo(textChunk);
            
            // Wait 2 seconds before stopping voiceover
            yield return new WaitForSeconds(2f);
            audioManager.StopVoiceover();
            
            // Move to the next position
            i += chunkLength;
            
            // Skip any leading spaces in the next chunk
            while (i < fullText.Length && fullText[i] == ' ')
            {
                i++;
            }
            
            // If this was the last chunk, we're done
            if (i >= fullText.Length)
            {
                break;
            }
            
            // Wait for the interval before processing the next chunk
            yield return new WaitForSeconds(interval);
        }
        
        // Mark display text as finished
        isDisplayTextFinished = true;
        
        // Re-enable button and input field when text display is finished
        button.enabled = true;
        inputField.enabled = true;
    }

    private IEnumerator DisplayTextInChunksRegular(string fullText)
    {
        displayText.text = ""; // Clear the display text initially
        
        // Reset text color to original (in case it was faded out from previous cycle)
        Color originalColor = displayText.color;
        displayText.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
        
        for (int i = 0; i < fullText.Length; i++)
        {
            // Check if mascot controller's lookOnCamera is false (no mascots active)
            if (mascotController != null && !mascotController.lookOnCamera)
            {
                Debug.Log("No mascots are active. Stopping DisplayTextInChunksRegular.");
                audioManager.StopVoiceover(); // Stop any playing voiceover
                isDisplayTextFinished = true; // Mark as finished when stopped
                
                // Re-enable button and input field when stopped early
                button.enabled = true;
                inputField.enabled = true;
                
                yield break; // Exit the coroutine immediately
            }

            char currentChar = fullText[i];
            
            // Play typing sound for each letter
            audioManager.PlayVoiceover("Typing");
            
            // Add the current character to the display text
            displayText.text += currentChar;
            
            // If it's a space, wait a bit longer for word separation
            if (currentChar == ' ')
            {
                yield return new WaitForSeconds(0.3f); // Longer pause for spaces
            }
            else
            {
                yield return new WaitForSeconds(0.1f); // Shorter pause for letters
            }
            
            // Stop voiceover after each character
            audioManager.StopVoiceover();
        }
        
        // Wait 3 seconds after typing is finished
        yield return new WaitForSeconds(3f);
        
        // Fade out the text
        yield return StartCoroutine(FadeOutText());
        
        // Mark display text as finished
        isDisplayTextFinished = true;
        
        // Re-enable button and input field when text display is finished
        button.enabled = true;
        inputField.enabled = true;
    }

    private IEnumerator FadeOutText()
    {
        Color originalColor = displayText.color;
        float fadeTime = 1f; // Fade duration
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeTime);
            displayText.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }
        
        // Ensure text is completely transparent
        displayText.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
    }

}
