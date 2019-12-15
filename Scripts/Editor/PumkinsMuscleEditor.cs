using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Pumkin.AvatarTools;
using Pumkin.DataStructures;
using Pumkin.HelperFunctions;
using Pumkin.Presets;

namespace Pumkin.PoseEditor
{
    public class PumkinsMuscleEditor : EditorWindow
    {
        static HumanPose avatarPose;
        static HumanPoseHandler avatarPoseHandler;

        float sliderRange = 2f;
        int toolbarSelection = 0;
        Vector2 scroll = Vector2.zero;

        static Animator AvatarAnimator
        {
            get { return PumkinsAvatarTools.SelectedAvatar.GetComponent<Animator>(); }
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

        private void OnEnable()
        {
            //Prevent duplicates when recompiling
            PumkinsAvatarTools.AvatarSelectionChanged -= HandleAvatarSelectionChanged;
            PumkinsAvatarTools.PoseChanged -= HandlePoseChange;

            PumkinsAvatarTools.AvatarSelectionChanged += HandleAvatarSelectionChanged;
            PumkinsAvatarTools.PoseChanged += HandlePoseChange;
        }

        private void OnDisable()
        {
            PumkinsAvatarTools.AvatarSelectionChanged -= HandleAvatarSelectionChanged;
            PumkinsAvatarTools.PoseChanged -= HandlePoseChange;
        }



        static void HandleAvatarSelectionChanged(GameObject newAvatar)
        {
            ReloadPoseVariables(newAvatar);
        }

        private static void ReloadPoseVariables(GameObject newAvatar)
        {
            if(newAvatar && AvatarAnimator && AvatarAnimator.isHuman)
            {
                avatarPose = new HumanPose();
                avatarPoseHandler = new HumanPoseHandler(AvatarAnimator.avatar, AvatarAnimator.transform);
                avatarPoseHandler.GetHumanPose(ref avatarPose);
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

        public void OnGUI()
        {
            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();
            {
                PumkinsAvatarTools.SelectedAvatar = EditorGUILayout.ObjectField(Strings.Main.avatar, PumkinsAvatarTools.SelectedAvatar, typeof(GameObject), true) as GameObject;
            }
            if(EditorGUI.EndChangeCheck())
            {
                HandleAvatarSelectionChanged(PumkinsAvatarTools.SelectedAvatar);
            }
            if(GUILayout.Button(Strings.Buttons.selectFromScene))
            {
                PumkinsAvatarTools.SelectAvatarFromScene();
                string s = "";
                foreach(string m in HumanTrait.MuscleName)
                {
                    s += "\"" + m + "\",\n";
                }
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

            Helpers.DrawGuiLine();

            if(!PumkinsAvatarTools.SelectedAvatar || !PumkinsAvatarTools.SelectedAvatarIsHuman)
                return;

            toolbarSelection = GUILayout.Toolbar(toolbarSelection, new string[] { "Body", "Head", "Arms", "Legs", "Fingers" });

            EditorGUILayout.Space();

            scroll = GUILayout.BeginScrollView(scroll);
            {
                Vector2Int range = Vector2Int.zero;

                switch(toolbarSelection)
                {
                    case 0:
                        range = PumkinsMuscleDefinitions.bodyRange;
                        break;
                    case 1:
                        range = PumkinsMuscleDefinitions.headRange;
                        break;
                    case 2:
                        range = PumkinsMuscleDefinitions.armsRange;
                        break;
                    case 3:
                        range = PumkinsMuscleDefinitions.legsRange;
                        break;
                    case 4:
                        range = PumkinsMuscleDefinitions.fingersRange;
                        break;
                    default:
                        range = new Vector2Int(0, HumanTrait.MuscleCount);
                        break;
                }


                EditorGUI.BeginChangeCheck();
                {
                    for(int i = range.x; i < range.y; i++)
                    {
                        avatarPose.muscles[i] = EditorGUILayout.Slider(new GUIContent(HumanTrait.MuscleName[i]), avatarPose.muscles[i], -sliderRange, sliderRange);
                    }
                }
                if(EditorGUI.EndChangeCheck())
                {
                    if(PumkinsAvatarTools.SelectedAvatar)
                    {
                        if(Mathf.Approximately(avatarPose.bodyPosition.y, 0))
                            avatarPose.bodyPosition.y = 1;

                        avatarPoseHandler.SetHumanPose(ref avatarPose);
                    }
                }
                Helpers.DrawGuiLine();

                PumkinsAvatarTools.Instance.DrawPresetGUI<PumkinsPosePreset>();
            }
            GUILayout.EndScrollView();
        }
    }
}