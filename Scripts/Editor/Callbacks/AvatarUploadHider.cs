using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

#if VRC_SDK_VRCSDK2 || (VRC_SDK_VRCSDK3 && !UDON)
using VRC.Core;
using VRC.SDKBase.Editor.BuildPipeline;
using VRC.SDKBase;
#endif

#if VRC_SDK_VRCSDK3 && !UDON
using VRC_AvatarDescriptor = VRC.SDK3.Avatars.Components.VRCAvatarDescriptor;
#elif VRC_SDK_VRCSDK2
using VRC_AvatarDescriptor = VRCSDK2.VRC_AvatarDescriptor;
#endif

namespace Pumkin.AvatarTools.Callbacks
{
#if VRC_SDK_VRCSDK2 || (VRC_SDK_VRCSDK3 && !UDON)
    class AvatarUploadHider : IVRCSDKPreprocessAvatarCallback
    {
        public int callbackOrder => 0;

        const string uploadTargetPrefName = "UploadTargetAvatarID";

        static bool _enabled = true;
        static GameObject _uploadTarget;
        static IEnumerable<GameObject> _avatarCache;

        static string AvatarID
        {
            get => PrefManager.GetString(uploadTargetPrefName);
            set => PrefManager.SetString(uploadTargetPrefName, value);
        }
        public static bool Enabled
        {
            get => _enabled;
            set
            {
                _enabled = value;
                if(EditorApplication.isPlaying)
                    SetOtherAvatarsActiveState(!value);
            }
        }
        public static GameObject UploadTarget
        {
            get
            {
                if(!_uploadTarget)
                {
                    string id = AvatarID;
                    if(!string.IsNullOrWhiteSpace(id))
                        _uploadTarget = GameObject.FindObjectsOfType<PipelineManager>()
                            .Where(a => a.blueprintId == id)
                            .Select(a => a.gameObject)
                            .FirstOrDefault();
                    AvatarID = null;
                }
                return _uploadTarget;
            }
        }
        static IEnumerable<GameObject> AvatarCache
        {
            get
            {
                if(_avatarCache == null)
                {
                    _avatarCache = GameObject.FindObjectsOfType<VRC_AvatarDescriptor>()
                        .Select(x => x.gameObject);
                }
                return _avatarCache;
            }
        }


        public AvatarUploadHider()
        {
            EditorApplication.playModeStateChanged -= HandlePlayModeStateChange;
            EditorApplication.playModeStateChanged += HandlePlayModeStateChange;
        }

        public bool OnPreprocessAvatar(GameObject avatarGameObject)
        {
            try
            {
                AvatarID = avatarGameObject.GetComponent<PipelineManager>().blueprintId;
            }
            catch(Exception e)
            {
                PumkinsAvatarTools.Log("Failed to hide avatars: " + e.Message, LogType.Warning);
            }
            return true;
        }

        void HandlePlayModeStateChange(PlayModeStateChange mode)
        {
            if(mode != PlayModeStateChange.EnteredPlayMode)
                return;

            if(!Enabled || UploadTarget == null)
                return;

            SetOtherAvatarsActiveState(false);
        }

        static void SetOtherAvatarsActiveState(bool isActive)
        {
            if(AvatarCache is null || UploadTarget is null)
                return;

            foreach(var av in AvatarCache)
            {
                if(av == UploadTarget)
                    continue;
                av.gameObject.SetActive(isActive);
            }
        }
    }
#endif
}
