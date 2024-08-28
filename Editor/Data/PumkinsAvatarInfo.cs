using System;
using System.Collections.Generic;
using System.Linq;
using Pumkin.DependencyChecker;
using UnityEngine;
using Pumkin.AvatarTools;
using System.Text;
using UnityEditor;

#if VRC_SDK_VRCSDK3
using VRC.SDKBase;
using VRC.SDK3.Dynamics.PhysBone.Components;
using VRC.SDKBase.Validation.Performance;
using VRC.SDKBase.Validation.Performance.Stats;
#endif

namespace Pumkin.DataStructures
{
    public class PumkinsAvatarInfo //Need to improve this class sometime when I overhaul the performance stats
    {
#if VRC_SDK_VRCSDK3 || VRC_SDK_VRCSDK2
        AvatarPerformanceStats PerfStats
        {
            get
            {
                if(_perfStats == null)
                {
                    _perfStats = new AvatarPerformanceStats(EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android);
                }

                return _perfStats;
            }
        }
        AvatarPerformanceStats _perfStats;
#endif
        public string Name { get; private set; }
        public string CachedInfo { get; private set; }

        public int SkinnedMeshRenderers { get; private set; }
        public int SkinnedMeshRenderers_Total { get; private set; }
        public int MeshRenderers { get; private set; }
        public int MeshRenderers_Total { get; private set; }
        public int PhysBoneTransforms { get; private set; }
        public int PhysBoneTransforms_Total { get; private set; }
        public int PhysBoneColliders { get; private set; }
        public int PhysBoneColliders_Total { get; private set; }
        public int PhysBoneColliderTransforms { get; private set; }
        public int PhysBoneColliderTransforms_Total { get; private set; }
        public int DynamicBoneTransforms { get; private set; }
        public int DynamicBoneTransforms_Total { get; private set; }
        public int DynamicBoneColliders { get; private set; }
        public int DynamicBoneColliders_Total { get; private set; }
        public int DynamicBoneColliderTransforms { get; private set; }
        public int DynamicBoneColliderTransforms_Total { get; private set; }
        public int Polygons { get; private set; }
        public int Polygons_Total { get; private set; }
        public int MaterialSlots { get; private set; }
        public int MaterialSlots_Total { get; private set; }
        public int UniqueMaterials { get; private set; }
        public int UniqueMaterials_Total { get; private set; }
        public int ShaderCount { get; private set; }
        public int ParticleSystems { get; private set; }
        public int ParticleSystems_Total { get; private set; }
        public int GameObjects { get; private set; }
        public int GameObjects_Total { get; private set; }
        public int MaxParticles { get; private set; }
        public int MaxParticles_Total { get; private set; }
        public int Bones { get; private set; }
        public int PhysboneComponents { get; private set; }
        public int PhysboneComponents_Total { get; private set; }

        public PumkinsAvatarInfo()
        {
            CachedInfo = null;

            SkinnedMeshRenderers = 0;
            SkinnedMeshRenderers_Total = 0;

            MeshRenderers = 0;
            MeshRenderers_Total = 0;

            PhysBoneTransforms = 0;
            PhysBoneTransforms_Total = 0;
            PhysBoneColliders = 0;
            PhysBoneColliders_Total = 0;
            PhysBoneColliderTransforms = 0;
            PhysBoneColliderTransforms_Total = 0;

            DynamicBoneTransforms = 0;
            DynamicBoneTransforms_Total = 0;
            DynamicBoneColliders = 0;
            DynamicBoneColliders_Total = 0;
            DynamicBoneColliderTransforms = 0;
            DynamicBoneColliderTransforms_Total = 0;

            Polygons = 0;
            Polygons_Total = 0;
            MaterialSlots = 0;
            MaterialSlots_Total = 0;
            UniqueMaterials = 0;
            UniqueMaterials_Total = 0;
            ShaderCount = 0;

            ParticleSystems = 0;
            ParticleSystems_Total = 0;
            MaxParticles = 0;
            MaxParticles_Total = 0;

            GameObjects = 0;
            GameObjects_Total = 0;
        }

        public PumkinsAvatarInfo(GameObject o)
        {
            if(o == null)
                return;
#if VRC_SDK_VRCSDK3
            try
            {
                AvatarPerformance.CalculatePerformanceStats(o.name, o, PerfStats, EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android);
            }
            catch { }
#endif

            Name = o.name;

            var shaderHash = new HashSet<Shader>();
            var matList = new List<Material>();
            var matList_total = new List<Material>();

            var ts = o.GetComponentsInChildren<Transform>(true);
            foreach(var t in ts)
            {
                GameObjects_Total++;
                if(t.gameObject.activeInHierarchy)
                    GameObjects++;
            }

            var sRenders = o.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            var bonesList = new List<string>();
            foreach(var r in sRenders)
            {
                SkinnedMeshRenderers_Total++;
                if(r.sharedMesh)
                    Polygons_Total += r.sharedMesh.triangles.Length / 3;
                if(r.bones != null)
                {
                    var bones = r.bones.Where(b => b != null).Select(b => b.gameObject.name);
                    bonesList.AddRange(bones);
                }

                if(r.gameObject.activeInHierarchy && r.enabled)
                {
                    SkinnedMeshRenderers++;
                    if(r.sharedMesh)
                        Polygons += r.sharedMesh.triangles.Length / 3;
                }

                foreach(var mat in r.sharedMaterials)
                {
                    if(mat != null)
                    {
                        shaderHash.Add(mat.shader);
                        matList_total.Add(mat);

                        if(r.gameObject.activeInHierarchy && r.enabled)
                        {
                            matList.Add(mat);
                        }
                    }
                }
            }

            Bones = new HashSet<string>(bonesList).Count;

            var renders = o.GetComponentsInChildren<MeshRenderer>(true);
            foreach(var r in renders)
            {
                var filter = r.GetComponent<MeshFilter>();

                if(filter != null && filter.sharedMesh != null)
                {
                    MeshRenderers_Total++;
                    if(filter.sharedMesh != null)
                        Polygons_Total += filter.sharedMesh.triangles.Length / 3;

                    if(r.gameObject.activeInHierarchy && r.enabled)
                    {
                        MeshRenderers++;
                        if(filter.sharedMesh != null)
                            Polygons += filter.sharedMesh.triangles.Length / 3;
                    }
                }

                foreach(var mat in r.sharedMaterials)
                {
                    if(mat != null)
                    {
                        shaderHash.Add(mat.shader);
                        matList_total.Add(mat);

                        if(r.gameObject.activeInHierarchy && r.enabled)
                        {
                            matList.Add(mat);
                        }
                    }
                }
            }

            MaterialSlots = matList.Count;
            MaterialSlots_Total = matList_total.Count;

            UniqueMaterials = new HashSet<Material>(matList).Count;
            UniqueMaterials_Total = new HashSet<Material>(matList_total).Count;

#if VRC_SDK_VRCSDK3
            var pbColliders = o.GetComponentsInChildren(PumkinsTypeCache.PhysBoneCollider, true);
            foreach (var p in pbColliders)
            {
                PhysBoneColliders_Total++;

                if(p.gameObject.activeInHierarchy)
                    PhysBoneColliders++;
            }

            var pbones = o.GetComponentsInChildren<VRCPhysBone>(true);
            foreach (var pb in pbones)
            {
                PhysboneComponents_Total++;
                if(pb.enabled)
                    PhysboneComponents++;

                if(pb.ignoreTransforms != null)
                {
                    var exclusions = pb.ignoreTransforms;
                    Transform[] rootChildren = null;

                    if(pb.rootTransform != null)
                        rootChildren = pb.rootTransform.GetComponentsInChildren<Transform>(true);
                    else
                        rootChildren = pb.transform.GetComponentsInChildren<Transform>(true);
                    
                    int affected = 0;
                    int affected_total = 0;

                    foreach (var t in rootChildren)
                    {
                        if(exclusions.IndexOf(t) == -1)
                        {
                            affected_total++;
                            if(t.gameObject.activeInHierarchy && pb.enabled)
                                affected++;
                        }
                        else
                        {
                            var childChildren = t.GetComponentsInChildren<Transform>(true);
                            for (int z = 1; z < childChildren.Length; z++)
                            {
                                affected_total--;
                                if(childChildren[z].gameObject.activeInHierarchy && pb.enabled)
                                    affected--;
                            }
                        }
                    }

                    foreach (var c in pb.colliders)
                    {
                        if(c != null)
                        {
                            PhysBoneColliderTransforms += affected;
                            PhysBoneColliderTransforms_Total += affected_total;
                            break;
                        }
                    }

                    PhysBoneTransforms += affected;
                    PhysBoneTransforms_Total += affected_total;
                }
            }
#endif
            if(PumkinsTypeCache.DynamicBone != null)
            {
                Type dynamicBoneColliderType = PumkinsTypeCache.DynamicBoneCollider;
                var dbColliders = o.GetComponentsInChildren(dynamicBoneColliderType, true);
                foreach(var c in dbColliders)
                {
                    DynamicBoneColliders_Total++;

                    if(c.gameObject.activeInHierarchy)
                        DynamicBoneColliders++;
                }

                Type dynamicBoneType = PumkinsTypeCache.DynamicBone;
                var dbones = o.GetComponentsInChildren(dynamicBoneType, true);
                foreach(var d in dbones)
                {
                    Transform root = (Transform)dynamicBoneType.GetField("m_Root").GetValue(d);
                    if(root != null)
                    {
                        List<Transform> exclusions = (List<Transform>)dynamicBoneType.GetField("m_Exclusions").GetValue(d);
                        var rootChildren = root.GetComponentsInChildren<Transform>(true);

                        int affected = 0;
                        int affected_total = 0;

                        foreach(var t in rootChildren)
                        {
                            if(exclusions.IndexOf(t) == -1)
                            {
                                affected_total++;

                                if(t.gameObject.activeInHierarchy && ((Behaviour)d).enabled)
                                {
                                    affected++;
                                }
                            }
                            else
                            {
                                var childChildren = t.GetComponentsInChildren<Transform>(true);

                                for(int z = 1; z < childChildren.Length; z++)
                                {
                                    affected_total -= 1;

                                    if(childChildren[z].gameObject.activeInHierarchy && ((Behaviour)d).enabled)
                                    {
                                        affected -= 1;
                                    }
                                }
                            }
                        }
                        var list = dynamicBoneType.GetField("m_Colliders").GetValue(d);
                        int length = (int)list.GetType().GetProperty("Count").GetValue(list);
                        for(int i = 0; i < length; i++)
                        {
                            var collider = list.GetType().GetMethod("get_Item").Invoke(list, new[] { (object)i });
                            if(collider != null)
                            {
                                DynamicBoneColliderTransforms += affected;
                                DynamicBoneColliderTransforms_Total += affected_total;
                                break;
                            }
                        }

                        DynamicBoneTransforms += affected;
                        DynamicBoneTransforms_Total += affected_total;
                    }
                }
            }

            var ptc = o.GetComponentsInChildren<ParticleSystem>(true);
            foreach(var p in ptc)
            {
                ParticleSystems_Total++;
                MaxParticles_Total += p.main.maxParticles;

                if(p.gameObject.activeInHierarchy && p.emission.enabled)
                {
                    ParticleSystems++;
                    MaxParticles += p.main.maxParticles;
                }
            }

            ShaderCount = shaderHash.Count;
        }

        public static PumkinsAvatarInfo GetInfo(GameObject o, out string toString)
        {
            PumkinsAvatarInfo a = new PumkinsAvatarInfo(o);
            toString = a.ToString();
            return a;
        }

        public override string ToString()
        {
            if(!string.IsNullOrEmpty(CachedInfo))
                return CachedInfo;

            bool useDefault = false;
            StringBuilder sb = new StringBuilder();
            try
            {
#if VRC_SDK_VRCSDK3
                sb.AppendLine(string.Format(Strings.AvatarInfo.name, Name));
                sb.AppendLine(string.Format(Strings.AvatarInfo.line));
                sb.AppendLine(string.Format(Strings.AvatarInfo.gameObjects, GameObjects, GameObjects_Total));
                sb.AppendLine(string.Format(Strings.AvatarInfo.bones, Bones, PerfStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.BoneCount)));
                sb.AppendLine();
                sb.AppendLine(string.Format(Strings.AvatarInfo.skinnedMeshRenderers, SkinnedMeshRenderers, SkinnedMeshRenderers_Total, PerfStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.SkinnedMeshCount)));
                sb.AppendLine(string.Format(Strings.AvatarInfo.meshRenderers, MeshRenderers, MeshRenderers_Total, PerfStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.MeshCount)));
                sb.AppendLine(string.Format(Strings.AvatarInfo.polygons, Polygons, Polygons_Total, PerfStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.PolyCount)));
                sb.AppendLine();
                sb.AppendLine(string.Format(Strings.AvatarInfo.usedMaterialSlots, MaterialSlots, MaterialSlots_Total, PerfStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.MaterialCount)));
                sb.AppendLine(string.Format(Strings.AvatarInfo.uniqueMaterials, UniqueMaterials, UniqueMaterials_Total));
                sb.AppendLine(string.Format(Strings.AvatarInfo.shaders, ShaderCount));
                sb.AppendLine();
                sb.AppendLine(string.Format(Strings.AvatarInfo.physBoneComponents, PhysboneComponents, PhysboneComponents_Total, PerfStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.PhysBoneComponentCount)));
                sb.AppendLine(string.Format(Strings.AvatarInfo.physBoneTransforms, PhysBoneTransforms, PhysBoneTransforms_Total, PerfStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.PhysBoneTransformCount)));
                sb.AppendLine(string.Format(Strings.AvatarInfo.physBoneColliders, PhysBoneColliders, PhysBoneColliders_Total, PerfStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.PhysBoneColliderCount)));
                sb.AppendLine(string.Format(Strings.AvatarInfo.physBoneColliderTransforms, PhysBoneColliderTransforms, PhysBoneColliderTransforms_Total, PerfStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.PhysBoneCollisionCheckCount)));
                sb.AppendLine();

                if(_DependencyChecker.DBonesVersion != _DependencyChecker.PumkinsDBonesVersion.NotFound)
                {
                    sb.AppendLine(string.Format(Strings.AvatarInfo.dynamicBoneTransforms, DynamicBoneTransforms, DynamicBoneTransforms_Total, "?"));
                    sb.AppendLine(string.Format(Strings.AvatarInfo.dynamicBoneColliders, DynamicBoneColliders, DynamicBoneColliders_Total, "?"));
                    sb.AppendLine(string.Format(Strings.AvatarInfo.dynamicBoneColliderTransforms, DynamicBoneColliderTransforms, DynamicBoneColliderTransforms_Total, "?"));
                    sb.AppendLine();
                }

                sb.AppendLine(string.Format(Strings.AvatarInfo.particleSystems, ParticleSystems, ParticleSystems_Total, PerfStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.ParticleSystemCount)));
                sb.AppendLine(string.Format(Strings.AvatarInfo.maxParticles, MaxParticles, MaxParticles_Total, PerfStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.ParticleTotalCount)));
                sb.AppendLine(Strings.AvatarInfo.line);
                sb.AppendLine(string.Format(Strings.AvatarInfo.overallPerformance, PerfStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.Overall)));
#else
                useDefault = true;
#endif
            }
            catch
            {
                useDefault = true;
            }

            if(useDefault)
            {
                sb.AppendLine(string.Format(Strings.AvatarInfo.name, Name));
                sb.AppendLine(string.Format(Strings.AvatarInfo.line));
                sb.AppendLine(string.Format(Strings.AvatarInfo.gameObjects, GameObjects, GameObjects_Total));
                sb.AppendLine(string.Format(Strings.AvatarInfo.bones, Bones, "?"));
                sb.AppendLine();
                sb.AppendLine(string.Format(Strings.AvatarInfo.skinnedMeshRenderers, SkinnedMeshRenderers, SkinnedMeshRenderers_Total, "?"));
                sb.AppendLine(string.Format(Strings.AvatarInfo.meshRenderers, MeshRenderers, MeshRenderers_Total, "?"));
                sb.AppendLine(string.Format(Strings.AvatarInfo.polygons, Polygons, Polygons_Total, "?"));
                sb.AppendLine();
                sb.AppendLine(string.Format(Strings.AvatarInfo.usedMaterialSlots, MaterialSlots, MaterialSlots_Total, "?"));
                sb.AppendLine(string.Format(Strings.AvatarInfo.uniqueMaterials, UniqueMaterials, UniqueMaterials_Total));
                sb.AppendLine(string.Format(Strings.AvatarInfo.shaders, ShaderCount));
                sb.AppendLine();
                sb.AppendLine(string.Format(Strings.AvatarInfo.physBoneTransforms, PhysBoneTransforms, PhysBoneTransforms_Total, "?"));
                sb.AppendLine(string.Format(Strings.AvatarInfo.physBoneColliders, PhysBoneColliders, PhysBoneColliders_Total, "?"));
                sb.AppendLine(string.Format(Strings.AvatarInfo.physBoneColliderTransforms, PhysBoneColliderTransforms, PhysBoneColliderTransforms_Total, "?"));
                sb.AppendLine();
                sb.AppendLine(string.Format(Strings.AvatarInfo.dynamicBoneTransforms, DynamicBoneTransforms, DynamicBoneTransforms_Total, "?"));
                sb.AppendLine(string.Format(Strings.AvatarInfo.dynamicBoneColliders, DynamicBoneColliders, DynamicBoneColliders_Total, "?"));
                sb.AppendLine(string.Format(Strings.AvatarInfo.dynamicBoneColliderTransforms, DynamicBoneColliderTransforms, DynamicBoneColliderTransforms_Total, "?"));
                sb.AppendLine();
                sb.AppendLine(string.Format(Strings.AvatarInfo.particleSystems, ParticleSystems, ParticleSystems_Total, "?"));
                sb.AppendLine(string.Format(Strings.AvatarInfo.maxParticles, MaxParticles, MaxParticles_Total, "?"));
                sb.AppendLine(Strings.AvatarInfo.line);
                sb.AppendLine(string.Format(Strings.AvatarInfo.overallPerformance, "?"));
                sb.AppendLine("Not supported in this version of the VRChat SDK");
            }

            CachedInfo = sb.ToString();
            return CachedInfo;

        }

        public static bool operator true(PumkinsAvatarInfo x) { return x != null; }
        public static bool operator false(PumkinsAvatarInfo x) { return x != null; }
    }
}
