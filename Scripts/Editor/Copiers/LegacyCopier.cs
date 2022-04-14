using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pumkin.AvatarTools.Destroyers;
using Pumkin.DataStructures;
using Pumkin.Extensions;
using Pumkin.HelperFunctions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Animations;

#if VRC_SDK_VRCSDK2 || VRC_SDK_VRCSDK3
using VRC.Core;
using VRC.SDKBase;
#endif
#if (VRC_SDK_VRCSDK2 || VRC_SDK_VRCSDK3) && PUMKIN_PBONES
using VRC.SDK3.Dynamics.PhysBone.Components;
using VRC.SDK3.Dynamics.Contact.Components;
#endif
#if VRC_SDK_VRCSDK3 && !UDON
using VRC_AvatarDescriptor = VRC.SDK3.Avatars.Components.VRCAvatarDescriptor;
using VRC_SpatialAudioSource = VRC.SDK3.Avatars.Components.VRCSpatialAudioSource;
#elif VRC_SDK_VRCSDK2
using VRC_AvatarDescriptor = VRCSDK2.VRC_AvatarDescriptor;
using VRC_SpatialAudioSource = VRCSDK2.VRC_SpatialAudioSource;
#endif

#if PUMKIN_FINALIK
using RootMotion.FinalIK;
#endif

namespace Pumkin.AvatarTools.Copiers
{
    internal static class LegacyCopier
    {
        static SettingsContainer Settings => PumkinsAvatarTools.Settings;

        internal static void CopyAllSpringVRMSpringBones(GameObject from, GameObject to, bool createGameObjects, bool useIgnoreList)
        {
            Type type = Type.GetType("VRM.VRMSpringBone");

            if(from == null || to == null || type == null)
                return;

            var vrmFromArr = from.GetComponentsInChildren(type, true);
            if(vrmFromArr == null || vrmFromArr.Length == 0)
                return;


            for(int i = 0; i < vrmFromArr.Length; i++)
            {
                var vrmFrom = vrmFromArr[i];
                var tTo = Helpers.FindTransformInAnotherHierarchy(vrmFrom.transform, to.transform, createGameObjects);
                if(!tTo || useIgnoreList && Helpers.ShouldIgnoreObject(vrmFrom.transform, Settings._copierIgnoreArray, Settings.bCopier_ignoreArray_includeChildren))
                    continue;

                string log = String.Format(Strings.Log.copyAttempt, type.Name, vrmFrom.gameObject, tTo.gameObject);

                if(!tTo.GetComponent(type))
                {
                    ComponentUtility.CopyComponent(vrmFrom);
                    ComponentUtility.PasteComponentAsNew(tTo.gameObject);
                    PumkinsAvatarTools.Log(Strings.Log.copyAttempt + " - " + Strings.Log.success, LogType.Log);
                }
                else
                {
                    PumkinsAvatarTools.Log(Strings.Log.copyAttempt + " - " + Strings.Log.failedAlreadyHas, LogType.Log);
                }
            }
        }
 
        /// <summary>
        /// Copies all DynamicBoneColliders from object and it's children to another object.
        /// </summary>
        internal static void CopyAllDynamicBoneColliders(GameObject from, GameObject to, bool createGameObjects, bool useIgnoreList, bool adjustScale)
        {
#if !PUMKIN_DBONES && !PUMKIN_OLD_DBONES

            Debug.Log("No DynamicBones found in project. You shouldn't be able to use this. Help!");
            return;
#else
            if(from == null || to == null)
                return;

            var dbcFromArr = from.GetComponentsInChildren<DynamicBoneCollider>(true);
            if(dbcFromArr == null || dbcFromArr.Length == 0)
                return;

            for (int i = 0; i < dbcFromArr.Length; i++)
            {
                var dbcFrom = dbcFromArr[i];
                var tTo = Helpers.FindTransformInAnotherHierarchy(dbcFrom.transform, to.transform, createGameObjects);
                if((!tTo) || (useIgnoreList && Helpers.ShouldIgnoreObject(dbcFrom.transform, Settings._copierIgnoreArray, Settings.bCopier_ignoreArray_includeChildren)))
                    continue;

                var dbcToArr = tTo.GetComponentsInChildren<DynamicBoneCollider>(true);

                bool found = false;
                for (int z = 0; z < dbcToArr.Length; z++)
                {
                    var d = dbcToArr[z];
                    if(d.m_Bound == dbcFrom.m_Bound && d.m_Center == dbcFrom.m_Center &&
                       d.m_Direction == dbcFrom.m_Direction && d.m_Height == dbcFrom.m_Height && d.m_Radius == dbcFrom.m_Radius)
                    {
                        found = true;
                        break;
                    }
                }

                if(!found)
                {
                    ComponentUtility.CopyComponent(dbcFrom);
                    DynamicBoneCollider dbcTo = tTo.gameObject.AddComponent<DynamicBoneCollider>();
                    ComponentUtility.PasteComponentValues(dbcTo);

                    if(adjustScale)
                    {
                        float scaleMu = Helpers.GetScaleMultiplier(dbcFrom.transform, dbcTo.transform);
                        dbcTo.m_Center *= scaleMu;
                        dbcTo.m_Height *= scaleMu;
                        dbcTo.m_Radius *= scaleMu;
                    }
                }

            }
#endif
        }

        /// <summary>
        /// Temporary fixes to dbones using dirty dynamic types
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="createMissing"></param>
        /// <param name="useIgnoreList"></param>
        internal static void CopyAllDynamicBonesNew(GameObject from, GameObject to, bool createMissing, bool useIgnoreList, bool adjustScale)
        {
#if !PUMKIN_DBONES && !PUMKIN_OLD_DBONES
            Debug.Log("No DynamicBones found in project. You shouldn't be able to use this. Help!");
            return;
#else
            if(!from || !to)
                return;

            var dBoneFromArr = from.GetComponentsInChildren<DynamicBone>(true);

            //Handle collider issues
            Type colliderListType = typeof(DynamicBone).GetField("m_Colliders").FieldType;
            Type colliderType = colliderListType.GetGenericArguments().FirstOrDefault();

            List<DynamicBone> newBones = new List<DynamicBone>();
            foreach(var dbFrom in dBoneFromArr)
            {
                if(useIgnoreList && Helpers.ShouldIgnoreObject(dbFrom.transform, Settings._copierIgnoreArray, Settings.bCopier_ignoreArray_includeChildren))
                    continue;

                var transTo = Helpers.FindTransformInAnotherHierarchy(dbFrom.transform, to.transform, Settings.bCopier_dynamicBones_createObjects);
                if(!transTo)
                    continue;

                var dBoneToArr = transTo.GetComponents<DynamicBone>();

                if(!dbFrom.m_Root)
                {
                    PumkinsAvatarTools.LogVerbose("DynamicBone {0} of {1} doesn't have a root assigned. Ignoring", LogType.Warning, 
                        dbFrom.transform.name, dbFrom.transform.root.name);
                    continue;
                }

                bool foundSameDynBone = false;

                foreach(var bone in dBoneToArr)
                {
                    if(!bone.m_Root || newBones.Contains(bone))
                        continue;

                    //Check if the roots are the same to decide if it's supposed to be the same dyn bone script
                    if(bone.m_Root.name == dbFrom.m_Root.name)
                    {
                        //Check if exclusions are the same
                        List<string> exToPaths = bone.m_Exclusions
                            .Where(o => o != null)
                            .Select(o => Helpers.GetTransformPath(o.transform, to.transform).ToLower())
                            .ToList();

                        List<string> exFromPaths = dbFrom.m_Exclusions
                            .Where(o => o != null)
                            .Select(o => Helpers.GetTransformPath(o.transform, from.transform).ToLower())
                            .ToList();

                        bool exclusionsDifferent = false;
                        var exArr = exToPaths.Intersect(exFromPaths).ToArray();

                        if(exArr != null && (exToPaths.Count != 0 && exFromPaths.Count != 0) && exArr.Length == 0)
                            exclusionsDifferent = true;

                        //Check if colliders are the same
                        List<string> colToPaths = bone.m_Colliders
                            .Where(c => c != null)
                            .Select(c => Helpers.GetTransformPath(c.transform, to.transform).ToLower())
                            .ToList();

                        List<string> colFromPaths = bone.m_Colliders
                            .Where(c => c != null)
                            .Select(c => Helpers.GetTransformPath(c.transform, from.transform).ToLower())
                            .ToList();

                        bool collidersDifferent = false;
                        var colArr = colToPaths.Intersect(colFromPaths).ToArray();

                        if(colArr != null && (colToPaths.Count != 0 && colFromPaths.Count != 0) && colArr.Length == 0)
                            collidersDifferent = true;

                        //Found the same bone because root, exclusions and colliders are the same
                        if(!exclusionsDifferent && !collidersDifferent)
                        {
                            foundSameDynBone = true;
                            if(Settings.bCopier_dynamicBones_copySettings)
                            {
                                PumkinsAvatarTools.LogVerbose("{0} already has this DynamicBone, but we have to copy settings. Copying.", LogType.Log, bone.name);

                                bone.m_Damping = dbFrom.m_Damping;
                                bone.m_DampingDistrib = dbFrom.m_DampingDistrib;
                                bone.m_DistanceToObject = dbFrom.m_DistanceToObject;
                                bone.m_DistantDisable = dbFrom.m_DistantDisable;
                                bone.m_Elasticity = dbFrom.m_Elasticity;
                                bone.m_ElasticityDistrib = dbFrom.m_ElasticityDistrib;
                                bone.m_EndLength = dbFrom.m_EndLength;
                                bone.m_EndOffset = dbFrom.m_EndOffset;
                                bone.m_Force = dbFrom.m_Force;
                                bone.m_FreezeAxis = dbFrom.m_FreezeAxis;
                                bone.m_Gravity = dbFrom.m_Gravity;
                                bone.m_Inert = dbFrom.m_Inert;
                                bone.m_InertDistrib = dbFrom.m_InertDistrib;
                                bone.m_Radius = dbFrom.m_Radius;
                                bone.m_RadiusDistrib = dbFrom.m_RadiusDistrib;
                                bone.m_Stiffness = dbFrom.m_Stiffness;
                                bone.m_StiffnessDistrib = dbFrom.m_StiffnessDistrib;

                                bone.m_ReferenceObject = Helpers.FindTransformInAnotherHierarchy(dbFrom.m_ReferenceObject, bone.transform, false);
                            
                                if(adjustScale)
                                {
                                    float scaleMul = Helpers.GetScaleMultiplier(dbFrom.transform, bone.transform);
                                    bone.m_Radius *= scaleMul;
                                    bone.m_EndLength *= scaleMul;
                                }
                            }
                            else
                            {
                                PumkinsAvatarTools.LogVerbose("{0} already has this DynamicBone but we aren't copying settings. Ignoring", LogType.Log, bone.name);
                            }
                            break;
                        }
                    }
                }

                if(!foundSameDynBone)
                {
                    if(createMissing)
                    {
                        PumkinsAvatarTools.LogVerbose("{0} doesn't have this DynamicBone but we have to create one. Creating.", LogType.Log, dbFrom.name);

                        var newDynBone = transTo.gameObject.AddComponent<DynamicBone>();
                        ComponentUtility.CopyComponent(dbFrom);
                        ComponentUtility.PasteComponentValues(newDynBone);

                        if(adjustScale)
                        {
                            float scaleMul = Helpers.GetScaleMultiplier(dbFrom.transform, newDynBone.transform);
                            newDynBone.m_Radius *= scaleMul;
                            newDynBone.m_EndLength *= scaleMul;
                        }

                        newDynBone.m_Root = Helpers.FindTransformInAnotherHierarchy(dbFrom.m_Root.transform, newDynBone.transform.root, false);

                        if(!newDynBone.m_Root)
                        {
                            PumkinsAvatarTools.Log("_Couldn't set root {0} for new DynamicBone in {1}'s {2}. GameObject is missing. Removing.", LogType.Warning, dbFrom.m_Root.name ?? "null", newDynBone.transform.root.name, newDynBone.transform.name == newDynBone.transform.root.name ? "root" : newDynBone.transform.root.name);
                            PumkinsAvatarTools.DestroyImmediate(newDynBone);
                            continue;
                        }

                        if(dbFrom.m_ReferenceObject)
                            newDynBone.m_ReferenceObject = Helpers.FindTransformInAnotherHierarchy(dbFrom.m_ReferenceObject, newDynBone.transform.root, false);

                        dynamic newColliders = Activator.CreateInstance(colliderListType);

                        for(int i = 0; i < newDynBone.m_Colliders.Count; i++)
                        {
                            var badRefCollider = newDynBone.m_Colliders[i];

                            if(!badRefCollider)
                                continue;

                            dynamic fixedRefCollider = null;

                            var t = Helpers.FindTransformInAnotherHierarchy(newDynBone.m_Colliders[i].transform, to.transform, false);

                            if(t == null)
                                continue;

                            dynamic[] toColls = t.GetComponents(colliderType);
                            foreach(var c in toColls)
                            {
                                if(c.m_Bound == badRefCollider.m_Bound && c.m_Center == badRefCollider.m_Center && c.m_Direction == badRefCollider.m_Direction &&
                                   !newDynBone.m_Colliders.Contains(c))
                                    fixedRefCollider = c;
                            }

                            if(fixedRefCollider != null)
                            {
                                PumkinsAvatarTools.LogVerbose("Fixed reference for {0} in {1}", LogType.Log, fixedRefCollider.name, newDynBone.name);
                                newColliders.Add(fixedRefCollider);
                            }
                        }

                        newDynBone.m_Colliders = newColliders;

                        var newExclusions = new HashSet<Transform>();

                        foreach(var ex in newDynBone.m_Exclusions)
                        {
                            if(!ex)
                                continue;

                            var t = Helpers.FindTransformInAnotherHierarchy(ex.transform, to.transform, false);
                            if(t)
                                newExclusions.Add(t);
                        }

                        newDynBone.m_Exclusions = newExclusions.ToList();
                        newBones.Add(newDynBone);

                        PumkinsAvatarTools.Log(Strings.Log.copiedDynamicBone, LogType.Log, dbFrom.transform.root.name, dbFrom.transform.name == dbFrom.transform.root.name ? "root" : dbFrom.transform.name, transTo.root.name);
                    }
                    else
                    {
                        PumkinsAvatarTools.LogVerbose("{0} doesn't have has this DynamicBone and we aren't creating a new one. Ignoring.", LogType.Log, dbFrom.name);
                    }
                }
            }
#endif
        }

        /// <summary>
        /// Copies Box, Capsule, Sphere and Mesh colliders from one object to another and all of it's children at once.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        internal static void CopyAllColliders(GameObject from, GameObject to, bool createGameObjects, bool useIgnoreList, bool adjustScale)
        {
            if(from == null || to == null)
                return;
            if(!(Settings.bCopier_colliders_copyBox || Settings.bCopier_colliders_copyCapsule || Settings.bCopier_colliders_copyMesh || Settings.bCopier_colliders_copySphere))
                return;

            var cFromArr = from.GetComponentsInChildren<Collider>(true);

            for(int i = 0; i < cFromArr.Length; i++)
            {
                string log = Strings.Log.copyAttempt;
                var type = cFromArr[i].GetType();

                var cc = cFromArr[i];
                var cFromPath = Helpers.GetTransformPath(cc.transform, from.transform);

                if(useIgnoreList && Helpers.ShouldIgnoreObject(cc.transform, Settings._copierIgnoreArray, Settings.bCopier_ignoreArray_includeChildren))
                    continue;

                if(cFromPath != null)
                {
                    var tTo = to.transform.root.Find(cFromPath, createGameObjects, cc.transform);

                    if(!tTo)
                        continue;

                    GameObject cToObj = tTo.gameObject;

                    var cToArr = cToObj.GetComponents<Collider>();
                    bool found = false;

                    for(int z = 0; z < cToArr.Length; z++)
                    {
                        if(Helpers.CollidersAreIdentical(cToArr[z], cFromArr[i]))
                        {
                            found = true;
                            PumkinsAvatarTools.Log(log + " - " + Strings.Log.failedAlreadyHas, LogType.Warning, cToObj.name, type.ToString());
                            break;
                        }
                    }
                    if(!found)
                    {
                        ComponentUtility.CopyComponent(cFromArr[i]);
                        ComponentUtility.PasteComponentAsNew(cToObj);

                        if(adjustScale)
                        {
                            Collider c = cToObj.GetComponents<Collider>().Last();
                            float mul = Helpers.GetScaleMultiplier(cFromArr[i].transform, cToObj.transform);
                            if(c is SphereCollider sphere)
                            {
                                sphere.center *= mul;
                                sphere.radius *= mul;
                            }
                            if(c is BoxCollider box)
                            {
                                box.center *= mul;
                                box.size *= mul;
                            }
                            if(c is CapsuleCollider capsule)
                            {
                                capsule.center *= mul;
                                capsule.radius *= mul;
                                capsule.height *= mul;
                            }
                        }
                        
                        PumkinsAvatarTools.Log(log + " - " + Strings.Log.success, LogType.Log, type.ToString(), cFromArr[i].gameObject.name, cToObj.name);
                    }
                }
            }
        }

        /// <summary>
        /// Copies character, configurable, fixed hinge and spring joints from one object to another and all of it's children at once.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        internal static void CopyAllJoints(GameObject from, GameObject to, bool createGameObjects, bool useIgnoreList)
        {
            if(from == null || to == null)
                return;
            if(!(Settings.bCopier_joints_character || Settings.bCopier_joints_configurable
                || Settings.bCopier_joints_fixed || Settings.bCopier_joints_hinge || Settings.bCopier_joints_spring))
                return;

            var jointFromArr = from.GetComponentsInChildren<Joint>(true);

            for(int i = 0; i < jointFromArr.Length; i++)
            {
                var jointFrom = jointFromArr[i];
                var jointTransFrom = jointFrom.transform;

                Type jointType = jointFrom.GetType();
                if((!Settings.bCopier_joints_character && jointType == typeof(CharacterJoint)) ||
                    (!Settings.bCopier_joints_configurable && jointType == typeof(ConfigurableJoint)) ||
                    (!Settings.bCopier_joints_fixed && jointType == typeof(FixedJoint)) ||
                    (!Settings.bCopier_joints_spring && jointType == typeof(SpringJoint)) ||
                    (!Settings.bCopier_joints_hinge && jointType == typeof(CharacterJoint)))
                {
                    PumkinsAvatarTools.Log(Strings.Log.notSelectedInCopierIgnoring, LogType.Log, jointTransFrom.gameObject.name, jointType.Name);
                    continue;
                }

                var jointTransTo = Helpers.FindTransformInAnotherHierarchy(jointTransFrom, to.transform, createGameObjects);

                if(!jointTransTo)
                    continue;

                PumkinsAvatarTools.Log(Strings.Log.copyAttempt, LogType.Log, jointType.Name, jointTransFrom.gameObject.name, jointTransTo.gameObject.name);
                Joint jointTo = jointTransTo.gameObject.AddComponent(jointFrom.GetType()) as Joint;

                ComponentUtility.CopyComponent(jointFrom);
                ComponentUtility.PasteComponentValues(jointTo);

                Transform targetTrans = null;
                Rigidbody targetBody = null;
                if(jointTo.connectedBody != null)
                    targetTrans = Helpers.FindTransformInAnotherHierarchy(jointFrom.connectedBody.transform, to.transform, createGameObjects);
                if(targetTrans != null)
                    targetBody = targetTrans.GetComponent<Rigidbody>();

                jointTo.connectedBody = targetBody;
            }
        }

        /// <summary>
        /// Copies all transform settings in children in object and children
        /// </summary>
        /// <param name="useIgnoreList">Whether or not to use copier ignore list</param>
        internal static void CopyAllTransforms(GameObject from, GameObject to, bool useIgnoreList)
        {
            if(from == null || to == null || !(Settings.bCopier_transforms_copyPosition || Settings.bCopier_transforms_copyRotation 
                   || Settings.bCopier_transforms_copyScale || Settings.bCopier_transforms_copyLayerAndTag || Settings.bCopier_transforms_copyActiveState))
                return;

            string type = typeof(Transform).Name;

            var tFromArr = from.GetComponentsInChildren<Transform>(true);

            for(int i = 0; i < tFromArr.Length; i++)
            {
                Transform tFrom = tFromArr[i];

                if(tFrom == tFrom.root || tFrom == tFrom.root.Find(tFrom.name) ||
                    (useIgnoreList && Helpers.ShouldIgnoreObject(tFrom, Settings._copierIgnoreArray, Settings.bCopier_ignoreArray_includeChildren)))
                    continue;

                string log = String.Format(Strings.Log.copyAttempt + " - ", tFrom.gameObject.name, from.name, to.name);

                Transform tTo = Helpers.FindTransformInAnotherHierarchy(tFrom, to.transform, false);
                if(!tTo) {
                    if(Settings.bCopier_transforms_createMissing) {
                        Transform targetParent = Helpers.FindTransformInAnotherHierarchy(tFrom.parent, to.transform, false);
                        GameObject createdObj = UnityEngine.Object.Instantiate(tFrom.gameObject, targetParent);
                        createdObj.name = tFrom.gameObject.name;
                        tTo = createdObj.transform;
                    } else {
                        PumkinsAvatarTools.Log(log + Strings.Log.failedHasNoIgnoring, LogType.Warning, from.name, tFrom.gameObject.name);
                        continue;
                    }
                }

                if(Settings.bCopier_transforms_copyPosition)
                    tTo.localPosition = tFrom.localPosition;
                if(Settings.bCopier_transforms_copyScale)
                    tTo.localScale = tFrom.localScale;
                if(Settings.bCopier_transforms_copyRotation)
                {
                    tTo.localEulerAngles = tFrom.localEulerAngles;
                    tTo.localRotation = tFrom.localRotation;
                }
                if(Settings.bCopier_transforms_copyLayerAndTag)
                {
                    tTo.gameObject.tag = tFrom.gameObject.tag;
                    tTo.gameObject.layer = tFrom.gameObject.layer;
                }
                if(Settings.bCopier_transforms_copyActiveState)
                    tTo.gameObject.SetActive(tFrom.gameObject.activeSelf);

                PumkinsAvatarTools.Log(log + Strings.Log.success, LogType.Log);
            }
        }

        /// <summary>
        /// Copies settings of all SkinnedMeshRenderers in object and children.
        /// Does NOT copy mesh, bounds and root bone settings because that breaks everything.
        /// </summary>
        internal static void CopyAllSkinnedMeshRenderersSettings(GameObject from, GameObject to, bool useIgnoreList)
        {
            if((from == null || to == null)
               || (!(Settings.bCopier_skinMeshRender_copyBlendShapeValues
               || Settings.bCopier_skinMeshRender_copyMaterials || Settings.bCopier_skinMeshRender_copySettings || Settings.bCopier_skinMeshRender_copyBounds)))
                return;

            string log = String.Format(Strings.Log.copyAttempt + " - ", Strings.Copier.skinMeshRender, from.name, to.name);

            var rFromArr = from.GetComponentsInChildren<SkinnedMeshRenderer>(true);

            for(int i = 0; i < rFromArr.Length; i++)
            {
                var rFrom = rFromArr[i];
                var rFromPath = Helpers.GetTransformPath(rFrom.transform, from.transform);

                if(rFromPath != null)
                {
                    var tTo = to.transform.root.Find(rFromPath);

                    if((!tTo) ||
                        (useIgnoreList && Helpers.ShouldIgnoreObject(rFrom.transform, Settings._copierIgnoreArray, Settings.bCopier_ignoreArray_includeChildren)))
                        continue;

                    GameObject rToObj = tTo.gameObject;

                    var rTo = rToObj.GetComponent<SkinnedMeshRenderer>();

                    if(rTo != null)
                    {
                        if(Settings.bCopier_skinMeshRender_copySettings)
                        {
                            var t = Helpers.FindTransformInAnotherHierarchy(rFrom.rootBone, rTo.transform.root, false);
                            rTo.rootBone = t ? t : rTo.rootBone;
                            t = Helpers.FindTransformInAnotherHierarchy(rFrom.probeAnchor, rTo.transform.root, false);

                            rTo.allowOcclusionWhenDynamic = rFrom.allowOcclusionWhenDynamic;
                            rTo.quality = rFrom.quality;
                            rTo.probeAnchor = t ? t : rTo.probeAnchor;
                            rTo.lightProbeUsage = rFrom.lightProbeUsage;
                            rTo.reflectionProbeUsage = rFrom.reflectionProbeUsage;
                            rTo.shadowCastingMode = rFrom.shadowCastingMode;
                            rTo.receiveShadows = rFrom.receiveShadows;
                            rTo.motionVectorGenerationMode = rFrom.motionVectorGenerationMode;
                            rTo.skinnedMotionVectors = rFrom.skinnedMotionVectors;
                            rTo.allowOcclusionWhenDynamic = rFrom.allowOcclusionWhenDynamic;
                            rTo.enabled = rFrom.enabled;
                        }
                        if(Settings.bCopier_skinMeshRender_copyBlendShapeValues && rFrom.sharedMesh)
                        {
                            for(int z = 0; z < rFrom.sharedMesh.blendShapeCount; z++)
                            {
                                string toShapeName = rFrom.sharedMesh.GetBlendShapeName(z);
                                int toShapeIndex = rTo.sharedMesh.GetBlendShapeIndex(toShapeName);
                                if(toShapeIndex != -1)
                                {
                                    int fromShapeIndex = rFrom.sharedMesh.GetBlendShapeIndex(toShapeName);
                                    if(fromShapeIndex != -1)
                                        rTo.SetBlendShapeWeight(toShapeIndex, rFrom.GetBlendShapeWeight(fromShapeIndex));
                                }
                            }
                        }
                        if(Settings.bCopier_skinMeshRender_copyMaterials)
                            rTo.sharedMaterials = rFrom.sharedMaterials;

                        if(Settings.bCopier_skinMeshRender_copyBounds)
                            rTo.localBounds = rFrom.localBounds;
                        
                        PumkinsAvatarTools.Log(log + Strings.Log.success);
                    }
                    else
                    {
                        PumkinsAvatarTools.Log(log + Strings.Log.failedDoesntHave, LogType.Warning, tTo.gameObject.name, rFrom.GetType().ToString());
                    }
                }
            }
        }

        /// <summary>
        /// Copies all TrailRenderers in object and it's children.
        /// </summary>
        /// <param name="createGameObjects">Whether to create missing GameObjects</param>
        internal static void CopyAllTrailRenderers(GameObject from, GameObject to, bool createGameObjects, bool useIgnoreList)
        {
            if(from == null || to == null)
                return;

            var rFromArr = from.GetComponentsInChildren<TrailRenderer>(true);

            for(int i = 0; i < rFromArr.Length; i++)
            {
                var rFrom = rFromArr[i];
                var tTo = Helpers.FindTransformInAnotherHierarchy(rFrom.transform, to.transform, createGameObjects);

                if(!tTo)
                    continue;

                if(useIgnoreList && Helpers.ShouldIgnoreObject(rFrom.transform, Settings._copierIgnoreArray, Settings.bCopier_ignoreArray_includeChildren))
                    continue;

                var rToObj = tTo.gameObject;
                var rTo = rToObj.GetComponent<TrailRenderer>();

                if(rTo == null && Settings.bCopier_trailRenderers_createMissing)
                {
                    rTo = rToObj.AddComponent<TrailRenderer>();
                }

                if((rTo != null && Settings.bCopier_trailRenderers_copySettings) || Settings.bCopier_trailRenderers_createMissing)
                {
                    ComponentUtility.CopyComponent(rFrom);
                    ComponentUtility.PasteComponentValues(rTo);
                }
            }
        }

        /// <summary>
        /// Copies all RigidBodies in object and in its children.
        /// </summary>
        internal static void CopyAllRigidBodies(GameObject from, GameObject to, bool createGameObjects, bool useIgnoreList)
        {
            if(from == null || to == null)
                return;

            var rFromArr = from.GetComponentsInChildren<Rigidbody>(true);

            for(int i = 0; i < rFromArr.Length; i++)
            {
                var rFrom = rFromArr[i];
                var tTo = Helpers.FindTransformInAnotherHierarchy(rFrom.transform, to.transform, createGameObjects);

                if(!tTo)
                    continue;

                if(useIgnoreList && Helpers.ShouldIgnoreObject(rFrom.transform, Settings._copierIgnoreArray, Settings.bCopier_ignoreArray_includeChildren))
                    continue;

                var rToObj = tTo.gameObject;

                var rTo = rToObj.GetComponent<Rigidbody>();

                if(rTo == null && Settings.bCopier_rigidBodies_createMissing)
                {
                    rTo = rToObj.AddComponent<Rigidbody>();
                }
                if(rTo != null && (Settings.bCopier_rigidBodies_copySettings || Settings.bCopier_rigidBodies_createMissing))
                {
                    ComponentUtility.CopyComponent(rFrom);
                    ComponentUtility.PasteComponentValues(rTo);
                }
            }
        }

        /// <summary>
        /// Copies all ParticleSystems in object and its children
        /// </summary>
        /// <param name="createGameObjects">Whether to create game objects if missing</param>
        internal static void CopyAllParticleSystems(GameObject from, GameObject to, bool createGameObjects, bool useIgnoreList)
        {
            var partSysFromArr = from.GetComponentsInChildren<ParticleSystem>(true);
            ParticleSystem[] partSysToArr = new ParticleSystem[partSysFromArr.Length];
            for(int i = 0; i < partSysFromArr.Length; i++)
            {
                var partSys = partSysFromArr[i];
                if(useIgnoreList && Helpers.ShouldIgnoreObject(partSys.transform, Settings._copierIgnoreArray, Settings.bCopier_ignoreArray_includeChildren))
                    continue;
                var transTo = Helpers.FindTransformInAnotherHierarchy(partSys.transform, to.transform, createGameObjects);
                if(transTo != null)
                {
                    var partSysTo = transTo.GetComponent<ParticleSystem>();
                    if(Settings.bCopier_particleSystems_replace || partSysTo == null)
                    {
                        LegacyDestroyer.DestroyParticleSystems(transTo.gameObject, false);

                        ComponentUtility.CopyComponent(partSys);
                        var newPartSys = transTo.gameObject.AddComponent<ParticleSystem>();
                        ComponentUtility.PasteComponentValues(newPartSys);
                        partSysToArr[i] = newPartSys;

                        PumkinsAvatarTools.Log(Strings.Log.successCopiedOverFromTo, LogType.Log, "ParticleSystem", PumkinsAvatarTools.CopierSelectedFrom.name,
                            partSys.gameObject.name, PumkinsAvatarTools.SelectedAvatar.name, transTo.gameObject.name);
                    }
                    else
                    {
                        PumkinsAvatarTools.Log(Strings.Log.failedAlreadyHas, LogType.Log, partSys.gameObject.name, "ParticleSystem");
                    }
                }
            }

            //Assign Sub-Emitters in 2nd iteration to avoid missing references
            for (int i = 0; i < partSysFromArr.Length; i++)
            {
                if(partSysToArr[i] == null) continue;

                var ogSys = partSysFromArr[i];
                var newSys = partSysToArr[i];

                for (int j = 0; j < ogSys.subEmitters.subEmittersCount; j++)
                {
                    var ogSubEmitter = ogSys.subEmitters.GetSubEmitterSystem(j);
                    newSys.subEmitters.SetSubEmitterSystem(j, Helpers.FindTransformInAnotherHierarchy(ogSubEmitter.transform, to.transform, false).GetComponent<ParticleSystem>());
                }
            }
        }

        /// <summary>
        /// Copies all Aim Constrains in object and it's children
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="createGameObjects">Whether to create game objects if missing</param>
        /// <param name="useIgnoreList"></param>
        internal static void CopyAllAimConstraints(GameObject from, GameObject to, bool createGameObjects, bool useIgnoreList)
        {
            var aimConFromArr = from.GetComponentsInChildren<AimConstraint>(true);
            const string typeString = "AimConstraint";

            for(int i = 0; i < aimConFromArr.Length; i++)
            {
                var aimCon = aimConFromArr[i];

                if(useIgnoreList && Helpers.ShouldIgnoreObject(aimCon.transform, Settings._copierIgnoreArray, Settings.bCopier_ignoreArray_includeChildren))
                    continue;

                var transTo = Helpers.FindTransformInAnotherHierarchy(aimCon.transform, to.transform, createGameObjects);

                if(transTo != null)
                {
                    var aimConTo = transTo.GetComponent<AimConstraint>();

                    if(Settings.bCopier_aimConstraint_replaceOld || aimConTo == null)
                    {
                        Helpers.DestroyAppropriate(aimConTo);

                        ComponentUtility.CopyComponent(aimCon);
                        aimConTo = transTo.gameObject.AddComponent<AimConstraint>();
                        ComponentUtility.PasteComponentValues(aimConTo);

                        if(aimConTo.worldUpType == AimConstraint.WorldUpType.ObjectRotationUp || aimConTo.worldUpType == AimConstraint.WorldUpType.ObjectUp)
                        {
                            var upObj = aimConTo.worldUpObject;
                            if(upObj && upObj.root == from.transform)
                                aimConTo.worldUpObject = Helpers.FindTransformInAnotherHierarchy(upObj, to.transform, createGameObjects);
                        }
                        var sources = new List<ConstraintSource>();
                        aimConTo.GetSources(sources);

                        for(int z = 0; z < sources.Count; z++)
                        {
                            var t = sources[z];
                            if(t.sourceTransform && t.sourceTransform.root == from.transform)
                            {
                                var cs = sources[z];
                                cs.sourceTransform = Helpers.FindTransformInAnotherHierarchy(t.sourceTransform, to.transform, createGameObjects);
                                aimConTo.SetSource(z, cs);
                            }
                        }

                        if(Settings.bCopier_aimConstraint_onlyIfHasValidSources && !Helpers.ConstraintHasValidSources(aimConTo))
                        {
                            PumkinsAvatarTools.Log(Strings.Log.constraintHasNoValidSources, LogType.Warning, to.name, aimCon.gameObject.name);
                            Helpers.DestroyAppropriate(aimConTo);
                            return;
                        }

                        PumkinsAvatarTools.Log(Strings.Log.successCopiedOverFromTo, LogType.Log, typeString,
                                PumkinsAvatarTools.CopierSelectedFrom.name,
                                aimCon.transform == aimCon.transform.root ? "root" : aimCon.gameObject.name,
                                PumkinsAvatarTools.SelectedAvatar.name,
                                transTo == transTo.root ? "root" : transTo.gameObject.name);
                    }
                    else
                    {
                        PumkinsAvatarTools.Log(Strings.Log.failedAlreadyHas, LogType.Log, aimCon.gameObject.name, typeString);
                    }
                }
            }
        }

        /// <summary>
        /// Copies all LookAt Constrains in object and it's children
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="createGameObjects">Whether to create game objects if missing</param>
        /// <param name="useIgnoreList"></param>
        internal static void CopyAllLookAtConstraints(GameObject from, GameObject to, bool createGameObjects, bool useIgnoreList)
        {
            var lookConFromArr = from.GetComponentsInChildren<LookAtConstraint>(true);
            const string typeString = "LookAtConstraint";

            for(int i = 0; i < lookConFromArr.Length; i++)
            {
                var lookCon = lookConFromArr[i];

                if(useIgnoreList && Helpers.ShouldIgnoreObject(lookCon.transform, Settings._copierIgnoreArray, Settings.bCopier_ignoreArray_includeChildren))
                    continue;

                var transTo = Helpers.FindTransformInAnotherHierarchy(lookCon.transform, to.transform, createGameObjects);

                if(transTo != null)
                {
                    var lookConTo = transTo.GetComponent<LookAtConstraint>();

                    if(Settings.bCopier_lookAtConstraint_replaceOld || lookConTo == null)
                    {
                        Helpers.DestroyAppropriate(lookConTo);

                        ComponentUtility.CopyComponent(lookCon);
                        lookConTo = transTo.gameObject.AddComponent<LookAtConstraint>();
                        ComponentUtility.PasteComponentValues(lookConTo);

                        if(lookConTo.useUpObject)
                        {
                            var upObj = lookConTo.worldUpObject;
                            if(upObj && upObj.root == from.transform)
                                lookConTo.worldUpObject = Helpers.FindTransformInAnotherHierarchy(upObj, to.transform, createGameObjects);
                        }

                        var sources = new List<ConstraintSource>();
                        lookConTo.GetSources(sources);

                        for(int z = 0; z < sources.Count; z++)
                        {
                            var t = sources[z];
                            if(t.sourceTransform && t.sourceTransform.root == from.transform)
                            {
                                var cs = sources[z];
                                cs.sourceTransform = Helpers.FindTransformInAnotherHierarchy(t.sourceTransform, to.transform, createGameObjects);
                                lookConTo.SetSource(z, cs);
                            }
                        }

                        if(Settings.bCopier_lookAtConstraint_onlyIfHasValidSources && !Helpers.ConstraintHasValidSources(lookConTo))
                        {
                            PumkinsAvatarTools.Log(Strings.Log.constraintHasNoValidSources, LogType.Warning, to.name, lookCon.gameObject.name, typeString);
                            Helpers.DestroyAppropriate(lookCon);
                            return;
                        }

                        PumkinsAvatarTools.Log(Strings.Log.successCopiedOverFromTo, LogType.Log, typeString,
                                PumkinsAvatarTools.CopierSelectedFrom.name,
                                lookCon.transform == lookCon.transform.root ? "root" : lookCon.gameObject.name,
                                PumkinsAvatarTools.SelectedAvatar.name,
                                transTo == transTo.root ? "root" : transTo.gameObject.name);
                    }
                    else
                    {
                        PumkinsAvatarTools.Log(Strings.Log.failedAlreadyHas, LogType.Log, lookCon.gameObject.name, typeString);
                    }
                }
            }
        }

        /// <summary>
        /// Copies all Parent Constrains in object and it's children
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="createGameObjects">Whether to create game objects if missing</param>
        /// <param name="useIgnoreList"></param>
        internal static void CopyAllParentConstraints(GameObject from, GameObject to, bool createGameObjects, bool useIgnoreList)
        {
            var parConFromArr = from.GetComponentsInChildren<ParentConstraint>(true);
            const string typeString = "ParentConstraint";

            for(int i = 0; i < parConFromArr.Length; i++)
            {
                var parCon = parConFromArr[i];

                if(useIgnoreList && Helpers.ShouldIgnoreObject(parCon.transform, Settings._copierIgnoreArray, Settings.bCopier_ignoreArray_includeChildren))
                    continue;

                var transTo = Helpers.FindTransformInAnotherHierarchy(parCon.transform, to.transform, createGameObjects);

                if(transTo != null)
                {
                    var parConTo = transTo.GetComponent<ParentConstraint>();

                    if(Settings.bCopier_parentConstraint_replaceOld || parConTo == null)
                    {
                        Helpers.DestroyAppropriate(parConTo);

                        ComponentUtility.CopyComponent(parCon);
                        parConTo = transTo.gameObject.AddComponent<ParentConstraint>();
                        ComponentUtility.PasteComponentValues(parConTo);

                        var sources = new List<ConstraintSource>();
                        parConTo.GetSources(sources);

                        for(int z = 0; z < sources.Count; z++)
                        {
                            var t = sources[z];
                            if(t.sourceTransform && t.sourceTransform.root == from.transform)
                            {
                                var cs = sources[z];
                                cs.sourceTransform = Helpers.FindTransformInAnotherHierarchy(t.sourceTransform, to.transform, createGameObjects);
                                parConTo.SetSource(z, cs);
                            }
                        }

                        if(Settings.bCopier_parentConstraint_onlyIfHasValidSources && !Helpers.ConstraintHasValidSources(parConTo))
                        {
                            PumkinsAvatarTools.Log(Strings.Log.constraintHasNoValidSources, LogType.Warning, to.name, parCon.gameObject.name, typeString);
                            Helpers.DestroyAppropriate(parCon);
                            return;
                        }

                        PumkinsAvatarTools.Log(Strings.Log.successCopiedOverFromTo, LogType.Log, typeString,
                                PumkinsAvatarTools.CopierSelectedFrom.name,
                                parCon.transform == parCon.transform.root ? "root" : parCon.gameObject.name,
                                PumkinsAvatarTools.SelectedAvatar.name,
                                transTo == transTo.root ? "root" : transTo.gameObject.name);
                    }
                    else
                    {
                        PumkinsAvatarTools.Log(Strings.Log.failedAlreadyHas, LogType.Log, parCon.gameObject.name, typeString);
                    }
                }
            }
        }

        /// <summary>
        /// Copies all Position Constrains in object and it's children
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="createGameObjects">Whether to create game objects if missing</param>
        /// <param name="useIgnoreList"></param>
        internal static void CopyAllPositionConstraints(GameObject from, GameObject to, bool createGameObjects, bool useIgnoreList)
        {
            var posConFromArr = from.GetComponentsInChildren<PositionConstraint>(true);
            const string typeString = "PositionConstraint";

            for(int i = 0; i < posConFromArr.Length; i++)
            {
                var posCon = posConFromArr[i];

                if(useIgnoreList && Helpers.ShouldIgnoreObject(posCon.transform, Settings._copierIgnoreArray, Settings.bCopier_ignoreArray_includeChildren))
                    continue;

                var transTo = Helpers.FindTransformInAnotherHierarchy(posCon.transform, to.transform, createGameObjects);

                if(transTo != null)
                {
                    var posConTo = transTo.GetComponent<PositionConstraint>();

                    if(Settings.bCopier_positionConstraint_replaceOld || posConTo == null)
                    {
                        Helpers.DestroyAppropriate(posConTo);

                        ComponentUtility.CopyComponent(posCon);
                        posConTo = transTo.gameObject.AddComponent<PositionConstraint>();
                        ComponentUtility.PasteComponentValues(posConTo);

                        var sources = new List<ConstraintSource>();
                        posConTo.GetSources(sources);

                        for(int z = 0; z < sources.Count; z++)
                        {
                            var t = sources[z];
                            if(t.sourceTransform && t.sourceTransform.root == from.transform)
                            {
                                var cs = sources[z];
                                cs.sourceTransform = Helpers.FindTransformInAnotherHierarchy(t.sourceTransform, to.transform, createGameObjects);
                                posConTo.SetSource(z, cs);
                            }
                        }

                        if(Settings.bCopier_positionConstraint_onlyIfHasValidSources && !Helpers.ConstraintHasValidSources(posConTo))
                        {
                            PumkinsAvatarTools.Log(Strings.Log.constraintHasNoValidSources, LogType.Warning, to.name, posCon.gameObject.name, typeString);
                            Helpers.DestroyAppropriate(posCon);
                            return;
                        }

                        PumkinsAvatarTools.Log(Strings.Log.successCopiedOverFromTo, LogType.Log, typeString,
                                PumkinsAvatarTools.CopierSelectedFrom.name,
                                posCon.transform == posCon.transform.root ? "root" : posCon.gameObject.name,
                                PumkinsAvatarTools.SelectedAvatar.name,
                                transTo == transTo.root ? "root" : transTo.gameObject.name);
                    }
                    else
                    {
                        PumkinsAvatarTools.Log(Strings.Log.failedAlreadyHas, LogType.Log, posCon.gameObject.name, typeString);
                    }
                }
            }
        }

        /// <summary>
        /// Copies all Rotation Constrains in object and it's children
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="createGameObjects">Whether to create game objects if missing</param>
        /// <param name="useIgnoreList"></param>
        internal static void CopyAllRotationConstraints(GameObject from, GameObject to, bool createGameObjects, bool useIgnoreList)
        {
            var rotConFromArr = from.GetComponentsInChildren<RotationConstraint>(true);
            const string typeString = "RotationConstraint";

            for(int i = 0; i < rotConFromArr.Length; i++)
            {
                var rotCon = rotConFromArr[i];

                if(useIgnoreList && Helpers.ShouldIgnoreObject(rotCon.transform, Settings._copierIgnoreArray, Settings.bCopier_ignoreArray_includeChildren))
                    continue;

                var transTo = Helpers.FindTransformInAnotherHierarchy(rotCon.transform, to.transform, createGameObjects);

                if(transTo != null)
                {
                    var rotConTo = transTo.GetComponent<RotationConstraint>();

                    if(Settings.bCopier_rotationConstraint_replaceOld || rotConTo == null)
                    {
                        Helpers.DestroyAppropriate(rotConTo);

                        ComponentUtility.CopyComponent(rotCon);
                        rotConTo = transTo.gameObject.AddComponent<RotationConstraint>();
                        ComponentUtility.PasteComponentValues(rotConTo);

                        var sources = new List<ConstraintSource>();
                        rotConTo.GetSources(sources);

                        for(int z = 0; z < sources.Count; z++)
                        {
                            var t = sources[z];
                            if(t.sourceTransform && t.sourceTransform.root == from.transform)
                            {
                                var cs = sources[z];
                                cs.sourceTransform = Helpers.FindTransformInAnotherHierarchy(t.sourceTransform, to.transform, createGameObjects);
                                rotConTo.SetSource(z, cs);
                            }
                        }

                        if(Settings.bCopier_rotationConstraint_onlyIfHasValidSources && !Helpers.ConstraintHasValidSources(rotConTo))
                        {
                            PumkinsAvatarTools.Log(Strings.Log.constraintHasNoValidSources, LogType.Warning, to.name, rotCon.gameObject.name, typeString);
                            Helpers.DestroyAppropriate(rotCon);
                            return;
                        }

                        PumkinsAvatarTools.Log(Strings.Log.successCopiedOverFromTo, LogType.Log, typeString,
                                PumkinsAvatarTools.CopierSelectedFrom.name,
                                rotCon.transform == rotCon.transform.root ? "root" : rotCon.gameObject.name,
                                PumkinsAvatarTools.SelectedAvatar.name,
                                transTo == transTo.root ? "root" : transTo.gameObject.name);
                    }
                    else
                    {
                        PumkinsAvatarTools.Log(Strings.Log.failedAlreadyHas, LogType.Log, rotCon.gameObject.name, typeString);
                    }
                }
            }
        }

        /// <summary>
        /// Copies all Scale Constrains in object and it's children
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="createGameObjects">Whether to create game objects if missing</param>
        /// <param name="useIgnoreList"></param>
        internal static void CopyAllScaleConstraints(GameObject from, GameObject to, bool createGameObjects, bool useIgnoreList)
        {
            var scaleConFromArr = from.GetComponentsInChildren<ScaleConstraint>(true);
            const string typeString = "ScaleConstraint";

            for(int i = 0; i < scaleConFromArr.Length; i++)
            {
                var scaleCon = scaleConFromArr[i];

                if(useIgnoreList && Helpers.ShouldIgnoreObject(scaleCon.transform, Settings._copierIgnoreArray, Settings.bCopier_ignoreArray_includeChildren))
                    continue;

                var transTo = Helpers.FindTransformInAnotherHierarchy(scaleCon.transform, to.transform, createGameObjects);

                if(transTo != null)
                {
                    var scaleConTo = transTo.GetComponent<ScaleConstraint>();

                    if(Settings.bCopier_scaleConstraint_replaceOld || scaleConTo == null)
                    {
                        Helpers.DestroyAppropriate(scaleConTo);

                        ComponentUtility.CopyComponent(scaleCon);
                        scaleConTo = transTo.gameObject.AddComponent<ScaleConstraint>();
                        ComponentUtility.PasteComponentValues(scaleConTo);

                        var sources = new List<ConstraintSource>();
                        scaleConTo.GetSources(sources);

                        for(int z = 0; z < sources.Count; z++)
                        {
                            var t = sources[z];
                            if(t.sourceTransform && t.sourceTransform.root == from.transform)
                            {
                                var cs = sources[z];
                                cs.sourceTransform = Helpers.FindTransformInAnotherHierarchy(t.sourceTransform, to.transform, createGameObjects);
                                scaleConTo.SetSource(z, cs);
                            }
                        }

                        if(Settings.bCopier_scaleConstraint_onlyIfHasValidSources && !Helpers.ConstraintHasValidSources(scaleConTo))
                        {
                            PumkinsAvatarTools.Log(Strings.Log.constraintHasNoValidSources, LogType.Warning, to.name, scaleCon.gameObject.name, typeString);
                            Helpers.DestroyAppropriate(scaleCon);
                            return;
                        }

                        PumkinsAvatarTools.Log(Strings.Log.successCopiedOverFromTo, LogType.Log, typeString,
                                PumkinsAvatarTools.CopierSelectedFrom.name,
                                scaleCon.transform == scaleCon.transform.root ? "root" : scaleCon.gameObject.name,
                                PumkinsAvatarTools.SelectedAvatar.name,
                                transTo == transTo.root ? "root" : transTo.gameObject.name);
                    }
                    else
                    {
                        PumkinsAvatarTools.Log(Strings.Log.failedAlreadyHas, LogType.Log, scaleCon.gameObject.name, typeString);
                    }
                }
            }
        }

        /// <summary>
        /// Copies all audio sources on object and it's children.
        /// </summary>
        /// <param name="createGameObjects">Whether to create missing objects</param>
        internal static void CopyAllAudioSources(GameObject from, GameObject to, bool createGameObjects, bool useIgnoreList)
        {
            if(from == null || to == null)
                return;

            var audioFromArr = from.GetComponentsInChildren<AudioSource>(true);
            string typeName = typeof(AudioSource).Name;

            for(int i = 0; i < audioFromArr.Length; i++)
            {
                var audioFrom = audioFromArr[i];
                var transTo = Helpers.FindTransformInAnotherHierarchy(audioFrom.transform, to.transform, createGameObjects);

                if((!transTo) || (useIgnoreList && Helpers.ShouldIgnoreObject(audioFrom.transform, Settings._copierIgnoreArray, Settings.bCopier_ignoreArray_includeChildren)))
                    continue;

                var audioToObj = transTo.gameObject;

                string log = String.Format(Strings.Log.copyAttempt, typeName, audioFrom.gameObject, transTo.gameObject);

                if(audioFrom != null)
                {
                    var audioTo = audioToObj.GetComponent<AudioSource>();
                    if(audioTo == null && Settings.bCopier_audioSources_createMissing)
                        audioTo = audioToObj.AddComponent<AudioSource>();

                    if((audioTo != null && Settings.bCopier_audioSources_copySettings) || Settings.bCopier_audioSources_createMissing)
                    {
                        ComponentUtility.CopyComponent(audioFrom);
                        ComponentUtility.PasteComponentValues(audioTo);
                        PumkinsAvatarTools.Log(log + " - " + Strings.Log.success);
                    }

#if VRC_SDK_VRCSDK2 || VRC_SDK_VRCSDK3

                    var spatialAudioFrom = audioFromArr[i].GetComponent<VRC_SpatialAudioSource>();
                    if(spatialAudioFrom)
                    {
                        var spatialAudioTo = audioToObj.GetComponent<VRC_SpatialAudioSource>();
                        if(spatialAudioTo == null && Settings.bCopier_audioSources_createMissing)
                            spatialAudioTo = audioToObj.AddComponent<VRC_SpatialAudioSource>();

                        if((spatialAudioTo != null && Settings.bCopier_audioSources_copySettings) ||
                           Settings.bCopier_audioSources_createMissing)
                        {
                            ComponentUtility.CopyComponent(spatialAudioFrom);
                            ComponentUtility.PasteComponentValues(spatialAudioTo);
                        }
                    }
#endif
                }
                else
                {
                    PumkinsAvatarTools.Log(log + " " + Strings.Log.failedDoesntHave, LogType.Warning, audioFrom.gameObject.name.ToString(), audioFrom.GetType().ToString());
                }
            }
        }

        /// <summary>
        /// Copies all Animators from one object and it's children to another.
        /// </summary>
        /// <param name="createGameObjects">Whether to create missing GameObjects</param>
        /// <param name="copyRootAnimator">Whether to copy the Animator on the root object. You don't usually want to.</param>
        internal static void CopyAllAnimators(GameObject from, GameObject to, bool createGameObjects, bool copyRootAnimator, bool useIgnoreList)
        {
            if(from == null || to == null)
                return;

            var aFromArr = from.GetComponentsInChildren<Animator>(true);

            for(int i = 0; i < aFromArr.Length; i++)
            {
                if(!copyRootAnimator && aFromArr[i].transform.parent == null)
                    continue;

                string log = Strings.Log.copyAttempt;
                string type = typeof(Animator).Name;

                var aFrom = aFromArr[i];
                var tTo = Helpers.FindTransformInAnotherHierarchy(aFrom.transform, to.transform, createGameObjects);

                if((!tTo) || (useIgnoreList && Helpers.ShouldIgnoreObject(aFrom.transform, Settings._copierIgnoreArray, Settings.bCopier_ignoreArray_includeChildren)))
                    continue;

                var aToObj = tTo.gameObject;

                if(aFrom != null)
                {
                    var lTo = aToObj.GetComponent<Animator>();

                    if(lTo == null && Settings.bCopier_animators_createMissing)
                    {
                        lTo = aToObj.AddComponent<Animator>();
                    }

                    if((lTo != null && Settings.bCopier_animators_copySettings) || Settings.bCopier_animators_createMissing)
                    {
                        ComponentUtility.CopyComponent(aFrom);
                        ComponentUtility.PasteComponentValues(lTo);
                        PumkinsAvatarTools.Log(log + " - " + Strings.Log.success, LogType.Log, type, tTo.gameObject.name, aFrom.gameObject.name);
                    }
                }
                else
                {
                    PumkinsAvatarTools.Log(log + " " + Strings.Log.failedDoesntHave, LogType.Warning, aFrom.gameObject.name.ToString(), aFrom.GetType().ToString());
                }
            }
        }

        /// <summary>
        /// Copies all lights in object and it's children to another object.
        /// </summary>
        /// <param name="createGameObjects">Whether to create missing game objects</param>
        internal static void CopyAllLights(GameObject from, GameObject to, bool createGameObjects, bool useIgnoreList)
        {
            if(from == null || to == null)
                return;

            var lFromArr = from.GetComponentsInChildren<Light>(true);

            for(int i = 0; i < lFromArr.Length; i++)
            {
                string log = Strings.Log.copyAttempt;
                string type = typeof(Light).Name;

                var lFrom = lFromArr[i];
                var tTo = Helpers.FindTransformInAnotherHierarchy(lFrom.transform, to.transform, createGameObjects);

                if((!tTo) ||
                    (useIgnoreList && Helpers.ShouldIgnoreObject(lFrom.transform, Settings._copierIgnoreArray, Settings.bCopier_ignoreArray_includeChildren)))
                    continue;

                var lToObj = tTo.gameObject;

                if(lFrom != null)
                {
                    var lTo = lToObj.GetComponent<Light>();

                    if(lTo == null && Settings.bCopier_lights_createMissing)
                    {
                        lTo = lToObj.AddComponent<Light>();
                    }

                    if((lTo != null && Settings.bCopier_lights_copySettings) || Settings.bCopier_lights_createMissing)
                    {
                        ComponentUtility.CopyComponent(lFrom);
                        ComponentUtility.PasteComponentValues(lTo);
                        PumkinsAvatarTools.Log(log + " - " + Strings.Log.success);
                    }
                }
                else
                {
                    PumkinsAvatarTools.Log(log + " " + Strings.Log.failedDoesntHave, LogType.Warning, lFrom.gameObject.name.ToString(), type);
                }
            }
        }

        /// <summary>
        /// Copies all MeshRenderers in object and it's children to another object.
        /// </summary>
        /// <param name="createGameObjects">Whether to create missing game objects</param>
        internal static void CopyAllMeshRenderers(GameObject from, GameObject to, bool createGameObjects, bool useIgnoreList)
        {
            if(from == null || to == null)
                return;

            var mFromArr = from.GetComponentsInChildren<MeshRenderer>(true);
            string type = typeof(MeshRenderer).Name;

            for(int i = 0; i < mFromArr.Length; i++)
            {
                var rFrom = mFromArr[i];
                var tTo = Helpers.FindTransformInAnotherHierarchy(rFrom.transform, to.transform, createGameObjects);
                
                if((!tTo) ||
                    (useIgnoreList && Helpers.ShouldIgnoreObject(rFrom.transform, Settings._copierIgnoreArray, Settings.bCopier_ignoreArray_includeChildren)))
                    continue;
                
                string log = string.Format(Strings.Log.copyAttempt, type, rFrom.gameObject.name, tTo.gameObject.name);

                var rToObj = tTo.gameObject;

                var fFrom = rFrom.GetComponent<MeshFilter>();

                if(fFrom != null)
                {
                    var rTo = rToObj.GetComponent<MeshRenderer>();
                    var fTo = rToObj.GetComponent<MeshFilter>();

                    if(rTo == null && Settings.bCopier_meshRenderers_createMissing)
                    {
                        rTo = Undo.AddComponent<MeshRenderer>(tTo.gameObject);
                        if(fTo == null)
                            fTo = Undo.AddComponent<MeshFilter>(tTo.gameObject);
                    }

                    if((rTo != null && Settings.bCopier_meshRenderers_copySettings) || Settings.bCopier_meshRenderers_createMissing)
                    {
                        ComponentUtility.CopyComponent(rFrom);
                        ComponentUtility.PasteComponentValues(rTo);

                        ComponentUtility.CopyComponent(fFrom);
                        ComponentUtility.PasteComponentValues(fTo);
                        PumkinsAvatarTools.Log(log + " - " + Strings.Log.success, LogType.Log);
                    }
                    else
                    {
                        PumkinsAvatarTools.Log(log += " - " + Strings.Log.failedHasNoIgnoring, LogType.Warning, rFrom.gameObject.name, type);
                    }
                }
            }
        }

        /// <summary>
        /// Copies all VRC_IKFollowers on an object and it's children.
        /// </summary>
        /// <param name="createGameObjects">Whether to create missing objects</param>
        internal static void CopyAllIKFollowers(GameObject from, GameObject to, bool createGameObjects, bool useIgnoreList)
        {
            Type ikFollowerType = PumkinsTypeCache.VRC_IKFollower;
            
            if(from == null || to == null || ikFollowerType == null)
                return;

            var ikFromArr = from.GetComponentsInChildren(ikFollowerType, true);
            if(ikFromArr == null || ikFromArr.Length == 0)
                return;

            string type = ikFollowerType.Name;

            for(int i = 0; i < ikFromArr.Length; i++)
            {
                var ikFrom = ikFromArr[i];
                var tTo = Helpers.FindTransformInAnotherHierarchy(ikFrom.transform, to.transform, createGameObjects);
                if((!tTo) || (useIgnoreList && Helpers.ShouldIgnoreObject(ikFrom.transform, Settings._copierIgnoreArray, Settings.bCopier_ignoreArray_includeChildren)))
                    continue;

                string log = String.Format(Strings.Log.copyAttempt, type, ikFrom.gameObject, tTo.gameObject);

                if(!tTo.GetComponent(ikFollowerType))
                {
                    ComponentUtility.CopyComponent(ikFrom);
                    ComponentUtility.PasteComponentAsNew(tTo.gameObject);
                    PumkinsAvatarTools.Log(Strings.Log.copyAttempt + " - " + Strings.Log.success, LogType.Log);
                }
                else
                {
                    PumkinsAvatarTools.Log(Strings.Log.copyAttempt + " - " + Strings.Log.failedAlreadyHas, LogType.Log);
                }
            }
        }

        internal static void CopyAvatarDescriptor(GameObject from, GameObject to, bool useIgnoreList)
        {
            Type descType = PumkinsTypeCache.VRC_AvatarDescriptor;
            Type pipelineType = PumkinsTypeCache.PipelineManager;
            
            if(to == null || from == null || descType == null || pipelineType == null)
                return;

            if(useIgnoreList && Helpers.ShouldIgnoreObject(from.transform, Settings._copierIgnoreArray))
                return;

            var dFrom = from.GetComponent(descType);
            var pFrom = from.GetComponent(pipelineType);
            var dTo = to.GetComponent(descType);

            if(dFrom == null)
                return;
            if(dTo == null)
                dTo = Undo.AddComponent(to, descType);

            var pTo = to.GetComponent(pipelineType) ?? to.AddComponent(pipelineType);

            var sDescTo = new SerializedObject(dTo);
            var sDescFrom = new SerializedObject(dFrom);

            var sPipeTo = new SerializedObject(pTo);

            var descPropNames = new List<string>();
            if(Settings.bCopier_descriptor_copyViewpoint)
                descPropNames.Add("ViewPosition");

            
            if(Settings.bCopier_descriptor_copyPipelineId)
            {
                var sPipeFrom = new SerializedObject(pFrom);
                var pipePropNames = new List<string> { "blueprintId", "completedSDKPipeline" };

                foreach(var s in pipePropNames)
                {
                    var prop = sPipeFrom.FindProperty(s);
                    if(prop != null)
                        sPipeTo.CopyFromSerializedProperty(prop);
                }

                sPipeTo.ApplyModifiedPropertiesWithoutUndo();
            }

            if(Settings.bCopier_descriptor_copySettings)
            {
                descPropNames.AddRange(new []
                {
                    //Shared
                    "Name", "Animations", "ScaleIPD", "lipSync", "VisemeSkinnedMesh", "MouthOpenBlendShapeName",
                    "VisemeBlendShapes", "portraitCameraPositionOffset", "portraitCameraRotationOffset", "lipSyncJawBone",
                    //SDK3
                    "enableEyeLook", "lipSyncJawClosed", "lipSyncJawOpen", "AnimationPreset", "autoFootsteps", "autoLocomotion"
                });
            }

            if(Settings.bCopier_descriptor_copyPlayableLayers)
            {
                descPropNames.AddRange(new []
                {
                    "customizeAnimationLayers", "baseAnimationLayers",
                    "specialAnimationLayers"
                });
            }

            if(Settings.bCopier_descriptor_copyEyeLookSettings)
            {
                descPropNames.Add("customEyeLookSettings");
            }

            if(Settings.bCopier_descriptor_copyAnimationOverrides) //SDK2 Only
            {
                descPropNames.AddRange(new []
                {
                    "CustomSittingAnims", "CustomStandingAnims",
                });
            }

            if(Settings.bCopier_descriptor_copyExpressions)
            {
                descPropNames.AddRange(new []
                {
                    "customExpressions", "expressionsMenu", "expressionParameters"
                });
            }

            if (Settings.bCopier_descriptor_copyColliders)
            {
                descPropNames.AddRange(new[]
                {
                    "collidersMirrored", "collider_head", "collider_torso",
                    "collider_handR", "collider_footR", "collider_fingerIndexR", "collider_fingerMiddleR", "collider_fingerRingR", "collider_fingerLittleR",
                    "collider_handL", "collider_footL", "collider_fingerIndexL", "collider_fingerMiddleL", "collider_fingerRingL", "collider_fingerLittleL"
                });
            }

            foreach (var s in descPropNames)
            {
                var prop = sDescFrom.FindProperty(s);
                if(prop != null)
                    sDescTo.CopyFromSerializedProperty(prop);
            }

            var eyes = sDescTo.FindProperty("customEyeLookSettings");

            if(eyes != null)
            {
                SerializedProperty[] transLocalize =
                {
                    eyes.FindPropertyRelative("leftEye"),
                    eyes.FindPropertyRelative("rightEye"),
                    eyes.FindPropertyRelative("upperLeftEyelid"),
                    eyes.FindPropertyRelative("upperRightEyelid"),
                    eyes.FindPropertyRelative("lowerLeftEyelid"),
                    eyes.FindPropertyRelative("lowerRightEyelid"),
                };
                Helpers.MakeReferencesLocal<Transform>(to.transform, true, transLocalize);
            }

            SerializedProperty[] rendererLocalize =
            {
                sDescTo.FindProperty("VisemeSkinnedMesh"),
                eyes != null ? eyes.FindPropertyRelative("eyelidsSkinnedMesh") : null,
            };
            Helpers.MakeReferencesLocal<SkinnedMeshRenderer>(to.transform, true, rendererLocalize);

            sDescTo.ApplyModifiedPropertiesWithoutUndo();
        }
        
        internal static void CopyTransformActiveStateTagsAndLayer(GameObject from, GameObject to, bool useIgnoreList)
        {
            if(from == null || to == null || !(Settings.bCopier_transforms_copyActiveState || Settings.bCopier_transforms_copyLayerAndTag))
                return;
            
            var tFromArr = from.GetComponentsInChildren<Transform>(true);

            foreach(var tFrom in tFromArr)
            {
                if(tFrom == tFrom.root || (useIgnoreList && Helpers.ShouldIgnoreObject(tFrom, Settings._copierIgnoreArray, Settings.bCopier_ignoreArray_includeChildren)))
                    continue;
                Transform tTo = Helpers.FindTransformInAnotherHierarchy(tFrom, to.transform, false);
                if(!tTo)
                    continue;
                
                if(Settings.bCopier_transforms_copyActiveState)
                    tTo.gameObject.SetActive(tFrom.gameObject.activeSelf);
                if(Settings.bCopier_transforms_copyLayerAndTag)
                {
                    to.tag = from.tag;
                    to.layer = from.layer;
                }
            }
        }

        internal static void CopyCameras(GameObject from, GameObject to, bool createGameObjects, bool useIgnoreList)
        {
            if(from == null || to == null)
                return;

            var cameraFromArr = from.GetComponentsInChildren<Camera>(true);
            if(cameraFromArr == null || cameraFromArr.Length == 0)
                return;

            string type = typeof(Camera).Name;

            for(int i = 0; i < cameraFromArr.Length; i++)
            {
                var camFrom = cameraFromArr[i];
                var tTo = Helpers.FindTransformInAnotherHierarchy(camFrom.transform, to.transform, createGameObjects);
                if((!tTo) || (useIgnoreList && Helpers.ShouldIgnoreObject(camFrom.transform, Settings._copierIgnoreArray, Settings.bCopier_ignoreArray_includeChildren)))
                    continue;

                string log = String.Format(Strings.Log.copyAttempt, type, camFrom.gameObject, tTo.gameObject);

                if(!tTo.GetComponent<Camera>())
                {
                    ComponentUtility.CopyComponent(camFrom);
                    ComponentUtility.PasteComponentAsNew(tTo.gameObject);
                    PumkinsAvatarTools.Log(Strings.Log.copyAttempt + " - " + Strings.Log.success, LogType.Log);
                }
                else
                {
                    PumkinsAvatarTools.Log(Strings.Log.copyAttempt + " - " + Strings.Log.failedAlreadyHas, LogType.Log);
                }
            }
        }
    }
}