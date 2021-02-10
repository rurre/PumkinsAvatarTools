using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using System;
using System.IO;
using UnityEngine;
using System.Linq;
using Pumkin.DependencyChecker;
using Pumkin.PoseEditor;
using UnityEngine.UI;
using UnityEditorInternal;
using Pumkin.DataStructures;
using Pumkin.HelperFunctions;
using Pumkin.Extensions;
using UnityEngine.SceneManagement;
using Pumkin.Presets;
using UnityEngine.Animations;
using Pumkin.YAML;
using UnityEditor.Experimental.SceneManagement;

#if VRC_SDK_VRCSDK2 || (VRC_SDK_VRCSDK3 && !UDON)
using VRC.Core;
using VRC.SDKBase;
#endif

#if VRC_SDK_VRCSDK3 && !UDON
using VRC_AvatarDescriptor = VRC.SDK3.Avatars.Components.VRCAvatarDescriptor;
#elif VRC_SDK_VRCSDK2
using VRC_AvatarDescriptor = VRCSDK2.VRC_AvatarDescriptor;
#endif

/// <summary>
/// PumkinsAvatarTools by, well, Pumkin
/// https://github.com/rurre/PumkinsAvatarTools
/// </summary>

namespace Pumkin.AvatarTools
{
    [ExecuteInEditMode, CanEditMultipleObjects, Serializable]
    public class PumkinsAvatarTools : EditorWindow
    {
#region Variables

#region Tools

        [SerializeField] private static GameObject _selectedAvatar; // use property

        [SerializeField] static bool _useSceneSelectionAvatar = false;

        //Quick Setup

        [SerializeField] bool _tools_quickSetup_settings_expand = false;
        [SerializeField] bool _tools_quickSetup_fillVisemes = true;
        [SerializeField] bool _tools_quickSetup_setViewpoint = true;
        //[SerializeField] bool _tools_quickSetup_autoRig = true;
        [SerializeField] bool _tools_quickSetup_forceTPose = false;

        [SerializeField] float _tools_quickSetup_viewpointZDepth = 0.06f;

        [SerializeField] bool _tools_quickSetup_setSkinnedMeshRendererAnchor = true;
        [SerializeField] bool _tools_quickSetup_setMeshRendererAnchor = true;
        [SerializeField] string _tools_quickSetup_setRenderAnchor_path = "Armature/Hips/Spine";

        //Editing Viewpoint
        bool _editingView = false;
        Vector3 _viewPosOld;
        Vector3 _viewPosTemp;
        Tool _tempToolOld = Tool.None;
        public static readonly Vector3 DEFAULT_VIEWPOINT = new Vector3(0, 1.6f, 0.2f);

        //Editing Scale
        bool _editingScale = false;
        Vector3 _avatarScaleOld;
        [SerializeField] float _avatarScaleTemp;
        [SerializeField] SerializedProperty _serializedAvatarScaleTempProp;
        [SerializeField] bool editingScaleMovesViewpoint = true;
        Transform _scaleViewpointDummy;

#if VRC_SDK_VRCSDK2 || (VRC_SDK_VRCSDK3 && !UDON)
        VRC_AvatarDescriptor _tempAvatarDescriptor;
        bool _tempAvatarDescriptorWasAdded = false;
#endif

        //Dynamic Bones
        bool _nextToggleDBoneState = false;
#if PUMKIN_DBONES || PUMKIN_OLD_DBONES
        List<DynamicBone> _dBonesThatWereAlreadyDisabled = new List<DynamicBone>();
#else
        List<object> _dBonesThatWereAlreadyDisabled = new List<object>();
#endif

        enum ToolMenuActions
        {
            RemoveDynamicBones,
            RemoveDynamicBoneColliders,
            RemoveColliders,
            RemoveRigidBodies,
            RemoveEmptyGameObjects,
            RemoveParticleSystems,
            RemoveTrailRenderers,
            RemoveMeshRenderers,
            RemoveAudioSources,
            RemoveLights,
            RemoveAnimatorsInChildren,
            RemoveJoints,
            ResetPose,
            RevertBlendshapes,
            ZeroBlendshapes,
            EditViewpoint,
            FillVisemes,
            SetTPose,
            EditScale,
            RevertScale,
            RemoveIKFollowers,
            RemoveMissingScripts,
            RemoveLookAtConstraint,
            RemoveParentConstraint,
            RemoveRotationConstraint,
            RemoveAimConstraint,
            RemoveScaleConstraint,
            RemovePositionConstraint,
            FixDynamicBoneScripts,
            FillEyeBones,
            ResetBoundingBoxes,
        }

        readonly static string DUMMY_NAME = "_Dummy";
        readonly static string VIEWPOINT_DUMMY_NAME = "_PumkinsViewpointDummy";
        readonly static string SCALE_RULER_DUMMY_NAME = "_PumkinsScaleRuler";

#endregion

#region Component Copier

        [SerializeField] private static GameObject _copierSelectedFrom;

        [SerializeField] bool bCopier_transforms_copy = true;
        [SerializeField] bool bCopier_transforms_copyPosition = false;
        [SerializeField] bool bCopier_transforms_copyRotation = false;
        [SerializeField] bool bCopier_transforms_copyScale = false;

        [SerializeField] bool bCopier_dynamicBones_copy = true;
        [SerializeField] bool bCopier_dynamicBones_copySettings = false;
        [SerializeField] bool bCopier_dynamicBones_createMissing = true;
        [SerializeField] bool bCopier_dynamicBones_createObjects = false;

        [SerializeField] bool bCopier_dynamicBones_copyColliders = true;
        [SerializeField] bool bCopier_dynamicBones_removeOldColliders = false;
        [SerializeField] bool bCopier_dynamicBones_removeOldBones = false;
        [SerializeField] bool bCopier_dynamicBones_createObjectsColliders = true;

        [SerializeField] bool bCopier_descriptor_copy = true;
        [SerializeField] bool bCopier_descriptor_copySettings = true;
        [SerializeField] bool bCopier_descriptor_copyPipelineId = false;
        [SerializeField] bool bCopier_descriptor_copyViewpoint = true;
        [SerializeField] bool bCopier_descriptor_copyAvatarScale = true;

        [SerializeField] bool bCopier_descriptor_copyAnimationOverrides = true;

        [SerializeField] bool bCopier_descriptor_copyEyeLookSettings = true;
        [SerializeField] bool bCopier_descriptor_copyPlayableLayers = true;
        [SerializeField] bool bCopier_descriptor_copyExpressions = true;

        [SerializeField] bool bCopier_colliders_copy = true;
        [SerializeField] bool bCopier_colliders_removeOld = false;
        [SerializeField] bool bCopier_colliders_copyBox = true;

        [SerializeField] bool bCopier_colliders_copyCapsule = true;
        [SerializeField] bool bCopier_colliders_copySphere = true;
        [SerializeField] bool bCopier_colliders_copyMesh = false;
        [SerializeField] bool bCopier_colliders_createObjects = true;

        [SerializeField] bool bCopier_skinMeshRender_copy = true;
        [SerializeField] bool bCopier_skinMeshRender_copySettings = true;
        [SerializeField] bool bCopier_skinMeshRender_copyBlendShapeValues = true;
        [SerializeField] bool bCopier_skinMeshRender_copyMaterials = false;

        [SerializeField] bool bCopier_particleSystems_copy = true;
        [SerializeField] bool bCopier_particleSystems_replace = false;
        [SerializeField] bool bCopier_particleSystems_createObjects = true;

        [SerializeField] bool bCopier_rigidBodies_copy = true;
        [SerializeField] bool bCopier_rigidBodies_copySettings = true;
        [SerializeField] bool bCopier_rigidBodies_createMissing = true;
        [SerializeField] bool bCopier_rigidBodies_createObjects = true;

        [SerializeField] bool bCopier_trailRenderers_copy = true;
        [SerializeField] bool bCopier_trailRenderers_copySettings = true;
        [SerializeField] bool bCopier_trailRenderers_createMissing = true;
        [SerializeField] bool bCopier_trailRenderers_createObjects = true;

        [SerializeField] bool bCopier_meshRenderers_copy = true;
        [SerializeField] bool bCopier_meshRenderers_copySettings = true;
        [SerializeField] bool bCopier_meshRenderers_createMissing = true;
        [SerializeField] bool bCopier_meshRenderers_createObjects = true;

        [SerializeField] bool bCopier_lights_copy = true;
        [SerializeField] bool bCopier_lights_copySettings = true;
        [SerializeField] bool bCopier_lights_createMissing = true;
        [SerializeField] bool bCopier_lights_createObjects = true;

        [SerializeField] bool bCopier_animators_copy = true;
        [SerializeField] bool bCopier_animators_copySettings = true;
        [SerializeField] bool bCopier_animators_createMissing = true;
        [SerializeField] bool bCopier_animators_createObjects = false;
        [SerializeField] bool bCopier_animators_copyMainAnimator = false;

        [SerializeField] bool bCopier_joints_copy = true;
        [SerializeField] bool bCopier_joints_fixed = true;
        [SerializeField] bool bCopier_joints_character = true;
        [SerializeField] bool bCopier_joints_configurable = true;
        [SerializeField] bool bCopier_joints_spring = true;
        [SerializeField] bool bCopier_joints_hinge = true;
        [SerializeField] bool bCopier_joints_createObjects = true;
        [SerializeField] bool bCopier_joints_removeOld = true;

        [SerializeField] bool bCopier_audioSources_copy = true;
        [SerializeField] bool bCopier_audioSources_copySettings = true;
        [SerializeField] bool bCopier_audioSources_createMissing = true;
        [SerializeField] bool bCopier_audioSources_createObjects = true;

        [SerializeField] bool bCopier_other_copy = true;
        [SerializeField] bool bCopier_other_copyIKFollowers = true;
        [SerializeField] bool bCopier_other_copyVRMSpringBones = true;
        [SerializeField] bool bCopier_other_createGameObjects = true;

        [SerializeField] bool bCopier_aimConstraint_copy = true;
        [SerializeField] bool bCopier_aimConstraint_replaceOld = true;
        [SerializeField] bool bCopier_aimConstraint_createObjects = true;
        [SerializeField] bool bCopier_aimConstraint_onlyIfHasValidSources = true;

        [SerializeField] bool bCopier_lookAtConstraint_replaceOld = true;
        [SerializeField] bool bCopier_lookAtConstraint_createObjects = true;
        [SerializeField] bool bCopier_lookAtConstraint_copy = true;
        [SerializeField] bool bCopier_lookAtConstraint_onlyIfHasValidSources = true;

        [SerializeField] bool bCopier_parentConstraint_replaceOld = true;
        [SerializeField] bool bCopier_parentConstraint_createObjects = true;
        [SerializeField] bool bCopier_parentConstraint_copy = true;
        [SerializeField] bool bCopier_parentConstraint_onlyIfHasValidSources = true;

        [SerializeField] bool bCopier_positionConstraint_replaceOld = true;
        [SerializeField] bool bCopier_positionConstraint_createObjects = true;
        [SerializeField] bool bCopier_positionConstraint_copy = true;
        [SerializeField] bool bCopier_positionConstraint_onlyIfHasValidSources = true;

        [SerializeField] bool bCopier_rotationConstraint_replaceOld = true;
        [SerializeField] bool bCopier_rotationConstraint_createObjects = true;
        [SerializeField] bool bCopier_rotationConstraint_copy = true;
        [SerializeField] bool bCopier_rotationConstraint_onlyIfHasValidSources = true;

        [SerializeField] bool bCopier_scaleConstraint_replaceOld = true;
        [SerializeField] bool bCopier_scaleConstraint_createObjects = true;
        [SerializeField] bool bCopier_scaleConstraint_copy = true;
        [SerializeField] bool bCopier_scaleConstraint_onlyIfHasValidSources = true;

        //[SerializeField] bool bCopier_cloth_replaceOld = true;
        //[SerializeField] bool bCopier_cloth_createObjects = true;
        //[SerializeField] bool bCopier_cloth_copy = true;

        [SerializeField] CopierTabs.Tab _copier_selectedTab = CopierTabs.Tab.Common;

        bool _copierCheckedArmatureScales = false;
        bool _copierShowArmatureScaleWarning = false;

        //Ignore Array
        [SerializeField] bool _copierIgnoreArray_expand = false;
        [SerializeField] SerializedProperty _serializedIgnoreArrayProp;
        [SerializeField] Transform[] _copierIgnoreArray = new Transform[0];
        [SerializeField] bool bCopier_ignoreArray_includeChildren = false;
        [SerializeField] Vector2 _copierIgnoreArrayScroll = Vector2.zero;

#endregion

#region Thumbnails

        [SerializeField] public bool bThumbnails_use_camera_overlay = false;
        [SerializeField] public bool bThumbnails_use_camera_background = false;
        [SerializeField] bool shouldHideOtherAvatars = true;
        [SerializeField] bool lockSelectedCameraToSceneView = false;

        GameObject _cameraOverlay = null;
        GameObject _cameraBackground = null;
        RawImage _cameraOverlayImage = null;
        RawImage _cameraBackgroundImage = null;
        [SerializeField] bool _centerCameraOffsets_expand = false;
        [SerializeField] int _presetToolbarSelectedIndex = 0;
        [SerializeField] CameraClearFlags _thumbsCameraBgClearFlagsOld = CameraClearFlags.Skybox;

        public enum PresetToolbarOptions { Camera, Pose, Blendshape }

        [SerializeField] public int _selectedCameraPresetIndex = 0;
        [SerializeField] public string _selectedCameraPresetString = "";

        [SerializeField] public string _selectedPosePresetString = "";
        [SerializeField] public int _selectedPosePresetIndex = 0;

        [SerializeField] public string _selectedBlendshapePresetString = "";
        [SerializeField] public int _selectedBlendshapePresetIndex = 0;

        [SerializeField] public bool posePresetTryFixSinking = true;
        [SerializeField] public bool posePresetApplyBodyPosition = true;
        [SerializeField] public bool posePresetApplyBodyRotation = true;

        [SerializeField] bool centerCameraFixClippingPlanes = true;
#if VRC_SDK_VRCSDK2 || (VRC_SDK_VRCSDK3 && !UDON)
        [SerializeField] PumkinsCameraPreset.CameraOffsetMode centerCameraMode = PumkinsCameraPreset.CameraOffsetMode.Viewpoint;
#else
        [SerializeField] PumkinsCameraPreset.CameraOffsetMode centerCameraMode = PumkinsCameraPreset.CameraOffsetMode.Transform;
#endif
        [SerializeField] string centerCameraTransformPath = "Armature/Hips/Spine/Chest/Neck/Head";
        Transform centerCameraTransform = null;

        [SerializeField] Vector3 centerCameraPositionOffsetViewpoint = new Vector3(-0.096f, 0.025f, 0.269f);
        [SerializeField] Vector3 centerCameraRotationOffsetViewpoint = new Vector3(4.193f, 164.274f, 0);

        [SerializeField] Vector3 centerCameraPositionOffsetTransform = new Vector3(-0.096f, 0.025f, 0.269f);
        [SerializeField] Vector3 centerCameraRotationOffsetTransform = new Vector3(4.193f, 164.274f, 0);

        [SerializeField] Vector3 centerCameraPositionOffsetAvatar = new Vector3(-0.096f, 0.025f, 0.269f);
        [SerializeField] Vector3 centerCameraRotationOffsetAvatar = new Vector3(4.193f, 164.274f, 0);

        readonly Vector3 DEFAULT_CAMERA_POSITION_OFFSET_VIEWPOINT = new Vector3(0, 0, 0.28f);
        readonly Vector3 DEFAULT_CAMERA_ROTATION_OFFSET_VIEWPOINT = new Vector3(0, 180f, 0);

        readonly Vector3 DEFAULT_CAMERA_POSITION_OFFSET_TRANSFORM = new Vector3(0, 0.083f, 0.388f);
        readonly Vector3 DEFAULT_CAMERA_ROTATION_OFFSET_TRANSFORM = new Vector3(0, 180f, 0);

        readonly Vector3 DEFAULT_CAMERA_POSITION_OFFSET_AVATAR = new Vector3(0, 0.025f, 0.269f);
        readonly Vector3 DEFAULT_CAMERA_ROTATION_OFFSET_AVATAR = new Vector3(0, 180f, 0);

        static Camera _selectedCamera;

        static Material _rtMat;
        static RenderTexture _defaultRT;
        static RenderTexture _vrcRT;
        static GameObject _scaleRulerPrefab;
        static GameObject _scaleRuler;

        static RenderTexture oldCamRt;

        [SerializeField] public Color cameraBackgroundImageTint = Color.white;
        [SerializeField] public Color cameraOverlayImageTint = Color.white;

        Texture2D _emptyTexture;
        Texture2D EmptyTexture
        {
            get
            {
                if(_emptyTexture == null)
                    _emptyTexture = new Texture2D(1, 1);
                return _emptyTexture;
            }
        }

        public Texture2D cameraBackgroundTexture;
        public Texture2D cameraOverlayTexture;
        [SerializeField] string _lastOpenFilePath = default(string);
        [SerializeField] public string _backgroundPath = "";
        [SerializeField] public string _overlayPath = "";

        public PumkinsCameraPreset.CameraBackgroundOverrideType cameraBackgroundType = PumkinsCameraPreset.CameraBackgroundOverrideType.Color;

        [SerializeField] public Color _thumbsCamBgColor = Colors.DarkCameraBackground;

        static readonly string CAMERA_OVERLAY_NAME = "_PumkinsCameraOverlay";
        static readonly string CAMERA_BACKGROUND_NAME = "_PumkinsCameraBackground";

        [SerializeField] HumanPose _tempHumanPose = new HumanPose();
        [SerializeField] float[] _tempHumanPoseMuscles;
        [SerializeField] SerializedProperty _serializedTempHumanPoseMuscles;

        static List<PumkinsRendererBlendshapesHolder> _selectedAvatarRendererHolders;

#endregion

#region Avatar Info

        static PumkinsAvatarInfo avatarInfo = new PumkinsAvatarInfo();
        static string _avatarInfoSpace = "\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n";
        static string _avatarInfoString = Strings.AvatarInfo.selectAvatarFirst + _avatarInfoSpace; //Please don't hurt me for this

#endregion

#region Misc

        //UI
        [SerializeField] public bool _tools_expand = true;
        [SerializeField] bool _tools_avatar_expand = true;
        [SerializeField] bool _tools_dynamicBones_expand = true;
        [SerializeField] bool _tools_removeAll_expand = false;

        [SerializeField] public bool _avatarInfo_expand = false;
        [SerializeField] public bool _thumbnails_expand = false;

        [SerializeField] public bool _info_expand = true;
        [SerializeField] bool _thumbnails_useCameraOverlay_expand = true;
        [SerializeField] bool _thumbnails_useCameraBackground_expand = true;

        [SerializeField] public bool _experimental_expand = false;

        [SerializeField] bool _copier_expand = false;
        [SerializeField] bool _copier_expand_transforms = false;
        [SerializeField] bool _copier_expand_dynamicBones = false;
        [SerializeField] bool _copier_expand_dynamicBoneColliders = false;
        [SerializeField] bool _copier_expand_avatarDescriptor = false;
        [SerializeField] bool _copier_expand_skinnedMeshRenderer = false;
        [SerializeField] bool _copier_expand_colliders = false;
        [SerializeField] bool _copier_expand_particleSystems = false;
        [SerializeField] bool _copier_expand_rigidBodies = false;
        [SerializeField] bool _copier_expand_trailRenderers = false;
        [SerializeField] bool _copier_expand_meshRenderers = false;
        [SerializeField] bool _copier_expand_lights = false;
        [SerializeField] bool _copier_expand_animators = false;
        [SerializeField] bool _copier_expand_audioSources = false;
        [SerializeField] bool _copier_expand_other = false;
        [SerializeField] bool _copier_expand_aimConstraints = false;
        [SerializeField] bool _copier_expand_lookAtConstraints = false;
        [SerializeField] bool _copier_expand_parentConstraints = false;
        [SerializeField] bool _copier_expand_positionConstraints = false;
        [SerializeField] bool _copier_expand_rotationConstraints = false;
        [SerializeField] bool _copier_expand_scaleConstraints = false;
        [SerializeField] bool _copier_expand_joints = false;

        //[SerializeField] bool showExperimentalMenu = false;

        //Languages
        [SerializeField] public string _selectedLanguageString = "English - Default";
        [SerializeField] int _selectedLanguageIndex = 0;

        //Misc
        readonly float COPIER_SETTINGS_INDENT_SIZE = 38f;

        SerializedObject _serializedScript;
        [SerializeField] bool _openedSettings = false;
        [SerializeField] Vector2 _mainScroll = Vector2.zero;
        [SerializeField] bool verboseLoggingEnabled = false;
        GameObject oldSelectedAvatar = null;

        [SerializeField] bool handlesUiWindowPositionAtBottom = false;

        static string _mainScriptPath = null;
        static string _mainFolderPath = null;
        static string _resourceFolderPath = null;
        static string _mainFolderPathLocal = null;
        static string _mainScriptPathLocal = null;
        static string _resourceFolderPathLocal = null;

        static bool _eventsAdded = false;
        static bool _loadedPrefs = false;

#endregion

#endregion

#region Properties

        public static PumkinsAvatarTools Instance
        {
            get
            {
                return _PumkinsAvatarToolsWindow.ToolsWindow;
            }
        }

        public static GameObject SelectedAvatar
        {
            get
            {
                return _selectedAvatar;
            }
            set
            {
                if(value != _selectedAvatar)
                {
                    _selectedAvatar = value;
                    OnAvatarSelectionChanged(_selectedAvatar);
                }
            }
        }

        public static bool SelectedAvatarIsHuman
        {
            get
            {
                if(!_selectedAvatar)
                    return false;
                Animator anim = _selectedAvatar.GetComponent<Animator>();
                if(!anim || !anim.isHuman)
                    return false;

                return true;
            }
        }

        public static GameObject CopierSelectedFrom
        {
            get
            {
                return _copierSelectedFrom;
            }

            private set
            {
                _copierSelectedFrom = value;
            }
        }

        public bool CopierHasSelections
        {
            get
            {
                if(_copier_selectedTab < CopierTabs.Tab.All)
                {
                    bool[] commonToggles =
                    {
                        bCopier_descriptor_copy,
                        bCopier_skinMeshRender_copy,
                        bCopier_dynamicBones_copy,
                        bCopier_dynamicBones_copyColliders,
                        bCopier_meshRenderers_copy,
                        bCopier_particleSystems_copy,
                        bCopier_trailRenderers_copy,
                        bCopier_audioSources_copy,
                        bCopier_lights_copy,
                    };
                    if(commonToggles.Any(b => b))
                        return true;
                }
                else
                {
                    bool[] allToggles =
                    {
                        bCopier_transforms_copy,
                        bCopier_animators_copy,
                        bCopier_colliders_copy,
                        bCopier_joints_copy,
                        bCopier_descriptor_copy,
                        bCopier_meshRenderers_copy,
                        bCopier_particleSystems_copy,
                        bCopier_rigidBodies_copy,
                        bCopier_trailRenderers_copy,
                        bCopier_lights_copy,
                        bCopier_skinMeshRender_copy,
                        bCopier_dynamicBones_copy,
                        bCopier_dynamicBones_copyColliders,
                        bCopier_audioSources_copy,
                        bCopier_aimConstraint_copy,
                        bCopier_lookAtConstraint_copy,
                        bCopier_parentConstraint_copy,
                        bCopier_positionConstraint_copy,
                        bCopier_rotationConstraint_copy,
                        bCopier_scaleConstraint_copy,

                        (bCopier_other_copy && (bCopier_other_copyIKFollowers || bCopier_other_copyVRMSpringBones))
                    };
                    if(allToggles.Any(b => b))
                        return true;
                }
                return false;
            }
        }

        public static string MainScriptPath
        {
            get
            {
                if(_mainScriptPath == null)
                {
                    var toolScriptPath = Directory.GetFiles(Application.dataPath, "PumkinsAvatarTools.cs", SearchOption.AllDirectories)[0];
                    string s = Helpers.AbsolutePathToLocalAssetsPath(toolScriptPath.Substring(0, toolScriptPath.LastIndexOf('\\')));
                    _mainScriptPath = s;
                }
                return _mainScriptPath;
            }

            private set
            {
                _mainScriptPath = value;
            }
        }

        public static string MainScriptPathLocal
        {
            get
            {
                if(_mainScriptPathLocal == null)
                    _mainScriptPathLocal = Helpers.AbsolutePathToLocalAssetsPath(MainScriptPath);
                return _mainScriptPathLocal;
            }
        }

        public static string MainFolderPath
        {
            get
            {
                if(_mainFolderPath == null)
                {
                    string[] folder = Directory.GetDirectories(Application.dataPath, "PumkinsAvatarTools*", SearchOption.AllDirectories);
                    if(folder.Length > 0)
                        _mainFolderPath = folder[0];
                    else
                        _mainFolderPath = Directory.GetParent(MainScriptPath).Parent.FullName;
                }
                return _mainFolderPath;
            }

            private set
            {
                _mainFolderPath = value;
            }
        }

        public static string MainFolderPathLocal
        {
            get
            {
                if(_mainFolderPathLocal == null)
                    _mainFolderPathLocal = Helpers.AbsolutePathToLocalAssetsPath(MainFolderPath);
                return _mainFolderPathLocal;
            }
        }

        public static string ResourceFolderPathLocal
        {
            get
            {
                if(_resourceFolderPathLocal == null)
                    _resourceFolderPathLocal = Helpers.AbsolutePathToLocalAssetsPath(ResourceFolderPath);
                return _resourceFolderPathLocal;
            }
        }

        public static string ResourceFolderPath
        {
            get
            {
                if(_resourceFolderPath == null)
                    _resourceFolderPath = MainFolderPath + "/Resources";
                return _resourceFolderPath;
            }
        }

        public static RenderTexture DefaultRT
        {
            get
            {
                return _defaultRT = Resources.Load<RenderTexture>("CameraRT/PumkinsThumbnailCamRT");
            }
        }

        public static GameObject ScaleRulerPrefab
        {
            get
            {
                if(!_scaleRulerPrefab)
                {
                    _scaleRulerPrefab = Resources.Load<GameObject>("ScaleRuler/lineup_prefab");
                    var smr = _scaleRulerPrefab.GetComponent<SkinnedMeshRenderer>();
                    if(smr && !smr.sharedMaterials[0])
                        smr.sharedMaterials[0] = Resources.Load<Material>("ScaleRuler/lineup_wall_white");
                    if(smr && !smr.sharedMaterials[1])
                        smr.sharedMaterials[1] = Resources.Load<Material>("ScaleRuler/lineup_wall_black");
                }
				return _scaleRulerPrefab;
            }
        }

        public static RenderTexture VRCCamRT
        {
            get
            {
                if(!_vrcRT)
                {
                    var camObj = Resources.Load<GameObject>("VRCCam");
                    if(camObj)
                    {
                        var cam = camObj.GetComponent<Camera>();
                        if(cam)
                            _vrcRT = cam.targetTexture;
                    }
                }
                return _vrcRT;
            }
        }

        public static RenderTexture SelectedCamRT
        {
            get
            {
                if(SelectedCamera)
                    return SelectedCamera.targetTexture;
                return null;
            }
            set
            {
                if(SelectedCamera)
                {
                    SelectedCamera.targetTexture = value;
                }
            }
        }
        public static Material RTMaterial
        {
            get
            {
                if(!_rtMat)
                {
                    _rtMat = Resources.Load("CameraRT/PumkinsThumbnailCamUnlit", typeof(Material)) as Material;
                }
                return _rtMat;
            }
            private set
            {
                _rtMat = value;
            }
        }

        public static Camera SelectedCamera
        {
            get
            {
                return _selectedCamera;
            }
            set
            {
                if(_selectedCamera != value)
                {
                    RestoreCameraRT(_selectedCamera);
                    OnCameraSelectionChanged(value);
                }
                _selectedCamera = value;
            }
        }

        public static void RestoreCameraRT(Camera camera)
        {
            if(!camera)
                return;

            //Restore BlueprintCam render texture if it's VRCCam we're changing from
            if(camera.name == "VRCCam")
                camera.targetTexture = VRCCamRT;
            else
                camera.targetTexture = null;
        }

        public GameObject GetCameraOverlay(bool createIfMissing = false)
        {
            if(!_cameraOverlay)
            {
                _cameraOverlay = Helpers.FindGameObjectEvenIfDisabled(CAMERA_OVERLAY_NAME);
                if(!_cameraOverlay && createIfMissing)
                {
                    _cameraOverlay = new GameObject(CAMERA_OVERLAY_NAME)
                    {
                        hideFlags = HideFlags.HideAndDontSave,
                        tag = "EditorOnly"
                    };
                }
            }
            return _cameraOverlay;
        }
        public GameObject GetCameraBackground(bool createIfMissing = false)
        {
            if(!_cameraBackground)
            {
                _cameraBackground = Helpers.FindGameObjectEvenIfDisabled(CAMERA_BACKGROUND_NAME);
                if(!_cameraBackground && createIfMissing)
                {
                    _cameraBackground = new GameObject(CAMERA_BACKGROUND_NAME)
                    {
                        hideFlags = HideFlags.HideAndDontSave,
                        tag = "EditorOnly"
                    };
                }
            }
            return _cameraBackground;
        }

        public RawImage GetCameraOverlayRawImage(bool createIfMissing = false)
        {
            GameObject overlay = GetCameraOverlay(createIfMissing);
            if(overlay && !_cameraOverlayImage)
                _cameraOverlayImage = overlay.GetComponent<RawImage>();

            if(!_cameraOverlayImage && createIfMissing)
                SetupCameraRawImageAndCanvas(_cameraOverlay, ref _cameraOverlayImage, true);

            return _cameraOverlayImage;
        }

        public RawImage GetCameraBackgroundRawImage(bool createIfMissing = false)
        {
            GameObject background = GetCameraBackground(createIfMissing);
            if(background && !_cameraBackgroundImage)
                _cameraBackgroundImage = background.GetComponent<RawImage>();

            if(!_cameraBackgroundImage && createIfMissing)
            {
                SetupCameraRawImageAndCanvas(_cameraBackground, ref _cameraBackgroundImage, false);

                if(!string.IsNullOrEmpty(_backgroundPath))
                    SetBackgroundToImageFromPath(_backgroundPath);
            }
            return _cameraBackgroundImage;
        }

        public SerializedObject SerializedScript
        {
            get
            {
                if(_serializedScript == null)
                {
                    _serializedScript = new SerializedObject(this);
                }
                return _serializedScript;
            }

            private set
            {
                _serializedScript = value;
            }
        }

        public SerializedProperty SerializedIgnoreArray
        {
            get
            {
                if(SerializedScript != null && _serializedIgnoreArrayProp == null)
                    _serializedIgnoreArrayProp = SerializedScript.FindProperty("_copierIgnoreArray");
                return _serializedIgnoreArrayProp;
            }

            private set
            {
                _serializedIgnoreArrayProp = value;
            }
        }

        public SerializedProperty SerializedScaleTemp
        {
            get
            {
                if(SerializedScript != null && _serializedAvatarScaleTempProp == null)
                    _serializedAvatarScaleTempProp = SerializedScript.FindProperty("_avatarScaleTemp");
                return _serializedAvatarScaleTempProp;
            }
            private set
            {
                _serializedAvatarScaleTempProp = value;
            }
        }

        public SerializedProperty SerializedHumanPoseMuscles
        {
            get
            {
                if(_tempHumanPoseMuscles == null)
                    _tempHumanPoseMuscles = _tempHumanPose.muscles;

                if(SerializedScript != null && _serializedTempHumanPoseMuscles == null)
                {
                    _serializedTempHumanPoseMuscles = SerializedScript.FindProperty("_tempHumanPoseMuscles");
                }
                return _serializedTempHumanPoseMuscles;
            }
            private set { _serializedTempHumanPoseMuscles = value; }
        }

        public bool DrawingHandlesGUI
        {
            get
            {
                if(_editingView || _editingScale)
                    return true;
                return false;
            }
        }

        public bool DynamicBonesExist
        {
            get
            {
#if PUMKIN_DBONES || PUMKIN_OLD_DBONES
                return true;
#else
                return false;
#endif
            }
        }

#endregion

#region Events and Delegates

        public delegate void AvatarChangedHandler(GameObject selection);
        public delegate void CameraChangeHandler(Camera camera);

        public static event AvatarChangedHandler AvatarSelectionChanged;
        public static event CameraChangeHandler CameraSelectionChanged;

        EditorApplication.CallbackFunction updateCallback;

#endregion

#region Event Definitions

        public static void OnCameraSelectionChanged(Camera camera)
        {
            if(CameraSelectionChanged != null)
                CameraSelectionChanged.Invoke(camera);
            string name = "none";
            if(camera && camera.gameObject)
                name = camera.gameObject.name;
            LogVerbose("Camera selection changed to " + name);

            //Handle overlay and background raw images camera references, setup canvas on foreground and background and camera clipping planes
            RawImage bg = Instance.GetCameraBackgroundRawImage(false);
            RawImage fg = Instance.GetCameraOverlayRawImage(false);
            if(bg)
            {
                Canvas bgc = bg.GetComponent<Canvas>();
                if(!bgc)
                    bgc = bg.gameObject.AddComponent<Canvas>();
                bgc.worldCamera = camera;
                if(camera)
                    bgc.planeDistance = camera.farClipPlane - 2;
            }
            if(fg)
            {
                Canvas fgc = fg.GetComponent<Canvas>();
                if(!fgc)
                    fgc = fg.gameObject.AddComponent<Canvas>();
                fgc.worldCamera = camera;
                if(camera)
                    fgc.planeDistance = camera.nearClipPlane + 0.01f;
            }
        }

        public static void OnAvatarSelectionChanged(GameObject selection)
        {
            if(AvatarSelectionChanged != null)
                AvatarSelectionChanged.Invoke(selection);
            LogVerbose("Avatar selection changed to " + (selection != null ? selection.name : "empty"));

            Transform armature = Helpers.GetAvatarArmature(selection);
            if(armature && (armature.localScale.x != 1 || armature.localScale.y != 1 || armature.localScale.z != 1))
                Log(Strings.Warning.armatureScaleNotOne, LogType.Warning); //Issue armature scale warning because this trips me up too

            //Handle skinned mesh renderer container for blendshape preset gui
            SetupBlendeshapeRendererHolders(selection);

            //Cancel editing viewpoint and scaling
            if(Instance._editingScale)
                Instance.EndScalingAvatar(null, true);
#if VRC_SDK_VRCSDK2 || (VRC_SDK_VRCSDK3 && !UDON)
            if(Instance._editingView)
                Instance.EndEditingViewpoint(null, true);
#endif

            Instance.centerCameraTransform = null;

            Instance._copierCheckedArmatureScales = false;

            Instance._nextToggleDBoneState = false;
#if PUMKIN_DBONES || PUMKIN_OLD_DBONES
            Instance._dBonesThatWereAlreadyDisabled = new List<DynamicBone>();
#endif
        }

        private static void SetupBlendeshapeRendererHolders(GameObject selection)
        {
            //Save old expanded values to prevent calling this from collapsing all blendshape holders in menus
            Dictionary<string, bool> oldHolderExpandValues = new Dictionary<string, bool>();
            if(_selectedAvatarRendererHolders == null)
            {
                _selectedAvatarRendererHolders = new List<PumkinsRendererBlendshapesHolder>();
            }
            else
            {
                foreach(var h in _selectedAvatarRendererHolders)
                    oldHolderExpandValues.Add(h.rendererPath, h.expandedInUI);
            }

            _selectedAvatarRendererHolders.Clear();

            if(selection)
            {
                SkinnedMeshRenderer[] smRenderers = selection.GetComponentsInChildren<SkinnedMeshRenderer>();
                foreach(var smRender in smRenderers)
                {
                    if(smRender)
                    {
                        var newHolder = (PumkinsRendererBlendshapesHolder)smRender;
                        if(newHolder == null)
                            continue;

                        if(oldHolderExpandValues.ContainsKey(newHolder.rendererPath))
                            newHolder.expandedInUI = oldHolderExpandValues[newHolder.rendererPath];

                        _selectedAvatarRendererHolders.Add(newHolder);
                    }
                }
            }
        }

#endregion

#region Callback Handlers

        public void HandleOnEnable()
        {
            LogVerbose("Tools window: OnEnable()");

            if(updateCallback == null)
                updateCallback = new EditorApplication.CallbackFunction(OnUpdate);

            SceneView.onSceneGUIDelegate += OnSceneGUI;
            EditorApplication.update += updateCallback;
            if(!_eventsAdded) //Not sure if this is necessary anymore
            {
                EditorApplication.playModeStateChanged += HandlePlayModeStateChange;
                Selection.selectionChanged += HandleSelectionChanged;
                EditorSceneManager.sceneOpened += HandleSceneChange;
#if UNITY_2018
                PrefabStage.prefabStageOpened += HandlePrefabStageOpened;
                PrefabStage.prefabStageClosing += HandlePrefabStageClosed;
#endif
                _eventsAdded = true;
            }

            SerializedScript = new SerializedObject(this);
            SerializedIgnoreArray = SerializedScript.FindProperty("_copierIgnoreArray");
            SerializedScaleTemp = SerializedScript.FindProperty("_avatarScaleTemp");
            SerializedHumanPoseMuscles = SerializedScript.FindProperty("_tempHumanPoseMuscles");

            _emptyTexture = new Texture2D(2, 2);
            cameraOverlayTexture = new Texture2D(2, 2);
            cameraBackgroundTexture = new Texture2D(2, 2);

            LoadPrefs();

            RestoreTexturesFromPaths();
            RefreshBackgroundOverrideType();

            DestroyDummies();

            SelectedCamera = GetVRCCamOrMainCam();

            if(_lastOpenFilePath == default(string))
                _lastOpenFilePath = MainFolderPath + PumkinsPresetManager.resourceCamerasPath + "/Example Images";

            lockSelectedCameraToSceneView = false;
        }

        public void HandleSceneChange(Scene scene, OpenSceneMode mode)
        {
            if(mode == OpenSceneMode.Single)
                RefreshLanguage();
        }

        public void HandleOnDisable()
        {
            LogVerbose("Tools window: OnDisable()");
            SceneView.onSceneGUIDelegate -= OnSceneGUI;
            EditorApplication.update -= updateCallback;
            Selection.selectionChanged -= HandleSelectionChanged;
            EditorApplication.playModeStateChanged -= HandlePlayModeStateChange;
            EditorSceneManager.sceneOpened -= HandleSceneChange;
#if UNITY_2018
            PrefabStage.prefabStageOpened -= HandlePrefabStageOpened;
            PrefabStage.prefabStageClosing -= HandlePrefabStageClosed;
#endif
            _eventsAdded = false;

            if(SerializedScript != null)
                SerializedScript.ApplyModifiedPropertiesWithoutUndo();
#if VRC_SDK_VRCSDK2 || (VRC_SDK_VRCSDK3 && !UDON)
            EndEditingViewpoint(null, true);
#endif
            EndScalingAvatar(null, true);

            SavePrefs();
        }

        public static void DestroyDummies()
        {
            Helpers.DestroyAppropriate(GameObject.Find(CAMERA_BACKGROUND_NAME));
            Helpers.DestroyAppropriate(GameObject.Find(CAMERA_OVERLAY_NAME));
            Helpers.DestroyAppropriate(GameObject.Find(SCALE_RULER_DUMMY_NAME));
        }

        void HandlePlayModeStateChange(PlayModeStateChange mode)
        {
            if(mode == PlayModeStateChange.ExitingEditMode || mode == PlayModeStateChange.ExitingPlayMode)
            {
#if VRC_SDK_VRCSDK2 || (VRC_SDK_VRCSDK3 && !UDON)
                if(_editingView)
                    EndEditingViewpoint(SelectedAvatar, true);
#endif
                if(_editingScale)
                    EndScalingAvatar(SelectedAvatar, true);

                SavePrefs();
            }
            else if(mode == PlayModeStateChange.EnteredEditMode)
            {
                LoadPrefs();

                SelectedCamera = GetVRCCamOrMainCam();
            }
            else if(mode == PlayModeStateChange.EnteredPlayMode)
            {
                _editingView = false;

                LoadPrefs();
                _emptyTexture = new Texture2D(2, 2);
                cameraOverlayTexture = new Texture2D(2, 2);

#if VRC_SDK_VRCSDK2 || (VRC_SDK_VRCSDK3 && !UDON)
                var ps = FindObjectOfType<PipelineSaver>();
                if(ps && ps.contentType == PipelineManager.ContentType.avatar)
                    SelectedAvatar = ps.gameObject;

                HideAllOtherAvatars(shouldHideOtherAvatars, SelectedAvatar);

                //Find the vrc ui camera and set it's depth higher than defaul to make sure it's the one that's rendering
                var camObj = GameObject.Find("VRCUICamera");
                if(camObj)
                {
                    var uiCam = camObj.GetComponent<Camera>();
                    if(uiCam)
                        uiCam.depth = 1f;
                }
#endif
            }
            SelectedCamera = GetVRCCamOrMainCam();
        }

        void HandleSelectionChanged()
        {
            if(_useSceneSelectionAvatar)
                SelectAvatarFromScene();
            _PumkinsAvatarToolsWindow.RequestRepaint(this);
        }

#if UNITY_2018
        private void HandlePrefabStageOpened(PrefabStage stage)
        {
            if(SelectedAvatar)
                oldSelectedAvatar = SelectedAvatar;

            SelectedAvatar = stage.prefabContentsRoot;
        }

        void HandlePrefabStageClosed(PrefabStage stage)
        {
            if(oldSelectedAvatar)
            {
                SelectedAvatar = oldSelectedAvatar;
                oldSelectedAvatar = null;
            }
        }
#endif

#endregion

#region Debug
#if PUMKIN_DEV
        string testPath = "";
#endif
#endregion

#region Unity GUI

        public static void ShowWindow()
        {
            //Show existing window instance. If one doesn't exist, make one.
            EditorWindow editorWindow = EditorWindow.GetWindow(typeof(PumkinsAvatarTools));
            editorWindow.autoRepaintOnSceneChange = true;

            editorWindow.Show();
            editorWindow.titleContent = new GUIContent("Pumkin Tools");

            _DependencyChecker.CheckForDependencies();
        }

        public void OnGUI()
        {
            SerializedScript.Update();

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField(Strings.Main.title, Styles.Label_mainTitle);

                if(GUILayout.Button(Icons.Settings, Styles.IconButton))
                    _openedSettings = !_openedSettings;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField(Strings.Credits.version);

            if(_openedSettings)
            {
                DrawSettingsGUI();
            }
            else
            {
                DrawMainGUI();
            }
        }

        void OnUpdate()
        {
            //Sync selected camera to scene camera
            if(lockSelectedCameraToSceneView && _selectedCamera)
                SelectedCamera.transform.SetPositionAndRotation(SceneView.lastActiveSceneView.camera.transform.position, SceneView.lastActiveSceneView.camera.transform.rotation);

            //Draw camera overlay
            GameObject overlay = GetCameraOverlay(true);
            if(bThumbnails_use_camera_overlay)
            {
                var raw = GetCameraOverlayRawImage(true);
                if(!raw.texture && !string.IsNullOrEmpty(_overlayPath))
                    SetOverlayToImageFromPath(_overlayPath);

                if(_selectedCamera && raw.texture)
                {
                    if(!overlay.activeInHierarchy)
                        overlay.SetActive(true);
                }
                else
                {
                    overlay.SetActive(false);
                }
            }
            else
            {
                if(overlay)
                    overlay.SetActive(false);
            }

            //Draw camera background
            GameObject background = GetCameraBackground(true);
            if(bThumbnails_use_camera_background)
            {
                var raw = GetCameraBackgroundRawImage(true);
                if(cameraBackgroundType == PumkinsCameraPreset.CameraBackgroundOverrideType.Image)
                {
                    if(!raw.texture && !string.IsNullOrEmpty(_backgroundPath))
                        SetBackgroundToImageFromPath(_backgroundPath);
                    if(_selectedCamera && raw.texture && !background.activeInHierarchy)
                        background.SetActive(true);
                    else if(!raw.texture && background.activeInHierarchy)
                        background.SetActive(false);
                }
                else
                {
                    if(background.activeInHierarchy)
                        background.SetActive(false);
                }
            }
            else
            {
                if(background)
                    background.SetActive(false);
            }
        }

        void DrawSettingsGUI()
        {
            EditorGUILayout.Space();
            GUILayout.BeginVertical();

            GUILayout.Label(Strings.Credits.redundantStrings);

            EditorGUILayout.Space();

            GUILayout.Label(Strings.Credits.addMoreStuff);

            GUILayout.BeginHorizontal();

            GUILayout.Label(Strings.Credits.pokeOnDiscord);

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            if(PumkinsLanguageManager.Languages.Count == 0)
                PumkinsLanguageManager.LoadTranslations();

            EditorGUILayout.Space();
            Helpers.DrawGUILine();

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUI.BeginChangeCheck();
                {
                    if(_selectedLanguageIndex >= PumkinsLanguageManager.Languages.Count)
                        _selectedLanguageIndex = PumkinsLanguageManager.GetIndexOfLanguage(_selectedLanguageString);

                    _selectedLanguageIndex = EditorGUILayout.Popup(Strings.Settings.language, _selectedLanguageIndex, PumkinsLanguageManager.Languages.Select(o => o.ToString()).ToArray(), Styles.Popup);
                }
                if(EditorGUI.EndChangeCheck() && PumkinsLanguageManager.Languages.Count > 1)
                {
                    PumkinsLanguageManager.SetLanguage(PumkinsLanguageManager.Languages[_selectedLanguageIndex]);
                    _selectedLanguageString = Strings.Translation.ToString();
                }

                if(GUILayout.Button(Icons.Refresh, Styles.IconButton))
                {
                    PumkinsLanguageManager.LoadTranslations();
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                if(GUILayout.Button(Strings.Buttons.openFolder))
                    Helpers.SelectAndPing(PumkinsLanguageManager.translationPathLocal);
                //if(GUILayout.Button(Strings.Settings.importLanguage))
                //    PumkinsLanguageManager.OpenFileImportLanguagePreset();
            }
            EditorGUILayout.EndHorizontal();

            if(!DynamicBonesExist)
            {
                Helpers.DrawGUILine();
                if(GUILayout.Button(Strings.Settings.searchForBones, Styles.BigButton))
                    _DependencyChecker.CheckForDependencies();
            }

            Helpers.DrawGUILine();
            GUILayout.Label(Strings.Settings.misc + ":");

            EditorGUILayout.Space();

            handlesUiWindowPositionAtBottom = GUILayout.Toggle(handlesUiWindowPositionAtBottom, Strings.Settings.sceneViewOverlayWindowsAtBottom);

            //EditorGUILayout.Space();
            //showExperimentalMenu = GUILayout.Toggle(showExperimentalMenu, Strings.Settings.showExperimentalMenu);
            verboseLoggingEnabled = GUILayout.Toggle(verboseLoggingEnabled, Strings.Settings.enableVerboseLogging);

            EditorGUILayout.Space();
#if PUMKIN_DEV
            if(GUILayout.Button("Generate Thry Manifest"))
            {
                ThryModuleManifest.Generate();
            }
#endif

            GUILayout.FlexibleSpace();

            if(GUILayout.Button(Strings.Settings.uwu, "IconButton", GUILayout.ExpandWidth(false)))
            {
                if(Strings.Settings.uwu == "uwu")
                    Strings.Settings.uwu = "OwO";
                else
                    Strings.Settings.uwu = "uwu";
            }
        }

        void DrawMainGUI()
        {
            EditorGUIUtility.SetIconSize(new Vector2(15, 15));

            EditorGUILayout.Space();

            DrawAvatarSelectionWithButtonGUI(true);

            Helpers.DrawGUILine();

            _mainScroll = EditorGUILayout.BeginScrollView(_mainScroll);
            {
                DrawToolsMenuGUI();

                EditorGUILayout.Space();

                DrawCopierMenuGUI();

                EditorGUILayout.Space();

                DrawAvatarInfoMenuGUI();

                EditorGUILayout.Space();

                DrawThumbnailsMenuGUI();

                EditorGUILayout.Space();

                DrawInfoMenuGUI();

                //EditorGUILayout.Space();

                //if(showExperimentalMenu)
                //    DrawExperimentalMenuGUI();

                Helpers.DrawGUILine();
            }
            EditorGUILayout.EndScrollView();

            if(GUI.changed)
            {
                SerializedScript.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(this);
            }
        }

        private void DrawExperimentalMenuGUI()
        {
            if(_experimental_expand = GUILayout.Toggle(_experimental_expand, Strings.Main.experimental, Styles.Foldout_title))
            {
                EditorGUILayout.Space();

                //EditorGUI.BeginDisabledGroup(!DynamicBonesExist || !SelectedAvatar);
                //{
                //    if(GUILayout.Button(Strings.Tools.fixDynamicBoneScripts, Styles.BigButton))
                //        DoAction(SelectedAvatar, ToolMenuActions.FixDynamicBoneScripts);
                //}
                EditorGUI.EndDisabledGroup();
            }
        }

        /// <summary>
        /// Draws the small windows inside the scene view when scaling the avatar or moving the viewpoint
        /// </summary>
        void OnSceneGUI(SceneView sceneView)
        {
            if(!DrawingHandlesGUI)
                return;

            HandleKeyboardInput();

            //var horOffset = sceneViewRect.width * 0.05f;
            //var vertOffset = sceneViewRect.height * 0.05f;

            if(_editingScale) //Scale editing
            {
                DrawEditingScaleGUI();
            }
#if VRC_SDK_VRCSDK2 || (VRC_SDK_VRCSDK3 && !UDON)
            else if(_editingView) //Viewpoint editing
            {
                DrawEditingViewpointGUI();
            }
#endif

            if(DrawingHandlesGUI)
                _PumkinsAvatarToolsWindow.RequestRepaint(this);
        }

#if VRC_SDK_VRCSDK2 || (VRC_SDK_VRCSDK3 && !UDON)
        private void DrawEditingViewpointGUI()
        {
            if(!SelectedAvatar)
            {
                EndEditingViewpoint(null, true);
                return;
            }

            Vector2 windowSize = new Vector2(200, 68);

            Rect rect = SceneView.currentDrawingSceneView.camera.pixelRect;
            if(handlesUiWindowPositionAtBottom)
                rect = new Rect(10, rect.height - 10 - windowSize.y, windowSize.x, windowSize.y);
            else
                rect = new Rect(new Vector2(10, 10), windowSize);

            Handles.BeginGUI();
            {
                GUILayout.BeginArea(rect, Styles.Box);
                {
                    GUILayout.Label(Strings.Tools.editViewpoint);
                    if(GUILayout.Button(Strings.Buttons.moveToEyes, GUILayout.MinWidth(80)))
                    {
                        _viewPosTemp = Helpers.GetViewpointAtEyeLevel(SelectedAvatar.GetComponent<Animator>());
                    }
                    GUILayout.BeginHorizontal();
                    {
                        if(GUILayout.Button(Strings.Buttons.cancel, GUILayout.MinWidth(80)))
                        {
                            EndEditingViewpoint(SelectedAvatar, true);
                        }

                        if(GUILayout.Button(Strings.Buttons.apply, GUILayout.MinWidth(80)))
                        {
                            EndEditingViewpoint(SelectedAvatar, false);
                        }
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndArea();
            }
            Handles.EndGUI();

            if(_tempAvatarDescriptor)
            {
                _viewPosTemp = Handles.PositionHandle(_viewPosTemp, Quaternion.identity);
                Handles.color = Colors.BallHandle;
                Handles.SphereHandleCap(0, _viewPosTemp, Quaternion.identity, 0.02f, EventType.Repaint);
            }
        }
#endif

        private void DrawEditingScaleGUI()
        {
            if(!SelectedAvatar)
            {
                EndScalingAvatar(null, true);
                return;
            }

            bool propertyChanged = false;
#if VRC_SDK_VRCSDK2 || (VRC_SDK_VRCSDK3 && !UDON)
            Vector2 windowSize = new Vector2(200, 85);
#else
            Vector2 windowSize = new Vector2(200, 70);
#endif

            Rect rect = SceneView.currentDrawingSceneView.camera.pixelRect;
            if(handlesUiWindowPositionAtBottom)
                rect = new Rect(10, rect.height - 10 - windowSize.y, windowSize.x, windowSize.y);
            else
                rect = new Rect(new Vector2(10, 10), windowSize);

            Handles.BeginGUI();
            {
                //GUILayout.BeginArea(new Rect(10, rect.height - 10 - windowSize.y, windowSize.x, windowSize.y), Styles.Box);
                GUILayout.BeginArea(rect, Styles.Box);
                {
                    GUILayout.Label(Strings.Tools.editScale);
                    if(SerializedScaleTemp != null)
                    {
                        EditorGUILayout.PropertyField(SerializedScaleTemp, GUIContent.none);
                        if(SerializedScript.ApplyModifiedPropertiesWithoutUndo())
                            propertyChanged = true;
                    }
                    else
                    {
                        EditorGUILayout.LabelField(_avatarScaleTemp.ToString());
                    }
#if VRC_SDK_VRCSDK2 || (VRC_SDK_VRCSDK3 && !UDON)
                    editingScaleMovesViewpoint = GUILayout.Toggle(editingScaleMovesViewpoint, Strings.Tools.editScaleMoveViewpoint);
#else
                    EditorGUILayout.Space();
#endif
                    GUILayout.BeginHorizontal();
                    {
                        if(GUILayout.Button(Strings.Buttons.cancel, GUILayout.MinWidth(80)))
                            EndScalingAvatar(SelectedAvatar, true);

                        if(GUILayout.Button(Strings.Buttons.apply, GUILayout.MinWidth(80)))
                            EndScalingAvatar(SelectedAvatar, false);
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndArea();
            }
            Handles.EndGUI();

#if VRC_SDK_VRCSDK2 || (VRC_SDK_VRCSDK3 && !UDON)
            if(_tempAvatarDescriptor)
            {
                EditorGUI.BeginChangeCheck();
                {
                    _avatarScaleTemp = Handles.ScaleSlider(_avatarScaleTemp, SelectedAvatar.transform.position, Vector3.up, Quaternion.identity, HandleUtility.GetHandleSize(SelectedAvatar.transform.position) * 2, 0.01f);
                }
                if(EditorGUI.EndChangeCheck() || propertyChanged)
                {
                    SetAvatarScaleAndMoveViewpoint(_tempAvatarDescriptor, _avatarScaleTemp);
                }

                if(editingScaleMovesViewpoint)
                {
                    Handles.color = Colors.BallHandle;
                    Handles.SphereHandleCap(0, _viewPosTemp, Quaternion.identity, 0.02f, EventType.Repaint);
                }
            }
            else
            {
                EndScalingAvatar(null, true);
            }
#else
            EditorGUI.BeginChangeCheck();
            {
                _avatarScaleTemp = Handles.ScaleSlider(_avatarScaleTemp, SelectedAvatar.transform.position, Vector3.up, Quaternion.identity, HandleUtility.GetHandleSize(SelectedAvatar.transform.position) * 2, 0.01f);
            }
            if(EditorGUI.EndChangeCheck() || propertyChanged)
            {
                SetAvatarScale(_avatarScaleTemp);
            }
#endif
        }

        private void HandleKeyboardInput()
        {
            Event current = Event.current;
            if(current.type != EventType.KeyDown)
                return;

            if(_editingScale)
            {
                if(current.keyCode == KeyCode.Return || current.keyCode == KeyCode.KeypadEnter)
                {
                    EndScalingAvatar(SelectedAvatar, false);
                    current.Use();
                }
                else if(current.keyCode == KeyCode.Escape)
                {
                    EndScalingAvatar(null, true);
                    current.Use();
                }
            }
#if VRC_SDK_VRCSDK2 || (VRC_SDK_VRCSDK3 && !UDON)
            else if(_editingView)
            {
                if(current.keyCode == KeyCode.Return || current.keyCode == KeyCode.KeypadEnter)
                {
                    EndEditingViewpoint(SelectedAvatar, false);
                    current.Use();
                }
                else if(current.keyCode == KeyCode.Escape)
                {
                    EndEditingViewpoint(null, true);
                    current.Use();
                }
            }
#endif
        }

        public static void DrawAvatarSelectionWithButtonGUI(bool showSelectFromSceneButton = true, bool showSceneSelectionCheckBox = true)
        {
            SelectedAvatar = (GameObject)EditorGUILayout.ObjectField(Strings.Main.avatar, SelectedAvatar, typeof(GameObject), true);

            if(_useSceneSelectionAvatar)
                if(Selection.activeObject != SelectedAvatar)
                    SelectAvatarFromScene();

            if(showSelectFromSceneButton)
                if(GUILayout.Button(Strings.Buttons.selectFromScene))
                    if(Selection.activeObject)
                        SelectAvatarFromScene();

            if(showSceneSelectionCheckBox)
                _useSceneSelectionAvatar = GUILayout.Toggle(_useSceneSelectionAvatar, Strings.Main.useSceneSelection);
        }

        private void DrawCopierMenuGUI()
        {
            if(_copier_expand = GUILayout.Toggle(_copier_expand, Strings.Main.copier, Styles.Foldout_title))
            {
                EditorGUILayout.Space();

                EditorGUI.BeginChangeCheck();
                {
                    CopierSelectedFrom = (GameObject)EditorGUILayout.ObjectField(Strings.Copier.copyFrom, CopierSelectedFrom, typeof(GameObject), true);

                    if(GUILayout.Button(Strings.Buttons.selectFromScene))
                        if(Selection.activeGameObject != null)
                            CopierSelectedFrom = Selection.activeGameObject.transform.root.gameObject;

                    if(_copierShowArmatureScaleWarning)
                        EditorGUILayout.LabelField(Strings.Warning.armatureScalesDontMatch, Styles.HelpBox_OneLine);
                }
                if(EditorGUI.EndChangeCheck())
                {
                    _copierCheckedArmatureScales = false;
                }

                if(!_copierCheckedArmatureScales)
                {
                    _copierCheckedArmatureScales = true;
                    Transform copyToArm = Helpers.GetAvatarArmature(CopierSelectedFrom);
                    Transform copyFromArm = Helpers.GetAvatarArmature(SelectedAvatar);

                    _copierShowArmatureScaleWarning = (copyToArm && copyFromArm) && (copyToArm.localScale != copyFromArm.localScale) ? true : false;
                    if(_copierShowArmatureScaleWarning)
                        Log(Strings.Warning.armatureScalesDontMatch, LogType.Warning);
                }

                EditorGUILayout.Space();

                EditorGUI.BeginDisabledGroup(CopierSelectedFrom == null || SelectedAvatar == null);
                {
                    Helpers.DrawGUILine(1, false);

                    var toolbarContent = new GUIContent[] { new GUIContent(Strings.Copier.showCommon), new GUIContent(Strings.Copier.showAll) };
                    _copier_selectedTab = (CopierTabs.Tab)GUILayout.Toolbar((int)_copier_selectedTab, toolbarContent);

                    Helpers.DrawGUILine(1, false);

                    if(CopierTabs.ComponentIsInSelectedTab("vrcavatardescriptor", _copier_selectedTab))
                    {
                        //AvatarDescriptor menu
                        Helpers.DrawDropdownWithToggle(ref _copier_expand_avatarDescriptor, ref bCopier_descriptor_copy, Strings.Copier.descriptor, Icons.Avatar);
                        if(_copier_expand_avatarDescriptor)
                        {
                            EditorGUI.BeginDisabledGroup(!bCopier_descriptor_copy);
                            EditorGUILayout.Space();

                            using(var cHorizontalScope = new GUILayout.HorizontalScope())
                            {
                                GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE); // horizontal indent size

                                using(var cVerticalScope = new GUILayout.VerticalScope())
                                {
#if VRC_SDK_VRCSDK2 || (VRC_SDK_VRCSDK3 && !UDON)
                                    bCopier_descriptor_copySettings = GUILayout.Toggle(bCopier_descriptor_copySettings, Strings.Copier.copySettings, Styles.CopierToggle);
                                    bCopier_descriptor_copyViewpoint = GUILayout.Toggle(bCopier_descriptor_copyViewpoint, Strings.Copier.descriptor_copyViewpoint, Styles.CopierToggle);
#endif
                                    bCopier_descriptor_copyAvatarScale = GUILayout.Toggle(bCopier_descriptor_copyAvatarScale, Strings.Copier.transforms_avatarScale, Styles.CopierToggle);

#if VRC_SDK_VRCSDK2 || (VRC_SDK_VRCSDK3 && !UDON)
                                    EditorGUILayout.Space();

                                    bCopier_descriptor_copyPlayableLayers = GUILayout.Toggle(bCopier_descriptor_copyPlayableLayers, Strings.Copier.descriptor_playableLayers, Styles.CopierToggle);
                                    bCopier_descriptor_copyEyeLookSettings = GUILayout.Toggle(bCopier_descriptor_copyEyeLookSettings, Strings.Copier.descriptor_eyeLookSettings, Styles.CopierToggle);
                                    bCopier_descriptor_copyExpressions = GUILayout.Toggle(bCopier_descriptor_copyExpressions, Strings.Copier.descriptor_expressions, Styles.CopierToggle);
#endif
                                    EditorGUILayout.Space();

#if VRC_SDK_VRCSDK2 || (VRC_SDK_VRCSDK3 && !UDON)
                                    bCopier_descriptor_copyPipelineId = GUILayout.Toggle(bCopier_descriptor_copyPipelineId, Strings.Copier.descriptor_pipelineId, Styles.CopierToggle);
#endif
                                }
                            }

                            EditorGUILayout.Space();
                            EditorGUI.EndDisabledGroup();
                        }
                        Helpers.DrawGUILine(1, false);
                    }

                    if(CopierTabs.ComponentIsInSelectedTab("dynamicbone", _copier_selectedTab))
                    {
                        //DynamicBones menu
                        if(!DynamicBonesExist)
                        {
                            EditorGUI.BeginDisabledGroup(true);
                            {
                                Helpers.DrawDropdownWithToggle(ref _copier_expand_dynamicBones, ref bCopier_dynamicBones_copy, Strings.Copier.dynamicBones + " (" + Strings.Warning.notFound + ")", Icons.BoneIcon);
                                bCopier_dynamicBones_copy = false;
                                _copier_expand_dynamicBones = false;
                            }
                            EditorGUI.EndDisabledGroup();
                        }
                        else
                        {
#if PUMKIN_OLD_DBONES
                        Helpers.DrawDropdownWithToggle(ref _copier_expand_dynamicBones, ref bCopier_dynamicBones_copy, Strings.Copier.dynamicBones + " (" + Strings.Warning.oldVersion + ")", Icons.BoneIcon);
#elif PUMKIN_DBONES
                        Helpers.DrawDropdownWithToggle(ref _copier_expand_dynamicBones, ref bCopier_dynamicBones_copy, Strings.Copier.dynamicBones, Icons.BoneIcon);
#endif
                        }

                        if(_copier_expand_dynamicBones)
                        {
                            EditorGUI.BeginDisabledGroup(!bCopier_dynamicBones_copy);
                            EditorGUILayout.Space();

                            using(var cHorizontalScope = new GUILayout.HorizontalScope())
                            {
                                GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE); // horizontal indent size

                                using(var cVerticalScope = new GUILayout.VerticalScope())
                                {
                                    bCopier_dynamicBones_copySettings = GUILayout.Toggle(bCopier_dynamicBones_copySettings, Strings.Copier.copySettings, Styles.CopierToggle);
                                    bCopier_dynamicBones_createMissing = GUILayout.Toggle(bCopier_dynamicBones_createMissing, Strings.Copier.dynamicBones_createMissing, Styles.CopierToggle);
                                    bCopier_dynamicBones_createObjects = GUILayout.Toggle(bCopier_dynamicBones_createObjects, Strings.Copier.copyGameObjects, Styles.CopierToggle);
                                    bCopier_dynamicBones_removeOldBones = GUILayout.Toggle(bCopier_dynamicBones_removeOldBones, Strings.Copier.dynamicBones_removeOldBones, Styles.CopierToggle);
                                }
                            }

                            EditorGUILayout.Space();
                            EditorGUI.EndDisabledGroup();
                        }

                        Helpers.DrawGUILine(1, false);
                    }

                    if(CopierTabs.ComponentIsInSelectedTab("dynamicbonecollider", _copier_selectedTab))
                    {
                        //Dynamic Bone Colliders menu
                        if(!DynamicBonesExist)
                        {
                            EditorGUI.BeginDisabledGroup(true);
                            {
                                Helpers.DrawDropdownWithToggle(ref _copier_expand_dynamicBoneColliders, ref bCopier_dynamicBones_copyColliders, Strings.Copier.dynamicBones_colliders + " (" + Strings.Warning.notFound + ")", Icons.BoneColliderIcon);
                                bCopier_dynamicBones_copyColliders = false;
                                _copier_expand_dynamicBoneColliders = false;
                            }
                            EditorGUI.EndDisabledGroup();
                        }
                        else
                        {
#if PUMKIN_OLD_DBONES
                        Helpers.DrawDropdownWithToggle(ref _copier_expand_dynamicBoneColliders, ref bCopier_dynamicBones_copyColliders, Strings.Copier.dynamicBones_colliders + " (" + Strings.Warning.oldVersion + ")", Icons.BoneColliderIcon);
#elif PUMKIN_DBONES
                        Helpers.DrawDropdownWithToggle(ref _copier_expand_dynamicBoneColliders, ref bCopier_dynamicBones_copyColliders, Strings.Copier.dynamicBones_colliders, Icons.BoneColliderIcon);
#endif
                        }

                        if(_copier_expand_dynamicBoneColliders)
                        {
                            EditorGUI.BeginDisabledGroup(!bCopier_dynamicBones_copyColliders);
                            EditorGUILayout.Space();

                            using(var cHorizontalScope = new GUILayout.HorizontalScope())
                            {
                                GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE); // horizontal indent size

                                using(var cVerticalScope = new GUILayout.VerticalScope())
                                {
                                    bCopier_dynamicBones_removeOldColliders = GUILayout.Toggle(bCopier_dynamicBones_removeOldColliders, Strings.Copier.dynamicBones_removeOldColliders, Styles.CopierToggle);
                                    bCopier_dynamicBones_createObjectsColliders = GUILayout.Toggle(bCopier_dynamicBones_createObjectsColliders, Strings.Copier.copyGameObjects, Styles.CopierToggle);
                                }
                            }

                            EditorGUILayout.Space();
                            EditorGUI.EndDisabledGroup();
                        }

                        Helpers.DrawGUILine(1, false);
                    }

                    if(CopierTabs.ComponentIsInSelectedTab<SkinnedMeshRenderer>(_copier_selectedTab))
                    {
                        //SkinnedMeshRenderer menu
                        Helpers.DrawDropdownWithToggle(ref _copier_expand_skinnedMeshRenderer, ref bCopier_skinMeshRender_copy, Strings.Copier.skinMeshRender, Icons.SkinnedMeshRenderer);
                        if(_copier_expand_skinnedMeshRenderer)
                        {
                            EditorGUI.BeginDisabledGroup(!bCopier_skinMeshRender_copy);
                            EditorGUILayout.Space();

                            using(var cHorizontalScope = new GUILayout.HorizontalScope())
                            {
                                GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE); // horizontal indent size

                                using(var cVerticalScope = new GUILayout.VerticalScope())
                                {
                                    bCopier_skinMeshRender_copySettings = GUILayout.Toggle(bCopier_skinMeshRender_copySettings, Strings.Copier.copySettings, Styles.CopierToggle);
                                    bCopier_skinMeshRender_copyMaterials = GUILayout.Toggle(bCopier_skinMeshRender_copyMaterials, Strings.Copier.skinMeshRender_materials, Styles.CopierToggle);
                                    bCopier_skinMeshRender_copyBlendShapeValues = GUILayout.Toggle(bCopier_skinMeshRender_copyBlendShapeValues, Strings.Copier.skinMeshRender_blendShapeValues, Styles.CopierToggle);
                                }
                            }

                            EditorGUILayout.Space();
                            EditorGUI.EndDisabledGroup();
                        }
                        Helpers.DrawGUILine(1, false);
                    }

                    if(CopierTabs.ComponentIsInSelectedTab<MeshRenderer>(_copier_selectedTab))
                    {
                        //MeshRenderers menu
                        Helpers.DrawDropdownWithToggle(ref _copier_expand_meshRenderers, ref bCopier_meshRenderers_copy, Strings.Copier.meshRenderers, Icons.MeshRenderer);
                        if(_copier_expand_meshRenderers)
                        {
                            EditorGUI.BeginDisabledGroup(!bCopier_meshRenderers_copy);
                            EditorGUILayout.Space();

                            using(var cHorizontalScope = new GUILayout.HorizontalScope())
                            {
                                GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE); // horizontal indent size

                                using(var cVerticalScope = new GUILayout.VerticalScope())
                                {
                                    bCopier_meshRenderers_copySettings = GUILayout.Toggle(bCopier_meshRenderers_copySettings, Strings.Copier.copySettings, Styles.CopierToggle);
                                    bCopier_meshRenderers_createMissing = GUILayout.Toggle(bCopier_meshRenderers_createMissing, Strings.Copier.createMissing, Styles.CopierToggle);
                                    bCopier_meshRenderers_createObjects = GUILayout.Toggle(bCopier_meshRenderers_createObjects, Strings.Copier.copyGameObjects, Styles.CopierToggle);
                                }
                            }

                            EditorGUILayout.Space();
                            EditorGUI.EndDisabledGroup();
                        }

                        Helpers.DrawGUILine(1, false);
                    }

                    if(CopierTabs.ComponentIsInSelectedTab<ParticleSystem>(_copier_selectedTab))
                    {
                        //Particles menu
                        Helpers.DrawDropdownWithToggle(ref _copier_expand_particleSystems, ref bCopier_particleSystems_copy, Strings.Copier.particleSystems, Icons.ParticleSystem);
                        if(_copier_expand_particleSystems)
                        {
                            EditorGUI.BeginDisabledGroup(!bCopier_particleSystems_copy);
                            EditorGUILayout.Space();

                            using(var cHorizontalScope = new GUILayout.HorizontalScope())
                            {
                                GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE); // horizontal indent size

                                using(var cVerticalScope = new GUILayout.VerticalScope())
                                {
                                    bCopier_particleSystems_replace = GUILayout.Toggle(bCopier_particleSystems_replace, Strings.Copier.replaceOld, Styles.CopierToggle);
                                    bCopier_particleSystems_createObjects = GUILayout.Toggle(bCopier_particleSystems_createObjects, Strings.Copier.copyGameObjects, Styles.CopierToggle);
                                }
                            }

                            EditorGUILayout.Space();
                            EditorGUI.EndDisabledGroup();
                        }
                        Helpers.DrawGUILine(1, false);
                    }

                    if(CopierTabs.ComponentIsInSelectedTab<TrailRenderer>(_copier_selectedTab))
                    {
                        //TrailRenderers menu
                        Helpers.DrawDropdownWithToggle(ref _copier_expand_trailRenderers, ref bCopier_trailRenderers_copy, Strings.Copier.trailRenderers, Icons.TrailRenderer);
                        if(_copier_expand_trailRenderers)
                        {
                            EditorGUI.BeginDisabledGroup(!bCopier_trailRenderers_copy);
                            EditorGUILayout.Space();

                            using(var cHorizontalScope = new GUILayout.HorizontalScope())
                            {
                                GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE); // horizontal indent size

                                using(var cVerticalScope = new GUILayout.VerticalScope())
                                {
                                    bCopier_trailRenderers_copySettings = GUILayout.Toggle(bCopier_trailRenderers_copySettings, Strings.Copier.copySettings, Styles.CopierToggle);
                                    bCopier_trailRenderers_createMissing = GUILayout.Toggle(bCopier_trailRenderers_createMissing, Strings.Copier.createMissing, Styles.CopierToggle);
                                    bCopier_trailRenderers_createObjects = GUILayout.Toggle(bCopier_trailRenderers_createObjects, Strings.Copier.copyGameObjects, Styles.CopierToggle);
                                }
                            }

                            EditorGUILayout.Space();
                            EditorGUI.EndDisabledGroup();
                        }
                        Helpers.DrawGUILine(1, false);
                    }

                    if(CopierTabs.ComponentIsInSelectedTab<AudioSource>(_copier_selectedTab))
                    {
                        //AudioSources menu
                        Helpers.DrawDropdownWithToggle(ref _copier_expand_audioSources, ref bCopier_audioSources_copy, Strings.Copier.audioSources, Icons.AudioSource);
                        if(_copier_expand_audioSources)
                        {
                            EditorGUI.BeginDisabledGroup(!bCopier_audioSources_copy);
                            EditorGUILayout.Space();

                            using(var cHorizontalScope = new GUILayout.HorizontalScope())
                            {
                                GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE); // horizontal indent size

                                using(var cVerticalScope = new GUILayout.VerticalScope())
                                {
                                    bCopier_audioSources_copySettings = GUILayout.Toggle(bCopier_audioSources_copySettings, Strings.Copier.copySettings, Styles.CopierToggle);
                                    bCopier_audioSources_createMissing = GUILayout.Toggle(bCopier_audioSources_createMissing, Strings.Copier.createMissing, Styles.CopierToggle);
                                    bCopier_audioSources_createObjects = GUILayout.Toggle(bCopier_audioSources_createObjects, Strings.Copier.copyGameObjects, Styles.CopierToggle);
                                }
                            }

                            EditorGUILayout.Space();
                            EditorGUI.EndDisabledGroup();
                        }
                        Helpers.DrawGUILine(1, false);
                    }

                    if(CopierTabs.ComponentIsInSelectedTab<Light>(_copier_selectedTab))
                    {
                        //Lights menu
                        Helpers.DrawDropdownWithToggle(ref _copier_expand_lights, ref bCopier_lights_copy, Strings.Copier.lights, Icons.Light);
                        if(_copier_expand_lights)
                        {
                            EditorGUI.BeginDisabledGroup(!bCopier_lights_copy);
                            EditorGUILayout.Space();
                            using(var cHorizontalScope = new GUILayout.HorizontalScope())
                            {
                                GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE); // horizontal indent size

                                using(var cVerticalScope = new GUILayout.VerticalScope())
                                {
                                    bCopier_lights_copySettings = GUILayout.Toggle(bCopier_lights_copySettings, Strings.Copier.copySettings, Styles.CopierToggle);
                                    bCopier_lights_createMissing = GUILayout.Toggle(bCopier_lights_createMissing, Strings.Copier.createMissing, Styles.CopierToggle);
                                    bCopier_lights_createObjects = GUILayout.Toggle(bCopier_lights_createObjects, Strings.Copier.copyGameObjects, Styles.CopierToggle);
                                }
                            }
                            EditorGUILayout.Space();
                            EditorGUI.EndDisabledGroup();
                        }

                        Helpers.DrawGUILine(1, false);
                    }

                    if(CopierTabs.ComponentIsInSelectedTab<Rigidbody>(_copier_selectedTab))
                    {
                        //RidigBodies menu
                        Helpers.DrawDropdownWithToggle(ref _copier_expand_rigidBodies, ref bCopier_rigidBodies_copy, Strings.Copier.rigidBodies, Icons.RigidBody);
                        if(_copier_expand_rigidBodies)
                        {
                            EditorGUI.BeginDisabledGroup(!bCopier_rigidBodies_copy);
                            EditorGUILayout.Space();

                            using(var cHorizontalScope = new GUILayout.HorizontalScope())
                            {
                                GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE); // horizontal indent size

                                using(var cVerticalScope = new GUILayout.VerticalScope())
                                {
                                    bCopier_rigidBodies_copySettings = GUILayout.Toggle(bCopier_rigidBodies_copySettings, Strings.Copier.copySettings, Styles.CopierToggle);
                                    bCopier_rigidBodies_createMissing = GUILayout.Toggle(bCopier_rigidBodies_createMissing, Strings.Copier.createMissing, Styles.CopierToggle);
                                    bCopier_rigidBodies_createObjects = GUILayout.Toggle(bCopier_rigidBodies_createObjects, Strings.Copier.copyGameObjects, Styles.CopierToggle);
                                }
                            }

                            EditorGUILayout.Space();
                            EditorGUI.EndDisabledGroup();
                        }
                        Helpers.DrawGUILine(1, false);
                    }

                    if(CopierTabs.ComponentIsInSelectedTab<Joint>(_copier_selectedTab))
                    {
                        //Joints menu
                        Helpers.DrawDropdownWithToggle(ref _copier_expand_joints, ref bCopier_joints_copy, Strings.Copier.joints, Icons.Joint);
                        if(_copier_expand_joints)
                        {
                            EditorGUI.BeginDisabledGroup(!bCopier_joints_copy);
                            EditorGUILayout.Space();

                            using(var cHorizontalScope = new GUILayout.HorizontalScope())
                            {
                                GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE); // horizontal indent size

                                using(var cVerticalScope = new GUILayout.VerticalScope())
                                {
                                    bCopier_joints_fixed = GUILayout.Toggle(bCopier_joints_fixed, Strings.Copier.joints_fixed, Styles.CopierToggle);
                                    bCopier_joints_hinge = GUILayout.Toggle(bCopier_joints_hinge, Strings.Copier.joints_hinge, Styles.CopierToggle);
                                    bCopier_joints_spring = GUILayout.Toggle(bCopier_joints_spring, Strings.Copier.joints_spring, Styles.CopierToggle);
                                    bCopier_joints_character = GUILayout.Toggle(bCopier_joints_character, Strings.Copier.joints_character, Styles.CopierToggle);
                                    bCopier_joints_configurable = GUILayout.Toggle(bCopier_joints_configurable, Strings.Copier.joints_configurable, Styles.CopierToggle);

                                    EditorGUILayout.Space();

                                    bCopier_joints_removeOld = GUILayout.Toggle(bCopier_joints_removeOld, Strings.Copier.joints_removeOld, Styles.CopierToggle);
                                    bCopier_joints_createObjects = GUILayout.Toggle(bCopier_joints_createObjects, Strings.Copier.copyGameObjects, Styles.CopierToggle);
                                }
                            }

                            EditorGUILayout.Space();
                            EditorGUI.EndDisabledGroup();
                        }

                        Helpers.DrawGUILine(1, false);
                    }

                    if(CopierTabs.ComponentIsInSelectedTab<Collider>(_copier_selectedTab))
                    {
                        //Colliders menu
                        Helpers.DrawDropdownWithToggle(ref _copier_expand_colliders, ref bCopier_colliders_copy, Strings.Copier.colliders, Icons.ColliderBox);
                        if(_copier_expand_colliders)
                        {
                            EditorGUI.BeginDisabledGroup(!bCopier_colliders_copy);
                            EditorGUILayout.Space();

                            using(var cHorizontalScope = new GUILayout.HorizontalScope())
                            {
                                GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE); // horizontal indent size

                                using(var cVerticalScope = new GUILayout.VerticalScope())
                                {
                                    bCopier_colliders_copyBox = GUILayout.Toggle(bCopier_colliders_copyBox, Strings.Copier.colliders_box, Styles.CopierToggle);
                                    bCopier_colliders_copyCapsule = GUILayout.Toggle(bCopier_colliders_copyCapsule, Strings.Copier.colliders_capsule, Styles.CopierToggle);
                                    bCopier_colliders_copySphere = GUILayout.Toggle(bCopier_colliders_copySphere, Strings.Copier.colliders_sphere, Styles.CopierToggle);
                                    bCopier_colliders_copyMesh = GUILayout.Toggle(bCopier_colliders_copyMesh, Strings.Copier.colliders_mesh, Styles.CopierToggle);

                                    EditorGUILayout.Space();

                                    bCopier_colliders_removeOld = GUILayout.Toggle(bCopier_colliders_removeOld, Strings.Copier.colliders_removeOld, Styles.CopierToggle);
                                    bCopier_colliders_createObjects = GUILayout.Toggle(bCopier_colliders_createObjects, Strings.Copier.copyGameObjects, Styles.CopierToggle);
                                }
                            }

                            EditorGUILayout.Space();
                            EditorGUI.EndDisabledGroup();
                        }

                        Helpers.DrawGUILine(1, false);
                    }

                    if(CopierTabs.ComponentIsInSelectedTab<Animator>(_copier_selectedTab))
                    {
                        //Animators menu
                        Helpers.DrawDropdownWithToggle(ref _copier_expand_animators, ref bCopier_animators_copy, Strings.Copier.animators, Icons.Animator);
                        if(_copier_expand_animators)
                        {
                            EditorGUI.BeginDisabledGroup(!bCopier_animators_copy);
                            EditorGUILayout.Space();

                            using(var cHorizontalScope = new GUILayout.HorizontalScope())
                            {
                                GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE); // horizontal indent size

                                using(var cVerticalScope = new GUILayout.VerticalScope())
                                {
                                    bCopier_animators_copySettings = GUILayout.Toggle(bCopier_animators_copySettings, Strings.Copier.copySettings, Styles.CopierToggle);
                                    bCopier_animators_createMissing = GUILayout.Toggle(bCopier_animators_createMissing, Strings.Copier.createMissing, Styles.CopierToggle);
                                    bCopier_animators_createObjects = GUILayout.Toggle(bCopier_animators_createObjects, Strings.Copier.copyGameObjects, Styles.CopierToggle);
                                    bCopier_animators_copyMainAnimator = GUILayout.Toggle(bCopier_animators_copyMainAnimator, Strings.Copier.copyMainAnimator, Styles.CopierToggle);
                                }
                            }

                            EditorGUILayout.Space();
                            EditorGUI.EndDisabledGroup();
                        }

                        Helpers.DrawGUILine(1, false);
                    }

                    if(CopierTabs.ComponentIsInSelectedTab<Transform>(_copier_selectedTab))
                    {
                        //Transforms menu
                        Helpers.DrawDropdownWithToggle(ref _copier_expand_transforms, ref bCopier_transforms_copy, Strings.Copier.transforms, Icons.Transform);
                        if(_copier_expand_transforms)
                        {
                            EditorGUI.BeginDisabledGroup(!bCopier_transforms_copy);
                            EditorGUILayout.Space();

                            using(var cHorizontalScope = new GUILayout.HorizontalScope())
                            {
                                GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE); // horizontal indent size

                                using(var cVerticalScope = new GUILayout.VerticalScope())
                                {
                                    bCopier_transforms_copyPosition = GUILayout.Toggle(bCopier_transforms_copyPosition, Strings.Copier.transforms_position, Styles.CopierToggle);
                                    bCopier_transforms_copyRotation = GUILayout.Toggle(bCopier_transforms_copyRotation, Strings.Copier.transforms_rotation, Styles.CopierToggle);
                                    bCopier_transforms_copyScale = GUILayout.Toggle(bCopier_transforms_copyScale, Strings.Copier.transforms_scale, Styles.CopierToggle);
                                }
                            }

                            EditorGUILayout.Space();
                            EditorGUI.EndDisabledGroup();
                        }

                        Helpers.DrawGUILine(1, false);
                    }

                    if(CopierTabs.ComponentIsInSelectedTab<AimConstraint>(_copier_selectedTab))
                    {
                        //Aim Constraints menu
                        Helpers.DrawDropdownWithToggle(ref _copier_expand_aimConstraints, ref bCopier_aimConstraint_copy, Strings.Copier.aimConstraints, Icons.AimConstraint);
                        if(_copier_expand_aimConstraints)
                        {
                            EditorGUI.BeginDisabledGroup(!bCopier_aimConstraint_copy);
                            EditorGUILayout.Space();

                            using(var cHorizontalScope = new GUILayout.HorizontalScope())
                            {
                                GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE); // horizontal indent size

                                using(var cVerticalScope = new GUILayout.VerticalScope())
                                {
                                    bCopier_aimConstraint_replaceOld = GUILayout.Toggle(bCopier_aimConstraint_replaceOld, Strings.Copier.replaceOld, Styles.CopierToggle);
                                    bCopier_aimConstraint_createObjects = GUILayout.Toggle(bCopier_aimConstraint_createObjects, Strings.Copier.copyGameObjects, Styles.CopierToggle);
                                    bCopier_aimConstraint_onlyIfHasValidSources = GUILayout.Toggle(bCopier_aimConstraint_onlyIfHasValidSources, Strings.Copier.onlyIfHasValidSources, Styles.CopierToggle);
                                }
                            }

                            EditorGUILayout.Space();
                            EditorGUI.EndDisabledGroup();
                        }

                        Helpers.DrawGUILine(1, false);
                    }

                    if(CopierTabs.ComponentIsInSelectedTab<LookAtConstraint>(_copier_selectedTab))
                    {
                        //LookAt Constraints menu
                        Helpers.DrawDropdownWithToggle(ref _copier_expand_lookAtConstraints, ref bCopier_lookAtConstraint_copy, Strings.Copier.lookAtConstraints, Icons.LookAtConstraint);
                        if(_copier_expand_lookAtConstraints)
                        {
                            EditorGUI.BeginDisabledGroup(!bCopier_lookAtConstraint_copy);
                            EditorGUILayout.Space();

                            using(var cHorizontalScope = new GUILayout.HorizontalScope())
                            {
                                GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE); // horizontal indent size

                                using(var cVerticalScope = new GUILayout.VerticalScope())
                                {
                                    bCopier_lookAtConstraint_replaceOld = GUILayout.Toggle(bCopier_lookAtConstraint_replaceOld, Strings.Copier.replaceOld, Styles.CopierToggle);
                                    bCopier_lookAtConstraint_createObjects = GUILayout.Toggle(bCopier_lookAtConstraint_createObjects, Strings.Copier.copyGameObjects, Styles.CopierToggle);
                                    bCopier_lookAtConstraint_onlyIfHasValidSources = GUILayout.Toggle(bCopier_lookAtConstraint_onlyIfHasValidSources, Strings.Copier.onlyIfHasValidSources, Styles.CopierToggle);
                                }
                            }

                            EditorGUILayout.Space();
                            EditorGUI.EndDisabledGroup();
                        }

                        Helpers.DrawGUILine(1, false);
                    }

                    if(CopierTabs.ComponentIsInSelectedTab<ParentConstraint>(_copier_selectedTab))
                    {
                        //Parent Constraints menu
                        Helpers.DrawDropdownWithToggle(ref _copier_expand_parentConstraints, ref bCopier_parentConstraint_copy, Strings.Copier.parentConstraints, Icons.ParentConstraint);
                        if(_copier_expand_parentConstraints)
                        {
                            EditorGUI.BeginDisabledGroup(!bCopier_parentConstraint_copy);
                            EditorGUILayout.Space();

                            using(var cHorizontalScope = new GUILayout.HorizontalScope())
                            {
                                GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE); // horizontal indent size

                                using(var cVerticalScope = new GUILayout.VerticalScope())
                                {
                                    bCopier_parentConstraint_replaceOld = GUILayout.Toggle(bCopier_parentConstraint_replaceOld, Strings.Copier.replaceOld, Styles.CopierToggle);
                                    bCopier_parentConstraint_createObjects = GUILayout.Toggle(bCopier_parentConstraint_createObjects, Strings.Copier.copyGameObjects, Styles.CopierToggle);
                                    bCopier_parentConstraint_onlyIfHasValidSources = GUILayout.Toggle(bCopier_parentConstraint_onlyIfHasValidSources, Strings.Copier.onlyIfHasValidSources, Styles.CopierToggle);
                                }
                            }

                            EditorGUILayout.Space();
                            EditorGUI.EndDisabledGroup();
                        }

                        Helpers.DrawGUILine(1, false);
                    }

                    if(CopierTabs.ComponentIsInSelectedTab<PositionConstraint>(_copier_selectedTab))
                    {
                        //Position Constraints menu
                        Helpers.DrawDropdownWithToggle(ref _copier_expand_positionConstraints, ref bCopier_positionConstraint_copy, Strings.Copier.positionConstraints, Icons.PositionConstraint);
                        if(_copier_expand_positionConstraints)
                        {
                            EditorGUI.BeginDisabledGroup(!bCopier_positionConstraint_copy);
                            EditorGUILayout.Space();

                            using(var cHorizontalScope = new GUILayout.HorizontalScope())
                            {
                                GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE); // horizontal indent size

                                using(var cVerticalScope = new GUILayout.VerticalScope())
                                {
                                    bCopier_positionConstraint_replaceOld = GUILayout.Toggle(bCopier_positionConstraint_replaceOld, Strings.Copier.replaceOld, Styles.CopierToggle);
                                    bCopier_positionConstraint_createObjects = GUILayout.Toggle(bCopier_positionConstraint_createObjects, Strings.Copier.copyGameObjects, Styles.CopierToggle);
                                    bCopier_positionConstraint_onlyIfHasValidSources = GUILayout.Toggle(bCopier_positionConstraint_onlyIfHasValidSources, Strings.Copier.onlyIfHasValidSources, Styles.CopierToggle);
                                }
                            }

                            EditorGUILayout.Space();
                            EditorGUI.EndDisabledGroup();
                        }

                        Helpers.DrawGUILine(1, false);
                    }

                    if(CopierTabs.ComponentIsInSelectedTab<RotationConstraint>(_copier_selectedTab))
                    {
                        //Rotation Constraints menu
                        Helpers.DrawDropdownWithToggle(ref _copier_expand_rotationConstraints, ref bCopier_rotationConstraint_copy, Strings.Copier.rotationConstraints, Icons.RotationConstraint);
                        if(_copier_expand_rotationConstraints)
                        {
                            EditorGUI.BeginDisabledGroup(!bCopier_rotationConstraint_copy);
                            EditorGUILayout.Space();

                            using(var cHorizontalScope = new GUILayout.HorizontalScope())
                            {
                                GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE); // horizontal indent size

                                using(var cVerticalScope = new GUILayout.VerticalScope())
                                {
                                    bCopier_rotationConstraint_replaceOld = GUILayout.Toggle(bCopier_rotationConstraint_replaceOld, Strings.Copier.replaceOld, Styles.CopierToggle);
                                    bCopier_rotationConstraint_createObjects = GUILayout.Toggle(bCopier_rotationConstraint_createObjects, Strings.Copier.copyGameObjects, Styles.CopierToggle);
                                    bCopier_rotationConstraint_onlyIfHasValidSources = GUILayout.Toggle(bCopier_rotationConstraint_onlyIfHasValidSources, Strings.Copier.onlyIfHasValidSources, Styles.CopierToggle);
                                }
                            }

                            EditorGUILayout.Space();
                            EditorGUI.EndDisabledGroup();
                        }

                        Helpers.DrawGUILine(1, false);
                    }

                    if(CopierTabs.ComponentIsInSelectedTab<ScaleConstraint>(_copier_selectedTab))
                    {
                        //Scale Constraints menu
                        Helpers.DrawDropdownWithToggle(ref _copier_expand_scaleConstraints, ref bCopier_scaleConstraint_copy, Strings.Copier.scaleConstraints, Icons.ScaleConstraint);
                        if(_copier_expand_scaleConstraints)
                        {
                            EditorGUI.BeginDisabledGroup(!bCopier_scaleConstraint_copy);
                            EditorGUILayout.Space();

                            using(var cHorizontalScope = new GUILayout.HorizontalScope())
                            {
                                GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE); // horizontal indent size

                                using(var cVerticalScope = new GUILayout.VerticalScope())
                                {
                                    bCopier_scaleConstraint_replaceOld = GUILayout.Toggle(bCopier_scaleConstraint_replaceOld, Strings.Copier.replaceOld, Styles.CopierToggle);
                                    bCopier_scaleConstraint_createObjects = GUILayout.Toggle(bCopier_scaleConstraint_createObjects, Strings.Copier.copyGameObjects, Styles.CopierToggle);
                                    bCopier_scaleConstraint_onlyIfHasValidSources = GUILayout.Toggle(bCopier_scaleConstraint_onlyIfHasValidSources, Strings.Copier.onlyIfHasValidSources, Styles.CopierToggle);
                                }
                            }

                            EditorGUILayout.Space();
                            EditorGUI.EndDisabledGroup();
                        }

                        Helpers.DrawGUILine(1, false);
                    }

                    if(CopierTabs.ComponentIsInSelectedTab("other", _copier_selectedTab))
                    {
                        //Other menu
                        Helpers.DrawDropdownWithToggle(ref _copier_expand_other, ref bCopier_other_copy, Strings.Copier.other, Icons.CsScript);
                        if(_copier_expand_other)
                        {
                            EditorGUI.BeginDisabledGroup(!bCopier_other_copy);
                            EditorGUILayout.Space();

                            using(var cHorizontalScope = new GUILayout.HorizontalScope())
                            {
                                GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE); // horizontal indent size

                                using(var cVerticalScope = new GUILayout.VerticalScope())
                                {
                                    bCopier_other_copyIKFollowers = GUILayout.Toggle(bCopier_other_copyIKFollowers, Strings.Copier.other_ikFollowers, Styles.CopierToggle);
                                    bCopier_other_copyVRMSpringBones = GUILayout.Toggle(bCopier_other_copyVRMSpringBones, Strings.Copier.other_vrmSpringBones, Styles.CopierToggle);
                                }
                            }

                            EditorGUILayout.Space();
                            EditorGUI.EndDisabledGroup();
                        }

                        Helpers.DrawGUILine(1, false);
                    }

                    EditorGUILayout.Space();

                    //=======================================================

                    //Ignore Array
                    EditorGUI.BeginChangeCheck();
                    {
                        Helpers.DrawPropertyArrayScrolling(SerializedIgnoreArray, Strings.Copier.exclusions, ref _copierIgnoreArray_expand, ref _copierIgnoreArrayScroll, 0, 100);
                    }
                    if(EditorGUI.EndChangeCheck())
                    {
                        RefreshIgnoreArray();
                    }

                    if(_copierIgnoreArray_expand && SerializedIgnoreArray.arraySize > 0)
                    {
                        using(var cHorizontalScope = new GUILayout.HorizontalScope())
                        {
                            GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE * 4); // horizontal indent size
                            using(var cVerticalScope = new GUILayout.VerticalScope())
                            {
                                bCopier_ignoreArray_includeChildren = GUILayout.Toggle(bCopier_ignoreArray_includeChildren, Strings.Copier.includeChildren);
                            }
                        }
                    }

                    Helpers.DrawGUILine();

                    EditorGUILayout.BeginHorizontal();
                    {
                        //TODO: Refactor these 2 buttons
                        if(GUILayout.Button(Strings.Buttons.selectNone, Styles.BigButton, GUILayout.MinWidth(100)))
                        {
                            if(_copier_selectedTab == CopierTabs.Tab.All)
                            {
                                bCopier_colliders_copy = false;
                                bCopier_rigidBodies_copy = false;
                                bCopier_transforms_copy = false;
                                bCopier_animators_copy = false;
                                bCopier_aimConstraint_copy = false;
                                bCopier_lookAtConstraint_copy = false;
                                bCopier_parentConstraint_copy = false;
                                bCopier_positionConstraint_copy = false;
                                bCopier_rotationConstraint_copy = false;
                                bCopier_scaleConstraint_copy = false;
                                bCopier_other_copy = false;
                                bCopier_joints_copy = false;
                            }

                            bCopier_descriptor_copy = false;
                            bCopier_trailRenderers_copy = false;
                            bCopier_lights_copy = false;
                            bCopier_skinMeshRender_copy = false;
                            bCopier_audioSources_copy = false;
                            bCopier_meshRenderers_copy = false;
                            bCopier_particleSystems_copy = false;

                            if(DynamicBonesExist)
                            {
                                bCopier_dynamicBones_copy = false;
                                bCopier_dynamicBones_copyColliders = false;
                            }
                        }
                        if(GUILayout.Button(Strings.Buttons.selectAll, Styles.BigButton, GUILayout.MinWidth(100)))
                        {
                            if(_copier_selectedTab == CopierTabs.Tab.All)
                            {
                                bCopier_colliders_copy = true;
                                bCopier_rigidBodies_copy = true;
                                bCopier_transforms_copy = true;
                                bCopier_animators_copy = true;
                                bCopier_aimConstraint_copy = true;
                                bCopier_lookAtConstraint_copy = true;
                                bCopier_parentConstraint_copy = true;
                                bCopier_positionConstraint_copy = true;
                                bCopier_rotationConstraint_copy = true;
                                bCopier_scaleConstraint_copy = true;
                                bCopier_other_copy = true;
                                bCopier_joints_copy = true;
                            }

                            bCopier_descriptor_copy = true;
                            bCopier_trailRenderers_copy = true;
                            bCopier_lights_copy = true;
                            bCopier_skinMeshRender_copy = true;
                            bCopier_audioSources_copy = true;
                            bCopier_meshRenderers_copy = true;
                            bCopier_particleSystems_copy = true;

                            if(DynamicBonesExist)
                            {
                                bCopier_dynamicBones_copy = true;
                                bCopier_dynamicBones_copyColliders = true;
                            }
                            else
                            {
                                bCopier_dynamicBones_copy = false;
                                bCopier_dynamicBones_copyColliders = false;
                            }
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    Helpers.DrawGUILine();

                    EditorGUI.BeginDisabledGroup(!CopierHasSelections);
                    {
                        if(GUILayout.Button(Strings.Buttons.copySelected, Styles.BigButton))
                        {
                            string log = "";
                            if(!CopierSelectedFrom)
                            {
                                log += Strings.Log.copyFromInvalid;
                                Log(log, LogType.Warning);
                            }
                            else
                            {
                                //Cancel Checks
                                if(CopierSelectedFrom == SelectedAvatar)
                                {
                                    Log(log + Strings.Log.cantCopyToSelf, LogType.Warning);
                                    return;
                                }

                                RefreshIgnoreArray();

                                CopyComponents(CopierSelectedFrom, SelectedAvatar);

                                EditorUtility.SetDirty(SelectedAvatar);
                                if(!EditorApplication.isPlaying)
                                    EditorSceneManager.MarkSceneDirty(SelectedAvatar.scene);

                                avatarInfo = PumkinsAvatarInfo.GetInfo(SelectedAvatar, out _avatarInfoString);

                                log += Strings.Log.done;
                                Log(log, LogType.Log);
                            }
                        }
                    }
                    EditorGUI.EndDisabledGroup();
                }
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.Space();
            }
        }

        public void DrawInfoMenuGUI()
        {
            if(_info_expand = GUILayout.Toggle(_info_expand, Strings.Main.info, Styles.Foldout_title))
            {
                EditorGUILayout.Space();

                GUILayout.BeginHorizontal();
                {
                    if(GUILayout.Button(new GUIContent(Strings.Buttons.openGithubPage, Icons.GithubIcon)))
                    {
                        Application.OpenURL(Strings.LINK_GITHUB);
                    }
                    if(GUILayout.Button(new GUIContent(Strings.Buttons.openHelpPage, Icons.Help)))
                    {
                        Application.OpenURL(Strings.LINK_GITHUB + "wiki");
                    }
                }
                GUILayout.EndHorizontal();

                if(GUILayout.Button(new GUIContent(Strings.Buttons.joinDiscordServer, Icons.DiscordIcon)))
                {
                    Application.OpenURL(Strings.LINK_DISCORD);
                }
                if(GUILayout.Button(new GUIContent(Strings.Buttons.openDonationPage, Icons.KofiIcon), Styles.BigButton))
                {
                    Application.OpenURL(Strings.LINK_DONATION);
                }
            }
        }

        public void DrawThumbnailsMenuGUI()
        {
            if(_thumbnails_expand = GUILayout.Toggle(_thumbnails_expand, Strings.Main.thumbnails, Styles.Foldout_title))
            {
                Helpers.DrawGUILine();

#if VRC_SDK_VRCSDK2 || (VRC_SDK_VRCSDK3 && !UDON)
                EditorGUI.BeginChangeCheck();
                {
                    shouldHideOtherAvatars = GUILayout.Toggle(shouldHideOtherAvatars, Strings.Thumbnails.hideOtherAvatars);
                }
                if(EditorApplication.isPlaying && EditorGUI.EndChangeCheck())
                {
                    HideAllOtherAvatars(shouldHideOtherAvatars, SelectedAvatar);
                }
#endif

                Helpers.DrawGUILine();

                EditorGUI.BeginChangeCheck();
                {
                    _presetToolbarSelectedIndex = GUILayout.Toolbar(_presetToolbarSelectedIndex, new string[] { Strings.Thumbnails.cameras, Strings.Thumbnails.poses, Strings.Thumbnails.blendshapes }, Styles.ToolbarBigButtons);
                }
                if(EditorGUI.EndChangeCheck())
                {
                    if(_presetToolbarSelectedIndex == (int)PresetToolbarOptions.Blendshape)
                        SetupBlendeshapeRendererHolders(SelectedAvatar);
                }

                EditorGUILayout.Space();
                Helpers.DrawGUILine();

                switch(_presetToolbarSelectedIndex)
                {
                    case 0:
                        DrawThumbnailCameraGUI();
                        DrawPresetGUI<PumkinsCameraPreset>();
                        break;
                    case 1:
                        DrawThumbnailPoseGUI();
                        DrawPresetGUI<PumkinsPosePreset>();
                        break;
                    case 2:
                        DrawThumbanailBlendshapeGUI();
                        DrawPresetGUI<PumkinsBlendshapePreset>();
                        break;
                    default:
                        break;
                }
            }
        }

        public void DrawThumbanailBlendshapeGUI()
        {
            EditorGUILayout.LabelField(new GUIContent(Strings.Thumbnails.blendshapes));
            if(SelectedAvatar)
                Helpers.DrawBlendshapeSlidersWithLabels(ref _selectedAvatarRendererHolders, SelectedAvatar);
            else
                EditorGUILayout.LabelField(new GUIContent(Strings.PoseEditor.selectHumanoidAvatar), Styles.HelpBox_OneLine);
            EditorGUILayout.Space();
        }

        public void DrawThumbnailPoseGUI()
        {
            if(GUILayout.Button(Strings.Buttons.openPoseEditor, Styles.BigButton))
                PumkinsPoseEditor.ShowWindow();

            Helpers.DrawGUILine();

            posePresetApplyBodyPosition = GUILayout.Toggle(posePresetApplyBodyPosition, Strings.Thumbnails.applyBodyPosition);
            posePresetApplyBodyRotation = GUILayout.Toggle(posePresetApplyBodyRotation, Strings.Thumbnails.applyBodyRotation);

            EditorGUILayout.Space();

            posePresetTryFixSinking = GUILayout.Toggle(posePresetTryFixSinking, Strings.Thumbnails.tryFixPoseSinking);
        }

        public void DrawThumbnailCameraGUI()
        {
            //TODO: Make it so camera isn't being searched for every frame in the property
            SelectedCamera = EditorGUILayout.ObjectField(Strings.Thumbnails.selectedCamera, SelectedCamera, typeof(Camera), true) as Camera;

            Helpers.DrawGUILine();

            DrawOverlayGUI();

            Helpers.DrawGUILine();

            DrawBackgroundGUI();

            Helpers.DrawGUILine();

            DrawCameraControlButtons();
        }

        public void DrawCameraControlButtons()
        {
            //Camera to scene view button
            if(GUILayout.Button(Strings.Buttons.alignCameraToView, Styles.BigButton))
            {
                SelectedCamera.transform.position = SceneView.lastActiveSceneView.camera.transform.position;
                SelectedCamera.transform.rotation = SceneView.lastActiveSceneView.camera.transform.rotation;
            }

            EditorGUILayout.Space();

            lockSelectedCameraToSceneView = GUILayout.Toggle(lockSelectedCameraToSceneView, Strings.Thumbnails.lockSelectedCameraToSceneView);

            Helpers.DrawGUILine();

            EditorGUI.BeginDisabledGroup(!SelectedCamera || !SelectedAvatar);
            {
                //Center Camera on Viewpoint button
                GUILayout.BeginHorizontal();
                {
                    string centerOnWhat = "?";
                    switch(centerCameraMode)
                    {
                        case PumkinsCameraPreset.CameraOffsetMode.AvatarRoot:
                            centerOnWhat = Strings.Main.avatar;
                            break;
                        case PumkinsCameraPreset.CameraOffsetMode.Transform:
                            if(SelectedAvatar && !centerCameraTransform)
                                centerCameraTransform = SelectedAvatar.transform.Find(centerCameraTransformPath);
                            if(centerCameraTransform)
                                centerOnWhat = centerCameraTransform.name;
                            break;
                        default:
                            centerOnWhat = Strings.Thumbnails.viewpoint;
                            break;
                    }

                    string centerCameraString = string.Format(Strings.Thumbnails.centerCameraOn, centerOnWhat);
                    if(GUILayout.Button(centerCameraString, Styles.BigButton))
                    {
                        if(SelectedCamera)
                        {
                            switch(centerCameraMode)
                            {
                                case PumkinsCameraPreset.CameraOffsetMode.AvatarRoot:
                                    CenterCameraOnTransform(SelectedAvatar.transform, centerCameraPositionOffsetAvatar, centerCameraRotationOffsetAvatar, centerCameraFixClippingPlanes);
                                    break;
                                case PumkinsCameraPreset.CameraOffsetMode.Transform:
                                    CenterCameraOnTransform(centerCameraTransform, centerCameraPositionOffsetTransform, centerCameraRotationOffsetTransform, centerCameraFixClippingPlanes);
                                    break;
#if VRC_SDK_VRCSDK2 || (VRC_SDK_VRCSDK3 && !UDON)
                                default:
                                    CenterCameraOnViewpoint(SelectedAvatar, centerCameraPositionOffsetViewpoint, centerCameraRotationOffsetViewpoint, centerCameraFixClippingPlanes);
                                    break;
#endif
                            }
                        }
                        else
                            Log(Strings.Warning.cameraNotFound, LogType.Warning);
                    }
                    if(GUILayout.Button(Icons.Settings, Styles.BigIconButton))
                    {
                        _centerCameraOffsets_expand = !_centerCameraOffsets_expand;
                    }
                }
                GUILayout.EndHorizontal();
                if(_centerCameraOffsets_expand)
                {
                    EditorGUILayout.Space();

                    centerCameraFixClippingPlanes = GUILayout.Toggle(centerCameraFixClippingPlanes, Strings.Thumbnails.centerCameraFixClippingPlanes);

                    EditorGUILayout.Space();

                    centerCameraMode = (PumkinsCameraPreset.CameraOffsetMode)EditorGUILayout.EnumPopup(Strings.Presets.mode, centerCameraMode);

                    if(centerCameraMode == PumkinsCameraPreset.CameraOffsetMode.Transform)
                    {
                        EditorGUI.BeginChangeCheck();
                        {
                            centerCameraTransformPath = EditorGUILayout.TextField(Strings.Presets.transform, centerCameraTransformPath);
                        }
                        if(EditorGUI.EndChangeCheck())
                        {
                            centerCameraTransform = SelectedAvatar.transform.Find(centerCameraTransformPath);
                        }
                    }
                    else
                        GUILayout.Space(18);

                    EditorGUILayout.Space();

                    switch(centerCameraMode)
                    {
                        case PumkinsCameraPreset.CameraOffsetMode.Transform:
                            centerCameraPositionOffsetTransform = EditorGUILayout.Vector3Field(Strings.Thumbnails.positionOffset, centerCameraPositionOffsetTransform);
                            centerCameraRotationOffsetTransform = EditorGUILayout.Vector3Field(Strings.Thumbnails.rotationOffset, centerCameraRotationOffsetTransform);
                            break;
#if VRC_SDK_VRCSDK2 || (VRC_SDK_VRCSDK3 && !UDON)
                        case PumkinsCameraPreset.CameraOffsetMode.Viewpoint:
                            centerCameraPositionOffsetViewpoint = EditorGUILayout.Vector3Field(Strings.Thumbnails.positionOffset, centerCameraPositionOffsetViewpoint);
                            centerCameraRotationOffsetViewpoint = EditorGUILayout.Vector3Field(Strings.Thumbnails.rotationOffset, centerCameraRotationOffsetViewpoint);
                            break;
#endif
                        case PumkinsCameraPreset.CameraOffsetMode.AvatarRoot:
                            centerCameraPositionOffsetAvatar = EditorGUILayout.Vector3Field(Strings.Thumbnails.positionOffset, centerCameraPositionOffsetAvatar);
                            centerCameraRotationOffsetAvatar = EditorGUILayout.Vector3Field(Strings.Thumbnails.rotationOffset, centerCameraRotationOffsetAvatar);
                            break;
                    }

                    EditorGUILayout.Space();

                    EditorGUILayout.BeginHorizontal();
                    {
                        if(GUILayout.Button(Strings.Buttons.reset, GUILayout.MaxWidth(90f)))
                        {
                            switch(centerCameraMode)
                            {
#if VRC_SDK_VRCSDK2 || (VRC_SDK_VRCSDK3 && !UDON)
                                case PumkinsCameraPreset.CameraOffsetMode.Viewpoint:
                                    centerCameraPositionOffsetViewpoint = DEFAULT_CAMERA_POSITION_OFFSET_VIEWPOINT;
                                    centerCameraRotationOffsetViewpoint = DEFAULT_CAMERA_ROTATION_OFFSET_VIEWPOINT;
                                    break;
#endif
                                case PumkinsCameraPreset.CameraOffsetMode.AvatarRoot:
                                    centerCameraPositionOffsetAvatar = DEFAULT_CAMERA_POSITION_OFFSET_AVATAR;
                                    centerCameraRotationOffsetAvatar = DEFAULT_CAMERA_ROTATION_OFFSET_AVATAR;
                                    break;
                                case PumkinsCameraPreset.CameraOffsetMode.Transform:
                                    centerCameraPositionOffsetTransform = DEFAULT_CAMERA_POSITION_OFFSET_TRANSFORM;
                                    centerCameraRotationOffsetTransform = DEFAULT_CAMERA_ROTATION_OFFSET_TRANSFORM;
                                    break;
                            }
                        }
                        if(GUILayout.Button(Strings.Buttons.setFromCamera))
                        {
                            SerialTransform st = null;
                            switch(centerCameraMode)
                            {
#if VRC_SDK_VRCSDK2 || (VRC_SDK_VRCSDK3 && !UDON)
                                case PumkinsCameraPreset.CameraOffsetMode.Viewpoint:
                                    st = PumkinsCameraPreset.GetCameraOffsetFromViewpoint(SelectedAvatar, SelectedCamera);
                                    if(st)
                                    {
                                        centerCameraPositionOffsetViewpoint = Helpers.RoundVectorValues(st.localPosition, 3);
                                        centerCameraRotationOffsetViewpoint = Helpers.RoundVectorValues(st.localEulerAngles, 3);
                                    }
                                    break;
#endif
                                case PumkinsCameraPreset.CameraOffsetMode.AvatarRoot:
                                    st = PumkinsCameraPreset.GetOffsetsFromTransform(SelectedAvatar.transform, SelectedCamera);
                                    if(st)
                                    {
                                        centerCameraPositionOffsetAvatar = Helpers.RoundVectorValues(st.localPosition, 3);
                                        centerCameraRotationOffsetAvatar = Helpers.RoundVectorValues(st.localEulerAngles, 3);
                                    }
                                    break;
                                case PumkinsCameraPreset.CameraOffsetMode.Transform:
                                    st = PumkinsCameraPreset.GetOffsetsFromTransform(centerCameraTransform, SelectedCamera);
                                    if(st)
                                    {
                                        centerCameraPositionOffsetTransform = Helpers.RoundVectorValues(st.localPosition, 3);
                                        centerCameraRotationOffsetTransform = Helpers.RoundVectorValues(st.localEulerAngles, 3);
                                    }
                                    break;
                            }
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUI.EndDisabledGroup();
        }

        public void DrawAvatarInfoMenuGUI()
        {
            if(_avatarInfo_expand = GUILayout.Toggle(_avatarInfo_expand, Strings.Main.avatarInfo, Styles.Foldout_title))
            {
                if(SelectedAvatar == null)
                {
                    if(avatarInfo != null)
                    {
                        avatarInfo = null;
                        _avatarInfoString = Strings.AvatarInfo.selectAvatarFirst;
                    }
                }
                else
                {
                    if(avatarInfo == null)
                    {
                        avatarInfo = PumkinsAvatarInfo.GetInfo(SelectedAvatar, out _avatarInfoString);
                    }
                }

                EditorGUILayout.SelectableLabel(_avatarInfoString, Styles.HelpBox, GUILayout.MinHeight(260));

                EditorGUI.BeginDisabledGroup(SelectedAvatar == null);
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        if(GUILayout.Button(Strings.Buttons.copy))
                        {
                            EditorGUIUtility.systemCopyBuffer = _avatarInfoString;
                        }
                        if(GUILayout.Button(Strings.Buttons.refresh))
                        {
                            avatarInfo = PumkinsAvatarInfo.GetInfo(SelectedAvatar, out _avatarInfoString);
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUI.EndDisabledGroup();
            }
        }

        public void DrawToolsMenuGUI()
        {
            if(_tools_expand = GUILayout.Toggle(_tools_expand, Strings.Main.tools, Styles.Foldout_title))
            {
                EditorGUI.BeginDisabledGroup(SelectedAvatar == null);
                {
                    Helpers.DrawGUILine();

                    //Quick setup
                    GUILayout.BeginHorizontal();
                    {
                        if(GUILayout.Button(Strings.Buttons.quickSetupAvatar, Styles.BigButton))
                        {
                            //if(_tools_quickSetup_autoRig)
                            //    SetupRig(SelectedAvatar);
#if VRC_SDK_VRCSDK2 || (VRC_SDK_VRCSDK3 && !UDON)
                            if(_tools_quickSetup_fillVisemes)
                                DoAction(SelectedAvatar, ToolMenuActions.FillVisemes);
                            if(_tools_quickSetup_setViewpoint)
                                QuickSetViewpoint(SelectedAvatar, _tools_quickSetup_viewpointZDepth);
#endif
                            if(_tools_quickSetup_forceTPose)
                                DoAction(SelectedAvatar, ToolMenuActions.SetTPose);
                            if(!Helpers.StringIsNullOrWhiteSpace(_tools_quickSetup_setRenderAnchor_path))
                            {
                                if(_tools_quickSetup_setSkinnedMeshRendererAnchor)
                                    SetSkinnedMeshRendererAnchor(SelectedAvatar, _tools_quickSetup_setRenderAnchor_path);
                                if(_tools_quickSetup_setMeshRendererAnchor)
                                    SetMeshRendererAnchor(SelectedAvatar, _tools_quickSetup_setRenderAnchor_path);
                            }
                        }

                        if(GUILayout.Button(Icons.Settings, Styles.BigIconButton))
                            _tools_quickSetup_settings_expand = !_tools_quickSetup_settings_expand;
                    }
                    GUILayout.EndHorizontal();

                    if(_tools_quickSetup_settings_expand)
                    {
                        EditorGUILayout.Space();

#if VRC_SDK_VRCSDK2 || (VRC_SDK_VRCSDK3 && !UDON)
                        GUILayout.BeginHorizontal();
                        {
                            Vector2 size = EditorStyles.toggle.CalcSize(new GUIContent(Strings.Tools.autoViewpoint));
                            _tools_quickSetup_setViewpoint = GUILayout.Toggle(_tools_quickSetup_setViewpoint, Strings.Tools.autoViewpoint, GUILayout.MaxWidth(size.x));

                            size = EditorStyles.numberField.CalcSize(new GUIContent(Strings.Tools.viewpointZDepth));
                            EditorGUI.BeginDisabledGroup(!_tools_quickSetup_setViewpoint);
                            {
                                float old = EditorGUIUtility.labelWidth;
                                EditorGUIUtility.labelWidth = size.x * 1.1f;
                                _tools_quickSetup_viewpointZDepth = EditorGUILayout.FloatField(Strings.Tools.viewpointZDepth, _tools_quickSetup_viewpointZDepth);
                                EditorGUIUtility.labelWidth = old;
                            }
                            EditorGUI.EndDisabledGroup();
                        }
                        GUILayout.EndHorizontal();

                        _tools_quickSetup_fillVisemes = GUILayout.Toggle(_tools_quickSetup_fillVisemes, Strings.Tools.fillVisemes);
#endif
                        _tools_quickSetup_forceTPose = GUILayout.Toggle(_tools_quickSetup_forceTPose, Strings.Tools.setTPose);
                        //_tools_quickSetup_autoRig = GUILayout.Toggle(_tools_quickSetup_autoRig, "_Setup Rig");

                        EditorGUILayout.Space();

                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.Label(Strings.Tools.anchorPath);
                            _tools_quickSetup_setRenderAnchor_path = EditorGUILayout.TextField(_tools_quickSetup_setRenderAnchor_path);
                        }
                        GUILayout.EndHorizontal();

                        EditorGUI.BeginDisabledGroup(Helpers.StringIsNullOrWhiteSpace(_tools_quickSetup_setRenderAnchor_path));
                        {
                            _tools_quickSetup_setSkinnedMeshRendererAnchor = GUILayout.Toggle(_tools_quickSetup_setSkinnedMeshRendererAnchor, Strings.Tools.setSkinnedMeshRendererAnchors);
                            _tools_quickSetup_setMeshRendererAnchor = GUILayout.Toggle(_tools_quickSetup_setMeshRendererAnchor, Strings.Tools.setMeshRendererAnchors);
                        }
                        EditorGUI.EndDisabledGroup();
                    }

                    Helpers.DrawGUILine();

                    //Tools
                    if(_tools_avatar_expand = GUILayout.Toggle(_tools_avatar_expand, Strings.Main.avatar, Styles.Foldout))
                    {
                        GUILayout.BeginHorizontal(); //Row
                        {
                            GUILayout.BeginVertical(GUILayout.ExpandWidth(true)); //Left Column
                            {
#if VRC_SDK_VRCSDK2 || (VRC_SDK_VRCSDK3 && !UDON)
                                if(GUILayout.Button(Strings.Tools.fillVisemes))
                                    DoAction(SelectedAvatar, ToolMenuActions.FillVisemes);
#endif
                                if(GUILayout.Button(Strings.Tools.revertBlendshapes))
                                    DoAction(SelectedAvatar, ToolMenuActions.RevertBlendshapes);
                                if(GUILayout.Button(Strings.Tools.resetPose))
                                    DoAction(SelectedAvatar, ToolMenuActions.ResetPose);
                                EditorGUI.BeginDisabledGroup(DrawingHandlesGUI);
                                {
                                    if(GUILayout.Button(Strings.Tools.revertScale))
                                        DoAction(SelectedAvatar, ToolMenuActions.RevertScale);
                                }
                                EditorGUI.EndDisabledGroup();

                                //if(GUILayout.Button(Strings.Tools.resetBoundingBoxes))
                                //    DoAction(SelectedAvatar, ToolMenuActions.ResetBoundingBoxes);
                            }
                            GUILayout.EndVertical();

                            GUILayout.BeginVertical(GUILayout.ExpandWidth(true)); //Right Column
                            {
#if VRC_SDK_VRCSDK2 || (VRC_SDK_VRCSDK3 && !UDON)
                                EditorGUI.BeginDisabledGroup(DrawingHandlesGUI);
                                {
                                    if(GUILayout.Button(Strings.Tools.editViewpoint))
                                        DoAction(SelectedAvatar, ToolMenuActions.EditViewpoint);
                                }
                                EditorGUI.EndDisabledGroup();
#endif
#if (VRC_SDK_VRCSDK3 && !UDON)
                                if(GUILayout.Button(Strings.Tools.fillEyeBones))
                                    DoAction(SelectedAvatar, ToolMenuActions.FillEyeBones);
#endif

                                if(GUILayout.Button(Strings.Tools.zeroBlendshapes))
                                    DoAction(SelectedAvatar, ToolMenuActions.ZeroBlendshapes);

                                if(GUILayout.Button(Strings.Tools.resetToTPose))
                                    DoAction(SelectedAvatar, ToolMenuActions.SetTPose);
                                EditorGUI.BeginDisabledGroup(DrawingHandlesGUI);
                                {
                                    if(GUILayout.Button(Strings.Tools.editScale))
                                        DoAction(SelectedAvatar, ToolMenuActions.EditScale);
                                }
                                EditorGUI.EndDisabledGroup();
                            }
                            GUILayout.EndVertical();
                        }
                        GUILayout.EndHorizontal();

                        GUILayout.Space(15);

                        _tools_quickSetup_setRenderAnchor_path = EditorGUILayout.TextField(Strings.Tools.anchorPath, _tools_quickSetup_setRenderAnchor_path);
                        EditorGUI.BeginDisabledGroup(Helpers.StringIsNullOrWhiteSpace(_tools_quickSetup_setRenderAnchor_path));
                        {
                            if(GUILayout.Button(Strings.Tools.setSkinnedMeshRendererAnchors))
                                SetSkinnedMeshRendererAnchor(SelectedAvatar, _tools_quickSetup_setRenderAnchor_path);
                            if(GUILayout.Button(Strings.Tools.setMeshRendererAnchors))
                                SetMeshRendererAnchor(SelectedAvatar, _tools_quickSetup_setRenderAnchor_path);
                        }
                        EditorGUI.EndDisabledGroup();
                    }

                    Helpers.DrawGUILine();

                    //Dynamic bones toggle

                    //Setup dbone gui stuff
                    string dboneStateString = Strings.Copier.dynamicBones;
                    if(!DynamicBonesExist)
                        dboneStateString += " (" + Strings.Warning.notFound + ")";

                    if(_tools_dynamicBones_expand = GUILayout.Toggle(_tools_dynamicBones_expand, dboneStateString, Styles.Foldout))
                    {
                        EditorGUI.BeginDisabledGroup(!DynamicBonesExist);
                        {
                            EditorGUILayout.BeginHorizontal();
                            {
                                if(GUILayout.Button(Strings.Tools.disableDynamicBones))
                                    SetDynamicBonesEnabledState(SelectedAvatar, false);
                                if(GUILayout.Button(Strings.Tools.enableDynamicBones))
                                    SetDynamicBonesEnabledState(SelectedAvatar, true);
                            }
                            EditorGUILayout.EndHorizontal();

                            if(DrawToggleButtonGUI(Strings.Tools.toggleDynamicBones, _nextToggleDBoneState))
                                ToggleDynamicBonesEnabledState(SelectedAvatar, ref _nextToggleDBoneState, ref _dBonesThatWereAlreadyDisabled);

                            EditorGUILayout.Space();

                            if(GUILayout.Button(Strings.Tools.fixDynamicBoneScripts, Styles.BigButton))
                                DoAction(SelectedAvatar, ToolMenuActions.FixDynamicBoneScripts);
                        }
                        EditorGUI.EndDisabledGroup();
                    }

                    Helpers.DrawGUILine();

                    //Remove all
                    if(_tools_removeAll_expand = GUILayout.Toggle(_tools_removeAll_expand, Strings.Main.removeAll, Styles.Foldout))
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true)); //Left Column
                            {
                                EditorGUI.BeginDisabledGroup(!DynamicBonesExist);
                                {
                                    if(GUILayout.Button(new GUIContent(Strings.Copier.dynamicBones, Icons.BoneIcon)))
                                        DoAction(SelectedAvatar, ToolMenuActions.RemoveDynamicBones);
                                }
                                EditorGUI.EndDisabledGroup();

                                if(GUILayout.Button(new GUIContent(Strings.Copier.particleSystems, Icons.ParticleSystem)))
                                    DoAction(SelectedAvatar, ToolMenuActions.RemoveParticleSystems);
                                if(GUILayout.Button(new GUIContent(Strings.Copier.lights, Icons.Light)))
                                    DoAction(SelectedAvatar, ToolMenuActions.RemoveLights);
                                if(GUILayout.Button(new GUIContent(Strings.Copier.joints, Icons.Joint)))
                                    DoAction(SelectedAvatar, ToolMenuActions.RemoveJoints);
                                if(GUILayout.Button(new GUIContent(Strings.Copier.animators_inChildren, Icons.Animator)))
                                    DoAction(SelectedAvatar, ToolMenuActions.RemoveAnimatorsInChildren);
                                if(GUILayout.Button(new GUIContent(Strings.Copier.colliders, Icons.ColliderBox)))
                                    DoAction(SelectedAvatar, ToolMenuActions.RemoveColliders);
#if VRC_SDK_VRCSDK2 || (VRC_SDK_VRCSDK3 && !UDON)
                                if(GUILayout.Button(new GUIContent(Strings.Copier.other_ikFollowers, Icons.CsScript)))
                                    DoAction(SelectedAvatar, ToolMenuActions.RemoveIKFollowers);
                                EditorGUILayout.Space();
#else

                                GUILayout.Space(32);
#endif

                                if(GUILayout.Button(new GUIContent(Strings.Copier.aimConstraints, Icons.AimConstraint)))
                                    DoAction(SelectedAvatar, ToolMenuActions.RemoveAimConstraint);
                                if(GUILayout.Button(new GUIContent(Strings.Copier.lookAtConstraints, Icons.LookAtConstraint)))
                                    DoAction(SelectedAvatar, ToolMenuActions.RemoveLookAtConstraint);
                                if(GUILayout.Button(new GUIContent(Strings.Copier.parentConstraints, Icons.ParentConstraint)))
                                    DoAction(SelectedAvatar, ToolMenuActions.RemoveParentConstraint);
                            }
                            EditorGUILayout.EndVertical();

                            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true)); //Right Column
                            {
                                EditorGUI.BeginDisabledGroup(!DynamicBonesExist);
                                {
                                    if(GUILayout.Button(new GUIContent(Strings.Copier.dynamicBones_colliders, Icons.BoneColliderIcon)))
                                        DoAction(SelectedAvatar, ToolMenuActions.RemoveDynamicBoneColliders);
                                }
                                EditorGUI.EndDisabledGroup();

                                if(GUILayout.Button(new GUIContent(Strings.Copier.trailRenderers, Icons.TrailRenderer)))
                                    DoAction(SelectedAvatar, ToolMenuActions.RemoveTrailRenderers);
                                if(GUILayout.Button(new GUIContent(Strings.Copier.audioSources, Icons.AudioSource)))
                                    DoAction(SelectedAvatar, ToolMenuActions.RemoveAudioSources);
                                if(GUILayout.Button(new GUIContent(Strings.Copier.rigidBodies, Icons.RigidBody)))
                                    DoAction(SelectedAvatar, ToolMenuActions.RemoveRigidBodies);
                                if(GUILayout.Button(new GUIContent(Strings.Copier.meshRenderers, Icons.MeshRenderer)))
                                    DoAction(SelectedAvatar, ToolMenuActions.RemoveMeshRenderers);
                                if(GUILayout.Button(new GUIContent(Strings.Copier.emptyGameObjects, Icons.Prefab)))
                                    DoAction(SelectedAvatar, ToolMenuActions.RemoveEmptyGameObjects);
                                if(GUILayout.Button(new GUIContent(Strings.Copier.other_emptyScripts, Icons.SerializableAsset)))
                                    DoAction(SelectedAvatar, ToolMenuActions.RemoveMissingScripts);

                                EditorGUILayout.Space();

                                if(GUILayout.Button(new GUIContent(Strings.Copier.positionConstraints, Icons.PositionConstraint)))
                                    DoAction(SelectedAvatar, ToolMenuActions.RemovePositionConstraint);
                                if(GUILayout.Button(new GUIContent(Strings.Copier.rotationConstraints, Icons.RotationConstraint)))
                                    DoAction(SelectedAvatar, ToolMenuActions.RemoveRotationConstraint);
                                if(GUILayout.Button(new GUIContent(Strings.Copier.scaleConstraints, Icons.ScaleConstraint)))
                                    DoAction(SelectedAvatar, ToolMenuActions.RemoveScaleConstraint);
                            }
                            EditorGUILayout.EndVertical();
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUI.EndDisabledGroup();
                    Helpers.DrawGUILine();

#if VRC_SDK_VRCSDK2 || (VRC_SDK_VRCSDK3 && !UDON)
                    if(GUILayout.Button(Strings.Tools.refreshSDK, Styles.BigButton))
                        RefreshSDK();
#endif
                    EditorGUILayout.Space();
                }
            }
        }

        //static void FixDynamicBoneScripts(GameObject avatar)
        //{
        //    var dboneType = _DependencyChecker.GetTypeFromName("DynamicBone");
        //    if(dboneType == null)
        //    {
        //        Debug.Log("Can't find dynamic bones in project");
        //        return;
        //    }

        //    Component tempDBone = null;
        //    try
        //    {
        //        var db = Resources.FindObjectsOfTypeAll(dboneType);
        //        tempDBone = avatar.AddComponent(dboneType);
        //        SerializedProperty tempScript = new SerializedObject(tempDBone).FindProperty("m_Script");

        //        var comps = avatar.GetComponentsInChildren<Component>().Where(c => c != null);
        //        foreach(var comp in comps)
        //        {
        //            if(comp.GetType() != dboneType)
        //                continue;
        //            var ms = MonoScript.FromMonoBehaviour(comp as MonoBehaviour);

        //            var serialComp = new SerializedObject(comp);
        //            var compScript = serialComp.FindProperty("m_Script");
        //            var dboneScript = new SerializedObject(ms).FindProperty("m_Script");
        //            var refs = dboneScript.FindPropertyRelative("m_DefaultReferences");
        //            //boneScript.objectReferenceValue = tempScript.objectReferenceValue;
        //            //boneScript.objectReferenceInstanceIDValue = tempScript.objectReferenceInstanceIDValue;
        //            //compScript.FindPropertyRelative("m_PathID").longValue = dboneScript.FindPropertyRelative("m_PathID").longValue;
        //            //compScript.FindPropertyRelative("m_FileID").longValue = dboneScript.FindPropertyRelative("m_FileID").longValue;
        //            var en = dboneScript.Copy();
        //            while(en.Next(true))
        //                Debug.Log(en.name);
        //            serialComp.ApplyModifiedProperties();
        //        }
        //    }
        //    finally
        //    {
        //        Helpers.DestroyAppropriate(tempDBone);
        //    }
        //}

        /// <summary>
        /// Bad function. Does too many things at once and other bad stuff. Will fix once I get a better yaml serializer
        /// </summary>
        /// <param name="avatar"></param>
        private static void FixDynamicBoneScriptsInPrefab(GameObject avatar)
        {
#if PUMKIN_DBONES || PUMKIN_OLD_DBONES
            bool selectionIsInAssets = false;
            var prefStage = PrefabStageUtility.GetCurrentPrefabStage();
            var prefType = PrefabUtility.GetPrefabAssetType(avatar);

            if(prefStage != null)
            {
                Log(Strings.Log.exitPrefabModeFirst, LogType.Warning);
            }
            else if(prefType == PrefabAssetType.NotAPrefab)
            {
                Log(Strings.Log.avatarHasNoPrefab, LogType.Error);
                return;
            }
            else
            {
                if(Helpers.IsAssetInAssets(avatar))
                    selectionIsInAssets = true;
                Log(Strings.Log.attemptingToFixDynamicBoneScripts, LogType.Log);
            }

            try
            {
                string prefPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(SelectedAvatar);

                if(string.IsNullOrEmpty(prefPath))
                    return;

                var guids = AssetDatabase.FindAssets("DynamicBone");
                string dboneGUID = null;
                string dboneColliderGUID = null;
                foreach(var guid in guids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    if(path.EndsWith("DynamicBone.cs"))
                        dboneGUID = guid;
                    else if(path.EndsWith("DynamicBoneCollider.cs"))
                        dboneColliderGUID = guid;

                    if(!string.IsNullOrEmpty(dboneGUID) && !string.IsNullOrEmpty(dboneColliderGUID))
                        break;
                }

                if(string.IsNullOrEmpty(dboneGUID) && string.IsNullOrEmpty(dboneColliderGUID))
                {
                    Log("Can't find DynamicBones for some reason", LogType.Error);
                    return;
                }

                var blocks = PumkinsYAMLTools.OpenFileGetBlocks(prefPath);
                for(int i = 0; i < blocks.Length; i++)
                {
                    if(blocks[i].StartsWith("MonoBehaviour:")) //Check that it's a script
                    {
                        if(blocks[i].Contains("m_Colliders:")  //Check if it's the DBone component
                        && blocks[i].Contains("m_Exclusions:")
                        && blocks[i].Contains("m_Damping:"))
                        {
                            var lines = PumkinsYAMLTools.BlockToLines(blocks[i]);
                            for(int j = 0; j < lines.Length; j++)
                            {
                                if(!lines[j].Contains("m_Script:"))
                                    continue;

                                lines[j] = Helpers.ReplaceGUIDInLine(lines[j], dboneGUID, out bool _);
                                break;
                            }
                            blocks[i] = PumkinsYAMLTools.LinesToBlock(lines);
                        }
                        else if(blocks[i].Contains("m_Radius:") //Check if it's the DBoneCollider component
                            && blocks[i].Contains("m_Height:")
                            && blocks[i].Contains("m_Center:")
                            && blocks[i].Contains("m_Direction:")
                            && blocks[i].Contains("m_Bound:"))
                        {
                            var lines = PumkinsYAMLTools.BlockToLines(blocks[i]);
                            for(int j = 0; j < lines.Length; j++)
                            {
                                if(!lines[j].Contains("m_Script:"))
                                    continue;

                                lines[j] = Helpers.ReplaceGUIDInLine(lines[j], dboneColliderGUID, out bool _);
                                break;
                            }
                            blocks[i] = PumkinsYAMLTools.LinesToBlock(lines);
                        }
                    }
                }

                PumkinsYAMLTools.WriteBlocksToFile(prefPath, blocks);
                AssetDatabase.ImportAsset(prefPath, ImportAssetOptions.ForceUpdate);

                if(selectionIsInAssets)
                {
                    var prefObj = AssetDatabase.LoadAssetAtPath<GameObject>(prefPath);
                    if(prefObj != null)
                    {
                        EditorGUIUtility.PingObject(prefObj);
                        Selection.activeObject = null;
                        EditorApplication.delayCall += () =>
                        {
                            EditorApplication.delayCall += () =>
                            {
                                Selection.activeObject = prefObj;
                            };
                        };
                    }
                }

                Log(Strings.Log.done);
            }
            catch(Exception e)
            {
                Log(e.Message);
            }
#else
        return;
#endif
        }

        public bool DrawToggleButtonGUI(string text, ref bool toggleBool)
        {
            Vector2 size = EditorGUIUtility.GetIconSize();
            bool b = GUILayout.Button(new GUIContent(text, toggleBool ? Icons.ToggleOff : Icons.ToggleOn), Styles.ButtonWithToggle);
            if(b)
                toggleBool = !toggleBool;
            return b;
        }

        public bool DrawToggleButtonGUI(string text, bool toggleBool)
        {
            bool b = GUILayout.Button(new GUIContent(text, toggleBool ? Icons.ToggleOff : Icons.ToggleOn), Styles.ButtonWithToggle);
            return b;
        }

        public void DrawPresetGUI<T>() where T : PumkinPreset
        {
            List<PumkinPreset> pr = new List<PumkinPreset>();
            string labelString = "Preset";
            IEnumerable<string> dropdownOptions = new List<string>();

            SerializedProperty pSelectedPresetString = null;
            SerializedProperty pSelectedPresetIndex = null;

            if(typeof(T) == typeof(PumkinsCameraPreset))
            {
                pSelectedPresetString = Instance.SerializedScript.FindProperty("_selectedCameraPresetString");
                pSelectedPresetIndex = Instance.SerializedScript.FindProperty("_selectedCameraPresetIndex");

                pr = PumkinsPresetManager.CameraPresets.Cast<PumkinPreset>().ToList();

                labelString = Strings.Thumbnails.cameras;
                dropdownOptions = PumkinsPresetManager.CameraPresets.Select(o => o.name);
            }
            else if(typeof(T) == typeof(PumkinsPosePreset))
            {
                pSelectedPresetString = Instance.SerializedScript.FindProperty("_selectedPosePresetString");
                pSelectedPresetIndex = Instance.SerializedScript.FindProperty("_selectedPosePresetIndex");

                pr = PumkinsPresetManager.PosePresets.Cast<PumkinPreset>().ToList();

                labelString = Strings.Thumbnails.poses;
                dropdownOptions = PumkinsPresetManager.PosePresets.Select(o => o.name);
            }
            else if(typeof(T) == typeof(PumkinsBlendshapePreset))
            {
                pSelectedPresetString = Instance.SerializedScript.FindProperty("_selectedBlendshapePresetString");
                pSelectedPresetIndex = Instance.SerializedScript.FindProperty("_selectedBlendshapePresetIndex");

                pr = PumkinsPresetManager.BlendshapePresets.Cast<PumkinPreset>().ToList();

                labelString = Strings.Thumbnails.blendshapes;
                dropdownOptions = PumkinsPresetManager.BlendshapePresets.Select(o => o.name);
            }

            if(pSelectedPresetIndex.intValue == -1)
                RefreshPresetIndex<T>();

            bool shouldDisable = !SelectedAvatar || (pr.Count > 0 && pSelectedPresetIndex.intValue >= pr.Count && pr[pSelectedPresetIndex.intValue] == null);

            Helpers.DrawGUILine();

            GUILayout.BeginHorizontal();
            {
                EditorGUI.BeginChangeCheck();
                {
                    pSelectedPresetIndex.intValue = EditorGUILayout.Popup(labelString, pSelectedPresetIndex.intValue, dropdownOptions.ToArray(), Styles.Popup);
                }
                if(EditorGUI.EndChangeCheck())
                {
                    pSelectedPresetString.stringValue = pr[pSelectedPresetIndex.intValue].ToString() ?? "";
                }

                if(GUILayout.Button(Icons.Refresh, Styles.IconButton))
                {
                    PumkinsPresetManager.LoadPresets<T>();
                }
            }
            GUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUI.BeginDisabledGroup(pr.Count == 0 || shouldDisable);
            {
                GUILayout.BeginHorizontal();
                {
                    if(GUILayout.Button(Strings.Buttons.edit))
                    {
                        int newIndex = PumkinsPresetManager.GetPresetIndex<T>(pSelectedPresetString.stringValue);
                        if(newIndex == -1)
                            RefreshPresetStringByIndex<T>(pSelectedPresetIndex.intValue);
                        else
                            pSelectedPresetIndex.intValue = newIndex;

                        pr[pSelectedPresetIndex.intValue].ApplyPreset(SelectedAvatar);

                        if(typeof(T) == typeof(PumkinsCameraPreset))
                            CreateCameraPresetPopup.ShowWindow(pr[pSelectedPresetIndex.intValue] as PumkinsCameraPreset);
                        else if(typeof(T) == typeof(PumkinsPosePreset))
                            CreatePosePresetPopup.ShowWindow(pr[pSelectedPresetIndex.intValue] as PumkinsPosePreset);
                        else if(typeof(T) == typeof(PumkinsBlendshapePreset))
                        {
                            CreateBlendshapePopup.ShowWindow(pr[pSelectedPresetIndex.intValue] as PumkinsBlendshapePreset);
                            PumkinsAvatarTools.SetupBlendeshapeRendererHolders(SelectedAvatar);
                        }
                    }
                    if(GUILayout.Button(Strings.Buttons.load))
                    {
                        if(typeof(T) == typeof(PumkinsBlendshapePreset))
                            Instance.DoAction(SelectedAvatar, ToolMenuActions.RevertBlendshapes);

                        int newIndex = PumkinsPresetManager.GetPresetIndex<T>(pSelectedPresetString.stringValue);
                        if(newIndex == -1)
                            RefreshPresetStringByIndex<T>(pSelectedPresetIndex.intValue);
                        else
                            pSelectedPresetIndex.intValue = newIndex;

                        pr[pSelectedPresetIndex.intValue].ApplyPreset(SelectedAvatar);

                        if(typeof(T) == typeof(PumkinsBlendshapePreset))
                            SetupBlendeshapeRendererHolders(SelectedAvatar);
                    }
                }
                GUILayout.EndHorizontal();
            }
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(pr.Count == 0);
            {
                if(GUILayout.Button(Strings.Buttons.selectInAssets))
                {
                    var asset = pr[pSelectedPresetIndex.intValue];
                    if(asset)
                    {
                        Helpers.SelectAndPing(asset);
                    }
                }
            }
            EditorGUI.EndDisabledGroup();

            if(GUILayout.Button(Strings.Buttons.selectFolder))
            {
                string path = null;
                if(typeof(T) == typeof(PumkinsCameraPreset))
                    path = PumkinsPresetManager.localCamerasPath;
                else if(typeof(T) == typeof(PumkinsPosePreset))
                    path = PumkinsPresetManager.localPosesPath;
                else if(typeof(T) == typeof(PumkinsBlendshapePreset))
                    path = PumkinsPresetManager.localBlendshapesPath;
                Helpers.SelectAndPing(path);
            }

            Helpers.DrawGUILine();

            EditorGUI.BeginDisabledGroup(!SelectedAvatar);
            {
                if(GUILayout.Button(Strings.Buttons.createNewPreset, Styles.BigButton))
                {
                    if(typeof(T) == typeof(PumkinsCameraPreset))
                        CreateCameraPresetPopup.ShowWindow();
                    else if(typeof(T) == typeof(PumkinsPosePreset))
                        CreatePosePresetPopup.ShowWindow();
                    else if(typeof(T) == typeof(PumkinsBlendshapePreset))
                        CreateBlendshapePopup.ShowWindow();
                }

                if(GUILayout.Button(Strings.Buttons.reset))
                {
#if VRC_SDK_VRCSDK2 || (VRC_SDK_VRCSDK3 && !UDON)
                    if(typeof(T) == typeof(PumkinsCameraPreset))
                        CenterCameraOnViewpoint(SelectedAvatar, DEFAULT_CAMERA_POSITION_OFFSET_VIEWPOINT, DEFAULT_CAMERA_ROTATION_OFFSET_VIEWPOINT, centerCameraFixClippingPlanes);
                    else
#endif
                    if(typeof(T) == typeof(PumkinsPosePreset))
                        DoAction(SelectedAvatar, ToolMenuActions.ResetPose);
                    else if(typeof(T) == typeof(PumkinsBlendshapePreset))
                    {
                        DoAction(SelectedAvatar, ToolMenuActions.RevertBlendshapes);
                        PumkinsAvatarTools.SetupBlendeshapeRendererHolders(SelectedAvatar);
                    }
                }
            }
            EditorGUI.EndDisabledGroup();

            Helpers.DrawGUILine();

            SerializedScript.ApplyModifiedProperties();
        }

        /// <summary>
        /// Draws the "Use Background" part of the thumbnail menu
        /// </summary>
        public void DrawBackgroundGUI()
        {
            bool needsRefresh = false;
            RawImage raw = _cameraBackgroundImage; //GetCameraBackgroundRawImage(false);
            GameObject background = _cameraBackground; //GetCameraBackground();

            EditorGUI.BeginDisabledGroup(!_selectedCamera);
            if(Helpers.DrawDropdownWithToggle(ref _thumbnails_useCameraBackground_expand, ref bThumbnails_use_camera_background, Strings.Thumbnails.useCameraBackground))
            {
                RefreshBackgroundOverrideType();
                needsRefresh = true;

                if(bThumbnails_use_camera_background && _selectedCamera)
                    _thumbsCameraBgClearFlagsOld = SelectedCamera.clearFlags;
                else
                    RestoreCameraClearFlags();
            }
            EditorGUI.EndDisabledGroup();

            if(_thumbnails_useCameraBackground_expand || needsRefresh)
            {
                EditorGUILayout.Space();
                EditorGUI.BeginDisabledGroup(!_selectedCamera);
                {
                    EditorGUI.BeginChangeCheck();
                    {
                        cameraBackgroundType = (PumkinsCameraPreset.CameraBackgroundOverrideType)EditorGUILayout.EnumPopup(Strings.Thumbnails.backgroundType, cameraBackgroundType);
                    }
                    if(EditorGUI.EndChangeCheck())
                    {
                        RefreshBackgroundOverrideType();
                    }
                    EditorGUILayout.Space();

                    switch(cameraBackgroundType)
                    {
                        case PumkinsCameraPreset.CameraBackgroundOverrideType.Color:
                            {
                                EditorGUI.BeginChangeCheck();
                                {
                                    _thumbsCamBgColor = EditorGUILayout.ColorField(Strings.Thumbnails.backgroundType_Color, _selectedCamera ? SelectedCamera.backgroundColor : Color.grey);
                                }
                                if(EditorGUI.EndChangeCheck())
                                {
                                    SetCameraBackgroundToColor(Instance._thumbsCamBgColor);
                                }
                            }
                            break;
                        case PumkinsCameraPreset.CameraBackgroundOverrideType.Skybox:
                            {
                                if(bThumbnails_use_camera_background && _selectedCamera)
                                {
                                    SelectedCamera.clearFlags = CameraClearFlags.Skybox;
                                }

                                Material mat = RenderSettings.skybox;
                                EditorGUI.BeginChangeCheck();
                                {
                                    mat = EditorGUILayout.ObjectField(Strings.Thumbnails.backgroundType_Material, mat, typeof(Material), true) as Material;
                                }
                                if(EditorGUI.EndChangeCheck())
                                {
                                    SetCameraBackgroundToSkybox(mat);
                                }
                            }
                            break;
                        case PumkinsCameraPreset.CameraBackgroundOverrideType.Image:
                            {
                                if(bThumbnails_use_camera_background && _selectedCamera)
                                    SelectedCamera.clearFlags = _thumbsCameraBgClearFlagsOld;

                                EditorGUILayout.Space();
                                GUILayout.BeginHorizontal();
                                {
                                    EditorGUILayout.SelectableLabel(_backgroundPath, Styles.TextField);
                                    if(GUILayout.Button(Strings.Buttons.browse, GUILayout.MaxWidth(60)) && SelectedCamera)
                                    {
                                        string newPath = Helpers.OpenImageGetPath(_lastOpenFilePath);
                                        if(!string.IsNullOrEmpty(newPath))
                                        {
                                            _lastOpenFilePath = newPath;
                                            SetBackgroundToImageFromPath(_lastOpenFilePath);
                                        }
                                    }
                                    if(GUILayout.Button("X", GUILayout.MaxWidth(25)))
                                    {
                                        _backgroundPath = null;
                                        SetBackgroundToImageFromTexture((Texture2D)null);
                                    }
                                }
                                EditorGUILayout.EndHorizontal();

                                EditorGUI.BeginDisabledGroup(!cameraBackgroundTexture);
                                {
                                    EditorGUI.BeginChangeCheck();
                                    {
                                        cameraBackgroundImageTint = EditorGUILayout.ColorField(Strings.Thumbnails.tint, cameraBackgroundImageTint);
                                    }
                                    if(EditorGUI.EndChangeCheck())
                                    {
                                        if(raw)
                                            raw.color = cameraBackgroundImageTint;
                                    }
                                }
                                EditorGUI.EndDisabledGroup();
                            }
                            break;
                        default:
                            break;
                    }
                }
                EditorGUI.EndDisabledGroup();
            }
        }

        /// <summary>
        /// Draws the "Use Overlay" section in the thumbnails menu
        /// </summary>
        public void DrawOverlayGUI()
        {
            bool needsRefresh = false;
            RawImage raw = _cameraOverlayImage; //GetCameraOverlayRawImage(false);
            GameObject overlay = _cameraOverlay; //GetCameraOverlay(false);

            EditorGUI.BeginDisabledGroup(!_selectedCamera);
            if(Helpers.DrawDropdownWithToggle(ref _thumbnails_useCameraOverlay_expand, ref bThumbnails_use_camera_overlay, Strings.Thumbnails.useCameraOverlay))
            {
                if(cameraOverlayTexture == null && !string.IsNullOrEmpty(_overlayPath))
                    SetOverlayToImageFromPath(_overlayPath);

                needsRefresh = true;
            }
            EditorGUI.EndDisabledGroup();

            if(_thumbnails_useCameraOverlay_expand || needsRefresh)
            {
                EditorGUI.BeginDisabledGroup(!bThumbnails_use_camera_overlay);
                {
                    EditorGUILayout.Space();
                    GUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.SelectableLabel(_overlayPath, Styles.TextField);
                        if(GUILayout.Button(Strings.Buttons.browse, GUILayout.MaxWidth(60)) && SelectedCamera)
                        {
                            string newPath = Helpers.OpenImageGetPath(_lastOpenFilePath);
                            if(!string.IsNullOrEmpty(newPath))
                            {
                                _lastOpenFilePath = newPath;
                                SetOverlayToImageFromPath(_lastOpenFilePath);
                            }
                        }
                        if(GUILayout.Button("X", GUILayout.MaxWidth(25)))
                        {
                            _overlayPath = null;
                            SetOverlayToImageFromTexture(null);
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUI.BeginDisabledGroup(!cameraOverlayTexture);
                    {
                        EditorGUI.BeginChangeCheck();
                        {
                            cameraOverlayImageTint = EditorGUILayout.ColorField(Strings.Thumbnails.tint, cameraOverlayImageTint);
                        }
                        if(EditorGUI.EndChangeCheck())
                        {
                            if(raw)
                                raw.color = cameraOverlayImageTint;
                        }
                    }
                    EditorGUI.EndDisabledGroup();
                }
                EditorGUI.EndDisabledGroup();
            }
        }

        /// <summary>
        /// Selects the thumbnail preset option and scrolls down
        /// </summary>
        /// <param name="option"></param>
        public void SelectThumbnailPresetToolbarOption(PresetToolbarOptions option)
        {
            _presetToolbarSelectedIndex = (int)option;
            _mainScroll = new Vector2(0, 1000);
        }

#endregion

#region Main Functions

#if VRC_SDK_VRCSDK2 || (VRC_SDK_VRCSDK3 && !UDON)
        /// <summary>
        /// This will hide or show all avatars except avatarToKeep
        /// </summary>
        public void HideAllOtherAvatars(bool hidden, GameObject avatarToKeep)
        {
            if(hidden && !avatarToKeep)
                return;

            VRC_AvatarDescriptor desc = avatarToKeep.GetComponent<VRC_AvatarDescriptor>();

            if(!desc)
                return;

            var av = FindObjectsOfType<VRC_AvatarDescriptor>();
            if(desc != null)
            {
                for(int i = 0; i < av.Length; i++)
                {
                    if(av[i] != desc)
                        av[i].transform.root.gameObject.SetActive(!hidden); //why did I call it hidden
                }
            }
        }
#endif

        /// <summary>
        /// Loads textures back into overlay and background objects if we have a path for them still stored. Useful for when we restart unity
        /// </summary>
        private void RestoreTexturesFromPaths()
        {
            RawImage overlayImg = GetCameraOverlayRawImage(bThumbnails_use_camera_overlay);
            RawImage backgroundImg = GetCameraBackgroundRawImage(bThumbnails_use_camera_background);

            if(!string.IsNullOrEmpty(_overlayPath))
            {
                if(overlayImg)
                {
                    if(overlayImg.texture)
                    {
                        cameraOverlayTexture = (Texture2D)overlayImg.texture;
                        overlayImg.color = cameraOverlayImageTint;
                    }
                    else
                    {
                        SetOverlayToImageFromPath(_overlayPath);
                    }
                }
            }
            else if(overlayImg && overlayImg.texture)
            {
                cameraOverlayTexture = null;
                overlayImg.texture = null;
            }

            if(!string.IsNullOrEmpty(_backgroundPath))
            {
                if(backgroundImg)
                {
                    if(backgroundImg.texture)
                    {
                        cameraBackgroundTexture = (Texture2D)backgroundImg.texture;
                        backgroundImg.color = cameraBackgroundImageTint;
                    }
                    else
                    {
                        SetBackgroundToImageFromPath(_backgroundPath);
                    }
                }
            }
            else if(backgroundImg && backgroundImg.texture)
            {
                cameraBackgroundTexture = null;
                backgroundImg.texture = null;
            }
        }

        /// <summary>
        /// Sets overlay texture to image from path
        /// </summary>
        /// <param name="texturePath"></param>
        public void SetOverlayToImageFromPath(string texturePath)
        {
            _overlayPath = texturePath;
            if(!GetCameraOverlay() || !GetCameraOverlayRawImage())
                return;

            Texture2D tex = Helpers.GetImageTextureFromPath(texturePath);
            SetOverlayToImageFromTexture(tex);
            if(tex)
            {
                string texName = string.IsNullOrEmpty(texturePath) ? "empty" : Path.GetFileName(texturePath);
                Log(Strings.Log.loadedImageAsOverlay, LogType.Log, texName);
            }
            else
            {
                Log(Strings.Warning.cantLoadImageAtPath, LogType.Warning, texturePath);
            }
        }
        /// <summary>
        /// Sets overlay image to texture
        /// </summary>
        public void SetOverlayToImageFromTexture(Texture2D newTexture)
        {
            var img = GetCameraOverlayRawImage();
            var fg = GetCameraOverlay();
            if(fg && img)
            {
                img.color = cameraOverlayImageTint;
                img.texture = newTexture;
                if(img.canvas)
                    img.canvas.worldCamera = SelectedCamera;
            }
        }

        /// <summary>
        /// Sets background texture to image from path
        /// </summary>
        /// <param name="texturePath"></param>
        public void SetBackgroundToImageFromPath(string texturePath)
        {
            _backgroundPath = texturePath;
            if(!GetCameraOverlay() || !GetCameraOverlayRawImage())
                return;

            Texture2D tex = Helpers.GetImageTextureFromPath(texturePath);
            SetBackgroundToImageFromTexture(tex);
            if(tex)
            {
                string texName = string.IsNullOrEmpty(texturePath) ? "empty" : Path.GetFileName(texturePath);
                Log(Strings.Log.loadedImageAsBackground, LogType.Log, texName);
            }
            else if(!string.IsNullOrEmpty(texturePath))
            {
                Log(Strings.Warning.cantLoadImageAtPath, LogType.Warning, texturePath);
            }
        }
        /// <summary>
        /// Sets background to image from texture
        /// </summary>
        /// <param name="newTexture"></param>
        public void SetBackgroundToImageFromTexture(Texture2D newTexture)
        {
            var img = GetCameraBackgroundRawImage();
            var bg = GetCameraBackground();
            if(bg && img)
            {
                img.color = cameraBackgroundImageTint;
                img.texture = newTexture;
                if(img.canvas)
                    img.canvas.worldCamera = SelectedCamera;
            }
        }

        /// <summary>
        /// Sets camera background clear flags to skybox and changes skybox to material
        /// </summary>
        public void SetCameraBackgroundToSkybox(Material skyboxMaterial)
        {
            if(!_selectedCamera)
                return;

            SelectedCamera.clearFlags = CameraClearFlags.Skybox;
            RenderSettings.skybox = skyboxMaterial;
        }

        /// <summary>
        /// Changes camera clear flags to solid color and sets background color
        /// </summary>
        public void SetCameraBackgroundToColor(Color color)
        {
            if(!_selectedCamera)
                return;

            _thumbsCamBgColor = color;
            SelectedCamera.backgroundColor = color;
            SelectedCamera.clearFlags = CameraClearFlags.SolidColor;
        }

#if UNITY_2018
        /// <summary>
        /// Doesn't work
        /// </summary>
        public void SetupRig(GameObject avatar)
        {
            GameObject pref = PrefabUtility.GetCorrespondingObjectFromOriginalSource(avatar);
            string prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(pref);
            var prefType = PrefabUtility.GetPrefabAssetType(pref);
            //Helpers.MakeHumanAvatar("Assets/Avatars/Stylish Energy Ukon/Models/original.fbx", false);

            if(prefType == PrefabAssetType.Model)
            {
                //GameObject newAvatar = PrefabUtility.InstantiatePrefab(pref) as GameObject;
                try
                {
                    ////Animator anim = newAvatar.GetComponent<Animator>();
                    ////if(anim == null)
                    ////{
                    ////    anim = newAvatar.AddComponent<Animator>();
                    ////    anim.applyRootMotion = true;
                    ////    anim.updateMode = AnimatorUpdateMode.Normal;
                    ////    anim.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
                    ////}

                    ////HumanBone[] humanBones = new HumanBone[HumanTrait.BoneName.Length];
                    ////for(int i = 0; i < humanBones.Length; i++)
                    ////{
                    ////    humanBones[i] = HumanRig.GetHumanBone(HumanTrait.BoneName[i], newAvatar.transform);
                    ////}

                    ////List<SkeletonBone> skeletonBones = new List<SkeletonBone>();
                    ////foreach(Transform t in newAvatar.GetComponentsInChildren<Transform>())
                    ////{
                    ////    skeletonBones.Add(new SkeletonBone()
                    ////    {
                    ////        name = t.name,
                    ////        position = t.localPosition,
                    ////        rotation = t.localRotation,
                    ////        scale = t.localScale
                    ////    });
                    ////}

                    ////HumanDescription h = new HumanDescription()
                    ////{
                    ////    human = humanBones,
                    ////    skeleton = skeletonBones.ToArray(),
                    ////    armStretch = 0.05f,
                    ////    hasTranslationDoF = false,
                    ////    feetSpacing = 0,
                    ////    legStretch = 0.05f,
                    ////    lowerArmTwist = 0.5f,
                    ////    lowerLegTwist = 0.5f,
                    ////    upperArmTwist = 0.5f,
                    ////    upperLegTwist = 0.5f,
                    ////};

                    ////Avatar ava = AvatarBuilder.BuildHumanAvatar(newAvatar, h);

                    ////string avPath = "Assets/_tempAvatar.asset";
                    ////AssetDatabase.CreateAsset(ava, avPath);
                    ////AssetDatabase.SaveAssets();

                    //if(!anim.isHuman && ava.isValid)
                    //{
                    //    anim.avatar = ava;
                    //}
                    //else
                    //{
                    //    anim.avatar = AvatarBuilder.BuildGenericAvatar(newAvatar, "");
                    //}
                }
                finally
                {
                    //DestroyImmediate(newAvatar);
                }
            }
        }
#endif

#if VRC_SDK_VRCSDK2 || (VRC_SDK_VRCSDK3 && !UDON)
        /// <summary>
        /// Quickly sets viewpoint to eye height if avatar is humanoid
        /// </summary>
        /// <param name="zDepth">Z Depth value of viewpoint</param>
        public void QuickSetViewpoint(GameObject avatar, float zDepth)
        {
            VRC_AvatarDescriptor desc = avatar.GetComponent<VRC_AvatarDescriptor>() ?? avatar.AddComponent<VRC_AvatarDescriptor>();
            var anim = SelectedAvatar.GetComponent<Animator>();

            desc.ViewPosition = Helpers.GetViewpointAtEyeLevel(SelectedAvatar.GetComponent<Animator>());
            desc.ViewPosition.z = zDepth;

            if(anim.isHuman)
                Log(Strings.Log.settingQuickViewpoint, LogType.Log, desc.ViewPosition.ToString());
            else
                Log(Strings.Log.cantSetViewpointNonHumanoid, LogType.Warning, desc.ViewPosition.ToString());
        }
#endif

        /// <summary>
        /// Tries to get the VRCCam, returns Camera.main if not found
        /// </summary>
        private static Camera GetVRCCamOrMainCam()
        {
            var obj = GameObject.Find("VRCCam");
            if(!obj)
            {
                Camera cam = Camera.main;
                if(cam)
                    obj = cam.gameObject;
            }
            if(obj)
                return obj.GetComponent<Camera>();
            return null;
        }

        /// <summary>
        /// Sets the root object of our current scene selection as our selected avatar
        /// </summary>
        public static void SelectAvatarFromScene()
        {
            try
            {
                var sel = Selection.activeGameObject;
                if(sel == null)
                    return;

                sel = Selection.activeGameObject.transform.root.gameObject;
                if(sel != null)
                {
                    if(sel.gameObject.scene.name != null)
                    {
                        SelectedAvatar = sel;
                        avatarInfo = PumkinsAvatarInfo.GetInfo(SelectedAvatar, out _avatarInfoString);
                    }
                    else if(!_useSceneSelectionAvatar)
                    {
                        Log(Strings.Warning.selectSceneObject, LogType.Warning);
                    }
                }
            }
            catch(Exception e)
            {
                Log(e.Message, LogType.Warning);
            }
            _PumkinsAvatarToolsWindow.RequestRepaint(_PumkinsAvatarToolsWindow.ToolsWindow);
        }

#if VRC_SDK_VRCSDK2 || (VRC_SDK_VRCSDK3 && !UDON)
        /// <summary>
        /// Sets the avatar scale and moves the viewpoint to compensate
        /// </summary>
        private void SetAvatarScaleAndMoveViewpoint(VRC_AvatarDescriptor desc, float newScale)
        {
            if(_editingScale)
            {
                SelectedAvatar.transform.localScale = Helpers.RoundVectorValues(new Vector3(newScale, newScale, newScale), 3);
                if(_scaleViewpointDummy)
                    _viewPosTemp = _scaleViewpointDummy.position;
                else
                    EndScalingAvatar(desc.gameObject, true);
            }
            else
            {
                var tempDummy = new GameObject(DUMMY_NAME).transform;
                tempDummy.position = desc.ViewPosition + desc.transform.root.position;
                tempDummy.parent = SelectedAvatar.transform;
                desc.transform.root.localScale = Helpers.RoundVectorValues(new Vector3(newScale, newScale, newScale), 3);
                SetViewpoint(desc, tempDummy.position);
                DestroyImmediate(tempDummy.gameObject);
                Log(Strings.Log.setAvatarScaleTo, LogType.Log, newScale.ToString(), desc.ViewPosition.ToString());
            }
        }
#endif

        private void SetAvatarScale(float newScale)
        {
            if(_editingScale)
            {
                SelectedAvatar.transform.localScale = Helpers.RoundVectorValues(new Vector3(newScale, newScale, newScale), 3);
#if VRC_SDK_VRCSDK2 || (VRC_SDK_VRCSDK3 && !UDON)
                if(_scaleViewpointDummy)
                    _viewPosTemp = _scaleViewpointDummy.position;
                else
                    EndScalingAvatar(SelectedAvatar, true);
#endif
            }
            else
            {
                SelectedAvatar.transform.root.localScale = Helpers.RoundVectorValues(new Vector3(newScale, newScale, newScale), 3);
                Log(Strings.Log.setAvatarScaleTo, LogType.Log, newScale.ToString());
            }
        }

        /// <summary>
        /// Function for all the actions in the tool menu. Use this instead of calling
        /// button functions directly.
        /// </summary>
        void DoAction(GameObject avatar, ToolMenuActions action)
        {
            if(!SelectedAvatar) //Shouldn't be possible with disable group
            {
                Log(Strings.Log.nothingSelected, LogType.Warning);
                return;
            }

            //Record Undo
            Undo.RegisterFullObjectHierarchyUndo(SelectedAvatar, "Tools menu: " + action.ToString());
            if(SelectedAvatar.gameObject.scene.name == null) //In case it's a prefab instance, which it probably is
                PrefabUtility.RecordPrefabInstancePropertyModifications(SelectedAvatar);

            switch(action)
            {
                case ToolMenuActions.RemoveColliders:
                    DestroyAllComponentsOfType(SelectedAvatar, typeof(Collider), false, false);
                    break;
                case ToolMenuActions.RemoveDynamicBoneColliders:
#if PUMKIN_DBONES || PUMKIN_OLD_DBONES
                    DestroyAllComponentsOfType(SelectedAvatar, typeof(DynamicBoneCollider), false, false);
                    CleanupDynamicBonesColliderArraySizes();

#endif
                    break;
                case ToolMenuActions.RemoveDynamicBones:
#if PUMKIN_DBONES || PUMKIN_OLD_DBONES
                    DestroyAllComponentsOfType(SelectedAvatar, typeof(DynamicBone), false, false);
#endif
                    break;
                case ToolMenuActions.ResetPose:
                    ResetPose(SelectedAvatar);
                    break;
                case ToolMenuActions.RevertBlendshapes:
                    if(EditorApplication.isPlaying)
                        ResetBlendshapes(SelectedAvatar, false);
                    else
                        ResetBlendshapes(SelectedAvatar, true);
                    break;
#if VRC_SDK_VRCSDK2 || (VRC_SDK_VRCSDK3 && !UDON)
                case ToolMenuActions.FillVisemes:
                    FillVisemes(SelectedAvatar);
                    break;
                case ToolMenuActions.EditViewpoint:
                    BeginEdittingViewpoint(SelectedAvatar);
                    break;
#endif
                case ToolMenuActions.ZeroBlendshapes:
                    ResetBlendshapes(SelectedAvatar, false);
                    break;
                case ToolMenuActions.SetTPose:
                    PumkinsPoseEditor.SetTPoseHardcoded(SelectedAvatar);
                    break;
                case ToolMenuActions.RemoveEmptyGameObjects:
                    DestroyEmptyGameObjects(SelectedAvatar);
                    break;
                case ToolMenuActions.RemoveParticleSystems:
                    DestroyAllComponentsOfType(SelectedAvatar, typeof(ParticleSystemRenderer), false, false);
                    DestroyAllComponentsOfType(SelectedAvatar, typeof(ParticleSystem), false, false);
                    break;
                case ToolMenuActions.RemoveRigidBodies:
                    DestroyAllComponentsOfType(SelectedAvatar, typeof(Rigidbody), false, false);
                    break;
                case ToolMenuActions.RemoveTrailRenderers:
                    DestroyAllComponentsOfType(SelectedAvatar, typeof(TrailRenderer), false, false);
                    break;
                case ToolMenuActions.RemoveMeshRenderers:
                    DestroyAllComponentsOfType(SelectedAvatar, typeof(MeshFilter), false, false);
                    DestroyAllComponentsOfType(SelectedAvatar, typeof(MeshRenderer), false, false);
                    break;
                case ToolMenuActions.RemoveLights:
                    DestroyAllComponentsOfType(SelectedAvatar, typeof(Light), false, false);
                    break;
                case ToolMenuActions.RemoveAnimatorsInChildren:
                    DestroyAllComponentsOfType(SelectedAvatar, typeof(Animator), true, false);
                    break;
                case ToolMenuActions.RemoveAudioSources:
#if VRC_SDK_VRCSDK2 || (VRC_SDK_VRCSDK3 && !UDON)
                    DestroyAllComponentsOfType(SelectedAvatar, typeof(VRC_SpatialAudioSource), false, false);
#endif
                    DestroyAllComponentsOfType(SelectedAvatar, typeof(AudioSource), false, false);
                    break;
                case ToolMenuActions.RemoveJoints:
                    DestroyAllComponentsOfType(SelectedAvatar, typeof(Joint), false, false);
                    break;
                case ToolMenuActions.EditScale:
                    BeginScalingAvatar(SelectedAvatar);
                    break;
                case ToolMenuActions.RevertScale:
                    RevertScale(SelectedAvatar);
                    break;
#if VRC_SDK_VRCSDK2 || (VRC_SDK_VRCSDK3 && !UDON)
                case ToolMenuActions.RemoveIKFollowers:
                    DestroyAllComponentsOfType(SelectedAvatar, typeof(VRC_IKFollower), false, false);
                    break;
#endif
                case ToolMenuActions.RemoveMissingScripts:
                    DestroyMissingScripts(SelectedAvatar);
                    break;
                case ToolMenuActions.RemoveAimConstraint:
                    DestroyAllComponentsOfType(SelectedAvatar, typeof(AimConstraint), false, false);
                    break;
                case ToolMenuActions.RemoveLookAtConstraint:
                    DestroyAllComponentsOfType(SelectedAvatar, typeof(LookAtConstraint), false, false);
                    break;
                case ToolMenuActions.RemoveParentConstraint:
                    DestroyAllComponentsOfType(SelectedAvatar, typeof(ParentConstraint), false, false);
                    break;
                case ToolMenuActions.RemovePositionConstraint:
                    DestroyAllComponentsOfType(SelectedAvatar, typeof(PositionConstraint), false, false);
                    break;
                case ToolMenuActions.RemoveRotationConstraint:
                    DestroyAllComponentsOfType(SelectedAvatar, typeof(RotationConstraint), false, false);
                    break;
                case ToolMenuActions.RemoveScaleConstraint:
                    DestroyAllComponentsOfType(SelectedAvatar, typeof(ScaleConstraint), false, false);
                    break;
                case ToolMenuActions.FixDynamicBoneScripts:
                    FixDynamicBoneScriptsInPrefab(SelectedAvatar);
                    break;
                case ToolMenuActions.ResetBoundingBoxes:
                    Helpers.ResetBoundingBoxes(SelectedAvatar);
                    break;
#if VRC_SDK_VRCSDK3 && !UDON
                case ToolMenuActions.FillEyeBones:
                    FillEyeBones(SelectedAvatar);
                    break;
#endif
                default:
                    break;
            }

            avatarInfo = PumkinsAvatarInfo.GetInfo(SelectedAvatar, out _avatarInfoString);

            EditorUtility.SetDirty(SelectedAvatar);
            if(!EditorApplication.isPlaying)
                EditorSceneManager.MarkSceneDirty(SelectedAvatar.scene);
        }

        private static void CleanupDynamicBonesColliderArraySizes()
        {
#if PUMKIN_DBONES || PUMKIN_OLD_DBONES
            var dbones = SelectedAvatar.GetComponentsInChildren<DynamicBone>(true);
            if(dbones != null && dbones.Length > 0)
            {
                SerializedObject so = new SerializedObject(dbones);
                if(so != null)
                {
                    var prop = so.FindProperty("m_Colliders");
                    if(prop != null)
                    {
                        prop.arraySize = 0;
                        so.ApplyModifiedProperties();   //Sets count of colliders in array to 0 so the safety system ignores them
                    }
                }
            }
#endif
        }

        /// <summary>
        /// Sets the enabled state on all dynamic bones on the avatar and returns affected bones
        /// </summary>
        /// <param name="enabled">Enabled state for dynamic bones</param>
        /// <param name="dBonesToIgnore">Dynamic Bones to ignore</param>
        /// <returns>Dynamic Bones that were disabled before we did anything</returns>
#if PUMKIN_DBONES || PUMKIN_OLD_DBONES
        static void SetDynamicBonesEnabledState(GameObject avatar, bool enabled, List<DynamicBone> dBonesToIgnore = null)
        {
            if(!avatar)
                return;

            foreach(var bone in avatar.GetComponentsInChildren<DynamicBone>(true))
                if(dBonesToIgnore == null || !dBonesToIgnore.Contains(bone))
                    bone.enabled = enabled;
        }
#else
        static void SetDynamicBonesEnabledState(GameObject avatar, bool enabled)
        {
            return;
        }
#endif
        /// <summary>
        /// Toggles the enbaled state of all Dynamic Bones on the avatar and returns affected bones
        /// </summary>
        /// <param name="enabledState">Bool to use as toggle state</param>
        /// <param name="dBonesToIgnore">Dynamic Bones to ignore</param>
        /// <returns>Dynamic Bones that have been enabled or disabled. Used to ignore bones that were disabled before we toggled off</returns>
#if PUMKIN_DBONES || PUMKIN_OLD_DBONES
        static void ToggleDynamicBonesEnabledState(GameObject avatar, ref bool enabledState, ref List<DynamicBone> dBonesToIgnore)
        {

            if(!enabledState)
            {
                dBonesToIgnore = new List<DynamicBone>();
                var bones = avatar.GetComponentsInChildren<DynamicBone>(true);
                foreach(var b in bones)
                    if(!b.enabled)
                        dBonesToIgnore.Add(b);
            }
            SetDynamicBonesEnabledState(avatar, enabledState, dBonesToIgnore);
            enabledState = !enabledState;

        }
#else
        static void ToggleDynamicBonesEnabledState(GameObject avatar, ref bool enabledState, ref List<object> dBonesToIgnore)
        {
            return;
        }
#endif

        /// <summary>
        /// Doesn't seem to work. Need to investigate
        /// </summary>
        /// <param name="avatar"></param>
        static void RefreshDynamicBoneTransforms(GameObject avatar)
        {
#if PUMKIN_DBONES || PUMKIN_OLD_DBONES
            if(!avatar)
                return;

            var bones = avatar.GetComponentsInChildren<DynamicBone>(true);
            foreach(var b in bones)
            {
                bool enabled = b.enabled;
                b.enabled = false;
                b.CallPrivate("InitTransforms");
                b.enabled = enabled;
            }
#endif
        }

#if VRC_SDK_VRCSDK2 || (VRC_SDK_VRCSDK3 && !UDON)
        /// <summary>
        /// Refreshes the VRC SDK window
        /// </summary>
        private void RefreshSDK()
        {
            VRCSdkControlPanel.window?.Reset();
        }
#endif

        /// <summary>
        /// Reverts avatar scale to prefab values and moves the viewpoint to compensate for the change if avatar a descriptor is present
        /// </summary>
        private void RevertScale(GameObject avatar)
        {
            if(!avatar)
                return;

            GameObject pref = PrefabUtility.GetCorrespondingObjectFromSource(avatar);
            Vector3 newScale = pref != null ? pref.transform.localScale : Vector3.one;

#if VRC_SDK_VRCSDK2 || (VRC_SDK_VRCSDK3 && !UDON)
            var desc = avatar.GetComponent<VRC_AvatarDescriptor>();

            if(desc)
                SetAvatarScaleAndMoveViewpoint(desc, newScale.y);
#endif

            avatar.transform.localScale = newScale;
        }

        /// <summary>
        /// Begin scaling Avatar.
        /// Used to uniformily scale an avatar as well as it's viewpoint position
        /// </summary>
        private void BeginScalingAvatar(GameObject avatar)
        {
            if(DrawingHandlesGUI || !avatar)
                return;

#if VRC_SDK_VRCSDK2 || (VRC_SDK_VRCSDK3 && !UDON)
            _tempAvatarDescriptor = avatar.GetComponent<VRC_AvatarDescriptor>();
            if(!_tempAvatarDescriptor)
            {
                _tempAvatarDescriptor = avatar.AddComponent<VRC_AvatarDescriptor>();
                _tempAvatarDescriptorWasAdded = true;
            }
            else
            {
                _tempAvatarDescriptorWasAdded = false;
            }

            _viewPosOld = _tempAvatarDescriptor.ViewPosition;
            _viewPosTemp = _viewPosOld + SelectedAvatar.transform.position;
#endif
            _avatarScaleOld = avatar.transform.localScale;
            _avatarScaleTemp = _avatarScaleOld.z;

#if VRC_SDK_VRCSDK2 || (VRC_SDK_VRCSDK3 && !UDON)
            if(!_scaleViewpointDummy)
            {
                var g = GameObject.Find(VIEWPOINT_DUMMY_NAME);
                if(g)
                    _scaleViewpointDummy = g.transform;
                else
                {
                    _scaleViewpointDummy = new GameObject(VIEWPOINT_DUMMY_NAME).transform;
                    _scaleViewpointDummy.gameObject.hideFlags = HideFlags.HideAndDontSave;
                }
            }

            _scaleViewpointDummy.position = _viewPosTemp;
            _scaleViewpointDummy.parent = SelectedAvatar.transform;
#endif

            _editingScale = true;
            _tempToolOld = Tools.current;
            Tools.current = Tool.None;
            Selection.activeGameObject = SelectedAvatar;

            SetupScaleRuler();
        }

        void SetupScaleRuler()
        {
            if(_scaleRuler != null)
                Helpers.DestroyAppropriate(_scaleRuler);

            if(!ScaleRulerPrefab)
                return;

            _scaleRuler = Instantiate(ScaleRulerPrefab, SelectedAvatar.transform.position, ScaleRulerPrefab.transform.rotation);
            _scaleRuler.name = SCALE_RULER_DUMMY_NAME;
            _scaleRuler.hideFlags = HideFlags.HideAndDontSave;
        }

        /// <summary>
        /// Ends scaling the avatar
        /// </summary>
        /// <param name="cancelled">If canceled returnt to old scale and viewpoint</param>
        private void EndScalingAvatar(GameObject avatar, bool cancelled)
        {
            try
            {
                if(avatar == null)
                {
                    _editingScale = false;
                }
                else
                {
#if VRC_SDK_VRCSDK2 || (VRC_SDK_VRCSDK3 && !UDON)
                    if(_tempAvatarDescriptor == null)
                    {
                        Log(Strings.Log.descriptorIsNull, LogType.Error);
                        return;
                    }
#endif

                    _editingScale = false;
                    Tools.current = _tempToolOld;
                    if(!cancelled)
                    {
                        if(editingScaleMovesViewpoint)
                        {
#if VRC_SDK_VRCSDK2 || (VRC_SDK_VRCSDK3 && !UDON)
                            SetViewpoint(_tempAvatarDescriptor, _viewPosTemp);
                            Log(Strings.Log.setAvatarScaleAndViewpointTo, LogType.Log, avatar.transform.localScale.z.ToString(), _tempAvatarDescriptor.ViewPosition.ToString());
#endif
                        }
                        else
                        {
                            Log(Strings.Log.setAvatarScaleTo, LogType.Log, avatar.transform.localScale.z.ToString());
                        }
                    }
                    else
                    {
#if VRC_SDK_VRCSDK2 || (VRC_SDK_VRCSDK3 && !UDON)
                        if(_tempAvatarDescriptorWasAdded)
                            Helpers.DestroyAvatarDescriptorAndPipeline(SelectedAvatar);
                        else
                            _tempAvatarDescriptor.ViewPosition = _viewPosOld;
#endif
                        SelectedAvatar.transform.localScale = _avatarScaleOld;
                        Log(Strings.Log.canceledScaleChanges);
                    }
                }
#if VRC_SDK_VRCSDK2 || (VRC_SDK_VRCSDK3 && !UDON)
                _tempAvatarDescriptor = null;
                _tempAvatarDescriptorWasAdded = false;
#endif
            }
            finally
            {
                if(_scaleViewpointDummy)
                    Helpers.DestroyAppropriate(_scaleViewpointDummy.gameObject);
                if(_scaleRuler)
                    Helpers.DestroyAppropriate(_scaleRuler);

                if(SerializedScript != null)
                    SerializedScript.ApplyModifiedPropertiesWithoutUndo();
            }
        }

#if VRC_SDK_VRCSDK2 || (VRC_SDK_VRCSDK3 && !UDON)
        /// <summary>
        /// Begin Editing Viewposition.
        /// Used to move the viewpoint using unit's transform gizmo
        /// </summary>
        private void BeginEdittingViewpoint(GameObject avatar)
        {
            if(_editingView || _editingScale || !avatar)
                return;

            _tempAvatarDescriptor = avatar.GetComponent<VRC_AvatarDescriptor>();
            if(!_tempAvatarDescriptor)
            {
                _tempAvatarDescriptor = avatar.AddComponent<VRC_AvatarDescriptor>();
                _tempAvatarDescriptorWasAdded = true;
            }
            else
            {
                _tempAvatarDescriptorWasAdded = false;
            }

            _viewPosOld = _tempAvatarDescriptor.ViewPosition;

            if(_tempAvatarDescriptor.ViewPosition == DEFAULT_VIEWPOINT)
                _viewPosTemp = Helpers.GetViewpointAtEyeLevel(SelectedAvatar.GetComponent<Animator>()) + avatar.transform.root.position;
            else
                _viewPosTemp = _tempAvatarDescriptor.ViewPosition + avatar.transform.root.position;

            _editingView = true;
            _tempToolOld = Tools.current;
            Tools.current = Tool.None;
            Selection.activeGameObject = SelectedAvatar;
        }

        /// <summary>
        /// Ends editing Viewposition
        /// </summary>
        /// <param name="cancelled">If cancelled revert viewposition to old value, if not leave it</param>
        private void EndEditingViewpoint(GameObject avatar, bool cancelled)
        {
            if(avatar == null)
            {
                _editingView = false;
            }
            else
            {
                if(_tempAvatarDescriptor == null)
                {
                    Log(Strings.Log.descriptorIsNull, LogType.Error);
                    return;
                }

                _editingView = false;
                Tools.current = _tempToolOld;
                if(!cancelled)
                {
                    SetViewpoint(_tempAvatarDescriptor, _viewPosTemp);
                    Log(Strings.Log.viewpointApplied, LogType.Log, _tempAvatarDescriptor.ViewPosition.ToString());
                }
                else
                {
                    if(_tempAvatarDescriptorWasAdded)
                        Helpers.DestroyAvatarDescriptorAndPipeline(SelectedAvatar);
                    else
                        _tempAvatarDescriptor.ViewPosition = _viewPosOld;

                    Log(Strings.Log.viewpointCancelled, LogType.Log);
                }
            }
            _tempAvatarDescriptor = null;
            _tempAvatarDescriptorWasAdded = false;
        }


        /// <summary>
        /// Sets the descriptor's viewpoint to a vector and rounds it's value to 3 decimals
        /// </summary>
        void SetViewpoint(VRC_AvatarDescriptor desc, Vector3 position)
        {
            if(!desc)
            {
                Log("Avatar has no Avatar Descriptor. Ignoring", LogType.Warning);
                return;
            }

            desc.ViewPosition = Helpers.RoundVectorValues(position - desc.gameObject.transform.position, 3);
        }

        /// <summary>
        /// Fill viseme tree on avatar descriptor or assign jaw flap bone if missing
        /// </summary>
        private void FillVisemes(GameObject avatar)
        {
            string log = Strings.Log.tryFillVisemes + " - ";
            string logFormat = avatar.name;

            string[] visemes =
            {
                    "vrc.v_sil",
                    "vrc.v_pp",
                    "vrc.v_ff",
                    "vrc.v_th",
                    "vrc.v_dd",
                    "vrc.v_kk",
                    "vrc.v_ch",
                    "vrc.v_ss",
                    "vrc.v_nn",
                    "vrc.v_rr",
                    "vrc.v_aa",
                    "vrc.v_e",
                    "vrc.v_ih",
                    "vrc.v_oh",
                    "vrc.v_ou",
                };

            var d = avatar.GetComponent<VRC_AvatarDescriptor>();
            if(!d)
            {
                d = avatar.AddComponent<VRC_AvatarDescriptor>();
            }
            if(d.VisemeBlendShapes == null || d.VisemeBlendShapes.Length != visemes.Length)
            {
                d.VisemeBlendShapes = new string[visemes.Length];
            }

            var renders = avatar.GetComponentsInChildren<SkinnedMeshRenderer>(true);

            bool foundShape = false;

            for(int i = 0; !foundShape && i < renders.Length; i++)
            {
                for(int j = 0; !foundShape && j < renders[i].sharedMesh.blendShapeCount; j++)
                {
                    for(int k = 0; k < visemes.Length; k++)
                    {
                        string s = "-none-";
                        int index = renders[i].sharedMesh.GetBlendShapeIndex(visemes[k]);

                        if(index != -1)
                        {
                            d.VisemeSkinnedMesh = renders[i];
                            foundShape = true;

                            s = visemes[k];
                        }

                        d.VisemeBlendShapes[k] = s;
                    }
                }
            }

            if(d.VisemeSkinnedMesh == null)
            {
                log += Strings.Log.noSkinnedMeshFound;
                Log(log, LogType.Error, logFormat);
            }
            else
            {
                if(foundShape)
                {
                    d.lipSync = VRC_AvatarDescriptor.LipSyncStyle.VisemeBlendShape;
                    log += Strings.Log.success;
                    Log(log, LogType.Log, logFormat);
                }
                else
                {
                    var anim = avatar.GetComponent<Animator>();
                    if(anim && anim.isHuman)
                    {
                        var jaw = anim.GetBoneTransform(HumanBodyBones.Jaw);
                        if(jaw)
                        {
                            d.lipSync = VRC_AvatarDescriptor.LipSyncStyle.JawFlapBone;
                            d.lipSyncJawBone = jaw;
                        }
                        else
                        {
                            d.lipSync = VRC_AvatarDescriptor.LipSyncStyle.Default;
                        }
                    }
                    else
                    {
                        d.lipSync = VRC_AvatarDescriptor.LipSyncStyle.Default;
                    }
                    log += Strings.Log.meshHasNoVisemes;
                    Log(log, LogType.Warning, logFormat);
                }
            }
        }
#endif

        /// <summary>
        /// Sets the Probe Anchor of all Skinned Mesh Renderers to transform by path
        /// </summary>
        private void SetSkinnedMeshRendererAnchor(GameObject avatar, string anchorPath)
        {
            Transform anchor = avatar.transform.Find(anchorPath);
            if(!anchor)
            {
                Log(Strings.Log.transformNotFound, LogType.Warning, anchorPath);
                return;
            }

            var renders = avatar.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            foreach(var render in renders)
            {
                if(render)
                {
                    render.probeAnchor = anchor;
                    Log(Strings.Log.setProbeAnchorTo, LogType.Log, render.name, anchor.name);
                }
            }
        }

        /// <summary>
        /// Sets the Probe Anchor of all Mesh Renderers to transform by path
        /// </summary>
        private void SetMeshRendererAnchor(GameObject avatar, string anchorPath)
        {
            Transform anchor = avatar.transform.Find(anchorPath);
            if(!anchor)
            {
                Log(Strings.Log.transformNotFound, LogType.Warning, anchorPath);
                return;
            }

            var renders = avatar.GetComponentsInChildren<MeshRenderer>(true);
            foreach(var render in renders)
            {
                if(render)
                {
                    render.probeAnchor = anchor;
                    Log(Strings.Log.setProbeAnchorTo, LogType.Log, render.name, anchor.name);
                }
            }
        }

#endregion

#region Copy Functions

        /// <summary>
        /// Copies Components and Values from one object to another.
        /// </summary>
        void CopyComponents(GameObject objFrom, GameObject objTo)
        {
            string log = "";
            //Cancel Checks
            if(objFrom == objTo)
            {
                log += Strings.Log.cantCopyToSelf;
                Log(log, LogType.Warning);
                return;
            }

#if VRC_SDK_VRCSDK2 || (VRC_SDK_VRCSDK3 && !UDON)
            VRC_AvatarDescriptor desc;

            if(bCopier_descriptor_copy && CopierTabs.ComponentIsInSelectedTab<VRC_AvatarDescriptor>(_copier_selectedTab))
            {
                CopyAvatarDescriptor(objFrom, objTo, true);

                if(bCopier_descriptor_copyAvatarScale)
                {
                    desc = objTo.GetComponentInChildren<VRC_AvatarDescriptor>();
                    if(desc)
                    {
                        if(!(bCopier_descriptor_copy && bCopier_descriptor_copyViewpoint))
                            SetAvatarScaleAndMoveViewpoint(desc, objFrom.transform.localScale.z);
                        objTo.transform.localScale = new Vector3(objFrom.transform.localScale.x, objFrom.transform.localScale.y, objFrom.transform.localScale.z);
                    }
                    else
                    {
                        objTo.transform.localScale = objFrom.transform.localScale;
                    }
                }
            }
#else
            if(bCopier_descriptor_copy && bCopier_descriptor_copyAvatarScale)
                objTo.transform.localScale = objFrom.transform.localScale;
#endif

            if(bCopier_particleSystems_copy && CopierTabs.ComponentIsInSelectedTab<ParticleSystem>(_copier_selectedTab))
            {
                CopyAllParticleSystems(objFrom, objTo, bCopier_particleSystems_createObjects, true);
            }
            if(bCopier_colliders_copy && CopierTabs.ComponentIsInSelectedTab<Collider>(_copier_selectedTab))
            {
                if(bCopier_colliders_removeOld)
                    DestroyAllComponentsOfType(objTo, typeof(Collider), false, true);
                CopyAllColliders(objFrom, objTo, bCopier_colliders_createObjects, true);
            }
            if(bCopier_rigidBodies_copy && CopierTabs.ComponentIsInSelectedTab<Rigidbody>(_copier_selectedTab))
            {
                CopyAllRigidBodies(objFrom, objTo, bCopier_rigidBodies_createObjects, true);
            }
            if(bCopier_trailRenderers_copy && CopierTabs.ComponentIsInSelectedTab<TrailRenderer>(_copier_selectedTab))
            {
                CopyAllTrailRenderers(objFrom, objTo, bCopier_trailRenderers_createObjects, true);
            }
            if(bCopier_meshRenderers_copy && CopierTabs.ComponentIsInSelectedTab<MeshRenderer>(_copier_selectedTab))
            {
                CopyAllMeshRenderers(objFrom, objTo, bCopier_meshRenderers_createObjects, true);
            }
            if(bCopier_lights_copy && CopierTabs.ComponentIsInSelectedTab<Light>(_copier_selectedTab))
            {
                CopyAllLights(objFrom, objTo, bCopier_lights_createObjects, true);
            }
            if(bCopier_skinMeshRender_copy && CopierTabs.ComponentIsInSelectedTab<SkinnedMeshRenderer>(_copier_selectedTab))
            {
                CopyAllSkinnedMeshRenderersSettings(objFrom, objTo, true);
            }
            if(bCopier_animators_copy && CopierTabs.ComponentIsInSelectedTab<Animator>(_copier_selectedTab))
            {
                CopyAllAnimators(objFrom, objTo, bCopier_animators_createObjects, bCopier_animators_copyMainAnimator, true);
            }
            if(bCopier_audioSources_copy && CopierTabs.ComponentIsInSelectedTab<AudioSource>(_copier_selectedTab))
            {
                CopyAllAudioSources(objFrom, objTo, bCopier_audioSources_createObjects, true);
            }
#if VRC_SDK_VRCSDK2 || (VRC_SDK_VRCSDK3 && !UDON)
            if(bCopier_other_copy && CopierTabs.ComponentIsInSelectedTab("other", _copier_selectedTab))
            {
                CopyAllIKFollowers(objFrom, objTo, bCopier_other_createGameObjects, true);
            }
#endif
            if(DynamicBonesExist)
            {
                if(bCopier_dynamicBones_copyColliders && CopierTabs.ComponentIsInSelectedTab("dynamicbonecollider", _copier_selectedTab))
                {
#if PUMKIN_DBONES || PUMKIN_OLD_DBONES
                    if(bCopier_dynamicBones_removeOldColliders)
                        DestroyAllComponentsOfType(objTo, typeof(DynamicBoneCollider), false, true);
                    CopyAllDynamicBoneColliders(objFrom, objTo, bCopier_dynamicBones_createObjectsColliders, true);
#endif
                }
                if(bCopier_dynamicBones_copy && CopierTabs.ComponentIsInSelectedTab("dynamicbone", _copier_selectedTab))
                {
#if PUMKIN_DBONES || PUMKIN_OLD_DBONES
                    if(bCopier_dynamicBones_removeOldBones)
                        DestroyAllComponentsOfType(objTo, typeof(DynamicBone), false, true);
                    if(bCopier_dynamicBones_copySettings || bCopier_dynamicBones_createMissing)
                        CopyAllDynamicBonesNew(objFrom, objTo, bCopier_dynamicBones_createMissing, true);
#endif
                }
            }
            else if(bCopier_dynamicBones_copy)
            {
                Log(Strings.Warning.noDBonesOrMissingScriptDefine, LogType.Error);
            }

            if(bCopier_aimConstraint_copy && CopierTabs.ComponentIsInSelectedTab<AimConstraint>(_copier_selectedTab))
            {
                CopyAllAimConstraints(objFrom, objTo, bCopier_aimConstraint_createObjects, true);
            }
            if(bCopier_lookAtConstraint_copy && CopierTabs.ComponentIsInSelectedTab<LookAtConstraint>(_copier_selectedTab))
            {
                CopyAllLookAtConstraints(objFrom, objTo, bCopier_aimConstraint_createObjects, true);
            }
            if(bCopier_parentConstraint_copy && CopierTabs.ComponentIsInSelectedTab<ParentConstraint>(_copier_selectedTab))
            {
                CopyAllParentConstraints(objFrom, objTo, bCopier_aimConstraint_createObjects, true);
            }
            if(bCopier_positionConstraint_copy && CopierTabs.ComponentIsInSelectedTab<PositionConstraint>(_copier_selectedTab))
            {
                CopyAllPositionConstraints(objFrom, objTo, bCopier_aimConstraint_createObjects, true);
            }
            if(bCopier_rotationConstraint_copy && CopierTabs.ComponentIsInSelectedTab<RotationConstraint>(_copier_selectedTab))
            {
                CopyAllRotationConstraints(objFrom, objTo, bCopier_aimConstraint_createObjects, true);
            }
            if(bCopier_scaleConstraint_copy && CopierTabs.ComponentIsInSelectedTab<ScaleConstraint>(_copier_selectedTab))
            {
                CopyAllScaleConstraints(objFrom, objTo, bCopier_aimConstraint_createObjects, true);
            }
            if(bCopier_joints_copy && CopierTabs.ComponentIsInSelectedTab<Joint>(_copier_selectedTab))
            {
                if(bCopier_joints_removeOld)
                    DestroyAllComponentsOfType(objTo, typeof(Joint), false, true);
                CopyAllJoints(objFrom, objTo, bCopier_joints_createObjects, true);
            }

            if(bCopier_transforms_copy && CopierTabs.ComponentIsInSelectedTab<Transform>(_copier_selectedTab))
            {
                CopyAllTransforms(objFrom, objTo, true);
            }
        }

        void CopyAllSpringVRMSpringBones(GameObject from, GameObject to, bool createGameObjects, bool useIgnoreList)
        {
            Type type = Type.GetType("VRM.VRMSpringBone");

            if(from == null || to == null || type == null)
                return;

            var vrmFromArr = from.GetComponentsInChildren(type, true);
            if(vrmFromArr == null || vrmFromArr.Length == 0)
                return;


            for(int i = 0; i < vrmFromArr.Length; i++)
            {
                var vrmFrom = vrmFromArr[i];
                var tTo = Helpers.FindTransformInAnotherHierarchy(vrmFrom.transform, to.transform, createGameObjects);
                if((!tTo) || (useIgnoreList && Helpers.ShouldIgnoreObject(vrmFrom.transform, _copierIgnoreArray, bCopier_ignoreArray_includeChildren)))
                    continue;

                string log = String.Format(Strings.Log.copyAttempt, type.Name, vrmFrom.gameObject, tTo.gameObject);

                if(!tTo.GetComponent(type))
                {
                    ComponentUtility.CopyComponent(vrmFrom);
                    ComponentUtility.PasteComponentAsNew(tTo.gameObject);
                    Log(Strings.Log.copyAttempt + " - " + Strings.Log.success, LogType.Log);
                }
                else
                {
                    Log(Strings.Log.copyAttempt + " - " + Strings.Log.failedAlreadyHas, LogType.Log);
                }
            }
        }

#if VRC_SDK_VRCSDK2 || (VRC_SDK_VRCSDK3 && !UDON)
        /// <summary>
        /// Copies all VRC_IKFollowers on an object and it's children.
        /// </summary>
        /// <param name="createGameObjects">Whether to create missing objects</param>
        private void CopyAllIKFollowers(GameObject from, GameObject to, bool createGameObjects, bool useIgnoreList)
        {
            if(from == null || to == null)
                return;

            var ikFromArr = from.GetComponentsInChildren<VRC_IKFollower>(true);
            if(ikFromArr == null || ikFromArr.Length == 0)
                return;

            string type = typeof(VRC_IKFollower).Name;

            for(int i = 0; i < ikFromArr.Length; i++)
            {
                var ikFrom = ikFromArr[i];
                var tTo = Helpers.FindTransformInAnotherHierarchy(ikFrom.transform, to.transform, createGameObjects);
                if((!tTo) || (useIgnoreList && Helpers.ShouldIgnoreObject(ikFrom.transform, _copierIgnoreArray, bCopier_ignoreArray_includeChildren)))
                    continue;

                string log = String.Format(Strings.Log.copyAttempt, type, ikFrom.gameObject, tTo.gameObject);

                if(!tTo.GetComponent<VRC_IKFollower>())
                {
                    ComponentUtility.CopyComponent(ikFrom);
                    ComponentUtility.PasteComponentAsNew(tTo.gameObject);
                    Log(Strings.Log.copyAttempt + " - " + Strings.Log.success, LogType.Log);
                }
                else
                {
                    Log(Strings.Log.copyAttempt + " - " + Strings.Log.failedAlreadyHas, LogType.Log);
                }
            }
        }
#endif

        /// <summary>
        /// Copies all audio sources on object and it's children.
        /// </summary>
        /// <param name="createGameObjects">Whether to create missing objects</param>
        void CopyAllAudioSources(GameObject from, GameObject to, bool createGameObjects, bool useIgnoreList)
        {
            if(from == null || to == null)
                return;

            var audioFromArr = from.GetComponentsInChildren<AudioSource>(true);
            string typeName = typeof(AudioSource).Name;

            for(int i = 0; i < audioFromArr.Length; i++)
            {
                var audioFrom = audioFromArr[i];
#if VRC_SDK_VRCSDK2 || (VRC_SDK_VRCSDK3 && !UDON)
                var spatialAudioFrom = audioFromArr[i].GetComponent<VRC_SpatialAudioSource>();
#endif
                var transTo = Helpers.FindTransformInAnotherHierarchy(audioFrom.transform, to.transform, createGameObjects);

                if((!transTo) || (useIgnoreList && Helpers.ShouldIgnoreObject(audioFrom.transform, _copierIgnoreArray, bCopier_ignoreArray_includeChildren)))
                    continue;

                var audioToObj = transTo.gameObject;

                string log = String.Format(Strings.Log.copyAttempt, typeName, audioFrom.gameObject, transTo.gameObject);

                if(audioFrom != null)
                {
                    var audioTo = audioToObj.GetComponent<AudioSource>();
#if VRC_SDK_VRCSDK2 || (VRC_SDK_VRCSDK3 && !UDON)
                    var spatialAudioTo = audioToObj.GetComponent<VRC_SpatialAudioSource>();
#endif

                    if(audioTo == null && bCopier_audioSources_createMissing)
                    {
                        audioTo = audioToObj.AddComponent<AudioSource>();
#if VRC_SDK_VRCSDK2 || (VRC_SDK_VRCSDK3 && !UDON)
                        if(spatialAudioFrom != null)
                            spatialAudioTo = audioToObj.AddComponent<VRC_SpatialAudioSource>();
#endif
                    }

                    if((audioTo != null && bCopier_audioSources_copySettings) || bCopier_audioSources_createMissing)
                    {
                        ComponentUtility.CopyComponent(audioFrom);
                        ComponentUtility.PasteComponentValues(audioTo);
#if VRC_SDK_VRCSDK2 || (VRC_SDK_VRCSDK3 && !UDON)
                        if(spatialAudioFrom != null)
                        {
                            ComponentUtility.CopyComponent(spatialAudioFrom);
                            ComponentUtility.PasteComponentValues(spatialAudioTo);
                        }
#endif
                        Log(log + " - " + Strings.Log.success);
                    }
                }
                else
                {
                    Log(log + " " + Strings.Log.failedDoesntHave, LogType.Warning, audioFrom.gameObject.name.ToString(), audioFrom.GetType().ToString());
                }
            }
        }

        /// <summary>
        /// Copies all Animators from one object and it's children to another.
        /// </summary>
        /// <param name="createGameObjects">Whether to create missing GameObjects</param>
        /// <param name="copyRootAnimator">Whether to copy the Animator on the root object. You don't usually want to.</param>
        void CopyAllAnimators(GameObject from, GameObject to, bool createGameObjects, bool copyRootAnimator, bool useIgnoreList)
        {
            if(from == null || to == null)
                return;

            var aFromArr = from.GetComponentsInChildren<Animator>(true);

            for(int i = 0; i < aFromArr.Length; i++)
            {
                if(!copyRootAnimator && aFromArr[i].transform.parent == null)
                    continue;

                string log = Strings.Log.copyAttempt;
                string type = typeof(Animator).Name;

                var aFrom = aFromArr[i];
                var tTo = Helpers.FindTransformInAnotherHierarchy(aFrom.transform, to.transform, createGameObjects);

                if((!tTo) || (useIgnoreList && Helpers.ShouldIgnoreObject(aFrom.transform, _copierIgnoreArray, bCopier_ignoreArray_includeChildren)))
                    continue;

                var aToObj = tTo.gameObject;

                if(aFrom != null)
                {
                    var lTo = aToObj.GetComponent<Animator>();

                    if(lTo == null && bCopier_animators_createMissing)
                    {
                        lTo = aToObj.AddComponent<Animator>();
                    }

                    if((lTo != null && bCopier_animators_copySettings) || bCopier_animators_createMissing)
                    {
                        ComponentUtility.CopyComponent(aFrom);
                        ComponentUtility.PasteComponentValues(lTo);
                        Log(log + " - " + Strings.Log.success, LogType.Log, type, tTo.gameObject.name, aFrom.gameObject.name);
                    }
                }
                else
                {
                    Log(log + " " + Strings.Log.failedDoesntHave, LogType.Warning, aFrom.gameObject.name.ToString(), aFrom.GetType().ToString());
                }
            }
        }

        /// <summary>
        /// Copies all lights in object and it's children to another object.
        /// </summary>
        /// <param name="createGameObjects">Whether to create missing game objects</param>
        void CopyAllLights(GameObject from, GameObject to, bool createGameObjects, bool useIgnoreList)
        {
            if(from == null || to == null)
                return;

            var lFromArr = from.GetComponentsInChildren<Light>(true);

            for(int i = 0; i < lFromArr.Length; i++)
            {
                string log = Strings.Log.copyAttempt;
                string type = typeof(Light).Name;

                var lFrom = lFromArr[i];
                var tTo = Helpers.FindTransformInAnotherHierarchy(lFrom.transform, to.transform, createGameObjects);

                if((!tTo) ||
                    (useIgnoreList && Helpers.ShouldIgnoreObject(lFrom.transform, _copierIgnoreArray, bCopier_ignoreArray_includeChildren)))
                    continue;

                var lToObj = tTo.gameObject;

                if(lFrom != null)
                {
                    var lTo = lToObj.GetComponent<Light>();

                    if(lTo == null && bCopier_lights_createMissing)
                    {
                        lTo = lToObj.AddComponent<Light>();
                    }

                    if((lTo != null && bCopier_lights_copySettings) || bCopier_lights_createMissing)
                    {
                        ComponentUtility.CopyComponent(lFrom);
                        ComponentUtility.PasteComponentValues(lTo);
                        Log(log + " - " + Strings.Log.success);
                    }
                }
                else
                {
                    Log(log + " " + Strings.Log.failedDoesntHave, LogType.Warning, lFrom.gameObject.name.ToString(), type);
                }
            }
        }

        /// <summary>
        /// Copies all MeshRenderers in object and it's children to another object.
        /// </summary>
        /// <param name="createGameObjects">Whether to create missing game objects</param>
        void CopyAllMeshRenderers(GameObject from, GameObject to, bool createGameObjects, bool useIgnoreList)
        {
            if(from == null || to == null)
                return;

            var mFromArr = from.GetComponentsInChildren<MeshRenderer>(true);
            string type = typeof(MeshRenderer).Name;

            for(int i = 0; i < mFromArr.Length; i++)
            {
                var rFrom = mFromArr[i];
                var tTo = Helpers.FindTransformInAnotherHierarchy(rFrom.transform, to.transform, createGameObjects);

                string log = string.Format(Strings.Log.copyAttempt, type, rFrom.gameObject.name, tTo.gameObject.name);

                if((!tTo) ||
                    (useIgnoreList && Helpers.ShouldIgnoreObject(rFrom.transform, _copierIgnoreArray, bCopier_ignoreArray_includeChildren)))
                    continue;

                var rToObj = tTo.gameObject;

                var fFrom = rFrom.GetComponent<MeshFilter>();

                if(fFrom != null)
                {
                    var rTo = rToObj.GetComponent<MeshRenderer>();
                    var fTo = rToObj.GetComponent<MeshFilter>();

                    if(rTo == null && bCopier_meshRenderers_createMissing)
                    {
                        rTo = Undo.AddComponent<MeshRenderer>(tTo.gameObject);
                        if(fTo == null)
                            fTo = Undo.AddComponent<MeshFilter>(tTo.gameObject);
                    }

                    if((rTo != null && bCopier_meshRenderers_copySettings) || bCopier_meshRenderers_createMissing)
                    {
                        ComponentUtility.CopyComponent(rFrom);
                        ComponentUtility.PasteComponentValues(rTo);

                        ComponentUtility.CopyComponent(fFrom);
                        ComponentUtility.PasteComponentValues(fTo);
                        Log(log + " - " + Strings.Log.success, LogType.Log);
                    }
                    else
                    {
                        Log(log += " - " + Strings.Log.failedHasNoIgnoring, LogType.Warning, rFrom.gameObject.name, type);
                    }
                }
            }
        }

#if VRC_SDK_VRCSDK2 || (VRC_SDK_VRCSDK3 && !UDON)
        void CopyAvatarDescriptor(GameObject from, GameObject to, bool useIgnoreList)
        {
            if(to == null || from == null)
                return;

            if(useIgnoreList && Helpers.ShouldIgnoreObject(from.transform, _copierIgnoreArray))
                return;

            var dFrom = from.GetComponent<VRC_AvatarDescriptor>();
            var pFrom = from.GetComponent<PipelineManager>();
            var dTo = to.GetComponent<VRC_AvatarDescriptor>();

            if(dFrom == null)
                return;
            if(dTo == null)
                dTo = Undo.AddComponent<VRC_AvatarDescriptor>(to);

            var pTo = to.GetComponent<PipelineManager>() ?? to.AddComponent<PipelineManager>();

            var sDescTo = new SerializedObject(dTo);
            var sDescFrom = new SerializedObject(dFrom);

            if(bCopier_descriptor_copyPipelineId)
            {
                pTo.blueprintId = pFrom.blueprintId;
                pTo.enabled = pFrom.enabled;
                pTo.completedSDKPipeline = true;

                EditorUtility.SetDirty(pTo);
                EditorSceneManager.MarkSceneDirty(pTo.gameObject.scene);
                EditorSceneManager.SaveScene(pTo.gameObject.scene);
            }

            var propNames = new List<string>();
            if(bCopier_descriptor_copyViewpoint)
                propNames.Add("ViewPosition");

            if(bCopier_descriptor_copySettings)
            {
                propNames.AddRange(new string[]
                {
                    //Shared
                    "Name", "Animations", "ScaleIPD", "lipSync", "VisemeSkinnedMesh", "MouthOpenBlendShapeName",
                    "VisemeBlendShapes", "portraitCameraPositionOffset", "portraitCameraRotationOffset", "lipSyncJawBone",
                    //SDK3
                    "enableEyeLook", "customizeAnimationLayers", "baseAnimationLayers",
                    "specialAnimationLayers", "lipSyncJawClosed", "lipSyncJawOpen", "AnimationPreset", "autoFootsteps", "autoLocomotion"
                });
            }

            if(bCopier_descriptor_copyEyeLookSettings)
            {
                propNames.Add("customEyeLookSettings");
            }

            if(bCopier_descriptor_copyAnimationOverrides) //SDK2 Only
            {
                propNames.AddRange(new string[]
                {
                    "CustomSittingAnims", "CustomStandingAnims",
                });
            }

            if(bCopier_descriptor_copyExpressions)
            {
                propNames.AddRange(new string[]
                {
                    "customExpressions", "expressionsMenu", "expressionParameters"
                });
            }

            foreach(var s in propNames)
            {
                var prop = sDescFrom.FindProperty(s);
                if(prop != null)
                    sDescTo.CopyFromSerializedProperty(prop);
            }

            var eyes = sDescTo.FindProperty("customEyeLookSettings");

            if(eyes != null)
            {
                SerializedProperty[] transLocalize =
                {
                    eyes.FindPropertyRelative("leftEye"),
                    eyes.FindPropertyRelative("rightEye"),
                    eyes.FindPropertyRelative("upperLeftEyelid"),
                    eyes.FindPropertyRelative("upperRightEyelid"),
                    eyes.FindPropertyRelative("lowerLeftEyelid"),
                    eyes.FindPropertyRelative("lowerRightEyelid"),
                };
                Helpers.MakeReferencesLocal<Transform>(to.transform, transLocalize);
            }

            SerializedProperty[] rendererLocalize =
            {
                sDescTo.FindProperty("VisemeSkinnedMesh"),
                eyes != null ? eyes.FindPropertyRelative("eyelidsSkinnedMesh") : null,
            };
            Helpers.MakeReferencesLocal<SkinnedMeshRenderer>(to.transform, rendererLocalize);

            sDescTo.ApplyModifiedProperties();
        }
#endif

#if VRC_SDK_VRCSDK3 && !UDON
        void FillEyeBones(GameObject avatar)
        {
            if(!avatar)
                return;
            var anim = avatar.GetComponent<Animator>();

            if(!anim)
                return;
            if(!anim.isHuman)
            {
                Log("FillEyeBones only works for humanoid avatars", LogType.Warning);
                return;
            }

            var desc = avatar.GetComponent<VRC_AvatarDescriptor>();
            var sDesc = new SerializedObject(desc);

            var leftEye = sDesc.FindProperty("customEyeLookSettings.leftEye");
            var rightEye = sDesc.FindProperty("customEyeLookSettings.rightEye");

            leftEye.objectReferenceValue = anim.GetBoneTransform(HumanBodyBones.LeftEye);
            rightEye.objectReferenceValue = anim.GetBoneTransform(HumanBodyBones.RightEye);

            sDesc.ApplyModifiedProperties();
        }
#endif

        /// <summary>
        /// Copies all DynamicBoneColliders from object and it's children to another object.
        /// </summary>
        /// <param name="removeOldColliders">Whether to remove all DynamicBoneColliders from target before copying</param>
        void CopyAllDynamicBoneColliders(GameObject from, GameObject to, bool createGameObjects, bool useIgnoreList)
        {
#if !PUMKIN_DBONES && !PUMKIN_OLD_DBONES

            Debug.Log("No DynamicBones found in project. You shouldn't be able to use this. Help!");
            return;
#else
            if(from == null || to == null)
                return;

            var dbcFromArr = from.GetComponentsInChildren<DynamicBoneCollider>(true);
            if(dbcFromArr == null || dbcFromArr.Length == 0)
                return;

            for(int i = 0; i < dbcFromArr.Length; i++)
            {
                var dbcFrom = dbcFromArr[i];
                var tTo = Helpers.FindTransformInAnotherHierarchy(dbcFrom.transform, to.transform, createGameObjects);
                if((!tTo) || (useIgnoreList && Helpers.ShouldIgnoreObject(dbcFrom.transform, _copierIgnoreArray, bCopier_ignoreArray_includeChildren)))
                    continue;

                var dbcToArr = tTo.GetComponentsInChildren<DynamicBoneCollider>(true);
                if(tTo != null)
                {
                    bool found = false;
                    for(int z = 0; z < dbcToArr.Length; z++)
                    {
                        var d = dbcToArr[z];
                        if(d.m_Bound == dbcFrom.m_Bound && d.m_Center == dbcFrom.m_Center &&
                            d.m_Direction == dbcFrom.m_Direction && d.m_Height == dbcFrom.m_Height && d.m_Radius == dbcFrom.m_Radius)
                        {
                            found = true;
                            break;
                        }
                    }

                    if(!found)
                    {
                        ComponentUtility.CopyComponent(dbcFrom);
                        ComponentUtility.PasteComponentAsNew(tTo.gameObject);
                    }
                }
            }
#endif
        }

        /// <summary>
        /// Temporary fixes to dbones using dirty dynamic types
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="createMissing"></param>
        /// <param name="useIgnoreList"></param>
        void CopyAllDynamicBonesNew(GameObject from, GameObject to, bool createMissing, bool useIgnoreList)
        {
#if !PUMKIN_DBONES && !PUMKIN_OLD_DBONES
            Debug.Log("No DynamicBones found in project. You shouldn't be able to use this. Help!");
            return;
#else
            if(!from || !to)
                return;

            var dBoneFromArr = from.GetComponentsInChildren<DynamicBone>(true);

            //Handle collider issues
            Type colliderListType = typeof(DynamicBone).GetField("m_Colliders").FieldType;
            Type colliderType = colliderListType.GetGenericArguments().FirstOrDefault();

            List<DynamicBone> newBones = new List<DynamicBone>();
            foreach(var dbFrom in dBoneFromArr)
            {
                if(useIgnoreList && Helpers.ShouldIgnoreObject(dbFrom.transform, _copierIgnoreArray, bCopier_ignoreArray_includeChildren))
                    continue;

                var transTo = Helpers.FindTransformInAnotherHierarchy(dbFrom.transform, to.transform, bCopier_dynamicBones_createObjects);
                if(!transTo)
                    continue;

                var dBoneToArr = transTo.GetComponents<DynamicBone>();

                if(!dbFrom.m_Root)
                {
                    LogVerbose("DynamicBone {0} of {1} doesn't have a root assigned. Ignoring", LogType.Warning, dbFrom.transform.name, dbFrom.transform.root.name);
                    continue;
                }

                bool foundSameDynBone = false;

                foreach(var bone in dBoneToArr)
                {
                    if(!bone.m_Root || newBones.Contains(bone))
                        continue;

                    //Check if the roots are the same to decide if it's supposed to be the same dyn bone script
                    if(bone.m_Root.name == dbFrom.m_Root.name)
                    {
                        //Check if exclusions are the same
                        List<string> exToPaths = bone.m_Exclusions
                            .Where(o => o != null)
                            .Select(o => Helpers.GetGameObjectPath(o.gameObject).ToLower())
                            .ToList();

                        List<string> exFromPaths = dbFrom.m_Exclusions
                            .Where(o => o != null)
                            .Select(o => Helpers.GetGameObjectPath(o.gameObject).ToLower())
                            .ToList();

                        bool exclusionsDifferent = false;
                        var exArr = exToPaths.Intersect(exFromPaths).ToArray();

                        if(exArr != null && (exToPaths.Count != 0 && exFromPaths.Count != 0) && exArr.Length == 0)
                            exclusionsDifferent = true;

                        //Check if colliders are the same
                        List<string> colToPaths = bone.m_Colliders
                            .Where(c => c != null)
                            .Select(c => Helpers.GetGameObjectPath(c.gameObject).ToLower())
                            .ToList();

                        List<string> colFromPaths = bone.m_Colliders
                            .Where(c => c != null)
                            .Select(c => Helpers.GetGameObjectPath(c.gameObject).ToLower())
                            .ToList();

                        bool collidersDifferent = false;
                        var colArr = colToPaths.Intersect(colFromPaths).ToArray();

                        if(colArr != null && (colToPaths.Count != 0 && colFromPaths.Count != 0) && colArr.Length == 0)
                            collidersDifferent = true;

                        //Found the same bone because root, exclusions and colliders are the same
                        if(!exclusionsDifferent && !collidersDifferent)
                        {
                            foundSameDynBone = true;
                            if(bCopier_dynamicBones_copySettings)
                            {
                                LogVerbose("{0} already has this DynamicBone, but we have to copy settings. Copying.", LogType.Log, bone.name);

                                bone.m_Damping = dbFrom.m_Damping;
                                bone.m_DampingDistrib = dbFrom.m_DampingDistrib;
                                bone.m_DistanceToObject = dbFrom.m_DistanceToObject;
                                bone.m_DistantDisable = dbFrom.m_DistantDisable;
                                bone.m_Elasticity = dbFrom.m_Elasticity;
                                bone.m_ElasticityDistrib = dbFrom.m_ElasticityDistrib;
                                bone.m_EndLength = dbFrom.m_EndLength;
                                bone.m_EndOffset = dbFrom.m_EndOffset;
                                bone.m_Force = dbFrom.m_Force;
                                bone.m_FreezeAxis = dbFrom.m_FreezeAxis;
                                bone.m_Gravity = dbFrom.m_Gravity;
                                bone.m_Inert = dbFrom.m_Inert;
                                bone.m_InertDistrib = dbFrom.m_InertDistrib;
                                bone.m_Radius = dbFrom.m_Radius;
                                bone.m_RadiusDistrib = dbFrom.m_RadiusDistrib;
                                bone.m_Stiffness = dbFrom.m_Stiffness;
                                bone.m_StiffnessDistrib = dbFrom.m_StiffnessDistrib;

                                bone.m_ReferenceObject = Helpers.FindTransformInAnotherHierarchy(dbFrom.m_ReferenceObject, bone.transform, false);
                            }
                            else
                            {
                                LogVerbose("{0} already has this DynamicBone but we aren't copying settings. Ignoring", LogType.Log, bone.name);
                            }
                            break;
                        }
                    }
                }

                if(!foundSameDynBone)
                {
                    if(createMissing)
                    {
                        LogVerbose("{0} doesn't have this DynamicBone but we have to create one. Creating.", LogType.Log, dbFrom.name);

                        var newDynBone = transTo.gameObject.AddComponent<DynamicBone>();
                        ComponentUtility.CopyComponent(dbFrom);
                        ComponentUtility.PasteComponentValues(newDynBone);

                        newDynBone.m_Root = Helpers.FindTransformInAnotherHierarchy(dbFrom.m_Root.transform, newDynBone.transform.root, false);

                        if(!newDynBone.m_Root)
                        {
                            Log("_Couldn't set root {0} for new DynamicBone in {1}'s {2}. GameObject is missing. Removing.", LogType.Warning, dbFrom.m_Root.name ?? "null", newDynBone.transform.root.name, newDynBone.transform.name == newDynBone.transform.root.name ? "root" : newDynBone.transform.root.name);
                            DestroyImmediate(newDynBone);
                            continue;
                        }

                        if(dbFrom.m_ReferenceObject)
                            newDynBone.m_ReferenceObject = Helpers.FindTransformInAnotherHierarchy(dbFrom.m_ReferenceObject, newDynBone.transform.root, false);

                        dynamic newColliders = Activator.CreateInstance(colliderListType);

                        for(int i = 0; i < newDynBone.m_Colliders.Count; i++)
                        {
                            var badRefCollider = newDynBone.m_Colliders[i];

                            if(!badRefCollider)
                                continue;

                            dynamic fixedRefCollider = null;

                            var t = Helpers.FindTransformInAnotherHierarchy(newDynBone.m_Colliders[i].transform, to.transform, false);

                            if(t == null)
                                continue;

                            dynamic[] toColls = t.GetComponents(colliderType);
                            foreach(var c in toColls)
                            {
                                if(c.m_Bound == badRefCollider.m_Bound && c.m_Center == badRefCollider.m_Center && c.m_Direction == badRefCollider.m_Direction &&
                                   !newDynBone.m_Colliders.Contains(c))
                                    fixedRefCollider = c;
                            }

                            if(fixedRefCollider != null)
                            {
                                LogVerbose("Fixed reference for {0} in {1}", LogType.Log, fixedRefCollider.name, newDynBone.name);
                                newColliders.Add(fixedRefCollider);
                            }
                        }

                        newDynBone.m_Colliders = newColliders;

                        var newExclusions = new HashSet<Transform>();

                        foreach(var ex in newDynBone.m_Exclusions)
                        {
                            if(!ex)
                                continue;

                            var t = Helpers.FindTransformInAnotherHierarchy(ex.transform, to.transform, false);
                            if(t)
                                newExclusions.Add(t);
                        }

                        newDynBone.m_Exclusions = newExclusions.ToList();
                        newBones.Add(newDynBone);

                        Log(Strings.Log.copiedDynamicBone, LogType.Log, dbFrom.transform.root.name, dbFrom.transform.name == dbFrom.transform.root.name ? "root" : dbFrom.transform.name, transTo.root.name);
                    }
                    else
                    {
                        LogVerbose("{0} doesn't have has this DynamicBone and we aren't creating a new one. Ignoring.", LogType.Log, dbFrom.name);
                    }
                }
            }
#endif
        }

        /// <summary>
        /// Copies Box, Capsule, Sphere and Mesh colliders from one object to another and all of it's children at once.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        void CopyAllColliders(GameObject from, GameObject to, bool createGameObjects, bool useIgnoreList)
        {
            if(from == null || to == null)
                return;
            if(!(bCopier_colliders_copyBox || bCopier_colliders_copyCapsule || bCopier_colliders_copyMesh || bCopier_colliders_copySphere))
                return;

            var cFromArr = from.GetComponentsInChildren<Collider>(true);

            for(int i = 0; i < cFromArr.Length; i++)
            {
                string log = Strings.Log.copyAttempt;
                var type = cFromArr[i].GetType();

                var cc = cFromArr[i];
                var cFromPath = Helpers.GetGameObjectPath(cc.gameObject);

                if(useIgnoreList && Helpers.ShouldIgnoreObject(cc.transform, _copierIgnoreArray, bCopier_ignoreArray_includeChildren))
                    continue;

                if(cFromPath != null)
                {
                    var tTo = to.transform.root.Find(cFromPath, createGameObjects, cc.transform);

                    if(!tTo)
                        continue;

                    GameObject cToObj = tTo.gameObject;

                    var cToArr = cToObj.GetComponents<Collider>();
                    bool found = false;

                    for(int z = 0; z < cToArr.Length; z++)
                    {
                        if(Helpers.CollidersAreIdentical(cToArr[z], cFromArr[i]))
                        {
                            found = true;
                            Log(log + " - " + Strings.Log.failedAlreadyHas, LogType.Warning, cToObj.name, type.ToString());
                            break;
                        }
                    }
                    if(!found)
                    {
                        ComponentUtility.CopyComponent(cFromArr[i]);
                        ComponentUtility.PasteComponentAsNew(cToObj);

                        Log(log + " - " + Strings.Log.success, LogType.Log, type.ToString(), cFromArr[i].gameObject.name, cToObj.name);
                    }
                }
            }
        }

        /// <summary>
        /// Copies character, configurable, fixed hinge and spring joints from one object to another and all of it's children at once.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        void CopyAllJoints(GameObject from, GameObject to, bool createGameObjects, bool useIgnoreList)
        {
            if(from == null || to == null)
                return;
            if(!(bCopier_joints_character || bCopier_joints_configurable || bCopier_joints_fixed || bCopier_joints_hinge || bCopier_joints_spring))
                return;

            var jointFromArr = from.GetComponentsInChildren<Joint>(true);

            for(int i = 0; i < jointFromArr.Length; i++)
            {
                var jointFrom = jointFromArr[i];
                var jointTransFrom = jointFrom.transform;

                Type jointType = jointFrom.GetType();
                if((!bCopier_joints_character && jointType == typeof(CharacterJoint)) ||
                    (!bCopier_joints_configurable && jointType == typeof(ConfigurableJoint)) ||
                    (!bCopier_joints_fixed && jointType == typeof(FixedJoint)) ||
                    (!bCopier_joints_spring && jointType == typeof(SpringJoint)) ||
                    (!bCopier_joints_hinge && jointType == typeof(CharacterJoint)))
                {
                    Log(Strings.Log.notSelectedInCopierIgnoring, LogType.Log, jointTransFrom.gameObject.name, jointType.Name);
                    continue;
                }

                var jointTransTo = Helpers.FindTransformInAnotherHierarchy(jointTransFrom, to.transform, createGameObjects);

                if(!jointTransTo)
                    continue;

                Log(Strings.Log.copyAttempt, LogType.Log, jointType.Name, jointTransFrom.gameObject.name, jointTransTo.gameObject.name);
                Joint jointTo = jointTransTo.gameObject.AddComponent(jointFrom.GetType()) as Joint;

                ComponentUtility.CopyComponent(jointFrom);
                ComponentUtility.PasteComponentValues(jointTo);

                Transform targetTrans = null;
                Rigidbody targetBody = null;
                if(jointTo.connectedBody != null)
                    targetTrans = Helpers.FindTransformInAnotherHierarchy(jointFrom.connectedBody.transform, to.transform, createGameObjects);
                if(targetTrans != null)
                    targetBody = targetTrans.GetComponent<Rigidbody>();

                jointTo.connectedBody = targetBody;
            }
        }

        /// <summary>
        /// Copies all transform settings in children in object and children
        /// </summary>
        /// <param name="useIgnoreList">Whether or not to use copier ignore list</param>
        void CopyAllTransforms(GameObject from, GameObject to, bool useIgnoreList)
        {
            if(from == null || to == null || !(bCopier_transforms_copyPosition || bCopier_transforms_copyRotation || bCopier_transforms_copyScale))
                return;

            string type = typeof(Transform).Name;

            var tFromArr = from.GetComponentsInChildren<Transform>(true);

            for(int i = 0; i < tFromArr.Length; i++)
            {
                Transform tFrom = tFromArr[i];

                if(tFrom == tFrom.root || tFrom == tFrom.root.Find(tFrom.name) ||
                    (useIgnoreList && Helpers.ShouldIgnoreObject(tFrom, _copierIgnoreArray, bCopier_ignoreArray_includeChildren)))
                    continue;

                string log = String.Format(Strings.Log.copyAttempt + " - ", tFrom.gameObject.name, from.name, to.name);

                Transform tTo = Helpers.FindTransformInAnotherHierarchy(tFrom, to.transform, false);
                if(tTo)
                {
                    if(bCopier_transforms_copyPosition)
                        tTo.localPosition = tFrom.localPosition;
                    if(bCopier_transforms_copyScale)
                        tTo.localScale = tFrom.localScale;
                    if(bCopier_transforms_copyRotation)
                    {
                        tTo.localEulerAngles = tFrom.localEulerAngles;
                        tTo.localRotation = tFrom.localRotation;
                    }
                    Log(log + Strings.Log.success, LogType.Log);
                }
                else
                    Log(log + Strings.Log.failedHasNoIgnoring, LogType.Warning, from.name, tFrom.gameObject.name);
            }
        }

        /// <summary>
        /// Copies settings of all SkinnedMeshRenderers in object and children.
        /// Does NOT copy mesh, bounds and root bone settings because that breaks everything.
        /// </summary>
        void CopyAllSkinnedMeshRenderersSettings(GameObject from, GameObject to, bool useIgnoreList)
        {
            if((from == null || to == null) || (!(bCopier_skinMeshRender_copyBlendShapeValues || bCopier_skinMeshRender_copyMaterials || bCopier_skinMeshRender_copySettings)))
                return;

            string log = String.Format(Strings.Log.copyAttempt + " - ", Strings.Copier.skinMeshRender, from.name, to.name);

            var rFromArr = from.GetComponentsInChildren<SkinnedMeshRenderer>(true);

            for(int i = 0; i < rFromArr.Length; i++)
            {
                var rFrom = rFromArr[i];
                var rFromPath = Helpers.GetGameObjectPath(rFrom.gameObject);

                if(rFromPath != null)
                {
                    var tTo = to.transform.root.Find(rFromPath);

                    if((!tTo) ||
                        (useIgnoreList && Helpers.ShouldIgnoreObject(rFrom.transform, _copierIgnoreArray, bCopier_ignoreArray_includeChildren)))
                        continue;

                    GameObject rToObj = tTo.gameObject;

                    var rTo = rToObj.GetComponent<SkinnedMeshRenderer>();

                    if(rTo != null)
                    {
                        if(bCopier_skinMeshRender_copySettings)
                        {
                            var t = Helpers.FindTransformInAnotherHierarchy(rFrom.rootBone, rTo.transform.root, false);
                            rTo.rootBone = t ?? rTo.rootBone;
                            t = Helpers.FindTransformInAnotherHierarchy(rFrom.probeAnchor, rTo.transform.root, false);

                            rTo.allowOcclusionWhenDynamic = rFrom.allowOcclusionWhenDynamic;
                            rTo.quality = rFrom.quality;
                            rTo.probeAnchor = t ?? rTo.probeAnchor;
                            rTo.lightProbeUsage = rFrom.lightProbeUsage;
                            rTo.reflectionProbeUsage = rFrom.reflectionProbeUsage;
                            rTo.shadowCastingMode = rFrom.shadowCastingMode;
                            rTo.receiveShadows = rFrom.receiveShadows;
                            rTo.motionVectorGenerationMode = rFrom.motionVectorGenerationMode;
                            rTo.skinnedMotionVectors = rFrom.skinnedMotionVectors;
                            rTo.allowOcclusionWhenDynamic = rFrom.allowOcclusionWhenDynamic;
                            rTo.enabled = rFrom.enabled;
                        }
                        if(bCopier_skinMeshRender_copyBlendShapeValues && rFrom.sharedMesh)
                        {
                            for(int z = 0; z < rFrom.sharedMesh.blendShapeCount; z++)
                            {
                                string shapeName = rFrom.sharedMesh.GetBlendShapeName(z);
                                int shapeIndex = rTo.sharedMesh.GetBlendShapeIndex(shapeName);
                                if(shapeIndex != -1)
                                    rTo.SetBlendShapeWeight(shapeIndex, rFrom.GetBlendShapeWeight(shapeIndex));
                            }
                        }
                        if(bCopier_skinMeshRender_copyMaterials)
                            rTo.sharedMaterials = rFrom.sharedMaterials;

                        Log(log + Strings.Log.success);
                    }
                    else
                    {
                        Log(log + Strings.Log.failedDoesntHave, LogType.Warning, rTo.gameObject.name, rFrom.GetType().ToString());
                    }
                }
            }
        }

        /// <summary>
        /// Copies all TrailRenderers in object and it's children.
        /// </summary>
        /// <param name="createGameObjects">Whether to create missing GameObjects</param>
        void CopyAllTrailRenderers(GameObject from, GameObject to, bool createGameObjects, bool useIgnoreList)
        {
            if(from == null || to == null)
                return;

            var rFromArr = from.GetComponentsInChildren<TrailRenderer>(true);

            for(int i = 0; i < rFromArr.Length; i++)
            {
                var rFrom = rFromArr[i];
                var tTo = Helpers.FindTransformInAnotherHierarchy(rFrom.transform, to.transform, createGameObjects);

                if(!tTo)
                    continue;

                if(useIgnoreList && Helpers.ShouldIgnoreObject(rFrom.transform, _copierIgnoreArray, bCopier_ignoreArray_includeChildren))
                    continue;

                var rToObj = tTo.gameObject;
                var rTo = rToObj.GetComponent<TrailRenderer>();

                if(rTo == null && bCopier_trailRenderers_createMissing)
                {
                    rTo = rToObj.AddComponent<TrailRenderer>();
                }

                if((rTo != null && bCopier_trailRenderers_copySettings) || bCopier_trailRenderers_createMissing)
                {
                    ComponentUtility.CopyComponent(rFrom);
                    ComponentUtility.PasteComponentValues(rTo);
                }
            }
        }

        /// <summary>
        /// Copies all RigidBodies in object and in its children.
        /// </summary>
        void CopyAllRigidBodies(GameObject from, GameObject to, bool createGameObjects, bool useIgnoreList)
        {
            if(from == null || to == null)
                return;

            var rFromArr = from.GetComponentsInChildren<Rigidbody>(true);

            for(int i = 0; i < rFromArr.Length; i++)
            {
                var rFrom = rFromArr[i];
                var tTo = Helpers.FindTransformInAnotherHierarchy(rFrom.transform, to.transform, createGameObjects);

                if(!tTo)
                    continue;

                if(useIgnoreList && Helpers.ShouldIgnoreObject(rFrom.transform, _copierIgnoreArray, bCopier_ignoreArray_includeChildren))
                    continue;

                var rToObj = tTo.gameObject;

                var rTo = rToObj.GetComponent<Rigidbody>();

                if(rTo == null && bCopier_rigidBodies_createMissing)
                {
                    rTo = rToObj.AddComponent<Rigidbody>();
                }
                if(rTo != null && (bCopier_rigidBodies_copySettings || bCopier_rigidBodies_createMissing))
                {
                    ComponentUtility.CopyComponent(rFrom);
                    ComponentUtility.PasteComponentValues(rTo);
                }
            }
        }

        /// <summary>
        /// Copies all ParticleSystems in object and its children
        /// </summary>
        /// <param name="createGameObjects">Whether to create game objects if missing</param>
        void CopyAllParticleSystems(GameObject from, GameObject to, bool createGameObjects, bool useIgnoreList)
        {
            var partSysFromArr = from.GetComponentsInChildren<ParticleSystem>(true);

            for(int i = 0; i < partSysFromArr.Length; i++)
            {
                var partSys = partSysFromArr[i];

                if(useIgnoreList && Helpers.ShouldIgnoreObject(partSys.transform, _copierIgnoreArray, bCopier_ignoreArray_includeChildren))
                    continue;

                var transTo = Helpers.FindTransformInAnotherHierarchy(partSys.transform, to.transform, createGameObjects);

                if(transTo != null)
                {
                    var partSysTo = transTo.GetComponent<ParticleSystem>();
                    if(bCopier_particleSystems_replace || partSysTo == null)
                    {
                        DestroyParticleSystems(transTo.gameObject, false);

                        ComponentUtility.CopyComponent(partSys);
                        ComponentUtility.PasteComponentAsNew(transTo.gameObject);

                        Log(Strings.Log.successCopiedOverFromTo, LogType.Log, "ParticleSystem", CopierSelectedFrom.name, partSys.gameObject.name, SelectedAvatar.name, transTo.gameObject.name);
                    }
                    else
                    {
                        Log(Strings.Log.failedAlreadyHas, LogType.Log, partSys.gameObject.name, "ParticleSystem");
                    }
                }
            }
        }

        /// <summary>
        /// Copies all Aim Constrains in object and it's children
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="createGameObjects">Whether to create game objects if missing</param>
        /// <param name="useIgnoreList"></param>
        void CopyAllAimConstraints(GameObject from, GameObject to, bool createGameObjects, bool useIgnoreList)
        {
            var aimConFromArr = from.GetComponentsInChildren<AimConstraint>(true);
            const string typeString = "AimConstraint";

            for(int i = 0; i < aimConFromArr.Length; i++)
            {
                var aimCon = aimConFromArr[i];

                if(useIgnoreList && Helpers.ShouldIgnoreObject(aimCon.transform, _copierIgnoreArray, bCopier_ignoreArray_includeChildren))
                    continue;

                var transTo = Helpers.FindTransformInAnotherHierarchy(aimCon.transform, to.transform, createGameObjects);

                if(transTo != null)
                {
                    var aimConTo = transTo.GetComponent<AimConstraint>();

                    if(bCopier_aimConstraint_replaceOld || aimConTo == null)
                    {
                        Helpers.DestroyAppropriate(aimConTo);

                        ComponentUtility.CopyComponent(aimCon);
                        aimConTo = transTo.gameObject.AddComponent<AimConstraint>();
                        ComponentUtility.PasteComponentValues(aimConTo);

                        if(aimConTo.worldUpType == AimConstraint.WorldUpType.ObjectRotationUp || aimConTo.worldUpType == AimConstraint.WorldUpType.ObjectUp)
                        {
                            var upObj = aimConTo.worldUpObject;
                            if(upObj && upObj.root == from.transform)
                                aimConTo.worldUpObject = Helpers.FindTransformInAnotherHierarchy(upObj, to.transform, createGameObjects);
                        }
                        var sources = new List<ConstraintSource>();
                        aimConTo.GetSources(sources);

                        for(int z = 0; z < sources.Count; z++)
                        {
                            var t = sources[z];
                            if(t.sourceTransform && t.sourceTransform.root == from.transform)
                            {
                                var cs = sources[z];
                                cs.sourceTransform = Helpers.FindTransformInAnotherHierarchy(t.sourceTransform, to.transform, createGameObjects);
                                aimConTo.SetSource(z, cs);
                            }
                        }

                        if(bCopier_aimConstraint_onlyIfHasValidSources && !Helpers.ConstraintHasValidSources(aimConTo))
                        {
                            Log(Strings.Log.constraintHasNoValidSources, LogType.Warning, to.name, aimCon.gameObject.name);
                            Helpers.DestroyAppropriate(aimConTo);
                            return;
                        }

                        Log(Strings.Log.successCopiedOverFromTo, LogType.Log, typeString,
                                CopierSelectedFrom.name,
                                aimCon.transform == aimCon.transform.root ? "root" : aimCon.gameObject.name,
                                SelectedAvatar.name,
                                transTo == transTo.root ? "root" : transTo.gameObject.name);
                    }
                    else
                    {
                        Log(Strings.Log.failedAlreadyHas, LogType.Log, aimCon.gameObject.name, typeString);
                    }
                }
            }
        }

        /// <summary>
        /// Copies all LookAt Constrains in object and it's children
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="createGameObjects">Whether to create game objects if missing</param>
        /// <param name="useIgnoreList"></param>
        void CopyAllLookAtConstraints(GameObject from, GameObject to, bool createGameObjects, bool useIgnoreList)
        {
            var lookConFromArr = from.GetComponentsInChildren<LookAtConstraint>(true);
            const string typeString = "LookAtConstraint";

            for(int i = 0; i < lookConFromArr.Length; i++)
            {
                var lookCon = lookConFromArr[i];

                if(useIgnoreList && Helpers.ShouldIgnoreObject(lookCon.transform, _copierIgnoreArray, bCopier_ignoreArray_includeChildren))
                    continue;

                var transTo = Helpers.FindTransformInAnotherHierarchy(lookCon.transform, to.transform, createGameObjects);

                if(transTo != null)
                {
                    var lookConTo = transTo.GetComponent<LookAtConstraint>();

                    if(bCopier_lookAtConstraint_replaceOld || lookConTo == null)
                    {
                        Helpers.DestroyAppropriate(lookConTo);

                        ComponentUtility.CopyComponent(lookCon);
                        lookConTo = transTo.gameObject.AddComponent<LookAtConstraint>();
                        ComponentUtility.PasteComponentValues(lookConTo);

                        if(lookConTo.useUpObject)
                        {
                            var upObj = lookConTo.worldUpObject;
                            if(upObj && upObj.root == from.transform)
                                lookConTo.worldUpObject = Helpers.FindTransformInAnotherHierarchy(upObj, to.transform, createGameObjects);
                        }

                        var sources = new List<ConstraintSource>();
                        lookConTo.GetSources(sources);

                        for(int z = 0; z < sources.Count; z++)
                        {
                            var t = sources[z];
                            if(t.sourceTransform && t.sourceTransform.root == from.transform)
                            {
                                var cs = sources[z];
                                cs.sourceTransform = Helpers.FindTransformInAnotherHierarchy(t.sourceTransform, to.transform, createGameObjects);
                                lookConTo.SetSource(z, cs);
                            }
                        }

                        if(bCopier_lookAtConstraint_onlyIfHasValidSources && !Helpers.ConstraintHasValidSources(lookConTo))
                        {
                            Log(Strings.Log.constraintHasNoValidSources, LogType.Warning, to.name, lookCon.gameObject.name, typeString);
                            Helpers.DestroyAppropriate(lookCon);
                            return;
                        }

                        Log(Strings.Log.successCopiedOverFromTo, LogType.Log, typeString,
                                CopierSelectedFrom.name,
                                lookCon.transform == lookCon.transform.root ? "root" : lookCon.gameObject.name,
                                SelectedAvatar.name,
                                transTo == transTo.root ? "root" : transTo.gameObject.name);
                    }
                    else
                    {
                        Log(Strings.Log.failedAlreadyHas, LogType.Log, lookCon.gameObject.name, typeString);
                    }
                }
            }
        }

        /// <summary>
        /// Copies all Parent Constrains in object and it's children
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="createGameObjects">Whether to create game objects if missing</param>
        /// <param name="useIgnoreList"></param>
        void CopyAllParentConstraints(GameObject from, GameObject to, bool createGameObjects, bool useIgnoreList)
        {
            var parConFromArr = from.GetComponentsInChildren<ParentConstraint>(true);
            const string typeString = "ParentConstraint";

            for(int i = 0; i < parConFromArr.Length; i++)
            {
                var parCon = parConFromArr[i];

                if(useIgnoreList && Helpers.ShouldIgnoreObject(parCon.transform, _copierIgnoreArray, bCopier_ignoreArray_includeChildren))
                    continue;

                var transTo = Helpers.FindTransformInAnotherHierarchy(parCon.transform, to.transform, createGameObjects);

                if(transTo != null)
                {
                    var parConTo = transTo.GetComponent<ParentConstraint>();

                    if(bCopier_parentConstraint_replaceOld || parConTo == null)
                    {
                        Helpers.DestroyAppropriate(parConTo);

                        ComponentUtility.CopyComponent(parCon);
                        parConTo = transTo.gameObject.AddComponent<ParentConstraint>();
                        ComponentUtility.PasteComponentValues(parConTo);

                        var sources = new List<ConstraintSource>();
                        parConTo.GetSources(sources);

                        for(int z = 0; z < sources.Count; z++)
                        {
                            var t = sources[z];
                            if(t.sourceTransform && t.sourceTransform.root == from.transform)
                            {
                                var cs = sources[z];
                                cs.sourceTransform = Helpers.FindTransformInAnotherHierarchy(t.sourceTransform, to.transform, createGameObjects);
                                parConTo.SetSource(z, cs);
                            }
                        }

                        if(bCopier_parentConstraint_onlyIfHasValidSources && !Helpers.ConstraintHasValidSources(parConTo))
                        {
                            Log(Strings.Log.constraintHasNoValidSources, LogType.Warning, to.name, parCon.gameObject.name, typeString);
                            Helpers.DestroyAppropriate(parCon);
                            return;
                        }

                        Log(Strings.Log.successCopiedOverFromTo, LogType.Log, typeString,
                                CopierSelectedFrom.name,
                                parCon.transform == parCon.transform.root ? "root" : parCon.gameObject.name,
                                SelectedAvatar.name,
                                transTo == transTo.root ? "root" : transTo.gameObject.name);
                    }
                    else
                    {
                        Log(Strings.Log.failedAlreadyHas, LogType.Log, parCon.gameObject.name, typeString);
                    }
                }
            }
        }

        ///// <summary>
        ///// Bad function //TODO:Rewrite with unity properties
        ///// </summary>
        //void CopyAllClothComponents(GameObject from, GameObject to, bool createGameObjects, bool useIgnoreList)
        //{
        //    var clothFromArr = from.GetComponentsInChildren<Cloth>(true);
        //    const string typeString = "Cloth";

        //    for(int i = 0; i < clothFromArr.Length; i++)
        //    {
        //        var clothFrom = clothFromArr[i];

        //        if(useIgnoreList && Helpers.ShouldIgnoreObject(clothFrom.transform, _copierIgnoreArray, bCopier_ignoreArray_includeChildren))
        //            continue;

        //        var transTo = Helpers.FindTransformInAnotherHierarchy(clothFrom.transform, to.transform, createGameObjects);

        //        if(transTo != null)
        //        {
        //            var clothTo = transTo.GetComponent<Cloth>();

        //            if(bCopier_cloth_replaceOld || clothTo == null)
        //            {
        //                Helpers.DestroyAppropriate(clothTo);

        //                ComponentUtility.CopyComponent(clothFrom);
        //                clothTo = transTo.gameObject.AddComponent<Cloth>();
        //                ComponentUtility.PasteComponentValues(clothTo);

        //                var capsuleColFrom = clothFrom.capsuleColliders;
        //                var capsuleColTo = new List<CapsuleCollider>();

        //                foreach(var fromCollider in capsuleColFrom)
        //                {
        //                    var newColTrans = Helpers.FindTransformInAnotherHierarchy(fromCollider.transform, to.transform, createGameObjects);
        //                    var newCol = newColTrans.GetComponent<CapsuleCollider>();
        //                    if(newColTrans && !capsuleColTo.Contains(newCol))
        //                        capsuleColTo.Add(newCol);
        //                }
        //                clothTo.capsuleColliders = capsuleColTo.ToArray();

        //                var sphereColFrom = clothFrom.sphereColliders;
        //                var sphereColTo = new List<ClothSphereColliderPair>();

        //                foreach(var fromCollider in sphereColFrom)
        //                {
        //                    var newColTransFirst = Helpers.FindTransformInAnotherHierarchy(fromCollider.first.transform, to.transform, createGameObjects);
        //                    var newColTransSecond = Helpers.FindTransformInAnotherHierarchy(fromCollider.second.transform, to.transform, createGameObjects);

        //                    var newColFirst = newColTransFirst.GetComponent<SphereCollider>();
        //                    var newColSecond = newColTransFirst.GetComponent<SphereCollider>();

        //                    var allSphereColls = new List<SphereCollider>();
        //                    foreach(var sc in sphereColTo)  //I forgot how to linq
        //                    {
        //                        allSphereColls.Add(sc.first);
        //                        allSphereColls.Add(sc.second);
        //                    }
        //                    if(allSphereColls.Contains(newColSecond))
        //                    {
        //                        var cols = newColTransSecond.GetComponents<SphereCollider>();
        //                        foreach(var c in cols)
        //                        {
        //                            if(!allSphereColls.Contains(c))
        //                                newColSecond = c;
        //                        }
        //                    }

        //                    var newPair = new ClothSphereColliderPair(newColFirst, newColSecond);

        //                    if(newColTransFirst && !sphereColTo.Contains(newPair))
        //                        sphereColTo.Add(newPair);
        //                }
        //                clothTo.sphereColliders = sphereColTo.ToArray();

        //                Log(Strings.Log.successCopiedOverFromTo, LogType.Log, typeString,
        //                        CopierSelectedFrom.name,
        //                        clothFrom.transform == clothFrom.transform.root ? "root" : clothFrom.gameObject.name,
        //                        SelectedAvatar.name,
        //                        transTo == transTo.root ? "root" : transTo.gameObject.name);
        //            }
        //            else
        //            {
        //                Log(Strings.Log.failedAlreadyHas, LogType.Log, clothFrom.gameObject.name, typeString);
        //            }
        //        }
        //    }
        //}

        /// <summary>
        /// Copies all Position Constrains in object and it's children
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="createGameObjects">Whether to create game objects if missing</param>
        /// <param name="useIgnoreList"></param>
        void CopyAllPositionConstraints(GameObject from, GameObject to, bool createGameObjects, bool useIgnoreList)
        {
            var posConFromArr = from.GetComponentsInChildren<PositionConstraint>(true);
            const string typeString = "PositionConstraint";

            for(int i = 0; i < posConFromArr.Length; i++)
            {
                var posCon = posConFromArr[i];

                if(useIgnoreList && Helpers.ShouldIgnoreObject(posCon.transform, _copierIgnoreArray, bCopier_ignoreArray_includeChildren))
                    continue;

                var transTo = Helpers.FindTransformInAnotherHierarchy(posCon.transform, to.transform, createGameObjects);

                if(transTo != null)
                {
                    var posConTo = transTo.GetComponent<PositionConstraint>();

                    if(bCopier_positionConstraint_replaceOld || posConTo == null)
                    {
                        Helpers.DestroyAppropriate(posConTo);

                        ComponentUtility.CopyComponent(posCon);
                        posConTo = transTo.gameObject.AddComponent<PositionConstraint>();
                        ComponentUtility.PasteComponentValues(posConTo);

                        var sources = new List<ConstraintSource>();
                        posConTo.GetSources(sources);

                        for(int z = 0; z < sources.Count; z++)
                        {
                            var t = sources[z];
                            if(t.sourceTransform && t.sourceTransform.root == from.transform)
                            {
                                var cs = sources[z];
                                cs.sourceTransform = Helpers.FindTransformInAnotherHierarchy(t.sourceTransform, to.transform, createGameObjects);
                                posConTo.SetSource(z, cs);
                            }
                        }

                        if(bCopier_positionConstraint_onlyIfHasValidSources && !Helpers.ConstraintHasValidSources(posConTo))
                        {
                            Log(Strings.Log.constraintHasNoValidSources, LogType.Warning, to.name, posCon.gameObject.name, typeString);
                            Helpers.DestroyAppropriate(posCon);
                            return;
                        }

                        Log(Strings.Log.successCopiedOverFromTo, LogType.Log, typeString,
                                CopierSelectedFrom.name,
                                posCon.transform == posCon.transform.root ? "root" : posCon.gameObject.name,
                                SelectedAvatar.name,
                                transTo == transTo.root ? "root" : transTo.gameObject.name);
                    }
                    else
                    {
                        Log(Strings.Log.failedAlreadyHas, LogType.Log, posCon.gameObject.name, typeString);
                    }
                }
            }
        }

        /// <summary>
        /// Copies all Rotation Constrains in object and it's children
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="createGameObjects">Whether to create game objects if missing</param>
        /// <param name="useIgnoreList"></param>
        void CopyAllRotationConstraints(GameObject from, GameObject to, bool createGameObjects, bool useIgnoreList)
        {
            var rotConFromArr = from.GetComponentsInChildren<RotationConstraint>(true);
            const string typeString = "RotationConstraint";

            for(int i = 0; i < rotConFromArr.Length; i++)
            {
                var rotCon = rotConFromArr[i];

                if(useIgnoreList && Helpers.ShouldIgnoreObject(rotCon.transform, _copierIgnoreArray, bCopier_ignoreArray_includeChildren))
                    continue;

                var transTo = Helpers.FindTransformInAnotherHierarchy(rotCon.transform, to.transform, createGameObjects);

                if(transTo != null)
                {
                    var rotConTo = transTo.GetComponent<RotationConstraint>();

                    if(bCopier_rotationConstraint_replaceOld || rotConTo == null)
                    {
                        Helpers.DestroyAppropriate(rotConTo);

                        ComponentUtility.CopyComponent(rotCon);
                        rotConTo = transTo.gameObject.AddComponent<RotationConstraint>();
                        ComponentUtility.PasteComponentValues(rotConTo);

                        var sources = new List<ConstraintSource>();
                        rotConTo.GetSources(sources);

                        for(int z = 0; z < sources.Count; z++)
                        {
                            var t = sources[z];
                            if(t.sourceTransform && t.sourceTransform.root == from.transform)
                            {
                                var cs = sources[z];
                                cs.sourceTransform = Helpers.FindTransformInAnotherHierarchy(t.sourceTransform, to.transform, createGameObjects);
                                rotConTo.SetSource(z, cs);
                            }
                        }

                        if(bCopier_rotationConstraint_onlyIfHasValidSources && !Helpers.ConstraintHasValidSources(rotConTo))
                        {
                            Log(Strings.Log.constraintHasNoValidSources, LogType.Warning, to.name, rotCon.gameObject.name, typeString);
                            Helpers.DestroyAppropriate(rotCon);
                            return;
                        }

                        Log(Strings.Log.successCopiedOverFromTo, LogType.Log, typeString,
                                CopierSelectedFrom.name,
                                rotCon.transform == rotCon.transform.root ? "root" : rotCon.gameObject.name,
                                SelectedAvatar.name,
                                transTo == transTo.root ? "root" : transTo.gameObject.name);
                    }
                    else
                    {
                        Log(Strings.Log.failedAlreadyHas, LogType.Log, rotCon.gameObject.name, typeString);
                    }
                }
            }
        }

        /// <summary>
        /// Copies all Scale Constrains in object and it's children
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="createGameObjects">Whether to create game objects if missing</param>
        /// <param name="useIgnoreList"></param>
        void CopyAllScaleConstraints(GameObject from, GameObject to, bool createGameObjects, bool useIgnoreList)
        {
            var scaleConFromArr = from.GetComponentsInChildren<ScaleConstraint>(true);
            const string typeString = "ScaleConstraint";

            for(int i = 0; i < scaleConFromArr.Length; i++)
            {
                var scaleCon = scaleConFromArr[i];

                if(useIgnoreList && Helpers.ShouldIgnoreObject(scaleCon.transform, _copierIgnoreArray, bCopier_ignoreArray_includeChildren))
                    continue;

                var transTo = Helpers.FindTransformInAnotherHierarchy(scaleCon.transform, to.transform, createGameObjects);

                if(transTo != null)
                {
                    var scaleConTo = transTo.GetComponent<ScaleConstraint>();

                    if(bCopier_scaleConstraint_replaceOld || scaleConTo == null)
                    {
                        Helpers.DestroyAppropriate(scaleConTo);

                        ComponentUtility.CopyComponent(scaleCon);
                        scaleConTo = transTo.gameObject.AddComponent<ScaleConstraint>();
                        ComponentUtility.PasteComponentValues(scaleConTo);

                        var sources = new List<ConstraintSource>();
                        scaleConTo.GetSources(sources);

                        for(int z = 0; z < sources.Count; z++)
                        {
                            var t = sources[z];
                            if(t.sourceTransform && t.sourceTransform.root == from.transform)
                            {
                                var cs = sources[z];
                                cs.sourceTransform = Helpers.FindTransformInAnotherHierarchy(t.sourceTransform, to.transform, createGameObjects);
                                scaleConTo.SetSource(z, cs);
                            }
                        }

                        if(bCopier_scaleConstraint_onlyIfHasValidSources && !Helpers.ConstraintHasValidSources(scaleConTo))
                        {
                            Log(Strings.Log.constraintHasNoValidSources, LogType.Warning, to.name, scaleCon.gameObject.name, typeString);
                            Helpers.DestroyAppropriate(scaleCon);
                            return;
                        }

                        Log(Strings.Log.successCopiedOverFromTo, LogType.Log, typeString,
                                CopierSelectedFrom.name,
                                scaleCon.transform == scaleCon.transform.root ? "root" : scaleCon.gameObject.name,
                                SelectedAvatar.name,
                                transTo == transTo.root ? "root" : transTo.gameObject.name);
                    }
                    else
                    {
                        Log(Strings.Log.failedAlreadyHas, LogType.Log, scaleCon.gameObject.name, typeString);
                    }
                }
            }
        }

#endregion

#region Destroy Functions

        /// <summary>
        /// Destroys ParticleSystem in object
        /// </summary>
        /// <param name="destroyInChildrenToo">Whether to destroy particle systems in children as well</param>
        private void DestroyParticleSystems(GameObject from, bool destroyInChildrenToo = true)
        {
            var sysList = new List<ParticleSystem>();
            if(destroyInChildrenToo)
                sysList.AddRange(from.GetComponentsInChildren<ParticleSystem>(true));
            else
                sysList.AddRange(from.GetComponents<ParticleSystem>());

            foreach(var p in sysList)
            {
                var rend = p.GetComponent<ParticleSystemRenderer>();

                if(rend != null)
                    DestroyImmediate(rend);

                Log(Strings.Log.removeAttempt + " - " + Strings.Log.success, LogType.Log, p.ToString(), from.name);
                DestroyImmediate(p);
            }
        }

        /// <summary>
        /// Destroys GameObjects in object and all children, if it has no children and if it's not a bone
        /// </summary>
        void DestroyEmptyGameObjects(GameObject from)
        {
            var obj = from.GetComponentsInChildren<Transform>(true);
            var renders = from.GetComponentsInChildren<SkinnedMeshRenderer>(true);

            var bones = new HashSet<Transform>();

            foreach(var r in renders)
            {
                foreach(var b in r.bones)
                {
                    bones.Add(b);
                }
            }

            foreach(var t in obj.OrderBy(o => o.childCount))
            {
                if(t != null && t != t.root && t.GetComponents<Component>().Length == 1 && !bones.Contains(t))
                {
                    int c = t.childCount;
                    for(int i = 0; i < t.childCount; i++)
                    {
                        var n = t.GetChild(i);
                        if(!bones.Contains(n))
                            c--;
                    }
                    if(c <= 0 && (t.name.ToLower() != (t.parent.name.ToLower() + "_end")))
                    {
                        if(PrefabUtility.GetPrefabInstanceStatus(t) == PrefabInstanceStatus.NotAPrefab || PrefabUtility.GetPrefabInstanceStatus(t) == PrefabInstanceStatus.Disconnected)
                        {
                            Log(Strings.Log.hasNoComponentsOrChildrenDestroying, LogType.Log, t.name);
                            DestroyImmediate(t.gameObject);
                        }
                        else
                        {
                            Log(Strings.Log.cantBeDestroyedPartOfPrefab, LogType.Warning, t.name, "GameObject");
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Destroys all Missing Script components on avatar
        /// </summary>
        void DestroyMissingScripts(GameObject avatar)
        {
            if(EditorApplication.isPlaying)
            {
                Log("Can't remove missing scripts in play mode, it causes crashes", LogType.Warning);
                return;
            }

            var ts = avatar.GetComponentsInChildren<Transform>(true);
            foreach(var t in ts)
            {
                if(Helpers.DestroyMissingScriptsInGameObject(t.gameObject))
                    Log(Strings.Log.hasMissingScriptDestroying, LogType.Log, Helpers.GetGameObjectPath(t));
            }
        }

        /// <summary>
        /// Destroy all components of type in object and it's children
        /// </summary>
        void DestroyAllComponentsOfType(GameObject obj, Type type, bool ignoreRoot, bool useIgnoreList)
        {
            string log = "";

            Component[] comps = obj.transform.GetComponentsInChildren(type, true);

            if(comps != null && comps.Length > 0)
            {
                for(int i = 0; i < comps.Length; i++)
                {
                    if((ignoreRoot && comps[i].transform.parent == null) ||
                        (useIgnoreList && Helpers.ShouldIgnoreObject(comps[i].transform, _copierIgnoreArray, bCopier_ignoreArray_includeChildren)))
                        continue;

                    log = Strings.Log.removeAttempt + " - ";
                    string name = comps[i].name;

                    if(!PrefabUtility.IsPartOfPrefabInstance(comps[i]))
                    {
                        try
                        {
                            Helpers.DestroyAppropriate(comps[i]);
                            log += Strings.Log.success;
                            Log(log, LogType.Log, type.Name, name);
                        }
                        catch(Exception e)
                        {
                            log += Strings.Log.failed + " - " + e.Message;
                            Log(log, LogType.Exception, type.Name, name);
                        }
                    }
                    else
                    {
                        Log(Strings.Log.cantBeDestroyedPartOfPrefab, LogType.Warning, name, type.Name);
                    }
                }
            }
        }

#endregion

#region Utility Functions

        /// <summary>
        /// Not actually resets everything but backgrounnd and overlay stuff
        /// </summary>
        public void ResetEverything()
        {
            _backgroundPath = null;
            _overlayPath = null;
            bThumbnails_use_camera_background = false;
            bThumbnails_use_camera_overlay = false;
            cameraBackgroundTexture = null;
            cameraOverlayTexture = null;
            DestroyDummies();
        }

        /// <summary>
        /// Refreshes the background override setting
        /// </summary>
        public void RefreshBackgroundOverrideType()
        {
            if(bThumbnails_use_camera_background)
            {
                switch(cameraBackgroundType)
                {
                    case PumkinsCameraPreset.CameraBackgroundOverrideType.Color:
                        Color color = SelectedCamera != null ? SelectedCamera.backgroundColor : _thumbsCamBgColor;
                        SetCameraBackgroundToColor(color);
                        break;
                    case PumkinsCameraPreset.CameraBackgroundOverrideType.Image:
                        SetBackgroundToImageFromPath(_backgroundPath);
                        break;
                    case PumkinsCameraPreset.CameraBackgroundOverrideType.Skybox:
                        SetCameraBackgroundToSkybox(RenderSettings.skybox);
                        break;
                }
            }
            else
                RestoreCameraClearFlags();
        }

        /// <summary>
        /// Refreshes ignore array for the copier by making the transform references local to the selected avatar
        /// </summary>
        private void RefreshIgnoreArray()
        {
            if(_copierIgnoreArray == null)
            {
                _copierIgnoreArray = new Transform[0];
                return;
            }
            else if(_copierIgnoreArray.Length == 0)
            {
                return;
            }

            var newList = new List<Transform>(_copierIgnoreArray.Length);

            foreach(var t in _copierIgnoreArray)
            {
                if(!t)
                    newList.Add(t);

                var tt = Helpers.FindTransformInAnotherHierarchy(t, CopierSelectedFrom.transform, false);
                if(tt && !newList.Contains(tt))
                    newList.Add(tt);
            }

            _copierIgnoreArray = newList.ToArray();
        }

        /// <summary>
        /// Refreshes the chosen language in the UI. Needed for when we go into and out of play mode
        /// </summary>
        public void RefreshLanguage()
        {
            PumkinsLanguageManager.LoadTranslations();
            PumkinsLanguageManager.SetLanguage(_selectedLanguageString);
            _selectedLanguageIndex = PumkinsLanguageManager.GetIndexOfLanguage(_selectedLanguageString);
        }

        /// <summary>
        /// Refreshes the cached selected preset index of a PumkinPreset type
        /// </summary>
        public static void RefreshPresetIndex<T>() where T : PumkinPreset
        {
            Type t = typeof(T);
            Type tP = typeof(PumkinPreset);
            if(typeof(T) == typeof(PumkinsCameraPreset) || t == tP)
                RefreshPresetIndexByString<T>(Instance._selectedCameraPresetString);
            if(typeof(T) == typeof(PumkinsPosePreset) || t == tP)
                RefreshPresetIndexByString<T>(Instance._selectedPosePresetString);
            if(typeof(T) == typeof(PumkinsBlendshapePreset) || t == tP)
                RefreshPresetIndexByString<T>(Instance._selectedBlendshapePresetString);
        }

        /// <summary>
        /// Refreshes preset index by string. Used to refresh the index based on the cached string
        /// </summary>
        public static void RefreshPresetIndexByString<T>(string selectedPresetString) where T : PumkinPreset
        {
            int count = PumkinsPresetManager.GetPresetCountOfType<T>();
            int selectedPresetIndex = PumkinsPresetManager.GetPresetIndex<T>(selectedPresetString);
            selectedPresetIndex = Mathf.Clamp(selectedPresetIndex, 0, count - 1);

            if(typeof(T) == typeof(PumkinsCameraPreset))
            {
                if(count == 0)
                    selectedPresetString = "";
                else
                    selectedPresetString = PumkinsPresetManager.CameraPresets[selectedPresetIndex].ToString() ?? "";
                Instance._selectedCameraPresetIndex = selectedPresetIndex;
            }
            else if(typeof(T) == typeof(PumkinsPosePreset))
            {
                if(count == 0)
                    selectedPresetString = "";
                else
                    selectedPresetString = PumkinsPresetManager.PosePresets[selectedPresetIndex].ToString() ?? "";
                Instance._selectedPosePresetIndex = selectedPresetIndex;
            }
            else if(typeof(T) == typeof(PumkinsBlendshapePreset))
            {
                if(count == 0)
                    selectedPresetString = "";
                else
                    selectedPresetString = PumkinsPresetManager.BlendshapePresets[selectedPresetIndex].ToString() ?? "";
                Instance._selectedBlendshapePresetIndex = selectedPresetIndex;
            }
        }

        /// <summary>
        /// Refreshes the cached selected preset string by index
        /// </summary>
        public static void RefreshPresetStringByIndex<T>(int index) where T : PumkinPreset
        {
            string presetString = PumkinsPresetManager.GetPresetName<T>(index);
            if(string.IsNullOrEmpty(presetString))
            {
                index = 0;
                presetString = PumkinsPresetManager.GetPresetName<T>(0);
            }

            if(typeof(T) == typeof(PumkinsCameraPreset))
                Instance._selectedCameraPresetString = presetString;
            else if(typeof(T) == typeof(PumkinsPosePreset))
                Instance._selectedPosePresetString = presetString;
            else if(typeof(T) == typeof(PumkinsBlendshapePreset))
                Instance._selectedBlendshapePresetString = presetString;
        }

#endregion

#region Helper Functions

        /// <summary>
        /// Sets selected camera clear flags back to _thumbsCameraBgClearFlagsOld
        /// </summary>
        public void RestoreCameraClearFlags()
        {
            if(SelectedCamera)
                SelectedCamera.clearFlags = _thumbsCameraBgClearFlagsOld;
        }

        /// <summary>
        /// Used to set up CameraBackground and CameraOverlay dummies
        /// </summary>
        /// <param name="clipPlaneIsNear">Whether to set the clipping plane to be near or far</param>
        public void SetupCameraRawImageAndCanvas(GameObject dummyGameObject, ref RawImage rawImage, bool clipPlaneIsNear)
        {
            if(!dummyGameObject)
                return;

            rawImage = dummyGameObject.GetComponent<RawImage>();
            if(!rawImage)
                rawImage = dummyGameObject.AddComponent<RawImage>();
            Canvas canvas = dummyGameObject.GetComponent<Canvas>();
            if(!canvas)
                canvas = dummyGameObject.AddComponent<Canvas>();

            canvas.worldCamera = SelectedCamera;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;

            if(!SelectedCamera)
                return;

            if(clipPlaneIsNear)
                canvas.planeDistance = SelectedCamera.nearClipPlane + 0.01f;
            else
                canvas.planeDistance = SelectedCamera.farClipPlane - 2f;
        }

        /// <summary>
        /// Resets all BlendShape weights to 0 on all SkinnedMeshRenderers or to prefab values
        /// </summary>
        /// <param name="revertToPrefab">Revert weights to prefab instead of 0</param>
        public static void ResetBlendshapes(GameObject objTo, bool revertToPrefab)
        {
            var renders = objTo.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            foreach(var r in renders)
            {
                ResetRendererBlendshapes(r, revertToPrefab);
            }
        }

        /// <summary>
        /// Reset all BlendShape weights to 0 on SkinnedMeshRenderer or to prefab values
        /// </summary>
        /// <param name="revertToPrefab">Revert weights to prefab instead of 0</param>
        public static void ResetRendererBlendshapes(SkinnedMeshRenderer render, bool revertToPrefab)
        {
            GameObject pref = null;
            SkinnedMeshRenderer prefRender = null;

            if(!revertToPrefab)
            {
                for(int i = 0; i < render.sharedMesh.blendShapeCount; i++)
                    render.SetBlendShapeWeight(i, 0);
            }
            else
            {
                pref = PrefabUtility.GetCorrespondingObjectFromSource(render.gameObject) as GameObject;

                if(pref != null)
                    prefRender = pref.GetComponent<SkinnedMeshRenderer>();
                else
                {
                    Log(Strings.Log.meshPrefabMissingCantRevertBlednshapes, LogType.Error);
                    return;
                }

                if(render.sharedMesh.blendShapeCount == prefRender.sharedMesh.blendShapeCount)
                {
                    for(int i = 0; i < render.sharedMesh.blendShapeCount; i++)
                    {
                        render.SetBlendShapeWeight(i, prefRender.GetBlendShapeWeight(i));
                    }
                }
                else
                {
                    Dictionary<string, float> prefWeights = new Dictionary<string, float>();
                    for(int i = 0; i < prefRender.sharedMesh.blendShapeCount; i++)
                    {
                        string n = render.sharedMesh.GetBlendShapeName(i);
                        float w = render.GetBlendShapeWeight(i);
                        prefWeights.Add(n, w);
                    }

                    for(int i = 0; i < render.sharedMesh.blendShapeCount; i++)
                    {
                        string n = render.sharedMesh.GetBlendShapeName(i);
                        float w = 0;

                        if(prefWeights.ContainsKey(n))
                            w = prefWeights[n];

                        render.SetBlendShapeWeight(i, w);
                    }
                }
            }
        }

        /// <summary>
        /// Resets avatar pose to prefab values
        /// </summary>
        public static bool ResetPose(GameObject avatar)
        {
            if(!avatar)
                return false;

            var pref = PrefabUtility.GetCorrespondingObjectFromSource(avatar.transform.root.gameObject) as GameObject;

            if(!pref)
            {
                Log(Strings.Log.meshPrefabMissingCantRevertPose, LogType.Error);
                return false;
            }

            var trans = avatar.GetComponentsInChildren<Transform>(true);

            foreach(var t in trans)
            {
                if(t == t.root)
                    continue;

                string tPath = Helpers.GetGameObjectPath(t.gameObject);
                Transform tPref = pref.transform.Find(tPath);

                if(!tPref)
                    continue;

                t.localPosition = tPref.localPosition;
                t.localRotation = tPref.localRotation;
                t.localEulerAngles = tPref.localEulerAngles;
            }

            PumkinsPoseEditor.OnPoseWasChanged(PumkinsPoseEditor.PoseChangeType.Reset);
            return true;
        }

        /// <summary>
        /// Looks for child object in an object's children. Can create if not found.
        /// </summary>
        /// <param name="parent">Parent object to look in</param>
        /// <param name="child">Child object to look for in parent?</param>
        /// <param name="createIfMissing">Create GameObject if not found</param>
        GameObject GetSameChild(GameObject parent, GameObject child, bool createIfMissing = false)
        {
            if(parent == null || child == null)
                return null;

            Transform newChild = null;
            if(createIfMissing)
                newChild = parent.transform.Find(child.name, createIfMissing, parent.transform);
            else
                newChild = parent.transform.Find(child.name);

            if(newChild != null)
                return newChild.gameObject;

            return null;
        }
#if VRC_SDK_VRCSDK2 || (VRC_SDK_VRCSDK3 && !UDON)
        /// <summary>
        /// Centers camera on viewpoint and fixes the near and far clipping planes
        /// </summary>
        /// <param name="avatarOverride">Avatar to center on</param>
        /// <param name="positionOffset">Position offset to apply</param>
        /// <param name="rotationOffset">Rotation offset to apply</param>
        /// <param name="fixClippingPlanes">Should change near clipping plane to 0.1 and far to 1000?</param>
        void CenterCameraOnViewpoint(GameObject avatarOverride, Vector3 positionOffset, Vector3 rotationOffset, bool fixClippingPlanes)
        {
            if(fixClippingPlanes)
                Helpers.FixCameraClippingPlanes(SelectedCamera);
            PumkinsCameraPreset.ApplyPositionAndRotationWithViewpointFocus(avatarOverride, SelectedCamera, positionOffset, rotationOffset, true);
        }
#endif

        void CenterCameraOnTransform(Transform transform, Vector3 positionOffset, Vector3 rotationOffset, bool fixClippingPlanes)
        {
            if(fixClippingPlanes)
                Helpers.FixCameraClippingPlanes(SelectedCamera);
            PumkinsCameraPreset.ApplyPositionAndRotationWithTransformFocus(transform, SelectedCamera, positionOffset, rotationOffset, true);
        }

        /// <summary>
        /// Saves serialized variables to PlayerPrefs.
        /// Used to keep same settings when restarting unity or going into play mode
        /// </summary>
        void SavePrefs()
        {
            var data = JsonUtility.ToJson(this, false);
            EditorPrefs.SetString("PumkinToolsWindow", data);
            LogVerbose("Saved tool window preferences");
        }

        /// <summary>
        /// Loads serialized variables from PlayerPrefs.
        /// Used to keep same settings when restarting unity or going into play mode
        /// </summary>
        void LoadPrefs()
        {
            if(_loadedPrefs)
                return;

            var data = EditorPrefs.GetString("PumkinToolsWindow", JsonUtility.ToJson(this, false));
            if(data != null)
            {
                JsonUtility.FromJsonOverwrite(data, this);
                RefreshLanguage();
                LogVerbose("Loaded tool window preferences");
            }
            else
            {
                LogVerbose("Failed to load window preferences");
            }
            RefreshBackgroundOverrideType();
            _loadedPrefs = true;
        }

        /// <summary>
        /// Logs a message to console with a red PumkinsAvatarTools: prefix. Only if verbose logging is enabled.
        /// </summary>
        /// <param name="logFormat">Same as string.format()</param>
        public static void LogVerbose(string message, LogType logType = LogType.Log, params string[] logFormat)
        {
            if(!Instance.verboseLoggingEnabled)
                return;

            if(logFormat.Length > 0)
                message = string.Format(message, logFormat);
            message = "<color=red>PumkinsAvatarTools</color>: " + message;

            switch(logType)
            {
                case LogType.Error:
                    Debug.Log(message);
                    break;
                case LogType.Warning:
                    Debug.LogWarning(message);
                    break;
                case LogType.Exception:
                    Debug.LogException(new Exception(message));
                    break;
                case LogType.Assert:
                    Debug.LogAssertion(message);
                    break;
                default:
                    Debug.Log(message);
                    break;
            }
        }

        /// <summary>
        /// Logs a message to console with a blue PumkinsAvatarTools: prefix.
        /// </summary>
        /// <param name="logFormat">Same as string.format</param>
        public static void Log(string message, LogType logType = LogType.Log, params string[] logFormat)
        {
            string msg = message;
            try
            {
                if(logFormat.Length > 0)
                    message = string.Format(message, logFormat);
                message = "<color=blue>PumkinsAvatarTools</color>: " + message;
            }
            catch
            {
                message = msg;
                logType = LogType.Warning;
            }
            switch(logType)
            {
                case LogType.Error:
                    Debug.LogError(message);
                    break;
                case LogType.Warning:
                    Debug.LogWarning(message);
                    break;
                case LogType.Exception:
                    Debug.LogException(new Exception(message));
                    break;
                case LogType.Assert:
                    Debug.LogAssertion(message);
                    break;
                default:
                    Debug.Log(message);
                    break;
            }
        }

#endregion
    }
}
