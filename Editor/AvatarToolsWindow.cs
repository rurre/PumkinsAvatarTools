using UnityEditor;

namespace PumkinsAvatarTools
{
    public class AvatarToolsWindow : EditorWindow
    {
        [MenuItem("Tools/Pumkin/Avatar Tools", false, -10)]
        static void ShowWindow()
        {
            var window = GetWindow(typeof(AvatarToolsWindow), false, "Pumkin Tools", true);
            window.Show();
        }
    }
}