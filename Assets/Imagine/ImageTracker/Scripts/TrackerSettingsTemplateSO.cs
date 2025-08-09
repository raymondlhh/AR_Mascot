using System.Collections;
using System.Collections.Generic;
using Imagine.WebAR;
using UnityEngine;

namespace Imagine.WebAR{
    // [CreateAssetMenu(fileName = "TrackerSettingsTemplate", menuName = "ImagineWebAR/TrackerSettingsTemplate", order = 1)]
    public class TrackerSettingsTemplateSO : ScriptableObject
    {
        public string label;
        public Color color = Color.black;
        [TextArea(3,10)] public string description;
        public TrackerSettings settings;
    }
}

