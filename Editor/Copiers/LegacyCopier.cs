using System;
using System.Collections.Generic;
using System.Linq;
using Pumkin.DataStructures;
using Pumkin.HelperFunctions;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Animations;

namespace Pumkin.AvatarTools.Copiers
{
    internal static class LegacyCopier
    {
        static SettingsContainer Settings => PumkinsAvatarTools.Settings;

        /// <summary>
        /// Copies Box, Capsule, Sphere and Mesh colliders from one object to another and all of it's children at once.
        /// </summary>
        /// <param name="from">Object to copy from</param>
        /// <param name="to">Object to copy to</param>
        /// <param name="createGameObjects">Whether to create missing game objects</param>
        /// <param name="adjustScale"></param>
        /// <param name="ignoreSet">Set of transforms to ignore when copying</param>
        internal static void CopyAllColliders(GameObject from, GameObject to, bool createGameObjects, bool adjustScale, HashSet<Transform> ignoreSet)
        {
            if(from == null || to == null)
                return;
            if(!(Settings.bCopier_colliders_copyBox || Settings.bCopier_colliders_copyCapsule || Settings.bCopier_colliders_copyMesh || Settings.bCopier_colliders_copySphere))
                return;

            var cFromArr = from.GetComponentsInChildren<Collider>(true);

            foreach(var cFrom in cFromArr)
            {
                if(Helpers.ShouldIgnoreObject(cFrom.transform, ignoreSet, Settings.bCopier_ignoreArray_includeChildren))
                    continue;
                
                string log = Strings.Log.copyAttempt;
                var type = cFrom.GetType();

                var tTo = Helpers.FindTransformInAnotherHierarchy(cFrom.transform, to.transform, createGameObjects);
                if(!tTo)
                    continue;

                var cToArr = tTo.GetComponents<Collider>();
                bool found = false;

                for(int z = 0; z < cToArr.Length; z++)
                {
                    if(Helpers.CollidersAreIdentical(cToArr[z], cFrom))
                    {
                        found = true;
                        PumkinsAvatarTools.Log(log + " - " + Strings.Log.failedAlreadyHas, LogType.Warning, tTo.name, type.ToString());
                        break;
                    }
                }
                if(!found)
                {
                    ComponentUtility.CopyComponent(cFrom);
                    ComponentUtility.PasteComponentAsNew(tTo.gameObject);

                    if(adjustScale)
                    {
                        Collider c = tTo.GetComponents<Collider>().Last();
                        float mul = Helpers.GetScaleMultiplier(cFrom.transform, tTo);
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
                    PumkinsAvatarTools.Log(log + " - " + Strings.Log.success, LogType.Log, type.ToString(), cFrom.gameObject.name, tTo.name);
                }
            }
        }

        /// <summary>
        /// Copies character, configurable, fixed hinge and spring joints from one object to another and all of it's children at once.
        /// </summary>
        /// <param name="from">Object to copy from</param>
        /// <param name="to">Object to copy to</param>
        /// <param name="createGameObjects">Whether to create missing game objects</param>
        /// <param name="ignoreSet">Set of transforms to ignore when copying</param>
        internal static void CopyAllJoints(GameObject from, GameObject to, bool createGameObjects, HashSet<Transform> ignoreSet)
        {
            if(from == null || to == null)
                return;
            
            if(!(Settings.bCopier_joints_character || Settings.bCopier_joints_configurable
                || Settings.bCopier_joints_fixed || Settings.bCopier_joints_hinge || Settings.bCopier_joints_spring))
                return;

            var jointFromArr = from.GetComponentsInChildren<Joint>(true);

            foreach(var jointFrom in jointFromArr)
            {
                var jointTransFrom = jointFrom.transform;
                if(Helpers.ShouldIgnoreObject(jointTransFrom, ignoreSet, Settings.bCopier_ignoreArray_includeChildren))
                    continue;

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
        /// <param name="ignoreSet != null">Whether or not to use copier ignore list</param>
        /// <param name="from">Object to copy from</param>
        /// <param name="to">Object to copy to</param>
        /// <param name="ignoreSet">Set of transforms to ignore when copying</param>
        internal static void CopyAllTransforms(GameObject from, GameObject to, HashSet<Transform> ignoreSet)
        {
            if(from == null || to == null || !(Settings.bCopier_transforms_copyPosition || Settings.bCopier_transforms_copyRotation 
                   || Settings.bCopier_transforms_copyScale || Settings.bCopier_transforms_copyLayerAndTag || Settings.bCopier_transforms_copyActiveState))
                return;

            var tFromArr = from.GetComponentsInChildren<Transform>(true);

            foreach(var tFrom in tFromArr)
            {
                if(tFrom == from.transform || tFrom == from.transform.Find(tFrom.name))
                    continue;

                if(Helpers.ShouldIgnoreObject(tFrom, ignoreSet, Settings.bCopier_ignoreArray_includeChildren))
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
        /// Copies all SkinnedMeshRenderers in object and children.        
        /// </summary>
        internal static void CopyAllSkinnedMeshRenderers(GameObject from, GameObject to, HashSet<Transform> ignoreSet)
        {
            if((from == null || to == null)
               || (!(Settings.bCopier_skinMeshRender_copyBlendShapeValues
                     || Settings.bCopier_skinMeshRender_copyMaterials
                     || Settings.bCopier_skinMeshRender_copySettings
                     || Settings.bCopier_skinMeshRender_copyBounds
                     || Settings.bCopier_skinMeshRender_createObjects)))
                return;

            Transform tToRoot = to.transform;

            string log = String.Format(Strings.Log.copyAttempt + " - ", Strings.Copier.skinMeshRender, from.name,
                to.name);

            var rFromArr = from.GetComponentsInChildren<SkinnedMeshRenderer>(true);

            foreach(var smrFrom in rFromArr)
            {
                Transform tFrom = smrFrom.transform;
                if(Helpers.ShouldIgnoreObject(smrFrom.transform, ignoreSet, Settings.bCopier_ignoreArray_includeChildren))
                    continue;

                var tTo = Helpers.FindTransformInAnotherHierarchy(tFrom, tToRoot,
                    Settings.bCopier_skinMeshRender_createObjects);

                if(!tTo)
                    continue;

                GameObject rToObj = tTo.gameObject;
                var smrTo = rToObj.GetComponent<SkinnedMeshRenderer>();

                if(smrTo == null)
                {
                    if(!Settings.bCopier_skinMeshRender_createObjects)
                        continue;


                    smrTo = rToObj.AddComponent<SkinnedMeshRenderer>();

                    Transform[] newBones = new Transform[smrFrom.bones.Length];
                    Transform[] oldBones = smrFrom.bones;

                    bool allBonesFound = true;
                    for(int j = 0; j < newBones.Length; j++)
                    {
                        newBones[j] = Helpers.FindTransformInAnotherHierarchy(oldBones[j], tToRoot, true);
                        if(!newBones[j])
                        {
                            allBonesFound = false;
                            break;
                        }
                    }

                    var newRoot = Helpers.FindTransformInAnotherHierarchy(smrFrom.rootBone, tToRoot, false);

                    if(!allBonesFound || !newRoot)
                    {
                        PumkinsAvatarTools.Log("Couldn't find all bones to assign to skinned mesh renderer.",
                            LogType.Warning);
                    }
                    else
                    {
                        smrTo.rootBone = newRoot;
                        smrTo.bones = newBones;
                        smrTo.sharedMesh = smrFrom.sharedMesh;
                    }
                }

                if(Settings.bCopier_skinMeshRender_copySettings)
                {
                    var t = Helpers.FindTransformInAnotherHierarchy(smrFrom.rootBone, tToRoot, false);
                    smrTo.rootBone = t ? t : smrTo.rootBone;

                    smrTo.allowOcclusionWhenDynamic = smrFrom.allowOcclusionWhenDynamic;
                    smrTo.quality = smrFrom.quality;
                    smrTo.probeAnchor = t ? t : smrTo.probeAnchor;
                    smrTo.lightProbeUsage = smrFrom.lightProbeUsage;
                    smrTo.reflectionProbeUsage = smrFrom.reflectionProbeUsage;
                    smrTo.shadowCastingMode = smrFrom.shadowCastingMode;
                    smrTo.receiveShadows = smrFrom.receiveShadows;
                    smrTo.motionVectorGenerationMode = smrFrom.motionVectorGenerationMode;
                    smrTo.skinnedMotionVectors = smrFrom.skinnedMotionVectors;
                    smrTo.allowOcclusionWhenDynamic = smrFrom.allowOcclusionWhenDynamic;
                    smrTo.enabled = smrFrom.enabled;
                }

                if(Settings.bCopier_skinMeshRender_copyBlendShapeValues && smrFrom.sharedMesh)
                {
                    Mesh fromMesh = smrFrom.sharedMesh;
                    Mesh toMesh = smrTo.sharedMesh;
                    for(int z = 0; z < smrFrom.sharedMesh.blendShapeCount; z++)
                    {
                        int toShapeIndex = toMesh.GetBlendShapeIndex(fromMesh.GetBlendShapeName(z));
                        if(toShapeIndex != -1)
                            smrTo.SetBlendShapeWeight(toShapeIndex, smrFrom.GetBlendShapeWeight(z));
                    }
                }

                if(Settings.bCopier_skinMeshRender_copyMaterials)
                    smrTo.sharedMaterials = smrFrom.sharedMaterials;

                if(Settings.bCopier_skinMeshRender_copyBounds)
                    smrTo.localBounds = smrFrom.localBounds;

                PumkinsAvatarTools.Log(log + Strings.Log.success);
            }
        }

        /// <summary>
        /// Copies all TrailRenderers in object and it's children.
        /// </summary>
        /// <param name="to">Object to copy to</param>
        /// <param name="createGameObjects">Whether to create missing GameObjects</param>
        /// <param name="from">Object to copy from</param>
        /// <param name="ignoreSet">Set of transforms to ignore when copying</param>
        internal static void CopyAllTrailRenderers(GameObject from, GameObject to, bool createGameObjects, HashSet<Transform> ignoreSet)
        {
            if(from == null || to == null)
                return;

            var rFromArr = from.GetComponentsInChildren<TrailRenderer>(true);

            foreach(var rFrom in rFromArr)
            {
                if(Helpers.ShouldIgnoreObject(rFrom.transform, ignoreSet, Settings.bCopier_ignoreArray_includeChildren))
                    continue;
                       
                var tTo = Helpers.FindTransformInAnotherHierarchy(rFrom.transform, to.transform, createGameObjects);
                if(!tTo)
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
        /// <param name="to">Object to copy to</param>
        /// <param name="createGameObjects">Whether to create missing GameObjects</param>
        /// <param name="from">Object to copy from</param>
        /// <param name="ignoreSet">Set of transforms to ignore when copying</param>
        /// </summary>
        internal static void CopyAllRigidBodies(GameObject from, GameObject to, bool createGameObjects, HashSet<Transform> ignoreSet)
        {
            if(from == null || to == null)
                return;

            var rFromArr = from.GetComponentsInChildren<Rigidbody>(true);

            foreach(var rFrom in rFromArr)
            {
                if(Helpers.ShouldIgnoreObject(rFrom.transform, ignoreSet, Settings.bCopier_ignoreArray_includeChildren))
                    continue;
                
                var tTo = Helpers.FindTransformInAnotherHierarchy(rFrom.transform, to.transform, createGameObjects);
                if(!tTo)
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
        /// Copies all Aim Constrains in object and it's children
        /// </summary>
        /// <param name="from">Object to copy from</param>
        /// <param name="to">Object to copy to</param>
        /// <param name="createGameObjects">Whether to create game objects if missing</param>
        /// <param name="ignoreSet != null"></param>
        /// <param name="ignoreSet">Set of transforms to ignore when copying</param>
        internal static void CopyAllAimConstraints(GameObject from, GameObject to, bool createGameObjects, HashSet<Transform> ignoreSet)
        {
            var aimConFromArr = from.GetComponentsInChildren<AimConstraint>(true);
            const string typeString = "AimConstraint";

            foreach(var aimCon in aimConFromArr)
            {
                if(Helpers.ShouldIgnoreObject(aimCon.transform, ignoreSet, Settings.bCopier_ignoreArray_includeChildren))
                    continue;

                var transTo = Helpers.FindTransformInAnotherHierarchy(aimCon.transform, to.transform, createGameObjects);
                if(transTo == null)
                    continue;
                
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

        /// <summary>
        /// Copies all LookAt Constrains in object and it's children
        /// </summary>
        /// <param name="from">Object to copy from</param>
        /// <param name="to">Object to copy to</param>
        /// <param name="createGameObjects">Whether to create game objects if missing</param>
        /// <param name="ignoreSet != null"></param>
        /// <param name="ignoreSet">Set of transforms to ignore when copying</param>
        internal static void CopyAllLookAtConstraints(GameObject from, GameObject to, bool createGameObjects, HashSet<Transform> ignoreSet)
        {
            var lookConFromArr = from.GetComponentsInChildren<LookAtConstraint>(true);
            const string typeString = "LookAtConstraint";

            foreach(var lookCon in lookConFromArr)
            {
                if(Helpers.ShouldIgnoreObject(lookCon.transform, ignoreSet, Settings.bCopier_ignoreArray_includeChildren))
                    continue;

                var transTo = Helpers.FindTransformInAnotherHierarchy(lookCon.transform, to.transform, createGameObjects);
                if(transTo == null)
                    continue;
                
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

        /// <summary>
        /// Copies all Parent Constrains in object and it's children
        /// </summary>
        /// <param name="from">Object to copy from</param>
        /// <param name="to">Object to copy to</param>
        /// <param name="createGameObjects">Whether to create game objects if missing</param>
        /// <param name="ignoreSet != null"></param>
        /// <param name="ignoreSet">Set of transforms to ignore when copying</param>
        internal static void CopyAllParentConstraints(GameObject from, GameObject to, bool createGameObjects, HashSet<Transform> ignoreSet)
        {
            var parConFromArr = from.GetComponentsInChildren<ParentConstraint>(true);
            const string typeString = "ParentConstraint";

            foreach(var parCon in parConFromArr)
            {
                if(Helpers.ShouldIgnoreObject(parCon.transform, ignoreSet, Settings.bCopier_ignoreArray_includeChildren))
                    continue;

                var transTo = Helpers.FindTransformInAnotherHierarchy(parCon.transform, to.transform, createGameObjects);
                if(transTo == null)
                    continue;
                
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

        /// <summary>
        /// Copies all Position Constrains in object and it's children
        /// </summary>
        /// <param name="from">Object to copy from</param>
        /// <param name="to">Object to copy to</param>
        /// <param name="createGameObjects">Whether to create game objects if missing</param>
        /// <param name="ignoreSet != null"></param>
        /// <param name="ignoreSet">Set of transforms to ignore when copying</param>
        internal static void CopyAllPositionConstraints(GameObject from, GameObject to, bool createGameObjects, HashSet<Transform> ignoreSet)
        {
            var posConFromArr = from.GetComponentsInChildren<PositionConstraint>(true);
            const string typeString = "PositionConstraint";

            foreach(var posCon in posConFromArr)
            {
                if(Helpers.ShouldIgnoreObject(posCon.transform, ignoreSet, Settings.bCopier_ignoreArray_includeChildren))
                    continue;

                var transTo = Helpers.FindTransformInAnotherHierarchy(posCon.transform, to.transform, createGameObjects);
                if(transTo == null)
                    continue;
                
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

        /// <summary>
        /// Copies all Rotation Constrains in object and it's children
        /// </summary>
        /// <param name="from">Object to copy from</param>
        /// <param name="to">Object to copy to</param>
        /// <param name="createGameObjects">Whether to create game objects if missing</param>
        /// <param name="ignoreSet != null"></param>
        /// <param name="ignoreSet">Set of transforms to ignore when copying</param>
        internal static void CopyAllRotationConstraints(GameObject from, GameObject to, bool createGameObjects, HashSet<Transform> ignoreSet)
        {
            var rotConFromArr = from.GetComponentsInChildren<RotationConstraint>(true);
            const string typeString = "RotationConstraint";

            for(int i = 0; i < rotConFromArr.Length; i++)
            {
                var rotCon = rotConFromArr[i];

                if(ignoreSet != null && Helpers.ShouldIgnoreObject(rotCon.transform, ignoreSet, Settings.bCopier_ignoreArray_includeChildren))
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
        /// <param name="from">Object to copy from</param>
        /// <param name="to">Object to copy to</param>
        /// <param name="createGameObjects">Whether to create game objects if missing</param>
        /// <param name="ignoreSet != null"></param>
        /// <param name="ignoreSet">Set of transforms to ignore when copying</param>
        internal static void CopyAllScaleConstraints(GameObject from, GameObject to, bool createGameObjects, HashSet<Transform> ignoreSet)
        {
            var scaleConFromArr = from.GetComponentsInChildren<ScaleConstraint>(true);
            const string typeString = "ScaleConstraint";

            foreach(var scaleCon in scaleConFromArr)
            {
                if(Helpers.ShouldIgnoreObject(scaleCon.transform, ignoreSet, Settings.bCopier_ignoreArray_includeChildren))
                    continue;

                var transTo = Helpers.FindTransformInAnotherHierarchy(scaleCon.transform, to.transform, createGameObjects);
                if(transTo == null)
                    continue;
                
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

        /// <summary>
        /// Copies all audio sources on object and it's children.
        /// </summary>
        /// <param name="createGameObjects">Whether to create missing objects</param>
        /// <param name="ignoreSet">Set of transforms to ignore when copying</param>
        /// <param name="from">Object to copy from</param>
        /// <param name="to">Object to copy to</param>
        internal static void CopyAllAudioSources(GameObject from, GameObject to, bool createGameObjects, HashSet<Transform> ignoreSet)
        {
            if(from == null || to == null)
                return;

            var audioFromArr = from.GetComponentsInChildren<AudioSource>(true);
            string typeName = nameof(AudioSource);

            foreach(var audioFrom in audioFromArr)
            {
                if(Helpers.ShouldIgnoreObject(audioFrom.transform, ignoreSet, Settings.bCopier_ignoreArray_includeChildren))
                    continue;
                
                var transTo = Helpers.FindTransformInAnotherHierarchy(audioFrom.transform, to.transform, createGameObjects);
                if(!transTo)
                    continue;

                var audioToObj = transTo.gameObject;
                string log = String.Format(Strings.Log.copyAttempt, typeName, audioFrom.gameObject, audioToObj);

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

#if VRC_SDK_VRCSDK3
                    var spatialAudioFrom = audioFrom.GetComponent(PumkinsTypeCache.VRC_SpatialAudioSource);
                    if(spatialAudioFrom)
                    {
                        var spatialAudioTo = audioToObj.GetComponent(PumkinsTypeCache.VRC_SpatialAudioSource);
                        if(spatialAudioTo == null && Settings.bCopier_audioSources_createMissing)
                            spatialAudioTo = audioToObj.AddComponent(PumkinsTypeCache.VRC_SpatialAudioSource);

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
        /// <param name="ignoreSet">Set of transforms to ignore when copying</param>
        internal static void CopyAllAnimators(GameObject from, GameObject to, bool createGameObjects, bool copyRootAnimator, HashSet<Transform> ignoreSet)
        {
            if(from == null || to == null)
                return;

            var aFromArr = from.GetComponentsInChildren<Animator>(true);

            foreach(var animFrom in aFromArr)
            {
                if(!copyRootAnimator && animFrom.transform.parent == null || Helpers.ShouldIgnoreObject(animFrom.transform, ignoreSet, Settings.bCopier_ignoreArray_includeChildren))
                    continue;

                string log = Strings.Log.copyAttempt;
                string type = nameof(Animator);

                var tTo = Helpers.FindTransformInAnotherHierarchy(animFrom.transform, to.transform, createGameObjects);
                if(!tTo)
                    continue;

                var aToObj = tTo.gameObject;

                if(animFrom != null)
                {
                    var lTo = aToObj.GetComponent<Animator>();

                    if(lTo == null && Settings.bCopier_animators_createMissing)
                    {
                        lTo = aToObj.AddComponent<Animator>();
                    }

                    if((lTo != null && Settings.bCopier_animators_copySettings) || Settings.bCopier_animators_createMissing)
                    {
                        ComponentUtility.CopyComponent(animFrom);
                        ComponentUtility.PasteComponentValues(lTo);
                        PumkinsAvatarTools.Log(log + " - " + Strings.Log.success, LogType.Log, type, tTo.gameObject.name, animFrom.gameObject.name);
                    }
                }
                else
                {
                    PumkinsAvatarTools.Log(log + " " + Strings.Log.failedDoesntHave, LogType.Warning, animFrom.gameObject.name.ToString(), animFrom.GetType().ToString());
                }
            }
        }

        /// <summary>
        /// Copies all lights in object and it's children to another object.
        /// </summary>
        /// <param name="to">Object to copy to</param>
        /// <param name="createGameObjects">Whether to create missing game objects</param>
        /// <param name="ignoreSet">Set of transforms to ignore when copying</param>
        /// <param name="from">Object to copy from</param>
        internal static void CopyAllLights(GameObject from, GameObject to, bool createGameObjects, HashSet<Transform> ignoreSet)
        {
            if(from == null || to == null)
                return;

            var lFromArr = from.GetComponentsInChildren<Light>(true);

            foreach(var lFrom in lFromArr)
            {
                if(ignoreSet != null && Helpers.ShouldIgnoreObject(lFrom.transform, ignoreSet, Settings.bCopier_ignoreArray_includeChildren))
                    continue;
                
                string log = Strings.Log.copyAttempt;
                string type = nameof(Light);

                var tTo = Helpers.FindTransformInAnotherHierarchy(lFrom.transform, to.transform, createGameObjects);
                if(!tTo)
                    continue;


                if(lFrom != null)
                {
                    var lTo = tTo.GetComponent<Light>();

                    if(lTo == null && Settings.bCopier_lights_createMissing)
                    {
                        lTo = tTo.gameObject.AddComponent<Light>();
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
                    PumkinsAvatarTools.Log(log + " " + Strings.Log.failedDoesntHave, LogType.Warning, lFrom.gameObject.name, type);
                }
            }
        }

        /// <summary>
        /// Copies all MeshRenderers in object and it's children to another object.
        /// </summary>
        /// <param name="to">Object to copy to</param>
        /// <param name="createGameObjects">Whether to create missing game objects</param>
        /// <param name="ignoreSet">Set of transforms to ignore when copying</param>
        /// <param name="from">Object to copy from</param>
        internal static void CopyAllMeshRenderers(GameObject from, GameObject to, bool createGameObjects, HashSet<Transform> ignoreSet)
        {
            if(from == null || to == null)
                return;

            var mFromArr = from.GetComponentsInChildren<MeshRenderer>(true);
            string type = nameof(MeshRenderer);

            foreach(var rFrom in mFromArr)
            {
                if(Helpers.ShouldIgnoreObject(rFrom.transform, ignoreSet, Settings.bCopier_ignoreArray_includeChildren))
                    continue;
                
                var tTo = Helpers.FindTransformInAnotherHierarchy(rFrom.transform, to.transform, createGameObjects);
                if(!tTo)
                    continue;
                
                string log = string.Format(Strings.Log.copyAttempt, type, rFrom.gameObject.name, tTo.gameObject.name);

                var rToObj = tTo.gameObject;

                var fFrom = rFrom.GetComponent<MeshFilter>();

                if(fFrom == null)
                    continue;
                
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

        internal static void CopyAvatarDescriptor(GameObject from, GameObject to, HashSet<Transform> ignoreSet)
        {
            Type descType = PumkinsTypeCache.VRC_AvatarDescriptor;
            Type pipelineType = PumkinsTypeCache.PipelineManager;
            
            if(to == null || from == null || descType == null || pipelineType == null)
                return;

            if(Helpers.ShouldIgnoreObject(from.transform, ignoreSet))
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
                    "lipSyncJawClosed", "lipSyncJawOpen", "AnimationPreset", "autoFootsteps", "autoLocomotion"
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
                descPropNames.Add("enableEyeLook");
                descPropNames.Add("customEyeLookSettings");
            }

            if(Settings.bCopier_descriptor_copyExpressions)
            {
                descPropNames.AddRange(new []
                {
                    "customExpressions", "expressionsMenu", "expressionParameters"
                });
            }

            List<SerializedProperty> transLocalize = new List<SerializedProperty>();
            if (Settings.bCopier_descriptor_copyColliders)
            {
                var colliderProps = new[]
                {
                    "collider_head", "collider_torso",
                    "collider_handR", "collider_footR", "collider_fingerIndexR", "collider_fingerMiddleR",
                    "collider_fingerRingR", "collider_fingerLittleR",
                    "collider_handL", "collider_footL", "collider_fingerIndexL", "collider_fingerMiddleL",
                    "collider_fingerRingL", "collider_fingerLittleL"
                };
                
                descPropNames.Add("collidersMirrored");
                descPropNames.AddRange(colliderProps);
                
                foreach(var colProp in colliderProps)
                {
                    var prop = sDescTo.FindProperty(colProp)?.FindPropertyRelative("transform");
                    if(prop != null)
                        transLocalize.Add(prop);
                }
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
                transLocalize.AddRange(new[] 
                {
                    eyes.FindPropertyRelative("leftEye"),
                    eyes.FindPropertyRelative("rightEye"),
                    eyes.FindPropertyRelative("upperLeftEyelid"),
                    eyes.FindPropertyRelative("upperRightEyelid"),
                    eyes.FindPropertyRelative("lowerLeftEyelid"),
                    eyes.FindPropertyRelative("lowerRightEyelid"),
                });
            }

            Helpers.MakeReferencesLocal<Transform>(to.transform, true, transLocalize.ToArray());
            
            SerializedProperty[] rendererLocalize =
            {
                sDescTo.FindProperty("VisemeSkinnedMesh"),
                eyes != null ? eyes.FindPropertyRelative("eyelidsSkinnedMesh") : null,
            };
            Helpers.MakeReferencesLocal<SkinnedMeshRenderer>(to.transform, true, rendererLocalize);
            sDescTo.ApplyModifiedPropertiesWithoutUndo();
        }
        
        internal static void CopyTransformActiveStateTagsAndLayer(GameObject from, GameObject to, HashSet<Transform> ignoreSet)
        {
            if(from == null || to == null || !(Settings.bCopier_transforms_copyActiveState || Settings.bCopier_transforms_copyLayerAndTag))
                return;
            
            var tFromArr = from.GetComponentsInChildren<Transform>(true);

            foreach(var tFrom in tFromArr)
            {
                if(tFrom == tFrom.root || (ignoreSet != null && Helpers.ShouldIgnoreObject(tFrom, ignoreSet, Settings.bCopier_ignoreArray_includeChildren)))
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

        internal static void CopyCameras(GameObject from, GameObject to, bool createGameObjects, HashSet<Transform> ignoreSet)
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
                if(!tTo || (ignoreSet != null && Helpers.ShouldIgnoreObject(camFrom.transform, ignoreSet, Settings.bCopier_ignoreArray_includeChildren)))
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