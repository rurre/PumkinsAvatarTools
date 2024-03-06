using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;
using Pumkin.Dependencies;
using System.Linq;
using UnityEditor.Compilation;

namespace Pumkin.DependencyChecker
{
    [InitializeOnLoad, ExecuteInEditMode]
    public class _DependencyChecker
    {
        const string HAS_PBONES = "PUMKIN_PBONES";
        const string HAS_DBONES = "PUMKIN_DBONES";
        const string HAS_FINALIK = "PUMKIN_FINALIK";
        const string HAS_OLD_DBONES = "PUMKIN_OLD_DBONES";
        const string HAS_SDK1 = "PUMKIN_VRCSDK1";
        const string HAS_SDK2 = "PUMKIN_VRCSDK2";

        public static string MainScriptPath { get; private set; }

        public enum PumkinsDBonesVersion { NotFound, OldVersion, NewVersionWithBaseColliders }
        public enum PumkinsSDKVersion { NotFound, SDK2, SDK3 }

        public static PumkinsSDKVersion SDKVersion
        {
            get; private set;
        }
        public static PumkinsDBonesVersion DBonesVersion
        {
            get; private set;
        }

        public static bool FinalIKExists { get; private set; } = false;

        static bool _mainToolsOK = true;

        public static bool MainToolsOK
        {
            get { return _mainToolsOK; } private set { _mainToolsOK = value; }
        }
        
        public static bool PhysBonesExist { get; private set; } = false;

        static _DependencyChecker()
        {
            CheckForDependencies();
        }

        public static void ResetDependencies()
        {
            Debug.Log("<color=blue>PumkinsAvatarTools</color>: Resetting tool preferences...");
            ScriptDefinesManager.RemoveDefines(HAS_SDK1, HAS_SDK2, HAS_DBONES, HAS_OLD_DBONES, HAS_PBONES, HAS_FINALIK);
        }

        public static void CheckForDependencies()
        {
            SDKVersion = GetVRCSDKVersion();
            PhysBonesExist = GetPhysBones();
            DBonesVersion = GetDynamicBonesVersion();
            MainToolsOK = GetTypeFromName("Pumkin.AvatarTools.PumkinsAvatarTools") != null;

            FinalIKExists = GetFinalIK();
        }

        static bool GetFinalIK()
        {
            Debug.Log("<color=blue>PumkinsAvatarTools</color>: Checking for FinalIK in project...");
            if(Directory.GetFiles(Path.GetDirectoryName(Application.dataPath), "VRIK.cs", SearchOption.AllDirectories).Length > 0)
            {
                Debug.Log("<color=blue>PumkinsAvatarTools</color>: FinalIK found in project.");
                return true;
            }
            Debug.Log("<color=blue>PumkinsAvatarTools</color>: FinalIK not found in project.");
            return false;
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
        static bool GetPhysBones()
        {
            Debug.Log("<color=blue>PumkinsAvatarTools</color>: Checking for PhysBones and Contacts in project...");
            if(AppDomain.CurrentDomain.GetAssemblies().Any(ass => ass.FullName.StartsWith("VRC.Dynamics")))
            {
                Debug.Log("<color=blue>PumkinsAvatarTools</color>: Found PhysBones and Contacts in project!");
                return true;
            }
            else
            {
                Debug.Log("<color=blue>PumkinsAvatarTools</color>: PhysBones and Contacts not found in project.");
                return false;
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
            dynPaths.AddRange(Directory.GetFiles(Path.GetDirectoryName(Application.dataPath), "DynamicBone.cs", SearchOption.AllDirectories));
            dynPaths.AddRange(Directory.GetFiles(Path.GetDirectoryName(Application.dataPath), "DynamicBoneCollider.cs"));            

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
    }
}
