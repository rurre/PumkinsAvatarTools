using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Pumkin.AvatarTools;
using Pumkin.DataStructures;
using Pumkin.HelperFunctions;
using Pumkin.Presets;
using System;

namespace Pumkin.PoseEditor
{
    public class PumkinsMuscleEditor : EditorWindow
    {
        static bool windowIsFocused = false;
        static HumanPose avatarPose;
        static HumanPoseHandler avatarPoseHandler;

        float sliderRange = 2f;
        int toolbarSelection = 0;
        Vector2 scroll = Vector2.zero;

        static Animator _avatarAnimator;

        static bool _drawSkeleton = true;

        static bool _openedSettings = false;

        static List<PoseEditorBone> bones = new List<PoseEditorBone>();

        public static Animator AvatarAnimator
        {
            get
            {
                if(!_avatarAnimator && PumkinsAvatarTools.SelectedAvatar)
                    _avatarAnimator = PumkinsAvatarTools.SelectedAvatar.GetComponent<Animator>();
                return _avatarAnimator;
            }
            private set
            {
                _avatarAnimator = value;
            }
        }

        public static bool DrawSkeleton
        {
            get
            {
                return windowIsFocused == true ? _drawSkeleton : false;
            }
            set
            {
                _drawSkeleton = value;
            }
        }

        [MenuItem("Tools/Pumkin/Muscle Editor", false, 20)]
        public static void ShowWindow()
        {
            //Show existing window instance. If one doesn't exist, make one.
            EditorWindow editorWindow = EditorWindow.GetWindow(typeof(PumkinsMuscleEditor));
            editorWindow.autoRepaintOnSceneChange = true;

            editorWindow.Show();
            editorWindow.titleContent = new GUIContent("Pose Editor");

            HandleAvatarSelectionChanged(PumkinsAvatarTools.SelectedAvatar);
        }

        private void OnFocus()
        {
            windowIsFocused = true;
        }

        private void OnLostFocus()
        {
            windowIsFocused = false;
        }

        private void OnEnable()
        {
            //Prevent duplicates when recompiling
            PumkinsAvatarTools.AvatarSelectionChanged -= HandleAvatarSelectionChanged;
            PumkinsAvatarTools.PoseChanged -= HandlePoseChange;
            SceneView.onSceneGUIDelegate -= DrawHandlesGUI;

            PumkinsAvatarTools.AvatarSelectionChanged += HandleAvatarSelectionChanged;
            PumkinsAvatarTools.PoseChanged += HandlePoseChange;

            SceneView.onSceneGUIDelegate += DrawHandlesGUI;
        }        

        private void OnDisable()
        {
            PumkinsAvatarTools.AvatarSelectionChanged -= HandleAvatarSelectionChanged;
            PumkinsAvatarTools.PoseChanged -= HandlePoseChange;

            SceneView.onSceneGUIDelegate -= DrawHandlesGUI;
        }

        static void HandleAvatarSelectionChanged(GameObject newAvatar)
        {
            ReloadPoseVariables(newAvatar);
        }

        private static void ReloadPoseVariables(GameObject newAvatar)
        {            
            if(newAvatar)
                _avatarAnimator = newAvatar.GetComponent<Animator>();
            else
                _avatarAnimator = null;

            if(bones == null)
                bones = new List<PoseEditorBone>();
            bones.Clear();

            if(newAvatar && AvatarAnimator && AvatarAnimator.isHuman)
            {
                avatarPose = new HumanPose();
                avatarPoseHandler = new HumanPoseHandler(AvatarAnimator.avatar, AvatarAnimator.transform);
                avatarPoseHandler.GetHumanPose(ref avatarPose);

                //Human bone transforms to compare against
                HashSet<Transform> humanBoneTransforms = new HashSet<Transform>();                
                for(int i = 0; i < HumanTrait.BoneCount; i++)
                    humanBoneTransforms.Add(AvatarAnimator.GetBoneTransform((HumanBodyBones)i));

                //Get bone human bone root and tip positions
                for(int i = 0; i < HumanTrait.BoneCount; i++)
                {                    
                    Transform bone = AvatarAnimator.GetBoneTransform((HumanBodyBones)i);
                    if(!bone || bone.childCount == 0)
                        continue;
                    for(int j = 0; j < bone.childCount; j++)
                    {
                        Transform child = bone.GetChild(j);
                        if(humanBoneTransforms.Contains(child))
                            bones.Add(new PoseEditorBone(bone, child));
                    }
                }
            }
        }

        static void HandlePoseChange(PumkinsAvatarTools.PoseChangeType changeType)
        {
            switch(changeType)
            {
                case PumkinsAvatarTools.PoseChangeType.PoseEditor:
                    break;
                case PumkinsAvatarTools.PoseChangeType.Normal:
                case PumkinsAvatarTools.PoseChangeType.Reset:
                default:
                    ReloadPoseVariables(PumkinsAvatarTools.SelectedAvatar);
                    break;
            }
        }        

        void DrawHandlesGUI(SceneView sceneView)
        {
            if(DrawSkeleton && bones.Count > 0)
            {
                PumkinsMuscleEditorUtils.DrawBones(bones);
            }
        }       

        public void OnGUI()
        {

            int tempSize = Styles.Label_mainTitle.fontSize + 6;
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField(Strings.PoseEditor.title, Styles.Label_mainTitle, GUILayout.MinHeight(tempSize));                

                if(GUILayout.Button(Icons.Settings, Styles.IconButton))                
                    _openedSettings = !_openedSettings;
                
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField(Strings.PoseEditor.version);
            EditorGUILayout.Space();
            
            EditorGUI.BeginChangeCheck();
            {
                PumkinsAvatarTools.DrawAvatarSelectionWithButton();
            }           

            //if(GUILayout.Button("Dump muscle names"))
            //{            
            //    string s = "";
            //    foreach(string m in HumanTrait.MuscleName)
            //    {
            //        s += "\"" + m + "\",\n";
            //    }
            //    Debug.Log(s);
            //}

            Helpers.DrawGUILine();

            if(_openedSettings)
                DrawSettingsGUI();
            else
                DrawMainGUI();            
        }

        private void DrawSettingsGUI()
        {
            DrawSkeleton = GUILayout.Toggle(DrawSkeleton, "_Show Skeleton");
        }

        private void DrawMainGUI()
        {
            if(!PumkinsAvatarTools.SelectedAvatar || !PumkinsAvatarTools.SelectedAvatarIsHuman)
            {
                EditorGUILayout.LabelField(Strings.PoseEditor.selectHumanoidAvatar, Styles.HelpBox_OneLine);
                Helpers.DrawGUILine();
                return;
            }

            toolbarSelection = GUILayout.Toolbar(toolbarSelection, new string[] { "Body", "Head", "Arms", "Legs", "Fingers" });

            EditorGUILayout.Space();

            scroll = GUILayout.BeginScrollView(scroll);
            {
                List<Vector2Int> ranges = new List<Vector2Int>();

                switch(toolbarSelection)
                {
                    case 0:
                        ranges.Add(PumkinsMuscleDefinitions.bodyRange);
                        break;
                    case 1:
                        ranges.Add(PumkinsMuscleDefinitions.headRange);
                        break;
                    case 2:
                        ranges.Add(PumkinsMuscleDefinitions.leftArmRange);
                        ranges.Add(PumkinsMuscleDefinitions.rightArmRange);
                        break;
                    case 3:
                        ranges.Add(PumkinsMuscleDefinitions.leftLegRange);
                        ranges.Add(PumkinsMuscleDefinitions.rightLegRange);
                        break;
                    case 4:
                        ranges.Add(PumkinsMuscleDefinitions.leftFingersRange);
                        ranges.Add(PumkinsMuscleDefinitions.rightFingersRange);
                        break;
                    default:
                        ranges.Add(new Vector2Int(0, HumanTrait.MuscleCount));
                        break;
                }


                EditorGUI.BeginChangeCheck();
                {
                    for(int i = 0; i < ranges.Count; i++)
                    {
                        Vector2Int range = ranges[i];
                        for(int j = range.x; j < range.y; j++)
                            avatarPose.muscles[j] = EditorGUILayout.Slider(new GUIContent(HumanTrait.MuscleName[j]), avatarPose.muscles[j], -sliderRange, sliderRange);
                        if(i != ranges.Count - 1)
                            EditorGUILayout.Space();
                    }
                }
                if(EditorGUI.EndChangeCheck())
                {
                    if(PumkinsAvatarTools.SelectedAvatar)
                    {
                        if(Mathf.Approximately(avatarPose.bodyPosition.y, 0))
                            avatarPose.bodyPosition.y = 1;

                        Undo.RegisterCompleteObjectUndo(PumkinsAvatarTools.SelectedAvatar, "Pose Editor: Set pose from sliders");

                        avatarPoseHandler.SetHumanPose(ref avatarPose);
                    }
                }
                Helpers.DrawGUILine();

                PumkinsAvatarTools.Instance.DrawPresetGUI<PumkinsPosePreset>();
            }
            GUILayout.EndScrollView();
        }
    }
}