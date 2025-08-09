using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Imagine.WebAR.Editor
{
    [CustomEditor(typeof(ImageTracker))]
    public class ImageTrackerEditor : UnityEditor.Editor
    {
        private ImageTracker _target;

        private void OnEnable()
        {
            _target = (ImageTracker)target;
        }

        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("trackerCam"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("imageTargets"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("trackerOrigin"));
            EditorGUILayout.Space(20);
            // var overrideTrackerSettingsProp = serializedObject.FindProperty("overrideTrackerSettings");
            // EditorGUILayout.PropertyField(overrideTrackerSettingsProp);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            // var overrideTrackerSettingsPropVal = overrideTrackerSettingsProp.boolValue;
            // if (!overrideTrackerSettingsPropVal)
            // {
            //     GUI.enabled = false;
            // }
            EditorGUI.indentLevel++;
            var trackerSettingsProp = serializedObject.FindProperty("trackerSettings");
            EditorGUILayout.PropertyField(trackerSettingsProp, true);
            
            // //smoothing
            // EditorGUILayout.Space(20);
            // var useExtraSmoothingProp = trackerSettingsProp.FindPropertyRelative("useExtraSmoothing");
            // EditorGUILayout.PropertyField(useExtraSmoothingProp);
            // if(useExtraSmoothingProp.boolValue){
            //     EditorGUI.indentLevel++;
            //     EditorGUILayout.PropertyField(trackerSettingsProp.FindPropertyRelative("smoothenFactor"));
            //     EditorGUI.indentLevel--;
            // }

            EditorGUI.indentLevel--;
            EditorGUILayout.Space(20);

            //settings templates
            EditorGUILayout.LabelField("Auto-set from the following templates", EditorStyles.boldLabel);
            var templates = ImageTrackerGlobalSettings.Instance.settingsTemplates;
            foreach(var t in templates){
                GUI.color = t.color;
                if(GUILayout.Button(new GUIContent(t.label, t.description))){
                    if(EditorUtility.DisplayDialog(
                        "Confirm settings overwrite",
                        "Are you sure you want to set your tracker settings to " + t.label + "?\n\n" + 
                        t.description + "\n\n" +
                        "This will overwrite your current tracker settings"
                    , "Proceed", "Cancel")){
                        
                        var tso = new SerializedObject(t);
                        var tSettingsProp = tso.FindProperty("settings");//.FindPropertyRelative("advancedSettings");
                        CopyTrackerSettings(
                            tSettingsProp,
                            trackerSettingsProp);

                        EditorUtility.DisplayDialog("Copy Settings Finished", "Your tracker settings set to " + t.label, "Okay");

                    }
                }
            }
            GUI.color = Color.white;

            GUI.enabled = true;
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(20);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("dontDeactivateOnLost"));
            
            

            EditorGUILayout.Space(20);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnImageFound"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnImageLost"));

            EditorGUILayout.Space(20);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("startStopOnEnableDisable"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("stopOnDestroy"));

            DrawEditorDebugger(); 

            serializedObject.ApplyModifiedProperties();
        }


        bool showKeyboardCameraControls = false;
        void DrawEditorDebugger(){
            //Editor Runtime Debugger
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Editor Debug Mode");
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            if(Application.IsPlaying(_target)){
                //Enable Disable
                var imageTargetsProp = serializedObject.FindProperty("imageTargets");
                var trackedIdsProp = serializedObject.FindProperty("trackedIds");
                var trackedIds = new List<string>();
                if(trackedIdsProp != null){
                    for(var i = 0; i < trackedIdsProp.arraySize; i++){
                        trackedIds.Add(trackedIdsProp.GetArrayElementAtIndex(i).stringValue);
                    }
                }
                
                for(var i = 0; i < imageTargetsProp.arraySize; i++){
                    EditorGUILayout.BeginHorizontal();
                    var imageTargetProp = imageTargetsProp.GetArrayElementAtIndex(i);
                    var id = imageTargetProp.FindPropertyRelative("id").stringValue;
                    EditorGUILayout.LabelField(id);
                    var imageFound = trackedIds.Contains(id);
                    GUI.enabled = !imageFound;
                    if(GUILayout.Button("Found")){
                        _target.SendMessage("OnTrackingFound",id);

                        var imageTargetTransform = ((Transform)imageTargetProp.FindPropertyRelative("transform").objectReferenceValue);
                        var cam = ((ARCamera)serializedObject.FindProperty("trackerCam").objectReferenceValue).transform;

                        cam.transform.position = imageTargetTransform.position + imageTargetTransform.forward * -3;
                        cam.LookAt(imageTargetTransform);
                    }
                    GUI.enabled = imageFound;
                    if(GUILayout.Button("Lost")){
                        _target.SendMessage("OnTrackingLost",id);
                    }
                    GUI.enabled = true;
                    EditorGUILayout.EndHorizontal();
                }    

                  
            }
            else{
                GUI.color = Color.yellow;
                EditorGUILayout.LabelField("Enter Play-mode to Debug In Editor");
                GUI.color = Color.white;
            }

            EditorGUILayout.Space();
            //keyboard camera controls
            showKeyboardCameraControls = EditorGUILayout.Toggle ("Show Keyboard Camera Controls", showKeyboardCameraControls);
            if(showKeyboardCameraControls){
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("W", "Move Forward (Z)");
                EditorGUILayout.LabelField("S", "Move Backward (Z)");
                EditorGUILayout.LabelField("A", "Move Left (X)");
                EditorGUILayout.LabelField("D", "Move Right (X)");
                EditorGUILayout.LabelField("R", "Move Up (Y)");
                EditorGUILayout.LabelField("F", "Move Down (Y)");
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Up Arrow", "Tilt Up (along X-Axis)");
                EditorGUILayout.LabelField("Down Arrow", "Tilt Down (along X-Axis)");
                EditorGUILayout.LabelField("Left Arrow", "Tilt Left (along Y-Axis)");
                EditorGUILayout.LabelField("Right Arrow", "Tilt Right (Along Y-Axis)");
                EditorGUILayout.LabelField("Period", "Tilt Clockwise (Along Z-Axis)");
                EditorGUILayout.LabelField("Comma", "Tilt Counter Clockwise (Along Z-Axis)");
                EditorGUILayout.Space(40);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("debugCamMoveSensitivity"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("debugCamTiltSensitivity"));
                EditorGUILayout.EndVertical();
                
            }    

            EditorGUILayout.EndVertical();
        }

        public void CopyTrackerSettings(SerializedProperty srcProp, SerializedProperty dstProp)
        {
            SerializedProperty currentProperty = srcProp.Copy();
            SerializedProperty nextSiblingProperty = srcProp.Copy();
            {
                nextSiblingProperty.Next(false);
            }
        
            if (currentProperty.Next(true))
            {
                do
                {
                    if (SerializedProperty.EqualContents(currentProperty, nextSiblingProperty))
                        break;

                    Debug.Log("Copying " + currentProperty.name + " (" + currentProperty.propertyType + ")");
                    var dstChildProp = dstProp.FindPropertyRelative(currentProperty.name);

                    if(currentProperty.hasChildren){
                        CopyTrackerSettings(currentProperty, dstChildProp);
                    }
                    else{
                        if(currentProperty.propertyType == SerializedPropertyType.Integer || 
                            currentProperty.propertyType == SerializedPropertyType.Enum){
                            dstChildProp.intValue = currentProperty.intValue;
                        }
                        else if(currentProperty.propertyType == SerializedPropertyType.Boolean){
                            dstChildProp.boolValue = currentProperty.boolValue;
                        }
                        else if(currentProperty.propertyType == SerializedPropertyType.Float){
                            dstChildProp.floatValue = currentProperty.floatValue;
                        }
                        else if(currentProperty.propertyType == SerializedPropertyType.String){
                            dstChildProp.stringValue = currentProperty.stringValue;
                        }
                        else{
                            Debug.LogError("Failed to copy property: " + currentProperty.name + "(" + currentProperty.propertyType + ")");
                        }
                    }

                }
                while (currentProperty.Next(false));
            }
        }

        object GetValue(SerializedProperty property)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    return property.intValue;
                case SerializedPropertyType.Boolean:
                    return property.boolValue;
                case SerializedPropertyType.Float:
                    return property.floatValue;
                case SerializedPropertyType.String:
                    return property.stringValue;
                // Add more cases as needed for other property types
                default:
                    return null;
            }
        }

    }
}

