using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;

namespace Pumkin.DependencyChecker
{
    [InitializeOnLoad]
    public class _DependecyChecker
    {
        static readonly float checkCooldown = 0.5f;

        static string hasBonesString = "#define BONES\r\n";
        static string hasOldBonesString = "#define OLD_BONES\r\n";

        public static string MainScriptPath { get; private set; }

        static bool needRewrite = false;
        static bool needCheck = true;

        static bool bonesAreOld = false;
        
        static float _now = 0f;
        static float _canCheckNext = 0f;

        static bool checkManually = true;
        static bool missingBoneFiles = false;

        public enum CheckerStatus { DEFAULT, NO_BONES, NO_SDK, OK, OK_OLDBONES };
        public static CheckerStatus Status
        {
            get; internal set;
        }        

        static _DependecyChecker()
        {            
            Check();  
        }

        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            if(!checkManually)
                Check();
        }

        public static void ForceCheck()
        {
            needCheck = true;
            Status = CheckerStatus.DEFAULT;
            Check();
        }

        public static void Check()
        {
            _now = Time.time;
            
            if(!checkManually && (!needCheck || _now < _canCheckNext))
                return;

            Debug.Log("<color=blue>PumkinsAvatarTools</color>: Checking for VRChat SDK in project...");
            Type sdkType = GetType("VRCSDK2.VRC_AvatarDescriptor");

            if(sdkType == null)
            {
                Debug.Log("<color=blue>PumkinsAvatarTools</color>: VRChat SDK not found. Please import the SDK to use these tools.");
                Status = CheckerStatus.NO_SDK;
                return;
            }
            else
            {
                Debug.Log("<color=blue>PumkinsAvatarTools</color>: Found VRChat SDK.");                
            }

            Debug.Log("<color=blue>PumkinsAvatarTools</color>: Checking for DynamicBones in project...");            
            Type boneColliderType = GetType("DynamicBoneCollider");
            Type boneColliderBaseType = GetType("DynamicBoneColliderBase");
            //Type toolsType = GetType("Pumkin.AvatarTools.PumkinsAvatarTools");            
                        
            var dynPaths = new List<string>();
            dynPaths.AddRange(Directory.GetFiles(Application.dataPath, "DynamicBone.cs", SearchOption.AllDirectories));
            dynPaths.AddRange(Directory.GetFiles(Application.dataPath, "DynamicBoneCollider.cs"));
            var toolScriptPath = Directory.GetFiles(Application.dataPath, "PumkinsAvatarTools.cs", SearchOption.AllDirectories);
            
            missingBoneFiles = dynPaths.Count == 0 ? true : false;            

            if(toolScriptPath.Length > 0 && !string.IsNullOrEmpty(toolScriptPath[0]))
            {
                string toolsFile = File.ReadAllText(toolScriptPath[0]);
                string header = toolsFile.Substring(0, toolsFile.IndexOf("using"));

                toolsFile = toolsFile.Substring(toolsFile.IndexOf("using"));

                int hasBonesIndex = header.IndexOf(hasBonesString);
                int hasOldBonesIndex = header.IndexOf(hasOldBonesString);
                                
                if(missingBoneFiles) //No bones in project
                {
                    Status = CheckerStatus.NO_BONES;
                    Debug.Log("<color=blue>PumkinsAvatarTools</color>: DynamicBones not found in project.");                    
                    if(hasBonesIndex != -1 || hasOldBonesIndex != -1) //#define BONES present, remove
                    {
                        header = "";
                        needRewrite = true;                        
                    }                    
                }
                else //DynamicBones Present
                {
                    if(boneColliderType.IsSubclassOf(boneColliderBaseType))
                        bonesAreOld = false;
                    else
                        bonesAreOld = true;

                    if(bonesAreOld)
                    {
                        Status = CheckerStatus.OK_OLDBONES;
                        Debug.Log("<color=blue>PumkinsAvatarTools</color>: Found old version of DynamicBones in project!");
                        if(hasOldBonesIndex == -1)
                        {
                            header = hasOldBonesString;
                            needRewrite = true;
                        }
                    }
                    else
                    {
                        Status = CheckerStatus.OK;
                        Debug.Log("<color=blue>PumkinsAvatarTools</color>: Found DynamicBones in project!");
                        if(hasBonesIndex == -1)
                        {
                            header = hasBonesString;
                            needRewrite = true;
                        }
                    }
                }
                if(needRewrite)
                {
                    File.WriteAllText(toolScriptPath[0], header + toolsFile);
                    AssetDatabase.ImportAsset(RelativePath(toolScriptPath[0]));
                    needRewrite = false;
                }
            }
            if(!checkManually)
                _canCheckNext = _now + checkCooldown;
            needCheck = false;
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

        public static string RelativePath(string path)
        {            
            if(path.StartsWith(Application.dataPath))
            {
                path = "Assets" + path.Substring(Application.dataPath.Length);
            }
            return path;
        }
    }
}
