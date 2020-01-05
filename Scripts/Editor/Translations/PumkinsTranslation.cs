using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Pumkin.Presets;

namespace Pumkin.Translations
{
    [Serializable, CreateAssetMenu
    (
        fileName = "Language_New",
        menuName = "Pumkin/Avatar Tools/Create Translation"
    )]
    public class PumkinsTranslation : ScriptableObject
    {
        public string languageName = "English";
        public string author = "Default";

        static PumkinsTranslation _default;
        public static PumkinsTranslation Default
        {
            get
            {
                if(!_default)
                {
                    var objArr = FindObjectsOfType<PumkinsTranslation>();
                    var obj = objArr.FirstOrDefault(o => o.ToString() == "English - Default") as PumkinsTranslation;
                    if(obj)
                        _default = obj;
                    else
                        _default = CreateInstance<PumkinsTranslation>();                    
                }
                return _default;
            }
        }

        public MainStrings main = new MainStrings();
        public ButtonStrings buttons = new ButtonStrings();
        public ToolStrings tools = new ToolStrings();
        public CopierStrings copier = new CopierStrings();
        public AvatarInfoStrings avatarInfo = new AvatarInfoStrings();
        public ThumbnailStrings thumbnails = new ThumbnailStrings();
        public MiscStrings misc = new MiscStrings();
        public LogStrings log = new LogStrings();
        public WarningStrings warnings = new WarningStrings();
        public CreditsStrings credits = new CreditsStrings();
        public PoseEditorStrings poseEditor = new PoseEditorStrings();
        public PresetStrings preset = new PresetStrings();

        public override string ToString()
        {
            string s = languageName;
            if(!string.IsNullOrEmpty(author))
                s += " - " + author;
            return s;
        }
    };

    [Serializable]
    public class PresetStrings
    {
        public string presetName = "Preset Name";
        public string mode = "Preset Mode";
        public string otherNames = "Other Names";
        public string poseMode = "Pose Name";
        public string editPosePreset = "Edit Pose Preset";
        public string createPosePreset = "Create Pose Preset";
        public string overwriteFile = "Overwrite File";
        public string transformDoesntBelongToAvatar = "{0} doesn't belong to avatar {1}";
        public string cameraPreset = "Camera Preset";
        public string posePreset = "Pose Preset";
        public string blendshapePreset = "Blendshape Preset";
        public string editBlendshapePreset = "Edit Blendshape Preset";
        public string createBlendshapePreset = "Create Blendshape Preset";
        public string transform = "Transform";
        public string offsetMode = "Offset Mode";
        public string camera = "Camera";
        public string editCameraPreset = "Edit Camera Preset";
        public string createCameraPreset = "Create Camera Preset";        
    }

    [Serializable]
    public class MainStrings
    {
        public string title = "Pumkin's Avatar Tools";
        public string windowName = "Pumkin Tools";
        public string version = "Version";
        public string tools = "Tools";
        public string removeAll = "Remove All";
        public string copier = "Copy Components";
        public string avatarInfo = "Avatar Info";
        public string thumbnails = "Thumbnails";
        public string misc = "Misc";
        public string avatar = "Avatar";
        public string useSceneSelection = "Use Scene Selection";
    };

    [Serializable]
    public class ButtonStrings
    {
        public string selectFromScene = "Select from Scene";
        public string copySelected = "Copy Selected";
        public string refresh = "Refresh";
        public string cancel = "Cancel";
        public string apply = "Apply";
        public string copy = "Copy Text";
        public string openHelpPage = "Open Help Page";
        public string openGithubPage = "Open Github Page";
        public string openDonationPage = "Buy me a Ko-Fi~";
        public string openPoseEditor = "Open Pose Editor";
        public string joinDiscordServer = "Join Discord Server!";
        public string selectNone = "Select None";
        public string selectAll = "Select All";
        public string browse = "Browse";
        public string setFromCamera = "Set from Camera";
        public string reset = "Reset";
        public string edit = "Edit";
        public string load = "Load";
        public string createNewPreset = "Create New Preset";
        public string quickSetupAvatar = "Quick Setup Avatar";
        public string selectInToolsWindow  = "Select in Tools Window";
        public string resetRenderer = "Reset Renderer";
        public string revertRenderer = "Revert Renderer";
        public string alignCameraToView = "Align Camera to View";
        public string savePreset = "Save Preset";
        public string selectInAssets = "Select in Assets";
        public string openFolder = "Open Folder";
    };

    [Serializable]
    public class ToolStrings
    {
        public string fillVisemes = "Fill Visemes";
        public string editViewpoint = "Edit Viewpoint";
        public string revertBlendshapes = "Revert Blendshapes";
        public string zeroBlendshapes = "Zero Blendshapes";
        public string resetPose = "Reset Pose";
        public string resetToTPose = "Reset to T-Pose";
        public string editScale = "Edit Scale";
        public string autoViewpoint = "Auto Viewpoint";
        public string setTPose = "Force TPose";
        public string setRendererAnchors = "Set Renderer Anchors";
        public string viewpointZDepth = "Z Depth";
        public string revertScale = "Revert Scale";
    };

    [Serializable]
    public class CopierStrings
    {
        public string copyFrom = "Copy From";

        public string copySettings = "Settings";
        public string createMissing = "Copy Missing";
        public string emptyGameObjects = "Empty GameObjects";
        public string replaceOld = "Replace Old";

        public string transforms = "Transforms";
        public string transforms_position = "Position";
        public string transforms_rotation = "Rotation";
        public string transforms_scale = "Scale";
        public string transforms_avatarScale = "Avatar Scale";
        public string dynamicBones = "Dynamic Bones";
        public string dynamicBones_colliders = "Colliders";
        public string dynamicBones_removeOldBones = "Remove Old Bones";
        public string dynamicBones_removeOldColliders = "Remove Old Colliders";
        public string dynamicBones_createMissing = "Copy Missing Bones";        
        public string colliders = "Colliders";
        public string colliders_box = "Box Colliders";
        public string colliders_capsule = "Capsule Colliders";
        public string colliders_sphere = "Sphere Colliders";
        public string colliders_mesh = "Mesh Colliders";
        public string colliders_removeOld = "Remove Old Colliders";
        public string descriptor = "Avatar Descriptor";
        public string descriptor_pipelineId = "Pipeline Id";
        public string descriptor_animationOverrides = "Animation Overrides";
        public string descriptor_copyViewpoint = "Viewpoint";
        public string skinMeshRender = "Skinned Mesh Renderers";
        public string skinMeshRender_materials = "Materials";
        public string skinMeshRender_blendShapeValues = "BlendShape Values";
        public string particleSystems = "Particle Systems";
        public string rigidBodies = "Rigid Bodies";
        public string trailRenderers = "Trail Renderers";
        public string meshRenderers = "Mesh Renderers";
        public string copyGameObjects = "Copy GameObjects";
        public string copyColliderObjects = "Copy Collider Objects";
        public string lights = "Lights";
        public string animators = "Animators";
        public string copyMainAnimator = "Copy Main Animator";
        public string animators_inChildren = "Child Animators";
        public string audioSources = "Audio Sources";
        public string joints = "Joints";

        public string ignoreList = "Ignore List";
        public string includeChildren = "Include Children";
        public string size = "Size";
    };

    [Serializable]
    public class AvatarInfoStrings
    {
        public string name = "{0}";
        public string line = "---------------------";
        public string gameObjects = "GameObjects: {0} ({1})";
        public string bones = "Bones: {0} - {1}";
        public string skinnedMeshRenderers = "Skinned Mesh Renderers: {0} ({1}) - {2}";
        public string meshRenderers = "Mesh Renderers: {0} ({1}) - {2}";
        public string polygons = "Polygons: {0} ({1}) - {2}";
        public string usedMaterialSlots = "Used Material Slots: {0} ({1}) - {2}";
        public string uniqueMaterials = "Unique Materials: {0} ({1})";
        public string shaders = "Shaders: {0}";
        public string dynamicBoneTransforms = "Dynamic Bone Transforms: {0} ({1}) - {2}";
        public string dynamicBoneColliders = "Dynamic Bone Colliders: {0} ({1}) - {2}";
        public string dynamicBoneColliderTransforms = "Collider Affected Transforms: {0} ({1}) - {2}";
        public string particleSystems = "Particle Systems: {0} ({1}) - {2}";
        public string maxParticles = "Max Particles: {0} ({1}) - {2}";
        public string overallPerformance = "Overall Performance: {0}";
        public string selectAvatarFirst = "Select an Avatar first";
    };

    [Serializable]
    public class ThumbnailStrings
    {
        public string overlayCameraImage = "Overlay Image";
        public string overlayTexture = "Overlay Texture";
        public string startUploadingFirst = "Start uploading an Avatar, or get into Play mode";        
        public string backgroundType = "Background Type";
        public string backgroundType_None = "None";
        public string backgroundType_Material = "Skybox";
        public string backgroundType_Color = "Color";
        public string backgroundType_Image = "Image";
        public string hideOtherAvatars = "Hide Other Avatars when Uploading";
        public string tint = "Tint";
        public string useCameraOverlay = "Use Camera Overlay";
        public string useCameraBackground = "Use Camera Background";
        public string selectedCamera = "Selected Camera";
        public string offset = "Offset";        
        public string blendshapes = "Blendshapes";
        public string poses = "Poses";
        public string cameras = "Cameras";
        public string centerCameraFixClippingPlanes = "Fix Clipping Planes";        
        public string positionOffset = "Position Offset";
        public string rotationOffset = "Rotation Offset";
        public string tryFixPoseSinking = "Try to Fix Pose Sinking";
        public string centerCameraOn = "Center Camera on {0}";
        public string viewpoint = "Viewpoint";

    };

    [Serializable]
    public class LogStrings
    {
        public string done = "Done";
        public string cancelled = "Cancelled";
        public string nothingSelected = "Select something first";
        public string cantCopyToSelf = "Can't copy Components from an object to itself. What are you doing?";
        public string copyAttempt = "Attempting to copy {0} from {1} to {2}";
        public string removeAttempt = "Attempting to remove {0} from {1}";
        public string copyFromInvalid = "Can't copy Components because 'Copy From' is invalid";
        public string viewpointApplied = "Set Viewposition to {0}";
        public string viewpointCancelled = "Cancelled Viewposition changes";
        public string tryFillVisemes = "Attempting to fill visemes on {0}";
        public string noSkinnedMeshFound = "Failed: No skinned mesh found";
        public string descriptorIsNull = "Avatar descriptor is null";
        public string success = "Success";
        public string meshHasNoVisemes = "Failed. Mesh has no Visemes. Set to Default";
        public string tryRemoveUnsupportedComponent = "Attempting to remove unsupported component {0} from {1}";
        public string failed = "Failed";
        public string failedIsNull = "Failed: {1} is null";
        public string nameIsEmpty = "Name is Empty";
        public string loadedPose = "Loaded Pose: {0}";
        public string loadedBlendshapePreset = "Loaded Blendshapes: {0}";
        public string failedDoesntHave = "Failed: {0} doesn't have a {1}";
        public string failedAlreadyHas = "Failed: {0} already has a {1}";
        public string loadedCameraOverlay = "Loaded {0} as Camera Overlay";
        public string failedHasNo = "{0} has no {1}, Ignoring.";
        public string settingQuickViewpoint = "Setting quick viewpoint to {0}";
        public string cantSetViewpointNonHumanoid = "Can't set Viewpoint for a non humanoid avatar";
        public string setAvatarScaleTo = "Set Avatar scale to {0} and Viewpoint to {1}";
        public string setAvatarScaleAndViewpointTo = "Set Avatar scale to {0} and Viewpoint to {1}";
        public string canceledScaleChanges = "Cancelled Scale changes";
        public string successCopiedOverFromTo = "Success: Copied over {0} from {1}'s {2} to {3}'s {4}";
        public string hasNoComponentsOrChildrenDestroying = "{0} has no components or children. Destroying";
        public string cantBeDestroyedPartOfPrefab = "{0}'s {1} can't be destroyed because it's part of a prefab instance. Ignoring";
        public string meshPrefabMissingCantRevertBlednshapes = "Mesh prefab is missing, can't revert to default blendshapes.";
        public string meshPrefabMissingCantRevertPose = "Mesh prefab is missing, can't revert to default pose";
        public string runtimeBlueprintNotFoundStartUploading = "RuntimeBlueprintCreation script not found. Start uploading an avatar to use this";
        public string failedToCenterCameraNoDescriptor = "Failed to center camera on Viewpoint. Avatar descriptor not found";
        public string cantSetPoseNonHumanoid = "Can't set humanoid pose {0} on a non humanoid avatar";
        public string loadedImageAsBackground = "Loaded {0} as Background image";
        public string loadedImageAsOverlay = "Loaded {0} as Overlay image";
        public string setProbeAnchorTo = "Set {0}'s probe anchor to {1}";
    };

    [Serializable]
    public class WarningStrings
    {
        public string warn = "Warning";
        public string notFound = "Not Found";
        public string oldVersion = "Old Version";
        public string selectSceneObject = "Please select an object from the scene";
        public string cameraNotFound = "Camera not found";
        public string invalidPreset = "Can't apply preset {0}: Invalid Preset";
        public string cantRevertRendererWithoutPrefab = "Can't revert Skinned Mesh Renderer {0}, object has no Prefab.";
        public string cantLoadImageAtPath = "Can't load image at {0}";
    };

    [Serializable]
    public class CreditsStrings
    {        
        public string version = "Version";
        public string redundantStrings = "Now with 100% more redundant strings";        
        public string addMoreStuff = "I'll add more stuff to this eventually";
        public string pokeOnDiscord = "Poke me on Discord at Pumkin#2020";
    };

    [Serializable]
    public class MiscStrings
    {
        public string uwu = "uwu";
        public string searchForBones = "Search for DynamicBones";
        public string superExperimental = "Super Experimental Stuff";
        public string language = "Language";
        public string refresh = "Refresh";
    };

    [Serializable]
    public class PoseEditorStrings
    {
        public string title = "Pumkin's Pose Editor";
        public string scene = "Scene";
        public string sceneLoadAdditive = "Load Additive";
        public string sceneOverrideLights = "Override Lights";
        public string avatarPosition = "Avatar Position";
        public string avatarPositionOverridePose = "Override Pose";
        public string avatarPositionOverrideBlendshapes = "Override Blendshapes";
        public string sceneSaveChanges = "Save Scene Changes";
        public string unloadScene = "Unload Scene";
        public string resetPosition = "Reset Position";
        public string pose = "Pose";
        public string newPose = "New Pose";
        public string onlySavePoseChanges = "Only Save Pose Changes";
        public string loadPose = "Load Pose";
        public string blendshapes = "Blendshapes";
        public string newPreset = "New Preset";
        public string loadPreset = "Load Preset";
        public string saveButton = "Save";
        public string reloadButton = "Reload";
        public string bodyPositionYTooSmall = "humanPose.bodyPosition.y is {0}, you probably don't want that. Setting humanPose.bodyPosition.y to 1";
        public string muscles = "Muscles";
        public string transformRotations = "Transform Rotations";
        public string selectHumanoidAvatar = "Select a Humanoid Avatar";
        public string animationTime = "Time";
        public string poseFromAnimation = "Pose from Animation";
    }
}