using System;
using System.Collections.Generic;
using System.Linq;
using Pumkin.DataStructures;
using Pumkin.HelperFunctions;
using UnityEditor;
using UnityEngine;

namespace Pumkin.AvatarTools.Destroyers
{
    internal static class LegacyDestroyer
    {
        /// <summary>
        /// Destroys ParticleSystem in object
        /// </summary>
        /// <param name="destroyInChildrenToo">Whether to destroy particle systems in children as well</param>
        internal static void DestroyParticleSystems(GameObject from, bool destroyInChildrenToo = true)
        {
            var sysList = new List<ParticleSystem>();
            if(destroyInChildrenToo)
                sysList.AddRange(from.GetComponentsInChildren<ParticleSystem>(true));
            else
                sysList.AddRange(from.GetComponents<ParticleSystem>());

            foreach(var p in sysList)
            {
                var rend = p.GetComponent<ParticleSystemRenderer>();

                if(rend != null)
                    GameObject.DestroyImmediate(rend);

                PumkinsAvatarTools.Log(Strings.Log.removeAttempt + " - " + Strings.Log.success, LogType.Log, p.ToString(), from.name);
                GameObject.DestroyImmediate(p);
            }
        }

        /// <summary>
        /// Destroys GameObjects in object and all children, if it has no children and if it's not a bone
        /// </summary>
        internal static void DestroyEmptyGameObjects(GameObject from)
        {
            var obj = from.GetComponentsInChildren<Transform>(true);
            var renders = from.GetComponentsInChildren<SkinnedMeshRenderer>(true);

            var bones = new HashSet<Transform>();

            foreach(var r in renders)
            {
                foreach(var b in r.bones)
                {
                    bones.Add(b);
                }
            }

            foreach(var t in obj.OrderBy(o => o.childCount))
            {
                if(t != null && t != t.root && t.GetComponents<Component>().Length == 1 && !bones.Contains(t))
                {
                    int c = t.childCount;
                    for(int i = 0; i < t.childCount; i++)
                    {
                        var n = t.GetChild(i);
                        if(!bones.Contains(n))
                            c--;
                    }
                    if(c <= 0 && (t.name.ToLower() != (t.parent.name.ToLower() + "_end")))
                    {
                        if(PrefabUtility.GetPrefabInstanceStatus(t) == PrefabInstanceStatus.NotAPrefab || PrefabUtility.GetPrefabInstanceStatus(t) == PrefabInstanceStatus.Disconnected)
                        {
                            PumkinsAvatarTools.Log(Strings.Log.hasNoComponentsOrChildrenDestroying, LogType.Log, t.name);
                            GameObject.DestroyImmediate(t.gameObject);
                        }
                        else
                        {
                            PumkinsAvatarTools.Log(Strings.Log.cantBeDestroyedPartOfPrefab, LogType.Warning, t.name, "GameObject");
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Destroys all Missing Script components on avatar
        /// </summary>
        internal static void DestroyMissingScripts(GameObject avatar)
        {
            #if UNITY_2018
            if(EditorApplication.isPlaying)
            {
                PumkinsAvatarTools.Log("Can't remove missing scripts in play mode, it causes crashes", LogType.Warning);
                return;
            }
            #endif

            var ts = avatar.GetComponentsInChildren<Transform>(true);
            foreach(var t in ts)
            {
                if(Helpers.DestroyMissingScriptsInGameObject(t.gameObject))
                    PumkinsAvatarTools.Log(Strings.Log.hasMissingScriptDestroying, LogType.Log, Helpers.GetTransformPath(t, avatar.transform));
            }
        }

        /// <summary>
        /// Destroy all components of type in object and it's children
        /// </summary>
        internal static void DestroyAllComponentsOfType(GameObject obj, Type type, bool ignoreRoot, bool useIgnoreList)
        {
			if(type == null)
			{
				PumkinsAvatarTools.Log("Invalid type to destroy", LogType.Error);
				return;
			}

            Transform oldParent = obj.transform.parent;
            obj.transform.parent = null;

            try
            {
                string log = "";

                Component[] comps = obj.transform.GetComponentsInChildren(type, true);

                if(comps != null && comps.Length > 0)
                {
                    for(int i = 0; i < comps.Length; i++)
                    {
                        if((ignoreRoot && comps[i].transform.parent == null) ||
                           (useIgnoreList && Helpers.ShouldIgnoreObject(comps[i].transform, PumkinsAvatarTools.Settings._copierIgnoreArray,
                               PumkinsAvatarTools.Settings.bCopier_ignoreArray_includeChildren)))
                            continue;

                        log = Strings.Log.removeAttempt + " - ";
                        string name = comps[i].name;

                        if(!PrefabUtility.IsPartOfPrefabInstance(comps[i]))
                        {
                            try
                            {
                                Helpers.DestroyAppropriate(comps[i]);
                                log += Strings.Log.success;
                                PumkinsAvatarTools.Log(log, LogType.Log, type.Name, name);
                            }
                            catch(Exception e)
                            {
                                log += Strings.Log.failed + " - " + e.Message;
                                PumkinsAvatarTools.Log(log, LogType.Exception, type.Name, name);
                            }
                        }
                        else
                        {
                            PumkinsAvatarTools.Log(Strings.Log.cantBeDestroyedPartOfPrefab, LogType.Warning, name, type.Name);
                        }
                    }
                }
            }
            finally
            {
                obj.transform.parent = oldParent;
            }
        }
    }
}