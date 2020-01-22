using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Pumkin.AvatarTools;
using Pumkin.DataStructures;
using Pumkin.HelperFunctions;
using Pumkin.Presets;
using UnityEditor.Animations;
using System;

namespace Pumkin.PoseEditor
{
    public class PumkinsMuscleEditor : EditorWindow
    {
        const string POSE_ANIMATOR_NAME = "PumkinsPoseEditorAnimator";
        
        //Main
        static bool windowIsFocused = false;
        static HumanPose avatarPose;
        static HumanPoseHandler avatarPoseHandler;
        Vector2 scroll = Vector2.zero;       

        //Toolbar and sliders
        float sliderRange = 2f;
        int toolbarSelection = 0;

        //Pose from animation                
        AnimationClip animClip;
        float animTimeCurrent = 0;
        AnimatorController ctrl;
        bool allowMotion = false;

        static Animator _avatarAnimator;        

        static bool _drawSkeleton = false;

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
                //return windowIsFocused == true ? _drawSkeleton : false;
                return _drawSkeleton;
            }
            set
            {
                _drawSkeleton = value;
            }
        }

        [MenuItem("Tools/Pumkin/Pose Editor", false, 20)]
        public static void ShowWindow()
        {
            //Show existing window instance. If one doesn't exist, make one.
            EditorWindow editorWindow = EditorWindow.GetWindow(typeof(PumkinsMuscleEditor));
            editorWindow.autoRepaintOnSceneChange = true;

            editorWindow.Show();
            editorWindow.titleContent = new GUIContent("Pose Editor");

            HandleAvatarSelectionChanged(PumkinsAvatarTools.SelectedAvatar);
        }

        private void Awake()
        {
            ctrl = Resources.Load<AnimatorController>("Pose Editor/" + POSE_ANIMATOR_NAME);
            if(!ctrl)            
                ctrl = AnimatorController.CreateAnimatorControllerAtPath(PumkinsAvatarTools.MainFolderPath + "Resources/Pose Editor" + POSE_ANIMATOR_NAME);            
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

        public void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField(Strings.PoseEditor.title, Styles.Label_mainTitle);

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

        static void HandleAvatarSelectionChanged(GameObject newAvatar)
        {
            ReloadPoseVariables(newAvatar);            
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
                PumkinsMuscleEditorUtils.DrawBones(bones);            
        }       

        private void DrawSettingsGUI()
        {
            _drawSkeleton = GUILayout.Toggle(_drawSkeleton, "_Show Skeleton");
        }

        private void DrawMainGUI()
        {
            if(!PumkinsAvatarTools.SelectedAvatar || !PumkinsAvatarTools.SelectedAvatarIsHuman)
            {
                EditorGUILayout.LabelField(Strings.PoseEditor.selectHumanoidAvatar, Styles.HelpBox_OneLine);
                Helpers.DrawGUILine();
                return;
            }

            DrawPoseFromAnimationGUI();

            //Draw the toolbar then get the muscle ranges based on it's selection
            toolbarSelection = GUILayout.Toolbar(toolbarSelection, new string[] { "Body", "Head", "Arms", "Legs", "Fingers" });

            EditorGUILayout.Space();

            scroll = GUILayout.BeginScrollView(scroll);
            {
                List<Vector2Int> muscleRanges = new List<Vector2Int>();

                switch(toolbarSelection)
                {
                    case 0:
                        muscleRanges.Add(PumkinsMuscleDefinitions.bodyRange);
                        break;
                    case 1:
                        muscleRanges.Add(PumkinsMuscleDefinitions.headRange);
                        break;
                    case 2:
                        muscleRanges.Add(PumkinsMuscleDefinitions.leftArmRange);
                        muscleRanges.Add(PumkinsMuscleDefinitions.rightArmRange);
                        break;
                    case 3:
                        muscleRanges.Add(PumkinsMuscleDefinitions.leftLegRange);
                        muscleRanges.Add(PumkinsMuscleDefinitions.rightLegRange);
                        break;
                    case 4:
                        muscleRanges.Add(PumkinsMuscleDefinitions.leftFingersRange);
                        muscleRanges.Add(PumkinsMuscleDefinitions.rightFingersRange);
                        break;
                    default:
                        muscleRanges.Add(new Vector2Int(0, HumanTrait.MuscleCount));
                        break;
                }

                //Draw sliders for the muscle ranges and apply the changes if they've changed
                EditorGUI.BeginChangeCheck();
                {
                    for(int i = 0; i < muscleRanges.Count; i++)
                    {
                        Vector2Int range = muscleRanges[i];
                        for(int j = range.x; j < range.y; j++)
                            avatarPose.muscles[j] = EditorGUILayout.Slider(new GUIContent(HumanTrait.MuscleName[j]), avatarPose.muscles[j], -sliderRange, sliderRange);
                        if(i != muscleRanges.Count - 1)
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

        private void DrawPoseFromAnimationGUI()
        {
            bool applyAnimation = false;            
            EditorGUI.BeginChangeCheck();
            {
                animClip = EditorGUILayout.ObjectField(Strings.PoseEditor.poseFromAnimation, animClip, typeof(AnimationClip), false) as AnimationClip;
            }
            if(EditorGUI.EndChangeCheck())
            {                
                animTimeCurrent = 0;
                applyAnimation = true;
            }

            if(animClip)
            {
                EditorGUI.BeginChangeCheck();
                {
                    animTimeCurrent = EditorGUILayout.Slider(Strings.PoseEditor.animationTime, animTimeCurrent, 0, animClip.length);
                }
                if(EditorGUI.EndChangeCheck() || applyAnimation)
                {                       
                    //Save transform position and rotation to prevent root motion
                    SerialTransform currentTrans = PumkinsAvatarTools.SelectedAvatar.transform;

#if UNITY_2017  //For some reason SampleAnimation refuses to work if there's no runtime animation controller set in unity 2017
                    var tempAnim = AvatarAnimator.runtimeAnimatorController;
                    AvatarAnimator.runtimeAnimatorController = ctrl;                    
#endif
                    animClip.SampleAnimation(PumkinsAvatarTools.SelectedAvatar, animTimeCurrent);

#if UNITY_2017  //I really don't like this but it seems to work
                    var serialTransforms = new List<SerialTransform>(); //Save all transform values after sampling animation to prevent reverting
                    var transforms = PumkinsAvatarTools.SelectedAvatar.GetComponentsInChildren<Transform>();

                    foreach(var t in transforms)                    
                        serialTransforms.Add(t);

                    AvatarAnimator.runtimeAnimatorController = tempAnim; //Apply old animator controller; this reverts pose to first frame of animation

                    for(int i = 0; i < transforms.Length; i++) //Restore pose based on transform rotations                   
                    {
                        transforms[i].localEulerAngles = serialTransforms[i].localEulerAngles;
                        if(allowMotion)
                            transforms[i].localPosition = serialTransforms[i].localPosition;
                    }
#endif
                    if(!allowMotion && currentTrans) //Restore position and rotation of avatar itself if no motion allowed
                        PumkinsAvatarTools.SelectedAvatar.transform.SetPositionAndRotation(currentTrans.position, currentTrans.rotation);                    

                   ReloadPoseVariables(PumkinsAvatarTools.SelectedAvatar);
                }

                allowMotion = GUILayout.Toggle(allowMotion, Strings.PoseEditor.allowMotion);
            }
            Helpers.DrawGUILine();
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
    }
}