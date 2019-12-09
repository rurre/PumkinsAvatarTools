using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;

namespace Pumkin.DependencyChecker
{
    [InitializeOnLoad]
    public class _DependencyChecker
    {
        static string hasBonesString = "#define BONES\r\n";
        static string hasOldBonesString = "#define OLD_BONES\r\n";
        static string hasNewSDKString = "#define NEWSDK\r\n";

        public static string MainScriptPath { get; private set; }

        static bool toolsNeedRewrite = false;
        static bool structsNeedRewrite = false;

        static bool bonesAreOld = false;
        static bool sdkIsNew = false;
        static bool missingBoneFiles = false;

        public enum CheckerStatus { DEFAULT, NO_BONES, NO_SDK, OK, OK_OLDBONES };
        public static CheckerStatus Status
        {
            get; internal set;
        }        

        static _DependencyChecker()
        {            
            Check();  
        }

        public static void ForceCheck()
        {            
            Status = CheckerStatus.DEFAULT;
            Check();
        }

        public static void Check()
        {
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
            Type perfStatsType = GetType("VRCSDK2.Validation.Performance.Stats.AvatarPerformanceStats");
            
            //Type toolsType = GetType("Pumkin.AvatarTools.PumkinsAvatarTools");            
                        
            var dynPaths = new List<string>();
            dynPaths.AddRange(Directory.GetFiles(Application.dataPath, "DynamicBone.cs", SearchOption.AllDirectories));
            dynPaths.AddRange(Directory.GetFiles(Application.dataPath, "DynamicBoneCollider.cs"));
            var toolScriptPath = Directory.GetFiles(Application.dataPath, "PumkinsAvatarTools.cs", SearchOption.AllDirectories);
            var structsPath = Directory.GetFiles(Application.dataPath, "PumkinsDataStructures.cs", SearchOption.AllDirectories);
            
            missingBoneFiles = dynPaths.Count == 0 ? true : false;

            if(perfStatsType != null)
                sdkIsNew = true;

            if(toolScriptPath.Length > 0 && !string.IsNullOrEmpty(toolScriptPath[0]) && structsPath.Length > 0 && !string.IsNullOrEmpty(structsPath[0]))
            {
                string structsFile = File.ReadAllText(structsPath[0]);
                string structsHeader = structsFile.Substring(0, structsFile.IndexOf("using"));
                                
                structsFile = structsFile.Substring(structsFile.IndexOf("using"));                

                string toolsFile = File.ReadAllText(toolScriptPath[0]);
                string toolsHeader = toolsFile.Substring(0, toolsFile.IndexOf("using"));

                toolsFile = toolsFile.Substring(toolsFile.IndexOf("using"));                

                int toolsHasBonesIndex = toolsHeader.IndexOf(hasBonesString);
                int toolsHasOldBonesIndex = toolsHeader.IndexOf(hasOldBonesString);

                int structsHasBonesIndex = structsHeader.IndexOf(hasBonesString);
                int structsHasOldBonesIndex = structsHeader.IndexOf(hasOldBonesString);
                int structsHasNewSdkIndex = structsHeader.IndexOf(hasNewSDKString);               
                                                
                if(missingBoneFiles) //No bones in project
                {
                    Status = CheckerStatus.NO_BONES;
                    Debug.Log("<color=blue>PumkinsAvatarTools</color>: DynamicBones not found in project.");                    
                    if(toolsHasBonesIndex != -1 || toolsHasOldBonesIndex != -1) //#define BONES present, remove
                    {
                        toolsHeader = "";
                        toolsNeedRewrite = true;                        
                    }

                    if(structsHasBonesIndex != -1 || toolsHasOldBonesIndex != -1)
                    {
                        structsHeader = "";
                        structsNeedRewrite = true;
                    }
                }
                else //DynamicBones Present
                {
                    if(boneColliderBaseType != null && boneColliderType.IsSubclassOf(boneColliderBaseType))
                        bonesAreOld = false;
                    else
                        bonesAreOld = true;

                    if(bonesAreOld)
                    {
                        Status = CheckerStatus.OK_OLDBONES;
                        Debug.Log("<color=blue>PumkinsAvatarTools</color>: Found old version of DynamicBones in project!");
                        if(toolsHasOldBonesIndex == -1)
                        {
                            toolsHeader = hasOldBonesString;
                            toolsNeedRewrite = true;
                        }

                        if(structsHasOldBonesIndex == -1)
                        {
                            structsHeader = hasOldBonesString;
                            structsNeedRewrite = true;
                        }
                    }
                    else
                    {
                        Status = CheckerStatus.OK;
                        Debug.Log("<color=blue>PumkinsAvatarTools</color>: Found DynamicBones in project!");
                        if(toolsHasBonesIndex == -1)
                        {
                            toolsHeader = hasBonesString;
                            toolsNeedRewrite = true;
                        }

                        if(structsHasBonesIndex == -1)
                        {
                            structsHeader = hasBonesString;
                            structsNeedRewrite = true;
                        }
                    }
                }

                if(sdkIsNew)
                {
                    if(structsHasNewSdkIndex == -1)
                    {
                        structsHeader = hasNewSDKString;
                        if(!missingBoneFiles)
                        {
                            if(bonesAreOld)
                            {
                                structsHeader += hasOldBonesString;
                            }
                            else
                            {
                                structsHeader += hasBonesString;
                            }
                        }
                        structsNeedRewrite = true;
                    }
                }
                else
                {
                    if(structsHasNewSdkIndex != -1)
                    {
                        structsHeader = "";
                        if(!missingBoneFiles)
                        {
                            if(bonesAreOld)
                            {
                                structsHeader += hasOldBonesString;
                            }
                            else
                            {
                                structsHeader += hasBonesString;
                            }
                        }
                        structsNeedRewrite = true;
                    }
                }

                if(toolsNeedRewrite)
                {
                    File.WriteAllText(toolScriptPath[0], toolsHeader + toolsFile);
                    AssetDatabase.ImportAsset(RelativePath(toolScriptPath[0]));
                    toolsNeedRewrite = false;
                    Pumkin.AvatarTools._PumkinsAvatarToolsWindow.RepaintSelf();
                }

                if(structsNeedRewrite)
                {
                    File.WriteAllText(structsPath[0], structsHeader + structsFile);
                    AssetDatabase.ImportAsset(RelativePath(structsPath[0]));
                    structsNeedRewrite = false;
                    Pumkin.AvatarTools._PumkinsAvatarToolsWindow.RepaintSelf();
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
