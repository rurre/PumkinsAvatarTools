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
        PumkinsBlendshapePreset _preset;

        private void OnEnable()
        {
            _preset = (PumkinsBlendshapePreset)target;
        }

        public PumkinsBlendshapePreset Preset
        {
            get { return _preset; }
        }


        public override void OnInspectorGUI()
        {               
            base.OnInspectorGUI();            
        }        
    }
}