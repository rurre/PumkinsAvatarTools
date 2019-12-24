using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Pumkin.HelperFunctions;
using Pumkin.AvatarTools;
using Pumkin.DataStructures;

namespace Pumkin.Presets
{
    [CustomEditor(typeof(PumkinsCameraPreset))]
    public class PumkinsCameraPresetEditor : Editor
    {        
        PumkinsCameraPreset preset;

        SerializedObject st;
        SerializedProperty pName,
            pOffsetMode,
            pTransformPath,
            pPositionOffset,
            pRotationAnglesOffset,
            pUseOverlay,
            pUseBackground,
            pOverlayImagePath,
            pOverlayImageTint,
            pBackgroundType,
            pBackgroundColor,
            pBackgroundImagePath,
            pBackgroundImageTint,
            pBackgroundMaterial;

        

        private void OnEnable()
        {
            preset = (PumkinsCameraPreset)target;

            st = new SerializedObject(preset);
            pName = st.FindProperty("name");
            pOffsetMode = st.FindProperty("offsetMode");
            pTransformPath = st.FindProperty("transformPath");
            pPositionOffset = st.FindProperty("positionOffset");
            pRotationAnglesOffset = st.FindProperty("rotationAnglesOffset");
            pUseOverlay = st.FindProperty("useOverlay");
            pUseBackground = st.FindProperty("useBackground");
            pOverlayImagePath = st.FindProperty("overlayImagePath");
            pOverlayImageTint = st.FindProperty("overlayImageTint");
            pBackgroundType = st.FindProperty("backgroundType");
            pBackgroundColor = st.FindProperty("backgroundColor");
            pBackgroundImagePath = st.FindProperty("backgroundImagePath");
            pBackgroundImageTint = st.FindProperty("backgroundImageTint");
            pBackgroundMaterial = st.FindProperty("backgroundMaterial");
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField(Strings.Presets.blendshapePreset, Styles.Label_mainTitle);
            Helpers.DrawGUILine();
            DrawPropertyGUI();
        }

        void DrawPropertyGUI()
        {            
            st.UpdateIfRequiredOrScript();

            EditorGUILayout.PropertyField(pName, new GUIContent("Preset Name"));

            Helpers.DrawGUILine();

            EditorGUILayout.PropertyField(pOffsetMode, new GUIContent("Offset Mode"));

            if((PumkinsCameraPreset.CameraOffsetMode)pOffsetMode.enumValueIndex == PumkinsCameraPreset.CameraOffsetMode.Transform)
                EditorGUILayout.PropertyField(pTransformPath, new GUIContent("Transform Path"));

            EditorGUILayout.Space();


            EditorGUILayout.PropertyField(pPositionOffset, new GUIContent("Position Offset"));
            EditorGUILayout.PropertyField(pRotationAnglesOffset, new GUIContent("Rotation Offset"));            

            Helpers.DrawGUILine();

            EditorGUILayout.PropertyField(pUseOverlay, new GUIContent("Use Camera Overlay"));            

            if(pUseOverlay.boolValue)
            {
                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(pOverlayImagePath, new GUIContent("Overlay Image Path"));
                EditorGUILayout.PropertyField(pOverlayImageTint, new GUIContent("Overlay Tint"));                
            }

            Helpers.DrawGUILine();

            EditorGUILayout.PropertyField(pUseBackground, new GUIContent("Use Camera Background"));

            if(pUseBackground.boolValue)
            {
                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(pBackgroundType, new GUIContent("Background Type"));

                EditorGUILayout.Space();

                switch((PumkinsAvatarTools.CameraBackgroundOverrideType)pBackgroundType.enumValueIndex)
                {
                    case PumkinsAvatarTools.CameraBackgroundOverrideType.Color:
                        EditorGUILayout.PropertyField(pBackgroundColor, new GUIContent("Background Color"));
                        break;
                    case PumkinsAvatarTools.CameraBackgroundOverrideType.Image:
                        EditorGUILayout.PropertyField(pBackgroundImagePath, new GUIContent("Image Path"));
                        EditorGUILayout.PropertyField(pBackgroundImageTint, new GUIContent("Image Tint"));
                        break;
                    case PumkinsAvatarTools.CameraBackgroundOverrideType.Skybox:
                        EditorGUILayout.PropertyField(pBackgroundMaterial, new GUIContent("Material"));
                        break;
                    default:
                        break;
                }
            }

            Helpers.DrawGUILine();

            if(GUILayout.Button(Strings.Buttons.selectInToolsWindow, Styles.BigButton))            
                PumkinsPresetManager.SelectPresetInToolWindow(preset);            

            st.ApplyModifiedProperties();
        }    
    }
}