using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Pumkin.DependencyChecker;

namespace Pumkin.AvatarTools
{
    [System.Serializable]
    public class _PumkinsAvatarToolsWindow : EditorWindow
    {
        [SerializeField, HideInInspector] static PumkinsAvatarTools tools;
                
        string[] boneErrors =
            {
            "The type or namespace name `DynamicBoneCollider' could not be found.",
            "The type or namespace name `DynamicBone' could not be found.",            
            "Cannot implicitly convert type `System.Collections.Generic.List<DynamicBoneCollider>' to `System.Collections.Generic.List<DynamicBoneColliderBase>'",
            };

        [MenuItem("Tools/Pumkin/Avatar Tools")]
        public static void ShowWindow()
        {
            //Show existing window instance. If one doesn't exist, make one.
            EditorWindow editorWindow = EditorWindow.GetWindow(typeof(_PumkinsAvatarToolsWindow));
            editorWindow.autoRepaintOnSceneChange = true;

            editorWindow.Show();
            editorWindow.titleContent = new GUIContent(Strings.Main.WindowName);
        }

        private void OnEnable()
        {
            Application.logMessageReceived -= HandleError;
            Application.logMessageReceived += HandleError;            
        }

        private void OnDisable()
        {
            Application.logMessageReceived -= HandleError;
        }

        void HandleError(string log, string stack, LogType type)
        {            
            if(type == LogType.Error)
            {
                for(int i = 0; i < boneErrors.Length; i++)
                {
                    if(log.Contains(boneErrors[i]))
                    {
                        _DependecyChecker.Status = _DependecyChecker.CheckerStatus.DEFAULT;
                        break;
                    }
                }
            }            
        }

        public void OnGUI()
        {
            if(tools == null)
            {
                tools = FindObjectOfType(typeof(PumkinsAvatarTools)) as PumkinsAvatarTools ?? CreateInstance<PumkinsAvatarTools>();
                //tools = CreateInstance<PumkinsAvatarTools>();
            }

            switch(_DependecyChecker.Status)
            {
                case _DependecyChecker.CheckerStatus.OK:
                case _DependecyChecker.CheckerStatus.NO_BONES:
                case _DependecyChecker.CheckerStatus.OK_OLDBONES:
                    {
                        tools.OnGUI();
                        break;
                    }
                case _DependecyChecker.CheckerStatus.NO_SDK:
                    {
                        if(tools)
                            tools.Repaint();

                        Repaint();                        
                        EditorGUILayout.LabelField(Strings.Main.Title, Styles.Label_mainTitle, GUILayout.MinHeight(Styles.Label_mainTitle.fontSize + 6));
                        EditorGUILayout.Space();

                        EditorGUILayout.HelpBox("VRChat SDK not found.\nPlease install the SDK and try again.", MessageType.Warning, true);
                        if(GUILayout.Button("Search again"))
                            _DependecyChecker.ForceCheck();
                    }
                    break;
                default:
                    {
                        if(tools)
                            tools.Repaint();

                        Repaint();
                        EditorGUILayout.LabelField(Strings.Main.Title, Styles.Label_mainTitle, GUILayout.MinHeight(Styles.Label_mainTitle.fontSize + 6));
                        EditorGUILayout.HelpBox("Something went wrong", MessageType.Warning, true);
                        EditorGUILayout.Space();

                        if(GUILayout.Button("Try again"))
                            _DependecyChecker.ForceCheck();
                    }
                    break;
            }
        }
    }
}
