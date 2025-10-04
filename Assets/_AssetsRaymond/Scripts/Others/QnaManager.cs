using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Linq;

[System.Serializable]
public class QnAPair
{
    public string question;
    public string answer;
}

public class QnaManager : MonoBehaviour
{
    [Header("Q&A Configuration")]
    [SerializeField] private TextAsset qnaTextFile;
    [SerializeField] private List<QnAPair> qnaPairs = new List<QnAPair>();
    
    [Header("Answer Variations")]
    [SerializeField] private bool enableVariations = true;
    [SerializeField] private List<string> defaultAnswers = new List<string>();
    
    [Header("UI References")]
    [SerializeField] private Button button;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TMP_Dropdown dropDownText;
    [SerializeField] private TMP_Text displayText;
    
    [Header("Mascot Dialogue")]
    [SerializeField] private MascotController mascotController;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private Canvas canvas;
    [SerializeField] private Dialogue3DText mascotDialogue;
    [SerializeField] private float interval = 1f;
    [SerializeField] public bool isDisplayTextFinished = true;
    [SerializeField] public bool is3DText = true;
    
    [Header("Mascot Profile Control")]
    [SerializeField] private GameObject MascotProfile;
    
    private Dictionary<string, List<string>> answerVariations = new Dictionary<string, List<string>>();
    private string userInput;
    private string chatHistory;
    private Coroutine displayCoroutine;
    
    void Start()
    {
        LoadQnAData();
        InitializeUI();
    }
    
    void InitializeUI()
    {
        if (button != null)
        {
            button.onClick.AddListener(AskQuestion);
        }
        
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
    
    public void AskQuestion()
    {
        audioManager.StopBGM();
        // Check if display text is still being processed
        if (!isDisplayTextFinished)
        {
            Debug.Log("Display text is still being processed - AskQuestion blocked");
            return;
        }

        // Check if MascotProfile is active before proceeding
        if (MascotProfile != null && !MascotProfile.activeInHierarchy)
        {
            Debug.Log("MascotProfile is inactive - AskQuestion blocked");
            return;
        }

        // Disable button and input field immediately to prevent multiple clicks
        button.enabled = false;
        inputField.enabled = false;
        isDisplayTextFinished = false; // Set to false immediately when button is clicked

        audioManager.PlaySFX("ButtonClick2");
        canvas.ShowTargetByName("Mascot_Idle");
        
        userInput = dropDownText.options[dropDownText.value].text;
        chatHistory += $"{userInput}\n";

        inputField.text = "";

        // Get answer from QnA data instead of ChatGPT
        string answer = GetAnswer(userInput);
        displayText.text = answer;
        
        // Split text into chunks and display
        string fullText = answer;
        
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
        
        chatHistory += $"{answer}\n";
    }
    
    void LoadQnAData()
    {
        if (qnaTextFile == null)
        {
            Debug.LogError("QnA Text File is not assigned!");
            return;
        }
        
        string[] lines = qnaTextFile.text.Split('\n');
        string currentQuestion = "";
        List<string> currentAnswers = new List<string>();
        
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            
            // Skip empty lines and comments
            if (string.IsNullOrEmpty(line) || line.StartsWith("//") || line.StartsWith("#"))
                continue;
                
            // Check if this is a question (starts with Q followed by number or is a direct question)
            if (line.StartsWith("Q") && line.Contains(":") || 
                (line.EndsWith("?") && !line.StartsWith("🔹") && !line.StartsWith("⚡") && !line.StartsWith("👉")))
            {
                // Save previous question if exists
                if (!string.IsNullOrEmpty(currentQuestion) && currentAnswers.Count > 0)
                {
                    answerVariations[currentQuestion.ToLower()] = new List<string>(currentAnswers);
                }
                
                // Start new question
                currentQuestion = line.Replace("Q1–Q10:", "").Replace("Q11–Q20:", "").Replace("Q21–Q28:", "").Trim();
                if (currentQuestion.StartsWith("Q") && currentQuestion.Contains(":"))
                {
                    currentQuestion = currentQuestion.Substring(currentQuestion.IndexOf(":") + 1).Trim();
                }
                currentAnswers.Clear();
            }
            // Check if this is an answer (starts with quote or is a variation)
            else if (line.StartsWith("\"") && line.EndsWith("\"") || 
                     (line.Length > 0 && !line.StartsWith("🔹") && !line.StartsWith("⚡") && !line.StartsWith("👉") && 
                      !line.StartsWith("Special Case") && !line.StartsWith("Example") && !line.StartsWith("Teams List") &&
                      !line.StartsWith("Leader:") && !line.StartsWith("Members:") && !line.StartsWith("Who are") &&
                      !line.StartsWith("Who is") && !line.StartsWith("Who made") && !line.StartsWith("Who developed") &&
                      !line.StartsWith("If the user asks") && !line.StartsWith("Instruction for AI") &&
                      !line.StartsWith("Keep answers") && !line.StartsWith("Each reply") &&
                      !line.StartsWith("When asked") && !line.StartsWith("Rule:") &&
                      !line.StartsWith("Use") && !line.StartsWith("Sometimes") && !line.StartsWith("Teams List") &&
                      !line.StartsWith("Organizing Team") && !line.StartsWith("Clerical Team") &&
                      !line.StartsWith("Event Planner Team") && !line.StartsWith("General Affair Team") &&
                      !line.StartsWith("Sponsorship Team") && !line.StartsWith("Multimedia Team") &&
                      !line.StartsWith("Photography") && !line.StartsWith("Technical Team") &&
                      !line.StartsWith("Other Key Questions") && !line.StartsWith("Always include") &&
                      !line.StartsWith("This answer") && !line.StartsWith("Keep Rendify") &&
                      !line.StartsWith("The Student Helpers") && !line.StartsWith("The Head of DMT") &&
                      !line.StartsWith("Must vary") && !line.StartsWith("DMT is headed") &&
                      !line.StartsWith("Ms. Heng") && !line.StartsWith("Tone:") && !line.StartsWith("Your name") &&
                      !line.StartsWith("You are") && !line.StartsWith("Answer length") && !line.StartsWith("Variation:") &&
                      !line.StartsWith("Never repeat") && !line.StartsWith("The AI must") && !line.StartsWith("Use synonyms") &&
                      !line.StartsWith("Change order") && !line.StartsWith("Switch style") && !line.StartsWith("Unrelated questions") &&
                      !line.StartsWith("Always reply") && !line.StartsWith("Slogan:") && !line.StartsWith("Must always") &&
                      !line.StartsWith("Realize") && !line.StartsWith("Always 5") && !line.StartsWith("Example variations") &&
                      !line.StartsWith("Meet Rendify") && !line.StartsWith("I'm your") && !line.StartsWith("Your AI-powered") &&
                      !line.StartsWith("Welcome!") && !line.StartsWith("Ready to") && !line.StartsWith("Here to") &&
                      !line.StartsWith("Your enthusiastic") && !line.StartsWith("Bringing the") && !line.StartsWith("Scan, explore") &&
                      !line.StartsWith("Transforming how") && !line.StartsWith("Making technology") && !line.StartsWith("Your bridge") &&
                      !line.StartsWith("Inspiring the") && !line.StartsWith("Where imagination") && !line.StartsWith("Unlocking the") &&
                      !line.StartsWith("Your gateway") && !line.StartsWith("The showcase") && !line.StartsWith("Made by") &&
                      !line.StartsWith("Developed by") && !line.StartsWith("Randomly choose") && !line.StartsWith("Give a short") &&
                      !line.StartsWith("Always vary") && !line.StartsWith("Keep the") && !line.StartsWith("Most Creative") &&
                      !line.StartsWith("Most Technical") && !line.StartsWith("Keep answers") && !line.StartsWith("Example variation") &&
                      !line.StartsWith("FYP:") && !line.StartsWith("VR/AR:") && !line.StartsWith("3D Modeling:") &&
                      !line.StartsWith("Video Game:") && !line.StartsWith("Audio Production:") && !line.StartsWith("Game Environment:") &&
                      !line.StartsWith("Board Game:") && !line.StartsWith("2D/3D Animation:") && !line.StartsWith("Each reply") &&
                      !line.StartsWith("When asked") && !line.StartsWith("Teams List") && !line.StartsWith("Organizing Team") &&
                      !line.StartsWith("Clerical Team") && !line.StartsWith("Event Planner Team") && !line.StartsWith("General Affair Team") &&
                      !line.StartsWith("Sponsorship Team") && !line.StartsWith("Multimedia Team") && !line.StartsWith("Photography") &&
                      !line.StartsWith("Technical Team") && !line.StartsWith("Other Key Questions") && !line.StartsWith("Who are") &&
                      !line.StartsWith("Who is") && !line.StartsWith("Always include") && !line.StartsWith("This answer") &&
                      !line.StartsWith("Keep Rendify") && !line.StartsWith("The Student Helpers") && !line.StartsWith("The Head of DMT") &&
                      !line.StartsWith("Must vary") && !line.StartsWith("DMT is headed") && !line.StartsWith("Ms. Heng")))
            {
                // Clean up the answer
                string answer = line.Trim();
                if (answer.StartsWith("\"") && answer.EndsWith("\""))
                {
                    answer = answer.Substring(1, answer.Length - 2);
                }
                
                if (!string.IsNullOrEmpty(answer) && answer.Length > 3)
                {
                    currentAnswers.Add(answer);
                }
            }
        }
        
        // Save the last question
        if (!string.IsNullOrEmpty(currentQuestion) && currentAnswers.Count > 0)
        {
            answerVariations[currentQuestion.ToLower()] = new List<string>(currentAnswers);
        }
        
        // Add special cases
        AddSpecialCases();
        
        Debug.Log($"Loaded {answerVariations.Count} Q&A pairs with variations");
    }
    
    void AddSpecialCases()
    {
        // Add special cases based on the text file content
        answerVariations["who are you?"] = new List<string> {
            "Meet Rendify, your digital creativity companion.",
            "I'm your interactive showcase navigator.",
            "Your AI-powered exhibition assistant at your service.",
            "Welcome! I'm your virtual tour guide for today.",
            "Ready to explore amazing student projects together?",
            "Here to make your showcase journey unforgettable.",
            "Your enthusiastic guide to digital innovation.",
            "Bringing the future of learning to life.",
            "Scan, explore, and discover with me as your guide.",
            "Transforming how you experience student showcases.",
            "Making technology accessible and exciting for everyone.",
            "Your bridge between creativity and cutting-edge tech.",
            "Inspiring the next generation of digital creators.",
            "Where imagination meets innovation – let's explore!",
            "Unlocking the potential of immersive learning experiences.",
            "Your gateway to tomorrow's creative possibilities."
        };
        
        answerVariations["what is your name?"] = new List<string> {
            "My name is Rendify, your digital companion.",
            "I'm Rendify, your interactive showcase guide.",
            "Rendify here, ready to assist you today.",
            "Call me Rendify, your virtual tour assistant.",
            "I'm Rendify, your AI-powered exhibition helper."
        };
        
        answerVariations["what is your slogan?"] = new List<string> {
            "Realize, Digitize, Amplify"
        };
        
        answerVariations["who developed you?"] = new List<string> {
            "Developed by Technical Team: Renee Chin Wei Wen, Hoo Zhi Ling, Ling Heng Hua, Goh Siew Chin, with support from Organizing Team member Joshua Lau Lik Seng."
        };
        
        answerVariations["who made the dmt showcase?"] = new List<string> {
            "Made by DMT Students and Lecturers"
        };
        
        answerVariations["how many courses are in the showcase?"] = new List<string> {
            "The showcase features 9 categories including Board Game, AVL/NLE/VFX/Audio/Animation, 3D Modeling, Game Environment, Video Game, Augmented Reality (AR), Mobile Application, Virtual Reality (VR), and Final Year Projects."
        };
        
        // Add team information
        answerVariations["who is in the organizing team?"] = new List<string> {
            "Led by Tan Xin Huey, includes Khor Jia Hui, Bryan Law Ee Jiun, Joshua Lau Lik Seng, Er Zheng Yang.",
            "Tan Xin Huey heads the team, supported by Khor Jia Hui, Bryan Law Ee Jiun, Joshua Lau Lik Seng, Er Zheng Yang.",
            "Organizing Team consists of Tan Xin Huey (leader), Khor Jia Hui, Bryan Law Ee Jiun, Joshua Lau Lik Seng, Er Zheng Yang."
        };
        
        answerVariations["who is in the clerical team?"] = new List<string> {
            "Led by Teoh Rui Rong, includes Chong Han Byn, Chin Zhi Ni, Leow Mei Qi, Lim Yan Ying.",
            "Teoh Rui Rong heads the team, supported by Chong Han Byn, Chin Zhi Ni, Leow Mei Qi, Lim Yan Ying.",
            "Clerical Team consists of Teoh Rui Rong (leader), Chong Han Byn, Chin Zhi Ni, Leow Mei Qi, Lim Yan Ying."
        };
        
        answerVariations["who is in the event planner team?"] = new List<string> {
            "Led by Liu Yunyi, includes Angel Ling Zhi, Chan Yikka, Zhu Ziqi.",
            "Liu Yunyi heads the team, supported by Angel Ling Zhi, Chan Yikka, Zhu Ziqi.",
            "Event Planner Team consists of Liu Yunyi (leader), Angel Ling Zhi, Chan Yikka, Zhu Ziqi."
        };
        
        answerVariations["who is in the general affair team?"] = new List<string> {
            "Led by Tan Wei Ping, includes Kho Renjiro, Teh Hung Tao, Lim Jia Zheng, Chew Kee Koon, Tan Hao Xuan.",
            "Tan Wei Ping heads the team, supported by Kho Renjiro, Teh Hung Tao, Lim Jia Zheng, Chew Kee Koon, Tan Hao Xuan.",
            "General Affair Team consists of Tan Wei Ping (leader), Kho Renjiro, Teh Hung Tao, Lim Jia Zheng, Chew Kee Koon, Tan Hao Xuan."
        };
        
        answerVariations["who is in the sponsorship team?"] = new List<string> {
            "Led by Ooi Lewei, includes Li Wanning, Mi Yuhan, Shao QiuTing, Lyu WanYun.",
            "Ooi Lewei heads the team, supported by Li Wanning, Mi Yuhan, Shao QiuTing, Lyu WanYun.",
            "Sponsorship Team consists of Ooi Lewei (leader), Li Wanning, Mi Yuhan, Shao QiuTing, Lyu WanYun."
        };
        
        answerVariations["who is in the multimedia team?"] = new List<string> {
            "Led by Ooi Yin, includes Yap Shuyi, Lin Yihan, Ellena Gazali, Ong Yen Xin, Hoo Min Hui, Wang Xizhen, Tan Yi Heng.",
            "Ooi Yin heads the team, supported by Yap Shuyi, Lin Yihan, Ellena Gazali, Ong Yen Xin, Hoo Min Hui, Wang Xizhen, Tan Yi Heng.",
            "Multimedia Team consists of Ooi Yin (leader), Yap Shuyi, Lin Yihan, Ellena Gazali, Ong Yen Xin, Hoo Min Hui, Wang Xizhen, Tan Yi Heng."
        };
        
        answerVariations["who is in the photography & videography team?"] = new List<string> {
            "Led by Tan Hao, includes Wilson Wong, Wu Qianyi.",
            "Tan Hao heads the team, supported by Wilson Wong, Wu Qianyi.",
            "Photography & Videography Team consists of Tan Hao (leader), Wilson Wong, Wu Qianyi."
        };
        
        answerVariations["who is the technical team?"] = new List<string> {
            "Led by Renee Chin Wei Wen, includes Hoo Zhi Ling, Ling Heng Hua, Goh Siew Chin.",
            "Renee Chin Wei Wen heads the team, supported by Hoo Zhi Ling, Ling Heng Hua, Goh Siew Chin.",
            "Technical Team consists of Renee Chin Wei Wen (leader), Hoo Zhi Ling, Ling Heng Hua, Goh Siew Chin."
        };
        
        answerVariations["who are the student helpers?"] = new List<string> {
            "The Student Helpers are Bouali Douaa, Ho Hui San, Gui Shi Jun, Yu WeiJie, Lin KeJun, Gan Yi Lin, Emily Lim Jia Qi, Woo Feng Yuan, Khaw Han Zhe, Lee Yi Qin, Khoo Jia Hui, and Altana Flegontova."
        };
        
        answerVariations["who is the head of dmt?"] = new List<string> {
            "The Head of DMT is Ms. Heng Yu Ping.",
            "DMT is headed by Ms. Heng Yu Ping.",
            "Ms. Heng Yu Ping leads the DMT department."
        };
        
        // Add course-related questions
        answerVariations["tell me about the fyp."] = new List<string> {
            "FYP showcases student capstone achievements.",
            "FYP demonstrates practical problem-solving skills.",
            "FYP integrates multidisciplinary learning approaches."
        };
        
        answerVariations["what is vr/ar?"] = new List<string> {
            "VR/AR creates interactive learning environments.",
            "VR/AR develops spatial computing applications.",
            "VR/AR transforms entertainment industry standards."
        };
        
        answerVariations["tell me about video games."] = new List<string> {
            "Video Game develops narrative storytelling mechanics.",
            "Video Game creates multiplayer gaming experiences.",
            "Video Game integrates advanced physics engines."
        };
        
        answerVariations["what is a board game?"] = new List<string> {
            "Board Game develops strategic gameplay mechanics.",
            "Board Game creates engaging social experiences.",
            "Board Game balances competitive rule systems."
        };
        
        answerVariations["tell me about 3d modeling."] = new List<string> {
            "3D Modeling designs architectural visualization projects.",
            "3D Modeling produces character animation assets.",
            "3D Modeling enables rapid prototyping solutions."
        };
        
        answerVariations["what is 2d/3d animation?"] = new List<string> {
            "2D/3D Animation brings characters to life.",
            "2D/3D Animation creates fluid motion sequences.",
            "2D/3D Animation develops visual storytelling techniques."
        };
        
        answerVariations["tell me about game environments."] = new List<string> {
            "Game Environment builds immersive virtual worlds.",
            "Game Environment designs atmospheric level layouts.",
            "Game Environment creates realistic terrain systems."
        };
        
        answerVariations["what is video/audio production?"] = new List<string> {
            "Audio Production masters professional recording techniques.",
            "Audio Production creates cinematic soundscapes.",
            "Audio Production develops podcast broadcasting skills."
        };
        
        // Add creative/technical course questions
        answerVariations["which course is the most creative?"] = new List<string> {
            "2D/3D Animation is creative, blending art with movement.",
            "Board Game course feels creative through storytelling and design.",
            "Game Environment shines creatively by building immersive worlds.",
            "3D Modeling is creative, shaping endless digital possibilities."
        };
        
        answerVariations["which course is the most technical?"] = new List<string> {
            "VR/AR is highly technical, requiring advanced interaction systems.",
            "Video Production is technical with editing and visual effects.",
            "Audio Production gets technical through mixing and sound engineering.",
            "FYP can be technical, combining research with implementation."
        };
    }
    
    public string GetAnswer(string question)
    {
        if (string.IsNullOrEmpty(question))
            return "I'm sorry, I don't know. I am designed only for DMT Showcase content.";
        
        string normalizedQuestion = question.ToLower().Trim();
        
        // Try exact match first
        if (answerVariations.ContainsKey(normalizedQuestion))
        {
            return GetRandomAnswer(answerVariations[normalizedQuestion]);
        }
        
        // Try partial matches
        foreach (var kvp in answerVariations)
        {
            if (normalizedQuestion.Contains(kvp.Key) || kvp.Key.Contains(normalizedQuestion))
            {
                return GetRandomAnswer(kvp.Value);
            }
        }
        
        // Check for specific keywords
        if (normalizedQuestion.Contains("who") && normalizedQuestion.Contains("team"))
        {
            if (normalizedQuestion.Contains("organizing"))
                return GetRandomAnswer(answerVariations["who is in the organizing team?"]);
            else if (normalizedQuestion.Contains("clerical"))
                return GetRandomAnswer(answerVariations["who is in the clerical team?"]);
            else if (normalizedQuestion.Contains("event"))
                return GetRandomAnswer(answerVariations["who is in the event planner team?"]);
            else if (normalizedQuestion.Contains("general"))
                return GetRandomAnswer(answerVariations["who is in the general affair team?"]);
            else if (normalizedQuestion.Contains("sponsorship"))
                return GetRandomAnswer(answerVariations["who is in the sponsorship team?"]);
            else if (normalizedQuestion.Contains("multimedia"))
                return GetRandomAnswer(answerVariations["who is in the multimedia team?"]);
            else if (normalizedQuestion.Contains("photography") || normalizedQuestion.Contains("videography"))
                return GetRandomAnswer(answerVariations["who is in the photography & videography team?"]);
            else if (normalizedQuestion.Contains("technical"))
                return GetRandomAnswer(answerVariations["who is the technical team?"]);
        }
        
        if (normalizedQuestion.Contains("student helper"))
            return GetRandomAnswer(answerVariations["who are the student helpers?"]);
        
        if (normalizedQuestion.Contains("head") && normalizedQuestion.Contains("dmt"))
            return GetRandomAnswer(answerVariations["who is the head of dmt?"]);
        
        if (normalizedQuestion.Contains("course") && normalizedQuestion.Contains("creative"))
            return GetRandomAnswer(answerVariations["which course is the most creative?"]);
        
        if (normalizedQuestion.Contains("course") && normalizedQuestion.Contains("technical"))
            return GetRandomAnswer(answerVariations["which course is the most technical?"]);
        
        if (normalizedQuestion.Contains("fyp"))
            return GetRandomAnswer(answerVariations["tell me about the fyp."]);
        
        if (normalizedQuestion.Contains("vr") || normalizedQuestion.Contains("ar"))
            return GetRandomAnswer(answerVariations["what is vr/ar?"]);
        
        if (normalizedQuestion.Contains("video game"))
            return GetRandomAnswer(answerVariations["tell me about video games."]);
        
        if (normalizedQuestion.Contains("board game"))
            return GetRandomAnswer(answerVariations["what is a board game?"]);
        
        if (normalizedQuestion.Contains("3d modeling") || normalizedQuestion.Contains("3d model"))
            return GetRandomAnswer(answerVariations["tell me about 3d modeling."]);
        
        if (normalizedQuestion.Contains("animation"))
            return GetRandomAnswer(answerVariations["what is 2d/3d animation?"]);
        
        if (normalizedQuestion.Contains("game environment"))
            return GetRandomAnswer(answerVariations["tell me about game environments."]);
        
        if (normalizedQuestion.Contains("video production") || normalizedQuestion.Contains("audio production"))
            return GetRandomAnswer(answerVariations["what is video/audio production?"]);
        
        // Default response for unrelated questions
        return "I'm sorry, I don't know. I am designed only for DMT Showcase content.";
    }
    
    private string GetRandomAnswer(List<string> answers)
    {
        if (answers == null || answers.Count == 0)
            return "I'm sorry, I don't know. I am designed only for DMT Showcase content.";
        
        if (enableVariations && answers.Count > 1)
        {
            return answers[Random.Range(0, answers.Count)];
        }
        else
        {
            return answers[0];
        }
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
