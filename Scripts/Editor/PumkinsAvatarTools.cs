using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using System;
using System.IO;
using UnityEngine;
using System.Linq;
using Pumkin.AvatarTools.Callbacks;
using Pumkin.AvatarTools.Copiers;
using Pumkin.AvatarTools.Destroyers;
using Pumkin.DependencyChecker;
using Pumkin.PoseEditor;
using UnityEngine.UI;
using Pumkin.DataStructures;
using Pumkin.HelperFunctions;
using Pumkin.Extensions;
using UnityEngine.SceneManagement;
using Pumkin.Presets;
using UnityEngine.Animations;
using Pumkin.YAML;
using UnityEditor.Experimental.SceneManagement;

#if VRC_SDK_VRCSDK2 || VRC_SDK_VRCSDK3
using VRC.Core;
using VRC.SDKBase;
#endif
#if PUMKIN_PBONES
using VRC.SDK3.Dynamics.PhysBone.Components;
using VRC.SDK3.Dynamics.Contact.Components;
#endif
#if VRC_SDK_VRCSDK3 && !UDON
using VRC_AvatarDescriptor = VRC.SDK3.Avatars.Components.VRCAvatarDescriptor;
using VRC_SpatialAudioSource = VRC.SDK3.Avatars.Components.VRCSpatialAudioSource;
#elif VRC_SDK_VRCSDK2
using VRC_AvatarDescriptor = VRCSDK2.VRC_AvatarDescriptor;
using VRC_SpatialAudioSource = VRCSDK2.VRC_SpatialAudioSource;
#endif

#if PUMKIN_FINALIK
using RootMotion.FinalIK;
#endif

namespace Pumkin.AvatarTools
{
    /// <summary>
    /// PumkinsAvatarTools by, well, Pumkin
    /// https://github.com/rurre/PumkinsAvatarTools
    /// </summary>
    [ExecuteInEditMode, CanEditMultipleObjects] // TODO: Check if this is still needed. Rider says it's not
    public class PumkinsAvatarTools : EditorWindow
    {
        #region Variables

        #region Tools

        SettingsContainer _settings;

        internal static SettingsContainer Settings
        {
            get
            {
                if(Instance._settings == null)
                    Instance._settings = CreateInstance<SettingsContainer>();
                return Instance._settings;
            }
        }

        PumkinTool[] newTools =
        {
            new RecalculateBoundsTool()
        };

        //Editing Viewpoint
        bool _editingView = false;
        Vector3 _viewPosOld;
        Vector3 _viewPosTemp;
        Tool _tempToolOld = Tool.None;
        public static readonly Vector3 DEFAULT_VIEWPOINT = new Vector3(0, 1.6f, 0.2f);

        //Editing Scale
        bool _editingScale = false;
        Vector3 _avatarScaleOld;

        Transform _scaleViewpointDummy;

#if VRC_SDK_VRCSDK2 || (VRC_SDK_VRCSDK3 && !UDON)
        VRC_AvatarDescriptor _tempAvatarDescriptor;
        bool _tempAvatarDescriptorWasAdded = false;
#endif
        //Phys Bones
        bool _nextTogglePBoneState = false;
#if PUMKIN_PBONES
        List<VRCPhysBone> _pBonesThatWereAlreadyDisabled = new List<VRCPhysBone>();
#else
        List<object> _pBonesThatWereAlreadyDisabled = new List<object>();
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
            RemovePhysBones,
            RemovePhysBoneColliders,
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
            RemoveContactReceiver,
            RemoveContactSender,
            RemoveLookAtConstraint,
            RemoveParentConstraint,
            RemoveRotationConstraint,
            RemoveAimConstraint,
            RemoveScaleConstraint,
            RemovePositionConstraint,
            FixDynamicBoneScripts,
            FillEyeBones,
            ResetBoundingBoxes,
            SetImportSettings,
            RemoveCameras,
            RemoveFinalIK_CCDIK,
            RemoveFinalIK_LimbIK,
            RemoveFinalIK_RotationLimits,
            RemoveFinalIK_FabrIK,
            RemoveFinalIK_AimIK,
            RemoveFinalIK_FbtBipedIK,
            RemoveFinalIK_VRIK
        }

        readonly static string SCALE_RULER_DUMMY_NAME = "_PumkinsScaleRuler";

#endregion

#region Component Copier

        bool _copierCheckedArmatureScales = false;
        bool _copierShowArmatureScaleWarning = false;

#endregion


#endregion

#region Thumbnails

        GameObject _cameraOverlay = null;
        GameObject _cameraBackground = null;
        RawImage _cameraOverlayImage = null;
        RawImage _cameraBackgroundImage = null;

        public enum PresetToolbarOptions { Camera, Pose, Blendshape }


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

        public PumkinsCameraPreset.CameraBackgroundOverrideType cameraBackgroundType = PumkinsCameraPreset.CameraBackgroundOverrideType.Color;

        static readonly string CAMERA_OVERLAY_NAME = "_PumkinsCameraOverlay";
        static readonly string CAMERA_BACKGROUND_NAME = "_PumkinsCameraBackground";

        static List<PumkinsRendererBlendshapesHolder> _selectedAvatarRendererHolders;

#endregion

#region Fallback Preview

        //TODO: Improve and revert #if
#if PUMKIN_DEV
        PumkinsFallbackMaterialPreview PumkinsFallbackPreview { get; set; } = new PumkinsFallbackMaterialPreview();
#endif
        
#endregion

#region Avatar Info

        static PumkinsAvatarInfo avatarInfo = new PumkinsAvatarInfo();
        static string _avatarInfoSpace = "\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n";
        static string _avatarInfoString = Strings.AvatarInfo.selectAvatarFirst + _avatarInfoSpace; //Please don't hurt me for this

#endregion

#region Misc

        //Misc
        readonly float COPIER_SETTINGS_INDENT_SIZE = 38f;

        SerializedObject _serializedScript;
        GameObject oldSelectedAvatar = null;

        const string DUMMY_NAME = "_DUMMY";
        const string VIEWPOINT_DUMMY_NAME = "_VIEWPOINT_DUMMY";


        static string _mainScriptPath = null;
        static string _mainFolderPath = null;
        static string _resourceFolderPath = null;
        static string _mainFolderPathLocal = null;
        static string _mainScriptPathLocal = null;
        static string _resourceFolderPathLocal = null;

        static bool _eventsAdded = false;
        static bool _loadedPrefs = false;

#endregion

#region Properties

        public static PumkinsAvatarTools Instance => _PumkinsAvatarToolsWindow.ToolsWindow;

        public static GameObject SelectedAvatar
        {
            get => SettingsContainer._selectedAvatar;
            set
            {
                if(value != SettingsContainer._selectedAvatar)
                {
                    SettingsContainer._selectedAvatar = value;
                    OnAvatarSelectionChanged(SettingsContainer._selectedAvatar);
                }
            }
        }

        public static bool SelectedAvatarIsHuman
        {
            get
            {
                if(!SettingsContainer._selectedAvatar)
                    return false;
                Animator anim = SettingsContainer._selectedAvatar.GetComponent<Animator>();
                if(!anim || !anim.isHuman)
                    return false;

                return true;
            }
        }

        public static GameObject CopierSelectedFrom
        {
            get => SettingsContainer._copierSelectedFrom;

            private set
            {
                SettingsContainer._copierSelectedFrom = value;
            }
        }

        public bool CopierHasSelections
        {
            get
            {
                if(Settings._copier_selectedTab < CopierTabs.Tab.All)
                {
                    bool[] commonToggles =
                    {
                        Settings.bCopier_descriptor_copy,
                        Settings.bCopier_skinMeshRender_copy,
                        Settings.bCopier_physBones_copy,
                        Settings.bCopier_physBones_copyColliders,
                        Settings.bCopier_dynamicBones_copy,
                        Settings.bCopier_dynamicBones_copyColliders,
                        Settings.bCopier_meshRenderers_copy,
                        Settings.bCopier_particleSystems_copy,
                        Settings.bCopier_trailRenderers_copy,
                        Settings.bCopier_audioSources_copy,
                        Settings.bCopier_lights_copy,
                    };
                    if(commonToggles.Any(b => b))
                        return true;
                }
                else
                {
                    bool[] allToggles =
                    {
                        Settings.bCopier_transforms_copy,
                        Settings.bCopier_animators_copy,
                        Settings.bCopier_colliders_copy,
                        Settings.bCopier_joints_copy,
                        Settings.bCopier_descriptor_copy,
                        Settings.bCopier_meshRenderers_copy,
                        Settings.bCopier_particleSystems_copy,
                        Settings.bCopier_rigidBodies_copy,
                        Settings.bCopier_trailRenderers_copy,
                        Settings.bCopier_lights_copy,
                        Settings.bCopier_skinMeshRender_copy,
                        Settings.bCopier_physBones_copy,
                        Settings.bCopier_physBones_copyColliders,
                        Settings.bCopier_dynamicBones_copy,
                        Settings.bCopier_dynamicBones_copyColliders,
                        Settings.bCopier_audioSources_copy,
                        Settings.bCopier_aimConstraint_copy,
                        Settings.bCopier_lookAtConstraint_copy,
                        Settings.bCopier_parentConstraint_copy,
                        Settings.bCopier_positionConstraint_copy,
                        Settings.bCopier_rotationConstraint_copy,
                        Settings.bCopier_scaleConstraint_copy,
                        Settings.bCopier_cameras_copy,

                        (Settings.bCopier_other_copy && (Settings.bCopier_other_copyIKFollowers || Settings.bCopier_other_copyVRMSpringBones)),
                        
#if PUMKIN_FINALIK
                        Settings.bCopier_finalIK_copy,
#endif
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

            private set => _mainScriptPath = value;
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

            private set => _mainFolderPath = value;
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

                if(!string.IsNullOrEmpty(Settings._backgroundPath))
                    SetBackgroundToImageFromPath(Settings._backgroundPath);
            }
            return _cameraBackgroundImage;
        }

        public SerializedProperty SerializedIgnoreArray
        {
            get
            {
                if(Settings.SerializedSettings != null && Settings._serializedIgnoreArrayProp == null)
                    Settings._serializedIgnoreArrayProp = Settings.SerializedSettings.FindProperty("_copierIgnoreArray");
                return Settings._serializedIgnoreArrayProp;
            }

            private set => Settings._serializedIgnoreArrayProp = value;
        }

        public SerializedProperty SerializedScaleTemp
        {
            get
            {
                if(Settings.SerializedSettings != null && Settings._serializedAvatarScaleTempProp == null)
                    Settings._serializedAvatarScaleTempProp = Settings.SerializedSettings.FindProperty("_avatarScaleTemp");
                return Settings._serializedAvatarScaleTempProp;
            }
            private set => Settings._serializedAvatarScaleTempProp = value;
        }

        public SerializedProperty SerializedHumanPoseMuscles
        {
            get
            {
                if(Settings._tempHumanPoseMuscles == null)
                    Settings._tempHumanPoseMuscles = Settings._tempHumanPose.muscles;

                if(Settings.SerializedSettings != null && Settings._serializedTempHumanPoseMuscles == null)
                {
                    Settings._serializedTempHumanPoseMuscles = Settings.SerializedSettings.FindProperty("_tempHumanPoseMuscles");
                }
                return Settings._serializedTempHumanPoseMuscles;
            }
            private set => Settings._serializedTempHumanPoseMuscles = value;
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
                if(_dynamicBonesExist == null)
                    _dynamicBonesExist = PumkinsTypeCache.DynamicBone != null && PumkinsTypeCache.DynamicBoneCollider != null;
                return (bool)_dynamicBonesExist;
            }
        }

        private bool? _dynamicBonesExist;

        public bool PhysBonesExist
        {
            get
            {
                if(_physBonesExist == null)
                    _physBonesExist = PumkinsTypeCache.ContactReceiver != null && PumkinsTypeCache.ContactSender != null && PumkinsTypeCache.PhysBone != null && PumkinsTypeCache.PhysBoneCollider != null;
                return (bool)_physBonesExist;
            }
        }

		public bool FinalIKExists
		{
			get
			{
				if(_finalIKExists == null)
					_finalIKExists = PumkinsTypeCache.AimIK != null && PumkinsTypeCache.FABRIK != null && PumkinsTypeCache.FullBodyBipedIK != null && PumkinsTypeCache.RotationLimit != null;
				return (bool)_finalIKExists;
			}
		}

		private bool? _finalIKExists;

        private bool? _physBonesExist;

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

            Settings.centerCameraTransform = null;

            Instance._copierCheckedArmatureScales = false;

            Instance._nextTogglePBoneState = false;
#if Has_Dynamics
            Instance._pBonesThatWereAlreadyDisabled = new List<VRCPhysBone>();
#endif
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

            SceneView.duringSceneGui += OnSceneGUI;
            EditorApplication.update += updateCallback;
            if(!_eventsAdded) //Not sure if this is necessary anymore
            {
                EditorApplication.playModeStateChanged += HandlePlayModeStateChange;
                Selection.selectionChanged += HandleSelectionChanged;
                EditorSceneManager.sceneOpened += HandleSceneChange;

                PrefabStage.prefabStageOpened += HandlePrefabStageOpened;
                PrefabStage.prefabStageClosing += HandlePrefabStageClosed;

                _eventsAdded = true;
            }

            SerializedIgnoreArray = Settings.SerializedSettings.FindProperty("_copierIgnoreArray");
            SerializedScaleTemp = Settings.SerializedSettings.FindProperty("_avatarScaleTemp");
            SerializedHumanPoseMuscles = Settings.SerializedSettings.FindProperty("_tempHumanPoseMuscles");

            _emptyTexture = new Texture2D(2, 2);
            cameraOverlayTexture = new Texture2D(2, 2);
            cameraBackgroundTexture = new Texture2D(2, 2);

            LoadPrefs();

#if VRC_SDK_VRCSDK2 || (VRC_SDK_VRCSDK3 && !UDON)
            AvatarUploadHider.Enabled = Settings.shouldHideOtherAvatars;
#endif
            RestoreTexturesFromPaths();
            RefreshBackgroundOverrideType();

            DestroyDummies();

            SelectedCamera = GetVRCCamOrMainCam();

            if(Settings._lastOpenFilePath == default(string))
                Settings._lastOpenFilePath = MainFolderPath + PumkinsPresetManager.resourceCamerasPath + "/Example Images";

            Settings.lockSelectedCameraToSceneView = false;
        }

        public void HandleSceneChange(Scene scene, OpenSceneMode mode)
        {
            if(mode == OpenSceneMode.Single)
                RefreshLanguage();
        }

        public void HandleOnDisable()
        {
            LogVerbose("Tools window: OnDisable()");
            SceneView.duringSceneGui -= OnSceneGUI;
            EditorApplication.update -= updateCallback;
            Selection.selectionChanged -= HandleSelectionChanged;
            EditorApplication.playModeStateChanged -= HandlePlayModeStateChange;
            EditorSceneManager.sceneOpened -= HandleSceneChange;

            PrefabStage.prefabStageOpened -= HandlePrefabStageOpened;
            PrefabStage.prefabStageClosing -= HandlePrefabStageClosed;

            _eventsAdded = false;

            if(Settings.SerializedSettings != null)
                Settings.SerializedSettings.ApplyModifiedPropertiesWithoutUndo();
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
                //Find the vrc ui camera and set it's depth higher than default to make sure it's the one that's rendering
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
            if(SettingsContainer._useSceneSelectionAvatar)
                SelectAvatarFromScene();
            _PumkinsAvatarToolsWindow.RequestRepaint(this);
        }

        void HandlePrefabStageOpened(PrefabStage stage)
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
            Settings.SerializedSettings.Update();

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField(Strings.Main.title, Styles.Label_mainTitle);

                if(GUILayout.Button(Icons.Settings, Styles.IconButton))
                    Settings._openedSettings = !Settings._openedSettings;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField(Strings.Credits.version);

            if(Settings._openedSettings)
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
            if(Settings.lockSelectedCameraToSceneView && _selectedCamera)
                SelectedCamera.transform.SetPositionAndRotation(SceneView.lastActiveSceneView.camera.transform.position, SceneView.lastActiveSceneView.camera.transform.rotation);

            //Draw camera overlay
            GameObject overlay = GetCameraOverlay(true);
            if(Settings.bThumbnails_use_camera_overlay)
            {
                var raw = GetCameraOverlayRawImage(true);
                if(!raw.texture && !string.IsNullOrEmpty(Settings._overlayPath))
                    SetOverlayToImageFromPath(Settings._overlayPath);

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
            if(Settings.bThumbnails_use_camera_background)
            {
                var raw = GetCameraBackgroundRawImage(true);
                if(cameraBackgroundType == PumkinsCameraPreset.CameraBackgroundOverrideType.Image)
                {
                    if(!raw.texture && !string.IsNullOrEmpty(Settings._backgroundPath))
                        SetBackgroundToImageFromPath(Settings._backgroundPath);
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
                    if(Settings._selectedLanguageIndex >= PumkinsLanguageManager.Languages.Count)
                        Settings._selectedLanguageIndex = PumkinsLanguageManager.GetIndexOfLanguage(Settings._selectedLanguageString);

                    Settings._selectedLanguageIndex = EditorGUILayout.Popup(Strings.Settings.language, Settings._selectedLanguageIndex, PumkinsLanguageManager.Languages.Select(o => o.ToString()).ToArray(), Styles.Popup);
                }
                if(EditorGUI.EndChangeCheck() && PumkinsLanguageManager.Languages.Count > 1)
                {
                    PumkinsLanguageManager.SetLanguage(PumkinsLanguageManager.Languages[Settings._selectedLanguageIndex]);
                    Settings._selectedLanguageString = Strings.Translation.ToString();
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

            Settings.handlesUiWindowPositionAtBottom = GUILayout.Toggle(Settings.handlesUiWindowPositionAtBottom, Strings.Settings.sceneViewOverlayWindowsAtBottom);

            EditorGUILayout.Space();
            Settings.verboseLoggingEnabled = GUILayout.Toggle(Settings.verboseLoggingEnabled, Strings.Settings.enableVerboseLogging);
            
            EditorGUILayout.Space();
            
            //TODO: Improve fallback and restore
#if PUMKIN_DEV
            Helpers.DrawGUILine();
            EditorGUILayout.HelpBox(Strings.Settings.experimentalWarning, MessageType.Warning);
            Settings.showExperimental = GUILayout.Toggle(Settings.showExperimental, Strings.Settings.showExperimental);           
#endif

            EditorGUILayout.Space();
#if PUMKIN_DEV
            if(GUILayout.Button("Generate Thry Manifest"))
            {
                ThryModuleManifest.Generate();
            }
#endif

            GUILayout.FlexibleSpace();

            if(GUILayout.Button(Strings.Settings.uwu, "label", GUILayout.ExpandWidth(false)))
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

            Settings._mainToolsScrollbar = EditorGUILayout.BeginScrollView(Settings._mainToolsScrollbar);
            {
                DrawToolsMenuGUI();

                EditorGUILayout.Space();

                DrawCopierMenuGUI();

                EditorGUILayout.Space();

                DrawRemoveComponentsMenuGUI();
                
                EditorGUILayout.Space();

                DrawAvatarInfoMenuGUI();

#if VRC_SDK_VRCSDK2 || VRC_SDK_VRCSDK3

                if(Settings.showExperimental)
                {
                    EditorGUILayout.Space();
                    DrawAvatarTestingMenuGUI();
                }

                EditorGUILayout.Space();
#endif
                DrawThumbnailsMenuGUI();

                EditorGUILayout.Space();

                DrawInfoMenuGUI();                

                Helpers.DrawGUILine();
            }
            EditorGUILayout.EndScrollView();

            if(GUI.changed)
            {
                Settings.SerializedSettings.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(Settings);
            }
        }

        private void DrawExperimentalMenuGUI()
        {
            if(Settings._experimental_expand = GUILayout.Toggle(Settings._experimental_expand, Strings.Main.experimental, Styles.Foldout_title))
            {
                EditorGUILayout.Space();

                //EditorGUI.BeginDisabledGroup(!DynamicBonesExist || !SelectedAvatar);
                //{
                //  if(GUILayout.Button(Strings.Tools.fixDynamicBoneScripts, Styles.BigButton))
                //      DoAction(SelectedAvatar, ToolMenuActions.FixDynamicBoneScripts);
                //}
                //EditorGUI.EndDisabledGroup();
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
            if(Settings.handlesUiWindowPositionAtBottom)
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
                        _viewPosTemp = Helpers.GetViewpointAtEyeLevel(SelectedAvatar.GetComponent<Animator>()) + SelectedAvatar.transform.position;
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
            if(Settings.handlesUiWindowPositionAtBottom)
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
                        if(Settings.SerializedSettings.ApplyModifiedPropertiesWithoutUndo())
                            propertyChanged = true;
                    }
                    else
                    {
                        EditorGUILayout.LabelField(Settings._avatarScaleTemp.ToString());
                    }
#if VRC_SDK_VRCSDK2 || (VRC_SDK_VRCSDK3 && !UDON)
                    Settings.editingScaleMovesViewpoint = GUILayout.Toggle(Settings.editingScaleMovesViewpoint, Strings.Tools.editScaleMoveViewpoint);
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
                    Settings._avatarScaleTemp = Handles.ScaleSlider(Settings._avatarScaleTemp, SelectedAvatar.transform.position, Vector3.up, Quaternion.identity, HandleUtility.GetHandleSize(SelectedAvatar.transform.position) * 2, 0.01f);
                }
                if(EditorGUI.EndChangeCheck() || propertyChanged)
                {
                    SetAvatarScaleAndMoveViewpoint(_tempAvatarDescriptor, Settings._avatarScaleTemp);
                }

                if(Settings.editingScaleMovesViewpoint)
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
                Settings._avatarScaleTemp = Handles.ScaleSlider(Settings._avatarScaleTemp, SelectedAvatar.transform.position, Vector3.up, Quaternion.identity, HandleUtility.GetHandleSize(SelectedAvatar.transform.position) * 2, 0.01f);
            }
            if(EditorGUI.EndChangeCheck() || propertyChanged)
            {
                SetAvatarScale(Settings._avatarScaleTemp);
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

            if(SettingsContainer._useSceneSelectionAvatar)
                if(Selection.activeObject != SelectedAvatar)
                    SelectAvatarFromScene();

            if(showSelectFromSceneButton)
                if(GUILayout.Button(Strings.Buttons.selectFromScene))
                    if(Selection.activeObject)
                        SelectAvatarFromScene();

            if(showSceneSelectionCheckBox) SettingsContainer._useSceneSelectionAvatar = GUILayout.Toggle(SettingsContainer._useSceneSelectionAvatar, Strings.Main.useSceneSelection);
        }

        void DrawCopierMenuGUI()
        {
            if(Settings._copier_expand = GUILayout.Toggle(Settings._copier_expand, Strings.Main.copier, Styles.Foldout_title))
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
                    Settings._copier_selectedTab = (CopierTabs.Tab)GUILayout.Toolbar((int)Settings._copier_selectedTab, toolbarContent);

                    Helpers.DrawGUILine(1, false);

                    if(CopierTabs.ComponentIsInSelectedTab("vrcavatardescriptor", Settings._copier_selectedTab))
                    {
                        //AvatarDescriptor menu
                        Helpers.DrawDropdownWithToggle(ref Settings._copier_expand_avatarDescriptor, ref Settings.bCopier_descriptor_copy, Strings.Copier.descriptor, Icons.Avatar);
                        if(Settings._copier_expand_avatarDescriptor)
                        {
                            EditorGUI.BeginDisabledGroup(!Settings.bCopier_descriptor_copy);
                            EditorGUILayout.Space();

                            using(var cHorizontalScope = new GUILayout.HorizontalScope())
                            {
                                GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE); // horizontal indent size

                                using(var cVerticalScope = new GUILayout.VerticalScope())
                                {
#if VRC_SDK_VRCSDK2 || (VRC_SDK_VRCSDK3 && !UDON)
                                    Settings.bCopier_descriptor_copySettings = GUILayout.Toggle(Settings.bCopier_descriptor_copySettings, Strings.Copier.copySettings, Styles.CopierToggle);
                                    Settings.bCopier_descriptor_copyViewpoint = GUILayout.Toggle(Settings.bCopier_descriptor_copyViewpoint, Strings.Copier.descriptor_copyViewpoint, Styles.CopierToggle);
#endif
                                    Settings.bCopier_descriptor_copyAvatarScale = GUILayout.Toggle(Settings.bCopier_descriptor_copyAvatarScale, Strings.Copier.transforms_avatarScale, Styles.CopierToggle);

#if VRC_SDK_VRCSDK2 || (VRC_SDK_VRCSDK3 && !UDON)
                                    EditorGUILayout.Space();

                                    Settings.bCopier_descriptor_copyPlayableLayers = GUILayout.Toggle(Settings.bCopier_descriptor_copyPlayableLayers, Strings.Copier.descriptor_playableLayers, Styles.CopierToggle);
                                    Settings.bCopier_descriptor_copyEyeLookSettings = GUILayout.Toggle(Settings.bCopier_descriptor_copyEyeLookSettings, Strings.Copier.descriptor_eyeLookSettings, Styles.CopierToggle);
                                    Settings.bCopier_descriptor_copyExpressions = GUILayout.Toggle(Settings.bCopier_descriptor_copyExpressions, Strings.Copier.descriptor_expressions, Styles.CopierToggle);
#endif
#if PUMKIN_PBONES
                                    Settings.bCopier_descriptor_copyColliders = GUILayout.Toggle(Settings.bCopier_descriptor_copyColliders, Strings.Copier.descriptor_colliders, Styles.CopierToggle);
#endif
                                    EditorGUILayout.Space();

#if VRC_SDK_VRCSDK2 || (VRC_SDK_VRCSDK3 && !UDON)
                                    Settings.bCopier_descriptor_copyPipelineId = GUILayout.Toggle(Settings.bCopier_descriptor_copyPipelineId, Strings.Copier.descriptor_pipelineId, Styles.CopierToggle);
#endif
                                }
                            }

                            EditorGUILayout.Space();
                            EditorGUI.EndDisabledGroup();
                        }
                        Helpers.DrawGUILine(1, false);
                    }
                    if(CopierTabs.ComponentIsInSelectedTab("physbone", Settings._copier_selectedTab))
                    {
                        //PhysBones menu
                        if(!PhysBonesExist)
                        {
                            EditorGUI.BeginDisabledGroup(true);
                            {
                                Helpers.DrawDropdownWithToggle(ref Settings._copier_expand_physBones, ref Settings.bCopier_physBones_copy, "Phys Bones | SDK version 2022.03.04.12.28 or newer required", Icons.PhysBone);
                                Settings.bCopier_physBones_copy = false;
                                Settings._copier_expand_physBones = false;
                            }
                            EditorGUI.EndDisabledGroup();
                        }
                        else
                        {
#if PUMKIN_PBONES
                        Helpers.DrawDropdownWithToggle(ref Settings._copier_expand_physBones, ref Settings.bCopier_physBones_copy, Strings.Copier.physBones, Icons.PhysBone);
#endif
                        }
                        if(Settings._copier_expand_physBones)
                        {

                            EditorGUI.BeginDisabledGroup(!Settings.bCopier_physBones_copy);
                            EditorGUILayout.Space();

                            using (var cHorizontalScope = new GUILayout.HorizontalScope())
                            {
                                GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE); // horizontal indent size

                                using (var cVerticalScope = new GUILayout.VerticalScope())
                                {
                                    Settings.bCopier_physBones_createObjects = GUILayout.Toggle(Settings.bCopier_physBones_createObjects, Strings.Copier.copyGameObjects, Styles.CopierToggle);
                                    Settings.bCopier_physBones_removeOldBones = GUILayout.Toggle(Settings.bCopier_physBones_removeOldBones, Strings.Copier.physBones_removeOldBones, Styles.CopierToggle);
                                    Settings.bCopier_physBones_adjustScale = GUILayout.Toggle(Settings.bCopier_physBones_adjustScale, Strings.Copier.physBones_adjustScale, Styles.CopierToggle);
                                }
                            }

                            EditorGUILayout.Space();
                            EditorGUI.EndDisabledGroup();
                        }

                        Helpers.DrawGUILine(1, false);
                    }

                    if(CopierTabs.ComponentIsInSelectedTab("physbonecollider", Settings._copier_selectedTab))
                    {
                        //Phys Bone Colliders menu
                        if(!PhysBonesExist)
                        {
                            EditorGUI.BeginDisabledGroup(true);
                            {
                                Helpers.DrawDropdownWithToggle(ref Settings._copier_expand_physBoneColliders, ref Settings.bCopier_physBones_copyColliders, "Phys Bones Colliders | SDK version 2022.03.04.12.28 or newer required", Icons.PhysBoneCollider);
                                Settings.bCopier_physBones_copyColliders = false;
                                Settings._copier_expand_physBoneColliders = false;
                            }
                            EditorGUI.EndDisabledGroup();
                        }
                        else
                        {
#if PUMKIN_PBONES
                        Helpers.DrawDropdownWithToggle(ref Settings._copier_expand_physBoneColliders, ref Settings.bCopier_physBones_copyColliders, Strings.Copier.physBones_colliders, Icons.PhysBoneCollider);
#endif
                        }
                        if(Settings._copier_expand_physBoneColliders)
                        {
                            EditorGUI.BeginDisabledGroup(!Settings.bCopier_physBones_copyColliders);
                            EditorGUILayout.Space();

                            using (var cHorizontalScope = new GUILayout.HorizontalScope())
                            {
                                GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE); // horizontal indent size

                                using (var cVerticalScope = new GUILayout.VerticalScope())
                                {
                                    Settings.bCopier_physBones_removeOldColliders = GUILayout.Toggle(Settings.bCopier_physBones_removeOldColliders, Strings.Copier.physBones_removeOldColliders, Styles.CopierToggle);
                                    Settings.bCopier_physBones_createObjectsColliders = GUILayout.Toggle(Settings.bCopier_physBones_createObjectsColliders, Strings.Copier.copyGameObjects, Styles.CopierToggle);
                                    Settings.bCopier_physBones_adjustScaleColliders = GUILayout.Toggle(Settings.bCopier_physBones_adjustScaleColliders, Strings.Copier.physBones_adjustScaleColliders, Styles.CopierToggle);
                                }
                            }

                            EditorGUILayout.Space();
                            EditorGUI.EndDisabledGroup();
                        }

                        Helpers.DrawGUILine(1, false);
                    }

                    if(CopierTabs.ComponentIsInSelectedTab("dynamicbone", Settings._copier_selectedTab))
                    {
                        //DynamicBones menu
                        if(!DynamicBonesExist)
                        {
                            EditorGUI.BeginDisabledGroup(true);
                            {
                                Helpers.DrawDropdownWithToggle(ref Settings._copier_expand_dynamicBones, ref Settings.bCopier_dynamicBones_copy, Strings.Copier.dynamicBones + " (" + Strings.Warning.notFound + ")", Icons.BoneIcon);
                                Settings.bCopier_dynamicBones_copy = false;
                                Settings._copier_expand_dynamicBones = false;
                            }
                            EditorGUI.EndDisabledGroup();
                        }
                        else
                        {
#if PUMKIN_OLD_DBONES
                        Helpers.DrawDropdownWithToggle(ref _copier_expand_dynamicBones, ref bCopier_dynamicBones_copy, Strings.Copier.dynamicBones + " (" + Strings.Warning.oldVersion + ")", Icons.BoneIcon);
#elif PUMKIN_DBONES
                        Helpers.DrawDropdownWithToggle(ref Settings._copier_expand_dynamicBones, ref Settings.bCopier_dynamicBones_copy, Strings.Copier.dynamicBones, Icons.BoneIcon);
#endif
                        }

                        if(Settings._copier_expand_dynamicBones)
                        {
                            EditorGUI.BeginDisabledGroup(!Settings.bCopier_dynamicBones_copy);
                            EditorGUILayout.Space();

                            using(var cHorizontalScope = new GUILayout.HorizontalScope())
                            {
                                GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE); // horizontal indent size

                                using(var cVerticalScope = new GUILayout.VerticalScope())
                                {
                                    Settings.bCopier_dynamicBones_copySettings = GUILayout.Toggle(Settings.bCopier_dynamicBones_copySettings, Strings.Copier.copySettings, Styles.CopierToggle);
                                    Settings.bCopier_dynamicBones_createMissing = GUILayout.Toggle(Settings.bCopier_dynamicBones_createMissing, Strings.Copier.dynamicBones_createMissing, Styles.CopierToggle);
                                    Settings.bCopier_dynamicBones_createObjects = GUILayout.Toggle(Settings.bCopier_dynamicBones_createObjects, Strings.Copier.copyGameObjects, Styles.CopierToggle);
                                    Settings.bCopier_dynamicBones_removeOldBones = GUILayout.Toggle(Settings.bCopier_dynamicBones_removeOldBones, Strings.Copier.dynamicBones_removeOldBones, Styles.CopierToggle);
                                    Settings.bCopier_dynamicBones_adjustScale = GUILayout.Toggle(Settings.bCopier_dynamicBones_adjustScale, Strings.Copier.dynamicBones_adjustScale, Styles.CopierToggle);
                                }
                            }

                            EditorGUILayout.Space();
                            EditorGUI.EndDisabledGroup();
                        }

                        Helpers.DrawGUILine(1, false);
                    }

                    if(CopierTabs.ComponentIsInSelectedTab("dynamicbonecollider", Settings._copier_selectedTab))
                    {
                        //Dynamic Bone Colliders menu
                        if(!DynamicBonesExist)
                        {
                            EditorGUI.BeginDisabledGroup(true);
                            {
                                Helpers.DrawDropdownWithToggle(ref Settings._copier_expand_dynamicBoneColliders, ref Settings.bCopier_dynamicBones_copyColliders, Strings.Copier.dynamicBones_colliders + " (" + Strings.Warning.notFound + ")", Icons.BoneColliderIcon);
                                Settings.bCopier_dynamicBones_copyColliders = false;
                                Settings._copier_expand_dynamicBoneColliders = false;
                            }
                            EditorGUI.EndDisabledGroup();
                        }
                        else
                        {
#if PUMKIN_OLD_DBONES
                        Helpers.DrawDropdownWithToggle(ref settings._copier_expand_dynamicBoneColliders, ref settings.bCopier_dynamicBones_copyColliders, Strings.Copier.dynamicBones_colliders + " (" + Strings.Warning.oldVersion + ")", Icons.BoneColliderIcon);
#elif PUMKIN_DBONES
                        Helpers.DrawDropdownWithToggle(ref Settings._copier_expand_dynamicBoneColliders, ref Settings.bCopier_dynamicBones_copyColliders, Strings.Copier.dynamicBones_colliders, Icons.BoneColliderIcon);
#endif
                        }

                        if(Settings._copier_expand_dynamicBoneColliders)
                        {
                            EditorGUI.BeginDisabledGroup(!Settings.bCopier_dynamicBones_copyColliders);
                            EditorGUILayout.Space();

                            using(var cHorizontalScope = new GUILayout.HorizontalScope())
                            {
                                GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE); // horizontal indent size

                                using(var cVerticalScope = new GUILayout.VerticalScope())
                                {
                                    Settings.bCopier_dynamicBones_removeOldColliders = GUILayout.Toggle(Settings.bCopier_dynamicBones_removeOldColliders, Strings.Copier.dynamicBones_removeOldColliders, Styles.CopierToggle);
                                    Settings.bCopier_dynamicBones_createObjectsColliders = GUILayout.Toggle(Settings.bCopier_dynamicBones_createObjectsColliders, Strings.Copier.copyGameObjects, Styles.CopierToggle);
                                    Settings.bCopier_dynamicBones_adjustScaleColliders = GUILayout.Toggle(Settings.bCopier_dynamicBones_adjustScaleColliders, Strings.Copier.dynamicBones_adjustScaleColliders, Styles.CopierToggle);
                                }
                            }

                            EditorGUILayout.Space();
                            EditorGUI.EndDisabledGroup();
                        }

                        Helpers.DrawGUILine(1, false);
                    }

                    if(CopierTabs.ComponentIsInSelectedTab<SkinnedMeshRenderer>(Settings._copier_selectedTab))
                    {
                        //SkinnedMeshRenderer menu
                        Helpers.DrawDropdownWithToggle(ref Settings._copier_expand_skinnedMeshRenderer, ref Settings.bCopier_skinMeshRender_copy, Strings.Copier.skinMeshRender, Icons.SkinnedMeshRenderer);
                        if(Settings._copier_expand_skinnedMeshRenderer)
                        {
                            EditorGUI.BeginDisabledGroup(!Settings.bCopier_skinMeshRender_copy);
                            EditorGUILayout.Space();

                            using(var cHorizontalScope = new GUILayout.HorizontalScope())
                            {
                                GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE); // horizontal indent size

                                using(var cVerticalScope = new GUILayout.VerticalScope())
                                {
                                    Settings.bCopier_skinMeshRender_copySettings = GUILayout.Toggle(Settings.bCopier_skinMeshRender_copySettings, Strings.Copier.copySettings, Styles.CopierToggle);
                                    Settings.bCopier_skinMeshRender_copyMaterials = GUILayout.Toggle(Settings.bCopier_skinMeshRender_copyMaterials, Strings.Copier.skinMeshRender_materials, Styles.CopierToggle);
                                    Settings.bCopier_skinMeshRender_copyBlendShapeValues = GUILayout.Toggle(Settings.bCopier_skinMeshRender_copyBlendShapeValues, Strings.Copier.skinMeshRender_blendShapeValues, Styles.CopierToggle);
                                    Settings.bCopier_skinMeshRender_copyBounds = GUILayout.Toggle(Settings.bCopier_skinMeshRender_copyBounds, Strings.Copier.skinMeshRender_bounds, Styles.CopierToggle);
                                }
                            }

                            EditorGUILayout.Space();
                            EditorGUI.EndDisabledGroup();
                        }
                        Helpers.DrawGUILine(1, false);
                    }

                    if(CopierTabs.ComponentIsInSelectedTab<MeshRenderer>(Settings._copier_selectedTab))
                    {
                        //MeshRenderers menu
                        Helpers.DrawDropdownWithToggle(ref Settings._copier_expand_meshRenderers, ref Settings.bCopier_meshRenderers_copy, Strings.Copier.meshRenderers, Icons.MeshRenderer);
                        if(Settings._copier_expand_meshRenderers)
                        {
                            EditorGUI.BeginDisabledGroup(!Settings.bCopier_meshRenderers_copy);
                            EditorGUILayout.Space();

                            using(var cHorizontalScope = new GUILayout.HorizontalScope())
                            {
                                GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE); // horizontal indent size

                                using(var cVerticalScope = new GUILayout.VerticalScope())
                                {
                                    Settings.bCopier_meshRenderers_copySettings = GUILayout.Toggle(Settings.bCopier_meshRenderers_copySettings, Strings.Copier.copySettings, Styles.CopierToggle);
                                    Settings.bCopier_meshRenderers_createMissing = GUILayout.Toggle(Settings.bCopier_meshRenderers_createMissing, Strings.Copier.createMissing, Styles.CopierToggle);
                                    Settings.bCopier_meshRenderers_createObjects = GUILayout.Toggle(Settings.bCopier_meshRenderers_createObjects, Strings.Copier.copyGameObjects, Styles.CopierToggle);
                                }
                            }

                            EditorGUILayout.Space();
                            EditorGUI.EndDisabledGroup();
                        }

                        Helpers.DrawGUILine(1, false);
                    }

                    if(CopierTabs.ComponentIsInSelectedTab("contactreceiver", Settings._copier_selectedTab))
                    {
                        //Contact Receivers menu
                        if(!PhysBonesExist)
                        {
                            EditorGUI.BeginDisabledGroup(true);
                            {
                                Helpers.DrawDropdownWithToggle(ref Settings._copier_expand_physBones, ref Settings.bCopier_contactReceiver_copy, "Contact Receivers | SDK version 2022.03.04.12.28 or newer required", Icons.ContactReceiver);
                                Settings.bCopier_contactReceiver_copy = false;
                                Settings._copier_expand_contactReceiver = false;
                            }
                            EditorGUI.EndDisabledGroup();
                        }
                        else
                        {
#if PUMKIN_PBONES
                        Helpers.DrawDropdownWithToggle(ref Settings._copier_expand_contactReceiver, ref Settings.bCopier_contactReceiver_copy, Strings.Copier.contactReceiver, Icons.ContactReceiver);
#endif
                        }
                        if(Settings._copier_expand_contactReceiver)
                        {

                            EditorGUI.BeginDisabledGroup(!Settings.bCopier_contactReceiver_copy);
                            EditorGUILayout.Space();

                            using (var cHorizontalScope = new GUILayout.HorizontalScope())
                            {
                                GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE); // horizontal indent size

                                using (var cVerticalScope = new GUILayout.VerticalScope())
                                {
                                    Settings.bCopier_contactReceiver_createObjects = GUILayout.Toggle(Settings.bCopier_contactReceiver_createObjects, Strings.Copier.copyGameObjects, Styles.CopierToggle);
                                    Settings.bCopier_contactReceiver_removeOld = GUILayout.Toggle(Settings.bCopier_contactReceiver_removeOld, Strings.Copier.contactReceiver_removeOld, Styles.CopierToggle);
                                    Settings.bCopier_contactReceiver_adjustScale = GUILayout.Toggle(Settings.bCopier_contactReceiver_adjustScale, Strings.Copier.contactReceiver_adjustScale, Styles.CopierToggle);
                                }
                            }

                            EditorGUILayout.Space();
                            EditorGUI.EndDisabledGroup();
                        }

                        Helpers.DrawGUILine(1, false);
                    }

                    if(CopierTabs.ComponentIsInSelectedTab("contactsender", Settings._copier_selectedTab))
                    {
                        //Contact Senders menu
                        if(!PhysBonesExist)
                        {
                            EditorGUI.BeginDisabledGroup(true);
                            {
                                Helpers.DrawDropdownWithToggle(ref Settings._copier_expand_physBones, ref Settings.bCopier_contactSender_copy, "Contact Senders | SDK version 2022.03.04.12.28 or newer required", Icons.ContactReceiver);
                                Settings.bCopier_contactSender_copy = false;
                                Settings._copier_expand_contactSender = false;
                            }
                            EditorGUI.EndDisabledGroup();
                        }
                        else
                        {
#if PUMKIN_PBONES
                        Helpers.DrawDropdownWithToggle(ref Settings._copier_expand_contactSender, ref Settings.bCopier_contactSender_copy, Strings.Copier.contactSender, Icons.ContactSender);
#endif
                        }
                        if(Settings._copier_expand_contactSender)
                        {

                            EditorGUI.BeginDisabledGroup(!Settings.bCopier_contactSender_copy);
                            EditorGUILayout.Space();

                            using (var cHorizontalScope = new GUILayout.HorizontalScope())
                            {
                                GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE); // horizontal indent size

                                using (var cVerticalScope = new GUILayout.VerticalScope())
                                {
                                    Settings.bCopier_contactSender_createObjects = GUILayout.Toggle(Settings.bCopier_contactSender_createObjects, Strings.Copier.copyGameObjects, Styles.CopierToggle);
                                    Settings.bCopier_contactSender_removeOld = GUILayout.Toggle(Settings.bCopier_contactSender_removeOld, Strings.Copier.contactSender_removeOld, Styles.CopierToggle);
                                    Settings.bCopier_contactSender_adjustScale = GUILayout.Toggle(Settings.bCopier_contactSender_adjustScale, Strings.Copier.contactSender_adjustScale, Styles.CopierToggle);
                                }
                            }

                            EditorGUILayout.Space();
                            EditorGUI.EndDisabledGroup();
                        }

                        Helpers.DrawGUILine(1, false);
                    }

                    if(CopierTabs.ComponentIsInSelectedTab<ParticleSystem>(Settings._copier_selectedTab))
                    {
                        //Particles menu
                        Helpers.DrawDropdownWithToggle(ref Settings._copier_expand_particleSystems, ref Settings.bCopier_particleSystems_copy, Strings.Copier.particleSystems, Icons.ParticleSystem);
                        if(Settings._copier_expand_particleSystems)
                        {
                            EditorGUI.BeginDisabledGroup(!Settings.bCopier_particleSystems_copy);
                            EditorGUILayout.Space();

                            using(var cHorizontalScope = new GUILayout.HorizontalScope())
                            {
                                GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE); // horizontal indent size

                                using(var cVerticalScope = new GUILayout.VerticalScope())
                                {
                                    Settings.bCopier_particleSystems_replace = GUILayout.Toggle(Settings.bCopier_particleSystems_replace, Strings.Copier.replaceOld, Styles.CopierToggle);
                                    Settings.bCopier_particleSystems_createObjects = GUILayout.Toggle(Settings.bCopier_particleSystems_createObjects, Strings.Copier.copyGameObjects, Styles.CopierToggle);
                                }
                            }

                            EditorGUILayout.Space();
                            EditorGUI.EndDisabledGroup();
                        }
                        Helpers.DrawGUILine(1, false);
                    }

                    if(CopierTabs.ComponentIsInSelectedTab<TrailRenderer>(Settings._copier_selectedTab))
                    {
                        //TrailRenderers menu
                        Helpers.DrawDropdownWithToggle(ref Settings._copier_expand_trailRenderers, ref Settings.bCopier_trailRenderers_copy, Strings.Copier.trailRenderers, Icons.TrailRenderer);
                        if(Settings._copier_expand_trailRenderers)
                        {
                            EditorGUI.BeginDisabledGroup(!Settings.bCopier_trailRenderers_copy);
                            EditorGUILayout.Space();

                            using(var cHorizontalScope = new GUILayout.HorizontalScope())
                            {
                                GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE); // horizontal indent size

                                using(var cVerticalScope = new GUILayout.VerticalScope())
                                {
                                    Settings.bCopier_trailRenderers_copySettings = GUILayout.Toggle(Settings.bCopier_trailRenderers_copySettings, Strings.Copier.copySettings, Styles.CopierToggle);
                                    Settings.bCopier_trailRenderers_createMissing = GUILayout.Toggle(Settings.bCopier_trailRenderers_createMissing, Strings.Copier.createMissing, Styles.CopierToggle);
                                    Settings.bCopier_trailRenderers_createObjects = GUILayout.Toggle(Settings.bCopier_trailRenderers_createObjects, Strings.Copier.copyGameObjects, Styles.CopierToggle);
                                }
                            }

                            EditorGUILayout.Space();
                            EditorGUI.EndDisabledGroup();
                        }
                        Helpers.DrawGUILine(1, false);
                    }

                    if(CopierTabs.ComponentIsInSelectedTab<AudioSource>(Settings._copier_selectedTab))
                    {
                        //AudioSources menu
                        Helpers.DrawDropdownWithToggle(ref Settings._copier_expand_audioSources, ref Settings.bCopier_audioSources_copy, Strings.Copier.audioSources, Icons.AudioSource);
                        if(Settings._copier_expand_audioSources)
                        {
                            EditorGUI.BeginDisabledGroup(!Settings.bCopier_audioSources_copy);
                            EditorGUILayout.Space();

                            using(var cHorizontalScope = new GUILayout.HorizontalScope())
                            {
                                GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE); // horizontal indent size

                                using(var cVerticalScope = new GUILayout.VerticalScope())
                                {
                                    Settings.bCopier_audioSources_copySettings = GUILayout.Toggle(Settings.bCopier_audioSources_copySettings, Strings.Copier.copySettings, Styles.CopierToggle);
                                    Settings.bCopier_audioSources_createMissing = GUILayout.Toggle(Settings.bCopier_audioSources_createMissing, Strings.Copier.createMissing, Styles.CopierToggle);
                                    Settings.bCopier_audioSources_createObjects = GUILayout.Toggle(Settings.bCopier_audioSources_createObjects, Strings.Copier.copyGameObjects, Styles.CopierToggle);
                                }
                            }

                            EditorGUILayout.Space();
                            EditorGUI.EndDisabledGroup();
                        }
                        Helpers.DrawGUILine(1, false);
                    }

                    if(CopierTabs.ComponentIsInSelectedTab<Light>(Settings._copier_selectedTab))
                    {
                        //Lights menu
                        Helpers.DrawDropdownWithToggle(ref Settings._copier_expand_lights, ref Settings.bCopier_lights_copy, Strings.Copier.lights, Icons.Light);
                        if(Settings._copier_expand_lights)
                        {
                            EditorGUI.BeginDisabledGroup(!Settings.bCopier_lights_copy);
                            EditorGUILayout.Space();
                            using(var cHorizontalScope = new GUILayout.HorizontalScope())
                            {
                                GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE); // horizontal indent size

                                using(var cVerticalScope = new GUILayout.VerticalScope())
                                {
                                    Settings.bCopier_lights_copySettings = GUILayout.Toggle(Settings.bCopier_lights_copySettings, Strings.Copier.copySettings, Styles.CopierToggle);
                                    Settings.bCopier_lights_createMissing = GUILayout.Toggle(Settings.bCopier_lights_createMissing, Strings.Copier.createMissing, Styles.CopierToggle);
                                    Settings.bCopier_lights_createObjects = GUILayout.Toggle(Settings.bCopier_lights_createObjects, Strings.Copier.copyGameObjects, Styles.CopierToggle);
                                }
                            }
                            EditorGUILayout.Space();
                            EditorGUI.EndDisabledGroup();
                        }

                        Helpers.DrawGUILine(1, false);
                    }

                    if(CopierTabs.ComponentIsInSelectedTab<Rigidbody>(Settings._copier_selectedTab))
                    {
                        //RidigBodies menu
                        Helpers.DrawDropdownWithToggle(ref Settings._copier_expand_rigidBodies, ref Settings.bCopier_rigidBodies_copy, Strings.Copier.rigidBodies, Icons.RigidBody);
                        if(Settings._copier_expand_rigidBodies)
                        {
                            EditorGUI.BeginDisabledGroup(!Settings.bCopier_rigidBodies_copy);
                            EditorGUILayout.Space();

                            using(var cHorizontalScope = new GUILayout.HorizontalScope())
                            {
                                GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE); // horizontal indent size

                                using(var cVerticalScope = new GUILayout.VerticalScope())
                                {
                                    Settings.bCopier_rigidBodies_copySettings = GUILayout.Toggle(Settings.bCopier_rigidBodies_copySettings, Strings.Copier.copySettings, Styles.CopierToggle);
                                    Settings.bCopier_rigidBodies_createMissing = GUILayout.Toggle(Settings.bCopier_rigidBodies_createMissing, Strings.Copier.createMissing, Styles.CopierToggle);
                                    Settings.bCopier_rigidBodies_createObjects = GUILayout.Toggle(Settings.bCopier_rigidBodies_createObjects, Strings.Copier.copyGameObjects, Styles.CopierToggle);
                                }
                            }

                            EditorGUILayout.Space();
                            EditorGUI.EndDisabledGroup();
                        }
                        Helpers.DrawGUILine(1, false);
                    }

                    if(CopierTabs.ComponentIsInSelectedTab<Joint>(Settings._copier_selectedTab))
                    {
                        //Joints menu
                        Helpers.DrawDropdownWithToggle(ref Settings._copier_expand_joints, ref Settings.bCopier_joints_copy, Strings.Copier.joints, Icons.Joint);
                        if(Settings._copier_expand_joints)
                        {
                            EditorGUI.BeginDisabledGroup(!Settings.bCopier_joints_copy);
                            EditorGUILayout.Space();

                            using(var cHorizontalScope = new GUILayout.HorizontalScope())
                            {
                                GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE); // horizontal indent size

                                using(var cVerticalScope = new GUILayout.VerticalScope())
                                {
                                    Settings.bCopier_joints_fixed = GUILayout.Toggle(Settings.bCopier_joints_fixed, Strings.Copier.joints_fixed, Styles.CopierToggle);
                                    Settings.bCopier_joints_hinge = GUILayout.Toggle(Settings.bCopier_joints_hinge, Strings.Copier.joints_hinge, Styles.CopierToggle);
                                    Settings.bCopier_joints_spring = GUILayout.Toggle(Settings.bCopier_joints_spring, Strings.Copier.joints_spring, Styles.CopierToggle);
                                    Settings.bCopier_joints_character = GUILayout.Toggle(Settings.bCopier_joints_character, Strings.Copier.joints_character, Styles.CopierToggle);
                                    Settings.bCopier_joints_configurable = GUILayout.Toggle(Settings.bCopier_joints_configurable, Strings.Copier.joints_configurable, Styles.CopierToggle);

                                    EditorGUILayout.Space();

                                    Settings.bCopier_joints_removeOld = GUILayout.Toggle(Settings.bCopier_joints_removeOld, Strings.Copier.joints_removeOld, Styles.CopierToggle);
                                    Settings.bCopier_joints_createObjects = GUILayout.Toggle(Settings.bCopier_joints_createObjects, Strings.Copier.copyGameObjects, Styles.CopierToggle);
                                }
                            }

                            EditorGUILayout.Space();
                            EditorGUI.EndDisabledGroup();
                        }

                        Helpers.DrawGUILine(1, false);
                    }

                    if(CopierTabs.ComponentIsInSelectedTab<Collider>(Settings._copier_selectedTab))
                    {
                        //Colliders menu
                        Helpers.DrawDropdownWithToggle(ref Settings._copier_expand_colliders, ref Settings.bCopier_colliders_copy, Strings.Copier.colliders, Icons.ColliderBox);
                        if(Settings._copier_expand_colliders)
                        {
                            EditorGUI.BeginDisabledGroup(!Settings.bCopier_colliders_copy);
                            EditorGUILayout.Space();

                            using(var cHorizontalScope = new GUILayout.HorizontalScope())
                            {
                                GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE); // horizontal indent size

                                using(var cVerticalScope = new GUILayout.VerticalScope())
                                {
                                    Settings.bCopier_colliders_copyBox = GUILayout.Toggle(Settings.bCopier_colliders_copyBox, Strings.Copier.colliders_box, Styles.CopierToggle);
                                    Settings.bCopier_colliders_copyCapsule = GUILayout.Toggle(Settings.bCopier_colliders_copyCapsule, Strings.Copier.colliders_capsule, Styles.CopierToggle);
                                    Settings.bCopier_colliders_copySphere = GUILayout.Toggle(Settings.bCopier_colliders_copySphere, Strings.Copier.colliders_sphere, Styles.CopierToggle);
                                    Settings.bCopier_colliders_copyMesh = GUILayout.Toggle(Settings.bCopier_colliders_copyMesh, Strings.Copier.colliders_mesh, Styles.CopierToggle);

                                    EditorGUILayout.Space();

                                    Settings.bCopier_colliders_adjustScale = GUILayout.Toggle(Settings.bCopier_colliders_adjustScale, Strings.Copier.colliders_adjustScale, Styles.CopierToggle);
                                    Settings.bCopier_colliders_removeOld = GUILayout.Toggle(Settings.bCopier_colliders_removeOld, Strings.Copier.colliders_removeOld, Styles.CopierToggle);
                                    Settings.bCopier_colliders_createObjects = GUILayout.Toggle(Settings.bCopier_colliders_createObjects, Strings.Copier.copyGameObjects, Styles.CopierToggle);
                                }
                            }

                            EditorGUILayout.Space();
                            EditorGUI.EndDisabledGroup();
                        }

                        Helpers.DrawGUILine(1, false);
                    }

                    if(CopierTabs.ComponentIsInSelectedTab<Animator>(Settings._copier_selectedTab))
                    {
                        //Animators menu
                        Helpers.DrawDropdownWithToggle(ref Settings._copier_expand_animators, ref Settings.bCopier_animators_copy, Strings.Copier.animators, Icons.Animator);
                        if(Settings._copier_expand_animators)
                        {
                            EditorGUI.BeginDisabledGroup(!Settings.bCopier_animators_copy);
                            EditorGUILayout.Space();

                            using(var cHorizontalScope = new GUILayout.HorizontalScope())
                            {
                                GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE); // horizontal indent size

                                using(var cVerticalScope = new GUILayout.VerticalScope())
                                {
                                    Settings.bCopier_animators_copySettings = GUILayout.Toggle(Settings.bCopier_animators_copySettings, Strings.Copier.copySettings, Styles.CopierToggle);
                                    Settings.bCopier_animators_createMissing = GUILayout.Toggle(Settings.bCopier_animators_createMissing, Strings.Copier.createMissing, Styles.CopierToggle);
                                    Settings.bCopier_animators_createObjects = GUILayout.Toggle(Settings.bCopier_animators_createObjects, Strings.Copier.copyGameObjects, Styles.CopierToggle);
                                    Settings.bCopier_animators_copyMainAnimator = GUILayout.Toggle(Settings.bCopier_animators_copyMainAnimator, Strings.Copier.copyMainAnimator, Styles.CopierToggle);
                                }
                            }

                            EditorGUILayout.Space();
                            EditorGUI.EndDisabledGroup();
                        }

                        Helpers.DrawGUILine(1, false);
                    }

                    if(CopierTabs.ComponentIsInSelectedTab<Transform>(Settings._copier_selectedTab))
                    {
                        //Transforms menu
                        Helpers.DrawDropdownWithToggle(ref Settings._copier_expand_transforms, ref Settings.bCopier_transforms_copy, Strings.Copier.transforms, Icons.Transform);
                        if(Settings._copier_expand_transforms)
                        {
                            EditorGUI.BeginDisabledGroup(!Settings.bCopier_transforms_copy);
                            EditorGUILayout.Space();

                            using(var cHorizontalScope = new GUILayout.HorizontalScope())
                            {
                                GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE); // horizontal indent size

                                using(var cVerticalScope = new GUILayout.VerticalScope())
                                {
                                    Settings.bCopier_transforms_copyPosition = GUILayout.Toggle(Settings.bCopier_transforms_copyPosition, Strings.Copier.transforms_position, Styles.CopierToggle);
                                    Settings.bCopier_transforms_copyRotation = GUILayout.Toggle(Settings.bCopier_transforms_copyRotation, Strings.Copier.transforms_rotation, Styles.CopierToggle);
                                    Settings.bCopier_transforms_copyScale = GUILayout.Toggle(Settings.bCopier_transforms_copyScale, Strings.Copier.transforms_scale, Styles.CopierToggle);
                                    Settings.bCopier_transforms_createMissing = GUILayout.Toggle(Settings.bCopier_transforms_createMissing, Strings.Copier.transforms_createMissing, Styles.CopierToggle);
                                    EditorGUILayout.Space();
                                    Settings.bCopier_transforms_copyActiveState = GUILayout.Toggle(Settings.bCopier_transforms_copyActiveState, Strings.Copier.transforms_copyActiveState, Styles.CopierToggle);
                                    Settings.bCopier_transforms_copyLayerAndTag = GUILayout.Toggle(Settings.bCopier_transforms_copyLayerAndTag, Strings.Copier.transforms_copyLayerAndTag, Styles.CopierToggle);
                                }
                            }

                            EditorGUILayout.Space();
                            EditorGUI.EndDisabledGroup();
                        }

                        Helpers.DrawGUILine(1, false);
                    }

                    if(CopierTabs.ComponentIsInSelectedTab<AimConstraint>(Settings._copier_selectedTab))
                    {
                        //Aim Constraints menu
                        Helpers.DrawDropdownWithToggle(ref Settings._copier_expand_aimConstraints, ref Settings.bCopier_aimConstraint_copy, Strings.Copier.aimConstraints, Icons.AimConstraint);
                        if(Settings._copier_expand_aimConstraints)
                        {
                            EditorGUI.BeginDisabledGroup(!Settings.bCopier_aimConstraint_copy);
                            EditorGUILayout.Space();

                            using(var cHorizontalScope = new GUILayout.HorizontalScope())
                            {
                                GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE); // horizontal indent size

                                using(var cVerticalScope = new GUILayout.VerticalScope())
                                {
                                    Settings.bCopier_aimConstraint_replaceOld = GUILayout.Toggle(Settings.bCopier_aimConstraint_replaceOld, Strings.Copier.replaceOld, Styles.CopierToggle);
                                    Settings.bCopier_aimConstraint_createObjects = GUILayout.Toggle(Settings.bCopier_aimConstraint_createObjects, Strings.Copier.copyGameObjects, Styles.CopierToggle);
                                    Settings.bCopier_aimConstraint_onlyIfHasValidSources = GUILayout.Toggle(Settings.bCopier_aimConstraint_onlyIfHasValidSources, Strings.Copier.onlyIfHasValidSources, Styles.CopierToggle);
                                }
                            }

                            EditorGUILayout.Space();
                            EditorGUI.EndDisabledGroup();
                        }

                        Helpers.DrawGUILine(1, false);
                    }

                    if(CopierTabs.ComponentIsInSelectedTab<LookAtConstraint>(Settings._copier_selectedTab))
                    {
                        //LookAt Constraints menu
                        Helpers.DrawDropdownWithToggle(ref Settings._copier_expand_lookAtConstraints, ref Settings.bCopier_lookAtConstraint_copy, Strings.Copier.lookAtConstraints, Icons.LookAtConstraint);
                        if(Settings._copier_expand_lookAtConstraints)
                        {
                            EditorGUI.BeginDisabledGroup(!Settings.bCopier_lookAtConstraint_copy);
                            EditorGUILayout.Space();

                            using(var cHorizontalScope = new GUILayout.HorizontalScope())
                            {
                                GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE); // horizontal indent size

                                using(var cVerticalScope = new GUILayout.VerticalScope())
                                {
                                    Settings.bCopier_lookAtConstraint_replaceOld = GUILayout.Toggle(Settings.bCopier_lookAtConstraint_replaceOld, Strings.Copier.replaceOld, Styles.CopierToggle);
                                    Settings.bCopier_lookAtConstraint_createObjects = GUILayout.Toggle(Settings.bCopier_lookAtConstraint_createObjects, Strings.Copier.copyGameObjects, Styles.CopierToggle);
                                    Settings.bCopier_lookAtConstraint_onlyIfHasValidSources = GUILayout.Toggle(Settings.bCopier_lookAtConstraint_onlyIfHasValidSources, Strings.Copier.onlyIfHasValidSources, Styles.CopierToggle);
                                }
                            }

                            EditorGUILayout.Space();
                            EditorGUI.EndDisabledGroup();
                        }

                        Helpers.DrawGUILine(1, false);
                    }

                    if(CopierTabs.ComponentIsInSelectedTab<ParentConstraint>(Settings._copier_selectedTab))
                    {
                        //Parent Constraints menu
                        Helpers.DrawDropdownWithToggle(ref Settings._copier_expand_parentConstraints, ref Settings.bCopier_parentConstraint_copy, Strings.Copier.parentConstraints, Icons.ParentConstraint);
                        if(Settings._copier_expand_parentConstraints)
                        {
                            EditorGUI.BeginDisabledGroup(!Settings.bCopier_parentConstraint_copy);
                            EditorGUILayout.Space();

                            using(var cHorizontalScope = new GUILayout.HorizontalScope())
                            {
                                GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE); // horizontal indent size

                                using(var cVerticalScope = new GUILayout.VerticalScope())
                                {
                                    Settings.bCopier_parentConstraint_replaceOld = GUILayout.Toggle(Settings.bCopier_parentConstraint_replaceOld, Strings.Copier.replaceOld, Styles.CopierToggle);
                                    Settings.bCopier_parentConstraint_createObjects = GUILayout.Toggle(Settings.bCopier_parentConstraint_createObjects, Strings.Copier.copyGameObjects, Styles.CopierToggle);
                                    Settings.bCopier_parentConstraint_onlyIfHasValidSources = GUILayout.Toggle(Settings.bCopier_parentConstraint_onlyIfHasValidSources, Strings.Copier.onlyIfHasValidSources, Styles.CopierToggle);
                                }
                            }

                            EditorGUILayout.Space();
                            EditorGUI.EndDisabledGroup();
                        }

                        Helpers.DrawGUILine(1, false);
                    }

                    if(CopierTabs.ComponentIsInSelectedTab<PositionConstraint>(Settings._copier_selectedTab))
                    {
                        //Position Constraints menu
                        Helpers.DrawDropdownWithToggle(ref Settings._copier_expand_positionConstraints, ref Settings.bCopier_positionConstraint_copy, Strings.Copier.positionConstraints, Icons.PositionConstraint);
                        if(Settings._copier_expand_positionConstraints)
                        {
                            EditorGUI.BeginDisabledGroup(!Settings.bCopier_positionConstraint_copy);
                            EditorGUILayout.Space();

                            using(var cHorizontalScope = new GUILayout.HorizontalScope())
                            {
                                GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE); // horizontal indent size

                                using(var cVerticalScope = new GUILayout.VerticalScope())
                                {
                                    Settings.bCopier_positionConstraint_replaceOld = GUILayout.Toggle(Settings.bCopier_positionConstraint_replaceOld, Strings.Copier.replaceOld, Styles.CopierToggle);
                                    Settings.bCopier_positionConstraint_createObjects = GUILayout.Toggle(Settings.bCopier_positionConstraint_createObjects, Strings.Copier.copyGameObjects, Styles.CopierToggle);
                                    Settings.bCopier_positionConstraint_onlyIfHasValidSources = GUILayout.Toggle(Settings.bCopier_positionConstraint_onlyIfHasValidSources, Strings.Copier.onlyIfHasValidSources, Styles.CopierToggle);
                                }
                            }

                            EditorGUILayout.Space();
                            EditorGUI.EndDisabledGroup();
                        }

                        Helpers.DrawGUILine(1, false);
                    }

                    if(CopierTabs.ComponentIsInSelectedTab<RotationConstraint>(Settings._copier_selectedTab))
                    {
                        //Rotation Constraints menu
                        Helpers.DrawDropdownWithToggle(ref Settings._copier_expand_rotationConstraints, ref Settings.bCopier_rotationConstraint_copy, Strings.Copier.rotationConstraints, Icons.RotationConstraint);
                        if(Settings._copier_expand_rotationConstraints)
                        {
                            EditorGUI.BeginDisabledGroup(!Settings.bCopier_rotationConstraint_copy);
                            EditorGUILayout.Space();

                            using(var cHorizontalScope = new GUILayout.HorizontalScope())
                            {
                                GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE); // horizontal indent size

                                using(var cVerticalScope = new GUILayout.VerticalScope())
                                {
                                    Settings.bCopier_rotationConstraint_replaceOld = GUILayout.Toggle(Settings.bCopier_rotationConstraint_replaceOld, Strings.Copier.replaceOld, Styles.CopierToggle);
                                    Settings.bCopier_rotationConstraint_createObjects = GUILayout.Toggle(Settings.bCopier_rotationConstraint_createObjects, Strings.Copier.copyGameObjects, Styles.CopierToggle);
                                    Settings.bCopier_rotationConstraint_onlyIfHasValidSources = GUILayout.Toggle(Settings.bCopier_rotationConstraint_onlyIfHasValidSources, Strings.Copier.onlyIfHasValidSources, Styles.CopierToggle);
                                }
                            }

                            EditorGUILayout.Space();
                            EditorGUI.EndDisabledGroup();
                        }

                        Helpers.DrawGUILine(1, false);
                    }

                    if(CopierTabs.ComponentIsInSelectedTab<ScaleConstraint>(Settings._copier_selectedTab))
                    {
                        //Scale Constraints menu
                        Helpers.DrawDropdownWithToggle(ref Settings._copier_expand_scaleConstraints, ref Settings.bCopier_scaleConstraint_copy, Strings.Copier.scaleConstraints, Icons.ScaleConstraint);
                        if(Settings._copier_expand_scaleConstraints)
                        {
                            EditorGUI.BeginDisabledGroup(!Settings.bCopier_scaleConstraint_copy);
                            EditorGUILayout.Space();

                            using(var cHorizontalScope = new GUILayout.HorizontalScope())
                            {
                                GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE); // horizontal indent size

                                using(var cVerticalScope = new GUILayout.VerticalScope())
                                {
                                    Settings.bCopier_scaleConstraint_replaceOld = GUILayout.Toggle(Settings.bCopier_scaleConstraint_replaceOld, Strings.Copier.replaceOld, Styles.CopierToggle);
                                    Settings.bCopier_scaleConstraint_createObjects = GUILayout.Toggle(Settings.bCopier_scaleConstraint_createObjects, Strings.Copier.copyGameObjects, Styles.CopierToggle);
                                    Settings.bCopier_scaleConstraint_onlyIfHasValidSources = GUILayout.Toggle(Settings.bCopier_scaleConstraint_onlyIfHasValidSources, Strings.Copier.onlyIfHasValidSources, Styles.CopierToggle);
                                }
                            }

                            EditorGUILayout.Space();
                            EditorGUI.EndDisabledGroup();
                        }

                        Helpers.DrawGUILine(1, false);
                    }
                    
                    if(CopierTabs.ComponentIsInSelectedTab<Camera>(Settings._copier_selectedTab))
                    {
                        //Camera menu
                        Helpers.DrawDropdownWithToggle(ref Settings._copier_expand_cameras, ref Settings.bCopier_cameras_copy, Strings.Copier.cameras, Icons.Camera);
                        if(Settings._copier_expand_cameras)
                        {
                            EditorGUI.BeginDisabledGroup(!Settings.bCopier_cameras_copy);
                            EditorGUILayout.Space();

                            using(var cHorizontalScope = new GUILayout.HorizontalScope())
                            {
                                GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE); // horizontal indent size

                                using(var cVerticalScope = new GUILayout.VerticalScope())
                                {
                                    Settings.bCopier_cameras_createObjects = GUILayout.Toggle(Settings.bCopier_cameras_createObjects, Strings.Copier.copyGameObjects, Styles.CopierToggle);
                                }
                            }

                            EditorGUILayout.Space();
                            EditorGUI.EndDisabledGroup();
                        }
                        Helpers.DrawGUILine(1, false);
                    }
                    
                    if(CopierTabs.ComponentIsInSelectedTab("finalik", Settings._copier_selectedTab))
                    {
						bool exists = FinalIKExists;
						//Other menu
						if(exists)
						{							
                        	Helpers.DrawDropdownWithToggle(ref Settings._copier_expand_finalIK, ref Settings.bCopier_finalIK_copy, Strings.Copier.finalIK, Icons.Avatar);
							if (Settings._copier_expand_finalIK)
							{
								EditorGUI.BeginDisabledGroup(!Settings.bCopier_finalIK_copy);
								EditorGUILayout.Space();

								using (var cHorizontalScope = new GUILayout.HorizontalScope())
								{
									GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE); // horizontal indent size

									using (var cVerticalScope = new GUILayout.VerticalScope())
									{
										Settings.bCopier_finalIK_copyFabrik = GUILayout.Toggle(Settings.bCopier_finalIK_copyFabrik, Strings.Copier.finalIK_fabrIK, Styles.CopierToggle);
										Settings.bCopier_finalIK_copyAimIK = GUILayout.Toggle(Settings.bCopier_finalIK_copyAimIK, Strings.Copier.finalIK_aimIK, Styles.CopierToggle);
										Settings.bCopier_finalIK_copyCCDIK = GUILayout.Toggle(Settings.bCopier_finalIK_copyCCDIK, Strings.Copier.finalIK_ccdIK, Styles.CopierToggle);
										Settings.bCopier_finalIK_copyRotationLimits = GUILayout.Toggle(Settings.bCopier_finalIK_copyRotationLimits, Strings.Copier.finalIK_rotationLimits, Styles.CopierToggle);
										Settings.bCopier_finalIK_copyLimbIK = GUILayout.Toggle(Settings.bCopier_finalIK_copyLimbIK, Strings.Copier.finalIK_limbIK, Styles.CopierToggle);
										Settings.bCopier_finalIK_copyFBTBipedIK = GUILayout.Toggle(Settings.bCopier_finalIK_copyFBTBipedIK, Strings.Copier.finalIK_fbtBipedIK, Styles.CopierToggle);
										Settings.bCopier_finalIK_copyVRIK = GUILayout.Toggle(Settings.bCopier_finalIK_copyVRIK, Strings.Copier.finalIK_VRIK, Styles.CopierToggle);

										EditorGUILayout.Space();

										Settings.bCopier_finalIK_createObjects = GUILayout.Toggle(
											Settings.bCopier_finalIK_createObjects, Strings.Copier.copyGameObjects,
											Styles.CopierToggle);
									}
								}

								EditorGUILayout.Space();
								EditorGUI.EndDisabledGroup();
							}
						}
						else
						{
							EditorGUI.BeginDisabledGroup(true);
							Helpers.DrawDropdownWithToggle(ref exists, ref exists, Strings.Copier.finalIK + $" ({Strings.Warning.notFound})", Icons.Avatar);
							EditorGUI.EndDisabledGroup();
						}                        
						
                        Helpers.DrawGUILine(1, false);
                    }

                    if(CopierTabs.ComponentIsInSelectedTab("other", Settings._copier_selectedTab))
                    {
                        //Other menu
                        Helpers.DrawDropdownWithToggle(ref Settings._copier_expand_other, ref Settings.bCopier_other_copy, Strings.Copier.other, Icons.CsScript);
                        if(Settings._copier_expand_other)
                        {
                            EditorGUI.BeginDisabledGroup(!Settings.bCopier_other_copy);
                            EditorGUILayout.Space();

                            using(var cHorizontalScope = new GUILayout.HorizontalScope())
                            {
                                GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE); // horizontal indent size

                                using(var cVerticalScope = new GUILayout.VerticalScope())
                                {
                                    Settings.bCopier_other_copyIKFollowers = GUILayout.Toggle(Settings.bCopier_other_copyIKFollowers, Strings.Copier.other_ikFollowers, Styles.CopierToggle);
                                    Settings.bCopier_other_copyVRMSpringBones = GUILayout.Toggle(Settings.bCopier_other_copyVRMSpringBones, Strings.Copier.other_vrmSpringBones, Styles.CopierToggle);
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
                        Helpers.DrawPropertyArrayScrolling(SerializedIgnoreArray, Strings.Copier.exclusions, ref Settings._copierIgnoreArray_expand,
                            ref Settings._copierIgnoreArrayScroll, 0, 100);
                    }
                    if(EditorGUI.EndChangeCheck())
                    {
                        RefreshIgnoreArray();
                    }

                    if(Settings._copierIgnoreArray_expand && SerializedIgnoreArray.arraySize > 0)
                    {
                        using(var cHorizontalScope = new GUILayout.HorizontalScope())
                        {
                            GUILayout.Space(COPIER_SETTINGS_INDENT_SIZE * 4); // horizontal indent size
                            using(var cVerticalScope = new GUILayout.VerticalScope())
                            {
                                Settings.bCopier_ignoreArray_includeChildren = GUILayout.Toggle(Settings.bCopier_ignoreArray_includeChildren, Strings.Copier.includeChildren);
                            }
                        }
                    }

                    Helpers.DrawGUILine();

                    EditorGUILayout.BeginHorizontal();
                    {
                        //TODO: Refactor these 2 buttons
                        if(GUILayout.Button(Strings.Buttons.selectNone, Styles.BigButton, GUILayout.MinWidth(100)))
                        {
                            if(Settings._copier_selectedTab == CopierTabs.Tab.All)
                            {
                                Settings.bCopier_colliders_copy = false;
                                Settings.bCopier_rigidBodies_copy = false;
                                Settings.bCopier_transforms_copy = false;
                                Settings.bCopier_animators_copy = false;
                                Settings.bCopier_aimConstraint_copy = false;
                                Settings.bCopier_lookAtConstraint_copy = false;
                                Settings.bCopier_parentConstraint_copy = false;
                                Settings.bCopier_positionConstraint_copy = false;
                                Settings.bCopier_rotationConstraint_copy = false;
                                Settings.bCopier_scaleConstraint_copy = false;
                                Settings.bCopier_other_copy = false;
                                Settings.bCopier_joints_copy = false;
                                Settings.bCopier_cameras_copy = false;
                                Settings.bCopier_finalIK_copy = false;
                            }

                            Settings.bCopier_descriptor_copy = false;
                            Settings.bCopier_trailRenderers_copy = false;
                            Settings.bCopier_lights_copy = false;
                            Settings.bCopier_skinMeshRender_copy = false;
                            Settings.bCopier_audioSources_copy = false;
                            Settings.bCopier_meshRenderers_copy = false;
                            Settings.bCopier_particleSystems_copy = false;
                            Settings.bCopier_physBones_copy = false;
                            Settings.bCopier_physBones_copyColliders = false;
                            Settings.bCopier_contactReceiver_copy = false;
                            Settings.bCopier_contactSender_copy = false;

                            if(DynamicBonesExist)
                            {
                                Settings.bCopier_dynamicBones_copy = false;
                                Settings.bCopier_dynamicBones_copyColliders = false;
                            }
                        }
                        if(GUILayout.Button(Strings.Buttons.selectAll, Styles.BigButton, GUILayout.MinWidth(100)))
                        {
                            if(Settings._copier_selectedTab == CopierTabs.Tab.All)
                            {
                                Settings.bCopier_colliders_copy = true;
                                Settings.bCopier_rigidBodies_copy = true;
                                Settings.bCopier_transforms_copy = true;
                                Settings.bCopier_animators_copy = true;
                                Settings.bCopier_aimConstraint_copy = true;
                                Settings.bCopier_lookAtConstraint_copy = true;
                                Settings.bCopier_parentConstraint_copy = true;
                                Settings.bCopier_positionConstraint_copy = true;
                                Settings.bCopier_rotationConstraint_copy = true;
                                Settings.bCopier_scaleConstraint_copy = true;
                                Settings.bCopier_other_copy = true;
                                Settings.bCopier_joints_copy = true;
                                Settings.bCopier_cameras_copy = true;
                                Settings.bCopier_finalIK_copy = true && FinalIKExists;

                            }

                            Settings.bCopier_descriptor_copy = true;
                            Settings.bCopier_trailRenderers_copy = true;
                            Settings.bCopier_lights_copy = true;
                            Settings.bCopier_skinMeshRender_copy = true;
                            Settings.bCopier_audioSources_copy = true;
                            Settings.bCopier_meshRenderers_copy = true;
                            Settings.bCopier_particleSystems_copy = true;
                            Settings.bCopier_physBones_copy = true;
                            Settings.bCopier_physBones_copyColliders = true;
                            Settings.bCopier_contactReceiver_copy = true;
                            Settings.bCopier_contactSender_copy = true;

                            if(DynamicBonesExist)
                            {
                                Settings.bCopier_dynamicBones_copy = true;
                                Settings.bCopier_dynamicBones_copyColliders = true;
                            }
                            else
                            {
                                Settings.bCopier_dynamicBones_copy = false;
                                Settings.bCopier_dynamicBones_copyColliders = false;
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

                                CopyComponentsWithoutParents(CopierSelectedFrom, SelectedAvatar);

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

        void DrawInfoMenuGUI()
        {
            if(Settings._info_expand = GUILayout.Toggle(Settings._info_expand, Strings.Main.info, Styles.Foldout_title))
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

        void DrawThumbnailsMenuGUI()
        {
            if(Settings._thumbnails_expand = GUILayout.Toggle(Settings._thumbnails_expand, Strings.Main.thumbnails, Styles.Foldout_title))
            {
                Helpers.DrawGUILine();

#if VRC_SDK_VRCSDK2 || (VRC_SDK_VRCSDK3 && !UDON)
                EditorGUI.BeginChangeCheck();
                {
                    Settings.shouldHideOtherAvatars = GUILayout.Toggle(Settings.shouldHideOtherAvatars, Strings.Thumbnails.hideOtherAvatars);
                }
                if(EditorGUI.EndChangeCheck())
                {
                    AvatarUploadHider.Enabled = Settings.shouldHideOtherAvatars;
                }
#endif

                Helpers.DrawGUILine();

                EditorGUI.BeginChangeCheck();
                {
                    Settings._presetToolbarSelectedIndex = GUILayout.Toolbar(Settings._presetToolbarSelectedIndex, new string[] { Strings.Thumbnails.cameras, Strings.Thumbnails.poses, Strings.Thumbnails.blendshapes }, Styles.ToolbarBigButtons);
                }
                if(EditorGUI.EndChangeCheck())
                {
                    if(Settings._presetToolbarSelectedIndex == (int)PresetToolbarOptions.Blendshape)
                        SetupBlendeshapeRendererHolders(SelectedAvatar);
                }

                EditorGUILayout.Space();
                Helpers.DrawGUILine();

                switch(Settings._presetToolbarSelectedIndex)
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

        void DrawThumbanailBlendshapeGUI()
        {
            EditorGUILayout.LabelField(new GUIContent(Strings.Thumbnails.blendshapes));
            if(SelectedAvatar)
                Helpers.DrawBlendshapeSlidersWithLabels(ref _selectedAvatarRendererHolders, SelectedAvatar);
            else
                EditorGUILayout.LabelField(new GUIContent(Strings.PoseEditor.selectHumanoidAvatar), Styles.HelpBox_OneLine);
            EditorGUILayout.Space();
        }

        void DrawThumbnailPoseGUI()
        {
            if(GUILayout.Button(Strings.Buttons.openPoseEditor, Styles.BigButton))
                PumkinsPoseEditor.ShowWindow();

            Helpers.DrawGUILine();

            Settings.posePresetApplyBodyPosition = GUILayout.Toggle(Settings.posePresetApplyBodyPosition, Strings.Thumbnails.applyBodyPosition);
            Settings.posePresetApplyBodyRotation = GUILayout.Toggle(Settings.posePresetApplyBodyRotation, Strings.Thumbnails.applyBodyRotation);

            EditorGUILayout.Space();

            Settings.posePresetTryFixSinking = GUILayout.Toggle(Settings.posePresetTryFixSinking, Strings.Thumbnails.tryFixPoseSinking);
        }

        void DrawThumbnailCameraGUI()
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

            Settings.lockSelectedCameraToSceneView = GUILayout.Toggle(Settings.lockSelectedCameraToSceneView, Strings.Thumbnails.lockSelectedCameraToSceneView);

            Helpers.DrawGUILine();

            EditorGUI.BeginDisabledGroup(!SelectedCamera || !SelectedAvatar);
            {
                //Center Camera on Viewpoint button
                GUILayout.BeginHorizontal();
                {
                    string centerOnWhat = "?";
                    switch(Settings.centerCameraMode)
                    {
                        case PumkinsCameraPreset.CameraOffsetMode.AvatarRoot:
                            centerOnWhat = Strings.Main.avatar;
                            break;
                        case PumkinsCameraPreset.CameraOffsetMode.Transform:
                            if(SelectedAvatar && !Settings.centerCameraTransform)
                                Settings.centerCameraTransform = SelectedAvatar.transform.Find(Settings.centerCameraTransformPath);
                            if(Settings.centerCameraTransform)
                                centerOnWhat = Settings.centerCameraTransform.name;
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
                            switch(Settings.centerCameraMode)
                            {
                                case PumkinsCameraPreset.CameraOffsetMode.AvatarRoot:
                                    CenterCameraOnTransform(SelectedAvatar.transform, Settings.centerCameraPositionOffsetAvatar, Settings.centerCameraRotationOffsetAvatar, Settings.centerCameraFixClippingPlanes);
                                    break;
                                case PumkinsCameraPreset.CameraOffsetMode.Transform:
                                    CenterCameraOnTransform(Settings.centerCameraTransform, Settings.centerCameraPositionOffsetTransform, Settings.centerCameraRotationOffsetTransform, Settings.centerCameraFixClippingPlanes);
                                    break;
#if VRC_SDK_VRCSDK2 || (VRC_SDK_VRCSDK3 && !UDON)
                                default:
                                    CenterCameraOnViewpoint(SelectedAvatar, Settings.centerCameraPositionOffsetViewpoint, Settings.centerCameraRotationOffsetViewpoint, Settings.centerCameraFixClippingPlanes);
                                    break;
#endif
                            }
                        }
                        else
                            Log(Strings.Warning.cameraNotFound, LogType.Warning);
                    }
                    if(GUILayout.Button(Icons.Settings, Styles.BigIconButton))
                    {
                        Settings._centerCameraOffsets_expand = !Settings._centerCameraOffsets_expand;
                    }
                }
                GUILayout.EndHorizontal();
                if(Settings._centerCameraOffsets_expand)
                {
                    EditorGUILayout.Space();

                    Settings.centerCameraFixClippingPlanes = GUILayout.Toggle(Settings.centerCameraFixClippingPlanes, Strings.Thumbnails.centerCameraFixClippingPlanes);

                    EditorGUILayout.Space();

                    Settings.centerCameraMode = (PumkinsCameraPreset.CameraOffsetMode)EditorGUILayout.EnumPopup(Strings.Presets.mode, Settings.centerCameraMode);

                    if(Settings.centerCameraMode == PumkinsCameraPreset.CameraOffsetMode.Transform)
                    {
                        EditorGUI.BeginChangeCheck();
                        {
                            Settings.centerCameraTransformPath = EditorGUILayout.TextField(Strings.Presets.transform, Settings.centerCameraTransformPath);
                        }
                        if(EditorGUI.EndChangeCheck())
                        {
                            Settings.centerCameraTransform = SelectedAvatar.transform.Find(Settings.centerCameraTransformPath);
                        }
                    }
                    else
                        GUILayout.Space(18);

                    EditorGUILayout.Space();

                    switch(Settings.centerCameraMode)
                    {
                        case PumkinsCameraPreset.CameraOffsetMode.Transform:
                            Settings.centerCameraPositionOffsetTransform = EditorGUILayout.Vector3Field(Strings.Thumbnails.positionOffset, Settings.centerCameraPositionOffsetTransform);
                            Settings.centerCameraRotationOffsetTransform = EditorGUILayout.Vector3Field(Strings.Thumbnails.rotationOffset, Settings.centerCameraRotationOffsetTransform);
                            break;
#if VRC_SDK_VRCSDK2 || (VRC_SDK_VRCSDK3 && !UDON)
                        case PumkinsCameraPreset.CameraOffsetMode.Viewpoint:
                            Settings.centerCameraPositionOffsetViewpoint = EditorGUILayout.Vector3Field(Strings.Thumbnails.positionOffset, Settings.centerCameraPositionOffsetViewpoint);
                            Settings.centerCameraRotationOffsetViewpoint = EditorGUILayout.Vector3Field(Strings.Thumbnails.rotationOffset, Settings.centerCameraRotationOffsetViewpoint);
                            break;
#endif
                        case PumkinsCameraPreset.CameraOffsetMode.AvatarRoot:
                            Settings.centerCameraPositionOffsetAvatar = EditorGUILayout.Vector3Field(Strings.Thumbnails.positionOffset, Settings.centerCameraPositionOffsetAvatar);
                            Settings.centerCameraRotationOffsetAvatar = EditorGUILayout.Vector3Field(Strings.Thumbnails.rotationOffset, Settings.centerCameraRotationOffsetAvatar);
                            break;
                    }

                    EditorGUILayout.Space();

                    EditorGUILayout.BeginHorizontal();
                    {
                        if(GUILayout.Button(Strings.Buttons.reset, GUILayout.MaxWidth(90f)))
                        {
                            switch(Settings.centerCameraMode)
                            {
#if VRC_SDK_VRCSDK2 || (VRC_SDK_VRCSDK3 && !UDON)
                                case PumkinsCameraPreset.CameraOffsetMode.Viewpoint:
                                    Settings.centerCameraPositionOffsetViewpoint = DEFAULT_CAMERA_POSITION_OFFSET_VIEWPOINT;
                                    Settings.centerCameraRotationOffsetViewpoint = DEFAULT_CAMERA_ROTATION_OFFSET_VIEWPOINT;
                                    break;
#endif
                                case PumkinsCameraPreset.CameraOffsetMode.AvatarRoot:
                                    Settings.centerCameraPositionOffsetAvatar = DEFAULT_CAMERA_POSITION_OFFSET_AVATAR;
                                    Settings.centerCameraRotationOffsetAvatar = DEFAULT_CAMERA_ROTATION_OFFSET_AVATAR;
                                    break;
                                case PumkinsCameraPreset.CameraOffsetMode.Transform:
                                    Settings.centerCameraPositionOffsetTransform = DEFAULT_CAMERA_POSITION_OFFSET_TRANSFORM;
                                    Settings.centerCameraRotationOffsetTransform = DEFAULT_CAMERA_ROTATION_OFFSET_TRANSFORM;
                                    break;
                            }
                        }
                        if(GUILayout.Button(Strings.Buttons.setFromCamera))
                        {
                            SerialTransform st = null;
                            switch(Settings.centerCameraMode)
                            {
#if VRC_SDK_VRCSDK2 || (VRC_SDK_VRCSDK3 && !UDON)
                                case PumkinsCameraPreset.CameraOffsetMode.Viewpoint:
                                    st = PumkinsCameraPreset.GetCameraOffsetFromViewpoint(SelectedAvatar, SelectedCamera);
                                    if(st)
                                    {
                                        Settings.centerCameraPositionOffsetViewpoint = Helpers.RoundVectorValues(st.localPosition, 3);
                                        Settings.centerCameraRotationOffsetViewpoint = Helpers.RoundVectorValues(st.localEulerAngles, 3);
                                    }
                                    break;
#endif
                                case PumkinsCameraPreset.CameraOffsetMode.AvatarRoot:
                                    st = PumkinsCameraPreset.GetOffsetsFromTransform(SelectedAvatar.transform, SelectedCamera);
                                    if(st)
                                    {
                                        Settings.centerCameraPositionOffsetAvatar = Helpers.RoundVectorValues(st.localPosition, 3);
                                        Settings.centerCameraRotationOffsetAvatar = Helpers.RoundVectorValues(st.localEulerAngles, 3);
                                    }
                                    break;
                                case PumkinsCameraPreset.CameraOffsetMode.Transform:
                                    st = PumkinsCameraPreset.GetOffsetsFromTransform(Settings.centerCameraTransform, SelectedCamera);
                                    if(st)
                                    {
                                        Settings.centerCameraPositionOffsetTransform = Helpers.RoundVectorValues(st.localPosition, 3);
                                        Settings.centerCameraRotationOffsetTransform = Helpers.RoundVectorValues(st.localEulerAngles, 3);
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

        void DrawAvatarInfoMenuGUI()
        {
            if(Settings._avatarInfo_expand = GUILayout.Toggle(Settings._avatarInfo_expand, Strings.Main.avatarInfo, Styles.Foldout_title))
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

        void DrawToolsMenuGUI()
        {
            if(Settings._tools_expand = GUILayout.Toggle(Settings._tools_expand, Strings.Main.tools, Styles.Foldout_title))
            {
                EditorGUI.BeginDisabledGroup(SelectedAvatar == null);
                {
                    Helpers.DrawGUILine();

                    //Quick setup
                    GUILayout.BeginHorizontal();
                    {
                        if(GUILayout.Button(Strings.Buttons.quickSetupAvatar, Styles.BigButton))
                        {
                            //if(settings._tools_quickSetup_autoRig)
                            //    SetupRig(SelectedAvatar);
#if VRC_SDK_VRCSDK2 || (VRC_SDK_VRCSDK3 && !UDON)
                            if(Settings._tools_quickSetup_fillVisemes)
                                DoAction(SelectedAvatar, ToolMenuActions.FillVisemes);
                            if(Settings._tools_quickSetup_setViewpoint)
                                QuickSetViewpoint(SelectedAvatar, Settings._tools_quickSetup_viewpointZDepth);
#endif
                            if(Settings._tools_quickSetup_forceTPose)
                                DoAction(SelectedAvatar, ToolMenuActions.SetTPose);

                            //Set renderer anchor
                            if(Settings._tools_quicksetup_setMeshRendererAnchor_usePath)
                                SetRendererAnchor(SelectedAvatar, Settings._tools_quickSetup_setRenderAnchor_path, 
                                    Settings._tools_quickSetup_setMeshRendererAnchor, Settings._tools_quickSetup_setSkinnedMeshRendererAnchor);
                            else
                                SetRendererAnchor(SelectedAvatar, Settings._tools_quickSetup_setRenderAnchor_bone, 
                                    Settings._tools_quickSetup_setMeshRendererAnchor, Settings._tools_quickSetup_setSkinnedMeshRendererAnchor);
                            foreach (var tool in newTools)
                                if(tool.quickSetupActive)
                                    tool.TryExecute(SelectedAvatar);
                        }

                        if(GUILayout.Button(Icons.Settings, Styles.BigIconButton))
                            Settings._tools_quickSetup_settings_expand = !Settings._tools_quickSetup_settings_expand;
                    }
                    GUILayout.EndHorizontal();

                    if(Settings._tools_quickSetup_settings_expand)
                    {
                        EditorGUILayout.Space();

#if VRC_SDK_VRCSDK2 || (VRC_SDK_VRCSDK3 && !UDON)
                        GUILayout.BeginHorizontal();
                        {
                            Vector2 size = EditorStyles.toggle.CalcSize(new GUIContent(Strings.Tools.autoViewpoint));
                            Settings._tools_quickSetup_setViewpoint = GUILayout.Toggle(Settings._tools_quickSetup_setViewpoint, Strings.Tools.autoViewpoint, GUILayout.MaxWidth(size.x));

                            size = EditorStyles.numberField.CalcSize(new GUIContent(Strings.Tools.viewpointZDepth));
                            EditorGUI.BeginDisabledGroup(!Settings._tools_quickSetup_setViewpoint);
                            {
                                float old = EditorGUIUtility.labelWidth;
                                EditorGUIUtility.labelWidth = size.x * 1.1f;
                                Settings._tools_quickSetup_viewpointZDepth = EditorGUILayout.FloatField(Strings.Tools.viewpointZDepth, Settings._tools_quickSetup_viewpointZDepth);
                                EditorGUIUtility.labelWidth = old;
                            }
                            EditorGUI.EndDisabledGroup();
                        }
                        GUILayout.EndHorizontal();

                        Settings._tools_quickSetup_fillVisemes = GUILayout.Toggle(Settings._tools_quickSetup_fillVisemes, Strings.Tools.fillVisemes);
#endif
                        Settings._tools_quickSetup_forceTPose = GUILayout.Toggle(Settings._tools_quickSetup_forceTPose, Strings.Tools.setTPose);

                        Helpers.DrawGUILine();

                        bool anchorUsePath = Settings._tools_quicksetup_setMeshRendererAnchor_usePath;
                        
                        Settings._tools_quicksetup_setMeshRendererAnchor_usePath =
                            EditorGUILayout.ToggleLeft(Strings.Tools.anchorUsePath, anchorUsePath);

                        if(anchorUsePath)
                        {
                            GUILayout.BeginHorizontal();
                            {
                                GUILayout.Label(Strings.Tools.anchorPath);
                                Settings._tools_quickSetup_setRenderAnchor_path =
                                    EditorGUILayout.TextField(Settings._tools_quickSetup_setRenderAnchor_path);
                            }
                            GUILayout.EndHorizontal();
                        }
                        else
                        {
                            Settings._tools_quickSetup_setRenderAnchor_bone =
                                (HumanBodyBones)EditorGUILayout.EnumPopup(Strings.Tools.humanoidBone, Settings._tools_quickSetup_setRenderAnchor_bone);
                        }

                        EditorGUILayout.Space();

                        bool disabled =
                            anchorUsePath && Helpers.StringIsNullOrWhiteSpace(Settings._tools_quickSetup_setRenderAnchor_path);
                        EditorGUI.BeginDisabledGroup(disabled);
                        {
                            Settings._tools_quickSetup_setSkinnedMeshRendererAnchor = GUILayout.Toggle(Settings._tools_quickSetup_setSkinnedMeshRendererAnchor, Strings.Tools.setSkinnedMeshRendererAnchors);
                            Settings._tools_quickSetup_setMeshRendererAnchor = GUILayout.Toggle(Settings._tools_quickSetup_setMeshRendererAnchor, Strings.Tools.setMeshRendererAnchors);
                        }
                        EditorGUI.EndDisabledGroup();

                        EditorGUILayout.Space();

                        foreach (var tool in newTools)
                            tool.DrawQuickSetupGUI();

                    }

                    Helpers.DrawGUILine();

                    //Tools
                    if(Settings._tools_avatar_expand = GUILayout.Toggle(Settings._tools_avatar_expand, Strings.Main.avatar, Styles.Foldout))
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
                                EditorGUI.BeginDisabledGroup(DrawingHandlesGUI);
                                {
                                    if(GUILayout.Button(Strings.Tools.revertScale))
                                        DoAction(SelectedAvatar, ToolMenuActions.RevertScale);
                                }
                                EditorGUI.EndDisabledGroup();
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
                            }
                            GUILayout.EndVertical();
                        }
                        GUILayout.EndHorizontal();

                        EditorGUI.BeginDisabledGroup(DrawingHandlesGUI);
                        {
                            if(GUILayout.Button(Strings.Tools.editScale))
                                DoAction(SelectedAvatar, ToolMenuActions.EditScale);
                        }
                        EditorGUI.EndDisabledGroup();

                        using (new GUILayout.HorizontalScope())
                        {
                            if(GUILayout.Button(Strings.Tools.resetPose))
                                DoAction(SelectedAvatar, ToolMenuActions.ResetPose);
                            Settings._tools_avatar_resetpose_expand = GUILayout.Toggle(Settings._tools_avatar_resetpose_expand, EditorGUIUtility.IconContent("align_vertically_center"), "button", GUILayout.Width(20));
                        }

                        if(Settings._tools_avatar_resetpose_expand)
                        {
                            using (new GUILayout.VerticalScope("helpbox"))
                            {
                                Settings._tools_avatar_resetPose_type = (SettingsContainer.ResetPoseType) EditorGUILayout.EnumPopup("Reset To",Settings._tools_avatar_resetPose_type);
                                
                                using (new EditorGUI.DisabledScope(Settings._tools_avatar_resetPose_type == SettingsContainer.ResetPoseType.TPose))
                                {
                                    Settings._tools_avatar_resetPose_position = EditorGUILayout.Toggle("Position", Settings._tools_avatar_resetPose_position);
                                    Settings._tools_avatar_resetPose_rotation = EditorGUILayout.Toggle("Rotation", Settings._tools_avatar_resetPose_rotation);
                                    Settings._tools_avatar_resetPose_scale = EditorGUILayout.Toggle("Scale", Settings._tools_avatar_resetPose_scale);
                                }

                                using (new EditorGUI.DisabledScope(Settings._tools_avatar_resetPose_type != SettingsContainer.ResetPoseType.AvatarDefinition))
                                    Settings._tools_avatar_resetPose_fullreset = EditorGUILayout.Toggle(new GUIContent("Full Reset","Reset all the objects included in the Avatar definition."), Settings._tools_avatar_resetPose_fullreset);

                            }
                        }


                        Helpers.DrawGUILine();

                        bool anchorUsePath = Settings._tools_quicksetup_setMeshRendererAnchor_usePath;
                        Settings._tools_quicksetup_setMeshRendererAnchor_usePath = EditorGUILayout.ToggleLeft(
                            Strings.Tools.anchorUsePath, anchorUsePath);

                        if(anchorUsePath)
                        {
                            Settings._tools_quickSetup_setRenderAnchor_path = EditorGUILayout.TextField(Strings.Tools.anchorPath,
                                Settings._tools_quickSetup_setRenderAnchor_path);

                            bool disabled = anchorUsePath &&
                                            string.IsNullOrWhiteSpace(Settings._tools_quickSetup_setRenderAnchor_path);
                            EditorGUI.BeginDisabledGroup(disabled);
                            {
                                if(GUILayout.Button(Strings.Tools.setSkinnedMeshRendererAnchors))
                                    SetRendererAnchor(SelectedAvatar, Settings._tools_quickSetup_setRenderAnchor_path, false, true);
                                if(GUILayout.Button(Strings.Tools.setMeshRendererAnchors))
                                    SetRendererAnchor(SelectedAvatar, Settings._tools_quickSetup_setRenderAnchor_path, true, false);
                            }
                            EditorGUI.EndDisabledGroup();
                        }
                        else
                        {
                            Settings._tools_quickSetup_setRenderAnchor_bone =
                                (HumanBodyBones)EditorGUILayout.EnumPopup(Strings.Tools.humanoidBone, Settings._tools_quickSetup_setRenderAnchor_bone);

                            if(GUILayout.Button(Strings.Tools.setSkinnedMeshRendererAnchors))
                                SetRendererAnchor(SelectedAvatar, Settings._tools_quickSetup_setRenderAnchor_bone, false, true);
                            if(GUILayout.Button(Strings.Tools.setMeshRendererAnchors))
                                SetRendererAnchor(SelectedAvatar, Settings._tools_quickSetup_setRenderAnchor_bone, true, false);
                        }
                        EditorGUILayout.Space();

                        foreach (var tool in newTools)
                            tool.DrawGUI();
                    }

                    Helpers.DrawGUILine();

                    //Phys bones toggle

                    //Setup pbone gui stuff
                    string pboneStateString = Strings.Copier.physBones;
                    if(!PhysBonesExist)
                        pboneStateString += " | SDK version 2022.03.04.12.28 or newer required";

                    if(Settings._tools_physBones_expand = GUILayout.Toggle(Settings._tools_physBones_expand, pboneStateString, Styles.Foldout))
                    {
                        EditorGUI.BeginDisabledGroup(!PhysBonesExist);
                        {
                            EditorGUILayout.BeginHorizontal();
                            {
                                if(GUILayout.Button(Strings.Tools.disablePhysBones))
                                    SetPhysBonesEnabledState(SelectedAvatar, false);
                                if(GUILayout.Button(Strings.Tools.enablePhysBones))
                                    SetPhysBonesEnabledState(SelectedAvatar, true);
                            }
                            EditorGUILayout.EndHorizontal();

                            if(DrawToggleButtonGUI(Strings.Tools.togglePhysBones, _nextTogglePBoneState))
                                TogglePhysBonesEnabledState(SelectedAvatar, ref _nextTogglePBoneState, ref _pBonesThatWereAlreadyDisabled);

                            EditorGUILayout.Space();
                        }
                        EditorGUI.EndDisabledGroup();
                    }
                    EditorGUI.EndDisabledGroup();
                    EditorGUILayout.Space();

                    //Dynamic bones toggle

                    //Setup dbone gui stuff
                    string dboneStateString = Strings.Copier.dynamicBones;
                    if(!DynamicBonesExist)
                        dboneStateString += " (" + Strings.Warning.notFound + ")";

                    if(Settings._tools_dynamicBones_expand = GUILayout.Toggle(Settings._tools_dynamicBones_expand, dboneStateString, Styles.Foldout))
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
                        }
                        EditorGUI.EndDisabledGroup();
                    }
                    EditorGUI.EndDisabledGroup();
                    EditorGUILayout.Space();
                }
            }
        }

        void DrawAvatarTestingMenuGUI()
        {
            //TODO: Improve and revert
#if PUMKIN_DEV
            if(Settings._avatar_testing_expand = GUILayout.Toggle(Settings._avatar_testing_expand, Strings.Main.avatarTesting, Styles.Foldout_title))
            {
                EditorGUI.BeginDisabledGroup(!SelectedAvatar);
                EditorGUILayout.Space();

                if(GUILayout.Button(Strings.Buttons.toggleMaterialPreview))
                    PumkinsFallbackPreview.TogglePreview(SelectedAvatar);

                EditorGUI.EndDisabledGroup();
            }
#endif
        }

        void DrawRemoveComponentsMenuGUI()
        {

            if(Settings._tools_removeAll_expand = GUILayout.Toggle(Settings._tools_removeAll_expand, Strings.Main.removeAll, Styles.Foldout_title))
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true)); //Left Column
                    {
                        EditorGUI.BeginDisabledGroup(!PhysBonesExist);
                        {
                            if(GUILayout.Button(new GUIContent(Strings.Copier.physBones, Icons.PhysBone)))
                                DoAction(SelectedAvatar, ToolMenuActions.RemovePhysBones);
                        }
                        EditorGUI.EndDisabledGroup();
                        EditorGUI.BeginDisabledGroup(!DynamicBonesExist);
                        {
                            if(GUILayout.Button(new GUIContent(Strings.Copier.dynamicBones, Icons.BoneIcon)))
                                DoAction(SelectedAvatar, ToolMenuActions.RemoveDynamicBones);
                        }
                        EditorGUI.EndDisabledGroup();
                        EditorGUI.BeginDisabledGroup(!PhysBonesExist);
                        {
                            if(GUILayout.Button(new GUIContent(Strings.Copier.contactReceiver, Icons.ContactReceiver)))
                                DoAction(SelectedAvatar, ToolMenuActions.RemoveContactReceiver);
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
                        if(GUILayout.Button(new GUIContent(Strings.Copier.cameras, Icons.Camera)))
                            DoAction(SelectedAvatar, ToolMenuActions.RemoveCameras);
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
                        
                        #if PUMKIN_FINALIK
                        
                        EditorGUILayout.Space();
                        if(GUILayout.Button(new GUIContent(Strings.Copier.finalIK_fabrIK, Icons.FinalIK_FabrIK)))
                            DoAction(SelectedAvatar, ToolMenuActions.RemoveFinalIK_FabrIK);
                        if(GUILayout.Button(new GUIContent(Strings.Copier.finalIK_aimIK, Icons.FINALIK_AimIK)))
                            DoAction(SelectedAvatar, ToolMenuActions.RemoveFinalIK_AimIK);
                        if(GUILayout.Button(new GUIContent(Strings.Copier.finalIK_fbtBipedIK, Icons.FinalIK_fbtBipedIK)))
                            DoAction(SelectedAvatar, ToolMenuActions.RemoveFinalIK_FbtBipedIK);
                        if(GUILayout.Button(new GUIContent(Strings.Copier.finalIK_VRIK, Icons.FinalIK_vrIK)))
                            DoAction(SelectedAvatar, ToolMenuActions.RemoveFinalIK_VRIK);
                            
                        #endif
                    }
                    EditorGUILayout.EndVertical();

                    EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true)); //Right Column
                    {
                        EditorGUI.BeginDisabledGroup(!PhysBonesExist);
                        {
                            if(GUILayout.Button(new GUIContent(Strings.Copier.physBones_colliders, Icons.PhysBoneCollider)))
                                DoAction(SelectedAvatar, ToolMenuActions.RemovePhysBoneColliders);
                        }
                        EditorGUI.EndDisabledGroup();
                        EditorGUI.BeginDisabledGroup(!DynamicBonesExist);
                        {
                            if(GUILayout.Button(new GUIContent(Strings.Copier.dynamicBones_colliders, Icons.BoneColliderIcon)))
                                DoAction(SelectedAvatar, ToolMenuActions.RemoveDynamicBoneColliders);
                        }
                        EditorGUI.BeginDisabledGroup(!PhysBonesExist);
                        {
                            if(GUILayout.Button(new GUIContent(Strings.Copier.contactSender, Icons.ContactSender)))
                                DoAction(SelectedAvatar, ToolMenuActions.RemoveContactSender);
                        }
                        EditorGUI.EndDisabledGroup();
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
                        
                        GUILayout.Space(21);
                        
                        EditorGUILayout.Space();
                        if(GUILayout.Button(
                            new GUIContent(Strings.Copier.positionConstraints, Icons.PositionConstraint)))
                            DoAction(SelectedAvatar, ToolMenuActions.RemovePositionConstraint);
                        if(GUILayout.Button(
                            new GUIContent(Strings.Copier.rotationConstraints, Icons.RotationConstraint)))
                            DoAction(SelectedAvatar, ToolMenuActions.RemoveRotationConstraint);
                        if(GUILayout.Button(new GUIContent(Strings.Copier.scaleConstraints, Icons.ScaleConstraint)))
                            DoAction(SelectedAvatar, ToolMenuActions.RemoveScaleConstraint);
                        
                        #if PUMKIN_FINALIK
                        EditorGUILayout.Space();
                        if(GUILayout.Button(new GUIContent(Strings.Copier.finalIK_ccdIK, Icons.FinalIK_CCDIK)))
                            DoAction(SelectedAvatar, ToolMenuActions.RemoveFinalIK_CCDIK);
                        if(GUILayout.Button(new GUIContent(Strings.Copier.finalIK_limbIK, Icons.FINALIK_LimbIK)))
                            DoAction(SelectedAvatar, ToolMenuActions.RemoveFinalIK_LimbIK);
                        if(GUILayout.Button(new GUIContent(Strings.Copier.finalIK_rotationLimits, Icons.FinalIK_RotationLimits)))
                            DoAction(SelectedAvatar, ToolMenuActions.RemoveFinalIK_RotationLimits);
                        #endif
                    }
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndHorizontal();
            }
        }


        public bool DrawToggleButtonGUI(string text, ref bool toggleBool)
        {
            bool b = GUILayout.Button(new GUIContent(text, toggleBool ? Icons.ToggleOff : Icons.ToggleOn), Styles.ButtonWithToggle);
            if(b)
                toggleBool = !toggleBool;
            return b;
        }

        bool DrawToggleButtonGUI(string text, bool toggleBool)
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
                pSelectedPresetString = Settings.SerializedSettings.FindProperty("_selectedCameraPresetString");
                pSelectedPresetIndex = Settings.SerializedSettings.FindProperty("_selectedCameraPresetIndex");

                pr = PumkinsPresetManager.CameraPresets.Cast<PumkinPreset>().ToList();

                labelString = Strings.Thumbnails.cameras;
                dropdownOptions = PumkinsPresetManager.CameraPresets.Select(o => o.name);
            }
            else if(typeof(T) == typeof(PumkinsPosePreset))
            {
                pSelectedPresetString = Settings.SerializedSettings.FindProperty("_selectedPosePresetString");
                pSelectedPresetIndex = Settings.SerializedSettings.FindProperty("_selectedPosePresetIndex");

                pr = PumkinsPresetManager.PosePresets.Cast<PumkinPreset>().ToList();

                labelString = Strings.Thumbnails.poses;
                dropdownOptions = PumkinsPresetManager.PosePresets.Select(o => o.name);
            }
            else if(typeof(T) == typeof(PumkinsBlendshapePreset))
            {
                pSelectedPresetString = Settings.SerializedSettings.FindProperty("_selectedBlendshapePresetString");
                pSelectedPresetIndex = Settings.SerializedSettings.FindProperty("_selectedBlendshapePresetIndex");

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
                        CenterCameraOnViewpoint(SelectedAvatar, DEFAULT_CAMERA_POSITION_OFFSET_VIEWPOINT,
                            DEFAULT_CAMERA_ROTATION_OFFSET_VIEWPOINT, Settings.centerCameraFixClippingPlanes);
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

            Settings.SerializedSettings.ApplyModifiedProperties();
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
            if(Helpers.DrawDropdownWithToggle(ref Settings._thumbnails_useCameraBackground_expand,
                ref Settings.bThumbnails_use_camera_background, Strings.Thumbnails.useCameraBackground))
            {
                RefreshBackgroundOverrideType();
                needsRefresh = true;

                if(Settings.bThumbnails_use_camera_background && _selectedCamera)
                    Settings._thumbsCameraBgClearFlagsOld = SelectedCamera.clearFlags;
                else
                    RestoreCameraClearFlags();
            }
            EditorGUI.EndDisabledGroup();

            if(Settings._thumbnails_useCameraBackground_expand || needsRefresh)
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
                                    Settings._thumbsCamBgColor = EditorGUILayout.ColorField(Strings.Thumbnails.backgroundType_Color, _selectedCamera ? SelectedCamera.backgroundColor : Color.grey);
                                }
                                if(EditorGUI.EndChangeCheck())
                                {
                                    SetCameraBackgroundToColor(Settings._thumbsCamBgColor);
                                }
                            }
                            break;
                        case PumkinsCameraPreset.CameraBackgroundOverrideType.Skybox:
                            {
                                if(Settings.bThumbnails_use_camera_background && _selectedCamera)
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
                                if(Settings.bThumbnails_use_camera_background && _selectedCamera)
                                    SelectedCamera.clearFlags = Settings._thumbsCameraBgClearFlagsOld;

                                EditorGUILayout.Space();
                                GUILayout.BeginHorizontal();
                                {
                                    EditorGUILayout.SelectableLabel(Settings._backgroundPath, Styles.TextField);
                                    if(GUILayout.Button(Strings.Buttons.browse, GUILayout.MaxWidth(60)) && SelectedCamera)
                                    {
                                        string newPath = Helpers.OpenImageGetPath(Settings._lastOpenFilePath);
                                        if(!string.IsNullOrEmpty(newPath))
                                        {
                                            Settings._lastOpenFilePath = newPath;
                                            SetBackgroundToImageFromPath(Settings._lastOpenFilePath);
                                        }
                                    }
                                    if(GUILayout.Button("X", GUILayout.MaxWidth(25)))
                                    {
                                        Settings._backgroundPath = null;
                                        SetBackgroundToImageFromTexture((Texture2D)null);
                                    }
                                }
                                EditorGUILayout.EndHorizontal();

                                EditorGUI.BeginDisabledGroup(!cameraBackgroundTexture);
                                {
                                    EditorGUI.BeginChangeCheck();
                                    {
                                        Settings.cameraBackgroundImageTint = EditorGUILayout.ColorField(Strings.Thumbnails.tint, Settings.cameraBackgroundImageTint);
                                    }
                                    if(EditorGUI.EndChangeCheck())
                                    {
                                        if(raw)
                                            raw.color = Settings.cameraBackgroundImageTint;
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
            if(Helpers.DrawDropdownWithToggle(ref Settings._thumbnails_useCameraOverlay_expand,
                ref Settings.bThumbnails_use_camera_overlay, Strings.Thumbnails.useCameraOverlay))
            {
                if(cameraOverlayTexture == null && !string.IsNullOrEmpty(Settings._overlayPath))
                    SetOverlayToImageFromPath(Settings._overlayPath);

                needsRefresh = true;
            }
            EditorGUI.EndDisabledGroup();

            if(Settings._thumbnails_useCameraOverlay_expand || needsRefresh)
            {
                EditorGUI.BeginDisabledGroup(!Settings.bThumbnails_use_camera_overlay);
                {
                    EditorGUILayout.Space();
                    GUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.SelectableLabel(Settings._overlayPath, Styles.TextField);
                        if(GUILayout.Button(Strings.Buttons.browse, GUILayout.MaxWidth(60)) && SelectedCamera)
                        {
                            string newPath = Helpers.OpenImageGetPath(Settings._lastOpenFilePath);
                            if(!string.IsNullOrEmpty(newPath))
                            {
                                Settings._lastOpenFilePath = newPath;
                                SetOverlayToImageFromPath(Settings._lastOpenFilePath);
                            }
                        }
                        if(GUILayout.Button("X", GUILayout.MaxWidth(25)))
                        {
                            Settings._overlayPath = null;
                            SetOverlayToImageFromTexture(null);
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUI.BeginDisabledGroup(!cameraOverlayTexture);
                    {
                        EditorGUI.BeginChangeCheck();
                        {
                            Settings.cameraOverlayImageTint = EditorGUILayout.ColorField(Strings.Thumbnails.tint, Settings.cameraOverlayImageTint);
                        }
                        if(EditorGUI.EndChangeCheck())
                        {
                            if(raw)
                                raw.color = Settings.cameraOverlayImageTint;
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
            Settings._presetToolbarSelectedIndex = (int)option;
            Settings._mainToolsScrollbar = new Vector2(0, 1000);
        }

#endregion

#region Main Functions

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

        /// <summary>
        /// Loads textures back into overlay and background objects if we have a path for them still stored. Useful for when we restart unity
        /// </summary>
        private void RestoreTexturesFromPaths()
        {
            RawImage overlayImg = GetCameraOverlayRawImage(Settings.bThumbnails_use_camera_overlay);
            RawImage backgroundImg = GetCameraBackgroundRawImage(Settings.bThumbnails_use_camera_background);

            if(!string.IsNullOrEmpty(Settings._overlayPath))
            {
                if(overlayImg)
                {
                    if(overlayImg.texture)
                    {
                        cameraOverlayTexture = (Texture2D)overlayImg.texture;
                        overlayImg.color = Settings.cameraOverlayImageTint;
                    }
                    else
                    {
                        SetOverlayToImageFromPath(Settings._overlayPath);
                    }
                }
            }
            else if(overlayImg && overlayImg.texture)
            {
                cameraOverlayTexture = null;
                overlayImg.texture = null;
            }

            if(!string.IsNullOrEmpty(Settings._backgroundPath))
            {
                if(backgroundImg)
                {
                    if(backgroundImg.texture)
                    {
                        cameraBackgroundTexture = (Texture2D)backgroundImg.texture;
                        backgroundImg.color = Settings.cameraBackgroundImageTint;
                    }
                    else
                    {
                        SetBackgroundToImageFromPath(Settings._backgroundPath);
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
            Settings._overlayPath = texturePath;
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
                img.color = Settings.cameraOverlayImageTint;
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
            Settings._backgroundPath = texturePath;
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
                img.color = Settings.cameraBackgroundImageTint;
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

            Settings._thumbsCamBgColor = color;
            SelectedCamera.backgroundColor = color;
            SelectedCamera.clearFlags = CameraClearFlags.SolidColor;
        }


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
                    else if(!SettingsContainer._useSceneSelectionAvatar)
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
                    LegacyDestroyer.DestroyAllComponentsOfType(SelectedAvatar, typeof(Collider), false, false);
                    break;
                case ToolMenuActions.RemovePhysBoneColliders:
#if PUMKIN_PBONES
                    LegacyDestroyer.DestroyAllComponentsOfType(SelectedAvatar, typeof(VRCPhysBoneCollider), false, false);
                    CleanupPhysBonesColliderArraySizes();
#endif
                    break;
                case ToolMenuActions.RemovePhysBones:
#if PUMKIN_PBONES

                    LegacyDestroyer.DestroyAllComponentsOfType(SelectedAvatar, typeof(VRCPhysBone), false, false);
#endif
                    break;
                case ToolMenuActions.RemoveDynamicBoneColliders:
#if PUMKIN_DBONES || PUMKIN_OLD_DBONES
                    LegacyDestroyer.DestroyAllComponentsOfType(SelectedAvatar, typeof(DynamicBoneCollider), false, false);
                    CleanupDynamicBonesColliderArraySizes();

#endif
                    break;
                case ToolMenuActions.RemoveDynamicBones:
#if PUMKIN_DBONES || PUMKIN_OLD_DBONES
                    LegacyDestroyer.DestroyAllComponentsOfType(SelectedAvatar, typeof(DynamicBone), false, false);
#endif
                    break;
                case ToolMenuActions.RemoveContactReceiver:
#if PUMKIN_PBONES
                    LegacyDestroyer.DestroyAllComponentsOfType(SelectedAvatar, typeof(VRCContactReceiver), false, false);
#endif
                    break;
                case ToolMenuActions.RemoveContactSender:
#if PUMKIN_PBONES
                    LegacyDestroyer.DestroyAllComponentsOfType(SelectedAvatar, typeof(VRCContactSender), false, false);
#endif
                    break;
                case ToolMenuActions.ResetPose:
                    switch (Settings._tools_avatar_resetPose_type)
                    {
                        case SettingsContainer.ResetPoseType.Prefab:
                            ResetPose(SelectedAvatar);
                            break;
                        case SettingsContainer.ResetPoseType.AvatarDefinition:
                            ResetToAvatarDefinition(SelectedAvatar, Settings._tools_avatar_resetPose_fullreset, Settings._tools_avatar_resetPose_position, Settings._tools_avatar_resetPose_rotation, Settings._tools_avatar_resetPose_scale);
                            break;
                        case SettingsContainer.ResetPoseType.TPose:
                            PumkinsPoseEditor.SetTPoseHardcoded(SelectedAvatar);
                            break;
                    }
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
                    LegacyDestroyer.DestroyEmptyGameObjects(SelectedAvatar);
                    break;
                case ToolMenuActions.RemoveParticleSystems:
                    LegacyDestroyer.DestroyAllComponentsOfType(SelectedAvatar, typeof(ParticleSystemRenderer), false, false);
                    LegacyDestroyer.DestroyAllComponentsOfType(SelectedAvatar, typeof(ParticleSystem), false, false);
                    break;
                case ToolMenuActions.RemoveRigidBodies:
                    LegacyDestroyer.DestroyAllComponentsOfType(SelectedAvatar, typeof(Rigidbody), false, false);
                    break;
                case ToolMenuActions.RemoveTrailRenderers:
                    LegacyDestroyer.DestroyAllComponentsOfType(SelectedAvatar, typeof(TrailRenderer), false, false);
                    break;
                case ToolMenuActions.RemoveMeshRenderers:
                    LegacyDestroyer.DestroyAllComponentsOfType(SelectedAvatar, typeof(MeshFilter), false, false);
                    LegacyDestroyer.DestroyAllComponentsOfType(SelectedAvatar, typeof(MeshRenderer), false, false);
                    break;
                case ToolMenuActions.RemoveLights:
                    LegacyDestroyer.DestroyAllComponentsOfType(SelectedAvatar, typeof(Light), false, false);
                    break;
                case ToolMenuActions.RemoveAnimatorsInChildren:
                    LegacyDestroyer.DestroyAllComponentsOfType(SelectedAvatar, typeof(Animator), true, false);
                    break;
                case ToolMenuActions.RemoveAudioSources:
#if VRC_SDK_VRCSDK2 || (VRC_SDK_VRCSDK3 && !UDON)
                    LegacyDestroyer.DestroyAllComponentsOfType(SelectedAvatar, typeof(VRC_SpatialAudioSource), false, false);
#endif
                    LegacyDestroyer.DestroyAllComponentsOfType(SelectedAvatar, typeof(AudioSource), false, false);
                    break;
                case ToolMenuActions.RemoveJoints:
                    LegacyDestroyer.DestroyAllComponentsOfType(SelectedAvatar, typeof(Joint), false, false);
                    break;
                case ToolMenuActions.EditScale:
                    BeginScalingAvatar(SelectedAvatar);
                    break;
                case ToolMenuActions.RevertScale:
                    RevertScale(SelectedAvatar);
                    break;
#if VRC_SDK_VRCSDK2 || (VRC_SDK_VRCSDK3 && !UDON)
                case ToolMenuActions.RemoveIKFollowers:
                    LegacyDestroyer.DestroyAllComponentsOfType(SelectedAvatar, typeof(VRC_IKFollower), false, false);
                    break;
#endif
                case ToolMenuActions.RemoveMissingScripts:
                    LegacyDestroyer.DestroyMissingScripts(SelectedAvatar);
                    break;
                case ToolMenuActions.RemoveAimConstraint:
                    LegacyDestroyer.DestroyAllComponentsOfType(SelectedAvatar, typeof(AimConstraint), false, false);
                    break;
                case ToolMenuActions.RemoveLookAtConstraint:
                    LegacyDestroyer.DestroyAllComponentsOfType(SelectedAvatar, typeof(LookAtConstraint), false, false);
                    break;
                case ToolMenuActions.RemoveParentConstraint:
                    LegacyDestroyer.DestroyAllComponentsOfType(SelectedAvatar, typeof(ParentConstraint), false, false);
                    break;
                case ToolMenuActions.RemovePositionConstraint:
                    LegacyDestroyer.DestroyAllComponentsOfType(SelectedAvatar, typeof(PositionConstraint), false, false);
                    break;
                case ToolMenuActions.RemoveRotationConstraint:
                    LegacyDestroyer.DestroyAllComponentsOfType(SelectedAvatar, typeof(RotationConstraint), false, false);
                    break;
                case ToolMenuActions.RemoveScaleConstraint:
                    LegacyDestroyer.DestroyAllComponentsOfType(SelectedAvatar, typeof(ScaleConstraint), false, false);
                    break;
                case ToolMenuActions.RemoveCameras:
                    LegacyDestroyer.DestroyAllComponentsOfType(SelectedAvatar, typeof(Camera), false, false);
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
                case ToolMenuActions.RemoveFinalIK_CCDIK:
                    LegacyDestroyer.DestroyAllComponentsOfType(SelectedAvatar, PumkinsTypeCache.CCDIK, false, false);
                    break;
                case ToolMenuActions.RemoveFinalIK_LimbIK:
                    LegacyDestroyer.DestroyAllComponentsOfType(SelectedAvatar, PumkinsTypeCache.LimbIK, false, false);
                    break;
                case ToolMenuActions.RemoveFinalIK_RotationLimits:
                    LegacyDestroyer.DestroyAllComponentsOfType(SelectedAvatar, PumkinsTypeCache.RotationLimit, false, false);
                    break;
                case ToolMenuActions.RemoveFinalIK_FabrIK:
                    LegacyDestroyer.DestroyAllComponentsOfType(SelectedAvatar, PumkinsTypeCache.FABRIK, false, false);
                    break;
                case ToolMenuActions.RemoveFinalIK_AimIK:
                    LegacyDestroyer.DestroyAllComponentsOfType(SelectedAvatar, PumkinsTypeCache.AimIK, false, false);
                    break;
                case ToolMenuActions.RemoveFinalIK_FbtBipedIK:
                    LegacyDestroyer.DestroyAllComponentsOfType(SelectedAvatar, PumkinsTypeCache.FullBodyBipedIK, false, false);
                    break;
                case ToolMenuActions.RemoveFinalIK_VRIK:
                    LegacyDestroyer.DestroyAllComponentsOfType(SelectedAvatar, PumkinsTypeCache.VRIK, false, false);
                    break;
                default:
                    break;
            }

            avatarInfo = PumkinsAvatarInfo.GetInfo(SelectedAvatar, out _avatarInfoString);

            EditorUtility.SetDirty(SelectedAvatar);
            if(!EditorApplication.isPlaying)
                EditorSceneManager.MarkSceneDirty(SelectedAvatar.scene);
        }

        private static void CleanupPhysBonesColliderArraySizes()
        {
#if PUMKIN_PBONES
            var pbones = SelectedAvatar.GetComponentsInChildren<VRCPhysBone>(true);
            if(pbones != null && pbones.Length > 0)
            {
                SerializedObject so = new SerializedObject(pbones);
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
        /// Sets the enabled state on all phys bones on the avatar and returns affected bones
        /// </summary>
        /// <param name="enabled">Enabled state for phys bones</param>
        /// <param name="pBonesToIgnore">Phys Bones to ignore</param>
        /// <returns>Phys Bones that were disabled before we did anything</returns>
#if PUMKIN_PBONES
        static void SetPhysBonesEnabledState(GameObject avatar, bool enabled, List<VRCPhysBone> pBonesToIgnore = null)
        {
            if(!avatar)
                return;

            foreach(var bone in avatar.GetComponentsInChildren<VRCPhysBone>(true))
                if(pBonesToIgnore == null || !pBonesToIgnore.Contains(bone))
                    bone.enabled = enabled;
        }
#else
        static void SetPhysBonesEnabledState(GameObject avatar, bool enabled)
        {
            return;
        }
#endif
        /// <summary>
        /// Toggles the enbaled state of all phys Bones on the avatar and returns affected bones
        /// </summary>
        /// <param name="enabledState">Bool to use as toggle state</param>
        /// <param name="pBonesToIgnore">Phys Bones to ignore</param>
        /// <returns>Phys Bones that have been enabled or disabled. Used to ignore bones that were disabled before we toggled off</returns>
#if PUMKIN_PBONES
        static void TogglePhysBonesEnabledState(GameObject avatar, ref bool enabledState, ref List<VRCPhysBone> pBonesToIgnore)
        {

            if(!enabledState)
            {
                pBonesToIgnore = new List<VRCPhysBone>();
                var bones = avatar.GetComponentsInChildren<VRCPhysBone>(true);
                foreach(var b in bones)
                    if(!b.enabled)
                        pBonesToIgnore.Add(b);
            }
            SetPhysBonesEnabledState(avatar, enabledState, pBonesToIgnore);
            enabledState = !enabledState;

        }
#else
        static void TogglePhysBonesEnabledState(GameObject avatar, ref bool enabledState, ref List<object> dBonesToIgnore)
        {
            return;
        }
#endif
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
            Settings._avatarScaleTemp = _avatarScaleOld.z;

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
                        if(Settings.editingScaleMovesViewpoint)
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

                if(Settings.SerializedSettings != null)
                    Settings.SerializedSettings.ApplyModifiedPropertiesWithoutUndo();
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
        private void SetRendererAnchor(GameObject avatar, string anchorPath, bool meshRenderer, bool skinnedRenderer)
        {
            Transform anchor = avatar.transform.Find(anchorPath);
            if(!anchor)
            {
                Log(Strings.Log.transformNotFound, LogType.Warning, anchorPath);
                return;
            }

            if(skinnedRenderer)
            {
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
            
            if(meshRenderer)
            {
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
        }

        private void SetRendererAnchor(GameObject avatar, HumanBodyBones humanBone, bool meshRenderer, bool skinnedRenderer)
        {
            string boneName = Enum.GetName(typeof(HumanBodyBones), humanBone);
            Transform anchor = avatar.GetComponent<Animator>()?.GetBoneTransform(humanBone);
            if(!anchor)
            {
                Log(Strings.Log.transformNotFound, LogType.Warning, boneName);
                return;
            }

            if(meshRenderer)
            {
                var meshRenderers = avatar.GetComponentsInChildren<MeshRenderer>(true);
                foreach(var render in meshRenderers)
                {
                    if(render)
                    {
                        render.probeAnchor = anchor;
                        Log(Strings.Log.setProbeAnchorTo, LogType.Log, render.name, anchor.name);
                    }
                }
            }

            if(skinnedRenderer)
            {
                var skinnedRenderers = avatar.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                foreach(var render in skinnedRenderers)
                {
                    if(render)
                    {
                        render.probeAnchor = anchor;
                        Log(Strings.Log.setProbeAnchorTo, LogType.Log, render.name, anchor.name);
                    }
                }
            }
        }

#endregion

#region Copy Functions

        /// <summary>
        /// Unparents then copies components and values from one object to another then reparents
        /// </summary>
        /// <param name="objFrom"></param>
        /// <param name="objTo"></param>
        void CopyComponentsWithoutParents(GameObject objFrom, GameObject objTo)
        {
            Transform fromParent = objFrom.transform.parent;
            Transform toParent = objTo.transform.parent;

            objFrom.transform.parent = null;
            objTo.transform.parent = null;

            try
            {
                CopyComponents(objFrom, objTo);
            }
            catch(Exception ex)
            {
                Log(ex.Message, LogType.Exception);
            }
            finally
            {
                objFrom.transform.parent = fromParent;
                objTo.transform.parent = toParent;
            }
        }

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
            
            if(Settings.bCopier_descriptor_copy &&
               CopierTabs.ComponentIsInSelectedTab(PumkinsTypeCache.VRC_AvatarDescriptor, Settings._copier_selectedTab))
            {
                LegacyCopier.CopyAvatarDescriptor(objFrom, objTo, true);
    
                if(Settings.bCopier_descriptor_copyAvatarScale)
                {
                    VRC_AvatarDescriptor descriptor = objTo.GetComponentInChildren<VRC_AvatarDescriptor>();
                    if(descriptor)
                    {
                        
                        if(!(Settings.bCopier_descriptor_copy && Settings.bCopier_descriptor_copyViewpoint))
                            SetAvatarScaleAndMoveViewpoint(descriptor, objFrom.transform.localScale.z);
                        objTo.transform.localScale = new Vector3(objFrom.transform.localScale.x
                            , objFrom.transform.localScale.y, objFrom.transform.localScale.z);
                    }
                    else
                    {
                        objTo.transform.localScale = objFrom.transform.localScale;
                    }
                }
            }
#else
            if(Settings.bCopier_descriptor_copy && Settings.bCopier_descriptor_copyAvatarScale)
                objTo.transform.localScale = objFrom.transform.localScale;
#endif
            if(Settings.bCopier_particleSystems_copy && CopierTabs.ComponentIsInSelectedTab<ParticleSystem>(Settings._copier_selectedTab))
            {
                LegacyCopier.CopyAllParticleSystems(objFrom, objTo, Settings.bCopier_particleSystems_createObjects, true);
            }
            if(Settings.bCopier_colliders_copy && CopierTabs.ComponentIsInSelectedTab<Collider>(Settings._copier_selectedTab))
            {
                if(Settings.bCopier_colliders_removeOld)
                    LegacyDestroyer.DestroyAllComponentsOfType(objTo, typeof(Collider), false, true);
                LegacyCopier.CopyAllColliders(objFrom, objTo, Settings.bCopier_colliders_createObjects, true, Settings.bCopier_colliders_adjustScale);
            }
            if(Settings.bCopier_rigidBodies_copy && CopierTabs.ComponentIsInSelectedTab<Rigidbody>(Settings._copier_selectedTab))
            {
                LegacyCopier.CopyAllRigidBodies(objFrom, objTo, Settings.bCopier_rigidBodies_createObjects, true);
            }
            if(Settings.bCopier_trailRenderers_copy && CopierTabs.ComponentIsInSelectedTab<TrailRenderer>(Settings._copier_selectedTab))
            {
                LegacyCopier.CopyAllTrailRenderers(objFrom, objTo, Settings.bCopier_trailRenderers_createObjects, true);
            }
            if(Settings.bCopier_lights_copy && CopierTabs.ComponentIsInSelectedTab<Light>(Settings._copier_selectedTab))
            {
                LegacyCopier.CopyAllLights(objFrom, objTo, Settings.bCopier_lights_createObjects, true);
            }
            if(Settings.bCopier_animators_copy && CopierTabs.ComponentIsInSelectedTab<Animator>(Settings._copier_selectedTab))
            {
                LegacyCopier.CopyAllAnimators(objFrom, objTo, Settings.bCopier_animators_createObjects, Settings.bCopier_animators_copyMainAnimator, true);
            }
            if(Settings.bCopier_audioSources_copy && CopierTabs.ComponentIsInSelectedTab<AudioSource>(Settings._copier_selectedTab))
            {
                LegacyCopier.CopyAllAudioSources(objFrom, objTo, Settings.bCopier_audioSources_createObjects, true);
            }
            if(Settings.bCopier_other_copy && CopierTabs.ComponentIsInSelectedTab("other", Settings._copier_selectedTab))
            {
                LegacyCopier.CopyAllIKFollowers(objFrom, objTo, Settings.bCopier_other_createGameObjects, true);
            }
            if(PhysBonesExist)
            {
                #if PUMKIN_PBONES
                if(Settings.bCopier_contactReceiver_copy && CopierTabs.ComponentIsInSelectedTab("contactreceiver", Settings._copier_selectedTab))
                {
                    if(Settings.bCopier_contactReceiver_removeOld)
                        LegacyDestroyer.DestroyAllComponentsOfType(objTo, PumkinsTypeCache.ContactReceiver, false, true);
                    GenericCopier.CopyComponent<VRCContactReceiver>(objFrom, objTo, Settings.bCopier_contactReceiver_createObjects, Settings.bCopier_contactReceiver_adjustScale, true, true);
                }
                if(Settings.bCopier_contactSender_copy && CopierTabs.ComponentIsInSelectedTab("contactsender", Settings._copier_selectedTab))
                {
                    if(Settings.bCopier_contactSender_removeOld)
                        LegacyDestroyer.DestroyAllComponentsOfType(objTo, PumkinsTypeCache.ContactSender, false, true);
                    GenericCopier.CopyComponent<VRCContactSender>(objFrom, objTo, Settings.bCopier_contactSender_createObjects, Settings.bCopier_contactSender_adjustScale, true, true);
                }
                if(Settings.bCopier_physBones_copyColliders && CopierTabs.ComponentIsInSelectedTab("physbonecollider", Settings._copier_selectedTab))
                {
                    if(Settings.bCopier_physBones_removeOldColliders)
                        LegacyDestroyer.DestroyAllComponentsOfType(objTo, PumkinsTypeCache.PhysBoneCollider, false, true);
                    GenericCopier.CopyComponent<VRCPhysBoneCollider>(objFrom, objTo, Settings.bCopier_physBones_createObjectsColliders, Settings.bCopier_physBones_adjustScaleColliders, true, true);
                }
                if(Settings.bCopier_physBones_copy && CopierTabs.ComponentIsInSelectedTab("physbone", Settings._copier_selectedTab))
                {
                    if(Settings.bCopier_physBones_removeOldBones)
                        LegacyDestroyer.DestroyAllComponentsOfType(objTo, PumkinsTypeCache.PhysBone, false, true);
                    GenericCopier.CopyComponent<VRCPhysBone>(objFrom, objTo, Settings.bCopier_physBones_createObjects, Settings.bCopier_physBones_adjustScale, true, true);
                }
                #endif
            } 
            if(DynamicBonesExist)
            {
                if(Settings.bCopier_dynamicBones_copyColliders && CopierTabs.ComponentIsInSelectedTab("dynamicbonecollider", Settings._copier_selectedTab))
                {
                    if(Settings.bCopier_dynamicBones_removeOldColliders)
                        LegacyDestroyer.DestroyAllComponentsOfType(objTo, PumkinsTypeCache.DynamicBoneCollider, false, true);
                    LegacyCopier.CopyAllDynamicBoneColliders(objFrom, objTo, Settings.bCopier_dynamicBones_createObjectsColliders, true, Settings.bCopier_dynamicBones_adjustScaleColliders);
                }
                if(Settings.bCopier_dynamicBones_copy && CopierTabs.ComponentIsInSelectedTab("dynamicbone", Settings._copier_selectedTab))
                {
                    if(Settings.bCopier_dynamicBones_removeOldBones)
                        LegacyDestroyer.DestroyAllComponentsOfType(objTo, PumkinsTypeCache.DynamicBone, false, true);
                    if(Settings.bCopier_dynamicBones_copySettings || Settings.bCopier_dynamicBones_createMissing)
                        LegacyCopier.CopyAllDynamicBonesNew(objFrom, objTo, Settings.bCopier_dynamicBones_createMissing, true, Settings.bCopier_dynamicBones_adjustScale);
                }
            }
            else if(Settings.bCopier_dynamicBones_copy)
            {
                Log(Strings.Warning.noDBonesOrMissingScriptDefine, LogType.Error);
            }

            if(Settings.bCopier_aimConstraint_copy && CopierTabs.ComponentIsInSelectedTab<AimConstraint>(Settings._copier_selectedTab))
            {
                LegacyCopier.CopyAllAimConstraints(objFrom, objTo, Settings.bCopier_aimConstraint_createObjects, true);
            }
            if(Settings.bCopier_lookAtConstraint_copy && CopierTabs.ComponentIsInSelectedTab<LookAtConstraint>(Settings._copier_selectedTab))
            {
                LegacyCopier.CopyAllLookAtConstraints(objFrom, objTo, Settings.bCopier_aimConstraint_createObjects, true);
            }
            if(Settings.bCopier_parentConstraint_copy && CopierTabs.ComponentIsInSelectedTab<ParentConstraint>(Settings._copier_selectedTab))
            {
                LegacyCopier.CopyAllParentConstraints(objFrom, objTo, Settings.bCopier_aimConstraint_createObjects, true);
            }
            if(Settings.bCopier_positionConstraint_copy && CopierTabs.ComponentIsInSelectedTab<PositionConstraint>(Settings._copier_selectedTab))
            {
                LegacyCopier.CopyAllPositionConstraints(objFrom, objTo, Settings.bCopier_aimConstraint_createObjects, true);
            }
            if(Settings.bCopier_rotationConstraint_copy && CopierTabs.ComponentIsInSelectedTab<RotationConstraint>(Settings._copier_selectedTab))
            {
                LegacyCopier.CopyAllRotationConstraints(objFrom, objTo, Settings.bCopier_aimConstraint_createObjects, true);
            }
            if(Settings.bCopier_scaleConstraint_copy && CopierTabs.ComponentIsInSelectedTab<ScaleConstraint>(Settings._copier_selectedTab))
            {
                LegacyCopier.CopyAllScaleConstraints(objFrom, objTo, Settings.bCopier_aimConstraint_createObjects, true);
            }
            if(Settings.bCopier_joints_copy && CopierTabs.ComponentIsInSelectedTab<Joint>(Settings._copier_selectedTab))
            {
                if(Settings.bCopier_joints_removeOld)
                    LegacyDestroyer.DestroyAllComponentsOfType(objTo, typeof(Joint), false, true);
                LegacyCopier.CopyAllJoints(objFrom, objTo, Settings.bCopier_joints_createObjects, true);
            }
            if(Settings.bCopier_transforms_copy && CopierTabs.ComponentIsInSelectedTab<Transform>(Settings._copier_selectedTab))
            {
                LegacyCopier.CopyAllTransforms(objFrom, objTo, true);
            }
            if(Settings.bCopier_meshRenderers_copy && CopierTabs.ComponentIsInSelectedTab<MeshRenderer>(Settings._copier_selectedTab))
            {
                LegacyCopier.CopyAllMeshRenderers(objFrom, objTo, Settings.bCopier_meshRenderers_createObjects, true);
            }
            if(Settings.bCopier_skinMeshRender_copy && CopierTabs.ComponentIsInSelectedTab<SkinnedMeshRenderer>(Settings._copier_selectedTab))
            {
                LegacyCopier.CopyAllSkinnedMeshRenderersSettings(objFrom, objTo, true);
            }
            
            if(Settings.bCopier_cameras_copy && CopierTabs.ComponentIsInSelectedTab<Camera>(Settings._copier_selectedTab))
            {
                LegacyCopier.CopyCameras(objFrom, objTo, Settings.bCopier_cameras_createObjects, true);
            }

            if(Settings.bCopier_transforms_copy && Settings.bCopier_transforms_copyActiveState && CopierTabs.ComponentIsInSelectedTab<Transform>(Settings._copier_selectedTab))
            {
                LegacyCopier.CopyTransformActiveStateTagsAndLayer(objFrom, objTo, true);
            }

			#if PUMKIN_FINALIK
            if(_DependencyChecker.FinalIKExists && Settings.bCopier_finalIK_copy && CopierTabs.ComponentIsInSelectedTab("finalik", Settings._copier_selectedTab))
            {
                if(Settings.bCopier_finalIK_copyCCDIK)
                    GenericCopier.CopyComponent<CCDIK>(objFrom, objTo, Settings.bCopier_finalIK_createObjects, false,true, true);
                if(Settings.bCopier_finalIK_copyLimbIK)
                    GenericCopier.CopyComponent<LimbIK>(objFrom, objTo, Settings.bCopier_finalIK_createObjects, false, true, true);
                if(Settings.bCopier_finalIK_copyRotationLimits)
                    GenericCopier.CopyComponent<RotationLimit>(objFrom, objTo, Settings.bCopier_finalIK_createObjects, false, true, true);
                if(Settings.bCopier_finalIK_copyFabrik)
                    GenericCopier.CopyComponent<FABRIK>(objFrom, objTo, Settings.bCopier_finalIK_createObjects, false, true, true);
                if(Settings.bCopier_finalIK_copyAimIK)
                    GenericCopier.CopyComponent<AimIK>(objFrom, objTo, Settings.bCopier_finalIK_createObjects, false, true, true);
                if(Settings.bCopier_finalIK_copyFBTBipedIK)
                    GenericCopier.CopyComponent<FullBodyBipedIK>(objFrom, objTo, Settings.bCopier_finalIK_createObjects, false, true, true);
                if(Settings.bCopier_finalIK_copyVRIK)
                    GenericCopier.CopyComponent<VRIK>(objFrom, objTo, Settings.bCopier_finalIK_createObjects, false, true, true);
            }
			#endif
        }


        void FillEyeBones(GameObject avatar)
        {
            Type descType = PumkinsTypeCache.VRC_AvatarDescriptor;
            
            if(!avatar || descType == null)
                return;

            var anim = avatar.GetComponent<Animator>();

            if(!anim)
                return;
            if(!anim.isHuman)
            {
                Log("FillEyeBones only works for humanoid avatars", LogType.Warning);
                return;
            }

            var desc = avatar.GetComponent(descType);
            var sDesc = new SerializedObject(desc);

            var leftEye = sDesc.FindProperty("customEyeLookSettings.leftEye");
            var rightEye = sDesc.FindProperty("customEyeLookSettings.rightEye");

            leftEye.objectReferenceValue = anim.GetBoneTransform(HumanBodyBones.LeftEye);
            rightEye.objectReferenceValue = anim.GetBoneTransform(HumanBodyBones.RightEye);

            sDesc.ApplyModifiedProperties();
        }
#endregion



#region Utility Functions

        /// <summary>
        /// Not actually resets everything but backgrounnd and overlay stuff
        /// </summary>
        public void ResetBackgroundsAndOverlays()
        {
            Settings._backgroundPath = null;
            Settings._overlayPath = null;
            Settings.bThumbnails_use_camera_background = false;
            Settings.bThumbnails_use_camera_overlay = false;
            cameraBackgroundTexture = null;
            cameraOverlayTexture = null;
            DestroyDummies();
        }

        /// <summary>
        /// Refreshes the background override setting
        /// </summary>
        public void RefreshBackgroundOverrideType()
        {
            if(Settings.bThumbnails_use_camera_background)
            {
                switch(cameraBackgroundType)
                {
                    case PumkinsCameraPreset.CameraBackgroundOverrideType.Color:
                        Color color = SelectedCamera != null ? SelectedCamera.backgroundColor : Settings._thumbsCamBgColor;
                        SetCameraBackgroundToColor(color);
                        break;
                    case PumkinsCameraPreset.CameraBackgroundOverrideType.Image:
                        SetBackgroundToImageFromPath(Settings._backgroundPath);
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
            if(Settings._copierIgnoreArray == null)
            {
                Settings._copierIgnoreArray = new Transform[0];
                return;
            }
            else if(Settings._copierIgnoreArray.Length == 0)
            {
                return;
            }

            var newList = new List<Transform>(Settings._copierIgnoreArray.Length);

            foreach(var t in Settings._copierIgnoreArray)
            {
                if(!t)
                    newList.Add(t);

                var tt = Helpers.FindTransformInAnotherHierarchy(t, CopierSelectedFrom.transform, false);
                if(tt && !newList.Contains(tt))
                    newList.Add(tt);
            }

            Settings._copierIgnoreArray = newList.ToArray();
        }

        /// <summary>
        /// Refreshes the chosen language in the UI. Needed for when we go into and out of play mode
        /// </summary>
        public void RefreshLanguage()
        {
            PumkinsLanguageManager.LoadTranslations();
            PumkinsLanguageManager.SetLanguage(Settings._selectedLanguageString);
            Settings._selectedLanguageIndex = PumkinsLanguageManager.GetIndexOfLanguage(Settings._selectedLanguageString);
        }

        /// <summary>
        /// Refreshes the cached selected preset index of a PumkinPreset type
        /// </summary>
        public static void RefreshPresetIndex<T>() where T : PumkinPreset
        {
            Type t = typeof(T);
            Type tP = typeof(PumkinPreset);
            if(typeof(T) == typeof(PumkinsCameraPreset) || t == tP)
                RefreshPresetIndexByString<T>(Settings._selectedCameraPresetString);
            if(typeof(T) == typeof(PumkinsPosePreset) || t == tP)
                RefreshPresetIndexByString<T>(Settings._selectedPosePresetString);
            if(typeof(T) == typeof(PumkinsBlendshapePreset) || t == tP)
                RefreshPresetIndexByString<T>(Settings._selectedBlendshapePresetString);
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
                Settings._selectedCameraPresetIndex = selectedPresetIndex;
            }
            else if(typeof(T) == typeof(PumkinsPosePreset))
            {
                if(count == 0)
                    selectedPresetString = "";
                else
                    selectedPresetString = PumkinsPresetManager.PosePresets[selectedPresetIndex].ToString() ?? "";
                Settings._selectedPosePresetIndex = selectedPresetIndex;
            }
            else if(typeof(T) == typeof(PumkinsBlendshapePreset))
            {
                if(count == 0)
                    selectedPresetString = "";
                else
                    selectedPresetString = PumkinsPresetManager.BlendshapePresets[selectedPresetIndex].ToString() ?? "";
                Settings._selectedBlendshapePresetIndex = selectedPresetIndex;
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
                Settings._selectedCameraPresetString = presetString;
            else if(typeof(T) == typeof(PumkinsPosePreset))
                Settings._selectedPosePresetString = presetString;
            else if(typeof(T) == typeof(PumkinsBlendshapePreset))
                Settings._selectedBlendshapePresetString = presetString;
        }

#endregion

#region Helper Functions

        /// <summary>
        /// Sets selected camera clear flags back to _thumbsCameraBgClearFlagsOld
        /// </summary>
        public void RestoreCameraClearFlags()
        {
            if(SelectedCamera)
                SelectedCamera.clearFlags = Settings._thumbsCameraBgClearFlagsOld;
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

            //This technically currently Resets all Objects and not just pose.
            //TODO: Use Humanoid Bones for Reset if only pose. Use This method if Full Reset.
            var trans = avatar.GetComponentsInChildren<Transform>(true);

            foreach(var t in trans)
            {
                if(t == t.root)
                    continue;

                string tPath = Helpers.GetTransformPath(t, avatar.transform);
                Transform tPref = pref.transform.Find(tPath);

                if(!tPref)
                    continue;

                if(Settings._tools_avatar_resetPose_position)
                    t.localPosition = tPref.localPosition;
                if(Settings._tools_avatar_resetPose_rotation)
                {
                    t.localRotation = tPref.localRotation;
                    t.localEulerAngles = tPref.localEulerAngles;
                }

                if(Settings._tools_avatar_resetPose_scale)
                {
                    t.localScale = tPref.localScale;
                }
            }

            PumkinsPoseEditor.OnPoseWasChanged(PumkinsPoseEditor.PoseChangeType.Reset);
            return true;
        }

        /// <summary>
        /// Resets target pose to avatar definition
        /// </summary>
        /// <param name="avatar">Target avatar to reset</param>
        /// <param name="fullReset">Should reset non-humanoid objects included in the definition?</param>
        /// <param name="position">Reset the transform position of objects</param>
        /// <param name="rotation">Reset the transform rotation of objects</param>
        /// <param name="scale">Reset the transform scale of objects</param>
        public static bool ResetToAvatarDefinition(GameObject avatar, bool fullReset = false, bool position = true, bool rotation = true, bool scale = true)
        {
            if(!avatar) return false;
            Animator ani = avatar.GetComponent<Animator>();
            if(!ani || !ani.avatar || !ani.avatar.isHuman)
            {
                Log(Strings.Log.cantSetPoseNonHumanoid, LogType.Warning, "Avatar Definition");
                return false;
            }

            // All IDs if full reset. Only Human IDs if not.
            // ID > Path
            // ID > Element > Transform Data
            Undo.RegisterFullObjectHierarchyUndo(avatar, "Reset To Avatar Definition");
            SerializedObject sAvi = new SerializedObject(ani.avatar);
            SerializedProperty humanIds = sAvi.FindProperty("m_Avatar.m_Human.data.m_Skeleton.data.m_ID");
            SerializedProperty allIds = sAvi.FindProperty("m_Avatar.m_AvatarSkeleton.data.m_ID");
            SerializedProperty defaultPose = sAvi.FindProperty("m_Avatar.m_DefaultPose.data.m_X");
            SerializedProperty tos = sAvi.FindProperty("m_TOS");

            Dictionary<long, int> idToElem = new Dictionary<long, int>();
            Dictionary<int, TransformData> elemToTransform = new Dictionary<int, TransformData>();
            Dictionary<long, string> IdToPath = new Dictionary<long, string>();

            for (int i = 0; i < allIds.arraySize; i++)
                idToElem.Add(allIds.GetArrayElementAtIndex(i).longValue, i);

            for (int i = 0; i < defaultPose.arraySize; i++)
                elemToTransform.Add(i, new TransformData(defaultPose.GetArrayElementAtIndex(i)));

            for (int i = 0; i < tos.arraySize; i++)
            {
                SerializedProperty currProp = tos.GetArrayElementAtIndex(i);
                IdToPath.Add(currProp.FindPropertyRelative("first").longValue, currProp.FindPropertyRelative("second").stringValue);
            }

            System.Action<Transform, TransformData> applyTransform = (transform, data) => {
                if(transform)
                {
                    if(position)
                        transform.localPosition = data.pos;
                    if(rotation)
                        transform.localRotation = data.rot;
                    if(scale)
                        transform.localScale = data.scale;
                }
            };

            if(!fullReset)
            {
                for (int i = 0; i < humanIds.arraySize; i++)
                {
                    Transform myBone = ani.transform.Find(IdToPath[humanIds.GetArrayElementAtIndex(i).longValue]);
                    TransformData data = elemToTransform[idToElem[humanIds.GetArrayElementAtIndex(i).longValue]];
                    applyTransform(myBone, data);
                }
            }
            else
            {
                for (int i = 0; i < allIds.arraySize; i++)
                {
                    Transform myBone = ani.transform.Find(IdToPath[allIds.GetArrayElementAtIndex(i).longValue]);
                    TransformData data = elemToTransform[idToElem[allIds.GetArrayElementAtIndex(i).longValue]];
                    applyTransform(myBone, data);
                }
            }

            return true;
        }
        private struct TransformData
        {
            public Vector3 pos;
            public Quaternion rot;
            public Vector3 scale;
            public TransformData(SerializedProperty t)
            {
                SerializedProperty tProp = t.FindPropertyRelative("t");
                SerializedProperty qProp = t.FindPropertyRelative("q");
                SerializedProperty sProp = t.FindPropertyRelative("s");
                pos = new Vector3(tProp.FindPropertyRelative("x").floatValue, tProp.FindPropertyRelative("y").floatValue, tProp.FindPropertyRelative("z").floatValue);
                rot = new Quaternion(qProp.FindPropertyRelative("x").floatValue, qProp.FindPropertyRelative("y").floatValue, qProp.FindPropertyRelative("z").floatValue, qProp.FindPropertyRelative("w").floatValue);
                scale = new Vector3(sProp.FindPropertyRelative("x").floatValue, sProp.FindPropertyRelative("y").floatValue, sProp.FindPropertyRelative("z").floatValue);
            }
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
                newChild = parent.transform.Find(child.name, true, parent.transform);
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
            var data = JsonUtility.ToJson(Settings, false);
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

            var data = EditorPrefs.GetString("PumkinToolsWindow", JsonUtility.ToJson(Settings, false));
            if(data != null)
            {
                JsonUtility.FromJsonOverwrite(data, Settings);
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
            if(!Settings.verboseLoggingEnabled)
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
