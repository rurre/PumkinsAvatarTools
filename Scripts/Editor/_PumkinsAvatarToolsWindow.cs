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
#if PUMKIN_OK
        [SerializeField, HideInInspector] static PumkinsAvatarTools _tools;        
#endif
        static EditorWindow _toolsWindow;

        string[] boneErrors =
            {
            "The type or namespace name `DynamicBoneCollider' could not be found.",
            "The type or namespace name `DynamicBone' could not be found.",
            "Cannot implicitly convert type `System.Collections.Generic.List<DynamicBoneCollider>' to `System.Collections.Generic.List<DynamicBoneColliderBase>'",
            };

        static bool pressedReloadButton = false;

#if PUMKIN_OK
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
#endif

        [DidReloadScripts]
        static void OnScriptsReloaded()
        {
            pressedReloadButton = false;
        }

        [MenuItem("Tools/Pumkin/Avatar Tools", false, 0)]
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

        [MenuItem("Tools/Pumkin/Reset Tool Preferences", false, 50)]
        public static void ResetPrefs()
        {
            EditorPrefs.DeleteKey("PumkinToolsWindow");
#if PUMKIN_OK
            if(_tools)            
                _tools.ResetEverything();                
#endif

            if(_toolsWindow)
                DestroyImmediate(_toolsWindow);

            _DependencyChecker.ResetDependencies();
        }

        private void OnEnable()
        {
            Application.logMessageReceived -= HandleError;
            Application.logMessageReceived += HandleError;

#if PUMKIN_OK
            if(ToolsWindow)
                ToolsWindow.HandleOnEnable();
#endif
        }

        private void OnDisable()
        {
            Application.logMessageReceived -= HandleError;

#if PUMKIN_OK
            if(ToolsWindow)
                ToolsWindow.HandleOnDisable();
#endif
        }

        private void OnDestroy()
        {
            PumkinsAvatarTools.DestroyDummies();
        }

        void HandleError(string log, string stack, LogType type)
        {
            if(type == LogType.Error)
            {
                for(int i = 0; i < boneErrors.Length; i++)
                {
                    if(log.Contains(boneErrors[i]))
                    {
                        ResetPrefs();
                        break;
                    }
                }
            }
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
#if PUMKIN_OK
            if(ToolsWindow)
                ToolsWindow.RefreshLanguage();
#endif
        }

        public void OnGUI()
        {
#if PUMKIN_OK
            if(!ToolsWindow)
                return;

            ToolsWindow.OnGUI();
#endif
#if !PUMKIN_VRCSDK1 && !PUMKIN_VRCSDK2 && !VRC_SDK_EXISTS

            EditorGUILayout.LabelField(Strings.Main.title, Styles.Label_mainTitle, GUILayout.MinHeight(Styles.Label_mainTitle.fontSize + 6));
            EditorGUILayout.Space();

            EditorGUILayout.HelpBox("VRChat SDK not found.\nPlease install the SDK and try again.", MessageType.Warning, true);
            EditorGUI.BeginDisabledGroup(pressedReloadButton);
            {
                if(GUILayout.Button(pressedReloadButton ? "Loading..." : "Reload", Styles.BigButton))
                {
                    _DependencyChecker.CheckForDependencies();
                    pressedReloadButton = true;
                }
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.HelpBox("If you need help, you can join my Discord server!", MessageType.Info, true);
            if(GUILayout.Button(new GUIContent(Strings.Buttons.joinDiscordServer ?? "Join Discord Server", Icons.DiscordIcon)))
                Application.OpenURL(Strings.LINK_DISCORD);
            EditorGUILayout.LabelField("I'm not sure why the button is so big. Help");            
#endif
#if PUMKIN_OK
            HandleRepaint(ToolsWindow);
                Repaint();
#else
            Repaint();
            EditorGUILayout.LabelField(Strings.Main.title, Styles.Label_mainTitle, GUILayout.MinHeight(Styles.Label_mainTitle.fontSize + 6));
            EditorGUILayout.HelpBox("Tool preferences have been reset.\nIf you just installed these, just press reload.\nIf not, and reload doesn't fix it, consider asking for help.", MessageType.Warning, true);            

            EditorGUI.BeginDisabledGroup(pressedReloadButton);
            {
                if(GUILayout.Button(pressedReloadButton ? "Loading..." : "Reload", Styles.BigButton))
                {
                    _DependencyChecker.CheckForDependencies();
                    pressedReloadButton = true;
                }
            }
            EditorGUI.EndDisabledGroup();

            Helpers.DrawGUILine();

            EditorGUILayout.HelpBox("If you need help, you can join my Discord server!", MessageType.Info, true);
            EditorGUIUtility.SetIconSize(new Vector2(25, 25));
            if(GUILayout.Button(new GUIContent(Strings.Buttons.joinDiscordServer ?? "Join Discord Server", Icons.DiscordIcon ?? null)))
                Application.OpenURL(Strings.LINK_DISCORD);
#endif
        }
    }
}