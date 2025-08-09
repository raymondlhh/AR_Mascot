using  System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Imagine.WebAR{

    [System.Serializable]
    public class ImageTargetInfo
    {
        public string id;
        public Texture2D texture;
    }

    [System.Serializable]
    public class TrackerSettings
    {
        [Tooltip("Allows tracking multiple image targets simultaneously (Experimental)")]
        [SerializeField] public int maxSimultaneousTargets = 1;

        //public enum TrackingQuality { SPEED_OVER_QUALITY, BALANCED, QUALITY_OVER_SPEED };
        //[SerializeField] public TrackingQuality trackingQuality = TrackingQuality.BALANCED;


        public enum FrameRate { FR_30_FPS = 30, FR_60FPS = -1};

        [SerializeField] public FrameRate targetFrameRate = FrameRate.FR_30_FPS;

        [SerializeField] public AdvancedSettings advancedSettings;

        [SerializeField] [Space] public bool useExtraSmoothing = false;
        [SerializeField] [Range(1f, 20)] public float smoothenFactor = 10;
        

        [Tooltip("If enabled, you can display imageTarget feature points by pressing 'I' in desktop browser")]
        [Space][SerializeField] public bool debugMode = false;


        public string Serialize() {
            var json = "{";
            json += "\"MAX_SIMULTANEOUS_TRACK\":" + maxSimultaneousTargets + ",";

            json += "\"FRAMERATE\":" + (int)targetFrameRate + ",";

            json += "\"MAX_AREA\":" + Mathf.RoundToInt(advancedSettings.maxFrameArea * 1000) + ",";
            
            json += "\"MAX_PIXELS\":" + Mathf.RoundToInt(advancedSettings.maxFrameLength) + ",";

            json += "\"TRACK_TARGET_MATCH_COUNT\":" + advancedSettings.trackedPoints + ",";

            json += "\"POSE_CORRECTION_INTERVAL\":" + advancedSettings.poseCorrectionInterval + ",";

            json += "\"DETECT_INTERVAL\":" + advancedSettings.detectInterval + ",";

            json += "\"DETECTABILITY\":" + advancedSettings.detectability.ToStringInvariantCulture() + ",";

            json += "\"DETECT_ZONE\":\"" + advancedSettings.detectZone + "\"";

            json += "}";

            return json;

        }
    }

    [System.Serializable]
    public class AdvancedSettings
    {
        [Tooltip("Higher values will increase accuracy, but decreases frame rate")]
        [SerializeField][Range(240, 500)]
        public int maxFrameLength = 450;

        [Tooltip("Higher values will make the image easily detected, but induces a short lag/delay")]
        [SerializeField] [Range(24, 80)] public float maxFrameArea = 80;

        [Tooltip("Higher values will improve stability, but decreases frame rate")]
        [SerializeField] [Range(16, 80)] public int trackedPoints = 80;

        [Tooltip("Lower values will be resistant to skewing, but introduces jitter")]
        [SerializeField] [Range(200, 3000)] public int poseCorrectionInterval = 1500;

        [Space]
        [Tooltip("Lower intervals will speed up detection, especially on multiple targets, but significantly decreases frame rate. Value in millisecods")]
        [SerializeField] [Range(50, 1000)] public int detectInterval = 200;

        [Tooltip("Higher values will help weaker image targets to get detected, but decreases fps")]
        [SerializeField] [Range(0.4f, 1)] public float detectability = 0.5f;

        public enum DetectZone {WIDE, NARROW};
        [Tooltip("WIDE - Recommended for strong targets, focuses detection on large image details\n\nNARROW - Recommended for weaker targets or when using a small frameSize(eg. 300px), focuses detection on small image details")]
        [SerializeField] public DetectZone detectZone = DetectZone.WIDE;
    }


    //[CreateAssetMenu(menuName = "Imagine WebAR/Image Tracker Global Settings", order = 1300)]
    public class ImageTrackerGlobalSettings : ScriptableObject
    {
        [SerializeField] public List<ImageTargetInfo> imageTargetInfos;

        [SerializeField] public List<TrackerSettingsTemplateSO> settingsTemplates;
        
        
        private static ImageTrackerGlobalSettings _instance;
        public static ImageTrackerGlobalSettings Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = Resources.Load<ImageTrackerGlobalSettings>("ImageTrackerGlobalSettings");
                }
                return _instance;

            }
        }
    }
}

