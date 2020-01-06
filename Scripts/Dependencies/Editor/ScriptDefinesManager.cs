using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Pumkin.Dependencies
{
    [InitializeOnLoad]
    public static class ScriptDefinesManager
    {
        /// <summary>
        /// Adds define strings to 'Edit > Project Settings > Player > Scripting Define Symbols' for the currently selected platform
        /// </summary>
        /// <param name="newDefines">New defines to add</param>
        public static void AddDefinesIfMissing(params string[] newDefines)
        {
            AddDefinesIfMissing(EditorUserBuildSettings.selectedBuildTargetGroup, newDefines);
        }

        /// <summary>
        /// Removes define strings from 'Edit > Project Settings > Player > Scripting Define Symbols' for the currently selected platform
        /// </summary>
        /// <param name="definesToRemove">Defines to remove from the current build target</param>
        public static void RemoveDefines(params string[] definesToRemove)
        {
            RemoveDefines(EditorUserBuildSettings.selectedBuildTargetGroup, definesToRemove);
        }

        /// <summary>
        /// Checks if define is set for the currently selected build target
        /// </summary>
        /// <param name="defineString">String to check for</param>                
        public static bool IsDefined(string defineString)
        {
            return IsDefined(EditorUserBuildSettings.selectedBuildTargetGroup, defineString);
        }

        /// <summary>
        /// Adds define strings to 'Edit > Project Settings > Player > Scripting Define Symbols' for the given platform
        /// </summary>
        /// <param name="buildTarget">Build target to add defines for. Different for every platform</param>
        /// <param name="newDefines">New defines to add</param>
        public static void AddDefinesIfMissing(BuildTargetGroup buildTarget, params string[] newDefines)
        {
            bool definesChanged = false;
            string existingDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTarget);
            HashSet<string> defineSet = new HashSet<string>();

            if(existingDefines.Length > 0)
                defineSet = new HashSet<string>(existingDefines.Split(';'));

            foreach(string def in newDefines)
                if(defineSet.Add(def))
                    definesChanged = true;

            if(definesChanged)
            {
                string finalDefineString = string.Join(";", defineSet.ToArray());
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTarget, finalDefineString);
                Debug.LogFormat("Set Scripting Define Symbols for selected build target ({0}) to: {1}", buildTarget.ToString(), finalDefineString);
            }
        }

        /// <summary>
        /// Removes define strings from 'Edit > Project Settings > Player > Scripting Define Symbols' for the given platform
        /// </summary>
        /// <param name="buildTarget">Build target to remove defines for. Different for every platform</param>
        /// <param name="definesToRemove">Defines to remove from selected build target</param>
        public static void RemoveDefines(BuildTargetGroup buildTarget, params string[] definesToRemove)
        {
            string existingDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTarget);            

            if(existingDefines.Length == 0)
                return;

            HashSet<string> defineSet = new HashSet<string>(existingDefines.Split(';'));
            bool removedSomething = false;

            foreach(string def in definesToRemove)
            {
                if(defineSet.Remove(def))
                    removedSomething = true;
            }

            if(!removedSomething)
                return;

            string finalDefineString = string.Join(";", defineSet.ToArray());
            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTarget, finalDefineString);
            Debug.LogFormat("Set Scripting Define Symbols for selected build target ({0}) to: {1}", buildTarget.ToString(), finalDefineString);            
        }

        /// <summary>
        /// Checks if define is set for given build target
        /// </summary>
        /// <param name="buildTarget">Build target to check define for</param>
        /// <param name="defineString">String to check for</param>        
        public static bool IsDefined(BuildTargetGroup buildTarget, string defineString)
        {
            string existingDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTarget);
            List<string> splitDefines = existingDefines.Split(';').ToList();

            if(splitDefines != null && splitDefines.Count > 0)            
                if(splitDefines.Contains(defineString))
                    return true;                
            
            return false;
        }

        /// <summary>
        /// Returns currently set defines for the given build target as an array split by ';'
        /// </summary>
        /// <param name="buildTarget">Selected build target</param>        
        public static string[] GetDefinesAsArray(BuildTargetGroup buildTarget)
        {
            return PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTarget).Split(';');
        }

        /// <summary>
        /// Returns currently set defines for the currently selected build target as an array split by ';'
        /// </summary>        
        public static string[] GetDefinesAsArray()
        {
            return PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup).Split(';');
        }
    }
}