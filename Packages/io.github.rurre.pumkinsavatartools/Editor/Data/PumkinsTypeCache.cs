using System;
using System.Collections.Generic;
using Pumkin.AvatarTools;
using Pumkin.HelperFunctions;
using System.IO;
using System.Linq;
using Pumkin.AvatarTools.Copiers;
using UnityEditor;
using UnityEngine;

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
        public static readonly Type IKExecutionOrder = TypeHelpers.GetTypeAnywhere("RootMotion.FinalIK.IKExecutionOrder");

        public static readonly Type VRMSpringBone = TypeHelpers.GetTypeAnywhere("VRM.VRMSpringBone");

        static readonly string extraTypesTemplatePath = $"{PumkinsAvatarTools.MainFolderPath}/extra_copier_types_template.json";
        const string extraTypesFilename = "extra_copier_types.json";
        const string extraTypesStatesFilename = "extra_copier_types_states.json";

        public static void OpenExtraTypesJson()
        {
	        PumkinsSettingsUtility.OpenSettingsFile(extraTypesFilename, CopierTypesTemplate);
        }

        public static List<DynamicCopierTypeCategory> ExtraTypes { get; private set; }

        public static bool HasExtraTypes
        {
	        get => hasValidTypes != null && (bool)hasValidTypes;
        }

        static bool? hasValidTypes;

        static DynamicCopierTypesWrapper CopierTypesTemplate
        {
	        get
	        {
		        if(_copierTypesTemplate == null)
		        {
			        if(File.Exists(extraTypesTemplatePath))
			        {
				        string exampleJson = File.ReadAllText(extraTypesTemplatePath);
				        _copierTypesTemplate = JsonUtility.FromJson<DynamicCopierTypesWrapper>(exampleJson);
				        return _copierTypesTemplate;
			        }
			        PumkinsAvatarTools.Log("_Couldn't find example file for extra copier types inside PumkinsAvatarTools folder.", LogType.Warning);
		        }
		        return _copierTypesTemplate;
	        }
        }

        class DynamicCopierStatesWrapper
        {
	        public string[] enabledTypeNames;
        }

        static DynamicCopierTypesWrapper _copierTypesTemplate;

        internal static void LoadExtraTypes()
        {
	        bool wasCreated = PumkinsSettingsUtility.EnsureSettingsFileExists(extraTypesFilename, CopierTypesTemplate);
	        if(PumkinsSettingsUtility.TryLoadSettings<DynamicCopierTypesWrapper>(extraTypesFilename, out var typesWrapper))
	        {
		        if(!wasCreated)
					typesWrapper.AddTypeNamesFromWrapper(CopierTypesTemplate);

		        typesWrapper.Initialize();
				hasValidTypes = typesWrapper.HasValidTypes;
				ExtraTypes = typesWrapper.extraCopierTypes;

				if(PumkinsSettingsUtility.TryLoadSettings<DynamicCopierStatesWrapper>(extraTypesStatesFilename, out var enabledTypeNames))
				{
					// Get names of all enabled types from json then compare it to a list of valid types and if found, set it's state to enabled.
					var enabledNames = enabledTypeNames.enabledTypeNames.Select(n => n.ToLower());
					foreach(var typeCategory in ExtraTypes)
					{
						var validTypeNames = typeCategory.types.Select(t => t.FullName.ToLower()).ToList();
						for(int i = 0; i < validTypeNames.Count; i++)
						{
							if(enabledNames.Contains(validTypeNames[i]))
								typeCategory.enableStates[i] = true;
						}
					}
				}
	        }
        }

        internal static void SaveExtraTypeEnableStates()
        {
	        List<string> enabledTypes = new List<string>();

	        // Get names of all enabled types across any category
	        foreach(var typeCategory in ExtraTypes)
	        {
		        for(int i = 0; i < typeCategory.enableStates.Count; i++)
		        {
			        if(typeCategory.enableStates[i])
				        enabledTypes.Add(typeCategory.types[i].FullName);
		        }
	        }

	        var statesWrapper = new DynamicCopierStatesWrapper { enabledTypeNames = enabledTypes.ToArray() };
	        PumkinsSettingsUtility.SaveSettings(extraTypesStatesFilename, statesWrapper, true);
        }
    }
}