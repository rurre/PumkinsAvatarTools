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
using Pumkin.Helpers;
using UnityEngine.UI;

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
        bool bCopier_transforms_copyPosition = true;
        bool bCopier_transforms_copyRotation = true;
        bool bCopier_transforms_copyScale = true;
        bool bCopier_transforms_copyAvatarScale = true;

        bool bCopier_dynamicBones_copy = true;
        bool bCopier_dynamicBones_copySettings = true;        
        bool bCopier_dynamicBones_createMissingBones = true;
        bool bCopier_dynamicBones_copyColliders = true;        
        bool bCopier_dynamicBones_removeOldColliders = true;
        bool bCopier_dynamicBones_removeOldBones = true;

        bool bCopier_descriptor_copy = true;
        bool bCopier_descriptor_copySettings = true;
        bool bCopier_descriptor_copyPipelineId = true;
        bool bCopier_descriptor_copyAnimationOverrides = true;

        bool bCopier_colliders_copy = true;        
        bool bCopier_colliders_removeOld = true;
        bool bCopier_colliders_copyBox = true;
        bool bCopier_colliders_copyCapsule = true;
        bool bCopier_colliders_copySphere = true;
        bool bCopier_colliders_copyMesh = true;
        bool bCopier_colliders_createObjects = true;

        bool bCopier_skinMeshRender_copy = true;
        bool bCopier_skinMeshRender_copySettings = true;
        bool bCopier_skinMeshRender_copyBlendShapeValues = true;        
        bool bCopier_skinMeshRender_copyMaterials = true;

        bool bCopier_rigidbodies_copy = true;
        bool bCopier_rigidbodies_copySettings = true;
        bool bCopier_rigidbodies_createMissing = true;

        bool bCopier_constraints_copy = true;
        bool bCopier_constraints_copySettings = true;
        bool bCopier_constraints_createMissing = true;

        bool bCopier_particleSystems_copy = true;
        bool bCopier_particleSystems_replace = false;
        bool bCopier_particleSystems_createObjects = true;

        //Editor
        bool _copier_expand = false;
        bool _copier_expand_transforms = false;
        bool _copier_expand_dynamicBones = false;
        bool _copier_expand_avatarDescriptor = false;
        bool _copier_expand_skinnedMeshRenderer = false;
        bool _copier_expand_colliders = false;
        bool _copier_expand_gameObjects = false;
                
        //Thumbnails
        bool bThumbnails_enable_animations = false;
        bool bThumbnails_override_camera_image = false;
        Texture2D cameraOverrideTexture = null;
        GameObject cameraOverlay = null;
        RawImage cameraOverlayImage = null;

        GameObject _vrcCam = null;


        bool _tools_expand = true;
        bool _avatarInfo_expand = false;
        bool _thumbnails_expand = false;        
        Vector2 _avatarInfo_scroll = Vector2.zero;
        
        bool _misc_expand = true;

        //Misc
        bool _openedInfo = false;
        Vector2 vertScroll = Vector2.zero;
        static string _mainScriptPath = null;
               

        static Avatar _tposeAvatar = null;

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

        //Thumbnail Stuff

        int _animationId = 0;
        Animator _animator;        

        enum ToolMenuActions
        {
            RemoveDynamicBones,
            RemoveDynamicBoneColliders,
            RemoveColliders,
            ResetPose,
            RevertBlendshapes,
            ZeroBlendshapes,
            FixRandomMouth,
            DisableBlinking,
            EditViewpoint,
            FillVisemes,       
            RemoveEmptyGameObjects,
            //RemoveEmptyBones,
            RemoveParticleSystems,
            SetTPose,
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
            typeof(PipelineManager)
        };

#endregion

#region Unity GUI

        void OnEnable()
        {
            SceneView.onSceneGUIDelegate += this.OnSceneGUI;

            Selection.selectionChanged += SelectAvatarFromScene_callback;
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

                GUILayout.Label(Strings.Credits.Line1);
                GUILayout.Label(Strings.Credits.Line2);
                GUILayout.Label(Strings.Credits.Line3);
                EditorGUILayout.Space();
                GUILayout.Label(Strings.Credits.Line4);

                GUILayout.BeginHorizontal();

                GUILayout.Label(Strings.Credits.Line5);

                GUILayout.EndHorizontal();
                GUILayout.EndVertical();

                EditorGUILayout.Space();
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
                
                _useSceneSelectionAvatar = GUILayout.Toggle(_useSceneSelectionAvatar, "_Use Scene Selection");

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

                                if(GUILayout.Button(Strings.Tools.resetToTPose))
                                    ActionButton(ToolMenuActions.SetTPose);
                            }
                            GUILayout.EndVertical();
                        }
                        GUILayout.EndHorizontal();

                        /*if(GUILayout.Button("_Fix Mouth Randomly Opening"))
                        {
                            ActionButton(ToolMenuActions.FixRandomMouth);
                        }

                        if(GUILayout.Button("_Disable Blinking"))
                        {
                            ActionButton(ToolMenuActions.DisableBlinking);
                        }*/

                        EditorGUILayout.Space();
                        EditorGUILayout.LabelField(Strings.Main.RemoveAll + ":");

                        EditorGUILayout.BeginHorizontal();

                        EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
#if NO_BONES
                        EditorGUI.BeginDisabledGroup(true);
#endif
                        if(GUILayout.Button(new GUIContent(Strings.Copier.DynamicBones, Icons.DefaultAsset)))
                        {
                            ActionButton(ToolMenuActions.RemoveDynamicBones);
                        }
#if NO_BONES
                        EditorGUI.EndDisabledGroup();
#endif
                        if(GUILayout.Button(new GUIContent(Strings.Copier.Colliders, Icons.ColliderBox)))
                        {
                            ActionButton(ToolMenuActions.RemoveColliders);
                        }
                        /*if(GUILayout.Button(new GUIContent("_Empty Bones (Slow)", Icons.DefaultAsset)))
                        {
                            ActionButton(ToolMenuActions.RemoveEmptyBones);                            
                        }*/
                        EditorGUILayout.EndVertical();

#if NO_BONES
                        EditorGUI.BeginDisabledGroup(true);
#endif
                        EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
                        if(GUILayout.Button(new GUIContent(Strings.Copier.DynamicBones_colliders, Icons.DefaultAsset)))
                        {
                            ActionButton(ToolMenuActions.RemoveDynamicBoneColliders);
                        }
#if NO_BONES
                        EditorGUI.EndDisabledGroup();                        
#endif
                        if(GUILayout.Button(new GUIContent(Strings.Copier.ParticleSystems, Icons.ParticleSystem)))
                        {
                            ActionButton(ToolMenuActions.RemoveParticleSystems);
                        }

                        if(GUILayout.Button(new GUIContent("_Empty GameObjects", Icons.DefaultAsset)))
                        {
                            ActionButton(ToolMenuActions.RemoveEmptyGameObjects);
                        }

                        EditorGUILayout.EndVertical();

                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUI.EndDisabledGroup();
                    EditorGUILayout.Space();
                }

                GuiLine();
                
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
                        /*//GameObject menu

                        EditorGUILayout.BeginHorizontal();
                        _copier_expand_gameObjects = GUILayout.Toggle(_copier_expand_gameObjects, Icons.Star, "Foldout", GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(true), GUILayout.MaxWidth(30), GUILayout.MaxHeight(10));
                        bCopier_gameObjects_copy = GUILayout.Toggle(bCopier_gameObjects_copy, Strings.Copier.GameObjects, GUILayout.ExpandWidth(false), GUILayout.MinWidth(20));
                        EditorGUILayout.EndHorizontal();

                        if(_copier_expand_gameObjects)
                        {
                            EditorGUI.BeginDisabledGroup(!bCopier_gameObjects_copy);
                            EditorGUILayout.Space();

                            bCopier_gameObjects_createMissing = EditorGUILayout.Toggle(Strings.Copier.GameObjects_createMissing, bCopier_gameObjects_createMissing, GUILayout.ExpandWidth(false));

                            EditorGUILayout.Space();
                            EditorGUI.EndDisabledGroup();
                        }*/

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
                        _copier_expand_dynamicBones = GUILayout.Toggle(_copier_expand_dynamicBones, Icons.CsScript, "Foldout", GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(true), GUILayout.MaxWidth(30), GUILayout.MaxHeight(10));
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

                            bCopier_dynamicBones_copySettings = EditorGUILayout.Toggle(Strings.Copier.DynamicBones_settings, bCopier_dynamicBones_copySettings, GUILayout.ExpandWidth(false));
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

                            bCopier_descriptor_copySettings = EditorGUILayout.Toggle(Strings.Copier.Descriptor_settings, bCopier_descriptor_copySettings, GUILayout.ExpandWidth(false));
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

                            bCopier_skinMeshRender_copySettings = EditorGUILayout.Toggle(Strings.Copier.SkinMeshRender_settings, bCopier_skinMeshRender_copySettings, GUILayout.ExpandWidth(false));
                            bCopier_skinMeshRender_copyMaterials = EditorGUILayout.Toggle(Strings.Copier.SkinMeshRender_materials, bCopier_skinMeshRender_copyMaterials, GUILayout.ExpandWidth(false));
                            bCopier_skinMeshRender_copyBlendShapeValues = EditorGUILayout.Toggle(Strings.Copier.SkinMeshRender_blendShapeValues, bCopier_skinMeshRender_copyBlendShapeValues, GUILayout.ExpandWidth(false));
                            
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
                            bCopier_colliders_createObjects = EditorGUILayout.Toggle("Copy GameObjects", bCopier_colliders_createObjects, GUILayout.ExpandWidth(false));

                            EditorGUILayout.Space();
                            EditorGUI.EndDisabledGroup();
                        }

                        //Particles menu
                        EditorGUILayout.BeginHorizontal();
                        //_copier_expand_particleSystems = GUILayout.Toggle(_copier_expand_particleSystems, Icons.Transform, "Foldout", GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(true), GUILayout.MaxWidth(30), GUILayout.MaxHeight(10));
                        bCopier_particleSystems_copy = GUILayout.Toggle(bCopier_particleSystems_copy, Strings.Copier.ParticleSystems, GUILayout.ExpandWidth(false), GUILayout.MinWidth(20));
                        EditorGUILayout.EndHorizontal();

                        //if(_copier_expand_transforms)
                        //{
                        //EditorGUI.BeginDisabledGroup(!bCopier_transforms_copy);
                        //EditorGUILayout.Space();

                        //bCopier_transforms_copyAvatarScale = EditorGUILayout.Toggle(Strings.Copier.Transforms_avatarScale, bCopier_transforms_copyAvatarScale);


                        //EditorGUILayout.Space();
                        //EditorGUI.EndDisabledGroup();
                        //}



                        EditorGUILayout.Space();
                    
                        EditorGUI.BeginDisabledGroup(!(bCopier_dynamicBones_copyColliders || bCopier_dynamicBones_copy || bCopier_colliders_copy || bCopier_descriptor_copy || bCopier_skinMeshRender_copy || bCopier_particleSystems_copy));
                        {
                            if(GUILayout.Button(Strings.Buttons.CopySelected))
                            {
                                string log = "";
                                if(copierSelectedFrom == null)
                                {
                                    log += Strings.Log.CopyFromInvalid;
                                    Log(log, LogType.Warning);
                                }
                                else
                                {
                                    /*//Prefab Check. Disabled copying to prefabs
                                    if(selectedAvatar.gameObject.scene.name == null)
                                    {
                                        if(!EditorUtility.DisplayDialog(Strings.GetString("warn_warning") ?? "_Warning",
                                            Strings.GetString("warn_copyToPrefab") ?? "_You are trying to copy components to a prefab.\nThis cannot be undone.\nAre you sure you want to continue?",
                                            Strings.GetString("warn_prefabOverwriteYes") ?? "_Yes, Overwrite", Strings.GetString("warn_prefabOverwriteNo") ?? "_No, Cancel"))
                                        {
                                            _msg = Strings.GetString("log_cancelled") ?? "_Canceled.";
                                            return;
                                        }
                                    }*/

                                    //Cancel Checks
                                    if(copierSelectedFrom == selectedAvatar)
                                    {
                                        log += Strings.Log.CantCopyToSelf;
                                        Log(log, LogType.Warning);
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
                    EditorGUILayout.Space();                    
                }

                GuiLine();

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

                        //if(GUILayout.Button("_Show detailed info"))
                        //{
                        //    /*avatarInfo = AvatarInfo.GetInfo(selectedAvatar, out _avatarInfoString);
                        //    _avatarInfoString = "" +
                        //    //string.Format("_GameObjects: {0} ({1}) - \n\n", avatarInfo.GameObjects, avatarInfo.GameObjects_Total, avatarInfo.GetStatLevel("gameobjects", avatarInfo.GameObjects));                            
                        //    string.Format("Skinned Mesh Renderers {0} ({1}) -\t{2}", avatarInfo.SkinnedMeshRenderers, avatarInfo.SkinnedMeshRenderers_Total, avatarInfo.GetStatLevel("skinned meshes", avatarInfo.SkinnedMeshRenderers)) + '\n' +
                        //    string.Format("Mesh Renderers: {0} ({1}) -\t\t{2}", avatarInfo.MeshRenderers, avatarInfo.MeshRenderers_Total, avatarInfo.GetStatLevel("Meshes", avatarInfo.MeshRenderers)) + '\n' +
                        //    string.Format("Polygons: {0} ({1}) -\t\t{2}", avatarInfo.Polygons, avatarInfo.Polygons_Total, avatarInfo.GetStatLevel("Polygons", avatarInfo.Polygons));
                        //    /*string.Format("_Used Material Slots: {0} ({1}) - \n", avatarInfo.MaterialSlots, avatarInfo.MaterialSlots_Total,
                        //    string.Format("_Unique Materials: {0} ({1}) - \n", avatarInfo.UniqueMaterials, avatarInfo.Ma
                        //    string.Format("_Shaders: {0} \n\n",
                        //    string.Format("_Dynamic Bone Transforms: {0} ({1}) - \n",
                        //    string.Format("_Dynamic Bone Colliders: {0} ({1}) - \n",
                        //    string.Format("_Collider Affected Transforms: {0} ({1}) - \n\n",
                        //    string.Format("_Particle Systems: {0} ({1}) - \n",
                        //    string.Format("_Max Particles: {0} ({1}) - ", 
                        //    */

                        //    VRCSDK2.AvatarPerformanceStats perfStats = AvatarPerformance.CalculatePerformanceStats(selectedAvatar.name, selectedAvatar);
                        //    Debug.Log(perfStats);
                        //}
                    }
                }
                EditorGUI.EndDisabledGroup();

                GuiLine();

                //Thumbnails menu                
                if(_thumbnails_expand = GUILayout.Toggle(_thumbnails_expand, Strings.Main.Thumbnails, Styles.Foldout_title))
                {                    
                    //EditorGUI.BeginDisabledGroup(!EditorApplication.isPlaying);
                    //{
                    //    if(!EditorApplication.isPlaying)
                    //        EditorGUILayout.HelpBox("Start uploading avatar first", MessageType.Info);

                    //    if(bThumbnails_enable_animations = GUILayout.Toggle(bThumbnails_enable_animations, "_Override Animations"))
                    //    {
                    //        if(selectedAvatar != null)
                    //        {
                    //            if(_animator == null)
                    //                _animator = selectedAvatar.GetComponent<Animator>();

                    //            if(_animator != null)
                    //            {
                    //                if(_animator.runtimeAnimatorController == null)
                    //                {
                    //                    var c = Resources.Load<RuntimeAnimatorController>("ThumbnailAnimations") as RuntimeAnimatorController;
                    //                    _animator.runtimeAnimatorController = c;
                    //                }

                    //                _animationId = EditorGUILayout.IntSlider("_Animation", _animationId, 0, 19);
                    //                _animator.SetInteger("pose", _animationId);
                    //            }
                    //        }
                    //        else
                    //        {
                    //            if(_animator != null)
                    //            {
                    //                _animator.runtimeAnimatorController = null;
                    //            }
                    //        }
                    //    }
                    //    else
                    //    {
                    //        if(_animator != null)
                    //            _animator.runtimeAnimatorController = null;
                    //    }
                    //}
                    //EditorGUI.EndDisabledGroup();
                    //EditorGUILayout.Space();
                    
                    //Camera Override Image
                    EditorGUI.BeginDisabledGroup(!EditorApplication.isPlaying);
                    {
                        EditorGUI.BeginChangeCheck();
                        {
                            bThumbnails_override_camera_image = GUILayout.Toggle(bThumbnails_override_camera_image, Strings.Thumbnails.OverrideCameraImage);
                            EditorGUI.BeginDisabledGroup(!bThumbnails_override_camera_image);
                            {
                                cameraOverrideTexture = EditorGUILayout.ObjectField(Strings.Thumbnails.OverrideTexture, cameraOverrideTexture, typeof(Texture2D), false) as Texture2D;
                            }
                            EditorGUI.EndDisabledGroup();
                        }                        
                        if(EditorGUI.EndChangeCheck())
                        {
                            if(bThumbnails_override_camera_image)
                            {
                                if(cameraOverrideTexture != null)
                                {
                                    if(_vrcCam == null)
                                        _vrcCam = GameObject.Find("VRCCam");

                                    if(cameraOverlay == null && _vrcCam != null)
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

                    if(!EditorApplication.isPlaying)
                    {
                        EditorGUILayout.HelpBox(Strings.Thumbnails.StartUploadingFirst, MessageType.Info);
                    }

                    //if(GUILayout.Button(Strings.Buttons.OpenPoseEditor))
                    //{
                    //    PumkinsPoseEditor.ShowWindow();
                    //}                    
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

                GuiLine();

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

                        if(GUILayout.Button(new GUIContent(Strings.Buttons.OpenGithubPage, Icons.Star)))
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
                    if(GUILayout.Button(new GUIContent(Strings.Buttons.OpenDonationPage, Icons.Star)))
                    {
                        Application.OpenURL(Strings.donationLink);
                    }
                }

                EditorGUILayout.EndScrollView();
            }
        }

        private static void SelectAvatarFromScene_callback()
        {
            if(_useSceneSelectionAvatar)
                SelectAvatarFromScene();
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
                Debug.LogWarning("_No avatar selected");
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
                //case ToolMenuActions.FixRandomMouth:
                //    FixRandomMouthOpening(selectedAvatar);
                //   break;
                //case ToolMenuActions.DisableBlinking:
                //    DisableBlinking(selectedAvatar);
                //    break;
                case ToolMenuActions.FillVisemes:
                    FillVisemes(selectedAvatar);
                    break;
                case ToolMenuActions.EditViewpoint:
                    BeginEditViewpoint(selectedAvatar);
                    break;
                //case ToolMenuActions.RemoveEmptyBones:
                //    DestroyEmptyBonesNew(selectedAvatar);
                //    break;
                case ToolMenuActions.RemoveEmptyGameObjects:
                    DestroyEmptyGameObjects(selectedAvatar);
                    break;
                case ToolMenuActions.RemoveParticleSystems:
                    DestroyParticleSystems(selectedAvatar);
                    break;
                case ToolMenuActions.ZeroBlendshapes:
                    ResetBlendShapes(selectedAvatar, false);
                    break;
                case ToolMenuActions.SetTPose:
                    PumkinsPoseEditor.SetTPose(selectedAvatar);
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

            //Pre Copying Operations

            //Run statment only if root so only run this once
            if(objTo != null && objTo.transform == objTo.transform.root)
            {                
                if(bCopier_dynamicBones_copy)
                {
                    if(bCopier_dynamicBones_removeOldColliders)
                        DestroyAllDynamicBoneColliders(selectedAvatar);
                    if(bCopier_dynamicBones_removeOldBones)
                        DestroyAllDynamicBones(selectedAvatar);
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
                
                //The fact that this runs only once on all children from within the function is quite inconsistent, I agree.
                //But I guess this is a better way of doing things.
                if(bCopier_particleSystems_copy)
                {
                    CopyParticleSystems(objFrom, objTo, bCopier_particleSystems_createObjects);
                }
                if(bCopier_colliders_copy)
                {
                    CopyAllColliders(objFrom, objTo, bCopier_colliders_createObjects);
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
            //Using CopyAllCollidersInstead
            //if(bCopier_colliders_copy)
            //{                
            //    CopyColliders(objFrom, objTo);
            //}
            if(bCopier_skinMeshRender_copy)
            {                
                CopySkinMeshRenderer(objFrom, objTo);
            }
            //if(bCopier_rigidbodies_copy)
            //{
            //    CopyRigidBodies(objFrom, objTo);
            //}
            //if(bCopier_constraints_copy)
            //{
            //    CopyConstraints(objFrom, objTo);
            //}

            //Copy Components in Children
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
                    string s = Functions.GetGameObjectPath(dFrom.VisemeSkinnedMesh.gameObject, true);
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
						
                        string tFromPath = Functions.GetGameObjectPath(colls[z].gameObject);
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
                        string p = Functions.GetGameObjectPath(dFrom.m_Exclusions[z].gameObject, true);
                        var t = to.transform.root.Find(p);

                        if(t != null && dFrom.m_Exclusions[z].name == t.name)
                            el.Add(t);
                    }
                }
                dTo.m_Exclusions = el;

                if(dFrom.m_Root != null)
                {
                    string rootPath = Functions.GetGameObjectPath(dFrom.m_Root.gameObject, true);
                    if(!string.IsNullOrEmpty(rootPath))
                    {
                        var toRoot = dTo.transform.root.Find(rootPath);
                        if(!string.IsNullOrEmpty(rootPath))
                            dTo.m_Root = toRoot;
                    }
                }

                if(dFrom.m_ReferenceObject != null)
                {
                    string refPath = Functions.GetGameObjectPath(dFrom.m_ReferenceObject.gameObject, true);
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

            var arr = from.GetComponentsInChildren<Collider>();

            for(int i = 0; i < arr.Length; i++)
            {
                //if(arr[i] is BoxCollider || arr[i] is MeshCollider || arr[i] is SphereCollider || arr[i] is CapsuleCollider)
                var t = arr[i].GetType();
                if(supportedComponents.Contains(t))
                {
                    var cc = arr[i];
                    var cFromPath = Functions.GetGameObjectPath(cc.gameObject);

                    if(cFromPath != null)
                    {
                        GameObject cToObj = to.transform.root.Find(cFromPath, createGameObjects, cc.transform).gameObject;
                        CopyColliders(arr[i].gameObject, cToObj);
                    }
                }
            }
        }

        /// <summary>
        /// Copies Box, Capsule, Sphere and Mesh colliders from one object to another
        /// </summary>        
        void CopyColliders(GameObject from, GameObject to)
        {
            if(from == null || to == null)
                return;

            if(!(bCopier_colliders_copyBox || bCopier_colliders_copyCapsule || bCopier_colliders_copyMesh || bCopier_colliders_copySphere))
                return;

            string log = Strings.Log.CopyAttempt;            

            var cFromList = new List<Collider>();
            var cToList = new List<Collider>();            

            cFromList.AddRange(from.GetComponents<BoxCollider>());
            cFromList.AddRange(from.GetComponents<CapsuleCollider>());
            cFromList.AddRange(from.GetComponents<SphereCollider>());
            cFromList.AddRange(from.GetComponents<MeshCollider>());

            cToList.AddRange(to.GetComponents<BoxCollider>());
            cToList.AddRange(to.GetComponents<CapsuleCollider>());
            cToList.AddRange(to.GetComponents<SphereCollider>());
            cToList.AddRange(to.GetComponents<MeshCollider>());

            foreach(var cFrom in cFromList)
            {                
                bool found = false;
                string[] logFormat = {  cFrom.GetType().ToString(), from.name, to.name };

                foreach(var c in cToList)
                {                    
                    found = CollidersAreIdentical(c, cFrom);
                }
                if(!found)
                {
                    PhysicMaterial tempMat = new PhysicMaterial();
                    if(bCopier_colliders_copyBox && cFrom is BoxCollider)
                    {
                        BoxCollider cc = (BoxCollider)cFrom;
                        BoxCollider cTo = to.AddComponent<BoxCollider>();
                        cTo.size = cc.size;
                        cTo.center = cc.center;
                        cTo.contactOffset = cc.contactOffset;

                        cTo.isTrigger = cc.isTrigger;

                        if(cc.material == tempMat)
                            cTo.material = null;
                        else
                            cTo.material = cc.material;

                        if(cc.sharedMaterial == tempMat)
                            cTo.sharedMaterial = null;
                        else
                            cTo.sharedMaterial = cc.sharedMaterial;

                        cTo.enabled = cc.enabled;                        
                    }
                    else if(bCopier_colliders_copyCapsule && cFrom is CapsuleCollider)
                    {
                        CapsuleCollider cc = (CapsuleCollider)cFrom;
                        CapsuleCollider cTo = to.AddComponent<CapsuleCollider>();
                        cTo.direction = cc.direction;
                        cTo.center = cc.center;
                        cTo.radius = cc.radius;
                        cTo.height = cc.height;
                        cTo.contactOffset = cc.contactOffset;

                        cTo.isTrigger = cc.isTrigger;

                        if(cc.material == tempMat)
                            cTo.material = null;
                        else
                            cTo.material = cc.material;

                        if(cc.sharedMaterial == tempMat)
                            cTo.sharedMaterial = null;
                        else
                            cTo.sharedMaterial = cc.sharedMaterial;

                        cTo.enabled = cc.enabled;                        
                    }
                    else if(bCopier_colliders_copySphere && cFrom is SphereCollider)
                    {
                        SphereCollider cc = (SphereCollider)cFrom;
                        SphereCollider cTo = to.AddComponent<SphereCollider>();
                        cTo.center = cc.center;
                        cTo.radius = cc.radius;

                        cTo.contactOffset = cc.contactOffset;
                        cTo.isTrigger = cc.isTrigger;

                        if(cc.material == tempMat)
                            cTo.material = null;
                        else
                            cTo.material = cc.material;

                        if(cc.sharedMaterial == tempMat)
                            cTo.sharedMaterial = null;
                        else
                            cTo.sharedMaterial = cc.sharedMaterial;

                        cTo.enabled = cc.enabled;                        
                    }
                    else if(bCopier_colliders_copyMesh && cFrom is MeshCollider)
                    {
                        MeshCollider cc = (MeshCollider)cFrom;
                        MeshCollider cTo = to.AddComponent<MeshCollider>();

                        cTo.convex = cc.convex;
                        cTo.inflateMesh = cc.inflateMesh;
                        cTo.sharedMesh = cc.sharedMesh;
                        cTo.skinWidth = cc.skinWidth;

                        cTo.contactOffset = cc.contactOffset;
                        cTo.isTrigger = cc.isTrigger;

                        if(cc.material == tempMat)
                            cTo.material = null;
                        else
                            cTo.material = cc.material;

                        if(cc.sharedMaterial == tempMat)
                            cTo.sharedMaterial = null;
                        else
                            cTo.sharedMaterial = cc.sharedMaterial;

                        cTo.enabled = cc.enabled;                        
                    }
                    else
                    {
                        log += "_Failed: Unsupported Collider type {0} on {1}. Ignoring";
                        Log(log, LogType.Error, logFormat);
                        return;
                    }
                }
                else
                {
                    log += "_Failed: {0} already exists on {2}. Ignoring";
                    Log(log, LogType.Warning, logFormat);
                    return;
                }
                log += "_Success: Added {0} to {2}";
                Log(log, LogType.Log, logFormat);                
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
        /// Copies SkinnedMeshRenderer settings. Note that only one can exist on an object.
        /// </summary>                
        void CopySkinMeshRenderer(GameObject from, GameObject to)
        {
            if(!(bCopier_skinMeshRender_copyBlendShapeValues || bCopier_skinMeshRender_copyMaterials || bCopier_skinMeshRender_copySettings))
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
                    path = Functions.GetGameObjectPath(rFrom.probeAnchor.gameObject);

                if(!string.IsNullOrEmpty(path))
                    rTo.probeAnchor = rTo.transform.root.Find(path);

                path = null;
                if(rFrom.rootBone != null)
                    path = Functions.GetGameObjectPath(rFrom.rootBone.gameObject);

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
        /// Remove BlendShapes by name.
        /// Copies over all blendshapes to a clone of the mesh with shapeNames excluded. Doesn't work yet
        /// </summary>                
        void RemoveBlendShapes(SkinnedMeshRenderer renderer, params string[] shapeNames)
        {
            /*Mesh myMesh = renderer.sharedMesh;
            Mesh tmpMesh = Instantiate(myMesh);
                        
            tmpMesh.ClearBlendShapes();

            var shapeValues = new Dictionary<string, float>();

            for(int z = 0; z < myMesh.blendShapeCount; z++)
            {
                shapeValues.Add(myMesh.GetBlendShapeName(z), renderer.GetBlendShapeWeight(z));
            }

            ResetBlendShapes(renderer);

            /*List<BlendShapeFrame> shapeList = new List<BlendShapeFrame>();
            shapeList = GetBlendShapeFrames(renderer);

            foreach(var b in shapeList)
            {
                bool found = false;
                for(int z = 0; z < shapeNames.Length; z++)                
                {
                    if(b.name.ToLower() == shapeNames[z].ToLower())
                    {
                        found = true;
                        break;
                    }
                }

                //if(!found)
                    tmpMesh.AddBlendShapeFrame(b.name, b.weight, b.deltaVertices, b.deltaNormals, b.deltaTangents);
            }

            myMesh = null;
            renderer.sharedMesh = tmpMesh;*/

            /*Vector3[] dVertices = new Vector3[myMesh.vertexCount];
            Vector3[] dNormals = new Vector3[myMesh.vertexCount];
            Vector3[] dTangents = new Vector3[myMesh.vertexCount];
            for(int shape = 0; shape < myMesh.blendShapeCount; shape++)
            {
                string shapeName = myMesh.GetBlendShapeName(shape);

                bool found = false;
                foreach(string s in shapeNames)
                {
                    if(s.ToLower() == shapeName.ToLower())
                    {
                        found = true;
                        break;
                    }
                }
                if(found)
                    continue;
                
                for(int frame = 0; frame < myMesh.GetBlendShapeFrameCount(shape); frame++)
                {
                    float frameWeight = myMesh.GetBlendShapeFrameWeight(shape, frame);                    

                    myMesh.GetBlendShapeFrameVertices(shape, frame, dVertices, dNormals, dTangents);
                    tmpMesh.AddBlendShapeFrame(shapeName, frameWeight, dVertices, dNormals, dTangents);
                }
            }*/

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
            
            //SaveMesh(tmpMesh, path + tmpMesh.name + ".asset", true, true, true);

            //var m = AssetDatabase.LoadAssetAtPath(path + tmpMesh.name, typeof(Mesh)) as Mesh;

            //renderer.sharedMesh = m;

            //string path = Functions.GetGameObjectPath(renderer.transform.root.gameObject, false);
            //path += "/(No Blink)";                        

            //string path = Functions.GetGameObjectPath(renderer.transform.root.gameObject, false);
            //path += " (No Blink)";

            //AssetDatabase.CreateAsset(tmpMesh, );
            //AssetDatabase.SaveAssets();

            //Restore blendshape values
            /*foreach(var kv in shapeValues)
            {
                int index = tmpMesh.GetBlendShapeIndex(kv.Key);

                if(index != -1)
                    renderer.SetBlendShapeWeight(index, kv.Value);
            }*/
        }

        void CopyRigidBodies(GameObject from, GameObject to)
        {
            var rFromList = new List<Rigidbody>();
            var rToList = new List<Rigidbody>();

            rFromList.AddRange(from.GetComponents<Rigidbody>());
            rToList.AddRange(to.GetComponents<Rigidbody>());
        }

        void CopyParticleSystems(GameObject from, GameObject to, bool createGameObjects)
        {
            var pFromArr = from.GetComponentsInChildren<ParticleSystem>(true);            

            for(int i = 0; i < pFromArr.Length; i++)
            {
                var pp = pFromArr[i];
                var pFromPath = Functions.GetGameObjectPath(pp.gameObject);

                if(pFromPath != null)
                {
                    var pToObj = to.transform.root.Find(pFromPath, createGameObjects, pp.transform);                    

                    if(pToObj != null)
                    {                        
                        var pTo = pToObj.GetComponent<ParticleSystem>();
                        if(bCopier_particleSystems_replace || pTo == null)
                        {
                            //I know there has to be a better way. Probably
                            #region Particle System 'Instantiation'
                            var p = pToObj.gameObject.AddComponent<ParticleSystem>();                            

                            var pRend = p.GetComponent<ParticleSystemRenderer>();
                            var ppRend = pp.GetComponent<ParticleSystemRenderer>();

                            pRend.alignment = ppRend.alignment;
                            pRend.allowOcclusionWhenDynamic = ppRend.allowOcclusionWhenDynamic;
                            pRend.cameraVelocityScale = ppRend.cameraVelocityScale;
                            pRend.enabled = ppRend.enabled;
                            pRend.lengthScale = ppRend.lengthScale;
                            pRend.lightmapIndex = ppRend.lightmapIndex;
                            pRend.lightmapScaleOffset = ppRend.lightmapScaleOffset;
                            pRend.lightProbeProxyVolumeOverride = ppRend.lightProbeProxyVolumeOverride;
                            pRend.lightProbeUsage = ppRend.lightProbeUsage;
                            pRend.maskInteraction = ppRend.maskInteraction;
                            pRend.maxParticleSize = ppRend.maxParticleSize;
                            pRend.minParticleSize = ppRend.minParticleSize;
                            pRend.motionVectorGenerationMode = ppRend.motionVectorGenerationMode;
                            pRend.normalDirection = ppRend.normalDirection;
                            pRend.pivot = ppRend.pivot;
                            pRend.probeAnchor = ppRend.probeAnchor;
                            pRend.realtimeLightmapIndex = ppRend.realtimeLightmapIndex;
                            pRend.realtimeLightmapScaleOffset = ppRend.realtimeLightmapScaleOffset;
                            pRend.receiveShadows = ppRend.receiveShadows;
                            pRend.reflectionProbeUsage = ppRend.reflectionProbeUsage;
                            pRend.renderMode = ppRend.renderMode;
                            pRend.shadowCastingMode = ppRend.shadowCastingMode;
                            pRend.sharedMaterials = ppRend.sharedMaterials;
                            pRend.sharedMaterial = ppRend.sharedMaterial;
                            pRend.sortingFudge = ppRend.sortingFudge;
                            pRend.sortingLayerID = ppRend.sortingLayerID;
                            pRend.sortingLayerName = ppRend.sortingLayerName;
                            pRend.sortingOrder = ppRend.sortingOrder;
                            pRend.sortMode = ppRend.sortMode;
                            pRend.trailMaterial = ppRend.trailMaterial;
                            pRend.velocityScale = ppRend.velocityScale;
                            pRend.mesh = ppRend.mesh;

                            var meshes = new Mesh[ppRend.meshCount];
                            ppRend.GetMeshes(meshes);                                
                            pRend.SetMeshes(meshes);

                            var pCol = p.collision;
                            pCol.bounce = pp.collision.bounce;
                            pCol.bounceMultiplier = pp.collision.bounceMultiplier;
                            pCol.colliderForce = pp.collision.colliderForce;
                            pCol.collidesWith = pp.collision.collidesWith;
                            pCol.dampen = pp.collision.dampen;
                            pCol.dampenMultiplier = pp.collision.dampenMultiplier;
                                                        
                            var pCbs = p.colorBySpeed;                            
                            pCbs.color = pp.colorBySpeed.color;
                            pCbs.enabled = pp.colorBySpeed.enabled;
                            pCbs.range = pp.colorBySpeed.range;

                            var pCl = p.colorOverLifetime;
                            pCl.color = pp.colorOverLifetime.color;
                            pCl.enabled = pp.colorOverLifetime.enabled;

                            var pE = p.emission;
                            pE.burstCount = pp.emission.burstCount;
                            pE.enabled = pp.emission.enabled;
                            pE.rateOverDistance = pp.emission.rateOverDistance;
                            pE.rateOverDistanceMultiplier = pp.emission.rateOverDistanceMultiplier;
                            pE.rateOverTime = pp.emission.rateOverTime;
                            pE.rateOverTimeMultiplier = pp.emission.rateOverTimeMultiplier;

                            ParticleSystem.Burst[] bursts = new ParticleSystem.Burst[pp.emission.burstCount];
                            pp.emission.GetBursts(bursts);
                            pE.SetBursts(bursts);

                            var pEf = p.externalForces;
                            pEf.enabled = pp.externalForces.enabled;
                            pEf.multiplier = pp.externalForces.multiplier;

                            var pFol = pp.forceOverLifetime;
                            pFol.enabled = pp.forceOverLifetime.enabled;
                            pFol.randomized = pp.forceOverLifetime.randomized;
                            pFol.space = pp.forceOverLifetime.space;
                            pFol.x = pp.forceOverLifetime.x;
                            pFol.xMultiplier = pp.forceOverLifetime.xMultiplier;
                            pFol.y = pp.forceOverLifetime.yMultiplier;
                            pFol.yMultiplier = pp.forceOverLifetime.yMultiplier;
                            pFol.z = pp.forceOverLifetime.z;
                            pFol.zMultiplier = pp.forceOverLifetime.zMultiplier;

                            var pIv = p.inheritVelocity;
                            pIv.curve = pp.inheritVelocity.curve;
                            pIv.curveMultiplier = pp.inheritVelocity.curveMultiplier;

                            var pL = p.lights;
                            pL.alphaAffectsIntensity = pp.lights.alphaAffectsIntensity;
                            pL.enabled = pp.lights.enabled;
                            pL.intensity = pp.lights.intensity;
                            pL.intensityMultiplier = pp.lights.intensityMultiplier;
                            pL.light = pp.lights.light;
                            pL.maxLights = pp.lights.maxLights;
                            pL.range = pp.lights.range;
                            pL.rangeMultiplier = pp.lights.rangeMultiplier;
                            pL.ratio = pp.lights.ratio;
                            pL.sizeAffectsRange = pp.lights.sizeAffectsRange;
                            pL.useParticleColor = pp.lights.useParticleColor;
                            pL.useRandomDistribution = pp.lights.useRandomDistribution;

                            var pLvol = p.limitVelocityOverLifetime;
                            pLvol.dampen = pp.limitVelocityOverLifetime.dampen;
                            pLvol.drag = pp.limitVelocityOverLifetime.drag;
                            pLvol.dragMultiplier = pp.limitVelocityOverLifetime.dragMultiplier;
                            pLvol.enabled = pp.limitVelocityOverLifetime.enabled;
                            pLvol.limit = pp.limitVelocityOverLifetime.limit;
                            pLvol.limitMultiplier = pp.limitVelocityOverLifetime.limitMultiplier;
                            pLvol.limitX = pp.limitVelocityOverLifetime.limitX;
                            pLvol.limitXMultiplier = pp.limitVelocityOverLifetime.limitXMultiplier;
                            pLvol.limitY = pp.limitVelocityOverLifetime.limitY;
                            pLvol.limitYMultiplier = pp.limitVelocityOverLifetime.limitYMultiplier;
                            pLvol.limitZ = pp.limitVelocityOverLifetime.limitZ;
                            pLvol.limitZMultiplier = pp.limitVelocityOverLifetime.limitZMultiplier;
                            pLvol.multiplyDragByParticleSize = pp.limitVelocityOverLifetime.multiplyDragByParticleSize;
                            pLvol.multiplyDragByParticleVelocity = pp.limitVelocityOverLifetime.multiplyDragByParticleVelocity;
                            pLvol.separateAxes = pp.limitVelocityOverLifetime.separateAxes;
                            pLvol.space = pp.limitVelocityOverLifetime.space;
                            
                            var pM = p.main;
                            pM.customSimulationSpace = pp.main.customSimulationSpace;
                            pM.duration = pp.main.duration;
                            pM.emitterVelocityMode = pp.main.emitterVelocityMode;
                            pM.gravityModifier = pp.main.gravityModifier;
                            pM.gravityModifierMultiplier = pp.main.gravityModifierMultiplier;
                            pM.loop = pp.main.loop;
                            pM.maxParticles = pp.main.maxParticles;
                            pM.playOnAwake = pp.main.playOnAwake;
                            pM.prewarm = pp.main.prewarm;
                            pM.randomizeRotationDirection = pp.main.randomizeRotationDirection;
                            pM.scalingMode = pp.main.scalingMode;
                            pM.simulationSpace = pp.main.simulationSpace;
                            pM.simulationSpeed = pp.main.simulationSpeed;
                            pM.startColor = pp.main.startColor;
                            pM.startDelay = pp.main.startDelay;
                            pM.startDelayMultiplier = pp.main.startDelayMultiplier;
                            pM.startLifetime = pp.main.startLifetime;
                            pM.startLifetimeMultiplier = pp.main.startLifetimeMultiplier;
                            pM.startRotation = pp.main.startRotation;
                            pM.startRotation3D = pp.main.startRotation3D;
                            pM.startRotationMultiplier = pp.main.startRotationMultiplier;
                            pM.startRotationX = pp.main.startRotationX;
                            pM.startRotationXMultiplier = pp.main.startRotationXMultiplier;
                            pM.startRotationY = pp.main.startRotationY;
                            pM.startRotationYMultiplier = pp.main.startRotationYMultiplier;
                            pM.startRotationZ = pp.main.startRotationZ;
                            pM.startSize = pp.main.startSize;
                            pM.startSize3D = pp.main.startSize3D;
                            pM.startSizeMultiplier = pp.main.startSizeMultiplier;
                            pM.startSizeX = pp.main.startSizeX;
                            pM.startSizeXMultiplier = pp.main.startSizeXMultiplier;
                            pM.startSizeY = pp.main.startSizeY;
                            pM.startSizeYMultiplier = pp.main.startSizeYMultiplier;
                            pM.startSizeZ = pp.main.startSizeZ;
                            pM.startSizeZMultiplier = pp.main.startSizeZMultiplier;
                            pM.startSpeed = pp.main.startSpeed;
                            pM.startSpeedMultiplier = pp.main.startSpeedMultiplier;
                            pM.stopAction = pp.main.stopAction;
                            pM.useUnscaledTime = pp.main.useUnscaledTime;
                            
                            var pN = p.noise;
                            pN.damping = pp.noise.damping;
                            pN.enabled = pp.noise.enabled;
                            pN.frequency = pp.noise.frequency;
                            pN.octaveCount = pp.noise.octaveCount;
                            pN.octaveMultiplier = pp.noise.octaveMultiplier;
                            pN.octaveScale = pp.noise.octaveScale;
                            pN.positionAmount = pp.noise.positionAmount;
                            pN.quality = pp.noise.quality;
                            pN.remap = pp.noise.remap;
                            pN.remapEnabled = pp.noise.remapEnabled;
                            pN.remapMultiplier = pp.noise.remapMultiplier;                            
                            pN.remapX = pp.noise.remapX;
                            pN.remapXMultiplier = pp.noise.remapXMultiplier;
                            pN.remapY = pp.noise.remapY;
                            pN.remapYMultiplier = pp.noise.remapYMultiplier;
                            pN.remapZ = pp.noise.remapZ;
                            pN.remapZMultiplier = pp.noise.remapZMultiplier;
                            pN.rotationAmount = pp.noise.rotationAmount;
                            pN.scrollSpeed = pp.noise.scrollSpeed;
                            pN.scrollSpeedMultiplier = pp.noise.scrollSpeedMultiplier;
                            pN.separateAxes = pp.noise.separateAxes;
                            pN.sizeAmount = pp.noise.sizeAmount;
                            pN.strength = pp.noise.strength;
                            pN.strengthMultiplier = pp.noise.strengthMultiplier;
                            pN.strengthX = pp.noise.strengthX;
                            pN.strengthXMultiplier = pp.noise.strengthXMultiplier;
                            pN.strengthY = pp.noise.strengthYMultiplier;
                            pN.strengthYMultiplier = pp.noise.strengthYMultiplier;
                            pN.strengthZ = pp.noise.strengthZMultiplier;
                            pN.strengthZMultiplier = pp.noise.strengthZMultiplier;
                                                        
                            var pRbs = p.rotationBySpeed;
                            pRbs.enabled = pp.rotationBySpeed.enabled;
                            pRbs.range = pp.rotationBySpeed.range;
                            pRbs.separateAxes = pp.rotationBySpeed.separateAxes;
                            pRbs.x = pp.rotationBySpeed.x;
                            pRbs.xMultiplier = pp.rotationBySpeed.xMultiplier;
                            pRbs.y = pp.rotationBySpeed.y;
                            pRbs.yMultiplier = pp.rotationBySpeed.yMultiplier;
                            pRbs.z = pp.rotationBySpeed.z;
                            pRbs.zMultiplier = pp.rotationBySpeed.zMultiplier;
                            
                            var pRol = p.rotationOverLifetime;
                            pRol.enabled = pp.rotationBySpeed.enabled;
                            pRol.separateAxes = pp.rotationBySpeed.separateAxes;
                            pRol.x = pp.rotationBySpeed.x;
                            pRol.xMultiplier = pp.rotationBySpeed.xMultiplier;
                            pRol.y = pp.rotationBySpeed.y;
                            pRol.yMultiplier = pp.rotationBySpeed.yMultiplier;
                            pRol.z = pp.rotationBySpeed.z;
                            pRol.zMultiplier = pp.rotationBySpeed.zMultiplier;
                            
                            var pS = p.shape;
                            pS.alignToDirection = pp.shape.alignToDirection;
                            pS.angle = pp.shape.angle;
                            pS.arc = pp.shape.arc;
                            pS.arcMode = pp.shape.arcMode;
                            pS.arcSpeed = pp.shape.arcSpeed;
                            pS.arcSpeedMultiplier = pp.shape.arcSpeedMultiplier;
                            pS.arcSpread = pp.shape.arcSpread;
                            pS.boxThickness = pp.shape.boxThickness;
                            pS.donutRadius = pp.shape.donutRadius;
                            pS.enabled = pp.shape.enabled;
                            pS.length = pp.shape.length;
                            pS.mesh = pp.shape.mesh;
                            pS.meshMaterialIndex = pp.shape.meshMaterialIndex;
                            pS.meshRenderer = pp.shape.meshRenderer;                            
                            pS.meshShapeType = pp.shape.meshShapeType;
                            pS.normalOffset = pp.shape.normalOffset;
                            pS.position = pp.shape.position;
                            pS.radius = pp.shape.radius;
                            pS.radiusMode = pp.shape.radiusMode;
                            pS.radiusSpeed = pp.shape.radiusSpeed;
                            pS.radiusSpeedMultiplier = pp.shape.radiusSpeedMultiplier;
                            pS.radiusSpread = pp.shape.radiusSpread;
                            pS.radiusThickness = pp.shape.radiusThickness;                            
                            pS.randomDirectionAmount = pp.shape.randomDirectionAmount;
                            pS.randomPositionAmount = pp.shape.randomPositionAmount;
                            pS.rotation = pp.shape.rotation;
                            pS.scale = pp.shape.scale;
                            pS.shapeType = pp.shape.shapeType;
                            pS.skinnedMeshRenderer = pp.shape.skinnedMeshRenderer;
                            pS.sphericalDirectionAmount = pp.shape.sphericalDirectionAmount;
                            pS.useMeshColors = pp.shape.useMeshColors;
                            pS.useMeshMaterialIndex = pp.shape.useMeshMaterialIndex;
                            
                            var pSbs = p.sizeBySpeed;
                            pSbs.enabled = pp.sizeBySpeed.enabled;
                            pSbs.range = pp.sizeBySpeed.range;
                            pSbs.separateAxes = pp.sizeBySpeed.separateAxes;
                            pSbs.size = pp.sizeBySpeed.size;
                            pSbs.sizeMultiplier = pp.sizeBySpeed.sizeMultiplier;
                            pSbs.x = pp.sizeBySpeed.x;
                            pSbs.xMultiplier = pp.sizeBySpeed.xMultiplier;
                            pSbs.y = pp.sizeBySpeed.y;
                            pSbs.yMultiplier = pp.sizeBySpeed.yMultiplier;
                            pSbs.z = pp.sizeBySpeed.z;
                            pSbs.zMultiplier = pp.sizeBySpeed.zMultiplier;

                            var pSol = p.sizeOverLifetime;
                            pSol.enabled = pp.sizeOverLifetime.enabled;                            
                            pSol.separateAxes = pp.sizeOverLifetime.separateAxes;
                            pSol.size = pp.sizeOverLifetime.size;
                            pSol.sizeMultiplier = pp.sizeOverLifetime.sizeMultiplier;
                            pSol.x = pp.sizeOverLifetime.x;
                            pSol.xMultiplier = pp.sizeOverLifetime.xMultiplier;
                            pSol.y = pp.sizeOverLifetime.y;
                            pSol.yMultiplier = pp.sizeOverLifetime.yMultiplier;
                            pSol.z = pp.sizeOverLifetime.z;
                            pSol.zMultiplier = pp.sizeOverLifetime.zMultiplier;

                            var pSe = p.subEmitters;
                            pSe.enabled = pp.subEmitters.enabled;                            
                            for(int z = 0; z < pp.subEmitters.subEmittersCount; z++)
                            {
                                var sys = pp.subEmitters.GetSubEmitterSystem(z);
                                var prop = pp.subEmitters.GetSubEmitterProperties(z);
                                var typ = pp.subEmitters.GetSubEmitterType(z);

                                pSe.AddSubEmitter(sys, typ, prop);
                            }                            

                            var pTsa = p.textureSheetAnimation;
                            for(int z = 0; z < pTsa.spriteCount; z++)
                            {
                                var spr = pp.textureSheetAnimation.GetSprite(z);
                                pTsa.SetSprite(z, spr);
                            }
                            pTsa.animation = pp.textureSheetAnimation.animation;
                            pTsa.cycleCount = pp.textureSheetAnimation.cycleCount;
                            pTsa.enabled = pp.textureSheetAnimation.enabled;
                            pTsa.flipU = pp.textureSheetAnimation.flipU;
                            pTsa.flipV = pp.textureSheetAnimation.flipV;
                            pTsa.frameOverTime = pp.textureSheetAnimation.frameOverTime;
                            pTsa.frameOverTimeMultiplier = pp.textureSheetAnimation.frameOverTimeMultiplier;
                            pTsa.mode = pp.textureSheetAnimation.mode;
                            pTsa.numTilesX = pp.textureSheetAnimation.numTilesX;
                            pTsa.numTilesY = pp.textureSheetAnimation.numTilesY;
                            pTsa.rowIndex = pp.textureSheetAnimation.rowIndex;
                            pTsa.startFrame = pp.textureSheetAnimation.startFrame;
                            pTsa.startFrameMultiplier = pp.textureSheetAnimation.startFrameMultiplier;
                            pTsa.useRandomRow = pp.textureSheetAnimation.useRandomRow;
                            pTsa.uvChannelMask = pp.textureSheetAnimation.uvChannelMask;                            

                            var pT = p.trails;
                            pT.colorOverLifetime = pp.trails.colorOverLifetime;
                            pT.colorOverTrail = pp.trails.colorOverTrail;
                            pT.dieWithParticles = pp.trails.dieWithParticles;
                            pT.enabled = pp.trails.enabled;
                            pT.generateLightingData = pp.trails.generateLightingData;
                            pT.inheritParticleColor = pp.trails.inheritParticleColor;
                            pT.lifetime = pp.trails.lifetime;
                            pT.lifetimeMultiplier = pp.trails.lifetimeMultiplier;
                            pT.minVertexDistance = pp.trails.minVertexDistance;
                            pT.mode = pp.trails.mode;
                            pT.ratio = pp.trails.ratio;
                            pT.ribbonCount = pp.trails.ribbonCount;
                            pT.sizeAffectsLifetime = pp.trails.sizeAffectsLifetime;
                            pT.sizeAffectsWidth = pp.trails.sizeAffectsWidth;
                            pT.textureMode = pp.trails.textureMode;
                            pT.widthOverTrail = pp.trails.widthOverTrail;
                            pT.widthOverTrailMultiplier = pp.trails.widthOverTrailMultiplier;
                            pT.worldSpace = pp.trails.worldSpace;                            

                            var pTr = p.trigger;
                            for(int z = 0; z <  pp.trigger.maxColliderCount; z++)
                            {
                                var c = pp.trigger.GetCollider(z);
                                if(c != null)
                                    pTr.SetCollider(z, c);
                            }
                            pTr.enabled = pp.trigger.enabled;
                            pTr.enter = pp.trigger.enter;
                            pTr.exit = pp.trigger.exit;
                            pTr.inside = pp.trigger.inside;                            
                            pTr.outside = pp.trigger.outside;
                            pTr.radiusScale = pp.trigger.radiusScale;                                

                            var pVol = p.velocityOverLifetime;
                            pVol.enabled = pp.velocityOverLifetime.enabled;
                            pVol.space = pp.velocityOverLifetime.space;
                            pVol.speedModifier = pp.velocityOverLifetime.speedModifier;
                            pVol.speedModifierMultiplier = pp.velocityOverLifetime.speedModifierMultiplier;
                            pVol.x = pp.velocityOverLifetime.x;
                            pVol.xMultiplier = pp.velocityOverLifetime.xMultiplier;
                            pVol.y = pp.velocityOverLifetime.y;
                            pVol.yMultiplier = pp.velocityOverLifetime.yMultiplier;
                            pVol.z = pp.velocityOverLifetime.z;
                            pVol.zMultiplier = pp.velocityOverLifetime.zMultiplier;                            

                            p.randomSeed = p.randomSeed;
                            p.useAutoRandomSeed = pp.useAutoRandomSeed;    
                            p.time = pp.time;
                            pToObj.tag = p.tag;

                            #endregion

                            Log("_Success: Copied over {0} from {1}'s {2} to {3}'s {4}", LogType.Log, "ParticleSystem", copierSelectedFrom.name, pp.gameObject.name, selectedAvatar.name, pToObj.gameObject.name);
                        }
                        else
                        {
                            Log("_Failed: {0}'s {1} already has a ParticleSystem. Ignoring.", LogType.Log, selectedAvatar.name, pp.gameObject.name);
                        }
                    }
                }
            }
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
        /// Doesn't work yet
        /// </summary>        
        void FixRandomMouthOpening(GameObject avatar)
        {
            throw new Exception("Don't. It doesn't work and will mess up your mesh.");
            /*var renders = avatar.GetComponentsInChildren<SkinnedMeshRenderer>(true);

            foreach(var r in renders)
            {
                int blinkLeft = r.sharedMesh.GetBlendShapeIndex("vrc.blink_left");
                int blinkRight = r.sharedMesh.GetBlendShapeIndex("vrc.blink_right");

                if(blinkLeft != -1 || blinkRight != -1)
                {
                    Debug.Log("_Found blinking Visemes on " + r.gameObject.name);
                    int lidLeft = r.sharedMesh.GetBlendShapeIndex("vrc.lowerlid_left");
                    int lidRight = r.sharedMesh.GetBlendShapeIndex("vrc.lowerlid_right");

                    Mesh m = r.sharedMesh;

                    if(lidLeft == -1 || lidRight == -1)
                    {
                        Debug.Log(string.Format("_Found missing lowerlid visemes on {0}. Adding", r.gameObject.name));
                        if(lidLeft == -1)
                        {
                            m.AddBlendShapeFrame("vrc.lowerlid_left", 0, new Vector3[m.vertexCount], new Vector3[m.vertexCount], new Vector3[m.vertexCount]);
                        }
                        if(lidRight == -1)
                        {
                            m.AddBlendShapeFrame("vrc.lowerlid_right", 0, new Vector3[m.vertexCount], new Vector3[m.vertexCount], new Vector3[m.vertexCount]);
                        }
                    }
                    else
                    {
                        Debug.Log(string.Format("_{0} has lowerlid visemes. Nothing to fix", r.gameObject.name));                        
                    }
                }
                else
                {
                    Debug.Log(string.Format("_{0} doesn't have blinking visemes. Nothing to fix", r.gameObject.name));
                }
            }*/
        }

        public static void ApplyTPose(GameObject objTo)
        {
            if(objTo == null)
                return;

            Animator oldAnim = objTo.GetComponent<Animator>();
            Avatar av = objTo.GetComponent<Avatar>();
            if(av != null && av.isHuman)
            {
                if(_tposeAvatar == null)
                {
                    
                }
            }
            else
            {
                Log("_Rig avatar missing or not humanoid", LogType.Error);
                return;
            }
        }
                
        /// <summary>
        /// Reset transforms to prefab
        /// </summary>        
        public static bool ResetPose(GameObject objTo)
        {
            if(objTo == null)
                return false;

            string toPath = Functions.GetGameObjectPath(objTo);
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

        void SetAnimation(int animationId)
        {

        }

        bool GameObjectIsEmpty(GameObject obj)
        {            
            if(obj.GetComponentsInChildren<Component>().Length > obj.GetComponentsInChildren<Transform>().Length)            
                return false;            
            return true;
        }

        GameObject GetSameChild(GameObject parent, GameObject child)
        {
            if(parent == null || child == null)
                return null;

            Transform newChild = parent.transform.Find(child.name);

            if(newChild != null)
                return newChild.gameObject;
            else
                return null;
        }

        #endregion

        #region Destroy Functions    

        /// <summary>
        /// Destroys ParticleSystem components in object and all children. 
        /// If the GameObject has no other components it will be destroyed as well.
        /// </summary>        
        private void DestroyParticleSystems(GameObject from)
        {
            var sys = from.GetComponentsInChildren<ParticleSystem>();

            foreach(var p in sys)
            {
                var rend = p.GetComponent<ParticleSystemRenderer>();

                if(rend != null)
                    DestroyImmediate(rend);

                Log(Strings.Log.RemoveAttempt + " " + "_Success.", LogType.Log, p.ToString(), from.name);
                DestroyImmediate(p);

                //Dangerous if we put particles on bones. Almost nobody does but removed for now. Just in case.
                //if(p.GetComponents<Component>().Length > 2)
                //{
                //    Log(Strings.Log.RemoveAttempt + " " + "_Success.", LogType.Log, p.ToString(), from.name);
                //    DestroyImmediate(p);
                //}                                
                //else
                //{
                //    Log(Strings.Log.RemoveAttempt + "_Success. Removed ParticleSystem and destroyed empty GameObject.", LogType.Log, p.ToString(), from.name);
                //    DestroyImmediate(p.gameObject);
                //}                
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

        /// <summary>
        /// Destroys empty GameObjects that are bone transforms with no weights
        /// </summary>
        /// <param name="from"></param>
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
        /// </summary>        
        void DestroyAllComponentsOfType(GameObject obj, Type type)
        {
            string log = "";
            string[] logFormat = { type.ToString(), obj.name };
            if(!IsSupportedComponentType(type))            
            {
                log += Strings.Log.TryRemoveUnsupportedComponent;
                Log(log, LogType.Assert, logFormat);                
                return;
            }

            Component[] comps = obj.transform.GetComponentsInChildren(type, true);

            if(comps != null && comps.Length > 0)
            {
                for(int i = 0; i < comps.Length; i++)
                {
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

        public static bool TransformInDefaultPosition(Transform t, bool onlyRotation)
        {
            if(t == null)
                return false;

            string tPath = Functions.GetGameObjectPath(t.gameObject);
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

        public static void GuiLine(float height = 1f)
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
        }             
    }

    /// <summary>
    /// Strings. Need to move these to files eventually
    /// </summary>
    public static class Strings
    {        
        public static readonly string version = "0.6b";
        public static readonly string toolsPage = "https://github.com/rurre/PumkinsAvatarTools/";
        public static readonly string donationLink ="https://ko-fi.com/notpumkin";
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
                Copy = GetString("buttons_copy") ?? "_Copy";
                OpenHelpPage = GetString("buttons_openHelpPage") ?? "_Open Help Page";
                OpenGithubPage = GetString("buttons_openGithubPage") ?? "_Open Github Page";
                OpenDonationPage = GetString("buttons_openDonationPage") ?? "_Buy me a Ko-Fi~";
                OpenPoseEditor = GetString("buttons_openPoseEditor") ?? "_Open Pose Editor";
            }
        };
        public static class Tools
        {
            public static string FillVisemes { get; private set; }
            public static string EditViewpoint { get; private set; }
            public static string RevertBlendshapes { get; private set; }
            public static string ZeroBlendshapes { get; private set; }
            public static string ResetPose { get; private set; }
            public static string resetToTPose { get; private set; }
            
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
                resetToTPose = GetString("ui_tools_resetToTPose") ?? "_Reset to T-Pose";
            }
        };
        public static class Thumbnails
        {
            public static string OverrideCameraImage { get; private set; }
            public static string OverrideTexture { get; private set; }
            public static string StartUploadingFirst { get; private set; }

            static Thumbnails()
            {
                Reload();
            }

            public static void Reload()
            {
                OverrideCameraImage = GetString("ui_thumbnails_overrideCameraImage") ?? "_Override Camera Image";
                OverrideTexture = GetString("ui_thumbnails_overrideTexture") ?? "_Override Texture";
                StartUploadingFirst = GetString("ui_thumbnails_startUploadingFirst") ?? "_Start uploading an Avatar first";
            }
        }
        public static class Copier
        {
            public static string CopyFrom { get; private set; }
            public static string GameObjects { get; private set; }
            public static string GameObjects_createMissing { get; private set; }
            public static string Transforms { get; private set; }
            public static string Transforms_position { get; private set; }
            public static string Transforms_rotation { get; private set; }
            public static string Transforms_scale { get; private set; }
            public static string Transforms_avatarScale { get; private set; }
            public static string DynamicBones { get; private set; }
            public static string DynamicBones_settings { get; private set; }
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
            public static string Descriptor_settings { get; private set; }
            public static string Descriptor_pipelineId { get; private set; }
            public static string Descriptor_animationOverrides { get; private set; }
            public static string SkinMeshRender { get; private set; }
            public static string SkinMeshRender_settings { get; private set; }
            public static string SkinMeshRender_materials { get; private set; }
            public static string SkinMeshRender_blendShapeValues { get; private set; }
            public static string ParticleSystems { get; private set; }

            static Copier()
            {
                Reload();
            }

            public static void Reload()
            {
                CopyFrom = GetString("ui_copier_copyFrom") ?? "_Copy From";
                GameObjects = GetString("ui_copier_gameObjects") ?? "_Game Objects";
                GameObjects_createMissing = GetString("ui_copier_gameObjects_createMissing") ?? "_Create Missing";
                Transforms = GetString("ui_copier_transforms") ?? "_Transforms";
                Transforms_position = GetString("ui_copier_transforms_position") ?? "_Position";
                Transforms_rotation = GetString("ui_copier_transforms_rotation") ?? "_Rotation";
                Transforms_scale = GetString("ui_copier_transforms_scale") ?? "_Scale";
                Transforms_avatarScale = GetString("ui_copier_transforms_avatarScale") ?? "_Avatar Scale";
                DynamicBones = GetString("ui_copier_dynamicBones") ?? "_Dynamic Bones";
                DynamicBones_settings = GetString("ui_copier_dynamicBones_settings") ?? "_Settings";
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
                Descriptor_settings = GetString("ui_copier_descriptor_settings") ?? "_Settings";
                Descriptor_pipelineId = GetString("ui_copier_descriptor_pipelineId") ?? "_Pipeline Id";
                Descriptor_animationOverrides = GetString("ui_copier_descriptor_animationOverrides") ?? "_Animation Overrides";
                SkinMeshRender = GetString("ui_copier_skinMeshRender") ?? "_Skinned Mesh Renderers";
                SkinMeshRender_settings = GetString("ui_copier_skinMeshRender_settings") ?? "_Settings";
                SkinMeshRender_materials = GetString("ui_copier_skinMeshRender_materials") ?? "_Materials";
                SkinMeshRender_blendShapeValues = GetString("ui_copier_skinMeshRender_blendShapeValues") ?? "_BlendShape Values";
                ParticleSystems = GetString("ui_copier_particleSystem") ?? "_Particle Systems";
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

            static Log()
            {
                Reload();
            }

            public static void Reload()
            {
                Cancelled = GetString("log_cancelled") ?? "_Cancelled";
                CantCopyToSelf = GetString("log_cantCopyToSelf") ?? "_Can't copy Components from an object to itself. What are you doing?";
                CopyAttempt = GetString("log_copyAttempt") ?? "_Attempting to copy {0} from {1} to {2}";
                RemoveAttempt = GetString("log_removeAttempt") ?? "_Attempting to remove {0} from {1}"; 
                CopyFromInvalid = GetString("log_copyFromInvalid") ?? "_Can't copy Components because 'Copy From' is invalid";
                Done = GetString("log_done") ?? "_Done";
                ViewpointApplied = GetString("log_viewpointApplied") ?? "_Set Viewposition to {0}";
                ViewpointCancelled = GetString("log_viewpointCancelled") ?? "_Cancelled Viewposition changes";
                TryFillVisemes = GetString("log_tryFillVisemes") ?? "_Attempting to fill visemes on {0}";
                NoSkinnedMeshFound = GetString("log_noSkinnedMeshFound") ?? "_Failed: No skinned mesh found";
                DescriptorIsNull = GetString("log_descriptorIsNull") ?? "_Avatar descriptor is null";
                Success = GetString("log_success") ?? "_Success";
                MeshHasNoVisemes = GetString("log_meshHasNoVisemes") ?? "_Failed. Mesh has no Visemes. Set to Default";
                TryRemoveUnsupportedComponent = GetString("log_tryRemoveUnsupportedComponent") ?? "_Attempted to remove unsupported component {0} from {1}";
                Failed = GetString("log_failed") ?? "_Failed";
                FailedIsNull = GetString("log_failedIsNull") ?? "_Failed. {1} is null";
                NameIsEmpty = GetString("log_nameIsEmpty") ?? "_Name is Empty";
                LoadedPose = GetString("log_loadedPose") ?? "_Loaded Pose: {0}";
                LoadedBlendshapePreset = GetString("log_loadedBlendshapePreset") ?? "_Loaded Blendshapes: {0}";                
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
            public static string Line1 { get; private set; }
            public static string Line2 { get; private set; }
            public static string Line3 { get; private set; }
            public static string Line4 { get; private set; }
            public static string Line5 { get; private set; }

            static Credits()
            {
                Reload();
            }

            public static void Reload()
            {
                Line1 = GetString("credits_line1") ?? "_Pumkin's Avatar Tools";
                Line2 = GetString("credits_line2") ?? "_Version" + " " + version;
                Line3 = GetString("credits_line3") ?? "_Now with 100% more redundant strings";
                Line4 = GetString("credits_line4") ?? "_I'll add more stuff to this eventually";
                Line5 = GetString("credits_line5") ?? "_Poke me on Discord at Pumkin#2020";
            }
        };        
        public static class Misc
        {
            public static string uwu { get; private set; }
            public static string SearchForBones { get; private set; }

            private static string searchForBones;

            static Misc()
            {
                Reload();
            }

            public static void Reload()
            {
                uwu = GetString("misc_uwu") ?? "_uwu";
                SearchForBones = GetString("misc_searchForBones") ?? "_Search for DynamicBones";
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
                {"buttons_copy", "Copy" },
                {"buttons_openHelpPage", "Open Help Page" },
                {"buttons_openGithubPage", "Open Github Page" },
                {"buttons_openDonationPage", "Buy me a Ko-Fi~" },
                {"buttons_openPoseEditor", "Open Pose Editor" },
            #endregion

            #endregion
            #region Tools
            //UI Tools                
                { "ui_tools_fillVisemes", "Fill Visemes" },
                {"ui_tools_editViewpoint", "Edit Viewpoint" },
                {"ui_tools_revertBlendShapes", "Revert Blendshapes" },
                {"ui_tools_zeroBlendShapes", "Zero Blendshapes" },
                {"ui_tools_resetPose", "Reset Pose" },
                {"ui_tools_resetToTPose", "Reset to T-Pose" },

            #endregion
            #region Copier
                //UI Copier
                { "ui_copier_copyFrom", "Copy from" },

                //UI Copier GameObjects
                {"ui_copier_gameObjects" , "Game Objects" },
                {"ui_copier_gameObjects_createMissing" , "Create Missing" },

                //UI Copier Transforms
                {"ui_copier_transforms", "Transforms" },
                {"ui_copier_transforms_position", "Position" },
                {"ui_copier_transforms_rotation", "Rotation" },
                {"ui_copier_transforms_scale", "Scale" },
                {"ui_copier_transforms_avatarScale", "Avatar Scale" },
            
                //UI Copier Dynamic Bones
                {"ui_copier_dynamicBones", "Dynamic Bones" },
                {"ui_copier_dynamicBones_settings", "Settings" },
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
                {"ui_copier_descriptor_settings", "Settings" },
                {"ui_copier_descriptor_pipelineId", "Pipeline Id" },
                {"ui_copier_descriptor_animationOverrides", "Animation Overrides" },

                //UI Copier Skinned Mesh Renderer
                {"ui_copier_skinMeshRender", "Skinned Mesh Renderers" },
                {"ui_copier_skinMeshRender_settings", "Settings" },
                {"ui_copier_skinMeshRender_materials", "Materials" },
                {"ui_copier_skinMeshRender_blendShapeValues", "BlendShape Values" },

                //UI Copier Particle System
                {"ui_copier_particleSystem", "Particle Systems" },

                //Thumbnails                
                {"ui_thumbnails_overrideCameraImage", "Override Camera Image" },
                {"ui_thumbnails_overrideTexture",  "Override Texture"},
                {"ui_thumbnails_startUploadingFirst", "Start uploading Avatar first" },
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
                {"log_tryRemoveUnsupportedComponent", "Attempted to remove unsupported component {0} from {1}" },
                {"log_failedIsNull" , "Failed. {1} is null. Ignoring" },
                {"log_nameIsEmpty", "Name is empty" },
                {"log_loadedPose", "Loaded Pose: {0}"},
                {"log_loadedBlendshapePreset", "Loaded Blendshapes: {0}"},
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
                { "credits_line1", "Pumkin's Avatar Tools"},
                { "credits_line2", "Version" + " " + version },
                { "credits_line3", "Now with 100% more redundant strings"},
                { "credits_line4", "I'll add more stuff to this eventually" },
                { "credits_line5", "Poke me on Discord at Pumkin#2020" },
            #endregion

                //Misc                
                { "misc_uwu", "uwu" },
                { "misc_searchForBones", "Search for DynamicBones" },
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
                {"buttons_copy", "Cowpy OwO" },
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
                {"ui_copier_particleSystem", "Pawticle Systums zzz" },

                //Thumbnails                
                { "ui_thumbnails_overrideCameraImage", "Ovewwide Camewa Image" },
                { "ui_thumbnails_overrideTexture",  "Ovewwide Textuwe"},
                { "ui_thumbnails_startUploadingFirst", "Stawt upwoading Avataw fiwst!!" },
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
                { "credits_line1", "Pumkin's Avataw Awoos~ :3"},
                { "credits_line2", "Vewsion" + " " + version },
                { "credits_line3", "Nyow with 0W0% mowe noticin things~"},
                { "credits_line4", "I'ww add mowe stuff to this eventuawwy >w<" },
                { "credits_line5", "Poke me! But on Discowd at Pumkin#2020~ uwus" },
            #endregion

                //Misc                
                { "misc_uwu", "OwO" },
                { "misc_searchForBones", "Seawch fow DynyamicBonyes" },
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
}
