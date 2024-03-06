﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Pumkin.DataStructures;
using Pumkin.Extensions;
using Pumkin.HelperFunctions;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Pumkin.AvatarTools.Copiers
{
    public static class GenericCopier
    {
        static readonly Dictionary<string, string[]> AdjustScaleTypeProps = new Dictionary<string, string[]>()
        {
            {"VRCPhysBone", new [] { "radius", "endpointPosition" }},
            {"VRCPhysBoneCollider", new [] { "radius", "height" }},
            {"VRCContactReceiver", new [] { "radius", "height", "position" }},
            {"VRCContactSender", new [] { "radius", "height", "position" }},
            {"DynamicBone", new [] { "m_Radius", "m_EndLength" }},
            {"DynamicBoneCollider", new [] { "m_Radius", "m_Height" }}
        };

        static MethodInfo CopyComponentGeneric;

        static GenericCopier()
        {
            CopyComponentGeneric = typeof(GenericCopier).GetMethods(BindingFlags.Public | BindingFlags.Static)
                                                        .FirstOrDefault(m => m.Name == nameof(CopyComponent) && m.IsGenericMethod);
        }

        static SettingsContainer Settings => PumkinsAvatarTools.Settings;

        public static void CopyComponent(Type type, CopyInstance inst, bool createGameObjects, bool adjustScale, bool fixReferences, bool onlyAllowOneComponentOfSameType, HashSet<Transform> ignoreSet)
        {
            if(type == null || !typeof(Component).IsAssignableFrom(type))
            {
                PumkinsAvatarTools.LogVerbose($"Attempting to copy type '{type}' which isn't a component. Skipping.", LogType.Error);
                return;
            }

            var method = CopyComponentGeneric.MakeGenericMethod(type);
            method.Invoke(null, new object[] { inst, createGameObjects, adjustScale, fixReferences, onlyAllowOneComponentOfSameType, ignoreSet });
        }

        public static void CopyComponent<T>(CopyInstance inst, bool createGameObjects, bool adjustScale, bool fixReferences, bool onlyAllowOneComponentOfSameType, HashSet<Transform> ignoreSet) where T : Component
        {
            if(inst.from == null || inst.to == null)
                return;

            var typeFromArr = inst.from.GetComponentsInChildren<T>(true);
            if(typeFromArr == null || typeFromArr.Length == 0)
                return;

            string typeName = typeof(T).Name;

            var addedComponents = new List<T>();

            foreach(var typeFrom in typeFromArr)
            {
                if(Helpers.ShouldIgnoreObject(typeFrom.transform, ignoreSet, Settings.bCopier_ignoreArray_includeChildren))
                    continue;

                var tTo = Helpers.FindTransformInAnotherHierarchy(typeFrom.transform, inst.from.transform, inst.to.transform, createGameObjects);
                if(!tTo)
                    continue;

                string log = String.Format(Strings.Log.copyAttempt, typeName, typeFrom.gameObject.name, tTo.gameObject.name);

                var type = typeFrom.GetType(); // Fixes abstract types not working since you can't add those
                T oldComp = tTo.GetComponents(type).Except(addedComponents).FirstOrDefault() as T;

                T newComp = onlyAllowOneComponentOfSameType && oldComp ? oldComp : tTo.gameObject.AddComponent(type) as T;
                if(newComp == null)
                    continue;

                addedComponents.Add(newComp);

                ComponentUtility.CopyComponent(typeFrom);
                ComponentUtility.PasteComponentValues(newComp);
                PumkinsAvatarTools.Log(log + " - " + Strings.Log.success);

                if(fixReferences)
                {
                    var objRef = GetReferencesToFixLater(newComp, inst.from.transform, inst.to.transform, createGameObjects);
                    if(objRef != null)
                        inst.propertyRefs.Add(objRef);
                    //FixReferences(newComp, inst.to.transform, createGameObjects);
                }

                if(adjustScale)
                {
                    string[] propToAdjust = AdjustScaleTypeProps.FirstOrDefault(kv => typeName.Equals(kv.Key)).Value;
                    if(propToAdjust == null || propToAdjust.Length == 0)
                        PumkinsAvatarTools.Log($"_Attempting to adjust scale on {tTo.name} for {typeName} but no properties to adjust found. Skipping");
                    else
                        AdjustScale(newComp, typeFrom.transform, propToAdjust);
                }
            }
        }

        public static CopierPropertyReference GetReferencesToFixLater(Component newComp, Transform currentHierarchyRoot, Transform targetHierarchyRoot, bool createGameObjects)
        {
            if(!newComp)
                return null;

            var serialComp = new SerializedObject(newComp);

            var trans = new List<Transform>();
            var props = new List<string>();

            serialComp.ForEachPropertyVisible(true, x =>
            {
                if(x.propertyType != SerializedPropertyType.ObjectReference || x.name == "m_Script")
                    return;

                Transform compTransform;
                if(x.objectReferenceValue is GameObject obj)
                    compTransform = obj.transform;
                else if(x.objectReferenceValue is Component comp)
                    compTransform = comp.transform;
                else
                    return;

                if(compTransform.gameObject.scene.name == null) // Don't fix if we're referencing an asset
                    return;

                var transTarget = Helpers.FindTransformInAnotherHierarchy(compTransform, currentHierarchyRoot, targetHierarchyRoot, createGameObjects);
                if(transTarget == null)
                    return;

                props.Add(x.propertyPath);
                trans.Add(transTarget);
            });

            return new CopierPropertyReference()
            {
                serialiedObject = serialComp,
                propertyPaths = props.ToArray(),
                targetTransforms = trans.ToArray()
            };
        }

        public static void FixInstanceReferences(CopyInstance inst)
        {
            foreach(var objRef in inst.propertyRefs)
            {
                if(objRef.propertyPaths == null || objRef.propertyPaths.Length == 0 || objRef.targetTransforms == null || objRef.targetTransforms.Length == 0)
                    continue;

                SerializedObject newObj = new SerializedObject(objRef.serialiedObject.targetObject);

                for(int i = 0; i < objRef.targetTransforms.Length; i++)
                {
                    SerializedProperty prop = newObj.FindProperty(objRef.propertyPaths[i]);
                    if(prop.objectReferenceValue is GameObject obj)
                    {
                        Transform targetTransform = Helpers.FindTransformInAnotherHierarchy(obj.transform, inst.from.transform, inst.to.transform, false);
                        if(!targetTransform)
                        {
                            PumkinsAvatarTools.Log($"_Attempted to fix reference for {obj.name}, but the object wasn't found on {inst.to.name}");
                            continue;
                        }
                        prop.objectReferenceValue = targetTransform.gameObject;
                    }
                    else if(prop.objectReferenceValue is Component comp)
                    {
                        Type compType = comp.GetType();
                        int compIndex = comp.gameObject.GetComponents(compType)
                                               .ToList()
                                               .IndexOf(comp);
                        var targetComps = objRef.targetTransforms[i].GetComponents(compType);
                        prop.objectReferenceValue = targetComps.Length > 0 ? targetComps[compIndex] : null;
                    }
                }
                newObj.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        public static void FixReferencesOnComponent(Component newComp, Transform currentHierarchyRoot, Transform targetHierarchyRoot, bool createGameObjects)
        {
            if(!newComp || newComp is Transform)
                return;

            var serialComp = new SerializedObject(newComp);

            serialComp.ForEachPropertyVisible(true, x =>
            {
                try
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

                    if(oldComp.gameObject.scene.name == null) // Don't fix if we're referencing an asset
                        return;

                    var transTarget = Helpers.FindTransformInAnotherHierarchy(oldComp.transform, currentHierarchyRoot, targetHierarchyRoot, createGameObjects);
                    if(transTarget == null)
                        return;

                    var targetComps = transTarget.GetComponents(compType);

                    x.objectReferenceValue = targetComps[compIndex];
                }
                catch(Exception e)
                {
                    PumkinsAvatarTools.Log($"_Error fixing reference on {newComp.name}: {e.Message}. Failed on property: {x.propertyPath}", LogType.Error);
                }
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

        public static void CopyPrefabs(CopyInstance inst, bool createGameObjects, bool adjustScale, bool fixReferences, bool copyPropertyOverrides, bool addPrefabsToIgnoreList, HashSet<Transform> ignoreSet)
        {
            List<GameObject> prefabs = new List<GameObject>();
            foreach(var trans in inst.from.GetComponentsInChildren<Transform>(true))
            {
                var pref = PrefabUtility.GetNearestPrefabInstanceRoot(trans);
                if(pref && pref.transform != inst.from.transform && !prefabs.Contains(pref))
                {
                    // Check if prefab isn't child of another prefab before adding
                    Transform parentPrefab = pref.transform.parent;
                    if(parentPrefab == null || parentPrefab == inst.from.transform || !PrefabUtility.GetNearestPrefabInstanceRoot(parentPrefab))
                        prefabs.Add(pref);
                }
            }

            Transform tTo = inst.to.transform;
            Transform tFrom = inst.from.transform;

            List<Transform> newPrefabTransformsToIgnore = addPrefabsToIgnoreList ? new List<Transform>() : null;

            foreach(var fromPref in prefabs)
            {
                if(Helpers.ShouldIgnoreObject(fromPref.transform, ignoreSet, Settings.bCopier_ignoreArray_includeChildren))
                    continue;

                var prefabStatus = PrefabUtility.GetPrefabInstanceStatus(fromPref);
                if(prefabStatus == PrefabInstanceStatus.MissingAsset)
                {
                    PumkinsAvatarTools.Log($"_Tried to copy prefab with missing asset. {fromPref.name}. Skipping", LogType.Warning);

                    if(addPrefabsToIgnoreList)
                        ignoreSet.Add(fromPref.transform);

                    continue;
                }
                else if(prefabStatus == PrefabInstanceStatus.NotAPrefab)
                {
                    PumkinsAvatarTools.Log($"_Prefab Copier tried to copy object {fromPref.name}, which isn't a prefab. Uh oh.. Skipping");
                    continue;
                }

                PumkinsAvatarTools.Log($"_Attempting to copy prefab {fromPref.name}");

                Transform tToParent = null;
                if(fromPref.transform.parent == tFrom)
                    tToParent = tTo;
                else
                    tToParent = Helpers.FindTransformInAnotherHierarchy(fromPref.transform.parent, inst.from.transform, inst.to.transform, createGameObjects);

                if(!tToParent)
                    continue;

                PropertyModification[] prefabMods = null;
                if(copyPropertyOverrides)
                    prefabMods = PrefabUtility.GetPropertyModifications(fromPref);

                string prefabAssetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(fromPref);
                GameObject toPref = PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(prefabAssetPath)) as GameObject;
                if(!toPref)
                    continue;

                if(copyPropertyOverrides)
                    PrefabUtility.SetPropertyModifications(toPref, prefabMods);

                toPref.transform.SetParent(tToParent, false);
                toPref.name = fromPref.name;

                if(!fixReferences && !adjustScale)
                    continue;

                if(addPrefabsToIgnoreList)
                    newPrefabTransformsToIgnore.Add(fromPref.transform);

                Component[] prefComponents = toPref.GetComponentsInChildren<Component>(true);
                foreach(var comp in prefComponents)
                {
                    if(fixReferences)
                    {
                        var refs = GetReferencesToFixLater(comp, tFrom, tTo, createGameObjects);
                        inst.propertyRefs.Add(refs);
                    }

                     if(adjustScale)
                     {
                         string typeName = comp.GetType().Name;
                         string[] propToAdjust = AdjustScaleTypeProps.FirstOrDefault(kv => typeName.Equals(kv.Key)).Value;
                         if(propToAdjust == null || propToAdjust.Length == 0)
                             PumkinsAvatarTools.Log($"_Attempting to adjust scale on {tTo.name} for {typeName} but no properties to adjust found. Skipping");
                         else
                         {
                             Transform fromTrans = Helpers.FindTransformInAnotherHierarchy(comp.transform, tTo, tFrom, false);
                             AdjustScale(comp, fromTrans, propToAdjust);
                         }
                     }
                }
            }

            if(addPrefabsToIgnoreList) // Add the new prefabs to our ignore set so that they don't get their stuff copied again
            {
                var allChildren = newPrefabTransformsToIgnore.GetAllChildrenOfTransforms();
                ignoreSet.AddRange(allChildren);
            }
        }
    }
}