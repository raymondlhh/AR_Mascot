using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

namespace Imagine.WebAR.Editor
{
    public class PostProcessBuild_ARCam : MonoBehaviour
    {
        [PostProcessBuild]
        public static void OnPostProcessBuild(BuildTarget target, string buildPath)
        {
            // Only process WebGL builds
            if (target != BuildTarget.WebGL)
            {
                Debug.Log("PostProcessBuild_ARCam: Skipping non-WebGL build target: " + target);
                return;
            }

            string indexPath = buildPath + "/index.html";
            if (!File.Exists(indexPath))
            {
                Debug.LogError("PostProcessBuild_ARCam: index.html not found at " + indexPath);
                return;
            }

            string[] htmlLines = File.ReadAllLines(indexPath);
            
            var facingMode = ARCameraGlobalSettings.Instance.facingMode;
            if(facingMode == ARCameraGlobalSettings.FacingMode.DONT_OVERRIDE)
                return;
            
            if(facingMode == ARCameraGlobalSettings.FacingMode.BACK){
                htmlLines = ReplaceFacingMode(htmlLines, "environment");
            }
            else if(facingMode == ARCameraGlobalSettings.FacingMode.FRONT){
                htmlLines = ReplaceFacingMode(htmlLines, "user");
            }
            // else if(facingMode == ARCameraGlobalSettings.FacingMode.BACK_AND_FRONT){
            //     htmlLines = ReplaceFacingMode(htmlLines, "");
            // }

            File.WriteAllLines(indexPath, htmlLines); 
        }

        static string[] ReplaceFacingMode(string[] lines, string facingMode){
            for(var i = 0; i < lines.Length; i++){
                if(lines[i].Contains("window.unityFacingMode = ")){
                    lines[i] = "\t\twindow.unityFacingMode = \"" + facingMode + "\"";
                    Debug.Log("Facing Mode: " + lines[i]);
                };
            }

            return lines;
        }
    }
}

