using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using System;
using System.IO;
using UnityEngine;
using System.Linq;
using Pumkin.AvatarTools.Callbacks;
using Pumkin.AvatarTools.Copiers;
using Pumkin.AvatarTools.Destroyers;
using Pumkin.DependencyChecker;
using Pumkin.PoseEditor;
using UnityEngine.UI;
using Pumkin.DataStructures;
using Pumkin.HelperFunctions;
using Pumkin.Extensions;
using UnityEngine.SceneManagement;
using Pumkin.Presets;
using UnityEngine.Animations;
using UnityEditor.Experimental.SceneManagement;

namespace Pumkin.AvatarTools
{
    public partial class PumkinsAvatarTools
    {
        /// <summary>
        /// Not actually resets everything but backgrounnd and overlay stuff
        /// </summary>
        public void ResetBackgroundsAndOverlays()
        {
            Settings._backgroundPath = null;
            Settings._overlayPath = null;
            Settings.bThumbnails_use_camera_background = false;
            Settings.bThumbnails_use_camera_overlay = false;
            cameraBackgroundTexture = null;
            cameraOverlayTexture = null;
            DestroyDummies();
        }

        /// <summary>
        /// Refreshes the background override setting
        /// </summary>
        public void RefreshBackgroundOverrideType()
        {
            if(Settings.bThumbnails_use_camera_background)
            {
                switch(cameraBackgroundType)
                {
                    case PumkinsCameraPreset.CameraBackgroundOverrideType.Color:
                        Color color = SelectedCamera != null ? SelectedCamera.backgroundColor : Settings._thumbsCamBgColor;
                        SetCameraBackgroundToColor(color);
                        break;
                    case PumkinsCameraPreset.CameraBackgroundOverrideType.Image:
                        SetBackgroundToImageFromPath(Settings._backgroundPath);
                        break;
                    case PumkinsCameraPreset.CameraBackgroundOverrideType.Skybox:
                        SetCameraBackgroundToSkybox(RenderSettings.skybox);
                        break;
                }
            }
            else
                RestoreCameraClearFlags();
        }

        /// <summary>
        /// Refreshes ignore array for the copier by making the transform references local to the selected avatar
        /// </summary>
        private void RefreshIgnoreArray()
        {
            if(Settings.copierIgnoreArray == null)
            {
                Settings.copierIgnoreArray = new Transform[0];
                return;
            }
            else if(Settings.copierIgnoreArray.Length == 0)
            {
                return;
            }

            var newList = new List<Transform>(Settings.copierIgnoreArray.Length);

            foreach(var t in Settings.copierIgnoreArray)
            {
                if(!t)
                    newList.Add(t);

                var tt = Helpers.FindTransformInAnotherHierarchy(t, CopierSelectedFrom.transform, false);
                if(tt && !newList.Contains(tt))
                    newList.Add(tt);
            }

            Settings.copierIgnoreArray = newList.ToArray();
        }

        /// <summary>
        /// Refreshes the chosen language in the UI. Needed for when we go into and out of play mode
        /// </summary>
        public void RefreshLanguage()
        {
            PumkinsLanguageManager.LoadTranslations();
            PumkinsLanguageManager.SetLanguage(Settings._selectedLanguageString);
            Settings._selectedLanguageIndex = PumkinsLanguageManager.GetIndexOfLanguage(Settings._selectedLanguageString);
        }

        /// <summary>
        /// Refreshes the cached selected preset index of a PumkinPreset type
        /// </summary>
        public static void RefreshPresetIndex<T>() where T : PumkinPreset
        {
            Type t = typeof(T);
            Type tP = typeof(PumkinPreset);
            if(typeof(T) == typeof(PumkinsCameraPreset) || t == tP)
                RefreshPresetIndexByString<T>(Settings._selectedCameraPresetString);
            if(typeof(T) == typeof(PumkinsPosePreset) || t == tP)
                RefreshPresetIndexByString<T>(Settings._selectedPosePresetString);
            if(typeof(T) == typeof(PumkinsBlendshapePreset) || t == tP)
                RefreshPresetIndexByString<T>(Settings._selectedBlendshapePresetString);
        }

        /// <summary>
        /// Refreshes preset index by string. Used to refresh the index based on the cached string
        /// </summary>
        public static void RefreshPresetIndexByString<T>(string selectedPresetString) where T : PumkinPreset
        {
            int count = PumkinsPresetManager.GetPresetCountOfType<T>();
            int selectedPresetIndex = PumkinsPresetManager.GetPresetIndex<T>(selectedPresetString);
            selectedPresetIndex = Mathf.Clamp(selectedPresetIndex, 0, count - 1);

            if(typeof(T) == typeof(PumkinsCameraPreset))
            {
                if(count == 0)
                    selectedPresetString = "";
                else
                    selectedPresetString = PumkinsPresetManager.CameraPresets[selectedPresetIndex].ToString() ?? "";
                Settings._selectedCameraPresetIndex = selectedPresetIndex;
            }
            else if(typeof(T) == typeof(PumkinsPosePreset))
            {
                if(count == 0)
                    selectedPresetString = "";
                else
                    selectedPresetString = PumkinsPresetManager.PosePresets[selectedPresetIndex].ToString() ?? "";
                Settings._selectedPosePresetIndex = selectedPresetIndex;
            }
            else if(typeof(T) == typeof(PumkinsBlendshapePreset))
            {
                if(count == 0)
                    selectedPresetString = "";
                else
                    selectedPresetString = PumkinsPresetManager.BlendshapePresets[selectedPresetIndex].ToString() ?? "";
                Settings._selectedBlendshapePresetIndex = selectedPresetIndex;
            }
        }

        /// <summary>
        /// Refreshes the cached selected preset string by index
        /// </summary>
        public static void RefreshPresetStringByIndex<T>(int index) where T : PumkinPreset
        {
            string presetString = PumkinsPresetManager.GetPresetName<T>(index);
            if(string.IsNullOrEmpty(presetString))
            {
                index = 0;
                presetString = PumkinsPresetManager.GetPresetName<T>(0);
            }

            if(typeof(T) == typeof(PumkinsCameraPreset))
                Settings._selectedCameraPresetString = presetString;
            else if(typeof(T) == typeof(PumkinsPosePreset))
                Settings._selectedPosePresetString = presetString;
            else if(typeof(T) == typeof(PumkinsBlendshapePreset))
                Settings._selectedBlendshapePresetString = presetString;
        }


        /// <summary>
        /// Sets selected camera clear flags back to _thumbsCameraBgClearFlagsOld
        /// </summary>
        public void RestoreCameraClearFlags()
        {
            if(SelectedCamera)
                SelectedCamera.clearFlags = Settings._thumbsCameraBgClearFlagsOld;
        }

        /// <summary>
        /// Used to set up CameraBackground and CameraOverlay dummies
        /// </summary>
        /// <param name="clipPlaneIsNear">Whether to set the clipping plane to be near or far</param>
        public void SetupCameraRawImageAndCanvas(GameObject dummyGameObject, ref RawImage rawImage, bool clipPlaneIsNear)
        {
            if(!dummyGameObject)
                return;

            rawImage = dummyGameObject.GetComponent<RawImage>();
            if(!rawImage)
                rawImage = dummyGameObject.AddComponent<RawImage>();
            Canvas canvas = dummyGameObject.GetComponent<Canvas>();
            if(!canvas)
                canvas = dummyGameObject.AddComponent<Canvas>();

            canvas.worldCamera = SelectedCamera;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;

            if(!SelectedCamera)
                return;

            if(clipPlaneIsNear)
                canvas.planeDistance = SelectedCamera.nearClipPlane + 0.01f;
            else
                canvas.planeDistance = SelectedCamera.farClipPlane - 2f;
        }

        /// <summary>
        /// Resets all BlendShape weights to 0 on all SkinnedMeshRenderers or to prefab values
        /// </summary>
        /// <param name="revertToPrefab">Revert weights to prefab instead of 0</param>
        public static void ResetBlendshapes(GameObject objTo, bool revertToPrefab)
        {
            var renders = objTo.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            foreach(var r in renders)
            {
                ResetRendererBlendshapes(r, revertToPrefab);
            }
        }

        /// <summary>
        /// Reset all BlendShape weights to 0 on SkinnedMeshRenderer or to prefab values
        /// </summary>
        /// <param name="revertToPrefab">Revert weights to prefab instead of 0</param>
        public static void ResetRendererBlendshapes(SkinnedMeshRenderer render, bool revertToPrefab)
        {
            GameObject pref = null;
            SkinnedMeshRenderer prefRender = null;

            if(!revertToPrefab)
            {
                for(int i = 0; i < render.sharedMesh.blendShapeCount; i++)
                    render.SetBlendShapeWeight(i, 0);
            }
            else
            {
                pref = PrefabUtility.GetCorrespondingObjectFromSource(render.gameObject) as GameObject;

                if(pref != null)
                    prefRender = pref.GetComponent<SkinnedMeshRenderer>();
                else
                {
                    Log(Strings.Log.meshPrefabMissingCantRevertBlednshapes, LogType.Error);
                    return;
                }

                if(render.sharedMesh.blendShapeCount == prefRender.sharedMesh.blendShapeCount)
                {
                    for(int i = 0; i < render.sharedMesh.blendShapeCount; i++)
                    {
                        render.SetBlendShapeWeight(i, prefRender.GetBlendShapeWeight(i));
                    }
                }
                else
                {
                    Dictionary<string, float> prefWeights = new Dictionary<string, float>();
                    for(int i = 0; i < prefRender.sharedMesh.blendShapeCount; i++)
                    {
                        string n = render.sharedMesh.GetBlendShapeName(i);
                        float w = render.GetBlendShapeWeight(i);
                        prefWeights.Add(n, w);
                    }

                    for(int i = 0; i < render.sharedMesh.blendShapeCount; i++)
                    {
                        string n = render.sharedMesh.GetBlendShapeName(i);
                        float w = 0;

                        if(prefWeights.ContainsKey(n))
                            w = prefWeights[n];

                        render.SetBlendShapeWeight(i, w);
                    }
                }
            }
        }

        /// <summary>
        /// Resets avatar pose to prefab values
        /// </summary>
        public static bool ResetPose(GameObject avatar)
        {
            if(!avatar)
                return false;

            var pref = PrefabUtility.GetCorrespondingObjectFromSource(avatar.transform.root.gameObject) as GameObject;

            if(!pref)
            {
                Log(Strings.Log.meshPrefabMissingCantRevertPose, LogType.Error);
                return false;
            }

            //This technically currently Resets all Objects and not just pose.
            //TODO: Use Humanoid Bones for Reset if only pose. Use This method if Full Reset.
            var trans = avatar.GetComponentsInChildren<Transform>(true);

            foreach(var t in trans)
            {
                if(t == t.root)
                    continue;

                string tPath = Helpers.GetTransformPath(t, avatar.transform);
                Transform tPref = pref.transform.Find(tPath);

                if(!tPref)
                    continue;

                if(Settings._tools_avatar_resetPose_position)
                    t.localPosition = tPref.localPosition;
                if(Settings._tools_avatar_resetPose_rotation)
                {
                    t.localRotation = tPref.localRotation;
                    t.localEulerAngles = tPref.localEulerAngles;
                }

                if(Settings._tools_avatar_resetPose_scale)
                {
                    t.localScale = tPref.localScale;
                }
            }

            PumkinsPoseEditor.OnPoseWasChanged(PumkinsPoseEditor.PoseChangeType.Reset);
            return true;
        }

        /// <summary>
        /// Resets target pose to avatar definition
        /// </summary>
        /// <param name="avatar">Target avatar to reset</param>
        /// <param name="fullReset">Should reset non-humanoid objects included in the definition?</param>
        /// <param name="position">Reset the transform position of objects</param>
        /// <param name="rotation">Reset the transform rotation of objects</param>
        /// <param name="scale">Reset the transform scale of objects</param>
        public static bool ResetToAvatarDefinition(GameObject avatar, bool fullReset = false, bool position = true, bool rotation = true, bool scale = true)
        {
            if(!avatar) return false;
            Animator ani = avatar.GetComponent<Animator>();
            if(!ani || !ani.avatar || !ani.avatar.isHuman)
            {
                Log(Strings.Log.cantSetPoseNonHumanoid, LogType.Warning, "Avatar Definition");
                return false;
            }

            // All IDs if full reset. Only Human IDs if not.
            // ID > Path
            // ID > Element > Transform Data
            Undo.RegisterFullObjectHierarchyUndo(avatar, "Reset To Avatar Definition");
            SerializedObject sAvi = new SerializedObject(ani.avatar);
            SerializedProperty humanIds = sAvi.FindProperty("m_Avatar.m_Human.data.m_Skeleton.data.m_ID");
            SerializedProperty allIds = sAvi.FindProperty("m_Avatar.m_AvatarSkeleton.data.m_ID");
            SerializedProperty defaultPose = sAvi.FindProperty("m_Avatar.m_DefaultPose.data.m_X");
            SerializedProperty tos = sAvi.FindProperty("m_TOS");

            Dictionary<long, int> idToElem = new Dictionary<long, int>();
            Dictionary<int, TransformData> elemToTransform = new Dictionary<int, TransformData>();
            Dictionary<long, string> IdToPath = new Dictionary<long, string>();

            for (int i = 0; i < allIds.arraySize; i++)
                idToElem.Add(allIds.GetArrayElementAtIndex(i).longValue, i);

            for (int i = 0; i < defaultPose.arraySize; i++)
                elemToTransform.Add(i, new TransformData(defaultPose.GetArrayElementAtIndex(i)));

            for (int i = 0; i < tos.arraySize; i++)
            {
                SerializedProperty currProp = tos.GetArrayElementAtIndex(i);
                IdToPath.Add(currProp.FindPropertyRelative("first").longValue, currProp.FindPropertyRelative("second").stringValue);
            }

            System.Action<Transform, TransformData> applyTransform = (transform, data) => {
                if(transform)
                {
                    if(position)
                        transform.localPosition = data.pos;
                    if(rotation)
                        transform.localRotation = data.rot;
                    if(scale)
                        transform.localScale = data.scale;
                }
            };

            if(!fullReset)
            {
                for (int i = 0; i < humanIds.arraySize; i++)
                {
                    Transform myBone = ani.transform.Find(IdToPath[humanIds.GetArrayElementAtIndex(i).longValue]);
                    TransformData data = elemToTransform[idToElem[humanIds.GetArrayElementAtIndex(i).longValue]];
                    applyTransform(myBone, data);
                }
            }
            else
            {
                for (int i = 0; i < allIds.arraySize; i++)
                {
                    Transform myBone = ani.transform.Find(IdToPath[allIds.GetArrayElementAtIndex(i).longValue]);
                    TransformData data = elemToTransform[idToElem[allIds.GetArrayElementAtIndex(i).longValue]];
                    applyTransform(myBone, data);
                }
            }

            return true;
        }
        private struct TransformData
        {
            public Vector3 pos;
            public Quaternion rot;
            public Vector3 scale;
            public TransformData(SerializedProperty t)
            {
                SerializedProperty tProp = t.FindPropertyRelative("t");
                SerializedProperty qProp = t.FindPropertyRelative("q");
                SerializedProperty sProp = t.FindPropertyRelative("s");
                pos = new Vector3(tProp.FindPropertyRelative("x").floatValue, tProp.FindPropertyRelative("y").floatValue, tProp.FindPropertyRelative("z").floatValue);
                rot = new Quaternion(qProp.FindPropertyRelative("x").floatValue, qProp.FindPropertyRelative("y").floatValue, qProp.FindPropertyRelative("z").floatValue, qProp.FindPropertyRelative("w").floatValue);
                scale = new Vector3(sProp.FindPropertyRelative("x").floatValue, sProp.FindPropertyRelative("y").floatValue, sProp.FindPropertyRelative("z").floatValue);
            }
        }

        /// <summary>
        /// Looks for child object in an object's children. Can create if not found.
        /// </summary>
        /// <param name="parent">Parent object to look in</param>
        /// <param name="child">Child object to look for in parent?</param>
        /// <param name="createIfMissing">Create GameObject if not found</param>
        GameObject GetSameChild(GameObject parent, GameObject child, bool createIfMissing = false)
        {
            if(parent == null || child == null)
                return null;

            Transform newChild = null;
            if(createIfMissing)
                newChild = parent.transform.Find(child.name, true, parent.transform);
            else
                newChild = parent.transform.Find(child.name);

            if(newChild != null)
                return newChild.gameObject;

            return null;
        }
#if VRC_SDK_VRCSDK3
        /// <summary>
        /// Centers camera on viewpoint and fixes the near and far clipping planes
        /// </summary>
        /// <param name="avatarOverride">Avatar to center on</param>
        /// <param name="positionOffset">Position offset to apply</param>
        /// <param name="rotationOffset">Rotation offset to apply</param>
        /// <param name="fixClippingPlanes">Should change near clipping plane to 0.1 and far to 1000?</param>
        void CenterCameraOnViewpoint(GameObject avatarOverride, Vector3 positionOffset, Vector3 rotationOffset, bool fixClippingPlanes)
        {
            if(fixClippingPlanes)
                Helpers.FixCameraClippingPlanes(SelectedCamera);
            PumkinsCameraPreset.ApplyPositionAndRotationWithViewpointFocus(avatarOverride, SelectedCamera, positionOffset, rotationOffset, true);
        }
#endif

        void CenterCameraOnTransform(Transform transform, Vector3 positionOffset, Vector3 rotationOffset, bool fixClippingPlanes)
        {
            if(fixClippingPlanes)
                Helpers.FixCameraClippingPlanes(SelectedCamera);
            PumkinsCameraPreset.ApplyPositionAndRotationWithTransformFocus(transform, SelectedCamera, positionOffset, rotationOffset, true);
        }

        void FillEyeBones(GameObject avatar)
        {
            Type descType = PumkinsTypeCache.VRC_AvatarDescriptor;

            if(!avatar || descType == null)
                return;

            var anim = avatar.GetComponent<Animator>();

            if(!anim)
                return;
            if(!anim.isHuman)
            {
                Log("FillEyeBones only works for humanoid avatars", LogType.Warning);
                return;
            }

            var desc = avatar.GetComponent(descType);
            var sDesc = new SerializedObject(desc);

            var leftEye = sDesc.FindProperty("customEyeLookSettings.leftEye");
            var rightEye = sDesc.FindProperty("customEyeLookSettings.rightEye");

            leftEye.objectReferenceValue = anim.GetBoneTransform(HumanBodyBones.LeftEye);
            rightEye.objectReferenceValue = anim.GetBoneTransform(HumanBodyBones.RightEye);

            sDesc.ApplyModifiedProperties();
        }

    }
}