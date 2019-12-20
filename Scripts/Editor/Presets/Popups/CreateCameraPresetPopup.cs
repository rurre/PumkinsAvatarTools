using Pumkin.AvatarTools;
using Pumkin.DataStructures;
using Pumkin.HelperFunctions;
using UnityEditor;
using UnityEngine;

namespace Pumkin.Presets
{
    public class CreateCamerePresetPopup : CreatePresetPopupBase
    {
        static Transform referenceTransform;
        static RenderTexture tempRT = null;

        public static void ShowWindow(PumkinsCameraPreset newPreset = null)
        {
            AssignOrCreatePreset<PumkinsCameraPreset>(newPreset);

            if(!_window)
            {
                _window = CreateInstance<CreateCamerePresetPopup>();
                _window.autoRepaintOnSceneChange = true;
            }

            if(newPreset)
            {
                _window.titleContent = new GUIContent("Edit Camera Preset");
                _overwriteFile = true;
            }
            else
            {
                _window.titleContent = new GUIContent("Create Camera Preset");
            }

            _window.ShowUtility();
        }

        void OnInspectorUpdate()
        {
            Repaint();
        }

        void OnGUI()
        {
            PumkinsCameraPreset preset = (PumkinsCameraPreset)CreatePresetPopupBase.preset;
            Camera newCam;

            if(!preset)
                AssignOrCreatePreset<PumkinsCameraPreset>(null);

            Rect r = GUILayoutUtility.GetAspectRect(1.3f);
            EditorGUI.DrawPreviewTexture(r, PumkinsAvatarTools.RTTexture, PumkinsAvatarTools.RTMaterial, ScaleMode.ScaleToFit);

            EditorGUILayout.HelpBox(Strings.Thumbnails.previewIsDark, MessageType.Info);

            scroll = EditorGUILayout.BeginScrollView(scroll);
            {
                preset.name = EditorGUILayout.TextField("Preset Name", preset.name);

                EditorGUILayout.Space();

                PumkinsAvatarTools.DrawAvatarSelectionWithButton(false, false);

                Helpers.DrawGUILine();

                EditorGUI.BeginChangeCheck();
                {
                    newCam = (Camera)EditorGUILayout.ObjectField("Camera", PumkinsAvatarTools.SelectedCamera, typeof(Camera), true);
                }
                if(EditorGUI.EndChangeCheck())
                {
                    PumkinsAvatarTools.SelectedCamera.targetTexture = tempRT;
                    tempRT = newCam.targetTexture;

                    PumkinsAvatarTools.SelectedCamera = newCam;
                }

                EditorGUILayout.Space();

                preset.offsetMode = (PumkinsCameraPreset.CameraOffsetMode)EditorGUILayout.EnumPopup("Offset Mode", preset.offsetMode);
                if(preset.offsetMode == PumkinsCameraPreset.CameraOffsetMode.Transform)
                {
                    EditorGUI.BeginDisabledGroup(!PumkinsAvatarTools.SelectedAvatar);
                    {
                        referenceTransform = EditorGUILayout.ObjectField("Transform", referenceTransform, typeof(Transform), true) as Transform;
                        if(referenceTransform && !referenceTransform.IsChildOf(PumkinsAvatarTools.SelectedAvatar.transform))
                        {
                            PumkinsAvatarTools.Log("{0} doesn't belong to avatar {1}.", LogType.Warning, referenceTransform.name, PumkinsAvatarTools.SelectedAvatar.name);
                            referenceTransform = null;
                        }
                    }
                }
                EditorGUILayout.Space();

                Helpers.DrawGUILine();

                PumkinsAvatarTools.Instance.DrawOverlayGUI();

                EditorGUILayout.Space();

                //PumkinsAvatarTools.Instance.DrawBackgroundGUI();

                EditorGUILayout.Space();

                Helpers.DrawGUILine();

                EditorGUI.BeginDisabledGroup(!PumkinsAvatarTools.SelectedCamera || string.IsNullOrEmpty(preset.name) || !PumkinsAvatarTools.SelectedAvatar);
                {
                    _overwriteFile = GUILayout.Toggle(_overwriteFile, "Overwrite File");
                    if(GUILayout.Button("Save Preset", Styles.BigButton))
                    {
                        if(preset.offsetMode == PumkinsCameraPreset.CameraOffsetMode.Transform)
                            preset.SavePreset(referenceTransform.gameObject, PumkinsAvatarTools.SelectedCamera, _overwriteFile);
                        else
                            preset.SavePreset(PumkinsAvatarTools.SelectedAvatar, PumkinsAvatarTools.SelectedCamera, _overwriteFile);
                    }
                }
                EditorGUI.EndDisabledGroup();
            }
            EditorGUILayout.EndScrollView();

            CreatePresetPopupBase.preset = preset;
        }

        void OnEnable()
        {
            Camera cam = PumkinsAvatarTools.SelectedCamera;
            if(cam)
            {
                tempRT = cam.activeTexture;
                if(!cam.targetTexture)
                    cam.targetTexture = PumkinsAvatarTools.DefaultRT;

                cam.farClipPlane = 1000;
                cam.nearClipPlane = 0.01f;

                PumkinsAvatarTools.RTTexture = cam.targetTexture;
            }
        }

        void OnDestroy()
        {
            PumkinsAvatarTools.SelectedCamera.targetTexture = tempRT;
        }

        protected override void RefreshSelectedPresetIndex()
        {
            PumkinsAvatarTools.RefreshPresetIndexByString<PumkinsCameraPreset>(preset.name);
        }
    }
}