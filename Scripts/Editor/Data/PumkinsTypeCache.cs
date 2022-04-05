using System;
using Pumkin.HelperFunctions;
#if PUMKIN_PBONES
using VRC.SDK3.Dynamics.PhysBone.Components;
using VRC.SDK3.Dynamics.Contact.Components;
#endif

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
#if PUMKIN_PBONES
        public static readonly Type ContactReceiver = typeof(VRCContactReceiver);
        public static readonly Type ContactSender = typeof(VRCContactSender);

        public static readonly Type PhysBone = typeof(VRCPhysBone);
        public static readonly Type PhysBoneCollider = typeof(VRCPhysBoneCollider);
#else
        public static readonly Type ContactReceiver = null;
        public static readonly Type ContactSender = null;

        public static readonly Type PhysBone = null;
        public static readonly Type PhysBoneCollider = null;
#endif
        public static readonly Type DynamicBone = TypeHelpers.GetTypeAnywhere("DynamicBone");
        public static readonly Type DynamicBoneCollider = TypeHelpers.GetTypeAnywhere("DynamicBoneCollider");

		public static readonly Type CCDIK = TypeHelpers.GetTypeAnywhere("RootMotion.FinalIK.CCDIK");
		public static readonly Type LimbIK = TypeHelpers.GetTypeAnywhere("RootMotion.FinalIK.");
		public static readonly Type RotationLimit = TypeHelpers.GetTypeAnywhere("RootMotion.FinalIK.LimbIK");
		public static readonly Type FABRIK = TypeHelpers.GetTypeAnywhere("RootMotion.FinalIK.FABRIK");
		public static readonly Type AimIK = TypeHelpers.GetTypeAnywhere("RootMotion.FinalIK.AimIK");
		public static readonly Type FullBodyBipedIK = TypeHelpers.GetTypeAnywhere("RootMotion.FinalIK.FullBodyBipedIK");
		public static readonly Type VRIK = TypeHelpers.GetTypeAnywhere("RootMotion.FinalIK.VRIK");
    }
}