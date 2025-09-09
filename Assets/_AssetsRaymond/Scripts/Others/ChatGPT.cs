using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenAI;
using OpenAI.Chat;
using TMPro;

public class ChatGPT : MonoBehaviour
{
    [SerializeField] private Button button;
    //[SerializeField] private TMP_InputField inputField;
    [SerializeField] private InputField inputField;
    [SerializeField] private TMP_Text displayText;
    [SerializeField] private string aiIdentity = "Act as an AI that responds to questions";
    
    [Header("API Configuration")]
    public string apiKey;
    private string userInput;
    private string chatHistory;
   
    private OpenAIClient api;

    private void Start()
    {
        chatHistory += aiIdentity + "\n";
        api = new OpenAIClient(new OpenAIAuthentication(apiKey));
        button.onClick.AddListener(AskAI);
    }

    private async void AskAI()
    {
        button.enabled = false;
        inputField.enabled = false;

        userInput = inputField.text;
        chatHistory += $"{userInput}\n";

        displayText.text = "Thinking...";
        inputField.text = "";

        var chatMessages = new List<Message>
        {
            new Message(Role.System, aiIdentity),
            new Message(Role.User, userInput)
        };

        var chatRequest = new ChatRequest(chatMessages);
        var result = await api.ChatEndpoint.GetCompletionAsync(chatRequest);

        displayText.text = result.FirstChoice.Message.Content.ToString();
        chatHistory += $"{result.FirstChoice.Message.Content.ToString()}\n";

        button.enabled = true;
        inputField.enabled = true;
        
    }
}
