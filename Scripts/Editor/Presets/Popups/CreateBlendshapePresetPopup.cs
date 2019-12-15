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

            if(newPreset)
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

        private void OnDisable()
        {
            PumkinsAvatarTools.AvatarSelectionChanged -= HandleSelectionChanged;
        }

        public void HandleSelectionChanged(GameObject selection)
        {
            SetupPreset();
        }

        private static void SetupPreset()
        {
            PumkinsBlendshapePreset preset = (PumkinsBlendshapePreset)CreatePresetPopupBase.preset;
            preset.SetupPreset(preset.name, PumkinsAvatarTools.SelectedAvatar);
            CreatePresetPopupBase.preset = preset;
        }

        private void OnGUI()
        {
            PumkinsBlendshapePreset preset = (PumkinsBlendshapePreset)CreatePresetPopupBase.preset;
            if(!preset)
            {
                AssignOrCreatePreset<PumkinsBlendshapePreset>(preset);                
            }
            scroll = EditorGUILayout.BeginScrollView(scroll);
            {
                EditorGUILayout.Space();

                preset.name = EditorGUILayout.TextField("Preset Name", preset.name);

                Helpers.DrawGuiLine();

                PumkinsAvatarTools.SelectedAvatar = (GameObject)EditorGUILayout.ObjectField("Avatar", PumkinsAvatarTools.SelectedAvatar, typeof(GameObject), true);

                Helpers.DrawGuiLine();                

                EditorGUI.BeginDisabledGroup(!PumkinsAvatarTools.SelectedCamera || string.IsNullOrEmpty(preset.name) || !PumkinsAvatarTools.SelectedAvatar);
                {
                    _overwriteFile = GUILayout.Toggle(_overwriteFile, "Overwrite File");
                    if(GUILayout.Button("Save Preset", Styles.BigButton))
                    {
                        SetupPreset();
                        preset.SavePreset(_overwriteFile);
                    }
                }
                EditorGUI.EndDisabledGroup();
            }
            EditorGUILayout.EndScrollView();
        }
    }
}