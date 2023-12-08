using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Pumkin.AvatarTools
{
    public static class PumkinsSettingsUtility
    {
        const string SavePath = "Pumkin/AvatarTools/";

        static string SettingsFolder
        {
            get
            {
                if(_settingsFolder == null)
                    return _settingsFolder = $"{ProjectFolder}{SavePath}";
                return _settingsFolder;
            }
        }

        public static string ProjectFolder
        {
            get
            {
                if(_projectFolder == null)
                {
                    string dataPath = Application.dataPath;
                    _projectFolder = dataPath.Substring(0, dataPath.LastIndexOf("Assets"));
                }
                return _projectFolder;
            }
        }

        static string _projectFolder;
        static string _settingsFolder;

        /// <summary>
        /// Serializes an object to json and saves it to [ProjectFolder]/Poi/Settings/<paramref name="filename"/>
        /// </summary>
        /// <param name="filename">The name of your settings file, including extension</param>
        /// <param name="obj">The object to serialize</param>
        /// <param name="prettyPrint">Pretty print the json</param>
        public static void SaveSettings(string filename, object obj, bool prettyPrint = true)
        {
            if(!Directory.Exists(SettingsFolder))
                Directory.CreateDirectory(SettingsFolder);

            string json = EditorJsonUtility.ToJson(obj, prettyPrint);
            File.WriteAllText($"{SettingsFolder}{filename}", json);
        }

        /// <summary>
        /// Tries to load a json settings file and deserialize it to type <typeparamref name="T"/>
        /// </summary>
        /// <param name="filename">The name of your settings file, including extension</param>
        /// <param name="obj">The deserialized object</param>
        /// <typeparam name="T">The type of the deserialized object</typeparam>
        /// <returns>True if loading succeeded</returns>
        public static bool TryLoadSettings<T>(string filename, out T obj) where T : class
        {
            obj = null;
            try
            {
                string path = $"{SettingsFolder}{filename}";
                if(!File.Exists(path))
                    return false;

                string json = File.ReadAllText(path);
                obj = JsonUtility.FromJson<T>(json);
                return true;
            }
            catch(Exception ex)
            {
                Debug.LogException(ex);
                return false;
            }
        }

        /// <summary>
        /// Checks whether a settings file exists
        /// </summary>
        /// <param name="filename">Name of the file</param>
        /// <returns>True if file exists</returns>
        public static bool SettingsFileExists(string filename)
        {
            return File.Exists($"{SettingsFolder}{filename}");
        }

        /// <summary>
        /// Ensures a settings file exists, creating it if necessary with the contents of a serialized <paramref name="defaultObject"/>/>
        /// </summary>
        /// <param name="filename">File to create if missing</param>
        /// <param name="defaultObject">Object to serialize into the new file if missing</param>
        /// <returns>True if file was created</returns>
        public static bool EnsureSettingsFileExists(string filename, object defaultObject)
        {
            if(!SettingsFileExists(filename))
            {
                SaveSettings(filename, defaultObject);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Opens a settings file. Creates it if it's missing.
        /// </summary>
        /// <param name="filename">File to open</param>
        /// <param name="defaultObject">Default object to serialize when creating a new file</param>
        public static void OpenSettingsFile(string filename, object defaultObject)
        {
            EnsureSettingsFileExists(filename, defaultObject);
            Application.OpenURL($"file:///{SettingsFolder}{filename}");
        }
    }
}
