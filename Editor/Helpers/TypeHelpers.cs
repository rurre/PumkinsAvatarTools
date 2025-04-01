using System;
using System.Collections.Generic;
using System.Linq;

namespace Pumkin.HelperFunctions
{
    internal static class TypeHelpers
    {
        /// <summary>
        /// Gets type from full name from all assemblies
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns>Type or null</returns>
        public static Type GetTypeAnywhere(string typeName)
        {
            if(string.IsNullOrWhiteSpace(typeName))
                return null;

            var type = Type.GetType(typeName);
            if(type != null)
                return type;
            foreach(var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = a.GetType(typeName, false, true);
                if(type != null)
                    return type;
            }
            return null;
        }
        
        public static Type[] GetTypesFromNamespace(string namespaceName)
        {
            if(namespaceName.EndsWith(".*"))
                namespaceName = namespaceName.Substring(0, namespaceName.Length - 2);
            
            if(string.IsNullOrWhiteSpace(namespaceName))
                return Array.Empty<Type>();
            
            List<Type> types = new List<Type>();
            foreach(var ass in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach(var type in ass.GetTypes())
                {
                    if(type.FullName?.StartsWith(namespaceName) ?? false)
                       types.Add(type);
                }
            }
            
            return types.ToArray();
        }
    }
}