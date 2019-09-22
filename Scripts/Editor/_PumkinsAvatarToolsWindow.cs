using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Pumkin.DependencyChecker;

namespace Pumkin.AvatarTools
{
    public class _PumkinsAvatarToolsWindow : EditorWindow
    {
        static PumkinsAvatarTools tools;
        [MenuItem("Tools/Pumkin/Avatar Tools")]
        public static void ShowWindow()
        {
            //Show existing window instance. If one doesn't exist, make one.
            EditorWindow editorWindow = EditorWindow.GetWindow(typeof(_PumkinsAvatarToolsWindow));
            editorWindow.autoRepaintOnSceneChange = true;

            editorWindow.Show();
            editorWindow.titleContent = new GUIContent(Strings.Main.WindowName);
        }

        public void OnGUI()
        {
            if(tools == null)
                tools = CreateInstance<PumkinsAvatarTools>();

            switch(_DependecyChecker.Status)
            {
                case _DependecyChecker.CheckerStatus.OK:
                    {
                        tools.OnGUI();
                        break;
                    }
                case _DependecyChecker.CheckerStatus.NO_SDK:
                    {
                        EditorGUILayout.LabelField(Strings.Main.Title, Styles.Label_mainTitle, GUILayout.MinHeight(Styles.Label_mainTitle.fontSize + 6));
                        EditorGUILayout.Space();

                        EditorGUILayout.HelpBox("VRChat SDK not found.\nPlease install the SDK and try again.", MessageType.Warning, true);
                        if(GUILayout.Button("Search again"))
                            _DependecyChecker.Check();
                    }
                    break;
                default:
                    {
                        EditorGUILayout.LabelField(Strings.Main.Title, Styles.Label_mainTitle, GUILayout.MinHeight(Styles.Label_mainTitle.fontSize + 6));
                        EditorGUILayout.HelpBox("Something went wrong", MessageType.Warning, true);
                        EditorGUILayout.Space();

                        if(GUILayout.Button("Try again"))
                            _DependecyChecker.Check();
                    }
                    break;
            }            
        }
    }
}
