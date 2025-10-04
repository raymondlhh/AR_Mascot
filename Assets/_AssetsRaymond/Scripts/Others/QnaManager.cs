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
        // Add null checks to prevent NullReferenceException
        if (audioManager == null)
        {
            Debug.LogError("AudioManager is not assigned!");
            return;
        }
        
        if (dropDownText == null)
        {
            Debug.LogError("DropDownText is not assigned!");
            return;
        }
        
        if (displayText == null)
        {
            Debug.LogError("DisplayText is not assigned!");
            return;
        }
        
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
        if (button != null) button.enabled = false;
        if (inputField != null) inputField.enabled = false;
        isDisplayTextFinished = false; // Set to false immediately when button is clicked

        audioManager.PlaySFX("ButtonClick2");
        if (canvas != null) canvas.ShowTargetByName("Mascot_Idle");
        
        userInput = dropDownText.options[dropDownText.value].text;
        chatHistory += $"{userInput}\n";

        if (inputField != null) inputField.text = "";

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
        // Use default answers from inspector instead of text file parsing
        if (defaultAnswers != null && defaultAnswers.Count > 0)
        {
            Debug.Log($"Using {defaultAnswers.Count} default answers from inspector");
            return;
        }
        
        // Fallback to text file if no default answers are set
        if (qnaTextFile == null)
        {
            Debug.LogWarning("QnA Text File is not assigned and no default answers set!");
            return;
        }
        
        // Parse text file and populate default answers
        PopulateDefaultAnswersFromTextFile();
        
        Debug.Log($"Loaded {defaultAnswers.Count} default answers from text file");
    }
    
    void PopulateDefaultAnswersFromTextFile()
    {
        if (qnaTextFile == null) return;
        
        string[] lines = qnaTextFile.text.Split('\n');
        defaultAnswers.Clear();
        
        foreach (string line in lines)
        {
            string trimmedLine = line.Trim();
            
            // Skip empty lines and comments
            if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("//") || trimmedLine.StartsWith("#"))
                continue;
                
            // Look for quoted answers
            if (trimmedLine.StartsWith("\"") && trimmedLine.EndsWith("\""))
            {
                string answer = trimmedLine.Substring(1, trimmedLine.Length - 2);
                if (!string.IsNullOrEmpty(answer) && answer.Length > 3)
                {
                    defaultAnswers.Add(answer);
                }
            }
            // Look for other potential answers (lines that look like responses)
            else if (trimmedLine.Length > 5 && !trimmedLine.StartsWith("🔹") && !trimmedLine.StartsWith("⚡") && 
                     !trimmedLine.StartsWith("👉") && !trimmedLine.StartsWith("Special Case") && 
                     !trimmedLine.StartsWith("Example") && !trimmedLine.StartsWith("Teams List") &&
                     !trimmedLine.StartsWith("Leader:") && !trimmedLine.StartsWith("Members:") &&
                     !trimmedLine.StartsWith("Who are") && !trimmedLine.StartsWith("Who is") &&
                     !trimmedLine.StartsWith("Who made") && !trimmedLine.StartsWith("Who developed") &&
                     !trimmedLine.StartsWith("If the user asks") && !trimmedLine.StartsWith("Instruction for AI") &&
                     !trimmedLine.StartsWith("Keep answers") && !trimmedLine.StartsWith("Each reply") &&
                     !trimmedLine.StartsWith("When asked") && !trimmedLine.StartsWith("Rule:") &&
                     !trimmedLine.StartsWith("Use") && !trimmedLine.StartsWith("Sometimes") &&
                     !trimmedLine.StartsWith("Organizing Team") && !trimmedLine.StartsWith("Clerical Team") &&
                     !trimmedLine.StartsWith("Event Planner Team") && !trimmedLine.StartsWith("General Affair Team") &&
                     !trimmedLine.StartsWith("Sponsorship Team") && !trimmedLine.StartsWith("Multimedia Team") &&
                     !trimmedLine.StartsWith("Photography") && !trimmedLine.StartsWith("Technical Team") &&
                     !trimmedLine.StartsWith("Other Key Questions") && !trimmedLine.StartsWith("Always include") &&
                     !trimmedLine.StartsWith("This answer") && !trimmedLine.StartsWith("Keep Rendify") &&
                     !trimmedLine.StartsWith("The Student Helpers") && !trimmedLine.StartsWith("The Head of DMT") &&
                     !trimmedLine.StartsWith("Must vary") && !trimmedLine.StartsWith("DMT is headed") &&
                     !trimmedLine.StartsWith("Ms. Heng") && !trimmedLine.StartsWith("Tone:") && 
                     !trimmedLine.StartsWith("Your name") && !trimmedLine.StartsWith("You are") &&
                     !trimmedLine.StartsWith("Answer length") && !trimmedLine.StartsWith("Variation:") &&
                     !trimmedLine.StartsWith("Never repeat") && !trimmedLine.StartsWith("The AI must") &&
                     !trimmedLine.StartsWith("Use synonyms") && !trimmedLine.StartsWith("Change order") &&
                     !trimmedLine.StartsWith("Switch style") && !trimmedLine.StartsWith("Unrelated questions") &&
                     !trimmedLine.StartsWith("Always reply") && !trimmedLine.StartsWith("Slogan:") &&
                     !trimmedLine.StartsWith("Must always") && !trimmedLine.StartsWith("Realize") &&
                     !trimmedLine.StartsWith("Always 5") && !trimmedLine.StartsWith("Example variations") &&
                     !trimmedLine.StartsWith("Meet Rendify") && !trimmedLine.StartsWith("I'm your") &&
                     !trimmedLine.StartsWith("Your AI-powered") && !trimmedLine.StartsWith("Welcome!") &&
                     !trimmedLine.StartsWith("Ready to") && !trimmedLine.StartsWith("Here to") &&
                     !trimmedLine.StartsWith("Your enthusiastic") && !trimmedLine.StartsWith("Bringing the") &&
                     !trimmedLine.StartsWith("Scan, explore") && !trimmedLine.StartsWith("Transforming how") &&
                     !trimmedLine.StartsWith("Making technology") && !trimmedLine.StartsWith("Your bridge") &&
                     !trimmedLine.StartsWith("Inspiring the") && !trimmedLine.StartsWith("Where imagination") &&
                     !trimmedLine.StartsWith("Unlocking the") && !trimmedLine.StartsWith("Your gateway") &&
                     !trimmedLine.StartsWith("The showcase") && !trimmedLine.StartsWith("Made by") &&
                     !trimmedLine.StartsWith("Developed by") && !trimmedLine.StartsWith("Randomly choose") &&
                     !trimmedLine.StartsWith("Give a short") && !trimmedLine.StartsWith("Always vary") &&
                     !trimmedLine.StartsWith("Keep the") && !trimmedLine.StartsWith("Most Creative") &&
                     !trimmedLine.StartsWith("Most Technical") && !trimmedLine.StartsWith("Keep answers") &&
                     !trimmedLine.StartsWith("Example variation") && !trimmedLine.StartsWith("FYP:") &&
                     !trimmedLine.StartsWith("VR/AR:") && !trimmedLine.StartsWith("3D Modeling:") &&
                     !trimmedLine.StartsWith("Video Game:") && !trimmedLine.StartsWith("Audio Production:") &&
                     !trimmedLine.StartsWith("Game Environment:") && !trimmedLine.StartsWith("Board Game:") &&
                     !trimmedLine.StartsWith("2D/3D Animation:") && !trimmedLine.StartsWith("Each reply") &&
                     !trimmedLine.StartsWith("When asked") && !trimmedLine.StartsWith("Teams List") &&
                     !trimmedLine.StartsWith("Organizing Team") && !trimmedLine.StartsWith("Clerical Team") &&
                     !trimmedLine.StartsWith("Event Planner Team") && !trimmedLine.StartsWith("General Affair Team") &&
                     !trimmedLine.StartsWith("Sponsorship Team") && !trimmedLine.StartsWith("Multimedia Team") &&
                     !trimmedLine.StartsWith("Photography") && !trimmedLine.StartsWith("Technical Team") &&
                     !trimmedLine.StartsWith("Other Key Questions") && !trimmedLine.StartsWith("Who are") &&
                     !trimmedLine.StartsWith("Who is") && !trimmedLine.StartsWith("Always include") &&
                     !trimmedLine.StartsWith("This answer") && !trimmedLine.StartsWith("Keep Rendify") &&
                     !trimmedLine.StartsWith("The Student Helpers") && !trimmedLine.StartsWith("The Head of DMT") &&
                     !trimmedLine.StartsWith("Must vary") && !trimmedLine.StartsWith("DMT is headed") &&
                     !trimmedLine.StartsWith("Ms. Heng"))
            {
                defaultAnswers.Add(trimmedLine);
            }
        }
        
        // Add special cases to default answers
        AddSpecialCasesToDefaultAnswers();
    }
    
    void AddSpecialCasesToDefaultAnswers()
    {
        // Add all the special case answers to the default answers list
        List<string> specialAnswers = new List<string>
        {
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
            "Your gateway to tomorrow's creative possibilities.",
            "Realize, Digitize, Amplify",
            "Developed by Technical Team: Renee Chin Wei Wen, Hoo Zhi Ling, Ling Heng Hua, Goh Siew Chin, with support from Organizing Team member Joshua Lau Lik Seng.",
            "Made by DMT Students and Lecturers",
            "The showcase features 9 categories including Board Game, AVL/NLE/VFX/Audio/Animation, 3D Modeling, Game Environment, Video Game, Augmented Reality (AR), Mobile Application, Virtual Reality (VR), and Final Year Projects.",
            "FYP showcases student capstone achievements.",
            "FYP demonstrates practical problem-solving skills.",
            "FYP integrates multidisciplinary learning approaches.",
            "VR/AR creates interactive learning environments.",
            "VR/AR develops spatial computing applications.",
            "VR/AR transforms entertainment industry standards.",
            "3D Modeling designs architectural visualization projects.",
            "3D Modeling produces character animation assets.",
            "3D Modeling enables rapid prototyping solutions.",
            "Video Game develops narrative storytelling mechanics.",
            "Video Game creates multiplayer gaming experiences.",
            "Video Game integrates advanced physics engines.",
            "Audio Production masters professional recording techniques.",
            "Audio Production creates cinematic soundscapes.",
            "Audio Production develops podcast broadcasting skills.",
            "Game Environment builds immersive virtual worlds.",
            "Game Environment designs atmospheric level layouts.",
            "Game Environment creates realistic terrain systems.",
            "Board Game develops strategic gameplay mechanics.",
            "Board Game creates engaging social experiences.",
            "Board Game balances competitive rule systems.",
            "2D/3D Animation brings characters to life.",
            "2D/3D Animation creates fluid motion sequences.",
            "2D/3D Animation develops visual storytelling techniques.",
            "The Student Helpers are Bouali Douaa, Ho Hui San, Gui Shi Jun, Yu WeiJie, Lin KeJun, Gan Yi Lin, Emily Lim Jia Qi, Woo Feng Yuan, Khaw Han Zhe, Lee Yi Qin, Khoo Jia Hui, and Altana Flegontova.",
            "The Head of DMT is Ms. Heng Yu Ping.",
            "DMT is headed by Ms. Heng Yu Ping.",
            "Ms. Heng Yu Ping leads the DMT department."
        };
        
        defaultAnswers.AddRange(specialAnswers);
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
        
        // First try to match specific questions with their answers
        string specificAnswer = GetSpecificAnswer(normalizedQuestion);
        if (specificAnswer != null)
        {
            return specificAnswer;
        }
        
        // If no specific match found, use default answers as fallback
        if (defaultAnswers != null && defaultAnswers.Count > 0)
        {
            return GetRandomAnswerFromDefault();
        }
        
        // Final fallback to answer variations
        
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
    
    private string GetSpecificAnswer(string normalizedQuestion)
    {
        // About Rendify questions
        if (normalizedQuestion.Contains("who are you") || normalizedQuestion.Contains("what are you"))
        {
            return GetRandomAnswer(new List<string> {
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
            });
        }
        
        // Specific questions from the dropdown
        if (normalizedQuestion.Contains("can you tell me about rendify"))
        {
            return GetRandomAnswer(new List<string> {
                "Rendify is your digital creativity companion for the DMT Showcase.",
                "I'm Rendify, the interactive AR mascot guiding you through amazing projects.",
                "Rendify brings the future of learning to life through immersive technology.",
                "Meet Rendify, your AI-powered exhibition assistant at your service.",
                "I'm Rendify, transforming how you experience student showcases."
            });
        }
        
        if (normalizedQuestion.Contains("why are you at this showcase"))
        {
            return GetRandomAnswer(new List<string> {
                "I'm here to guide you through the amazing DMT student projects.",
                "To showcase the incredible creativity and innovation of our students.",
                "I'm here to make your showcase journey unforgettable and inspiring.",
                "To demonstrate how technology can enhance learning experiences.",
                "I'm here to bridge the gap between creativity and cutting-edge tech."
            });
        }
        
        if (normalizedQuestion.Contains("what do you do"))
        {
            return GetRandomAnswer(new List<string> {
                "I guide visitors through the DMT Showcase and answer questions.",
                "I help showcase student projects and explain different courses.",
                "I provide information about teams, courses, and the showcase itself.",
                "I make the showcase experience interactive and engaging for everyone.",
                "I demonstrate how AR technology can enhance educational experiences."
            });
        }
        
        if (normalizedQuestion.Contains("what makes you special"))
        {
            return GetRandomAnswer(new List<string> {
                "I combine AI technology with AR to create an immersive experience.",
                "I can answer questions about all aspects of the DMT Showcase.",
                "I bring student projects to life through interactive 3D technology.",
                "I make learning fun and engaging through cutting-edge AR technology.",
                "I'm the first AR mascot designed specifically for educational showcases."
            });
        }
        
        if (normalizedQuestion.Contains("what is your name"))
        {
            return GetRandomAnswer(new List<string> {
                "My name is Rendify, your digital companion.",
                "I'm Rendify, your interactive showcase guide.",
                "Rendify here, ready to assist you today.",
                "Call me Rendify, your virtual tour assistant.",
                "I'm Rendify, your AI-powered exhibition helper."
            });
        }
        
        if (normalizedQuestion.Contains("what is your slogan"))
        {
            return "Realize, Digitize, Amplify";
        }
        
        if (normalizedQuestion.Contains("who developed you"))
        {
            return "Developed by Technical Team: Renee Chin Wei Wen, Hoo Zhi Ling, Ling Heng Hua, Goh Siew Chin, with support from Organizing Team member Joshua Lau Lik Seng.";
        }
        
        if (normalizedQuestion.Contains("who made the dmt showcase"))
        {
            return "Made by DMT Students and Lecturers";
        }
        
        if (normalizedQuestion.Contains("how many courses") || normalizedQuestion.Contains("how many course"))
        {
            return "The showcase features 9 categories including Board Game, AVL/NLE/VFX/Audio/Animation, 3D Modeling, Game Environment, Video Game, Augmented Reality (AR), Mobile Application, Virtual Reality (VR), and Final Year Projects.";
        }
        
        // Course questions
        if (normalizedQuestion.Contains("tell me about the fyp") || normalizedQuestion.Contains("what is fyp"))
        {
            return GetRandomAnswer(new List<string> {
                "FYP showcases student capstone achievements.",
                "FYP demonstrates practical problem-solving skills.",
                "FYP integrates multidisciplinary learning approaches."
            });
        }
        
        if (normalizedQuestion.Contains("what is vr") || normalizedQuestion.Contains("what is ar") || normalizedQuestion.Contains("vr/ar"))
        {
            return GetRandomAnswer(new List<string> {
                "VR/AR creates interactive learning environments.",
                "VR/AR develops spatial computing applications.",
                "VR/AR transforms entertainment industry standards."
            });
        }
        
        if (normalizedQuestion.Contains("tell me about video game") || normalizedQuestion.Contains("what is video game"))
        {
            return GetRandomAnswer(new List<string> {
                "Video Game develops narrative storytelling mechanics.",
                "Video Game creates multiplayer gaming experiences.",
                "Video Game integrates advanced physics engines."
            });
        }
        
        if (normalizedQuestion.Contains("what is a board game") || normalizedQuestion.Contains("tell me about board game"))
        {
            return GetRandomAnswer(new List<string> {
                "Board Game develops strategic gameplay mechanics.",
                "Board Game creates engaging social experiences.",
                "Board Game balances competitive rule systems."
            });
        }
        
        if (normalizedQuestion.Contains("tell me about 3d modeling") || normalizedQuestion.Contains("what is 3d modeling"))
        {
            return GetRandomAnswer(new List<string> {
                "3D Modeling designs architectural visualization projects.",
                "3D Modeling produces character animation assets.",
                "3D Modeling enables rapid prototyping solutions."
            });
        }
        
        if (normalizedQuestion.Contains("what is 2d") || normalizedQuestion.Contains("what is 3d") || normalizedQuestion.Contains("animation"))
        {
            return GetRandomAnswer(new List<string> {
                "2D/3D Animation brings characters to life.",
                "2D/3D Animation creates fluid motion sequences.",
                "2D/3D Animation develops visual storytelling techniques."
            });
        }
        
        if (normalizedQuestion.Contains("tell me about game environment") || normalizedQuestion.Contains("what is game environment"))
        {
            return GetRandomAnswer(new List<string> {
                "Game Environment builds immersive virtual worlds.",
                "Game Environment designs atmospheric level layouts.",
                "Game Environment creates realistic terrain systems."
            });
        }
        
        if (normalizedQuestion.Contains("video production") || normalizedQuestion.Contains("audio production"))
        {
            return GetRandomAnswer(new List<string> {
                "Audio Production masters professional recording techniques.",
                "Audio Production creates cinematic soundscapes.",
                "Audio Production develops podcast broadcasting skills."
            });
        }
        
        // Team questions
        if (normalizedQuestion.Contains("who is in the organizing team"))
        {
            return GetRandomAnswer(new List<string> {
                "Led by Tan Xin Huey, includes Khor Jia Hui, Bryan Law Ee Jiun, Joshua Lau Lik Seng, Er Zheng Yang.",
                "Tan Xin Huey heads the team, supported by Khor Jia Hui, Bryan Law Ee Jiun, Joshua Lau Lik Seng, Er Zheng Yang.",
                "Organizing Team consists of Tan Xin Huey (leader), Khor Jia Hui, Bryan Law Ee Jiun, Joshua Lau Lik Seng, Er Zheng Yang."
            });
        }
        
        if (normalizedQuestion.Contains("who is in the clerical team"))
        {
            return GetRandomAnswer(new List<string> {
                "Led by Teoh Rui Rong, includes Chong Han Byn, Chin Zhi Ni, Leow Mei Qi, Lim Yan Ying.",
                "Teoh Rui Rong heads the team, supported by Chong Han Byn, Chin Zhi Ni, Leow Mei Qi, Lim Yan Ying.",
                "Clerical Team consists of Teoh Rui Rong (leader), Chong Han Byn, Chin Zhi Ni, Leow Mei Qi, Lim Yan Ying."
            });
        }
        
        if (normalizedQuestion.Contains("who is in the event planner team"))
        {
            return GetRandomAnswer(new List<string> {
                "Led by Liu Yunyi, includes Angel Ling Zhi, Chan Yikka, Zhu Ziqi.",
                "Liu Yunyi heads the team, supported by Angel Ling Zhi, Chan Yikka, Zhu Ziqi.",
                "Event Planner Team consists of Liu Yunyi (leader), Angel Ling Zhi, Chan Yikka, Zhu Ziqi."
            });
        }
        
        if (normalizedQuestion.Contains("who is in the general affair team"))
        {
            return GetRandomAnswer(new List<string> {
                "Led by Tan Wei Ping, includes Kho Renjiro, Teh Hung Tao, Lim Jia Zheng, Chew Kee Koon, Tan Hao Xuan.",
                "Tan Wei Ping heads the team, supported by Kho Renjiro, Teh Hung Tao, Lim Jia Zheng, Chew Kee Koon, Tan Hao Xuan.",
                "General Affair Team consists of Tan Wei Ping (leader), Kho Renjiro, Teh Hung Tao, Lim Jia Zheng, Chew Kee Koon, Tan Hao Xuan."
            });
        }
        
        if (normalizedQuestion.Contains("who is in the sponsorship team"))
        {
            return GetRandomAnswer(new List<string> {
                "Led by Ooi Lewei, includes Li Wanning, Mi Yuhan, Shao QiuTing, Lyu WanYun.",
                "Ooi Lewei heads the team, supported by Li Wanning, Mi Yuhan, Shao QiuTing, Lyu WanYun.",
                "Sponsorship Team consists of Ooi Lewei (leader), Li Wanning, Mi Yuhan, Shao QiuTing, Lyu WanYun."
            });
        }
        
        if (normalizedQuestion.Contains("who is in the multimedia team"))
        {
            return GetRandomAnswer(new List<string> {
                "Led by Ooi Yin, includes Yap Shuyi, Lin Yihan, Ellena Gazali, Ong Yen Xin, Hoo Min Hui, Wang Xizhen, Tan Yi Heng.",
                "Ooi Yin heads the team, supported by Yap Shuyi, Lin Yihan, Ellena Gazali, Ong Yen Xin, Hoo Min Hui, Wang Xizhen, Tan Yi Heng.",
                "Multimedia Team consists of Ooi Yin (leader), Yap Shuyi, Lin Yihan, Ellena Gazali, Ong Yen Xin, Hoo Min Hui, Wang Xizhen, Tan Yi Heng."
            });
        }
        
        if (normalizedQuestion.Contains("who is in the photography") || normalizedQuestion.Contains("who is in the videography"))
        {
            return GetRandomAnswer(new List<string> {
                "Led by Tan Hao, includes Wilson Wong, Wu Qianyi.",
                "Tan Hao heads the team, supported by Wilson Wong, Wu Qianyi.",
                "Photography & Videography Team consists of Tan Hao (leader), Wilson Wong, Wu Qianyi."
            });
        }
        
        if (normalizedQuestion.Contains("who is the technical team"))
        {
            return GetRandomAnswer(new List<string> {
                "Led by Renee Chin Wei Wen, includes Hoo Zhi Ling, Ling Heng Hua, Goh Siew Chin.",
                "Renee Chin Wei Wen heads the team, supported by Hoo Zhi Ling, Ling Heng Hua, Goh Siew Chin.",
                "Technical Team consists of Renee Chin Wei Wen (leader), Hoo Zhi Ling, Ling Heng Hua, Goh Siew Chin."
            });
        }
        
        if (normalizedQuestion.Contains("who are the student helpers"))
        {
            return "The Student Helpers are Bouali Douaa, Ho Hui San, Gui Shi Jun, Yu WeiJie, Lin KeJun, Gan Yi Lin, Emily Lim Jia Qi, Woo Feng Yuan, Khaw Han Zhe, Lee Yi Qin, Khoo Jia Hui, and Altana Flegontova.";
        }
        
        if (normalizedQuestion.Contains("who is the head of dmt"))
        {
            return GetRandomAnswer(new List<string> {
                "The Head of DMT is Ms. Heng Yu Ping.",
                "DMT is headed by Ms. Heng Yu Ping.",
                "Ms. Heng Yu Ping leads the DMT department."
            });
        }
        
        // Creative/Technical course questions
        if (normalizedQuestion.Contains("which course is the most creative"))
        {
            return GetRandomAnswer(new List<string> {
                "2D/3D Animation is creative, blending art with movement.",
                "Board Game course feels creative through storytelling and design.",
                "Game Environment shines creatively by building immersive worlds.",
                "3D Modeling is creative, shaping endless digital possibilities."
            });
        }
        
        if (normalizedQuestion.Contains("which course is the most technical"))
        {
            return GetRandomAnswer(new List<string> {
                "VR/AR is highly technical, requiring advanced interaction systems.",
                "Video Production is technical with editing and visual effects.",
                "Audio Production gets technical through mixing and sound engineering.",
                "FYP can be technical, combining research with implementation."
            });
        }
        
        // No specific match found
        return null;
    }
    
    private string GetRandomAnswerFromDefault()
    {
        if (defaultAnswers == null || defaultAnswers.Count == 0)
            return "I'm sorry, I don't know. I am designed only for DMT Showcase content.";
        
        if (enableVariations && defaultAnswers.Count > 1)
        {
            return defaultAnswers[Random.Range(0, defaultAnswers.Count)];
        }
        else
        {
            return defaultAnswers[0];
        }
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
