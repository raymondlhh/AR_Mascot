using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

namespace Imagine.WebAR.Editor
{
    public class PostProcessBuild : MonoBehaviour
    {
        [PostProcessBuild]
        public static void OnPostProcessBuild(BuildTarget target, string buildPath)
        {
            // Only process WebGL builds
            if (target != BuildTarget.WebGL)
            {
                Debug.Log("PostProcessBuild: Skipping non-WebGL build target: " + target);
                return;
            }

            Debug.Log("PostProcessBuild: Processing WebGL build at " + buildPath);
            var targetsHtml = "";

            string targetsPath = buildPath + "/targets";
            if(!Directory.Exists(targetsPath))
            {
                Directory.CreateDirectory(targetsPath);
            }

            foreach (var info in ImageTrackerGlobalSettings.Instance.imageTargetInfos)
            {
                var src = AssetDatabase.GetAssetPath(info.texture);
                var fileName = Path.GetFileName(src);
                Debug.Log(info.id + "->" + src);

                File.Copy(src, targetsPath + "/" + fileName, true);

                targetsHtml += ("\t\t<imagetarget id='" + info.id + "' src='targets/" + fileName + "'></imagetarget>\n");
            }

            Debug.Log(targetsHtml);

            string indexPath = buildPath + "/index.html";
            if (!File.Exists(indexPath))
            {
                Debug.LogError("PostProcessBuild: index.html not found at " + indexPath);
                return;
            }

            var lines = File.ReadAllLines(indexPath).ToList();
            var html = "";
            foreach(var line in lines)
            {
                if (string.IsNullOrEmpty(line))
                    continue;

                var trimmed = line.Trim();
                if (trimmed.StartsWith("<imagetarget") && trimmed.EndsWith("</imagetarget>"))
                    continue;
                html += line + "\n";
            }
            html = html.Replace("<!--IMAGETARGETS-->", "<!--IMAGETARGETS-->\n" + targetsHtml);
            File.WriteAllText(indexPath, html);
        }
    }
}

