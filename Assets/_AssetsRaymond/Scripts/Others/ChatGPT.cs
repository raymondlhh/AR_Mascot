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
    }

    private void Update()
    {
        // Check if mascot controller's lookOnCamera is false (no mascots active)
        if (mascotController != null && !mascotController.lookOnCamera && displayCoroutine != null)
        {
            StopCoroutine(displayCoroutine);
            displayCoroutine = null;
            audioManager.StopVoiceover(); // Stop any playing voiceover
        }
    }

    private async void AskAI()
    {
        // Randomly show one of the three Mascot_QNA options
        string[] mascotOptions = { "Mascot_QNA1", "Mascot_QNA2", "Mascot_QNA4" };
        string randomMascot = mascotOptions[Random.Range(0, mascotOptions.Length)];
        canvas.ShowTargetByName(randomMascot);
        // Check if mascot controller's lookOnCamera is false (no mascots active)
        if (mascotController == null || !mascotController.lookOnCamera)
        {
            return;
        }

        //button.enabled = false;
        //dropDownText.enabled = false;
        inputField.enabled = false;

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
        displayCoroutine = StartCoroutine(DisplayTextInChunks(fullText));
        chatHistory += $"{result.FirstChoice.Message.Content.ToString()}\n";

        button.enabled = true;
        inputField.enabled = true;
        
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
                yield break; // Exit the coroutine immediately
            }

            int chunkLength = Mathf.Min(18, fullText.Length - i);
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
    }

}
