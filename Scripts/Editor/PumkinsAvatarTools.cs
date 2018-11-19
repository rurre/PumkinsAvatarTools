#define NO_DBONES
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VRCSDK2;
using VRC.Core;
using UnityEditor.SceneManagement;
using System;
using System.IO;

/// <summary>
/// VRCAvatar tools by Pumkin
/// https://github.com/rurre/VRCAvatarTools
/// </summary>

namespace Pumkin
{
    [ExecuteInEditMode, CanEditMultipleObjects]
    public class PumkinsAvatarTools : EditorWindow
    {
#region Variables
        
        //Tools
        static GameObject selectedAvatar;        

        //Component Copier
        static GameObject copierSelectedFrom;        

        bool bCopier_transforms_copy = true;
        bool bCopier_transforms_copyPosition = true;
        bool bCopier_transforms_copyRotation = true;
        bool bCopier_transforms_copyScale = true;

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

        bool bCopier_skinMeshRender_copy = true;
        bool bCopier_skinMeshRender_copySettings = true;
        bool bCopier_skinMeshRender_copyBlendShapeValues = true;
        //bool bCopier_skinMeshRender_resetBlendShapeValues = true;
        bool bCopier_skinMeshRender_copyMaterials = true;

        //Editor
        bool _copier_expand = false;
        bool _copier_expand_transforms = false;
        bool _copier_expand_dynamicBones = false;
        bool _copier_expand_avatarDescriptor = false;
        bool _copier_expand_skinnedMeshRenderer = false;
        bool _copier_expand_colliders = false;

        bool _tools_expand = true;
        bool _avatarInfo_expand = false;
        bool _misc_expand = true;

        //Misc
        bool _openedInfo = false;
        Vector2 vertScroll = Vector2.zero;

        //Editing Viewpoint
        bool _edittingView = false;        
        Vector3 _viewPos;
        Vector3 _viewPosOld;
        VRC_AvatarDescriptor _viewPos_descriptor;

        static AvatarInfo avatarInfo = null;
        static string _avatarInfoStringTemplate;
        static string _avatarInfoString;        

        enum ToolMenuActions
        {
            RemoveDynamicBones,
            RemoveDynamicBoneColliders,
            RemoveColliders,
            ResetPose,
            ResetBlendShapes,
            FixRandomMouth,
            DisableBlinking,
            EditViewpoint,
            FillVisemes            
        };        

        static readonly Type[] supportedComponents =
        {
#if !NO_DBONES
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
        }

        void OnDisable()
        {
            SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
        }
        
        //Scene GUI
        void OnSceneGUI(SceneView sceneView)
        {            
            if(_edittingView)
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

                Vector3 pos = _viewPos;
                Tools.current = Tool.None;
                Selection.activeGameObject = selectedAvatar.transform.root.gameObject;

                EditorGUI.BeginChangeCheck();
                {
                    pos = Handles.PositionHandle(_viewPos, Quaternion.identity) + selectedAvatar.transform.position;                    
                }
                if(EditorGUI.EndChangeCheck())
                {                    
                    _viewPos = pos;
                    _viewPos_descriptor.ViewPosition = _viewPos + selectedAvatar.transform.position;
                }
            }            
        }

        [MenuItem("Tools/Pumkin/Avatar Tools")]
        public static void ShowWindow()
        {
            //Show existing window instance. If one doesn't exist, make one.
            EditorWindow editorWindow = EditorWindow.GetWindow(typeof(PumkinsAvatarTools));
            editorWindow.autoRepaintOnSceneChange = true;            

            editorWindow.Show();
            editorWindow.titleContent = new GUIContent(Strings.Main.WindowName);

            _DependecyChecker.Check();
        }
        
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
#if !NO_DBONES
                    DestroyAllComponentsOfType(selectedAvatar, typeof(DynamicBoneCollider));
#endif
                    break;
                case ToolMenuActions.RemoveDynamicBones:
#if !NO_DBONES
                    DestroyAllComponentsOfType(selectedAvatar, typeof(DynamicBone));
#endif
                    break;
                case ToolMenuActions.ResetPose:
                    ResetPose(selectedAvatar);
                    break;
                case ToolMenuActions.ResetBlendShapes:
                    ResetBlendShapes(selectedAvatar);
                    break;
                case ToolMenuActions.FixRandomMouth:
                    FixRandomMouthOpening(selectedAvatar);
                    break;
                case ToolMenuActions.DisableBlinking:
                    DisableBlinking(selectedAvatar);
                    break;
                case ToolMenuActions.FillVisemes:
                    FillVisemes(selectedAvatar);
                    break;
                case ToolMenuActions.EditViewpoint:
                    BeginEditViewpoint(selectedAvatar);
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
            _viewPos = _viewPos_descriptor.ViewPosition;
            _viewPosOld = _viewPos_descriptor.ViewPosition;

            if(_viewPos == defaultView)
            {
                var render = selectedAvatar.GetComponentInChildren<SkinnedMeshRenderer>();
                var anim = selectedAvatar.GetComponent<Animator>();

                if(anim != null && anim.isHuman)
                {
                    _viewPos = anim.GetBoneTransform(HumanBodyBones.Head).position + new Vector3(0, 0, defaultView.z);
                    float eyeHeight = anim.GetBoneTransform(HumanBodyBones.LeftEye).position.y;                    
                    _viewPos.y = eyeHeight;

                    _viewPos_descriptor.ViewPosition = _viewPos + selectedAvatar.transform.position;
                }
            }
            _edittingView = true;
        }        
        
        /// <summary>
        /// End editing Viewposition
        /// </summary>        
        /// <param name="cancelled">If cancelled revert viewposition to old value, if not leave it</param>
        private void EndEditingViewpoint(GameObject avatar, bool cancelled)
        {
            if(avatar == null)
            {
                _edittingView = false;                
            }
            else
            {                
                if(_viewPos_descriptor == null)
                {
                    Log(Strings.Log.DescriptorIsNull, LogType.Error);                    
                    return;
                }

                _edittingView = false;
                if(!cancelled)
                {
                    _viewPos_descriptor.ViewPosition = RoundVectorValues(_viewPos_descriptor.gameObject.transform.position + _viewPos, 3);
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

            var render = avatar.GetComponentInChildren<SkinnedMeshRenderer>();

            if(render == null)
            {
                log += Strings.Log.NoSkinnedMeshFound; 
                Log(log, LogType.Error, logFormat);
            }

            d.VisemeSkinnedMesh = render;

            if(render.sharedMesh.blendShapeCount > 0)
            {   
                d.lipSync = VRC_AvatarDescriptor.LipSyncStyle.VisemeBlendShape;
                for(int z = 0; z < visemes.Length; z++)
                {
                    string s = "-none-";
                    if(render.sharedMesh.GetBlendShapeIndex(visemes[z]) != -1)
                        s = visemes[z];
                    d.VisemeBlendShapes[z] = s;
                }
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

        void OnGUI()
        {
            int tempSize = Styles.Label_mainTitle.fontSize + 6;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(Strings.Main.Title, Styles.Label_mainTitle, GUILayout.MinHeight(tempSize));

            EditorGUIUtility.SetIconSize(new Vector2(tempSize-3, tempSize-3));

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
                if(GUILayout.Button(Strings.Buttons.SelectFromScene))
                {
                    if(Selection.activeObject != null)
                    {
                        try
                        {
                            selectedAvatar = Selection.activeGameObject.transform.root.gameObject;
                            avatarInfo = AvatarInfo.GetInfo(selectedAvatar, out _avatarInfoString);
                        }
                        catch
                        {
                                                       
                        }
                    }
                }

                if(selectedAvatar != null && selectedAvatar.gameObject.scene.name == null)
                {
                    Log(Strings.Warning.SelectSceneObject, LogType.Warning);
                    selectedAvatar = null;                    
                }

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

                        GUILayout.BeginVertical(); //Left Column
                        if(GUILayout.Button(Strings.Tools.FillVisemes))
                        {
                            ActionButton(ToolMenuActions.FillVisemes);
                        }
                        if(GUILayout.Button(Strings.Tools.ResetBlendshapes))
                        {
                            ActionButton(ToolMenuActions.ResetBlendShapes);
                        }

                        GUILayout.EndVertical();

                        GUILayout.BeginVertical(); //Right Column                    

                        EditorGUI.BeginDisabledGroup(_edittingView);
                        if(GUILayout.Button(Strings.Tools.EditViewpoint))
                        {
                            ActionButton(ToolMenuActions.EditViewpoint);
                        }
                        EditorGUI.EndDisabledGroup();

                        if(GUILayout.Button(Strings.Tools.ResetPose))
                        {
                            ActionButton(ToolMenuActions.ResetPose);
                        }
                        GUILayout.EndVertical();

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

                        EditorGUILayout.BeginVertical();
#if NO_DBONES
                        EditorGUI.BeginDisabledGroup(true);
#endif
                        if(GUILayout.Button(new GUIContent(Strings.Copier.DynamicBones, Icons.DefaultAsset)))
                        {
                            ActionButton(ToolMenuActions.RemoveDynamicBones);
                        }
#if NO_DBONES
                        EditorGUI.EndDisabledGroup();
#endif
                        if(GUILayout.Button(new GUIContent(Strings.Copier.Colliders, Icons.ColliderBox)))
                        {
                            ActionButton(ToolMenuActions.RemoveColliders);
                        }
                        EditorGUILayout.EndVertical();

#if NO_DBONES
                        EditorGUI.BeginDisabledGroup(true);
#endif
                        EditorGUILayout.BeginVertical();
                        if(GUILayout.Button(new GUIContent(Strings.Copier.DynamicBones_colliders, Icons.DefaultAsset)))
                        {
                            ActionButton(ToolMenuActions.RemoveDynamicBoneColliders);
                        }
#if NO_DBONES
                        EditorGUI.EndDisabledGroup();
#endif
                        EditorGUILayout.EndVertical();

                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUI.EndDisabledGroup();
                    EditorGUILayout.Space();
                }
                
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

                            EditorGUILayout.Space();
                            EditorGUI.EndDisabledGroup();
                        }

                        //DynamicBones menu
#if NO_DBONES
                        EditorGUI.BeginDisabledGroup(true);
#endif
                        EditorGUILayout.BeginHorizontal();
                        _copier_expand_dynamicBones = GUILayout.Toggle(_copier_expand_dynamicBones, Icons.CsScript, "Foldout", GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(true), GUILayout.MaxWidth(30), GUILayout.MaxHeight(10));
#if NO_DBONES
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
#if NO_DBONES
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

                            EditorGUILayout.Space();
                            EditorGUI.EndDisabledGroup();
                        }

                        EditorGUILayout.Space();
                    
                        EditorGUI.BeginDisabledGroup(!(bCopier_dynamicBones_copyColliders || bCopier_dynamicBones_copy || bCopier_colliders_copy || bCopier_descriptor_copy || bCopier_skinMeshRender_copy));
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

                //Avatar Info menu
                if(_avatarInfo_expand = GUILayout.Toggle(_avatarInfo_expand, Strings.Main.AvatarInfo, Styles.Foldout_title))
                {
                    if(selectedAvatar == null)
                    {
                        if(avatarInfo != null)
                        {
                            avatarInfo = null;
                            _avatarInfoString = _avatarInfoStringTemplate;
                        }
                    }
                    else
                    {
                        if(avatarInfo == null)
                        {
                            avatarInfo = AvatarInfo.GetInfo(selectedAvatar, out _avatarInfoString);                            
                        }                        
                    }                    
                    EditorGUILayout.HelpBox(_avatarInfoString, MessageType.None);

                    EditorGUI.BeginDisabledGroup(selectedAvatar == null);
                    {
                        if(GUILayout.Button(Strings.Buttons.Refresh))
                        {
                            avatarInfo = AvatarInfo.GetInfo(selectedAvatar, out _avatarInfoString);
                        }
                    }
                }
                EditorGUI.EndDisabledGroup();

#if NO_DBONES
                if(_misc_expand = GUILayout.Toggle(_misc_expand, Strings.Main.Misc, Styles.Foldout_title))
                {
                    if(GUILayout.Button(Strings.Misc.SearchForBones))
                    {
                        _DependecyChecker.Check();
                    }

                }
#endif
                EditorGUILayout.EndScrollView();
            }
        }

#endregion

#region Main Functions

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
            if(objTo.transform == objTo.transform.root)
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
            }
            //End run once
            
            if(bCopier_transforms_copy)
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
            if(bCopier_colliders_copy)
            {                
                CopyColliders(objFrom, objTo);
            }
            if(bCopier_skinMeshRender_copy)
            {                
                CopySkinMeshRenderer(objFrom, objTo);
            }

            //Copy Components in Children
            for(int i = 0; i < objFrom.transform.childCount; i++)
            {
                var fromChild = objFrom.transform.GetChild(i).gameObject;
                var t = objTo.transform.Find(fromChild.name);

                if(t == null)
                    continue;

                var toChild = t.gameObject;

                if(fromChild != null && toChild != null)
                {
                    CopyComponents(fromChild, toChild);
                }
            }
        }

        /// <summary>
        /// Copies over the AvatarDescriptor and PipelineManager components.
        /// </summary>
        void CopyAvatarDescriptor(GameObject from, GameObject to)
        {
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
                string id = pFrom.blueprintId ?? string.Empty;

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

                string s = GetGameObjectPath(dFrom.VisemeSkinnedMesh.gameObject, true);
                Transform t = dTo.transform.Find(s);
                if(t != null)
                {
                    dTo.VisemeSkinnedMesh = t.GetComponent<SkinnedMeshRenderer>();
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
#if !NO_DBONES
            string[] logFormat = { "DynamicBoneCollider", from.name, to.name };
            string log = Strings.Log.CopyAttempt;
            List<DynamicBoneCollider> dFromList = new List<DynamicBoneCollider>();
            dFromList.AddRange(from.GetComponents<DynamicBoneCollider>());
            if(dFromList.Count == 0)
            {
                var ar = from.GetComponents<DynamicBoneColliderBase>();
                foreach(var obj in ar)
                {
                    dFromList.Add((DynamicBoneCollider)obj);
                }
            }

            List<DynamicBoneCollider> dToList = new List<DynamicBoneCollider>();
            dToList.AddRange(to.GetComponents<DynamicBoneCollider>());

            if(dToList.Count == 0)
            {
                var ar = to.GetComponents<DynamicBoneColliderBase>();
                foreach(var obj in ar)
                {
                    dToList.Add((DynamicBoneCollider)obj);
                }
            }

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
#if !NO_DBONES
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
                var newCollList = new List<DynamicBoneColliderBase>();

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
                        dTo.m_Colliders = new List<DynamicBoneColliderBase>();
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
                                    DynamicBoneColliderBase tempC = c;
                                    foreach(var cc in dTo.m_Colliders)
                                    {
                                        if(c == cc)
                                        {
                                            tempC = null;
                                            break;
                                        }
                                    }
                                    if(tempC != null)
                                        newCollList.Add(tempC);
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
                dTo.m_UpdateMode = dFrom.m_UpdateMode;
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
        /// Copies Box, Capsule, Sphere and Mesh colliders from one object to another
        /// </summary>        
        void CopyColliders(GameObject from, GameObject to)
        {
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
                log += "_Success - Added {0} to {2}";
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

            string log = Strings.Log.CopyAttempt;
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
            if(!(bCopier_skinMeshRender_copyBlendShapeValues || bCopier_skinMeshRender_copyMaterials || bCopier_skinMeshRender_copySettings))// || bCopier_skinMeshRender_resetBlendShapeValues))
                return;

            string log = Strings.Log.CopyAttempt;
            string[] logFormat = { "SkinnedMeshRenderer", from.name, to.name };

            SkinnedMeshRenderer rFrom = from.GetComponent<SkinnedMeshRenderer>();
            SkinnedMeshRenderer rTo = to.GetComponent<SkinnedMeshRenderer>();

            if(rFrom == null)
            {
                log += "Failed: {1} is null. Ignoring";
                Log(log, LogType.Warning, logFormat);
                return;
            }

            if(bCopier_skinMeshRender_copySettings)
            {
                rTo.enabled = rFrom.enabled;
                rTo.quality = rFrom.quality;
                rTo.updateWhenOffscreen = rFrom.updateWhenOffscreen;
                rTo.skinnedMotionVectors = rFrom.skinnedMotionVectors;                
                rTo.rootBone = rFrom.rootBone;
                rTo.lightProbeUsage = rFrom.lightProbeUsage;
                rTo.reflectionProbeUsage = rFrom.reflectionProbeUsage;
                rTo.probeAnchor = rFrom.probeAnchor;
                rTo.shadowCastingMode = rFrom.shadowCastingMode;
                rTo.receiveShadows = rFrom.receiveShadows;                
                rTo.motionVectorGenerationMode = rFrom.motionVectorGenerationMode;                
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

            //string path = GetGameObjectPath(renderer.transform.root.gameObject, false);
            //path += "/(No Blink)";                        

            //string path = GetGameObjectPath(renderer.transform.root.gameObject, false);
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
        
        /// <summary>
        /// Reset transforms to prefab
        /// </summary>        
        void ResetPose(GameObject objTo)
        {
            string toPath = GetGameObjectPath(objTo);
            var pref = PrefabUtility.GetPrefabParent(objTo.transform.root.gameObject) as GameObject;
            Transform tr = pref.transform.Find(toPath);

            if(tr == null)
                return;

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
        }

        /// <summary>
        /// Resets all BlendShape weights to 0 on all SkinnedMeshRenderers
        /// </summary>        
        void ResetBlendShapes(GameObject objTo)
        {
            var renders = objTo.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            foreach(var r in renders)
            {
                ResetBlendShapes(r);
            }
        }

        /// <summary>
        /// Reset all BlendShape weights to 0 on SkinnedMeshRenderer
        /// </summary>        
        void ResetBlendShapes(SkinnedMeshRenderer render)
        {
            for(int i = 0; i < render.sharedMesh.blendShapeCount; i++)
            {
                render.SetBlendShapeWeight(i, 0);
            }
        }

#endregion

#region Destroy Functions    

        /// <summary>
        /// Destroys all Collider components from object and all of it's children.
        /// </summary>    
        void DestroyAllColliders(GameObject from)
        {            
            var col = from.GetComponentsInChildren<Collider>(true);
            foreach(var c in col)
            {
                Log(string.Format("Removing collider {0} from {1}", c, from.name));
                DestroyImmediate(c);
            }
        }

        /// <summary>
        /// Destroys all DynamicBone components from object and all of it's children.
        /// </summary>    
        void DestroyAllDynamicBones(GameObject from)
        {
#if !NO_DBONES
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

#if !NO_DBONES
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

        static bool IsSupportedComponentType(Type type)
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

        static string GetGameObjectPath(GameObject obj, bool skipRoot = true)
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

        void Log(string message, LogType logType = LogType.Log, params string[] logFormat)
        {
            if(logFormat.Length > 0)
                message = string.Format(message, logFormat);
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

        bool PhysMaterialsAreIdentical(PhysicMaterial mat1, PhysicMaterial mat2)
        {
            if(mat1 == null && mat2 == null)
                return true;

            if(mat1.bounceCombine == mat2.bounceCombine && mat1.bounciness == mat2.bounciness && mat1.dynamicFriction == mat2.dynamicFriction &&
                mat1.frictionCombine == mat2.frictionCombine && mat1.staticFriction == mat2.staticFriction)
                return true;
            else
                return false;
        }

        bool CollidersAreIdentical(Collider col1, Collider col2)
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

        static Icons()
        {
            Star = EditorGUIUtility.FindTexture("Favorite Icon");
            CsScript = EditorGUIUtility.FindTexture("cs Script Icon");
            Transform = EditorGUIUtility.FindTexture("Transform Icon");
            Avatar = EditorGUIUtility.FindTexture("Avatar Icon");
            SkinnedMeshRenderer = EditorGUIUtility.FindTexture("SkinnedMeshRenderer Icon");
            ColliderBox = EditorGUIUtility.FindTexture("BoxCollider Icon");
            DefaultAsset = EditorGUIUtility.FindTexture("DefaultAsset Icon");
        }             
    }

    public static class Strings
    {
        public static readonly string version = "0.5b";
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
            public static string AvatarInfo_template { get; private set; }
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

                AvatarInfo_template = GetString("ui_avatarInfo_template") ??
                    "_{0}\n-------------------- -\n" +
                    "_GameObjects: {1} ({2})\n\n" +
                    "_Skinned Mesh Renderers: {3} ({4})\n" +
                    "_Mesh Renderers: {5} ({6})\n" +
                    "_Triangles: {7} ({8})\n\n" +
                    "_Materials: {9} ({10})\n" +
                    "_Shaders: {11} \n\n" +
                    "_Dynamic Bone Transforms: {12} ({13})\n" +
                    "_Dynamic Bone Colliders: {14} ({15})\n" +
                    "_Collider Affected Transforms: {16} ({17})\n\n" +
                    "_Particle Systems: {18} ({19})\n" +
                    "_Max Particles: {20} ({21})";
            }            
        };        
        public static class Buttons
        {
            public static string SelectFromScene { get; private set; }
            public static string CopySelected { get; private set; }
            public static string Cancel { get; private set; }
            public static string Apply { get; private set; }
            public static string Refresh { get; private set; }
                       
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
            }
        };
        public static class Tools
        {
            public static string FillVisemes { get; private set; }
            public static string EditViewpoint { get; private set; }
            public static string ResetBlendshapes { get; private set; }
            public static string ResetPose { get; private set; }
            
            static Tools()
            {
                Reload();
            }

            public static void Reload()
            {
                FillVisemes = GetString("ui_tools_fillVisemes") ?? "_Fill Visemes";
                EditViewpoint = GetString("ui_tools_editViewpoint") ?? "_Edit Viewpoint";
                ResetBlendshapes = GetString("ui_tools_resetBlendShapes") ?? "_Reset Blendshapes";
                ResetPose = GetString("ui_tools_resetPose") ?? "_Reset Pose";
            }
        };
        public static class Copier
        {
            public static string CopyFrom { get; private set; }
            public static string Transforms { get; private set; }
            public static string Transforms_position { get; private set; }
            public static string Transforms_rotation { get; private set; }
            public static string Transforms_scale { get; private set; }
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

            static Copier()
            {
                Reload();
            }

            public static void Reload()
            {
                CopyFrom = GetString("ui_copier_copyFrom") ?? "_Copy From";
                Transforms = GetString("ui_copier_transforms") ?? "_Transforms";
                Transforms_position = GetString("ui_copier_transforms_position") ?? "_Position";
                Transforms_rotation = GetString("ui_copier_transforms_rotation") ?? "_Rotation";
                Transforms_scale = GetString("ui_copier_transforms_scale") ?? "_Scale";
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
            }
        };        
        public static class Warning
        {
            public static string Warn { get; private set; }
            public static string NotFound { get; private set; }
            public static string SelectSceneObject { get; private set; }

            static Warning()
            {
                Reload();
            }

            public static void Reload()
            {
                Warn = GetString("warn_warning") ?? "_Warning";
                NotFound = GetString("warn_notFound") ?? "_(Not Found)";
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
                {"ui_misc", "Misc" },
                {"ui_removeAll", "Remove All" },
                {
                    "ui_avatarInfo_template",

                    "{0}\n---------------------\n" +
                    "GameObjects: {1} ({2})\n\n" +
                    "Skinned Mesh Renderers: {3} ({4})\n" +
                    "Mesh Renderers: {5} ({6})\n" +
                    "Triangles: {7} ({8})\n\n" +
                    "Materials: {9} ({10})\n" +
                    "Shaders: {11} \n\n"+
                    "Dynamic Bone Transforms: {12} ({13})\n" +
                    "Dynamic Bone Colliders: {14} ({15})\n" +
                    "Collider Affected Transforms: {16} ({17})\n\n" +
                    "Particle Systems: {18} ({19})\n" +
                    "Max Particles: {20} ({21})"
                },

#region Buttons
                {"buttons_selectFromScene", "Select from Scene" },
                {"buttons_copySelected" , "Copy Selected" },
                {"buttons_refresh", "Refresh" },
                {"buttons_apply", "Apply" },
                {"buttons_cancel", "Cancel" },
#endregion

#endregion
#region Tools
                //UI Tools                
                {"ui_tools_fillVisemes", "Fill Visemes" },
                {"ui_tools_editViewpoint", "Edit Viewpoint" },
                {"ui_tools_resetBlendShapes", "Reset Blendshapes" },
                {"ui_tools_resetPose", "Reset Pose" },
                
#endregion
#region Copier
                //UI Copier
                {"ui_copier_copyFrom", "Copy from" },                

                //UI Copier Transforms
                {"ui_copier_transforms", "Transforms" },
                {"ui_copier_transforms_position", "Position" },
                {"ui_copier_transforms_rotation", "Rotation" },
                {"ui_copier_transforms_scale", "Scale" },
            
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
#endregion

#region Log
                //Log
                { "log_failed", "Failed" },
                { "log_cancelled", "Cancelled" },
                { "log_success", "Success" },
                { "log_done", "Done. Check Unity Console for full Output Log" },
                { "log_copyAttempt", "Attempting to copy {0} from {1} to {2}" },
                { "log_removeAttempt", "Attempting to remove {0} from {1}" },                
                { "log_copyFromInvalid", "Can't copy Components because 'Copy From' is invalid" },
                { "log_cantCopyToSelf", "Can't copy Components from an object to itself. What are you doing?" },
                { "log_viewpointApplied", "Set Viewposition to {0}" },
                { "log_viewpointCancelled", "Cancelled Viewposition changes" },
                { "log_tryFillVisemes", "Attempting to fill visemes on {0}" },
                { "log_noSkinnedMeshFound", "Failed: No skinned mesh found" },
                { "log_descriptorIsNull", "Avatar descriptor is null"},
                { "log_meshHasNoVisemes", "Failed. Mesh has no Visemes. Set to Default" },
                { "log_tryRemoveUnsupportedComponent", "Attempted to remove unsupported component {0} from {1}" },
#endregion

#region Warnings
                //Warnings
                { "log_warning", "Warning" },
                { "warn_selectSceneObject" , "Please select an object from the scene" },
                { "warn_notFound", "(Not Found)" },
                //{ "warn_copyToPrefab", "You are trying to copy components to a prefab.\nThis cannot be undone.\nAre you sure you want to continue?" },
                //{ "warn_prefabOverwriteYes", "Yes, Overwrite" },
                //{ "warn_prefabOverwriteNo", "No, Cancel" },
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
                {"ui_tools", "Toows òwó" },
                {"ui_copier", "Copy Componyents uwu" },
                {"ui_avatarInfo", "Avataw Info 0w0" },
                {"ui_misc", "Misc ;o" },
                {"ui_removeAll", "Wemuv Aww (⁰д⁰ )" },
                {
                    "ui_avatarInfo_template",

                    "{0}\n---------------------\n" +
                    "GameObjects: {1} ({2})\n\n" +
                    "Skinnyed Mesh Wendewews: {3} ({4})\n" +
                    "Mesh Wendewews: {5} ({6})\n" +
                    "Twiangwes: {7} ({8})\n\n" +
                    "Matewiaws: {9} ({10})\n" +
                    "Shadews: {11} \n\n"+
                    "Dynyamic Bonye Twansfowms: {12} ({13})\n" +
                    "Dynyamic Bonye Cowwidews: {14} ({15})\n" +
                    "Cowwidew Affected Twansfowms: {16} ({17})\n\n" +
                    "Pawticwe Systems: {18} ({19})\n" +
                    "Max Pawticwes: {20} ({21})\n\n" +
                    "owos: 12000 (42000)"
                },

#region Buttons
                {"buttons_selectFromScene", "Sewect fwom Scenye x3" },
                {"buttons_copySelected" , "Copy Sewected (´• ω •`)" },
                {"buttons_refresh", "Wefwesh (ﾟωﾟ;)" },
                {"buttons_apply", "Appwy （>﹏<）" },
                {"buttons_cancel", "Cancew ; o;" },
#endregion

#endregion
#region Tools
                //UI Toows                
                {"ui_tools_fillVisemes", "Fiww Visemes ;~;" },
                {"ui_tools_editViewpoint", "Edit Viewpoint o-o" },
                {"ui_tools_resetBlendShapes", "Weset Bwendshapes uwu" },
                {"ui_tools_resetPose", "Weset Pose ;3" },

#endregion
#region Copier
                //UI Copier
                {"ui_copier_copyFrom", "Copy fwom~" },                

                //UI Copier Transforms
                {"ui_copier_transforms", "Twansfowms!" },
                {"ui_copier_transforms_position", "Position~" },
                {"ui_copier_transforms_wotation", "Wotation @~@" },
                {"ui_copier_transforms_scawe", "Scawe www" },

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
#endregion

#region Log
                //Log
                { "log_failed", "Faiwed ùwú" },
                { "log_cancelled", "Cancewwed .-." },
                { "log_success", "Success OWO" },
                { "log_done", "Donye. Check Unyity Consowe fow fuww Output Wog uwus" },
                { "log_copyAttempt", "Attempting to copy {0} fwom {1} to {2} o-o" },
                { "log_remuveAttempt", "Attempting to wemuv {0} fwom {1} ;-;" },
                { "log_copyFromInvalid", "Can't copy Componyents because 'Copy Fwom' is invawid ; o ;" },
                { "log_cantCopyToSelf", "Can't copy Componyents fwom an object to itsewf. What awe you doing? ;     w     ;" },
                { "log_viewpointApplied", "Set Viewposition to {0}!" },
                { "log_viewpointCancelled", "Cancewwed Viewposition changes uwu" },
                { "log_tryFixVisemes", "Attempting to fiww visemes on {0}!" },
                { "log_noSkinnedMeshFound", "Faiwed: Nyo skinnyed mesh found ;o;" },
                { "log_descriptorIsNull", "Avataw descwiptow is nyuww humpf"},
                { "log_meshHasNoVisemes", "Faiwed. Mesh has nyo Visemes. Set to Defauwt ;w;" },
                { "log_tryRemoveUnsupportedComponent", "Attempted to wemuv unsuppowted componyent {0} fwom {1} uwu7" },
#endregion

#region Warnings
                //Warnings
                { "log_warning", "Wawnying! unu" },
                { "warn_selectSceneObject" , "Pwease sewect an object fwom the scenye!!" },
                { "warn_notFound", "(Nyot Fownd ; ﻿﻿~;)" },
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
        }

        static string GetString(string stringName)//, params string[] formatArgs)
        {
            if(string.IsNullOrEmpty(stringName))
                return stringName;

            string s = string.Empty;
            stringDictionary.TryGetValue(stringName, out s);

            /*if(formatArgs.Length > 0)
            {
                if(!string.IsNullOrEmpty(s))
                {
                    s = string.Format(stringName, formatArgs);
                }
            }*/
            return s;
        }
    };

    public class AvatarInfo
    {
        string name;
        string cachedInfo;

        int skinnedMeshRenders;
        int skinnedMeshRenders_total;

        int meshRenderers;
        int meshRenderers_total;

        int dynamicBoneTransforms;
        int dynamicBoneTransforms_total;
        int dynamicBoneColliders;
        int dynamicBoneColliders_total;
        int dynamicBoneColliderTransforms;
        int dynamicBoneColliderTransforms_total;

        int triangles;
        int triangles_total;
        int materials;
        int materials_total;
        int shaderCount;        

        int particleSystems;
        int particleSystems_total;
        int maxParticles;
        int maxParticles_total;

        int gameObjects;
        int gameObjects_total;

        AvatarInfo()
        {
            cachedInfo = null;

            skinnedMeshRenders = 0;
            skinnedMeshRenders_total = 0;

            meshRenderers = 0;
            meshRenderers_total = 0;

            dynamicBoneTransforms = 0;
            dynamicBoneTransforms_total = 0;
            dynamicBoneColliders = 0;
            dynamicBoneColliders_total = 0;
            dynamicBoneColliderTransforms = 0;
            dynamicBoneColliderTransforms_total = 0;

            triangles = 0;
            triangles_total = 0;
            materials = 0;
            materials_total = 0;
            shaderCount = 0;            

            particleSystems = 0;
            particleSystems_total = 0;
            maxParticles = 0;
            maxParticles_total = 0;

            gameObjects = 0;
            gameObjects_total = 0;
        } 

        public AvatarInfo(GameObject o) : base()
        {
            if(o == null)
                return;

            name = o.name;
            var shaders = new List<Shader>();

            var ts = o.GetComponentsInChildren<Transform>(true);
            foreach(var t in ts)
            {
                gameObjects_total += 1;
                if(t.gameObject.activeInHierarchy)
                    gameObjects += 1;
            }

            var sRenders = o.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            foreach(var r in sRenders)
            {
                skinnedMeshRenders_total += 1;
                triangles_total += r.sharedMesh.triangles.Length/3;
                materials_total += r.sharedMaterials.Length;

                if(r.gameObject.activeInHierarchy && r.enabled)
                {
                    skinnedMeshRenders += 1;
                    triangles += r.sharedMesh.triangles.Length/3;
                    materials += r.sharedMaterials.Length;                    
                }

                foreach(var mat in r.sharedMaterials)
                {
                    if(shaders.IndexOf(mat.shader) == -1)
                        shaders.Add(mat.shader);
                }
            }

            var renders = o.GetComponentsInChildren<MeshRenderer>(true);
            foreach(var r in renders)
            {                
                var filter = r.GetComponent<MeshFilter>();

                if(filter != null && filter.sharedMesh != null)
                {
                    meshRenderers_total += 1;
                    triangles_total += filter.sharedMesh.triangles.Length;

                    if(r.gameObject.activeInHierarchy && r.enabled)
                    {
                        meshRenderers += 1;
                        triangles += filter.sharedMesh.triangles.Length;
                    }
                }

                foreach(var mat in r.sharedMaterials)
                {
                    if(mat != null && shaders.IndexOf(mat.shader) == -1)
                        shaders.Add(mat.shader);
                }
            }

#if !NO_DBONES

            var dbColliders = o.GetComponentsInChildren<DynamicBoneCollider>(true);
            foreach(var c in dbColliders)
            {
                dynamicBoneColliders_total += 1;

                if(c.gameObject.activeInHierarchy)
                    dynamicBoneColliders += 1;
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
                            dynamicBoneColliderTransforms += affected;
                            dynamicBoneColliderTransforms_total += affected_total;
                            break;
                        }
                    }

                    dynamicBoneTransforms += affected;
                    dynamicBoneTransforms_total += affected_total;
                }
            }

#endif

            var ptc = o.GetComponentsInChildren<ParticleSystem>(true);
            foreach(var p in ptc)
            {
                particleSystems_total += 1;
                maxParticles_total += p.main.maxParticles;

                if(p.gameObject.activeInHierarchy && p.emission.enabled)
                {
                    particleSystems += 1;
                    maxParticles += p.main.maxParticles;
                }
            }

            shaderCount = shaders.Count;
        }

        public static AvatarInfo GetInfo(GameObject o, out string toString)
        {
            AvatarInfo a = new AvatarInfo(o);
            toString = a.ToString();
            return a;
        }

        public override string ToString()
        {
            if(!string.IsNullOrEmpty(cachedInfo))
                return cachedInfo;                        
            else
            {
                if(this == null)
                {
                    return null;
                }
                try
                {
                    cachedInfo = string.Format
                    (        
                        Strings.Main.AvatarInfo_template,
                        name,
                        gameObjects,
                        gameObjects_total,
                        skinnedMeshRenders,
                        skinnedMeshRenders_total,
                        meshRenderers,
                        meshRenderers_total,
                        triangles,
                        triangles_total,
                        materials,
                        materials_total,
                        shaderCount,
                        dynamicBoneTransforms,
                        dynamicBoneTransforms_total,
                        dynamicBoneColliders,
                        dynamicBoneColliders_total,
                        dynamicBoneColliderTransforms,
                        dynamicBoneColliderTransforms_total,
                        particleSystems,
                        particleSystems_total,
                        maxParticles,
                        maxParticles_total
                    );                    
                }
                catch(Exception)
                {
                    cachedInfo = null;
                }
                return cachedInfo;
            }            
        }        
    }
#endregion
}
