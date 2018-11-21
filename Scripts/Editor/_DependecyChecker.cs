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

        void Awake()
        {
            Check();
        }

        public static void Check()
        {
            Type boneType = GetType("DynamicBoneCollider");
            Type toolsType = GetType("Pumkin.PumkinsAvatarTools");
            var toolScriptPath = Directory.GetFiles(Application.dataPath, "PumkinsAvatarTools.cs", SearchOption.AllDirectories)[0];

            if(!string.IsNullOrEmpty(toolScriptPath))
            {
                string toolsFile = File.ReadAllText(toolScriptPath);
                string header = toolsFile.Substring(0, toolsFile.IndexOf("using"));

                toolsFile = toolsFile.Substring(toolsFile.IndexOf("using"));

                int noBonesIndex = header.IndexOf(noBones);
                int noOldBonesIndex = header.IndexOf(oldBones);
                                
                if(toolsType == null || boneType == null) //DynamicBones Missing
                {                       
                    if(noBonesIndex == -1) //#define NO_BONES missing
                    {
                        header = noBones;
                        header += toolsFile;
                        File.WriteAllText(toolScriptPath, header);
                        AssetDatabase.ImportAsset(RelativePath(toolScriptPath));
                    }
                }
                else //DynamicBones Present
                {
                    header = "";

                    if(noOldBonesIndex == -1) //#define OLD_BONES not found
                    {                        
                        var colScriptPath = Directory.GetFiles(Application.dataPath, "DynamicBoneCollider.cs", SearchOption.AllDirectories)[0];

                        if(!string.IsNullOrEmpty(colScriptPath))
                        {
                            string colFile = File.ReadAllText(colScriptPath);

                            string colHeader = colFile.Substring(0, colFile.IndexOf('{'));
                            colHeader = colHeader.Replace(" ", "");
                            colHeader = colHeader.Replace("\t", "");

                            if(colHeader.IndexOf(":DynamicBoneColliderBase") == -1)
                            {
                                header = oldBones;
                            }

                            File.WriteAllText(toolScriptPath, header + toolsFile);
                            AssetDatabase.ImportAsset(RelativePath(toolScriptPath));
                        }
                    }
                }
            }
        }                

        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            Check();
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
