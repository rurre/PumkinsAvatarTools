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
            //base.OnInspectorGUI();
            Helpers.DrawGUILine();
            EditorGUILayout.PropertyField(pName, new GUIContent(Strings.Preset.name));
            Helpers.DrawGUILine();
            Helpers.DrawBlendshapeSlidersWithDeleteAndAdd(ref preset.renderers, null);

            serializedPreset.ApplyModifiedProperties();
        }        
    }
}