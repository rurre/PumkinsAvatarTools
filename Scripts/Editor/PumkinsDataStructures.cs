#define NEWSDK
using Newtonsoft.Json;
using Pumkin.AvatarTools;
using Pumkin.HelperFunctions;
using Pumkin.PoseEditor;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using VRCSDK2;
using Pumkin.Translations;
using System.Linq;
using Pumkin.Extensions;

#if NEWSDK
using VRCSDK2.Validation.Performance;
using VRCSDK2.Validation.Performance.Stats;
#endif

namespace Pumkin.DataStructures
{
    [ExecuteInEditMode, InitializeOnLoad] //needed for string singleton
    public class Strings : SingletonScriptableObject<Strings>
    {
#if NEWSDK
        public readonly string TOOLS_VERSION_NUMBER = "0.7b - Work in Progress";
#else
        public readonly string TOOLS_VERSION_NUMBER = "0.7b - Work in Progress - Old SDK";
#endif
        public readonly string POSE_EDITOR_VERSION_NUMBER = "0.1b - Work in Progress";
        public readonly string LINK_GITHUB = "https://github.com/rurre/PumkinsAvatarTools/";
        public readonly string LINK_DONATION = "https://ko-fi.com/notpumkin";
        public readonly string LINK_DISCORD = "https://discord.gg/7vyekJv";

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
            public static string title = "_Pumkin's Avatar Tools";
            public static string windowName = "_Pumkin Tools";
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
                if(!Translation)
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

            static Buttons()
            {
                Reload();
            }

            public static void Reload()
            {
                if(!Translation)
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

            static Tools()
            {
                Reload();
            }

            public static void Reload()
            {
                if(!Translation)
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
            public static string overallPerformance = "_Overall Performance: {0}";
#if NEWSDK
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
                if(!Translation)
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
            }
        }
        public static class Thumbnails
        {
            public static string overlayCameraImage = "_Overlay Image";
            public static string overlayTexture = "_Overlay Texture";
            public static string startUploadingFirst = "_Start uploading an Avatar, or get into Play mode";
            public static string centerCameraOnViewpoint = "_Center Camera on Viewpoint";
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
            public static string previewIsDark = "_The preview is currently shown darker than it actually is.\nUse the Game view for a more accurate preview.";
            public static string positionOffset = "_Position Offset";
            public static string rotationOffset = "_Rotation Offset";

            static Thumbnails()
            {
                Reload();
            }

            public static void Reload()
            {
                if(!Translation)
                    return;

                overlayCameraImage = Translation.thumbnails.overlayCameraImage;
                overlayTexture = Translation.thumbnails.overlayTexture;
                startUploadingFirst = Translation.thumbnails.startUploadingFirst;
                centerCameraOnViewpoint = Translation.thumbnails.centerCameraOnViewpoint;
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
                previewIsDark = Translation.thumbnails.previewIsDark;
                positionOffset = Translation.thumbnails.positionOffset;
                rotationOffset = Translation.thumbnails.rotationOffset;
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
            public static string dynamicBones_colliders = "_Colliders";
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

            public static string exclusions = "_Exclusions";
            public static string includeChildren = "_Include Children";
            public static string size = "_Size";

            static Copier()
            {
                Reload();
            }

            public static void Reload()
            {
                if(!Translation)
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
                dynamicBones_colliders = Translation.copier.colliders;
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
            public static string copyAttempt = "_Attempting to copy {0} from {1} to {2}";
            public static string removeAttempt = "_Attempting to remove {0} from {1}";
            public static string copyFromInvalid = "_Can't copy Components because 'Copy From' is invalid";
            public static string viewpointApplied = "_Set Viewposition to {0}";
            public static string viewpointCancelled = "_Cancelled Viewposition changes";
            public static string tryFillVisemes = "_Attempting to fill visemes on {0}";
            public static string noSkinnedMeshFound = "_Failed: No skinned mesh found";
            public static string descriptorIsNull = "_Avatar descriptor is null";
            public static string success = "_Success";
            public static string meshHasNoVisemes = "_Failed. Mesh has no Visemes. Set to Default";
            public static string tryRemoveUnsupportedComponent = "_Attempting to remove unsupported component {0} from {1}";
            public static string failed = "_Failed";
            public static string failedIsNull = "_Failed: {1} is null";
            public static string nameIsEmpty = "_Name is Empty";
            public static string loadedPose = "_Loaded Pose: {0}";
            public static string loadedBlendshapePreset = "_Loaded Blendshapes: {0}";
            public static string failedDoesntHave = "_Failed: {0} doesn't have a {1}";
            public static string failedAlreadyHas = "_Failed: {0} already has a {1}";
            public static string loadedCameraOverlay = "_Loaded {0} as Camera Overlay";
            public static string failedHasNo = "_{0} has no {1}, Ignoring.";
            public static string settingQuickViewpoint = "_Setting quick viewpoint to {0}";
            public static string cantSetViewpointNonHumanoid = "_Can't set Viewpoint for a non humanoid avatar";
            public static string setAvatarScaleTo = "_Set Avatar scale to {0} and Viewpoint to {1}";
            public static string setAvatarScaleAndViewpointTo = "_Set Avatar scale to {0} and Viewpoint to {1}";
            public static string canceledScaleChanges = "_Cancelled Scale changes";
            public static string successCopiedOverFromTo = "_Success: Copied over {0} from {1}'s {2} to {3}'s {4}";
            public static string hasNoComponentsOrChildrenDestroying = "_{0} has no components or children. Destroying";
            public static string cantBeDestroyedPartOfPrefab = "_{0}'s {1} can't be destroyed because it's part of a prefab instance. Ignoring";
            public static string meshPrefabMissingCantRevertBlednshapes = "_Mesh prefab is missing, can't revert to default blendshapes";
            public static string meshPrefabMissingCantRevertPose = "_Mesh prefab is missing, can't revert to default pose";
            public static string runtimeBlueprintNotFoundStartUploading = "_RuntimeBlueprintCreation script not found. Start uploading an avatar to use this";
            public static string failedToCenterCameraNoDescriptor = "_Failed to center camera on Viewpoint. Avatar descriptor not found";            
            public static string setProbeAnchorTo = "_Set {0}'s probe anchor to {1}";
            public static string cantSetPoseNonHumanoid = "_Can't set humanoid pose {0} on a non humanoid avatar";
            public static string loadedImageAsBackground = "_Loaded {0} as Background image";
            public static string loadedImageAsOverlay = "_Loaded {0} as Overlay image";

            static Log()
            {
                Reload();
            }

            public static void Reload()
            {
                if(!Translation)
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
                failedHasNo = Translation.log.failedHasNo;
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
            public static string cantRevertRendererWithoutPrefab = "_Can't revert Skinned Mesh Renderer {0}, object has no Prefab.";
            public static string cantLoadImageAtPath = "_Can't load image at {0}";

            static Warning()
            {
                Reload();
            }

            public static void Reload()
            {
                if(!Translation)
                    return;

                warn = Translation.warnings.warn;
                notFound = Translation.warnings.notFound;
                oldVersion = Translation.warnings.oldVersion;
                selectSceneObject = Translation.warnings.selectSceneObject;
                cameraNotFound = Translation.warnings.cameraNotFound;
                invalidPreset = Translation.warnings.invalidPreset;
                cantRevertRendererWithoutPrefab = Translation.warnings.cantRevertRendererWithoutPrefab;
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
                if(!Translation)
                    return;

                version = (Translation.credits.version + " " + Instance.TOOLS_VERSION_NUMBER) ?? ("_Version" + " " + Instance.TOOLS_VERSION_NUMBER);
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

            static Misc()
            {
                Reload();
            }

            public static void Reload()
            {
                if(!Translation)
                    return;

                uwu = Translation.misc.uwu;
                searchForBones = Translation.misc.searchForBones;
                superExperimental = Translation.misc.superExperimental;
                language = Translation.misc.language;
                refresh = Translation.misc.refresh;
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

            static PoseEditor()
            {
                Reload();
            }

            public static void Reload()
            {
                if(!Translation)
                    return;

                version = (Translation.credits.version + " " + Instance.POSE_EDITOR_VERSION_NUMBER) ?? ("_Version" + " " + Instance.POSE_EDITOR_VERSION_NUMBER);

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
            }
        }
        public static class Preset
        {
            public static string name = "_Preset name";
            public static string mode = "_Preset mode";
            public static string otherNames = "_Other names";            

            static Preset()
            {
                Reload();
            }

            public static void Reload()
            {
                if(!Translation)
                    return;

                name = Translation.preset.name;
                mode = Translation.preset.mode;
                otherNames = Translation.preset.otherNames;                
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

    public static class Colors
    {
        public static Color SceneGUIWindow { get; internal set; }
        public static Color DefaultCameraBackground { get; internal set; }
        public static Color DarkCameraBackground { get; internal set; }
        public static Color BallHandle { get; internal set; }
        public static Color LightLabelText { get; internal set; }
        public static Color FoldoutTitleBackground { get; internal set; }

        static Colors()
        {
            SceneGUIWindow = new Color(0.3804f, 0.3804f, 0.3804f, 0.7f);
            DefaultCameraBackground = new Color(0.192f, 0.302f, 0.475f);
            DarkCameraBackground = new Color(0.235f, 0.22f, 0.22f);
            BallHandle = new Color(1, 0.92f, 0.016f, 0.5f);
            LightLabelText = Color.white;
            FoldoutTitleBackground = Color.yellow;
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
        public static GUIStyle PaddedBox { get; internal set; }
        public static GUIStyle HeaderToggle { get; internal set; }
        public static GUIStyle CopierToggle { get; internal set; }
        public static GUIStyle BigIconButton { get; internal set; }
        public static GUIStyle ToolbarBigButtons { get; internal set; }
        public static GUIStyle Popup { get; internal set; }
        public static GUIStyle IconButton { get; internal set; }
        public static GUIStyle TextField { get; internal set; }
        public static GUIStyle IconLabel { get; internal set; }

        static Styles()
        {
            BigButton = new GUIStyle("Button")
            {
                fixedHeight = 28f,
                stretchHeight = false,
                stretchWidth = true,
            };

            Foldout_title = new GUIStyle("ToolbarDropDown")
            {
                fontSize = 13,
                fixedHeight = 26,
                fontStyle = FontStyle.Bold,
                contentOffset = new Vector2(5f, 0),
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
                fontSize = 12,
                fixedHeight = 21,
                stretchHeight = false,
            };

            PaddedBox = new GUIStyle("box")
            {
                padding = new RectOffset(10, 10, 10, 10),
            };

            ToolbarBigButtons = new GUIStyle("button")
            {
                fixedHeight = 24f,
            };

            IconLabel = new GUIStyle("label")
            {
                fixedWidth = 20f,
                fixedHeight = 20f,
                imagePosition = ImagePosition.ImageOnly,
                padding = new RectOffset(0, 0, 0, 0),
            };

            IconButton = new GUIStyle("button")
            {
                fixedWidth = 20f,
                fixedHeight = 20f,
                imagePosition = ImagePosition.ImageOnly,
                padding = new RectOffset(0, 0, 0, 0),
            };

            CopierToggle = new GUIStyle("Toggle");
            Popup = new GUIStyle("Popup");
            Foldout = new GUIStyle("Foldout");
            HelpBox = new GUIStyle("HelpBox");
            Box = new GUIStyle("box");

            TextField = new GUIStyle("Textfield")
            {
                fixedHeight = 19f,
            };
            TextField.normal.textColor = Color.black;

            LightTextField = new GUIStyle("TextField")
            {
                fixedHeight = 19f,
            };
            LightTextField.normal.textColor = Color.white;

            BigIconButton = new GUIStyle(BigButton);
            BigIconButton.fixedWidth = 40f;
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
        public static Texture2D Settings { get; internal set; }
        public static Texture2D Delete { get; internal set; }

        public static Texture2D DiscordIcon { get; internal set; }
        public static Texture2D GithubIcon { get; internal set; }
        public static Texture2D KofiIcon { get; internal set; }
        public static Texture2D Refresh { get; internal set; }

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
            Refresh = EditorGUIUtility.FindTexture("TreeEditor.Refresh");
            Delete = EditorGUIUtility.FindTexture("TreeEditor.Trash");

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
            Settings = (Texture2D)EditorGUIUtility.IconContent("Settings").image;
            Delete = (Texture2D)EditorGUIUtility.IconContent("TreeEditor.Trash").image;

            Refresh = EditorGUIUtility.FindTexture("TreeEditor.Refresh");

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
#if NEWSDK
        AvatarPerformanceStats perfStats = new AvatarPerformanceStats();
#endif

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
        public int Bones { get; private set; }

        public AvatarInfo()
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

#if NEWSDK
            try
            {
                AvatarPerformance.CalculatePerformanceStats(o.name, o, perfStats);
            }
            catch { }
#endif
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
            var bonesList = new List<string>();
            foreach(var r in sRenders)
            {
                SkinnedMeshRenderers_Total += 1;
                if(r.sharedMesh)
                    Polygons_Total += r.sharedMesh.triangles.Length / 3;
                if(r.bones != null)
                    bonesList.AddRange(r.bones.Select(b => b.gameObject.name));

                if(r.gameObject.activeInHierarchy && r.enabled)
                {
                    SkinnedMeshRenderers += 1;
                    if(r.sharedMesh)
                        Polygons += r.sharedMesh.triangles.Length / 3;
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

            Bones = new HashSet<string>(bonesList).Count;

            var renders = o.GetComponentsInChildren<MeshRenderer>(true);
            foreach(var r in renders)
            {
                var filter = r.GetComponent<MeshFilter>();

                if(filter != null && filter.sharedMesh != null)
                {
                    MeshRenderers_Total += 1;
                    if(filter.sharedMesh != null)
                        Polygons_Total += filter.sharedMesh.triangles.Length / 3;

                    if(r.gameObject.activeInHierarchy && r.enabled)
                    {
                        MeshRenderers += 1;
                        if(filter.sharedMesh != null)
                            Polygons += filter.sharedMesh.triangles.Length / 3;
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
#if NEWSDK
                    CachedInfo =
                    string.Format(Strings.AvatarInfo.name, Name) + "\n" +
                    string.Format(Strings.AvatarInfo.line) + "\n" +
                    string.Format(Strings.AvatarInfo.gameObjects, GameObjects, GameObjects_Total) + "\n" +
                    string.Format(Strings.AvatarInfo.bones, Bones, perfStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.BoneCount)) + "\n\n" +
                    string.Format(Strings.AvatarInfo.skinnedMeshRenderers, SkinnedMeshRenderers, SkinnedMeshRenderers_Total, perfStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.SkinnedMeshCount)) + "\n" +
                    string.Format(Strings.AvatarInfo.meshRenderers, MeshRenderers, MeshRenderers_Total, perfStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.MeshCount)) + "\n" +
                    string.Format(Strings.AvatarInfo.polygons, Polygons, Polygons_Total, perfStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.PolyCount)) + "\n\n" +
                    string.Format(Strings.AvatarInfo.usedMaterialSlots, MaterialSlots, MaterialSlots_Total, perfStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.MaterialCount)) + "\n" +
                    string.Format(Strings.AvatarInfo.uniqueMaterials, UniqueMaterials, UniqueMaterials_Total) + "\n" +
                    string.Format(Strings.AvatarInfo.shaders, ShaderCount) + "\n\n" +
                    string.Format(Strings.AvatarInfo.dynamicBoneTransforms, DynamicBoneTransforms, DynamicBoneTransforms_Total, perfStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.DynamicBoneSimulatedBoneCount)) + "\n" +
                    string.Format(Strings.AvatarInfo.dynamicBoneColliders, DynamicBoneColliders, DynamicBoneColliders_Total, perfStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.DynamicBoneColliderCount)) + "\n" +
                    string.Format(Strings.AvatarInfo.dynamicBoneColliderTransforms, DynamicBoneColliderTransforms, DynamicBoneColliderTransforms_Total, perfStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.DynamicBoneCollisionCheckCount)) + "\n\n" +
                    string.Format(Strings.AvatarInfo.particleSystems, ParticleSystems, ParticleSystems_Total, perfStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.ParticleSystemCount)) + "\n" +
                    string.Format(Strings.AvatarInfo.maxParticles, MaxParticles, MaxParticles_Total, perfStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.ParticleTotalCount)) + "\n" +
                    Strings.AvatarInfo.line + "\n" +
                    string.Format(Strings.AvatarInfo.overallPerformance, perfStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.Overall));
#else
                    CachedInfo =
                    string.Format(Strings.AvatarInfo.name, Name) + "\n" +
                    string.Format(Strings.AvatarInfo.line) + "\n" +
                    string.Format(Strings.AvatarInfo.gameObjects, GameObjects, GameObjects_Total) + "\n" +
                    string.Format(Strings.AvatarInfo.bones, Bones, "?") + "\n\n" +
                    string.Format(Strings.AvatarInfo.skinnedMeshRenderers, SkinnedMeshRenderers, SkinnedMeshRenderers_Total, "?") + "\n" +
                    string.Format(Strings.AvatarInfo.meshRenderers, MeshRenderers, MeshRenderers_Total, "?") + "\n" +
                    string.Format(Strings.AvatarInfo.polygons, Polygons, Polygons_Total, "?") + "\n\n" +
                    string.Format(Strings.AvatarInfo.usedMaterialSlots, MaterialSlots, MaterialSlots_Total, "?") + "\n" +
                    string.Format(Strings.AvatarInfo.uniqueMaterials, UniqueMaterials, UniqueMaterials_Total) + "\n" +
                    string.Format(Strings.AvatarInfo.shaders, ShaderCount) + "\n\n" +
                    string.Format(Strings.AvatarInfo.dynamicBoneTransforms, DynamicBoneTransforms, DynamicBoneTransforms_Total, "?") + "\n" +
                    string.Format(Strings.AvatarInfo.dynamicBoneColliders, DynamicBoneColliders, DynamicBoneColliders_Total, "?") + "\n" +
                    string.Format(Strings.AvatarInfo.dynamicBoneColliderTransforms, DynamicBoneColliderTransforms, DynamicBoneColliderTransforms_Total, "?") + "\n\n" +
                    string.Format(Strings.AvatarInfo.particleSystems, ParticleSystems, ParticleSystems_Total, "?") + "\n" +
                    string.Format(Strings.AvatarInfo.maxParticles, MaxParticles, MaxParticles_Total, "?") + "\n" +
                    Strings.AvatarInfo.line + "\n" +
                    string.Format(Strings.AvatarInfo.overallPerformance, "?");
#endif
                }
                catch
                {
                    CachedInfo =
                    string.Format(Strings.AvatarInfo.name, Name) + "\n" +
                    string.Format(Strings.AvatarInfo.line) + "\n" +
                    string.Format(Strings.AvatarInfo.gameObjects, GameObjects, GameObjects_Total) + "\n" +
                    string.Format(Strings.AvatarInfo.bones, Bones, "?") + "\n\n" +
                    string.Format(Strings.AvatarInfo.skinnedMeshRenderers, SkinnedMeshRenderers, SkinnedMeshRenderers_Total, "?") + "\n" +
                    string.Format(Strings.AvatarInfo.meshRenderers, MeshRenderers, MeshRenderers_Total, "?") + "\n" +
                    string.Format(Strings.AvatarInfo.polygons, Polygons, Polygons_Total, "?") + "\n\n" +
                    string.Format(Strings.AvatarInfo.usedMaterialSlots, MaterialSlots, MaterialSlots_Total, "?") + "\n" +
                    string.Format(Strings.AvatarInfo.uniqueMaterials, UniqueMaterials, UniqueMaterials_Total) + "\n" +
                    string.Format(Strings.AvatarInfo.shaders, ShaderCount) + "\n\n" +
                    string.Format(Strings.AvatarInfo.dynamicBoneTransforms, DynamicBoneTransforms, DynamicBoneTransforms_Total, "?") + "\n" +
                    string.Format(Strings.AvatarInfo.dynamicBoneColliders, DynamicBoneColliders, DynamicBoneColliders_Total, "?") + "\n" +
                    string.Format(Strings.AvatarInfo.dynamicBoneColliderTransforms, DynamicBoneColliderTransforms, DynamicBoneColliderTransforms_Total, "?") + "\n\n" +
                    string.Format(Strings.AvatarInfo.particleSystems, ParticleSystems, ParticleSystems_Total, "?") + "\n" +
                    string.Format(Strings.AvatarInfo.maxParticles, MaxParticles, MaxParticles_Total, "?") + "\n" +
                    Strings.AvatarInfo.line + "\n" +
                    string.Format(Strings.AvatarInfo.overallPerformance, "?");
                }
                return CachedInfo;
            }
        }

        public static bool operator true(AvatarInfo x) { return x != null; }
        public static bool operator false(AvatarInfo x) { return !(x == null); }
    }

    /// <summary>
    /// Serializable Transform class
    /// </summary>
    [Serializable]
    public class SerialTransform
    {
        public SerialVector3 position, localPosition, scale, localScale, eulerAngles, localEulerAngles;
        public SerialQuaternion rotation, localRotation;

        public SerialTransform()
        {
            this.position = Vector3.zero;
            this.rotation = Quaternion.identity;
            this.scale = Vector3.one;
            this.localEulerAngles = Vector3.zero;
            this.localPosition = Vector3.zero;
            this.localRotation = Quaternion.identity;
            this.localScale = Vector3.one;
        }

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

        public static implicit operator bool(SerialTransform t)
        {
            if(t != null)
                return true;
            return false;
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

    [Serializable]
    public class PumkinsRendererBlendshapesHolder
    {
        [SerializeField] public string rendererPath;
        [SerializeField] public List<PumkinsBlendshape> blendshapes = new List<PumkinsBlendshape>();
        [SerializeField][HideInInspector] public bool expandedInUI = false; //oof, probably not a good idea to have this ui related bool here
                                                                            //but then again this class only exists so unity can serialize the list of PumkinsBlendshape objects
        public PumkinsRendererBlendshapesHolder(string path, List<PumkinsBlendshape> shapeList)
        {
            rendererPath = path;
            blendshapes = shapeList;
        }

        public static explicit operator PumkinsRendererBlendshapesHolder(SkinnedMeshRenderer render)
        {
            if(!render)
                return null;

            string renderPath = Helpers.GetGameObjectPath(render.gameObject);
            var blendshapes = new List<PumkinsBlendshape>();
            for(int i = 0; i < render.sharedMesh.blendShapeCount; i++)
                blendshapes.Add(new PumkinsBlendshape(render.sharedMesh.GetBlendShapeName(i), render.GetBlendShapeWeight(i)));
            
            return new PumkinsRendererBlendshapesHolder(renderPath, blendshapes);
        }
    }

    [Serializable]
    public class PumkinsBlendshape
    {
        [SerializeField] public string name;
        [SerializeField] public float weight;
        [SerializeField] public List<string> otherNames;        

        [SerializeField][HideInInspector] public bool otherNamesExpandedInUI = false; //oof, probably also not a good idea to have this ui related bool here as well                                                                                    

        public PumkinsBlendshape(string name, float weight = 0, List<string> otherNames = null)
        {
            this.name = name;
            this.weight = weight;            
            if(otherNames != null)
                this.otherNames = otherNames;
            else
                this.otherNames = new List<string>();
        }
    }

    public static class HumanRig
    {
        static readonly List<HumanRigBone> Bones = new List<HumanRigBone>()
        {
            new HumanRigBone("Hips", "Hips"),
            new HumanRigBone("LeftUpperLeg", "Left leg"),
            new HumanRigBone("RightUpperLeg", "Right leg"),
            new HumanRigBone("LeftLowerLeg", "Left knee"),
            new HumanRigBone("RightLowerLeg", "Right knee"),
            new HumanRigBone("LeftFoot", "Left ankle"),
            new HumanRigBone("RightFoot", "Right ankle"),
            new HumanRigBone("Spine", "Spine"),
            new HumanRigBone("Chest", "Chest"),
            new HumanRigBone("Neck", "Neck"),
            new HumanRigBone("Head", "Head"),
            new HumanRigBone("LeftShoulder", "Left shoulder"),
            new HumanRigBone("RightShoulder", "Right shoulder"),
            new HumanRigBone("LeftUpperArm", "Left arm"),
            new HumanRigBone("RightUpperArm", "Right arm"),
            new HumanRigBone("LeftLowerArm", "Left elbow"),
            new HumanRigBone("RightLowerArm", "Right elbow"),
            new HumanRigBone("LeftHand", "Left wrist"),
            new HumanRigBone("RightHand", "Right wrist"),
            new HumanRigBone("LeftToes", "Left toe"),
            new HumanRigBone("RightToes", "Right toe"),
            new HumanRigBone("LeftEye", "LeftEye"),
            new HumanRigBone("RightEye", "RightEye"),
            new HumanRigBone("Left Thumb Proximal", "Thumb0_L"),
            new HumanRigBone("Left Thumb Intermediate", "Thumb1_L"),
            new HumanRigBone("Left Thumb Distal", "Thumb2_L"),
            new HumanRigBone("Left Index Proximal", "IndexFinger1_L"),
            new HumanRigBone("Left Index Intermediate", "IndexFinger2_L"),
            new HumanRigBone("Left Index Distal", "IndexFinger3_L"),
            new HumanRigBone("Left Middle Proximal", "MiddleFinger1_L"),
            new HumanRigBone("Left Middle Intermediate", "MiddleFinger2_L"),
            new HumanRigBone("Left Middle Distal", "MiddleFinger3_L"),
            new HumanRigBone("Left Ring Proximal", "RingFinger1_L"),
            new HumanRigBone("Left Ring Intermediate", "RingFinger2_L"),
            new HumanRigBone("Left Ring Distal", "RingFinger3_L"),
            new HumanRigBone("Left Little Proximal", "LittleFinger1_L"),
            new HumanRigBone("Left Little Intermediate", "LittleFinger2_L"),
            new HumanRigBone("Left Little Distal", "LittleFinger3_L"),
            new HumanRigBone("Right Thumb Proximal", "Thumb0_R"),
            new HumanRigBone("Right Thumb Intermediate", "Thumb1_R"),
            new HumanRigBone("Right Thumb Distal", "Thumb2_R"),
            new HumanRigBone("Right Index Proximal", "IndexFinger1_R"),
            new HumanRigBone("Right Index Intermediate", "IndexFinger2_R"),
            new HumanRigBone("Right Index Distal", "IndexFinger3_R"),
            new HumanRigBone("Right Middle Proximal", "MiddleFinger1_R"),
            new HumanRigBone("Right Middle Intermediate", "MiddleFinger2_R"),
            new HumanRigBone("Right Middle Distal", "MiddleFinger3_R"),
            new HumanRigBone("Right Ring Proximal", "RingFinger1_R"),
            new HumanRigBone("Right Ring Intermediate", "RingFinger2_R"),
            new HumanRigBone("Right Ring Distal", "RingFinger3_R"),
            new HumanRigBone("Right Little Proximal", "LittleFinger1_R"),
            new HumanRigBone("Right Little Intermediate", "LittleFinger2_R"),
            new HumanRigBone("Right Little Distal", "LittleFinger3_R"),
        };

        public static HumanBone GetHumanBone(string humanBoneName, Transform transformToSearch)
        {
            HumanRigBone hrb = Bones.Find(o => o.humanBoneName.ToLower() == humanBoneName.ToLower());
            if(hrb)
            {
                for(int i = 0; i < hrb.boneNames.Length; i++)
                {
                    string s = hrb.boneNames[i];
                    Transform t = transformToSearch.FindDeepChild(s);
                    if(t)
                    {
                        HumanBone hb = new HumanBone()
                        {
                            boneName = t.name,
                            humanName = hrb.humanBoneName,
                            limit = hrb.humanLimit,
                        };
                        return hb;
                    }
                }
            }
            return default(HumanBone);
        }
    }

    public class HumanRigBone
    {
        public string humanBoneName;
        public string[] boneNames;
        public HumanLimit humanLimit = new HumanLimit()
        {
            useDefaultValues = true
        };

        public HumanRigBone(string humanBoneName, params string[] boneNames)
        {
            this.humanBoneName = humanBoneName;
            this.boneNames = boneNames;
        }

        public HumanRigBone(string humanBoneName, HumanLimit humanLimit, params string[] boneNames) : this(humanBoneName, boneNames)
        {
            this.humanLimit = humanLimit;
        }

        public static implicit operator bool(HumanRigBone hrb)
        {
            if(hrb == null)
                return false;
            return true;
        }
    }

    public class PumkinsMuscleDefinitions
    {
        public static readonly Vector2Int bodyRange = new Vector2Int(0, 8);
        public static readonly Vector2Int headRange = new Vector2Int(9, 20);

        public static readonly Vector2Int leftLegRange = new Vector2Int(21, 28);
        public static readonly Vector2Int rightLegRange = new Vector2Int(29, 36);
        public static readonly Vector2Int legsRange = new Vector2Int(leftLegRange.x, rightLegRange.y);

        public static readonly Vector2Int leftArmRange = new Vector2Int(37, 45);
        public static readonly Vector2Int rightArmRange = new Vector2Int(46, 54);
        public static readonly Vector2Int armsRange = new Vector2Int(leftArmRange.x, rightArmRange.y);

        public static readonly Vector2Int leftFingersRange = new Vector2Int(55, 74);
        public static readonly Vector2Int rightFingersRange = new Vector2Int(75, 94);
        public static readonly Vector2Int fingersRange = new Vector2Int(leftFingersRange.x, rightFingersRange.y);

        public static string[] Body
        {
            get { return HumanTrait.MuscleName.SubArray(bodyRange.x, bodyRange.y - bodyRange.x); }
        }
        public static string[] Head
        {
            get { return HumanTrait.MuscleName.SubArray(headRange.x, headRange.y - headRange.x); }
        }
        public static string[] LeftLeg
        {
            get { return HumanTrait.MuscleName.SubArray(leftLegRange.x, leftLegRange.y - leftLegRange.x); }
        }
        public static string[] RightLeg
        {
            get { return HumanTrait.MuscleName.SubArray(rightLegRange.x, rightLegRange.y - rightLegRange.x); }
        }
        public static string[] LeftArm
        {
            get { return HumanTrait.MuscleName.SubArray(leftArmRange.x, leftArmRange.y - leftArmRange.x); }
        }
        public static string[] RightArm
        {
            get { return HumanTrait.MuscleName.SubArray(rightArmRange.x, rightArmRange.y - rightArmRange.x); }
        }
        public static string[] LeftFingers
        {
            get { return HumanTrait.MuscleName.SubArray(leftFingersRange.x, leftFingersRange.y - leftFingersRange.x); }
        }
        public static string[] RightFingers
        {
            get { return HumanTrait.MuscleName.SubArray(rightFingersRange.x, rightFingersRange.y - rightFingersRange.x); }
        }
    }
}