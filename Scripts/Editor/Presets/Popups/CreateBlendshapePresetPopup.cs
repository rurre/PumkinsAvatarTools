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

            if(!_window || _window.GetType() != typeof(CreateBlendshapePopup))
            {
                _window = CreateInstance<CreateBlendshapePopup>();
                _window.autoRepaintOnSceneChange = true;

                if(minWindowSize.magnitude > Vector2.zero.magnitude)
                {
                    float maxX = Mathf.Max(_window.minSize.x, minWindowSize.x);
                    float maxY = Mathf.Max(_window.minSize.y, minWindowSize.y);
                    _window.minSize = new Vector2(maxX, maxY);
                }
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

        void OnEnable()
        {            
            PumkinsAvatarTools.AvatarSelectionChanged += HandleSelectionChanged;            
        }

        private new void OnDisable()
        {            
            PumkinsAvatarTools.AvatarSelectionChanged -= HandleSelectionChanged;
            base.OnDisable();
        }

        private void OnDestroy()
        {
            if(editingExistingPreset)            
                SetupPreset();            
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
            scroll = EditorGUILayout.BeginScrollView(scroll);
            try
            {
                PumkinsBlendshapePreset preset = (PumkinsBlendshapePreset)CreatePresetPopupBase.preset;
                if(!preset)
                {
                    AssignOrCreatePreset<PumkinsBlendshapePreset>(preset);
                    return;
                }

                EditorGUILayout.Space();

                preset.name = EditorGUILayout.TextField(Strings.Presets.presetName, preset.name);

                Helpers.DrawGUILine();

                PumkinsAvatarTools.DrawAvatarSelectionWithButtonGUI(false, false);

                Helpers.DrawGUILine();

                DrawBlendshapePresetControls();

                EditorGUILayout.EndScrollView();
                Helpers.DrawGUILine();

                EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(preset.name) || !PumkinsAvatarTools.SelectedAvatar);
                {
                    if(!editingExistingPreset)
                    {
                        _overwriteFile = GUILayout.Toggle(_overwriteFile, Strings.Presets.overwriteFile);
                        if(GUILayout.Button(Strings.Buttons.savePreset, Styles.BigButton))
                        {
                            EditorApplication.delayCall += () =>
                            {
                                preset.SavePreset(_overwriteFile);
                                Close();
                            };
                        }
                    }
                }
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.Space();
                CreatePresetPopupBase.preset = preset;
            }
            catch
            {
                EditorGUILayout.EndScrollView();                
                Close();
            }            
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