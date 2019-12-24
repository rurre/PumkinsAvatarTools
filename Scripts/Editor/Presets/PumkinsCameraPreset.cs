using Pumkin.AvatarTools;
using Pumkin.DataStructures;
using Pumkin.HelperFunctions;
using System;
using UnityEditor;
using UnityEngine;
using VRCSDK2;

namespace Pumkin.Presets
{
    [Serializable]
    public class PumkinsCameraPreset : PumkinPreset
    {
        public enum CameraOffsetMode { Viewpoint, AvatarRoot, Transform };
        
        public CameraOffsetMode offsetMode = CameraOffsetMode.Viewpoint;
        public Vector3 positionOffset = Vector3.zero;
        public Vector3 rotationAnglesOffset = Vector3.zero;
        public string transformPath = "";

        public bool useOverlay = false;        
        public string overlayImagePath = "";
        public Color overlayImageTint = Color.white;

        public bool useBackground = false;
        public PumkinsAvatarTools.CameraBackgroundOverrideType backgroundType = PumkinsAvatarTools.CameraBackgroundOverrideType.Color;
        public Color backgroundColor = Color.white;

        public string backgroundImagePath = "";
        public Color backgroundImageTint = Color.white;

        public Material backgroundMaterial = null;

        private PumkinsCameraPreset() { }

        public override bool ApplyPreset(GameObject avatar)
        {
            Camera cam = PumkinsAvatarTools.SelectedCamera;
            if(!cam || !avatar)
                return false;

            Transform dummy = null;
            try
            {
                if(offsetMode == CameraOffsetMode.Viewpoint)
                {
                    dummy = new GameObject("Dummy").transform;
                    var desc = avatar.GetComponent<VRC_AvatarDescriptor>();
                    dummy.localPosition = desc.transform.position + desc.ViewPosition;

                    cam.transform.localPosition = positionOffset + dummy.transform.localPosition;
                    cam.transform.localEulerAngles = rotationAnglesOffset + dummy.transform.localEulerAngles;
                }
                else
                {
                    Transform t = avatar.transform.Find(transformPath);
                    dummy = new GameObject("Dummy").transform;
                    if(t)
                    {                        
                        Transform oldCamParent = cam.transform.parent;
                        cam.transform.parent = dummy;
                        cam.transform.localPosition = positionOffset;
                        cam.transform.localEulerAngles = rotationAnglesOffset;
                        dummy.SetPositionAndRotation(t.position, t.rotation);
                        cam.transform.parent = oldCamParent;
                    }
                }
            }
            catch(Exception e)
            {
                Debug.LogError(e.Message);
                if(dummy)
                    DestroyImmediate(dummy.gameObject);
                return false;
            }
            finally
            {
                if(dummy)
                    DestroyImmediate(dummy.gameObject);
            }

            PumkinsAvatarTools.Instance.bThumbnails_use_camera_overlay = useOverlay;
            if(useOverlay)
            {
                PumkinsAvatarTools.Instance.SetOverlayToImageFromPath(overlayImagePath);
            }            

            PumkinsAvatarTools.Instance.bThumbnails_use_camera_background = useBackground;
            if(useBackground)
            {
                PumkinsAvatarTools.Instance.cameraBackgroundType = backgroundType;
                switch(backgroundType)
                {
                    case PumkinsAvatarTools.CameraBackgroundOverrideType.Color:                        
                        PumkinsAvatarTools.Instance.SetCameraBackgroundToColor(backgroundColor);
                        break;
                    case PumkinsAvatarTools.CameraBackgroundOverrideType.Image:
                        PumkinsAvatarTools.Instance.cameraBackgroundImageTint = backgroundImageTint;
                        PumkinsAvatarTools.Instance.SetBackgroundToImageFromPath(backgroundImagePath);
                        break;
                    case PumkinsAvatarTools.CameraBackgroundOverrideType.Skybox:
                        PumkinsAvatarTools.Instance.SetCameraBackgroundToSkybox(backgroundMaterial);
                        break;
                    default:                        
                        break;
                }
            }

            PumkinsAvatarTools.Instance.RefreshBackgroundOverrideType();

            return true;
        }       

        public bool SavePreset(GameObject referenceObject, Camera camera, bool overwriteExisting)
        {
            PumkinsCameraPreset p = ScriptableObjectUtility.CreateAndSaveAsset<PumkinsCameraPreset>(name, PumkinsAvatarTools.MainFolderPath + "/Resources/Presets/Cameras/", overwriteExisting) as PumkinsCameraPreset;
            if(!p)
                return false;

            p.name = name;
            p.offsetMode = offsetMode;

            if(p.offsetMode == CameraOffsetMode.AvatarRoot)
                CalculateOffsets(referenceObject.transform.root, camera);
            else if(p.offsetMode == CameraOffsetMode.Transform)
                CalculateOffsets(referenceObject.transform, camera);
            else
                CalculateOffsets(PumkinsAvatarTools.SelectedAvatar.GetComponent<VRC_AvatarDescriptor>(), camera);
            
            p.positionOffset = positionOffset;
            p.rotationAnglesOffset = rotationAnglesOffset;
            p.transformPath = transformPath;

            p.useOverlay = PumkinsAvatarTools.Instance.bThumbnails_use_camera_overlay;

            if(p.useOverlay)
            {
                p.overlayImagePath = PumkinsAvatarTools.Instance._overlayPath;
                p.overlayImageTint = PumkinsAvatarTools.Instance.cameraOverlayImageTint;
            }

            p.useBackground = PumkinsAvatarTools.Instance.bThumbnails_use_camera_background;
            p.backgroundType = PumkinsAvatarTools.Instance.cameraBackgroundType;

            if(p.useBackground)
            {
                switch(p.backgroundType)
                {
                    case PumkinsAvatarTools.CameraBackgroundOverrideType.Color:
                        p.backgroundColor = PumkinsAvatarTools.Instance._thumbsCamBgColor;
                        break;
                    case PumkinsAvatarTools.CameraBackgroundOverrideType.Image:
                        p.backgroundImagePath = PumkinsAvatarTools.Instance._backgroundPath;
                        p.backgroundImageTint = PumkinsAvatarTools.Instance.cameraBackgroundImageTint;
                        break;
                    case PumkinsAvatarTools.CameraBackgroundOverrideType.Skybox:
                        p.backgroundMaterial = RenderSettings.skybox;
                        break;
                    default:
                        break;
                }
            }
            EditorUtility.SetDirty(p);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
                        
            PumkinsPresetManager.LoadPresets<PumkinsCameraPreset>();
            return true;
        }

        public void CalculateOffsets(Transform target, Camera cam)
        {
            if(!target || !cam)
                return;

            if(target == target.root)
                offsetMode = CameraOffsetMode.AvatarRoot;
            else
                offsetMode = CameraOffsetMode.Transform;
            
            positionOffset = cam.transform.localPosition - target.position;
            rotationAnglesOffset = cam.transform.localEulerAngles - target.eulerAngles;
            transformPath = Helpers.GetGameObjectPath(target);            
        }

        public void CalculateOffsets(VRC_AvatarDescriptor desc, Camera cam)
        {
            if(!desc || !cam)
                return;            

            SerialTransform offsets = GetOffsets(desc, cam);
            if(offsets)
            {
                offsetMode = CameraOffsetMode.Viewpoint;
                positionOffset = offsets.position;
                rotationAnglesOffset = offsets.localEulerAngles;
            }
        }

        //Static Functions
        public static void ApplyTransformWithViewpointOffset(GameObject avatar, Camera cam, SerialTransform trans)
        {
            ApplyPositionAndRotationWithViewpointOffset(avatar, cam, trans.position, trans.localEulerAngles);
        }

        public static void ApplyPositionAndRotationWithViewpointOffset(GameObject avatar, Camera cam, Vector3 position, Vector3 rotationAngles)
        {
            if(!cam || !avatar)
                return;

            Transform dummy = null;
            try
            {
                dummy = new GameObject("Dummy").transform;
                var desc = avatar.GetComponent<VRC_AvatarDescriptor>();
                dummy.localPosition = desc.ViewPosition + desc.gameObject.transform.position;

                cam.transform.localPosition = position + dummy.transform.position;
                cam.transform.localEulerAngles = rotationAngles + dummy.transform.eulerAngles;                
            }
            catch(Exception e)
            {
                Debug.LogError(e.Message);
            }
            finally
            {
                if(dummy)
                    DestroyImmediate(dummy.gameObject);
            }
        }

        /// <summary>
        /// Gets camera offsets from viewpoint and returns a SerialTransform
        /// </summary>        
        /// <returns>SerialTransform only holds values, it doesn't reference a real transform</returns>
        public static SerialTransform GetOffsets(VRC_AvatarDescriptor desc, Camera cam)
        {
            SerialTransform offsets = new SerialTransform();
            Transform dummy = null;            
            try
            {
                dummy = new GameObject("Dummy").transform;
                dummy.localPosition = desc.ViewPosition + desc.gameObject.transform.position;                

                offsets.position = cam.transform.localPosition - dummy.localPosition;
                offsets.localEulerAngles = cam.transform.localEulerAngles - dummy.localEulerAngles;
            }
            catch(Exception e)
            {
                Debug.LogError(e.Message);
            }
            finally
            {
                if(dummy)
                    DestroyImmediate(dummy.gameObject);                
            }
            return offsets;
        }

        public static SerialTransform GetCameraOffsetFromViewpoint(GameObject avatar, Camera cam)
        {            
            VRC_AvatarDescriptor desc = avatar.GetComponent<VRC_AvatarDescriptor>();
            SerialTransform offsets = null;

            if(desc)            
                offsets = GetOffsets(desc, cam);
            return offsets;
        }

        public override string ToString()
        {
            return name;
        }
    }
}