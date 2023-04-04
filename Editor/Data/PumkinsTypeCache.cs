using System;
using Pumkin.HelperFunctions;

namespace Pumkin.DataStructures
{
    /// <summary>
    /// Cached types. Null if not found
    /// </summary>
    internal static class PumkinsTypeCache
    {
	    public static readonly Type VRC_AvatarDescriptor = TypeHelpers.GetTypeAnywhere("VRC.SDK3.Avatars.Components.VRCAvatarDescriptor");
        public static readonly Type VRC_SpatialAudioSource = TypeHelpers.GetTypeAnywhere("VRC.SDK3.Avatars.Components.VRCSpatialAudioSource");
        public static readonly Type ContactReceiver = TypeHelpers.GetTypeAnywhere("VRC.SDK3.Dynamics.Contact.Components.VRCContactReceiver");
        public static readonly Type ContactSender = TypeHelpers.GetTypeAnywhere("VRC.SDK3.Dynamics.Contact.Components.VRCContactSender");

        public static readonly Type PhysBone = TypeHelpers.GetTypeAnywhere("VRC.SDK3.Dynamics.PhysBone.Components.VRCPhysBone");
        public static readonly Type PhysBoneCollider = TypeHelpers.GetTypeAnywhere("VRC.SDK3.Dynamics.PhysBone.Components.VRCPhysBoneCollider");

        public static readonly Type PipelineManager = TypeHelpers.GetTypeAnywhere("VRC.Core.PipelineManager");
        public static readonly Type VRC_AvatarDescriptor_LipSyncStyle = TypeHelpers.GetTypeAnywhere("VRC.SDKBase.VRC_AvatarDescriptor+LipSyncStyle");
        public static readonly Type VRC_AvatarDescriptor_Viseme = TypeHelpers.GetTypeAnywhere("VRC.SDKBase.VRC_AvatarDescriptor+Viseme");
        public static readonly Type VRC_IKFollower = TypeHelpers.GetTypeAnywhere("VRC.SDKBase.VRC_IKFollower");
        public static readonly Type ONSPAudioSource = TypeHelpers.GetTypeAnywhere("ONSPAudioSource");
        public static readonly Type VRCStation = TypeHelpers.GetTypeAnywhere("VRC.SDK3.Avatars.Components.VRCStation");

        public static readonly Type DynamicBone = TypeHelpers.GetTypeAnywhere("DynamicBone");
        public static readonly Type DynamicBoneCollider = TypeHelpers.GetTypeAnywhere("DynamicBoneCollider");
        public static readonly Type DynamicBoneColliderBase = TypeHelpers.GetTypeAnywhere("DynamicBoneColliderBase");
        public static readonly Type DynamicBonePlaneCollider = TypeHelpers.GetTypeAnywhere("DynamicBonePlaneCollider");

		public static readonly Type CCDIK = TypeHelpers.GetTypeAnywhere("RootMotion.FinalIK.CCDIK");
		public static readonly Type LimbIK = TypeHelpers.GetTypeAnywhere("RootMotion.FinalIK.");
		public static readonly Type RotationLimit = TypeHelpers.GetTypeAnywhere("RootMotion.FinalIK.LimbIK");
		public static readonly Type FABRIK = TypeHelpers.GetTypeAnywhere("RootMotion.FinalIK.FABRIK");
		public static readonly Type AimIK = TypeHelpers.GetTypeAnywhere("RootMotion.FinalIK.AimIK");
		public static readonly Type FullBodyBipedIK = TypeHelpers.GetTypeAnywhere("RootMotion.FinalIK.FullBodyBipedIK");
		public static readonly Type VRIK = TypeHelpers.GetTypeAnywhere("RootMotion.FinalIK.VRIK");
        public static readonly Type Grounder = TypeHelpers.GetTypeAnywhere("RootMotion.FinalIK.Grounder");
        public static readonly Type VRMSpringBone = TypeHelpers.GetTypeAnywhere("VRM.VRMSpringBone");
    }
}