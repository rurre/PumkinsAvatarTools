using System.Collections.Generic;
using UnityEditor;
using VRCSDK2;
using VRC.Core;
using UnityEditor.SceneManagement;
using System;
using System.IO;
using UnityEngine;
using System.Linq;
using Pumkin.DependencyChecker;
using Pumkin.PoseEditor;
using UnityEngine.UI;
using UnityEditorInternal;
using VRCSDK2.Validation.Performance.Stats;
using VRCSDK2.Validation.Performance;

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
        [SerializeField] bool bCopier_animators_createMissing = false;
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

        //Editor
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
        ///[SerializeField] bool _copier_expand_joints = false;
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
        [SerializeField] Color _vrcCamBgColor = new Color(0.235f, 0.22f, 0.22f);
        [SerializeField] Color _vrcCamColorOld = new Color(0.192f, 0.302f, 0.475f);
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

        static readonly Type[] supportedComponents =
        {
#if BONES || OLD_BONES
            typeof(DynamicBone),
            typeof(DynamicBoneCollider),
            typeof(DynamicBoneColliderBase),
#endif
            typeof(Collider),
            typeof(BoxCollider),
            typeof(CapsuleCollider),
            typeof(SphereCollider),
            typeof(MeshCollider),
            typeof(SkinnedMeshRenderer),
            typeof(Transform),
            typeof(VRC_AvatarDescriptor),
            typeof(PipelineManager),
            typeof(Rigidbody),
            typeof(ParticleSystem),
            typeof(ParticleSystemRenderer),
            typeof(TrailRenderer),
            typeof(MeshRenderer),
            typeof(Light),
            typeof(AudioSource),
            typeof(ONSPAudioSource),
            typeof(MeshFilter),
            typeof(Animator),
            typeof(Joint),
            typeof(FixedJoint),
            typeof(HingeJoint),
            typeof(CharacterJoint),
            typeof(ConfigurableJoint),
            typeof(SpringJoint),
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
                    string s = _DependecyChecker.RelativePath(toolScriptPath.Substring(0, toolScriptPath.LastIndexOf('\\')));
                    //int i = s.LastIndexOf('\\');
                    //string ss = s.Substring(0, i);
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
                if(!_cameraOverlayImage && _cameraOverlay)
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
                if(_cameraBackgroundImage == null && _cameraBackground)
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
        }

        //[MenuItem("Tools/Pumkin/Avatar Tools")] //_PumkinsAvatarToolsWindow.cs is responsible for calling this, for now
        public static void ShowWindow()
        {
            //Show existing window instance. If one doesn't exist, make one.
            EditorWindow editorWindow = EditorWindow.GetWindow(typeof(PumkinsAvatarTools));
            editorWindow.autoRepaintOnSceneChange = true;

            editorWindow.Show();
            editorWindow.titleContent = new GUIContent(Strings.Main.WindowName);

            _DependecyChecker.Check();
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

                //Who thought this was a good way of displaying credits
                GUILayout.Label(Strings.Credits.Title);
                GUILayout.Label(Strings.Credits.Version);

                EditorGUILayout.Space();

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
                    if(Strings.Language != Strings.DictionaryLanguage.uwu)
                        Strings.Language = Strings.DictionaryLanguage.uwu;
                    else
                        Strings.Language = Strings.DictionaryLanguage.English;
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

                DrawGuiLine();

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
                                            ActionButton(ToolMenuActions.FillVisemes);
                                        if(GUILayout.Button(Strings.Tools.RevertBlendshapes))
                                            ActionButton(ToolMenuActions.RevertBlendshapes);
                                        if(GUILayout.Button(Strings.Tools.ResetPose))
                                            ActionButton(ToolMenuActions.ResetPose);
                                    }
                                    GUILayout.EndVertical();

                                    GUILayout.BeginVertical(GUILayout.ExpandWidth(true)); //Right Column
                                    {
                                        EditorGUI.BeginDisabledGroup(DrawingHandlesGUI);
                                        {
                                            if(GUILayout.Button(Strings.Tools.EditViewpoint))
                                                ActionButton(ToolMenuActions.EditViewpoint);
                                        }
                                        EditorGUI.EndDisabledGroup();

                                        if(GUILayout.Button(Strings.Tools.ZeroBlendshapes))
                                            ActionButton(ToolMenuActions.ZeroBlendshapes);

                                        if(GUILayout.Button(Strings.Tools.ResetToTPose))
                                            ActionButton(ToolMenuActions.SetTPose);
                                        EditorGUI.BeginDisabledGroup(DrawingHandlesGUI);
                                        {
                                            if(GUILayout.Button(Strings.Tools.EditScale))
                                                ActionButton(ToolMenuActions.EditScale);
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
                                    ActionButton(ToolMenuActions.RemoveDynamicBones);
#if !BONES
                            EditorGUI.EndDisabledGroup();
#endif
                                if(GUILayout.Button(new GUIContent(Strings.Copier.ParticleSystems, Icons.ParticleSystem)))
                                    ActionButton(ToolMenuActions.RemoveParticleSystems);
                                if(GUILayout.Button(new GUIContent(Strings.Copier.Lights, Icons.Light)))
                                    ActionButton(ToolMenuActions.RemoveLights);
                                if(GUILayout.Button(new GUIContent(Strings.Copier.Joints, Icons.Joint)))
                                    ActionButton(ToolMenuActions.RemoveJoints);
                                if(GUILayout.Button(new GUIContent(Strings.Copier.Animators_inChildren, Icons.Animator)))
                                    ActionButton(ToolMenuActions.RemoveAnimatorsInChildren);
                                if(GUILayout.Button(new GUIContent(Strings.Copier.Colliders, Icons.ColliderBox)))
                                    ActionButton(ToolMenuActions.RemoveColliders);

                                EditorGUILayout.EndVertical();

#if !BONES
                            EditorGUI.BeginDisabledGroup(true);
#endif
                                EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true)); //Right Column
                                if(GUILayout.Button(new GUIContent(Strings.Copier.DynamicBones_colliders, Icons.BoneColliderIcon)))
                                    ActionButton(ToolMenuActions.RemoveDynamicBoneColliders);
#if !BONES
                            EditorGUI.EndDisabledGroup();
#endif
                                if(GUILayout.Button(new GUIContent(Strings.Copier.TrailRenderers, Icons.TrailRenderer)))
                                    ActionButton(ToolMenuActions.RemoveTrailRenderers);
                                if(GUILayout.Button(new GUIContent(Strings.Copier.AudioSources, Icons.AudioSource)))
                                    ActionButton(ToolMenuActions.RemoveAudioSources);
                                if(GUILayout.Button(new GUIContent(Strings.Copier.RigidBodies, Icons.RigidBody)))
                                    ActionButton(ToolMenuActions.RemoveRigidBodies);
                                if(GUILayout.Button(new GUIContent(Strings.Copier.MeshRenderers, Icons.MeshRenderer)))
                                    ActionButton(ToolMenuActions.RemoveMeshRenderers);
                                if(GUILayout.Button(new GUIContent(Strings.Copier.EmptyGameObjects, Icons.Prefab)))
                                    ActionButton(ToolMenuActions.RemoveEmptyGameObjects);

                                EditorGUILayout.EndVertical();

                                EditorGUILayout.EndHorizontal();
                            }
                            EditorGUI.EndDisabledGroup();
                            EditorGUILayout.Space();
                        }
                    }

                    DrawGuiLine();

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
                                DrawPropertyArrayScrolling(SerializedIgnoreArray, "_Exclusions", ref _copierIgnoreArray_expand, ref _copierIgnoreArrayScroll, 0, 100);                                
                            }
                            if(EditorGUI.EndChangeCheck())
                            {
                                RefreshIgnoreArray();                            
                            }

                            if(_copierIgnoreArray_expand)
                            {
                                EditorGUILayout.Space();
                                bCopier_ignoreArray_includeChildren = EditorGUILayout.Toggle("_Include children", bCopier_ignoreArray_includeChildren);
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

                    DrawGuiLine();

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

                    DrawGuiLine();

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
                                        cameraOverrideTexture = OpenImageTexture(ref _lastOpenFilePath);
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
                                    Repaint();
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
                                                cameraBackgroundTexture = OpenImageTexture(ref _lastOpenFilePath);
                                                _backgroundPathText = _lastOpenFilePath;
                                            }
                                            if(GUILayout.Button("X", GUILayout.MaxWidth(25)))
                                            {
                                                if(CameraBackgroundRawImage)
                                                    CameraBackgroundRawImage.color = Color.clear;
                                                _backgroundPathText = null;
                                            }
                                        }
                                        if(EditorGUI.EndChangeCheck() && cameraBackgroundTexture != null)
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

                    DrawGuiLine();

                    //Misc menu
                    if(_misc_expand = GUILayout.Toggle(_misc_expand, Strings.Main.Misc, Styles.Foldout_title))
                    {
                        EditorGUILayout.Space();
#if !BONES && !OLD_BONES
                        if(GUILayout.Button(Strings.Misc.SearchForBones, Styles.BigButton))
                        {
                            _DependecyChecker.Check();
                        }
                        EditorGUILayout.Space();
#endif
                        GUILayout.BeginHorizontal();
                        {

                            if(GUILayout.Button(new GUIContent(Strings.Buttons.OpenGithubPage, Icons.GithubIcon)))
                            {
                                Application.OpenURL(Strings.toolsPage);
                            }
                            if(GUILayout.Button(new GUIContent(Strings.Buttons.OpenHelpPage, Icons.Help)))
                            {
                                Application.OpenURL(Strings.toolsPage + "wiki");
                            }
                        }
                        GUILayout.EndHorizontal();

                        if(GUILayout.Button(new GUIContent(Strings.Buttons.JoinDiscordServer, Icons.DiscordIcon)))
                        {
                            Application.OpenURL(Strings.discordLink);
                        }
                        if(GUILayout.Button(new GUIContent(Strings.Buttons.OpenDonationPage, Icons.KofiIcon)))
                        {
                            Application.OpenURL(Strings.donationLink);
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
                
                var tt = FindTransformInAnotherHierarchy(t, copierSelectedFrom.transform, false);
                if(tt && !newList.Contains(tt))
                    newList.Add(tt);
            }

            _copierIgnoreArray = newList.ToArray();                  
        }

        Texture2D OpenImageTexture(ref string startPath)
        {
            Texture2D tex = null;
            string path = EditorUtility.OpenFilePanel("Pick an Image", startPath, "png,jpg,jpeg");
            if(File.Exists(path))
            {
                byte[] data = File.ReadAllBytes(path);
                try
                {
                    tex = new Texture2D(2, 2);
                    tex.LoadImage(data);
                    tex.alphaIsTransparency = true;
                    tex.name = path;
                }
                catch
                {
                    tex = null;
                }
            }
            startPath = path;
            return tex;
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

                    Handles.color = new Color(1, 0.92f, 0.016f, 0.5f);
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
                    Handles.color = new Color(1, 0.92f, 0.016f, 0.5f);
                    Handles.SphereHandleCap(0, _viewPosTemp, Quaternion.identity, 0.02f, EventType.Repaint);				
                }                
            }
            sceneView.Repaint();
        }

        private void SetAvatarScale(VRC_AvatarDescriptor desc, object newScale)
        {
            selectedAvatar.transform.localScale = RoundVectorValues(new Vector3(_avatarScaleTemp, _avatarScaleTemp, _avatarScaleTemp), 3);
            if(_scaleViewpointDummy)
                _viewPosTemp = _scaleViewpointDummy.position;
            else
                EndScalingAvatar(desc.gameObject, true);
        }

        private float GetDeltaMultiplier(float startValue, float endValue)
        {               
            return (endValue - startValue) / startValue;            
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
        /// Function for all the actions in the tool menu. Use this instead of calling
        /// button functions directly.
        /// </summary>        
        void ActionButton(ToolMenuActions action)
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
            _viewPosTemp = _viewPosOld;

            if(!_scaleViewpointDummy)
            {
                var g = GameObject.Find("_PumkinsViewpointDummy");
                if(g)
                    _scaleViewpointDummy = g.transform;
                else
                    _scaleViewpointDummy = new GameObject("_PumkinsViewpointDummy").transform;
            }

            _scaleViewpointDummy.parent = selectedAvatar.transform;
            _scaleViewpointDummy.position = _viewPosTemp;

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
                    _tempAvatarDescriptor.ViewPosition = _viewPosTemp;
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
                    _tempAvatarDescriptor.ViewPosition = RoundVectorValues(_viewPosTemp - _tempAvatarDescriptor.gameObject.transform.position, 3);
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
            if(d == null)
            {
                d = avatar.AddComponent<VRC_AvatarDescriptor>();
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

            //Run statment only if root so only run this once
            if(objTo != null && objTo.transform == objTo.transform.root)
            {
                if(bCopier_transforms_copyAvatarScale)
                {
                    objTo.transform.localScale = objFrom.transform.localScale;
                }
                if(bCopier_descriptor_copy)
                {
                    CopyAvatarDescriptor(objFrom, objTo, true);
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

                var tTo = FindTransformInAnotherHierarchy(aFrom.transform, to.transform, createGameObjects);

                if((!tTo) || (useignoreList && ShouldIgnoreObject(aFrom.transform, _copierIgnoreArray, bCopier_ignoreArray_includeChildren)))
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
                var tTo = FindTransformInAnotherHierarchy(aFrom.transform, to.transform, createGameObjects);

                if((!tTo) || (useignoreList && ShouldIgnoreObject(aFrom.transform, _copierIgnoreArray, bCopier_ignoreArray_includeChildren)))
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
                var tTo = FindTransformInAnotherHierarchy(lFrom.transform, to.transform, createGameObjects);

                if((!tTo) ||
                    (useignoreList && ShouldIgnoreObject(lFrom.transform, _copierIgnoreArray, bCopier_ignoreArray_includeChildren)))
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
                var tTo = FindTransformInAnotherHierarchy(rFrom.transform, to.transform, createGameObjects);

                if((!tTo) ||
                    (useignoreList && ShouldIgnoreObject(rFrom.transform, _copierIgnoreArray, bCopier_ignoreArray_includeChildren)))
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

            if(useignoreList && ShouldIgnoreObject(from.transform, _copierIgnoreArray))
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
                dTo.ViewPosition = dFrom.ViewPosition;
                dTo.VisemeBlendShapes = dFrom.VisemeBlendShapes;

                if(dFrom.VisemeSkinnedMesh != null)
                {
                    string s = GetGameObjectPath(dFrom.VisemeSkinnedMesh.gameObject, true);
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
                var tTo = FindTransformInAnotherHierarchy(dbcFrom.transform, to.transform, createGameObjects);
                if((!tTo) || (useignoreList && ShouldIgnoreObject(dbcFrom.transform, _copierIgnoreArray, bCopier_ignoreArray_includeChildren)))
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
                if(useignoreList && ShouldIgnoreObject(dFrom.transform, _copierIgnoreArray, bCopier_ignoreArray_includeChildren))
                    continue;

                var tTo = FindTransformInAnotherHierarchy(dFrom.transform, to.transform, false);
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

                    newDynBone.m_Root = FindTransformInAnotherHierarchy(dFrom.m_Root.transform, newDynBone.transform.root, false);

                    if(dFrom.m_ReferenceObject)
                        newDynBone.m_ReferenceObject = FindTransformInAnotherHierarchy(dFrom.m_ReferenceObject, newDynBone.transform.root, false);

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
                        var t = FindTransformInAnotherHierarchy(newDynBone.m_Colliders[i].transform, to.transform, false);

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

                        var t = FindTransformInAnotherHierarchy(ex.transform, to.transform, false);
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

                if(supportedComponents.Contains(t))
                {
                    var cc = cFromArr[i];
                    var cFromPath = GetGameObjectPath(cc.gameObject);

                    if(useignoreList && ShouldIgnoreObject(cc.transform, _copierIgnoreArray, bCopier_ignoreArray_includeChildren))
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
                            if(CollidersAreIdentical(cToArr[z], cFromArr[i]))
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
                else
                {
                    Log(log + Strings.Log.FailedNotSupported, LogType.Warning, t.ToString());
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

            if(useignoreList && ShouldIgnoreObject(tFrom, _copierIgnoreArray, bCopier_ignoreArray_includeChildren))
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
                var rFromPath = GetGameObjectPath(rFrom.gameObject);

                if(rFromPath != null)
                {
                    var tTo = to.transform.root.Find(rFromPath);

                    if((!tTo) ||
                        (useignoreList && ShouldIgnoreObject(rFrom.transform, _copierIgnoreArray, bCopier_ignoreArray_includeChildren)))
                        continue;

                    GameObject rToObj = tTo.gameObject;

                    var rTo = rToObj.GetComponent<SkinnedMeshRenderer>();

                    if(rTo != null)
                    {
                        if(bCopier_skinMeshRender_copySettings)
                        {
                            var t = FindTransformInAnotherHierarchy(rFrom.rootBone, rTo.transform, false);
                            rTo.rootBone = t ?? rTo.rootBone;
                            t = FindTransformInAnotherHierarchy(rFrom.probeAnchor, rTo.transform, false);

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
                var tTo = FindTransformInAnotherHierarchy(rFrom.transform, to.transform, createGameObjects);

                if(!tTo)
                    continue;

                if(useignoreList && ShouldIgnoreObject(rFrom.transform, _copierIgnoreArray, bCopier_ignoreArray_includeChildren))
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
                var tTo = FindTransformInAnotherHierarchy(rFrom.transform, to.transform, createGameObjects);

                if(!tTo)
                    continue;

                if(useignoreList && ShouldIgnoreObject(rFrom.transform, _copierIgnoreArray, bCopier_ignoreArray_includeChildren))
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

                if(useignoreList && ShouldIgnoreObject(pp.transform, _copierIgnoreArray, bCopier_ignoreArray_includeChildren))
                    continue;

                var tTo = FindTransformInAnotherHierarchy(pp.transform, to.transform, createGameObjects);

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
                    if(c <= 0)
                    {
                        Log("_{0} has no components or children. Destroying", LogType.Log, t.name);
                        DestroyImmediate(t.gameObject);
                    }
                }
            }
        }

        /// <summary>
        /// Destroy all components of type, if the type is in supportedComponents.
        /// Not sure if it's even necessary to check but we'll keep it for now
        /// </summary>        
        void DestroyAllComponentsOfType(GameObject obj, Type type, bool ignoreRoot, bool useignoreList)
        {
            string log = "";
            string[] logFormat = { type.ToString(), obj.name };
            if(!IsSupportedComponentType(type))
            {
                log += Strings.Log.TryRemoveUnsupportedComponent;
                Log(log, LogType.Warning, logFormat);
                return;
            }

            Component[] comps = obj.transform.GetComponentsInChildren(type, true);

            if(comps != null && comps.Length > 0)
            {
                for(int i = 0; i < comps.Length; i++)
                {
                    if((ignoreRoot && comps[i].transform.parent == null) ||
                        (useignoreList && ShouldIgnoreObject(comps[i].transform, _copierIgnoreArray, bCopier_ignoreArray_includeChildren)))
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
        /// Reset transforms back to prefab values
        /// </summary>        
        public static bool ResetPose(GameObject objTo)
        {
            if(objTo == null)
                return false;

            string toPath = GetGameObjectPath(objTo);
            var pref = PrefabUtility.GetPrefabParent(objTo.transform.root.gameObject) as GameObject;

            if(pref == null)
            {
                Log("_Mesh prefab is missing, can't revert to default pose.", LogType.Error);
                return false;
            }

            Transform tr = pref.transform.Find(toPath);

            if(tr == null)
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
                pref = PrefabUtility.GetPrefabParent(render.gameObject) as GameObject;
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
        /// Returns true if GameObject has no children or components
        /// </summary>                
        bool GameObjectIsEmpty(GameObject obj)
        {
            if(obj.GetComponentsInChildren<Component>().Length > obj.GetComponentsInChildren<Transform>().Length)
                return false;
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
        /// Check whether or not we should ignore the object based on it being in the set.
        /// </summary>
        static bool ShouldIgnoreObject(Transform trans, Transform[] ignoreArray, bool includeChildren = false)
        {
            if((!trans || ignoreArray == null) || trans == trans.root)
                return false;

            if(ignoreArray.Length > 0 && includeChildren)
            {
                var t = trans;
                do
                {
                    if(ignoreArray.Contains(t))
                        return true;
                    t = t.parent;
                } while(t != null && t != t.root);
                return false;
            }

            if(ignoreArray.Contains(trans))
                return true;
            return false;
        }

        void SavePrefs()
        {
            //if(!_canSavePrefs)
            //    return;
            //EditorCoroutine.Start(SavePrefsCooldown(0.5f));

            var data = JsonUtility.ToJson(this, false);
            EditorPrefs.SetString("PumkinToolsWindow", data);
            LogVerbose("Saved tool window preferences");
        }

        void LoadPrefs()
        {
            //if(!_canLoadPrefs)
            //    return;
            //EditorCoroutine.Start(LoadPrefsCooldown(0.5f));

            var data = EditorPrefs.GetString("PumkinToolsWindow", JsonUtility.ToJson(this, false));
            JsonUtility.FromJsonOverwrite(data, this);
            LogVerbose("Loaded tool window preferences");
        }        

        public static string GetGameObjectPath(Transform trans, bool skipRoot = true)
        {
            if(trans != null)
                return GetGameObjectPath(trans.gameObject, skipRoot);
            return null;
        }

        public static string GetGameObjectPath(GameObject obj, bool skipRoot = true)
        {
            if(!obj)
                return null;

            string path = null;
            if(obj.transform != obj.transform.root)
            {
                if(!skipRoot)
                    path = obj.transform.root.name + "/";
                path += (AnimationUtility.CalculateTransformPath(obj.transform, obj.transform.root));
            }
            else
            {
                if(!skipRoot)
                    path = obj.transform.root.name;
            }
            return path;
        }

        public static string GetNameFromPath(string path)
        {
            if(string.IsNullOrEmpty(path))
                return path;

            for(int i = path.Length - 1; i >= 0; i--)
            {
                if(path[i] == '\\' || path[i] == '/')
                {
                    try
                    {
                        return path.Substring(i + 1);
                    }
                    catch
                    {
                        return path;
                    }
                }
            }
            return path;
        }

        public static string GetPathNoName(string path)
        {
            if(string.IsNullOrEmpty(path))
                return path;

            for(int i = path.Length - 1; i >= 0; i--)
            {
                if((path[i] == '\\' || path[i] == '/'))
                {
                    if(i + 1 < path.Length - 1 && (path[i + 1] != '\r' && path[i + 1] != '\n'))
                    {
                        return path.Substring(0, i) + '/';
                    }
                }
            }
            return path;
        }

        public static string[] GetPathAsArray(string path)
        {
            if(string.IsNullOrEmpty(path))
                return null;

            return path.Split('\\', '/');
        }

        /// <summary>
        /// Looks for object in another object's child hierarchy. Can create if missing.
        /// </summary>                
        /// <returns>Transform of found object</returns>
        public Transform FindTransformInAnotherHierarchy(Transform trans, Transform otherHierarchyTrans, bool createIfMissing)
        {
            if(!trans || !otherHierarchyTrans)
                return null;

            var childPath = GetGameObjectPath(trans);
            var childTrans = otherHierarchyTrans.Find(childPath, createIfMissing, trans);

            return childTrans;
        }

        public static bool IsSupportedComponentType(Type type)
        {
            foreach(Type t in supportedComponents)
            {
                if(t == type)
                {
                    return true;
                }
            }
            return false;
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

        public static bool PhysMaterialsAreIdentical(PhysicMaterial mat1, PhysicMaterial mat2)
        {
            if(mat1 == null && mat2 == null)
                return true;

            if(mat1.bounceCombine == mat2.bounceCombine && mat1.bounciness == mat2.bounciness && mat1.dynamicFriction == mat2.dynamicFriction &&
                mat1.frictionCombine == mat2.frictionCombine && mat1.staticFriction == mat2.staticFriction)
                return true;
            else
                return false;
        }

        public static bool JointsAreIdentical(Joint j1, Joint j2)
        {
            throw new NotImplementedException();

            //if(j1 == null && j2 == null)
            //    return true;
            //else if(j1.GetType() != j2.GetType())
            //    return false;            

            //if(j1 is FixedJoint)
            //{
            //    var j = (FixedJoint)j1;
            //    var jj = (FixedJoint)j2;                
            //}

            //if(j1 is HingeJoint)
            //{
            //    var j = (HingeJoint)j1;
            //    var jj = (HingeJoint)j2;
            //}

            //if(j1 is SpringJoint)
            //{
            //    var j = (SpringJoint)j1;
            //    var jj = (SpringJoint)j2;
            //}

            //if(j1 is CharacterJoint)
            //{
            //    var j = (CharacterJoint)j1;
            //    var jj = (CharacterJoint)j2;
            //}

            //if(j1 is ConfigurableJoint)
            //{
            //    var j = (ConfigurableJoint)j1;
            //    var jj = (ConfigurableJoint)j2;
            //}            
        }

        public static bool CollidersAreIdentical(Collider col1, Collider col2)
        {
            if(col1 == null && col2 == null)
                return true;
            else if(col1.GetType() != col2.GetType())
                return false;

            if(!PhysMaterialsAreIdentical(col1.material, col2.material) ||
                !PhysMaterialsAreIdentical(col1.sharedMaterial, col2.sharedMaterial) || col1.isTrigger != col2.isTrigger)
                return false;

            if(col1 is BoxCollider)
            {
                var c = (BoxCollider)col1;
                var cc = (BoxCollider)col2;

                if(c.size != cc.size || c.center != c.center)
                    return false;
            }
            else if(col1 is CapsuleCollider)
            {
                var c = (CapsuleCollider)col1;
                var cc = (CapsuleCollider)col2;

                if(c.center != cc.center || c.radius != c.radius || c.height != cc.height || c.direction != cc.direction)
                    return false;

            }
            else if(col1 is SphereCollider)
            {
                var c = (SphereCollider)col1;
                var cc = (SphereCollider)col2;

                if(c.center != cc.center || c.radius != cc.radius)
                    return false;
            }
            return true;
        }

        public static Vector3 RoundVectorValues(Vector3 v, int decimals)
        {
            return new Vector3((float)Math.Round(v.x, decimals), (float)Math.Round(v.y, decimals), (float)Math.Round(v.z, decimals));
        }

        public static string NextAvailableFilename(string path)
        {
            string numberPattern = " {0}";

            // Short-cut if already available
            if(!File.Exists(path))
                return path;

            // If path has extension then insert the number pattern just before the extension and return next filename
            if(Path.HasExtension(path))
                return GetNextFilename(path.Insert(path.LastIndexOf(Path.GetExtension(path)), numberPattern));

            // Otherwise just append the pattern to the path and return next filename
            return GetNextFilename(path + numberPattern);
        }

        private static string GetNextFilename(string pattern)
        {
            string tmp = string.Format(pattern, 1);
            if(tmp == pattern)
                throw new System.ArgumentException("The pattern must include an index place-holder", "pattern");

            if(!File.Exists(tmp))
                return tmp; // short-circuit if no matches

            int min = 1, max = 2; // min is inclusive, max is exclusive/untested

            while(File.Exists(string.Format(pattern, max)))
            {
                min = max;
                max *= 2;
            }

            while(max != min + 1)
            {
                int pivot = (max + min) / 2;
                if(File.Exists(string.Format(pattern, pivot)))
                    min = pivot;
                else
                    max = pivot;
            }

            return string.Format(pattern, max);
        }

        public static bool TransformIsInDefaultPosition(Transform t, bool onlyRotation)
        {
            if(t == null)
                return false;

            string tPath = GetGameObjectPath(t.gameObject);
            var pref = PrefabUtility.GetPrefabParent(t.root.gameObject) as GameObject;

            if(pref == null)
                return false;

            Transform tP = pref.transform.Find(tPath);

            if(tP == null)
                return false;

            if(onlyRotation)
            {
                return t.localRotation == tP.localRotation;
            }
            else
            {
                if(t.localPosition == tP.localPosition && t.localRotation == tP.localRotation && t.localEulerAngles == tP.localEulerAngles)
                {
                    return true;
                }
                return false;
            }
        }

        public static void DrawGuiLine(float height = 1f)
        {
            EditorGUILayout.Space();
            GUILayout.Box(GUIContent.none, Styles.Editor_line, GUILayout.ExpandWidth(true), GUILayout.Height(height));
            EditorGUILayout.Space();
        }

        public static void DrawPropertyArrayScrolling(SerializedProperty property, string displayName, ref bool expanded, ref Vector2 scrollPosition, float minHeight, float maxHeight)
        {
            if(property == null)
                return;

            expanded = EditorGUILayout.Foldout(expanded, displayName);
            if(expanded)
            {
                SerializedProperty arraySizeProp = property.FindPropertyRelative("Array.size");
                EditorGUILayout.PropertyField(arraySizeProp);

                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.MinHeight(Mathf.Clamp(arraySizeProp.intValue * 20, 0, maxHeight)), GUILayout.MaxHeight(maxHeight));
                EditorGUI.indentLevel++;
                for(int i = 0; i < arraySizeProp.intValue; i++)
                {
                    EditorGUILayout.PropertyField(property.GetArrayElementAtIndex(i));
                }                
                EditorGUI.indentLevel--;
                EditorGUILayout.EndScrollView();
            }            
        }

        public static void DrawPropertyArray(SerializedProperty property, string displayName, ref bool expanded)
        {
            if(property == null)
                return;

            expanded = EditorGUILayout.Foldout(expanded, displayName);
            if(expanded)
            {
                SerializedProperty arraySizeProp = property.FindPropertyRelative("Array.size");
                EditorGUILayout.PropertyField(arraySizeProp);

                EditorGUI.indentLevel++;

                for(int i = 0; i < arraySizeProp.intValue; i++)
                {
                    EditorGUILayout.PropertyField(property.GetArrayElementAtIndex(i));
                }

                EditorGUI.indentLevel--;
            }
        }

        #endregion
    }
    #region Data Structures

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
        }
    }

    /// <summary>
    /// Strings. Need to move these to files eventually
    /// </summary>
    public static class Strings
    {
        public static readonly string version = "0.7b - Work in Progress";
        public static readonly string toolsPage = "https://github.com/rurre/PumkinsAvatarTools/";
        public static readonly string donationLink = "https://ko-fi.com/notpumkin";
        public static readonly string discordLink = "https://discord.gg/7vyekJv";
        readonly static Dictionary<string, string> dictionary_english, dictionary_uwu;
        static Dictionary<string, string> stringDictionary;
        static DictionaryLanguage language;

        public enum DictionaryLanguage { English, uwu = 100 };

        static public DictionaryLanguage Language
        {
            get { return language; }
            set
            {
                if(value != language)
                {
                    switch(value)
                    {
                        case DictionaryLanguage.English:
                            stringDictionary = dictionary_english;
                            break;
                        case DictionaryLanguage.uwu:
                            stringDictionary = dictionary_uwu;
                            break;
                        default:
                            stringDictionary = dictionary_english;
                            break;
                    }
                    language = value;
                    ReloadStrings();
                }
            }
        }

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
                Name = GetString("ui_avatarInfo_name") ?? "_{0}";
                Line = GetString("ui_avatarInfo_line") ?? "---------------------";
                GameObjects = GetString("ui_avatarInfo_gameobjects") ?? "_GameObjects: {0} ({1})";
                Bones = GetString("ui_avatarInfo_bones") ?? "_Bones: {0} - {1}";
                SkinnedMeshRenderers = GetString("ui_avatarInfo_skinnedMeshRenderers") ?? "_Skinned Mesh Renderers: {0} ({1}) - {2}";
                MeshRenderers = GetString("ui_avatarInfo_meshRenderers") ?? "_Mesh Renderers: {0} ({1}) - {2}";
                Polygons = GetString("ui_avatarInfo_polygons") ?? "_Polygons: {0} ({1}) - {2}";
                UsedMaterialSlots = GetString("ui_avatarInfo_usedMaterialSlots") ?? "_Used Material Slots: {0} ({1}) - {2}";
                UniqueMaterials = GetString("ui_avatarInfo_uniqueMaterials") ?? "_Unique Materials: {0} ({1})";
                Shaders = GetString("ui_avatarInfo_shaders") ?? "_Shaders: {0}";
                DynamicBoneTransforms = GetString("ui_avatarInfo_dynamicBoneTransforms") ?? "_Dynamic Bone Transforms: {0} ({1}) - {2}";
                DynamicBoneColliders = GetString("ui_avatarInfo_dynamicBoneColliders") ?? "_Dynamic Bone Colliders: {0} ({1}) - {2}";
                DynamicBoneColliderTransforms = GetString("ui_avatarInfo_dynamicBoneColliderTransforms") ?? "_Collider Affected Transforms: {0} ({1}) - {2}";
                ParticleSystems = GetString("ui_avatarInfo_particleSystems") ?? "_Particle Systems: {0} ({1}) - {2}";
                MaxParticles = GetString("ui_avatarInfo_maxParticles") ?? "_Max Particles: {0} ({1}) - {2}";
                OverallPerformance = GetString("ui_avatarInfo_overallPerformance") ?? "_Overall Performance: {0}";
                SelectAvatarFirst = GetString("ui_avatarInfo_selectAvatarFirst") ?? "_Select an Avatar first";
            }
        }

        public static class Main
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
                Avatar = GetString("ui_main_avatar") ?? "_Avatar";
                Title = GetString("ui_main_title") ?? "_Pumkin's Avatar Tools";
                Version = GetString("ui_main_version") ?? "_Version";
                WindowName = GetString("ui_main_windowName") ?? "_Pumkin Tools";
                Tools = GetString("ui_tools") ?? "_Tools";
                Copier = GetString("ui_copier") ?? "_Copy Components";
                AvatarInfo = GetString("ui_avatarInfo") ?? "_Avatar Info";
                RemoveAll = GetString("ui_removeAll") ?? "_Remove All";
                Misc = GetString("ui_misc") ?? "_Misc";
                Thumbnails = GetString("ui_thumbnails") ?? "_Thumbnails";
                UseSceneSelection = GetString("ui_useSceneSelection") ?? "_Use Scene Selection";
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
                SelectFromScene = GetString("buttons_selectFromScene") ?? "_Select from Scene";
                CopySelected = GetString("buttons_copySelected") ?? "_Copy Selected";
                Refresh = GetString("buttons_refresh") ?? "_Refresh";
                Cancel = GetString("buttons_cancel") ?? "_Cancel";
                Apply = GetString("buttons_apply") ?? "_Apply";
                Copy = GetString("buttons_copyText") ?? "_Copy Text";
                OpenHelpPage = GetString("buttons_openHelpPage") ?? "_Open Help Page";
                OpenGithubPage = GetString("buttons_openGithubPage") ?? "_Open Github Page";
                OpenDonationPage = GetString("buttons_openDonationPage") ?? "_Buy me a Ko-Fi~";
                OpenPoseEditor = GetString("buttons_openPoseEditor") ?? "_Open Pose Editor";
                JoinDiscordServer = GetString("buttons_joinDiscordServer") ?? "_Join Discord Server!";
                SelectNone = GetString("buttons_selectNone") ?? "_Select None";
                SelectAll = GetString("buttons_selectAll") ?? "_Select All";
                Browse = GetString("buttons_browse") ?? "_Browse";
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
                FillVisemes = GetString("ui_tools_fillVisemes") ?? "_Fill Visemes";
                EditViewpoint = GetString("ui_tools_editViewpoint") ?? "_Edit Viewpoint";
                RevertBlendshapes = GetString("ui_tools_revertBlendShapes") ?? "_Revert Blendshapes";
                ZeroBlendshapes = GetString("ui_tools_zeroBlendShapes") ?? "_Zero Blendshapes";
                ResetPose = GetString("ui_tools_resetPose") ?? "_Reset Pose";
                ResetToTPose = GetString("ui_tools_resetToTPose") ?? "_Reset to T-Pose";
                EditScale = GetString("ui_tools_editScale") ?? "_Edit Scale";
            }
        };
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
                OverlayCameraImage = GetString("ui_thumbnails_overlayCameraImage") ?? "_Overlay Image";
                OverlayTexture = GetString("ui_thumbnails_overlayTexture") ?? "_Overlay Texture";
                StartUploadingFirst = GetString("ui_thumbnails_startUploadingFirst") ?? "_Start uploading an Avatar first";
                CenterCameraOnViewpoint = GetString("ui_thumbnails_centerCameraOnViewpoint") ?? "_Center Camera on Viewpoint";

                BackgroundType = GetString("ui_thumbnails_backgroundType") ?? "_Background Type";

                BackgroundType_None = GetString("ui_thumbnails_backgroundType_none") ?? "_None";
                BackgroundType_Material = GetString("ui_thumbnails_backgroundType_material") ?? "_Material";
                BackgroundType_Color = GetString("ui_thumbnails_backgroundType_color") ?? "_Color";
                BackgroundType_Image = GetString("ui_thumbnails_backgroundType_image") ?? "_Image";

                HideOtherAvatars = GetString("ui_thumbnails_hideOtherAvatars") ?? "_Hide Other Avatars when Uploading";
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

            static Copier()
            {
                Reload();
            }

            public static void Reload()
            {
                CopyFrom = GetString("ui_copier_copyFrom") ?? "_Copy From";

                CopySettings = GetString("ui_copier_copySettings") ?? "_Settings";
                CreateMissing = GetString("ui_copier_createMissing") ?? "_Create Missing";
                EmptyGameObjects = GetString("ui_copier_emptyGameObjects") ?? "_Empty GameObjects";
                ReplaceOld = GetString("ui_copier_replaceOld") ?? "_Replace Old";

                Transforms = GetString("ui_copier_transforms") ?? "_Transforms";
                Transforms_position = GetString("ui_copier_transforms_position") ?? "_Position";
                Transforms_rotation = GetString("ui_copier_transforms_rotation") ?? "_Rotation";
                Transforms_scale = GetString("ui_copier_transforms_scale") ?? "_Scale";
                Transforms_avatarScale = GetString("ui_copier_transforms_avatarScale") ?? "_Avatar Scale";
                DynamicBones = GetString("ui_copier_dynamicBones") ?? "_Dynamic Bones";
                DynamicBones_colliders = GetString("ui_copier_dynamicBones_colliders") ?? "_Colliders";
                DynamicBones_removeOldBones = GetString("ui_copier_dynamicBones_removeOld") ?? "_Remove Old Bones";
                DynamicBones_removeOldColliders = GetString("ui_copier_dynamicBones_removeOldColliders") ?? "_Remove Old Colliders";
                DynamicBones_createMissing = GetString("ui_copier_dynamicBones_createMissing") ?? "_Create Missing Bones";
                Colliders = GetString("ui_copier_colliders") ?? "_Colliders";
                Colliders_box = GetString("ui_copier_colliders_box") ?? "_Box Colliders";
                Colliders_capsule = GetString("ui_copier_colliders_capsule") ?? "_Capsule Colliders";
                Colliders_sphere = GetString("ui_copier_colliders_sphere") ?? "_Sphere Colliders";
                Colliders_mesh = GetString("ui_copier_colliders_mesh") ?? "_Mesh Colliders";
                Colliders_removeOld = GetString("ui_copier_colliders_removeOld") ?? "_Remove Old Colliders";
                Descriptor = GetString("ui_copier_descriptor") ?? "_Avatar Descriptor";
                Descriptor_pipelineId = GetString("ui_copier_descriptor_pipelineId") ?? "_Pipeline Id";
                Descriptor_animationOverrides = GetString("ui_copier_descriptor_animationOverrides") ?? "_Animation Overrides";
                SkinMeshRender = GetString("ui_copier_skinMeshRender") ?? "_Skinned Mesh Renderers";
                SkinMeshRender_materials = GetString("ui_copier_skinMeshRender_materials") ?? "_Materials";
                SkinMeshRender_blendShapeValues = GetString("ui_copier_skinMeshRender_blendShapeValues") ?? "_BlendShape Values";
                ParticleSystems = GetString("ui_copier_particleSystem") ?? "_Particle Systems";
                RigidBodies = GetString("ui_copier_rigidBodies") ?? "_Rigid Bodies";
                TrailRenderers = GetString("ui_copier_trailRenderers") ?? "_Trail Renderers";
                MeshRenderers = GetString("ui_copier_meshRenderers") ?? "_Mesh Renderers";
                CopyGameObjects = GetString("ui_copier_copyGameObjects") ?? "_Copy GameObjects";
                CopyColliderObjects = GetString("ui_copier_dynamicBones_copyColliderObjects") ?? "_Copy Collider Objects";
                Lights = GetString("ui_copier_lights") ?? "_Lights";
                Animators = GetString("ui_copier_animators") ?? "_Animators";
                CopyMainAnimator = GetString("ui_copier_animators_copyMain") ?? "_Copy Main Animator";
                Animators_inChildren = GetString("ui_copier_animatorsInChildren") ?? "_Child Animators";
                AudioSources = GetString("ui_copier_audioSources") ?? "_Audio Sources";
                Joints = GetString("ui_copier_joints") ?? "_Joints";
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
            public static string TryRemoveUnsupportedComponent { get; internal set; }
            public static string MeshHasNoVisemes { get; internal set; }
            public static string FailedIsNull { get; internal set; }
            public static string NameIsEmpty { get; internal set; }
            public static string LoadedPose { get; internal set; }
            public static string LoadedBlendshapePreset { get; internal set; }
            public static string NothingSelected { get; internal set; }
            public static string FailedDoesntHave { get; internal set; }
            public static string FailedNotSupported { get; internal set; }
            public static string FailedAlreadyHas { get; internal set; }
            public static string LoadedCameraOverlay { get; internal set; }
            public static string FailedHasNo { get; internal set; }

            static Log()
            {
                Reload();
            }

            public static void Reload()
            {
                Done = GetString("log_done") ?? "_Done";
                Cancelled = GetString("log_cancelled") ?? "_Cancelled";
                NothingSelected = GetString("log_nothingSelected") ?? "_Select something first";
                CantCopyToSelf = GetString("log_cantCopyToSelf") ?? "_Can't copy Components from an object to itself. What are you doing?";
                CopyAttempt = GetString("log_copyAttempt") ?? "_Attempting to copy {0} from {1} to {2}";
                RemoveAttempt = GetString("log_removeAttempt") ?? "_Attempting to remove {0} from {1}";
                CopyFromInvalid = GetString("log_copyFromInvalid") ?? "_Can't copy Components because 'Copy From' is invalid";
                ViewpointApplied = GetString("log_viewpointApplied") ?? "_Set Viewposition to {0}";
                ViewpointCancelled = GetString("log_viewpointCancelled") ?? "_Cancelled Viewposition changes";
                TryFillVisemes = GetString("log_tryFillVisemes") ?? "_Attempting to fill visemes on {0}";
                NoSkinnedMeshFound = GetString("log_noSkinnedMeshFound") ?? "_Failed: No skinned mesh found";
                DescriptorIsNull = GetString("log_descriptorIsNull") ?? "_Avatar descriptor is null";
                Success = GetString("log_success") ?? "_Success";
                MeshHasNoVisemes = GetString("log_meshHasNoVisemes") ?? "_Failed. Mesh has no Visemes. Set to Default";
                TryRemoveUnsupportedComponent = GetString("log_tryRemoveUnsupportedComponent") ?? "_Attempting to remove unsupported component {0} from {1}";
                Failed = GetString("log_failed") ?? "_Failed";
                FailedIsNull = GetString("log_failedIsNull") ?? "_Failed: {1} is null";
                NameIsEmpty = GetString("log_nameIsEmpty") ?? "_Name is Empty";
                LoadedPose = GetString("log_loadedPose") ?? "_Loaded Pose: {0}";
                LoadedBlendshapePreset = GetString("log_loadedBlendshapePreset") ?? "_Loaded Blendshapes: {0}";
                FailedDoesntHave = GetString("log_failedDoesntHave") ?? "_Failed: {0} doesn't have a {1}";
                FailedNotSupported = GetString("log_failedNotSupported") ?? "_Failed: {0} is not supported";
                FailedAlreadyHas = GetString("log_failedAlreadyHas") ?? "_Failed: {1} already has {0}";
                LoadedCameraOverlay = GetString("log_loadedCameraOverlay") ?? "_Loaded {0} as Camera Overlay";
                FailedHasNo = GetString("log_failedHasNo") ?? "_{0} has no {1}, Ignoring.";
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
                Warn = GetString("warn_warning") ?? "_Warning";
                NotFound = GetString("warn_notFound") ?? "_(Not Found)";
                OldVersion = GetString("warn_oldVersion") ?? "_(Old Version)";
                SelectSceneObject = GetString("warn_selectSceneObject") ?? "_Please select an object from the scene";
                VRCCamNotFound = GetString("warn_vrcCamNotFound") ?? "_VRCCam not found";
            }
        };
        public static class Credits
        {
            public static string Title { get; internal set; }
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
                Title = GetString("credits_title") ?? "_Pumkin's Avatar Tools";
                Version = GetString("credits_version") ?? "_Version" + " " + version;
                RedundantStrings = GetString("credits_redundantStrings") ?? "_Now with 100% more redundant strings";
                JsonDotNetCredit = GetString("credits_jsonDotNetCredit") ?? "_JsonDotNet by Newtonsoft";
                AddMoreStuff = GetString("credits_addMoreStuff") ?? "_I'll add more stuff to this eventually";
                PokeOnDiscord = GetString("credits_pokeOnDiscord") ?? "_Poke me on Discord at Pumkin#2020";
            }
        };
        public static class Misc
        {
            public static string uwu { get; internal set; }
            public static string SearchForBones { get; internal set; }
            public static string SuperExperimental { get; internal set; }

            private static string searchForBones;

            static Misc()
            {
                Reload();
            }

            public static void Reload()
            {
                uwu = GetString("misc_uwu") ?? "_uwu";
                SearchForBones = GetString("misc_searchForBones") ?? "_Search for DynamicBones";
                SuperExperimental = GetString("misc_superExperimental") ?? "_Super Experimental Stuff:";
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
            //Language Dictionaries
            dictionary_english = new Dictionary<string, string>
            {
#region Main
                //Main
                {"ui_main_title", "Pumkin's Avatar Tools" },
                {"ui_main_windowName", "Pumkin Tools" },
                {"ui_main_version", "Version" },
                {"ui_main_avatar", "Avatar" },
                {"ui_tools", "Tools" },
                {"ui_copier", "Copy Components" },
                {"ui_avatarInfo", "Avatar Info" },
                {"ui_thumbnails", "Thumbnails" },
                {"ui_misc", "Misc" },
                {"ui_removeAll", "Remove All" },
                {"ui_useSceneSelection", "Use Scene Selection" },

#region AvatarInfo
                { "ui_avatarInfo_name", "Name: {0}"},
                { "ui_avatarInfo_line", "---------------------"},
                { "ui_avatarInfo_gameobjects", "GameObjects: {0} ({1})"},
                { "ui_avatarInfo_bones", "Bones: {0} - {1}"},
                { "ui_avatarInfo_skinnedMeshRenderers", "Skinned Mesh Renderers: {0} ({1}) - {2}"},
                { "ui_avatarInfo_meshRenderers", "Mesh Renderers: {0} ({1}) - {2}"},
                { "ui_avatarInfo_polygons", "Polygons: {0} ({1}) - {2}"},
                { "ui_avatarInfo_usedMaterialSlots", "Used Material Slots: {0} ({1}) - {2}"},
                { "ui_avatarInfo_uniqueMaterials", "Unique Materials: {0} ({1})"},
                { "ui_avatarInfo_shaders", "Shaders: {0}"},
                { "ui_avatarInfo_dynamicBoneTransforms", "Dynamic Bone Transforms: {0} ({1}) - {2}"},
                { "ui_avatarInfo_dynamicBoneColliders", "Dynamic Bone Colliders: {0} ({1}) - {2}"},
                { "ui_avatarInfo_dynamicBoneColliderTransforms", "Collider Affected Transforms: {0} ({1}) - {2}"},
                { "ui_avatarInfo_particleSystems", "Particle Systems: {0} ({1}) - {2}"},
                { "ui_avatarInfo_maxParticles", "Max Particles: {0} ({1}) - {2}"},
                { "ui_avatarInfo_overallPerformance", "Overall Performance: {0}"},
                { "ui_avatarInfo_selectAvatarFirst", "Select an Avatar first"},                
#endregion

#region Buttons
                {"buttons_selectFromScene", "Select from Scene" },
                {"buttons_copySelected" , "Copy Selected" },
                {"buttons_refresh", "Refresh" },
                {"buttons_apply", "Apply" },
                {"buttons_cancel", "Cancel" },
                {"buttons_copyText", "Copy Text" },
                {"buttons_openHelpPage", "Open Help Page" },
                {"buttons_openGithubPage", "Open Github Page" },
                {"buttons_openDonationPage", "Buy me a Ko-Fi~" },
                {"buttons_openPoseEditor", "Open Pose Editor" },
                {"buttons_joinDiscordServer", "Join Discord Server!" },
                {"buttons_selectAll", "Select All" },
                {"buttons_selectNone", "Select None" },
                {"buttons_browse", "Browse" },
#endregion

#endregion
#region Tools
            //UI Tools                
                {"ui_tools_fillVisemes", "Fill Visemes" },
                {"ui_tools_editViewpoint", "Edit Viewpoint" },
                {"ui_tools_revertBlendShapes", "Revert Blendshapes" },
                {"ui_tools_zeroBlendShapes", "Zero Blendshapes" },
                {"ui_tools_resetPose", "Reset Pose" },
                {"ui_tools_resetToTPose", "Reset to T-Pose" },
                {"ui_tools_editScale", "Scale Avatar" },

#endregion
#region Copier
                //UI Copier
                {"ui_copier_copyFrom", "Copy from" },                

                //UI Copier Misc
                {"ui_copier_copyGameObjects", "Copy GameObjects" },
                {"ui_copier_emptyGameObjects", "Empty GameObjects" },
                {"ui_copier_copySettings", "Settings" },
                {"ui_copier_createMissing", "Create Missing" },
                {"ui_copier_replaceOld", "Replace Old" },

                //UI Copier Transforms
                {"ui_copier_transforms", "Transforms" },
                {"ui_copier_transforms_position", "Position" },
                {"ui_copier_transforms_rotation", "Rotation" },
                {"ui_copier_transforms_scale", "Scale" },
                {"ui_copier_transforms_avatarScale", "Avatar Scale" },
            
                //UI Copier Dynamic Bones
                {"ui_copier_dynamicBones", "Dynamic Bones" },
                {"ui_copier_dynamicBones_colliders", "Dynamic Bone Colliders" },
                {"ui_copier_dynamicBones_removeOld", "Remove Old Bones" },
                {"ui_copier_dynamicBones_removeOldColliders", "Remove Old Colliders" },
                {"ui_copier_dynamicBones_createMissing", "Create Missing Bones" },
                {"ui_copier_dynamicBones_copyColliderObjects" , "Copy Collider Objects" },


                //UI Copier Colliders
                {"ui_copier_colliders", "Colliders" },
                {"ui_copier_colliders_box", "Box Colliders" },
                {"ui_copier_colliders_capsule", "Capsule Colliders" },
                {"ui_copier_colliders_sphere", "Sphere Colliders" },
                {"ui_copier_colliders_mesh", "Mesh Colliders" },
                {"ui_copier_colliders_removeOld", "Remove Old Colliders" },                

                //UI Copier Avatar Descriptor
                {"ui_copier_descriptor", "Avatar Descriptor" },
                {"ui_copier_descriptor_pipelineId", "Pipeline Id" },
                {"ui_copier_descriptor_animationOverrides", "Animation Overrides" },

                //UI Copier Skinned Mesh Renderer
                {"ui_copier_skinMeshRender", "Skinned Mesh Renderers" },
                {"ui_copier_skinMeshRender_materials", "Materials" },
                {"ui_copier_skinMeshRender_blendShapeValues", "BlendShape Values" },

                //UI Copier Particle System
                {"ui_copier_particleSystem", "Particle Systems" },

                //UI Copier Rigid Bodies
                {"ui_copier_rigidBodies", "Rigid Bodies" },

                //UI Copier Trail Renderers
                {"ui_copier_trailRenderers", "Trail Renderers" },

                //UI Copier MeshRenderers
                {"ui_copier_meshRenderers",  "Mesh Renderers"},

                //UI Copier Lights
                {"ui_copier_lights",  "Lights"},
                
                //UI Copier Animators
                {"ui_copier_animators",  "Animators"},
                {"ui_copier_animators_copyMain", "Copy Main Animator" },
                {"ui_copier_animatorsInChildren", "Child Animators" },

                //UI Copier Audio Sources
                {"ui_copier_audioSources", "Audio Sources" },

                //UI Copier Joints
                {"ui_copier_joints", "Joints"},

#endregion

#region Thumbnails
                //Thumbnails                
                {"ui_thumbnails_overlayCameraImage", "Overlay Image" },
                {"ui_thumbnails_overlayTexture",  "Overlay Texture"},
                {"ui_thumbnails_startUploadingFirst", "Begin uploading an Avatar first" },
                {"ui_thumbnails_backgroundColor", "Background Color" },
                {"ui_thumbnails_centerCameraOnViewpoint", "Center Camera on Viewpoint" },

                {"ui_thumbnails_backgroundType", "Background Type" },
                {"ui_thumbnails_backgroundType_none", "None" },
                {"ui_thumbnails_backgroundType_material", "Material" },
                {"ui_thumbnails_backgroundType_color", "Color" },
                {"ui_thumbnails_backgroundType_image", "Image" },
                {"ui_thumbnails_hideOtherAvatars" , "Hide Other Avatars when Uploading"},
#endregion
#region PoseEditor
                //Pose Editor
                {"ui_poseEditor", "Pose Editor (Very Beta)"},
                {"ui_poseEditor_scene", "Scene"},
                {"ui_poseEditor_scene_loadAdditive", "Load Additive"},
                {"ui_poseEditor_scene_overrideLights", "Override Lights"},
                {"ui_poseEditor_avatarPosition", "Avatar Position"},
                {"ui_poseEditor_avatarPosition_overridePose", "Override Pose"},
                {"ui_poseEditor_scene_saveChanges", "Save Scene Changes"},
                {"ui_poseEditor_scene_unload", "Unload Scene"},
                {"ui_poseEditor_resetPosition", "Reset Position"},
                {"ui_poseEditor_pose", "Pose"},
                {"ui_poseEditor_newPose", "New Pose"},
                {"ui_poseEditor_onlySavePoseChanges", "Only Save Pose Changes"},
                {"ui_poseEditor_loadPose", "Load Pose"},
                {"ui_poseEditor_blendshapes", "Blendshapes"},
                {"ui_poseEditor_newPreset", "New Preset"},
                {"ui_poseEditor_loadPreset", "Load Preset"},
                {"ui_poseEditor_save", "Save"},
                {"ui_poseEditor_reload", "Reload"},
#endregion

#region Log
                //Log
                {"log_failed", "Failed" },
                {"log_cancelled", "Cancelled" },
                {"log_success", "Success" },
                {"log_nothingSelected" , "Select something first" },
                {"log_done", "Done. Check Unity Console for full Output Log" },
                {"log_copyAttempt", "Attempting to copy {0} from {1} to {2}" },
                {"log_removeAttempt", "Attempting to remove {0} from {1}" },
                {"log_copyFromInvalid", "Can't copy Components because 'Copy From' is invalid" },
                {"log_cantCopyToSelf", "Can't copy Components from an object to itself. What are you doing?" },
                {"log_viewpointApplied", "Set Viewposition to {0}" },
                {"log_viewpointCancelled", "Cancelled Viewposition changes" },
                {"log_tryFillVisemes", "Attempting to fill visemes on {0}" },
                {"log_noSkinnedMeshFound", "Failed: No skinned mesh found" },
                {"log_descriptorIsNull", "Avatar descriptor is null"},
                {"log_meshHasNoVisemes", "Failed. Mesh has no Visemes. Set to Default" },
                {"log_tryRemoveUnsupportedComponent", "Attempting to remove unsupported component {0} from {1}" },
                {"log_failedIsNull" , "Failed. {1} is null. Ignoring" },
                {"log_nameIsEmpty", "Name is empty" },
                {"log_loadedPose", "Loaded Pose: {0}"},
                {"log_loadedBlendshapePreset", "Loaded Blendshapes: {0}"},
                {"log_failedDoesntHave", "Failed: {0} doesn't have a {1}" },
                {"log_failedAlreadyHas", "Failed: {1} already has {0}" },
                {"log_loadedCameraOverlay", "Loaded {0} as Camera Overlay" },
                {"log_failedHasNo", "{0} has no {1}, Ignoring."},
#endregion

#region Warnings
                //Warnings
                { "log_warning", "Warning" },
                { "warn_selectSceneObject" , "Please select an object from the scene" },
                { "warn_notFound", "(Not Found)" },
                { "warn_oldVersion", "(Old Version)" },
                { "warn_poseEditor_bodyPositionYTooSmall", "humanPose.bodyPosition.y is {0}, you probably don't want that. Setting humanPose.bodyPosition.y to 1" },
                { "warn_vrcCamNotFound" , "VRCCam not found" },
#endregion

#region Credits
                //Credits
                { "credits_title", "Pumkin's Avatar Tools"},
                { "credits_version", "Version" + " " + version },
                { "credits_redundantStrings", "Now with 100% more redundant strings"},
                { "credits_jsonDotNetCredit", "JsonDotNet by Newtonsoft"},
                { "credits_addMoreStuff", "I'll add more stuff to this eventually" },
                { "credits_pokeOnDiscord", "Poke me on Discord at Pumkin#2020" },
#endregion

                //Misc                
                { "misc_uwu", "uwu" },
                { "misc_searchForBones", "Search for DynamicBones" },
                { "misc_superExperimental", "Super Experimental stuff" },
            };

            //Mistakes
            dictionary_uwu = new Dictionary<string, string>
            {
#region Main
                //Main
                {"ui_main_title", "Pumkin's Avataw Awoos! ÒwÓ" },
                {"ui_main_windowName", "Pumkin Awoos" },
                {"ui_main_version", "Vewsion~" },
                {"ui_main_avatar", "Avataw :o" },
                {"ui_thumbnails", "Thumbnyaiws >:3" },
                {"ui_tools", "Toows òwó" },
                {"ui_copier", "Copy Componyents uwu" },
                {"ui_avatarInfo", "Avataw Info 0w0" },
                {"ui_misc", "Misc ;o" },
                {"ui_removeAll", "Wemuv Aww (⁰д⁰ )" },

#region AvatarInfo
                { "ui_avatarInfo_name", "Nyame: {0}"},
                { "ui_avatarInfo_line", "---------------------"},
                { "ui_avatarInfo_gameobjects", "GamyeObwects: {0} ({1})"},
                { "ui_avatarInfo_bones", "Bonyes: {0} - {1}"},
                { "ui_avatarInfo_skinnedMeshRenderers", "Skinnyed Mesh Wendewews: {0} ({1}) - {2}"},
                { "ui_avatarInfo_meshRenderers", "Mesh Wendewews: {0} ({1}) - {2}"},
                { "ui_avatarInfo_polygons", "Powygons: {0} ({1}) - {2}"},
                { "ui_avatarInfo_usedMaterialSlots", "Used Matewiaw Swots: {0} ({1}) - {2}"},
                { "ui_avatarInfo_uniqueMaterials", "Unyique Matewiaws: {0} ({1})"},
                { "ui_avatarInfo_shaders", "Shadews: {0}"},
                { "ui_avatarInfo_dynamicBoneTransforms", "Dynyamic Bonye Twansfowms: {0} ({1}) - {2}"},
                { "ui_avatarInfo_dynamicBoneColliders", "Dynyamic Bonye Cowwidews: {0} ({1}) - {2}"},
                { "ui_avatarInfo_dynamicBoneColliderTransforms", "Cowwidew Affected Twansfowms: {0} ({1}) - {2}"},
                { "ui_avatarInfo_particleSystems", "Pawticwe Systems: {0} ({1}) - {2}"},
                { "ui_avatarInfo_maxParticles", "Max Pawticwes: {0} ({1}) - {2}"},
                { "ui_avatarInfo_overallPerformance", "Ovewaww Pewfowmance: {0}"},
                { "ui_avatarInfo_selectAvatarFirst", "Sewect an Avataw furst ewe" },
#endregion

#region Buttons
                {"buttons_selectFromScene", "Sewect fwom Scenye x3" },
                {"buttons_copySelected" , "Copy Sewected (´• ω •`)" },
                {"buttons_refresh", "Wefwesh (ﾟωﾟ;)" },
                {"buttons_apply", "Appwy （>﹏<）" },
                {"buttons_cancel", "Cancew ; o;" },
                {"buttons_copyText", "Cowpy OwO" },
                {"buttons_openHelpPage", "Opyen Hewp Paws uwu" },
                {"buttons_openGithubPage", "Opyen Gitwub Paws :o" },
                {"buttons_openDonationPage", "Buy Pumkin a Ko-Fi~ OwO" },
                {"buttons_openPoseEditor", "Open Paws Editow" },
#endregion

#endregion
#region Tools
                //UI Toows                
                {"ui_tools_fillVisemes", "Fiww Visemes ;~;" },
                {"ui_tools_editViewpoint", "Edit Viewpoint o-o" },
                {"ui_tools_revertBlendShapes", "Weset Bwendshapyes uwu" },
                {"ui_tools_zeroBlendShapes", "Zewo Bwendshapyes 0~0" },
                {"ui_tools_resetPose", "Weset Pose ;3" },
                {"ui_tools_resetToTPose", "Weset to T-Pose" },

#endregion
#region Copier
                //UI Copier
                {"ui_copier_copyFrom", "Copy fwom~" },                

                //UI Copier Transforms
                {"ui_copier_transforms", "Twansfowms!" },
                {"ui_copier_transforms_position", "Position~" },
                {"ui_copier_transforms_wotation", "Wotation @~@" },
                {"ui_copier_transforms_scawe", "Scawe www" },
                {"ui_copier_transforms_avatarScale", "Avatar Scale o-o" },

                //UI Copier Dynamic Bones
                {"ui_copier_dynamicBones", "Dynyamic Bonyes~" },
                {"ui_copier_dynamicBones_settings", "Settings º ^º" },
                {"ui_copier_dynamicBones_colliders", "Dynyamic Bonye Cowwidews~" },
                {"ui_copier_dynamicBones_removeOld", "Wemuv Owd Bonyes uwu" },
                {"ui_copier_dynamicBones_removeOldColliders", "Wemuv Owd Cowwidews ;w;" },
                {"ui_copier_dynamicBones_createMissing", "Cweate Missing Bonyes!" },

                //UI Copier Colliders
                {"ui_copier_colliders", "Cowwidews ;o;" },
                {"ui_copier_colliders_box", "Box Cowwidews!" },
                {"ui_copier_colliders_capsule", "Capsuwe Cowwidews o-o" },
                {"ui_copier_colliders_sphere", "Sphewe Cowwidews O~O" },
                {"ui_copier_colliders_mesh", "Mesh Cowwidews zzz" },
                {"ui_copier_colliders_removeOld", "Wemuv Owd Cowwidews uwu" },

                //UI Copier Avatar Descriptor
                {"ui_copier_descriptor", "Avataw Descwiptow~" },
                {"ui_copier_descriptor_settings", "Settings agen" },
                {"ui_copier_descriptor_pipelineId", "Pipewinye Id!" },
                {"ui_copier_descriptor_animationOverrides", "Anyimation Ovewwides :o" },

                //UI Copier Skinned Mesh Renderer
                {"ui_copier_skinMeshRender", "Skinnyed Mesh Wendewews ;w;" },
                {"ui_copier_skinMeshRender_settings", "Settings ageeen" },
                {"ui_copier_skinMeshRender_materials", "Matewiaws uwu" },
                {"ui_copier_skinMeshRender_blendShapeValues", "BwendShape Vawues ùwú" },

                //UI Copier Particle System
                {"ui_copier_particleSystem", "Pawtickle Systums zzz" },

                //UI Copier Rigid Bodies
                {"ui_copier_rigidBodies", "Wigid Bodyes" },                

                //UI Copier Trail Renderers
                {"ui_copier_trailRenderers", "Twail Wendewuws" },

                //UI Copier MeshRenderers
                {"ui_copier_meshRenderers",  "Mesh Wenderur"},

                //UI Copier Lights
                {"ui_copier_lights",  "Wighties"},
                
                //UI Copier Animators
                {"ui_copier_animators",  "Anyanmaytows"},
                {"ui_copier_animators_copyMain", "Copy main Anyanmaytow" },
                {"ui_copier_animatorsInChildren", "Smol Anyanmaytows" },

                //UI Copier Audio Sources
                { "ui_copier_audioSources", "Awwdio Sauces" },

                //UI Copier Joints
                {"ui_copier_joints", "Joints"},

#endregion

#region Thumbnails
                //Thumbnails                
                { "ui_thumbnails_overlayCameraImage", "Ovewwide Camewa Image" },
                { "ui_thumbnails_overlayTexture",  "Ovewwide Textuwe"},
                { "ui_thumbnails_startUploadingFirst", "Stawt upwoading Avataw fiwst!!" },
                { "ui_thumbnails_backgroundColor", "Background Color" },
                { "ui_thumbnails_centerCameraOnViewpoint", "Center Camera on Viewpoint" },
#endregion
#region PoseEditor
                //Pose Editor
                {"ui_poseEditor", "Pose Editow (Vewy Beta)"},
                {"ui_poseEditor_scene", "Scenye"},
                {"ui_poseEditor_scene_loadAdditive", "Woad Addititive"},
                {"ui_poseEditor_scene_overrideLights", "Ovewwide Wights"},
                {"ui_poseEditor_avatarPosition", "Avataw Position"},
                {"ui_poseEditor_avatarPosition_overridePose", "Ovewwide Pose"},
                {"ui_poseEditor_scene_saveChanges", "Save Scenye Changes"},
                {"ui_poseEditor_scene_unload", "Unwoad Scenye"},
                {"ui_poseEditor_resetPosition", "Weset Position"},
                {"ui_poseEditor_pose", "Paws"},
                {"ui_poseEditor_newPose", "Nyew Paws"},
                {"ui_poseEditor_onlySavePoseChanges", "Onwy Save Paws Changes"},
                {"ui_poseEditor_loadPose", "Woad Paws"},
                {"ui_poseEditor_blendshapes", "Bwendshapes"},
                {"ui_poseEditor_newPreset", "Nyew Pweset"},
                {"ui_poseEditor_loadPreset", "Woad Pweset"},
                {"ui_poseEditor_save", "Save"},
                {"ui_poseEditor_reload", "Wewoad"},
#endregion

#region Log
                //Log
                {"log_failed", "Faiwed ùwú" },
                {"log_cancelled", "Cancewwed .-." },
                {"log_success", "Success OWO" },
                {"log_nothingSelected" , "Sewect sumstuf furst uwu" },
                {"log_done", "Donye. Check Unyity Consowe fow fuww Output Wog uwus" },
                {"log_copyAttempt", "Attempting to copy {0} fwom {1} to {2} o-o" },
                {"log_remuveAttempt", "Attempting to wemuv {0} fwom {1} ;-;" },
                {"log_copyFromInvalid", "Can't copy Componyents because 'Copy Fwom' is invawid ; o ;" },
                {"log_cantCopyToSelf", "Can't copy Componyents fwom an object to itsewf. What awe you doing? ;     w     ;" },
                {"log_viewpointApplied", "Set Viewposition to {0}!" },
                {"log_viewpointCancelled", "Cancewwed Viewposition changes uwu" },
                {"log_tryFixVisemes", "Attempting to fiww visemes on {0}!" },
                {"log_noSkinnedMeshFound", "Faiwed: Nyo skinnyed mesh found ;o;" },
                {"log_descriptorIsNull", "Avataw descwiptow is nyuww humpf"},
                {"log_meshHasNoVisemes", "Faiwed. Mesh has nyo Visemes. Set to Defauwt ;w;" },
                {"log_tryRemoveUnsupportedComponent", "Attempted to wemuv unsuppowted componyent {0} fwom {1} uwu7" },
                {"log_failedIsNull" , "Faiwed {1} is nyull /w\\. Ignyowing uwu" },
                {"log_nameIsEmpty", "Nyame ish emptyyy ;w;" },
                {"log_loadedPose", "Woaded Paws: {0}"},
                {"log_loadedBlendshapePreset", "Woaded Bwendshapyes: {0}"},
                {"log_failedDoesntHave", "Faiwed: {0} dun have a {1} ;o;" },
                {"log_failedAlreadyHas", "Faiwed: {0} alredy has {1} ;w;" },
                {"log_loadedCameraOverlay", "Loaded {0} as Camera Overlay" },
                {"log_failedHasNo", "{0} has no {1}, Ignoring."},
#endregion

#region Warnings
                //Warnings
                { "log_warning", "Wawnying! unu" },
                { "warn_selectSceneObject" , "Pwease sewect an object fwom the scenye!!" },
                { "warn_notFound", "(Nyot Fownd ;~;)" },
                { "warn_oldVersion", "(Old Version)" },
                { "warn_poseEditor_bodyPositionYTooSmall", "humanPose.bodyPosition.y is {0}, you pwobabwy don't want that. Setting humanPose.bodyPosition.y to 1 uwu" },
#endregion

#region Credits
                //Credits
                { "credits_title", "Pumkin's Avataw Awoos~ :3"},
                { "credits_version", "Vewsion" + " " + version },
                { "credits_redundantStrings", "Nyow with 0W0% mowe noticin things~"},
                { "credits_jsonDotNetCredit", "JsonDotNet by Newtonsoft" },
                { "credits_addMoreStuff", "I'ww add mowe stuff to this eventuawwy >w<" },
                { "credits_pokeOnDiscord", "Poke me! But on Discowd at Pumkin#2020~ uwus" },
#endregion

                //Misc                
                { "misc_uwu", "OwO" },
                { "misc_searchForBones", "Seawch fow DynyamicBonyes" },
                { "misc_superExperimental", "Supew Scawy stuffs ヾ(´囗｀｡)ﾉ" },
            };

            stringDictionary = dictionary_english;
            language = DictionaryLanguage.English;
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
        }

        static string GetString(string stringName)
        {
            if(string.IsNullOrEmpty(stringName))
                return stringName;

            string s = string.Empty;
            stringDictionary.TryGetValue(stringName, out s);

            return s;
        }
    };

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
                PumkinsAvatarTools.Log("_GameObject {0} not found in {1}. Ignoring", LogType.Warning, PumkinsAvatarTools.GetNameFromPath(transformPath), avatar.name);
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
    #endregion

    #region Extensions
    public static class PumkinsAvatarToolsExtensions
    {
        public static Transform Find(this Transform transform, string name, bool createIfMissing = false, Transform sourceTransform = null)
        {
            var t = transform.Find(name);

            if(t == null && createIfMissing)
            {
                var arr = PumkinsAvatarTools.GetPathAsArray(name);
                if(arr.Length > 0)
                {
                    string path = "";
                    for(int i = 0; i < arr.Length; i++)
                    {
                        path += arr[i] + '/';
                        var tNew = transform.Find(path);

                        if(tNew == null)
                        {
                            string s = PumkinsAvatarTools.GetPathNoName(path);
                            var parent = transform.Find(s);

                            if(!parent)
                                return null;

                            tNew = new GameObject(arr[i]).transform;
                            tNew.parent = parent;

                            var trans = sourceTransform.root.Find(s + arr[i]);
                            if(trans)
                            {
                                tNew.localScale = Vector3.one;
                                tNew.localPosition = trans.localPosition;
                                tNew.localRotation = trans.localRotation;
                                tNew.localEulerAngles = trans.localEulerAngles;
                                tNew.localScale = trans.localScale;

                                tNew.gameObject.tag = trans.gameObject.tag;
                                tNew.gameObject.layer = trans.gameObject.layer;
                                tNew.gameObject.SetActive(trans.gameObject.activeInHierarchy);

                            }
                            else
                            {
                                tNew.localPosition = Vector3.zero;
                                tNew.localRotation = Quaternion.identity;
                                tNew.localEulerAngles = Vector3.zero;
                                tNew.localScale = Vector3.one;
                            }
                            t = tNew;
                        }
                    }
                }
            }
            return t;
        }

        public static bool IsSameTexture(this Texture2D first, Texture2D second)
        {
            Color[] firstPix = first.GetPixels();
            Color[] secondPix = second.GetPixels();
            if(firstPix.Length != secondPix.Length)
            {
                return false;
            }
            for(int i = 0; i < firstPix.Length; i++)
            {
                if(firstPix[i] != secondPix[i])
                {
                    return false;
                }
            }
            return true;
        }

    }
    #endregion
}
