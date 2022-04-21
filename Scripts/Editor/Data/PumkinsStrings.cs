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
        public const string TOOLS_VERSION_STRING = "1.0";
        public const double toolsVersion = 1.0;

        public const string POSE_EDITOR_VERSION_NUMBER = "0.1.3b - Work in Progress";
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
            Settings.Reload();
            Thumbnails.Reload();
            AvatarInfo.Reload();
        }

        public static class Main
        {
            public static string title = "Pumkin's Avatar Tools";
            public static string windowName = "Pumkin Tools";
            public static string version = "_Version";
            public static string avatar = "_Avatar";
            public static string tools = "_Tools";
            public static string copier = "_Copier";
            public static string removeAll = "_Remove Components";
            public static string avatarInfo = "_Avatar Info";
            public static string thumbnails = "_Thumbnails";
            public static string avatarTesting = "_Avatar Testing";

            public static string info = "_Info";
            public static string useSceneSelection = "_Use Scene Selection";
            public static string experimental = "_Experimental";

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
                info = Translation.main.info;
                thumbnails = Translation.main.thumbnails;
                useSceneSelection = Translation.main.useSceneSelection;
                experimental = Translation.main.experimental;
                avatarTesting = Translation.main.avatarTesting;
            }
        }
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
            public static string selectFolder = "_Select Folder";
            public static string ok = "_Ok";
            public static string moveToEyes = "_Move to Eyes";
            public static string toggleMaterialPreview = "_Toggle Material Preview";

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
                selectFolder = Translation.buttons.selectFolder;
                ok = Translation.buttons.ok;
                moveToEyes = Translation.buttons.moveToEyes;
                toggleMaterialPreview = Translation.buttons.toggleMaterialPreview;
            }
        }
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
            public static string setMeshRendererAnchors = "_Set Mesh Renderer Anchors";
            public static string setSkinnedMeshRendererAnchors = "_Set Skinned Mesh Renderer Anchors";
            public static string viewpointZDepth = "_Z Depth";
            public static string revertScale = "_Revert Scale";
            public static string editScaleMoveViewpoint = "_Move Viewpoint";
            public static string enablePhysBones = "_Enable PhysBones";
            public static string disablePhysBones = "_Disable PhysBones";
            public static string togglePhysBones = "_Toggle PhysBones";
            public static string fixPhysBoneScripts = "_Fix Missing PhysBone Scripts in Prefab";
            public static string enableDynamicBones = "_Enable DynamicBones";
            public static string disableDynamicBones = "_Disable DynamicBones";
            public static string toggleDynamicBones = "_Toggle DynamicBones";
            public static string fixDynamicBoneScripts = "_Fix Missing DynamicBone Scripts in Prefab";
            public static string anchorPath = "_Anchor Path";
            public static string humanoidBone = "_Humanoid Bone";
            public static string anchorUsePath = "_Use Hierarchy Path"; 
            public static string fillEyeBones = "_Fill Eye Bones";
            public static string resetBoundingBoxes = "_Reset Bounding Boxes";
            public static string setImportSettings = "_Set Import Settings";

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
                setMeshRendererAnchors = Translation.tools.setRendererAnchors;
                setSkinnedMeshRendererAnchors = Translation.tools.setSkinnedMeshRendererAnchors;
                revertScale = Translation.tools.revertScale;
                editScaleMoveViewpoint = Translation.tools.editScaleMoveViewpoint;
                enablePhysBones = Translation.tools.enablePhysBones;
                disablePhysBones = Translation.tools.disablePhysBones;
                togglePhysBones = Translation.tools.togglePhysBones;
                fixPhysBoneScripts = Translation.tools.fixPhysBoneScripts;
                enableDynamicBones = Translation.tools.enableDynamicBones;
                disableDynamicBones = Translation.tools.disableDynamicBones;
                toggleDynamicBones = Translation.tools.toggleDynamicBones;
                fixDynamicBoneScripts = Translation.tools.fixDynamicBoneScripts;
                anchorPath = Translation.tools.anchorPath;
                humanoidBone = Translation.tools.humanoidBone;
                anchorUsePath = Translation.tools.anchorUsePath;
                fillEyeBones = Translation.tools.fillEyeBones;
                setImportSettings = Translation.tools.setImportSettings;

            }
        }
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

            public static string bones = "_Bones: {0} - {1}";
            public static string skinnedMeshRenderers = "_Skinned Mesh Renderers: {0} ({1}) - {2}";
            public static string meshRenderers = "_Mesh Renderers: {0} ({1}) - {2}";
            public static string polygons = "_Polygons: {0} ({1}) - {2}";
            public static string usedMaterialSlots = "_Used Material Slots: {0} ({1}) - {2}";
            public static string physBoneTransforms = "_Phys Bone Transforms: {0} ({1}) - {2}";
            public static string physBoneColliders = "_Phys Bone Colliders: {0} ({1}) - {2}";
            public static string physBoneColliderTransforms = "_PhysCollider Affected Transforms: {0} ({1}) - {2}";
            public static string dynamicBoneTransforms = "_Dynamic Bone Transforms: {0} ({1}) - {2}";
            public static string dynamicBoneColliders = "_Dynamic Bone Colliders: {0} ({1}) - {2}";
            public static string dynamicBoneColliderTransforms = "_Collider Affected Transforms: {0} ({1}) - {2}";
            public static string particleSystems = "_Particle Systems: {0} ({1}) - {2}";
            public static string maxParticles = "_Max Particles: {0} ({1}) - {2}";

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
                physBoneTransforms = Translation.avatarInfo.physBoneTransforms;
                physBoneColliders = Translation.avatarInfo.physBoneColliders;
                physBoneColliderTransforms = Translation.avatarInfo.physBoneColliderTransforms;
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
            public static string overlayImagePath = "_Overlay Image Path";
            public static string imagePath = "_Image Path";
            public static string backgroundColor = "_Background Color";

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
                overlayImagePath = Translation.thumbnails.overlayImagePath;
                imagePath = Translation.thumbnails.imagePath;
                backgroundColor = Translation.thumbnails.backgroundColor;
            }
        }
        public static class Copier
        {
            public static string copyFrom = "_Copy From";

            public static string copySettings = "_Settings";
            public static string createMissing = "_Copy Missing Components";
            public static string emptyGameObjects = "_Empty GameObjects";
            public static string replaceOld = "_Replace Old";

            public static string transforms = "_Transforms";
            public static string transforms_position = "_Position";
            public static string transforms_rotation = "_Rotation";
            public static string transforms_scale = "_Scale";
            public static string transforms_createMissing = "_Create Missing";
            public static string transforms_avatarScale = "_Avatar Scale";
            public static string transforms_copyActiveState = "_Active State";
            public static string transforms_copyLayerAndTag = "_Layer and Tag";
            public static string physBones = "_Phys Bones";
            public static string physBones_colliders = "_Phys Bone Colliders";
            public static string physBones_removeOldBones = "_Remove Old Phys Bones";
            public static string physBones_removeOldColliders = "_Remove Old Phys Colliders";
            public static string physBones_createMissing = "_Create Missing Phys Bones";
            public static string physBones_adjustScale = "_Adjust Scale";
            public static string physBones_adjustScaleColliders = "_Adjust Scale";
            public static string dynamicBones = "_Dynamic Bones";
            public static string dynamicBones_colliders = "_Dynamic Bone Colliders";
            public static string dynamicBones_removeOldBones = "_Remove Old Dynamic Bones";
            public static string dynamicBones_removeOldColliders = "_Remove Old Colliders";
            public static string dynamicBones_createMissing = "_Create Missing Dynamic Bones";
            public static string dynamicBones_adjustScale = "_Adjust Scale";
            public static string dynamicBones_adjustScaleColliders = "_Adjust Scale";
            public static string colliders = "_Colliders";
            public static string colliders_box = "_Box Colliders";
            public static string colliders_capsule = "_Capsule Colliders";
            public static string colliders_sphere = "_Sphere Colliders";
            public static string colliders_mesh = "_Mesh Colliders";
            public static string colliders_adjustScale = "_Adjust Scale";
            public static string colliders_removeOld = "_Remove Old Colliders";
            public static string descriptor = "_Avatar Descriptor";
            public static string descriptor_pipelineId = "_Pipeline Id";
            public static string descriptor_animationOverrides = "_Animation Overrides";
            public static string descriptor_copyViewpoint = "_Viewpoint";
            public static string descriptor_playableLayers = "_Playable Layers";
            public static string descriptor_eyeLookSettings = "_Eye Look Settings";
            public static string descriptor_expressions = "_Expressions";
            public static string descriptor_colliders = "_Colliders";
            public static string skinMeshRender = "_Skinned Mesh Renderers";
            public static string skinMeshRender_materials = "_Materials";
            public static string skinMeshRender_blendShapeValues = "_BlendShape Values";
            public static string skinMeshRender_bounds = "_Bounds";
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
            public static string other_vrmSpringBones = "_VRM Spring Bones";
            public static string contactReceiver = "_Contact Receivers";
            public static string contactReceiver_removeOld = "_Remove Old Contact Receivers";
            public static string contactReceiver_createMissing = "_Create Missing Contact Receivers";
            public static string contactReceiver_adjustScale = "_Adjust Scale";
            public static string contactSender = "_Contact Senders";
            public static string contactSender_removeOld = "_Remove Old Contact Senders";
            public static string contactSender_createMissing = "_Create Missing Contact Senders";
            public static string contactSender_adjustScale = "_Adjust Scale";
            public static string aimConstraints = "_Aim Constraints";
            public static string lookAtConstraints = "_LookAt Constraints";
            public static string parentConstraints = "_Parent Constraints";
            public static string positionConstraints = "_Position Constraints";
            public static string rotationConstraints = "_Rotation Constraints";
            public static string scaleConstraints = "_Scale Constraints";
            public static string onlyIfHasValidSources = "_Only if has Valid Sources";
            public static string joints_fixed = "_Fixed Joint";
            public static string joints_hinge = "_Hinge Joint";
            public static string joints_spring = "_Spring Joint";
            public static string joints_character = "_Character Joint";
            public static string joints_configurable = "_Configurable Joint";
            public static string joints_removeOld = "_Remove Old Joints";
            
            public static string finalIK = "_FinalIK";
            public static string finalIK_fabrIK = "_FabrIK";
            public static string finalIK_aimIK = "_AimIK";
            public static string finalIK_ccdIK = "_CCDIK";
            public static string finalIK_rotationLimits = "_Rotation Limits";
            public static string finalIK_limbIK = "_LimbIK";
            public static string finalIK_fbtBipedIK = "_Full Body Biped IK";
            public static string finalIK_VRIK = "_VRIK";
            
            public static string cameras = "_Cameras";

            public static string exclusions = "_Exclusions";
            public static string includeChildren = "_Include Children";
            public static string size = "_Size";
            public static string showCommon = "_Show Common";
            public static string showAll = "_Show All";

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
                transforms_createMissing = Translation.copier.transforms_createMissing;
                transforms_avatarScale = Translation.copier.transforms_avatarScale;
                transforms_copyActiveState = Translation.copier.transforms_copyActiveState; 
                transforms_copyLayerAndTag = Translation.copier.transforms_copyLayerAndTag;
                physBones = Translation.copier.physBones;
                physBones_colliders = Translation.copier.physBones_colliders;
                physBones_removeOldBones = Translation.copier.physBones_removeOldBones;
                physBones_removeOldColliders = Translation.copier.physBones_removeOldColliders;
                physBones_createMissing = Translation.copier.physBones_createMissing;
                physBones_adjustScale = Translation.copier.physBones_adjustScale;
                physBones_adjustScaleColliders = Translation.copier.physBones_adjustScaleColliders;
                dynamicBones = Translation.copier.dynamicBones;
                dynamicBones_colliders = Translation.copier.dynamicBones_colliders;
                dynamicBones_removeOldBones = Translation.copier.dynamicBones_removeOldBones;
                dynamicBones_removeOldColliders = Translation.copier.dynamicBones_removeOldColliders;
                dynamicBones_createMissing = Translation.copier.dynamicBones_createMissing;
                dynamicBones_adjustScale = Translation.copier.dynamicBones_adjustScale;
                dynamicBones_adjustScaleColliders = Translation.copier.dynamicBones_adjustScaleColliders;
                colliders = Translation.copier.colliders;
                colliders_box = Translation.copier.colliders_box;
                colliders_capsule = Translation.copier.colliders_capsule;
                colliders_sphere = Translation.copier.colliders_sphere;
                colliders_mesh = Translation.copier.colliders_mesh;
                colliders_removeOld = Translation.copier.colliders_removeOld;
                colliders_adjustScale = Translation.copier.colliders_adjustScale;

                descriptor = Translation.copier.descriptor;
                descriptor_pipelineId = Translation.copier.descriptor_pipelineId;
                descriptor_animationOverrides = Translation.copier.descriptor_animationOverrides;
                descriptor_copyViewpoint = Translation.copier.descriptor_copyViewpoint;
                descriptor_playableLayers = Translation.copier.descriptor_playableLayers;
                descriptor_eyeLookSettings = Translation.copier.descriptor_eyeLookSettings;
                descriptor_expressions = Translation.copier.descriptor_expressions;
                descriptor_colliders = Translation.copier.descriptor_colliders;

                skinMeshRender = Translation.copier.skinMeshRender;
                skinMeshRender_materials = Translation.copier.skinMeshRender_materials;
                skinMeshRender_blendShapeValues = Translation.copier.skinMeshRender_blendShapeValues;
                skinMeshRender_bounds = Translation.copier.skinMeshRender_bounds;
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
                contactReceiver = Translation.copier.contactReceiver;
                contactReceiver_removeOld = Translation.copier.contactReceiver_removeOld;
                contactReceiver_createMissing = Translation.copier.contactReceiver_createMissing;
                contactReceiver_adjustScale = Translation.copier.contactReceiver_adjustScale;
                contactSender = Translation.copier.contactSender;
                contactSender_removeOld = Translation.copier.contactSender_removeOld;
                contactSender_createMissing = Translation.copier.contactSender_createMissing;
                contactSender_adjustScale = Translation.copier.contactSender_adjustScale;
                aimConstraints = Translation.copier.aimConstraints;
                lookAtConstraints = Translation.copier.lookAtConstraints;
                parentConstraints = Translation.copier.parentConstraints;
                positionConstraints = Translation.copier.positionConstraints;
                rotationConstraints = Translation.copier.rotationConstraints;
                scaleConstraints = Translation.copier.scaleConstraints;
                onlyIfHasValidSources = Translation.copier.onlyIfHasValidSources;
                other_vrmSpringBones = Translation.copier.other_vrmSpringBones;

                joints_fixed = Translation.copier.joints_fixed;
                joints_hinge = Translation.copier.joints_hinge;
                joints_spring = Translation.copier.joints_spring;
                joints_character = Translation.copier.joints_character;
                joints_configurable = Translation.copier.joints_configurable;
                joints_removeOld = Translation.copier.joints_removeOld;

                finalIK = Translation.copier.finalIK;
                finalIK_fabrIK = Translation.copier.finalIK_fabrIK;
                finalIK_aimIK = Translation.copier.finalIK_aimIK;
                finalIK_ccdIK = Translation.copier.finalIK_ccdIK;
                finalIK_rotationLimits = Translation.copier.finalIK_rotationLimits;
                finalIK_limbIK = Translation.copier.finalIK_limbIK;
                finalIK_fbtBipedIK = Translation.copier.finalIK_fbtBipedIK;
                finalIK_VRIK = Translation.copier.finalIK_VRIK;
                
                cameras = Translation.copier.cameras;

                exclusions = Translation.copier.ignoreList;
                includeChildren = Translation.copier.includeChildren;
                size = Translation.copier.size;
                showCommon = Translation.copier.showCommon;
                showAll = Translation.copier.showAll;
            }
        }
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
            public static string failedHasNoIgnoring = "_'{0}' has no '{1}', Ignoring";
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
            public static string hasMissingScriptDestroying = "_{0} has a missing script. Destroying";
            public static string copiedPhysBone = "_Copied PhysBone from {0}'s {1} to {2}'s {1}";
            public static string copiedDynamicBone = "_Copied DynamicBone from {0}'s {1} to {2}'s {1}";
            public static string invalidTranslation = "_Translation {0} is invalid";
            public static string constraintHasNoValidSources = "_{0}'s {1} has no valid sources. Destroying";
            public static string avatarHasNoPrefab = "_Selected Avatar has no prefab associated with it. Only prefabs can be fixed for now";
            public static string attemptingToFixPhysBoneScripts = "_Attempting to fix PhysBone Scripts";
            public static string attemptingToFixDynamicBoneScripts = "_Attempting to fix DynamicBone Scripts";
            public static string notSelectedInCopierIgnoring = "_{0}'s {1} is not selected in the copier. Ignoring";
            public static string exitPrefabModeFirst = "_Please exit prefab mode before doing this";
            public static string transformNotFound = "_Transform at '{0}' not found";
            public static string cantApplyPreset = "_Can't apply preset";

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
                copiedPhysBone = Translation.log.copiedPhysBone;
                copiedDynamicBone = Translation.log.copiedDynamicBone;
                invalidTranslation = Translation.log.invalidTranslation;
                constraintHasNoValidSources = Translation.log.constraintHasNoValidSources;
                avatarHasNoPrefab = Translation.log.avatarHasNoPrefab;
                attemptingToFixPhysBoneScripts = Translation.log.attemptingToFixPhysBoneScripts;
                attemptingToFixDynamicBoneScripts = Translation.log.attemptingToFixDynamicBoneScripts;
                notSelectedInCopierIgnoring = Translation.log.notSelectedInCopierIgnoring;
                exitPrefabModeFirst = Translation.log.exitPrefabModeFirst;
                transformNotFound = Translation.log.transformNotFound;
                cantApplyPreset = Translation.log.cantApplyPreset;
            }
        }
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
            public static string armatureScaleNotOne = "_Armature scale for selected avatar isn't 1! This can cause issues. Please re-export your avatar with CATS' export option";
            public static string armatureScalesDontMatch = "_Armature scales for selected avatars don't match!\nThis can cause issues";
            public static string noDBonesOrMissingScriptDefine = "_No DynamicBones found or missing script define";
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
                armatureScaleNotOne = Translation.warnings.armatureScaleNotOne;
                armatureScalesDontMatch = Translation.warnings.armatureScalesDontMatch;
                noDBonesOrMissingScriptDefine = Translation.warnings.noDBonesOrMissingScriptDefine;
                languageAlreadyExistsOverwrite = Translation.warnings.languageAlreadyExistsOverwrite;
            }
        }
        public static class Credits
        {
            public static string version = "_Version";
            public static string redundantStrings = "_Now with 100% more redundant strings";
            public static string addMoreStuff = "_I'll add more stuff to this eventually";
            public static string pokeOnDiscord = "_Poke me on Discord at Pumkin#9524";

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
        public static class Settings
        {
            public static string uwu = "_uwu";
            public static string searchForBones = "_Search for DynamicBones";
            public static string searchForPhysBones = "_Search for PhysBones";
            public static string language = "_Language";
            public static string refresh = "_Refresh";
            public static string importLanguage = "_Import Language";
            public static string enableVerboseLogging = "_Enable verbose logging";
            public static string sceneViewOverlayWindowsAtBottom = "_Draw scene view overlays at the bottom";
            public static string misc = "_Misc";
            public static string showExperimental = "_Show experimental features";
            public static string experimentalWarning = "_These features are unfinished and will probably cause issues";

            static Settings()
            {
                Reload();
            }

            public static void Reload()
            {
                if(Translation is null)
                    return;

                uwu = Translation.misc.uwu;
                searchForBones = Translation.misc.searchForBones;
                language = Translation.misc.language;
                refresh = Translation.misc.refresh;
                importLanguage = Translation.misc.importLanguage;
                enableVerboseLogging = Translation.misc.enableVerboseLogging;
                sceneViewOverlayWindowsAtBottom = Translation.misc.sceneViewOverlayWindowsAtBottom;
                misc = Translation.misc.misc;
                showExperimental = Translation.misc.showExperimental;
                experimentalWarning = Translation.misc.experimentalWarning;
            }
        }
    }
}
