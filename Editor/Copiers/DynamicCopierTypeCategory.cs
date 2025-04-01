using System;
using System.Collections.Generic;
using System.Linq;
using Pumkin.HelperFunctions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Pumkin.AvatarTools.Copiers
{
    [Serializable]
    internal class DynamicCopierTypesWrapper
    {
        public List<DynamicCopierTypeCategory> extraCopierTypes = new List<DynamicCopierTypeCategory>();
        public bool HasValidTypes { get; private set; }

        internal void Initialize()
        {
            foreach(var extraType in extraCopierTypes)
                extraType.Initialize();

            extraCopierTypes = extraCopierTypes.Where(t => t.types.Count > 0).ToList();
            HasValidTypes = extraCopierTypes.Count > 0;
        }

        public void AddTypeNamesFromWrapper(DynamicCopierTypesWrapper newWrapper)
        {
            // Look for matching category by name, if found add it's types to the template, if not add the whole category
            foreach(var newCategory in newWrapper.extraCopierTypes)
            {
                var foundCategory = extraCopierTypes.FirstOrDefault(t => t.categoryName.Equals(newCategory.categoryName, StringComparison.OrdinalIgnoreCase));
                if(foundCategory != null)
                {
                    foundCategory.typeFullNames.AddRange(newCategory.typeFullNames);
                    foundCategory.typeFullNames = foundCategory.typeFullNames.Distinct().ToList();
                }
                else
                {
                    extraCopierTypes.Add(newCategory);
                }
            }
        }
    }

    [Serializable]
    internal class DynamicCopierTypeCategory
    {
        public string categoryName;
        [FormerlySerializedAs("typeNames")] public List<string> typeFullNames;

        [NonSerialized] public List<Type> types;
        [NonSerialized] public List<bool> enableStates;
        [NonSerialized] public List<string> names;

        internal void Initialize()
        {
            types = new List<Type>();
            enableStates = new List<bool>();
            names = new List<string>();

            if(typeFullNames == null)
                return;
            
            foreach(var name in typeFullNames)
            {
                if(name.EndsWith(".*"))
                {
                    var newTypes = TypeHelpers.GetTypesFromNamespace(name);
                    types.AddRange(newTypes);
                    names.AddRange(newTypes.Select(t => ObjectNames.NicifyVariableName(t.Name)));
                    enableStates.AddRange(Enumerable.Repeat(false, types.Count));
                    continue;
                }

                var type = TypeHelpers.GetTypeAnywhere(name);
                if(type == null)
                    continue;

                types.Add(type);
                names.Add(ObjectNames.NicifyVariableName(type.Name));
                enableStates.Add(false);
            }
        }
    }
}