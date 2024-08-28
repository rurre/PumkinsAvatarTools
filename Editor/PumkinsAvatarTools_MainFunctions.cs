using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using System;
using System.IO;
using UnityEngine;
using System.Linq;
using System.Reflection;
using Pumkin.AvatarTools.Destroyers;
using Pumkin.DependencyChecker;
using Pumkin.PoseEditor;
using UnityEngine.UI;
using Pumkin.DataStructures;
using Pumkin.HelperFunctions;
using UnityEngine.Animations;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Pumkin.AvatarTools
{
    public partial class PumkinsAvatarTools
    {
        /// <summary>
        /// Loads textures back into overlay and background objects if we have a path for them still stored. Useful for when we restart unity
        /// </summary>
        private void RestoreTexturesFromPaths()
        {
            RawImage overlayImg = GetCameraOverlayRawImage(Settings.bThumbnails_use_camera_overlay);
            RawImage backgroundImg = GetCameraBackgroundRawImage(Settings.bThumbnails_use_camera_background);

            if(!string.IsNullOrEmpty(Settings._overlayPath))
            {
                if(overlayImg)
                {
                    if(overlayImg.texture)
                    {
                        cameraOverlayTexture = (Texture2D)overlayImg.texture;
                        overlayImg.color = Settings.cameraOverlayImageTint;
                    }
                    else
                    {
                        SetOverlayToImageFromPath(Settings._overlayPath);
                    }
                }
            }
            else if(overlayImg && overlayImg.texture)
            {
                cameraOverlayTexture = null;
                overlayImg.texture = null;
            }

            if(!string.IsNullOrEmpty(Settings._backgroundPath))
            {
                if(backgroundImg)
                {
                    if(backgroundImg.texture)
                    {
                        cameraBackgroundTexture = (Texture2D)backgroundImg.texture;
                        backgroundImg.color = Settings.cameraBackgroundImageTint;
                    }
                    else
                    {
                        SetBackgroundToImageFromPath(Settings._backgroundPath);
                    }
                }
            }
            else if(backgroundImg && backgroundImg.texture)
            {
                cameraBackgroundTexture = null;
                backgroundImg.texture = null;
            }
        }

        /// <summary>
        /// Sets overlay texture to image from path
        /// </summary>
        /// <param name="texturePath"></param>
        public void SetOverlayToImageFromPath(string texturePath)
        {
            Settings._overlayPath = texturePath;
            if(!GetCameraOverlay() || !GetCameraOverlayRawImage())
                return;

            Texture2D tex = Helpers.GetImageTextureFromPath(texturePath);
            SetOverlayToImageFromTexture(tex);
            if(tex)
            {
                string texName = string.IsNullOrEmpty(texturePath) ? "empty" : Path.GetFileName(texturePath);
                Log(Strings.Log.loadedImageAsOverlay, LogType.Log, texName);
            }
            else
            {
                Log(Strings.Warning.cantLoadImageAtPath, LogType.Warning, texturePath);
            }
        }

        /// <summary>
        /// Sets overlay image to texture
        /// </summary>
        public void SetOverlayToImageFromTexture(Texture2D newTexture)
        {
            var img = GetCameraOverlayRawImage();
            var fg = GetCameraOverlay();
            if(fg && img)
            {
                img.color = Settings.cameraOverlayImageTint;
                img.texture = newTexture;
                if(img.canvas)
                    img.canvas.worldCamera = SelectedCamera;
            }
        }

        /// <summary>
        /// Sets background texture to image from path
        /// </summary>
        /// <param name="texturePath"></param>
        public void SetBackgroundToImageFromPath(string texturePath)
        {
            Settings._backgroundPath = texturePath;
            if(!GetCameraOverlay() || !GetCameraOverlayRawImage())
                return;

            Texture2D tex = Helpers.GetImageTextureFromPath(texturePath);
            SetBackgroundToImageFromTexture(tex);
            if(tex)
            {
                string texName = string.IsNullOrEmpty(texturePath) ? "empty" : Path.GetFileName(texturePath);
                Log(Strings.Log.loadedImageAsBackground, LogType.Log, texName);
            }
            else if(!string.IsNullOrEmpty(texturePath))
            {
                Log(Strings.Warning.cantLoadImageAtPath, LogType.Warning, texturePath);
            }
        }
        /// <summary>
        /// Sets background to image from texture
        /// </summary>
        /// <param name="newTexture"></param>
        public void SetBackgroundToImageFromTexture(Texture2D newTexture)
        {
            var img = GetCameraBackgroundRawImage();
            var bg = GetCameraBackground();
            if(bg && img)
            {
                img.color = Settings.cameraBackgroundImageTint;
                img.texture = newTexture;
                if(img.canvas)
                    img.canvas.worldCamera = SelectedCamera;
            }
        }

        /// <summary>
        /// Sets camera background clear flags to skybox and changes skybox to material
        /// </summary>
        public void SetCameraBackgroundToSkybox(Material skyboxMaterial)
        {
            if(!_selectedCamera)
                return;

            SelectedCamera.clearFlags = CameraClearFlags.Skybox;
            RenderSettings.skybox = skyboxMaterial;
        }

        /// <summary>
        /// Changes camera clear flags to solid color and sets background color
        /// </summary>
        public void SetCameraBackgroundToColor(Color color)
        {
            if(!_selectedCamera)
                return;

            Settings._thumbsCamBgColor = color;
            SelectedCamera.backgroundColor = color;
            SelectedCamera.clearFlags = CameraClearFlags.SolidColor;
        }


#if VRC_SDK_VRCSDK3
        /// <summary>
        /// Quickly sets viewpoint to eye height if avatar is humanoid
        /// </summary>
        /// <param name="zDepth">Z Depth value of viewpoint</param>
        public void QuickSetViewpoint(GameObject avatar, float zDepth)
        {
            Component desc = avatar.GetComponent(PumkinsTypeCache.VRC_AvatarDescriptor) ?? avatar.AddComponent(PumkinsTypeCache.VRC_AvatarDescriptor);
            dynamic descriptor = Convert.ChangeType(desc, PumkinsTypeCache.VRC_AvatarDescriptor);
            var anim = SelectedAvatar.GetComponent<Animator>();

            descriptor.ViewPosition = Helpers.GetViewpointAtEyeLevel(SelectedAvatar.GetComponent<Animator>());
            descriptor.ViewPosition.z = zDepth;

            if(anim.isHuman)
                Log(Strings.Log.settingQuickViewpoint, LogType.Log, descriptor.ViewPosition.ToString());
            else
                Log(Strings.Log.cantSetViewpointNonHumanoid, LogType.Warning, descriptor.ViewPosition.ToString());
        }
#endif

        /// <summary>
        /// Tries to get the VRCCam, returns Camera.main if not found
        /// </summary>
        private static Camera GetVRCCamOrMainCam()
        {
            var obj = GameObject.Find("VRCCam");
            if(!obj)
            {
                Camera cam = Camera.main;
                if(cam)
                    obj = cam.gameObject;
            }
            if(obj)
                return obj.GetComponent<Camera>();
            return null;
        }

        /// <summary>
        /// Tries to select the avatar from anywhere in it's hierarchy. Recursively looks for an avatar descriptor.
        /// </summary>
        public static bool GetAvatarFromSceneSelection(bool updateAvatarInfo, out GameObject avatar)
        {
            avatar = null;
            GameObject selection;
            try
            {
                selection = Selection.activeGameObject;
                if(!selection)
                    return false;


#if VRC_SDK_VRCSDK3
                Transform parent = selection.transform.parent;
                while(parent != null)
                {
                    if(parent.GetComponent(PumkinsTypeCache.VRC_AvatarDescriptor))
                    {
                        selection = parent.gameObject;
                        break;
                    }

                    parent = parent.parent;
                }
#else
                selection = Selection.activeGameObject.transform.root.gameObject;
#endif

                if(selection != null)
                {
                    if(selection.gameObject.scene.name != null)
                    {
                        if(updateAvatarInfo)
                            avatarInfo = PumkinsAvatarInfo.GetInfo(SelectedAvatar, out _avatarInfoString);
                        avatar = selection;
                        return true;
                    }

                    if(!SettingsContainer._useSceneSelectionAvatar)
                    {
                        Log(Strings.Warning.selectSceneObject, LogType.Warning);
                    }
                }
            }
            catch(Exception e)
            {
                Log(e.Message, LogType.Warning);
            }
            _PumkinsAvatarToolsWindow.RepaintSelf();
            return false;
        }

#if VRC_SDK_VRCSDK3
        /// <summary>
        /// Sets the avatar scale and moves the viewpoint to compensate
        /// </summary>
        private void SetAvatarScaleAndMoveViewpoint(Component desc, float newScale)
        {
            if(_editingScale)
            {
                SelectedAvatar.transform.localScale = Helpers.RoundVectorValues(new Vector3(newScale, newScale, newScale), 3);
                if(_scaleViewpointDummy)
                    _viewPosTemp = _scaleViewpointDummy.position;
                else
                    EndScalingAvatar(desc.gameObject, true);
            }
            else
            {
                dynamic descriptor = Convert.ChangeType(desc, PumkinsTypeCache.VRC_AvatarDescriptor);
                var tempDummy = new GameObject(DUMMY_NAME).transform;
                tempDummy.position = descriptor.ViewPosition + desc.transform.root.position;
                tempDummy.parent = SelectedAvatar.transform;
                desc.transform.root.localScale = Helpers.RoundVectorValues(new Vector3(newScale, newScale, newScale), 3);
                SetViewpoint(desc, tempDummy.position);
                Object.DestroyImmediate(tempDummy.gameObject);
                Log(Strings.Log.setAvatarScaleTo, LogType.Log, newScale.ToString(), descriptor.ViewPosition.ToString());
            }
        }
#endif

        private void SetAvatarScale(float newScale)
        {
            if(_editingScale)
            {
                SelectedAvatar.transform.localScale = Helpers.RoundVectorValues(new Vector3(newScale, newScale, newScale), 3);
#if VRC_SDK_VRCSDK3
                if(_scaleViewpointDummy)
                    _viewPosTemp = _scaleViewpointDummy.position;
                else
                    EndScalingAvatar(SelectedAvatar, true);
#endif
            }
            else
            {
                SelectedAvatar.transform.root.localScale = Helpers.RoundVectorValues(new Vector3(newScale, newScale, newScale), 3);
                Log(Strings.Log.setAvatarScaleTo, LogType.Log, newScale.ToString());
            }
        }

        /// <summary>
        /// Function for all the actions in the tool menu. Use this instead of calling
        /// button functions directly.
        /// </summary>
        void DoAction(GameObject avatar, ToolMenuActions action)
        {
            if(!SelectedAvatar) //Shouldn't be possible with disable group
            {
                Log(Strings.Log.nothingSelected, LogType.Warning);
                return;
            }

            //Record Undo
            Undo.RegisterFullObjectHierarchyUndo(SelectedAvatar, "Tools menu: " + action.ToString());
            if(SelectedAvatar.gameObject.scene.name == null) //In case it's a prefab instance, which it probably is
                PrefabUtility.RecordPrefabInstancePropertyModifications(SelectedAvatar);

            switch(action)
            {
                case ToolMenuActions.RemoveColliders:
                    LegacyDestroyer.DestroyAllComponentsOfType(SelectedAvatar, typeof(Collider), false);
                    break;
                case ToolMenuActions.RemovePhysBoneColliders:
#if VRC_SDK_VRCSDK3
                    LegacyDestroyer.DestroyAllComponentsOfType(SelectedAvatar, PumkinsTypeCache.PhysBoneCollider, false);
                    CleanupPhysBonesColliderArraySizes();
#endif
                    break;
                case ToolMenuActions.RemovePhysBones:
#if VRC_SDK_VRCSDK3

                    LegacyDestroyer.DestroyAllComponentsOfType(SelectedAvatar, PumkinsTypeCache.PhysBone, false);
#endif
                    break;
                case ToolMenuActions.RemoveDynamicBoneColliders:
                    LegacyDestroyer.DestroyAllComponentsOfType(SelectedAvatar, PumkinsTypeCache.DynamicBoneColliderBase, false);
                    CleanupDynamicBonesColliderArraySizes();
                    break;
                case ToolMenuActions.RemoveDynamicBones:
                    LegacyDestroyer.DestroyAllComponentsOfType(SelectedAvatar, PumkinsTypeCache.DynamicBone, false);
                    break;
                case ToolMenuActions.RemoveContactReceiver:
#if VRC_SDK_VRCSDK3
                    LegacyDestroyer.DestroyAllComponentsOfType(SelectedAvatar, PumkinsTypeCache.ContactReceiver, false);
#endif
                    break;
                case ToolMenuActions.RemoveContactSender:
#if VRC_SDK_VRCSDK3
                    LegacyDestroyer.DestroyAllComponentsOfType(SelectedAvatar, PumkinsTypeCache.ContactSender, false);
#endif
                    break;
                case ToolMenuActions.RemoveVRCStation:
#if VRC_SDK_VRCSDK3
                    LegacyDestroyer.DestroyAllComponentsOfType(SelectedAvatar, PumkinsTypeCache.VRCStation, false);
#endif
                    break;
                case ToolMenuActions.ResetPose:
                    switch (Settings._tools_avatar_resetPose_type)
                    {
                        case SettingsContainer.ResetPoseType.Prefab:
                            ResetPose(SelectedAvatar);
                            break;
                        case SettingsContainer.ResetPoseType.AvatarDefinition:
                            ResetToAvatarDefinition(SelectedAvatar, Settings._tools_avatar_resetPose_fullreset, Settings._tools_avatar_resetPose_position, Settings._tools_avatar_resetPose_rotation, Settings._tools_avatar_resetPose_scale);
                            break;
                        case SettingsContainer.ResetPoseType.TPose:
                            PumkinsPoseEditor.SetTPoseHardcoded(SelectedAvatar);
                            break;
                    }
                    break;
                case ToolMenuActions.RevertBlendshapes:
                    if(EditorApplication.isPlaying)
                        ResetBlendshapes(SelectedAvatar, false);
                    else
                        ResetBlendshapes(SelectedAvatar, true);
                    break;
#if VRC_SDK_VRCSDK3
                case ToolMenuActions.FillVisemes:
                    FillVisemes(SelectedAvatar);
                    break;
                case ToolMenuActions.EditViewpoint:
                    BeginEdittingViewpoint(SelectedAvatar);
                    break;
#endif
                case ToolMenuActions.ZeroBlendshapes:
                    ResetBlendshapes(SelectedAvatar, false);
                    break;
                case ToolMenuActions.SetTPose:
                    PumkinsPoseEditor.SetTPoseHardcoded(SelectedAvatar);
                    break;
                case ToolMenuActions.RemoveEmptyGameObjects:
                    LegacyDestroyer.DestroyEmptyGameObjects(SelectedAvatar);
                    break;
                case ToolMenuActions.RemoveParticleSystems:
                    LegacyDestroyer.DestroyAllComponentsOfType(SelectedAvatar, typeof(ParticleSystemRenderer), false);
                    LegacyDestroyer.DestroyAllComponentsOfType(SelectedAvatar, typeof(ParticleSystem), false);
                    break;
                case ToolMenuActions.RemoveRigidBodies:
                    LegacyDestroyer.DestroyAllComponentsOfType(SelectedAvatar, typeof(Rigidbody), false);
                    break;
                case ToolMenuActions.RemoveTrailRenderers:
                    LegacyDestroyer.DestroyAllComponentsOfType(SelectedAvatar, typeof(TrailRenderer), false);
                    break;
                case ToolMenuActions.RemoveMeshRenderers:
                    LegacyDestroyer.DestroyAllComponentsOfType(SelectedAvatar, typeof(MeshFilter), false);
                    LegacyDestroyer.DestroyAllComponentsOfType(SelectedAvatar, typeof(MeshRenderer), false);
                    break;
                case ToolMenuActions.RemoveLights:
                    LegacyDestroyer.DestroyAllComponentsOfType(SelectedAvatar, typeof(Light), false);
                    break;
                case ToolMenuActions.RemoveAnimatorsInChildren:
                    LegacyDestroyer.DestroyAllComponentsOfType(SelectedAvatar, typeof(Animator), true);
                    break;
                case ToolMenuActions.RemoveAudioSources:
#if VRC_SDK_VRCSDK3
                    LegacyDestroyer.DestroyAllComponentsOfType(SelectedAvatar, PumkinsTypeCache.VRC_SpatialAudioSource, false);
#endif
                    LegacyDestroyer.DestroyAllComponentsOfType(SelectedAvatar, typeof(AudioSource), false);
                    break;
                case ToolMenuActions.RemoveJoints:
                    LegacyDestroyer.DestroyAllComponentsOfType(SelectedAvatar, typeof(Joint), false);
                    break;
                case ToolMenuActions.EditScale:
                    BeginScalingAvatar(SelectedAvatar);
                    break;
                case ToolMenuActions.RevertScale:
                    RevertScale(SelectedAvatar);
                    break;
                case ToolMenuActions.RemoveMissingScripts:
                    LegacyDestroyer.DestroyMissingScripts(SelectedAvatar);
                    break;
                case ToolMenuActions.RemoveAimConstraint:
                    LegacyDestroyer.DestroyAllComponentsOfType(SelectedAvatar, typeof(AimConstraint), false);
                    break;
                case ToolMenuActions.RemoveLookAtConstraint:
                    LegacyDestroyer.DestroyAllComponentsOfType(SelectedAvatar, typeof(LookAtConstraint), false);
                    break;
                case ToolMenuActions.RemoveParentConstraint:
                    LegacyDestroyer.DestroyAllComponentsOfType(SelectedAvatar, typeof(ParentConstraint), false);
                    break;
                case ToolMenuActions.RemovePositionConstraint:
                    LegacyDestroyer.DestroyAllComponentsOfType(SelectedAvatar, typeof(PositionConstraint), false);
                    break;
                case ToolMenuActions.RemoveRotationConstraint:
                    LegacyDestroyer.DestroyAllComponentsOfType(SelectedAvatar, typeof(RotationConstraint), false);
                    break;
                case ToolMenuActions.RemoveScaleConstraint:
                    LegacyDestroyer.DestroyAllComponentsOfType(SelectedAvatar, typeof(ScaleConstraint), false);
                    break;

                case ToolMenuActions.RemoveVRCAimConstraint:
                    LegacyDestroyer.DestroyAllComponentsOfType(SelectedAvatar, PumkinsTypeCache.VRCAimConstraint, false);
                    break;
                case ToolMenuActions.RemoveVRCLookAtConstraint:
                    LegacyDestroyer.DestroyAllComponentsOfType(SelectedAvatar, PumkinsTypeCache.VRCLookAtConstraint, false);
                    break;
                case ToolMenuActions.RemoveVRCParentConstraint:
                    LegacyDestroyer.DestroyAllComponentsOfType(SelectedAvatar, PumkinsTypeCache.VRCParentConstraint, false);
                    break;
                case ToolMenuActions.RemoveVRCPositionConstraint:
                    LegacyDestroyer.DestroyAllComponentsOfType(SelectedAvatar, PumkinsTypeCache.VRCPositionConstraint, false);
                    break;
                case ToolMenuActions.RemoveVRCRotationConstraint:
                    LegacyDestroyer.DestroyAllComponentsOfType(SelectedAvatar, PumkinsTypeCache.VRCRotationConstraint, false);
                    break;
                case ToolMenuActions.RemoveVRCScaleConstraint:
                    LegacyDestroyer.DestroyAllComponentsOfType(SelectedAvatar, PumkinsTypeCache.VRCScaleConstraint, false);
                    break;
                case ToolMenuActions.RemoveVRCHeadChop:
                    LegacyDestroyer.DestroyAllComponentsOfType(SelectedAvatar, PumkinsTypeCache.VRCHeadChop, false);
                    break;
                case ToolMenuActions.RemoveCameras:
                    LegacyDestroyer.DestroyAllComponentsOfType(SelectedAvatar, typeof(Camera), false);
                    break;
                case ToolMenuActions.ResetBoundingBoxes:
                    Helpers.ResetBoundingBoxes(SelectedAvatar);
                    break;
#if VRC_SDK_VRCSDK3
                case ToolMenuActions.FillEyeBones:
                    FillEyeBones(SelectedAvatar);
                    break;
#endif
                case ToolMenuActions.RemoveFinalIK_CCDIK:
                    LegacyDestroyer.DestroyAllComponentsOfType(SelectedAvatar, PumkinsTypeCache.CCDIK, false);
                    break;
                case ToolMenuActions.RemoveFinalIK_LimbIK:
                    LegacyDestroyer.DestroyAllComponentsOfType(SelectedAvatar, PumkinsTypeCache.LimbIK, false);
                    break;
                case ToolMenuActions.RemoveFinalIK_RotationLimits:
                    LegacyDestroyer.DestroyAllComponentsOfType(SelectedAvatar, PumkinsTypeCache.RotationLimit, false);
                    break;
                case ToolMenuActions.RemoveFinalIK_FabrIK:
                    LegacyDestroyer.DestroyAllComponentsOfType(SelectedAvatar, PumkinsTypeCache.FABRIK, false);
                    break;
                case ToolMenuActions.RemoveFinalIK_AimIK:
                    LegacyDestroyer.DestroyAllComponentsOfType(SelectedAvatar, PumkinsTypeCache.AimIK, false);
                    break;
                case ToolMenuActions.RemoveFinalIK_FbtBipedIK:
                    LegacyDestroyer.DestroyAllComponentsOfType(SelectedAvatar, PumkinsTypeCache.FullBodyBipedIK, false);
                    break;
                case ToolMenuActions.RemoveFinalIK_VRIK:
                    LegacyDestroyer.DestroyAllComponentsOfType(SelectedAvatar, PumkinsTypeCache.VRIK, false);
                    break;
                case ToolMenuActions.RemoveFinalIK_Grounder:
                    LegacyDestroyer.DestroyAllComponentsOfType(SelectedAvatar, PumkinsTypeCache.Grounder, false);
                    break;
                default:
                    break;
            }

            avatarInfo = PumkinsAvatarInfo.GetInfo(SelectedAvatar, out _avatarInfoString);

            EditorUtility.SetDirty(SelectedAvatar);
            if(!EditorApplication.isPlaying)
                EditorSceneManager.MarkSceneDirty(SelectedAvatar.scene);
        }

        private static void CleanupPhysBonesColliderArraySizes()
        {
#if VRC_SDK_VRCSDK3
            var pbones = SelectedAvatar.GetComponentsInChildren(PumkinsTypeCache.PhysBone, true);
            if(pbones != null && pbones.Length > 0)
            {
                SerializedObject so = new SerializedObject(pbones);
                if(so != null)
                {
                    var prop = so.FindProperty("m_Colliders");
                    if(prop != null)
                    {
                        prop.arraySize = 0;
                        so.ApplyModifiedProperties();   //Sets count of colliders in array to 0 so the safety system ignores them
                    }
                }
            }
#endif
        }

        private static void CleanupDynamicBonesColliderArraySizes()
        {
            Component[] dbones = SelectedAvatar.GetComponentsInChildren(PumkinsTypeCache.DynamicBone, true);
            if(dbones != null && dbones.Length > 0)
            {
                SerializedObject so = new SerializedObject(dbones);
                var prop = so.FindProperty("m_Colliders");
                if(prop != null)
                {
                    prop.arraySize = 0;
                    so.ApplyModifiedProperties();   //Sets count of colliders in array to 0 so the safety system ignores them
                }
            }
        }

#if VRC_SDK_VRCSDK3
        /// <summary>
        /// Sets the enabled state on all phys bones on the avatar and returns affected bones
        /// </summary>
        /// <param name="enabled">Enabled state for phys bones</param>
        /// <param name="pBonesToIgnore">Phys Bones to ignore</param>
        /// <returns>Phys Bones that were disabled before we did anything</returns>
        static void SetPhysBonesEnabledState(GameObject avatar, bool enabled, List<Component> pBonesToIgnore = null)
        {
            if(!avatar || !_DependencyChecker.PhysBonesExist)
                return;

            foreach(var bone in avatar.GetComponentsInChildren(PumkinsTypeCache.PhysBone, true))
                if(pBonesToIgnore == null || !pBonesToIgnore.Contains(bone))
                {
                    dynamic pb = Convert.ChangeType(bone, PumkinsTypeCache.PhysBone);
                    pb.enabled = enabled;
                }
        }
#else
        static void SetPhysBonesEnabledState(GameObject avatar, bool enabled)
        {
            return;
        }
#endif
        /// <summary>
        /// Toggles the enbaled state of all phys Bones on the avatar and returns affected bones
        /// </summary>
        /// <param name="enabledState">Bool to use as toggle state</param>
        /// <param name="pBonesToIgnore">Phys Bones to ignore</param>
        /// <returns>Phys Bones that have been enabled or disabled. Used to ignore bones that were disabled before we toggled off</returns>
#if VRC_SDK_VRCSDK3
        static void TogglePhysBonesEnabledState(GameObject avatar, ref bool enabledState, ref List<Component> pBonesToIgnore)
        {
            if(!_DependencyChecker.PhysBonesExist)
                return;

            if(!enabledState)
            {
                pBonesToIgnore = new List<Component>();
                var bones = avatar.GetComponentsInChildren(PumkinsTypeCache.PhysBone, true);
                foreach(var b in bones)
                {
                    dynamic pb = Convert.ChangeType(b, PumkinsTypeCache.PhysBone);
                    if(!pb.enabled)
                        pBonesToIgnore.Add(pb);
                }
            }
            SetPhysBonesEnabledState(avatar, enabledState, pBonesToIgnore);
            enabledState = !enabledState;

        }
#endif
        /// <summary>
        /// Sets the enabled state on all dynamic bones on the avatar and returns affected bones
        /// </summary>
        /// <param name="enabled">Enabled state for dynamic bones</param>
        /// <param name="dBonesToIgnore">Dynamic Bones to ignore</param>
        /// <returns>Dynamic Bones that were disabled before we did anything</returns>

        static void SetDynamicBonesEnabledState(GameObject avatar, bool enabled, List<Component> dBonesToIgnore = null)
        {
            if(!avatar)
                return;

            foreach(var bone in avatar.GetComponentsInChildren(PumkinsTypeCache.DynamicBone, true))
                if(dBonesToIgnore == null || !dBonesToIgnore.Contains(bone))
                {
                    var boneBehaviour = bone as Behaviour;
                    if(boneBehaviour)
                        boneBehaviour.enabled = enabled;
                }
        }

        /// <summary>
        /// Toggles the enbaled state of all Dynamic Bones on the avatar and returns affected bones
        /// </summary>
        /// <param name="enabledState">Bool to use as toggle state</param>
        /// <param name="dBonesToIgnore">Dynamic Bones to ignore</param>
        /// <returns>Dynamic Bones that have been enabled or disabled. Used to ignore bones that were disabled before we toggled off</returns>

        static void ToggleDynamicBonesEnabledState(GameObject avatar, ref bool enabledState, ref List<Component> dBonesToIgnore)
        {
            if(!enabledState)
            {
                dBonesToIgnore = new List<Component>();
                var bones = avatar.GetComponentsInChildren(PumkinsTypeCache.DynamicBone, true);
                foreach(var b in bones)
                {
                    var boneBehaviour = b as Behaviour;
                    if(boneBehaviour && !boneBehaviour.enabled)
                        dBonesToIgnore.Add(b);
                }
            }
            SetDynamicBonesEnabledState(avatar, enabledState, dBonesToIgnore);
            enabledState = !enabledState;
        }

        /// <summary>
        /// Reverts avatar scale to prefab values and moves the viewpoint to compensate for the change if avatar a descriptor is present
        /// </summary>
        private void RevertScale(GameObject avatar)
        {
            if(!avatar)
                return;

            GameObject pref = PrefabUtility.GetCorrespondingObjectFromSource(avatar);
            Vector3 newScale = pref != null ? pref.transform.localScale : Vector3.one;

#if VRC_SDK_VRCSDK3
            var desc = avatar.GetComponent(PumkinsTypeCache.VRC_AvatarDescriptor);

            if(desc)
                SetAvatarScaleAndMoveViewpoint(desc, newScale.y);
#endif

            avatar.transform.localScale = newScale;
        }

        /// <summary>
        /// Begin scaling Avatar.
        /// Used to uniformily scale an avatar as well as it's viewpoint position
        /// </summary>
        private void BeginScalingAvatar(GameObject avatar)
        {
            if(DrawingHandlesGUI || !avatar)
                return;

#if VRC_SDK_VRCSDK3
            _tempAvatarDescriptor = avatar.GetComponent(PumkinsTypeCache.VRC_AvatarDescriptor);
            if(!_tempAvatarDescriptor)
            {
                _tempAvatarDescriptor = avatar.AddComponent(PumkinsTypeCache.VRC_AvatarDescriptor);
                _tempAvatarDescriptorWasAdded = true;
            }
            else
            {
                _tempAvatarDescriptorWasAdded = false;
            }


            dynamic desc = Convert.ChangeType(_tempAvatarDescriptor, PumkinsTypeCache.VRC_AvatarDescriptor);
            _viewPosOld = desc.ViewPosition;
            _viewPosTemp = _viewPosOld + SelectedAvatar.transform.position;
#endif
            _avatarScaleOld = avatar.transform.localScale;
            Settings._avatarScaleTemp = _avatarScaleOld.z;

#if VRC_SDK_VRCSDK3
            if(!_scaleViewpointDummy)
            {
                var g = GameObject.Find(VIEWPOINT_DUMMY_NAME);
                if(g)
                    _scaleViewpointDummy = g.transform;
                else
                {
                    _scaleViewpointDummy = new GameObject(VIEWPOINT_DUMMY_NAME).transform;
                    _scaleViewpointDummy.gameObject.hideFlags = HideFlags.HideAndDontSave;
                }
            }

            _scaleViewpointDummy.position = _viewPosTemp;
            _scaleViewpointDummy.parent = SelectedAvatar.transform;
#endif

            _editingScale = true;
            _tempToolOld = Tools.current;
            Tools.current = Tool.None;
            Selection.activeGameObject = SelectedAvatar;

            SetupScaleRuler();
        }

        void SetupScaleRuler()
        {
            if(_scaleRuler != null)
                Helpers.DestroyAppropriate(_scaleRuler);

            if(!ScaleRulerPrefab)
                return;

            _scaleRuler = Object.Instantiate(ScaleRulerPrefab, SelectedAvatar.transform.position, ScaleRulerPrefab.transform.rotation);
            _scaleRuler.name = SCALE_RULER_DUMMY_NAME;
            _scaleRuler.hideFlags = HideFlags.HideAndDontSave;
        }

        /// <summary>
        /// Ends scaling the avatar
        /// </summary>
        /// <param name="cancelled">If canceled returnt to old scale and viewpoint</param>
        private void EndScalingAvatar(GameObject avatar, bool cancelled)
        {
            try
            {
                if(avatar == null)
                {
                    _editingScale = false;
                }
                else
                {
#if VRC_SDK_VRCSDK3
                    if(_tempAvatarDescriptor == null)
                    {
                        Log(Strings.Log.descriptorIsNull, LogType.Error);
                        return;
                    }
#endif

                    _editingScale = false;
                    Tools.current = _tempToolOld;
                    if(!cancelled)
                    {
                        if(Settings.editingScaleMovesViewpoint)
                        {
#if VRC_SDK_VRCSDK3
                            SetViewpoint(_tempAvatarDescriptor, _viewPosTemp);
                            dynamic desc = Convert.ChangeType(_tempAvatarDescriptor, PumkinsTypeCache.VRC_AvatarDescriptor);
                            Log(Strings.Log.setAvatarScaleAndViewpointTo, LogType.Log, avatar.transform.localScale.z.ToString(), desc.ViewPosition.ToString());
#endif
                        }
                        else
                        {
                            Log(Strings.Log.setAvatarScaleTo, LogType.Log, avatar.transform.localScale.z.ToString());
                        }
                    }
                    else
                    {
#if VRC_SDK_VRCSDK3
                        if(_tempAvatarDescriptorWasAdded)
                            Helpers.DestroyAvatarDescriptorAndPipeline(SelectedAvatar);
                        else
                        {
                            dynamic desc = Convert.ChangeType(_tempAvatarDescriptor, PumkinsTypeCache.VRC_AvatarDescriptor);
                            desc.ViewPosition = _viewPosOld;
                        }
#endif
                        SelectedAvatar.transform.localScale = _avatarScaleOld;
                        Log(Strings.Log.canceledScaleChanges);
                    }
                }
#if VRC_SDK_VRCSDK3
                _tempAvatarDescriptor = null;
                _tempAvatarDescriptorWasAdded = false;
#endif
            }
            finally
            {
                if(_scaleViewpointDummy)
                    Helpers.DestroyAppropriate(_scaleViewpointDummy.gameObject);
                if(_scaleRuler)
                    Helpers.DestroyAppropriate(_scaleRuler);

                if(Settings.SerializedSettings != null)
                    Settings.SerializedSettings.ApplyModifiedPropertiesWithoutUndo();
            }
        }

#if VRC_SDK_VRCSDK3
        /// <summary>
        /// Begin Editing Viewposition.
        /// Used to move the viewpoint using unit's transform gizmo
        /// </summary>
        private void BeginEdittingViewpoint(GameObject avatar)
        {
            if(_editingView || _editingScale || !avatar)
                return;

            _tempAvatarDescriptor = avatar.GetComponent(PumkinsTypeCache.VRC_AvatarDescriptor);
            if(!_tempAvatarDescriptor)
            {
                _tempAvatarDescriptor = avatar.AddComponent(PumkinsTypeCache.VRC_AvatarDescriptor);
                _tempAvatarDescriptorWasAdded = true;
            }
            else
            {
                _tempAvatarDescriptorWasAdded = false;
            }

            dynamic desc = Convert.ChangeType(_tempAvatarDescriptor, PumkinsTypeCache.VRC_AvatarDescriptor);
            _viewPosOld = desc.ViewPosition;

            if(desc.ViewPosition == DEFAULT_VIEWPOINT)
                _viewPosTemp = Helpers.GetViewpointAtEyeLevel(SelectedAvatar.GetComponent<Animator>()) + avatar.transform.root.position;
            else
                _viewPosTemp = desc.ViewPosition + avatar.transform.root.position;

            _editingView = true;
            _tempToolOld = Tools.current;
            Tools.current = Tool.None;
            Selection.activeGameObject = SelectedAvatar;
        }

        /// <summary>
        /// Ends editing Viewposition
        /// </summary>
        /// <param name="cancelled">If cancelled revert viewposition to old value, if not leave it</param>
        private void EndEditingViewpoint(GameObject avatar, bool cancelled)
        {
            if(avatar == null)
            {
                _editingView = false;
            }
            else
            {
                if(_tempAvatarDescriptor == null)
                {
                    Log(Strings.Log.descriptorIsNull, LogType.Error);
                    return;
                }

                _editingView = false;
                Tools.current = _tempToolOld;
                if(!cancelled)
                {
                    SetViewpoint(_tempAvatarDescriptor, _viewPosTemp);
                    dynamic desc = Convert.ChangeType(_tempAvatarDescriptor, PumkinsTypeCache.VRC_AvatarDescriptor);
                    Log(Strings.Log.viewpointApplied, LogType.Log, desc.ViewPosition.ToString());
                }
                else
                {
                    if(_tempAvatarDescriptorWasAdded)
                        Helpers.DestroyAvatarDescriptorAndPipeline(SelectedAvatar);
                    else
                    {
                        dynamic desc = Convert.ChangeType(_tempAvatarDescriptor, PumkinsTypeCache.VRC_AvatarDescriptor);
                        desc.ViewPosition = _viewPosOld;
                    }

                    Log(Strings.Log.viewpointCancelled, LogType.Log);
                }
            }
            _tempAvatarDescriptor = null;
            _tempAvatarDescriptorWasAdded = false;
        }


        /// <summary>
        /// Sets the descriptor's viewpoint to a vector and rounds it's value to 3 decimals
        /// </summary>
        void SetViewpoint(Component desc, Vector3 position)
        {
            if(!desc)
            {
                Log("Avatar has no Avatar Descriptor. Ignoring", LogType.Warning);
                return;
            }

            dynamic descriptor = Convert.ChangeType(desc, PumkinsTypeCache.VRC_AvatarDescriptor);
            descriptor.ViewPosition = Helpers.RoundVectorValues(position - desc.gameObject.transform.position, 3);
        }

        /// <summary>
        /// Fill viseme tree on avatar descriptor or assign jaw flap bone if missing
        /// </summary>
        private void FillVisemes(GameObject avatar)
        {
            string log = Strings.Log.tryFillVisemes + " - ";
            string logFormat = avatar.name;

            string[] visemes =
            {
                    "vrc.v_sil",
                    "vrc.v_pp",
                    "vrc.v_ff",
                    "vrc.v_th",
                    "vrc.v_dd",
                    "vrc.v_kk",
                    "vrc.v_ch",
                    "vrc.v_ss",
                    "vrc.v_nn",
                    "vrc.v_rr",
                    "vrc.v_aa",
                    "vrc.v_e",
                    "vrc.v_ih",
                    "vrc.v_oh",
                    "vrc.v_ou",
                };

            var desc = avatar.GetComponent(PumkinsTypeCache.VRC_AvatarDescriptor);
            if(!desc)
                desc = avatar.AddComponent(PumkinsTypeCache.VRC_AvatarDescriptor);

            dynamic descriptor = Convert.ChangeType(desc, PumkinsTypeCache.VRC_AvatarDescriptor);
            if(descriptor.VisemeBlendShapes == null || descriptor.VisemeBlendShapes.Length != visemes.Length)
            {
                descriptor.VisemeBlendShapes = new string[visemes.Length];
            }

            var renders = avatar.GetComponentsInChildren<SkinnedMeshRenderer>(true)
                                .Where(r => r.sharedMesh != null)
                                .ToArray();

            bool foundShape = false;

            for(int i = 0; !foundShape && i < renders.Length; i++)
            {
                for(int j = 0; !foundShape && j < renders[i].sharedMesh.blendShapeCount; j++)
                {
                    for(int k = 0; k < visemes.Length; k++)
                    {
                        string s = "-none-";
                        int index = renders[i].sharedMesh.GetBlendShapeIndex(visemes[k]);

                        if(index != -1)
                        {
                            descriptor.VisemeSkinnedMesh = renders[i];
                            foundShape = true;

                            s = visemes[k];
                        }

                        descriptor.VisemeBlendShapes[k] = s;
                    }
                }
            }

            if(descriptor.VisemeSkinnedMesh == null)
            {
                log += Strings.Log.noSkinnedMeshFound;
                Log(log, LogType.Error, logFormat);
            }
            else
            {
                FieldInfo lipSyncField = PumkinsTypeCache.VRC_AvatarDescriptor.GetField("lipSync");
                void SetLipSyncType(string type)
                {
                    var intValue = (int)Enum.Parse(PumkinsTypeCache.VRC_AvatarDescriptor_LipSyncStyle, type);
                    var enumValue = Enum.ToObject(PumkinsTypeCache.VRC_AvatarDescriptor_LipSyncStyle, intValue);
                    lipSyncField.SetValue(descriptor, enumValue);
                }

                if(foundShape)
                {
                    SetLipSyncType("VisemeBlendShape");
                    log += Strings.Log.success;
                    Log(log, LogType.Log, logFormat);
                }
                else
                {
                    var anim = avatar.GetComponent<Animator>();
                    if(anim && anim.isHuman)
                    {
                        var jaw = anim.GetBoneTransform(HumanBodyBones.Jaw);
                        if(jaw)
                            SetLipSyncType("JawFlapBone");
                        else
                            SetLipSyncType("Default");
                    }
                    else
                    {
                        SetLipSyncType("Default");
                    }
                    log += Strings.Log.meshHasNoVisemes;
                    Log(log, LogType.Warning, logFormat);
                }
            }
        }
#endif

        /// <summary>
        /// Sets the Probe Anchor of all Skinned Mesh Renderers to transform by path
        /// </summary>
        private void SetRendererAnchor(GameObject avatar, string anchorPath, params Type[] rendererTypes)
        {
            Transform anchor = avatar.transform.Find(anchorPath);
            if(!anchor)
            {
                Log(Strings.Log.transformNotFound, LogType.Warning, anchorPath);
                return;
            }
            SetRendererAnchor(avatar, anchor, rendererTypes);
        }

        private void SetRendererAnchor(GameObject avatar, HumanBodyBones humanBone, params Type[] rendererTypes)
        {
            string boneName = Enum.GetName(typeof(HumanBodyBones), humanBone);
            Transform anchor = avatar.GetComponent<Animator>()?.GetBoneTransform(humanBone);
            if(!anchor)
            {
                Log(Strings.Log.transformNotFound, LogType.Warning, boneName);
                return;
            }
            SetRendererAnchor(avatar, anchor, rendererTypes);
        }

        void SetRendererAnchor(GameObject avatar, Transform target, params Type[] rendererTypes)
        {
            foreach(var renderType in rendererTypes)
            {
                if(!typeof(Renderer).IsAssignableFrom(renderType))
                    continue;
                var renders = avatar.GetComponentsInChildren(renderType, true);
                foreach(var render in renders)
                {
                    if(render is Renderer ren && ren.reflectionProbeUsage != ReflectionProbeUsage.Off)
                    {
                        ren.probeAnchor = target;
                        Log(Strings.Log.setProbeAnchorTo, LogType.Log, render.name, target.name);
                    }
                }
            }
        }
    }
}