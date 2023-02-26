using Pumkin.AvatarTools;
using Pumkin.DataStructures;
using Pumkin.HelperFunctions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
        public static readonly string resourcePresetsPath = "Presets/";
        public static readonly string resourceCamerasPath = resourcePresetsPath + "Cameras";
        public static readonly string resourcePosesPath = resourcePresetsPath + "Poses";
        public static readonly string resourceBlendshapesPath = resourcePresetsPath + "Blendshapes";

        public static readonly string localPresetsPath = PumkinsAvatarTools.MainFolderPathLocal + "/resources/" + resourcePresetsPath;
        public static readonly string localCamerasPath = PumkinsAvatarTools.MainFolderPathLocal + "/resources/" + resourceCamerasPath;
        public static readonly string localPosesPath = PumkinsAvatarTools.MainFolderPathLocal + "/resources/" + resourcePosesPath;
        public static readonly string localBlendshapesPath = PumkinsAvatarTools.MainFolderPathLocal + "/resources/" + resourceBlendshapesPath;

        static string cameraPresetScriptGUID = null;
        static string blendshapePresetScriptGUID = null;
        static string posePresetScriptGUID = null;

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
            string typeName = typeof(T).Name;
            var presets = GameObject.FindObjectsOfType<T>();

            Type t = typeof(T);
            Type tP = typeof(PumkinPreset);

            foreach(var preset in presets)
            {
                if(preset && !Helpers.IsAssetInAssets(preset))
                {
                    PumkinsAvatarTools.LogVerbose($"Destroying orphanned {typeName}");
                    Helpers.DestroyAppropriate(preset);
                }
            }

            if(typeof(T) == typeof(PumkinsCameraPreset) || t == tP)
                CameraPresets.RemoveAll(o => o == default(PumkinsCameraPreset) || !Helpers.IsAssetInAssets(o));
            if(typeof(T) == typeof(PumkinsPosePreset) || t == tP)
                PosePresets.RemoveAll(o => o == default(PumkinsPosePreset) || !Helpers.IsAssetInAssets(o));
            if(typeof(T) == typeof(PumkinsBlendshapePreset) || t == tP)
                BlendshapePresets.RemoveAll(o => o == default(PumkinsBlendshapePreset) || !Helpers.IsAssetInAssets(o));

            PumkinsAvatarTools.RefreshPresetIndex<T>();
        }

        public static void ValidatedPresetNames<T>() where T : PumkinPreset
        {
            bool dirty = false;
            if(typeof(T) == typeof(PumkinsCameraPreset))
            {
                foreach(var preset in CameraPresets)
                    if(string.IsNullOrEmpty(preset.name))
                    {
                        preset.name = "Camera Preset";
                        EditorUtility.SetDirty(preset);
                        dirty = true;
                    }
            }
            else if(typeof(T) == typeof(PumkinsPosePreset))
            {
                foreach(var preset in PosePresets)
                    if(string.IsNullOrEmpty(preset.name))
                    {
                        preset.name = "Pose Preset";
                        EditorUtility.SetDirty(preset);
                        dirty = true;
                    }
            }
            else if(typeof(T) == typeof(PumkinsBlendshapePreset))
            {
                foreach(var preset in BlendshapePresets)
                    if(string.IsNullOrEmpty(preset.name))
                    {
                        preset.name = "Blendshape Preset";
                        EditorUtility.SetDirty(preset);
                        dirty = true;
                    }
            }
            
            if(dirty)
                AssetDatabase.SaveAssets();
        }

        public static void LoadPresets<T>() where T : PumkinPreset
        {
            Type t = typeof(T);
            Type pT = typeof(PumkinPreset);
            FixScriptReferences<T>();
            if(t == typeof(PumkinsCameraPreset) || t == pT)
            {
                Resources.LoadAll<PumkinsCameraPreset>(resourceCamerasPath);
                
                CameraPresets = Resources.FindObjectsOfTypeAll<PumkinsCameraPreset>().ToList();
                CleanupPresetsOfType<PumkinsCameraPreset>();
            }
            if(typeof(T) == typeof(PumkinsPosePreset) || t == pT)
            {
                Resources.LoadAll<PumkinsPosePreset>(resourcePosesPath);
                PosePresets = Resources.FindObjectsOfTypeAll<PumkinsPosePreset>().ToList();
                CleanupPresetsOfType<PumkinsPosePreset>();
            }
            if(typeof(T) == typeof(PumkinsBlendshapePreset) || t == pT)
            {
                Resources.LoadAll<PumkinsBlendshapePreset>(resourceBlendshapesPath);
                BlendshapePresets = Resources.FindObjectsOfTypeAll<PumkinsBlendshapePreset>().ToList();
                CleanupPresetsOfType<PumkinsBlendshapePreset>();
            }

            ValidatedPresetNames<T>();
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
                    PumkinsAvatarTools.Settings._info_expand = false;
                    PumkinsAvatarTools.Settings._tools_expand = false;
                    PumkinsAvatarTools.Settings._thumbnails_expand = true;
                }

                if(typeof(T) == typeof(PumkinsCameraPreset))
                {
                    PumkinsAvatarTools.Settings._selectedCameraPresetString = preset.name;
                    PumkinsAvatarTools.Settings._selectedCameraPresetIndex = index;
                    toolbarOption = PumkinsAvatarTools.PresetToolbarOptions.Camera;
                }
                else if(typeof(T) == typeof(PumkinsPosePreset))
                {
                    PumkinsAvatarTools.Settings._selectedPosePresetString = preset.name;
                    PumkinsAvatarTools.Settings._selectedPosePresetIndex = index;
                    toolbarOption = PumkinsAvatarTools.PresetToolbarOptions.Pose;
                }
                else if(typeof(T) == typeof(PumkinsBlendshapePreset))
                {
                    PumkinsAvatarTools.Settings._selectedBlendshapePresetString = preset.name;
                    PumkinsAvatarTools.Settings._selectedBlendshapePresetIndex = index;
                    toolbarOption = PumkinsAvatarTools.PresetToolbarOptions.Blendshape;
                }

                if(openWindow)
                    PumkinsAvatarTools.Instance.SelectThumbnailPresetToolbarOption(toolbarOption);
            }
        }

        static void SetupGUID<T>() where T : PumkinPreset
        {
            if(typeof(T) == typeof(PumkinPreset))
            {
                SetupAllGUIDs();    // bad redirect to setup all
                return;
            }

            string typeName = typeof(T).Name;
            var guids = AssetDatabase.FindAssets(typeName);
            string guid = null;
            string path = null;
            foreach(var g in guids)
            {
                path = AssetDatabase.GUIDToAssetPath(g);
                if(!path.EndsWith($"{typeName}.cs"))
                    continue;
                guid = g;
                break;
            }

            if(typeof(T) == typeof(PumkinsBlendshapePreset))
                blendshapePresetScriptGUID = guid;
            else if(typeof(T) == typeof(PumkinsCameraPreset))
                cameraPresetScriptGUID = guid;
            else if(typeof(T) == typeof(PumkinsPosePreset))
                posePresetScriptGUID = guid;
            else
                return;

            PumkinsAvatarTools.LogVerbose($"Set GUID of {typeof(T).Name} script at {path} to {guid}");
        }
        static void SetupAllGUIDs()
        {
            SetupGUID<PumkinsCameraPreset>();
            SetupGUID<PumkinsPosePreset>();
            SetupGUID<PumkinsBlendshapePreset>();
        }
        public static void FixScriptReferences<T>() where T : PumkinPreset
        {
            SetupGUID<T>();

            Type t = typeof(T);
            Type pT = typeof(PumkinPreset);

            var pathsGuids = new Dictionary<string, string>();
            if(t == typeof(PumkinsBlendshapePreset) || t == pT)
                pathsGuids.Add(Helpers.LocalAssetsPathToAbsolutePath(localBlendshapesPath), blendshapePresetScriptGUID);
            if(t == typeof(PumkinsCameraPreset) || t == pT)
                pathsGuids.Add(Helpers.LocalAssetsPathToAbsolutePath(localCamerasPath), cameraPresetScriptGUID);
            if(t == typeof(PumkinsPosePreset) || t == pT)
                pathsGuids.Add(Helpers.LocalAssetsPathToAbsolutePath(localPosesPath), posePresetScriptGUID);

            foreach(var kv in pathsGuids)
            {
                var path = kv.Key;
                var guid = kv.Value;
                if(!Directory.Exists(path))
                {
                    PumkinsAvatarTools.LogVerbose($"Directory {path} doesn't exist. Can't fix references");
                    continue;
                }
                var files = Directory.GetFiles($"{path}", "*.asset", SearchOption.AllDirectories);
                foreach(var file in files)
                {
                    if(ReplacePresetGUIDTemp(file, guid))
                        AssetDatabase.ImportAsset(Helpers.AbsolutePathToLocalAssetsPath(file), ImportAssetOptions.ForceUpdate);
                }
                PumkinsAvatarTools.LogVerbose($"Fixed references for type {typeof(T).Name}");
            }
        }

        public static void FixAllPresetScriptReferences()
        {
            SetupAllGUIDs();

            FixScriptReferences<PumkinsCameraPreset>();
            FixScriptReferences<PumkinsPosePreset>();
            FixScriptReferences<PumkinsBlendshapePreset>();
        }

        /// <summary>
        /// TODO: Replace with one that reads only the needed lines
        /// </summary>
        static bool ReplacePresetGUIDTemp(string filePath, string newGUID)
        {
            filePath = Helpers.AbsolutePathToLocalAssetsPath(filePath);
            if(Helpers.StringIsNullOrWhiteSpace(filePath) || Helpers.StringIsNullOrWhiteSpace(newGUID))
            {
                PumkinsAvatarTools.LogVerbose($"Filepath ({filePath}) or GUID ({newGUID}) is empty", LogType.Warning);
                return false;
            }
            else if(!File.Exists(filePath))
            {
                PumkinsAvatarTools.Log($"File {filePath} doesn't exist. Can't fix preset references", LogType.Warning);
                return false;
            }
            var lines = File.ReadAllLines(filePath);
            for(int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];

                if(!line.Contains("m_Script"))
                    continue;

                lines[i] = Helpers.ReplaceGUIDInLine(line, newGUID, out bool replaced);
                if(replaced)
                {
                    File.WriteAllLines(filePath, lines);
                    return true;
                }
                break;
            }
            return false;
        }
    }
}
