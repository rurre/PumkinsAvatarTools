using System;
using Pumkin.HelperFunctions;

namespace Pumkin.DataStructures
{
    /// <summary>
    /// Cached types. Null if not found
    /// </summary>
    internal static class PumkinsTypeCache
    {

#if VRC_SDK_VRCSDK2
        public static readonly Type VRC_AvatarDescriptor = TypeHelpers.GetTypeAnywhere("VRCSDK2.VRC_AvatarDescriptor");
        public static readonly Type VRC_SpatialAudioSource = TypeHelpers.GetTypeAnywhere("VRCSDK2.VRC_SpatialAudioSource");
#elif VRC_SDK_VRCSDK3 && !UDON
        public static readonly Type VRC_AvatarDescriptor = TypeHelpers.GetTypeAnywhere("VRC.SDK3.Avatars.Components.VRCAvatarDescriptor");
        public static readonly Type VRC_SpatialAudioSource = TypeHelpers.GetTypeAnywhere("VRC.SDK3.Avatars.Components.VRCSpatialAudioSource");
#else
        public static readonly Type VRC_AvatarDescriptor = null;
        public static readonly Type VRC_SpatialAudioSource = null;
#endif

        public static readonly Type PipelineManager = TypeHelpers.GetTypeAnywhere("VRC.Core.PipelineManager");
        public static readonly Type VRC_AvatarDescriptor_LipSyncStyle = TypeHelpers.GetTypeAnywhere("VRC.SDKBase.VRC_AvatarDescriptor+LipSyncStyle");
        public static readonly Type VRC_AvatarDescriptor_Viseme = TypeHelpers.GetTypeAnywhere("VRC.SDKBase.VRC_AvatarDescriptor+Viseme");
        public static readonly Type VRC_SpacialAudioSource = TypeHelpers.GetTypeAnywhere("VRC.SDKBase.VRC_SpatialAudioSource");
        public static readonly Type VRC_IKFollower = TypeHelpers.GetTypeAnywhere("VRC.SDKBase.VRC_IKFollower");
        public static readonly Type ONSPAudioSource = TypeHelpers.GetTypeAnywhere("ONSPAudioSource");
        
        public static readonly Type DynamicBone = TypeHelpers.GetTypeAnywhere("DynamicBone");
        public static readonly Type DynamicBoneCollider = TypeHelpers.GetTypeAnywhere("DynamicBoneCollider");
    }
}