using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;
using OpenAI;
using System.Threading.Tasks;
using UnityEngine.Events;

public class MascotSpeech : MonoBehaviour
{
    [Header("OpenAI Configuration")]
    [Tooltip("Set this at runtime or in Editor for testing. Prefer secure storage in production.")]
    [SerializeField] private string openAIApiKey = "";
    [SerializeField] private string chatModel = "gpt-3.5-turbo";
    [SerializeField] private string ttsModel = "gpt-4o-mini-tts";
    [SerializeField] private string voice = "alloy"; // try: "ember", "juniper", "verse", "sky", etc.

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [Tooltip("WAV is easiest to load at runtime in Unity")]
    [SerializeField] private string audioFormat = "wav"; // "wav" recommended

    [Header("UI References")]
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TextMeshProUGUI replyText;
    [SerializeField] private Button sendButton;
    
    [Header("UI Settings")]
    [SerializeField] private string thinkingMessage = "Mascot is thinking...";
    [SerializeField] private string errorMessage = "Sorry, I couldn't process that. Please try again.";
    [SerializeField] private float typingSpeed = 0.05f; // Delay between characters for typing effect
    
    [Header("Networking")]
    [SerializeField] private float requestTimeout = 30f;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;

    [Header("Events")]
    public OnResponseEvent OnResponse;
    public OnErrorEvent OnError;

    [System.Serializable]
    public class OnResponseEvent : UnityEvent<string> { }
    
    [System.Serializable]
    public class OnErrorEvent : UnityEvent<string> { }

    // Simple in-session cache: text -> local file path
    private readonly Dictionary<string, string> clipCache = new Dictionary<string, string>();
    
    // ChatGPT components
    private OpenAIApi openAI;
    private List<ChatMessage> messages = new List<ChatMessage>();
    
    // UI state management
    private bool isProcessing = false;
    private Coroutine typingCoroutine;

    void Start()
    {
        InitializeOpenAI();
        SetupUI();
        SetupEventListeners();
    }
    
    void InitializeOpenAI()
    {
        // Get API key from inspector or environment variable
        string apiKey = GetApiKey();
        
        if (string.IsNullOrEmpty(apiKey))
        {
            Debug.LogError("MascotSpeech: OpenAI API key is not set! Please set it in the inspector or OPENAI_API_KEY environment variable.");
            return;
        }
        
        openAI = new OpenAIApi(apiKey);
        Debug.Log("MascotSpeech: OpenAI API initialized successfully.");
    }
    
    string GetApiKey()
    {
        // Prefer inspector setting
        if (!string.IsNullOrEmpty(openAIApiKey)) 
            return openAIApiKey;
        
        // Fallback to environment variable
        var env = System.Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (!string.IsNullOrEmpty(env)) 
            return env;
        
        return "";
    }
    
    void SetupUI()
    {
        // Auto-find components if not assigned
        if (inputField == null)
            inputField = FindObjectOfType<TMP_InputField>();
            
        if (replyText == null)
        {
            var replyTextObj = GameObject.Find("ReplyText (TMP)");
            if (replyTextObj != null)
                replyText = replyTextObj.GetComponent<TextMeshProUGUI>();
        }
            
        if (sendButton == null)
            sendButton = GetComponentInChildren<Button>();
            
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        
        // Validation
        if (inputField == null)
            Debug.LogError("MascotSpeech: InputField not found! Please assign it in the inspector.");
        if (replyText == null)
            Debug.LogError("MascotSpeech: ReplyText not found! Please assign it in the inspector.");
        if (audioSource == null)
            Debug.LogError("MascotSpeech: AudioSource not found! Please assign it in the inspector.");
    }
    
    void SetupEventListeners()
    {
        // Set up send button listener
        if (sendButton != null)
        {
            sendButton.onClick.AddListener(OnSendButtonClicked);
        }
        
        // Set up input field listener for Enter key
        if (inputField != null)
        {
            inputField.onEndEdit.AddListener(OnInputFieldEndEdit);
        }
    }

    // Main entry point for user input
    public void ProcessUserInput(string userInput)
    {
        if (string.IsNullOrWhiteSpace(userInput))
        {
            Debug.LogWarning("MascotSpeech: Empty input text provided.");
            OnError?.Invoke("Please enter some text to ask the mascot.");
            return;
        }

        if (openAI == null)
        {
            Debug.LogError("MascotSpeech: OpenAI API not initialized. Check your API key.");
            OnError?.Invoke("OpenAI API not initialized. Please check your API key.");
            return;
        }

        Debug.Log($"MascotSpeech: Processing user input: {userInput}");
        StartCoroutine(ProcessInputRoutine(userInput));
    }

    private IEnumerator ProcessInputRoutine(string userInput)
    {
        // Send to ChatGPT
        yield return StartCoroutine(GetChatGPTResponse(userInput));
    }

    private IEnumerator GetChatGPTResponse(string userInput)
    {
        bool requestCompleted = false;
        string response = "";
        string error = "";

        // Start async ChatGPT request
        Task.Run(async () =>
        {
            try
            {
                ChatMessage newMessage = new ChatMessage();
                newMessage.Content = userInput;
                newMessage.Role = "user";
                messages.Add(newMessage);

                CreateChatCompletionRequest request = new CreateChatCompletionRequest();
                request.Messages = messages;
                request.Model = chatModel;

                var chatResponse = await openAI.CreateChatCompletion(request);

                if (chatResponse.Choices != null && chatResponse.Choices.Count > 0)
                {
                    var chatMessage = chatResponse.Choices[0].Message;
                    messages.Add(chatMessage);
                    response = chatMessage.Content;
                    Debug.Log($"MascotSpeech: Received ChatGPT response: {response}");
                }
                else
                {
                    error = "No response received from the AI. Please try again.";
                    Debug.LogError("MascotSpeech: No response choices received from OpenAI.");
                }
            }
            catch (System.Exception e)
            {
                error = $"Error: {e.Message}";
                Debug.LogError($"MascotSpeech: Error calling OpenAI API: {e.Message}");
            }
            finally
            {
                requestCompleted = true;
            }
        });

        // Wait for request to complete
        yield return new WaitUntil(() => requestCompleted);

        if (!string.IsNullOrEmpty(error))
        {
            OnMascotSpeechError(error);
        }
        else if (!string.IsNullOrEmpty(response))
        {
            OnMascotSpeechResponse(response);
            // Automatically speak the response
            Speak(response);
        }
    }

    // UI Event Handlers
    public void OnSendButtonClicked()
    {
        ProcessUserInput();
    }
    
    public void OnInputFieldEndEdit(string text)
    {
        // Only process if Enter was pressed (not just lost focus)
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            ProcessUserInput();
        }
    }
    
    void ProcessUserInput()
    {
        if (isProcessing)
        {
            if (enableDebugLogs)
                Debug.Log("MascotSpeech: Already processing a request, ignoring new input.");
            return;
        }
        
        if (inputField == null || string.IsNullOrWhiteSpace(inputField.text))
        {
            if (enableDebugLogs)
                Debug.Log("MascotSpeech: No input text to process.");
            return;
        }
        
        string userInput = inputField.text.Trim();
        
        if (enableDebugLogs)
            Debug.Log($"MascotSpeech: Processing user input: {userInput}");
        
        // Clear the input field
        inputField.text = "";
        
        // Show thinking message
        ShowReplyText(thinkingMessage);
        
        // Start processing
        isProcessing = true;
        
        // Process the input
        ProcessUserInput(userInput);
    }
    
    void OnMascotSpeechResponse(string response)
    {
        if (enableDebugLogs)
            Debug.Log($"MascotSpeech: Received response: {response}");
        
        // Stop any existing typing effect
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        
        // Start typing effect
        typingCoroutine = StartCoroutine(TypeText(response));
        
        // Audio is automatically handled by Speak method
        Debug.Log("MascotSpeech: Audio will be played automatically");
        
        // Reset processing state
        isProcessing = false;
    }
    
    void OnMascotSpeechError(string errorMessage)
    {
        if (enableDebugLogs)
            Debug.LogError($"MascotSpeech: Error: {errorMessage}");
        
        // Stop any existing typing effect
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        
        // Show error message
        ShowReplyText(errorMessage);
        
        // Reset processing state
        isProcessing = false;
    }
    
    IEnumerator TypeText(string text)
    {
        replyText.text = "";
        
        for (int i = 0; i < text.Length; i++)
        {
            replyText.text += text[i];
            yield return new WaitForSeconds(typingSpeed);
        }
    }
    
    void ShowReplyText(string text)
    {
        if (replyText != null)
        {
            replyText.text = text;
        }
    }
    
    // Public methods for external control
    public void ClearReplyText()
    {
        if (replyText != null)
        {
            replyText.text = "";
        }
    }
    
    public void SetReplyText(string text)
    {
        ShowReplyText(text);
    }
    
    public void ClearConversation()
    {
        messages.Clear();
        Debug.Log("MascotSpeech: Conversation history cleared.");
    }

    public void StopSpeaking()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    public bool IsSpeaking()
    {
        return audioSource != null && audioSource.isPlaying;
    }

    // Entry point for TTS only
    public void Speak(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            Debug.LogWarning("[MascotSpeech] Empty input text for TTS.");
            return;
        }
        StartCoroutine(SpeakRoutine(text));
    }

    private IEnumerator SpeakRoutine(string text)
    {
        // Return cached clip if available
        if (clipCache.TryGetValue(text, out var cachedPath) && File.Exists(cachedPath))
        {
            yield return StartCoroutine(LoadAndPlayClip(cachedPath));
            yield break;
        }

        // Create JSON body for TTS
        var json = JsonUtility.ToJson(new TTSRequest
        {
            model = ttsModel,
            voice = voice,
            input = text,
        });

        // Build request
        var url = "https://api.openai.com/v1/audio/speech";
        var bodyRaw = Encoding.UTF8.GetBytes(json);
        using (var req = new UnityWebRequest(url, "POST"))
        {
            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.timeout = Mathf.RoundToInt(requestTimeout);

            req.SetRequestHeader("Authorization", $"Bearer {GetApiKey()}");
            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("Accept", "audio/wav"); // ensure WAV bytes returned

            yield return req.SendWebRequest();

#if UNITY_2020_2_OR_NEWER
            if (req.result != UnityWebRequest.Result.Success)
#else
            if (req.isNetworkError || req.isHttpError)
#endif
            {
                Debug.LogError($"[MascotSpeech] TTS request failed: {req.error}\n{req.downloadHandler?.text}");
                yield break;
            }

            var bytes = req.downloadHandler.data;
            if (bytes == null || bytes.Length == 0)
            {
                Debug.LogError("[MascotSpeech] Empty audio response.");
                yield break;
            }

            // Save bytes to a .wav file and load as AudioClip
            var fileName = $"tts_{Hash(text)}.wav";
            var path = Path.Combine(Application.persistentDataPath, fileName);
            try
            {
                File.WriteAllBytes(path, bytes);
            }
            catch (Exception e)
            {
                Debug.LogError($"[MascotSpeech] Failed writing WAV: {e.Message}");
                yield break;
            }

            // Cache & play
            clipCache[text] = path;
            yield return StartCoroutine(LoadAndPlayClip(path));
        }
    }

    private IEnumerator LoadAndPlayClip(string path)
    {
        // Ensure we have an AudioSource
        if (!audioSource) audioSource = GetComponent<AudioSource>();
        if (!audioSource)
        {
            Debug.LogError("[MascotSpeech] No AudioSource assigned or found on GameObject.");
            yield break;
        }

        // Stop any currently playing audio
        audioSource.Stop();

        // Try different audio loading methods for better compatibility
        AudioClip clip = null;
        
        // Method 1: Try UnityWebRequestMultimedia with WAV
        using (var www = UnityWebRequestMultimedia.GetAudioClip("file://" + path, AudioType.WAV))
        {
            www.timeout = Mathf.RoundToInt(requestTimeout);
            yield return www.SendWebRequest();

#if UNITY_2020_2_OR_NEWER
            if (www.result == UnityWebRequest.Result.Success)
#else
            if (!www.isNetworkError && !www.isHttpError)
#endif
            {
                clip = DownloadHandlerAudioClip.GetContent(www);
                if (clip != null)
                {
                    Debug.Log($"[MascotSpeech] Successfully loaded WAV clip: {clip.name}");
                }
            }
            else
            {
                Debug.LogWarning($"[MascotSpeech] WAV loading failed: {www.error}, trying alternative method...");
            }
        }

        // Method 2: If WAV failed, try loading as MP3 (some systems handle this better)
        if (clip == null)
        {
            using (var www = UnityWebRequestMultimedia.GetAudioClip("file://" + path, AudioType.MPEG))
            {
                www.timeout = Mathf.RoundToInt(requestTimeout);
                yield return www.SendWebRequest();

#if UNITY_2020_2_OR_NEWER
                if (www.result == UnityWebRequest.Result.Success)
#else
                if (!www.isNetworkError && !www.isHttpError)
#endif
                {
                    clip = DownloadHandlerAudioClip.GetContent(www);
                    if (clip != null)
                    {
                        Debug.Log($"[MascotSpeech] Successfully loaded MPEG clip: {clip.name}");
                    }
                }
                else
                {
                    Debug.LogWarning($"[MascotSpeech] MPEG loading also failed: {www.error}");
                }
            }
        }

        // Method 3: Try loading as UNKNOWN type (fallback)
        if (clip == null)
        {
            using (var www = UnityWebRequestMultimedia.GetAudioClip("file://" + path, AudioType.UNKNOWN))
            {
                www.timeout = Mathf.RoundToInt(requestTimeout);
                yield return www.SendWebRequest();

#if UNITY_2020_2_OR_NEWER
                if (www.result == UnityWebRequest.Result.Success)
#else
                if (!www.isNetworkError && !www.isHttpError)
#endif
                {
                    clip = DownloadHandlerAudioClip.GetContent(www);
                    if (clip != null)
                    {
                        Debug.Log($"[MascotSpeech] Successfully loaded UNKNOWN type clip: {clip.name}");
                    }
                }
                else
                {
                    Debug.LogError($"[MascotSpeech] All audio loading methods failed: {www.error}");
                }
            }
        }

        if (clip == null)
        {
            Debug.LogError("[MascotSpeech] Failed to load audio clip with all methods. Check file format and Unity audio settings.");
            yield break;
        }

        // Set and play the clip
        audioSource.clip = clip;
        audioSource.Play();
        
        Debug.Log($"[MascotSpeech] Playing audio clip: {clip.name} (Length: {clip.length}s, Sample Rate: {clip.frequency}Hz)");
    }

    void OnDestroy()
    {
        // Clean up event listeners
        if (sendButton != null)
        {
            sendButton.onClick.RemoveListener(OnSendButtonClicked);
        }
        
        if (inputField != null)
        {
            inputField.onEndEdit.RemoveListener(OnInputFieldEndEdit);
        }
    }

    [Serializable]
    private class TTSRequest
    {
        public string model;
        public string voice;
        public string input;
    }

    private static string Hash(string s)
    {
        unchecked
        {
            int h = 23;
            foreach (var c in s) h = (h * 31) + c;
            return h.ToString("X");
        }
    }
}
