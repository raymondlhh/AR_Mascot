using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Runtime.InteropServices;
using System.IO;
using System.Data.Common;


namespace Imagine.WebAR
{
    public class TextureExtractor_WarpedImage : MonoBehaviour
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern bool WebGLIsCameraStarted();
        [DllImport("__Internal")] private static extern void GetWebGLWarpedTexture(string targetId, int textureId, int resolution);
#endif

        private ARCamera arCamera;

        //[SerializeField] UnityEngine.UI.RawImage testRawImage, testRawImage2;

        private Texture2D warpedTexture;
        private int warpedTextureId;

        private enum TextureExtractorMode {EVERY_FRAME, MANUAL};
        [SerializeField] private TextureExtractorMode mode = TextureExtractorMode.EVERY_FRAME;
        [SerializeField] private string targetId;
        [SerializeField] private RenderTexture outputTexture;

        [SerializeField] private bool checkVisibility = false;
        [SerializeField] private Camera isVisibleCamera;
        [SerializeField] private MeshRenderer isVisibleRenderer;

        private bool isFullyVisibleInLastFrame = false;
        private bool isInitializing = false;
        
        [Tooltip("Use these events to check for full visibility before extracting the texture manually")]
        [SerializeField] private UnityEvent DidBecomeFullyVisible, DidBecomeObscured;

        void Start(){
            StartCoroutine(Initialize());
        }

        IEnumerator Initialize(){

            isInitializing = true;

#if UNITY_WEBGL && !UNITY_EDITOR
            while(!WebGLIsCameraStarted()){
                Debug.Log("Unity Waiting for WebGLIsCameraStarted");
                yield return null;
            }
            Debug.Log("Unity WebGLIsCameraStarted");
#else
            // For non-WebGL platforms, skip the WebGL camera check
            Debug.Log("Unity Skipping WebGL camera check for non-WebGL platform");
#endif


            warpedTexture = new Texture2D(outputTexture.width, outputTexture.height);
            Debug.Log("Unity Initialized warpedTexture");

            warpedTextureId = (int)warpedTexture.GetNativeTexturePtr();

            Debug.Log("Unity Initialized warpedTextureId = " + warpedTextureId);

            yield break;
        }

        void OnDisable(){
            isFullyVisibleInLastFrame = false;
            isInitializing = false;
            DidBecomeObscured?.Invoke();
        }

        void Update(){

            if(mode == TextureExtractorMode.EVERY_FRAME){
                //Debug.Log("Unity Update EVERY_FRAME()");
                ExtractTexture();
            }

            if(checkVisibility && isVisibleCamera && isVisibleRenderer){
                
                var isFullyVisible = false;
                if(!isVisibleRenderer.gameObject.activeInHierarchy){
                    isFullyVisible = false;
                    DidBecomeObscured?.Invoke();
                    return;
                }

                isFullyVisible = IsFullyVisibleInCamera();
                if(isFullyVisible && !isFullyVisibleInLastFrame){
                    DidBecomeFullyVisible?.Invoke();
                }
                else if(!isFullyVisible && isFullyVisibleInLastFrame){
                    DidBecomeObscured?.Invoke();
                }

                isFullyVisibleInLastFrame = isFullyVisible;
            }      

            return;
        }

        public void ExtractTexture(){
            if(warpedTexture == null){
                if(!isInitializing){
                    StartCoroutine(Initialize());
                }
                return;
            }

            //Debug.Log("Unity Update ExtractTexture()");

            var resolution = outputTexture.width;
#if UNITY_WEBGL && !UNITY_EDITOR
            GetWebGLWarpedTexture(targetId, warpedTextureId, resolution);
#else
            // For non-WebGL platforms, skip the WebGL texture extraction
            Debug.Log("Unity Skipping WebGL texture extraction for non-WebGL platform");
#endif

            Graphics.Blit(warpedTexture, outputTexture);
        }

        bool IsFullyVisibleInCamera()
        {
            if (isVisibleRenderer && isVisibleCamera)
            {
                Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(isVisibleCamera);

                // Get the bounds of the MeshRenderer in world space
                Bounds bounds = isVisibleRenderer.bounds;

                // Calculate the vertices of the bounding box
                Vector3[] boundsVertices = new Vector3[8];
                boundsVertices[0] = bounds.center + new Vector3(bounds.extents.x, bounds.extents.y, bounds.extents.z);
                boundsVertices[1] = bounds.center + new Vector3(-bounds.extents.x, bounds.extents.y, bounds.extents.z);
                boundsVertices[2] = bounds.center + new Vector3(bounds.extents.x, -bounds.extents.y, bounds.extents.z);
                boundsVertices[3] = bounds.center + new Vector3(-bounds.extents.x, -bounds.extents.y, bounds.extents.z);
                boundsVertices[4] = bounds.center + new Vector3(bounds.extents.x, bounds.extents.y, -bounds.extents.z);
                boundsVertices[5] = bounds.center + new Vector3(-bounds.extents.x, bounds.extents.y, -bounds.extents.z);
                boundsVertices[6] = bounds.center + new Vector3(bounds.extents.x, -bounds.extents.y, -bounds.extents.z);
                boundsVertices[7] = bounds.center + new Vector3(-bounds.extents.x, -bounds.extents.y, -bounds.extents.z);

                // Check if all vertices are inside the camera's frustum
                for (int i = 0; i < 8; i++)
                {
                    if (!GeometryUtility.TestPlanesAABB(frustumPlanes, new Bounds(boundsVertices[i], Vector3.zero)))
                    {
                        return false; // If any vertex is outside the frustum, return false
                    }
                }

                return true;
            }

            return false;
        }

        // public GetBase64String(){
        //     Texture2D tex = new Texture2D(outputTexture.width, outputTexture.height, TextureFormat.RGB24, false);

        //     RenderTexture lastActive = RenderTexture.active
        //     RenderTexture.active = outputTexture;
        //     tex.ReadPixels(new Rect(0, 0, outputTexture.width, outputTexture.height), 0, 0);
        //     RenderTexture.active = lastActive;

        //     byte[] pngBytes = tex.EncodeToPNG();
        //     Destroy(tex)

        //     return "data:image/jpeg;base64," + System.Convert.ToBase64String(pngBytes);
        // }

    }
}

