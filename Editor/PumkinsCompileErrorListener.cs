using Pumkin.DependencyChecker;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace Pumkin.HelperFunctions
{
    [InitializeOnLoad]
    public class PumkinsCompileErrorListener
    {
        static readonly string[] boneErrors =
        {
        "The type or namespace name 'DynamicBone' could not be found",        
        "Cannot implicitly convert type `System.Collections.Generic.List<DynamicBoneCollider>' to `System.Collections.Generic.List<DynamicBoneColliderBase>'",
        };        
        
        static PumkinsCompileErrorListener()
        {            
            CompilationPipeline.assemblyCompilationFinished += ProcessCompileFinish;
        }

        private static void ProcessCompileFinish(string s, CompilerMessage[] message)
        {            
            var errors = message.Where(m => m.type == CompilerMessageType.Error);
            foreach(var e in errors)    //TODO: Rewrite with LINQ
            {
                foreach(var be in boneErrors)
                {
                    if(e.message.Contains(be))
                    {                        
                        _DependencyChecker.ResetDependencies();
                        return;
                    }
                }                
            }            
        }
    }
}