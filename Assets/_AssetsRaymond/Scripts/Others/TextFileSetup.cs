using UnityEngine;
using UnityEditor;
using System.IO;

#if UNITY_EDITOR
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
