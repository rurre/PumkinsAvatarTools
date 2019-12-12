using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Pumkin.HelperFunctions;
using Pumkin.AvatarTools;
using Pumkin.DataStructures;
using System;

namespace Pumkin.Presets
{
    [CustomEditor(typeof(PumkinsPosePreset))]
    public class PumkinsPosePresetEditor : Editor
    {
        static string[] defaultMusclesNames;

        bool muscles_expand = false;
        bool transforms_expand = false;

        SerializedObject serializedPosePreset;

        SerializedProperty pName,
            pMuscles,
            pPresetMode,
            pTransformPaths,
            pTransformRotations,
            pMuscleNames;        

        PumkinsPosePreset Preset
        {
            get { return (PumkinsPosePreset)target; }
        }

        private void OnEnable()
        {            
            if(defaultMusclesNames == null || defaultMusclesNames.Length == 0)
                defaultMusclesNames = HumanTrait.MuscleName;

            serializedPosePreset = new SerializedObject(Preset);            
            
            pName = serializedPosePreset.FindProperty("name");
            pMuscles = serializedPosePreset.FindProperty("muscles");
            pPresetMode = serializedPosePreset.FindProperty("presetMode");
            pTransformPaths = serializedPosePreset.FindProperty("transformPaths");
            pTransformRotations = serializedPosePreset.FindProperty("transformRotations");            
        }

        void DrawPropertyGUI()
        {
            serializedPosePreset.UpdateIfRequiredOrScript();
               
            EditorGUILayout.PropertyField(pName, new GUIContent("Preset Name"));

            Helpers.DrawGuiLine();

            EditorGUILayout.PropertyField(pPresetMode, new GUIContent("Preset Mode"));

            Helpers.DrawGuiLine();

            if((PumkinsPosePreset.PosePresetMode)pPresetMode.enumValueIndex == PumkinsPosePreset.PosePresetMode.HumanPose)
            {
                Helpers.DrawPropertyArrayWithNames(pMuscles, "Muscles", defaultMusclesNames, ref muscles_expand, false, 185);
            }
            else
            {
                Helpers.DrawPropertyArraysHorizontal(new SerializedProperty[] { pTransformPaths, pTransformRotations }, "Transform Rotations", ref transforms_expand);
            }            

            Helpers.DrawGuiLine();

            if(GUILayout.Button(Strings.Buttons.selectInToolsWindow))
            {
                PumkinsPresetManager.SelectPresetInToolWindow(Preset);
            }

            serializedPosePreset.ApplyModifiedProperties();
        }

        public override void OnInspectorGUI()
        {
            DrawPropertyGUI();
        }
    }
}