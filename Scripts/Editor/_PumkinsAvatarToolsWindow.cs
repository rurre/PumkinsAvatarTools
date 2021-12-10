using UnityEditor;
using UnityEngine;
using Pumkin.DependencyChecker;
using Pumkin.DataStructures;
using Pumkin.HelperFunctions;
using UnityEditor.Callbacks;

namespace Pumkin.AvatarTools
{
    [System.Serializable]
    public class _PumkinsAvatarToolsWindow : EditorWindow
    {
        [SerializeField, HideInInspector] static PumkinsAvatarTools _tools;

        static EditorWindow _toolsWindow;

        static bool pressedReloadButton = false;

        public static PumkinsAvatarTools ToolsWindow
        {
            get
            {
                if(!_tools)
                    _tools = FindObjectOfType(typeof(PumkinsAvatarTools)) as PumkinsAvatarTools ?? CreateInstance<PumkinsAvatarTools>();
                return _tools;
            }

            private set
            {
                _tools = value;
            }
        }

        [DidReloadScripts]
        static void OnScriptsReloaded()
        {
            pressedReloadButton = false;
        }

        [MenuItem("Pumkin/Tools/Avatar Tools", false, 0)]
        public static void ShowWindow()
        {
            //Show existing window instance. If one doesn't exist, make one.
            if(!_toolsWindow)
            {
                _toolsWindow = EditorWindow.GetWindow(typeof(_PumkinsAvatarToolsWindow));
                _toolsWindow.autoRepaintOnSceneChange = true;
                _toolsWindow.titleContent = new GUIContent(Strings.Main.windowName);
            }
            _toolsWindow.Show();
        }

        [MenuItem("Pumkin/Tools/Reset Tool Preferences", false, 50)]
        public static void ResetPrefs()
        {
            EditorPrefs.DeleteKey("PumkinToolsWindow");

            if(_tools)
                _tools.ResetBackgroundsAndOverlays();

            if(_toolsWindow)
                DestroyImmediate(_toolsWindow);

            _DependencyChecker.ResetDependencies();
        }

        private void OnEnable()
        {
            if(ToolsWindow)
                ToolsWindow.HandleOnEnable();
        }

        private void OnDisable()
        {
            if(ToolsWindow)
                ToolsWindow.HandleOnDisable();
        }

        private void OnDestroy()
        {
            PumkinsAvatarTools.DestroyDummies();
        }

        void HandleRepaint(EditorWindow window)
        {
            if(!window || !EditorWindow.focusedWindow)
                return;

            //This check is needed otherwise we can't rename any objects in the hierarchy
            //Works for now, but we need to check if changing language will affect it
            if(EditorWindow.focusedWindow.titleContent.text != "Hierarchy") //UnityEditor.SceneHierarchyWindow
                window.Repaint();
        }

        private void OnInspectorUpdate()
        {
            HandleRepaint(this);
        }

        public static void RequestRepaint(EditorWindow window)
        {
            if(window)
                window.Repaint();
        }

        public static void RepaintSelf()
        {
            if(_toolsWindow)
                _toolsWindow.Repaint();
        }

        [UnityEditor.Callbacks.DidReloadScripts]
        static void OnReloadScript()
        {
            if(ToolsWindow)
                ToolsWindow.RefreshLanguage();
        }

        public void OnGUI()
        {
            if(_DependencyChecker.MainToolsOK)
            {
                if(ToolsWindow)
                    ToolsWindow.OnGUI();
            }
            else
            {
                Repaint();
                EditorGUILayout.LabelField(Strings.Main.title, Styles.Label_mainTitle, GUILayout.MinHeight(Styles.Label_mainTitle.fontSize + 6));
                EditorGUILayout.HelpBox("Thanks for getting my tools!\nPress the button below to set everything up.", MessageType.Info, true);

                EditorGUI.BeginDisabledGroup(pressedReloadButton);
                {
                    if(GUILayout.Button(pressedReloadButton ? "Loading..." : "The Button", Styles.BigButton))
                    {
                        _DependencyChecker.CheckForDependencies();
                        pressedReloadButton = true;
                    }
                }
                EditorGUI.EndDisabledGroup();

                Helpers.DrawGUILine();

                EditorGUILayout.HelpBox("If you need help, you can join my Discord server!", MessageType.Info, true);
                EditorGUIUtility.SetIconSize(new Vector2(25, 25));
                if(GUILayout.Button(new GUIContent("Join Discord Server", Icons.DiscordIcon ?? null)))
                    Application.OpenURL(Strings.LINK_DISCORD);
            }
        }
    }
}