using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Pumkin.AvatarTools
{
    /// <summary>
    /// Gets and sets preferences on a per project basis.
    /// For global preferences use UnityEngine.EditorPrefs
    /// </summary>
    public static class PrefManager
    {
        static readonly string prefString = $"Pumkin.AvatarTools-{Application.productName}";    //productName returns the name of the project

        /// <summary>
        /// Saves a bool value to player prefs using the project name as a prefix
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public static void SetBool(string name, bool value) =>
            EditorPrefs.SetBool($"{prefString}.{name}", value);

        /// <summary>
        /// Saves a float value to player prefs using the project name as a prefix
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public static void SetFloat(string name, float value) =>
            EditorPrefs.SetFloat($"{prefString}.{name}", value);

        /// <summary>
        /// Saves a int value to player prefs using the project name as a prefix
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public static void SetInt(string name, int value) =>
            EditorPrefs.SetInt($"{prefString}.{name}", value);

        /// <summary>
        /// Saves a string value to player prefs using the project name as a prefix
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public static void SetString(string name, string value) =>
            EditorPrefs.SetString($"{prefString}.{name}", value);

        /// <summary>
        /// Saves a Unity Object InstanceID to player prefs using the project name as a prefix
        /// </summary>
        /// <param name="name"></param>
        /// <param name="obj"></param>
        public static void SetObject(string name, UnityEngine.Object obj) =>
            SetInt($"{prefString}.{name}", obj?.GetInstanceID() ?? 0);

        /// <summary>
        /// Gets a bool value from player prefs using the project name as a prefix
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool GetBool(string name) =>
            EditorPrefs.GetBool($"{prefString}.{name}");

        /// <summary>
        /// Gets a bool value from player prefs using the project name as a prefix
        /// </summary>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns>The value or <paramref name="defaultValue"/> if not found</returns>
        public static bool GetBool(string name, bool defaultValue) =>
            EditorPrefs.GetBool($"{prefString}.{name}", defaultValue);

        /// <summary>
        /// Gets a float value from player prefs using the project name as a prefix
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static float GetFloat(string name) =>
            EditorPrefs.GetFloat($"{prefString}.{name}");

        /// <summary>
        /// Gets a float value from player prefs using the project name as a prefix
        /// </summary>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns>Value or <paramref name="defaultValue"/> if not found</returns>
        public static float GetFloat(string name, float defaultValue) =>
            EditorPrefs.GetFloat($"{prefString}.{name}", defaultValue);

        /// <summary>
        /// Gets a Int value from player prefs using the project name as a prefix
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static int GetInt(string name) =>
            EditorPrefs.GetInt($"{prefString}.{name}");

        /// <summary>
        /// Gets a bool value from player prefs using the project name as a prefix
        /// </summary>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns>Int value or <paramref name="defaultValue"/> if not found</returns>
        public static int GetInt(string name, int defaultValue) =>
            EditorPrefs.GetInt($"{prefString}.{name}", defaultValue);

        /// <summary>
        /// Gets a string value from player prefs using the project name as a prefix
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetString(string name) =>
            EditorPrefs.GetString($"{prefString}.{name}");

        /// <summary>
        /// Gets a string value from player prefs using the project name as a prefix
        /// </summary>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns>The string value or <paramref name="defaultValue"/> if not foudn</returns>
        public static string GetString(string name, string defaultValue) =>
            EditorPrefs.GetString($"{prefString}.{name}", defaultValue);

        /// <summary>
        /// Gets an instance ID from player prefs using project name as a prefix, then searches all unity objects for a matching one. Warning: Slow!
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns>Object reference of type <typeparamref name="T"/></returns>
        public static T GetUnityObject<T>(string name) where T : UnityEngine.Object
        {
            int id = GetInt($"{prefString}.{name}", 0);
            return Resources.FindObjectsOfTypeAll<T>().FirstOrDefault(o => o.GetInstanceID() == id);
        }

        /// <summary>
        /// Gets an instance ID from player prefs using project name as a prefix, then searches all objects for a matching one. Warning: Slow!
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns>Object reference of type <typeparamref name="T"/> or <paramref name="defaultValue"/></returns>
        public static T GetUnityObject<T>(string name, T defaultValue) where T : UnityEngine.Object =>
            GetUnityObject<T>(name) ?? defaultValue;
    }
}