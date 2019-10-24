using UnityEditor;
using UnityEngine;
using Pumkin.DependencyChecker;
using Pumkin.DataStructures;

namespace Pumkin.AvatarTools
{
    [System.Serializable]
    public class _PumkinsAvatarToolsWindow : EditorWindow
    {
        [SerializeField, HideInInspector] static PumkinsAvatarTools _tools;        
        static EditorWindow _window;
                
        string[] boneErrors =
            {
            "The type or namespace name `DynamicBoneCollider' could not be found.",
            "The type or namespace name `DynamicBone' could not be found.",            
            "Cannot implicitly convert type `System.Collections.Generic.List<DynamicBoneCollider>' to `System.Collections.Generic.List<DynamicBoneColliderBase>'",
            };

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

        [MenuItem("Tools/Pumkin/Avatar Tools", false, 0)]
        public static void ShowWindow()
        {
            //Show existing window instance. If one doesn't exist, make one.
            if(!_window)
            {
                _window = EditorWindow.GetWindow(typeof(_PumkinsAvatarToolsWindow));
                _window.autoRepaintOnSceneChange = true;
                _window.titleContent = new GUIContent(Strings.Main.WindowName);
            }
            _window.Show();
        }

        [MenuItem("Tools/Pumkin/Clear Tool Preferences",false, 11)]
        public static void ResetPrefs()
        {
            EditorPrefs.DeleteKey("PumkinToolsWindow");
            ToolsWindow = null;
            _DependencyChecker.Status = _DependencyChecker.CheckerStatus.DEFAULT;

            if(_window)
                _window.Repaint();
        }

        private void OnEnable()
        {
            Application.logMessageReceived -= HandleError;
            Application.logMessageReceived += HandleError;
            
            if(ToolsWindow)
                ToolsWindow.HandleOnEnable();
        }

        private void OnDisable()
        {
            Application.logMessageReceived -= HandleError;

            if(ToolsWindow)
                ToolsWindow.HandleOnDisable();
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

        public void OnGUI()
        {
            if(!ToolsWindow)
                return;

            switch(_DependencyChecker.Status)
            {
                case _DependencyChecker.CheckerStatus.OK:
                case _DependencyChecker.CheckerStatus.NO_BONES:
                case _DependencyChecker.CheckerStatus.OK_OLDBONES:
                    {
                        ToolsWindow.OnGUI();
                        break;
                    }
                case _DependencyChecker.CheckerStatus.NO_SDK:
                    {

                        HandleRepaint(ToolsWindow);                                              
                        EditorGUILayout.LabelField(Strings.Main.Title, Styles.Label_mainTitle, GUILayout.MinHeight(Styles.Label_mainTitle.fontSize + 6));
                        EditorGUILayout.Space();

                        EditorGUILayout.HelpBox("VRChat SDK not found.\nPlease install the SDK and try again.", MessageType.Warning, true);
                        if(GUILayout.Button("Reload", Styles.BigButton, GUILayout.MaxHeight(40)))
                            _DependencyChecker.ForceCheck();                                                

                        EditorGUILayout.HelpBox("If you need help, you can join my Discord server!", MessageType.Info, true);
                        if(GUILayout.Button(new GUIContent(Strings.Buttons.JoinDiscordServer ?? "Join Discord Server", Icons.DiscordIcon)))
                            Application.OpenURL(Strings.Instance.discordLink);
                        EditorGUILayout.LabelField("I'm not sure why the button is so big. Help");
                    }
                    break;
                default:
                    {
                        HandleRepaint(ToolsWindow);
                        Repaint();
                        EditorGUILayout.LabelField(Strings.Main.Title, Styles.Label_mainTitle, GUILayout.MinHeight(Styles.Label_mainTitle.fontSize + 6));
                        EditorGUILayout.HelpBox("You reset tool preferences or something went wrong", MessageType.Warning, true);
                        EditorGUILayout.Space();

                        if(GUILayout.Button("Reload", Styles.BigButton, GUILayout.MaxHeight(40)))
                            _DependencyChecker.ForceCheck();
                        
                        EditorGUILayout.HelpBox("If you need help, you can join my Discord server!", MessageType.Info, true);
                        if(GUILayout.Button(new GUIContent(Strings.Buttons.JoinDiscordServer ?? "Join Discord Server", Icons.DiscordIcon ?? null)))
                            Application.OpenURL(Strings.Instance.discordLink);
                        EditorGUILayout.LabelField("I'm not sure why the button is so big. Help");
                    }
                    break;
            }
        }
    }
}