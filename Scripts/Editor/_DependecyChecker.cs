using UnityEngine;
using UnityEditor;
using System;
using System.IO;

namespace Pumkin.DependencyChecker
{
    [InitializeOnLoad]
    public class _DependecyChecker
    {
        static readonly float checkCooldown = 5f;

        static string noBones = "#define NO_BONES\r\n";
        static string oldBones = "#define OLD_BONES\r\n";

        public static string MainScriptPath { get; private set; }
        public enum CheckerStatus { NO_BONES, OLD_BONES, NO_SDK, OK, DEFAULT };

        public static CheckerStatus Status { get; private set; }

        static float lastCheck = 0;
                
        static _DependecyChecker()
        {
            Status = CheckerStatus.DEFAULT;
            Check();  
        }        

        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            Check();
        }

        public static void Check()
        {
            if(Status != CheckerStatus.DEFAULT)
            {
                float now = Time.time;
                if(now >= lastCheck + checkCooldown)
                    lastCheck = now;
                else
                    return;
            }

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
                Status = CheckerStatus.OK;
            }


            Debug.Log("<color=blue>PumkinsAvatarTools</color>: Checking for DynamicBones in project...");
            Type boneType = GetType("DynamicBoneCollider");
            Type toolsType = GetType("Pumkin.AvatarTools.PumkinsAvatarTools");            

            var toolScriptPath = Directory.GetFiles(Application.dataPath, "PumkinsAvatarTools.cs", SearchOption.AllDirectories);            

            if(toolScriptPath.Length > 0 && !string.IsNullOrEmpty(toolScriptPath[0]))
            {
                string toolsFile = File.ReadAllText(toolScriptPath[0]);
                string header = toolsFile.Substring(0, toolsFile.IndexOf("using"));

                toolsFile = toolsFile.Substring(toolsFile.IndexOf("using"));

                int noBonesIndex = header.IndexOf(noBones);
                int noOldBonesIndex = header.IndexOf(oldBones);
                                
                if(toolsType == null || boneType == null)
                {
                    if(noBonesIndex == -1) //#define NO_BONES missing, add
                    {
                        header = noBones;

                        File.WriteAllText(toolScriptPath[0], header + toolsFile);
                        AssetDatabase.ImportAsset(RelativePath(toolScriptPath[0]));

                        Status = CheckerStatus.NO_BONES;
                        Debug.Log("<color=blue>PumkinsAvatarTools</color>: DynamicBones not found in project");
                    }
                }
                else //DynamicBones Present
                {                    
                    var colScriptPath = Directory.GetFiles(Application.dataPath, "DynamicBoneCollider.cs", SearchOption.AllDirectories);                    

                    if(colScriptPath.Length > 0 && !string.IsNullOrEmpty(colScriptPath[0]))
                    {
                        string colFile = File.ReadAllText(colScriptPath[0]);

                        string colHeader = colFile.Substring(0, colFile.IndexOf('{'));
                        colHeader = colHeader.Replace(" ", "");
                        colHeader = colHeader.Replace("\t", "");

                        if(colHeader.IndexOf(":DynamicBoneColliderBase") == -1) //Old version of DynamicBone
                        {
                            if(noOldBonesIndex == -1) //#define OLD_BONES not found, add
                            {
                                header = oldBones;

                                File.WriteAllText(toolScriptPath[0], header + toolsFile);
                                AssetDatabase.ImportAsset(RelativePath(toolScriptPath[0]));

                                Status = CheckerStatus.OLD_BONES;
                                Debug.Log("<color=blue>PumkinsAvatarTools</color>: Found old version of Dynamic Bones in project");
                            }                            
                        }
                        else //New Version of DynamicBone
                        {
                            if(noOldBonesIndex != -1 || noBonesIndex != -1) //#define OLD_BONES found, remove
                            {
                                File.WriteAllText(toolScriptPath[0], toolsFile);
                                AssetDatabase.ImportAsset(RelativePath(toolScriptPath[0]));

                                Status = CheckerStatus.OK;                                
                            }
                        }
                        if(Status == CheckerStatus.OK)
                        {
                            Debug.Log("<color=blue>PumkinsAvatarTools</color>: Found Dynamic Bones in project");
                        }
                    }
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
