using Pumkin.AvatarTools;
using Pumkin.DataStructures;
using Pumkin.HelperFunctions;
using Pumkin.PoseEditor;
using UnityEditor;
using UnityEngine;

namespace Pumkin.Presets
{
    public class CreatePosePresetPopup : CreatePresetPopupBase
    {
        public static string[] defaultMusclesNames;

        SerializedObject serializedPosePreset;
        SerializedProperty pMuscles,
            pTransformPaths,
            pTransformRotations;

        private void OnEnable()
        {
            if(defaultMusclesNames == null || defaultMusclesNames.Length == 0)
                defaultMusclesNames = HumanTrait.MuscleName;

            SetupProperties();
        }

        private void SetupProperties()
        {
            if(preset is null)
                Close();

            serializedPosePreset = new SerializedObject(preset);

            pMuscles = serializedPosePreset.FindProperty("muscles");
            pTransformPaths = serializedPosePreset.FindProperty("transformPaths");
            pTransformRotations = serializedPosePreset.FindProperty("transformRotations");
        }

        public static void ShowWindow(PumkinsPosePreset newPreset = null)
        {
            AssignOrCreatePreset<PumkinsPosePreset>(newPreset);

            if(!_window || _window.GetType() != typeof(CreatePosePresetPopup))
            {
                _window = CreateInstance<CreatePosePresetPopup>();
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
                _window.titleContent = new GUIContent(Strings.Presets.editPosePreset);
                _overwriteFile = true;
            }
            else
            {
                _window.titleContent = new GUIContent(Strings.Presets.createPosePreset);
            }

            _window.ShowUtility();
        }

        private void OnGUI()
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);
            try
            {
                PumkinsPosePreset preset = (PumkinsPosePreset)CreatePresetPopupBase.preset;
                if(!preset)
                {
                    AssignOrCreatePreset<PumkinsPosePreset>(null);
                    return;
                }

                EditorGUILayout.Space();

                preset.name = EditorGUILayout.TextField(Strings.Presets.presetName, preset.name);

                Helpers.DrawGUILine();

                PumkinsAvatarTools.DrawAvatarSelectionWithButtonGUI(false, false);

                Helpers.DrawGUILine();

                preset.presetMode = (PumkinsPosePreset.PosePresetMode)EditorGUILayout.EnumPopup(Strings.Presets.poseMode, preset.presetMode);

                Helpers.DrawGUILine();

                if(GUILayout.Button(Strings.Buttons.openPoseEditor, Styles.BigButton))
                    PumkinsPoseEditor.ShowWindow();

                Helpers.DrawGUILine();

                EditorGUILayout.EndScrollView();

                Helpers.DrawGUILine();

                EditorGUI.BeginDisabledGroup(!PumkinsAvatarTools.SelectedAvatar || !preset || string.IsNullOrEmpty(preset.name));
                {
                    if(!editingExistingPreset)
                    {
                        _overwriteFile = GUILayout.Toggle(_overwriteFile, Strings.Presets.overwriteFile);
                        if(GUILayout.Button(Strings.Buttons.savePreset, Styles.BigButton))
                        {
                            preset.SetupPreset(preset.name, PumkinsAvatarTools.SelectedAvatar, preset.presetMode);
                            if(preset)
                            {
                                EditorApplication.delayCall += () =>
                                {
                                    preset.SavePreset(_overwriteFile);
                                    Close();
                                };
                            }
                        }
                    }
                }
                EditorGUI.EndDisabledGroup();
                CreatePresetPopupBase.preset = preset;
                
                EditorGUILayout.Space();
            }
            catch
            {
                EditorGUILayout.EndScrollView();                
                Close();
            }            
        }

        protected override void RefreshSelectedPresetIndex()
        {
            PumkinsAvatarTools.RefreshPresetIndexByString<PumkinsPosePreset>(preset.name);
        }

        private void OnDestroy()
        {
            if(editingExistingPreset)
            {
                PumkinsPosePreset preset = (PumkinsPosePreset)CreatePresetPopupBase.preset;
                if(!preset)
                    AssignOrCreatePreset<PumkinsPosePreset>(null);
                preset.SetupPreset(preset.name, PumkinsAvatarTools.SelectedAvatar, preset.presetMode);
            }
        }
    }
}