using Pumkin.DataStructures;
using Pumkin.HelperFunctions;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Pumkin.Presets
{
    [CustomEditor(typeof(PumkinsBlendshapePreset))]
    public class PumkinsBlendshapePresetEditor : Editor
    {        
        PumkinsBlendshapePreset preset;
        SerializedObject serializedPreset;
        SerializedProperty pName;

        private void OnEnable()
        {
            preset = (PumkinsBlendshapePreset)target;
            serializedPreset = new SerializedObject(preset);

            pName = serializedPreset.FindProperty("name");            
        }

        public override void OnInspectorGUI()
        {
            serializedPreset.Update();
            EditorGUILayout.LabelField(Strings.Presets.blendshapePreset, Styles.Label_mainTitle);
            Helpers.DrawGUILine();
            EditorGUILayout.PropertyField(pName, new GUIContent(Strings.Presets.presetName));
            Helpers.DrawGUILine();
            Helpers.DrawBlendshapeSlidersWithDeleteAndAdd(ref preset.renderers, null);

            Helpers.DrawGUILine();

            if(GUILayout.Button(Strings.Buttons.selectInToolsWindow, Styles.BigButton))
                PumkinsPresetManager.SelectPresetInToolWindow(preset);

            serializedPreset.ApplyModifiedProperties();
        }        
    }
}