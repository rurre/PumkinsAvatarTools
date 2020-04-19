using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;
using Pumkin.Dependencies;
using System.Linq;

namespace Pumkin.DependencyChecker
{
    [InitializeOnLoad, ExecuteInEditMode]
    public class _DependencyChecker
    {
        const string HAS_DBONES = "PUMKIN_DBONES";
        const string HAS_OLD_DBONES = "PUMKIN_OLD_DBONES";
        const string HAS_SDK1 = "PUMKIN_VRCSDK1";
        const string HAS_SDK2 = "PUMKIN_VRCSDK2";        

        public static string MainScriptPath { get; private set; }        

        public enum PumkinsDBonesVersion { NotFound, OldVersion, NewVersionWithBaseColliders }
        public enum PumkinsSDKVersion { NotFound, BeforePerformanceRanks, WithPerfromanceRanks }        

        public static PumkinsSDKVersion SDKVersion
        {
            get; private set;
        }
        public static PumkinsDBonesVersion DBonesVersion
        {
            get; private set;
        }

        static bool _mainToolsOK = true;

        public static bool MainToolsOK
        {
            get { return _mainToolsOK; } private set { _mainToolsOK = value; }
        }

        static _DependencyChecker() 
        {
            CheckForDependencies();
        }      

        public static void ResetDependencies()
        {
            Debug.Log("<color=blue>PumkinsAvatarTools</color>: Resetting tool preferences...");
            ScriptDefinesManager.RemoveDefines(HAS_SDK1, HAS_SDK2, HAS_DBONES, HAS_OLD_DBONES);
        }

        public static void CheckForDependencies()
        {            
            SDKVersion = GetVRCSDKVersion();
            DBonesVersion = GetDynamicBonesVersion();
            MainToolsOK = GetType("Pumkin.AvatarTools.PumkinsAvatarTools") != null ? true : false;            

            var definesToAdd = new HashSet<string>();
            var currentDefines = ScriptDefinesManager.GetDefinesAsArray();                

            switch(SDKVersion)
            {
                case PumkinsSDKVersion.BeforePerformanceRanks:
                    definesToAdd.Add(HAS_SDK1);
                    break;
                case PumkinsSDKVersion.WithPerfromanceRanks:
                    definesToAdd.Add(HAS_SDK2);
                    break;
                case PumkinsSDKVersion.NotFound:
                default:
                    break;
            }

            switch(DBonesVersion)
            {
                case PumkinsDBonesVersion.NewVersionWithBaseColliders:
                    definesToAdd.Add(HAS_DBONES);
                    break;
                case PumkinsDBonesVersion.OldVersion:
                    definesToAdd.Add(HAS_OLD_DBONES);
                    break;
                case PumkinsDBonesVersion.NotFound:
                default:
                    break;
            }
            
            ScriptDefinesManager.AddDefinesIfMissing(definesToAdd.ToArray());
        }

        /// <summary>
        /// Check if we have VRCSDK installed and get it's "version"
        /// </summary>        
        static PumkinsSDKVersion GetVRCSDKVersion()
        {
            Debug.Log("<color=blue>PumkinsAvatarTools</color>: Checking for VRChat SDK in project...");
            Type sdkType = GetType("VRCSDK2.VRC_AvatarDescriptor");

            if(sdkType != null)
            {
                Debug.Log("<color=blue>PumkinsAvatarTools</color>: Found VRChat SDK.");
                Type perfStatsType = GetType("VRCSDK2.Validation.Performance.Stats.AvatarPerformanceStats");
                if(perfStatsType != null)
                    return PumkinsSDKVersion.WithPerfromanceRanks;
                return PumkinsSDKVersion.BeforePerformanceRanks;
            }            

            Debug.Log("<color=blue>PumkinsAvatarTools</color>: VRChat SDK not found. Please import the SDK to use these tools.");
            return PumkinsSDKVersion.NotFound;
        }

        /// <summary>
        /// Check if we have DynamicBones and get their "version"
        /// </summary>        
        static PumkinsDBonesVersion GetDynamicBonesVersion()
        {
            Debug.Log("<color=blue>PumkinsAvatarTools</color>: Checking for DynamicBones in project...");
            Type boneColliderType = GetType("DynamicBoneCollider");
            Type boneColliderBaseType = GetType("DynamicBoneColliderBase");                               

            var dynPaths = new List<string>();
            dynPaths.AddRange(Directory.GetFiles(Application.dataPath, "DynamicBone.cs", SearchOption.AllDirectories));
            dynPaths.AddRange(Directory.GetFiles(Application.dataPath, "DynamicBoneCollider.cs"));            

            if(dynPaths.Count == 0) //No bones in project
            {                
                Debug.Log("<color=blue>PumkinsAvatarTools</color>: DynamicBones not found in project.");
                return PumkinsDBonesVersion.NotFound;
            }
            else //DynamicBones Present
            {
                if(boneColliderBaseType != null && boneColliderType.IsSubclassOf(boneColliderBaseType))
                {
                    Debug.Log("<color=blue>PumkinsAvatarTools</color>: Found DynamicBones in project!");
                    return PumkinsDBonesVersion.NewVersionWithBaseColliders;
                }
                else
                {
                    Debug.Log("<color=blue>PumkinsAvatarTools</color>: Found old version of DynamicBones in project!");
                    return PumkinsDBonesVersion.OldVersion;
                }                
            }
        }        

        static Type GetType(string typeName)
        {
            var type = Type.GetType(typeName);
            if(type != null)
                return type;
            foreach(var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = a.GetType(typeName);
                if(type != null)
                    return type;
            }
            return null;
        }

        public static string GetRelativePath(string path)
        {            
            if(path.StartsWith(Application.dataPath))            
                path = "Assets" + path.Substring(Application.dataPath.Length);            
            return path;
        }
    }
}
