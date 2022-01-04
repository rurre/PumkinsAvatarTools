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
    public class PumkinsPoseEditor : EditorWindow
    {
        public enum PoseChangeType { Reset, Normal, PoseEditor }

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

        static List<PumkinsPosePreset> humanPoses = new List<PumkinsPosePreset>();

        public delegate void PoseChangedHandler(PoseChangeType changeType);
        public static event PoseChangedHandler PoseChanged;

        //Main
        //static bool windowIsFocused = false;
        static HumanPose avatarPose;
        static HumanPoseHandler avatarPoseHandler;
        Vector2 scroll = Vector2.zero;

        //Toolbar and sliders
        float sliderRange = 2f;
        int toolbarSelection = 0;

        //Pose from animation
        AnimationClip animClip;
        float animTimeCurrent = 0;
        bool playAnimation = false;
        public float animationSpeed = 0.03f;
        bool allowMotion = false;

        static Animator _avatarAnimator;

        static bool _drawSkeleton = false;

        static bool _openedSettings = false;

        static List<PoseEditorBone> bones = new List<PoseEditorBone>();

        EditorApplication.CallbackFunction updateCallback;

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

        [MenuItem("Pumkin/Tools/Pose Editor", false, 20)]
        public static void ShowWindow()
        {
            //Show existing window instance. If one doesn't exist, make one.
            EditorWindow editorWindow = EditorWindow.GetWindow(typeof(PumkinsPoseEditor));
            editorWindow.autoRepaintOnSceneChange = true;

            editorWindow.Show();
            editorWindow.titleContent = new GUIContent("Pose Editor");

            HandleAvatarSelectionChanged(PumkinsAvatarTools.SelectedAvatar);
        }

        private void Awake()
        {
        }

        private void OnFocus()
        {
            //windowIsFocused = true;
        }

        private void OnLostFocus()
        {
            //windowIsFocused = false;
        }

        private void OnEnable()
        {
            //Prevent duplicates when recompiling
            PumkinsAvatarTools.AvatarSelectionChanged -= HandleAvatarSelectionChanged;
            PoseChanged -= HandlePoseChange;
            SceneView.duringSceneGui -= DrawHandlesGUI;

            PumkinsAvatarTools.AvatarSelectionChanged += HandleAvatarSelectionChanged;
            PoseChanged += HandlePoseChange;

            SceneView.duringSceneGui += DrawHandlesGUI;

            if(updateCallback == null)
                updateCallback = OnUpdate;

            EditorApplication.update += updateCallback;
        }

        private void OnDisable()
        {
            PumkinsAvatarTools.AvatarSelectionChanged -= HandleAvatarSelectionChanged;
            PoseChanged -= HandlePoseChange;

            playAnimation = false;

            SceneView.duringSceneGui -= DrawHandlesGUI;
            EditorApplication.update -= updateCallback;
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
                PumkinsAvatarTools.DrawAvatarSelectionWithButtonGUI();
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

        public static void OnPoseWasChanged(PoseChangeType changeType)
        {
            //bool toggle = false;
            //var bonesToKeepDisabled = new List<DynamicBone>();
            //ToggleDynamicBonesEnabledState(SelectedAvatar, ref toggle, ref bonesToKeepDisabled);

            if(PoseChanged != null)
                PoseChanged.Invoke(changeType);

            //EditorApplication.delayCall += () => ToggleDynamicBonesEnabledState(SelectedAvatar, ref toggle, ref bonesToKeepDisabled);
            //PumkinsAvatarTools.RefreshDynamicBoneTransforms(SelectedAvatar);
            PumkinsAvatarTools.LogVerbose("Pose was changed and OnPoseWasChanged() was called with changeType as " + changeType.ToString());
        }

        static void HandleAvatarSelectionChanged(GameObject newAvatar)
        {
            ReloadPoseVariables(newAvatar);
        }

        static void HandlePoseChange(PoseChangeType changeType)
        {
            switch(changeType)
            {
                case PoseChangeType.PoseEditor:
                    break;
                case PoseChangeType.Normal:
                case PoseChangeType.Reset:
                default:
                    ReloadPoseVariables(PumkinsAvatarTools.SelectedAvatar);
                    break;
            }
        }

        void DrawHandlesGUI(SceneView sceneView)
        {
            if(DrawSkeleton && bones.Count > 0)
                PumkinsPoseEditorUtils.DrawBones(bones);
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
            toolbarSelection = GUILayout.Toolbar(toolbarSelection, new[] { "Body", "Head", "Arms", "Legs", "Fingers" });

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
                        if(PumkinsAvatarTools.Settings.posePresetTryFixSinking && avatarPose.bodyPosition.y > 0 && avatarPose.bodyPosition.y <= 0.01f)
                        {
                            PumkinsAvatarTools.Log(Strings.PoseEditor.bodyPositionYTooSmall, LogType.Warning, avatarPose.bodyPosition.y.ToString());
                            avatarPose.bodyPosition.y = 1;
                        }

                        Undo.RegisterCompleteObjectUndo(PumkinsAvatarTools.SelectedAvatar, "Pose Editor: Set pose from sliders");
                        avatarPoseHandler.SetHumanPose(ref avatarPose);
                    }
                }

                Helpers.DrawGUILine();

                PumkinsAvatarTools.Settings.posePresetApplyBodyPosition = GUILayout.Toggle(PumkinsAvatarTools.Settings.posePresetApplyBodyPosition, Strings.Thumbnails.applyBodyPosition);
                PumkinsAvatarTools.Settings.posePresetApplyBodyRotation = GUILayout.Toggle(PumkinsAvatarTools.Settings.posePresetApplyBodyRotation, Strings.Thumbnails.applyBodyRotation);

                EditorGUILayout.Space();

                PumkinsAvatarTools.Settings.posePresetTryFixSinking = GUILayout.Toggle(PumkinsAvatarTools.Settings.posePresetTryFixSinking, Strings.Thumbnails.tryFixPoseSinking);

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
                playAnimation = false;
            }

            if(animClip)
            {
                EditorGUI.BeginChangeCheck();
                {
                    animTimeCurrent = EditorGUILayout.Slider(Strings.PoseEditor.animationTime, animTimeCurrent, 0, animClip.length);
                }
                if(EditorGUI.EndChangeCheck() || applyAnimation)
                {
                    ApplyAnimation();
                }

                allowMotion = GUILayout.Toggle(allowMotion, Strings.PoseEditor.allowMotion);

                if(GUILayout.Button(playAnimation ? "Pause" : "Play"))
                    playAnimation = !playAnimation;
            }
            Helpers.DrawGUILine();
        }

        private void ApplyAnimation()
        {
            //Save transform position and rotation to prevent root motion
            SerialTransform currentTrans = PumkinsAvatarTools.SelectedAvatar.transform;

            animClip.SampleAnimation(PumkinsAvatarTools.SelectedAvatar, animTimeCurrent);

            if(currentTrans) //Restore position and rotation of avatar
                PumkinsAvatarTools.SelectedAvatar.transform.SetPositionAndRotation(currentTrans.position, currentTrans.rotation);

            ReloadPoseVariables(PumkinsAvatarTools.SelectedAvatar);
        }

        void OnUpdate()
        {
            if(playAnimation)
            {
                if(!animClip || animClip.length == 0)
                {
                    playAnimation = false;
                    return;
                }

                animTimeCurrent = Helpers.WrapToRange(animTimeCurrent + Time.deltaTime, 0, animClip.length);
                ApplyAnimation();
            }
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

                SerialTransform st = newAvatar.transform;
                Vector3 avatarPos;

                avatarPoseHandler = new HumanPoseHandler(AvatarAnimator.avatar, AvatarAnimator.transform);
                avatarPoseHandler.GetHumanPose(ref avatarPose);

                avatarPos = st.position;
                newAvatar.transform.position = avatarPos;

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

        public static void SetDefaultPoseByName(GameObject avatar, string poseName)
        {
            PumkinsPosePreset pose = DefaultPoses.Find(o => o.name.ToLower() == poseName.ToLower());

            if(poseName.ToLower() == "tpose")
                OnPoseWasChanged(PoseChangeType.Reset);
            else
                OnPoseWasChanged(PoseChangeType.Normal);

            pose.ApplyPreset(avatar);
        }

        public static void SetTPose(GameObject avatar)
        {
            //For some reason this doesn't behave the same as SetTPoseHardcoded
            //despite using the same values. TODO: Investigate this later.
            SetDefaultPoseByName(avatar, "tpose");
        }

        /// <summary>
        /// Sets hardcoded TPose.
        /// </summary>
        public static void SetTPoseHardcoded(GameObject avatar)
        {
            Undo.RegisterFullObjectHierarchyUndo(avatar, "Set TPose");

            var anim = avatar.GetComponent<Animator>();

            if(anim && anim.avatar && anim.avatar.isHuman)
            {
                Vector3 pos = avatar.transform.position;
                Quaternion rot = avatar.transform.rotation;

                avatar.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

                var humanPoseHandler = new HumanPoseHandler(anim.avatar, avatar.transform);

                var humanPose = new HumanPose();
                humanPoseHandler.GetHumanPose(ref humanPose);

                Transform armature = Helpers.GetAvatarArmature(avatar);
                if(!(armature && armature.localScale == Vector3.one))
                {
                    if(humanPose.bodyPosition.y < 1 && !Mathf.Approximately(humanPose.bodyPosition.y, 1))
                    {
                        humanPose.bodyPosition.y = 1;
                        PumkinsAvatarTools.LogVerbose(Strings.PoseEditor.bodyPositionYTooSmall, LogType.Warning, humanPose.bodyPosition.y.ToString());
                    }
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
                PumkinsAvatarTools.Log(Strings.Log.cantSetPoseNonHumanoid, LogType.Warning, "TPose");
            }
        }

        void ApplyHumanPose(GameObject avatar, PumkinsPosePreset hp)
        {
            Undo.RegisterFullObjectHierarchyUndo(avatar, "Load Human Pose");

            Vector3 pos = avatar.transform.position;
            Quaternion rot = avatar.transform.rotation;
            avatar.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

            hp.ApplyPreset(avatar);
            OnPoseWasChanged(PoseChangeType.Normal);
            avatar.transform.SetPositionAndRotation(pos, rot);
        }
    }
}
