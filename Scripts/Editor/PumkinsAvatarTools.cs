#define OLD_BONES
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

/// <summary>
/// PumkinsAvatarTools by, well, Pumkin
/// https://github.com/rurre/PumkinsAvatarTools
/// </summary>

namespace Pumkin.AvatarTools
{
    [ExecuteInEditMode, CanEditMultipleObjects]
    public class PumkinsAvatarTools : EditorWindow
    {
#region Variables
        
        //Tools
        public static GameObject selectedAvatar;
        static bool _useSceneSelectionAvatar = false;

        //Component Copier
        public static GameObject copierSelectedFrom;
        
        bool bCopier_transforms_copy = true;
        bool bCopier_transforms_copyPosition = false;
        bool bCopier_transforms_copyRotation = false;
        bool bCopier_transforms_copyScale = false;
        bool bCopier_transforms_copyAvatarScale = true;

        bool bCopier_dynamicBones_copy = true;
        bool bCopier_dynamicBones_copySettings = true;        
        bool bCopier_dynamicBones_createMissingBones = true;
        bool bCopier_dynamicBones_copyColliders = true;        
        bool bCopier_dynamicBones_removeOldColliders = false;
        bool bCopier_dynamicBones_removeOldBones = false;

        bool bCopier_descriptor_copy = true;
        bool bCopier_descriptor_copySettings = true;
        bool bCopier_descriptor_copyPipelineId = false;
        bool bCopier_descriptor_copyAnimationOverrides = true;

        bool bCopier_colliders_copy = true;        
        bool bCopier_colliders_removeOld = false;
        bool bCopier_colliders_copyBox = true;
        bool bCopier_colliders_copyCapsule = true;
        bool bCopier_colliders_copySphere = true;
        bool bCopier_colliders_copyMesh = false;
        bool bCopier_colliders_createObjects = true;

        bool bCopier_skinMeshRender_copy = true;
        bool bCopier_skinMeshRender_copySettings = false;
        bool bCopier_skinMeshRender_copyBlendShapeValues = true;
        bool bCopier_skinMeshRender_copyMaterials = false;
                
        bool bCopier_particleSystems_copy = true;
        bool bCopier_particleSystems_replace = false;
        bool bCopier_particleSystems_createObjects = true;

        bool bCopier_rigidBodies_copy = true;
        bool bCopier_rigidBodies_copySettings = true;
        bool bCopier_rigidBodies_createMissing = true;
        bool bCopier_rigidBodies_createObjects = true;

        bool bCopier_trailRenderers_copy = true;
        bool bCopier_trailRenderers_copySettings = true;
        bool bCopier_trailRenderers_createMissing = true;
        bool bCopier_trailRenderers_createObjects = true;

        bool bCopier_meshRenderers_copy = true;
        bool bCopier_meshRenderers_copySettings = true;
        bool bCopier_meshRenderers_createMissing = true;
        bool bCopier_meshRenderers_createObjects = true;        

        bool bCopier_lights_copy = true;
        bool bCopier_lights_copySettings = true;
        bool bCopier_lights_createMissing = true;
        bool bCopier_lights_createObjects = true;

        bool bCopier_animators_copy = true;
        bool bCopier_animators_copySettings = true;
        bool bCopier_animators_createMissing = false;
        bool bCopier_animators_createObjects = false;
        bool bCopier_animators_copyMainAnimator = false;

        bool bCopier_joints_copy = true;
        //bool bCopier_joints_copySettings = true;
        //bool bCopier_joints_fixed = true;
        //bool bCopier_joints_character = true;
        //bool bCopier_joints_configurable = true;
        //bool bCopier_joints_sprint = true;
        //bool bCopier_joints_hinge = true;
        //bool bCopier_joints_createMissing = true;
        //bool bCopier_joints_createObjects = true;

        bool bCopier_audioSources_copy = true;
        bool bCopier_audioSources_copySettings = true;
        bool bCopier_audioSources_createMissing = true;
        bool bCopier_audioSources_createObjects = true;

        //Editor
        bool _copier_expand = false;
        bool _copier_expand_transforms = false;
        bool _copier_expand_dynamicBones = false;
        bool _copier_expand_avatarDescriptor = false;
        bool _copier_expand_skinnedMeshRenderer = false;
        bool _copier_expand_colliders = false;        
        bool _copier_expand_particleSystems = false;
        bool _copier_expand_rigidBodies = false;        
        bool _copier_expand_trailRenderers = false;
        bool _copier_expand_meshRenderers = false;
        bool _copier_expand_lights = false;
        bool _copier_expand_animators = false;
        bool _copier_expand_joints = false;
        bool _copier_expand_audioSources = false;

        //Thumbnails        
        bool bThumbnails_override_camera_image = false;        
        GameObject cameraOverlay = null;
        RawImage cameraOverlayImage = null;

        Texture2D cameraOverrideTexture;
        string cameraOverrideTexturePath = "";
                
        bool vrcCamSetBgColor = false;        
        Camera _vrcCam = null;
        Color vrcCamBgColor = new Color(0.235f, 0.22f, 0.22f);
        Color _vrcCamColorOld = Color.blue;
        CameraClearFlags _vrcCamOldClearFlags = CameraClearFlags.Skybox;


        //UI
        bool _tools_expand = true;
        bool _avatarInfo_expand = false;
        bool _thumbnails_expand = false;        
        Vector2 _avatarInfo_scroll = Vector2.zero;
        
        bool _misc_expand = true;

        //Misc
        bool _openedInfo = false;
        Vector2 vertScroll = Vector2.zero;
        static string _mainScriptPath = null;        

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

        //Editing Viewpoint        
		bool _editingView = false;                
        Vector3 _viewPosOld;
		Vector3 _viewPosTemp;
        Tool _viewPosToolOld = Tool.None;        

        VRC_AvatarDescriptor _viewPos_descriptor;

        static AvatarInfo avatarInfo = null;                
        static string _avatarInfoSpace = "\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n";
        static string _avatarInfoString = Strings.AvatarInfo.SelectAvatarFirst + _avatarInfoSpace; //Please don't hurt me for this        

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
        };        

        static readonly Type[] supportedComponents =
        {
#if !NO_BONES
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

#region Unity GUI

        void OnEnable()
        {
            SceneView.onSceneGUIDelegate += this.OnSceneGUI;

            Selection.selectionChanged += () =>
            {
                if(_useSceneSelectionAvatar)
                    SelectAvatarFromScene();
            };

            cameraOverrideTexture = new Texture2D(2, 2);
        }

        void OnDisable()
        {
            SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
        }

        //[MenuItem("Tools/Pumkin/Avatar Tools")] //New window is responsible for calling this, for now
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
            int tempSize = Styles.Label_mainTitle.fontSize + 6;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(Strings.Main.Title, Styles.Label_mainTitle, GUILayout.MinHeight(tempSize));

            EditorGUIUtility.SetIconSize(new Vector2(tempSize - 3, tempSize - 3));
                        
            if(GUILayout.Button(Icons.Star, "IconButton", GUILayout.MaxWidth(tempSize + 3)))
            {
                _openedInfo = !_openedInfo;
            }            
            EditorGUILayout.EndHorizontal();
                        
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
                EditorGUIUtility.SetIconSize(new Vector2(15,15));

                EditorGUILayout.Space();

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

                vertScroll = EditorGUILayout.BeginScrollView(vertScroll);                

                EditorGUILayout.Space();

                //Tools menu
                if(_tools_expand = GUILayout.Toggle(_tools_expand, Strings.Main.Tools, Styles.Foldout_title))
                {
                    EditorGUI.BeginDisabledGroup(selectedAvatar == null);
                    {
                        EditorGUILayout.Space();

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

                                EditorGUI.BeginDisabledGroup(_editingView);
                                {
                                    if(GUILayout.Button(Strings.Tools.EditViewpoint))
                                        ActionButton(ToolMenuActions.EditViewpoint);
                                }
                                EditorGUI.EndDisabledGroup();

                                if(GUILayout.Button(Strings.Tools.ZeroBlendshapes))
                                    ActionButton(ToolMenuActions.ZeroBlendshapes);

                                if(GUILayout.Button(Strings.Tools.ResetToTPose))
                                    ActionButton(ToolMenuActions.SetTPose);
                            }
                            GUILayout.EndVertical();
                        }
                        GUILayout.EndHorizontal();

                        EditorGUILayout.Space();

                        EditorGUILayout.LabelField(Strings.Main.RemoveAll + ":");

                        EditorGUILayout.BeginHorizontal();

                        EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true)); //Left Column
#if NO_BONES
                        EditorGUI.BeginDisabledGroup(true);                        
#endif
                        if(GUILayout.Button(new GUIContent(Strings.Copier.DynamicBones, Icons.BoneIcon)))                        
                            ActionButton(ToolMenuActions.RemoveDynamicBones);
#if NO_BONES
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

#if NO_BONES
                        EditorGUI.BeginDisabledGroup(true);
#endif
                        EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true)); //Right Column
                        if(GUILayout.Button(new GUIContent(Strings.Copier.DynamicBones_colliders, Icons.BoneColliderIcon)))
                            ActionButton(ToolMenuActions.RemoveDynamicBoneColliders);
#if NO_BONES
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

                DrawGuiLine();
                
                //Component Copier menu
                if(_copier_expand = GUILayout.Toggle(_copier_expand, Strings.Main.Copier, Styles.Foldout_title))
                {
                    EditorGUILayout.Space();
                    
                    copierSelectedFrom = (GameObject)EditorGUILayout.ObjectField(Strings.Copier.CopyFrom + ":", copierSelectedFrom, typeof(GameObject), true);

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
#if NO_BONES
                        EditorGUI.BeginDisabledGroup(true);
#endif
                        EditorGUILayout.BeginHorizontal();
                        _copier_expand_dynamicBones = GUILayout.Toggle(_copier_expand_dynamicBones, Icons.BoneIcon, "Foldout", GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(true), GUILayout.MaxWidth(30), GUILayout.MaxHeight(10));
#if NO_BONES
                        bCopier_dynamicBones_copy = GUILayout.Toggle(false, Strings.Copier.DynamicBones + " " + Strings.Warning.NotFound, GUILayout.ExpandWidth(false), GUILayout.MinWidth(20));
#else
                        bCopier_dynamicBones_copy = GUILayout.Toggle(bCopier_dynamicBones_copy, Strings.Copier.DynamicBones, GUILayout.ExpandWidth(false), GUILayout.MinWidth(20));
#endif
                        EditorGUILayout.EndHorizontal();

                        if(_copier_expand_dynamicBones)
                        {
                            EditorGUI.BeginDisabledGroup(!bCopier_dynamicBones_copy);
                            EditorGUILayout.Space();

                            bCopier_dynamicBones_copySettings = EditorGUILayout.Toggle(Strings.Copier.CopySettings, bCopier_dynamicBones_copySettings, GUILayout.ExpandWidth(false));
                            bCopier_dynamicBones_copyColliders = EditorGUILayout.Toggle(Strings.Copier.DynamicBones_colliders, bCopier_dynamicBones_copyColliders, GUILayout.ExpandWidth(false));
                            bCopier_dynamicBones_createMissingBones = EditorGUILayout.Toggle(Strings.Copier.DynamicBones_createMissingBones, bCopier_dynamicBones_createMissingBones, GUILayout.ExpandWidth(false));
                            bCopier_dynamicBones_removeOldBones = EditorGUILayout.Toggle(Strings.Copier.DynamicBones_removeOldBones, bCopier_dynamicBones_removeOldBones, GUILayout.ExpandWidth(false));
                            bCopier_dynamicBones_removeOldColliders = EditorGUILayout.Toggle(Strings.Copier.DynamicBones_removeOldColliders, bCopier_dynamicBones_removeOldColliders, GUILayout.ExpandWidth(false));

                            EditorGUILayout.Space();
                            EditorGUI.EndDisabledGroup();
                        }
#if NO_BONES
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
                                    bCopier_colliders_copy = false; bCopier_joints_copy = false; bCopier_descriptor_copy = false; bCopier_dynamicBones_copy = false;
                                    bCopier_lights_copy = false; bCopier_meshRenderers_copy = false; bCopier_particleSystems_copy = false; bCopier_rigidBodies_copy = false;
                                    bCopier_skinMeshRender_copy = false; bCopier_trailRenderers_copy = false; bCopier_transforms_copy = false; bCopier_animators_copy = false;
                                    bCopier_audioSources_copy = false;
                                }
                                if(GUILayout.Button(Strings.Buttons.SelectAll, GUILayout.MinWidth(100)))
                                {
                                    bCopier_colliders_copy = true; bCopier_joints_copy = true; bCopier_descriptor_copy = true; bCopier_dynamicBones_copy = true;
                                    bCopier_lights_copy = true; bCopier_meshRenderers_copy = true; bCopier_particleSystems_copy = true; bCopier_rigidBodies_copy = true;
                                    bCopier_skinMeshRender_copy = true; bCopier_trailRenderers_copy = true; bCopier_transforms_copy = true; bCopier_animators_copy = true;
                                    bCopier_audioSources_copy = true;
                                }
                            }
                            EditorGUILayout.EndHorizontal();

                            EditorGUI.BeginDisabledGroup(!CopierHasSelections());
                            {
                                if(GUILayout.Button(Strings.Buttons.CopySelected, GUILayout.MinHeight(25)))
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
                            _avatarInfoString = Strings.AvatarInfo.SelectAvatarFirst + _avatarInfoSpace; //Please don't hurt me
                        }
                    }
                    else
                    {
                        if(avatarInfo == null)
                        {
                            avatarInfo = AvatarInfo.GetInfo(selectedAvatar, out _avatarInfoString);                            
                        }                        
                    }
                    _avatarInfo_scroll = EditorGUILayout.BeginScrollView(_avatarInfo_scroll, GUILayout.MinHeight(250));
                    EditorGUILayout.HelpBox(_avatarInfoString, MessageType.None, true);                    
                    EditorGUILayout.EndScrollView();

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
                }
                EditorGUI.EndDisabledGroup();

                DrawGuiLine();

                //Thumbnails menu                
                if(_thumbnails_expand = GUILayout.Toggle(_thumbnails_expand, Strings.Main.Thumbnails, Styles.Foldout_title))
                {                            
                    //Camera Override Image
                    EditorGUI.BeginDisabledGroup(!EditorApplication.isPlaying);
                    {
                        EditorGUI.BeginChangeCheck();
                        {
                            EditorGUILayout.BeginHorizontal();
                            {
                                bThumbnails_override_camera_image = GUILayout.Toggle(bThumbnails_override_camera_image, Strings.Thumbnails.OverlayCameraImage);
                                EditorGUI.BeginDisabledGroup(!bThumbnails_override_camera_image);
                                {
                                    //cameraOverrideTexture = EditorGUILayout.ObjectField(Strings.Thumbnails.OverlayTexture, cameraOverrideTexture, typeof(Texture2D), false) as Texture2D;
                                    
                                    if(GUILayout.Button("Browse"))
                                    {
                                        var path = EditorUtility.OpenFilePanel("Pick an Image", cameraOverrideTexturePath, "png,jpg,jpeg");
                                        if(File.Exists(path))
                                        {
                                            byte[] data = File.ReadAllBytes(path);                                            
                                            cameraOverrideTexture.LoadImage(data);
                                            cameraOverrideTexturePath = path;
                                            cameraOverrideTexture.alphaIsTransparency = true;
                                            Log(Strings.Log.LoadedCameraOverlay, LogType.Log, path);
                                        }
                                    }
                                    if(GUILayout.Button("X", GUILayout.MaxWidth(25)))
                                    {
                                        cameraOverrideTexture = new Texture2D(2,2);                                        
                                    }                                    
                                }
                                EditorGUI.EndDisabledGroup();
                            }
                            EditorGUILayout.EndHorizontal();
                        }                        
                        if(EditorGUI.EndChangeCheck())
                        {
                            if(bThumbnails_override_camera_image)
                            {
                                if(cameraOverrideTexture != null)
                                {                                    
                                    if(cameraOverlay == null && FindVRCCam())
                                    {
                                        cameraOverlay = new GameObject("_PumkinsCameraOverlay");
                                        Canvas c = cameraOverlay.AddComponent<Canvas>();
                                        c.worldCamera = _vrcCam.GetComponent<Camera>();
                                        c.renderMode = RenderMode.ScreenSpaceCamera;
                                        c.planeDistance = 0.02f;
                                    }

                                    cameraOverlayImage = cameraOverlay.GetComponent<RawImage>();
                                    if(cameraOverlayImage == null)
                                        cameraOverlayImage = cameraOverlay.AddComponent<RawImage>();

                                    cameraOverlayImage.texture = cameraOverrideTexture;
                                }
                            }
                            else
                            {
                                if(cameraOverlay != null)
                                    GameObject.Destroy(cameraOverlay);
                            }
                        }
                    }
                    EditorGUI.EndDisabledGroup();

                    //if(GUILayout.Button(Strings.Buttons.OpenPoseEditor))
                    //{
                    //    PumkinsPoseEditor.ShowWindow();
                    //}      

                    EditorGUILayout.Space();
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUI.BeginDisabledGroup(!EditorApplication.isPlaying);
                        {
                            EditorGUI.BeginChangeCheck();
                            {
                                vrcCamSetBgColor = GUILayout.Toggle(vrcCamSetBgColor, Strings.Thumbnails.SetBackgroundColor);
                            }
                            if(EditorGUI.EndChangeCheck())
                            {
                                if(FindVRCCam())
                                {
                                    if(vrcCamSetBgColor)
                                    {
                                        _vrcCamOldClearFlags = _vrcCam.clearFlags;
                                        _vrcCamColorOld = _vrcCam.backgroundColor;
                                        _vrcCam.clearFlags = CameraClearFlags.SolidColor;
                                        _vrcCam.backgroundColor = vrcCamBgColor;
                                    }
                                    else
                                    {
                                        _vrcCam.clearFlags = _vrcCamOldClearFlags;
                                        _vrcCam.backgroundColor = _vrcCamColorOld;
                                    }
                                }
                            }
                        }
                        EditorGUI.EndDisabledGroup();
                        EditorGUI.BeginDisabledGroup(!vrcCamSetBgColor);
                        {
                            EditorGUI.BeginChangeCheck();
                            {
                                vrcCamBgColor = EditorGUILayout.ColorField(vrcCamBgColor);
                            }
                            if(EditorGUI.EndChangeCheck())
                            {
                                if(FindVRCCam())
                                {
                                    _vrcCam.backgroundColor = vrcCamBgColor;
                                }
                            }
                        }
                        EditorGUI.EndDisabledGroup();
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.Space();

                    EditorGUI.BeginDisabledGroup(!EditorApplication.isPlaying);
                    {
                        if(GUILayout.Button(Strings.Thumbnails.CenterCameraOnFace))
                        {
                            CenterCameraOnFace(selectedAvatar);
                        }
                    }
                    EditorGUI.EndDisabledGroup();

                    EditorGUILayout.Space();

                    if(!EditorApplication.isPlaying)
                    {
                        EditorGUILayout.HelpBox(Strings.Thumbnails.StartUploadingFirst, MessageType.Info);
                    }
                }                

                if(!EditorApplication.isPlaying)
                {
                    if(bThumbnails_override_camera_image)
                    {
                        cameraOverrideTexture = null;
                        bThumbnails_override_camera_image = false;
                    }

                    if(cameraOverlay != null)
                        GameObject.Destroy(cameraOverlay);
                }

                DrawGuiLine();

                //Misc menu
                if(_misc_expand = GUILayout.Toggle(_misc_expand, Strings.Main.Misc, Styles.Foldout_title))
                {
                    EditorGUILayout.Space();
#if NO_BONES
                    if(GUILayout.Button(Strings.Misc.SearchForBones))
                    {
                        _DependecyChecker.Check();
                    }
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
                            //GUILayout.MaxWidth(tempSize + 3), GUILayout.MaxHeight(tempSize + 3)
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

                EditorGUILayout.EndScrollView();
            }
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

        private bool FindVRCCam()
        {
            if(_vrcCam == null)
            {
                var c = GameObject.Find("VRCCam");
                if(c != null)
                {
                    _vrcCam = c.GetComponent<Camera>();
                    return true;
                }
            }
            else
                return true;

            return false;
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
            if(_editingView)
            {
                if(selectedAvatar == null)
                {
                    EndEditingViewpoint(null, true);
                    return;
                }

                Vector2 windowSize = new Vector2(200, 50);

                Handles.BeginGUI();
                {
                    var r = SceneView.currentDrawingSceneView.camera.pixelRect;
                    GUILayout.BeginArea(new Rect(10, r.height - 10 - windowSize.y, windowSize.x, windowSize.y), new GUIStyle("box"));
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

                EditorGUI.BeginChangeCheck();
                {
                    if(_viewPos_descriptor != null)
                    {
                        float time = Time.realtimeSinceStartup;                                               
                        
                        _viewPosTemp = Handles.PositionHandle(_viewPosTemp, Quaternion.identity);
                        Handles.color = new Color(1, 0.92f, 0.016f, 0.5f);
                        Handles.SphereHandleCap(0, _viewPosTemp, Quaternion.identity, 0.02f, EventType.Repaint);                        

                        //if(_viewPosSetTimeNext >= time)
                        //{
                        //    _viewPos_descriptor.ViewPosition = _viewPosTemp;
                        //    _viewPosSetTimeNext = time + viewChangeDelayTime;
                        //}						
                    }
                }
            }
        }

        void OnDrawGizmos()
        {
            Debug.Log("Drawing!");
            if(_editingView)
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
                    DestroyAllComponentsOfType(selectedAvatar, typeof(Collider));
                    break;
                case ToolMenuActions.RemoveDynamicBoneColliders:
#if !NO_BONES
                    DestroyAllComponentsOfType(selectedAvatar, typeof(DynamicBoneCollider));
#endif
                    break;
                case ToolMenuActions.RemoveDynamicBones:
#if !NO_BONES
                    DestroyAllComponentsOfType(selectedAvatar, typeof(DynamicBone));
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
                    BeginEditViewpoint(selectedAvatar);
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
                    DestroyAllComponentsOfType(selectedAvatar, typeof(ParticleSystem));
                    DestroyAllComponentsOfType(selectedAvatar, typeof(ParticleSystemRenderer));
                    break;
                case ToolMenuActions.RemoveRigidBodies:
                    DestroyAllComponentsOfType(selectedAvatar, typeof(Rigidbody), true);
                    break;
                case ToolMenuActions.RemoveTrailRenderers:
                    DestroyAllComponentsOfType(selectedAvatar, typeof(TrailRenderer), false);
                    break;
                case ToolMenuActions.RemoveMeshRenderers:
                    DestroyAllComponentsOfType(selectedAvatar, typeof(MeshFilter));
                    DestroyAllComponentsOfType(selectedAvatar, typeof(MeshRenderer));
                    break;
                case ToolMenuActions.RemoveLights:
                    DestroyAllComponentsOfType(selectedAvatar, typeof(Light));
                    break;
                case ToolMenuActions.RemoveAnimatorsInChildren:
                    DestroyAllComponentsOfType(selectedAvatar, typeof(Animator), true);
                    break;
                case ToolMenuActions.RemoveAudioSources:
                    DestroyAllComponentsOfType(selectedAvatar, typeof(AudioSource), false);
                    DestroyAllComponentsOfType(selectedAvatar, typeof(ONSPAudioSource), false);
                    break;
                case ToolMenuActions.RemoveJoints:
                    DestroyAllComponentsOfType(selectedAvatar, typeof(Joint), false);
                    break;
                default:
                    break;
            }

            avatarInfo = AvatarInfo.GetInfo(selectedAvatar, out _avatarInfoString);

            EditorUtility.SetDirty(selectedAvatar);
            EditorSceneManager.MarkSceneDirty(selectedAvatar.scene);
        }


        /// <summary>
        /// Begin Editing Viewposition
        /// </summary>        
        private void BeginEditViewpoint(GameObject avatar)
        {
            _viewPos_descriptor = avatar.GetComponent<VRC_AvatarDescriptor>();
            if(_viewPos_descriptor == null)
            {
                _viewPos_descriptor = avatar.AddComponent<VRC_AvatarDescriptor>();
            }

            Vector3 defaultView = new Vector3(0, 1.6f, 0.2f);
            _viewPosOld = _viewPos_descriptor.ViewPosition;

            if(_viewPos_descriptor.ViewPosition == defaultView)
            {
                var anim = selectedAvatar.GetComponent<Animator>();

                if(anim != null && anim.isHuman)
                {
                    _viewPosTemp = anim.GetBoneTransform(HumanBodyBones.Head).position;
                    float eyeHeight = anim.GetBoneTransform(HumanBodyBones.LeftEye).position.y - 0.05f;
                    _viewPosTemp.y = eyeHeight;
                    _viewPosTemp.z = defaultView.z;
                }
            }
            else
            {
                _viewPosTemp = _viewPos_descriptor.ViewPosition + avatar.transform.root.position;
            }
            _editingView = true;
            //_viewPosTemp += _viewPos_descriptor.gameObject.transform.position;

            Tools.current = Tool.None;
            Selection.activeGameObject = selectedAvatar.transform.root.gameObject;
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
                if(_viewPos_descriptor == null)
                {
                    Log(Strings.Log.DescriptorIsNull, LogType.Error);
                    return;
                }

                _editingView = false;
                Tools.current = _viewPosToolOld;
                if(!cancelled)
                {
                    _viewPos_descriptor.ViewPosition = RoundVectorValues(_viewPosTemp - _viewPos_descriptor.gameObject.transform.position, 3);
                    Log(Strings.Log.ViewpointApplied, LogType.Log, _viewPos_descriptor.ViewPosition.ToString());
                    _viewPos_descriptor = null;
                }
                else
                {
                    _viewPos_descriptor.ViewPosition = _viewPosOld;
                    Log(Strings.Log.ViewpointCancelled, LogType.Log);
                    _viewPos_descriptor = null;
                }
            }
            this.Repaint();
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
                if(bCopier_dynamicBones_copy)
                {
                    if(bCopier_dynamicBones_removeOldColliders)
                        DestroyAllComponentsOfType(selectedAvatar, typeof(DynamicBoneCollider));                        
                    if(bCopier_dynamicBones_removeOldBones)                        
                        DestroyAllComponentsOfType(selectedAvatar, typeof(DynamicBone));
                }
                if(bCopier_colliders_copy)
                {
                    if(bCopier_colliders_removeOld)
                        DestroyAllColliders(selectedAvatar);
                }
                if(bCopier_descriptor_copy)
                {
                    CopyAvatarDescriptor(objFrom, objTo);
                }
                if(bCopier_transforms_copyAvatarScale)
                {
                    objTo.transform.localScale = objFrom.transform.localScale;
                }
              
                //New way of doing it with all child components being copied within the functions so we only run once.
                //Will eventually replace running CopyComponents() on each child with this.
                if(bCopier_particleSystems_copy)
                {
                    CopyAllParticleSystems(objFrom, objTo, bCopier_particleSystems_createObjects);
                }
                if(bCopier_colliders_copy)
                {
                    CopyAllColliders(objFrom, objTo, bCopier_colliders_createObjects);
                }
                if(bCopier_rigidBodies_copy)
                {
                    CopyAllRigidBodies(objFrom, objTo, bCopier_rigidBodies_createObjects);
                }
                if(bCopier_trailRenderers_copy)
                {
                    CopyAllTrailRenderers(objFrom, objTo, bCopier_trailRenderers_createObjects);
                }
                if(bCopier_meshRenderers_copy)
                {
                    CopyAllMeshRenderers(objFrom, objTo, bCopier_meshRenderers_createObjects);
                }
                if(bCopier_lights_copy)
                {
                    CopyAllLights(objFrom, objTo, bCopier_lights_copy);
                }
                if(bCopier_skinMeshRender_copy)
                {
                    CopyAllSkinnedMeshRenderersSettings(objFrom, objTo);
                }
                if(bCopier_animators_copy)
                {
                    CopyAllAnimators(objFrom, objTo, bCopier_animators_createObjects, bCopier_animators_copyMainAnimator);
                }
                //if(bCopier_joints_copy) //Coming soon
                //{
                //    CopyAllJoints(objFrom, objTo, bCopier_joints_createObjects);
                //}
                if(bCopier_audioSources_copy)
                {
                    CopyAllAudioSources(objFrom, objTo, bCopier_audioSources_createObjects);
                }
            }
            //End run once
            
            if(bCopier_transforms_copy && (bCopier_transforms_copyPosition || bCopier_transforms_copyRotation || bCopier_transforms_copyScale))
            {
                CopyTransforms(objFrom, objTo);
            }
            if(bCopier_dynamicBones_copy)
            {
                if(bCopier_dynamicBones_copySettings || bCopier_dynamicBones_copyColliders)
                {
                    CopyDynamicBones(objFrom, objTo, bCopier_dynamicBones_createMissingBones);
                }
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

        //private void CopyAllJoints(GameObject from, GameObject to, bool createGameObjects)
        //{
        //    if((from == null || to == null) ||
        //    (!(bCopier_joints_copySettings || bCopier_joints_fixed || bCopier_joints_character || bCopier_joints_createObjects ||
        //    bCopier_joints_configurable || bCopier_joints_sprint || bCopier_joints_hinge || bCopier_joints_createMissing)))
        //        return;

        //    var jFromArr = from.GetComponentsInChildren<Joint>();

        //    for(int i = 0; i < jFromArr.Length; i++)
        //    {
        //        string log = Strings.Log.CopyAttempt;
        //        var t = jFromArr[i].GetType();

        //        if(supportedComponents.Contains(t))
        //        {
        //            var jj = jFromArr[i];
        //            var jFromPath = GetGameObjectPath(jj.gameObject);

        //            if(jFromPath != null)
        //            {
        //                GameObject jToObj = to.transform.root.Find(jFromPath, createGameObjects, jj.transform).gameObject;
        //                var jToArr = jToObj.GetComponents<Joint>();
        //                bool found = false;

        //                for(int z = 0; z < jToArr.Length; z++)
        //                {
        //                    if(JointsAreIdentical(jToArr[z], jFromArr[i]))
        //                    {
        //                        found = true;
        //                        Log(log + Strings.Log.FailedAlreadyHas, LogType.Warning, jToObj.name, t.ToString());
        //                        break;
        //                    }
        //                }
        //                if(!found)
        //                {
        //                    ComponentUtility.CopyComponent(jFromArr[i]);
        //                    ComponentUtility.PasteComponentAsNew(jToObj);

        //                    Log(log + " - " + Strings.Log.Success, LogType.Log, t.ToString(), jToObj.name);
        //                }
        //            }
        //        }
        //        else
        //        {
        //            Log(log + Strings.Log.FailedNotSupported, LogType.Warning, t.ToString());
        //        }
        //    }
        //}

        /// <summary>
        /// Copies all audio sources on object and it's children.
        /// </summary>            
        /// <param name="createGameObjects">Whether to create missing objects</param>            
        void CopyAllAudioSources(GameObject from, GameObject to, bool createGameObjects)
        {
            if(from == null || to == null)
                return;

            var aFromArr = from.GetComponentsInChildren<AudioSource>();

            for(int i = 0; i < aFromArr.Length; i++)
            {
                var aFrom = aFromArr[i];
                var oFrom = aFromArr[i].GetComponent<ONSPAudioSource>();

                var tTo = FindChildInAnotherHierarchy(aFrom.gameObject, to, createGameObjects);
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
        void CopyAllAnimators(GameObject from, GameObject to, bool createGameObjects, bool copyRootAnimator)
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
                var tTo = FindChildInAnotherHierarchy(aFrom.gameObject, to, createGameObjects);
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
        /// Copies all lights in object and it's children to another object.
        /// </summary>        
        /// <param name="createGameObjects">Whether to create missing game objects</param>
        void CopyAllLights(GameObject from, GameObject to, bool createGameObjects)
        {
            if(from == null || to == null)
                return;

            var lFromArr = from.GetComponentsInChildren<Light>();
                        
            for(int i = 0; i < lFromArr.Length; i++)
            {
                string log = Strings.Log.CopyAttempt;

                var lFrom = lFromArr[i];
                var tTo = FindChildInAnotherHierarchy(lFrom.gameObject, to, createGameObjects);
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
        void CopyAllMeshRenderers(GameObject from, GameObject to, bool createGameObjects)
        {
            if(from == null || to == null)
                return;

            var mFromArr = from.GetComponentsInChildren<MeshRenderer>();

            for(int i = 0; i < mFromArr.Length; i++)
            {
                string log = Strings.Log.CopyAttempt;

                var rFrom = mFromArr[i];
                var tTo = FindChildInAnotherHierarchy(rFrom.gameObject, to, createGameObjects);
                var rToObj = tTo.gameObject;

                var fFrom = rFrom.GetComponent<MeshFilter>();

                if(fFrom != null)
                {
                    var rTo = rToObj.GetComponent<MeshRenderer>();
                    var fTo = rToObj.GetComponent<MeshFilter>();

                    if(rTo == null && bCopier_meshRenderers_createMissing)
                    {
                        rTo = rToObj.AddComponent<MeshRenderer>();
                        if(fTo == null)
                            fTo = rToObj.AddComponent<MeshFilter>();
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
        void CopyAvatarDescriptor(GameObject from, GameObject to)
        {
            if(to == null)
                return;


            var dFrom = from.GetComponent<VRC_AvatarDescriptor>();
            var pFrom = from.GetComponent<PipelineManager>();
            var dTo = to.GetComponent<VRC_AvatarDescriptor>();

            if(dFrom == null)
                return;
            if(dTo == null)
                dTo = to.AddComponent<VRC_AvatarDescriptor>();

            var pTo = to.GetComponent<PipelineManager>();

            if(pTo == null) //but it shouldn't be
                pTo = to.AddComponent<PipelineManager>();

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
        /// Copy over DynamicBoneCollider components.
        /// </summary>
        void CopyDynamicBoneColliders(GameObject from, GameObject to, bool removeOld = false)
        {
#if !NO_BONES

            string[] logFormat = { "DynamicBoneCollider", from.name, to.name };
            string log = Strings.Log.CopyAttempt;

            List<DynamicBoneCollider> dFromList = new List<DynamicBoneCollider>();
            dFromList.AddRange(from.GetComponents<DynamicBoneCollider>());

            List<DynamicBoneCollider> dToList = new List<DynamicBoneCollider>();
            dToList.AddRange(to.GetComponents<DynamicBoneCollider>());

#if !OLD_BONES
            
            if(dFromList.Count == 0)
            {
                var ar = from.GetComponents<DynamicBoneColliderBase>();
                foreach(var obj in ar)
                {                    
                    dFromList.Add((DynamicBoneCollider)obj);
                }
            }

            if(dToList.Count == 0)
            {
                var ar = to.GetComponents<DynamicBoneColliderBase>();
                foreach(var obj in ar)
                {
                    dToList.Add((DynamicBoneCollider)obj);
                }
            }

#endif
            if(removeOld)
            {
                foreach(var c in dToList)
                {
                    DestroyImmediate(c);
                }
            }

            for(int i = 0; i < dFromList.Count; i++)
            {
                var dFrom = dFromList[i];
                DynamicBoneCollider dTo = null;

                if(dFrom == null)
                {
                    log += "Failed: {2} has no {1}";
                    Log(log, LogType.Warning, logFormat);                    
                    return;
                }

                log += "{2} has no {1}. Creating - ";                                
                dTo = to.AddComponent<DynamicBoneCollider>();

                dTo.m_Bound = dFrom.m_Bound;
                dTo.m_Center = dFrom.m_Center;
                dTo.m_Direction = dFrom.m_Direction;
                dTo.m_Height = dFrom.m_Height;
                dTo.m_Radius = dFrom.m_Radius;

                dTo.enabled = dFrom.enabled;

                if(!removeOld)
                {
                    foreach(var c in dToList)
                    {
                        if(c.m_Bound == dTo.m_Bound && c.m_Center == dTo.m_Center && c.m_Direction == dTo.m_Direction && c.m_Height == dTo.m_Height && c.m_Radius == dTo.m_Radius)
                        {
                            DestroyImmediate(dTo);
                            break;
                        }
                    }
                    log += "_Duplicate {1} with the same settings already exists. Removing duplicate.";
                    Log(log, LogType.Warning, logFormat);                    
                }
                else
                {
                    log += "_Success: Added {0} to {2}.";
                    Log(log, LogType.Log, logFormat);                    
                }
            }
#endif
        }

        /// <summary>
        /// Copies DynamicBone components. 
        /// </summary>
        void CopyDynamicBones(GameObject from, GameObject to, bool createMissing = true)
        {
#if !NO_BONES
            string log = Strings.Log.CopyAttempt;
            string[] logFormat = {  "DynamicBoneCollider", from.name, to.name };

            var dFromList = new List<DynamicBone>();
            var dToList = new List<DynamicBone>();

            dFromList.AddRange(from.GetComponents<DynamicBone>());
            dToList.AddRange(to.GetComponents<DynamicBone>());

            for(int i = 0; i < dFromList.Count; i++)
            {
                var dFrom = dFromList[i];
                var garbageBones = new List<DynamicBone>();
#if !OLD_BONES
                var newCollList = new List<DynamicBoneColliderBase>();
#else
                var newCollList = new List<DynamicBoneCollider>();
#endif
            

                DynamicBone dTo = null;

                foreach(var d in dToList)
                {
                    if(d.m_Root == null || (d.m_Root.name == dFrom.m_Root.name))
                    {
                        garbageBones.Add(d);
                        dTo = d;
                        break;
                    }
                }

                if(dTo == null)
                    if(createMissing)
                    {
                        dTo = to.AddComponent<DynamicBone>();

#if !OLD_BONES
                        dTo.m_Colliders = new List<DynamicBoneColliderBase>();
#else
                        dTo.m_Colliders = new List<DynamicBoneCollider>();
#endif
                    }
                    else
                        return;

                if(garbageBones != null)
                {
                    foreach(var d in garbageBones)
                    {
                        dToList.Remove(d);
                    }
                }

                if(bCopier_dynamicBones_copyColliders)
                {
                    var colls = dFrom.m_Colliders;
                    for(int z = 0; z < colls.Count; z++)
                    {
						if(colls[z] == null)
							continue;
						
                        string tFromPath = GetGameObjectPath(colls[z].gameObject);
                        var tTo = to.transform.root.Find(tFromPath);

                        DynamicBoneCollider fc = null;
                        if(fc == null)
                        {
                            var tempCol = colls[z].GetComponent<DynamicBoneCollider>();
                            if(tempCol != null)
                                fc = tempCol;
                            else
                                fc = (DynamicBoneCollider)colls[z];
                        }

                        if(tTo != null)
                        {
                            var oldColls = tTo.GetComponents<DynamicBoneCollider>();

                            bool isSame = false;
                            foreach(var c in oldColls)
                            {
                                if(c.m_Bound == fc.m_Bound && c.m_Center == fc.m_Center && c.m_Direction == fc.m_Direction && c.m_Height == fc.m_Height && c.m_Radius == fc.m_Radius)
                                {
                                    isSame = true;
#if OLD_BONES
                                    DynamicBoneCollider tempC = c;
#else
                                    DynamicBoneColliderBase tempC = c;
#endif
                                    foreach(var cc in dTo.m_Colliders)
                                    {
                                        if(c == cc)
                                        {
                                            tempC = null;
                                            break;
                                        }
                                    }
                                    if(tempC != null)
                                    {
                                        newCollList.Add(tempC);
                                    }
                                    break;
                                }
                            }
                            if(!isSame)
                            {
                                var cTo = tTo.gameObject.AddComponent<DynamicBoneCollider>();
                                cTo.m_Bound = fc.m_Bound;
                                cTo.m_Center = fc.m_Center;
                                cTo.m_Direction = fc.m_Direction;
                                cTo.m_Height = fc.m_Height;
                                cTo.m_Radius = fc.m_Radius;

                                cTo.enabled = fc.enabled;

                                newCollList.Add(cTo);
                                log += "Success: Added {0} to {2}";
                                Log(log, LogType.Log, logFormat);
                            }
                        }
                    }
                }

                logFormat = new string[] { "_DynamicBone", from.name, to.name };
                log = Strings.Log.CopyAttempt;

                if(dFrom == null)
                {
                    log += "_Failed: {1} has no {0}. Ignoring";
                    Log(log, LogType.Warning, logFormat);
                    return;
                }
                else if(!bCopier_dynamicBones_copySettings)
                {
                    log += "_Failed: Not allowed to - Copy settings is unchecked";
                    Log(log, LogType.Warning, logFormat);
                    return;
                }
                else if(dFrom.m_Root == null)
                {
                    Log(log, LogType.Warning, logFormat);
                    log += "_Failed: {2}'s {0} has no Root set. Ignoring";
                    return;
                }

                dTo.enabled = dFrom.enabled;

                dTo.m_Damping = dFrom.m_Damping;
                dTo.m_DampingDistrib = dFrom.m_DampingDistrib;
                dTo.m_DistanceToObject = dFrom.m_DistanceToObject;
                dTo.m_DistantDisable = dFrom.m_DistantDisable;
                dTo.m_Elasticity = dFrom.m_Elasticity;
                dTo.m_ElasticityDistrib = dFrom.m_ElasticityDistrib;
                dTo.m_EndLength = dFrom.m_EndLength;
                dTo.m_EndOffset = dFrom.m_EndOffset;
                dTo.m_Force = dFrom.m_Force;
                dTo.m_FreezeAxis = dFrom.m_FreezeAxis;
                dTo.m_Gravity = dFrom.m_Gravity;
                dTo.m_Inert = dFrom.m_Inert;
                dTo.m_InertDistrib = dFrom.m_InertDistrib;
                dTo.m_Radius = dFrom.m_Radius;
                dTo.m_RadiusDistrib = dFrom.m_RadiusDistrib;
                dTo.m_ReferenceObject = dFrom.m_ReferenceObject;
                dTo.m_Stiffness = dFrom.m_Stiffness;
                dTo.m_StiffnessDistrib = dFrom.m_StiffnessDistrib;                
                dTo.m_UpdateRate = dFrom.m_UpdateRate;

                if(dTo.m_Colliders != null)
                    dTo.m_Colliders.TrimExcess();

                dTo.m_Colliders.AddRange(newCollList);

                List<Transform> el = new List<Transform>();
                for(int z = 0; z < dFrom.m_Exclusions.Count; z++)
                {
                    if(dFrom.m_Exclusions[z] != null)
                    {
                        string p = GetGameObjectPath(dFrom.m_Exclusions[z].gameObject, true);
                        var t = to.transform.root.Find(p);

                        if(t != null && dFrom.m_Exclusions[z].name == t.name)
                            el.Add(t);
                    }
                }
                dTo.m_Exclusions = el;

                if(dFrom.m_Root != null)
                {
                    string rootPath = GetGameObjectPath(dFrom.m_Root.gameObject, true);
                    if(!string.IsNullOrEmpty(rootPath))
                    {
                        var toRoot = dTo.transform.root.Find(rootPath);
                        if(!string.IsNullOrEmpty(rootPath))
                            dTo.m_Root = toRoot;
                    }
                }

                if(dFrom.m_ReferenceObject != null)
                {
                    string refPath = GetGameObjectPath(dFrom.m_ReferenceObject.gameObject, true);
                    if(!string.IsNullOrEmpty(refPath))
                    {
                        var toRef = dTo.transform.root.Find(refPath);
                        if(!string.IsNullOrEmpty(refPath))
                            dTo.m_ReferenceObject = toRef;
                    }
                }

                log += "Success: Copied {0} from {1} to {2}";
                Log(log, LogType.Log, logFormat);                
            }
#endif
        }

        /// <summary>
        /// Copies Box, Capsule, Sphere and Mesh colliders from one object to another AND ALL OF IT'S CHILDREN AT ONCE.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        void CopyAllColliders(GameObject from, GameObject to, bool createGameObjects)
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

                    if(cFromPath != null)
                    {
                        GameObject cToObj = to.transform.root.Find(cFromPath, createGameObjects, cc.transform).gameObject;
                        var cToArr = cToObj.GetComponents<Collider>();
                        bool found = false;

                        for(int z = 0; z < cToArr.Length; z++)
                        {
                            if(CollidersAreIdentical(cToArr[z], cFromArr[i]))
                            {
                                found = true;
                                Log(log + Strings.Log.FailedAlreadyHas, LogType.Warning, cToObj.name, t.ToString());
                                break;
                            }
                        }
                        if(!found)
                        {
                            ComponentUtility.CopyComponent(cFromArr[i]);
                            ComponentUtility.PasteComponentAsNew(cToObj);
                            
                            Log(log + " - " + Strings.Log.Success, LogType.Log, t.ToString(), cToObj.name);
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
        void CopyTransforms(GameObject from, GameObject to)
        {            
            var tFrom = from.transform;
            var tTo = to.transform;

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
                Log(log, LogType.Warning, logFormat);
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
            Log(log, LogType.Log ,logFormat);
        }

        /// <summary>
        /// Copies settings of all SkinnedMeshRenderers in object and children.
        /// Does NOT copy mesh, bounds and root bone settings.
        /// </summary>        
        void CopyAllSkinnedMeshRenderersSettings(GameObject from, GameObject to)
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
                    GameObject rToObj = to.transform.root.Find(rFromPath).gameObject;
                    var rTo = rToObj.GetComponent<SkinnedMeshRenderer>();

                    if(rTo != null)
                    {
                        var bounds = rTo.localBounds;
                        var mesh = rTo.sharedMesh;
                        var root = rTo.rootBone;
                        var mats = rTo.sharedMaterials;

                        if(bCopier_skinMeshRender_copySettings)
                        {                            
                            ComponentUtility.CopyComponent(rFromArr[i]);
                            ComponentUtility.PasteComponentValues(rTo);

                            rTo.localBounds = bounds;
                            rTo.sharedMesh = mesh;
                            rTo.rootBone = root;
                            rTo.sharedMaterials = mats;                                
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
        /// Copies SkinnedMeshRenderer settings. Note that only one can exist on an object.
        /// </summary>                
        void CopySkinMeshRenderer(GameObject from, GameObject to)
        {
            
            if((from == null || to == null) ||
                (!(bCopier_skinMeshRender_copyBlendShapeValues || bCopier_skinMeshRender_copyMaterials || bCopier_skinMeshRender_copySettings)))
                return;

            string log = Strings.Log.CopyAttempt + " - ";
            string[] logFormat = { "SkinnedMeshRenderer", from.name, to.name };

            SkinnedMeshRenderer rFrom = from.GetComponent<SkinnedMeshRenderer>();
            SkinnedMeshRenderer rTo = to.GetComponent<SkinnedMeshRenderer>();

            if(rFrom == null)
            {                
                log += Strings.Log.FailedIsNull;
                Log(log, LogType.Warning, logFormat);
                return;
            }

            if(bCopier_skinMeshRender_copySettings)
            {
                rTo.enabled = rFrom.enabled;
                rTo.quality = rFrom.quality;
                rTo.updateWhenOffscreen = rFrom.updateWhenOffscreen;
                rTo.skinnedMotionVectors = rFrom.skinnedMotionVectors;
                rTo.lightProbeUsage = rFrom.lightProbeUsage;
                rTo.reflectionProbeUsage = rFrom.reflectionProbeUsage;
                rTo.shadowCastingMode = rFrom.shadowCastingMode;
                rTo.receiveShadows = rFrom.receiveShadows;
                rTo.motionVectorGenerationMode = rFrom.motionVectorGenerationMode;

                string path = null;
                if(rFrom.probeAnchor != null)
                    path = GetGameObjectPath(rFrom.probeAnchor.gameObject);

                if(!string.IsNullOrEmpty(path))
                    rTo.probeAnchor = rTo.transform.root.Find(path);

                path = null;
                if(rFrom.rootBone != null)
                    path = GetGameObjectPath(rFrom.rootBone.gameObject);

                if(!string.IsNullOrEmpty(path))
                    rTo.rootBone = rTo.transform.root.Find(path);
            }

            if(bCopier_skinMeshRender_copyBlendShapeValues)
            {
                for(int i = 0; i < rFrom.sharedMesh.blendShapeCount; i++)
                {
                    int index = rFrom.sharedMesh.GetBlendShapeIndex(rFrom.sharedMesh.GetBlendShapeName(i));
                    if(index != -1)
                    {
                        rTo.SetBlendShapeWeight(index, rFrom.GetBlendShapeWeight(index));
                    }
                }
            }

            if(bCopier_skinMeshRender_copyMaterials)
            {
                rTo.sharedMaterials = rFrom.sharedMaterials;
            }

            rTo.sharedMesh.RecalculateBounds();

            log += "Success: Copied {0} from {1} to {2}";
            Log(log, LogType.Log, logFormat);
        }

        /// <summary>
        /// Copies all TrailRenderers in object and it's children.
        /// </summary>        
        /// <param name="createGameObjects">Whether to create missing GameObjects</param>
        void CopyAllTrailRenderers(GameObject from, GameObject to, bool createGameObjects)
        {
            if(from == null || to == null)
                return;

            var rFromArr = from.GetComponentsInChildren<TrailRenderer>();

            for(int i = 0; i < rFromArr.Length; i++)
            {
                var rFrom = rFromArr[i];
                var tTo = FindChildInAnotherHierarchy(rFrom.gameObject, to, createGameObjects);
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
        void CopyAllRigidBodies(GameObject from, GameObject to, bool createGameObjects)
        {
            if(from == null || to == null)
                return;

            var rFromArr = from.GetComponentsInChildren<Rigidbody>();

            for(int i = 0; i < rFromArr.Length; i++)
            {
                var rFrom = rFromArr[i];
                var tTo = FindChildInAnotherHierarchy(rFrom.gameObject, to, createGameObjects);
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
        void CopyAllParticleSystems(GameObject from, GameObject to, bool createGameObjects)
        {
            var pFromArr = from.GetComponentsInChildren<ParticleSystem>(true);

            for(int i = 0; i < pFromArr.Length; i++)
            {
                var pp = pFromArr[i];
                var tTo = FindChildInAnotherHierarchy(pp.gameObject, to, createGameObjects);                

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

        /// <summary>
        /// Remove BlendShapes by name.
        /// Copies over all blendshapes to a clone of the mesh with shapeNames excluded. Doesn't work yet
        /// </summary>                
        void RemoveBlendShapes(SkinnedMeshRenderer renderer, params string[] shapeNames)
        {

            Mesh myMesh = renderer.sharedMesh;
            Mesh tmpMesh = Instantiate(myMesh);

            myMesh = null;
            tmpMesh.name += " (No Blink)";
            tmpMesh.RecalculateBounds();

            GameObject prefabParent = (GameObject)PrefabUtility.GetPrefabParent(renderer.gameObject);

            string path = AssetDatabase.GetAssetPath(prefabParent.transform.root.gameObject);
            string name = Path.GetFileName(path);

            path = path.Substring(0, path.Length - name.Length);

            AssetDatabase.AddObjectToAsset(tmpMesh, path + name);
        }

        /// <summary>
        /// Disables blinking by removing blinking Visemes. Not quite working because "empty" visemes are needed instead
        /// </summary>        
        void DisableBlinking(GameObject avatar)
        {
            var renders = avatar.GetComponentsInChildren<SkinnedMeshRenderer>(true);

            foreach(var r in renders)
            {
                RemoveBlendShapes(r, "vrc.blink_left", "vrc.blink_right", "vrc.lowerlid_left", "vrc.lowerlid_right");                
            }


            /*var renders = avatar.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            foreach(var r in renders)
            {
                int blinkLeft = r.sharedMesh.GetBlendShapeIndex("vrc.blink_left");
                int blinkRight = r.sharedMesh.GetBlendShapeIndex("vrc.blink_right");

                int lowerLeft = r.sharedMesh.GetBlendShapeIndex("vrc.lowerlid_left");
                int lowerRight = r.sharedMesh.GetBlendShapeIndex("vrc.lowerlid_right");

                List<BlendShape> blends = GetBlendShapes(r);

                if(blinkLeft == -1 && blinkRight != -1)
                {
                    Debug.Log("_Blinking visemes already missing. Ignoring");
                }
                else
                {
                    r.sharedMesh.ClearBlendShapes();
                    if(blinkLeft != -1)
                    {
                        //blends[blinkLeft] = null;
                    }
                    if(blinkRight != -1)
                    {
                        //blends[blinkRight] = null;
                    }
                    if(lowerLeft != -1)
                    {
                        //blends[lowerLeft] = null;
                    }
                    if(lowerRight != -1)
                    {
                        //blends[lowerRight] = null;
                    }
                    foreach(var b in blends)
                    {
                        //if(b != null)                            
                            r.sharedMesh.AddBlendShapeFrame(b.name, b.weight, b.deltaVertices, b.deltaNormals, b.deltaTangents);
                    }
                }

            }*/
        }

        /// <summary>
        /// Get list of BlendShapes from mesh. Applying blendshapes from this list doesn't seem to work.
        /// </summary>        
        List<BlendShapeFrame> GetBlendShapeFrames(SkinnedMeshRenderer renderer)
        {
            var shapes = new List<BlendShapeFrame>();

            if(renderer != null)
            {
                Mesh myMesh = renderer.sharedMesh;

                Vector3[] dVertices = new Vector3[myMesh.vertexCount];
                Vector3[] dNormals = new Vector3[myMesh.vertexCount];
                Vector3[] dTangents = new Vector3[myMesh.vertexCount];
                for(int shape = 0; shape < myMesh.blendShapeCount; shape++)
                {
                    string shapeName = myMesh.GetBlendShapeName(shape);

                    for(int frame = 0; frame < myMesh.GetBlendShapeFrameCount(shape); frame++)
                    {
                        float frameWeight = myMesh.GetBlendShapeFrameWeight(shape, frame);

                        myMesh.GetBlendShapeFrameVertices(shape, frame, dVertices, dNormals, dTangents);
                        shapes.Add(new BlendShapeFrame(shapeName, frameWeight, dVertices, dNormals, dTangents));
                    }
                }
            }
            return shapes;
        }

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
        /// Meant to apply a humanoid TPose, not done.
        /// </summary>
        /// <param name="objTo"></param>
        public static void ApplyTPose(GameObject objTo)
        {
            throw new NotImplementedException();

            //if(objTo == null)
            //    return;

            //Animator oldAnim = objTo.GetComponent<Animator>();
            //Avatar av = objTo.GetComponent<Avatar>();
            //if(av != null && av.isHuman)
            //{
            //    if(_tposeAvatar == null)
            //    {

            //    }
            //}
            //else
            //{
            //    Log("_Rig avatar missing or not humanoid", LogType.Error);
            //    return;
            //}
        }

        /// <summary>
        /// This will move the VRCCam thumbnail camera on the avatar's face, with an offset
        /// </summary>
        /// <param name="avatarOverride">If this is null, get the avatar we're currently uploading</param>
        void CenterCameraOnFace(GameObject avatarOverride = null)
        {
            if(!FindVRCCam())
                return;

            _vrcCam.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

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
                Debug.Log("_Failed to center camera on face. Avatar descriptor not found");
                return;
            }

            GameObject focusObj = new GameObject("FocusDummy");
            focusObj.transform.position = desc.transform.position + desc.ViewPosition;
            focusObj.transform.rotation = desc.transform.rotation;            

            Transform oldParent = _vrcCam.transform.parent;
            _vrcCam.transform.parent = focusObj.transform;

            focusObj.transform.localEulerAngles = focusObj.transform.localEulerAngles + new Vector3(5, -14, 0);
            
            _vrcCam.transform.localPosition = Vector3.zero + new Vector3(0, desc.ViewPosition.z * 0.05f, desc.ViewPosition.y * 0.21f);

            _vrcCam.transform.parent = oldParent;

            Destroy(focusObj);
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

            var bones = new List<Transform>();

            foreach(var r in renders)
            {                
                foreach(var b in r.bones)
                {
                    if(!bones.Contains(b))
                        bones.Add(b);
                }
            }
                        
            foreach(var t in obj.OrderBy(o => o.childCount))
            {                
                if(t!= null && t != t.root && t.GetComponents<Component>().Length == 1 && !bones.Contains(t))
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

        void DestroyEmptyBonesNew(GameObject from)
        {
            var renders = from.GetComponentsInChildren<SkinnedMeshRenderer>();
            var weights = new Dictionary<Transform, bool>();

            foreach(var r in renders)
            {
                foreach(var b in r.bones)
                {
                    if(b != null && !weights.ContainsKey(b))
                        weights.Add(b, false);
                }

                foreach(var w in r.sharedMesh.boneWeights)
                {
                    float vertWeight = 0;

                    if(vertWeight < 1 && w.weight0 != 0 && r.bones[w.boneIndex0] != null)
                    {
                        vertWeight += w.weight0;

                        Transform t = r.bones[w.boneIndex0];
                        if(weights.ContainsKey(t))
                            weights[t] = true;
                    }
                    if(vertWeight < 1 && w.weight1 != 0 && r.bones[w.boneIndex1] != null)
                    {
                        vertWeight += w.weight1;

                        Transform t = r.bones[w.boneIndex1];
                        if(weights.ContainsKey(t))
                            weights[t] = true;
                    }
                    if(vertWeight < 1 && w.weight2 != 0 && r.bones[w.boneIndex2] != null)
                    {
                        vertWeight += w.weight2;

                        Transform t = r.bones[w.boneIndex2];
                        if(weights.ContainsKey(t))
                            weights[t] = true;
                    }
                    if(vertWeight < 1 && w.weight3 != 0 && r.bones[w.boneIndex3] != null)
                    {
                        vertWeight += w.weight3;

                        Transform t = r.bones[w.boneIndex3];
                        if(weights.ContainsKey(t))
                            weights[t] = true;
                    }
                }
            }            

            var sortedBones = from pair in weights
                        orderby pair.Key.childCount ascending
                        select pair;
            
            foreach(var kv in sortedBones)
            {
                if(kv.Key != null && !kv.Value && kv.Key.childCount == 0 && kv.Key.GetComponents<Component>().Length == 1)
                {
                    Log("_{0} has no bone weights, components or children. Destroying.", LogType.Log, kv.Key.name);
                    DestroyImmediate(kv.Key.gameObject);
                }
            }
        }

        void DestroyEmptyBones(GameObject from)
        {
            var renders = from.GetComponentsInChildren<SkinnedMeshRenderer>();          

            foreach(var r in renders)
            {
                foreach(var b in r.bones)
                {
                    if(b.childCount > 0)
                        continue; 

                    bool empty = true;
                    foreach(var w in r.sharedMesh.boneWeights)
                    {
                        if((b == r.bones[w.boneIndex0] && w.weight0 > 0) || (b == r.bones[w.boneIndex1] && w.weight1 > 0) ||
                                (b == r.bones[w.boneIndex2] && w.weight2 > 0) || (b == r.bones[w.boneIndex3] && w.weight3 > 0))
                        {
                            empty = false;
                            break;
                        }                        
                    }

                    if(empty)
                    {
                        Log("_Bone {0} in {1} is empty. Removing", LogType.Log, b.name, r.gameObject.name);
                        DestroyImmediate(b.gameObject);
                    }
                }

            }            
        }

        /// <summary>
        /// Destroys all Collider components from object and all of it's children.
        /// </summary>    
        void DestroyAllColliders(GameObject from)
        {            
            var col = from.GetComponentsInChildren<Collider>(true);
            foreach(var c in col)
            {
                Log(Strings.Log.RemoveAttempt, LogType.Log, c.ToString(), from.name);                
                DestroyImmediate(c);
            }
        }

        /// <summary>
        /// Destroys all DynamicBone components from object and all of it's children.
        /// </summary>    
        void DestroyAllDynamicBones(GameObject from)
        {
#if !NO_BONES
            var bones = from.GetComponentsInChildren<DynamicBone>(true);
            foreach(var b in bones)
            {
                DestroyImmediate(b);
            }
#endif
        }

        /// <summary>
        /// Destroys all DynamicBoneCollider components from object and it's children 
        /// and clears all DynamicBone collider lists.
        /// </summary>    
        void DestroyAllDynamicBoneColliders(GameObject from)
        {

#if !NO_BONES
            List<DynamicBoneColliderBase> cl = new List<DynamicBoneColliderBase>();
            cl.AddRange(from.GetComponentsInChildren<DynamicBoneColliderBase>(true));

            foreach(var c in cl)
            {
                DestroyImmediate(c);
            }

            List<DynamicBone> dl = new List<DynamicBone>();
            dl.AddRange(from.GetComponentsInChildren<DynamicBone>(true));

            foreach(var d in dl)
            {
                if(d.m_Colliders != null)
                    d.m_Colliders.Clear();
            }
#endif
        }

        /// <summary>
        /// Destroy all components of type, if the type is in supportedComponents
        /// Not sure if it's even necessary to check but we'll keep it for now
        /// </summary>        
        void DestroyAllComponentsOfType(GameObject obj, Type type, bool ignoreRoot = true)
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
                    if(ignoreRoot && comps[i].transform.parent == null)
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
                        log += Strings.Log.Failed + ": "+ e.Message;
                        Log(log, LogType.Exception, type.ToString(), name);
                    }                
                }
            }
        }

                            #endregion

                            #region Helper Functions

        public static string GetGameObjectPath(GameObject obj, bool skipRoot = true)
        {
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
                if(path[i] == '\\' || path[i] == '/')
                    return path.Substring(0, i);

            return path;
        }

        public static string[] GetPathAsArray(string path)
        {
            if(string.IsNullOrEmpty(path))
                return null;

            return path.Split('\\', '/');
        }

        public Transform FindChildInAnotherHierarchy(GameObject child, GameObject parent, bool createIfMissing)
        {
            var childPath = GetGameObjectPath(child);
            var childTrans = parent.transform.root.Find(childPath, createIfMissing, child.transform);

            return childTrans;
        }

        public static bool IsBoneHasWeights(Transform t, SkinnedMeshRenderer r)
        {
            if(t == r.rootBone)
            {
                return true;
            }

            bool isBone = false;
            foreach(var b in r.bones)
            {
                if(t == b)
                {
                    isBone = true;
                    break;
                }
            }

            if(isBone)
            {                
                foreach(var b in r.sharedMesh.boneWeights)
                {
                    if((t == r.bones[b.boneIndex0] && b.weight0 > 0) || (t == r.bones[b.boneIndex1] && b.weight1 > 0)
                            || (t == r.bones[b.boneIndex2] && b.weight2 > 0) || (t == r.bones[b.boneIndex3] && b.weight3 > 0))
                    {
                        return true;
                    }
                }
                return false;
            }
            else
            {
                return false;
            }            
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

            if(j1 == null && j2 == null)
                return true;
            else if(j1.GetType() != j2.GetType())
                return false;            

            if(j1 is FixedJoint)
            {
                var j = (FixedJoint)j1;
                var jj = (FixedJoint)j2;                
            }

            if(j1 is HingeJoint)
            {
                var j = (HingeJoint)j1;
                var jj = (HingeJoint)j2;
            }

            if(j1 is SpringJoint)
            {
                var j = (SpringJoint)j1;
                var jj = (SpringJoint)j2;
            }

            if(j1 is CharacterJoint)
            {
                var j = (CharacterJoint)j1;
                var jj = (CharacterJoint)j2;
            }

            if(j1 is ConfigurableJoint)
            {
                var j = (ConfigurableJoint)j1;
                var jj = (ConfigurableJoint)j2;
            }            
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

        public static void SaveMesh(Mesh mesh, string path, bool makeNewInstance, bool optimizeMesh, bool overwrite)
        {
            if(string.IsNullOrEmpty(path))
                return;

            if(overwrite)
                AssetDatabase.DeleteAsset(path);

            Mesh meshToSave = (makeNewInstance) ? Instantiate(mesh) as Mesh : mesh;

            if(optimizeMesh)
                MeshUtility.Optimize(meshToSave);

            AssetDatabase.CreateAsset(meshToSave, path);
            AssetDatabase.SaveAssets();
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

                            #endregion
    }
                            #region Data Structures

    public class BlendShapeFrame
    {
        public string name;
        public float weight;

        public Vector3[] deltaVertices, deltaNormals, deltaTangents;

        private BlendShapeFrame() { }

        public BlendShapeFrame(string name, float weight, Vector3[] deltaVertices, Vector3[] deltaNormals, Vector3[] deltaTangents)
        {
            this.name = name;            
            this.deltaVertices = deltaVertices;
            this.deltaNormals = deltaNormals;
            this.deltaTangents = deltaTangents;
            this.weight = weight;
        }
    }

    public static class Styles
    {
        public static GUIStyle Foldout_title { get; private set; }
        public static GUIStyle Label_mainTitle { get; private set; }
        public static GUIStyle Label_centered { get; private set; }
        public static GUIStyle Editor_line { get; private set; }
        public static GUIStyle Label_rightAligned { get; private set; }

        static Styles()
        {
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
                border = new RectOffset(1,1,1,1),
                margin = new RectOffset(5,5,1,1),
                padding = new RectOffset(1,1,1,1),
            };
        }
    }

    public struct Icons
    {
        public static Texture2D Star { get; private set; }
        public static Texture2D CsScript { get; private set; }
        public static Texture2D Transform { get; private set; }
        public static Texture2D Avatar { get; private set; }
        public static Texture2D SkinnedMeshRenderer { get; private set; }
        public static Texture2D ColliderBox { get; private set; }
        public static Texture2D DefaultAsset { get; private set; }
        public static Texture2D Help { get; private set; }
        public static Texture2D ParticleSystem { get; private set; }
        public static Texture2D RigidBody { get; private set; }
        public static Texture2D Prefab { get; private set; }
        public static Texture2D TrailRenderer { get; private set; }
        public static Texture2D BoneIcon { get; private set; }
        public static Texture2D BoneColliderIcon { get; private set; }
        public static Texture2D MeshRenderer { get; private set; }
        public static Texture2D Light { get; private set; }
        public static Texture2D Animator { get; private set; }
        public static Texture2D AudioSource { get; private set; }
        public static Texture2D Joint { get; private set; }

        public static Texture2D DiscordIcon { get; private set; }
        public static Texture2D GithubIcon { get; private set; }
        public static Texture2D KofiIcon { get; private set; }        

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
        public static readonly string version = "0.6b";
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
            public static string SelectAvatarFirst { get; private set; }

            public static string Name { get; private set; }
            public static string GameObjects { get; private set; }
            public static string Bones { get; private set; }
            public static string SkinnedMeshRenderers { get; private set; }
            public static string MeshRenderers { get; private set; }
            public static string Polygons { get; private set; }
            public static string UsedMaterialSlots { get; private set; }
            public static string UniqueMaterials { get; private set; }
            public static string Shaders { get; private set; }
            public static string DynamicBoneTransforms { get; private set; }
            public static string DynamicBoneColliders { get; private set; }
            public static string DynamicBoneColliderTransforms { get; private set; }
            public static string ParticleSystems { get; private set; }
            public static string MaxParticles { get; private set; }
            public static string OverallPerformance { get; private set; }
            public static string Line { get; private set; }

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
            public static string Title { get; private set; }
            public static string WindowName { get; private set; }
            public static string Version { get; private set; }
            public static string Avatar { get; private set; }
            public static string Tools { get; private set; }
            public static string Copier { get; private set; }
            public static string RemoveAll { get; private set; }
            public static string AvatarInfo { get; private set; }   
            public static string Thumbnails { get; private set; }
            
            public static string Misc { get; private set; }
            public static string UseSceneSelection { get; private set; }

            static Main()
            {
                Reload();
            }

            public static void Reload()
            {
                Avatar = GetString("ui_main_avatar") ?? "_Avatar";
                Title = GetString("ui_main_title") ?? "_Pumkin's Avatar Tools (Beta)";
                Version = GetString("ui_main_version") ?? "_Version";
                WindowName = GetString("ui_main_windowName") ?? "_Avatar Tools";
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
            public static string SelectFromScene { get; private set; }
            public static string CopySelected { get; private set; }
            public static string Cancel { get; private set; }
            public static string Apply { get; private set; }
            public static string Refresh { get; private set; }
            public static string Copy { get; private set; }
            public static string OpenHelpPage { get; private set; }
            public static string OpenGithubPage { get; private set; }
            public static string OpenDonationPage { get; private set; }
            public static string OpenPoseEditor { get; private set; }
            public static string JoinDiscordServer { get; private set; }
            public static string SelectNone { get; private set; }
            public static string SelectAll { get; private set; }

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
            }
        };
        public static class Tools
        {
            public static string FillVisemes { get; private set; }
            public static string EditViewpoint { get; private set; }
            public static string RevertBlendshapes { get; private set; }
            public static string ZeroBlendshapes { get; private set; }
            public static string ResetPose { get; private set; }
            public static string ResetToTPose { get; private set; }
            
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
            }
        };
        public static class Thumbnails
        {
            public static string OverlayCameraImage { get; private set; }
            public static string OverlayTexture { get; private set; }
            public static string StartUploadingFirst { get; private set; }
            public static string SetBackgroundColor { get; private set; }
            public static string CenterCameraOnFace { get; private set; }

            static Thumbnails()
            {
                Reload();
            }

            public static void Reload()
            {
                OverlayCameraImage = GetString("ui_thumbnails_overlayCameraImage") ?? "_Overlay Camera Image";
                OverlayTexture = GetString("ui_thumbnails_overlayTexture") ?? "_Overlay Texture";
                StartUploadingFirst = GetString("ui_thumbnails_startUploadingFirst") ?? "_Start uploading an Avatar first";
                SetBackgroundColor = GetString("ui_thumbnails_setBackgroundColor") ?? "_Set Background Color";                
            }
        }
        public static class Copier
        {
            public static string CopyFrom { get; private set; }
            public static string CopyGameObjects { get; private set; }
            public static string CopySettings { get; private set; }
            public static string CreateMissing { get; private set; }

            public static string Transforms { get; private set; }
            public static string Transforms_position { get; private set; }
            public static string Transforms_rotation { get; private set; }
            public static string Transforms_scale { get; private set; }
            public static string Transforms_avatarScale { get; private set; }
            public static string DynamicBones { get; private set; }            
            public static string DynamicBones_colliders { get; private set; }
            public static string DynamicBones_removeOldBones { get; private set; }
            public static string DynamicBones_removeOldColliders { get; private set; }
            public static string DynamicBones_createMissingBones { get; private set; }
            public static string Colliders { get; private set; }
            public static string Colliders_box { get; private set; }
            public static string Colliders_capsule { get; private set; }
            public static string Colliders_sphere { get; private set; }
            public static string Colliders_mesh { get; private set; }
            public static string Colliders_removeOld { get; private set; }
            public static string Descriptor { get; private set; }            
            public static string Descriptor_pipelineId { get; private set; }
            public static string Descriptor_animationOverrides { get; private set; }
            public static string SkinMeshRender { get; private set; }            
            public static string SkinMeshRender_materials { get; private set; }
            public static string SkinMeshRender_blendShapeValues { get; private set; }
            public static string ParticleSystems { get; private set; }
            public static string RigidBodies { get; private set; }
            public static string TrailRenderers { get; private set; }
            public static string EmptyGameObjects { get; private set; }
            public static string MeshRenderers { get; private set; }
            public static string Lights { get; private set; }
            public static string Animators { get; private set; }
            public static string CopyMainAnimator { get; private set; }
            public static string ReplaceOld { get; private set; }
            public static string Animators_inChildren { get; private set; }
            public static string AudioSources { get; private set; }
            public static string Joints { get; private set; }

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
                DynamicBones_createMissingBones = GetString("ui_copier_dynamicBones_createMissing") ?? "_Create Missing Bones";
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
            public static string CopyAttempt { get; private set; }
            public static string RemoveAttempt { get; private set; }
            public static string CopyFromInvalid { get; private set; }
            public static string Done { get; private set; }
            public static string Failed { get; private set; }
            public static string CantCopyToSelf { get; private set; }
            public static string ViewpointApplied { get; private set; }
            public static string ViewpointCancelled { get; private set; }
            public static string Cancelled { get; private set; }
            public static string NoSkinnedMeshFound { get; private set; }
            public static string DescriptorIsNull { get; private set; }
            public static string Success { get; private set; }
            public static string TryFillVisemes { get; private set; }
            public static string TryRemoveUnsupportedComponent { get; private set; }
            public static string MeshHasNoVisemes { get; private set; }
            public static string FailedIsNull { get; private set; }       
            public static string NameIsEmpty { get; private set; }
            public static string LoadedPose { get; private set; }
            public static string LoadedBlendshapePreset { get; private set; }
            public static string NothingSelected { get; private set; }
            public static string FailedDoesntHave { get; private set; }
            public static string FailedNotSupported { get; private set; }
            public static string FailedAlreadyHas { get; private set; }
            public static string LoadedCameraOverlay { get; private set; }
            public static string FailedHasNo { get; private set; }

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
                FailedAlreadyHas = GetString("log_failedAlreadyHas") ?? "_Failed: {0} already has {1}";
                LoadedCameraOverlay = GetString("log_loadedCameraOverlay") ?? "_Loaded {0} as Camera Overlay";
                FailedHasNo = GetString("log_failedHasNo") ?? "_{0} has no {1}, Ignoring.";
            }
        };        
        public static class Warning
        {
            public static string Warn { get; private set; }
            public static string NotFound { get; private set; }
            public static string SelectSceneObject { get; private set; }
			public static string OldVersion { get; private set; }

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
            }
        };
        public static class Credits
        {
            public static string Title { get; private set; }
            public static string Version { get; private set; }
            public static string RedundantStrings { get; private set; }
            public static string JsonDotNetCredit { get; private set; }
            public static string AddMoreStuff { get; private set; }
            public static string PokeOnDiscord { get; private set; }

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
            public static string uwu { get; private set; }
            public static string SearchForBones { get; private set; }
            public static string SuperExperimental { get; private set; }

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
            public static string Title { get; private set; }

            public static string Scene { get; private set; }            
            public static string SceneLoadAdditive { get; private set; }
            public static string SceneOverrideLights { get; private set; }

            public static string AvatarPosition { get; private set; }
            public static string AvatarPositionOverridePose { get; private set; }
            public static string AvatarPositionOverrideBlendshapes { get; private set; }

            public static string SceneSaveChanges { get; private set; }
            public static string UnloadScene { get; private set; }
            public static string ResetPosition { get; private set; }

            public static string Pose { get; private set; }
            public static string NewPose { get; private set; }
            public static string OnlySavePoseChanges { get; private set; }
            public static string LoadPose { get; private set; }

            public static string Blendshapes { get; private set; }
            public static string NewPreset { get; private set; }
            public static string LoadPreset { get; private set; }

            public static string SaveButton { get; private set; }
            public static string ReloadButton { get; private set; }

            public static string BodyPositionYTooSmall { get; private set; }            

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
                {"ui_main_title", "Pumkin's Avatar Tools (Beta)" },
                {"ui_main_windowName", "Avatar Tools" },
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


                //Thumbnails                
                {"ui_thumbnails_overlayCameraImage", "Overlay Camera Image" },
                {"ui_thumbnails_overlayTexture",  "Overlay Texture"},
                {"ui_thumbnails_startUploadingFirst", "Begin uploading an Avatar first" },
                {"ui_thumbnails_setBackgroundColor", "Set Background Color" },
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
                {"log_failedAlreadyHas", "Failed: {0} already has {1}" },
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
                {"ui_main_windowName", "Avataw Awoos" },
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

                //Thumbnails                
                { "ui_thumbnails_overlayCameraImage", "Ovewwide Camewa Image" },
                { "ui_thumbnails_overlayTexture",  "Ovewwide Textuwe"},
                { "ui_thumbnails_startUploadingFirst", "Stawt upwoading Avataw fiwst!!" },
                { "ui_thumbnails_setBackgroundColor", "Set Background Color" },
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

    public class AvatarStat
    {
        public string Name
        {
            get; private set;
        }
        public int Value
        {
            get; private set;
        }        
        public int[] Levels
        {
            get; private set;
        }
                
        public enum StatLevel {Excellent = -1, VeryGood, Good, Medium, Poor, VeryPoor, Invalid = 100};

        public AvatarStat(string name, int val_poor, int val_medium, int val_good, int val_veryGood)
        {
            Name = name;
            Levels = new int[] { val_veryGood, val_good, val_medium, val_poor };
        }

        public StatLevel GetStatLevelPolygon(int polycount)
        {
            if(polycount <= Levels[(int)StatLevel.VeryGood])
                return StatLevel.VeryGood;
            else if(polycount > Levels[(int)StatLevel.VeryGood] && polycount < Levels[(int)StatLevel.Good])
                return StatLevel.Good;
            else
                return StatLevel.Poor;
        }

        public StatLevel GetStatLevel(int stat)
        {
            if(stat < Levels[0])
                return StatLevel.Excellent;

            for(int i = 0; i < Levels.Length; i++)            
                if(stat <= Levels[i])                
                    return (StatLevel)i;            

            if(stat > Levels[Levels.Length - 1])
                return StatLevel.VeryPoor;

            return StatLevel.Invalid;
        }
    }

    public class AvatarInfo
    {   
        readonly List<AvatarStat> statRanks = new List<AvatarStat>()
        {
            { new AvatarStat("Polygons", 70000, 70000, 70000, 32000 ) },
            { new AvatarStat("Skinned Meshes", 16, 8, 2, 1) },
            { new AvatarStat("Meshes", 24, 16, 8, 4) },
            { new AvatarStat("Unique Materials", 24, 16, 8, 4) },
            { new AvatarStat("Dynamic Bone Components", 24, 16, 8, 4) },
            { new AvatarStat("Dynamic Bone Transforms", 24, 16, 8, 4) },
            { new AvatarStat("Dynamic Bone Colliders", 24, 16, 8, 4) },
            { new AvatarStat("Dynamic Bone Collision Check Count", 24, 16, 8, 4) },
            { new AvatarStat("Animators", 24, 16, 8, 4) },
            { new AvatarStat("Bones", 24, 16, 8, 4) },
            { new AvatarStat("Lights", 24, 16, 8, 4) },
            { new AvatarStat("Particle Systems", 24, 16, 8, 4) },
            { new AvatarStat("Total Particles Active", 24, 16, 8, 4) },
            { new AvatarStat("Mesh Particle Active Polys", 24, 16, 8, 4) },
            { new AvatarStat("Particle Trails Enabled", 1, 1, 0, 0) },
            { new AvatarStat("Particle Collision Enabled", 1, 1, 0, 0) },
            { new AvatarStat("Trail Renderers", 8, 4, 2, 1) },
            { new AvatarStat("Line Renderers", 8, 4, 2, 1) },
            { new AvatarStat("Cloths", 1, 1, 1, 0) },
            { new AvatarStat("Total Cloth Vertices", 200, 100, 5, 0) },
            { new AvatarStat("Physics Colliders", 8, 8, 1, 0) },
            { new AvatarStat("Physics Rigidbodies", 8, 8, 1, 0) },
            { new AvatarStat("Audio Sources", 8, 8, 4, 1) },
        };
        VRCSDK2.AvatarPerformanceStats perfStats;

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

            perfStats = AvatarPerformance.CalculatePerformanceStats(o.name, o);

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

#if !NO_BONES

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

        public AvatarStat.StatLevel GetStatLevel(string statName, int statNr)
        {
            if(string.IsNullOrEmpty(statName))
                return AvatarStat.StatLevel.Invalid;

            var stat = statRanks.Find(x => !string.IsNullOrEmpty(x.Name) && x.Name.ToLower() == statName.ToLower());

            if(stat != null && stat.Name.ToLower() == "polygons")            
                return stat.GetStatLevelPolygon(statNr);
                        
            return stat.GetStatLevel(statNr);
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
                    string.Format(Strings.AvatarInfo.Bones, perfStats.BoneCount, perfStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.BoneCount)) + "\n\n" +
                    string.Format(Strings.AvatarInfo.SkinnedMeshRenderers, SkinnedMeshRenderers, SkinnedMeshRenderers_Total, perfStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.SkinnedMeshCount)) + "\n" +
                    string.Format(Strings.AvatarInfo.MeshRenderers, MeshRenderers, MeshRenderers_Total, perfStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.MeshCount)) + "\n" +
                    string.Format(Strings.AvatarInfo.Polygons, Polygons, Polygons_Total, perfStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.PolyCount)) + "\n\n" +
                    string.Format(Strings.AvatarInfo.UsedMaterialSlots, MaterialSlots, MaterialSlots_Total, perfStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.MaterialCount)) + "\n" +
                    string.Format(Strings.AvatarInfo.UniqueMaterials, UniqueMaterials, UniqueMaterials_Total) + "\n" +
                    string.Format(Strings.AvatarInfo.Shaders, ShaderCount) + "\n\n" +
                    string.Format(Strings.AvatarInfo.DynamicBoneTransforms, DynamicBoneTransforms, DynamicBoneTransforms_Total, perfStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.DynamicBoneAffectedTransformCount)) + "\n" +
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
                            tNew = new GameObject(arr[i]).transform;
                            if(i == 0)
                                tNew.parent = transform;
                            else
                            {
                                string s = path.Substring(0, path.IndexOf(arr[i]));                                
                                tNew.parent = transform.Find(s);
                                var trans = sourceTransform.root.Find(s + arr[i]);
                                if(trans != null)
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
            }
            return t;
        }

    }
                            #endregion
}
