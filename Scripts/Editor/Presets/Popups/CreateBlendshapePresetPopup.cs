using System;
using Pumkin.AvatarTools;
using Pumkin.DataStructures;
using Pumkin.HelperFunctions;
using UnityEditor;
using UnityEngine;

namespace Pumkin.Presets
{
    public class CreateBlendshapePopup : CreatePresetPopupBase
    {
        public static void ShowWindow(PumkinsBlendshapePreset newPreset = null)
        {
            AssignOrCreatePreset<PumkinsBlendshapePreset>(newPreset);
            newPreset = (PumkinsBlendshapePreset)preset;

            if(PumkinsAvatarTools.SelectedAvatar)
                newPreset.SetupPreset("", PumkinsAvatarTools.SelectedAvatar);

            if(!_window)
            {
                _window = CreateInstance<CreateBlendshapePopup>();
                _window.autoRepaintOnSceneChange = true;
            }

            if(editingExistingPreset)
            {
                _window.titleContent = new GUIContent("Edit Blendshape Preset");
                _overwriteFile = true;
            }
            else
            {
                _window.titleContent = new GUIContent("Create Blendshape Preset");
            }

            _window.ShowUtility();
        }

        private void OnEnable()
        {
            PumkinsAvatarTools.AvatarSelectionChanged += HandleSelectionChanged;            
        }

        private new void OnDisable()
        {            
            PumkinsAvatarTools.AvatarSelectionChanged -= HandleSelectionChanged;
            base.OnDisable();
        }

        public void HandleSelectionChanged(GameObject selection)
        {
            SetupPreset();
        }

        private void SetupPreset()
        {
            PumkinsBlendshapePreset preset = (PumkinsBlendshapePreset)CreatePresetPopupBase.preset;
            preset.SetupPreset(preset.name, PumkinsAvatarTools.SelectedAvatar);
            CreatePresetPopupBase.preset = preset;            
        }

        private void OnGUI()
        {
            PumkinsBlendshapePreset preset = (PumkinsBlendshapePreset)CreatePresetPopupBase.preset;
            if(!preset)            
                AssignOrCreatePreset<PumkinsBlendshapePreset>(preset);
            if(!preset)
                return;
            
            scroll = EditorGUILayout.BeginScrollView(scroll);
            {
                EditorGUILayout.Space();

                preset.name = EditorGUILayout.TextField("Preset Name", preset.name);

                Helpers.DrawGuiLine();

                PumkinsAvatarTools.DrawAvatarSelectionWithButton(false, false);

                Helpers.DrawGuiLine();

                DrawBlendshapePresetControls();

                Helpers.DrawGuiLine();
                
                EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(preset.name) || !PumkinsAvatarTools.SelectedAvatar);
                {
                    if(editingExistingPreset)
                    {
                        //EditorGUILayout.BeginHorizontal();
                        //if(GUILayout.Button("Cancel", Styles.BigButton))
                        //{
                        //    _saveEdittedChanges = false;
                        //    Close();
                        //}
                        //if(GUILayout.Button("Save Changes", Styles.BigButton))
                        //{
                        //    _saveEdittedChanges = true;
                        //    EditorUtility.SetDirty(preset);
                        //    Close();
                        //}
                        //EditorGUILayout.EndHorizontal();
                    }
                    else
                    {
                        _overwriteFile = GUILayout.Toggle(_overwriteFile, "Overwrite File");
                        if(GUILayout.Button("Save Preset", Styles.BigButton))
                            preset.SavePreset(_overwriteFile);
                    }                    
                }
                EditorGUI.EndDisabledGroup();
                
            }
            EditorGUILayout.EndScrollView();

            CreatePresetPopupBase.preset = preset;
        }        

        private void DrawBlendshapePresetControls()
        {
            PumkinsBlendshapePreset p = (PumkinsBlendshapePreset)preset;
            if(PumkinsAvatarTools.SelectedAvatar)
                Helpers.DrawBlendshapeSlidersWithDeleteAndAdd(ref p.renderers);
            else
                EditorGUILayout.TextField("_Select an Avatar first.", Styles.HelpBox_OneLine);
        }

        protected override void RefreshSelectedPresetIndex()
        {
            PumkinsAvatarTools.RefreshPresetIndexByString<PumkinsBlendshapePreset>(preset.name);
        }
    }
}