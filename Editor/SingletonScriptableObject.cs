using System.Linq;
using UnityEngine;

namespace Pumkin
{
    /// <summary>
    /// Abstract class for making reload-proof singletons out of ScriptableObjects
    /// Returns the asset created on the editor, or creates one if it's missing
    /// Based on https://www.youtube.com/watch?v=VBA1QCoEAX4
    /// </summary>
    /// <typeparam name="T">Singleton type</typeparam>

    public abstract class SingletonScriptableObject<T> : ScriptableObject where T : ScriptableObject
    {
        static T _instance = null;
        public static T Instance
        {
            get
            {
                if(!_instance)
                    _instance = Resources.FindObjectsOfTypeAll<T>().FirstOrDefault() ?? CreateInstance<T>();
                return _instance;
            }
        }
    }
}