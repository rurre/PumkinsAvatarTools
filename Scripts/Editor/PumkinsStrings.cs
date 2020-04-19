using Pumkin.AvatarTools;
using Pumkin.Translations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Pumkin.DataStructures
{
    [ExecuteInEditMode, InitializeOnLoad] //needed for string singleton
    public class Strings : SingletonScriptableObject<Strings>
    {
#if PUMKIN_VRCSDK1
        public const string TOOLS_VERSION_STRING = "0.7.2b - Old SDK";
#else
        public const string TOOLS_VERSION_STRING = "0.7.2b - Work in Progress";
#endif        
        public const double toolsVersion = 0.72;

        public const string POSE_EDITOR_VERSION_NUMBER = "0.1.1b - Work in Progress";
        public const string LINK_GITHUB = "https://github.com/rurre/PumkinsAvatarTools/";
        public const string LINK_DONATION = "https://ko-fi.com/notpumkin";
        public const string LINK_DISCORD = "https://discord.gg/7vyekJv";

        static PumkinsTranslation _translationHolder;
        public static PumkinsTranslation Translation
        {
            get
            {
                return _translationHolder;
            }
            set
            {
                _translationHolder = value;
                LanguageName = value.languageName;
                Author = value.author;
                ReloadStrings();
            }
        }

        public static string LanguageName
        {
            get;
            private set;
        }
        public static string Author
        {
            get; private set;
        }

        public static class Main
        {
            public static string title = "Pumkin's Avatar Tools";
            public static string windowName = "Pumkin Tools";
            public static string version = "_Version";
            public static string avatar = "_Avatar";
            public static string tools = "_Tools";
            public static string copier = "_Copier";
            public static string removeAll = "_Remove All";
            public static string avatarInfo = "_Avatar Info";
            public static string thumbnails = "_Thumbnails";

            public static string misc = "_Misc";
            public static string useSceneSelection = "_Use Scene Selection";

            static Main()
            {
                Reload();
            }

            public static void Reload()
            {
                if(Translation is null)
                    return;

                avatar = Translation.main.avatar;
                title = Translation.main.title;
                version = Translation.main.version;
                windowName = Translation.main.windowName;
                tools = Translation.main.tools;
                copier = Translation.main.copier;
                avatarInfo = Translation.main.avatarInfo;
                removeAll = Translation.main.removeAll;
                misc = Translation.main.misc;
                thumbnails = Translation.main.thumbnails;
                useSceneSelection = Translation.main.useSceneSelection;
            }
        };
        public static class Buttons
        {
            public static string selectFromScene = "_Select from Scene";
            public static string copySelected = "_Copy Selected";
            public static string refresh = "_Refresh";
            public static string cancel = "_Cancel";
            public static string apply = "_Apply";
            public static string copy = "_Copy Text";
            public static string openHelpPage = "_Open Help Page";
            public static string openGithubPage = "_Open Github Page";
            public static string openDonationPage = "_Buy me a Ko-Fi~";
            public static string openPoseEditor = "_Open Pose Editor";
            public static string joinDiscordServer = "_Join Discord Server!";
            public static string selectNone = "_Select None";
            public static string selectAll = "_Select All";
            public static string browse = "_Browse";
            public static string setFromCamera = "_Set from Camera";
            public static string reset = "_Reset";
            public static string edit = "_Edit";
            public static string load = "_Load";
            public static string createNewPreset = "_Create New Preset";
            public static string quickSetupAvatar = "_Quick Setup Avatar";
            public static string selectInToolsWindow  = "_Select in Tools Window";
            public static string resetRenderer = "_Reset Renderer";
            public static string revertRenderer = "_Revert Renderer";
            public static string alignCameraToView = "_Align Camera to View";
            public static string savePreset = "_Save Preset";
            public static string selectInAssets = "_Select in Assets";
            public static string openFolder = "_Open Folder";
            public static string ok = "_Ok";

            static Buttons()
            {
                Reload();
            }

            public static void Reload()
            {
                if(Translation is null)
                    return;

                selectFromScene = Translation.buttons.selectFromScene;
                copySelected = Translation.buttons.copySelected;
                refresh = Translation.buttons.refresh;
                cancel = Translation.buttons.cancel;
                apply = Translation.buttons.apply;
                copy = Translation.buttons.copy;
                openHelpPage = Translation.buttons.openHelpPage;
                openGithubPage = Translation.buttons.openGithubPage;
                openDonationPage = Translation.buttons.openDonationPage;
                openPoseEditor = Translation.buttons.openPoseEditor;
                joinDiscordServer = Translation.buttons.joinDiscordServer;
                selectNone = Translation.buttons.selectNone;
                selectAll = Translation.buttons.selectAll;
                browse = Translation.buttons.browse;
                setFromCamera = Translation.buttons.setFromCamera;
                reset = Translation.buttons.reset;
                edit = Translation.buttons.edit;
                load = Translation.buttons.load;
                createNewPreset = Translation.buttons.createNewPreset;
                quickSetupAvatar = Translation.buttons.quickSetupAvatar;
                selectInToolsWindow = Translation.buttons.selectInToolsWindow;
                resetRenderer = Translation.buttons.resetRenderer;
                revertRenderer = Translation.buttons.revertRenderer;
                alignCameraToView = Translation.buttons.alignCameraToView;
                savePreset = Translation.buttons.savePreset;
                selectInAssets = Translation.buttons.selectInAssets;
                openFolder = Translation.buttons.openFolder;
                ok = Translation.buttons.ok;
            }
        };
        public static class Tools
        {
            public static string fillVisemes = "_Fill Visemes";
            public static string editViewpoint = "_Edit Viewpoint";
            public static string revertBlendshapes = "_Revert Blendshapes";
            public static string zeroBlendshapes = "_Zero Blendshapes";
            public static string resetPose = "_Reset Pose";
            public static string resetToTPose = "_Reset to T-Pose";
            public static string editScale = "_Edit Scale";
            public static string autoViewpoint = "_Auto Viewpoint";
            public static string setTPose = "_Force TPose";
            public static string setRendererAnchors = "_Set Renderer Anchors";
            public static string viewpointZDepth = "_Z Depth";
            public static string revertScale = "_Revert Scale";
            public static string editScaleMoveViewpoint = "_Move Viewpoint";
            public static string refreshRig = "_Refresh Rig";
            public static string enableDynamicBones = "_Enable DynamicBones";
            public static string disableDynamicBones = "_Disable DynamicBones";
            public static string toggleDynamicBones = "_Toggle DynamicBones";

            static Tools()
            {
                Reload();
            }

            public static void Reload()
            {
                if(Translation is null)
                    return;

                fillVisemes = Translation.tools.fillVisemes;
                editViewpoint = Translation.tools.editViewpoint;
                revertBlendshapes = Translation.tools.revertBlendshapes;
                zeroBlendshapes = Translation.tools.zeroBlendshapes;
                resetPose = Translation.tools.resetPose;
                resetToTPose = Translation.tools.resetToTPose;
                editScale = Translation.tools.editScale;
                autoViewpoint = Translation.tools.autoViewpoint;
                setTPose = Translation.tools.setTPose;
                viewpointZDepth = Translation.tools.viewpointZDepth;
                setRendererAnchors = Translation.tools.setRendererAnchors;
                revertScale = Translation.tools.revertScale;
                editScaleMoveViewpoint = Translation.tools.editScaleMoveViewpoint;
                refreshRig = Translation.tools.refreshRig;                
                enableDynamicBones = Translation.tools.enableDynamicBones;
                disableDynamicBones = Translation.tools.disableDynamicBones;
                toggleDynamicBones = Translation.tools.toggleDynamicBones;
            }
        };
        public static class AvatarInfo
        {
            public static string name = "_{0}";
            public static string line = "_---------------------";
            public static string gameObjects = "_GameObjects: {0} ({1})";
            public static string shaders = "_Shaders: {0}";
            public static string selectAvatarFirst = "_Select an Avatar first";
            public static string uniqueMaterials = "_Unique Materials: {0} ({1})";
            public static string ikFollowers = "_IK Followers: {0} ({1})";
            public static string overallPerformance = "_Overall Performance: {0}";
#if PUMKIN_VRCSDK2
            public static string bones = "_Bones: {0} - {1}";
            public static string skinnedMeshRenderers = "_Skinned Mesh Renderers: {0} ({1}) - {2}";
            public static string meshRenderers = "_Mesh Renderers: {0} ({1}) - {2}";
            public static string polygons = "_Polygons: {0} ({1}) - {2}";
            public static string usedMaterialSlots = "_Used Material Slots: {0} ({1}) - {2}";
            public static string dynamicBoneTransforms = "_Dynamic Bone Transforms: {0} ({1}) - {2}";
            public static string dynamicBoneColliders = "_Dynamic Bone Colliders: {0} ({1}) - {2}";
            public static string dynamicBoneColliderTransforms = "_Collider Affected Transforms: {0} ({1}) - {2}";
            public static string particleSystems = "_Particle Systems: {0} ({1}) - {2}";
            public static string maxParticles = "_Max Particles: {0} ({1}) - {2}";            
#else
            public static string bones = "_Bones: {0}";
            public static string skinnedMeshRenderers = "_Skinned Mesh Renderers: {0} ({1})";
            public static string meshRenderers = "_Mesh Renderers: {0} ({1})";
            public static string polygons = "_Polygons: {0} ({1})";
            public static string usedMaterialSlots = "_Used Material Slots: {0} ({1})";            
            public static string dynamicBoneTransforms = "_Dynamic Bone Transforms: {0} ({1})";
            public static string dynamicBoneColliders = "_Dynamic Bone Colliders: {0} ({1})";
            public static string dynamicBoneColliderTransforms = "_Collider Affected Transforms: {0} ({1})";
            public static string particleSystems = "_Particle Systems: {0} ({1})";
            public static string maxParticles = "_Max Particles: {0} ({1})";        
#endif

            static AvatarInfo()
            {
                Reload();
            }

            public static void Reload()
            {
                if(Translation is null)
                    return;

                name = Translation.avatarInfo.name;
                line = Translation.avatarInfo.line;
                gameObjects = Translation.avatarInfo.gameObjects;
                bones = Translation.avatarInfo.bones;
                skinnedMeshRenderers = Translation.avatarInfo.skinnedMeshRenderers;
                meshRenderers = Translation.avatarInfo.meshRenderers;
                polygons = Translation.avatarInfo.polygons;
                usedMaterialSlots = Translation.avatarInfo.usedMaterialSlots;
                uniqueMaterials = Translation.avatarInfo.uniqueMaterials;
                shaders = Translation.avatarInfo.shaders;
                dynamicBoneTransforms = Translation.avatarInfo.dynamicBoneTransforms;
                dynamicBoneColliders = Translation.avatarInfo.dynamicBoneColliders;
                dynamicBoneColliderTransforms = Translation.avatarInfo.dynamicBoneColliderTransforms;
                particleSystems = Translation.avatarInfo.particleSystems;
                maxParticles = Translation.avatarInfo.maxParticles;
                selectAvatarFirst = Translation.avatarInfo.selectAvatarFirst;
                overallPerformance = Translation.avatarInfo.overallPerformance;
                ikFollowers = Translation.avatarInfo.ikFollowers;
            }
        }
        public static class Thumbnails
        {
            public static string overlayCameraImage = "_Overlay Image";
            public static string overlayTexture = "_Overlay Texture";
            public static string startUploadingFirst = "_Start uploading an Avatar, or get into Play mode";                        
            public static string backgroundType = "_Background Type";
            public static string backgroundType_None = "_None";
            public static string backgroundType_Material = "_Skybox";
            public static string backgroundType_Color = "_Color";
            public static string backgroundType_Image = "_Image";
            public static string hideOtherAvatars = "_Hide Other Avatars when Uploading";
            public static string tint = "_Tint";
            public static string useCameraOverlay = "_Use Camera Overlay Image";
            public static string useCameraBackground = "_Use Camera Background Image";
            public static string selectedCamera = "_Selected Camera";
            public static string offset = "_Offset";
            public static string blendshapes = "_Blendshapes";
            public static string poses = "_Poses";
            public static string cameras = "_Cameras";
            public static string centerCameraFixClippingPlanes = "_Fix Clipping Planes";
            public static string positionOffset = "_Position Offset";
            public static string rotationOffset = "_Rotation Offset";
            public static string tryFixPoseSinking = "_Try to Fix Pose Sinking";
            public static string centerCameraOn = "_Center Camera on {0}";
            public static string viewpoint = "_Viewpoint";
            public static string applyBodyPosition = "_Apply Body Position";
            public static string applyBodyRotation = "_Apply Body Rotation";
            public static string lockSelectedCameraToSceneView = "_Lock Selected Camera to Scene View";

            static Thumbnails()
            {
                Reload();
            }

            public static void Reload()
            {
                if(Translation is null)
                    return;

                overlayCameraImage = Translation.thumbnails.overlayCameraImage;
                overlayTexture = Translation.thumbnails.overlayTexture;
                startUploadingFirst = Translation.thumbnails.startUploadingFirst;                
                backgroundType = Translation.thumbnails.backgroundType;
                backgroundType_None = Translation.thumbnails.backgroundType_None;
                backgroundType_Material = Translation.thumbnails.backgroundType_Material;
                backgroundType_Color = Translation.thumbnails.backgroundType_Color;
                backgroundType_Image = Translation.thumbnails.backgroundType_Image;
                hideOtherAvatars = Translation.thumbnails.hideOtherAvatars;
                tint = Translation.thumbnails.tint;
                useCameraOverlay = Translation.thumbnails.useCameraOverlay;
                useCameraBackground = Translation.thumbnails.useCameraBackground;
                selectedCamera = Translation.thumbnails.selectedCamera;
                offset = Translation.thumbnails.offset;
                blendshapes = Translation.thumbnails.blendshapes;
                poses = Translation.thumbnails.poses;
                cameras = Translation.thumbnails.cameras;
                centerCameraFixClippingPlanes = Translation.thumbnails.centerCameraFixClippingPlanes;
                positionOffset = Translation.thumbnails.positionOffset;
                rotationOffset = Translation.thumbnails.rotationOffset;
                tryFixPoseSinking = Translation.thumbnails.tryFixPoseSinking;
                centerCameraOn = Translation.thumbnails.centerCameraOn;
                viewpoint = Translation.thumbnails.viewpoint;
                applyBodyPosition = Translation.thumbnails.applyBodyPosition;
                applyBodyRotation = Translation.thumbnails.applyBodyRotation;
                lockSelectedCameraToSceneView = Translation.thumbnails.lockSelectedCameraToSceneView;
            }
        }
        public static class Copier
        {
            public static string copyFrom = "_Copy From";

            public static string copySettings = "_Settings";
            public static string createMissing = "_Copy Missing";
            public static string emptyGameObjects = "_Empty GameObjects";
            public static string replaceOld = "_Replace Old";

            public static string transforms = "_Transforms";
            public static string transforms_position = "_Position";
            public static string transforms_rotation = "_Rotation";
            public static string transforms_scale = "_Scale";
            public static string transforms_avatarScale = "_Avatar Scale";
            public static string dynamicBones = "_Dynamic Bones";
            public static string dynamicBones_colliders = "_Dynamic Bone Colliders";
            public static string dynamicBones_removeOldBones = "_Remove Old Bones";
            public static string dynamicBones_removeOldColliders = "_Remove Old Colliders";
            public static string dynamicBones_createMissing = "_Copy Missing Bones";
            public static string colliders = "_Colliders";
            public static string colliders_box = "_Box Colliders";
            public static string colliders_capsule = "_Capsule Colliders";
            public static string colliders_sphere = "_Sphere Colliders";
            public static string colliders_mesh = "_Mesh Colliders";
            public static string colliders_removeOld = "_Remove Old Colliders";
            public static string descriptor = "_Avatar Descriptor";
            public static string descriptor_pipelineId = "_Pipeline Id";
            public static string descriptor_animationOverrides = "_Animation Overrides";
            public static string descriptor_copyViewpoint = "_Viewpoint";
            public static string skinMeshRender = "_Skinned Mesh Renderers";
            public static string skinMeshRender_materials = "_Materials";
            public static string skinMeshRender_blendShapeValues = "_BlendShape Values";
            public static string particleSystems = "_Particle Systems";
            public static string rigidBodies = "_Rigid Bodies";
            public static string trailRenderers = "_Trail Renderers";
            public static string meshRenderers = "_Mesh Renderers";
            public static string copyGameObjects = "_Copy GameObjects";
            public static string copyColliderObjects = "_Copy Collider Objects";
            public static string lights = "_Lights";
            public static string animators = "_Animators";
            public static string copyMainAnimator = "_Copy Main Animator";
            public static string animators_inChildren = "_Child Animators";
            public static string audioSources = "_Audio Sources";
            public static string joints = "_Joints";
            public static string other = "_Other";
            public static string other_ikFollowers = "_IK Followers";            
            public static string other_emptyScripts = "_Empty Scripts";

            public static string exclusions = "_Exclusions";
            public static string includeChildren = "_Include Children";
            public static string size = "_Size";            

            static Copier()
            {
                Reload();
            }

            public static void Reload()
            {
                if(Translation is null)
                    return;

                copyFrom = Translation.copier.copyFrom;

                copySettings = Translation.copier.copySettings;
                createMissing = Translation.copier.createMissing;
                emptyGameObjects = Translation.copier.emptyGameObjects;
                replaceOld = Translation.copier.replaceOld;

                transforms = Translation.copier.transforms;
                transforms_position = Translation.copier.transforms_position;
                transforms_rotation = Translation.copier.transforms_rotation;
                transforms_scale = Translation.copier.transforms_scale;
                transforms_avatarScale = Translation.copier.transforms_avatarScale;
                dynamicBones = Translation.copier.dynamicBones;
                dynamicBones_colliders = Translation.copier.dynamicBones_colliders;
                dynamicBones_removeOldBones = Translation.copier.dynamicBones_removeOldBones;
                dynamicBones_removeOldColliders = Translation.copier.dynamicBones_removeOldColliders;
                dynamicBones_createMissing = Translation.copier.createMissing;
                colliders = Translation.copier.colliders;
                colliders_box = Translation.copier.colliders_box;
                colliders_capsule = Translation.copier.colliders_capsule;
                colliders_sphere = Translation.copier.colliders_sphere;
                colliders_mesh = Translation.copier.colliders_mesh;
                colliders_removeOld = Translation.copier.colliders_removeOld;
                descriptor = Translation.copier.descriptor;
                descriptor_pipelineId = Translation.copier.descriptor_pipelineId;
                descriptor_animationOverrides = Translation.copier.descriptor_animationOverrides;
                descriptor_copyViewpoint = Translation.copier.descriptor_copyViewpoint;
                skinMeshRender = Translation.copier.skinMeshRender;
                skinMeshRender_materials = Translation.copier.skinMeshRender_materials;
                skinMeshRender_blendShapeValues = Translation.copier.skinMeshRender_blendShapeValues;
                particleSystems = Translation.copier.particleSystems;
                rigidBodies = Translation.copier.rigidBodies;
                trailRenderers = Translation.copier.trailRenderers;
                meshRenderers = Translation.copier.meshRenderers;
                copyGameObjects = Translation.copier.copyGameObjects;
                copyColliderObjects = Translation.copier.copyColliderObjects;
                lights = Translation.copier.lights;
                animators = Translation.copier.animators;
                copyMainAnimator = Translation.copier.copyMainAnimator;
                animators_inChildren = Translation.copier.animators_inChildren;
                audioSources = Translation.copier.audioSources;
                joints = Translation.copier.joints;
                other = Translation.copier.other;
                other_ikFollowers = Translation.copier.other_ikFollowers;
                other_emptyScripts = Translation.copier.other_emptyScripts;

                exclusions = Translation.copier.ignoreList;
                includeChildren = Translation.copier.includeChildren;
                size = Translation.copier.size;               
            }
        };
        public static class Log
        {
            public static string done = "_Done";
            public static string cancelled = "_Cancelled";
            public static string nothingSelected = "_Select something first";
            public static string cantCopyToSelf = "_Can't copy Components from an object to itself. What are you doing?";
            public static string copyAttempt = "_Attempting to copy '{0}' from '{1}' to '{2}'";
            public static string removeAttempt = "_Attempting to remove '{0}' from '{1}'";
            public static string copyFromInvalid = "_Can't copy Components because 'Copy From' is invalid";
            public static string viewpointApplied = "_Set Viewposition to '{0}'";
            public static string viewpointCancelled = "_Cancelled Viewposition changes";
            public static string tryFillVisemes = "_Attempting to fill visemes on '{0}'";
            public static string noSkinnedMeshFound = "_Failed: No skinned mesh found";
            public static string descriptorIsNull = "_Avatar Descriptor is null";
            public static string success = "_Success";
            public static string meshHasNoVisemes = "_Failed. Mesh has no Visemes. Set to Default";            
            public static string failed = "_Failed";
            public static string failedIsNull = "_Failed: '{1}' is null";
            public static string nameIsEmpty = "_Name is Empty";
            public static string loadedPose = "_Loaded Pose: '{0}'";
            public static string loadedBlendshapePreset = "_Loaded Blendshapes: '{0}'";
            public static string failedDoesntHave = "_Failed: '{0}' doesn't have a '{1}'";
            public static string failedAlreadyHas = "_Failed: '{0}' already has a '{1}'";
            public static string loadedCameraOverlay = "_Loaded '{0}' as Camera Overlay";
            public static string failedHasNoIgnoring = "_'{0}' has no '{1}', Ignoring.";
            public static string settingQuickViewpoint = "_Setting quick Viewpoint to '{0}'";
            public static string cantSetViewpointNonHumanoid = "_Can't set Viewpoint for a non humanoid avatar";
            public static string setAvatarScaleTo = "_Set Avatar scale to '{0}'";
            public static string setAvatarScaleAndViewpointTo = "_Set Avatar scale to '{0}' and Viewpoint to '{1}'";
            public static string canceledScaleChanges = "_Cancelled Scale changes";
            public static string successCopiedOverFromTo = "_Success: Copied over '{0}' from '{1}''s '{2}' to '{3}''s '{4}'";
            public static string hasNoComponentsOrChildrenDestroying = "_'{0}' has no components or children. Destroying";
            public static string cantBeDestroyedPartOfPrefab = "_'{0}''s '{1}' can't be destroyed because it's part of a prefab instance. Ignoring";
            public static string meshPrefabMissingCantRevertBlednshapes = "_Mesh prefab is missing, can't revert to default blendshapes";
            public static string meshPrefabMissingCantRevertPose = "_Mesh prefab is missing, can't revert to default pose";
            public static string runtimeBlueprintNotFoundStartUploading = "_RuntimeBlueprintCreation script not found. Start uploading an avatar to use this";
            public static string failedToCenterCameraNoDescriptor = "_Failed to center camera on Viewpoint. Avatar descriptor not found";
            public static string setProbeAnchorTo = "_Set '{0}''s probe anchor to '{1}'";
            public static string cantSetPoseNonHumanoid = "_Can't set humanoid pose '{0}' on a non humanoid avatar";
            public static string loadedImageAsBackground = "_Loaded '{0}' as Background image";
            public static string loadedImageAsOverlay = "_Loaded '{0}' as Overlay image";
            public static string descriptorIsMissingCantGetViewpoint = "_Avatar Descriptor is missing. Can't get Viewpoint position";
            public static string hasMissingScriptDestroying = "_{0}'s component number {1} is a missing script. Destroying";
            public static string copiedDynamicBone = "_Copied DynamicBone from {0}'s {1} to {2}'s {1}";
            public static string invalidTranslation = "_Can't load translation asset. Invalid translation";

            static Log()
            {
                Reload();
            }

            public static void Reload()
            {
                if(Translation is null)
                    return;

                done = Translation.log.done;
                cancelled = Translation.log.cancelled;
                nothingSelected = Translation.log.nothingSelected;
                cantCopyToSelf = Translation.log.cantCopyToSelf;
                copyAttempt = Translation.log.copyAttempt;
                removeAttempt = Translation.log.removeAttempt;
                copyFromInvalid = Translation.log.copyFromInvalid;
                viewpointApplied = Translation.log.viewpointApplied;
                viewpointCancelled = Translation.log.viewpointCancelled;
                tryFillVisemes = Translation.log.tryFillVisemes;
                noSkinnedMeshFound = Translation.log.noSkinnedMeshFound;
                descriptorIsNull = Translation.log.descriptorIsNull;
                success = Translation.log.success;
                meshHasNoVisemes = Translation.log.meshHasNoVisemes;
                failed = Translation.log.failed;
                failedIsNull = Translation.log.failedIsNull;
                nameIsEmpty = Translation.log.nameIsEmpty;
                loadedPose = Translation.log.loadedPose;
                loadedBlendshapePreset = Translation.log.loadedBlendshapePreset;
                failedDoesntHave = Translation.log.failedDoesntHave;
                failedAlreadyHas = Translation.log.failedAlreadyHas;
                loadedCameraOverlay = Translation.log.loadedCameraOverlay;
                failedHasNoIgnoring = Translation.log.failedHasNoIgnoring;
                settingQuickViewpoint = Translation.log.settingQuickViewpoint;
                cantSetViewpointNonHumanoid = Translation.log.cantSetViewpointNonHumanoid;
                setAvatarScaleTo = Translation.log.setAvatarScaleTo;
                setAvatarScaleAndViewpointTo = Translation.log.setAvatarScaleAndViewpointTo;
                canceledScaleChanges = Translation.log.canceledScaleChanges;
                successCopiedOverFromTo = Translation.log.successCopiedOverFromTo;
                hasNoComponentsOrChildrenDestroying = Translation.log.hasNoComponentsOrChildrenDestroying;
                cantBeDestroyedPartOfPrefab = Translation.log.cantBeDestroyedPartOfPrefab;
                meshPrefabMissingCantRevertBlednshapes = Translation.log.meshPrefabMissingCantRevertBlednshapes;
                meshPrefabMissingCantRevertPose = Translation.log.meshPrefabMissingCantRevertPose;
                runtimeBlueprintNotFoundStartUploading = Translation.log.runtimeBlueprintNotFoundStartUploading;
                failedToCenterCameraNoDescriptor = Translation.log.failedToCenterCameraNoDescriptor;
                cantSetPoseNonHumanoid = Translation.log.cantSetPoseNonHumanoid;
                setProbeAnchorTo = Translation.log.setProbeAnchorTo;
                loadedImageAsBackground = Translation.log.loadedImageAsBackground;
                loadedImageAsOverlay = Translation.log.loadedImageAsOverlay;
                descriptorIsMissingCantGetViewpoint = Translation.log.descriptorIsMissingCantGetViewpoint;
                hasMissingScriptDestroying = Translation.log.hasMissingScriptDestroying;
                copiedDynamicBone = Translation.log.copiedDynamicBone;
                invalidTranslation = Translation.log.invalidTranslation;
            }
        };
        public static class Warning
        {
            public static string warn = "_Warning";
            public static string notFound = "_Not Found";
            public static string oldVersion = "_Old Version";
            public static string selectSceneObject = "_Please select an object from the scene";
            public static string cameraNotFound = "_Camera not found";
            public static string invalidPreset = "_Can't apply preset {0}: Invalid Preset";
            public static string cantRevertRendererWithoutPrefab = "_Can't revert Skinned Mesh Renderer {0}, object has no Prefab";
            public static string cantLoadImageAtPath = "_Can't load image at {0}";
            public static string doesntWorkInUnity2017 = "_Doesn't work in Unity 2017 :(";
            public static string armatureScaleNotOne = "_Armature scale for selected avatar isn't 1! This can cause issues. Please re-export your avatar with CATS' export option";
            public static string armatureScalesDontMatch = "_Armature scales for selected avatars don't match!\nThis can cause issues";
            public static string noDBonesOrMissingScriptDefine = "_No DynamicBones found or missing script define.";
            public static string languageAlreadyExistsOverwrite = "_Language Asset already exists. Overwrite?";

            static Warning()
            {
                Reload();
            }

            public static void Reload()
            {
                if(Translation is null)
                    return;

                warn = Translation.warnings.warn;
                notFound = Translation.warnings.notFound;
                oldVersion = Translation.warnings.oldVersion;
                selectSceneObject = Translation.warnings.selectSceneObject;
                cameraNotFound = Translation.warnings.cameraNotFound;
                invalidPreset = Translation.warnings.invalidPreset;
                cantRevertRendererWithoutPrefab = Translation.warnings.cantRevertRendererWithoutPrefab;
                doesntWorkInUnity2017 = Translation.warnings.doesntWorkInUnity2017;
                armatureScaleNotOne = Translation.warnings.armatureScaleNotOne;
                armatureScalesDontMatch = Translation.warnings.armatureScalesDontMatch;
                noDBonesOrMissingScriptDefine = Translation.warnings.noDBonesOrMissingScriptDefine;
                languageAlreadyExistsOverwrite = Translation.warnings.languageAlreadyExistsOverwrite;
            }
        };
        public static class Credits
        {
            public static string version = "_Version";
            public static string redundantStrings = "_Now with 100% more redundant strings";
            public static string addMoreStuff = "_I'll add more stuff to this eventually";
            public static string pokeOnDiscord = "_Poke me on Discord at Pumkin#2020";

            static Credits()
            {
                Reload();
            }

            public static void Reload()
            {
                if(Translation is null)
                    return;

                version = (Translation.credits.version + " " + TOOLS_VERSION_STRING) ?? ("_Version" + " " + TOOLS_VERSION_STRING);
                redundantStrings = Translation.credits.redundantStrings;
                addMoreStuff = Translation.credits.addMoreStuff;
                pokeOnDiscord = Translation.credits.pokeOnDiscord;
            }
        };
        public static class Misc
        {
            public static string uwu = "_uwu";
            public static string searchForBones = "_Search for DynamicBones";
            public static string superExperimental = "_Super Experimental Stuff";
            public static string language = "_Language";
            public static string refresh = "_Refresh";
            public static string importLanguageAsset = "_Import Language Asset";

            static Misc()
            {
                Reload();
            }

            public static void Reload()
            {
                if(Translation is null)
                    return;

                uwu = Translation.misc.uwu;
                searchForBones = Translation.misc.searchForBones;
                superExperimental = Translation.misc.superExperimental;
                language = Translation.misc.language;
                refresh = Translation.misc.refresh;
                importLanguageAsset = Translation.misc.importLanguageAsset;
            }
        }
        public static class PoseEditor
        {
            public static string version = "_Version";
            public static string title = "_Pumkin's Pose Editor";
            public static string scene = "_Scene";
            public static string sceneLoadAdditive = "_Load Additive";
            public static string sceneOverrideLights = "_Override Lights";
            public static string avatarPosition = "_Avatar Position";
            public static string avatarPositionOverridePose = "_Override Pose";
            public static string avatarPositionOverrideBlendshapes = "_Override Blendshapes";
            public static string sceneSaveChanges = "_Save Scene Changes";
            public static string unloadScene = "_Unload Scene";
            public static string resetPosition = "_Reset Position";
            public static string pose = "_Pose";
            public static string newPose = "_New Pose";
            public static string onlySavePoseChanges = "_Only Save Pose Changes";
            public static string loadPose = "_Load Pose";
            public static string blendshapes = "_Blendshapes";
            public static string newPreset = "_New Preset";
            public static string loadPreset = "_Load Preset";
            public static string saveButton = "_Save";
            public static string reloadButton = "_Reload";
            public static string bodyPositionYTooSmall = "_humanPose.bodyPosition.y is {0}, you probably don't want that. Setting humanPose.bodyPosition.y to 1";
            public static string muscles = "_Muscles";
            public static string transformRotations = "_Transform Rotations";
            public static string selectHumanoidAvatar = "_Select a Humanoid Avatar";
            public static string animationTime = "_Time";
            public static string poseFromAnimation = "_Pose from Animation";
            public static string allowMotion = "_Allow Motion";

            static PoseEditor()
            {
                Reload();
            }

            public static void Reload()
            {
                if(Translation is null)
                    return;

                version = (Translation.credits.version + " " + POSE_EDITOR_VERSION_NUMBER) ?? ("_Version" + " " + POSE_EDITOR_VERSION_NUMBER);

                title = Translation.poseEditor.title;
                scene = Translation.poseEditor.scene;
                sceneLoadAdditive = Translation.poseEditor.sceneLoadAdditive;
                sceneOverrideLights = Translation.poseEditor.sceneOverrideLights;
                avatarPosition = Translation.poseEditor.avatarPosition;
                avatarPositionOverridePose = Translation.poseEditor.avatarPositionOverridePose;
                avatarPositionOverrideBlendshapes = Translation.poseEditor.avatarPositionOverrideBlendshapes;
                sceneSaveChanges = Translation.poseEditor.sceneSaveChanges;
                unloadScene = Translation.poseEditor.unloadScene;
                resetPosition = Translation.poseEditor.resetPosition;
                pose = Translation.poseEditor.pose;
                newPose = Translation.poseEditor.newPose;
                onlySavePoseChanges = Translation.poseEditor.onlySavePoseChanges;
                loadPose = Translation.poseEditor.loadPose;
                blendshapes = Translation.poseEditor.blendshapes;
                newPreset = Translation.poseEditor.newPreset;
                loadPreset = Translation.poseEditor.loadPreset;
                saveButton = Translation.poseEditor.saveButton;
                reloadButton = Translation.poseEditor.reloadButton;
                bodyPositionYTooSmall = Translation.poseEditor.bodyPositionYTooSmall;
                muscles = Translation.poseEditor.muscles;
                transformRotations = Translation.poseEditor.transformRotations;
                selectHumanoidAvatar = Translation.poseEditor.selectHumanoidAvatar;
                animationTime = Translation.poseEditor.animationTime;
                poseFromAnimation = Translation.poseEditor.poseFromAnimation;
                allowMotion = Translation.poseEditor.allowMotion;
            }
        }
        public static class Presets
        {
            public static string presetName = "_Preset Name";
            public static string mode = "_Preset Mode";
            public static string otherNames = "_Other Names";
            public static string poseMode = "_Pose Mode";
            public static string editPosePreset = "_Edit Pose Preset";
            public static string createPosePreset = "_Create Pose Preset";
            public static string overwriteFile = "Overwrite File";
            public static string transformDoesntBelongToAvatar = "_{0} doesn't belong to avatar {1}";
            public static string cameraPreset = "_Camera Preset";
            public static string posePreset = "_Pose Preset";
            public static string blendshapePreset = "_Blendshape Preset";
            public static string editBlendshapePreset = "_Edit Blendshape Preset";
            public static string createBlendshapePreset = "_Create Blendshape Preset";
            public static string transform = "_Transform";
            public static string offsetMode = "_Offset Mode";
            public static string camera = "_Camera";
            public static string editCameraPreset = "_Edit Camera Preset";
            public static string createCameraPreset = "_Create Camera Preset";

            static Presets()
            {
                Reload();
            }

            public static void Reload()
            {
                if(Translation is null)
                    return;

                presetName = Translation.preset.presetName;
                mode = Translation.preset.mode;
                otherNames = Translation.preset.otherNames;
                poseMode = Translation.preset.poseMode;
                editPosePreset = Translation.preset.editPosePreset;
                createPosePreset = Translation.preset.createPosePreset;
                overwriteFile = Translation.preset.overwriteFile;
                transformDoesntBelongToAvatar = Translation.preset.transformDoesntBelongToAvatar;
                cameraPreset = Translation.preset.cameraPreset;
                posePreset = Translation.preset.posePreset;
                blendshapePreset = Translation.preset.blendshapePreset;
                editBlendshapePreset = Translation.preset.editBlendshapePreset;
                createBlendshapePreset = Translation.preset.createBlendshapePreset;
                transform = Translation.preset.transform;
                offsetMode = Translation.preset.offsetMode;
                camera = Translation.preset.camera;
                editCameraPreset = Translation.preset.editCameraPreset;
                createCameraPreset = Translation.preset.createCameraPreset;
            }
        }

        static Strings()
        {
            ReloadStrings();
        }

        static void ReloadStrings()
        {
            Main.Reload();
            Buttons.Reload();
            Tools.Reload();
            Copier.Reload();
            Log.Reload();
            Warning.Reload();
            Credits.Reload();
            Misc.Reload();
            Thumbnails.Reload();
            AvatarInfo.Reload();
        }
    };
}