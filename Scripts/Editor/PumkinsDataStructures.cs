using Newtonsoft.Json;
using Pumkin.AvatarTools;
using Pumkin.HelperFunctions;
using Pumkin.PoseEditor;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using VRC.Core;
using VRCSDK2;
using VRCSDK2.Validation.Performance;
using VRCSDK2.Validation.Performance.Stats;
using Pumkin.Translations;

namespace Pumkin.DataStructures
{
    [ExecuteInEditMode, InitializeOnLoad]
    public class Strings : SingletonScriptableObject<Strings>
    {        
        static PumkinsLanguageHolder _holder;        
        public static PumkinsLanguageHolder Holder
        {
            get
            {
                return _holder;
            }
            private set
            {
                _holder = value;
                ReloadStrings();
            }
        }

        [RuntimeInitializeOnLoadMethod]
        public static void LoadHolder()
        {
            if(Holder)
                return;

            Holder = (FindObjectOfType<PumkinsLanguageHolder>()) ?? (_holder = CreateInstance<PumkinsLanguageHolder>());            
        }

        public readonly string versionNr = "0.7b - Work in Progress";
        public readonly string toolsPage = "https://github.com/rurre/PumkinsAvatarTools/";
        public readonly string donationLink = "https://ko-fi.com/notpumkin";
        public readonly string discordLink = "https://discord.gg/7vyekJv";
        static readonly string translationsPath = "Translations/";
        
        public string Language
        {
            get;
            private set;
        }        
        public string Author
        {
            get; private set;
        }
                
        public class Main
        {
            public static string Title { get; internal set; }
            public static string WindowName { get; internal set; }
            public static string Version { get; internal set; }
            public static string Avatar { get; internal set; }
            public static string Tools { get; internal set; }
            public static string Copier { get; internal set; }
            public static string RemoveAll { get; internal set; }
            public static string AvatarInfo { get; internal set; }
            public static string Thumbnails { get; internal set; }

            public static string Misc { get; internal set; }
            public static string UseSceneSelection { get; internal set; }

            static Main()
            {
                Reload();
            }

            public static void Reload()
            {
                if(!Holder)
                    return;

                Avatar = Holder.main.avatar ?? "_Avatar";
                Title = Holder.main.title ?? "_Pumkin's Avatar Tools";
                Version = Holder.main.version ?? "_Version";
                WindowName = Holder.main.windowName ?? "_Pumkin Tools";
                Tools = Holder.main.tools ?? "_Tools";
                Copier = Holder.main.copier ?? "_Copy Components";
                AvatarInfo = Holder.main.avatarInfo ?? "_Avatar Info";
                RemoveAll = Holder.main.removeAll ?? "_Remove All";
                Misc = Holder.main.misc ?? "_Misc";
                Thumbnails = Holder.main.thumbnails ?? "_Thumbnails";
                UseSceneSelection = Holder.main.useSceneSelection ?? "_Use Scene Selection";
            }
        };
        public static class Buttons
        {
            public static string SelectFromScene { get; internal set; }
            public static string CopySelected { get; internal set; }
            public static string Cancel { get; internal set; }
            public static string Apply { get; internal set; }
            public static string Refresh { get; internal set; }
            public static string Copy { get; internal set; }
            public static string OpenHelpPage { get; internal set; }
            public static string OpenGithubPage { get; internal set; }
            public static string OpenDonationPage { get; internal set; }
            public static string OpenPoseEditor { get; internal set; }
            public static string JoinDiscordServer { get; internal set; }
            public static string SelectNone { get; internal set; }
            public static string SelectAll { get; internal set; }
            public static string Browse { get; internal set; }

            static Buttons()
            {
                Reload();
            }

            public static void Reload()
            {
                if(!Holder)
                    return;

                SelectFromScene = Holder.buttons.selectFromScene ?? "_Select From Scene";
                CopySelected = Holder.buttons.copySelected ?? "_Copy Selected";
                Refresh = Holder.buttons.refresh ?? "_Refresh";
                Cancel = Holder.buttons.cancel ?? "_Cancel";
                Apply = Holder.buttons.apply ?? "_Apply";
                Copy = Holder.buttons.copy ?? "_Copy Text";
                OpenHelpPage = Holder.buttons.openHelpPage ?? "_Open Help Page";
                OpenGithubPage = Holder.buttons.openGithubPage ?? "_Open Github Page";
                OpenDonationPage = Holder.buttons.openDonationPage ?? "_Buy me a Ko-Fi~";
                OpenPoseEditor = Holder.buttons.openPoseEditor ?? "_Open Pose Editor";
                JoinDiscordServer = Holder.buttons.joinDiscordServer ?? "_Join Discord Server!";
                SelectNone = Holder.buttons.selectNone ?? "_Select None";
                SelectAll = Holder.buttons.selectAll ?? "_Select All";
                Browse = Holder.buttons.browse ?? "_Browse";
            }
        };
        public static class Tools
        {
            public static string FillVisemes { get; internal set; }
            public static string EditViewpoint { get; internal set; }
            public static string RevertBlendshapes { get; internal set; }
            public static string ZeroBlendshapes { get; internal set; }
            public static string ResetPose { get; internal set; }
            public static string ResetToTPose { get; internal set; }
            public static string EditScale { get; internal set; }

            static Tools()
            {
                Reload();
            }

            public static void Reload()
            {
                if(!Holder)
                    return;

                FillVisemes = Holder.tools.fillVisemes ?? "_Fill Visemes";
                EditViewpoint = Holder.tools.editViewpoint ?? "_Edit Viewpoint";
                RevertBlendshapes = Holder.tools.revertBlendshapes ?? "_Revert Blendshapes";
                ZeroBlendshapes = Holder.tools.zeroBlendshapes ?? "_Zero Blendshapes";
                ResetPose = Holder.tools.resetPose ?? "_Reset Pose";
                ResetToTPose = Holder.tools.resetToTPose ?? "_Reset to T-Pose";
                EditScale = Holder.tools.editScale ?? "_Edit Scale";
            }
        };
        public static class AvatarInfo
        {
            public static string SelectAvatarFirst { get; internal set; }

            public static string Name { get; internal set; }
            public static string GameObjects { get; internal set; }
            public static string Bones { get; internal set; }
            public static string SkinnedMeshRenderers { get; internal set; }
            public static string MeshRenderers { get; internal set; }
            public static string Polygons { get; internal set; }
            public static string UsedMaterialSlots { get; internal set; }
            public static string UniqueMaterials { get; internal set; }
            public static string Shaders { get; internal set; }
            public static string DynamicBoneTransforms { get; internal set; }
            public static string DynamicBoneColliders { get; internal set; }
            public static string DynamicBoneColliderTransforms { get; internal set; }
            public static string ParticleSystems { get; internal set; }
            public static string MaxParticles { get; internal set; }
            public static string OverallPerformance { get; internal set; }
            public static string Line { get; internal set; }

            static AvatarInfo()
            {
                Reload();
            }

            public static void Reload()
            {
                if(!Holder)
                    return;

                Name = Holder.avatarInfo.name ?? "_{0}";
                Line = Holder.avatarInfo.line ?? "---------------------";
                GameObjects = Holder.avatarInfo.gameObjects ?? "_GameObjects: {0} ({1})";
                Bones = Holder.avatarInfo.bones ?? "_Bones: {0} - {1}";
                SkinnedMeshRenderers = Holder.avatarInfo.skinnedMeshRenderers ?? "_Skinned Mesh Renderers: {0} ({1}) - {2}";
                MeshRenderers = Holder.avatarInfo.meshRenderers ?? "_Mesh Renderers: {0} ({1}) - {2}";
                Polygons = Holder.avatarInfo.polygons ?? "_Polygons: {0} ({1}) - {2}";
                UsedMaterialSlots = Holder.avatarInfo.usedMaterialSlots ?? "_Used Material Slots: {0} ({1}) - {2}";
                UniqueMaterials = Holder.avatarInfo.uniqueMaterials ?? "_Unique Materials: {0} ({1})";
                Shaders = Holder.avatarInfo.shaders ?? "_Shaders: {0}";
                DynamicBoneTransforms = Holder.avatarInfo.dynamicBoneTransforms ?? "_Dynamic Bone Transforms: {0} ({1}) - {2}";
                DynamicBoneColliders = Holder.avatarInfo.dynamicBoneColliders ?? "_Dynamic Bone Colliders: {0} ({1}) - {2}";
                DynamicBoneColliderTransforms = Holder.avatarInfo.dynamicBoneColliderTransforms ?? "_Collider Affected Transforms: {0} ({1}) - {2}";
                ParticleSystems = Holder.avatarInfo.particleSystems ?? "_Particle Systems: {0} ({1}) - {2}";
                MaxParticles = Holder.avatarInfo.maxParticles ?? "_Max Particles: {0} ({1}) - {2}";
                OverallPerformance = Holder.avatarInfo.overallPerformance ?? "_Overall Performance: {0}";
                SelectAvatarFirst = Holder.avatarInfo.selectAvatarFirst ?? "_Select an Avatar first";
            }
        }
        public static class Thumbnails
        {
            public static string OverlayCameraImage { get; internal set; }
            public static string OverlayTexture { get; internal set; }
            public static string StartUploadingFirst { get; internal set; }
            public static string CenterCameraOnViewpoint { get; internal set; }
            public static string BackgroundType { get; internal set; }
            public static string BackgroundType_None { get; internal set; }
            public static string BackgroundType_Material { get; internal set; }
            public static string BackgroundType_Color { get; internal set; }
            public static string HideOtherAvatars { get; internal set; }
            public static string BackgroundType_Image { get; internal set; }

            static Thumbnails()
            {
                Reload();
            }

            public static void Reload()
            {
                if(!Holder)
                    return;

                OverlayCameraImage = Holder.thumbnails.overlayCameraImage ?? "_Overlay Image";
                OverlayTexture = Holder.thumbnails.overlayTexture ?? "_Overlay Texture";
                StartUploadingFirst = Holder.thumbnails.startUploadingFirst ?? "_Start uploading an Avatar first";
                CenterCameraOnViewpoint = Holder.thumbnails.centerCameraOnViewpoint ?? "_Center Camera on Viewpoint";
                BackgroundType = Holder.thumbnails.backgroundType ?? "_Background Type";
                BackgroundType_None = Holder.thumbnails.backgroundType_None ?? "_None";
                BackgroundType_Material = Holder.thumbnails.backgroundType_Material ?? "_Material";
                BackgroundType_Color = Holder.thumbnails.backgroundType_Color ?? "_Color";
                BackgroundType_Image = Holder.thumbnails.backgroundType_Image ?? "_Image";
                HideOtherAvatars = Holder.thumbnails.hideOtherAvatars ?? "_Hide Other Avatars when Uploading";
            }
        }
        public static class Copier
        {
            public static string CopyFrom { get; internal set; }
            public static string CopyGameObjects { get; internal set; }
            public static string CopyColliderObjects { get; internal set; }
            public static string CopySettings { get; internal set; }
            public static string CreateMissing { get; internal set; }

            public static string Transforms { get; internal set; }
            public static string Transforms_position { get; internal set; }
            public static string Transforms_rotation { get; internal set; }
            public static string Transforms_scale { get; internal set; }
            public static string Transforms_avatarScale { get; internal set; }
            public static string DynamicBones { get; internal set; }
            public static string DynamicBones_colliders { get; internal set; }
            public static string DynamicBones_removeOldBones { get; internal set; }
            public static string DynamicBones_removeOldColliders { get; internal set; }
            public static string DynamicBones_createMissing { get; internal set; }
            public static string Colliders { get; internal set; }
            public static string Colliders_box { get; internal set; }
            public static string Colliders_capsule { get; internal set; }
            public static string Colliders_sphere { get; internal set; }
            public static string Colliders_mesh { get; internal set; }
            public static string Colliders_removeOld { get; internal set; }
            public static string Descriptor { get; internal set; }
            public static string Descriptor_pipelineId { get; internal set; }
            public static string Descriptor_animationOverrides { get; internal set; }
            public static string SkinMeshRender { get; internal set; }
            public static string SkinMeshRender_materials { get; internal set; }
            public static string SkinMeshRender_blendShapeValues { get; internal set; }
            public static string ParticleSystems { get; internal set; }
            public static string RigidBodies { get; internal set; }
            public static string TrailRenderers { get; internal set; }
            public static string EmptyGameObjects { get; internal set; }
            public static string MeshRenderers { get; internal set; }
            public static string Lights { get; internal set; }
            public static string Animators { get; internal set; }
            public static string CopyMainAnimator { get; internal set; }
            public static string ReplaceOld { get; internal set; }
            public static string Animators_inChildren { get; internal set; }
            public static string AudioSources { get; internal set; }
            public static string Joints { get; internal set; }
            public static string Exclusions { get; internal set; }
            public static string IncludeChildren { get; private set; }
            public static string Size { get; private set; }
            public static string Descriptor_copyViewpoint { get; internal set; }

            static Copier()
            {
                Reload();
            }

            public static void Reload()
            {
                if(!Holder)
                    return;

                CopyFrom = Holder.copier.copyFrom ?? "_Copy From";

                CopySettings = Holder.copier.copySettings ?? "_Settings";
                CreateMissing = Holder.copier.createMissing ?? "_Create Missing";
                EmptyGameObjects = Holder.copier.emptyGameObjects ?? "_Empty GameObjects";
                ReplaceOld = Holder.copier.replaceOld ?? "_Replace Old";

                Transforms = Holder.copier.transforms ?? "_Transforms";
                Transforms_position = Holder.copier.transforms_position ?? "_Position";
                Transforms_rotation = Holder.copier.transforms_rotation ?? "_Rotation";
                Transforms_scale = Holder.copier.transforms_scale ?? "_Scale";
                Transforms_avatarScale = Holder.copier.transforms_avatarScale ?? "_Avatar Scale";
                DynamicBones = Holder.copier.dynamicBones ?? "_Dynamic Bones";
                DynamicBones_colliders = Holder.copier.colliders ?? "_Colliders";
                DynamicBones_removeOldBones = Holder.copier.dynamicBones_removeOldBones ?? "_Remove Old Bones";
                DynamicBones_removeOldColliders = Holder.copier.dynamicBones_removeOldColliders ?? "_Remove Old Colliders";
                DynamicBones_createMissing = Holder.copier.createMissing ?? "_Create Missing Bones";
                Colliders = Holder.copier.colliders ?? "_Colliders";
                Colliders_box = Holder.copier.colliders_box ?? "_Box Colliders";
                Colliders_capsule = Holder.copier.colliders_capsule ?? "_Capsule Colliders";
                Colliders_sphere = Holder.copier.colliders_sphere ?? "_Sphere Colliders";
                Colliders_mesh = Holder.copier.colliders_mesh ?? "_Mesh Colliders";
                Colliders_removeOld = Holder.copier.colliders_removeOld ?? "_Remove Old Colliders";
                Descriptor = Holder.copier.descriptor ?? "_Avatar Descriptor";
                Descriptor_pipelineId = Holder.copier.descriptor_pipelineId ?? "_Pipeline Id";
                Descriptor_animationOverrides = Holder.copier.descriptor_animationOverrides ?? "_Animation Overrides";
                Descriptor_copyViewpoint = Holder.copier.descriptor_copyViewpoint ?? "_Viewpoint";
                SkinMeshRender = Holder.copier.skinMeshRender ?? "_Skinned Mesh Renderers";
                SkinMeshRender_materials = Holder.copier.skinMeshRender_materials ?? "_Materials";
                SkinMeshRender_blendShapeValues = Holder.copier.skinMeshRender_blendShapeValues ?? "_BlendShape Values";
                ParticleSystems = Holder.copier.particleSystems ?? "_Particle Systems";
                RigidBodies = Holder.copier.rigidBodies ?? "_Rigid Bodies";
                TrailRenderers = Holder.copier.trailRenderers ?? "_Trail Renderers";
                MeshRenderers = Holder.copier.meshRenderers ?? "_Mesh Renderers";
                CopyGameObjects = Holder.copier.copyGameObjects ?? "_Copy GameObjects";
                CopyColliderObjects = Holder.copier.copyColliderObjects ?? "_Copy Collider Objects";
                Lights = Holder.copier.lights ?? "_Lights";
                Animators = Holder.copier.animators ?? "_Animators";
                CopyMainAnimator = Holder.copier.copyMainAnimator ?? "_Copy Main Animator";
                Animators_inChildren = Holder.copier.animators_inChildren ?? "_Child Animators";
                AudioSources = Holder.copier.audioSources ?? "_Audio Sources";
                Joints = Holder.copier.joints ?? "_Joints";
                Exclusions = Holder.copier.exclusions ?? "_Exclusions";
                IncludeChildren = Holder.copier.includeChildren ?? "_Include Children";
                Size = Holder.copier.size ?? "_Size";
            }
        };
        public static class Log
        {
            public static string CopyAttempt { get; internal set; }
            public static string RemoveAttempt { get; internal set; }
            public static string CopyFromInvalid { get; internal set; }
            public static string Done { get; internal set; }
            public static string Failed { get; internal set; }
            public static string CantCopyToSelf { get; internal set; }
            public static string ViewpointApplied { get; internal set; }
            public static string ViewpointCancelled { get; internal set; }
            public static string Cancelled { get; internal set; }
            public static string NoSkinnedMeshFound { get; internal set; }
            public static string DescriptorIsNull { get; internal set; }
            public static string Success { get; internal set; }
            public static string TryFillVisemes { get; internal set; }            
            public static string MeshHasNoVisemes { get; internal set; }
            public static string FailedIsNull { get; internal set; }
            public static string NameIsEmpty { get; internal set; }
            public static string LoadedPose { get; internal set; }
            public static string LoadedBlendshapePreset { get; internal set; }
            public static string NothingSelected { get; internal set; }
            public static string FailedDoesntHave { get; internal set; }
            public static string FailedAlreadyHas { get; internal set; }
            public static string LoadedCameraOverlay { get; internal set; }
            public static string FailedHasNo { get; internal set; }

            static Log()
            {
                Reload();
            }

            public static void Reload()
            {
                if(!Holder)
                    return;

                Done = Holder.log.done ?? "_Done";
                Cancelled = Holder.log.cancelled ?? "_Cancelled";
                NothingSelected = Holder.log.nothingSelected ?? "_Select something first";
                CantCopyToSelf = Holder.log.cantCopyToSelf ?? "_Can't copy Components from an object to itself. What are you doing?";
                CopyAttempt = Holder.log.copyAttempt ?? "_Attempting to copy {0} from {1} to {2}";
                RemoveAttempt = Holder.log.removeAttempt ?? "_Attempting to remove {0} from {1}";
                CopyFromInvalid = Holder.log.copyFromInvalid ?? "_Can't copy Components because 'Copy From' is invalid";
                ViewpointApplied = Holder.log.viewpointApplied ?? "_Set Viewposition to {0}";
                ViewpointCancelled = Holder.log.viewpointCancelled ?? "_Cancelled Viewposition changes";
                TryFillVisemes = Holder.log.tryFillVisemes ?? "_Attempting to fill visemes on {0}";
                NoSkinnedMeshFound = Holder.log.noSkinnedMeshFound ?? "_Failed: No skinned mesh found";
                DescriptorIsNull = Holder.log.descriptorIsNull ?? "_Avatar descriptor is null";
                Success = Holder.log.success ?? "_Success";
                MeshHasNoVisemes = Holder.log.meshHasNoVisemes ?? "_Failed. Mesh has no Visemes. Set to Default";                
                Failed = Holder.log.failed ?? "_Failed";
                FailedIsNull = Holder.log.failedIsNull ?? "_Failed: {1} is null";
                NameIsEmpty = Holder.log.nameIsEmpty ?? "_Name is Empty";
                LoadedPose = Holder.log.loadedPose ?? "_Loaded Pose: {0}";
                LoadedBlendshapePreset = Holder.log.loadedBlendshapePreset ?? "_Loaded Blendshapes: {0}";
                FailedDoesntHave = Holder.log.failedDoesntHave ?? "_Failed: {0} doesn't have a {1}";
                FailedAlreadyHas = Holder.log.failedAlreadyHas ?? "_Failed: {1} already has {0}";
                LoadedCameraOverlay = Holder.log.loadedCameraOverlay ?? "_Loaded {0} as Camera Overlay";
                FailedHasNo = Holder.log.failedHasNo ?? "_{0} has no {1}, Ignoring.";
            }
        };
        public static class Warning
        {
            public static string Warn { get; internal set; }
            public static string NotFound { get; internal set; }
            public static string SelectSceneObject { get; internal set; }
            public static string OldVersion { get; internal set; }
            public static string VRCCamNotFound { get; internal set; }

            static Warning()
            {
                Reload();
            }

            public static void Reload()
            {
                if(!Holder)
                    return;

                Warn = Holder.warnings.warn ?? "_Warning";
                NotFound = Holder.warnings.notFound ?? "_(Not Found)";
                OldVersion = Holder.warnings.oldVersion ?? "_(Old Version)";
                SelectSceneObject = Holder.warnings.selectSceneObject ?? "_Please select an object from the scene";
                VRCCamNotFound = Holder.warnings.vrcCamNotFound ?? "_VRCCam not found";
            }
        };
        public static class Credits
        {            
            public static string Version { get; internal set; }
            public static string RedundantStrings { get; internal set; }
            public static string JsonDotNetCredit { get; internal set; }
            public static string AddMoreStuff { get; internal set; }
            public static string PokeOnDiscord { get; internal set; }

            static Credits()
            {
                Reload();
            }

            public static void Reload()
            {
                if(!Holder)
                    return;
                                
                Version = (Holder.credits.version + Instance.versionNr) ?? ("_Version" + " " + Instance.versionNr);
                RedundantStrings = Holder.credits.redundantStrings ?? "_Now with 100% more redundant strings";
                JsonDotNetCredit = Holder.credits.jsonDotNetCredit ?? "_JsonDotNet by Newtonsoft";
                AddMoreStuff = Holder.credits.addMoreStuff ?? "_I'll add more stuff to this eventually";
                PokeOnDiscord = Holder.credits.pokeOnDiscord ?? "_Poke me on Discord at Pumkin#2020";
            }
        };
        public static class Misc
        {
            public static string uwu { get; internal set; }
            public static string SearchForBones { get; internal set; }
            public static string SuperExperimental { get; internal set; }
            public static string Language { get; internal set; }

            private static string searchForBones;

            static Misc()
            {
                Reload();
            }

            public static void Reload()
            {
                if(!Holder)
                    return;

                uwu = Holder.misc.uwu ?? "_uwu";
                SearchForBones = Holder.misc.searchForBones ?? "_Search for DynamicBones";
                SuperExperimental = Holder.misc.superExperimental ?? "_Super Experimental Stuff";
                Language = Holder.misc.language ?? "_Language";
            }
        }

        public static class PoseEditor
        {
            public static string Title { get; internal set; }

            public static string Scene { get; internal set; }
            public static string SceneLoadAdditive { get; internal set; }
            public static string SceneOverrideLights { get; internal set; }

            public static string AvatarPosition { get; internal set; }
            public static string AvatarPositionOverridePose { get; internal set; }
            public static string AvatarPositionOverrideBlendshapes { get; internal set; }

            public static string SceneSaveChanges { get; internal set; }
            public static string UnloadScene { get; internal set; }
            public static string ResetPosition { get; internal set; }

            public static string Pose { get; internal set; }
            public static string NewPose { get; internal set; }
            public static string OnlySavePoseChanges { get; internal set; }
            public static string LoadPose { get; internal set; }

            public static string Blendshapes { get; internal set; }
            public static string NewPreset { get; internal set; }
            public static string LoadPreset { get; internal set; }

            public static string SaveButton { get; internal set; }
            public static string ReloadButton { get; internal set; }

            public static string BodyPositionYTooSmall { get; internal set; }

            static PoseEditor()
            {
                Reload();
            }

            public static void Reload()
            {
                if(!Holder)
                    return;

                Title = GetString("ui_poseEditor") ?? "_Pose Editor (Very Beta)";
                Scene = GetString("ui_poseEditor_scene") ?? "_Scene";
                SceneLoadAdditive = GetString("ui_poseEditor_scene_loadAdditive") ?? "_Load Additive";
                SceneOverrideLights = GetString("ui_poseEditor_scene_overrideLights") ?? "_Override Lights";
                AvatarPosition = GetString("ui_poseEditor_avatarPosition") ?? "_Avatar Position";
                AvatarPositionOverridePose = GetString("ui_poseEditor_avatarPosition_overridePose") ?? "_Override Pose";
                AvatarPositionOverrideBlendshapes = GetString("ui_poseEditor_avatarPositionOverrideBlendshapes") ?? "_Override Blendshapes";
                SceneSaveChanges = GetString("ui_poseEditor_scene_saveChanges") ?? "_Save Scene Changes";
                UnloadScene = GetString("ui_poseEditor_scene_unload") ?? "_Unload Scene";
                ResetPosition = GetString("ui_poseEditor_resetPosition") ?? "_Reset Position";
                Pose = GetString("ui_poseEditor_pose") ?? "_Pose";
                NewPose = GetString("ui_poseEditor_newPose") ?? "_New Pose";
                OnlySavePoseChanges = GetString("ui_poseEditor_onlySavePoseChanges") ?? "_Only Save Pose Changes";
                LoadPose = GetString("ui_poseEditor_loadPose") ?? "_Load Pose";
                Blendshapes = GetString("ui_poseEditor_blendshapes") ?? "_Blendshapes";
                NewPreset = GetString("ui_poseEditor_newPreset") ?? "_New Preset";
                LoadPreset = GetString("ui_poseEditor_loadPreset") ?? "_Load Preset";
                SaveButton = GetString("ui_poseEditor_save") ?? "_Save";
                ReloadButton = GetString("ui_poseEditor_reload") ?? "_Reload";
                BodyPositionYTooSmall = GetString("warn_poseEditor_bodyPositionYTooSmall") ?? "_humanPose.bodyPosition.y is {0}, you probably don't want that. Setting humanPose.bodyPosition.y to 1";
            }
        }

        static Strings()
        {
            ReloadStrings();
        }        

        static void ReloadStrings()
        {
            if(!Holder)
                return;

            Main.Reload();
            Buttons.Reload();
            Tools.Reload();
            Copier.Reload();
            Log.Reload();
            Warning.Reload();
            Credits.Reload();
            Misc.Reload();
            Thumbnails.Reload();
        }

        static string GetString(string stringName)
        {
            return null;
        }

        public static void SetLanguage(PumkinsLanguageHolder languageHolder)
        {
            Holder = languageHolder;
        }

        public static PumkinsLanguageHolder[] GetLanguages()
        {
            return Resources.FindObjectsOfTypeAll<PumkinsLanguageHolder>();
        }
    };

    public static class Colors
    {
        public static Color SceneGUIWindow { get; internal set; }
        public static Color DefaultCameraBackground { get; internal set; }
        public static Color DarkCameraBackground { get; internal set; }
        public static Color BallHandle { get; internal set; }
        public static Color LightLabelText { get; internal set; }

        static Colors()
        {
            SceneGUIWindow = new Color(0.3804f, 0.3804f, 0.3804f, 0.7f);
            DefaultCameraBackground = new Color(0.192f, 0.302f, 0.475f);
            DarkCameraBackground = new Color(0.235f, 0.22f, 0.22f);
            BallHandle = new Color(1, 0.92f, 0.016f, 0.5f);
            LightLabelText = Color.white;
        }
    }

    public static class Styles
    {
        public static GUIStyle Foldout_title { get; internal set; }
        public static GUIStyle Label_mainTitle { get; internal set; }
        public static GUIStyle Label_centered { get; internal set; }
        public static GUIStyle Editor_line { get; internal set; }
        public static GUIStyle Label_rightAligned { get; internal set; }
        public static GUIStyle Foldout { get; internal set; }
        public static GUIStyle HelpBox { get; internal set; }
        public static GUIStyle HelpBox_OneLine { get; internal set; }
        public static GUIStyle Box { get; internal set; }
        public static GUIStyle BigButton { get; internal set; }
        public static GUIStyle LightTextField { get; internal set; }

        static Styles()
        {
            BigButton = new GUIStyle("Button")
            {
                fixedHeight = 28f,
                stretchHeight = false,
            };
            Foldout_title = new GUIStyle("Foldout")
            {
                fontStyle = FontStyle.Bold,
                fontSize = 12,
            };

            Label_mainTitle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontStyle = FontStyle.Bold,
                fontSize = 14,
            };

            Label_centered = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.UpperCenter,
            };

            Label_rightAligned = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleRight,
                stretchWidth = true,
            };

            Editor_line = new GUIStyle("box")
            {
                border = new RectOffset(1, 1, 1, 1),
                margin = new RectOffset(5, 5, 1, 1),
                padding = new RectOffset(1, 1, 1, 1),
            };

            HelpBox_OneLine = new GUIStyle("HelpBox")
            {
                fixedHeight = 17,
                stretchHeight = false,
            };

            LightTextField = new GUIStyle("TextField");
            LightTextField.normal.textColor = Color.white;

            Foldout = new GUIStyle("Foldout");
            HelpBox = new GUIStyle("HelpBox");
            Box = new GUIStyle("box");
        }
    }

    public struct Icons
    {

        public static Texture2D Star { get; internal set; }
        public static Texture2D CsScript { get; internal set; }
        public static Texture2D Transform { get; internal set; }
        public static Texture2D Avatar { get; internal set; }
        public static Texture2D SkinnedMeshRenderer { get; internal set; }
        public static Texture2D ColliderBox { get; internal set; }
        public static Texture2D DefaultAsset { get; internal set; }
        public static Texture2D Help { get; internal set; }
        public static Texture2D ParticleSystem { get; internal set; }
        public static Texture2D RigidBody { get; internal set; }
        public static Texture2D Prefab { get; internal set; }
        public static Texture2D TrailRenderer { get; internal set; }
        public static Texture2D BoneIcon { get; internal set; }
        public static Texture2D BoneColliderIcon { get; internal set; }
        public static Texture2D MeshRenderer { get; internal set; }
        public static Texture2D Light { get; internal set; }
        public static Texture2D Animator { get; internal set; }
        public static Texture2D AudioSource { get; internal set; }
        public static Texture2D Joint { get; internal set; }

        public static Texture2D DiscordIcon { get; internal set; }
        public static Texture2D GithubIcon { get; internal set; }
        public static Texture2D KofiIcon { get; internal set; }

        static Icons()
        {
#if UNITY_2017
            Star = EditorGUIUtility.FindTexture("Favorite Icon");
            CsScript = EditorGUIUtility.FindTexture("cs Script Icon");
            Transform = EditorGUIUtility.FindTexture("Transform Icon");
            Avatar = EditorGUIUtility.FindTexture("Avatar Icon");
            SkinnedMeshRenderer = EditorGUIUtility.FindTexture("SkinnedMeshRenderer Icon");
            ColliderBox = EditorGUIUtility.FindTexture("BoxCollider Icon");
            DefaultAsset = EditorGUIUtility.FindTexture("DefaultAsset Icon");
            Help = EditorGUIUtility.FindTexture("_Help");
            ParticleSystem = EditorGUIUtility.FindTexture("ParticleSystem Icon");
            RigidBody = EditorGUIUtility.FindTexture("Rigidbody Icon");
            Prefab = EditorGUIUtility.FindTexture("Prefab Icon");
            TrailRenderer = EditorGUIUtility.FindTexture("TrailRenderer Icon");
            MeshRenderer = EditorGUIUtility.FindTexture("MeshRenderer Icon");
            Light = EditorGUIUtility.FindTexture("Light Icon");
            Animator = EditorGUIUtility.FindTexture("Animator Icon");
            AudioSource = EditorGUIUtility.FindTexture("AudioSource Icon");
            Joint = EditorGUIUtility.FindTexture("FixedJoint Icon");

            BoneIcon = Resources.Load("icons/bone-icon") as Texture2D ?? CsScript;
            BoneColliderIcon = Resources.Load("icons/bonecollider-icon") as Texture2D ?? DefaultAsset;
            DiscordIcon = Resources.Load("icons/discord-logo") as Texture2D ?? Star;
            GithubIcon = Resources.Load("icons/github-logo") as Texture2D ?? Star;
            KofiIcon = Resources.Load("icons/kofi-logo") as Texture2D ?? Star;
#else
            Star = (Texture2D)EditorGUIUtility.IconContent("Favorite Icon").image;
            CsScript = (Texture2D)EditorGUIUtility.IconContent("cs Script Icon").image;
            Transform = (Texture2D)EditorGUIUtility.IconContent("Transform Icon").image;
            Avatar = (Texture2D)EditorGUIUtility.IconContent("Avatar Icon").image;
            SkinnedMeshRenderer = (Texture2D)EditorGUIUtility.IconContent("SkinnedMeshRenderer Icon").image;
            ColliderBox = (Texture2D)EditorGUIUtility.IconContent("BoxCollider Icon").image;
            DefaultAsset = (Texture2D)EditorGUIUtility.IconContent("DefaultAsset Icon").image;
            Help = (Texture2D)EditorGUIUtility.IconContent("_Help").image;
            ParticleSystem = (Texture2D)EditorGUIUtility.IconContent("ParticleSystem Icon").image;
            RigidBody = (Texture2D)EditorGUIUtility.IconContent("Rigidbody Icon").image;
            Prefab = (Texture2D)EditorGUIUtility.IconContent("Prefab Icon").image;
            TrailRenderer = (Texture2D)EditorGUIUtility.IconContent("TrailRenderer Icon").image;
            MeshRenderer = (Texture2D)EditorGUIUtility.IconContent("MeshRenderer Icon").image;
            Light = (Texture2D)EditorGUIUtility.IconContent("Light Icon").image;
            Animator = (Texture2D)EditorGUIUtility.IconContent("Animator Icon").image;
            AudioSource = (Texture2D)EditorGUIUtility.IconContent("AudioSource Icon").image;
            Joint = (Texture2D)EditorGUIUtility.IconContent("FixedJoint Icon").image;

            BoneIcon = Resources.Load("icons/bone-icon") as Texture2D ?? CsScript;
            BoneColliderIcon = Resources.Load("icons/bonecollider-icon") as Texture2D ?? DefaultAsset;
            DiscordIcon = Resources.Load("icons/discord-logo") as Texture2D ?? Star;
            GithubIcon = Resources.Load("icons/github-logo") as Texture2D ?? Star;
            KofiIcon = Resources.Load("icons/kofi-logo") as Texture2D ?? Star;        
#endif
        }

    }

    public class AvatarInfo
    {
        AvatarPerformanceStats perfStats = new AvatarPerformanceStats();

        public string Name { get; private set; }
        public string CachedInfo { get; private set; }

        public int SkinnedMeshRenderers { get; private set; }
        public int SkinnedMeshRenderers_Total { get; private set; }
        public int MeshRenderers { get; private set; }
        public int MeshRenderers_Total { get; private set; }
        public int DynamicBoneTransforms { get; private set; }
        public int DynamicBoneTransforms_Total { get; private set; }
        public int DynamicBoneColliders { get; private set; }
        public int DynamicBoneColliders_Total { get; private set; }
        public int DynamicBoneColliderTransforms { get; private set; }
        public int DynamicBoneColliderTransforms_Total { get; private set; }
        public int Polygons { get; private set; }
        public int Polygons_Total { get; private set; }
        public int MaterialSlots { get; private set; }
        public int MaterialSlots_Total { get; private set; }
        public int UniqueMaterials { get; private set; }
        public int UniqueMaterials_Total { get; private set; }
        public int ShaderCount { get; private set; }
        public int ParticleSystems { get; private set; }
        public int ParticleSystems_Total { get; private set; }
        public int GameObjects { get; private set; }
        public int GameObjects_Total { get; private set; }
        public int MaxParticles { get; private set; }
        public int MaxParticles_Total { get; private set; }

        AvatarInfo()
        {
            CachedInfo = null;

            SkinnedMeshRenderers = 0;
            SkinnedMeshRenderers_Total = 0;

            MeshRenderers = 0;
            MeshRenderers_Total = 0;

            DynamicBoneTransforms = 0;
            DynamicBoneTransforms_Total = 0;
            DynamicBoneColliders = 0;
            DynamicBoneColliders_Total = 0;
            DynamicBoneColliderTransforms = 0;
            DynamicBoneColliderTransforms_Total = 0;

            Polygons = 0;
            Polygons_Total = 0;
            MaterialSlots = 0;
            MaterialSlots_Total = 0;
            UniqueMaterials = 0;
            UniqueMaterials_Total = 0;
            ShaderCount = 0;

            ParticleSystems = 0;
            ParticleSystems_Total = 0;
            MaxParticles = 0;
            MaxParticles_Total = 0;

            GameObjects = 0;
            GameObjects_Total = 0;

        }

        public AvatarInfo(GameObject o) : base()
        {
            if(o == null)
                return;

            AvatarPerformance.CalculatePerformanceStats(o.name, o, perfStats);

            Name = o.name;

            var shaderHash = new HashSet<Shader>();
            var matList = new List<Material>();
            var matList_total = new List<Material>();

            var ts = o.GetComponentsInChildren<Transform>(true);
            foreach(var t in ts)
            {
                GameObjects_Total += 1;
                if(t.gameObject.activeInHierarchy)
                    GameObjects += 1;
            }

            var sRenders = o.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            foreach(var r in sRenders)
            {
                SkinnedMeshRenderers_Total += 1;
                Polygons_Total += r.sharedMesh.triangles.Length;
                //Triangles_Total += r.sharedMesh.triangles.Length / 3;

                if(r.gameObject.activeInHierarchy && r.enabled)
                {
                    SkinnedMeshRenderers += 1;
                    Polygons += r.sharedMesh.triangles.Length;
                    //Triangles += r.sharedMesh.triangles.Length/3;
                }

                foreach(var mat in r.sharedMaterials)
                {
                    if(mat != null)
                    {
                        shaderHash.Add(mat.shader);
                        matList_total.Add(mat);

                        if(r.gameObject.activeInHierarchy && r.enabled)
                        {
                            matList.Add(mat);
                        }
                    }
                }
            }

            var renders = o.GetComponentsInChildren<MeshRenderer>(true);
            foreach(var r in renders)
            {
                var filter = r.GetComponent<MeshFilter>();

                if(filter != null && filter.sharedMesh != null)
                {
                    MeshRenderers_Total += 1;
                    Polygons_Total += filter.sharedMesh.triangles.Length;

                    if(r.gameObject.activeInHierarchy && r.enabled)
                    {
                        MeshRenderers += 1;
                        Polygons += filter.sharedMesh.triangles.Length;
                    }
                }

                foreach(var mat in r.sharedMaterials)
                {
                    if(mat != null)
                    {
                        shaderHash.Add(mat.shader);
                        matList_total.Add(mat);

                        if(r.gameObject.activeInHierarchy && r.enabled)
                        {
                            matList.Add(mat);
                        }
                    }
                }
            }

            MaterialSlots = matList.Count;
            MaterialSlots_Total = matList_total.Count;

            UniqueMaterials = new HashSet<Material>(matList).Count;
            UniqueMaterials_Total = new HashSet<Material>(matList_total).Count;

#if BONES || OLD_BONES

            var dbColliders = o.GetComponentsInChildren<DynamicBoneCollider>(true);
            foreach(var c in dbColliders)
            {
                DynamicBoneColliders_Total += 1;

                if(c.gameObject.activeInHierarchy)
                    DynamicBoneColliders += 1;
            }

            var dbones = o.GetComponentsInChildren<DynamicBone>(true);
            foreach(var d in dbones)
            {
                if(d.m_Root != null)
                {
                    var exclusions = d.m_Exclusions;
                    var rootChildren = d.m_Root.GetComponentsInChildren<Transform>(true);

                    int affected = 0;
                    int affected_total = 0;

                    foreach(var t in rootChildren)
                    {
                        if(exclusions.IndexOf(t) == -1)
                        {
                            affected_total += 1;

                            if(t.gameObject.activeInHierarchy && d.enabled)
                            {
                                affected += 1;
                            }
                        }
                        else
                        {
                            var childChildren = t.GetComponentsInChildren<Transform>(true);

                            for(int z = 1; z < childChildren.Length; z++)
                            {
                                affected_total -= 1;

                                if(childChildren[z].gameObject.activeInHierarchy && d.enabled)
                                {
                                    affected -= 1;
                                }
                            }
                        }
                    }

                    foreach(var c in d.m_Colliders)
                    {
                        if(c != null)
                        {
                            DynamicBoneColliderTransforms += affected;
                            DynamicBoneColliderTransforms_Total += affected_total;
                            break;
                        }
                    }

                    DynamicBoneTransforms += affected;
                    DynamicBoneTransforms_Total += affected_total;
                }
            }

#endif

            var ptc = o.GetComponentsInChildren<ParticleSystem>(true);
            foreach(var p in ptc)
            {
                ParticleSystems_Total += 1;
                MaxParticles_Total += p.main.maxParticles;

                if(p.gameObject.activeInHierarchy && p.emission.enabled)
                {
                    ParticleSystems += 1;
                    MaxParticles += p.main.maxParticles;
                }
            }

            ShaderCount = shaderHash.Count;
        }

        public static AvatarInfo GetInfo(GameObject o, out string toString)
        {
            AvatarInfo a = new AvatarInfo(o);
            toString = a.ToString();
            return a;
        }

        public override string ToString()
        {
            if(!string.IsNullOrEmpty(CachedInfo))
                return CachedInfo;
            else
            {
                if(this == null)
                {
                    return null;
                }
                try
                {
                    CachedInfo =
                    string.Format(Strings.AvatarInfo.Name, Name) + "\n" +
                    string.Format(Strings.AvatarInfo.Line) + "\n" +
                    string.Format(Strings.AvatarInfo.GameObjects, GameObjects, GameObjects_Total) + "\n" +
                    string.Format(Strings.AvatarInfo.Bones, perfStats.boneCount, perfStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.BoneCount)) + "\n\n" +
                    string.Format(Strings.AvatarInfo.SkinnedMeshRenderers, SkinnedMeshRenderers, SkinnedMeshRenderers_Total, perfStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.SkinnedMeshCount)) + "\n" +
                    string.Format(Strings.AvatarInfo.MeshRenderers, MeshRenderers, MeshRenderers_Total, perfStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.MeshCount)) + "\n" +
                    string.Format(Strings.AvatarInfo.Polygons, Polygons, Polygons_Total, perfStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.PolyCount)) + "\n\n" +
                    string.Format(Strings.AvatarInfo.UsedMaterialSlots, MaterialSlots, MaterialSlots_Total, perfStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.MaterialCount)) + "\n" +
                    string.Format(Strings.AvatarInfo.UniqueMaterials, UniqueMaterials, UniqueMaterials_Total) + "\n" +
                    string.Format(Strings.AvatarInfo.Shaders, ShaderCount) + "\n\n" +
                    string.Format(Strings.AvatarInfo.DynamicBoneTransforms, DynamicBoneTransforms, DynamicBoneTransforms_Total, perfStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.DynamicBoneSimulatedBoneCount)) + "\n" +
                    string.Format(Strings.AvatarInfo.DynamicBoneColliders, DynamicBoneColliders, DynamicBoneColliders_Total, perfStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.DynamicBoneColliderCount)) + "\n" +
                    string.Format(Strings.AvatarInfo.DynamicBoneColliderTransforms, DynamicBoneColliderTransforms, DynamicBoneColliderTransforms_Total, perfStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.DynamicBoneCollisionCheckCount)) + "\n\n" +
                    string.Format(Strings.AvatarInfo.ParticleSystems, ParticleSystems, ParticleSystems_Total, perfStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.ParticleSystemCount)) + "\n" +
                    string.Format(Strings.AvatarInfo.MaxParticles, MaxParticles, MaxParticles_Total, perfStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.ParticleTotalCount)) + "\n" +
                    Strings.AvatarInfo.Line + "\n" +
                    string.Format(Strings.AvatarInfo.OverallPerformance, perfStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.Overall));
                }
                catch(Exception)
                {
                    CachedInfo = null;
                }
                return CachedInfo;
            }
        }

        public static bool operator true(AvatarInfo x) { return x != null; }
        public static bool operator false(AvatarInfo x) { return !(x == null); }
    }

    [Serializable]
    public class CameraPreset
    {
        string presetName;
        SerialVector3 positionOffset;
        SerialQuaternion rotationOffset;
        SerialVector3 anglesOffset;

        string transformPath;

        Color backgroundColor;
        string overlayPath;
        Material skyboxMaterial;

        public CameraPreset(string name, GameObject focusObject, Camera cam)
        {
            Transform tFocus = focusObject.transform;
            Transform tCam = cam.transform;

            Transform tDummy = new GameObject("_FocusDummy").transform;

            tDummy.SetPositionAndRotation(tFocus.position, tFocus.rotation);



        }

        public CameraPreset(string name, Vector3 posOffset, Quaternion rotOffset, Vector3 angleOffset, string transPath, Color bgColor, string overlayPath = null)
        {
            this.positionOffset = posOffset;
            this.rotationOffset = rotOffset;
            this.anglesOffset = angleOffset;
            this.backgroundColor = bgColor;
            this.overlayPath = overlayPath;
            this.presetName = name;
        }

        void ApplyPreset(Camera camera)
        {
            GameObject avatar = null;
            VRC_AvatarDescriptor desc = null;
            Camera cam = PumkinsAvatarTools.VRCCam;

            if(EditorApplication.isPlaying)
            {
                var pm = GameObject.FindObjectOfType<RuntimeBlueprintCreation>();
                if(pm == null)
                {
                    Debug.Log("_RuntimeBlueprintCreation script not found. Start uploading an avatar to use this");
                    return;
                }
                else if(pm.pipelineManager.contentType == PipelineManager.ContentType.world)
                {
                    Debug.Log("_You must be uploading an avatar, not a world to use this.");
                    return;
                }
                desc = pm.pipelineManager.GetComponent<VRC_AvatarDescriptor>();
                avatar = desc.transform.root.gameObject;
            }
            else if((avatar = PumkinsAvatarTools.selectedAvatar) == null)
            {
                Debug.Log("_Begin uploading an avatar or select one manually before using this.");
                return;
            }

            Transform focusObj = avatar.transform.Find(transformPath);
            if(focusObj == null)
            {
                PumkinsAvatarTools.Log("_GameObject {0} not found in {1}. Ignoring", LogType.Warning, Helpers.GetNameFromPath(transformPath), avatar.name);
                return;
            }

            cam.transform.parent = focusObj;
            cam.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

            cam.transform.position = positionOffset;
            cam.transform.rotation = rotationOffset;
            cam.transform.eulerAngles = anglesOffset;

            cam.transform.parent = null;
        }

        bool SavePreset(Camera cam, string path)
        {
            return true;
        }
    }

    /// <summary>
    /// Serializable Transform class
    /// </summary>
    [Serializable]
    public class SerialTransform
    {
        public SerialVector3 position, localPosition, scale, localScale, eulerAngles, localEulerAngles;
        public SerialQuaternion rotation, localRotation;

        SerialTransform()
        {
            this.position = Vector3.zero;
            this.rotation = Quaternion.identity;
            this.scale = Vector3.one;
            this.localEulerAngles = Vector3.zero;
            this.localPosition = Vector3.zero;
            this.localRotation = Quaternion.identity;
            this.localScale = Vector3.one;
        }

        [JsonConstructor]
        public SerialTransform(Vector3 position, Quaternion rotation, Vector3 scale, Vector3 eulerAngles, Vector3 localEulerAngles, Vector3 localScale, Vector3 localPosition, Quaternion localRotation, float version) : base()
        {
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
            this.localEulerAngles = localEulerAngles;
            this.localPosition = localPosition;
            this.localRotation = localRotation;
            this.localScale = localScale;
        }

        public SerialTransform(Vector3 localPosition, Quaternion localRotation, Vector3 localScale, Vector3 localEulerAngles) : base()
        {
            this.localPosition = localPosition;
            this.localRotation = localRotation;
            this.localScale = localScale;
            this.localEulerAngles = localEulerAngles;
        }

        public SerialTransform(Quaternion localRotation) : base()
        {
            this.localRotation = localRotation;
        }

        public SerialTransform(Transform t) : base()
        {
            position = t.position;
            localPosition = t.localPosition;

            scale = t.localScale;
            localScale = t.localScale;

            rotation = t.rotation;
            localRotation = t.localRotation;

            eulerAngles = t.eulerAngles;

            if(localEulerAngles != null)
                localEulerAngles = t.localEulerAngles;
        }

        public static implicit operator SerialTransform(Transform t)
        {
            return new SerialTransform(t);
        }
    }

    /// <summary>
    /// Serializable Quaternion class
    /// </summary>
    [Serializable]
    public class SerialQuaternion
    {
        public float x, y, z, w;

        public SerialQuaternion(Quaternion q)
        {
            x = q.x;
            y = q.y;
            z = q.z;
            w = q.w;
        }

        private SerialQuaternion(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public static implicit operator SerialQuaternion(Quaternion q)
        {
            return new SerialQuaternion(q);
        }

        public static implicit operator Quaternion(SerialQuaternion q)
        {
            return new Quaternion(q.x, q.y, q.z, q.w);
        }
    }

    /// <summary>
    /// Serializable Vector3 class
    /// </summary>
    [Serializable]
    public class SerialVector3
    {
        public float x, y ,z;

        public SerialVector3(Vector3 v)
        {
            x = v.x;
            y = v.y;
            z = v.z;
        }

        public SerialVector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static implicit operator SerialVector3(Vector3 v)
        {
            return new SerialVector3(v);
        }

        public static implicit operator Vector3(SerialVector3 v)
        {
            return new Vector3(v.x, v.y, v.z);
        }

        public static SerialVector3 operator *(SerialVector3 v, float f)
        {
            return new SerialVector3(new Vector3(v.x, v.y, v.z) * f);
        }
    }

    /// <summary>
    /// This might be much better than the transform-based PosePreset
    /// </summary>
    [Serializable]
    public class HumanPosePreset
    {
        public string poseName;
        public Vector3 bodyPosition;
        public Quaternion bodyRotation;
        public Vector3 rootPosition;
        public float[] muscles;

        public HumanPosePreset(string poseName, Vector3 rootPosition, HumanPose p)
        {
            this.poseName = poseName;
            bodyPosition = p.bodyPosition;
            bodyRotation = p.bodyRotation;
            muscles = p.muscles;
            this.rootPosition = rootPosition;
        }

        public HumanPosePreset(string poseName, Vector3 bodyPosition, Quaternion bodyRotation, Vector3 rootPosition, float[] muscles)
        {
            this.poseName = poseName;
            this.rootPosition = rootPosition;
            this.bodyPosition = bodyPosition;
            this.bodyRotation = bodyRotation;
            this.muscles = muscles;
        }

        internal bool ApplyPose(GameObject avatar)
        {
            Animator anim = avatar.GetComponent<Animator>();
            if(anim == null || !anim.isHuman)
                return false;

            HumanPoseHandler hph = new HumanPoseHandler(anim.avatar, avatar.transform);
            HumanPose hp = this;
            hph.SetHumanPose(ref hp);

            return true;
        }

        public static implicit operator HumanPose(HumanPosePreset v)
        {
            HumanPose hp = new HumanPose();
            hp.bodyPosition = v.bodyPosition;
            hp.bodyRotation = v.bodyRotation;
            hp.muscles = v.muscles;

            return hp;
        }
    }

    /// <summary>
    /// Serializable Pose preset, used to store avatar transforms associated to a name
    /// </summary>
    [Serializable]
    public class PosePreset
    {
        public string poseName;
        public Dictionary<string, SerialTransform> transforms;

        public PosePreset(string poseName, Dictionary<string, SerialTransform> transformSettings = null)
        {
            this.poseName = poseName;
            this.transforms = transformSettings;
        }

        /// <summary>
        /// Adds or overwrites a transform to the pose dictionary.
        /// </summary>
        /// <param name="path">Path of the transform</param>
        /// <param name="transform">Transform for positoin and rotation settings</param>
        /// <param name="allowOverwrite">Whether to overwrite settings if transform exists or not</param>
        /// <returns>Returns true if successfully added or overwritten</returns>
        public bool AddTransform(string path, Transform transform, bool allowOverwrite = true)
        {
            if(transforms.ContainsKey(path))
            {
                if(allowOverwrite)
                    transforms[path] = transform;
                else
                    return false;
            }
            else
            {
                transforms.Add(path, transform);
            }
            return true;
        }

        /// <summary>
        /// Serialize and save pose to file
        /// </summary>
        /// <param name="filePath">Path to folder where to save the file to, excluding filename</param>
        /// <param name="overwriteExisting">Overwrite file if one with the same name already exists, if not will add a number to the end and create new file</param>
        /// <returns></returns>
        public bool SaveToFile(string filePath, bool overwriteExisting)
        {
            if(transforms == null)
                transforms = new Dictionary<string, SerialTransform>();

            string path = filePath + "/" + poseName + '.' + PumkinsPoseEditor.poseExtension;

            if(!overwriteExisting)
                path = Helpers.NextAvailableFilename(filePath + "/" + poseName + '.' + PumkinsPoseEditor.poseExtension);

            string json = JsonConvert.SerializeObject(this, Formatting.Indented);

            if(!string.IsNullOrEmpty(json))
            {
                File.WriteAllText(path, json);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Don't hurt me, I need this for a default failsafe or two.
        /// </summary>
        /// <returns>C# code to declare and initialize this object</returns>
        public string ToHardcodedString()
        {
            if(transforms == null)
                transforms = new Dictionary<string, SerialTransform>();

            string s = string.Format("new PosePreset(\"{0}\", new Dictionary<string, SerialTransform>\n\t", poseName);
            s += "{\n";
            foreach(var t in transforms)
            {
                s += "\t\t{";
                s += string.Format("\"{0}\", new SerialTransform(new Quaternion({1}f, {2}f, {3}f, {4}f))", t.Key, t.Value.localRotation.x, t.Value.localRotation.y, t.Value.localRotation.z, t.Value.localRotation.w);
                s += "},\n";
            }
            s += "}),\n";

            return s;
        }

        /// <summary>
        /// Apply this pose to avatar
        /// </summary>        
        public void ApplyPose(GameObject avatar)//, bool childrenFirst = false)
        {
            if(!avatar)
                return;

            foreach(var kv in transforms)
            {
                var t = avatar.transform.Find(kv.Key);

                if(t != null)
                {
                    t.localEulerAngles = kv.Value.localEulerAngles;
                    t.localRotation = kv.Value.localRotation;
                }
            }            
        }
    }

    [Serializable]
    public class BlendshapePreset
    {
        public string presetName;
        public Dictionary<string, List<PoseBlendshape>> blendshapes;

        public BlendshapePreset(string presetName, Dictionary<string, List<PoseBlendshape>> blendshapes = null)
        {
            this.presetName = presetName;
            this.blendshapes = blendshapes;

            if(blendshapes == null)
                blendshapes = new Dictionary<string, List<PoseBlendshape>>();
        }

        public bool AddBlendshape(string meshRendererPath, PoseBlendshape blend)
        {
            var d = blendshapes[meshRendererPath];

            if(d != null && d.Count > 0 && !d.Exists(x => x.Name.ToLower() == blend.Name.ToLower()))
            {
                d.Add(blend);
                return true;
            }
            return false;
        }

        public bool RemoveBlendshape(string meshRendererPath, string shapeName)
        {
            var d = blendshapes[meshRendererPath];
            var b = d.Find(x => x.Name.ToLower() == shapeName.ToLower());

            if(d != null && d.Count > 0 && b != null)
            {
                d.Remove(b);
                return true;
            }
            return false;
        }

        public bool SaveToFile(string filePath, bool overwriteExisting)
        {
            string path = filePath + "/" + presetName + '.' + PumkinsPoseEditor.blendshapeExtension;

            if(!overwriteExisting)
                path = Helpers.NextAvailableFilename(filePath + "/" + presetName + '.' + PumkinsPoseEditor.blendshapeExtension);

            string json = JsonConvert.SerializeObject(this, Formatting.Indented);

            if(!string.IsNullOrEmpty(json))
            {
                File.WriteAllText(path, json);
                return true;
            }
            return false;
        }

        public void ApplyBlendshapes(GameObject avatar)
        {
            if(!avatar)
                return;

            foreach(var b in blendshapes)
            {
                var t = avatar.transform.Find(b.Key);
                if(t)
                {
                    var sr = t.GetComponent<SkinnedMeshRenderer>();
                    if(sr)
                    {
                        foreach(var shape in b.Value)
                        {
                            int index = sr.sharedMesh.GetBlendShapeIndex(shape.Name);

                            if(shape.AlternateNames.Count > 0)
                            {
                                for(int i = 0; index == -1 && i < shape.AlternateNames.Count; i++)
                                {
                                    index = sr.sharedMesh.GetBlendShapeIndex(shape.AlternateNames[i]);
                                }
                            }

                            if(index != -1)
                            {
                                sr.SetBlendShapeWeight(index, shape.Weight);
                            }
                        }
                    }
                }
            }
        }
    }

    [Serializable]
    public class PoseBlendshape
    {
        public string FriendlyName
        {
            get
            {
                return friendlyName;
            }
            set
            {
                if(string.IsNullOrEmpty(value))
                    friendlyName = Name;
            }
        }

        public string Name
        {
            get; set;
        }

        public float Weight
        {
            get; set;
        }

        public List<string> AlternateNames
        {
            get; set;
        }

        string friendlyName;

        public PoseBlendshape(string name, float weight = 0, string friendlyName = null, List<string> alternateNames = null)
        {
            Name = name;
            Weight = weight;

            FriendlyName = friendlyName;
            if(alternateNames != null)
                AlternateNames = alternateNames;
            else
                AlternateNames = new List<string>();
        }
    }
}