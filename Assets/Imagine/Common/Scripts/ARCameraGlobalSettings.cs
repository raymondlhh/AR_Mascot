using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Imagine.WebAR{
    // [CreateAssetMenu(fileName = "ARCameraGlobalSettings", menuName = "Imagine/ARCameraGlobalSettings", order = 1)]

    public class ARCameraGlobalSettings : ScriptableObject
    {
        public enum FacingMode {BACK, FRONT, DONT_OVERRIDE};
        [SerializeField] public FacingMode facingMode;
        
        
        private static ARCameraGlobalSettings _instance;
        public static ARCameraGlobalSettings Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = Resources.Load<ARCameraGlobalSettings>("ARCameraGlobalSettings");
                }
                return _instance;

            }
        }
    }
}
