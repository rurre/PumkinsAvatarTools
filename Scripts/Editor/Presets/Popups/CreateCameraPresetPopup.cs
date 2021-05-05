using Pumkin.AvatarTools;
using Pumkin.DataStructures;
using Pumkin.HelperFunctions;
using UnityEditor;
using UnityEngine;
#if VRC_SDK_VRCSDK2 || (VRC_SDK_VRCSDK3 && !UDON)
using VRC.SDKBase;
#endif

namespace Pumkin.Presets
{
    public class CreateCameraPresetPopup : CreatePresetPopupBase
    {
        static Transform referenceTransform;

        public static void ShowWindow(PumkinsCameraPreset newPreset = null)
        {
            AssignOrCreatePreset<PumkinsCameraPreset>(newPreset);

            if(!_window || _window.GetType() != typeof(CreateCameraPresetPopup))
            {
                _window = CreateInstance<CreateCameraPresetPopup>();
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
                _window.titleContent = new GUIContent(Strings.Presets.editCameraPreset);
                _overwriteFile = true;
            }
            else
            {
                _window.titleContent = new GUIContent(Strings.Presets.createCameraPreset);
            }

            _window.ShowUtility();
        }

        void OnEnable()
        {
            PumkinsAvatarTools.CameraSelectionChanged += HandleCameraSelectionChanged;
            PumkinsAvatarTools.SelectedCamera.targetTexture = PumkinsAvatarTools.DefaultRT;

            RefreshReferenceTransform();
        }

        private void OnDestroy()
        {
            PumkinsAvatarTools.RestoreCameraRT(PumkinsAvatarTools.SelectedCamera);
            if(editingExistingPreset)
                GetNewOffsetsAndApplyToPreset();
        }

        private new void OnDisable()
        {
            base.OnDisable();
            PumkinsAvatarTools.CameraSelectionChanged -= HandleCameraSelectionChanged;
        }

        private void HandleCameraSelectionChanged(Camera camera)
        {
            if(!camera)
                return;

            camera.targetTexture = PumkinsAvatarTools.DefaultRT;
            RefreshReferenceTransform();
        }

        protected override void RefreshSelectedPresetIndex()
        {
            PumkinsAvatarTools.RefreshPresetIndexByString<PumkinsCameraPreset>(preset.name);
        }

        void OnInspectorUpdate()
        {
            Repaint();
        }

        void OnGUI()
        {
            try
            {
                PumkinsCameraPreset newPreset = (PumkinsCameraPreset)preset;
                if(!newPreset) //Not sure I like this part
                {
                    newPreset = (PumkinsCameraPreset)AssignOrCreatePreset<PumkinsCameraPreset>(null);
                    preset = newPreset;
                }

                Rect r = GUILayoutUtility.GetAspectRect(1.3f);
                EditorGUI.DrawTextureTransparent(r, PumkinsAvatarTools.SelectedCamRT, ScaleMode.ScaleToFit);

                scroll = EditorGUILayout.BeginScrollView(scroll);
                {
                    if(newPreset)
                    {
                        newPreset.name = EditorGUILayout.TextField(Strings.Presets.presetName, newPreset.name);

                        EditorGUILayout.Space();

                        PumkinsAvatarTools.DrawAvatarSelectionWithButtonGUI(false, false);

                        Helpers.DrawGUILine();

                        PumkinsAvatarTools.SelectedCamera = (Camera)EditorGUILayout.ObjectField(Strings.Presets.camera, PumkinsAvatarTools.SelectedCamera, typeof(Camera), true);

                        EditorGUILayout.Space();

                        newPreset.offsetMode = (PumkinsCameraPreset.CameraOffsetMode)EditorGUILayout.EnumPopup(Strings.Presets.offsetMode, newPreset.offsetMode);
                        if(newPreset.offsetMode == PumkinsCameraPreset.CameraOffsetMode.Transform)
                        {
                            EditorGUI.BeginDisabledGroup(!PumkinsAvatarTools.SelectedAvatar);
                            {
                                referenceTransform = EditorGUILayout.ObjectField(Strings.Presets.transform, referenceTransform, typeof(Transform), true) as Transform;
                                if(referenceTransform && !referenceTransform.IsChildOf(PumkinsAvatarTools.SelectedAvatar.transform))
                                {
                                    PumkinsAvatarTools.Log(Strings.Presets.transformDoesntBelongToAvatar, LogType.Warning, referenceTransform.name, PumkinsAvatarTools.SelectedAvatar.name);
                                    referenceTransform = null;
                                }
                            }
                        }
                        EditorGUILayout.Space();

                        Helpers.DrawGUILine();

                        PumkinsAvatarTools.Instance.DrawOverlayGUI();

                        Helpers.DrawGUILine();

                        PumkinsAvatarTools.Instance.DrawBackgroundGUI();

                        Helpers.DrawGUILine();

                        PumkinsAvatarTools.Instance.DrawCameraControlButtons();
                    }
                    EditorGUILayout.EndScrollView();

                    Helpers.DrawGUILine();

                    if(!editingExistingPreset)
                    {
                        EditorGUI.BeginDisabledGroup(!PumkinsAvatarTools.SelectedCamera || string.IsNullOrEmpty(newPreset.name) || !PumkinsAvatarTools.SelectedAvatar);
                        {
                            _overwriteFile = GUILayout.Toggle(_overwriteFile, Strings.Presets.overwriteFile);
                            EditorGUI.BeginDisabledGroup(newPreset.offsetMode == PumkinsCameraPreset.CameraOffsetMode.Transform && referenceTransform == null);
                            {
                                if(GUILayout.Button(Strings.Buttons.savePreset, Styles.BigButton))
                                {
#if (VRC_SDK_VRCSDK3 || VRC_SDK_VRCSDK2) && !UDON
                                    if(newPreset.offsetMode == PumkinsCameraPreset.CameraOffsetMode.Viewpoint && (Avatar.GetComponent<VRC_AvatarDescriptor>() == null))
                                    {
                                        PumkinsAvatarTools.Log(Strings.Log.descriptorIsMissingCantGetViewpoint, LogType.Warning);
                                    }
                                    else
#endif
                                    if(newPreset.offsetMode == PumkinsCameraPreset.CameraOffsetMode.Transform)
                                    {
                                        EditorApplication.delayCall += () =>
                                        {
                                            newPreset.SavePreset(referenceTransform.gameObject, PumkinsAvatarTools.SelectedCamera, _overwriteFile);
                                            Close();
                                        };
                                    }
                                    else
                                    {
                                        EditorApplication.delayCall += () =>
                                        {
                                            newPreset.SavePreset(PumkinsAvatarTools.SelectedAvatar, PumkinsAvatarTools.SelectedCamera, _overwriteFile);
                                            Close();
                                        };
                                    }
                                }
                                EditorGUILayout.Space();
                            }
                            EditorGUI.EndDisabledGroup();
                        }
                        EditorGUI.EndDisabledGroup();
                    }
                }
            }
            catch
            {
                if(this)
                    Close();
            }
        }

        private static void RefreshReferenceTransform()
        {
            PumkinsCameraPreset p = (PumkinsCameraPreset)preset;
            if(!p)
                return;
#if (VRC_SDK_VRCSDK3 || VRC_SDK_VRCSDK2) && !UDON
            if(p.offsetMode == PumkinsCameraPreset.CameraOffsetMode.Viewpoint)
                return;
#endif

            GameObject avatar = PumkinsAvatarTools.SelectedAvatar;
            if(!string.IsNullOrEmpty(p.transformPath) && avatar)
                referenceTransform = avatar.transform.Find(p.transformPath);
        }

        private static void GetNewOffsetsAndApplyToPreset()
        {
            PumkinsCameraPreset p = (PumkinsCameraPreset)preset;
            Camera camera = PumkinsAvatarTools.SelectedCamera;
            GameObject avatar = PumkinsAvatarTools.SelectedAvatar;
            if(avatar && camera && p)
            {
                if(p.offsetMode == PumkinsCameraPreset.CameraOffsetMode.AvatarRoot)
                    p.CalculateOffsets(avatar.transform.root, camera);
                else if(p.offsetMode == PumkinsCameraPreset.CameraOffsetMode.Transform && referenceTransform)
                    p.CalculateOffsets(referenceTransform, camera);
#if (VRC_SDK_VRCSDK3 || VRC_SDK_VRCSDK2) && !UDON
                else
                    p.CalculateOffsets(PumkinsAvatarTools.SelectedAvatar.GetComponent<VRC_AvatarDescriptor>(), camera);
#endif
            }
            preset = p;
        }
    }
}