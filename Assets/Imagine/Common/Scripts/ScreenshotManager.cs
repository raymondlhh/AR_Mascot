using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;


namespace Imagine.WebAR
{
    public class ScreenshotManager : MonoBehaviour
    {
        [DllImport("__Internal")] private static extern void ShowWebGLScreenshot(string dataUrl);

        private ARCamera arCamera;

        [SerializeField] private AudioClip shutterSound;
        [SerializeField] private AudioSource shutterSoundSource;

        public Texture2D screenShot;


        void Start(){
            arCamera = GameObject.FindObjectOfType<ARCamera>();
        }


        public void GetScreenShot()
        {
            if(arCamera.videoPlaneMode == ARCamera.VideoPlaneMode.NONE)
            {
                Debug.LogWarning("Your screenshot will not include the webcam image. Enable Video plane in your AR Camera to properly capture screenshots");
            }  

            Debug.Log("Getting Screenshot...");

            // Delete old textures to avoid memory leaks
            if(screenShot != null){
                Destroy(screenShot);
            }
            
            // Create a RenderTexture to temporarily hold the camera image
            screenShot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGBA32, false);
            RenderTexture renderTexture = new RenderTexture(screenShot.width, screenShot.height, 24);
            Camera.main.targetTexture = renderTexture;
            Camera.main.Render();

            // Read the pixels from the RenderTexture into the Texture2D
            RenderTexture.active = renderTexture;
            screenShot.ReadPixels(new Rect(0, 0, screenShot.width, screenShot.height), 0, 0);
            screenShot.Apply();

            // Clean up by resetting the targetTexture and releasing the RenderTexture
            Camera.main.targetTexture = null;
            RenderTexture.active = null;
            Destroy(renderTexture);    


            if(shutterSoundSource != null && shutterSound != null){
                shutterSoundSource.PlayOneShot(shutterSound);
            }

#if UNITY_EDITOR
            Debug.Log("Screenshots are only displayed on WebGL builds");
#else
            byte[] textureBytes = screenShot.EncodeToJPG();
            string dataUrlStr = "data:image/jpeg;base64," + System.Convert.ToBase64String(textureBytes);
            ShowWebGLScreenshot(dataUrlStr);
#endif
        }
    }
}

