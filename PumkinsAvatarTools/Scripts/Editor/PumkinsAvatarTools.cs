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
using VRC.Core;
using VRCSDK2;
using Pumkin.DataStructures;
using Pumkin.HelperFunctions;
using Pumkin.Extensions;
using UnityEngine.SceneManagement;
using Pumkin.Presets;
using UnityEditor.Experimental.SceneManagement;

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
        [SerializeField] bool _tools_quickSetup_autoRig = true;
        [SerializeField] bool _tools_quickSetup_forceTPose = true;

        [SerializeField] float _tools_quickSetup_viewpointZDepth = 0.06f;        

        [SerializeField] bool _tools_quickSetup_setRendererAnchor = true;
        [SerializeField] string _tools_quickSetup_setRenderAnchor_path = "Armature/Hips/Spine";
        
        
        //Editing Viewpoint        
        bool _editingView = false;
        Vector3 _viewPosOld;
        Vector3 _viewPosTemp;
        Tool _tempToolOld = Tool.None;
        static readonly Vector3 DEFAULT_VIEWPOINT = new Vector3(0, 1.6f, 0.2f);        

        //Editing Scale
        bool _editingScale = false;
        Vector3 _avatarScaleOld;
        [SerializeField] float _avatarScaleTemp;
        [SerializeField] SerializedProperty _serializedAvatarScaleTempProp;
        Transform _scaleViewpointDummy;

        VRC_AvatarDescriptor _tempAvatarDescriptor;

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
        };

        #endregion

        #region Component Copier

        [SerializeField] public static GameObject copierSelectedFrom;

        [SerializeField] bool bCopier_transforms_copy = true;
        [SerializeField] bool bCopier_transforms_copyPosition = false;
        [SerializeField] bool bCopier_transforms_copyRotation = false;
        [SerializeField] bool bCopier_transforms_copyScale = false;
        [SerializeField] bool bCopier_transforms_copyAvatarScale = true;

        [SerializeField] bool bCopier_dynamicBones_copy = true;
        [SerializeField] bool bCopier_dynamicBones_copySettings = false;
        [SerializeField] bool bCopier_dynamicBones_createMissing = true;
        [SerializeField] bool bCopier_dynamicBones_copyColliders = true;
        [SerializeField] bool bCopier_dynamicBones_removeOldColliders = false;
        [SerializeField] bool bCopier_dynamicBones_removeOldBones = false;
        [SerializeField] bool bCopier_dynamicBones_createObjectsColliders = true;

        [SerializeField] bool bCopier_descriptor_copy = true;
        [SerializeField] bool bCopier_descriptor_copySettings = true;
        [SerializeField] bool bCopier_descriptor_copyPipelineId = false;
        [SerializeField] bool bCopier_descriptor_copyAnimationOverrides = true;
        [SerializeField] bool bCopier_descriptor_copyViewpoint = true;

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
        //[SerializeField] bool bCopier_joints_copySettings = true;
        //[SerializeField] bool bCopier_joints_fixed = true;
        //[SerializeField] bool bCopier_joints_character = true;
        //[SerializeField] bool bCopier_joints_configurable = true;
        //[SerializeField] bool bCopier_joints_sprint = true;
        //[SerializeField] bool bCopier_joints_hinge = true;
        //[SerializeField] bool bCopier_joints_createMissing = true;
        //[SerializeField] bool bCopier_joints_createObjects = true;

        [SerializeField] bool bCopier_audioSources_copy = true;
        [SerializeField] bool bCopier_audioSources_copySettings = true;
        [SerializeField] bool bCopier_audioSources_createMissing = true;
        [SerializeField] bool bCopier_audioSources_createObjects = true;

        //UI Expand
        [SerializeField] bool _copier_expand = false;
        [SerializeField] bool _copier_expand_transforms = false;
        [SerializeField] bool _copier_expand_dynamicBones = false;
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
        GameObject _cameraOverlay = null;
        GameObject _cameraBackground = null;
        RawImage _cameraOverlayImage = null;
        RawImage _cameraBackgroundImage = null;
        [SerializeField] bool _centerCameraOffsets_expand = false;
        [SerializeField] int _presetToolbarSelectedIndex = 0;

        public enum PresetToolbarOptions { Camera, Pose, Blendshape };

        [SerializeField] public int _selectedCameraPresetIndex = 0;
        [SerializeField] public string _selectedCameraPresetString = "";

        [SerializeField] public string _selectedPosePresetString = "";
        [SerializeField] public int _selectedPosePresetIndex = 0;

        [SerializeField] public string _selectedBlendshapePresetString = "";
        [SerializeField] public int _selectedBlendshapePresetIndex = 0;

        [SerializeField] Vector3 centerCameraPositionOffset = new Vector3(-0.02f, 0, 0);
        [SerializeField] Vector3 centerCameraRotationOffset = new Vector3(5, 166, 0);
        [SerializeField] bool centerCameraFixClippingPlanes = true;

        readonly Vector3 _defaultCameraPositionOffset = new Vector3(0, 0.347f, 0.225f);
        readonly Vector3 _defaultCameraRotationOffset = new Vector3(0, 180f, 0);

        static Camera _selectedCamera;

        static Material _rtMat;
        static RenderTexture _rtTex;
        static RenderTexture _defaultRT;

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
        string _lastOpenFilePath = "";
        public string _backgroundPathText = "";
        public string _overlayPathText = "";

        public CameraBackgroundOverrideType cameraBackgroundType = CameraBackgroundOverrideType.Color;

        public bool vrcCamSetBgColor = false;
        bool vrcCamSetBGSkybox = false;
        bool vrcCamSetBGImage = false;
        [SerializeField] public Color vrcCamBgColor = Colors.DarkCameraBackground;
        [SerializeField] Color _vrcCamColorOld = Colors.DefaultCameraBackground;
        [SerializeField] CameraClearFlags _vrcCamClearFlagsOld = CameraClearFlags.Skybox;
        Material _thumbsSkyboxOld = null;

        public enum CameraBackgroundOverrideType { Color, Material, Image };

        readonly string CAMERA_OVERLAY_NAME = "_PumkinsCameraOverlay";
        readonly string CAMERA_BACKGROUND_NAME = "_PumkinsCameraBackground";

        [SerializeField] HumanPose _tempHumanPose = new HumanPose();
        [SerializeField] float[] _tempHumanPoseMuscles;
        [SerializeField] SerializedProperty _serializedTempHumanPoseMuscles;

        #endregion

        #region Avatar Info

        static AvatarInfo avatarInfo = new AvatarInfo();
        static string _avatarInfoSpace = "\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n";
        static string _avatarInfoString = Strings.AvatarInfo.selectAvatarFirst + _avatarInfoSpace; //Please don't hurt me for this        

        #endregion

        #region Misc

        //UI
        [SerializeField] public bool _tools_expand = true;
        [SerializeField] bool _tools_avatar_expand = true;
        [SerializeField] bool _tools_removeAll_expand = true;

        [SerializeField] public bool _avatarInfo_expand = false;
        [SerializeField] public bool _thumbnails_expand = false;

        [SerializeField] public bool _misc_expand = true;
        [SerializeField] bool _thumbnails_useCameraOverlay_expand = true;
        [SerializeField] bool _thumbnails_useCameraBackground_expand = true;

        //Misc
        SerializedObject _serializedScript;
        [SerializeField] bool _openedInfo = false;
        [SerializeField] Vector2 _mainScroll = Vector2.zero;
        [SerializeField] bool verboseLoggingEnabled = false;
        GameObject oldSelectedAvatar = null;

        static string _mainScriptPath = null;
        static string _mainFolderPath = null;
        private static Camera _vrcCam = null; //use property
        static bool _eventsAdded = false;

        [SerializeField] public string _selectedLanguageString = "English - Default";
        [SerializeField] int _selectedLanguageIndex = 0;

        readonly float COPIER_SETTINGS_INDENT_SIZE = 38f;

        #endregion        

        #endregion

        #region Properties

        public static PumkinsAvatarTools Instance
        {
            get { return _PumkinsAvatarToolsWindow.ToolsWindow; }
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

        public static string MainScriptPath
        {
            get
            {
                if(_mainScriptPath == null)
                {
                    var toolScriptPath = Directory.GetFiles(Application.dataPath, "PumkinsAvatarTools.cs", SearchOption.AllDirectories)[0];
                    string s = _DependencyChecker.RelativePath(toolScriptPath.Substring(0, toolScriptPath.LastIndexOf('\\')));
                    _mainScriptPath = s;
                }
                return _mainScriptPath;
            }

            private set
            {
                _mainScriptPath = value;
            }
        }

        public static string MainFolderPath
        {
            get
            {
                if(_mainFolderPath == null)
                {
                    string[] folder = Directory.GetDirectories(Application.dataPath, "PumkinsAvatarTools", SearchOption.AllDirectories);
                    _mainFolderPath = folder[0];
                }
                return _mainFolderPath;
            }

            private set
            {
                _mainScriptPath = value;
            }
        }

        public static RenderTexture DefaultRT
        {
            get
            {
                return _defaultRT = Resources.Load("Materials/PumkinsThumbnailCamRT", typeof(RenderTexture)) as RenderTexture;
            }
        }

        public static RenderTexture RTTexture
        {
            get
            {
                if(!_rtTex)
                {
                    _rtTex = DefaultRT;
                }
                return _rtTex;
            }
            set
            {
                _rtTex = value;

                if(SelectedCamera)
                    SelectedCamera.targetTexture = _rtTex;
            }
        }
        public static Material RTMaterial
        {
            get
            {
                if(!_rtMat)
                {
                    _rtMat = Resources.Load("Materials/PumkinsThumbnailCamUnlit", typeof(Material)) as Material;
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
                if(!_selectedCamera)
                    _selectedCamera = GetVRCCamOrMainCam();

                return _selectedCamera;
            }
            set
            {
                _selectedCamera = value;
            }
        }

        public GameObject CameraOverlay
        {
            get
            {
                if(SelectedCamera && !_cameraOverlay && _thumbnails_expand)
                    _cameraOverlay = GameObject.Find(CAMERA_OVERLAY_NAME) ?? new GameObject(CAMERA_OVERLAY_NAME);

                return _cameraOverlay;
            }
        }

        public RawImage CameraOverlayRawImage
        {
            get
            {
                if(SelectedCamera && !_cameraOverlayImage && CameraOverlay)
                {
                    _cameraOverlayImage = CameraOverlay.GetComponent<RawImage>() ?? CameraOverlay.AddComponent<RawImage>();

                    Canvas c = CameraOverlay.GetComponent<Canvas>();
                    if(!c)
                        c = CameraOverlay.AddComponent<Canvas>();
                    c.worldCamera = SelectedCamera;
                    c.renderMode = RenderMode.ScreenSpaceCamera;
                    c.planeDistance = SelectedCamera.nearClipPlane + 0.01f;

                }
                return _cameraOverlayImage;
            }
        }

        public GameObject CameraBackground
        {
            get
            {
                if(SelectedCamera && !_cameraBackground && _thumbnails_expand)
                    _cameraBackground = GameObject.Find(CAMERA_BACKGROUND_NAME) ?? new GameObject(CAMERA_BACKGROUND_NAME);
                return _cameraBackground;
            }
        }

        public RawImage CameraBackgroundRawImage
        {
            get
            {
                if(SelectedCamera && _cameraBackgroundImage == null && CameraBackground)
                {
                    _cameraBackgroundImage = CameraBackground.GetComponent<RawImage>();
                    if(_cameraBackgroundImage == null)
                        _cameraBackgroundImage = CameraBackground.AddComponent<RawImage>();

                    Canvas c = CameraBackground.GetComponent<Canvas>();
                    if(!c)
                        c = CameraBackground.AddComponent<Canvas>();
                    c.worldCamera = SelectedCamera;
                    c.renderMode = RenderMode.ScreenSpaceCamera;
                    c.planeDistance = SelectedCamera.farClipPlane - 1.5f;
                }
                return _cameraBackgroundImage;
            }
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

        #endregion

        #region Events and Delegates

        public delegate void AvatarChangedHandler(GameObject selection);
        public delegate void PoseChangedHandler(PoseChangeType changeType);

        public static event AvatarChangedHandler AvatarSelectionChanged;
        public static event PoseChangedHandler PoseChanged;

        public enum PoseChangeType { Reset, Normal, PoseEditor };

        #endregion

        #region Event Definitions

        public static void OnAvatarSelectionChanged(GameObject selectedAvatar)
        {
            AvatarSelectionChanged?.Invoke(selectedAvatar);
            LogVerbose("Avatar selection changed to " + selectedAvatar?.name);
        }

        public static void OnPoseWasChanged(PoseChangeType changeType)
        {
            PoseChanged?.Invoke(changeType);
            LogVerbose("Pose was changed and OnPoseWasChanged() was called with changeType as " + changeType.ToString());
        }

        #endregion

        #region Callback Handlers

        public void HandleOnEnable()
        {
            LogVerbose("Tools window: OnEnable()");
            SceneView.onSceneGUIDelegate += OnSceneGUI;
            if(!_eventsAdded)
            {
                EditorApplication.playModeStateChanged += HandlePlayModeStateChange;
                Selection.selectionChanged += HandleSelectionChanged;
                EditorSceneManager.sceneOpened += HandleSceneChange;
                PrefabStage.prefabStageOpened += HandlePrefabStageOpened;
                PrefabStage.prefabStageClosing += HandlePrefabStageClosed;

                _eventsAdded = true;
            }

            SerializedScript = new SerializedObject(this);
            SerializedIgnoreArray = SerializedScript.FindProperty("_copierIgnoreArray");
            SerializedScaleTemp = SerializedScript.FindProperty("_avatarScaleTemp");
            SerializedHumanPoseMuscles = SerializedScript.FindProperty("_tempHumanPoseMuscles");            

            _emptyTexture = new Texture2D(2, 2);
            cameraOverlayTexture = new Texture2D(2, 2);
            cameraBackgroundTexture = new Texture2D(2, 2);
            _thumbsSkyboxOld = RenderSettings.skybox;

            LoadPrefs();

            RestoreTexturesFromPaths();
        }        

        public void HandleSceneChange(Scene scene, OpenSceneMode mode)
        {
            Debug.Log("We changed scene");
            if(PrefabStageUtility.GetCurrentPrefabStage() != null)
            {
                Debug.Log("We're in prefab mode!");
            }
            else
            {
                Debug.Log("Not in prefab mode");
            }
            if(mode == OpenSceneMode.Single)
            {
                RefreshLanguage();
            }
        }

        public void HandleOnDisable()
        {
            LogVerbose("Tools window: OnDisable()");
            SceneView.onSceneGUIDelegate -= OnSceneGUI;
            Selection.selectionChanged -= HandleSelectionChanged;
            EditorApplication.playModeStateChanged -= HandlePlayModeStateChange;
            _eventsAdded = false;

            if(SerializedScript != null)
                SerializedScript.ApplyModifiedProperties();

            EndEditingViewpoint(null, true);
            EndScalingAvatar(null, true);

            SavePrefs();
        }

        void HandlePlayModeStateChange(PlayModeStateChange mode)
        {
            if(mode == PlayModeStateChange.ExitingEditMode || mode == PlayModeStateChange.ExitingPlayMode)
            {
                if(_editingView)
                    EndEditingViewpoint(SelectedAvatar, true);
                if(_editingScale)
                    EndScalingAvatar(SelectedAvatar, true);

                SavePrefs();
            }
            else if(mode == PlayModeStateChange.EnteredEditMode)
            {
                LoadPrefs();
                ResetBackground();
            }
            else if(mode == PlayModeStateChange.EnteredPlayMode)
            {
                _editingView = false;

                LoadPrefs();
                _emptyTexture = new Texture2D(2, 2);
                cameraOverlayTexture = new Texture2D(2, 2);
                _thumbsSkyboxOld = RenderSettings.skybox;

                SelectedCamera = GetVRCCamOrMainCam();

                var pm = FindObjectOfType<RuntimeBlueprintCreation>();
                if(pm && pm.pipelineManager && pm.pipelineManager.contentType == PipelineManager.ContentType.avatar)
                {
                    SelectedAvatar = pm.pipelineManager.transform.root.gameObject;
                }

                HideAllOtherAvatars(shouldHideOtherAvatars, SelectedAvatar);
            }
        }

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
                        av[i].transform.root.gameObject.SetActive(hidden);
                }
            }            
        }

        void HandleSelectionChanged()
        {
            if(_useSceneSelectionAvatar)
                SelectAvatarFromScene();
            _PumkinsAvatarToolsWindow.RequestRepaint(this);
        }

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

        #endregion

        #region Unity GUI                   
                
        //[MenuItem("Tools/Pumkin/Pose Editor")]
        public static void ShowWindow()
        {
            //Show existing window instance. If one doesn't exist, make one.
            EditorWindow editorWindow = EditorWindow.GetWindow(typeof(PumkinsAvatarTools));
            editorWindow.autoRepaintOnSceneChange = true;

            editorWindow.Show();
            editorWindow.titleContent = new GUIContent("Pumkin Tools");

            _DependencyChecker.Check();
        }

        public void OnGUI()
        {
            SerializedScript.Update();

            int tempSize = Styles.Label_mainTitle.fontSize + 6;
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField(Strings.Main.title, Styles.Label_mainTitle, GUILayout.MinHeight(tempSize));

                EditorGUIUtility.SetIconSize(new Vector2(tempSize - 3, tempSize - 3));

                if(GUILayout.Button(Icons.Star, "IconButton", GUILayout.MaxWidth(tempSize + 3)))
                {
                    _openedInfo = !_openedInfo;
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField(Strings.Credits.version);

            if(_openedInfo) //Credits Screen
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

                EditorGUILayout.Space();
                GUILayout.Label(Strings.Misc.superExperimental + ':');
                if(GUILayout.Button(Strings.Buttons.openPoseEditor))
                {
                    PumkinsPoseEditor.ShowWindow();
                }                

                verboseLoggingEnabled = EditorGUILayout.Toggle("Enable verbose logging", verboseLoggingEnabled);

                GUILayout.FlexibleSpace();

                if(GUILayout.Button(Strings.Misc.uwu, "IconButton", GUILayout.ExpandWidth(false)))
                {
                    if(Strings.Misc.uwu == "uwu")
                        Strings.Misc.uwu = "OwO";
                    else
                        Strings.Misc.uwu = "uwu";                    
                }
            }
            else
            {
                EditorGUIUtility.SetIconSize(new Vector2(15, 15));

                EditorGUILayout.Space();
                
                SelectedAvatar = (GameObject)EditorGUILayout.ObjectField(Strings.Main.avatar, SelectedAvatar, typeof(GameObject), true);                

                if(_useSceneSelectionAvatar)
                {
                    if(Selection.activeObject != SelectedAvatar)
                        SelectAvatarFromScene();
                }

                if(GUILayout.Button(Strings.Buttons.selectFromScene))
                {
                    if(Selection.activeObject)                    
                        SelectAvatarFromScene();
                }                

                _useSceneSelectionAvatar = GUILayout.Toggle(_useSceneSelectionAvatar, Strings.Main.useSceneSelection);
                
                Helpers.DrawGuiLine();

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

                    DrawMiscMenuGUI();

                    EditorGUILayout.Space();
                }
                EditorGUILayout.EndScrollView();


                if(GUI.changed)
                {                    
                    SerializedScript.ApplyModifiedProperties();
                    EditorUtility.SetDirty(this);                 
                }                                             
            }
        }

        //Draws the small windows inside the scene view when scaling the avatar or moving the viewpoint
        void OnSceneGUI(SceneView sceneView)
        {
            if(_editingScale)
            {
                bool propertyChanged = false;
                if(!SelectedAvatar)
                {
                    EndScalingAvatar(null, true);
                    return;
                }

                Vector2 windowSize = new Vector2(200, 65);

                Handles.BeginGUI();
                {
                    var r = SceneView.currentDrawingSceneView.camera.pixelRect;
                    GUILayout.BeginArea(new Rect(10, r.height - 10 - windowSize.y, windowSize.x, windowSize.y), Styles.Box);
                    {
                        GUILayout.Label(Strings.Tools.editScale);
                        if(SerializedScaleTemp != null)
                        {
                            EditorGUILayout.PropertyField(SerializedScaleTemp, GUIContent.none);
                            if(SerializedScript.ApplyModifiedProperties())
                                propertyChanged = true;
                        }
                        else
                        {
                            EditorGUILayout.LabelField(_avatarScaleTemp.ToString());
                        }

                        GUILayout.BeginHorizontal();
                        {
                            if(GUILayout.Button(Strings.Buttons.cancel, GUILayout.MinWidth(80)))
                            {
                                EndScalingAvatar(SelectedAvatar, true);
                            }

                            if(GUILayout.Button(Strings.Buttons.apply, GUILayout.MinWidth(80)))
                            {
                                EndScalingAvatar(SelectedAvatar, false);
                            }
                        }
                        GUILayout.EndHorizontal();
                    }
                    GUILayout.EndArea();
                }
                Handles.EndGUI();

                if(_tempAvatarDescriptor)
                {
                    EditorGUI.BeginChangeCheck();
                    {
                        _avatarScaleTemp = Handles.ScaleSlider(_avatarScaleTemp, SelectedAvatar.transform.position, Vector3.up, Quaternion.identity, HandleUtility.GetHandleSize(SelectedAvatar.transform.position) * 2, 0.01f);
                    }
                    if(EditorGUI.EndChangeCheck() || propertyChanged)
                    {
                        SetAvatarScale(_tempAvatarDescriptor, _avatarScaleTemp);
                    }

                    Handles.color = Colors.BallHandle;
                    Handles.SphereHandleCap(0, _viewPosTemp, Quaternion.identity, 0.02f, EventType.Repaint);
                }
                else
                    EndScalingAvatar(null, true);
            }
            if(_editingView)
            {
                if(!SelectedAvatar)
                {
                    EndEditingViewpoint(null, true);
                    return;
                }

                Vector2 windowSize = new Vector2(200, 50);

                Handles.BeginGUI();
                {
                    var r = SceneView.currentDrawingSceneView.camera.pixelRect;
                    GUILayout.BeginArea(new Rect(10, r.height - 10 - windowSize.y, windowSize.x, windowSize.y), Styles.Box);
                    {
                        GUILayout.Label(Strings.Tools.editViewpoint);
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
            if(DrawingHandlesGUI)
                _PumkinsAvatarToolsWindow.RequestRepaint(this);
        }

        //Draws the viewpoint sphere
        void OnDrawGizmos()
        {
            if(DrawingHandlesGUI)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(_viewPosTemp, 0.1f);
            }
        }

        private void DrawCopierMenuGUI()
        {
            if(_copier_expand = GUILayout.Toggle(_copier_expand, Strings.Main.copier, Styles.Foldout_title))
            {
                EditorGUILayout.Space();
                copierSelectedFrom = (GameObject)EditorGUILayout.ObjectField(Strings.Copier.copyFrom, copierSelectedFrom, typeof(GameObject), true);

                EditorGUILayout.BeginHorizontal();
                if(GUILayout.Button(Strings.Buttons.selectFromScene))
                {
                    if(Selection.activeGameObject != null)
                        copierSelectedFrom = Selection.activeGameObject.transform.root.gameObject;
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();

                EditorGUI.BeginDisabledGroup(copierSelectedFrom == null || SelectedAvatar == null);
                {
                    Helpers.DrawGuiLine(1, false);

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

                                bCopier_transforms_copyAvatarScale = GUILayout.Toggle(bCopier_transforms_copyAvatarScale, Strings.Copier.transforms_avatarScale, Styles.CopierToggle);
                            }
                        }

                        EditorGUILayout.Space();
                        EditorGUI.EndDisabledGroup();
                    }

                    Helpers.DrawGuiLine(1, false);

                    //DynamicBones menu
#if !BONES && !OLD_BONES                                        
                    EditorGUI.BeginDisabledGroup(true);
                    {
                        Helpers.DrawDropdownWithToggle(ref _copier_expand_dynamicBones, ref bCopier_dynamicBones_copy, Strings.Copier.dynamicBones + " (" + Strings.Warning.notFound + ")", Icons.BoneIcon);
                        bCopier_dynamicBones_copy = false;                        
                    }
                    EditorGUI.EndDisabledGroup();
#elif OLD_BONES
                    Helpers.DrawDropdownWithToggle(ref _copier_expand_dynamicBones, ref bCopier_dynamicBones_copy, Strings.Copier.dynamicBones + " (" + Strings.Warning.oldVersion + ")", Icons.BoneIcon);
#elif BONES
                    Helpers.DrawDropdownWithToggle(ref _copier_expand_dynamicBones, ref bCopier_dynamicBones_copy, Strings.Copier.dynamicBones, Icons.BoneIcon);
#endif
                    
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
                                bCopier_dynamicBones_removeOldBones = GUILayout.Toggle(bCopier_dynamicBones_removeOldBones, Strings.Copier.dynamicBones_removeOldBones, Styles.CopierToggle);
                                EditorGUILayout.Space();
                                bCopier_dynamicBones_copyColliders = GUILayout.Toggle(bCopier_dynamicBones_copyColliders, Strings.Copier.dynamicBones_colliders, Styles.CopierToggle);
                                bCopier_dynamicBones_removeOldColliders = GUILayout.Toggle(bCopier_dynamicBones_removeOldColliders, Strings.Copier.dynamicBones_removeOldColliders, Styles.CopierToggle);
                                bCopier_dynamicBones_createObjectsColliders = GUILayout.Toggle(bCopier_dynamicBones_createObjectsColliders, Strings.Copier.copyColliderObjects, Styles.CopierToggle);
                            }
                        }

                        EditorGUILayout.Space();
                        EditorGUI.EndDisabledGroup();
                    }

                    Helpers.DrawGuiLine(1, false);

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
                                bCopier_descriptor_copySettings = GUILayout.Toggle(bCopier_descriptor_copySettings, Strings.Copier.copySettings, Styles.CopierToggle);
                                bCopier_descriptor_copyViewpoint = GUILayout.Toggle(bCopier_descriptor_copyViewpoint, Strings.Copier.descriptor_copyViewpoint, Styles.CopierToggle);
                                bCopier_descriptor_copyPipelineId = GUILayout.Toggle(bCopier_descriptor_copyPipelineId, Strings.Copier.descriptor_pipelineId, Styles.CopierToggle);
                                bCopier_descriptor_copyAnimationOverrides = GUILayout.Toggle(bCopier_descriptor_copyAnimationOverrides, Strings.Copier.descriptor_animationOverrides, Styles.CopierToggle);
                            }
                        }

                        EditorGUILayout.Space();
                        EditorGUI.EndDisabledGroup();
                    }

                    Helpers.DrawGuiLine(1, false);

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

                    Helpers.DrawGuiLine(1, false);

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

                    Helpers.DrawGuiLine(1, false);

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

                    Helpers.DrawGuiLine(1, false);

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

                    Helpers.DrawGuiLine(1, false);

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

                    Helpers.DrawGuiLine(1, false);

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

                    Helpers.DrawGuiLine(1, false);

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

                    Helpers.DrawGuiLine(1, false);

                    //Collider menu
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

                                bCopier_colliders_removeOld = GUILayout.Toggle(bCopier_colliders_removeOld, Strings.Copier.colliders_removeOld, Styles.CopierToggle);
                                bCopier_colliders_createObjects = GUILayout.Toggle(bCopier_colliders_createObjects, Strings.Copier.copyGameObjects, Styles.CopierToggle);
                            }
                        }

                        EditorGUILayout.Space();
                        EditorGUI.EndDisabledGroup();
                    }

                    Helpers.DrawGuiLine(1, false);

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

                    Helpers.DrawGuiLine(1, false);

                    EditorGUILayout.Space();

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

                    EditorGUILayout.Space();

                    EditorGUI.BeginDisabledGroup(
                        !(
                        bCopier_dynamicBones_copyColliders || bCopier_dynamicBones_copy || bCopier_colliders_copy || bCopier_particleSystems_copy ||
                        bCopier_descriptor_copy || bCopier_skinMeshRender_copy || bCopier_meshRenderers_copy || bCopier_lights_copy ||
                        bCopier_rigidBodies_copy || bCopier_trailRenderers_copy || bCopier_animators_copy
                        ));
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            if(GUILayout.Button(Strings.Buttons.selectNone, GUILayout.MinWidth(100)))
                            {
#if BONES || OLD_BONES
                                bCopier_dynamicBones_copy = false;
#endif
                                bCopier_colliders_copy = false;
                                bCopier_joints_copy = false;
                                bCopier_descriptor_copy = false;
                                bCopier_lights_copy = false;
                                bCopier_meshRenderers_copy = false;
                                bCopier_particleSystems_copy = false;
                                bCopier_rigidBodies_copy = false;
                                bCopier_skinMeshRender_copy = false;
                                bCopier_trailRenderers_copy = false;
                                bCopier_transforms_copy = false;
                                bCopier_animators_copy = false;
                                bCopier_audioSources_copy = false;
                            }
                            if(GUILayout.Button(Strings.Buttons.selectAll, GUILayout.MinWidth(100)))
                            {
#if BONES || OLD_BONES
                                bCopier_dynamicBones_copy = true;
#endif
                                bCopier_colliders_copy = true;
                                bCopier_joints_copy = true;
                                bCopier_descriptor_copy = true;
                                bCopier_lights_copy = true;
                                bCopier_meshRenderers_copy = true;
                                bCopier_particleSystems_copy = true;
                                bCopier_rigidBodies_copy = true;
                                bCopier_skinMeshRender_copy = true;
                                bCopier_trailRenderers_copy = true;
                                bCopier_transforms_copy = true;
                                bCopier_animators_copy = true;
                                bCopier_audioSources_copy = true;
                            }
                        }
                        EditorGUILayout.EndHorizontal();

                        EditorGUI.BeginDisabledGroup(!CopierHasSelections());
                        {
                            if(GUILayout.Button(Strings.Buttons.copySelected, Styles.BigButton))
                            {
                                string log = "";
                                if(copierSelectedFrom == null)
                                {
                                    log += Strings.Log.copyFromInvalid;
                                    Log(log, LogType.Warning);
                                }
                                else
                                {
                                    //Cancel Checks
                                    if(copierSelectedFrom == SelectedAvatar)
                                    {
                                        Log(log + Strings.Log.cantCopyToSelf, LogType.Warning);
                                        return;
                                    }

                                    //Figure out how to prevent undo from adding multiple copies of the same component on
                                    /*//Record Undo
                                    Undo.RegisterFullObjectHierarchyUndo(selectedAvatar, "Copy Components");
                                    if(selectedAvatar.gameObject.scene.name == null) //In case it's a prefab instance, which it probably is
                                        PrefabUtility.RecordPrefabInstancePropertyModifications(selectedAvatar);*/

                                    RefreshIgnoreArray();

                                    CopyComponents(copierSelectedFrom, SelectedAvatar);

                                    EditorUtility.SetDirty(SelectedAvatar);
                                    EditorSceneManager.MarkSceneDirty(SelectedAvatar.scene);

                                    avatarInfo = AvatarInfo.GetInfo(SelectedAvatar, out _avatarInfoString);

                                    log += Strings.Log.done;
                                    Log(log, LogType.Log);
                                }
                            }
                        }
                        EditorGUI.EndDisabledGroup();
                    }
                    EditorGUI.EndDisabledGroup();
                }
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.Space();
            }
        }        

        public void DrawMiscMenuGUI()
        {
            if(_misc_expand = GUILayout.Toggle(_misc_expand, Strings.Main.misc, Styles.Foldout_title))
            {
                if(PumkinsLanguageManager.Languages.Count == 0)
                    PumkinsLanguageManager.LoadTranslations();

                EditorGUILayout.Space();
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUI.BeginChangeCheck();
                    {
                        if(_selectedLanguageIndex >= PumkinsLanguageManager.Languages.Count)
                            _selectedLanguageIndex = PumkinsLanguageManager.GetIndexOfLanguage(_selectedLanguageString);

                        _selectedLanguageIndex = EditorGUILayout.Popup(Strings.Misc.language, _selectedLanguageIndex, PumkinsLanguageManager.Languages.Select(o => o.ToString()).ToArray(), Styles.Popup);
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

                Helpers.DrawGuiLine();

#if !BONES && !OLD_BONES
                if(GUILayout.Button(Strings.Misc.searchForBones, Styles.BigButton))
                {
                    _DependencyChecker.Check();
                }
                Helpers.DrawGuiLine();
#endif
                GUILayout.BeginHorizontal();
                {

                    if(GUILayout.Button(new GUIContent(Strings.Buttons.openGithubPage, Icons.GithubIcon)))
                    {
                        Application.OpenURL(Strings.Instance.LINK_GITHUB);
                    }
                    if(GUILayout.Button(new GUIContent(Strings.Buttons.openHelpPage, Icons.Help)))
                    {
                        Application.OpenURL(Strings.Instance.LINK_GITHUB + "wiki");
                    }
                }
                GUILayout.EndHorizontal();

                if(GUILayout.Button(new GUIContent(Strings.Buttons.joinDiscordServer, Icons.DiscordIcon)))
                {
                    Application.OpenURL(Strings.Instance.LINK_DISCORD);
                }
                if(GUILayout.Button(new GUIContent(Strings.Buttons.openDonationPage, Icons.KofiIcon)))
                {
                    Application.OpenURL(Strings.Instance.LINK_DONATION);
                }
            }
        }

        public void DrawThumbnailsMenuGUI()
        {
            if(_thumbnails_expand = GUILayout.Toggle(_thumbnails_expand, Strings.Main.thumbnails, Styles.Foldout_title))
            {
                Helpers.DrawGuiLine();

                _presetToolbarSelectedIndex = GUILayout.Toolbar(_presetToolbarSelectedIndex, new string[] { Strings.Thumbnails.cameras, Strings.Thumbnails.poses, Strings.Thumbnails.blendshapes }, Styles.ToolbarBigButtons);

                EditorGUILayout.Space();
                Helpers.DrawGuiLine();

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
                        EditorGUI.BeginDisabledGroup(true);
                        DrawPresetGUI<PumkinsBlendshapePreset>();
                        EditorGUI.EndDisabledGroup();                        
                        break;
                    default:
                        break;
                }                
            }
        }

        public void DrawThumbanailBlendshapeGUI()
        {
            EditorGUILayout.LabelField("Not working yet", Styles.HelpBox_OneLine);
            EditorGUILayout.Space();
        }

        public void DrawThumbnailPoseGUI()
        {
            if(GUILayout.Button(Strings.Buttons.openPoseEditor, Styles.BigButton))
            {
                PumkinsMuscleEditor.ShowWindow();
            }

            Helpers.DrawGuiLine();
        }

        public void DrawThumbnailCameraGUI()
        {
            EditorGUI.BeginChangeCheck();
            {
                shouldHideOtherAvatars = GUILayout.Toggle(shouldHideOtherAvatars, Strings.Thumbnails.hideOtherAvatars);
            }
            if(EditorGUI.EndChangeCheck())
            {
                HideAllOtherAvatars(shouldHideOtherAvatars, SelectedAvatar);
            }

            Helpers.DrawGuiLine();            

            SelectedCamera = EditorGUILayout.ObjectField(Strings.Thumbnails.selectedCamera, SelectedCamera, typeof(Camera), true) as Camera;

            Helpers.DrawGuiLine();

            DrawOverlayGUI();

            DrawBackgroundGUI();

            Helpers.DrawGuiLine();

            EditorGUI.BeginDisabledGroup(!SelectedCamera || !SelectedAvatar);
            {
                GUILayout.BeginHorizontal();
                {
                    if(GUILayout.Button(Strings.Thumbnails.centerCameraOnViewpoint, Styles.BigButton))
                    {
                        if(SelectedCamera)
                            CenterCameraOnViewpointNew(SelectedAvatar, centerCameraPositionOffset, centerCameraRotationOffset);
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

                    centerCameraPositionOffset = EditorGUILayout.Vector3Field(Strings.Thumbnails.positionOffset, centerCameraPositionOffset);
                    centerCameraRotationOffset = EditorGUILayout.Vector3Field(Strings.Thumbnails.rotationOffset, centerCameraRotationOffset);

                    EditorGUILayout.Space();

                    EditorGUILayout.BeginHorizontal();
                    {
                        if(GUILayout.Button(Strings.Buttons.setFromCamera))
                        {
                            SerialTransform st = PumkinsCameraPreset.GetCameraOffsetFromViewpoint(SelectedAvatar, SelectedCamera);
                            if(st)
                            {
                                centerCameraPositionOffset = Helpers.RoundVectorValues(st.position, 3);
                                centerCameraRotationOffset = Helpers.RoundVectorValues(st.localEulerAngles, 3);
                            }
                        }
                        if(GUILayout.Button(Strings.Buttons.reset, GUILayout.MaxWidth(90f)))
                        {
                            centerCameraPositionOffset = _defaultCameraPositionOffset;
                            centerCameraRotationOffset = _defaultCameraRotationOffset;
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }

                Helpers.DrawGuiLine();

                EditorGUILayout.Space();
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
                        avatarInfo = AvatarInfo.GetInfo(SelectedAvatar, out _avatarInfoString);
                    }
                }

                EditorGUILayout.SelectableLabel(_avatarInfoString, Styles.HelpBox, GUILayout.MinHeight(240));

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
                            avatarInfo = AvatarInfo.GetInfo(SelectedAvatar, out _avatarInfoString);
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
                    Helpers.DrawGuiLine();

                    GUILayout.BeginHorizontal();
                    {
                        if(GUILayout.Button(Strings.Buttons.quickSetupAvatar, Styles.BigButton))
                        {
                            //if(_tools_quickSetup_autoRig)
                            //    SetupRig(SelectedAvatar);
                            if(_tools_quickSetup_fillVisemes)
                                DoAction(SelectedAvatar, ToolMenuActions.FillVisemes);
                            if(_tools_quickSetup_setViewpoint)
                                QuickSetViewpoint(SelectedAvatar, _tools_quickSetup_viewpointZDepth);
                            if(_tools_quickSetup_forceTPose)
                                DoAction(SelectedAvatar, ToolMenuActions.SetTPose);
                            if(_tools_quickSetup_setRendererAnchor)
                                SetRendererAnchor(SelectedAvatar, _tools_quickSetup_setRenderAnchor_path);
                        }

                        if(GUILayout.Button(Icons.Settings, Styles.BigIconButton))
                            _tools_quickSetup_settings_expand = !_tools_quickSetup_settings_expand;
                    }
                    GUILayout.EndHorizontal();

                    if(_tools_quickSetup_settings_expand)
                    {
                        EditorGUILayout.Space();

                        GUILayout.BeginHorizontal();
                        {
                            float oldWidth = EditorGUIUtility.labelWidth;
                            EditorGUIUtility.labelWidth = Helpers.CalculateTextWidth(Strings.Tools.autoViewpoint);
                            _tools_quickSetup_setViewpoint = GUILayout.Toggle(_tools_quickSetup_setViewpoint, Strings.Tools.autoViewpoint);
                            
                            EditorGUIUtility.labelWidth = Helpers.CalculateTextWidth(Strings.Tools.viewpointZDepth);
                            EditorGUI.BeginDisabledGroup(!_tools_quickSetup_setViewpoint);
                            {
                                _tools_quickSetup_viewpointZDepth = EditorGUILayout.FloatField(Strings.Tools.viewpointZDepth, _tools_quickSetup_viewpointZDepth);
                            }
                            EditorGUI.EndDisabledGroup();
                            EditorGUIUtility.labelWidth = oldWidth;
                        }
                        GUILayout.EndHorizontal();

                        _tools_quickSetup_fillVisemes = GUILayout.Toggle(_tools_quickSetup_fillVisemes, Strings.Tools.fillVisemes);
                        _tools_quickSetup_forceTPose = GUILayout.Toggle(_tools_quickSetup_forceTPose, Strings.Tools.setTPose);
                        //_tools_quickSetup_autoRig = GUILayout.Toggle(_tools_quickSetup_autoRig, "_Setup Rig");

                        GUILayout.BeginHorizontal();
                        {
                            _tools_quickSetup_setRendererAnchor = GUILayout.Toggle(_tools_quickSetup_setRendererAnchor, Strings.Tools.setRendererAnchors);
                            EditorGUI.BeginDisabledGroup(!_tools_quickSetup_setRendererAnchor);
                            {
                                _tools_quickSetup_setRenderAnchor_path = EditorGUILayout.TextField(_tools_quickSetup_setRenderAnchor_path);
                            }
                            EditorGUI.EndDisabledGroup();
                        }
                        GUILayout.EndHorizontal();

                    }

                    Helpers.DrawGuiLine();


                    if(_tools_avatar_expand = GUILayout.Toggle(_tools_avatar_expand, Strings.Main.avatar, Styles.Foldout))
                    {
                        GUILayout.BeginHorizontal(); //Row
                        {
                            GUILayout.BeginVertical(GUILayout.ExpandWidth(true)); //Left Column
                            {
                                if(GUILayout.Button(Strings.Tools.fillVisemes))
                                    DoAction(SelectedAvatar, ToolMenuActions.FillVisemes);
                                if(GUILayout.Button(Strings.Tools.revertBlendshapes))
                                    DoAction(SelectedAvatar, ToolMenuActions.RevertBlendshapes);
                                if(GUILayout.Button(Strings.Tools.resetPose))
                                    DoAction(SelectedAvatar, ToolMenuActions.ResetPose);
                            }
                            GUILayout.EndVertical();

                            GUILayout.BeginVertical(GUILayout.ExpandWidth(true)); //Right Column
                            {
                                EditorGUI.BeginDisabledGroup(DrawingHandlesGUI);
                                {
                                    if(GUILayout.Button(Strings.Tools.editViewpoint))
                                        DoAction(SelectedAvatar, ToolMenuActions.EditViewpoint);
                                }
                                EditorGUI.EndDisabledGroup();

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
                    }

                    Helpers.DrawGuiLine();

                    if(_tools_removeAll_expand = GUILayout.Toggle(_tools_removeAll_expand, Strings.Main.removeAll, Styles.Foldout))
                    {

                        EditorGUILayout.BeginHorizontal();

                        EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true)); //Left Column
#if !BONES
                        EditorGUI.BeginDisabledGroup(true);
#endif
                        if(GUILayout.Button(new GUIContent(Strings.Copier.dynamicBones, Icons.BoneIcon)))
                            DoAction(SelectedAvatar, ToolMenuActions.RemoveDynamicBones);
#if !BONES
                        EditorGUI.EndDisabledGroup();
#endif
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

                        EditorGUILayout.EndVertical();

#if !BONES
                        EditorGUI.BeginDisabledGroup(true);
#endif
                        EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true)); //Right Column
                        if(GUILayout.Button(new GUIContent(Strings.Copier.dynamicBones_colliders, Icons.BoneColliderIcon)))
                            DoAction(SelectedAvatar, ToolMenuActions.RemoveDynamicBoneColliders);
#if !BONES
                        EditorGUI.EndDisabledGroup();
#endif
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

                        EditorGUILayout.EndVertical();

                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUI.EndDisabledGroup();
                    EditorGUILayout.Space();
                }
            }
        }        
       
        public void DrawPresetGUI<T>() where T : PumkinPreset
        {
            List<PumkinPreset> pr = new List<PumkinPreset>();
            string labelString = "Preset";            
            IEnumerable<string> dropdownOptions = new List<string>();

            SerializedProperty p_selectedPresetString = null;
            SerializedProperty p_selectedPresetIndex = null;            

            if(typeof(T) == typeof(PumkinsCameraPreset))
            {
                p_selectedPresetString = Instance.SerializedScript.FindProperty("_selectedCameraPresetString");
                p_selectedPresetIndex = Instance.SerializedScript.FindProperty("_selectedCameraPresetIndex");

                pr = PumkinsPresetManager.CameraPresets.Cast<PumkinPreset>().ToList();

                labelString = Strings.Thumbnails.cameras;
                dropdownOptions = PumkinsPresetManager.CameraPresets.Select(o => o.name);
            }
            else if(typeof(T) == typeof(PumkinsPosePreset))
            {
                p_selectedPresetString = Instance.SerializedScript.FindProperty("_selectedPosePresetString");
                p_selectedPresetIndex = Instance.SerializedScript.FindProperty("_selectedPosePresetIndex");

                pr = PumkinsPresetManager.PosePresets.Cast<PumkinPreset>().ToList();

                labelString = Strings.Thumbnails.poses;
                dropdownOptions = PumkinsPresetManager.PosePresets.Select(o => o.name);
            }
            else if(typeof(T) == typeof(PumkinsBlendshapePreset))
            {
                p_selectedPresetString = Instance.SerializedScript.FindProperty("_selectedBlendshapePresetString");
                p_selectedPresetIndex = Instance.SerializedScript.FindProperty("_selectedBlendshapePresetIndex");

                pr = PumkinsPresetManager.BlendshapePresets.Cast<PumkinPreset>().ToList();

                labelString = Strings.Thumbnails.blendshapes;
                dropdownOptions = PumkinsPresetManager.BlendshapePresets.Select(o => o.name);
            }

            if(p_selectedPresetIndex.intValue == -1)
                RefreshPresetIndex<T>();

            EditorGUI.BeginDisabledGroup(!SelectedAvatar || (pr.Count > 0 && p_selectedPresetIndex.intValue >= pr.Count && pr[p_selectedPresetIndex.intValue] == null));
            {
                GUILayout.BeginHorizontal();
                {
                    EditorGUI.BeginChangeCheck();
                    {
                        p_selectedPresetIndex.intValue = EditorGUILayout.Popup(labelString, p_selectedPresetIndex.intValue, dropdownOptions.ToArray(), Styles.Popup);
                    }
                    if(EditorGUI.EndChangeCheck())
                    {
                        p_selectedPresetString.stringValue = pr[p_selectedPresetIndex.intValue].ToString() ?? "";
                    }

                    if(GUILayout.Button(Icons.Refresh, Styles.IconButton))
                    {
                        PumkinsPresetManager.LoadPresets<T>();
                    }
                }
                GUILayout.EndHorizontal();

                EditorGUILayout.Space();

                GUILayout.BeginHorizontal();
                {
                    EditorGUI.BeginDisabledGroup(pr.Count == 0);
                    {
                        if(GUILayout.Button(Strings.Buttons.edit))
                        {
                            p_selectedPresetIndex.intValue = PumkinsPresetManager.GetPresetIndex<T>(p_selectedPresetString.stringValue);
                            pr[p_selectedPresetIndex.intValue].ApplyPreset(SelectedAvatar);

                            if(typeof(T) == typeof(PumkinsCameraPreset))
                                CreateCamerePresetPopup.ShowWindow(pr[p_selectedPresetIndex.intValue] as PumkinsCameraPreset);
                            else if(typeof(T) == typeof(PumkinsPosePreset))
                                CreatePosePresetPopup.ShowWindow(pr[p_selectedPresetIndex.intValue] as PumkinsPosePreset);
                            else if(typeof(T) == typeof(PumkinsBlendshapePreset))
                                CreateBlendshapePopup.ShowWindow(pr[p_selectedPresetIndex.intValue] as PumkinsBlendshapePreset);
                        }                        
                        if(GUILayout.Button(Strings.Buttons.load))
                        {
                            p_selectedPresetIndex.intValue = PumkinsPresetManager.GetPresetIndex<T>(p_selectedPresetString.stringValue);
                            pr[p_selectedPresetIndex.intValue].ApplyPreset(SelectedAvatar);
                        }
                    }
                    EditorGUI.EndDisabledGroup();
                }
                GUILayout.EndHorizontal();                
            }
            EditorGUI.EndDisabledGroup();            
            Helpers.DrawGuiLine();

            if(GUILayout.Button(Strings.Buttons.createNewPreset, Styles.BigButton))
            {
                if(typeof(T) == typeof(PumkinsCameraPreset))
                    CreateCamerePresetPopup.ShowWindow();
                else if(typeof(T) == typeof(PumkinsPosePreset))
                    CreatePosePresetPopup.ShowWindow();
                else if(typeof(T) == typeof(PumkinsBlendshapePreset))
                    CreateBlendshapePopup.ShowWindow();
            }            

            if(GUILayout.Button(Strings.Buttons.reset))
            {
                if(typeof(T) == typeof(PumkinsCameraPreset))
                    CenterCameraOnViewpointNew(SelectedAvatar, _defaultCameraPositionOffset, _defaultCameraRotationOffset);
                else if(typeof(T) == typeof(PumkinsPosePreset))
                    DoAction(SelectedAvatar, ToolMenuActions.ResetPose);
                else if(typeof(T) == typeof(PumkinsBlendshapePreset))
                    DoAction(SelectedAvatar, ToolMenuActions.RevertBlendshapes);
            }

            Helpers.DrawGuiLine();

            SerializedScript.ApplyModifiedProperties();
        }

        //Draws the "Use Background" part of the thumbnail menu
        public void DrawBackgroundGUI(bool needsRefresh = false)
        {
            EditorGUI.BeginChangeCheck();
            {
                Helpers.DrawDropdownWithToggle(ref _thumbnails_useCameraBackground_expand, ref bThumbnails_use_camera_background, Strings.Thumbnails.useCameraBackground);                
            }
            if(EditorGUI.EndChangeCheck())
            {
                needsRefresh = true;                
            }

            if(_thumbnails_useCameraBackground_expand || needsRefresh)
            {
                EditorGUI.BeginDisabledGroup(!bThumbnails_use_camera_background);
                {
                    EditorGUILayout.Space();

                    EditorGUI.BeginChangeCheck();
                    {
                        cameraBackgroundType = (CameraBackgroundOverrideType)EditorGUILayout.EnumPopup(Strings.Thumbnails.backgroundType, cameraBackgroundType);
                    }
                    if(EditorGUI.EndChangeCheck() || needsRefresh)
                    {
                        if(!bThumbnails_use_camera_background)
                        {
                            ResetBackground();
                        }
                        else
                        {
                            if(SelectedCamera)
                            {
                                switch(cameraBackgroundType)
                                {
                                    case CameraBackgroundOverrideType.Color:
                                        SetBackgroundToColor();
                                        break;
                                    case CameraBackgroundOverrideType.Image:
                                        SetBackgroundTexture(true, _backgroundPathText);
                                        break;
                                    case CameraBackgroundOverrideType.Material:
                                        SetBackgroundToMaterial();
                                        break;
                                    default:
                                        break;
                                }
                            }
                            else
                            {
                                Log(Strings.Warning.cameraNotFound, LogType.Warning);
                            }
                        }
                    }

                    EditorGUILayout.Space();

                    if(SelectedCamera)
                    {
                        if(vrcCamSetBgColor)
                        {
                            EditorGUI.BeginChangeCheck();
                            {
                                vrcCamBgColor = EditorGUILayout.ColorField(Strings.Thumbnails.backgroundType_Color, vrcCamBgColor);
                            }
                            if(EditorGUI.EndChangeCheck() || needsRefresh)
                            {
                                if(SelectedCamera)
                                    SelectedCamera.backgroundColor = vrcCamBgColor;
                            }
                            GUILayout.Space(34f);
                        }
                        else if(vrcCamSetBGSkybox)
                        {
                            EditorGUI.BeginChangeCheck();
                            {
                                RenderSettings.skybox = (Material)EditorGUILayout.ObjectField(Strings.Thumbnails.backgroundType_Material, RenderSettings.skybox, typeof(Material), false);
                            }
                            if(EditorGUI.EndChangeCheck() || needsRefresh)
                            {
                                if(SelectedCamera)
                                    SelectedCamera.backgroundColor = vrcCamBgColor;
                            }
                            GUILayout.Space(34f);
                        }
                        else if(vrcCamSetBGImage)
                        {
                            GUILayout.BeginHorizontal();

                            EditorGUILayout.LabelField(Strings.Thumbnails.backgroundType_Image, GUILayout.MaxWidth(100));
                            EditorGUILayout.SelectableLabel(_backgroundPathText, Styles.HelpBox, GUILayout.MaxHeight(18), GUILayout.ExpandHeight(false));

                            EditorGUI.BeginChangeCheck();
                            {
                                if(GUILayout.Button(Strings.Buttons.browse, GUILayout.MaxWidth(60)))
                                {
                                    string s = _lastOpenFilePath;
                                    Texture2D tex = OpenImageGetTextureGUI(ref s);

                                    if(tex)
                                    {
                                        cameraBackgroundTexture = tex;
                                        _lastOpenFilePath = s;
                                        _backgroundPathText = _lastOpenFilePath;
                                    }
                                }
                                else if(GUILayout.Button("X", GUILayout.MaxWidth(25)))
                                {
                                    if(CameraBackgroundRawImage)
                                    {
                                        CameraBackgroundRawImage.enabled = false;

                                        if(cameraBackgroundTexture)
                                            cameraBackgroundTexture.name = _emptyTexture.name;
                                    }
                                    _backgroundPathText = null;
                                }
                                GUILayout.EndHorizontal();
                            }
                            if(EditorGUI.EndChangeCheck() || needsRefresh)
                            {
                                if(cameraBackgroundTexture != null)
                                {
                                    if(cameraBackgroundTexture.name != EmptyTexture.name)
                                    {
                                        if(SelectedCamera)
                                        {
                                            CameraBackgroundRawImage.texture = cameraBackgroundTexture;
                                            cameraBackgroundTexture.name = cameraBackgroundTexture.name;
                                            CameraBackgroundRawImage.color = cameraBackgroundImageTint;
                                            CameraBackgroundRawImage.enabled = true;
                                        }
                                    }
                                    else
                                    {
                                        if(CameraBackground != null)
                                            DestroyImmediate(CameraBackground);
                                    }
                                }
                                else
                                {
                                    CameraBackgroundRawImage.enabled = false;
                                }
                            }

                            EditorGUILayout.Space();

                            if(cameraBackgroundTexture && vrcCamSetBGImage)
                            {
                                CameraBackgroundRawImage.enabled = true;
                                EditorGUI.BeginChangeCheck();
                                {
                                    cameraBackgroundImageTint = EditorGUILayout.ColorField(Strings.Thumbnails.tint, cameraBackgroundImageTint);
                                }
                                if(EditorGUI.EndChangeCheck() || needsRefresh)
                                {
                                    CameraBackgroundRawImage.color = cameraBackgroundImageTint;
                                }
                            }
                            EditorGUILayout.Space();
                        }
                    }
                }
                EditorGUI.EndDisabledGroup();
                needsRefresh = false;
            }
        }

        //Draws the "Use Overlay" section in the thumbnails menu
        public void DrawOverlayGUI()
        {
            EditorGUI.BeginChangeCheck();
            {
                Helpers.DrawDropdownWithToggle(ref _thumbnails_useCameraOverlay_expand, ref bThumbnails_use_camera_overlay, Strings.Thumbnails.useCameraOverlay);
            }
            if(EditorGUI.EndChangeCheck())
            {
                SetOverlayTexture(bThumbnails_use_camera_overlay, _overlayPathText);
            }

            if(_thumbnails_useCameraOverlay_expand)
            {
                EditorGUI.BeginDisabledGroup(!bThumbnails_use_camera_overlay);
                {
                    EditorGUILayout.Space();
                    GUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField(Strings.Thumbnails.overlayCameraImage, GUILayout.MaxWidth(100));
                        EditorGUILayout.SelectableLabel(_overlayPathText, Styles.HelpBox_OneLine);

                        EditorGUI.BeginChangeCheck();
                        {
                            if(GUILayout.Button(Strings.Buttons.browse, GUILayout.MaxWidth(60)) && SelectedCamera)
                            {
                                Texture2D tex = Helpers.OpenImageTexture(ref _lastOpenFilePath);
                                if(tex)
                                {
                                    cameraOverlayTexture = tex;
                                    cameraOverlayTexture.name = Path.GetFileNameWithoutExtension(_lastOpenFilePath);
                                    _overlayPathText = _lastOpenFilePath;
                                }
                            }
                            if(GUILayout.Button("X", GUILayout.MaxWidth(25)))
                            {
                                if(CameraOverlayRawImage)
                                {
                                    if(cameraOverlayTexture)
                                        cameraOverlayTexture.name = _emptyTexture.name;
                                }
                                _overlayPathText = null;
                                SetOverlayTexture(bThumbnails_use_camera_overlay);
                            }
                        }
                    }
                    GUILayout.EndHorizontal();
                    if(EditorGUI.EndChangeCheck())
                    {
                        if(cameraOverlayTexture != null)
                        {
                            if(cameraOverlayTexture.name != _emptyTexture.name)
                            {
                                if(SelectedCamera)
                                {
                                    CameraOverlayRawImage.texture = cameraOverlayTexture;
                                    CameraOverlayRawImage.color = cameraOverlayImageTint;
                                }
                            }
                            else
                            {
                                if(CameraOverlay != null)
                                    DestroyImmediate(CameraOverlay);
                            }
                        }
                        else
                        {
                            CameraOverlayRawImage.color = Color.clear;
                        }
                    }

                    EditorGUI.BeginDisabledGroup(!cameraOverlayTexture);
                    {
                        EditorGUI.BeginChangeCheck();
                        {
                            cameraOverlayImageTint = EditorGUILayout.ColorField(Strings.Thumbnails.tint, cameraOverlayImageTint);
                        }
                        if(EditorGUI.EndChangeCheck())
                        {
                            CameraOverlayRawImage.color = cameraOverlayImageTint;
                        }
                    }
                    EditorGUI.EndDisabledGroup();
                }
                EditorGUI.EndDisabledGroup();
            }

            Helpers.DrawGuiLine();
        }        

        public void SelectThumbnailPresetToolbarOption(PresetToolbarOptions option)
        {
            _presetToolbarSelectedIndex = (int)option;
            _mainScroll = new Vector2(0, 1000);
        }

        #endregion

        #region Main Functions

        private void RestoreTexturesFromPaths()
        {
            if(!string.IsNullOrEmpty(_overlayPathText) && cameraOverlayTexture == null)
            {
                if(CameraOverlay)
                {
                    RawImage img = CameraOverlay.GetComponent<RawImage>();
                    if(img)
                        cameraOverlayTexture = (Texture2D)img.texture;
                    else
                        _overlayPathText = "";
                }
            }

            if(!string.IsNullOrEmpty(_backgroundPathText) && cameraBackgroundTexture == null)
            {
                if(CameraBackground)
                {
                    RawImage img = CameraBackground.GetComponent<RawImage>();
                    if(img)
                        cameraBackgroundTexture = (Texture2D)img.texture;
                    else
                        _backgroundPathText = "";
                }
            }
        }

        public void SetBackgroundToMaterial(Material mat = null)
        {
            ResetBackground();

            if(mat)
                RenderSettings.skybox = mat;

            vrcCamSetBGSkybox = true;
            SelectedCamera.clearFlags = CameraClearFlags.Skybox;
        }

        public void SetBackgroundToColor()
        {
            ResetBackground();

            vrcCamSetBgColor = true;
            SelectedCamera.clearFlags = CameraClearFlags.SolidColor;
            SelectedCamera.backgroundColor = vrcCamBgColor;
        }

        public void ResetBackground()
        {
            vrcCamSetBgColor = false;
            vrcCamSetBGSkybox = false;
            vrcCamSetBGImage = false;

            if(SelectedCamera)
            {
                SelectedCamera.clearFlags = _vrcCamClearFlagsOld;
                SelectedCamera.backgroundColor = _vrcCamColorOld;
            }
            
            if(CameraBackgroundRawImage && !bThumbnails_use_camera_background)
                CameraBackgroundRawImage.enabled = false;
        }

        public void SetBackgroundImage(Texture2D texOverride = null, string pathOverride = null)
        {
            ResetBackground();
            vrcCamSetBGImage = true;
            if(CameraBackground && CameraBackgroundRawImage)
            {
                CameraBackgroundRawImage.enabled = true;

                if(texOverride && !string.IsNullOrEmpty(pathOverride))
                    SetBackgroundTexture(vrcCamSetBGImage, texOverride, pathOverride);
                else if(!string.IsNullOrEmpty(pathOverride))
                    SetBackgroundTexture(vrcCamSetBGImage, pathOverride);
                else if(!string.IsNullOrEmpty(_backgroundPathText))
                    SetBackgroundTexture(vrcCamSetBGImage, _backgroundPathText);
                else
                    CameraBackgroundRawImage.enabled = false;
            }
        }

        private Texture2D OpenImageGetTextureGUI(ref string path)
        {
            Texture2D texture = Helpers.OpenImageTexture(ref path);
            if(texture)
                texture.name = Path.GetFileNameWithoutExtension(path);

            return texture;
        }

        public void SetOverlayTexture(bool enabled, Texture2D newTexture = null)
        {
            if(CameraOverlay && CameraOverlayRawImage)
            {
                CameraOverlayRawImage.enabled = enabled;
                if(newTexture)
                {
                    CameraOverlayRawImage.color = cameraOverlayImageTint;
                    CameraOverlayRawImage.texture = newTexture;
                }
            }
        }

        public void SetOverlayTexture(bool enabled, string texturePath)
        {
            if(SelectedCamera)
            {
                Texture2D tex = Helpers.GetImageTextureFromPath(texturePath);
                if(tex)
                {
                    _overlayPathText = texturePath;
                    SetOverlayTexture(enabled, tex);
                }
            }
        }

        public void SetBackgroundTexture(bool enabled, string texturePath)
        {
            ResetBackground();
            vrcCamSetBGImage = true;

            Texture2D tex = Helpers.GetImageTextureFromPath(texturePath);
            if(tex)
            {                
                SetBackgroundTexture(enabled, tex, texturePath);                
            }
            else
            {                        
                _backgroundPathText = "";
            }
        }

        public void SetBackgroundTexture(bool enabled, Texture2D newTexture = null, string texPath = null)
        {
            ResetBackground();
            vrcCamSetBGImage = true;

            if(CameraBackground && CameraBackgroundRawImage)
            {
                CameraBackgroundRawImage.enabled = enabled;

                if(newTexture && !string.IsNullOrEmpty(texPath))
                {
                    CameraBackgroundRawImage.texture = newTexture;
                    cameraBackgroundTexture = newTexture;
                    _backgroundPathText = texPath;
                }
                else
                {                    
                    CameraBackgroundRawImage.enabled = false;
                    cameraBackgroundTexture = null;
                }

            }
        }

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

        /// <summary>
        /// Quickly sets viewpoint to eye height if avatar is humanoid
        /// </summary>        
        /// <param name="zDepth">Z Depth value of viewpoint</param>
        public void QuickSetViewpoint(GameObject avatar, float zDepth)
        {
            VRC_AvatarDescriptor desc = avatar.GetComponent<VRC_AvatarDescriptor>() ?? avatar.AddComponent<VRC_AvatarDescriptor>();
            var anim = SelectedAvatar.GetComponent<Animator>();

            if(anim && anim.isHuman)
            {
                Vector3 pos = anim.GetBoneTransform(HumanBodyBones.Head).position;
                float eyeHeight = anim.GetBoneTransform(HumanBodyBones.LeftEye).position.y - 0.005f;
                pos.y = eyeHeight;
                pos.z = zDepth;

                desc.ViewPosition = Helpers.RoundVectorValues(pos, 3);
                Log(Strings.Log.settingQuickViewpoint, LogType.Log, desc.ViewPosition.ToString());
            }
            else
            {
                Log(Strings.Log.cantSetViewpointNonHumanoid, LogType.Log, desc.ViewPosition.ToString());
            }
        }

        /// <summary>
        /// Tries to get the VRCCam, returns Camera.main if not found
        /// </summary>        
        private static Camera GetVRCCamOrMainCam()
        {
            var obj = GameObject.Find("VRCCam") ?? Camera.main.gameObject;
            if(obj)
                return obj.GetComponent<Camera>();            
            return null;
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
                        avatarInfo = AvatarInfo.GetInfo(SelectedAvatar, out _avatarInfoString);

                    }
                    else if(!_useSceneSelectionAvatar)
                    {
                        Log(Strings.Warning.selectSceneObject, LogType.Warning);
                    }
                }
            }
            catch(Exception e)
            {
                Debug.Log(e.Message);
            }
            _PumkinsAvatarToolsWindow.RequestRepaint(_PumkinsAvatarToolsWindow.ToolsWindow);
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

                var tt = Helpers.FindTransformInAnotherHierarchy(t, copierSelectedFrom.transform, false);
                if(tt && !newList.Contains(tt))
                    newList.Add(tt);
            }

            _copierIgnoreArray = newList.ToArray();
        }

        /// <summary>
        /// Returns true if we have any components selected in the copier
        /// </summary>
        /// <returns></returns>
        bool CopierHasSelections()
        {
            if(!(bCopier_animators_copy || bCopier_colliders_copy || bCopier_joints_copy || bCopier_descriptor_copy ||
              bCopier_lights_copy || bCopier_meshRenderers_copy || bCopier_particleSystems_copy || bCopier_rigidBodies_copy ||
              bCopier_trailRenderers_copy || bCopier_transforms_copy || bCopier_skinMeshRender_copy || bCopier_dynamicBones_copy ||
              bCopier_audioSources_copy))
                return false;
            return true;
        }

        /// <summary>
        /// Sets the avatar scale and moves the viewpoint to compensate
        /// </summary>        
        private void SetAvatarScale(VRC_AvatarDescriptor desc, float newScale)
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
                var tempDummy = new GameObject("_tempDummy").transform;
                tempDummy.position = desc.ViewPosition + desc.transform.root.position;
                tempDummy.parent = SelectedAvatar.transform;
                desc.transform.root.localScale = Helpers.RoundVectorValues(new Vector3(newScale, newScale, newScale), 3);
                SetViewpoint(desc, tempDummy.position);
                DestroyImmediate(tempDummy.gameObject);
                Log(Strings.Log.setAvatarScaleTo, LogType.Log, newScale.ToString(), desc.ViewPosition.ToString());
            }
        }

        /// <summary>
        /// Function for all the actions in the tool menu. Use this instead of calling
        /// button functions directly.
        /// </summary>        
        void DoAction(GameObject avatar, ToolMenuActions action)
        {
            if(SelectedAvatar == null)
            {
                //Shouldn't be possible with disable group
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
#if BONES || OLD_BONES
                    DestroyAllComponentsOfType(selectedAvatar, typeof(DynamicBoneCollider), false, false);
#endif
                    break;
                case ToolMenuActions.RemoveDynamicBones:
#if BONES || OLD_BONES
                    DestroyAllComponentsOfType(selectedAvatar, typeof(DynamicBone), false, false);
#endif
                    break;
                case ToolMenuActions.ResetPose:
                    ResetPose(SelectedAvatar);
                    break;
                case ToolMenuActions.RevertBlendshapes:
                    ResetBlendShapes(SelectedAvatar, true);
                    break;
                case ToolMenuActions.FillVisemes:
                    FillVisemes(SelectedAvatar);
                    break;
                case ToolMenuActions.EditViewpoint:
                    BeginEdittingViewpoint(SelectedAvatar);
                    break;
                case ToolMenuActions.ZeroBlendshapes:
                    ResetBlendShapes(SelectedAvatar, false);
                    break;
                case ToolMenuActions.SetTPose:
                    //PumkinsPoseEditor.SetTPose(selectedAvatar);
                    PumkinsPoseEditor.SetDefaultPoseByName(SelectedAvatar, "TPose");
                    break;
                case ToolMenuActions.RemoveEmptyGameObjects:
                    DestroyEmptyGameObjects(SelectedAvatar);
                    break;
                case ToolMenuActions.RemoveParticleSystems:
                    DestroyAllComponentsOfType(SelectedAvatar, typeof(ParticleSystem), false, false);
                    DestroyAllComponentsOfType(SelectedAvatar, typeof(ParticleSystemRenderer), false, false);
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
                    DestroyAllComponentsOfType(SelectedAvatar, typeof(AudioSource), false, false);
                    DestroyAllComponentsOfType(SelectedAvatar, typeof(ONSPAudioSource), false, false);
                    break;
                case ToolMenuActions.RemoveJoints:
                    DestroyAllComponentsOfType(SelectedAvatar, typeof(Joint), false, false);
                    break;
                case ToolMenuActions.EditScale:
                    BeginScalingAvatar(SelectedAvatar);
                    break;
                default:
                    break;
            }

            avatarInfo = AvatarInfo.GetInfo(SelectedAvatar, out _avatarInfoString);

            EditorUtility.SetDirty(SelectedAvatar);
            if(!EditorApplication.isPlaying)
                EditorSceneManager.MarkSceneDirty(SelectedAvatar.scene);
        }       

        /// <summary>
        /// Begin scaling Avatar. 
        /// Used to uniformily scale an avatar as well as it's viewpoint position
        /// </summary>        
        private void BeginScalingAvatar(GameObject avatar)
        {
            if(DrawingHandlesGUI || !avatar)
                return;

            _tempAvatarDescriptor = avatar.GetComponent<VRC_AvatarDescriptor>() ?? avatar.AddComponent<VRC_AvatarDescriptor>();

            _avatarScaleOld = avatar.transform.localScale;
            _avatarScaleTemp = _avatarScaleOld.z;
            _viewPosOld = _tempAvatarDescriptor.ViewPosition;
            _viewPosTemp = _viewPosOld + SelectedAvatar.transform.position;

            if(!_scaleViewpointDummy)
            {
                var g = GameObject.Find("_PumkinsViewpointDummy");
                if(g)
                    _scaleViewpointDummy = g.transform;
                else
                    _scaleViewpointDummy = new GameObject("_PumkinsViewpointDummy").transform;
            }

            _scaleViewpointDummy.position = _viewPosTemp;
            _scaleViewpointDummy.parent = SelectedAvatar.transform;                 

            _editingScale = true;
            _tempToolOld = Tools.current;
            Tools.current = Tool.None;
            Selection.activeGameObject = SelectedAvatar;
        }

        /// <summary>
        /// Ends scaling the avatar
        /// </summary>        
        /// <param name="cancelled">If canceled returnt to old scale and viewpoint</param>
        private void EndScalingAvatar(GameObject avatar, bool cancelled)
        {
            if(avatar == null)
            {
                _editingScale = false;
            }
            else
            {
                if(_tempAvatarDescriptor == null)
                {
                    Log(Strings.Log.descriptorIsNull, LogType.Error);
                    return;
                }

                _editingScale = false;
                Tools.current = _tempToolOld;
                if(!cancelled)
                {
                    SetViewpoint(_tempAvatarDescriptor, _viewPosTemp);
                    Log(Strings.Log.setAvatarScaleAndViewpointTo, LogType.Log, avatar.transform.localScale.z.ToString(), _tempAvatarDescriptor.ViewPosition.ToString());
                }
                else
                {
                    _tempAvatarDescriptor.ViewPosition = _viewPosOld;
                    SelectedAvatar.transform.localScale = _avatarScaleOld;
                    Log(Strings.Log.canceledScaleChanges);
                }
            }
            _tempAvatarDescriptor = null;

            if(_scaleViewpointDummy)
                DestroyImmediate(_scaleViewpointDummy.gameObject);
        }

        /// <summary>
        /// Begin Editing Viewposition. 
        /// Used to move the viewpoint using unit's transform gizmo
        /// </summary>        
        private void BeginEdittingViewpoint(GameObject avatar)
        {
            if(_editingView || _editingScale || !avatar)
                return;

            _tempAvatarDescriptor = avatar.GetComponent<VRC_AvatarDescriptor>() ?? avatar.AddComponent<VRC_AvatarDescriptor>();
            
            _viewPosOld = _tempAvatarDescriptor.ViewPosition;

            if(_tempAvatarDescriptor.ViewPosition == DEFAULT_VIEWPOINT)
            {
                var anim = SelectedAvatar.GetComponent<Animator>();

                if(anim != null && anim.isHuman)
                {
                    _viewPosTemp = anim.GetBoneTransform(HumanBodyBones.Head).position;
                    float eyeHeight = anim.GetBoneTransform(HumanBodyBones.LeftEye).position.y - 0.005f;
                    _viewPosTemp.y = eyeHeight;
                    _viewPosTemp.z = DEFAULT_VIEWPOINT.z - 0.1f;
                }
            }
            else
            {
                _viewPosTemp = _tempAvatarDescriptor.ViewPosition + avatar.transform.root.position;
            }
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
                    _tempAvatarDescriptor.ViewPosition = _viewPosOld;
                    Log(Strings.Log.viewpointCancelled, LogType.Log);
                }
            }
            _tempAvatarDescriptor = null;
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

            var renders = avatar.GetComponentsInChildren<SkinnedMeshRenderer>();

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

        /// <summary>
        /// Sets the Probe Anchor of all Skinned Mesh Renderers to transform by path
        /// </summary>        
        private void SetRendererAnchor(GameObject selectedAvatar, string anchorPath)
        {
            Transform anchor = selectedAvatar.transform.Find(anchorPath);
            if(!anchor)
            {
                Log(Strings.Log.noSkinnedMeshFound);
                return;
            }

            var renders = selectedAvatar.GetComponentsInChildren<SkinnedMeshRenderer>();
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

            VRC_AvatarDescriptor desc;

            //Run statment only if root so only run this once
            if(objTo != null && objTo.transform == objTo.transform.root)
            {
                if(bCopier_descriptor_copy)
                {
                    CopyAvatarDescriptor(objFrom, objTo, true);
                }
                if(bCopier_transforms_copy && bCopier_transforms_copyAvatarScale)
                {
                    desc = objTo.GetComponentInChildren<VRC_AvatarDescriptor>();
                    if(desc)
                    {
                        if(!(bCopier_descriptor_copy && bCopier_descriptor_copyViewpoint))
                            SetAvatarScale(desc, objFrom.transform.localScale.z);
                        objTo.transform.localScale = new Vector3(objFrom.transform.localScale.x, objFrom.transform.localScale.y, objTo.transform.localScale.z);
                    }
                    else
                        objTo.transform.localScale = objFrom.transform.localScale;
                }                
                if(bCopier_particleSystems_copy)
                {
                    CopyAllParticleSystems(objFrom, objTo, bCopier_particleSystems_createObjects, true);
                }
                if(bCopier_colliders_copy)
                {
                    if(bCopier_colliders_removeOld)
                        DestroyAllComponentsOfType(objTo, typeof(Collider), false, true);
                    CopyAllColliders(objFrom, objTo, bCopier_colliders_createObjects, true);
                }
                if(bCopier_rigidBodies_copy)
                {
                    CopyAllRigidBodies(objFrom, objTo, bCopier_rigidBodies_createObjects, true);
                }
                if(bCopier_trailRenderers_copy)
                {
                    CopyAllTrailRenderers(objFrom, objTo, bCopier_trailRenderers_createObjects, true);
                }
                if(bCopier_meshRenderers_copy)
                {
                    CopyAllMeshRenderers(objFrom, objTo, bCopier_meshRenderers_createObjects, true);
                }
                if(bCopier_lights_copy)
                {
                    CopyAllLights(objFrom, objTo, bCopier_lights_createObjects, true);
                }
                if(bCopier_skinMeshRender_copy)
                {
                    CopyAllSkinnedMeshRenderersSettings(objFrom, objTo, true);
                }
                if(bCopier_animators_copy)
                {
                    CopyAllAnimators(objFrom, objTo, bCopier_animators_createObjects, bCopier_animators_copyMainAnimator, true);
                }
                if(bCopier_audioSources_copy)
                {
                    CopyAllAudioSources(objFrom, objTo, bCopier_audioSources_createObjects, true);
                }
#if BONES || OLD_BONES
                if(bCopier_dynamicBones_copy)
                {
                    if(bCopier_dynamicBones_removeOldColliders)
                        DestroyAllComponentsOfType(selectedAvatar, typeof(DynamicBoneCollider), false, true);
                    if(bCopier_dynamicBones_copyColliders)
                        CopyAllDynamicBoneColliders(objFrom, objTo, bCopier_dynamicBones_createObjectsColliders, true);

                    if(bCopier_dynamicBones_removeOldBones)
                        DestroyAllComponentsOfType(selectedAvatar, typeof(DynamicBone), false, true);
                    if(bCopier_dynamicBones_copySettings || bCopier_dynamicBones_createMissing)
                        CopyAllDynamicBonesNew(objFrom, objTo, bCopier_dynamicBones_createMissing, true);
                }
#endif
            }
            //End run once

            if(bCopier_transforms_copy && (bCopier_transforms_copyPosition || bCopier_transforms_copyRotation || bCopier_transforms_copyScale))
            {
                CopyTransforms(objFrom, objTo, true);
            }

            //Copy Components in Children. Not a good way of doing it, will eventually replace this.
            for(int i = 0; i < objFrom.transform.childCount; i++)
            {
                var fromChild = objFrom.transform.GetChild(i).gameObject;
                //var t = objTo.transform.Find(fromChild.name);                

                if(fromChild == null)
                    continue;

                var t = GetSameChild(objTo, fromChild);

                GameObject toChild = null;

                if(t != null)
                    toChild = t.gameObject;

                CopyComponents(fromChild, toChild);
            }
        }

        /// <summary>
        /// Copies all audio sources on object and it's children.
        /// </summary>            
        /// <param name="createGameObjects">Whether to create missing objects</param>            
        void CopyAllAudioSources(GameObject from, GameObject to, bool createGameObjects, bool useignoreList)
        {
            if(from == null || to == null)
                return;

            var aFromArr = from.GetComponentsInChildren<AudioSource>();

            for(int i = 0; i < aFromArr.Length; i++)
            {
                var aFrom = aFromArr[i];
                var oFrom = aFromArr[i].GetComponent<ONSPAudioSource>();

                var tTo = Helpers.FindTransformInAnotherHierarchy(aFrom.transform, to.transform, createGameObjects);

                if((!tTo) || (useignoreList && Helpers.ShouldIgnoreObject(aFrom.transform, _copierIgnoreArray, bCopier_ignoreArray_includeChildren)))
                    continue;

                var aToObj = tTo.gameObject;

                string log = String.Format(Strings.Log.copyAttempt, aFromArr[i].GetType().ToString(), aFrom.gameObject, tTo.gameObject);

                if(aFrom != null)
                {
                    var lTo = aToObj.GetComponent<AudioSource>();
                    var oTo = aToObj.GetComponent<ONSPAudioSource>();

                    if(lTo == null && bCopier_audioSources_createMissing)
                    {
                        lTo = aToObj.AddComponent<AudioSource>();
                        if(oFrom != null)
                            oTo = aToObj.AddComponent<ONSPAudioSource>();
                    }

                    if((lTo != null && bCopier_audioSources_copySettings) || bCopier_audioSources_createMissing)
                    {
                        ComponentUtility.CopyComponent(aFrom);
                        ComponentUtility.PasteComponentValues(lTo);

                        if(oFrom != null)
                        {
                            ComponentUtility.CopyComponent(oFrom);
                            ComponentUtility.PasteComponentValues(oTo);
                        }
                        Log(log + ": " + Strings.Log.success);
                    }
                }
                else
                {
                    Log(log + " " + Strings.Log.failedDoesntHave, LogType.Warning, aFrom.gameObject.name.ToString(), aFrom.GetType().ToString());
                }
            }
        }

        /// <summary>
        /// Copies all Animators from one object and it's children to another.
        /// </summary>
        /// <param name="createGameObjects">Whether to create missing GameObjects</param>
        /// <param name="copyRootAnimator">Whether to copy the Animator on the root object. You don't usually want to.</param>
        void CopyAllAnimators(GameObject from, GameObject to, bool createGameObjects, bool copyRootAnimator, bool useignoreList)
        {
            if(from == null || to == null)
                return;

            var aFromArr = from.GetComponentsInChildren<Animator>();

            for(int i = 0; i < aFromArr.Length; i++)
            {
                if(!copyRootAnimator && aFromArr[i].transform.parent == null)
                    continue;

                string log = Strings.Log.copyAttempt;

                var aFrom = aFromArr[i];
                var tTo = Helpers.FindTransformInAnotherHierarchy(aFrom.transform, to.transform, createGameObjects);

                if((!tTo) || (useignoreList && Helpers.ShouldIgnoreObject(aFrom.transform, _copierIgnoreArray, bCopier_ignoreArray_includeChildren)))
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
                        Log(log + ": " + Strings.Log.success, LogType.Log, lTo.GetType().ToString(), tTo.gameObject.name, aFrom.gameObject.name);
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
        void CopyAllLights(GameObject from, GameObject to, bool createGameObjects, bool useignoreList)
        {
            if(from == null || to == null)
                return;

            var lFromArr = from.GetComponentsInChildren<Light>();

            for(int i = 0; i < lFromArr.Length; i++)
            {
                string log = Strings.Log.copyAttempt;

                var lFrom = lFromArr[i];
                var tTo = Helpers.FindTransformInAnotherHierarchy(lFrom.transform, to.transform, createGameObjects);

                if((!tTo) ||
                    (useignoreList && Helpers.ShouldIgnoreObject(lFrom.transform, _copierIgnoreArray, bCopier_ignoreArray_includeChildren)))
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
                        Log(log + ": " + Strings.Log.success);
                    }
                }
                else
                {
                    Log(log + " " + Strings.Log.failedDoesntHave, LogType.Warning, lFrom.gameObject.name.ToString(), lFrom.GetType().ToString());
                }
            }
        }

        /// <summary>
        /// Copies all MeshRenderers in object and it's children to another object.
        /// </summary>        
        /// <param name="createGameObjects">Whether to create missing game objects</param>
        void CopyAllMeshRenderers(GameObject from, GameObject to, bool createGameObjects, bool useignoreList)
        {
            if(from == null || to == null)
                return;

            var mFromArr = from.GetComponentsInChildren<MeshRenderer>();

            for(int i = 0; i < mFromArr.Length; i++)
            {
                var rFrom = mFromArr[i];
                var tTo = Helpers.FindTransformInAnotherHierarchy(rFrom.transform, to.transform, createGameObjects);

                if((!tTo) ||
                    (useignoreList && Helpers.ShouldIgnoreObject(rFrom.transform, _copierIgnoreArray, bCopier_ignoreArray_includeChildren)))
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
                    }
                }
                else
                {
                    Log(Strings.Log.failedHasNo, LogType.Warning, rFrom.gameObject.name, rFrom.GetType().ToString());
                }
            }
        }

        /// <summary>
        /// Copies over the AvatarDescriptor and PipelineManager components.
        /// </summary>
        void CopyAvatarDescriptor(GameObject from, GameObject to, bool useignoreList)
        {
            if(to == null || from == null)
                return;

            if(useignoreList && Helpers.ShouldIgnoreObject(from.transform, _copierIgnoreArray))
                return;

            var dFrom = from.GetComponent<VRC_AvatarDescriptor>();
            var pFrom = from.GetComponent<PipelineManager>();
            var dTo = to.GetComponent<VRC_AvatarDescriptor>();

            if(dFrom == null)
                return;
            if(dTo == null)
                dTo = Undo.AddComponent<VRC_AvatarDescriptor>(to);

            var pTo = to.GetComponent<PipelineManager>();

            if(pTo == null) //but it shouldn't be
                pTo = Undo.AddComponent<PipelineManager>(to);

            if(bCopier_descriptor_copyPipelineId)
            {
                pTo.blueprintId = pFrom.blueprintId;
                pTo.enabled = pFrom.enabled;
                pTo.completedSDKPipeline = true;

                EditorUtility.SetDirty(pTo);
                EditorSceneManager.MarkSceneDirty(pTo.gameObject.scene);
                EditorSceneManager.SaveScene(pTo.gameObject.scene);
            }

            if(bCopier_descriptor_copySettings)
            {
                dTo.Animations = dFrom.Animations;
                dTo.apiAvatar = dFrom.apiAvatar;
                dTo.lipSync = dFrom.lipSync;
                dTo.lipSyncJawBone = dFrom.lipSyncJawBone;
                dTo.MouthOpenBlendShapeName = dFrom.MouthOpenBlendShapeName;
                dTo.Name = dFrom.Name;
                dTo.ScaleIPD = dFrom.ScaleIPD;
                dTo.unityVersion = dFrom.unityVersion;                
                dTo.VisemeBlendShapes = dFrom.VisemeBlendShapes;

                if(dFrom.VisemeSkinnedMesh != null)
                {
                    string s = Helpers.GetGameObjectPath(dFrom.VisemeSkinnedMesh.gameObject, true);
                    Transform t = dTo.transform.Find(s);
                    if(t != null)
                    {
                        dTo.VisemeSkinnedMesh = t.GetComponent<SkinnedMeshRenderer>();
                    }
                }

                if(bCopier_descriptor_copyAnimationOverrides)
                {
                    dTo.CustomSittingAnims = dFrom.CustomSittingAnims;
                    dTo.CustomStandingAnims = dFrom.CustomStandingAnims;
                }               
            }

            if(bCopier_descriptor_copyViewpoint)
            {
                dTo.ViewPosition = dFrom.ViewPosition;
            }
        }

        /// <summary>
        /// Copies all DynamicBoneColliders from object and it's children to another object.
        /// </summary>        
        /// <param name="removeOldColliders">Whether to remove all DynamicBoneColliders from target before copying</param>
        void CopyAllDynamicBoneColliders(GameObject from, GameObject to, bool createGameObjects, bool useIgnoreList)
        {
#if !BONES && !OLD_BONES
            Debug.Log("No DynamicBones found in project. You shouldn't be able to use this. Help!");
            return;
#else
            if(from == null || to == null)
                return;

            var dbcFromArr = from.GetComponentsInChildren<DynamicBoneCollider>();
            if(dbcFromArr == null || dbcFromArr.Length == 0)
                return;

            for(int i = 0; i < dbcFromArr.Length; i++)
            {
                var dbcFrom = dbcFromArr[i];
                var tTo = Helpers.FindTransformInAnotherHierarchy(dbcFrom.transform, to.transform, createGameObjects);
                if((!tTo) || (useIgnoreList && Helpers.ShouldIgnoreObject(dbcFrom.transform, _copierIgnoreArray, bCopier_ignoreArray_includeChildren)))
                    continue;

                var dbcToArr = tTo.GetComponentsInChildren<DynamicBoneCollider>();
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

        void CopyAllDynamicBonesNew(GameObject from, GameObject to, bool createMissing, bool useIgnoreList)
        {
#if !BONES && !OLD_BONES
            Debug.Log("No DynamicBones found in project. You shouldn't be able to use this. Help!");
            return;
#else
            if(!from || !to)
                return;

            var dFromArr = from.GetComponentsInChildren<DynamicBone>();

            List<DynamicBone> newBones = new List<DynamicBone>();
            foreach(var dFrom in dFromArr)
            {
                if(useIgnoreList && Helpers.ShouldIgnoreObject(dFrom.transform, _copierIgnoreArray, bCopier_ignoreArray_includeChildren))
                    continue;

                var tTo = Helpers.FindTransformInAnotherHierarchy(dFrom.transform, to.transform, false);
                if(!tTo)
                    continue;

                var dToArr = tTo.GetComponents<DynamicBone>();

                if(!dFrom.m_Root)
                {
                    LogVerbose("DynamicBone {0} of {1} doesn't have a root assigned. Ignoring", LogType.Warning, dFrom.transform.name, dFrom.transform.root.name);
                    continue;
                }

                bool foundSameDynBone = false;

                foreach(var d in dToArr)
                {                    
                    if(!d.m_Root || newBones.Contains(d))
                        continue;                    

                    //Check if the roots are the same to decide if it's supposed to be the same dyn bone script
                    if(d.m_Root.name == dFrom.m_Root.name)
                    {
                        //Check if exclusions are the same
                        List<string> exToPaths = d.m_Exclusions
                             .Select(o => Helpers.GetGameObjectPath(o.gameObject).ToLower())
                             .Where(o => o != null)
                             .ToList();

                        List<string> exFromPaths = dFrom.m_Exclusions
                            .Select(o => Helpers.GetGameObjectPath(o.gameObject).ToLower())
                            .Where(o => o != null)
                            .ToList();

                        bool exclusionsDifferent = false;
                        var exArr = exToPaths.Intersect(exFromPaths).ToArray();

                        if(exArr != null && (exToPaths.Count != 0 && exFromPaths.Count != 0) && exArr.Length == 0)                        
                            exclusionsDifferent = true;      

                        //Check if colliders are the same
                        List<string> colToPaths = d.m_Colliders
                            .Select(c => Helpers.GetGameObjectPath(c.gameObject).ToLower())
                            .Where(c => c != null)
                            .ToList();

                        List<string> colFromPaths = d.m_Colliders
                            .Select(c => Helpers.GetGameObjectPath(c.gameObject).ToLower())
                            .Where(c => c != null)
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
                                LogVerbose("{0} already has this DynamicBone, but we have to copy settings. Copying.", LogType.Log, d.name);

                                d.m_Damping = dFrom.m_Damping;
                                d.m_DampingDistrib = dFrom.m_DampingDistrib;
                                d.m_DistanceToObject = dFrom.m_DistanceToObject;
                                d.m_DistantDisable = dFrom.m_DistantDisable;
                                d.m_Elasticity = dFrom.m_Elasticity;
                                d.m_ElasticityDistrib = dFrom.m_ElasticityDistrib;
                                d.m_EndLength = dFrom.m_EndLength;
                                d.m_EndOffset = dFrom.m_EndOffset;
                                d.m_Force = dFrom.m_Force;
                                d.m_FreezeAxis = dFrom.m_FreezeAxis;
                                d.m_Gravity = dFrom.m_Gravity;
                                d.m_Inert = dFrom.m_Inert;
                                d.m_InertDistrib = dFrom.m_InertDistrib;
                                d.m_Radius = dFrom.m_Radius;
                                d.m_RadiusDistrib = dFrom.m_RadiusDistrib;
                                d.m_Stiffness = dFrom.m_Stiffness;
                                d.m_StiffnessDistrib = dFrom.m_StiffnessDistrib;                                

                                d.m_ReferenceObject = Helpers.FindTransformInAnotherHierarchy(dFrom.m_ReferenceObject, d.transform, false);                                
                            }
                            else
                            {
                                LogVerbose("{0} already has this DynamicBone but we aren't copying settings. Ignoring", LogType.Log, d.name);
                            }
                            break;
                        }
                    }
                }

                if(!foundSameDynBone)
                {
                    if(createMissing)
                    {
                        LogVerbose("{0} doesn't have has this DynamicBone but we have to create one. Creating.", LogType.Log, dFrom.name);

                        var newDynBone = tTo.gameObject.AddComponent<DynamicBone>();
                        ComponentUtility.CopyComponent(dFrom);
                        ComponentUtility.PasteComponentValues(newDynBone);

                        newDynBone.m_Root = Helpers.FindTransformInAnotherHierarchy(dFrom.m_Root.transform, newDynBone.transform.root, false);

                        if(!newDynBone.m_Root)
                        {
                            Log("_Couldn't set root {0} for new DynamicBone in {1}'s {2}. GameObject is missing. Removing.", LogType.Warning, dFrom.m_Root.name ?? "null", newDynBone.transform.root.name, newDynBone.transform.name == newDynBone.transform.root.name ? "root" : newDynBone.transform.root.name);
                            DestroyImmediate(newDynBone);
                        }

                        if(dFrom.m_ReferenceObject)
                            newDynBone.m_ReferenceObject = Helpers.FindTransformInAnotherHierarchy(dFrom.m_ReferenceObject, newDynBone.transform.root, false);

#if BONES
                        var newColliders = new List<DynamicBoneColliderBase>();
#elif OLD_BONES
                        var newColliders = new List<DynamicBoneCollider>();
#endif


                        for(int i = 0; i < newDynBone.m_Colliders.Count; i++)
                        {
                            var badRefCollider = newDynBone.m_Colliders[i];

                            if(!badRefCollider)
                                continue;

#if BONES
                            DynamicBoneColliderBase fixedRefCollider = null;
#elif OLD_BONES
                            DynamicBoneCollider fixedRefCollider = null;
#endif
                            var t = Helpers.FindTransformInAnotherHierarchy(newDynBone.m_Colliders[i].transform, to.transform, false);

                            if(t == null)
                                continue;

                            var toColls = t.GetComponents<DynamicBoneCollider>();
                            foreach(var c in toColls)
                            {
                                if(c.m_Bound == badRefCollider.m_Bound && c.m_Center == badRefCollider.m_Center && c.m_Direction == badRefCollider.m_Direction &&
                                   !newDynBone.m_Colliders.Contains(c))
                                    fixedRefCollider = c;
                            }

                            if(fixedRefCollider)
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

                        Log("_Copied DynamicBone from {0}'s {1} to {2}'s {1}", LogType.Log, dFrom.transform.root.name, dFrom.transform.name == dFrom.transform.root.name ? "root" : dFrom.transform.name, tTo.root.name);                                                
                    }
                    else
                    {
                        LogVerbose("{0} doesn't have has this DynamicBone and we aren't creating a new one. Ignoring.", LogType.Log, dFrom.name);
                    }
                }
            }
#endif
        }

        /// <summary>
        /// Copies all DynamicBones from one object and all of it's children to another
        /// </summary>        
		void CopyAllDynamicBones(GameObject from, GameObject to, bool createIfMissing, bool useignoreList)
        {
#if !BONES && !OLD_BONES
            Debug.Log("No DynamicBones found in project. You shouldn't be able to use this. Help!");
            return;
#else
            if(!from || !to)
                return;

            var dFromArr = from.GetComponentsInChildren<DynamicBone>();
            bool foundSameDynBone = false;

            foreach(var dFrom in dFromArr)
            {
                if(useignoreList && Helpers.ShouldIgnoreObject(dFrom.transform, _copierIgnoreArray, bCopier_ignoreArray_includeChildren))
                    continue;

                var tTo = Helpers.FindTransformInAnotherHierarchy(dFrom.transform, to.transform, false);
                var dToArr = tTo.GetComponents<DynamicBone>();

                if(!dFrom.m_Root)
                {
                    LogVerbose("DynamicBone {0} of {1} doesn't have a root assigned. Ignoring", LogType.Warning, dFrom.transform.name, dFrom.transform.root.name);
                    continue;
                }

                foreach(var d in dToArr)
                {
                    if(!d.m_Root)
                        continue;
                    
                    if(d.m_Root.name == dFrom.m_Root.name)
                    {
                        foundSameDynBone = true;
                        LogVerbose("{0} already has this DynamicBone. Ignoring", LogType.Log, d.name);
                    }
                }

                if(!foundSameDynBone)
                {
                    var newDynBone = tTo.gameObject.AddComponent<DynamicBone>();
                    ComponentUtility.CopyComponent(dFrom);
                    ComponentUtility.PasteComponentValues(newDynBone);

                    newDynBone.m_Root = Helpers.FindTransformInAnotherHierarchy(dFrom.m_Root.transform, newDynBone.transform.root, false);

                    if(dFrom.m_ReferenceObject)
                        newDynBone.m_ReferenceObject = Helpers.FindTransformInAnotherHierarchy(dFrom.m_ReferenceObject, newDynBone.transform.root, false);

#if BONES
                    var newColliders = new List<DynamicBoneColliderBase>();
#elif OLD_BONES
                    var newColliders = new List<DynamicBoneCollider>();
#endif


                    for(int i = 0; i < newDynBone.m_Colliders.Count; i++)
                    {
                        var badRefCollider = newDynBone.m_Colliders[i];

                        if(!badRefCollider)
                            continue;

#if BONES
                        DynamicBoneColliderBase fixedRefCollider = null;
#elif OLD_BONES
                        DynamicBoneCollider fixedRefCollider = null;
#endif
                        var t = Helpers.FindTransformInAnotherHierarchy(newDynBone.m_Colliders[i].transform, to.transform, false);

                        if(t == null)
                            continue;

                        var toColls = t.GetComponents<DynamicBoneCollider>();
                        foreach(var c in toColls)
                        {
                            if(c.m_Bound == badRefCollider.m_Bound && c.m_Center == badRefCollider.m_Center && c.m_Direction == badRefCollider.m_Direction &&
                               !newDynBone.m_Colliders.Contains(c))
                                fixedRefCollider = c;
                        }

                        if(fixedRefCollider)
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

                    if(!newDynBone.m_Root)
                    {
                        Log("_Couldn't set root {0} for new DynamicBone in {1}'s {2}. Removing.", LogType.Warning, newDynBone.m_Root.name, newDynBone.transform.root.name, newDynBone.transform.name);
                        DestroyImmediate(newDynBone);
                    }
                    else
                    {                        
                        Log("_Copied DynamicBone from {0}'s {1} to {2}'s", LogType.Log, dFrom.transform.root.name, dFrom.transform.name == dFrom.transform.root.name ? "root" : dFrom.transform.name, tTo.root.name);
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
        void CopyAllColliders(GameObject from, GameObject to, bool createGameObjects, bool useignoreList)
        {
            if(from == null || to == null)
                return;
            if(!(bCopier_colliders_copyBox || bCopier_colliders_copyCapsule || bCopier_colliders_copyMesh || bCopier_colliders_copySphere))
                return;

            var cFromArr = from.GetComponentsInChildren<Collider>();

            for(int i = 0; i < cFromArr.Length; i++)
            {
                string log = Strings.Log.copyAttempt;
                var type = cFromArr[i].GetType();
                
                var cc = cFromArr[i];
                var cFromPath = Helpers.GetGameObjectPath(cc.gameObject);

                if(useignoreList && Helpers.ShouldIgnoreObject(cc.transform, _copierIgnoreArray, bCopier_ignoreArray_includeChildren))
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
        /// Copies Transform component settings. Note that only one can exist on an object, and every object should have one already.
        /// </summary>    
        void CopyTransforms(GameObject from, GameObject to, bool useignoreList)
        {
            if(from == null || to == null)
                return;

            var tFrom = from.transform;
            var tTo = to.transform;

            if(useignoreList && Helpers.ShouldIgnoreObject(tFrom, _copierIgnoreArray, bCopier_ignoreArray_includeChildren))
                return;

            string log = Strings.Log.copyAttempt + " ";
            string[] logFormat = { "Transforms", from.name, to.name };

            if(tTo == null || tFrom == null)
            {
                log += "_Failed: {1} or {2} is null. This shouldn't even be possible. What are you doing?";
                Log(log, LogType.Error);
                return;
            }

            if(tFrom == tFrom.root || tFrom == tFrom.root.Find(tFrom.name))
            {
                log += "_Ignored: {2} is root or child of root.";
                Log(log, LogType.Log, logFormat);
                return;
            }

            if(bCopier_transforms_copyPosition)
                tTo.localPosition = tFrom.localPosition;
            if(bCopier_transforms_copyScale)
                tTo.localScale = tFrom.localScale;
            if(bCopier_transforms_copyRotation)
            {
                tTo.localEulerAngles = tFrom.localEulerAngles;
                tTo.localRotation = tFrom.localRotation;
            }

            log += "Success: Copied {0} from {1} to {2}";
            Log(log, LogType.Log, logFormat);
        }

        /// <summary>
        /// Copies settings of all SkinnedMeshRenderers in object and children.
        /// Does NOT copy mesh, bounds and root bone settings because that breaks everything.
        /// </summary>        
        void CopyAllSkinnedMeshRenderersSettings(GameObject from, GameObject to, bool useignoreList)
        {
            if((from == null || to == null) || (!(bCopier_skinMeshRender_copyBlendShapeValues || bCopier_skinMeshRender_copyMaterials || bCopier_skinMeshRender_copySettings)))
                return;

            string log = String.Format(Strings.Log.copyAttempt + " - ", Strings.Copier.skinMeshRender, from.name, to.name);

            var rFromArr = from.GetComponentsInChildren<SkinnedMeshRenderer>();

            for(int i = 0; i < rFromArr.Length; i++)
            {
                var rFrom = rFromArr[i];
                var rFromPath = Helpers.GetGameObjectPath(rFrom.gameObject);

                if(rFromPath != null)
                {
                    var tTo = to.transform.root.Find(rFromPath);

                    if((!tTo) ||
                        (useignoreList && Helpers.ShouldIgnoreObject(rFrom.transform, _copierIgnoreArray, bCopier_ignoreArray_includeChildren)))
                        continue;

                    GameObject rToObj = tTo.gameObject;

                    var rTo = rToObj.GetComponent<SkinnedMeshRenderer>();

                    if(rTo != null)
                    {
                        if(bCopier_skinMeshRender_copySettings)
                        {
                            var t = Helpers.FindTransformInAnotherHierarchy(rFrom.rootBone, rTo.transform, false);
                            rTo.rootBone = t ?? rTo.rootBone;
                            t = Helpers.FindTransformInAnotherHierarchy(rFrom.probeAnchor, rTo.transform, false);

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
                        if(bCopier_skinMeshRender_copyBlendShapeValues)
                        {
                            for(int z = 0; z < rFrom.sharedMesh.blendShapeCount; z++)
                            {
                                int index = rFrom.sharedMesh.GetBlendShapeIndex(rFrom.sharedMesh.GetBlendShapeName(z));
                                if(index != -1)
                                {
                                    rTo.SetBlendShapeWeight(index, rFrom.GetBlendShapeWeight(index));
                                }
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
        void CopyAllTrailRenderers(GameObject from, GameObject to, bool createGameObjects, bool useignoreList)
        {
            if(from == null || to == null)
                return;

            var rFromArr = from.GetComponentsInChildren<TrailRenderer>();

            for(int i = 0; i < rFromArr.Length; i++)
            {
                var rFrom = rFromArr[i];
                var tTo = Helpers.FindTransformInAnotherHierarchy(rFrom.transform, to.transform, createGameObjects);

                if(!tTo)
                    continue;

                if(useignoreList && Helpers.ShouldIgnoreObject(rFrom.transform, _copierIgnoreArray, bCopier_ignoreArray_includeChildren))
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
        void CopyAllRigidBodies(GameObject from, GameObject to, bool createGameObjects, bool useignoreList)
        {
            if(from == null || to == null)
                return;

            var rFromArr = from.GetComponentsInChildren<Rigidbody>();

            for(int i = 0; i < rFromArr.Length; i++)
            {
                var rFrom = rFromArr[i];
                var tTo = Helpers.FindTransformInAnotherHierarchy(rFrom.transform, to.transform, createGameObjects);

                if(!tTo)
                    continue;

                if(useignoreList && Helpers.ShouldIgnoreObject(rFrom.transform, _copierIgnoreArray, bCopier_ignoreArray_includeChildren))
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
        void CopyAllParticleSystems(GameObject from, GameObject to, bool createGameObjects, bool useignoreList)
        {
            var pFromArr = from.GetComponentsInChildren<ParticleSystem>(true);

            for(int i = 0; i < pFromArr.Length; i++)
            {
                var pp = pFromArr[i];

                if(useignoreList && Helpers.ShouldIgnoreObject(pp.transform, _copierIgnoreArray, bCopier_ignoreArray_includeChildren))
                    continue;

                var tTo = Helpers.FindTransformInAnotherHierarchy(pp.transform, to.transform, createGameObjects);

                if(tTo != null)
                {
                    var pTo = tTo.GetComponent<ParticleSystem>();
                    if(bCopier_particleSystems_replace || pTo == null)
                    {
                        DestroyParticleSystems(tTo.gameObject, false);

                        ComponentUtility.CopyComponent(pp);
                        ComponentUtility.PasteComponentAsNew(tTo.gameObject);

                        Log(Strings.Log.successCopiedOverFromTo, LogType.Log, "ParticleSystem", copierSelectedFrom.name, pp.gameObject.name, SelectedAvatar.name, tTo.gameObject.name);
                    }
                    else
                    {                        
                        Log(Strings.Log.failedAlreadyHas , LogType.Log, pp.gameObject.name, "ParticleSystem");
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
                sysList.AddRange(from.GetComponentsInChildren<ParticleSystem>());
            else
                sysList.AddRange(from.GetComponents<ParticleSystem>());

            foreach(var p in sysList)
            {
                var rend = p.GetComponent<ParticleSystemRenderer>();

                if(rend != null)
                    DestroyImmediate(rend);

                Log(Strings.Log.removeAttempt + ": " + "_Success.", LogType.Log, p.ToString(), from.name);
                DestroyImmediate(p);
            }
        }

        /// <summary>
        /// Destroys GameObjects in object and all children, if it has no children and if it's not a bone
        /// </summary>        
        void DestroyEmptyGameObjects(GameObject from)
        {
            var obj = from.GetComponentsInChildren<Transform>();
            var renders = from.GetComponentsInChildren<SkinnedMeshRenderer>();

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
        /// Destroy all components of type.        
        /// </summary>        
        void DestroyAllComponentsOfType(GameObject obj, Type type, bool ignoreRoot, bool useignoreList)
        {
            string log = "";            

            Component[] comps = obj.transform.GetComponentsInChildren(type, true);

            if(comps != null && comps.Length > 0)
            {
                for(int i = 0; i < comps.Length; i++)
                {
                    if((ignoreRoot && comps[i].transform.parent == null) ||
                        (useignoreList && Helpers.ShouldIgnoreObject(comps[i].transform, _copierIgnoreArray, bCopier_ignoreArray_includeChildren)))
                        continue;

                    log = Strings.Log.removeAttempt + " - ";
                    string name = comps[i].name;

                    if(!PrefabUtility.IsPartOfPrefabInstance(comps[i]))
                    {
                        try
                        {
                            DestroyImmediate(comps[i]);
                            log += Strings.Log.success;
                            Log(log, LogType.Log, type.ToString(), name);
                        }
                        catch(Exception e)
                        {
                            log += Strings.Log.failed + ": " + e.Message;
                            Log(log, LogType.Exception, type.ToString(), name);
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

        #region Helper Functions

        public static void RefreshPresetIndex<T>() where T : PumkinPreset
        {
            if(typeof(T) == typeof(PumkinsCameraPreset))
                RefreshPresetIndex<T>(ref Instance._selectedCameraPresetString, ref Instance._selectedCameraPresetIndex);
            else if(typeof(T) == typeof(PumkinsPosePreset))
                RefreshPresetIndex<T>(ref Instance._selectedPosePresetString, ref Instance._selectedPosePresetIndex);
            else if(typeof(T) == typeof(PumkinsBlendshapePreset))
                RefreshPresetIndex<T>(ref Instance._selectedBlendshapePresetString, ref Instance._selectedBlendshapePresetIndex);
        }

        static void RefreshPresetIndex<T>(ref string selectedPresetString, ref int selectedPresetIndex) where T : PumkinPreset
        {
            int count = PumkinsPresetManager.GetPresetCountOfType<T>();
            int i = PumkinsPresetManager.GetPresetIndex<T>(selectedPresetString);
            selectedPresetIndex = Mathf.Clamp(i, 0, count - 1);

            if(count > 0)
            {
                if(typeof(T) == typeof(PumkinsCameraPreset))
                    selectedPresetString = PumkinsPresetManager.CameraPresets[selectedPresetIndex].ToString() ?? "";
                else if(typeof(T) == typeof(PumkinsPosePreset))
                    selectedPresetString = PumkinsPresetManager.PosePresets[selectedPresetIndex].ToString() ?? "";
                else if(typeof(T) == typeof(PumkinsBlendshapePreset))
                    selectedPresetString = PumkinsPresetManager.BlendshapePresets[selectedPresetIndex].ToString() ?? "";
            }
        }                

        /// <summary>
        /// Resets all BlendShape weights to 0 on all SkinnedMeshRenderers or to prefab values
        /// </summary>        
        /// <param name="revertToPrefab">Revert weights to prefab instead of 0</param>
        public static void ResetBlendShapes(GameObject objTo, bool revertToPrefab)
        {
            var renders = objTo.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            foreach(var r in renders)
            {
                ResetBlendShapes(r, revertToPrefab);
            }
        }

        /// <summary>
        /// Reset all BlendShape weights to 0 on SkinnedMeshRenderer or to prefab values
        /// </summary>        
        /// <param name="revertToPrefab">Revert weights to prefab instead of 0</param>        
        public static void ResetBlendShapes(SkinnedMeshRenderer render, bool revertToPrefab)
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
#if UNITY_2017
                pref = PrefabUtility.GetPrefabParent(render.gameObject) as GameObject;
#else
			    pref = PrefabUtility.GetCorrespondingObjectFromSource(render.gameObject) as GameObject;
#endif
                
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

        public static bool ResetPose(GameObject avatar)
        {
            if(!avatar)
                return false;

#if UNITY_2017
            var pref = PrefabUtility.GetPrefabParent(objTo.transform.root.gameObject) as GameObject;
#else
            var pref = PrefabUtility.GetCorrespondingObjectFromSource(avatar.transform.root.gameObject) as GameObject;
#endif
            if(!pref)
            {
                Log(Strings.Log.meshPrefabMissingCantRevertPose, LogType.Error);
                return false;
            }

            var trans = avatar.GetComponentsInChildren<Transform>();

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

            OnPoseWasChanged(PoseChangeType.Reset);
            return true;
        }

        /// <summary>
        /// Reset transforms back to prefab values
        /// </summary>        
        public static bool ResetPoseOld(GameObject objTo)
        {
            if(objTo == null)
                return false;            

            string toPath = Helpers.GetGameObjectPath(objTo);
#if UNITY_2017
            var pref = PrefabUtility.GetPrefabParent(objTo.transform.root.gameObject) as GameObject;
#else
			var pref = PrefabUtility.GetCorrespondingObjectFromSource(objTo.transform.root.gameObject) as GameObject;
#endif
            if(!pref)
            {
                Log(Strings.Log.meshPrefabMissingCantRevertPose, LogType.Error);
                return false;
            }

            Transform tr = pref.transform.Find(toPath);

            if(!tr)
                return false;

            GameObject objFrom = tr.gameObject;

            if(objTo.transform != objTo.transform.root)
            {
                objTo.transform.localPosition = objFrom.transform.localPosition;
                objTo.transform.localEulerAngles = objFrom.transform.localEulerAngles;
                objTo.transform.localRotation = objFrom.transform.localRotation;
            }            

            //Loop through Children
            for(int i = 0; i < objFrom.transform.childCount; i++)
            {
                var fromChild = objFrom.transform.GetChild(i).gameObject;
                var t = objTo.transform.Find(fromChild.name);

                if(t == null)
                    continue;

                var toChild = t.gameObject;

                if(fromChild != null && toChild != null)
                {
                    ResetPose(toChild);
                }
            }            
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

        /// <summary>
        /// This will move the VRCCam thumbnail camera on the avatar's face, with an offset
        /// </summary>
        /// <param name="avatarOverride">If this is null, get the avatar we're currently uploading</param>
        void CenterCameraOnViewpoint(Camera camera, GameObject avatarOverride, Vector3 positionOffset, Vector3 rotationOffset)
        {
            if(!camera)
                return;

            VRC_AvatarDescriptor desc;
            if(avatarOverride == null)
            {
                var pm = FindObjectOfType<RuntimeBlueprintCreation>();
                if(pm == null)
                {
                    Log(Strings.Log.runtimeBlueprintNotFoundStartUploading, LogType.Warning);
                    return;
                }
                desc = pm.pipelineManager.GetComponent<VRC_AvatarDescriptor>();
            }
            else
            {
                desc = avatarOverride.GetComponent<VRC_AvatarDescriptor>();
            }

            if(desc == null)
            {
                Log(Strings.Log.failedToCenterCameraNoDescriptor, LogType.Error);
                return;
            }

            camera.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

            GameObject focusObj = new GameObject("FocusDummy");
            focusObj.transform.position = desc.transform.position + desc.ViewPosition;
            focusObj.transform.rotation = desc.transform.rotation;

            Transform oldParent = camera.transform.parent;
            camera.transform.parent = focusObj.transform;

            focusObj.transform.localEulerAngles = focusObj.transform.localEulerAngles + rotationOffset;

            camera.transform.localPosition = Vector3.zero + new Vector3(0, desc.ViewPosition.z * 0.05f, desc.ViewPosition.y * -0.28f);
            camera.transform.parent = null;
            camera.transform.position = camera.transform.position + positionOffset;
            camera.transform.parent = oldParent;

            DestroyImmediate(focusObj);
        }

        void CenterCameraOnViewpointNew(GameObject avatarOverride, Vector3 positionOffset, Vector3 rotationOffset)
        {
            if(centerCameraFixClippingPlanes)
            {
                SelectedCamera.farClipPlane = 1000;
                SelectedCamera.nearClipPlane = 0.01f;
            }
            PumkinsCameraPreset.ApplyPositionAndRotationWithViewpointOffset(avatarOverride, SelectedCamera, positionOffset, rotationOffset);            
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
            var data = EditorPrefs.GetString("PumkinToolsWindow", JsonUtility.ToJson(this, false));
            if(data != null)
            {
                JsonUtility.FromJsonOverwrite(data, this);
                RefreshLanguage();
                RefreshPresetIndex<PumkinsCameraPreset>();
                RefreshPresetIndex<PumkinsPosePreset>();
                RefreshPresetIndex<PumkinsBlendshapePreset>();
                LogVerbose("Loaded tool window preferences");
            }
            else
            {
                LogVerbose("Failed to load window preferences");
            }
        }        

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

#endregion
    }    
}