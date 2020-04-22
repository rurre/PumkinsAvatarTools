using Pumkin.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VRC.SDKBase;
using VRCSDK2.Validation.Performance;
using VRCSDK2.Validation.Performance.Stats;

namespace Pumkin.AvatarTools
{
    public class PumkinsAvatarInfo //Need to improve this class sometime when I overhaul the performance stats
    {
#if PUMKIN_VRCSDK2
        AvatarPerformanceStats perfStats = new AvatarPerformanceStats();
#endif
        public string Name { get; private set; }
        public string CachedInfo { get; private set; }

        public int SkinnedMeshRenderers { get; private set; }
        public int SkinnedMeshRenderers_Total { get; private set; }
        public int MeshRenderers { get; private set; }
        public int MeshRenderers_Total { get; private set; }
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
        public int IKFollowers { get; private set; }
        public int IKFollowers_Total { get; private set; }

        public PumkinsAvatarInfo()
        {
            CachedInfo = null;

            SkinnedMeshRenderers = 0;
            SkinnedMeshRenderers_Total = 0;

            MeshRenderers = 0;
            MeshRenderers_Total = 0;

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

            IKFollowers = 0;
        }

        public PumkinsAvatarInfo(GameObject o) : base()
        {
            if(o == null)
                return;

#if PUMKIN_VRCSDK2
            try
            {
                AvatarPerformance.CalculatePerformanceStats(o.name, o, perfStats);
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
                GameObjects_Total += 1;
                if(t.gameObject.activeInHierarchy)
                    GameObjects += 1;
            }

            var sRenders = o.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            var bonesList = new List<string>();
            foreach(var r in sRenders)
            {
                SkinnedMeshRenderers_Total += 1;
                if(r.sharedMesh)
                    Polygons_Total += r.sharedMesh.triangles.Length / 3;
                if(r.bones != null)
                {
                    var bones = r.bones.Where(b => b != null).Select(b => b.gameObject.name);
                    bonesList.AddRange(bones);
                }

                if(r.gameObject.activeInHierarchy && r.enabled)
                {
                    SkinnedMeshRenderers += 1;
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
                    MeshRenderers_Total += 1;
                    if(filter.sharedMesh != null)
                        Polygons_Total += filter.sharedMesh.triangles.Length / 3;

                    if(r.gameObject.activeInHierarchy && r.enabled)
                    {
                        MeshRenderers += 1;
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

#if PUMKIN_DBONES || PUMKIN_OLD_DBONES

            var dbColliders = o.GetComponentsInChildren<DynamicBoneCollider>(true);
            foreach(var c in dbColliders)
            {
                DynamicBoneColliders_Total += 1;

                if(c.gameObject.activeInHierarchy)
                    DynamicBoneColliders += 1;
            }

            var dbones = o.GetComponentsInChildren<DynamicBone>(true);
            foreach(var d in dbones)
            {
                if(d.m_Root != null)
                {
                    var exclusions = d.m_Exclusions;
                    var rootChildren = d.m_Root.GetComponentsInChildren<Transform>(true);

                    int affected = 0;
                    int affected_total = 0;

                    foreach(var t in rootChildren)
                    {
                        if(exclusions.IndexOf(t) == -1)
                        {
                            affected_total += 1;

                            if(t.gameObject.activeInHierarchy && d.enabled)
                            {
                                affected += 1;
                            }
                        }
                        else
                        {
                            var childChildren = t.GetComponentsInChildren<Transform>(true);

                            for(int z = 1; z < childChildren.Length; z++)
                            {
                                affected_total -= 1;

                                if(childChildren[z].gameObject.activeInHierarchy && d.enabled)
                                {
                                    affected -= 1;
                                }
                            }
                        }
                    }

                    foreach(var c in d.m_Colliders)
                    {
                        if(c != null)
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

#endif

            var ptc = o.GetComponentsInChildren<ParticleSystem>(true);
            foreach(var p in ptc)
            {
                ParticleSystems_Total += 1;
                MaxParticles_Total += p.main.maxParticles;

                if(p.gameObject.activeInHierarchy && p.emission.enabled)
                {
                    ParticleSystems += 1;
                    MaxParticles += p.main.maxParticles;
                }
            }

            ShaderCount = shaderHash.Count;

            var ikf = o.GetComponentsInChildren<VRC_IKFollower>(true);
            foreach(var ik in ikf)
            {
                IKFollowers_Total += 1;

                if(ik.gameObject.activeInHierarchy)
                    IKFollowers += 1;
            }
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
            else
            {
                if(this == null)
                {
                    return null;
                }
                try
                {
#if PUMKIN_VRCSDK2
                    CachedInfo =
                    string.Format(Strings.AvatarInfo.name, Name) + "\n" +
                    string.Format(Strings.AvatarInfo.line) + "\n" +
                    string.Format(Strings.AvatarInfo.gameObjects, GameObjects, GameObjects_Total) + "\n" +
                    string.Format(Strings.AvatarInfo.bones, Bones, perfStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.BoneCount)) + "\n\n" +
                    string.Format(Strings.AvatarInfo.skinnedMeshRenderers, SkinnedMeshRenderers, SkinnedMeshRenderers_Total, perfStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.SkinnedMeshCount)) + "\n" +
                    string.Format(Strings.AvatarInfo.meshRenderers, MeshRenderers, MeshRenderers_Total, perfStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.MeshCount)) + "\n" +
                    string.Format(Strings.AvatarInfo.polygons, Polygons, Polygons_Total, perfStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.PolyCount)) + "\n\n" +
                    string.Format(Strings.AvatarInfo.usedMaterialSlots, MaterialSlots, MaterialSlots_Total, perfStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.MaterialCount)) + "\n" +
                    string.Format(Strings.AvatarInfo.uniqueMaterials, UniqueMaterials, UniqueMaterials_Total) + "\n" +
                    string.Format(Strings.AvatarInfo.shaders, ShaderCount) + "\n\n" +
                    string.Format(Strings.AvatarInfo.dynamicBoneTransforms, DynamicBoneTransforms, DynamicBoneTransforms_Total, perfStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.DynamicBoneSimulatedBoneCount)) + "\n" +
                    string.Format(Strings.AvatarInfo.dynamicBoneColliders, DynamicBoneColliders, DynamicBoneColliders_Total, perfStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.DynamicBoneColliderCount)) + "\n" +
                    string.Format(Strings.AvatarInfo.dynamicBoneColliderTransforms, DynamicBoneColliderTransforms, DynamicBoneColliderTransforms_Total, perfStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.DynamicBoneCollisionCheckCount)) + "\n\n" +
                    string.Format(Strings.AvatarInfo.particleSystems, ParticleSystems, ParticleSystems_Total, perfStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.ParticleSystemCount)) + "\n" +
                    string.Format(Strings.AvatarInfo.maxParticles, MaxParticles, MaxParticles_Total, perfStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.ParticleTotalCount)) + "\n\n" +
                    string.Format(Strings.AvatarInfo.ikFollowers, IKFollowers, IKFollowers_Total) + "\n" +
                    Strings.AvatarInfo.line + "\n" +
                    string.Format(Strings.AvatarInfo.overallPerformance, perfStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.Overall));
#else
                    CachedInfo =
                    string.Format(Strings.AvatarInfo.name, Name) + "\n" +
                    string.Format(Strings.AvatarInfo.line) + "\n" +
                    string.Format(Strings.AvatarInfo.gameObjects, GameObjects, GameObjects_Total) + "\n" +
                    string.Format(Strings.AvatarInfo.bones, Bones, "?") + "\n\n" +
                    string.Format(Strings.AvatarInfo.skinnedMeshRenderers, SkinnedMeshRenderers, SkinnedMeshRenderers_Total, "?") + "\n" +
                    string.Format(Strings.AvatarInfo.meshRenderers, MeshRenderers, MeshRenderers_Total, "?") + "\n" +
                    string.Format(Strings.AvatarInfo.polygons, Polygons, Polygons_Total, "?") + "\n\n" +
                    string.Format(Strings.AvatarInfo.usedMaterialSlots, MaterialSlots, MaterialSlots_Total, "?") + "\n" +
                    string.Format(Strings.AvatarInfo.uniqueMaterials, UniqueMaterials, UniqueMaterials_Total) + "\n" +
                    string.Format(Strings.AvatarInfo.shaders, ShaderCount) + "\n\n" +
                    string.Format(Strings.AvatarInfo.dynamicBoneTransforms, DynamicBoneTransforms, DynamicBoneTransforms_Total, "?") + "\n" +
                    string.Format(Strings.AvatarInfo.dynamicBoneColliders, DynamicBoneColliders, DynamicBoneColliders_Total, "?") + "\n" +
                    string.Format(Strings.AvatarInfo.dynamicBoneColliderTransforms, DynamicBoneColliderTransforms, DynamicBoneColliderTransforms_Total, "?") + "\n\n" +
                    string.Format(Strings.AvatarInfo.particleSystems, ParticleSystems, ParticleSystems_Total, "?") + "\n" +
                    string.Format(Strings.AvatarInfo.maxParticles, MaxParticles, MaxParticles_Total, "?") + "\n\n" +
                    string.Format(Strings.AvatarInfo.ikFollowers, IKFollowers, IKFollowers_Total) + "\n" +
                    Strings.AvatarInfo.line + "\n" +
                    string.Format(Strings.AvatarInfo.overallPerformance, "?");
#endif
                }
                catch
                {
                    CachedInfo =
                    string.Format(Strings.AvatarInfo.name, Name) + "\n" +
                    string.Format(Strings.AvatarInfo.line) + "\n" +
                    string.Format(Strings.AvatarInfo.gameObjects, GameObjects, GameObjects_Total) + "\n" +
                    string.Format(Strings.AvatarInfo.bones, Bones, "?") + "\n\n" +
                    string.Format(Strings.AvatarInfo.skinnedMeshRenderers, SkinnedMeshRenderers, SkinnedMeshRenderers_Total, "?") + "\n" +
                    string.Format(Strings.AvatarInfo.meshRenderers, MeshRenderers, MeshRenderers_Total, "?") + "\n" +
                    string.Format(Strings.AvatarInfo.polygons, Polygons, Polygons_Total, "?") + "\n\n" +
                    string.Format(Strings.AvatarInfo.usedMaterialSlots, MaterialSlots, MaterialSlots_Total, "?") + "\n" +
                    string.Format(Strings.AvatarInfo.uniqueMaterials, UniqueMaterials, UniqueMaterials_Total) + "\n" +
                    string.Format(Strings.AvatarInfo.shaders, ShaderCount) + "\n\n" +
                    string.Format(Strings.AvatarInfo.dynamicBoneTransforms, DynamicBoneTransforms, DynamicBoneTransforms_Total, "?") + "\n" +
                    string.Format(Strings.AvatarInfo.dynamicBoneColliders, DynamicBoneColliders, DynamicBoneColliders_Total, "?") + "\n" +
                    string.Format(Strings.AvatarInfo.dynamicBoneColliderTransforms, DynamicBoneColliderTransforms, DynamicBoneColliderTransforms_Total, "?") + "\n\n" +
                    string.Format(Strings.AvatarInfo.particleSystems, ParticleSystems, ParticleSystems_Total, "?") + "\n" +
                    string.Format(Strings.AvatarInfo.maxParticles, MaxParticles, MaxParticles_Total, "?") + "\n" +
                    Strings.AvatarInfo.line + "\n" +
                    string.Format(Strings.AvatarInfo.overallPerformance, "?");
                }
                return CachedInfo;
            }
        }

        public static bool operator true(PumkinsAvatarInfo x) { return x != null; }
        public static bool operator false(PumkinsAvatarInfo x) { return !(x == null); }
    }
}
