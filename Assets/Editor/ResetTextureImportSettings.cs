using UnityEngine;
using UnityEditor;
using System.IO;

public class ResetTextureImportSettings
{
    [MenuItem("Tools/Reset Texture Import Settings")]
    public static void ResetAllTextureImportSettings()
    {
        string[] textureGuids = AssetDatabase.FindAssets("t:Texture2D");
        int processed = 0;
        
        foreach (string guid in textureGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            
            if (importer != null)
            {
                // Reset to default settings
                importer.textureType = TextureImporterType.Default;
                importer.textureShape = TextureImporterShape.Texture2D;
                importer.sRGBTexture = true;
                importer.alphaSource = TextureImporterAlphaSource.FromInput;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = true;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.filterMode = FilterMode.Bilinear;
                
                // Clear platform-specific settings
                importer.ClearPlatformTextureSettings("Android");
                importer.ClearPlatformTextureSettings("iPhone");
                importer.ClearPlatformTextureSettings("WebGL");
                importer.ClearPlatformTextureSettings("Standalone");
                
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                processed++;
            }
        }
        
        AssetDatabase.Refresh();
        Debug.Log($"Reset import settings for {processed} textures.");
    }
}
