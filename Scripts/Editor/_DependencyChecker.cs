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
        const string HAS_PBONES = "PUMKIN_PBONES";
        const string HAS_DBONES = "PUMKIN_DBONES";
        const string HAS_OLD_DBONES = "PUMKIN_OLD_DBONES";
        const string HAS_SDK1 = "PUMKIN_VRCSDK1";
        const string HAS_SDK2 = "PUMKIN_VRCSDK2";

        public static string MainScriptPath { get; private set; }

        public enum PumkinsDBonesVersion { NotFound, OldVersion, NewVersionWithBaseColliders }
        public enum PumkinsPBones { NotFound, Found, }
        public enum PumkinsSDKVersion { NotFound, SDK2, SDK3 }

        public static PumkinsSDKVersion SDKVersion
        {
            get; private set;
        }
        public static PumkinsPBones PBones
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
            ScriptDefinesManager.RemoveDefines(HAS_SDK1, HAS_SDK2, HAS_DBONES, HAS_OLD_DBONES, HAS_PBONES);
        }

        public static void CheckForDependencies()
        {
            SDKVersion = GetVRCSDKVersion();
            PBones = GetPhysBones();
            DBonesVersion = GetDynamicBonesVersion();
            MainToolsOK = GetTypeFromName("Pumkin.AvatarTools.PumkinsAvatarTools") != null ? true : false;            

            var definesToAdd = new HashSet<string>();
            var currentDefines = ScriptDefinesManager.GetDefinesAsArray();

            switch (PBones)
            {
                case PumkinsPBones.Found:
                    definesToAdd.Add(HAS_PBONES);
                    break;
                case PumkinsPBones.NotFound:
                default:
                    break;
            }

            switch (DBonesVersion)
            {
                case PumkinsDBonesVersion.NewVersionWithBaseColliders:
                case PumkinsDBonesVersion.OldVersion:
                    definesToAdd.Add(HAS_DBONES);
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
#if VRC_SDK_VRCSDK3
            return PumkinsSDKVersion.SDK3;
#elif VRC_SDK_VRCSDK2
            return PumkinsSDKVersion.SDK2;
#else
            return PumkinsSDKVersion.NotFound;
#endif
        }

        /// <summary>
        /// Check if we have PhysBones and Contacts
        /// </summary>        
        static PumkinsPBones GetPhysBones()
        {
            Debug.Log("<color=blue>PumkinsAvatarTools</color>: Checking for PhysBones and Contacts in project...");

            var pPaths = new List<string>(); 
            pPaths.AddRange(Directory.GetFiles(Application.dataPath, "VRC.SDK3.Dynamics.PhysBone.dll", SearchOption.AllDirectories));
            pPaths.AddRange(Directory.GetFiles(Application.dataPath, "VRC.SDK3.Dynamics.Contact.dll", SearchOption.AllDirectories));

            if (pPaths.Count == 0) //No Physbones or Contacts in project
            {
                Debug.Log("<color=blue>PumkinsAvatarTools</color>: PhysBones and Contacts not found in project.");
                return PumkinsPBones.NotFound;
            }
            else //PhysBones and  and Contacts Present
            {
                Debug.Log("<color=blue>PumkinsAvatarTools</color>: Found PhysBones and Contacts in project!");
                return PumkinsPBones.Found;
            }
        }

        /// <summary>
        /// Check if we have DynamicBones and get their "version"
        /// </summary>        
        static PumkinsDBonesVersion GetDynamicBonesVersion()
        {
            Debug.Log("<color=blue>PumkinsAvatarTools</color>: Checking for DynamicBones in project...");
            Type boneColliderType = GetTypeFromName("DynamicBoneCollider");
            Type boneColliderBaseType = GetTypeFromName("DynamicBoneColliderBase");                               

            var dynPaths = new List<string>();
            dynPaths.AddRange(Directory.GetFiles(Application.dataPath, "DynamicBone.cs", SearchOption.AllDirectories));
            dynPaths.AddRange(Directory.GetFiles(Application.dataPath, "DynamicBoneCollider.cs"));            

            if(dynPaths.Count == 0) //No Dynamicbones in project
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

        public static Type GetTypeFromName(string typeName)
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

        static string GetRelativePath(string path)
        {            
            if(path.StartsWith(Application.dataPath))            
                path = "Assets" + path.Substring(Application.dataPath.Length);            
            return path;
        }
    }
}
