using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using System;
using System.IO;
using UnityEngine;
using System.Linq;
using Pumkin.AvatarTools.Copiers;
using Pumkin.AvatarTools.Destroyers;
using Pumkin.DependencyChecker;
using UnityEngine.UI;
using Pumkin.DataStructures;
using Pumkin.HelperFunctions;
using UnityEngine.SceneManagement;
using Pumkin.Presets;
using UnityEngine.Animations;
using static Pumkin.AvatarTools.PumkinToolsLogger;

using UnityEditor.Experimental.SceneManagement;

namespace Pumkin.AvatarTools
{
    /// <summary>
    /// PumkinsAvatarTools by, well, Pumkin
    /// https://github.com/rurre/PumkinsAvatarTools
    /// </summary>
    [Serializable]
    public partial class PumkinsAvatarTools
    {
        #region Variables

        #region Tools

        internal const string SettingsEditorPrefsKey = "PumkinToolsSettings";
        [SerializeField] SettingsContainer _settings;

        internal static SettingsContainer Settings
        {
            get
            {
                if(Instance._settings == null)
                {
                    Instance._settings = ScriptableObject.CreateInstance<SettingsContainer>();
                    Instance.LoadPrefs();
                }

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

#if VRC_SDK_VRCSDK3
        Component _tempAvatarDescriptor;
        bool _tempAvatarDescriptorWasAdded = false;
        List<Component> _pBonesThatWereAlreadyDisabled = new List<Component>();
#endif
        bool _nextTogglePBoneState = false;
        bool _nextToggleDBoneState = false;

        List<Component> _dBonesThatWereAlreadyDisabled = new List<Component>();

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
            RemoveMissingScripts,
            RemoveContactReceiver,
            RemoveContactSender,
            RemoveLookAtConstraint,
            RemoveParentConstraint,
            RemoveRotationConstraint,
            RemoveAimConstraint,
            RemoveScaleConstraint,
            RemovePositionConstraint,
            FillEyeBones,
            ResetBoundingBoxes,
            RemoveCameras,
            RemoveFinalIK_CCDIK,
            RemoveFinalIK_LimbIK,
            RemoveFinalIK_RotationLimits,
            RemoveFinalIK_FabrIK,
            RemoveFinalIK_AimIK,
            RemoveFinalIK_FbtBipedIK,
            RemoveFinalIK_VRIK,
            RemoveFinalIK_Grounder,
            RemoveVRCStation,
            RemoveVRCHeadChop,
            RemoveVRCAimConstraint,
            RemoveVRCLookAtConstraint,
            RemoveVRCParentConstraint,
            RemoveVRCPositionConstraint,
            RemoveVRCRotationConstraint,
            RemoveVRCScaleConstraint,
        }

        readonly static string SCALE_RULER_DUMMY_NAME = "_PumkinsScaleRuler";

#endregion

#region Component Copier

        bool? _copierArmatureScalesDontMatch;

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

#pragma warning disable
        static string _mainScriptPath = null;
        static string _mainFolderPath = null;
        static string _saveFolderPath = null;
        static string _resourceFolderPath = null;
        static string _mainFolderPathLocal = null;
        static string _mainScriptPathLocal = null;
        static string _resourceFolderPathLocal = null;
#pragma warning restore
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
                        DynamicBonesExist && Settings.bCopier_dynamicBones_copy,
                        DynamicBonesExist && Settings.bCopier_dynamicBones_copyColliders,
                        Settings.bCopier_meshRenderers_copy,
                        Settings.bCopier_particleSystems_copy,
                        Settings.bCopier_trailRenderers_copy,
                        Settings.bCopier_audioSources_copy,
                        Settings.bCopier_lights_copy,
                        Settings.bCopier_prefabs_copy,
                        Settings.bCopier_aimConstraint_copy,
                        Settings.bCopier_lookAtConstraint_copy,
                        Settings.bCopier_parentConstraint_copy,
                        Settings.bCopier_positionConstraint_copy,
                        Settings.bCopier_rotationConstraint_copy,
                        Settings.bCopier_scaleConstraint_copy,
                        FinalIKExists && Settings.bCopier_finalIK_copy,
                        Settings.bCopier_other_copy,
#if VRC_SDK_VRCSDK3
                        Settings.bCopier_vrcAimConstraint_copy,
                        Settings.bCopier_vrcLookAtConstraint_copy,
                        Settings.bCopier_vrcParentConstraint_copy,
                        Settings.bCopier_vrcPositionConstraint_copy,
                        Settings.bCopier_vrcRotationConstraint_copy,
                        Settings.bCopier_vrcScaleConstraint_copy,
                        Settings.bCopier_vrcHeadChop_copy,
                        Settings.bCopier_contactSender_copy,
                        Settings.bCopier_contactReceiver_copy
#endif
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
                        Settings.bCopier_meshRenderers_copy,
                        Settings.bCopier_particleSystems_copy,
                        Settings.bCopier_rigidBodies_copy,
                        Settings.bCopier_trailRenderers_copy,
                        Settings.bCopier_lights_copy,
                        Settings.bCopier_skinMeshRender_copy,
                        Settings.bCopier_physBones_copy,
                        Settings.bCopier_physBones_copyColliders,
                        DynamicBonesExist && Settings.bCopier_dynamicBones_copy,
                        DynamicBonesExist && Settings.bCopier_dynamicBones_copyColliders,
                        Settings.bCopier_audioSources_copy,
                        Settings.bCopier_aimConstraint_copy,
                        Settings.bCopier_lookAtConstraint_copy,
                        Settings.bCopier_parentConstraint_copy,
                        Settings.bCopier_positionConstraint_copy,
                        Settings.bCopier_rotationConstraint_copy,
                        Settings.bCopier_scaleConstraint_copy,
                        Settings.bCopier_cameras_copy,
                        Settings.bCopier_prefabs_copy,
#if VRC_SDK_VRCSDK3
                        Settings.bCopier_descriptor_copy,
                        Settings.bCopier_vrcStations_copy,
                        Settings.bCopier_contactSender_copy,
                        Settings.bCopier_contactReceiver_copy,
                        Settings.bCopier_vrcAimConstraint_copy,
                        Settings.bCopier_vrcLookAtConstraint_copy,
                        Settings.bCopier_vrcParentConstraint_copy,
                        Settings.bCopier_vrcPositionConstraint_copy,
                        Settings.bCopier_vrcRotationConstraint_copy,
                        Settings.bCopier_vrcScaleConstraint_copy,
                        Settings.bCopier_vrcHeadChop_copy,
#endif
                        Settings.bCopier_other_copy,
                        Settings.bCopier_finalIK_copy
                    };
                    if(allToggles.Any(b => b))
                        return true;
                }
                return false;
            }
        }

        public static string ProjectPath
        {
            get
            {
                if(_projectPath == null)
                {
                    string dataPath = Application.dataPath;
                    _projectPath = dataPath.Substring(0, dataPath.LastIndexOf("Assets"));
                }
                return _projectPath;
            }
        }

        static string _projectPath;

        public static string MainFolderPath
        {
            get
            {
                if(_mainFolderPath == null)
                {
                    string packagePath = $"{ProjectPath}Packages/io.github.rurre.pumkinsavatartools";
                    if(Directory.Exists(packagePath)) // Check packages
                    {
                        _mainFolderPath = packagePath;
                        return _mainFolderPath;
                    }

                    // Check assets
                    var pumkinFolders = Directory.GetDirectories($"{ProjectPath}Assets", "PumkinsAvatarTools*", SearchOption.AllDirectories);
                    if(pumkinFolders.Length > 0)
                        return _mainFolderPath = pumkinFolders[0] + "/";
                    Debug.LogError("Couldn't find PumkinsAvatarTools folder..");
                }
                return _mainFolderPath;
            }
        }

        public static string SaveFolderPath
        {
            get
            {
                if(_saveFolderPath == null)
                {
                    _saveFolderPath = Application.dataPath + "/PumkinsAvatarTools/Resources";
                }
                return _saveFolderPath;
            }
        }

        public static string MainFolderPathLocal
        {
            get
            {
                if(_mainFolderPathLocal == null)
                    _mainFolderPathLocal = Helpers.AbsolutePathToLocalProjectPath(MainFolderPath);
                return _mainFolderPathLocal;
            }
        }

        public static string ResourceFolderPathLocal
        {
            get
            {
                if(_resourceFolderPathLocal == null)
                    _resourceFolderPathLocal = Helpers.AbsolutePathToLocalProjectPath(ResourceFolderPath);
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
                    Settings._serializedIgnoreArrayProp = Settings.SerializedSettings.FindProperty(nameof(Settings.copierIgnoreArray));
                return Settings._serializedIgnoreArrayProp;
            }

            private set => Settings._serializedIgnoreArrayProp = value;
        }

        public SerializedProperty SerializedScaleTemp
        {
            get
            {
                if(Settings.SerializedSettings != null && Settings._serializedAvatarScaleTempProp == null)
                    Settings._serializedAvatarScaleTempProp = Settings.SerializedSettings.FindProperty(nameof(Settings._avatarScaleTemp));
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
                    Settings._serializedTempHumanPoseMuscles = Settings.SerializedSettings.FindProperty(nameof(Settings._tempHumanPoseMuscles));
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

        bool? _dynamicBonesExist;

        public bool PhysBonesExist
        {
            get
            {
                return _DependencyChecker.PhysBonesExist;
            }
        }

        public bool SpringBonesExist
        {
            get
            {
                if(_springBonesExist == null)
                    _springBonesExist = PumkinsTypeCache.VRMSpringBone != null;
                return (bool)_springBonesExist;
            }
        }
        bool? _springBonesExist;

		public bool FinalIKExists
		{
			get
			{
				if(_finalIKExists == null)
					_finalIKExists = PumkinsTypeCache.AimIK != null && PumkinsTypeCache.FABRIK != null && PumkinsTypeCache.FullBodyBipedIK != null && PumkinsTypeCache.RotationLimit != null;
				return (bool)_finalIKExists;
			}
		}

		bool? _finalIKExists;

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
#if VRC_SDK_VRCSDK3
            if(Instance._editingView)
                Instance.EndEditingViewpoint(null, true);
#endif

            Settings.centerCameraTransform = null;

            Instance._copierArmatureScalesDontMatch = null;

            Instance._nextTogglePBoneState = false;
#if VRC_SDK_VRCSDK3
            Instance._pBonesThatWereAlreadyDisabled = new List<Component>();
#endif
            Instance._nextToggleDBoneState = false;
            Instance._dBonesThatWereAlreadyDisabled = new List<Component>();
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
                updateCallback = OnUpdate;

            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.duringSceneGui += OnSceneGUI;
            EditorApplication.update -= updateCallback;
            EditorApplication.update += updateCallback;

            EditorApplication.playModeStateChanged -= HandlePlayModeStateChange;
            EditorApplication.playModeStateChanged += HandlePlayModeStateChange;
            Selection.selectionChanged -= HandleSelectionChanged;
            Selection.selectionChanged += HandleSelectionChanged;
            EditorSceneManager.sceneOpened -= HandleSceneChange;
            EditorSceneManager.sceneOpened += HandleSceneChange;
            EditorSceneManager.newSceneCreated -= HandleNewScene;
            EditorSceneManager.newSceneCreated += HandleNewScene;
            EditorSceneManager.sceneClosing -= HandleSceneClosing;
            EditorSceneManager.sceneClosing += HandleSceneClosing;

            PrefabStage.prefabStageOpened -= HandlePrefabStageOpened;
            PrefabStage.prefabStageOpened += HandlePrefabStageOpened;
            PrefabStage.prefabStageClosing -= HandlePrefabStageClosed;
            PrefabStage.prefabStageClosing += HandlePrefabStageClosed;

            EditorApplication.quitting -= HandleQuitting;
            EditorApplication.quitting += HandleQuitting;

            SerializedIgnoreArray = Settings.SerializedSettings.FindProperty(nameof(Settings.copierIgnoreArray));
            SerializedScaleTemp = Settings.SerializedSettings.FindProperty(nameof(Settings._avatarScaleTemp));
            SerializedHumanPoseMuscles = Settings.SerializedSettings.FindProperty(nameof(Settings._tempHumanPoseMuscles));

            _emptyTexture = new Texture2D(2, 2);
            cameraOverlayTexture = new Texture2D(2, 2);
            cameraBackgroundTexture = new Texture2D(2, 2);

            LoadPrefs();

            RestoreTexturesFromPaths();
            RefreshBackgroundOverrideType();

            DestroyDummies();

            SelectedCamera = GetVRCCamOrMainCam();

            if(Settings._lastOpenFilePath == default)
                Settings._lastOpenFilePath = MainFolderPath + PumkinsPresetManager.resourceCamerasPath + "/Example Images";

            Settings.lockSelectedCameraToSceneView = false;
        }

        void HandleSceneClosing(Scene scene, bool removingscene)
        {
            SavePrefs();
        }

        void HandleNewScene(Scene scene, NewSceneSetup setup, NewSceneMode mode)
        {
            if(mode == NewSceneMode.Single)
                LoadPrefs();
        }

        void HandleQuitting()
        {
            SavePrefs();
        }

        public void HandleSceneChange(Scene scene, OpenSceneMode mode)
        {
            if(mode == OpenSceneMode.Single)
            {
                LoadPrefs();
                RefreshLanguage();
            }
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

            if(Settings.SerializedSettings != null)
                Settings.SerializedSettings.ApplyModifiedPropertiesWithoutUndo();
#if VRC_SDK_VRCSDK3
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
#if VRC_SDK_VRCSDK3
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

#if VRC_SDK_VRCSDK3
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
            if(SettingsContainer._useSceneSelectionAvatar && GetAvatarFromSceneSelection(true, out var avatar))
                SelectedAvatar = avatar;
            _PumkinsAvatarToolsWindow.RepaintSelf();
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

#if PUMKIN_DEV
        [MenuItem("Tools/Pumkin/Save Bad Copier Settings", false, 4)]
        public static void SaveBadSettings()
        {
            string json = JsonUtility.ToJson(PumkinsAvatarTools.Settings, true);
            string guid = AssetDatabase.AssetPathToGUID("Assets\\PumkinsAvatarTools\\Editor\\Resources\\Icons\\bone-icon.png");
            json = json.Replace("\"copierIgnoreArray\": []", $"\"copierIgnoreArray\": [\"{guid}\"]");
            EditorPrefs.SetString(SettingsEditorPrefsKey, json);
            Debug.Log(json);
        }
#endif

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
#if VRC_SDK_VRCSDK3
            else if(_editingView) //Viewpoint editing
            {
                DrawEditingViewpointGUI();
            }
#endif

            if(DrawingHandlesGUI)
                _PumkinsAvatarToolsWindow.RepaintSelf();
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

        /// <summary>
        /// Unparents then copies components and values from one object to another then reparents
        /// </summary>
        /// <param name="objFrom"></param>
        /// <param name="objTo"></param>
        void CopyComponentsWithoutParents(GameObject objFrom, GameObject objTo)
        {
            Transform toParent = objTo.transform.parent;
            int childTransformIndex = objTo.transform.GetSiblingIndex();

            CopyInstance copyInst = new CopyInstance(objFrom, objTo, Settings.copierIgnoreArray);

            try
            {
                CopyComponents(copyInst);
                GenericCopier.FixInstanceReferences(copyInst);
            }
            catch(Exception ex)
            {
                Log($"{ex.Message},{ex.TargetSite},\n{ex.StackTrace}", LogType.Exception);
            }
            finally
            {
                objTo.transform.parent = toParent;
                objTo.transform.SetSiblingIndex(childTransformIndex);
            }
        }

        /// <summary>
        /// Copies Components and Values from one object to another.
        /// </summary>
        void CopyComponents(CopyInstance inst)
        {
            string log = "";

            GameObject objFrom = inst.from;
            GameObject objTo = inst.to;

            bool scaleMismatch = _copierArmatureScalesDontMatch != null && (bool)_copierArmatureScalesDontMatch;

            //Cancel Checks
            if(objFrom == objTo)
            {
                log += Strings.Log.cantCopyToSelf;
                Log(log, LogType.Warning);
                return;
            }

            try
            {
                if(Settings.bCopier_prefabs_copy && CopierTabs.ComponentIsInSelectedTab("prefab", Settings._copier_selectedTab))
                {
                    GenericCopier.CopyPrefabs(inst, Settings.bCopier_prefabs_createObjects, scaleMismatch && Settings.bCopier_prefabs_adjustScale, Settings.bCopier_prefabs_fixReferences,
                            Settings.bCopier_prefabs_copyPropertyOverrides, Settings.bCopier_prefabs_ignorePrefabByOtherCopiers, inst.ignoredTransforms);
                }
            }
            catch(Exception ex) { Log("_Failed to copy Prefabs: " + ex.Message, LogType.Error); }

            try
            {
                if(Settings.bCopier_particleSystems_copy && CopierTabs.ComponentIsInSelectedTab<ParticleSystem>(Settings._copier_selectedTab))
                    GenericCopier.CopyComponent<ParticleSystem>(inst, Settings.bCopier_prefabs_createObjects, false, true, true, inst.ignoredTransforms);
            }
            catch(Exception ex) { Log("_Failed to copy Particle Systems: " + ex.Message, LogType.Error); }

            try
            {
                if(Settings.bCopier_colliders_copy && CopierTabs.ComponentIsInSelectedTab<Collider>(Settings._copier_selectedTab))
                {
                    if(Settings.bCopier_colliders_removeOld)
                        LegacyDestroyer.DestroyAllComponentsOfType(objTo, typeof(Collider), false);
                    LegacyCopier.CopyAllColliders(objFrom, objTo, Settings.bCopier_colliders_createObjects, scaleMismatch && Settings.bCopier_colliders_adjustScale, inst.ignoredTransforms);
                }
            }
            catch(Exception ex) { Log("_Failed to copy Collider: " + ex.Message, LogType.Error); }

            try
            {
                if(Settings.bCopier_rigidBodies_copy && CopierTabs.ComponentIsInSelectedTab<Rigidbody>(Settings._copier_selectedTab))
                    LegacyCopier.CopyAllRigidBodies(objFrom, objTo, Settings.bCopier_rigidBodies_createObjects, inst.ignoredTransforms);
            }
            catch(Exception ex) { Log("_Failed to copy Rigid Bodies: " + ex.Message, LogType.Error); }

            try
            {
                if(Settings.bCopier_trailRenderers_copy && CopierTabs.ComponentIsInSelectedTab<TrailRenderer>(Settings._copier_selectedTab))
                    LegacyCopier.CopyAllTrailRenderers(objFrom, objTo, Settings.bCopier_trailRenderers_createObjects, inst.ignoredTransforms);
            }
            catch(Exception ex) { Log("_Failed to copy Trail Renderers: " + ex.Message, LogType.Error); }

            try
            {
                if(Settings.bCopier_lights_copy && CopierTabs.ComponentIsInSelectedTab<Light>(Settings._copier_selectedTab))
                    LegacyCopier.CopyAllLights(objFrom, objTo, Settings.bCopier_lights_createObjects, inst.ignoredTransforms);
            }
            catch(Exception ex) { Log("_Failed to copy Lights: " + ex.Message, LogType.Error); }

            try
            {
                if(Settings.bCopier_animators_copy && CopierTabs.ComponentIsInSelectedTab<Animator>(Settings._copier_selectedTab))
                    LegacyCopier.CopyAllAnimators(objFrom, objTo, Settings.bCopier_animators_createObjects, Settings.bCopier_animators_copyMainAnimator, inst.ignoredTransforms);
            }
            catch(Exception ex) { Log("_Failed to copy Animators: " + ex.Message, LogType.Error); }

            try
            {
                if(Settings.bCopier_audioSources_copy && CopierTabs.ComponentIsInSelectedTab<AudioSource>(Settings._copier_selectedTab))
                    LegacyCopier.CopyAllAudioSources(objFrom, objTo, Settings.bCopier_audioSources_createObjects, inst.ignoredTransforms);
            }
            catch(Exception ex) { Log("_Failed to copy Audio Sources: " + ex.Message, LogType.Error); }


            if(PhysBonesExist)
            {
#if VRC_SDK_VRCSDK3
                try
                {
                    if(Settings.bCopier_contactReceiver_copy && CopierTabs.ComponentIsInSelectedTab("contactreceiver", Settings._copier_selectedTab))
                    {
                        if(Settings.bCopier_contactReceiver_removeOld)
                            LegacyDestroyer.DestroyAllComponentsOfType(objTo, PumkinsTypeCache.ContactReceiver, false);
                        GenericCopier.CopyComponent(PumkinsTypeCache.ContactReceiver, inst, Settings.bCopier_contactReceiver_createObjects, scaleMismatch && Settings.bCopier_contactReceiver_adjustScale, true,
                            false, inst.ignoredTransforms);
                    }
                }
                catch(Exception ex) { Log("_Failed to copy Contact Receivers: " + ex.Message, LogType.Error); }

                try
                {
                    if(Settings.bCopier_contactSender_copy && CopierTabs.ComponentIsInSelectedTab("contactsender", Settings._copier_selectedTab))
                    {
                        if(Settings.bCopier_contactSender_removeOld)
                            LegacyDestroyer.DestroyAllComponentsOfType(objTo, PumkinsTypeCache.ContactSender, false);
                        GenericCopier.CopyComponent(PumkinsTypeCache.ContactSender, inst, Settings.bCopier_contactSender_createObjects, scaleMismatch && Settings.bCopier_contactSender_adjustScale, true, false,
                            inst.ignoredTransforms);
                    }
                }
                catch(Exception ex) { Log("_Failed to copy Contact Senders: " + ex.Message, LogType.Error); }

                try
                {
                    if(Settings.bCopier_physBones_copyColliders && CopierTabs.ComponentIsInSelectedTab("physbonecollider", Settings._copier_selectedTab))
                    {
                        if(Settings.bCopier_physBones_removeOldColliders)
                            LegacyDestroyer.DestroyAllComponentsOfType(objTo, PumkinsTypeCache.PhysBoneCollider, false);
                        GenericCopier.CopyComponent(PumkinsTypeCache.PhysBoneCollider, inst, Settings.bCopier_physBones_createObjectsColliders, Settings.bCopier_physBones_adjustScaleColliders, true, false, inst.ignoredTransforms);
                    }
                }
                catch(Exception ex) { Log("_Failed to copy Physbone Colliders: " + ex.Message, LogType.Error); }

                try
                {
                    if(Settings.bCopier_physBones_copy && CopierTabs.ComponentIsInSelectedTab("physbone", Settings._copier_selectedTab))
                    {
                        if(Settings.bCopier_physBones_removeOldBones)
                            LegacyDestroyer.DestroyAllComponentsOfType(objTo, PumkinsTypeCache.PhysBone, false);
                        GenericCopier.CopyComponent(PumkinsTypeCache.PhysBone, inst, Settings.bCopier_physBones_createObjects, scaleMismatch && Settings.bCopier_physBones_adjustScale, true, false,
                            inst.ignoredTransforms);
                    }
                }
                catch(Exception ex) { Log("_Failed to copy Physbone Colliders: " + ex.Message, LogType.Error); }

                try
                {
                    if(Settings.bCopier_vrcStations_copy && CopierTabs.ComponentIsInSelectedTab("vrcstation", Settings._copier_selectedTab))
                    {
                        GenericCopier.CopyComponent(PumkinsTypeCache.VRCStation , inst, Settings.bCopier_vrcStations_createObjects, false, Settings.bCopier_vrcStations_fixReferences, true,
                            inst.ignoredTransforms);
                    }
                }
                catch(Exception ex) { Log("_Failed to copy VRC Stations: " + ex.Message, LogType.Error); }

                try
                {
                    if(Settings.bCopier_vrcHeadChop_copy && CopierTabs.ComponentIsInSelectedTab("vrcheadchop", Settings._copier_selectedTab))
                    {
                        GenericCopier.CopyComponent(PumkinsTypeCache.VRCHeadChop, inst, Settings.bCopier_vrcHeadChop_createObjects, false, Settings.bCopier_vrcHeadChop_fixReferences, false,
                            inst.ignoredTransforms);
                    }
                }
                catch(Exception ex) { Log("_Failed to copy VRC HeadChop components: " + ex.Message, LogType.Error); }
#endif
            }



            try
            {
                if(DynamicBonesExist)
                {
                    if(Settings.bCopier_dynamicBones_copyColliders && CopierTabs.ComponentIsInSelectedTab("dynamicbonecollider", Settings._copier_selectedTab))
                    {
                        if(Settings.bCopier_dynamicBones_removeOldColliders)
                            LegacyDestroyer.DestroyAllComponentsOfType(objTo, PumkinsTypeCache.DynamicBoneColliderBase, false);
                        GenericCopier.CopyComponent(PumkinsTypeCache.DynamicBoneCollider, inst, Settings.bCopier_dynamicBones_createObjectsColliders,
                            scaleMismatch && Settings.bCopier_dynamicBones_adjustScaleColliders, true, false, inst.ignoredTransforms);
                        GenericCopier.CopyComponent(PumkinsTypeCache.DynamicBonePlaneCollider, inst, Settings.bCopier_dynamicBones_createObjectsColliders,
                            scaleMismatch && Settings.bCopier_dynamicBones_adjustScaleColliders, true, false, inst.ignoredTransforms);
                    }

                    if(Settings.bCopier_dynamicBones_copy && CopierTabs.ComponentIsInSelectedTab("dynamicbone", Settings._copier_selectedTab))
                    {
                        if(Settings.bCopier_dynamicBones_removeOldBones)
                            LegacyDestroyer.DestroyAllComponentsOfType(objTo, PumkinsTypeCache.DynamicBone, false);
                        if(Settings.bCopier_dynamicBones_copy)
                            GenericCopier.CopyComponent(PumkinsTypeCache.DynamicBone, inst, Settings.bCopier_dynamicBones_createObjects, scaleMismatch && Settings.bCopier_dynamicBones_adjustScale,
                                true, false, inst.ignoredTransforms);
                    }
                }
                else if(Settings.bCopier_dynamicBones_copy || Settings.bCopier_dynamicBones_copyColliders)
                {
                    //Log(Strings.Warning.noDBonesInProject, LogType.Error);
                }
            }
            catch(Exception ex) { Log("_Failed to copy Dynamic Bones: " + ex.Message, LogType.Error); }

            try
            {
                if(Settings.bCopier_aimConstraint_copy && CopierTabs.ComponentIsInSelectedTab<AimConstraint>(Settings._copier_selectedTab))
                    LegacyCopier.CopyAllAimConstraints(objFrom, objTo, Settings.bCopier_aimConstraint_createObjects, inst.ignoredTransforms);
            }
            catch(Exception ex) { Log("_Failed to copy Aim Constraints: " + ex.Message, LogType.Error); }

            try
            {
                if(Settings.bCopier_lookAtConstraint_copy && CopierTabs.ComponentIsInSelectedTab<LookAtConstraint>(Settings._copier_selectedTab))
                    LegacyCopier.CopyAllLookAtConstraints(objFrom, objTo, Settings.bCopier_lookAtConstraint_createObjects, inst.ignoredTransforms);
            }
            catch(Exception ex) { Log("_Failed to copy Look At Constraints: " + ex.Message, LogType.Error); }

            try
            {
                if(Settings.bCopier_parentConstraint_copy && CopierTabs.ComponentIsInSelectedTab<ParentConstraint>(Settings._copier_selectedTab))
                    LegacyCopier.CopyAllParentConstraints(objFrom, objTo, Settings.bCopier_parentConstraint_createObjects, inst.ignoredTransforms);
            }
            catch(Exception ex) { Log("_Failed to copy Parent Constraints: " + ex.Message, LogType.Error); }

            try
            {
                if(Settings.bCopier_positionConstraint_copy && CopierTabs.ComponentIsInSelectedTab<PositionConstraint>(Settings._copier_selectedTab))
                    LegacyCopier.CopyAllPositionConstraints(objFrom, objTo, Settings.bCopier_positionConstraint_createObjects, inst.ignoredTransforms);
            }
            catch(Exception ex) { Log("_Failed to copy Position Constraints: " + ex.Message, LogType.Error); }

            try
            {
                if(Settings.bCopier_rotationConstraint_copy && CopierTabs.ComponentIsInSelectedTab<RotationConstraint>(Settings._copier_selectedTab))
                    LegacyCopier.CopyAllRotationConstraints(objFrom, objTo, Settings.bCopier_rotationConstraint_createObjects, inst.ignoredTransforms);
            }
            catch(Exception ex) { Log("_Failed to copy Rotation Constraints: " + ex.Message, LogType.Error); }

            try
            {
                if(Settings.bCopier_scaleConstraint_copy && CopierTabs.ComponentIsInSelectedTab<ScaleConstraint>(Settings._copier_selectedTab))
                    LegacyCopier.CopyAllScaleConstraints(objFrom, objTo, Settings.bCopier_scaleConstraint_createObjects, inst.ignoredTransforms);
            }
            catch(Exception ex) { Log("_Failed to copy Scale Constraints: " + ex.Message, LogType.Error); }

#if VRC_SDK_VRCSDK3

            try
            {
                if(Settings.bCopier_vrcAimConstraint_copy && CopierTabs.ComponentIsInSelectedTab(PumkinsTypeCache.VRCAimConstraint, Settings._copier_selectedTab))
                    GenericCopier.CopyComponent(PumkinsTypeCache.VRCAimConstraint, inst, Settings.bCopier_vrcAimConstraint_createObjects, false, true, false, inst.ignoredTransforms);
            }
            catch(Exception ex) { Log("_Failed to copy VRC Aim Constraints: " + ex.Message, LogType.Error); }

            try
            {
                if(Settings.bCopier_vrcLookAtConstraint_copy && CopierTabs.ComponentIsInSelectedTab(PumkinsTypeCache.VRCLookAtConstraint, Settings._copier_selectedTab))
                    GenericCopier.CopyComponent(PumkinsTypeCache.VRCLookAtConstraint, inst, Settings.bCopier_vrcLookAtConstraint_createObjects, false, true, false, inst.ignoredTransforms);
            }
            catch(Exception ex) { Log("_Failed to copy VRC Look At Constraints: " + ex.Message, LogType.Error); }

            try
            {
                if(Settings.bCopier_vrcParentConstraint_copy && CopierTabs.ComponentIsInSelectedTab(PumkinsTypeCache.VRCParentConstraint, Settings._copier_selectedTab))
                    GenericCopier.CopyComponent(PumkinsTypeCache.VRCParentConstraint, inst, Settings.bCopier_vrcParentConstraint_createObjects, false, true, false, inst.ignoredTransforms);
            }
            catch(Exception ex) { Log("_Failed to copy VRC Parent Constraints: " + ex.Message, LogType.Error); }

            try
            {
                if(Settings.bCopier_vrcPositionConstraint_copy && CopierTabs.ComponentIsInSelectedTab(PumkinsTypeCache.VRCPositionConstraint, Settings._copier_selectedTab))
                    GenericCopier.CopyComponent(PumkinsTypeCache.VRCPositionConstraint, inst, Settings.bCopier_vrcPositionConstraint_createObjects, false, true, false, inst.ignoredTransforms);
            }
            catch(Exception ex) { Log("_Failed to copy VRC Position Constraints: " + ex.Message, LogType.Error); }

            try
            {
                if(Settings.bCopier_vrcRotationConstraint_copy && CopierTabs.ComponentIsInSelectedTab(PumkinsTypeCache.VRCRotationConstraint, Settings._copier_selectedTab))
                    GenericCopier.CopyComponent(PumkinsTypeCache.VRCRotationConstraint, inst, Settings.bCopier_vrcRotationConstraint_createObjects, false, true, false, inst.ignoredTransforms);
            }
            catch(Exception ex) { Log("_Failed to copy VRC Rotation Constraints: " + ex.Message, LogType.Error); }

            try
            {
                if(Settings.bCopier_vrcScaleConstraint_copy && CopierTabs.ComponentIsInSelectedTab(PumkinsTypeCache.VRCScaleConstraint, Settings._copier_selectedTab))
                    GenericCopier.CopyComponent(PumkinsTypeCache.VRCScaleConstraint, inst, Settings.bCopier_vrcScaleConstraint_createObjects, false, true, false, inst.ignoredTransforms);
            }
            catch(Exception ex) { Log("_Failed to copy VRC Scale Constraints: " + ex.Message, LogType.Error); }
#endif

            try
            {
                if(Settings.bCopier_joints_copy && CopierTabs.ComponentIsInSelectedTab<Joint>(Settings._copier_selectedTab))
                {
                    if(Settings.bCopier_joints_removeOld)
                        LegacyDestroyer.DestroyAllComponentsOfType(objTo, typeof(Joint), false);
                    LegacyCopier.CopyAllJoints(objFrom, objTo, Settings.bCopier_joints_createObjects, inst.ignoredTransforms);
                }
            }
            catch(Exception ex) { Log("_Failed to copy Joints: " + ex.Message, LogType.Error); }

            try
            {
                if(Settings.bCopier_transforms_copy && CopierTabs.ComponentIsInSelectedTab<Transform>(Settings._copier_selectedTab))
                    LegacyCopier.CopyAllTransforms(objFrom, objTo, inst.ignoredTransforms);
            }
            catch(Exception ex) { Log("_Failed to copy Transforms: " + ex.Message, LogType.Error); }

            try
            {
                if(Settings.bCopier_meshRenderers_copy && CopierTabs.ComponentIsInSelectedTab<MeshRenderer>(Settings._copier_selectedTab))
                    LegacyCopier.CopyAllMeshRenderers(objFrom, objTo, Settings.bCopier_meshRenderers_createObjects, inst.ignoredTransforms);
            }
            catch(Exception ex) { Log("_Failed to copy Mesh Renderer: " + ex.Message, LogType.Error); }

            try
            {
                if(Settings.bCopier_skinMeshRender_copy && CopierTabs.ComponentIsInSelectedTab<SkinnedMeshRenderer>(Settings._copier_selectedTab))
                    LegacyCopier.CopyAllSkinnedMeshRenderers(objFrom, objTo, Settings.bCopier_skinMeshRender_createObjects, inst.ignoredTransforms);
            }
            catch(Exception ex) { Log("_Failed to copy Skinned Mesh Renderers: " + ex.Message, LogType.Error); }

            try
            {
                if(Settings.bCopier_cameras_copy && CopierTabs.ComponentIsInSelectedTab<Camera>(Settings._copier_selectedTab))
                    LegacyCopier.CopyCameras(objFrom, objTo, Settings.bCopier_cameras_createObjects, inst.ignoredTransforms);
            }
            catch(Exception ex) { Log("_Failed to copy Camera: " + ex.Message, LogType.Error); }

            try
            {
                if(Settings.bCopier_transforms_copy && Settings.bCopier_transforms_copyActiveState && CopierTabs.ComponentIsInSelectedTab<Transform>(Settings._copier_selectedTab))
                    LegacyCopier.CopyTransformActiveStateTagsAndLayer(objFrom, objTo, inst.ignoredTransforms);
            }
            catch(Exception ex) { Log("_Failed to copy GameObject Active states, Layers and Tags: " + ex.Message, LogType.Error); }

            if(_DependencyChecker.FinalIKExists && Settings.bCopier_finalIK_copy && CopierTabs.ComponentIsInSelectedTab("finalik", Settings._copier_selectedTab))
            {
                CopyFinalIKComponents(PumkinsTypeCache.CCDIK, Settings.bCopier_finalIK_copyCCDIK, "CCDIK");
                CopyFinalIKComponents(PumkinsTypeCache.LimbIK, Settings.bCopier_finalIK_copyLimbIK, "LimbIK");
                CopyFinalIKComponents(PumkinsTypeCache.RotationLimit, Settings.bCopier_finalIK_copyRotationLimits, "RotationLimits");
                CopyFinalIKComponents(PumkinsTypeCache.FABRIK, Settings.bCopier_finalIK_copyFabrik, "FABRIK");
                CopyFinalIKComponents(PumkinsTypeCache.AimIK, Settings.bCopier_finalIK_copyAimIK, "AimIK");
                CopyFinalIKComponents(PumkinsTypeCache.FullBodyBipedIK, Settings.bCopier_finalIK_copyFBTBipedIK, "FullBodyBipedIK");
                CopyFinalIKComponents(PumkinsTypeCache.VRIK, Settings.bCopier_finalIK_copyVRIK, "VRIK");
                CopyFinalIKComponents(PumkinsTypeCache.Grounder, Settings.bCopier_finalIK_copyGrounders, "Grounder");
                CopyFinalIKComponents(PumkinsTypeCache.IKExecutionOrder, Settings.bCopier_finalIK_copyIKExecutionOrder, "IKExecutionOrder");

                void CopyFinalIKComponents(Type type, bool toggle, string componentName)
                {
                    try
                    {
                        if(toggle)
                            GenericCopier.CopyComponent(type, inst, Settings.bCopier_finalIK_createObjects, false, true, true, inst.ignoredTransforms);
                    }
                    catch(Exception ex) { Log($"_Failed to copy FinalIK {componentName}: " + ex.Message, LogType.Error); }
                }
            }

            try
            {
#if VRC_SDK_VRCSDK3
                if(Settings.bCopier_descriptor_copy && CopierTabs.ComponentIsInSelectedTab(PumkinsTypeCache.VRC_AvatarDescriptor, Settings._copier_selectedTab))
                {
                    LegacyCopier.CopyAvatarDescriptor(objFrom, objTo, inst.ignoredTransforms);
                    if(Settings.bCopier_descriptor_copyAvatarScale)
                    {
                        Component descriptor = objTo.GetComponentInChildren(PumkinsTypeCache.VRC_AvatarDescriptor);
                        if(descriptor)
                        {
                            dynamic desc = Convert.ChangeType(descriptor, PumkinsTypeCache.VRC_AvatarDescriptor);
                            if(!(Settings.bCopier_descriptor_copy && Settings.bCopier_descriptor_copyViewpoint))
                                SetAvatarScaleAndMoveViewpoint(desc, objFrom.transform.localScale.z);
                            objTo.transform.localScale = new Vector3(objFrom.transform.localScale.x,
                                objFrom.transform.localScale.y, objFrom.transform.localScale.z);
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
            }
            catch(Exception ex) { Log("_Failed to copy Avatar Descriptor: " + ex.Message, LogType.Error); }

            try
            {
                if(PumkinsTypeCache.HasExtraTypes && Settings.bCopier_other_copy)
                {
                    foreach(var typeCategory in PumkinsTypeCache.ExtraTypes)
                    {
                        for(int i = 0; i < typeCategory.types.Count; i++)
                        {
                            if(!typeCategory.enableStates[i])
                                continue;
                            GenericCopier.CopyComponent(typeCategory.types[i], inst, Settings.bCopier_other_createGameObjects, false, Settings.bCopier_other_fixReferences, false, inst.ignoredTransforms);
                        }
                    }
                }
            }
            catch(Exception ex) { Log("_Failed to copy Other objects: " + ex.Message, LogType.Error); }
        }

        /// <summary>
        /// Saves serialized variables to PlayerPrefs.
        /// Used to keep same settings when restarting unity or going into play mode
        /// </summary>
        void SavePrefs()
        {
            var data = JsonUtility.ToJson(Settings, false);
            EditorPrefs.SetString(SettingsEditorPrefsKey, data);
            PumkinsTypeCache.SaveExtraTypeEnableStates();
            LogVerbose("Saved tool window preferences");
        }

        /// <summary>
        /// Loads serialized variables from PlayerPrefs.
        /// Used to keep same settings when restarting unity or going into play mode
        /// </summary>
        void LoadPrefs()
        {
            var data = EditorPrefs.GetString(SettingsEditorPrefsKey, JsonUtility.ToJson(Settings, false));
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
            PumkinsTypeCache.LoadExtraTypes();
        }
    }
}
