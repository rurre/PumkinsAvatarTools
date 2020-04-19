using Pumkin.AvatarTools;
using Pumkin.DataStructures;
using Pumkin.HelperFunctions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Pumkin.Presets
{
    public abstract class PumkinPreset : ScriptableObject
    {
        public new string name;        
        public abstract bool ApplyPreset(GameObject avatar);
        public override string ToString()
        {
            return name;
        }
    }

    public static class PumkinsPresetManager
    {
        public static readonly string presetsPath = "Presets/";
        public static readonly string camerasPath = presetsPath + "Cameras";
        public static readonly string posesPath = presetsPath + "Poses";
        public static readonly string blendshapesPath = presetsPath + "Blendshapes";

        static List<PumkinsCameraPreset> _cameraPresets;
        static List<PumkinsPosePreset> _humanPosePresets;
        static List<PumkinsBlendshapePreset> _blendshapePresets;

        public static List<PumkinsCameraPreset> CameraPresets
        {
            get
            {
                if(_cameraPresets == null)
                {
                    _cameraPresets = new List<PumkinsCameraPreset>();
                    LoadPresets<PumkinsCameraPreset>();
                }
                return _cameraPresets;
            }
            private set { _cameraPresets = value; }
        }
        public static List<PumkinsPosePreset> PosePresets
        {
            get
            {
                if(_humanPosePresets == null)
                {
                    _humanPosePresets = new List<PumkinsPosePreset>();
                    LoadPresets<PumkinsPosePreset>();
                }
                return _humanPosePresets;
            }
            private set { _humanPosePresets = value; }
        }
        public static List<PumkinsBlendshapePreset> BlendshapePresets
        {
            get
            {
                if(_blendshapePresets == null)
                {
                    _blendshapePresets = new List<PumkinsBlendshapePreset>();
                    LoadPresets<PumkinsBlendshapePreset>();
                }
                return _blendshapePresets;
            }
            private set { _blendshapePresets = value; }
        }        

        /// <summary>
        /// Destroys loose preset objects that live in memory and not in assets 
        /// </summary>        
        public static void CleanupPresetsOfType<T>() where T : PumkinPreset
        {
            var tools = PumkinsAvatarTools.Instance;
            foreach(var p in GameObject.FindObjectsOfType<T>())
            {
                if(p && !Helpers.IsAssetInAssets(p))
                {
                    PumkinsAvatarTools.LogVerbose("Destroying orphanned " + typeof(T).Name);
                    Helpers.DestroyAppropriate(p);
                }
            }

            if(typeof(T) == typeof(PumkinsCameraPreset))            
                CameraPresets.RemoveAll(o => o == default(PumkinsCameraPreset) || string.IsNullOrEmpty(AssetDatabase.GetAssetPath(o)) || string.IsNullOrEmpty(o.name));            
            else if(typeof(T) == typeof(PumkinsPosePreset))            
                PosePresets.RemoveAll(o => o == default(PumkinsPosePreset) || string.IsNullOrEmpty(AssetDatabase.GetAssetOrScenePath(o)) || string.IsNullOrEmpty(o.name));
            else if(typeof(T) == typeof(PumkinsBlendshapePreset))
                BlendshapePresets.RemoveAll(o => o == default(PumkinsBlendshapePreset) || string.IsNullOrEmpty(AssetDatabase.GetAssetOrScenePath(o)) || string.IsNullOrEmpty(o.name));

            PumkinsAvatarTools.RefreshPresetIndex<T>();
        }

        public static void LoadPresets<T>() where T : PumkinPreset
        {
            if(typeof(T) == typeof(PumkinsCameraPreset))
            {                
                Resources.LoadAll<PumkinsCameraPreset>(camerasPath);
                CameraPresets = Resources.FindObjectsOfTypeAll<PumkinsCameraPreset>().ToList();
                CleanupPresetsOfType<PumkinsCameraPreset>();                
            }
            else if(typeof(T) == typeof(PumkinsPosePreset))
            {
                Resources.LoadAll<PumkinsPosePreset>(posesPath);
                PosePresets = Resources.FindObjectsOfTypeAll<PumkinsPosePreset>().ToList();
                CleanupPresetsOfType<PumkinsPosePreset>();
            }
            else if(typeof(T) == typeof(PumkinsBlendshapePreset))
            {
                Resources.LoadAll<PumkinsBlendshapePreset>(blendshapesPath);
                BlendshapePresets = Resources.FindObjectsOfTypeAll<PumkinsBlendshapePreset>().ToList();
                CleanupPresetsOfType<PumkinsBlendshapePreset>();
            }
        }        
        public static int GetPresetIndex<T>(T preset) where T : PumkinPreset
        {
            int i = -1;

            if(typeof(T) == typeof(PumkinsCameraPreset))
                i = CameraPresets.IndexOf(preset as PumkinsCameraPreset);
            else if(typeof(T) == typeof(PumkinsPosePreset))
                i = PosePresets.IndexOf(preset as PumkinsPosePreset);
            else if(typeof(T) == typeof(PumkinsBlendshapePreset))
                i = BlendshapePresets.IndexOf(preset as PumkinsBlendshapePreset);

            return i;
        }
        public static int GetPresetIndex<T>(string name) where T : PumkinPreset
        {
            int i = -1;

            if(typeof(T) == typeof(PumkinsCameraPreset))
                i = CameraPresets.FindIndex(o => o.name.ToLower() == name.ToLower());
            else if(typeof(T) == typeof(PumkinsPosePreset))
                i = PosePresets.FindIndex(o => o.name.ToLower() == name.ToLower());
            else if(typeof(T) == typeof(PumkinsBlendshapePreset))
                i = BlendshapePresets.FindIndex(o => o.name.ToLower() == name.ToLower());

            return i;
        }
        public static string GetPresetName<T>(int index) where T : PumkinPreset
        {
            if(typeof(T) == typeof(PumkinsCameraPreset))
            {
                if(CameraPresets[index])
                    return CameraPresets[index].name;
            }
            else if(typeof(T) == typeof(PumkinsPosePreset))
            {
                if(PosePresets[index])
                    return PosePresets[index].name;
            }
            else if(typeof(T) == typeof(PumkinsBlendshapePreset))
            {
                if(BlendshapePresets[index])
                    return BlendshapePresets[index].name;
            }
            return "";
        }

        public static T FindPreset<T>(string name) where T : PumkinPreset
        {
            object preset = null;

            if(typeof(T) == typeof(PumkinsCameraPreset))
                preset = CameraPresets.Find(o => o.name.ToLower() == name.ToLower());
            else if(typeof(T) == typeof(PumkinsCameraPreset))
                preset = PosePresets.Find(o => o.name.ToLower() == name.ToLower());
            else if(typeof(T) == typeof(PumkinsBlendshapePreset))
                preset = BlendshapePresets.Find(o => o.name.ToLower() == name.ToLower());

            return (T)preset;
        }        
        public static int GetPresetCountOfType<T>() where T : PumkinPreset
        {
            if(typeof(T) == typeof(PumkinsCameraPreset))
                return CameraPresets.Count;
            else if(typeof(T) == typeof(PumkinsPosePreset))
                return PosePresets.Count;
            else if(typeof(T) == typeof(PumkinsBlendshapePreset))
                return BlendshapePresets.Count;

            return -1;
        }
        public static void SelectPresetInToolWindow<T>(T preset, bool openWindow = true) where T : PumkinPreset
        {            
            int index = GetPresetIndex<T>(preset.name);
            if(index != -1)
            {
                PumkinsAvatarTools.PresetToolbarOptions toolbarOption = PumkinsAvatarTools.PresetToolbarOptions.Camera;

                if(openWindow)
                {
                    _PumkinsAvatarToolsWindow.ShowWindow();
                    PumkinsAvatarTools.Instance._misc_expand = false;
                    PumkinsAvatarTools.Instance._tools_expand = false;
                    PumkinsAvatarTools.Instance._thumbnails_expand = true;
                }                

                if(typeof(T) == typeof(PumkinsCameraPreset))
                {
                    PumkinsAvatarTools.Instance._selectedCameraPresetString = preset.name;
                    PumkinsAvatarTools.Instance._selectedCameraPresetIndex = index;
                    toolbarOption = PumkinsAvatarTools.PresetToolbarOptions.Camera;
                }
                else if(typeof(T) == typeof(PumkinsPosePreset))
                {
                    PumkinsAvatarTools.Instance._selectedPosePresetString = preset.name;
                    PumkinsAvatarTools.Instance._selectedPosePresetIndex = index;
                    toolbarOption = PumkinsAvatarTools.PresetToolbarOptions.Pose;
                }
                else if(typeof(T) == typeof(PumkinsBlendshapePreset))
                {
                    PumkinsAvatarTools.Instance._selectedBlendshapePresetString = preset.name;
                    PumkinsAvatarTools.Instance._selectedBlendshapePresetIndex = index;
                    toolbarOption = PumkinsAvatarTools.PresetToolbarOptions.Blendshape;
                }

                if(openWindow)
                    PumkinsAvatarTools.Instance.SelectThumbnailPresetToolbarOption(toolbarOption);
            }            
        }
    }
}
