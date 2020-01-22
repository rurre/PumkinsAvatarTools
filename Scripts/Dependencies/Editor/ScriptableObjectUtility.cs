using UnityEngine;
using UnityEditor;
using System.IO;
using Pumkin.HelperFunctions;
using Pumkin.Presets;

namespace Pumkin.Dependencies
{
    public static class ScriptableObjectUtility
    {
        public static void SaveAsset<T>(T asset, string name, string path, bool overwriteExisting = false) where T : ScriptableObject
        {
            if(asset == null)
            {
                Debug.LogWarning("Attempting to create a null asset.");
                return;
            }
            try
            {
                if(!name.Contains(".asset"))
                    name += ".asset";

                if(path.StartsWith(Application.dataPath))
                {
                    path = path.Substring(Application.dataPath.Length);
                }

                path = path.TrimStart('/', '\\');
                string finalPath = path;

                if(!finalPath.ToLower().StartsWith("assets/"))
                    finalPath = "Assets/" + finalPath;

                var subFolders = name.Split('/', '\\');
                if(subFolders.Length > 0)
                    name = subFolders[subFolders.Length - 1];

                string fullPath = Application.dataPath + "/" + path + name;

                if(File.Exists(fullPath))
                {
                    if(overwriteExisting)
                    {
                        File.Delete(fullPath);
                        //Debug.Log("Deleting " + fullPath);
                    }
                    else
                    {
                        finalPath = path + '/' + Helpers.NextAvailableFilename(name);
                        //Debug.Log("new path is" + fullPath);
                    }
                }

                AssetDatabase.CreateAsset(asset, finalPath + name);
                //Debug.Log("Should create " + finalPath);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = asset;
            }
            catch(System.Exception e)
            {
                Debug.LogError(e.Message);
                return;
            }
        }

        /// <summary>
        //	This makes it easy to create, name and place unique new ScriptableObject asset files.
        /// </summary>
        public static T CreateAndSaveAsset<T>(string name, string path, bool overwriteExisting = false) where T : ScriptableObject
        {
            T asset = ScriptableObject.CreateInstance<T>();
            SaveAsset(asset, name, path, overwriteExisting);
            return asset;
        }
    }
}