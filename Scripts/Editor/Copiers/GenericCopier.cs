using System;
using System.Collections.Generic;
using System.Linq;
using Pumkin.DataStructures;
using Pumkin.Extensions;
using Pumkin.HelperFunctions;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
#if PUMKIN_FINALIK
using RootMotion.FinalIK;
#endif

namespace Pumkin.AvatarTools.Copiers
{
    public static class GenericCopier
    {
        static readonly Dictionary<string, string[]> AdjustScaleTypeProps = new Dictionary<string, string[]>()
        {
            {"VRCPhysBone", new [] { "radius", "endpointPosition" }},
            {"VRCPhysBoneCollider", new [] { "radius", "height", "position" }},
            {"VRCContactReceiver", new [] { "radius", "height", "position" }},
            {"VRCContactSender", new [] { "radius", "height", "position" }},
            {"DynamicBone", new [] { "m_Radius", "m_EndLength" }},
            {"DynamicBoneCollider", new [] { "m_Radius", "m_Height" }}
        };
        
        static SettingsContainer Settings => PumkinsAvatarTools.Settings;
        
        public static void CopyComponent<T>(GameObject from, GameObject to, bool createGameObjects, bool adjustScale, bool fixReferences, bool useIgnoreList) where T : Component
        {
            if(from == null || to == null)
                return;

            var typeFromArr = from.GetComponentsInChildren<T>(true);
            if(typeFromArr == null || typeFromArr.Length == 0)
                return;

            string typeName = typeof(T).Name;
            
            var addedComponents = new List<T>();

            foreach(var typeFrom in typeFromArr)
            {
                var tTo = Helpers.FindTransformInAnotherHierarchy(typeFrom.transform, to.transform, createGameObjects);
                if((!tTo) || (useIgnoreList && Helpers.ShouldIgnoreObject(typeFrom.transform, Settings._copierIgnoreArray, Settings.bCopier_ignoreArray_includeChildren)))
                    continue;

                string log = String.Format(Strings.Log.copyAttempt, typeName, typeFrom.gameObject.name, tTo.gameObject.name);

                if(tTo.GetComponents<T>().Except(addedComponents).FirstOrDefault() == null) // Kinda inefficient but whatever
                {
                    T newComp = tTo.gameObject.AddComponent<T>();
                    addedComponents.Add(newComp);
                    
                    ComponentUtility.CopyComponent(typeFrom);
                    ComponentUtility.PasteComponentValues(newComp);
                    PumkinsAvatarTools.Log(log + " - " + Strings.Log.success);

                    if(fixReferences)
                        FixReferences(newComp, to.transform, createGameObjects);

                    if(adjustScale)
                    {
                        string[] propToAdjust = AdjustScaleTypeProps.FirstOrDefault(kv => typeName.Equals(kv.Key)).Value;
                        if(propToAdjust == null || propToAdjust.Length == 0)
                            PumkinsAvatarTools.Log($"_Attempting to adjust scale on {tTo.name} for {typeName} but no properties to adjust found. Skipping");
                        else
                            AdjustScale(newComp, typeFrom.transform, propToAdjust);
                    }
                }
                else
                {
                    PumkinsAvatarTools.Log($"{log} {String.Format(Strings.Log.failedAlreadyHas, to.name, typeName)}", LogType.Warning);
                }
            }
        }
        
        static void FixReferences(Component newComp, Transform targetHierarchyRoot, bool createGameObjects)
        {
            if(!newComp)
                return;

            var serialComp = new SerializedObject(newComp);

            serialComp.ForEachPropertyVisible(true, x =>
            {
                if(x.propertyType != SerializedPropertyType.ObjectReference || x.name == "m_Script")
                    return;

                var oldComp = x.objectReferenceValue as Component;
                if(!oldComp)
                    return;

                Type compType = oldComp.GetType();
                int compIndex = oldComp.gameObject.GetComponents(compType)
                                       .ToList()
                                       .IndexOf(oldComp);

                var transTarget = Helpers.FindTransformInAnotherHierarchy(oldComp.transform, targetHierarchyRoot, createGameObjects);
                if(transTarget == null)
                    return;

                var targetComps = transTarget.GetComponents(compType);

                x.objectReferenceValue = targetComps[compIndex];
            });

            serialComp.ApplyModifiedPropertiesWithoutUndo();
        }

        static void AdjustScale(Component newComp, Transform oldCompTransform, params string[] propNames)
        {
            if(newComp == null || oldCompTransform == null || propNames == null || propNames.Length == 0)
                return;
            
            var serialComp = new SerializedObject(newComp);
            float multiplier = Helpers.GetScaleMultiplier(oldCompTransform, newComp.transform);

            if(multiplier == 1)
                return;
            
            foreach(string name in propNames)
            {
                var prop = serialComp.FindProperty(name);
                if(prop.propertyType == SerializedPropertyType.Float)
                    prop.floatValue *= multiplier;
                else if(prop.propertyType == SerializedPropertyType.Vector3)
                    prop.vector3Value *= multiplier;
                else if(prop.propertyType == SerializedPropertyType.Vector2)
                    prop.vector2Value *= multiplier;
            }
            
            serialComp.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}