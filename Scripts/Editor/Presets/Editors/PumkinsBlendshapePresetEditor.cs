using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Pumkin.Presets
{
    [CustomEditor(typeof(PumkinsBlendshapePreset))]
    public class PumkinsBlendshapePresetEditor : Editor
    {
        //SerializedObject serializedPreset;
        //SerializedProperty pName;

        private void OnSceneGUI()
        {
            PumkinsBlendshapePreset p = (PumkinsBlendshapePreset)target;
            p.name = EditorGUILayout.TextField(new GUIContent("Name"), p.name);
            DrawDefaultInspector();
        }
    }
}