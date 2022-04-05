using System;
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
        static SettingsContainer Settings => PumkinsAvatarTools.Settings;
        
        public static void CopyComponent<T>(GameObject from, GameObject to, bool createGameObjects, bool useIgnoreList) where T : Component
        {
            if(from == null || to == null)
                return;

            var typeFromArr = from.GetComponentsInChildren<T>(true);
            if(typeFromArr == null || typeFromArr.Length == 0)
                return;

            string type = typeof(T).Name;

            foreach(var typeFrom in typeFromArr)
            {
                var tTo = Helpers.FindTransformInAnotherHierarchy(typeFrom.transform, to.transform, createGameObjects);
                if((!tTo) || (useIgnoreList && Helpers.ShouldIgnoreObject(typeFrom.transform, Settings._copierIgnoreArray, Settings.bCopier_ignoreArray_includeChildren)))
                    continue;

                string log = String.Format(Strings.Log.copyAttempt, type, typeFrom.gameObject.name, tTo.gameObject.name);

                if(!tTo.GetComponent<T>())
                {
                    T[] oldComps = tTo.GetComponents<T>();
                    ComponentUtility.CopyComponent(typeFrom);
                    ComponentUtility.PasteComponentAsNew(tTo.gameObject);
                    PumkinsAvatarTools.Log(log + " - " + Strings.Log.success, LogType.Log);

                    T[] newComps = tTo.GetComponents<T>();
                    T comp = newComps.Except(oldComps).FirstOrDefault();
                    FixReferences(comp, to.transform, createGameObjects);
                }
                else
                {
                    PumkinsAvatarTools.Log(log + " - " + Strings.Log.failedAlreadyHas, LogType.Log);
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

            serialComp.ApplyModifiedProperties();
        }

    }
}