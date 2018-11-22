using UnityEngine;
using UnityEditor;
using System;
using System.IO;

namespace Pumkin
{
    [InitializeOnLoad]
    public class _DependecyChecker
    {
        static string noBones = "#define NO_BONES\r\n";
        static string oldBones = "#define OLD_BONES\r\n";
        
        static _DependecyChecker()
        {
            Check();
        }        

        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            Check();
        }

        public static void Check()
        {
            Type boneType = GetType("DynamicBoneCollider");
            Type toolsType = GetType("Pumkin.PumkinsAvatarTools");
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
                            }                            
                        }
                        else //New Version of DynamicBone
                        {
                            if(noOldBonesIndex != -1 || noBonesIndex != -1) //#define OLD_BONES found, remove
                            {
                                File.WriteAllText(toolScriptPath[0], toolsFile);
                                AssetDatabase.ImportAsset(RelativePath(toolScriptPath[0]));
                            }
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

        static string RelativePath(string path)
        {            
            if(path.StartsWith(Application.dataPath))
            {
                path = "Assets" + path.Substring(Application.dataPath.Length);
            }
            return path;
        }
    }
}
