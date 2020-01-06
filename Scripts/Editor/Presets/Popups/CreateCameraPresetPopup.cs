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

        public static void ShowWindow(PumkinsCameraPreset newPreset = null)
        {
            AssignOrCreatePreset<PumkinsCameraPreset>(newPreset);

            if(!_window)
            {
                _window = CreateInstance<CreateCamerePresetPopup>();
                _window.autoRepaintOnSceneChange = true;
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

        private static void RefreshReferenceTransform()
        {
            PumkinsCameraPreset p = (PumkinsCameraPreset)preset;
            if(!p || p.offsetMode == PumkinsCameraPreset.CameraOffsetMode.Viewpoint)
                return;

            GameObject avatar = PumkinsAvatarTools.SelectedAvatar;
            if(!string.IsNullOrEmpty(p.transformPath) && avatar)
                referenceTransform = avatar.transform.Find(p.transformPath);
        }

        private void OnDestroy()
        {
            PumkinsAvatarTools.RestoreCameraRT(PumkinsAvatarTools.SelectedCamera);
            if(editingExistingPreset)            
                GetNewOffsetsAndApplyToPreset();            
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
                else
                    p.CalculateOffsets(PumkinsAvatarTools.SelectedAvatar.GetComponent<VRCSDK2.VRC_AvatarDescriptor>(), camera);                
            }
            preset = p;
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

                    PumkinsAvatarTools.DrawAvatarSelectionWithButton(false, false);

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

                    if(!editingExistingPreset)
                    {
                        EditorGUI.BeginDisabledGroup(!PumkinsAvatarTools.SelectedCamera || string.IsNullOrEmpty(newPreset.name) || !PumkinsAvatarTools.SelectedAvatar);
                        {
                            _overwriteFile = GUILayout.Toggle(_overwriteFile, Strings.Presets.overwriteFile);
                            if(GUILayout.Button(Strings.Buttons.savePreset, Styles.BigButton))
                            {
                                if(newPreset.offsetMode == PumkinsCameraPreset.CameraOffsetMode.Transform)
                                    newPreset.SavePreset(referenceTransform.gameObject, PumkinsAvatarTools.SelectedCamera, _overwriteFile);                                
                                else
                                    newPreset.SavePreset(PumkinsAvatarTools.SelectedAvatar, PumkinsAvatarTools.SelectedCamera, _overwriteFile);
                            }
                        }
                        EditorGUI.EndDisabledGroup();
                    }
                }
            }
            EditorGUILayout.EndScrollView();            
        }
    }
}