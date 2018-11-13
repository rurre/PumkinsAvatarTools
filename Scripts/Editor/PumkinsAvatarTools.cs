using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VRCSDK2;
using VRC.Core;
using UnityEditor.SceneManagement;


/// <summary>
/// VRCAvatar tools by Pumkin
/// https://github.com/rurre/VRCAvatarTools
/// </summary>

namespace Pumkin
{
    [ExecuteInEditMode]
    public class PumkinsAvatarTools : EditorWindow
    {
        #region Variables

        static GameObject selectedCopyFrom;
        static GameObject selectedCopyTo;

        bool bTransforms_copy = true;
        bool bTransforms_copyPosition = true;
        bool bTransforms_copyRotation = true;
        bool bTransforms_CopyScale = true;

        bool bDynamicBones_copy = true;
        bool bDynamicBones_copySettings = true;        
        bool bDynamicBones_createMissingBones = true;
        bool bDynamicBones_copyColliders = true;
        bool bDynamicBones_removeOldColliders = true;
        bool bDynamicBones_removeOldBones = true;

        bool bDescriptor_copy = true;
        bool bDescriptor_copySettings = true;
        bool bDescriptor_copyPipelineId = true;
        bool bDescriptor_copyAnimationOverrides = true;

        bool bColliders_copy = true;
        bool bColliders_removeOld = true;
        bool bColliders_copyBox = true;
        bool bColliders_copyCapsule = true;
        bool bColliders_copySphere = true;
        bool bColliders_copyMesh = true;

        bool bSkinMeshRender_copy = true;
        bool bSkinMeshRender_copySettings = true;
        bool bSkinMeshRender_copyBlendShapeValues = true;
        bool bSkinMeshRender_resetBlendShapeValues = true;
        bool bSkinMeshRender_copyMaterials = true;        

        //Editor
        bool _expandTransforms = false;
        bool _expandDynamicBones = false;
        bool _expandAvatarDescriptor = false;
        bool _expandSkinnedMeshRenderer = false;
        bool _expandColliders = false;

        //Strings

        static string _logTemplate, _msg;        

        bool _openedInfo = false;
        Vector2 vertScroll = Vector2.zero;        

        #endregion

        #region Unity GUI

        [MenuItem("Tools/Pumkin/Avatar Tools")]
        public static void ShowWindow()
        {
            //Show existing window instance. If one doesn't exist, make one.
            EditorWindow editorWindow = EditorWindow.GetWindow(typeof(PumkinsAvatarTools));
            editorWindow.autoRepaintOnSceneChange = true;
            editorWindow.Show();
            editorWindow.titleContent = new GUIContent(AvatarToolsStrings.GetString("main_windowName") ?? "_Avatar Tools");
        }

        public static void ReloadStrings()
        {
            _msg = AvatarToolsStrings.GetString("main_msgDefault") ?? "_Pick Objects to copy Components to and from.";
            _logTemplate = AvatarToolsStrings.GetString("log_actionAttempt") ?? "_Attempting to copy {0} from {1} to {2}";
        }

        void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(AvatarToolsStrings.GetString("main_title") ?? "_Pumkin's Avatar Tools", EditorStyles.boldLabel);

            EditorGUIUtility.SetIconSize(new Vector2(14, 14));

            if(GUILayout.Button(EditorGUIUtility.FindTexture("Favorite Icon"), "IconButton", GUILayout.MaxWidth(17)))
            {
                _openedInfo = !_openedInfo;
            }
            EditorGUILayout.EndHorizontal();

            if(_openedInfo)
            {
                EditorGUILayout.Space();
                GUILayout.BeginVertical();
                                
                GUILayout.Label(AvatarToolsStrings.GetString("credits_line1") ?? "Pumkin's Avatar Tools");
                GUILayout.Label(AvatarToolsStrings.GetString("credits_line2") ?? "Version " + AvatarToolsStrings.version );
                GUILayout.Label(AvatarToolsStrings.GetString("credits_line3") ?? "Now with 100% more redundant strings");
                EditorGUILayout.Space();                
                GUILayout.Label(AvatarToolsStrings.GetString("credits_line4") ?? "Why did the default strings load? Help!");

                GUILayout.BeginHorizontal();
                
                GUILayout.Label(AvatarToolsStrings.GetString("credits_line5") ?? "Poke me on Discord at Pumkin#2020", GUILayout.ExpandWidth(false));                

                GUILayout.EndHorizontal();
                GUILayout.EndVertical();

                GUILayout.FlexibleSpace();                

                if(GUILayout.Button(AvatarToolsStrings.GetString("misc_uwu") ?? "uwu", "IconButton", GUILayout.ExpandWidth(false)))
                {
                    if(AvatarToolsStrings.Language != AvatarToolsStrings.DictionaryLanguage.uwu)
                        AvatarToolsStrings.SetLanguage(AvatarToolsStrings.DictionaryLanguage.uwu);
                    else
                        AvatarToolsStrings.SetLanguage(AvatarToolsStrings.DictionaryLanguage.English);
                }
            }
            else
            {
                EditorGUILayout.Space();                
                
                selectedCopyTo = (GameObject)EditorGUILayout.ObjectField(AvatarToolsStrings.GetString("ui_copyTo") ?? "_Copy to:", selectedCopyTo, typeof(GameObject), true);
                selectedCopyFrom = (GameObject)EditorGUILayout.ObjectField(AvatarToolsStrings.GetString("ui_copyFrom") ?? "_Copy from:", selectedCopyFrom, typeof(GameObject), true);

                EditorGUILayout.BeginHorizontal();
                if(GUILayout.Button(AvatarToolsStrings.GetString("button_clear") ?? "_Clear"))
                {
                    selectedCopyTo = null;
                    selectedCopyFrom = null;
                }
                if(GUILayout.Button(AvatarToolsStrings.GetString("button_swap") ?? "_Swap"))
                {
                    GameObject o = selectedCopyTo;
                    selectedCopyTo = selectedCopyFrom;
                    selectedCopyFrom = o;
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();

                vertScroll = EditorGUILayout.BeginScrollView(vertScroll);

                //Transforms menu
                EditorGUILayout.BeginHorizontal();
                _expandTransforms = GUILayout.Toggle(_expandTransforms, EditorGUIUtility.IconContent("Transform Icon"), "Foldout", GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(true), GUILayout.MaxWidth(30), GUILayout.MaxHeight(10));
                bTransforms_copy = GUILayout.Toggle(bTransforms_copy, AvatarToolsStrings.GetString("ui_transforms") ?? "_Transforms", GUILayout.ExpandWidth(false), GUILayout.MinWidth(20));
                EditorGUILayout.EndHorizontal();

                if(_expandTransforms)
                {
                    EditorGUI.BeginDisabledGroup(!bTransforms_copy);
                    EditorGUILayout.Space();

                    bTransforms_copyPosition = EditorGUILayout.Toggle(AvatarToolsStrings.GetString("ui_transforms_position") ?? "_Position", bTransforms_copyPosition, GUILayout.ExpandWidth(false));
                    bTransforms_copyRotation = EditorGUILayout.Toggle(AvatarToolsStrings.GetString("ui_transforms_rotation") ?? "_Rotation", bTransforms_copyRotation, GUILayout.ExpandWidth(false));
                    bTransforms_CopyScale = EditorGUILayout.Toggle(AvatarToolsStrings.GetString("ui_transforms_scale") ?? "_Scale", bTransforms_CopyScale);

                    EditorGUILayout.Space();
                    EditorGUI.EndDisabledGroup();
                }

                //DynamicBones menu
                EditorGUILayout.BeginHorizontal();
                _expandDynamicBones = GUILayout.Toggle(_expandDynamicBones, EditorGUIUtility.IconContent("cs Script Icon"), "Foldout", GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(true), GUILayout.MaxWidth(30), GUILayout.MaxHeight(10));
                bDynamicBones_copy = GUILayout.Toggle(bDynamicBones_copy, AvatarToolsStrings.GetString("ui_dynamicBones") ?? "_Dynamic Bones", GUILayout.ExpandWidth(false), GUILayout.MinWidth(20));
                EditorGUILayout.EndHorizontal();

                if(_expandDynamicBones)
                {
                    EditorGUI.BeginDisabledGroup(!bDynamicBones_copy);
                    EditorGUILayout.Space();

                    bDynamicBones_copySettings = EditorGUILayout.Toggle(AvatarToolsStrings.GetString("ui_dynamicBones_settings") ?? "_Settings", bDynamicBones_copySettings, GUILayout.ExpandWidth(false));
                    bDynamicBones_copyColliders = EditorGUILayout.Toggle(AvatarToolsStrings.GetString("ui_dynamicBones_colliders") ?? "_Colliders", bDynamicBones_copyColliders, GUILayout.ExpandWidth(false));
                    bDynamicBones_createMissingBones = EditorGUILayout.Toggle(AvatarToolsStrings.GetString("ui_dynamicBones_createMissing") ?? "_Create Missing Bones", bDynamicBones_createMissingBones, GUILayout.ExpandWidth(false));
                    bDynamicBones_removeOldBones = EditorGUILayout.Toggle(AvatarToolsStrings.GetString("ui_dynamicBones_removeOld") ?? "_Remove Old Bones", bDynamicBones_removeOldBones, GUILayout.ExpandWidth(false));
                    bDynamicBones_removeOldColliders = EditorGUILayout.Toggle(AvatarToolsStrings.GetString("ui_dynamicBones_removeOldColliders") ?? "_Remove Old Colliders", bDynamicBones_removeOldColliders, GUILayout.ExpandWidth(false));

                    EditorGUILayout.Space();
                    EditorGUI.EndDisabledGroup();
                }

                //AvatarDescriptor menu
                EditorGUILayout.BeginHorizontal();
                _expandAvatarDescriptor = GUILayout.Toggle(_expandAvatarDescriptor, EditorGUIUtility.IconContent("Avatar Icon"), "Foldout", GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(true), GUILayout.MaxWidth(30), GUILayout.MaxHeight(10));
                bDescriptor_copy = GUILayout.Toggle(bDescriptor_copy, AvatarToolsStrings.GetString("ui_descriptor") ?? "_Avatar Descriptor", GUILayout.ExpandWidth(false), GUILayout.MinWidth(20));
                EditorGUILayout.EndHorizontal();

                if(_expandAvatarDescriptor)
                {
                    EditorGUI.BeginDisabledGroup(!bDescriptor_copy);
                    EditorGUILayout.Space();

                    bDescriptor_copySettings = EditorGUILayout.Toggle(AvatarToolsStrings.GetString("ui_descriptor_settings") ?? "_Settings", bDescriptor_copySettings, GUILayout.ExpandWidth(false));
                    bDescriptor_copyPipelineId = EditorGUILayout.Toggle(AvatarToolsStrings.GetString("ui_descriptor_pipelineId") ?? "_Pipeline ID", bDescriptor_copyPipelineId, GUILayout.ExpandWidth(false));
                    bDescriptor_copyAnimationOverrides = EditorGUILayout.Toggle(AvatarToolsStrings.GetString("ui_descriptor_animationOverrides") ?? "_Animation Overrides", bDescriptor_copyAnimationOverrides, GUILayout.ExpandWidth(false));

                    EditorGUILayout.Space();
                    EditorGUI.EndDisabledGroup();
                }

                //SkinnedMeshRenderer menu
                EditorGUILayout.BeginHorizontal();
                _expandSkinnedMeshRenderer = GUILayout.Toggle(_expandSkinnedMeshRenderer, EditorGUIUtility.IconContent("SkinnedMeshRenderer Icon"), "Foldout", GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(true), GUILayout.MaxWidth(30), GUILayout.MaxHeight(10));
                bSkinMeshRender_copy = GUILayout.Toggle(bSkinMeshRender_copy, AvatarToolsStrings.GetString("ui_skinMeshRender") ?? "_Skinned Mesh Renderers", GUILayout.ExpandWidth(false), GUILayout.MinWidth(20));
                EditorGUILayout.EndHorizontal();

                if(_expandSkinnedMeshRenderer)
                {
                    EditorGUI.BeginDisabledGroup(!bSkinMeshRender_copy);
                    EditorGUILayout.Space();

                    bSkinMeshRender_copySettings = EditorGUILayout.Toggle(AvatarToolsStrings.GetString("ui_skinMeshRender_settings") ?? "_Settings", bSkinMeshRender_copySettings, GUILayout.ExpandWidth(false));
                    bSkinMeshRender_copyMaterials = EditorGUILayout.Toggle(AvatarToolsStrings.GetString("ui_skinMeshRender_materials") ?? "_Materials", bSkinMeshRender_copyMaterials, GUILayout.ExpandWidth(false));
                    bSkinMeshRender_copyBlendShapeValues = EditorGUILayout.Toggle(AvatarToolsStrings.GetString("ui_skinMeshRender_blendShapeValues") ?? "_BlendShape Values", bSkinMeshRender_copyBlendShapeValues, GUILayout.ExpandWidth(false));
                    bSkinMeshRender_resetBlendShapeValues = EditorGUILayout.Toggle(AvatarToolsStrings.GetString("ui_skinMeshRender_resetBlendShapes") ?? "_Reset BlendShapes", bSkinMeshRender_resetBlendShapeValues, GUILayout.ExpandWidth(false));

                    EditorGUILayout.Space();
                    EditorGUI.EndDisabledGroup();
                }

                //Collider menu
                EditorGUILayout.BeginHorizontal();
                _expandColliders = GUILayout.Toggle(_expandColliders, EditorGUIUtility.IconContent("BoxCollider Icon"), "Foldout", GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(true), GUILayout.MaxWidth(30), GUILayout.MaxHeight(10));
                bColliders_copy = GUILayout.Toggle(bColliders_copy, AvatarToolsStrings.GetString("ui_colliders") ?? "_Colliders", GUILayout.ExpandWidth(false), GUILayout.MinWidth(20));
                EditorGUILayout.EndHorizontal();

                if(_expandColliders)
                {
                    EditorGUI.BeginDisabledGroup(!bColliders_copy);
                    EditorGUILayout.Space();

                    bColliders_copyBox = EditorGUILayout.Toggle(AvatarToolsStrings.GetString("ui_colliders_box") ?? "_Box Colliders", bColliders_copyBox, GUILayout.ExpandWidth(false));
                    bColliders_copyCapsule = EditorGUILayout.Toggle(AvatarToolsStrings.GetString("ui_colliders_capsule") ?? "_Capsule Colliders", bColliders_copyCapsule, GUILayout.ExpandWidth(false));
                    bColliders_copySphere = EditorGUILayout.Toggle(AvatarToolsStrings.GetString("ui_colliders_sphere") ?? "_Sphere Colliders", bColliders_copySphere, GUILayout.ExpandWidth(false));
                    bColliders_copyMesh = EditorGUILayout.Toggle(AvatarToolsStrings.GetString("ui_colliders_mesh") ?? "_Mesh Colliders", bColliders_copyMesh, GUILayout.ExpandWidth(false));

                    bColliders_removeOld = EditorGUILayout.Toggle(AvatarToolsStrings.GetString("ui_colliders_removeOld") ?? "_Remove Old Colliders", bColliders_removeOld, GUILayout.ExpandWidth(false));

                    EditorGUILayout.Space();
                    EditorGUI.EndDisabledGroup();
                }

                EditorGUILayout.EndScrollView();

                EditorGUILayout.Space();
                if(GUILayout.Button(AvatarToolsStrings.GetString("button_copySelected") ?? "_Copy Selected"))
                {
                    if(selectedCopyFrom == null && selectedCopyTo == null)
                        _msg = AvatarToolsStrings.GetString("log_copyToAndFromInvalid") ?? "_Can't copy Components because 'Copy From' & 'Copy To' are invalid.";
                    else if(selectedCopyFrom == null)
                        _msg = AvatarToolsStrings.GetString("log_copyFromInvalid") ?? "_Can't copy Components because 'Copy From' is invalid.";
                    else if(selectedCopyTo == null)
                        _msg = AvatarToolsStrings.GetString("log_copyToInvalid") ?? "_Can't copy Components because 'Copy To' is invalid.";
                    else
                    {
                        if(selectedCopyTo.gameObject.scene.name == null)
                        {
                            if(!EditorUtility.DisplayDialog(AvatarToolsStrings.GetString("warn_warning") ?? "_Warning",
                                AvatarToolsStrings.GetString("warn_copyToPrefab") ?? "_You are trying to copy components to a prefab.\nThis cannot be undone.\nAre you sure you want to continue?",
                                AvatarToolsStrings.GetString("warn_prefabOverwriteYes") ?? "_Yes, Overwrite", AvatarToolsStrings.GetString("warn_prefabOverwriteNo") ?? "_No, Cancel"))
                            {
                                _msg = "_Canceled.";
                                return;
                            }
                        }

                        //Cancel Checks
                        if(selectedCopyFrom == selectedCopyTo)
                        {
                            _msg = AvatarToolsStrings.GetString("log_cantCopyToItself") ?? "_Can't copy Components from an object to itself. What are you doing?";
                            return;
                        }
                        if(!(bDynamicBones_copyColliders || bDynamicBones_copy || bColliders_copy || bDescriptor_copy || bSkinMeshRender_copy))
                        {
                            _msg = AvatarToolsStrings.GetString("log_noComponentsSelected") ?? "_No components selected";
                            return;
                        }                        

                        CopyComponents(selectedCopyFrom, selectedCopyTo);
                        _msg = AvatarToolsStrings.GetString("log_done") ?? "_Done. Check Unity Console for Output Log";
                    }
                }
                EditorGUILayout.BeginVertical(GUILayout.MinHeight(30));
                EditorGUILayout.HelpBox(_msg, MessageType.None);
                EditorGUILayout.Space();
                EditorGUILayout.EndVertical();
            }
        }

        #endregion

        #region Copy Functions

        /// <summary>
        /// Copies Components and Values from one object to another.
        /// </summary>       
        void CopyComponents(GameObject objFrom, GameObject objTo)
        {
            //Cancel Checks
            if(objFrom == objTo)
            {
                _msg = AvatarToolsStrings.GetString("log_cantCopyToItself") ?? "_Can't copy Components from an object to itself. What are you doing?";
                return;
            }

            //Pre Copying Operations

            //Run statment only if root so only run this once
            if(objTo.transform == objTo.transform.root)
            {                
                if(bDynamicBones_copy)
                {
                    if(bDynamicBones_removeOldColliders)
                        DestroyAllDynamicBoneColliders(selectedCopyTo);
                    if(bDynamicBones_removeOldBones)
                        DestroyAllDynamicBones(selectedCopyTo);
                }
                if(bColliders_copy)
                {
                    if(bColliders_removeOld)
                        DestroyAllColliders(selectedCopyTo);
                }
                if(bDescriptor_copy)
                {
                    CopyAvatarDescriptor(objFrom, objTo);
                }
            }
            //End run once
            
            if(bTransforms_copy)
            {
                CopyTransforms(objFrom, objTo);
            }
            if(bDynamicBones_copy)
            {
                if(bDynamicBones_copySettings || bDynamicBones_copyColliders)
                {
                    CopyDynamicBones(objFrom, objTo, bDynamicBones_createMissingBones);
                }
            }
            if(bColliders_copy)
            {                
                CopyColliders(objFrom, objTo);
            }
            if(bSkinMeshRender_copy)
            {
                //CopySkinMeshRenderer(objFrom, objTo, bSkinMeshRender_copyBlendShapeValues, bSkinMeshRender_copyMaterials, bSkinMeshRender_copySettings, bSkinMeshRender_resetBlendShapeValues);
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

            if(bDescriptor_copyPipelineId)
            {
                string id = pFrom.blueprintId ?? string.Empty;

                pTo.blueprintId = pFrom.blueprintId;
                pTo.enabled = pFrom.enabled;
                pTo.completedSDKPipeline = true;

                EditorUtility.SetDirty(pTo);
                EditorSceneManager.MarkSceneDirty(pTo.gameObject.scene);
                EditorSceneManager.SaveScene(pTo.gameObject.scene);
            }

            if(bDescriptor_copySettings)
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

                if(bDescriptor_copyAnimationOverrides)
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
            string[] logFormat = {  "DynamicBoneCollider", from.name, to.name };
            string log = _logTemplate;
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
                    //Debug.LogFormat("_{0} has no DynamicBoneCollider. Ignoring.", from.name);
                    return;
                }

                log += "{2} has no {1}. Creating - ";                
                //Debug.LogWarningFormat("_{0} has no DynamicBoneCollider. Creating", to.name);
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
                    log += "Duplicate {1} with the same settings already exists. Removing duplicate.";
                    Log(log, LogType.Warning, logFormat);
                    //Debug.LogFormat("_Duplicate DynamicBoneCollider with the exact same settings already exists. Removing duplicate.");
                }
                else
                {
                    log += "Success: Added {1} to {3}.";
                    Log(log, LogType.Log, logFormat);
                    //Debug.LogFormat("_Succesfully copied DynamicBoneCollider component from {0} to {1}", from.name, to.name);
                }
            }
        }

        /// <summary>
        /// Copies DynamicBone components. 
        /// </summary>
        void CopyDynamicBones(GameObject from, GameObject to, bool createMissing = true)
        {            
            string log = _logTemplate;
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

                if(bDynamicBones_copyColliders)
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
                                log += "Success: Added {1} to {3}";
                                Log(log, LogType.Log, logFormat);
                            }
                        }
                    }
                }

                logFormat = new string[] {  "DynamicBone", from.name, to.name };
                log = _logTemplate;

                if(dFrom == null)
                {
                    log += "Failed - {2} has no {1}. Ignoring";
                    Log(log, LogType.Warning, logFormat);
                    return;
                }
                else if(!bDynamicBones_copySettings)
                {
                    log += "Failed - Not allowed to: Copy settings is unchecked.";
                    Log(log, LogType.Warning, logFormat);
                    return;
                }
                else if(dFrom.m_Root == null)
                {
                    Log(log, LogType.Warning, logFormat);
                    log += "Failed - {3}'s {2} has no Root set. Ignoring";
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

                log += "Success: Copied {1} from {2} to {3}";
                Log(log, LogType.Log, logFormat);
                //Debug.LogFormat("Succesfully copied DynamicBone component from {0} to {1}", from.name, to.name);
            }
        }

        /// <summary>
        /// Copies Box, Capsule, Sphere and Mesh colliders from one object to another
        /// </summary>        
        void CopyColliders(GameObject from, GameObject to)
        {
            if(!(bColliders_copyBox || bColliders_copyCapsule || bColliders_copyMesh || bColliders_copySphere))
                return;

            string log = _logTemplate;            

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
                    if(bColliders_copyBox && cFrom is BoxCollider)
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
                    else if(bColliders_copyCapsule && cFrom is CapsuleCollider)
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
                    else if(bColliders_copySphere && cFrom is SphereCollider)
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
                    else if(bColliders_copyMesh && cFrom is MeshCollider)
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
                        log += "Failed: Unsupported Collider type {1} on {2}. Ignoring";
                        Log(log, LogType.Error, logFormat);
                        return;
                    }
                }
                else
                {
                    log += "Failed: {1} already exists on {3}. Ignoring";
                    Log(log, LogType.Warning, logFormat);
                    return;
                }
                log += "Success - Added {1} to {3}";
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

            string log = _logTemplate;
            string[] logFormat = { "Transforms", from.name, to.name };

            if(tTo == null || tFrom == null)
            {
                log += "Failed: {2} or {3} is null. This shouldn't even be possible. What are you doing?";                
                Log(log, LogType.Error);
                return;
            }

            if(tFrom == tFrom.root || tFrom == tFrom.root.Find(tFrom.name))
            {
                log += "Ignored: {2} is root or child of root.";
                Log(log, LogType.Warning, logFormat);
                return;
            }

            if(bTransforms_copyPosition)
                tTo.localPosition = tFrom.localPosition;
            if(bTransforms_CopyScale)
                tTo.localScale = tFrom.localScale;
            if(bTransforms_copyRotation)
            {
                tTo.localEulerAngles = tFrom.localEulerAngles;
                tTo.localRotation = tFrom.localRotation;
            }

            log += "Success: Copied {1} from {2} to {3}";
            Log(log, LogType.Log ,logFormat);
        }

        /// <summary>
        /// Copies SkinnedMeshRenderer settings. Note that only one can exist on an object.
        /// </summary>                
        void CopySkinMeshRenderer(GameObject from, GameObject to)//, bool copyBlendShapeValues, bool copyMaterials, bool copySettings, bool resetBlendShapeValues)
        {
            //if(!(copyBlendShapeValues || copyMaterials || copySettings || resetBlendShapeValues))
            if(!(bSkinMeshRender_copyBlendShapeValues || bSkinMeshRender_copyMaterials || bSkinMeshRender_copySettings || bSkinMeshRender_resetBlendShapeValues))
                return;

            string log = _logTemplate;
            string[] logFormat = { "SkinnedMeshRenderer", from.name, to.name };

            SkinnedMeshRenderer rFrom = from.GetComponent<SkinnedMeshRenderer>();
            SkinnedMeshRenderer rTo = to.GetComponent<SkinnedMeshRenderer>();

            if(rTo != null )
            {
                log += "Failed: {2} is null. Ignoring";
                Log(log, LogType.Warning, logFormat);

                if(bSkinMeshRender_resetBlendShapeValues)//(resetBlendShapeValues)
                {
                    for(int i = 0; i < rTo.sharedMesh.blendShapeCount; i++)
                    {
                        rTo.SetBlendShapeWeight(i, 0);
                    }
                }
            }
            if(rFrom == null)
            {
                log += "Failed: {1} is null. Ignoring";
                Log(log, LogType.Warning, logFormat);
                return;
            }
            
            Mesh fromMesh = rFrom.sharedMesh;

            if(bSkinMeshRender_copySettings)//(copySettings)
            {
                rTo.enabled = rFrom.enabled;
                rTo.quality = rFrom.quality;
                rTo.updateWhenOffscreen = rFrom.updateWhenOffscreen;
                rTo.skinnedMotionVectors = rFrom.skinnedMotionVectors;
                rTo.sharedMesh = rFrom.sharedMesh;
                rTo.rootBone = rFrom.rootBone;
                rTo.lightProbeUsage = rFrom.lightProbeUsage;
                rTo.reflectionProbeUsage = rFrom.reflectionProbeUsage;
                rTo.probeAnchor = rFrom.probeAnchor;
                rTo.shadowCastingMode = rFrom.shadowCastingMode;
                rTo.receiveShadows = rFrom.receiveShadows;                
                rTo.motionVectorGenerationMode = rFrom.motionVectorGenerationMode;                
            }

            if(bSkinMeshRender_copyBlendShapeValues)//(copyBlendShapeValues)
            {                
                for(int i = 0; i < rFrom.sharedMesh.blendShapeCount; i++)
                {
                    string name = fromMesh.GetBlendShapeName(i);
                    int frames = fromMesh.GetBlendShapeFrameCount(i);

                    if(!string.IsNullOrEmpty(name))
                    {
                        rTo.SetBlendShapeWeight(rTo.sharedMesh.GetBlendShapeIndex(name), fromMesh.GetBlendShapeFrameWeight(i,0));
                    }
                }
            }

            if(bSkinMeshRender_copyMaterials)//(copyMaterials)
            {
                rTo.sharedMaterials = rFrom.sharedMaterials;                
            }

            log += "Success: Copied {1} from {2} to {3}";
            Log(log, LogType.Log, logFormat);
        }

        #endregion

        #region Destroy Functions    

        /// <summary>
        /// Destroys all Collider components from object and all of it's children.
        /// </summary>    
        void DestroyAllColliders(GameObject from)
        {            
            var col = from.GetComponentsInChildren<Collider>();
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
            var bones = from.GetComponentsInChildren<DynamicBone>();
            foreach(var b in bones)
            {
                DestroyImmediate(b);
            }
        }

        /// <summary>
        /// Destroys all DynamicBoneCollider components from object and it's children 
        /// and clears all DynamicBone collider lists.
        /// </summary>    
        void DestroyAllDynamicBoneColliders(GameObject from)
        {
            List<DynamicBoneColliderBase> cl = new List<DynamicBoneColliderBase>();
            cl.AddRange(from.GetComponentsInChildren<DynamicBoneColliderBase>());

            foreach(var c in cl)
            {
                DestroyImmediate(c);
            }

            List<DynamicBone> dl = new List<DynamicBone>();
            dl.AddRange(from.GetComponentsInChildren<DynamicBone>());

            foreach(var d in dl)
            {
                if(d.m_Colliders != null)
                    d.m_Colliders.Clear();
            }
        }

        #endregion

        #region Misc Functions

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

        #endregion
    }

    #region Strings Struct

    public struct AvatarToolsStrings
    {
        static readonly Dictionary<string, string> dictionary_english, dictionary_uwu;
        public enum DictionaryLanguage { English, uwu = 100 };
        public static readonly string version = "0.4b";

        static Dictionary<string, string> stringDictionary;
        static public DictionaryLanguage Language { get; private set; }

        static AvatarToolsStrings()
        {
            dictionary_uwu = new Dictionary<string, string>
            {
                {"main_title", "Pumkin's Avataw Toows! ÒwÓ" },
                {"main_windowName", "Avataw Toows" },
                {"main_msgDefault", "Pick Objects to copy Componyents to and fwom." },
                {"main_version", "Vewsion" },

                //UI Main
                {"ui_copyFrom", "Copy fwom:" },
                {"ui_copyTo", "Copy to:" },
                {"button_copySelected" , "Copy sewected (´• ω •`)" },
                {"button_swap" , "Swap (ﾟωﾟ;)" },
                {"button_clear" , "Cweaw（>﹏<）" },

                //UI Transforms
                {"ui_transforms", "Twansfowms! º^º" },
                {"ui_transforms_position", "Position~" },
                {"ui_transforms_rotation", "Wotation~" },
                {"ui_transforms_scale", "Scawe~" },
            
                //UI Dynamic Bones
                {"ui_dynamicBones", "Dynyamic Bonyes +w+" },
                {"ui_dynamicBones_settings", "Settings~" },
                {"ui_dynamicBones_colliders", "Cowwidews~" },
                {"ui_dynamicBones_removeOld", "Wemuv Owd Bonyes~" },
                {"ui_dynamicBones_removeOldColliders", "Wemuv Owd Cowwidews~" },
                {"ui_dynamicBones_createMissing", "Cweate Missing Bonyes~" },

                //UI Colliders
                {"ui_colliders", "Cowwidews! >w<" },
                {"ui_colliders_box", "Box Cowwidews! :o" },
                {"ui_colliders_capsule", "Capsule Cowwidews! :0" },
                {"ui_colliders_sphere", "Sphere Cowwidews! :O" },
                {"ui_colliders_mesh", "Mesh Cowwidews! :C" },
                {"ui_colliders_removeOld", "Wemuv Owd Cowwidews" },

                //UI Avatar Descriptor
                {"ui_descriptor", "Avataw Descwiptow! =w=" },
                {"ui_descriptor_settings", "Settings~" },
                {"ui_descriptor_pipelineId", "Pipewinye Id uwu" },
                {"ui_descriptor_animationOverrides", "Anyimation Ovewwides!" },

                //UI Skinned Mesh Renderer
                {"ui_skinMeshRender", "Skinnyed Mesh Wendewews o-o" },
                {"ui_skinMeshRender_settings", "Settings agen!" },
                {"ui_skinMeshRender_materials", "Matewials owo" },
                {"ui_skinMeshRender_blendShapeValues", "BwendShape Vawues (⁰д⁰ )" },
                {"ui_skinMeshRender_resetBlendShapes", "Weset BwendShapes òwó" }, 

                //Log
                { "log_actionAttempt", "Attempting to copy {0} fwom {1} to {2} OwO" },
                { "log_copyToAndFromInvalid", "Can't copy Componyents because 'Copy Fwom' & 'Copy To' awe invawid~" },
                { "log_copyFwomInvalid" , "Can't copy Componyents because 'Copy Fwom' is invawid~" },
                { "log_copyToInvalid" , "Can't copy Componyents because 'Copy To' is invawid~" },
                { "log_done" , "Donye. Check Unyity Consowe fow fuww Output Wog uwu" },                

                //Warnings
                { "warn_warning", "O no~" },
                { "warn_copyToPrefab", "You awe twying to copy componyents to a pwefab!\nThis cannyot be undonye.\nAwe you suwe you want to continyue uwu?" },
                { "warn_pwefabOverwriteYes", "Mhm, uwu" },
                { "warn_pwefabOverwriteNyo", "Nyo, ;w;" },

                //Cwedits
                { "credits_line1", "Pumkin's Avataw Toows~"},
                { "credits_line2", "Vewsion" + " " + version },
                { "credits_line3", "Nyow with 100% mowe wedundant stwings~"},
                { "credits_line4", "I'ww add mowe stuff to this eventuawwy >w<" },
                { "credits_line5", "Poke me! But on Discowd at Pumkin#2020~ uwus" },

                //Misc
                { "misc_poke", "Poke me~" },
                { "misc_uwu", "OwO" },
            };
            dictionary_english = new Dictionary<string, string>
            {
                //Main
                {"main_title", "Pumkin's Avatar Tools" },
                {"main_windowName", "Avatar Tools" },
                {"main_msgDefault", "Pick Objects to copy Components to and from." },
                {"main_version", "Version" },

                //UI Main
                {"ui_copyFrom", "Copy from:" },
                {"ui_copyTo", "Copy to:" },                
                {"button_copySelected" , "Copy Selected" },
                {"button_swap" , "Swap" },
                {"button_clear" , "Clear" },

                //UI Transforms
                {"ui_transforms", "Transforms" },
                {"ui_transforms_position", "Position" },
                {"ui_transforms_rotation", "Rotation" },
                {"ui_transforms_scale", "Scale" },
            
                //UI Dynamic Bones
                {"ui_dynamicBones", "Dynamic Bones" },
                {"ui_dynamicBones_settings", "Settings" },
                {"ui_dynamicBones_colliders", "Colliders" },
                {"ui_dynamicBones_removeOld", "Remove Old Bones" },
                {"ui_dynamicBones_removeOldColliders", "Remove Old Colliders" },
                {"ui_dynamicBones_createMissing", "Create Missing Bones" },

                //UI Colliders
                {"ui_colliders", "Colliders" },
                {"ui_colliders_box", "Box Colliders" },
                {"ui_colliders_capsule", "Capsule Colliders" },
                {"ui_colliders_sphere", "Sphere Colliders" },
                {"ui_colliders_mesh", "Mesh Colliders" },
                {"ui_colliders_removeOld", "Remove Old Colliders" },

                //UI Avatar Descriptor
                {"ui_descriptor", "Avatar Descriptor" },
                {"ui_descriptor_settings", "Settings" },
                {"ui_descriptor_pipelineId", "Pipeline Id" },
                {"ui_descriptor_animationOverrides", "Animation Overrides" },

                //UI Skinned Mesh Renderer
                {"ui_skinMeshRender", "Skinned Mesh Renderers" },
                {"ui_skinMeshRender_settings", "Settings" },
                {"ui_skinMeshRender_materials", "Materials" },
                {"ui_skinMeshRender_blendShapeValues", "BlendShape Values" },
                {"ui_skinMeshRender_resetBlendShapes", "Reset BlendShapes" },

                //Log
                { "log_actionAttempt", "Attempting to copy {0} from {1} to {2}" },
                { "log_copyToAndFromInvalid", "Can't copy Components because 'Copy From' & 'Copy To' are invalid" },
                { "log_copyFromInvalid" , "Can't copy Components because 'Copy From' is invalid" },
                { "log_copyToInvalid" , "Can't copy Components because 'Copy To' is invalid" },
                { "log_done" , "Done. Check Unity Console for full Output Log" },
                { "log_cantCopyToItself", "Can't copy Components from an object to itself. What are you doing?" },
                { "log_noComponentsSelected", "No components selected" },

                //Warnings
                { "warn_warning", "Warning" },
                { "warn_copyToPrefab", "You are trying to copy components to a prefab.\nThis cannot be undone.\nAre you sure you want to continue?" },
                { "warn_prefabOverwriteYes", "Yes, Overwrite" },
                { "warn_prefabOverwriteNo", "No, Cancel" },

                //Credits
                { "credits_line1", "Pumkin's Avatar Tools"},
                { "credits_line2", "Version" + " " + version },
                { "credits_line3", "Now with 100% more redundant strings"},
                { "credits_line4", "I'll add more stuff to this eventually" },
                { "credits_line5", "Poke me on Discord at Pumkin#2020" },

                //Misc                
                { "misc_uwu", "uwu" },
            };

            Language = DictionaryLanguage.English;
            stringDictionary = dictionary_english;

            SetLanguage(DictionaryLanguage.English);
        }

        public static void SetLanguage(DictionaryLanguage lang)
        {
            switch(lang)
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
            Language = lang;
            PumkinsAvatarTools.ReloadStrings();
        }

        public static string GetString(string stringName, params string[] formatArgs)
        {
            if(string.IsNullOrEmpty(stringName))
                return stringName;

            string s = string.Empty;
            stringDictionary.TryGetValue(stringName, out s);

            if(formatArgs.Length > 0)
            {
                if(!string.IsNullOrEmpty(s))
                {
                    s = string.Format(stringName, formatArgs);
                }
            }
            return s;
        }
    };
    #endregion
}
