using System;

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
    }
}