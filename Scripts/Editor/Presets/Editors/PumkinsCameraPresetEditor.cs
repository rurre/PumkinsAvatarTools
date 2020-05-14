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

            EditorGUILayout.PropertyField(pName, new GUIContent(Strings.Presets.presetName));

            Helpers.DrawGUILine();

            EditorGUILayout.PropertyField(pOffsetMode, new GUIContent(Strings.Presets.offsetMode));

            if((PumkinsCameraPreset.CameraOffsetMode)pOffsetMode.enumValueIndex == PumkinsCameraPreset.CameraOffsetMode.Transform)
                EditorGUILayout.PropertyField(pTransformPath, new GUIContent(Strings.Presets.transform));

            EditorGUILayout.Space();


            EditorGUILayout.PropertyField(pPositionOffset, new GUIContent(Strings.Thumbnails.positionOffset));
            EditorGUILayout.PropertyField(pRotationAnglesOffset, new GUIContent(Strings.Thumbnails.rotationOffset));            

            Helpers.DrawGUILine();

            EditorGUILayout.PropertyField(pUseOverlay, new GUIContent(Strings.Thumbnails.useCameraOverlay));            

            if(pUseOverlay.boolValue)
            {
                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(pOverlayImagePath, new GUIContent(Strings.Thumbnails.overlayImagePath));
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(pOverlayImageTint, new GUIContent(Strings.Thumbnails.tint));                
            }

            Helpers.DrawGUILine();

            EditorGUILayout.PropertyField(pUseBackground, new GUIContent(Strings.Thumbnails.useCameraBackground));

            if(pUseBackground.boolValue)
            {
                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(pBackgroundType, new GUIContent(Strings.Thumbnails.backgroundType));

                EditorGUILayout.Space();

                switch((PumkinsCameraPreset.CameraBackgroundOverrideType)pBackgroundType.enumValueIndex)
                {
                    case PumkinsCameraPreset.CameraBackgroundOverrideType.Color:
                        EditorGUILayout.PropertyField(pBackgroundColor, new GUIContent(Strings.Thumbnails.backgroundColor));
                        break;
                    case PumkinsCameraPreset.CameraBackgroundOverrideType.Image:
                        EditorGUILayout.PropertyField(pBackgroundImagePath, new GUIContent(Strings.Thumbnails.imagePath));
                        EditorGUILayout.PropertyField(pBackgroundImageTint, new GUIContent(Strings.Thumbnails.tint));
                        break;
                    case PumkinsCameraPreset.CameraBackgroundOverrideType.Skybox:
                        EditorGUILayout.PropertyField(pBackgroundMaterial, new GUIContent(Strings.Thumbnails.backgroundType_Material));
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