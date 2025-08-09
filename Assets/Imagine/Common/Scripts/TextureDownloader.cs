using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using UnityEngine;

namespace Imagine.WebAR
{
    public class TextureDownloader : MonoBehaviour
    {
        [DllImport("__Internal")] private static extern void DownloadWebGLTexture(byte[] img, int size, string filename, string extension);

        private enum FileExtension { PNG, JPEG};
        [SerializeField] private FileExtension fileExt = FileExtension.PNG;

        public void DownloadTexture(Texture2D texture)
        {


            if(fileExt == FileExtension.PNG)
            {
                var bytes = texture.EncodeToPNG();
                
#if UNITY_WEBGL && !UNITY_EDITOR
                DownloadWebGLTexture(bytes, bytes.Length, texture.name, ".png");
#endif
            }
            else if (fileExt == FileExtension.JPEG)
            {
                var bytes = texture.EncodeToJPG();

#if UNITY_WEBGL && !UNITY_EDITOR
                DownloadWebGLTexture(bytes, bytes.Length, texture.name, ".jpeg");
#endif

            }

#if !UNITY_WEBGL || UNITY_EDITOR
            Debug.Log("Texture downloads only available in WebGL builds");
#endif

        }
    }
}
