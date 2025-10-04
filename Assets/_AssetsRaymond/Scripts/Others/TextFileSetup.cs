using UnityEngine;
using System.IO;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;

public class TextFileSetup : MonoBehaviour
{
    [Header("Text File Setup")]
    [SerializeField] private string sourceFilePath = "e:\\Downloads\\AR Mascot.txt";
    [SerializeField] private string targetPath = "Assets/Resources/AR_Mascot.txt";
    
    [ContextMenu("Copy Text File to Resources")]
    public void CopyTextFileToResources()
    {
        if (File.Exists(sourceFilePath))
        {
            // Ensure Resources folder exists
            string resourcesPath = "Assets/Resources";
            if (!AssetDatabase.IsValidFolder(resourcesPath))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }
            
            // Copy the file
            File.Copy(sourceFilePath, targetPath, true);
            AssetDatabase.Refresh();
            
            Debug.Log($"Text file copied from {sourceFilePath} to {targetPath}");
            
            // Find the QnaManager and assign the text file
            QnaManager qnaManager = FindObjectOfType<QnaManager>();
            if (qnaManager != null)
            {
                TextAsset textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(targetPath);
                if (textAsset != null)
                {
                    // Use reflection to set the private field
                    var field = typeof(QnaManager).GetField("qnaTextFile", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (field != null)
                    {
                        field.SetValue(qnaManager, textAsset);
                        EditorUtility.SetDirty(qnaManager);
                        Debug.Log("QnaManager text file assigned automatically!");
                    }
                }
            }
        }
        else
        {
            Debug.LogError($"Source file not found at {sourceFilePath}. Please update the path in the inspector.");
        }
    }
    
    [ContextMenu("Validate Text File")]
    public void ValidateTextFile()
    {
        if (File.Exists(targetPath))
        {
            string content = File.ReadAllText(targetPath);
            string[] lines = content.Split('\n');
            
            int questionCount = 0;
            int answerCount = 0;
            
            foreach (string line in lines)
            {
                string trimmedLine = line.Trim();
                if (trimmedLine.EndsWith("?"))
                    questionCount++;
                if (trimmedLine.StartsWith("\"") && trimmedLine.EndsWith("\""))
                    answerCount++;
            }
            
            Debug.Log($"Text file validation complete:");
            Debug.Log($"- Total lines: {lines.Length}");
            Debug.Log($"- Questions found: {questionCount}");
            Debug.Log($"- Answers found: {answerCount}");
            Debug.Log($"- File size: {content.Length} characters");
        }
        else
        {
            Debug.LogError($"Target file not found at {targetPath}. Run 'Copy Text File to Resources' first.");
        }
    }
    
    [ContextMenu("Setup QnaManager References")]
    public void SetupQnaManagerReferences()
    {
        QnaManager qnaManager = FindObjectOfType<QnaManager>();
        if (qnaManager == null)
        {
            Debug.LogError("QnaManager not found in scene!");
            return;
        }
        
        // Find ChatGPTManager to get references
        ChatGPT chatGPT = FindObjectOfType<ChatGPT>();
        if (chatGPT != null)
        {
            // Use reflection to copy references from ChatGPT to QnaManager
            var chatGPTType = typeof(ChatGPT);
            var qnaManagerType = typeof(QnaManager);
            
            // Copy UI references
            CopyField(chatGPT, qnaManager, "button");
            CopyField(chatGPT, qnaManager, "inputField");
            CopyField(chatGPT, qnaManager, "dropDownText");
            CopyField(chatGPT, qnaManager, "displayText");
            
            // Copy Mascot Dialogue references
            CopyField(chatGPT, qnaManager, "mascotController");
            CopyField(chatGPT, qnaManager, "audioManager");
            CopyField(chatGPT, qnaManager, "canvas");
            CopyField(chatGPT, qnaManager, "mascotDialogue");
            CopyField(chatGPT, qnaManager, "interval");
            CopyField(chatGPT, qnaManager, "is3DText");
            CopyField(chatGPT, qnaManager, "MascotProfile");
            
            EditorUtility.SetDirty(qnaManager);
            Debug.Log("QnaManager references copied from ChatGPT successfully!");
        }
        else
        {
            Debug.LogError("ChatGPT not found in scene! Please make sure ChatGPTManager is in the scene.");
        }
    }
    
    [ContextMenu("Populate Default Answers from Text File")]
    public void PopulateDefaultAnswers()
    {
        QnaManager qnaManager = FindObjectOfType<QnaManager>();
        if (qnaManager == null)
        {
            Debug.LogError("QnaManager not found in scene!");
            return;
        }
        
        if (File.Exists(sourceFilePath))
        {
            string content = File.ReadAllText(sourceFilePath);
            string[] lines = content.Split('\n');
            
            List<string> answers = new List<string>();
            
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
                        answers.Add(answer);
                    }
                }
                // Look for other potential answers
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
                    answers.Add(trimmedLine);
                }
            }
            
            // Add special case answers
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
            
            answers.AddRange(specialAnswers);
            
            // Use reflection to set the default answers
            var field = typeof(QnaManager).GetField("defaultAnswers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(qnaManager, answers);
                EditorUtility.SetDirty(qnaManager);
                Debug.Log($"Populated {answers.Count} default answers in QnaManager inspector!");
            }
        }
        else
        {
            Debug.LogError($"Source file not found at {sourceFilePath}. Please update the path in the inspector.");
        }
    }
    
    private void CopyField(object source, object target, string fieldName)
    {
        var sourceField = source.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var targetField = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (sourceField != null && targetField != null)
        {
            var value = sourceField.GetValue(source);
            targetField.SetValue(target, value);
        }
    }
}
#endif
