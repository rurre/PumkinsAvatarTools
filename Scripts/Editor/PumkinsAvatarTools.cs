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

        //Tools
        [SerializeField] public static GameObject selectedAvatar;
        [SerializeField] static bool _useSceneSelectionAvatar = false;       

        //Component Copier
        [SerializeField] public static GameObject copierSelectedFrom;        

        [SerializeField] bool bCopier_transforms_copy = true;
        [SerializeField] bool bCopier_transforms_copyPosition = false;
        [SerializeField] bool bCopier_transforms_copyRotation = false;
        [SerializeField] bool bCopier_transforms_copyScale = false;
        [SerializeField] bool bCopier_transforms_copyAvatarScale = true;

        [SerializeField] bool bCopier_dynamicBones_copy = true;
        [SerializeField] bool bCopier_dynamicBones_copySettings = true;
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

        //Thumbnails        
        [SerializeField] bool bThumbnails_override_camera_image = false;
        [SerializeField] bool hideAvatarsOnStart = true;
        GameObject _cameraOverlay = null;
        GameObject _cameraBackground = null;
        RawImage _cameraOverlayImage = null;
        RawImage _cameraBackgroundImage = null;

        Texture2D _emptyTexture;

        Texture2D cameraBackgroundTexture;
        Texture2D cameraOverrideTexture;
        string _lastOpenFilePath = "";
        string _backgroundPathText = "";
        string _overlayPathText = "";

        CameraBackgroundOverrideType camBgOverrideType = CameraBackgroundOverrideType.None;

        bool vrcCamSetBgColor = false;
        bool vrcCamSetSkybox = false;
        bool vrcCamSetBackground = false;
        [SerializeField] Color _vrcCamBgColor = Colors.DarkCameraBackground;
        [SerializeField] Color _vrcCamColorOld = Colors.DefaultCameraBackground;
        [SerializeField] CameraClearFlags _vrcCamClearFlagsOld = CameraClearFlags.Skybox;
        Material _thumbsSkyboxOld = null;

        public enum CameraBackgroundOverrideType { None, Color, Material, Image };

        //UI
        [SerializeField] bool _tools_expand = true;
        [SerializeField] bool _tools_avatar_expand = true;
        [SerializeField] bool _tools_removeAll_expand = true;

        [SerializeField] bool _avatarInfo_expand = false;
        [SerializeField] bool _thumbnails_expand = false;

        [SerializeField] bool _misc_expand = true;
                
        //Ignore Array        
        [SerializeField] bool _copierIgnoreArray_expand = false;
        [SerializeField] SerializedProperty _serializedIgnoreArrayProp;        
        [SerializeField] Transform[] _copierIgnoreArray = new Transform[0];
        [SerializeField] bool bCopier_ignoreArray_includeChildren = false;
        [SerializeField] Vector2 _copierIgnoreArrayScroll = Vector2.zero;

        static string _mainScriptPath = null;
        private static Camera _vrcCam = null; //use property
        static bool _eventsAdded = false;

        VRC_AvatarDescriptor _tempAvatarDescriptor;

        //Editing Viewpoint        
        bool _editingView = false;
        Vector3 _viewPosOld;
        Vector3 _viewPosTemp;
        Tool _tempToolOld = Tool.None;        

        //Editing Scale
        bool _editingScale = false;
        Vector3 _avatarScaleOld;
        [SerializeField] float _avatarScaleTemp;
        [SerializeField] SerializedProperty _serializedAvatarScaleTempProp;
        Transform _scaleViewpointDummy;

        //Avatar info
        static AvatarInfo avatarInfo = null;
        static string _avatarInfoSpace = "\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n";
        static string _avatarInfoString = Strings.AvatarInfo.SelectAvatarFirst + _avatarInfoSpace; //Please don't hurt me for this        

        //Camera Overlay
        readonly string cameraOverlayName = "_PumkinsCameraOverlay";
        readonly string cameraBackgroundName = "_PumkinsCameraBackground";

        //Misc
        SerializedObject _serializedScript;
        [SerializeField] bool _openedInfo = false;
        [SerializeField] Vector2 _mainScroll = Vector2.zero;
        static bool verboseLoggingEnabled = false;

        int _selectedLanguageIndex = 0;

        enum ToolMenuActions
        {
            RemoveDynamicBones,
            RemoveDynamicBoneColliders,
            RemoveColliders,
            RemoveRigidBodies,
            ResetPose,
            RevertBlendshapes,
            ZeroBlendshapes,
            FixRandomMouth,
            DisableBlinking,
            EditViewpoint,
            FillVisemes,
            RemoveEmptyGameObjects,
            RemoveParticleSystems,
            SetTPose,
            RemoveTrailRenderers,
            RemoveMeshRenderers,
            RemoveAudioSources,
            RemoveLights,
            RemoveAnimatorsInChildren,
            RemoveJoints,
            EditScale,
        };

        #endregion

        #region Properties

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

        public static Camera VRCCam
        {
            get
            {
                if(_vrcCam == null)
                {
                    var c = GameObject.Find("VRCCam");
                    if(c != null)
                    {
                        VRCCam = c.GetComponent<Camera>();
                    }
                }
                return _vrcCam;
            }

            private set
            {
                _vrcCam = value;
            }
        }

        public GameObject CameraOverlay
        {
            get
            {
                if(!_cameraOverlay && _thumbnails_expand)
                    _cameraOverlay = GameObject.Find(cameraOverlayName) ?? new GameObject(cameraOverlayName);

                return _cameraOverlay;
            }
        }

        public RawImage CameraOverlayRawImage
        {
            get
            {
                if(!_cameraOverlayImage && CameraOverlay)
                {
                    _cameraOverlayImage = CameraOverlay.GetComponent<RawImage>() ?? CameraOverlay.AddComponent<RawImage>();

                    Canvas c = CameraOverlay.GetComponent<Canvas>();
                    if(!c)
                        c = CameraOverlay.AddComponent<Canvas>();
                    c.worldCamera = VRCCam;
                    c.renderMode = RenderMode.ScreenSpaceCamera;
                    c.planeDistance = VRCCam.nearClipPlane + 0.01f;

                }
                return _cameraOverlayImage;
            }
        }

        public GameObject CameraBackground
        {
            get
            {
                if(!_cameraBackground && _thumbnails_expand)
                    _cameraBackground = GameObject.Find(cameraBackgroundName) ?? new GameObject(cameraBackgroundName);
                return _cameraBackground;
            }
        }

        public RawImage CameraBackgroundRawImage
        {
            get
            {
                if(_cameraBackgroundImage == null && CameraBackground)
                {
                    _cameraBackgroundImage = CameraBackground.GetComponent<RawImage>();
                    if(_cameraBackgroundImage == null)
                        _cameraBackgroundImage = CameraBackground.AddComponent<RawImage>();

                    Canvas c = CameraBackground.GetComponent<Canvas>();
                    if(!c)
                        c = CameraBackground.AddComponent<Canvas>();
                    c.worldCamera = VRCCam;
                    c.renderMode = RenderMode.ScreenSpaceCamera;
                    c.planeDistance = VRCCam.farClipPlane - 0.01f;
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

        #region Unity GUI             

        public void HandleOnEnable()
        {
            LogVerbose("Tools window: OnEnable()");
            SceneView.onSceneGUIDelegate += OnSceneGUI;
            if(!_eventsAdded)
            {
                EditorApplication.playModeStateChanged += HandlePlayModeStateChange;
                Selection.selectionChanged += HandleSelectionChanged;
                _eventsAdded = true;
            }                        
            
            SerializedScript = new SerializedObject(this);            
            SerializedIgnoreArray = SerializedScript.FindProperty("_copierIgnoreArray");
            SerializedScaleTemp = SerializedScript.FindProperty("_avatarScaleTemp");
            
            _emptyTexture = new Texture2D(2, 2);
            cameraOverrideTexture = new Texture2D(2, 2);
            _thumbsSkyboxOld = RenderSettings.skybox;

            if(!Strings.Holder)
            {
                Log("Need to initialize strings holder");
                Strings.LoadHolder();
            }

            LoadPrefs();
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
                    EndEditingViewpoint(selectedAvatar, true);
                if(_editingScale)
                    EndScalingAvatar(selectedAvatar, true);

                SavePrefs();
            }
            else if(mode == PlayModeStateChange.EnteredEditMode)
            {
                LoadPrefs();
            }
            else if(mode == PlayModeStateChange.EnteredPlayMode)
            {
                _editingView = false;

                LoadPrefs();
                _emptyTexture = new Texture2D(2, 2);
                cameraOverrideTexture = new Texture2D(2, 2);
                _thumbsSkyboxOld = RenderSettings.skybox;

                if(hideAvatarsOnStart)
                {
                    var pm = FindObjectOfType<RuntimeBlueprintCreation>();
                    if(pm != null)
                    {
                        var desc = pm.pipelineManager.GetComponent<VRC_AvatarDescriptor>();
                        var av = GameObject.FindObjectsOfType<VRC_AvatarDescriptor>();
                        if(desc != null)
                        {
                            for(int i = 0; i < av.Length; i++)
                            {
                                if(av[i] != desc)
                                    av[i].transform.root.gameObject.SetActive(false);
                            }
                        }
                    }
                }

            }
        }

        void HandleSelectionChanged()
        {
            if(_useSceneSelectionAvatar)
                SelectAvatarFromScene();
            _PumkinsAvatarToolsWindow.RequestRepaint(this);
        }

        //[MenuItem("Tools/Pumkin/Avatar Tools")] //_PumkinsAvatarToolsWindow.cs is responsible for calling this, for now
        public static void ShowWindow()
        {
            //Show existing window instance. If one doesn't exist, make one.
            EditorWindow editorWindow = EditorWindow.GetWindow(typeof(PumkinsAvatarTools));
            editorWindow.autoRepaintOnSceneChange = true;

            editorWindow.Show();
            editorWindow.titleContent = new GUIContent(Strings.Main.WindowName);

            _DependencyChecker.Check();
        }

        public void OnGUI()
        {
            SerializedScript.Update();

            int tempSize = Styles.Label_mainTitle.fontSize + 6;
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField(Strings.Main.Title, Styles.Label_mainTitle, GUILayout.MinHeight(tempSize));

                EditorGUIUtility.SetIconSize(new Vector2(tempSize - 3, tempSize - 3));

                if(GUILayout.Button(Icons.Star, "IconButton", GUILayout.MaxWidth(tempSize + 3)))
                {
                    _openedInfo = !_openedInfo;
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField(Strings.Credits.Version);

            if(_openedInfo) //Credits Screen
            {
                EditorGUILayout.Space();
                GUILayout.BeginVertical();                                

                GUILayout.Label(Strings.Credits.RedundantStrings);
                GUILayout.Label(Strings.Credits.JsonDotNetCredit);

                EditorGUILayout.Space();

                GUILayout.Label(Strings.Credits.AddMoreStuff);

                GUILayout.BeginHorizontal();

                GUILayout.Label(Strings.Credits.PokeOnDiscord);

                GUILayout.EndHorizontal();
                GUILayout.EndVertical();

                EditorGUILayout.Space();
                GUILayout.Label(Strings.Misc.SuperExperimental + ':');
                if(GUILayout.Button(Strings.Buttons.OpenPoseEditor))
                {
                    PumkinsPoseEditor.ShowWindow();
                }

                //EditorGUILayout.Space();

                //verboseLoggingEnabled = EditorGUILayout.Toggle("Enable verbose logging", verboseLoggingEnabled);

                GUILayout.FlexibleSpace();

                if(GUILayout.Button(Strings.Misc.uwu, "IconButton", GUILayout.ExpandWidth(false)))
                {
                    //if(Strings.Language != "uwu")
                    //    Strings.language = Strings.DictionaryLanguage.uwu;
                    //else
                    //    Strings.Language = Strings.DictionaryLanguage.English;
                }
            }
            else
            {
                EditorGUIUtility.SetIconSize(new Vector2(15, 15));

                EditorGUILayout.Space();
                
                selectedAvatar = (GameObject)EditorGUILayout.ObjectField(Strings.Main.Avatar, selectedAvatar, typeof(GameObject), true);                

                if(_useSceneSelectionAvatar)
                {
                    if(Selection.activeObject != selectedAvatar)
                        SelectAvatarFromScene();
                }

                if(GUILayout.Button(Strings.Buttons.SelectFromScene))
                {
                    if(Selection.activeObject != null)
                    {
                        SelectAvatarFromScene();
                    }
                }

                _useSceneSelectionAvatar = GUILayout.Toggle(_useSceneSelectionAvatar, Strings.Main.UseSceneSelection);

                EditorGUILayout.Space();

                Helpers.DrawGuiLine();

                _mainScroll = EditorGUILayout.BeginScrollView(_mainScroll);
                {

                    //Tools menu
                    if(_tools_expand = GUILayout.Toggle(_tools_expand, Strings.Main.Tools, Styles.Foldout_title))
                    {
                        EditorGUI.BeginDisabledGroup(selectedAvatar == null);
                        {
                            EditorGUILayout.Space();

                            if(_tools_avatar_expand = GUILayout.Toggle(_tools_avatar_expand, Strings.Main.Avatar, Styles.Foldout))
                            {
                                GUILayout.BeginHorizontal(); //Row
                                {
                                    GUILayout.BeginVertical(GUILayout.ExpandWidth(true)); //Left Column
                                    {
                                        if(GUILayout.Button(Strings.Tools.FillVisemes))
                                            DoActionButton(ToolMenuActions.FillVisemes);
                                        if(GUILayout.Button(Strings.Tools.RevertBlendshapes))
                                            DoActionButton(ToolMenuActions.RevertBlendshapes);
                                        if(GUILayout.Button(Strings.Tools.ResetPose))
                                            DoActionButton(ToolMenuActions.ResetPose);
                                    }
                                    GUILayout.EndVertical();

                                    GUILayout.BeginVertical(GUILayout.ExpandWidth(true)); //Right Column
                                    {
                                        EditorGUI.BeginDisabledGroup(DrawingHandlesGUI);
                                        {
                                            if(GUILayout.Button(Strings.Tools.EditViewpoint))
                                                DoActionButton(ToolMenuActions.EditViewpoint);
                                        }
                                        EditorGUI.EndDisabledGroup();

                                        if(GUILayout.Button(Strings.Tools.ZeroBlendshapes))
                                            DoActionButton(ToolMenuActions.ZeroBlendshapes);

                                        if(GUILayout.Button(Strings.Tools.ResetToTPose))
                                            DoActionButton(ToolMenuActions.SetTPose);
                                        EditorGUI.BeginDisabledGroup(DrawingHandlesGUI);
                                        {
                                            if(GUILayout.Button(Strings.Tools.EditScale))
                                                DoActionButton(ToolMenuActions.EditScale);
                                        }
                                        EditorGUI.EndDisabledGroup();
                                    }
                                    GUILayout.EndVertical();
                                }
                                GUILayout.EndHorizontal();
                            }

                            EditorGUILayout.Space();

                            if(_tools_removeAll_expand = GUILayout.Toggle(_tools_removeAll_expand, Strings.Main.RemoveAll, Styles.Foldout))
                            {

                                EditorGUILayout.BeginHorizontal();

                                EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true)); //Left Column
#if !BONES
                            EditorGUI.BeginDisabledGroup(true);
#endif
                                if(GUILayout.Button(new GUIContent(Strings.Copier.DynamicBones, Icons.BoneIcon)))
                                    DoActionButton(ToolMenuActions.RemoveDynamicBones);
#if !BONES
                            EditorGUI.EndDisabledGroup();
#endif
                                if(GUILayout.Button(new GUIContent(Strings.Copier.ParticleSystems, Icons.ParticleSystem)))
                                    DoActionButton(ToolMenuActions.RemoveParticleSystems);
                                if(GUILayout.Button(new GUIContent(Strings.Copier.Lights, Icons.Light)))
                                    DoActionButton(ToolMenuActions.RemoveLights);
                                if(GUILayout.Button(new GUIContent(Strings.Copier.Joints, Icons.Joint)))
                                    DoActionButton(ToolMenuActions.RemoveJoints);
                                if(GUILayout.Button(new GUIContent(Strings.Copier.Animators_inChildren, Icons.Animator)))
                                    DoActionButton(ToolMenuActions.RemoveAnimatorsInChildren);
                                if(GUILayout.Button(new GUIContent(Strings.Copier.Colliders, Icons.ColliderBox)))
                                    DoActionButton(ToolMenuActions.RemoveColliders);

                                EditorGUILayout.EndVertical();

#if !BONES
                            EditorGUI.BeginDisabledGroup(true);
#endif
                                EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true)); //Right Column
                                if(GUILayout.Button(new GUIContent(Strings.Copier.DynamicBones_colliders, Icons.BoneColliderIcon)))
                                    DoActionButton(ToolMenuActions.RemoveDynamicBoneColliders);
#if !BONES
                            EditorGUI.EndDisabledGroup();
#endif
                                if(GUILayout.Button(new GUIContent(Strings.Copier.TrailRenderers, Icons.TrailRenderer)))
                                    DoActionButton(ToolMenuActions.RemoveTrailRenderers);
                                if(GUILayout.Button(new GUIContent(Strings.Copier.AudioSources, Icons.AudioSource)))
                                    DoActionButton(ToolMenuActions.RemoveAudioSources);
                                if(GUILayout.Button(new GUIContent(Strings.Copier.RigidBodies, Icons.RigidBody)))
                                    DoActionButton(ToolMenuActions.RemoveRigidBodies);
                                if(GUILayout.Button(new GUIContent(Strings.Copier.MeshRenderers, Icons.MeshRenderer)))
                                    DoActionButton(ToolMenuActions.RemoveMeshRenderers);
                                if(GUILayout.Button(new GUIContent(Strings.Copier.EmptyGameObjects, Icons.Prefab)))
                                    DoActionButton(ToolMenuActions.RemoveEmptyGameObjects);

                                EditorGUILayout.EndVertical();

                                EditorGUILayout.EndHorizontal();
                            }
                            EditorGUI.EndDisabledGroup();
                            EditorGUILayout.Space();
                        }
                    }

                    Helpers.DrawGuiLine();

                    //Component Copier menu
                    if(_copier_expand = GUILayout.Toggle(_copier_expand, Strings.Main.Copier, Styles.Foldout_title))
                    {
                        EditorGUILayout.Space();
                        copierSelectedFrom = (GameObject)EditorGUILayout.ObjectField(Strings.Copier.CopyFrom, copierSelectedFrom, typeof(GameObject), true);

                        EditorGUILayout.BeginHorizontal();
                        if(GUILayout.Button(Strings.Buttons.SelectFromScene))
                        {
                            if(Selection.activeGameObject != null)
                                copierSelectedFrom = Selection.activeGameObject.transform.root.gameObject;
                        }
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.Space();

                        EditorGUI.BeginDisabledGroup(copierSelectedFrom == null || selectedAvatar == null);
                        {
                            //Transforms menu
                            EditorGUILayout.BeginHorizontal();
                            _copier_expand_transforms = GUILayout.Toggle(_copier_expand_transforms, Icons.Transform, "Foldout", GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(true), GUILayout.MaxWidth(30), GUILayout.MaxHeight(10));
                            bCopier_transforms_copy = GUILayout.Toggle(bCopier_transforms_copy, Strings.Copier.Transforms, GUILayout.ExpandWidth(false), GUILayout.MinWidth(20));
                            EditorGUILayout.EndHorizontal();

                            if(_copier_expand_transforms)
                            {
                                EditorGUI.BeginDisabledGroup(!bCopier_transforms_copy);
                                EditorGUILayout.Space();

                                bCopier_transforms_copyPosition = EditorGUILayout.Toggle(Strings.Copier.Transforms_position, bCopier_transforms_copyPosition, GUILayout.ExpandWidth(false));
                                bCopier_transforms_copyRotation = EditorGUILayout.Toggle(Strings.Copier.Transforms_rotation, bCopier_transforms_copyRotation, GUILayout.ExpandWidth(false));
                                bCopier_transforms_copyScale = EditorGUILayout.Toggle(Strings.Copier.Transforms_scale, bCopier_transforms_copyScale);

                                bCopier_transforms_copyAvatarScale = EditorGUILayout.Toggle(Strings.Copier.Transforms_avatarScale, bCopier_transforms_copyAvatarScale);

                                EditorGUILayout.Space();
                                EditorGUI.EndDisabledGroup();
                            }

                            //DynamicBones menu
#if !BONES && !OLD_BONES
                        EditorGUI.BeginDisabledGroup(true);
                        {
#endif
                            EditorGUILayout.BeginHorizontal();
                            _copier_expand_dynamicBones = GUILayout.Toggle(_copier_expand_dynamicBones, Icons.BoneIcon, "Foldout", GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(true), GUILayout.MaxWidth(30), GUILayout.MaxHeight(10));
#if !BONES && !OLD_BONES
                        }
                        bCopier_dynamicBones_copy = GUILayout.Toggle(false, Strings.Copier.DynamicBones + " " + Strings.Warning.NotFound, GUILayout.ExpandWidth(false), GUILayout.MinWidth(20));
#endif
#if BONES || OLD_BONES
                            bCopier_dynamicBones_copy = GUILayout.Toggle(bCopier_dynamicBones_copy, Strings.Copier.DynamicBones, GUILayout.ExpandWidth(false), GUILayout.MinWidth(20));
#endif
                            EditorGUILayout.EndHorizontal();

                            if(_copier_expand_dynamicBones)
                            {
                                EditorGUI.BeginDisabledGroup(!bCopier_dynamicBones_copy);
                                EditorGUILayout.Space();

                                bCopier_dynamicBones_copySettings = EditorGUILayout.Toggle(Strings.Copier.CopySettings, bCopier_dynamicBones_copySettings, GUILayout.ExpandWidth(false));
                                bCopier_dynamicBones_createMissing = EditorGUILayout.Toggle(Strings.Copier.DynamicBones_createMissing, bCopier_dynamicBones_createMissing, GUILayout.ExpandWidth(false));
                                bCopier_dynamicBones_removeOldBones = EditorGUILayout.Toggle(Strings.Copier.DynamicBones_removeOldBones, bCopier_dynamicBones_removeOldBones, GUILayout.ExpandWidth(false));
                                EditorGUILayout.Space();
                                bCopier_dynamicBones_copyColliders = EditorGUILayout.Toggle(Strings.Copier.DynamicBones_colliders, bCopier_dynamicBones_copyColliders, GUILayout.ExpandWidth(false));
                                bCopier_dynamicBones_removeOldColliders = EditorGUILayout.Toggle(Strings.Copier.DynamicBones_removeOldColliders, bCopier_dynamicBones_removeOldColliders, GUILayout.ExpandWidth(false));
                                bCopier_dynamicBones_createObjectsColliders = EditorGUILayout.Toggle(Strings.Copier.CopyColliderObjects, bCopier_dynamicBones_createObjectsColliders, GUILayout.ExpandWidth(false));

                                EditorGUILayout.Space();
                                EditorGUI.EndDisabledGroup();
                            }
#if !BONES && !OLD_BONES
                        EditorGUI.EndDisabledGroup();
#endif

                            //AvatarDescriptor menu
                            EditorGUILayout.BeginHorizontal();
                            _copier_expand_avatarDescriptor = GUILayout.Toggle(_copier_expand_avatarDescriptor, Icons.Avatar, "Foldout", GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(true), GUILayout.MaxWidth(30), GUILayout.MaxHeight(10));
                            bCopier_descriptor_copy = GUILayout.Toggle(bCopier_descriptor_copy, Strings.Copier.Descriptor, GUILayout.ExpandWidth(false), GUILayout.MinWidth(20));
                            EditorGUILayout.EndHorizontal();

                            if(_copier_expand_avatarDescriptor)
                            {
                                EditorGUI.BeginDisabledGroup(!bCopier_descriptor_copy);
                                EditorGUILayout.Space();

                                bCopier_descriptor_copySettings = EditorGUILayout.Toggle(Strings.Copier.CopySettings, bCopier_descriptor_copySettings, GUILayout.ExpandWidth(false));
                                bCopier_descriptor_copyViewpoint = EditorGUILayout.Toggle(Strings.Copier.Descriptor_copyViewpoint, bCopier_descriptor_copyViewpoint, GUILayout.ExpandWidth(false));
                                bCopier_descriptor_copyPipelineId = EditorGUILayout.Toggle(Strings.Copier.Descriptor_pipelineId, bCopier_descriptor_copyPipelineId, GUILayout.ExpandWidth(false));
                                bCopier_descriptor_copyAnimationOverrides = EditorGUILayout.Toggle(Strings.Copier.Descriptor_animationOverrides, bCopier_descriptor_copyAnimationOverrides, GUILayout.ExpandWidth(false));

                                EditorGUILayout.Space();
                                EditorGUI.EndDisabledGroup();
                            }

                            //SkinnedMeshRenderer menu
                            EditorGUILayout.BeginHorizontal();
                            _copier_expand_skinnedMeshRenderer = GUILayout.Toggle(_copier_expand_skinnedMeshRenderer, Icons.SkinnedMeshRenderer, "Foldout", GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(true), GUILayout.MaxWidth(30), GUILayout.MaxHeight(10));
                            bCopier_skinMeshRender_copy = GUILayout.Toggle(bCopier_skinMeshRender_copy, Strings.Copier.SkinMeshRender, GUILayout.ExpandWidth(false), GUILayout.MinWidth(20));
                            EditorGUILayout.EndHorizontal();

                            if(_copier_expand_skinnedMeshRenderer)
                            {
                                EditorGUI.BeginDisabledGroup(!bCopier_skinMeshRender_copy);
                                EditorGUILayout.Space();

                                bCopier_skinMeshRender_copySettings = EditorGUILayout.Toggle(Strings.Copier.CopySettings, bCopier_skinMeshRender_copySettings, GUILayout.ExpandWidth(false));
                                bCopier_skinMeshRender_copyMaterials = EditorGUILayout.Toggle(Strings.Copier.SkinMeshRender_materials, bCopier_skinMeshRender_copyMaterials, GUILayout.ExpandWidth(false));
                                bCopier_skinMeshRender_copyBlendShapeValues = EditorGUILayout.Toggle(Strings.Copier.SkinMeshRender_blendShapeValues, bCopier_skinMeshRender_copyBlendShapeValues, GUILayout.ExpandWidth(false));

                                EditorGUILayout.Space();
                                EditorGUI.EndDisabledGroup();
                            }

                            //MeshRenderers menu
                            EditorGUILayout.BeginHorizontal();
                            _copier_expand_meshRenderers = GUILayout.Toggle(_copier_expand_meshRenderers, Icons.MeshRenderer, "Foldout", GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(true), GUILayout.MaxWidth(30), GUILayout.MaxHeight(10));
                            bCopier_meshRenderers_copy = GUILayout.Toggle(bCopier_meshRenderers_copy, Strings.Copier.MeshRenderers, GUILayout.ExpandWidth(false), GUILayout.MinWidth(20));
                            EditorGUILayout.EndHorizontal();

                            if(_copier_expand_meshRenderers)
                            {
                                EditorGUI.BeginDisabledGroup(!bCopier_meshRenderers_copy);
                                EditorGUILayout.Space();

                                bCopier_meshRenderers_copySettings = EditorGUILayout.Toggle(Strings.Copier.CopySettings, bCopier_meshRenderers_copySettings);
                                bCopier_meshRenderers_createMissing = EditorGUILayout.Toggle(Strings.Copier.CreateMissing, bCopier_meshRenderers_createMissing);
                                bCopier_meshRenderers_createObjects = EditorGUILayout.Toggle(Strings.Copier.CopyGameObjects, bCopier_meshRenderers_createObjects);

                                EditorGUILayout.Space();
                                EditorGUI.EndDisabledGroup();
                            }

                            //Particles menu
                            EditorGUILayout.BeginHorizontal();
                            _copier_expand_particleSystems = GUILayout.Toggle(_copier_expand_particleSystems, Icons.ParticleSystem, "Foldout", GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(true), GUILayout.MaxWidth(30), GUILayout.MaxHeight(10));
                            bCopier_particleSystems_copy = GUILayout.Toggle(bCopier_particleSystems_copy, Strings.Copier.ParticleSystems, GUILayout.ExpandWidth(false), GUILayout.MinWidth(20));
                            EditorGUILayout.EndHorizontal();

                            if(_copier_expand_particleSystems)
                            {
                                EditorGUI.BeginDisabledGroup(!bCopier_particleSystems_copy);
                                EditorGUILayout.Space();

                                bCopier_particleSystems_replace = EditorGUILayout.Toggle(Strings.Copier.ReplaceOld, bCopier_particleSystems_replace);
                                bCopier_particleSystems_createObjects = EditorGUILayout.Toggle(Strings.Copier.CopyGameObjects, bCopier_particleSystems_createObjects);


                                EditorGUILayout.Space();
                                EditorGUI.EndDisabledGroup();
                            }

                            //TrailRenderers menu
                            EditorGUILayout.BeginHorizontal();
                            _copier_expand_trailRenderers = GUILayout.Toggle(_copier_expand_trailRenderers, Icons.TrailRenderer, "Foldout", GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(true), GUILayout.MaxWidth(30), GUILayout.MaxHeight(10));
                            bCopier_trailRenderers_copy = GUILayout.Toggle(bCopier_trailRenderers_copy, Strings.Copier.TrailRenderers, GUILayout.ExpandWidth(false), GUILayout.MinWidth(20));
                            EditorGUILayout.EndHorizontal();

                            if(_copier_expand_trailRenderers)
                            {
                                EditorGUI.BeginDisabledGroup(!bCopier_trailRenderers_copy);
                                EditorGUILayout.Space();

                                bCopier_trailRenderers_copySettings = EditorGUILayout.Toggle(Strings.Copier.CopySettings, bCopier_trailRenderers_copySettings);
                                bCopier_trailRenderers_createMissing = EditorGUILayout.Toggle(Strings.Copier.CreateMissing, bCopier_trailRenderers_createMissing);
                                bCopier_trailRenderers_createObjects = EditorGUILayout.Toggle(Strings.Copier.CopyGameObjects, bCopier_trailRenderers_createObjects);

                                EditorGUILayout.Space();
                                EditorGUI.EndDisabledGroup();
                            }

                            //AudioSources menu
                            EditorGUILayout.BeginHorizontal();
                            _copier_expand_audioSources = GUILayout.Toggle(_copier_expand_audioSources, Icons.AudioSource, "Foldout", GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(true), GUILayout.MaxWidth(30), GUILayout.MaxHeight(10));
                            bCopier_audioSources_copy = GUILayout.Toggle(bCopier_audioSources_copy, Strings.Copier.AudioSources, GUILayout.ExpandWidth(false), GUILayout.MinWidth(20));
                            EditorGUILayout.EndHorizontal();

                            if(_copier_expand_audioSources)
                            {
                                EditorGUI.BeginDisabledGroup(!bCopier_audioSources_copy);
                                EditorGUILayout.Space();

                                bCopier_audioSources_copySettings = EditorGUILayout.Toggle(Strings.Copier.CopySettings, bCopier_audioSources_copySettings);
                                bCopier_audioSources_createMissing = EditorGUILayout.Toggle(Strings.Copier.CreateMissing, bCopier_audioSources_createMissing);
                                bCopier_audioSources_createObjects = EditorGUILayout.Toggle(Strings.Copier.CopyGameObjects, bCopier_audioSources_createObjects);

                                EditorGUILayout.Space();
                                EditorGUI.EndDisabledGroup();
                            }

                            //Lights menu
                            EditorGUILayout.BeginHorizontal();
                            _copier_expand_lights = GUILayout.Toggle(_copier_expand_lights, Icons.Light, "Foldout", GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(true), GUILayout.MaxWidth(30), GUILayout.MaxHeight(10));
                            bCopier_lights_copy = GUILayout.Toggle(bCopier_lights_copy, Strings.Copier.Lights, GUILayout.ExpandWidth(false), GUILayout.MinWidth(20));
                            EditorGUILayout.EndHorizontal();

                            if(_copier_expand_lights)
                            {
                                EditorGUI.BeginDisabledGroup(!bCopier_lights_copy);
                                EditorGUILayout.Space();

                                bCopier_lights_copySettings = EditorGUILayout.Toggle(Strings.Copier.CopySettings, bCopier_lights_copySettings);
                                bCopier_lights_createMissing = EditorGUILayout.Toggle(Strings.Copier.CreateMissing, bCopier_lights_createMissing);
                                bCopier_lights_createObjects = EditorGUILayout.Toggle(Strings.Copier.CopyGameObjects, bCopier_lights_createObjects);

                                EditorGUILayout.Space();
                                EditorGUI.EndDisabledGroup();
                            }

                            //RidigBodies menu
                            EditorGUILayout.BeginHorizontal();
                            _copier_expand_rigidBodies = GUILayout.Toggle(_copier_expand_rigidBodies, Icons.RigidBody, "Foldout", GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(true), GUILayout.MaxWidth(30), GUILayout.MaxHeight(10));
                            bCopier_rigidBodies_copy = GUILayout.Toggle(bCopier_rigidBodies_copy, Strings.Copier.RigidBodies, GUILayout.ExpandWidth(false), GUILayout.MinWidth(20));
                            EditorGUILayout.EndHorizontal();

                            if(_copier_expand_rigidBodies)
                            {
                                EditorGUI.BeginDisabledGroup(!bCopier_rigidBodies_copy);
                                EditorGUILayout.Space();

                                bCopier_rigidBodies_copySettings = EditorGUILayout.Toggle(Strings.Copier.CopySettings, bCopier_rigidBodies_copySettings);
                                bCopier_rigidBodies_createMissing = EditorGUILayout.Toggle(Strings.Copier.CreateMissing, bCopier_rigidBodies_createMissing);
                                bCopier_rigidBodies_createObjects = EditorGUILayout.Toggle(Strings.Copier.CopyGameObjects, bCopier_rigidBodies_createObjects);

                                EditorGUILayout.Space();
                                EditorGUI.EndDisabledGroup();
                            }

                            //Collider menu
                            EditorGUILayout.BeginHorizontal();
                            _copier_expand_colliders = GUILayout.Toggle(_copier_expand_colliders, Icons.ColliderBox, "Foldout", GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(true), GUILayout.MaxWidth(30), GUILayout.MaxHeight(10));
                            bCopier_colliders_copy = GUILayout.Toggle(bCopier_colliders_copy, Strings.Copier.Colliders, GUILayout.ExpandWidth(false), GUILayout.MinWidth(20));
                            EditorGUILayout.EndHorizontal();

                            if(_copier_expand_colliders)
                            {
                                EditorGUI.BeginDisabledGroup(!bCopier_colliders_copy);
                                EditorGUILayout.Space();

                                bCopier_colliders_copyBox = EditorGUILayout.Toggle(Strings.Copier.Colliders_box, bCopier_colliders_copyBox, GUILayout.ExpandWidth(false));
                                bCopier_colliders_copyCapsule = EditorGUILayout.Toggle(Strings.Copier.Colliders_capsule, bCopier_colliders_copyCapsule, GUILayout.ExpandWidth(false));
                                bCopier_colliders_copySphere = EditorGUILayout.Toggle(Strings.Copier.Colliders_sphere, bCopier_colliders_copySphere, GUILayout.ExpandWidth(false));
                                bCopier_colliders_copyMesh = EditorGUILayout.Toggle(Strings.Copier.Colliders_mesh, bCopier_colliders_copyMesh, GUILayout.ExpandWidth(false));

                                bCopier_colliders_removeOld = EditorGUILayout.Toggle(Strings.Copier.Colliders_removeOld, bCopier_colliders_removeOld, GUILayout.ExpandWidth(false));
                                bCopier_colliders_createObjects = EditorGUILayout.Toggle(Strings.Copier.CopyGameObjects, bCopier_colliders_createObjects, GUILayout.ExpandWidth(false));

                                EditorGUILayout.Space();
                                EditorGUI.EndDisabledGroup();
                            }

                            //Animators menu
                            EditorGUILayout.BeginHorizontal();
                            _copier_expand_animators = GUILayout.Toggle(_copier_expand_animators, Icons.Animator, "Foldout", GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(true), GUILayout.MaxWidth(30), GUILayout.MaxHeight(10));
                            bCopier_animators_copy = GUILayout.Toggle(bCopier_animators_copy, Strings.Copier.Animators, GUILayout.ExpandWidth(false), GUILayout.MinWidth(20));
                            EditorGUILayout.EndHorizontal();

                            if(_copier_expand_animators)
                            {
                                EditorGUI.BeginDisabledGroup(!bCopier_animators_copy);
                                EditorGUILayout.Space();

                                bCopier_animators_copySettings = EditorGUILayout.Toggle(Strings.Copier.CopySettings, bCopier_animators_copySettings);
                                bCopier_animators_createMissing = EditorGUILayout.Toggle(Strings.Copier.CreateMissing, bCopier_animators_createMissing);
                                bCopier_animators_createObjects = EditorGUILayout.Toggle(Strings.Copier.CopyGameObjects, bCopier_animators_createObjects);
                                bCopier_animators_copyMainAnimator = EditorGUILayout.Toggle(Strings.Copier.CopyMainAnimator, bCopier_animators_copyMainAnimator);

                                EditorGUILayout.Space();
                                EditorGUI.EndDisabledGroup();
                            }

                            EditorGUILayout.Space();

                            EditorGUI.BeginChangeCheck();
                            {
                                Helpers.DrawPropertyArrayScrolling(SerializedIgnoreArray, Strings.Copier.Exclusions, ref _copierIgnoreArray_expand, ref _copierIgnoreArrayScroll, 0, 100);                                
                            }
                            if(EditorGUI.EndChangeCheck())
                            {
                                RefreshIgnoreArray();                            
                            }

                            if(_copierIgnoreArray_expand)
                            {
                                EditorGUILayout.Space();
                                bCopier_ignoreArray_includeChildren = EditorGUILayout.Toggle(Strings.Copier.IncludeChildren, bCopier_ignoreArray_includeChildren);
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
                                    if(GUILayout.Button(Strings.Buttons.SelectNone, GUILayout.MinWidth(100)))
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
                                    if(GUILayout.Button(Strings.Buttons.SelectAll, GUILayout.MinWidth(100)))
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
                                    if(GUILayout.Button(Strings.Buttons.CopySelected, Styles.BigButton))
                                    {
                                        string log = "";
                                        if(copierSelectedFrom == null)
                                        {
                                            log += Strings.Log.CopyFromInvalid;
                                            Log(log, LogType.Warning);
                                        }
                                        else
                                        {
                                            //Cancel Checks
                                            if(copierSelectedFrom == selectedAvatar)
                                            {
                                                Log(log + Strings.Log.CantCopyToSelf, LogType.Warning);
                                                return;
                                            }

                                            //Figure out how to prevent undo from adding multiple copies of the same component on
                                            /*//Record Undo
                                            Undo.RegisterFullObjectHierarchyUndo(selectedAvatar, "Copy Components");
                                            if(selectedAvatar.gameObject.scene.name == null) //In case it's a prefab instance, which it probably is
                                                PrefabUtility.RecordPrefabInstancePropertyModifications(selectedAvatar);*/

                                            RefreshIgnoreArray();

                                            CopyComponents(copierSelectedFrom, selectedAvatar);

                                            EditorUtility.SetDirty(selectedAvatar);
                                            EditorSceneManager.MarkSceneDirty(selectedAvatar.scene);

                                            avatarInfo = AvatarInfo.GetInfo(selectedAvatar, out _avatarInfoString);

                                            log += Strings.Log.Done;
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

                    Helpers.DrawGuiLine();

                    //Avatar Info menu
                    if(_avatarInfo_expand = GUILayout.Toggle(_avatarInfo_expand, Strings.Main.AvatarInfo, Styles.Foldout_title))
                    {
                        if(selectedAvatar == null)
                        {
                            if(avatarInfo != null)
                            {
                                avatarInfo = null;
                                _avatarInfoString = Strings.AvatarInfo.SelectAvatarFirst;
                            }
                        }
                        else
                        {
                            if(avatarInfo == null)
                            {
                                avatarInfo = AvatarInfo.GetInfo(selectedAvatar, out _avatarInfoString);
                            }
                        }

                        EditorGUILayout.SelectableLabel(_avatarInfoString, Styles.HelpBox, GUILayout.MinHeight(240));

                        EditorGUI.BeginDisabledGroup(selectedAvatar == null);
                        {
                            EditorGUILayout.BeginHorizontal();
                            {
                                if(GUILayout.Button(Strings.Buttons.Copy))
                                {
                                    EditorGUIUtility.systemCopyBuffer = _avatarInfoString;
                                }
                                if(GUILayout.Button(Strings.Buttons.Refresh))
                                {
                                    avatarInfo = AvatarInfo.GetInfo(selectedAvatar, out _avatarInfoString);
                                }
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                        EditorGUI.EndDisabledGroup();
                    }

                    Helpers.DrawGuiLine();

                    //Thumbnails menu                
                    if(_thumbnails_expand = GUILayout.Toggle(_thumbnails_expand, Strings.Main.Thumbnails, Styles.Foldout_title))
                    {
                        if(!EditorApplication.isPlaying)
                        {
                            EditorGUILayout.SelectableLabel(Strings.Thumbnails.StartUploadingFirst, Styles.HelpBox_OneLine);
                            hideAvatarsOnStart = GUILayout.Toggle(hideAvatarsOnStart, Strings.Thumbnails.HideOtherAvatars);
                        }

                        EditorGUILayout.Space();

                        //Camera Override Image
                        EditorGUI.BeginDisabledGroup(!EditorApplication.isPlaying);
                        {
                            GUILayout.BeginHorizontal();
                            {
                                EditorGUILayout.LabelField(Strings.Thumbnails.OverlayCameraImage, GUILayout.MaxWidth(100));
                                EditorGUILayout.SelectableLabel(_overlayPathText, Styles.HelpBox_OneLine);

                                EditorGUI.BeginChangeCheck();
                                {
                                    if(GUILayout.Button(Strings.Buttons.Browse, GUILayout.MaxWidth(60)) && VRCCam)
                                    {
                                        cameraOverrideTexture = Helpers.OpenImageTexture(ref _lastOpenFilePath);
                                        _overlayPathText = _lastOpenFilePath;
                                    }
                                    if(GUILayout.Button("X", GUILayout.MaxWidth(25)))
                                    {
                                        if(!CameraOverlayRawImage)
                                            CameraOverlayRawImage.color = Color.clear;
                                        _overlayPathText = null;
                                    }
                                }
                                if(EditorGUI.EndChangeCheck() && cameraOverrideTexture != null)
                                {
                                    _PumkinsAvatarToolsWindow.RequestRepaint(this);
                                    if(cameraOverrideTexture.name != _emptyTexture.name)
                                    {
                                        if(VRCCam)
                                        {
                                            CameraOverlayRawImage.texture = cameraOverrideTexture;
                                            CameraOverlayRawImage.color = Color.white;
                                            cameraOverrideTexture.name = _emptyTexture.name;
                                        }
                                    }
                                    else
                                    {
                                        if(CameraOverlay != null)
                                            DestroyImmediate(CameraOverlay);
                                    }
                                }                                
                            }
                            GUILayout.EndHorizontal();
                        }
                        EditorGUI.EndDisabledGroup();

                        //Background Type
                        EditorGUI.BeginDisabledGroup(!EditorApplication.isPlaying);
                        {
                            EditorGUI.BeginChangeCheck();
                            {
                                camBgOverrideType = (CameraBackgroundOverrideType)EditorGUILayout.EnumPopup(Strings.Thumbnails.BackgroundType, camBgOverrideType);
                            }
                            if(EditorGUI.EndChangeCheck())
                            {
                                if(VRCCam)
                                {
                                    vrcCamSetBgColor = false;
                                    vrcCamSetSkybox = false;
                                    vrcCamSetBackground = false;

                                    VRCCam.clearFlags = _vrcCamClearFlagsOld;
                                    VRCCam.backgroundColor = _vrcCamColorOld;
                                    RenderSettings.skybox = _thumbsSkyboxOld;
                                    if(CameraBackgroundRawImage)
                                        CameraBackgroundRawImage.enabled = false;

                                    switch(camBgOverrideType)
                                    {
                                        case CameraBackgroundOverrideType.Color:
                                            vrcCamSetBgColor = true;
                                            VRCCam.clearFlags = CameraClearFlags.SolidColor;
                                            VRCCam.backgroundColor = _vrcCamBgColor;
                                            break;
                                        case CameraBackgroundOverrideType.Image:
                                            vrcCamSetBackground = true;
                                            if(CameraBackgroundRawImage)
                                                CameraBackgroundRawImage.enabled = true;
                                            break;
                                        case CameraBackgroundOverrideType.Material:
                                            vrcCamSetSkybox = true;
                                            VRCCam.clearFlags = CameraClearFlags.Skybox;
                                            break;
                                        case CameraBackgroundOverrideType.None:
                                        default:
                                            break;
                                    }
                                }
                                else
                                {
                                    Log(Strings.Warning.VRCCamNotFound, LogType.Warning);
                                }
                            }

                            if(VRCCam)
                            {
                                if(vrcCamSetBgColor)
                                {
                                    EditorGUI.BeginChangeCheck();
                                    {
                                        _vrcCamBgColor = EditorGUILayout.ColorField(Strings.Thumbnails.BackgroundType_Color, _vrcCamBgColor);
                                    }
                                    if(EditorGUI.EndChangeCheck())
                                    {
                                        if(VRCCam)
                                            VRCCam.backgroundColor = _vrcCamBgColor;
                                    }
                                }
                                else if(vrcCamSetSkybox)
                                {
                                    EditorGUI.BeginChangeCheck();
                                    {
                                        RenderSettings.skybox = (Material)EditorGUILayout.ObjectField(Strings.Thumbnails.BackgroundType_Material, RenderSettings.skybox, typeof(Material), false);
                                    }
                                    if(EditorGUI.EndChangeCheck())
                                    {
                                        if(VRCCam)
                                            VRCCam.backgroundColor = _vrcCamBgColor;
                                    }
                                }
                                else if(vrcCamSetBackground)
                                {
                                    GUILayout.BeginHorizontal();
                                    {
                                        EditorGUILayout.LabelField(Strings.Thumbnails.BackgroundType_Image, GUILayout.MaxWidth(100));
                                        EditorGUILayout.SelectableLabel(_backgroundPathText, Styles.HelpBox, GUILayout.MaxHeight(18), GUILayout.ExpandHeight(false));

                                        EditorGUI.BeginChangeCheck();
                                        {
                                            if(GUILayout.Button(Strings.Buttons.Browse, GUILayout.MaxWidth(60)))
                                            {
                                                cameraBackgroundTexture = Helpers.OpenImageTexture(ref _lastOpenFilePath);
                                                _backgroundPathText = _lastOpenFilePath;
                                            }
                                            if(GUILayout.Button("X", GUILayout.MaxWidth(25)))
                                            {
                                                if(CameraBackgroundRawImage)
                                                    CameraBackgroundRawImage.color = Color.clear;
                                                _backgroundPathText = null;
                                            }
                                        }
                                        if(EditorGUI.EndChangeCheck())
                                        {
                                            if(cameraBackgroundTexture != null)
                                            {
                                                if(cameraBackgroundTexture.name != _emptyTexture.name)
                                                {
                                                    if(VRCCam)
                                                    {
                                                        CameraBackgroundRawImage.texture = cameraBackgroundTexture;
                                                        CameraBackgroundRawImage.color = Color.white;
                                                        cameraBackgroundTexture.name = _emptyTexture.name;
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
                                                CameraBackgroundRawImage.color = Color.clear;
                                            }
                                        }
                                    }
                                    GUILayout.EndHorizontal();
                                }

                                EditorGUILayout.Space();

                                if(GUILayout.Button(Strings.Thumbnails.CenterCameraOnViewpoint, Styles.BigButton))
                                {
                                    if(VRCCam)
                                        CenterCameraOnViewpoint(selectedAvatar);
                                    else
                                        Log(Strings.Warning.VRCCamNotFound, LogType.Warning);
                                }
                            }
                        }
                        EditorGUI.EndDisabledGroup();

                        if(!EditorApplication.isPlaying)
                        {
                            if(bThumbnails_override_camera_image)
                            {
                                cameraOverrideTexture = null;
                                bThumbnails_override_camera_image = false;
                            }

                            if(CameraOverlay)
                                GameObject.DestroyImmediate(CameraOverlay);

                            if(CameraBackground)
                                GameObject.DestroyImmediate(CameraBackground);
                        }
                    }

                    Helpers.DrawGuiLine();

                    //Misc menu
                    if(_misc_expand = GUILayout.Toggle(_misc_expand, Strings.Main.Misc, Styles.Foldout_title))
                    {
                        EditorGUILayout.Space();

                        //Set language
                        var lang = Strings.GetLanguages();
                        string[] langNames;

                        if(lang == null || lang.Length == 0)
                        {
                            langNames = new string[] { "English - Default" };
                        }
                        else
                        {
                            langNames = lang.Select(o => o.ToString()).ToArray<string>();
                            _selectedLanguageIndex = Array.FindIndex(langNames, o => o == Strings.Holder.ToString());
                        }

                        EditorGUI.BeginChangeCheck();
                        {
                            _selectedLanguageIndex = EditorGUILayout.Popup(Strings.Misc.Language, _selectedLanguageIndex, langNames);
                        }
                        if(EditorGUI.EndChangeCheck() && lang.Length > 0)
                        {
                            Strings.SetLanguage(lang[_selectedLanguageIndex]);
                        }

                        EditorGUILayout.Space();

#if !BONES && !OLD_BONES
                        if(GUILayout.Button(Strings.Misc.SearchForBones, Styles.BigButton))
                        {
                            _DependencyChecker.Check();
                        }
                        EditorGUILayout.Space();
#endif
                        GUILayout.BeginHorizontal();
                        {

                            if(GUILayout.Button(new GUIContent(Strings.Buttons.OpenGithubPage, Icons.GithubIcon)))
                            {
                                Application.OpenURL(Strings.Instance.toolsPage);
                            }
                            if(GUILayout.Button(new GUIContent(Strings.Buttons.OpenHelpPage, Icons.Help)))
                            {
                                Application.OpenURL(Strings.Instance.toolsPage + "wiki");
                            }
                        }
                        GUILayout.EndHorizontal();

                        if(GUILayout.Button(new GUIContent(Strings.Buttons.JoinDiscordServer, Icons.DiscordIcon)))
                        {
                            Application.OpenURL(Strings.Instance.discordLink);
                        }
                        if(GUILayout.Button(new GUIContent(Strings.Buttons.OpenDonationPage, Icons.KofiIcon)))
                        {
                            Application.OpenURL(Strings.Instance.donationLink);
                        }
                    }

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
        
        private static void SelectAvatarFromScene()
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
                        selectedAvatar = sel;
                        avatarInfo = AvatarInfo.GetInfo(selectedAvatar, out _avatarInfoString);
                        
                    }
                    else if(!_useSceneSelectionAvatar)
                    {
                        Log(Strings.Warning.SelectSceneObject, LogType.Warning);
                    }
                }
            }
            catch(Exception e)
            {
                Debug.Log(e.Message);
            }
        }

        //Scene GUI
        void OnSceneGUI(SceneView sceneView)
        {            
            if(_editingScale)
            {
                bool propertyChanged = false;
                if(!selectedAvatar)
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
                        GUILayout.Label(Strings.Tools.EditScale);
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
                            if(GUILayout.Button(Strings.Buttons.Cancel, GUILayout.MinWidth(80)))
                            {
                                EndScalingAvatar(selectedAvatar, true);
                            }

                            if(GUILayout.Button(Strings.Buttons.Apply, GUILayout.MinWidth(80)))
                            {
                                EndScalingAvatar(selectedAvatar, false);
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
                        _avatarScaleTemp = Handles.ScaleSlider(_avatarScaleTemp, selectedAvatar.transform.position, Vector3.up, Quaternion.identity, HandleUtility.GetHandleSize(selectedAvatar.transform.position) * 2, 0.01f);
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
                if(!selectedAvatar)
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
                        GUILayout.Label(Strings.Tools.EditViewpoint);
                        GUILayout.BeginHorizontal();
                        {
                            if(GUILayout.Button(Strings.Buttons.Cancel, GUILayout.MinWidth(80)))
                            {
                                EndEditingViewpoint(selectedAvatar, true);
                            }

                            if(GUILayout.Button(Strings.Buttons.Apply, GUILayout.MinWidth(80)))
                            {
                                EndEditingViewpoint(selectedAvatar, false);
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
                
        void OnDrawGizmos()
        {            
            if(DrawingHandlesGUI)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(_viewPosTemp, 0.1f);
            }            
        }

        #endregion

        #region Main Functions

        /// <summary>
        /// Refreshes ignore list for the copier by making the transform references local to the selected avatar
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

        bool CopierHasSelections()
        {
            if(!(bCopier_animators_copy || bCopier_colliders_copy || bCopier_joints_copy || bCopier_descriptor_copy ||
              bCopier_lights_copy || bCopier_meshRenderers_copy || bCopier_particleSystems_copy || bCopier_rigidBodies_copy ||
              bCopier_trailRenderers_copy || bCopier_transforms_copy || bCopier_skinMeshRender_copy || bCopier_dynamicBones_copy ||
              bCopier_audioSources_copy))
                return false;
            return true;
        }

        private void SetAvatarScale(VRC_AvatarDescriptor desc, float newScale)
        {
            if(_editingScale)
            {
                selectedAvatar.transform.localScale = Helpers.RoundVectorValues(new Vector3(newScale, newScale, newScale), 3);
                if(_scaleViewpointDummy)
                    _viewPosTemp = _scaleViewpointDummy.position;
                else
                    EndScalingAvatar(desc.gameObject, true);
            }
            else
            {
                var tempDummy = new GameObject("_tempDummy").transform;
                tempDummy.position = desc.ViewPosition + desc.transform.root.position;
                tempDummy.parent = selectedAvatar.transform;
                desc.transform.root.localScale = Helpers.RoundVectorValues(new Vector3(newScale, newScale, newScale), 3);
                SetViewpoint(desc, tempDummy.position);
                DestroyImmediate(tempDummy.gameObject);
                Log("_Set Avatar scale to {0} and Viewpoint to {1}", LogType.Log, newScale.ToString(), desc.ViewPosition.ToString());
            }
        }

        /// <summary>
        /// Function for all the actions in the tool menu. Use this instead of calling
        /// button functions directly.
        /// </summary>        
        void DoActionButton(ToolMenuActions action)
        {
            if(selectedAvatar == null)
            {
                //Shouldn't be possible with disable group
                Log(Strings.Log.NothingSelected, LogType.Warning);
                return;
            }

            //Record Undo            
            Undo.RegisterFullObjectHierarchyUndo(selectedAvatar, "Tools menu: " + action.ToString());
            if(selectedAvatar.gameObject.scene.name == null) //In case it's a prefab instance, which it probably is
                PrefabUtility.RecordPrefabInstancePropertyModifications(selectedAvatar);

            switch(action)
            {
                case ToolMenuActions.RemoveColliders:
                    DestroyAllComponentsOfType(selectedAvatar, typeof(Collider), false, false);
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
                    ResetPose(selectedAvatar);
                    break;
                case ToolMenuActions.RevertBlendshapes:
                    ResetBlendShapes(selectedAvatar, true);
                    break;
                case ToolMenuActions.FillVisemes:
                    FillVisemes(selectedAvatar);
                    break;
                case ToolMenuActions.EditViewpoint:
                    BeginEdittingViewpoint(selectedAvatar);
                    break;
                case ToolMenuActions.ZeroBlendshapes:
                    ResetBlendShapes(selectedAvatar, false);
                    break;
                case ToolMenuActions.SetTPose:
                    PumkinsPoseEditor.SetTPose(selectedAvatar);
                    break;
                case ToolMenuActions.RemoveEmptyGameObjects:
                    DestroyEmptyGameObjects(selectedAvatar);
                    break;
                case ToolMenuActions.RemoveParticleSystems:
                    DestroyAllComponentsOfType(selectedAvatar, typeof(ParticleSystem), false, false);
                    DestroyAllComponentsOfType(selectedAvatar, typeof(ParticleSystemRenderer), false, false);
                    break;
                case ToolMenuActions.RemoveRigidBodies:
                    DestroyAllComponentsOfType(selectedAvatar, typeof(Rigidbody), false, false);
                    break;
                case ToolMenuActions.RemoveTrailRenderers:
                    DestroyAllComponentsOfType(selectedAvatar, typeof(TrailRenderer), false, false);
                    break;
                case ToolMenuActions.RemoveMeshRenderers:
                    DestroyAllComponentsOfType(selectedAvatar, typeof(MeshFilter), false, false);
                    DestroyAllComponentsOfType(selectedAvatar, typeof(MeshRenderer), false, false);
                    break;
                case ToolMenuActions.RemoveLights:
                    DestroyAllComponentsOfType(selectedAvatar, typeof(Light), false, false);
                    break;
                case ToolMenuActions.RemoveAnimatorsInChildren:
                    DestroyAllComponentsOfType(selectedAvatar, typeof(Animator), true, false);
                    break;
                case ToolMenuActions.RemoveAudioSources:
                    DestroyAllComponentsOfType(selectedAvatar, typeof(AudioSource), false, false);
                    DestroyAllComponentsOfType(selectedAvatar, typeof(ONSPAudioSource), false, false);
                    break;
                case ToolMenuActions.RemoveJoints:
                    DestroyAllComponentsOfType(selectedAvatar, typeof(Joint), false, false);
                    break;
                case ToolMenuActions.EditScale:
                    BeginScalingAvatar(selectedAvatar);
                    break;
                default:
                    break;
            }

            avatarInfo = AvatarInfo.GetInfo(selectedAvatar, out _avatarInfoString);

            EditorUtility.SetDirty(selectedAvatar);
            EditorSceneManager.MarkSceneDirty(selectedAvatar.scene);
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
            _viewPosTemp = _viewPosOld + selectedAvatar.transform.position;

            if(!_scaleViewpointDummy)
            {
                var g = GameObject.Find("_PumkinsViewpointDummy");
                if(g)
                    _scaleViewpointDummy = g.transform;
                else
                    _scaleViewpointDummy = new GameObject("_PumkinsViewpointDummy").transform;
            }

            _scaleViewpointDummy.position = _viewPosTemp;
            _scaleViewpointDummy.parent = selectedAvatar.transform;                 

            _editingScale = true;
            _tempToolOld = Tools.current;
            Tools.current = Tool.None;
            Selection.activeGameObject = selectedAvatar;
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

            Vector3 defaultView = new Vector3(0, 1.6f, 0.2f);
            _viewPosOld = _tempAvatarDescriptor.ViewPosition;

            if(_tempAvatarDescriptor.ViewPosition == defaultView)
            {
                var anim = selectedAvatar.GetComponent<Animator>();

                if(anim != null && anim.isHuman)
                {
                    _viewPosTemp = anim.GetBoneTransform(HumanBodyBones.Head).position;
                    float eyeHeight = anim.GetBoneTransform(HumanBodyBones.LeftEye).position.y - 0.005f;
                    _viewPosTemp.y = eyeHeight;
                    _viewPosTemp.z = defaultView.z - 0.1f;
                }
            }
            else
            {
                _viewPosTemp = _tempAvatarDescriptor.ViewPosition + avatar.transform.root.position;
            }
            _editingView = true;
            _tempToolOld = Tools.current;
            Tools.current = Tool.None;
            Selection.activeGameObject = selectedAvatar;
        }

        void SetViewpoint(VRC_AvatarDescriptor desc, Vector3 position)
        {
            if(!desc)
            {
                Log("Avatar has no Avatar Descriptor. Ignoring", LogType.Warning);
                return;
            }

            desc.ViewPosition = Helpers.RoundVectorValues(position - desc.gameObject.transform.position, 3);
        }

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
                    Log(Strings.Log.DescriptorIsNull, LogType.Error);
                    return;
                }

                _editingScale = false;
                Tools.current = _tempToolOld;
                if(!cancelled)
                {
                    SetViewpoint(_tempAvatarDescriptor, _viewPosTemp);
                    Log("_Set Avatar scale to {0} and Viewpoint to {1}", LogType.Log, avatar.transform.localScale.z.ToString(), _tempAvatarDescriptor.ViewPosition.ToString());                    
                }
                else
                {
                    _tempAvatarDescriptor.ViewPosition = _viewPosOld;
                    selectedAvatar.transform.localScale = _avatarScaleOld;                    
                    Log("_Cancelled Scale changes");                    
                }
            }
            _tempAvatarDescriptor = null;

            if(_scaleViewpointDummy)
                DestroyImmediate(_scaleViewpointDummy.gameObject);
        }        

        /// <summary>
        /// End editing Viewposition
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
                    Log(Strings.Log.DescriptorIsNull, LogType.Error);
                    return;
                }

                _editingView = false;
                Tools.current = _tempToolOld;
                if(!cancelled)
                {
                    SetViewpoint(_tempAvatarDescriptor, _viewPosTemp);                    
                    Log(Strings.Log.ViewpointApplied, LogType.Log, _tempAvatarDescriptor.ViewPosition.ToString());                    
                }
                else
                {
                    _tempAvatarDescriptor.ViewPosition = _viewPosOld;
                    Log(Strings.Log.ViewpointCancelled, LogType.Log);                    
                }
            }
            _tempAvatarDescriptor = null;
        }

        /// <summary>
        /// Fill viseme tree on avatar descriptor
        /// </summary>        
        private void FillVisemes(GameObject avatar)
        {
            string log = Strings.Log.TryFillVisemes + " - ";
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
                log += Strings.Log.NoSkinnedMeshFound;
                Log(log, LogType.Error, logFormat);
            }
            else
            {
                if(foundShape)
                {
                    d.lipSync = VRC_AvatarDescriptor.LipSyncStyle.VisemeBlendShape;
                    log += Strings.Log.Success;
                    Log(log, LogType.Log, logFormat);
                }
                else
                {
                    d.lipSync = VRC_AvatarDescriptor.LipSyncStyle.Default;
                    log += Strings.Log.MeshHasNoVisemes;
                    Log(log, LogType.Warning, logFormat);
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
                log += Strings.Log.CantCopyToSelf;
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
                if(bCopier_transforms_copyAvatarScale)
                {
                    desc = objTo.GetComponentInChildren<VRC_AvatarDescriptor>();
                    if(desc)
                    {
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
                    if(bCopier_dynamicBones_copySettings)
                        CopyAllDynamicBones(objFrom, objTo, bCopier_dynamicBones_createMissing, true);
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

                string log = String.Format(Strings.Log.CopyAttempt, aFromArr[i].GetType().ToString(), aFrom.gameObject, tTo.gameObject);

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
                        Log(log + ": " + Strings.Log.Success);
                    }
                }
                else
                {
                    Log(log + " " + Strings.Log.FailedDoesntHave, LogType.Warning, aFrom.gameObject.name.ToString(), aFrom.GetType().ToString());
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

                string log = Strings.Log.CopyAttempt;

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
                        Log(log + ": " + Strings.Log.Success, LogType.Log, lTo.GetType().ToString(), tTo.gameObject.name, aFrom.gameObject.name);
                    }
                }
                else
                {
                    Log(log + " " + Strings.Log.FailedDoesntHave, LogType.Warning, aFrom.gameObject.name.ToString(), aFrom.GetType().ToString());
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
                string log = Strings.Log.CopyAttempt;

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
                        Log(log + ": " + Strings.Log.Success);
                    }
                }
                else
                {
                    Log(log + " " + Strings.Log.FailedDoesntHave, LogType.Warning, lFrom.gameObject.name.ToString(), lFrom.GetType().ToString());
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
                string log = Strings.Log.CopyAttempt;

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
                    Log(Strings.Log.FailedHasNo, LogType.Warning, rFrom.gameObject.name, rFrom.GetType().ToString());
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

                if(bCopier_descriptor_copyViewpoint)
                {                    
                    dTo.ViewPosition = dFrom.ViewPosition;
                }
            }
        }

        /// <summary>
        /// Copies all DynamicBoneColliders from object and it's children to another object.
        /// </summary>        
        /// <param name="removeOldColliders">Whether to remove all DynamicBoneColliders from target before copying</param>
        void CopyAllDynamicBoneColliders(GameObject from, GameObject to, bool createGameObjects, bool useignoreList)
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
                if((!tTo) || (useignoreList && Helpers.ShouldIgnoreObject(dbcFrom.transform, _copierIgnoreArray, bCopier_ignoreArray_includeChildren)))
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
            bool found = false;

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
                        LogVerbose("{0} already has this DynamicBone. Ignoring", LogType.Log, d.name);
                        found = true;
                    }
                }

                if(!found)
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
                        Log("_Copied DynamicBone from {0}'s {1} to {2}'s", LogType.Log, dFrom.transform.root.name, dFrom.transform.name, tTo.name);
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
                string log = Strings.Log.CopyAttempt;
                var t = cFromArr[i].GetType();
                
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
                            Log(log + " - " + Strings.Log.FailedAlreadyHas, LogType.Warning, t.ToString(), cFromArr[i].gameObject.name, cToObj.name);
                            break;
                        }
                    }
                    if(!found)
                    {
                        ComponentUtility.CopyComponent(cFromArr[i]);
                        ComponentUtility.PasteComponentAsNew(cToObj);

                        Log(log + " - " + Strings.Log.Success, LogType.Log, t.ToString(), cFromArr[i].gameObject.name, cToObj.name);
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

            string log = Strings.Log.CopyAttempt + " ";
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

            string log = String.Format(Strings.Log.CopyAttempt + " - ", Strings.Copier.SkinMeshRender, from.name, to.name);

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

                        Log(log + Strings.Log.Success);
                    }
                    else
                    {
                        Log(log + Strings.Log.FailedDoesntHave, LogType.Warning, rTo.gameObject.name, rFrom.GetType().ToString());
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

                        Log("_Success: Copied over {0} from {1}'s {2} to {3}'s {4}", LogType.Log, "ParticleSystem", copierSelectedFrom.name, pp.gameObject.name, selectedAvatar.name, tTo.gameObject.name);
                    }
                    else
                    {
                        Log("_Failed: {0}'s {1} already has a ParticleSystem. Ignoring.", LogType.Log, selectedAvatar.name, pp.gameObject.name);
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

                Log(Strings.Log.RemoveAttempt + ": " + "_Success.", LogType.Log, p.ToString(), from.name);
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
                    if(c <= 0 && (t.name != (t.parent.name + "_end")))
                    {
                        Log("_{0} has no components or children. Destroying", LogType.Log, t.name);
                        DestroyImmediate(t.gameObject);
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

                    log = Strings.Log.RemoveAttempt + " - ";
                    string name = comps[i].name;

                    try
                    {
                        DestroyImmediate(comps[i]);
                        log += Strings.Log.Success;
                        Log(log, LogType.Log, type.ToString(), name);
                    }
                    catch(Exception e)
                    {
                        log += Strings.Log.Failed + ": " + e.Message;
                        Log(log, LogType.Exception, type.ToString(), name);
                    }
                }
            }
        }

        #endregion

        #region Helper Functions

        /// <summary>
        /// Resets all BlendShape weights to 0 on all SkinnedMeshRenderers or to prefab values
        /// </summary>        
        /// <param name="revertToPrefab">Revert weights to prefab instead</param>
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
        /// <param name="revertToPrefab">Revert weights to prefab instead</param>        
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
                    Log("_Mesh prefab is missing, can't revert to default blendshapes.", LogType.Error);
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
        /// Reset transforms back to prefab values
        /// </summary>        
        public static bool ResetPose(GameObject objTo)
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
                Log("_Mesh prefab is missing, can't revert to default pose.", LogType.Error);
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
        void CenterCameraOnViewpoint(GameObject avatarOverride = null)
        {
            if(!VRCCam)
                return;

            VRC_AvatarDescriptor desc;
            if(avatarOverride == null)
            {
                var pm = FindObjectOfType<RuntimeBlueprintCreation>();
                if(pm == null)
                {
                    Debug.Log("_RuntimeBlueprintCreation script not found. Start uploading an avatar to use this");
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
                Debug.Log("_Failed to center camera on Viewpoint. Avatar descriptor not found");
                return;
            }

            VRCCam.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

            GameObject focusObj = new GameObject("FocusDummy");
            focusObj.transform.position = desc.transform.position + desc.ViewPosition;
            focusObj.transform.rotation = desc.transform.rotation;

            Transform oldParent = VRCCam.transform.parent;
            VRCCam.transform.parent = focusObj.transform;

            focusObj.transform.localEulerAngles = focusObj.transform.localEulerAngles + new Vector3(5, 166, 0);

            VRCCam.transform.localPosition = Vector3.zero + new Vector3(0, desc.ViewPosition.z * 0.05f, desc.ViewPosition.y * -0.28f);
            VRCCam.transform.parent = null;
            VRCCam.transform.position = VRCCam.transform.position + new Vector3(-0.02f, 0, 0);
            VRCCam.transform.parent = oldParent;

            DestroyImmediate(focusObj);
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
            JsonUtility.FromJsonOverwrite(data, this);
            LogVerbose("Loaded tool window preferences");
        }        

        public static void LogVerbose(string message, LogType logType = LogType.Log, params string[] logFormat)
        {
            if(!verboseLoggingEnabled)
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
            if(logFormat.Length > 0)
                message = string.Format(message, logFormat);
            message = "<color=blue>PumkinsAvatarTools</color>: " + message;

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