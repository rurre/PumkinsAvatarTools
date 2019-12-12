using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using System;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Pumkin.AvatarTools;
using Pumkin.DataStructures;
using Pumkin.HelperFunctions;
using Pumkin.Presets;
using static Pumkin.AvatarTools.PumkinsAvatarTools;

namespace Pumkin.PoseEditor
{
    public class PumkinsPoseEditor : EditorWindow
    {
        public static readonly string blendshapeExtension = "shapes";
        public static readonly string sceneExtension = "unity";
        public static readonly string poseExtension = "pose";

        static GameObject selectedAvatar;

        string _posePresetName, _scenePresetName, _blendshapePresetName;

        bool overwriteExisting = true;

        bool useHumanPoses = false;

        bool _scene_expand = true;
        bool _blendshape_expand = true;
        bool _pose_expand = true;

        bool positionOverridesPose = true;
        bool positionOverridesShapes = true;
        bool overrideLights = true;

        bool poseOnlySaveChangedRotations = false;

        Vector2 _mainScroll = Vector2.zero;

        int selectedPoseIndex = 0;
        int selectedHumanPoseIndex = 0;
        int selectedShapeIndex = 0;
        int selectedSceneIndex = 0;
        int selectedPositionIndex = 0;
                
        static List<PumkinsPosePreset> _defaultPoses;
        public static List<PumkinsPosePreset> DefaultPoses
        {
            get
            {
                if(_defaultPoses == null || _defaultPoses.Count == 0)
                {
                    PumkinsPresetManager.CleanupPresetsOfType<PumkinsPosePreset>();
                    _defaultPoses = new List<PumkinsPosePreset>()
                    {
                        PumkinsPosePreset.CreatePreset("TPose", new float[]
                            {
                                -6.830189E-07f, 4.268869E-08f, 4.268868E-08f, -8.537737E-08f, 0f, 0f, 0f, 0f, 0f, 4.268868E-08f, 8.537737E-08f, 4.268868E-08f, 3.415095E-07f, 0f, 0f, 0f, 0f,
                                0f, 0f, 0f, 0f, 0.5994893f, 0.004985309f, 0.00297395f, 0.9989594f, -0.02284526f, -3.449878E-05f, -0.0015229f, -4.781132E-07f, 0.599489f, 0.004985378f, 0.002974245f,
                                0.9989589f, -0.02284535f, -3.548912E-05f, -0.001522672f, -1.024528E-07f, -2.429621E-07f, 1.549688E-07f, 0.3847253f, 0.310061f, -0.103543f, 0.9925866f, 0.159403f,
                                -0.01539368f, 0.01405432f, 5.680533E-08f, 2.701393E-07f, 0.3847259f, 0.3100605f, -0.1035404f, 0.9925874f, 0.1593992f, -0.01539393f, 0.01405326f, -0.7706841f, 0.423209f,
                                0.6456643f, 0.6362566f, 0.6677276f, -0.4597229f, 0.811684f, 0.8116837f, 0.6683907f, -0.5737826f, 0.8116839f, 0.8116843f, 0.6670681f, -0.6459302f, 0.8116837f, 0.8116839f,
                                0.666789f, -0.4676594f, 0.811684f, 0.8116839f, -0.7706831f, 0.4232127f, 0.6456538f, 0.6362569f, 0.6678051f, -0.4589976f, 0.8116843f, 0.8116842f, 0.668391f, -0.5737844f,
                                0.811684f, 0.8116837f, 0.6669571f, -0.6492739f, 0.8116841f, 0.8116843f, 0.6667888f, -0.4676568f, 0.8116842f, 0.8116836f,
                            }),
                        PumkinsPosePreset.CreatePreset("Idle", new float[]
                            {

                            }),
                    };
                }
                return _defaultPoses;
            }
        }

        static List<PosePreset> poses;
        static List<PumkinsPosePreset> humanPoses = new List<PumkinsPosePreset>();        
        static List<BlendshapePreset> shapes;
        static List<string> scenePaths;

        static List<PumkinsPoseEditorPosition> scenePositions;
        static List<PumkinsPoseEditorPosition> addedScenePositions;

        static Dictionary<Light, bool> addedSceneLightStates;
        static Dictionary<Light, bool> sceneLightStates;
                
        Scene addedScene;        

        public static string PosePresetPath
        {            
            get { return PumkinsAvatarTools.MainScriptPath + "/../../Resources/Presets/Poses/"; }
        }

        public static string BlendshapesPresetPath
        {
            get { return PumkinsAvatarTools.MainScriptPath + "/../../Resources/Presets/Blendshapes/"; }
        }

        public static string ScenePresetPath
        {
            get { return PumkinsAvatarTools.MainScriptPath + "/../../Resources/Presets/Scenes/"; }
        }

        public Scene ThisScene
        {
            get { return EditorSceneManager.GetSceneAt(0); }
        }

        public bool InPlaymode
        {
            get { return EditorApplication.isPlaying; }
        }

        //[MenuItem("Tools/Pumkin/Pose Editor")]
        public static void ShowWindow()
        {
            //Show existing window instance. If one doesn't exist, make one.
            EditorWindow editorWindow = EditorWindow.GetWindow(typeof(PumkinsPoseEditor));
            editorWindow.autoRepaintOnSceneChange = true;

            editorWindow.Show();
            editorWindow.titleContent = new GUIContent("Pose Editor");            
        }
         
        private void OnEnable()
        {
            selectedAvatar = PumkinsAvatarTools.SelectedAvatar;           

            CheckFolders();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField(Strings.PoseEditor.title, Styles.Label_mainTitle, GUILayout.MinHeight(Styles.Label_mainTitle.fontSize + 6));
            EditorGUILayout.Space();

            selectedAvatar = (GameObject)EditorGUILayout.ObjectField(Strings.Main.avatar, selectedAvatar, typeof(GameObject), true);

            if(GUILayout.Button(Strings.Buttons.selectFromScene))
            {
                if(Selection.activeObject != null)
                {
                    try { selectedAvatar = Selection.activeGameObject.transform.root.gameObject; } catch {}
                }
            }

            if(selectedAvatar != null && selectedAvatar.gameObject.scene.name == null)
            {
                PumkinsAvatarTools.Log(Strings.Warning.selectSceneObject, LogType.Warning);
                selectedAvatar = null;
            }

            EditorGUILayout.Space();

            _mainScroll = EditorGUILayout.BeginScrollView(_mainScroll);

            //===========================================================================
            //Scene======================================================================
            //===========================================================================

            if(_scene_expand = GUILayout.Toggle(_scene_expand, Strings.PoseEditor.scene, Styles.Foldout_title))
            {
                EditorGUI.BeginDisabledGroup(!selectedAvatar);
                {
                    EditorGUILayout.Space();                    
                    
                    /*EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                    {
                        //_scenePresetName = EditorGUILayout.TextField("_Scene Preset" + ":", _scenePresetName);

                        //EditorGUILayout.Space();

                        //if(GUILayout.Button("_Save", GUILayout.ExpandWidth(true)))
                        //{

                        //}
                    }
                    EditorGUILayout.EndHorizontal();
                    */

                    EditorGUI.BeginChangeCheck();
                    {
                        selectedSceneIndex = EditorGUILayout.Popup(Strings.PoseEditor.sceneLoadAdditive + ":", selectedSceneIndex, scenePaths.Select(x => Path.GetFileNameWithoutExtension(x)).ToArray());
                    }
                    if(EditorGUI.EndChangeCheck() && shapes.Count > 0)
                    {   
                        if(InPlaymode)
                        {
                            if(addedScene.isLoaded)
                                SceneManager.UnloadSceneAsync(addedScene);

                            var guid = EditorBuildSettings.scenes.ToList().Find(x => x.path.Replace('/', '\\').ToLower() == scenePaths[selectedSceneIndex].Replace('/', '\\').ToLower()).guid;

                            //SceneManager.LoadScene(scenePaths[selectedSceneIndex], LoadSceneMode.Additive);
                            var path = AssetDatabase.GUIDToAssetPath(guid.ToString());
                            SceneManager.LoadScene(path, LoadSceneMode.Additive);
                            for(int i = 0; i < SceneManager.sceneCount; i++)
                            {
                                if(SceneManager.GetSceneAt(i).path == path)
                                {
                                    addedScene = SceneManager.GetSceneAt(i);
                                    break;
                                }
                            }

                            EditorCoroutine.Start(GetSceneAndSetupNextFrame_co(path));
                        }
                        else
                        {
                            EditorSceneManager.CloseScene(addedScene, true);
                            addedScene = EditorSceneManager.OpenScene(scenePaths[selectedSceneIndex], OpenSceneMode.Additive);

                            if(addedScenePositions == null)
                                addedScenePositions = new List<PumkinsPoseEditorPosition>();

                            addedScenePositions.Clear();
                            addedScenePositions.AddRange(GameObject.FindObjectsOfType<PumkinsPoseEditorPosition>());

                            if(overrideLights)
                            {
                                sceneLightStates = SetSceneLightsDisabledReturnStates(ThisScene);
                            }
                            else
                            {
                                addedSceneLightStates = SetSceneLightsDisabledReturnStates(addedScene);
                            }
                        }
                    }

                    EditorGUI.BeginChangeCheck();
                    {
                        overrideLights = EditorGUILayout.Toggle(Strings.PoseEditor.sceneOverrideLights, overrideLights);
                    }
                    if(addedScene.isLoaded && EditorGUI.EndChangeCheck())
                    {
                        if(overrideLights)
                        {
                            sceneLightStates = SetSceneLightsDisabledReturnStates(ThisScene);
                            SetSceneLightsEnabled(addedScene, addedSceneLightStates);
                        }
                        else
                        {
                            addedSceneLightStates = SetSceneLightsDisabledReturnStates(addedScene);
                            SetSceneLightsEnabled(ThisScene, sceneLightStates);
                        }
                    }

                    EditorGUILayout.Space();

                    EditorGUI.BeginDisabledGroup(addedScenePositions == null || addedScenePositions.Count == 0);
                    {
                        EditorGUI.BeginChangeCheck();
                        {
                            var s = new string[0];

                            if(addedScenePositions != null)
                            {
                                s = addedScenePositions.Select(x => x.positionName).ToArray();
                            }                            

                            selectedPositionIndex = EditorGUILayout.Popup(Strings.PoseEditor.avatarPosition + ":", selectedPositionIndex, s);

                            positionOverridesPose = EditorGUILayout.Toggle(Strings.PoseEditor.avatarPositionOverridePose, positionOverridesPose);
                            positionOverridesShapes = EditorGUILayout.Toggle(Strings.PoseEditor.avatarPositionOverrideBlendshapes, positionOverridesShapes);

                            EditorGUILayout.Space();                            
                        }
                        if(EditorGUI.EndChangeCheck())
                        {
                            ApplyPosition(selectedAvatar, addedScenePositions[selectedPositionIndex], positionOverridesPose, positionOverridesShapes);
                        }
                    }
                    EditorGUI.EndDisabledGroup();

                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUI.BeginDisabledGroup(InPlaymode);
                        {
                            if(GUILayout.Button(Strings.PoseEditor.sceneSaveChanges))
                            {
                                EditorSceneManager.SaveScene(addedScene);
                            }
                        }
                        EditorGUI.EndDisabledGroup();

                        if(GUILayout.Button(Strings.PoseEditor.unloadScene, GUILayout.ExpandWidth(true)))
                        {
                            SetSceneLightsEnabled(ThisScene, sceneLightStates);

                            if(InPlaymode)
                            {
                                SceneManager.UnloadSceneAsync(addedScene);
                            }
                            else
                            {
                                EditorSceneManager.CloseScene(addedScene, true);
                            }

                            if(addedScenePositions != null)
                                addedScenePositions.Clear();
                            if(addedSceneLightStates != null)
                                addedSceneLightStates.Clear();
                            if(sceneLightStates != null)
                                sceneLightStates.Clear();
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.Space();

                    if(GUILayout.Button(Strings.PoseEditor.resetPosition))
                    {
                        selectedAvatar.transform.position = Vector3.zero;
                    }

                    /*if(GUILayout.Button("_Load Scene", GUILayout.ExpandWidth(true)))
                    {
                        EditorSceneManager.CloseScene(addedScene, true);

                        addedScene = EditorSceneManager.OpenScene(ScenePresetPath + _scenePresetName + ".unity", OpenSceneMode.Additive);
                    }
                    */                    

                    EditorGUILayout.Space();
                }
                EditorGUI.EndDisabledGroup();
            }

            Helpers.DrawGuiLine();
            EditorGUILayout.Space();

            //===========================================================================
            //Pose=======================================================================
            //===========================================================================

            if(_pose_expand = GUILayout.Toggle(_pose_expand, Strings.PoseEditor.pose, Styles.Foldout_title))
            {
                EditorGUI.BeginDisabledGroup(!selectedAvatar);
                {
                    EditorGUILayout.Space();
                    //_posePresetName = EditorGUILayout.TextField("_Pose Preset" + ":", _posePresetName);

                    EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                    {
                        _posePresetName = EditorGUILayout.TextField(Strings.PoseEditor.newPose, _posePresetName);
                        if(GUILayout.Button(Strings.PoseEditor.saveButton, GUILayout.ExpandWidth(true)))
                        {
                            if(string.IsNullOrEmpty(_posePresetName))
                            {
                                PumkinsAvatarTools.Log(Strings.Log.nameIsEmpty, LogType.Warning);
                            }
                            else
                            {
                                if(useHumanPoses)
                                {
                                    var anim = selectedAvatar.GetComponent<Animator>();
                                    HumanPoseHandler hph = new HumanPoseHandler(anim.avatar, selectedAvatar.transform);
                                    HumanPose hp = new HumanPose();

                                    hph.GetHumanPose(ref hp);

                                    PumkinsPosePreset newPose = PumkinsPosePreset.CreatePreset(_posePresetName, hp);

                                    int i = humanPoses.FindIndex(o => o != null && o.name.ToLower() == newPose.name);
                                    if(i != -1)
                                        humanPoses[i] = newPose;
                                    else
                                        humanPoses.Add(newPose);
                                }
                                else
                                {
                                    var ts = selectedAvatar.GetComponentsInChildren<Transform>();
                                    var settings = new Dictionary<string, SerialTransform>();

                                    foreach(var t in ts)
                                    {
                                        if(t.root != t && (!poseOnlySaveChangedRotations || !Helpers.TransformIsInDefaultPosition(t, true)))
                                            settings.Add(Helpers.GetGameObjectPath(t.gameObject), t);
                                    }

                                    var p = new PosePreset(_posePresetName, settings);
                                    p.SaveToFile(PosePresetPath, overwriteExisting);
                                }

                                CheckFolders();
                            }
                        }
                        if(GUILayout.Button("Save (new)"))
                        {
                            SaveCurrentPoseNew(selectedAvatar, _posePresetName);
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    poseOnlySaveChangedRotations = EditorGUILayout.Toggle(Strings.PoseEditor.onlySavePoseChanges, poseOnlySaveChangedRotations);

                    EditorGUILayout.Space();

                    EditorGUI.BeginChangeCheck();
                    {
                        selectedPoseIndex = EditorGUILayout.Popup(Strings.PoseEditor.loadPose + ":", selectedPoseIndex, poses.Select(x => x.poseName).ToArray());
                        //selectedHumanPoseIndex = EditorGUILayout.Popup(Strings.PoseEditor.LoadPose + ":", selectedHumanPoseIndex, humanPoses.Select(x => x.poseName).ToArray());
                    }
                    if(EditorGUI.EndChangeCheck() && poses.Count > 0)
                    {
                        ApplyPose(selectedAvatar, positionOverridesPose);
                        //ApplyHumanPose(selectedAvatar, humanPoses[selectedHumanPoseIndex].poseName);
                    }

                    EditorGUILayout.Space();

                    if(GUILayout.Button(Strings.Tools.resetPose))
                    {
                        Undo.RegisterFullObjectHierarchyUndo(selectedAvatar, Strings.Tools.resetPose);
                        if(!PumkinsAvatarTools.ResetPose(selectedAvatar))
                            SetTPose(selectedAvatar);
                    }

                    EditorGUILayout.Space();
                }
                EditorGUI.EndDisabledGroup();
            }

            Helpers.DrawGuiLine();
            EditorGUILayout.Space();

            //===========================================================================
            //Blendshapes================================================================
            //===========================================================================

            if(_blendshape_expand = GUILayout.Toggle(_blendshape_expand, Strings.PoseEditor.blendshapes, Styles.Foldout_title))
            {
                EditorGUI.BeginDisabledGroup(!selectedAvatar);
                {
                    EditorGUILayout.Space();                    

                    EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                    {
                        _blendshapePresetName = EditorGUILayout.TextField(Strings.PoseEditor.newPreset + ":", _blendshapePresetName);
                        if(GUILayout.Button(Strings.PoseEditor.saveButton, GUILayout.ExpandWidth(true)))
                        {
                            if(string.IsNullOrEmpty(_blendshapePresetName))
                            {
                                PumkinsAvatarTools.Log(Strings.Log.nameIsEmpty, LogType.Warning);
                            }
                            var renders = selectedAvatar.GetComponentsInChildren<SkinnedMeshRenderer>();

                            if(renders.Length > 0)
                            {
                                var shapeDict = new Dictionary<string, List<PumkinsBlendshape>>();

                                foreach(var r in renders)
                                {
                                    string renderPath = Helpers.GetGameObjectPath(r.gameObject);
                                    var shapeList = new List<PumkinsBlendshape>();

                                    for(int i = 0; i < r.sharedMesh.blendShapeCount; i++)
                                    {
                                        float weight = r.GetBlendShapeWeight(i);
                                        var b = new PumkinsBlendshape(r.sharedMesh.GetBlendShapeName(i), weight);

                                        if(weight > 0)
                                            shapeList.Add(b);
                                    }

                                    shapeDict.Add(renderPath, shapeList);
                                }

                                BlendshapePreset bp = new BlendshapePreset(_blendshapePresetName, shapeDict);

                                //PoseEditorSaveBlendshapesPopup.ShowWindow(bp);

                                bp.SaveToFile(BlendshapesPresetPath, overwriteExisting);

                                CheckFolders();                                                                
                            }
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.Space();

                    EditorGUI.BeginChangeCheck();
                    {
                        selectedShapeIndex = EditorGUILayout.Popup(Strings.PoseEditor.loadPreset + ":", selectedShapeIndex, shapes.Select(x => x.presetName).ToArray());
                    }
                    if(EditorGUI.EndChangeCheck() && shapes.Count > 0)
                    {
                        ApplyBlendshapes(selectedAvatar, positionOverridesShapes);
                    }

                    EditorGUILayout.Space();

                    EditorGUILayout.BeginHorizontal();
                    {
                        if(GUILayout.Button(Strings.Tools.zeroBlendshapes))
                        {
                            Undo.RegisterFullObjectHierarchyUndo(selectedAvatar, "Reset Blendshapes");
                            PumkinsAvatarTools.ResetBlendShapes(selectedAvatar, false);
                        }
                        if(GUILayout.Button(Strings.Tools.revertBlendshapes))
                        {
                            Undo.RegisterFullObjectHierarchyUndo(selectedAvatar, "Revert Blendshapes to Prefab");
                            PumkinsAvatarTools.ResetBlendShapes(selectedAvatar, true);
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.Space();
                }
                EditorGUI.EndDisabledGroup();
            }

            Helpers.DrawGuiLine();
            EditorGUILayout.Space();

            EditorGUI.BeginDisabledGroup(!selectedAvatar);
            {
                if(GUILayout.Button(Strings.PoseEditor.reloadButton))
                {
                    CheckFolders(true);                    
                }
                //if(GUILayout.Button("Extract TPose"))
                //{
                //    var tpose = poses.First(o => o.poseName.ToLower() == "tpose");
                                        
                //    string s = tpose.ToHardcodedString();
                //    Debug.Log(s);
                //}
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// Check folders for scenes, serialized poses and blendshape files
        /// </summary>
        public static void CheckFolders(bool forceRefresh = false)
        {
            var pFiles = Directory.GetFiles(PosePresetPath, "*." + poseExtension, SearchOption.AllDirectories);
            var sFiles = Directory.GetFiles(BlendshapesPresetPath, "*." + blendshapeExtension, SearchOption.AllDirectories);
            var scFiles = Directory.GetFiles(ScenePresetPath, "*." + sceneExtension, SearchOption.AllDirectories);


            if(poses == null)
                poses = new List<PosePreset>();
            if(shapes == null)
                shapes = new List<BlendshapePreset>();
            if(scenePaths == null)
                scenePaths = new List<string>();

            if(forceRefresh || (poses.Count != pFiles.Length))
            {
                poses.Clear();

                foreach(var s in pFiles)
                {
                    try
                    {
                        string json = File.ReadAllText(s);
                        if(!string.IsNullOrEmpty(json))
                        {
                            var pose = JsonConvert.DeserializeObject<PosePreset>(json);

                            if(pose != null)
                            {
                                poses.Add(pose);
                                Debug.LogFormat(Strings.Log.loadedPose, s);
                            }
                        }
                    }
                    catch(Exception e)
                    {
                        Debug.LogError(e.Message);
                        continue;
                    }
                }
            }
            if(forceRefresh || (shapes.Count != sFiles.Length))
            {
                shapes.Clear();

                foreach(var s in sFiles)
                {
                    try
                    {
                        string json = File.ReadAllText(s);
                        if(!string.IsNullOrEmpty(json))
                        {
                            var shape = JsonConvert.DeserializeObject<BlendshapePreset>(json);

                            if(shape != null)
                            {
                                shapes.Add(shape);
                                Debug.LogFormat(Strings.Log.loadedBlendshapePreset, s);
                            }
                        }
                    }
                    catch(Exception e)
                    {
                        Debug.LogError(e.Message);
                        continue;
                    }
                }
            }
            if(forceRefresh || (scenePaths.Count != scFiles.Length))
            {
                scenePaths = scFiles.ToList();
            }

            var sc = EditorBuildSettings.scenes.Select(x => x.path.Replace('\\', '/')).ToList();
            var scb = new List<EditorBuildSettingsScene>();

            foreach(var s in scFiles)
            {
                if(!sc.Contains(s.Replace('\\', '/')))
                    scb.Add(new EditorBuildSettingsScene(s, true));
            }
            if(scb.Count > 0)
                EditorBuildSettings.scenes = scb.ToArray();
        }

        /// <summary>
        /// Gets muscle values from a humanoid avatar and returns them as a string. Don't do this
        /// </summary>        
        static string GetHumanPoseValues(GameObject avatar)
        {
            var anim = avatar.GetComponent<Animator>();

            if(anim == null && !anim.isHuman)
                return null;

            HumanPoseHandler hph = new HumanPoseHandler(anim.avatar, avatar.transform);
            HumanPose hp = new HumanPose();
            hph.GetHumanPose(ref hp);

            string s = "";
            for(int i = 0; i < hp.muscles.Length; i++)
            {
                s += String.Format("humanPose.muscles[{0}] = {1}f;", i, hp.muscles[i]);
                s += "\n";
            }            
            return s;
        }

        public static void SetDefaultPoseByName(GameObject avatar, string poseName)
        {
            PumkinsPosePreset pose = DefaultPoses.Find(o => o.name.ToLower() == poseName.ToLower());

            if(poseName.ToLower() == "tpose")
                OnPoseWasChanged(PoseChangeType.Reset);            
            else
                OnPoseWasChanged(PoseChangeType.Normal);

            pose.ApplyPreset(avatar);
        }

        /// <summary>
        /// Sets hardcoded TPose.
        /// </summary>        
        public static void SetTPose(GameObject avatar)
        {
            Undo.RegisterFullObjectHierarchyUndo(avatar, "Set TPose");            
                        
            var anim = avatar.GetComponent<Animator>();

            if(anim.avatar && anim.avatar.isHuman)
            {
                Vector3 pos = avatar.transform.position;
                Quaternion rot = avatar.transform.rotation;

                avatar.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

                var humanPoseHandler = new HumanPoseHandler(anim.avatar, avatar.transform);

                var humanPose = new HumanPose();
                humanPoseHandler.GetHumanPose(ref humanPose);

                if(humanPose.bodyPosition.y < 1 && !Mathf.Approximately(humanPose.bodyPosition.y, 1))
                {
                    PumkinsAvatarTools.Log(Strings.PoseEditor.bodyPositionYTooSmall, LogType.Warning, humanPose.bodyPosition.y.ToString());
                    humanPose.bodyPosition.y = 1;
                }

                #region Hardcoded TPose Values
                humanPose.muscles[0] = -6.830189E-07f;
                humanPose.muscles[1] = 4.268869E-08f;
                humanPose.muscles[2] = 4.268868E-08f;
                humanPose.muscles[3] = -8.537737E-08f;
                humanPose.muscles[4] = 0f;
                humanPose.muscles[5] = 0f;
                humanPose.muscles[6] = 0f;
                humanPose.muscles[7] = 0f;
                humanPose.muscles[8] = 0f;
                humanPose.muscles[9] = 4.268868E-08f;
                humanPose.muscles[10] = -8.537737E-08f;
                humanPose.muscles[11] = 4.268868E-08f;
                humanPose.muscles[12] = 3.415095E-07f;
                humanPose.muscles[13] = 0f;
                humanPose.muscles[14] = 0f;
                humanPose.muscles[15] = 0f;
                humanPose.muscles[16] = 0f;
                humanPose.muscles[17] = 0f;
                humanPose.muscles[18] = 0f;
                humanPose.muscles[19] = 0f;
                humanPose.muscles[20] = 0f;
                humanPose.muscles[21] = 0.5994893f;
                humanPose.muscles[22] = 0.004985309f;
                humanPose.muscles[23] = 0.00297395f;
                humanPose.muscles[24] = 0.9989594f;
                humanPose.muscles[25] = -0.02284526f;
                humanPose.muscles[26] = -3.449878E-05f;
                humanPose.muscles[27] = -0.0015229f;
                humanPose.muscles[28] = -4.781132E-07f;
                humanPose.muscles[29] = 0.599489f;
                humanPose.muscles[30] = 0.004985378f;
                humanPose.muscles[31] = 0.002974245f;
                humanPose.muscles[32] = 0.9989589f;
                humanPose.muscles[33] = -0.02284535f;
                humanPose.muscles[34] = -3.548912E-05f;
                humanPose.muscles[35] = -0.001522672f;
                humanPose.muscles[36] = -1.024528E-07f;
                humanPose.muscles[37] = -2.429621E-07f;
                humanPose.muscles[38] = 1.549688E-07f;
                humanPose.muscles[39] = 0.3847253f;
                humanPose.muscles[40] = 0.310061f;
                humanPose.muscles[41] = -0.103543f;
                humanPose.muscles[42] = 0.9925866f;
                humanPose.muscles[43] = 0.159403f;
                humanPose.muscles[44] = -0.01539368f;
                humanPose.muscles[45] = 0.01405432f;
                humanPose.muscles[46] = 5.680533E-08f;
                humanPose.muscles[47] = 2.701393E-07f;
                humanPose.muscles[48] = 0.3847259f;
                humanPose.muscles[49] = 0.3100605f;
                humanPose.muscles[50] = -0.1035404f;
                humanPose.muscles[51] = 0.9925874f;
                humanPose.muscles[52] = 0.1593992f;
                humanPose.muscles[53] = -0.01539393f;
                humanPose.muscles[54] = 0.01405326f;
                humanPose.muscles[55] = -0.7706841f;
                humanPose.muscles[56] = 0.423209f;
                humanPose.muscles[57] = 0.6456643f;
                humanPose.muscles[58] = 0.6362566f;
                humanPose.muscles[59] = 0.6677276f;
                humanPose.muscles[60] = -0.4597229f;
                humanPose.muscles[61] = 0.811684f;
                humanPose.muscles[62] = 0.8116837f;
                humanPose.muscles[63] = 0.6683907f;
                humanPose.muscles[64] = -0.5737826f;
                humanPose.muscles[65] = 0.8116839f;
                humanPose.muscles[66] = 0.8116843f;
                humanPose.muscles[67] = 0.6670681f;
                humanPose.muscles[68] = -0.6459302f;
                humanPose.muscles[69] = 0.8116837f;
                humanPose.muscles[70] = 0.8116839f;
                humanPose.muscles[71] = 0.666789f;
                humanPose.muscles[72] = -0.4676594f;
                humanPose.muscles[73] = 0.811684f;
                humanPose.muscles[74] = 0.8116839f;
                humanPose.muscles[75] = -0.7706831f;
                humanPose.muscles[76] = 0.4232127f;
                humanPose.muscles[77] = 0.6456538f;
                humanPose.muscles[78] = 0.6362569f;
                humanPose.muscles[79] = 0.6678051f;
                humanPose.muscles[80] = -0.4589976f;
                humanPose.muscles[81] = 0.8116843f;
                humanPose.muscles[82] = 0.8116842f;
                humanPose.muscles[83] = 0.668391f;
                humanPose.muscles[84] = -0.5737844f;
                humanPose.muscles[85] = 0.811684f;
                humanPose.muscles[86] = 0.8116837f;
                humanPose.muscles[87] = 0.6669571f;
                humanPose.muscles[88] = -0.6492739f;
                humanPose.muscles[89] = 0.8116841f;
                humanPose.muscles[90] = 0.8116843f;
                humanPose.muscles[91] = 0.6667888f;
                humanPose.muscles[92] = -0.4676568f;
                humanPose.muscles[93] = 0.8116842f;
                humanPose.muscles[94] = 0.8116836f;
                #endregion

                humanPoseHandler.SetHumanPose(ref humanPose);
                avatar.transform.SetPositionAndRotation(pos, rot);

                OnPoseWasChanged(PoseChangeType.Reset);
            }
            else
            {
                PumkinsAvatarTools.Log(Strings.Log.cantSetTPoseNonHumanoid, LogType.Warning);
            }
        }

        void ApplyHumanPose(GameObject avatar, PumkinsPosePreset hp)
        {
            Undo.RegisterFullObjectHierarchyUndo(avatar, "Load Human Pose");
            hp.ApplyPreset(avatar);
            OnPoseWasChanged(PoseChangeType.Normal);
        }

        void ApplyHumanPose(GameObject avatar, string poseName)
        {
            if(poseName != null)
            {
                int i = humanPoses.FindIndex(x => x.name.ToLower() == poseName.ToLower());
                if(i != -1)
                    selectedHumanPoseIndex = i;
            }

            ApplyHumanPose(avatar, humanPoses[selectedHumanPoseIndex]);
        }

        void SaveCurrentPoseNew(GameObject avatar, string poseName)
        {
            Animator anim = avatar.GetComponent<Animator>();
            if(!anim || !anim.isHuman)
                return;

            PumkinsPosePreset pose = PumkinsPosePreset.CreatePreset(poseName, anim);
            pose.SavePreset(true);
        }

        /// <summary>
        /// Applies pose to avatar from file
        /// </summary>        
        /// <param name="overridePose">I don't remember what this does</param>
        /// <param name="overrideName">Name of pose to apply</param>
        void ApplyPose(GameObject avatar, bool overridePose, string overrideName = null)
        {
            Undo.RegisterFullObjectHierarchyUndo(avatar, "Load Pose");
            PumkinsAvatarTools.ResetPose(avatar);

            if(overridePose && overrideName != null)
            {
                int i = poses.FindIndex(x => x.poseName.ToLower() == overrideName.ToLower());
                if(i != -1)
                    selectedPoseIndex = i;
            }
                
            poses[selectedPoseIndex].ApplyPose(avatar);
        }

        /// <summary>
        /// Applies Blendshapes from file
        /// </summary>        
        /// <param name="overrideShapes">I don't remember what this does either</param>
        /// <param name="overrideName">Name of blendshape to apply</param>
        void ApplyBlendshapes(GameObject avatar, bool overrideShapes, string overrideName = null)
        {
            Undo.RegisterFullObjectHierarchyUndo(avatar, "Load Blendshapes");
            PumkinsAvatarTools.ResetBlendShapes(avatar, false);

            if(overrideShapes && overrideName != null)
            {
                int i = shapes.FindIndex(x => x.presetName.ToLower() == overrideName.ToLower());
                if(i != -1)
                    selectedShapeIndex = i;
            }

            shapes[selectedShapeIndex].ApplyBlendshapes(avatar);
        }

        /// <summary>
        /// Applies position to avatar, for when loading a scene
        /// </summary>        
        /// <param name="pos">Position to move to</param>
        /// <param name="applyPose">Should the pose associated with the position be applied?</param>
        /// <param name="applyBlendshapes">Should the blendshapes associated with the position be applied?</param>
        void ApplyPosition(GameObject avatar, PumkinsPoseEditorPosition pos, bool applyPose, bool applyBlendshapes)
        {
            if(avatar == null || pos == null)
                return;

            avatar.transform.position = pos.transform.position;

            if(applyPose && !string.IsNullOrEmpty(pos.poseOverrideName))            
                ApplyPose(avatar, applyPose, pos.poseOverrideName);            

            if(applyBlendshapes && !string.IsNullOrEmpty(pos.blendshapePresetOverrideName))
                ApplyBlendshapes(avatar, applyBlendshapes, pos.blendshapePresetOverrideName);
        }

        /// <summary>
        /// Toggles between overriding scene lights with additively loaded scene and saves old states, by scene id
        /// </summary>
        /// <param name="sceneIndex">Index of scene, should be given by scene manager</param>
        /// <param name="lightsStates">Old light states use to toggle back old lights</param>
        void SetSceneLightsEnabled(int sceneIndex, Dictionary<Light, bool> lightsStates)
        {
            Scene sc;
            if(InPlaymode)
                sc = SceneManager.GetSceneAt(sceneIndex);
            else
                sc = EditorSceneManager.GetSceneAt(sceneIndex);
            
            SetSceneLightsEnabled(sc, lightsStates);
        }

        /// <summary>
        /// Toggles between overriding scene lights with additively loaded scene and saves old states, by scene name
        /// </summary>
        /// <param name="scene">Scene name</param>
        /// <param name="lightStates">Old light states use to toggle back old lights</param>
        void SetSceneLightsEnabled(Scene scene, Dictionary<Light, bool> lightStates)
        {
            if(lightStates != null && lightStates.Count > 0)
            {
                foreach(var kv in lightStates)
                {
                    kv.Key.enabled = kv.Value;
                }
            }
        }

        /// <summary>
        /// Saves current light states and disables all lights
        /// </summary>
        /// <param name="sceneIndex">Index of scene, should be given by the scene manager</param>
        /// <returns>Light states, used for toggles. Useful in case lights are already disabled so we don't turn them on when toggling</returns>
        Dictionary<Light, bool> SetSceneLightsDisabledReturnStates(int sceneIndex)
        {
            Scene sc;
            if(InPlaymode)
                sc = SceneManager.GetSceneAt(sceneIndex);
            else
                sc = EditorSceneManager.GetSceneAt(sceneIndex);

            return SetSceneLightsDisabledReturnStates(sc);
        }

        /// <summary>
        /// Saves current light states and disables all lights
        /// </summary>
        /// <param name="scene">Name of scene</param>
        /// <returns>Light states, used for toggles. Useful in case lights are already disabled so we don't turn them on when toggling</returns>
        Dictionary<Light, bool> SetSceneLightsDisabledReturnStates(Scene scene)
        {
            if(!scene.isLoaded)
                return null;

            Dictionary<Light, bool> d = new Dictionary<Light, bool>();

            var objs = scene.GetRootGameObjects();

            foreach(var o in objs)
            {
                var l = o.GetComponentsInChildren<Light>();
                for(int i = 0; i < l.Length; i++)
                {                    
                    d.Add(l[i], l[i].enabled);
                    l[i].enabled = false;
                }
            }

            if(d.Count == 0)
                return null;
            else
                return d;
        }

        /// <summary>
        /// Gets scene then toggles lights if needed
        /// </summary>
        /// <param name="path">Path of scene</param>
        /// <returns>Nothing, but is needed to run the couroutine to load everything next frame</returns>
        IEnumerator GetSceneAndSetupNextFrame_co(string path)
        {
            yield return new WaitForEndOfFrame();
            addedScene = SceneManager.GetSceneByPath(path);

            if(addedScenePositions == null)
                addedScenePositions = new List<PumkinsPoseEditorPosition>();

            addedScenePositions.Clear();
            addedScenePositions.AddRange(GameObject.FindObjectsOfType<PumkinsPoseEditorPosition>());

            if(overrideLights)
            {
                sceneLightStates = SetSceneLightsDisabledReturnStates(ThisScene);
            }
            else
            {
                addedSceneLightStates = SetSceneLightsDisabledReturnStates(addedScene);
            }
        }
    }       
}