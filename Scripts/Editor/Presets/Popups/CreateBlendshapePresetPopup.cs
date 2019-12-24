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
                _window.titleContent = new GUIContent(Strings.Presets.editBlendshapePreset);
                _overwriteFile = true;
            }
            else
            {
                _window.titleContent = new GUIContent(Strings.Presets.createBlendshapePreset);
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

                preset.name = EditorGUILayout.TextField(Strings.Presets.presetName, preset.name);

                Helpers.DrawGUILine();

                PumkinsAvatarTools.DrawAvatarSelectionWithButton(false, false);

                Helpers.DrawGUILine();

                DrawBlendshapePresetControls();

                Helpers.DrawGUILine();
                
                EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(preset.name) || !PumkinsAvatarTools.SelectedAvatar);
                {
                    if(!editingExistingPreset)
                    {
                        _overwriteFile = GUILayout.Toggle(_overwriteFile, Strings.Presets.overwriteFile);
                        if(GUILayout.Button(Strings.Buttons.savePreset, Styles.BigButton))
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
                Helpers.DrawBlendshapeSlidersWithDeleteAndAdd(ref p.renderers, PumkinsAvatarTools.SelectedAvatar);
            else
                EditorGUILayout.TextField(Strings.PoseEditor.selectHumanoidAvatar, Styles.HelpBox_OneLine);
        }

        protected override void RefreshSelectedPresetIndex()
        {
            PumkinsAvatarTools.RefreshPresetIndexByString<PumkinsBlendshapePreset>(preset.name);
        }
    }
}