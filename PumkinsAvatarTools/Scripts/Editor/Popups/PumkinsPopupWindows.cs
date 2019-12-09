using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Pumkin.DataStructures;
using Pumkin.HelperFunctions;
using Pumkin.AvatarTools;
using Pumkin.Presets;

public abstract class CreatePresetPopupBase : EditorWindow
{
    static protected CreatePresetPopupBase _window;
    static protected PumkinPreset preset;

    static protected GameObject avatar;
    static protected bool _overwriteFile = true;

    static protected Vector2 scroll = Vector2.zero;

    static protected void AssignOrCreatePreset<T>(PumkinPreset newPreset) where T : PumkinPreset
    {
        PumkinsPresetManager.CleanupPresetsOfType<T>();
        if(!newPreset)
            newPreset = CreateInstance<T>();
        preset = newPreset;            
    }
}

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

            PumkinsAvatarTools.SelectedAvatar = (GameObject)EditorGUILayout.ObjectField("Avatar", PumkinsAvatarTools.SelectedAvatar, typeof(GameObject), true);

            Helpers.DrawGuiLine();

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

            Helpers.DrawGuiLine();

            PumkinsAvatarTools.Instance.DrawOverlayGUI();

            EditorGUILayout.Space();

            PumkinsAvatarTools.Instance.DrawBackgroundGUI();

            EditorGUILayout.Space();

            Helpers.DrawGuiLine();

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
}

public class CreatePosePresetPopup : CreatePresetPopupBase
{    
    public static void ShowWindow(PumkinsPosePreset newPreset = null)
    {
        AssignOrCreatePreset<PumkinsPosePreset>(newPreset);

        if(!_window)
        {
            _window = CreateInstance<CreatePosePresetPopup>();
            _window.autoRepaintOnSceneChange = true;
        }

        if(newPreset)
        {
            _window.titleContent = new GUIContent("Edit Pose Preset");
            _overwriteFile = true;
        }
        else
        {
            _window.titleContent = new GUIContent("Create Pose Preset");
        }

        _window.ShowUtility();
    }

    private void OnGUI()
    {
        PumkinsPosePreset preset = (PumkinsPosePreset)CreatePresetPopupBase.preset;
        if(!preset)
            AssignOrCreatePreset<PumkinsPosePreset>(null);

        scroll = EditorGUILayout.BeginScrollView(scroll);
        {
            EditorGUILayout.Space();

            preset.name = EditorGUILayout.TextField("Preset Name", preset.name);

            Helpers.DrawGuiLine();

            PumkinsAvatarTools.SelectedAvatar = (GameObject)EditorGUILayout.ObjectField("Avatar", PumkinsAvatarTools.SelectedAvatar, typeof(GameObject), true);

            Helpers.DrawGuiLine();

            preset.presetMode = (PumkinsPosePreset.PosePresetMode)EditorGUILayout.EnumPopup("Pose Mode", preset.presetMode);

            preset.SetupPreset(preset.name, PumkinsAvatarTools.SelectedAvatar, preset.presetMode);

            EditorGUI.BeginDisabledGroup(!PumkinsAvatarTools.SelectedAvatar || !preset || string.IsNullOrEmpty(preset.name));
            {
                _overwriteFile = GUILayout.Toggle(_overwriteFile, "Overwrite File");
                if(GUILayout.Button("Save Preset", Styles.BigButton))
                {                    
                    preset.SavePreset(_overwriteFile);
                }                
            }
            EditorGUI.EndDisabledGroup();

        }
        EditorGUILayout.EndScrollView();
    }
}

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

    private void OnGUI()
    {
        PumkinsBlendshapePreset preset = (PumkinsBlendshapePreset)CreatePresetPopupBase.preset;
        if(!preset)
            AssignOrCreatePreset<PumkinsBlendshapePreset>(preset);

        scroll = EditorGUILayout.BeginScrollView(scroll);
        {
            EditorGUILayout.Space();

            preset.name = EditorGUILayout.TextField("Preset Name", preset.name);

            Helpers.DrawGuiLine();

            EditorGUI.BeginChangeCheck();
            {
                PumkinsAvatarTools.SelectedAvatar = (GameObject)EditorGUILayout.ObjectField("Avatar", PumkinsAvatarTools.SelectedAvatar, typeof(GameObject), true);
            }
            if(EditorGUI.EndChangeCheck())
            {
                preset.SetupPreset(preset.name, PumkinsAvatarTools.SelectedAvatar);
            }

            Helpers.DrawGuiLine();

            EditorGUI.BeginDisabledGroup(!PumkinsAvatarTools.SelectedCamera || string.IsNullOrEmpty(preset.name) || !PumkinsAvatarTools.SelectedAvatar);
            {
                _overwriteFile = GUILayout.Toggle(_overwriteFile, "Overwrite File");
                if(GUILayout.Button("Save Preset", Styles.BigButton))
                {
                    preset.SavePreset(_overwriteFile);                    
                }
            }
            EditorGUI.EndDisabledGroup();
        }
        EditorGUILayout.EndScrollView();
    }
}