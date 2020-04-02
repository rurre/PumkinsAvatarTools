using Pumkin.HelperFunctions;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;
using Pumkin.Extensions;
using Pumkin.AvatarTools;
using System.IO;
using VRCSDK2;

#if PUMKIN_VRCSDK2
using VRCSDK2.Validation.Performance;
using VRCSDK2.Validation.Performance.Stats;
#endif

namespace Pumkin.DataStructures
{
    public static class Colors
    {
        public static Color SceneGUIWindow { get; internal set; }
        public static Color DefaultCameraBackground { get; internal set; }
        public static Color DarkCameraBackground { get; internal set; }
        public static Color BallHandle { get; internal set; }
        public static Color LightLabelText { get; internal set; }
        public static Color FoldoutTitleBackground { get; internal set; }

        static Colors()
        {
            SceneGUIWindow = new Color(0.3804f, 0.3804f, 0.3804f, 0.7f);
            DefaultCameraBackground = new Color(0.192f, 0.302f, 0.475f);
            DarkCameraBackground = new Color(0.235f, 0.22f, 0.22f);
            BallHandle = new Color(1, 0.92f, 0.016f, 0.5f);
            LightLabelText = Color.white;
            FoldoutTitleBackground = Color.yellow;
        }
    }

    public static class Styles
    {
        public static GUIStyle Foldout_title { get; internal set; }
        public static GUIStyle Label_mainTitle { get; internal set; }
        public static GUIStyle Label_centered { get; internal set; }
        public static GUIStyle Editor_line { get; internal set; }
        public static GUIStyle Label_rightAligned { get; internal set; }
        public static GUIStyle Foldout { get; internal set; }
        public static GUIStyle HelpBox { get; internal set; }
        public static GUIStyle HelpBox_OneLine { get; internal set; }
        public static GUIStyle Box { get; internal set; }
        public static GUIStyle BigButton { get; internal set; }
        public static GUIStyle LightTextField { get; internal set; }
        public static GUIStyle PaddedBox { get; internal set; }
        public static GUIStyle HeaderToggle { get; internal set; }
        public static GUIStyle CopierToggle { get; internal set; }
        public static GUIStyle BigIconButton { get; internal set; }
        public static GUIStyle ToolbarBigButtons { get; internal set; }
        public static GUIStyle Popup { get; internal set; }
        public static GUIStyle IconButton { get; internal set; }
        public static GUIStyle TextField { get; internal set; }
        public static GUIStyle IconLabel { get; internal set; }
        public static GUIStyle ButtonWithToggle { get; internal set; }

        static Styles()
        {
            BigButton = new GUIStyle("Button")
            {
                fixedHeight = 28f,
                stretchHeight = false,
                stretchWidth = true,
            };

            Foldout_title = new GUIStyle("ToolbarDropDown")
            {
                fontSize = 13,
                fixedHeight = 26,
                fontStyle = FontStyle.Bold,
                contentOffset = new Vector2(5f, 0),
            };

            Label_mainTitle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontStyle = FontStyle.Bold,
                fontSize = 14,
                fixedHeight = 20,
            };

            Label_centered = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.UpperCenter,
            };

            Label_rightAligned = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleRight,
                stretchWidth = true,
            };

            Editor_line = new GUIStyle("box")
            {
                border = new RectOffset(1, 1, 1, 1),
                margin = new RectOffset(5, 5, 1, 1),
                padding = new RectOffset(1, 1, 1, 1),
            };

            HelpBox_OneLine = new GUIStyle("HelpBox")
            {
                fontSize = 12,
                fixedHeight = 21,
                stretchHeight = false,
            };

            PaddedBox = new GUIStyle("box")
            {
                padding = new RectOffset(10, 10, 10, 10),
            };

            ToolbarBigButtons = new GUIStyle("button")
            {
                fixedHeight = 24f,
            };

            IconLabel = new GUIStyle("label")
            {
                fixedWidth = 20f,
                fixedHeight = 20f,
                imagePosition = ImagePosition.ImageOnly,
                padding = new RectOffset(0, 0, 0, 0),
            };

            IconButton = new GUIStyle("button")
            {
                fixedWidth = 20f,
                fixedHeight = 20f,
                imagePosition = ImagePosition.ImageOnly,
                padding = new RectOffset(0, 0, 0, 0),
            };

            CopierToggle = new GUIStyle("Toggle");
            Popup = new GUIStyle("Popup");
            Foldout = new GUIStyle("Foldout");
            HelpBox = new GUIStyle("HelpBox");
            Box = new GUIStyle("box");

            TextField = new GUIStyle("Textfield")
            {
                fixedHeight = 19f,
            };
            TextField.normal.textColor = Color.black;

            LightTextField = new GUIStyle("TextField")
            {
                fixedHeight = 19f,
            };
            LightTextField.normal.textColor = Color.white;

            BigIconButton = new GUIStyle(BigButton);
            BigIconButton.fixedWidth = 40f;

            ButtonWithToggle = new GUIStyle("Button")
            {
                fixedHeight = 19
            };
        }
    }

    public struct Icons
    {
        public static Texture2D Star { get; internal set; }
        public static Texture2D CsScript { get; internal set; }
        public static Texture2D Transform { get; internal set; }
        public static Texture2D Avatar { get; internal set; }
        public static Texture2D SkinnedMeshRenderer { get; internal set; }
        public static Texture2D ColliderBox { get; internal set; }
        public static Texture2D DefaultAsset { get; internal set; }
        public static Texture2D Help { get; internal set; }
        public static Texture2D ParticleSystem { get; internal set; }
        public static Texture2D RigidBody { get; internal set; }
        public static Texture2D Prefab { get; internal set; }
        public static Texture2D TrailRenderer { get; internal set; }
        public static Texture2D BoneIcon { get; internal set; }
        public static Texture2D BoneColliderIcon { get; internal set; }
        public static Texture2D MeshRenderer { get; internal set; }
        public static Texture2D Light { get; internal set; }
        public static Texture2D Animator { get; internal set; }
        public static Texture2D AudioSource { get; internal set; }
        public static Texture2D Joint { get; internal set; }
        public static Texture2D Settings { get; internal set; }
        public static Texture2D Delete { get; internal set; }
        public static Texture2D ToggleOn { get; internal set; }
        public static Texture2D ToggleOff { get; internal set; }
        public static Texture2D SerializableAsset { get; internal set; }

        public static Texture2D DiscordIcon { get; internal set; }
        public static Texture2D GithubIcon { get; internal set; }
        public static Texture2D KofiIcon { get; internal set; }
        public static Texture2D Refresh { get; internal set; }

        static Icons()
        {
#if UNITY_2017
            Star = EditorGUIUtility.FindTexture("Favorite Icon");
            CsScript = EditorGUIUtility.FindTexture("cs Script Icon");
            Transform = EditorGUIUtility.FindTexture("Transform Icon");
            Avatar = EditorGUIUtility.FindTexture("Avatar Icon");
            SkinnedMeshRenderer = EditorGUIUtility.FindTexture("SkinnedMeshRenderer Icon");
            ColliderBox = EditorGUIUtility.FindTexture("BoxCollider Icon");
            DefaultAsset = EditorGUIUtility.FindTexture("DefaultAsset Icon");
            Help = EditorGUIUtility.FindTexture("_Help");
            ParticleSystem = EditorGUIUtility.FindTexture("ParticleSystem Icon");
            RigidBody = EditorGUIUtility.FindTexture("Rigidbody Icon");
            Prefab = EditorGUIUtility.FindTexture("Prefab Icon");
            TrailRenderer = EditorGUIUtility.FindTexture("TrailRenderer Icon");
            MeshRenderer = EditorGUIUtility.FindTexture("MeshRenderer Icon");
            Light = EditorGUIUtility.FindTexture("Light Icon");
            Animator = EditorGUIUtility.FindTexture("Animator Icon");
            AudioSource = EditorGUIUtility.FindTexture("AudioSource Icon");
            Joint = EditorGUIUtility.FindTexture("FixedJoint Icon");
            Refresh = EditorGUIUtility.FindTexture("TreeEditor.Refresh");            
            Delete = EditorGUIUtility.FindTexture("TreeEditor.Trash");
            ToggleOff = EditorGUIUtility.FindTexture("toggle");
            ToggleOn = EditorGUIUtility.FindTexture("toggle on");
            SerializableAsset = EditorGUIUtility.FindTexture("BillboardAsset Icon");

            Settings = Resources.Load("icons/settings-icon") as Texture2D ?? EditorGUIUtility.FindTexture("ClothInspector.SettingsTool");
            BoneIcon = Resources.Load("icons/bone-icon") as Texture2D ?? CsScript;
            BoneColliderIcon = Resources.Load("icons/bonecollider-icon") as Texture2D ?? DefaultAsset;
            DiscordIcon = Resources.Load("icons/discord-logo") as Texture2D ?? Star;
            GithubIcon = Resources.Load("icons/github-logo") as Texture2D ?? Star;
            KofiIcon = Resources.Load("icons/kofi-logo") as Texture2D ?? Star;
#else
            Star = (Texture2D)EditorGUIUtility.IconContent("Favorite Icon").image;
            CsScript = (Texture2D)EditorGUIUtility.IconContent("cs Script Icon").image;
            Transform = (Texture2D)EditorGUIUtility.IconContent("Transform Icon").image;
            Avatar = (Texture2D)EditorGUIUtility.IconContent("Avatar Icon").image;
            SkinnedMeshRenderer = (Texture2D)EditorGUIUtility.IconContent("SkinnedMeshRenderer Icon").image;
            ColliderBox = (Texture2D)EditorGUIUtility.IconContent("BoxCollider Icon").image;
            DefaultAsset = (Texture2D)EditorGUIUtility.IconContent("DefaultAsset Icon").image;
            Help = (Texture2D)EditorGUIUtility.IconContent("_Help").image;
            ParticleSystem = (Texture2D)EditorGUIUtility.IconContent("ParticleSystem Icon").image;
            RigidBody = (Texture2D)EditorGUIUtility.IconContent("Rigidbody Icon").image;
            Prefab = (Texture2D)EditorGUIUtility.IconContent("Prefab Icon").image;
            TrailRenderer = (Texture2D)EditorGUIUtility.IconContent("TrailRenderer Icon").image;
            MeshRenderer = (Texture2D)EditorGUIUtility.IconContent("MeshRenderer Icon").image;
            Light = (Texture2D)EditorGUIUtility.IconContent("Light Icon").image;
            Animator = (Texture2D)EditorGUIUtility.IconContent("Animator Icon").image;
            AudioSource = (Texture2D)EditorGUIUtility.IconContent("AudioSource Icon").image;
            Joint = (Texture2D)EditorGUIUtility.IconContent("FixedJoint Icon").image;
            Settings = (Texture2D)EditorGUIUtility.IconContent("Settings").image;
            Delete = (Texture2D)EditorGUIUtility.IconContent("TreeEditor.Trash").image;
            ToggleOff = (Texture2D)EditorGUIUtility.IconContent("Toggle").image;
            ToggleOn = (Texture2D)EditorGUIUtility.IconContent("Toggle On").image;
            SerializableAsset = (Texture2D)EditorGUIUtility.IconContent("BillboardAsset Icon").image;

            Refresh = EditorGUIUtility.FindTexture("TreeEditor.Refresh");

            BoneIcon = Resources.Load("icons/bone-icon") as Texture2D ?? CsScript;
            BoneColliderIcon = Resources.Load("icons/bonecollider-icon") as Texture2D ?? DefaultAsset;
            DiscordIcon = Resources.Load("icons/discord-logo") as Texture2D ?? Star;
            GithubIcon = Resources.Load("icons/github-logo") as Texture2D ?? Star;
            KofiIcon = Resources.Load("icons/kofi-logo") as Texture2D ?? Star;
#endif
        }

    }

    public class AvatarInfo //Need to improve this class sometime when I overhaul the performance stats
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

        public AvatarInfo()
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

        public AvatarInfo(GameObject o) : base()
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
                    bonesList.AddRange(r.bones.Select(b => b.gameObject.name));

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

        public static AvatarInfo GetInfo(GameObject o, out string toString)
        {
            AvatarInfo a = new AvatarInfo(o);
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

        public static bool operator true(AvatarInfo x) { return x != null; }
        public static bool operator false(AvatarInfo x) { return !(x == null); }
    }

    /// <summary>
    /// Serializable Transform class
    /// </summary>
    [Serializable]
    public class SerialTransform
    {
        public SerialVector3 position, localPosition, scale, localScale, eulerAngles, localEulerAngles;
        public SerialQuaternion rotation, localRotation;

        public SerialTransform()
        {
            this.position = Vector3.zero;
            this.rotation = Quaternion.identity;
            this.scale = Vector3.one;
            this.localEulerAngles = Vector3.zero;
            this.localPosition = Vector3.zero;
            this.localRotation = Quaternion.identity;
            this.localScale = Vector3.one;
        }

        public SerialTransform(Vector3 position, Quaternion rotation, Vector3 scale, Vector3 eulerAngles, Vector3 localEulerAngles, Vector3 localScale, Vector3 localPosition, Quaternion localRotation, float version) : base()
        {
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
            this.localEulerAngles = localEulerAngles;
            this.localPosition = localPosition;
            this.localRotation = localRotation;
            this.localScale = localScale;
        }

        public SerialTransform(Vector3 localPosition, Quaternion localRotation, Vector3 localScale, Vector3 localEulerAngles) : base()
        {
            this.localPosition = localPosition;
            this.localRotation = localRotation;
            this.localScale = localScale;
            this.localEulerAngles = localEulerAngles;
        }

        public SerialTransform(Quaternion localRotation) : base()
        {
            this.localRotation = localRotation;
        }

        public SerialTransform(Transform t) : base()
        {
            position = t.position;
            localPosition = t.localPosition;

            scale = t.localScale;
            localScale = t.localScale;

            rotation = t.rotation;
            localRotation = t.localRotation;

            eulerAngles = t.eulerAngles;
            
            localEulerAngles = t.localEulerAngles;
        }

        public static implicit operator SerialTransform(Transform t)
        {
            return new SerialTransform(t);
        }

        public static implicit operator bool(SerialTransform t)
        {
            if(t != null)
                return true;
            return false;
        }
    }

    /// <summary>
    /// Serializable Quaternion class
    /// </summary>
    [Serializable]
    public struct SerialQuaternion
    {
        public float x, y, z, w;

        public SerialQuaternion(Quaternion q)
        {
            x = q.x;
            y = q.y;
            z = q.z;
            w = q.w;
        }

        private SerialQuaternion(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public static implicit operator SerialQuaternion(Quaternion q)
        {
            return new SerialQuaternion(q);
        }

        public static implicit operator Quaternion(SerialQuaternion q)
        {
            return new Quaternion(q.x, q.y, q.z, q.w);
        }
    }

    /// <summary>
    /// Serializable Vector3 class
    /// </summary>
    [Serializable]
    public struct SerialVector3
    {
        public float x, y ,z;

        public SerialVector3(Vector3 v)
        {
            x = v.x;
            y = v.y;
            z = v.z;
        }

        public SerialVector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static implicit operator SerialVector3(Vector3 v)
        {
            return new SerialVector3(v);
        }

        public static implicit operator Vector3(SerialVector3 v)
        {
            return new Vector3(v.x, v.y, v.z);
        }

        public static SerialVector3 operator *(SerialVector3 v, float f)
        {
            return new SerialVector3(new Vector3(v.x, v.y, v.z) * f);
        }

        public override string ToString()
        {
            return string.Format("({0}, {1}, {2})", x, y, z);
        }
    }

    [Serializable]
    public class PumkinsRendererBlendshapesHolder
    {
        [SerializeField] public string rendererPath;
        [SerializeField] public List<PumkinsBlendshape> blendshapes = new List<PumkinsBlendshape>();
        [SerializeField][HideInInspector] public bool expandedInUI = false; //oof, probably not a good idea to have this ui related bool here
                                                                            //but then again this class only exists so unity can serialize the list of PumkinsBlendshape objects
        public PumkinsRendererBlendshapesHolder(string path, List<PumkinsBlendshape> shapeList)
        {
            rendererPath = path;
            blendshapes = shapeList;
        }

        public static explicit operator PumkinsRendererBlendshapesHolder(SkinnedMeshRenderer render)
        {
            if(!render || render.sharedMesh == null)
                return null;

            string renderPath = Helpers.GetGameObjectPath(render.gameObject);
            var blendshapes = new List<PumkinsBlendshape>();
            for(int i = 0; i < render.sharedMesh.blendShapeCount; i++)
                blendshapes.Add(new PumkinsBlendshape(render.sharedMesh.GetBlendShapeName(i), render.GetBlendShapeWeight(i)));
            
            return new PumkinsRendererBlendshapesHolder(renderPath, blendshapes);
        }
    }

    [Serializable]
    public class PumkinsBlendshape
    {
        [SerializeField] public string name;
        [SerializeField] public float weight;
        [SerializeField] public List<string> otherNames;        

        [SerializeField][HideInInspector] public bool otherNamesExpandedInUI = false; //oof, probably also not a good idea to have this ui related bool here as well                                                                                    

        public PumkinsBlendshape(string name, float weight = 0, List<string> otherNames = null)
        {
            this.name = name;
            this.weight = weight;            
            if(otherNames != null)
                this.otherNames = otherNames;
            else
                this.otherNames = new List<string>();
        }
    }

    public static class HumanRig
    {
        static readonly List<HumanRigBone> Bones = new List<HumanRigBone>()
        {
            new HumanRigBone("Hips", "Hips"),
            new HumanRigBone("LeftUpperLeg", "Left leg"),
            new HumanRigBone("RightUpperLeg", "Right leg"),
            new HumanRigBone("LeftLowerLeg", "Left knee"),
            new HumanRigBone("RightLowerLeg", "Right knee"),
            new HumanRigBone("LeftFoot", "Left ankle"),
            new HumanRigBone("RightFoot", "Right ankle"),
            new HumanRigBone("Spine", "Spine"),
            new HumanRigBone("Chest", "Chest"),
            new HumanRigBone("Neck", "Neck"),
            new HumanRigBone("Head", "Head"),
            new HumanRigBone("LeftShoulder", "Left shoulder"),
            new HumanRigBone("RightShoulder", "Right shoulder"),
            new HumanRigBone("LeftUpperArm", "Left arm"),
            new HumanRigBone("RightUpperArm", "Right arm"),
            new HumanRigBone("LeftLowerArm", "Left elbow"),
            new HumanRigBone("RightLowerArm", "Right elbow"),
            new HumanRigBone("LeftHand", "Left wrist"),
            new HumanRigBone("RightHand", "Right wrist"),
            new HumanRigBone("LeftToes", "Left toe"),
            new HumanRigBone("RightToes", "Right toe"),
            new HumanRigBone("LeftEye", "LeftEye"),
            new HumanRigBone("RightEye", "RightEye"),
            new HumanRigBone("Left Thumb Proximal", "Thumb0_L"),
            new HumanRigBone("Left Thumb Intermediate", "Thumb1_L"),
            new HumanRigBone("Left Thumb Distal", "Thumb2_L"),
            new HumanRigBone("Left Index Proximal", "IndexFinger1_L"),
            new HumanRigBone("Left Index Intermediate", "IndexFinger2_L"),
            new HumanRigBone("Left Index Distal", "IndexFinger3_L"),
            new HumanRigBone("Left Middle Proximal", "MiddleFinger1_L"),
            new HumanRigBone("Left Middle Intermediate", "MiddleFinger2_L"),
            new HumanRigBone("Left Middle Distal", "MiddleFinger3_L"),
            new HumanRigBone("Left Ring Proximal", "RingFinger1_L"),
            new HumanRigBone("Left Ring Intermediate", "RingFinger2_L"),
            new HumanRigBone("Left Ring Distal", "RingFinger3_L"),
            new HumanRigBone("Left Little Proximal", "LittleFinger1_L"),
            new HumanRigBone("Left Little Intermediate", "LittleFinger2_L"),
            new HumanRigBone("Left Little Distal", "LittleFinger3_L"),
            new HumanRigBone("Right Thumb Proximal", "Thumb0_R"),
            new HumanRigBone("Right Thumb Intermediate", "Thumb1_R"),
            new HumanRigBone("Right Thumb Distal", "Thumb2_R"),
            new HumanRigBone("Right Index Proximal", "IndexFinger1_R"),
            new HumanRigBone("Right Index Intermediate", "IndexFinger2_R"),
            new HumanRigBone("Right Index Distal", "IndexFinger3_R"),
            new HumanRigBone("Right Middle Proximal", "MiddleFinger1_R"),
            new HumanRigBone("Right Middle Intermediate", "MiddleFinger2_R"),
            new HumanRigBone("Right Middle Distal", "MiddleFinger3_R"),
            new HumanRigBone("Right Ring Proximal", "RingFinger1_R"),
            new HumanRigBone("Right Ring Intermediate", "RingFinger2_R"),
            new HumanRigBone("Right Ring Distal", "RingFinger3_R"),
            new HumanRigBone("Right Little Proximal", "LittleFinger1_R"),
            new HumanRigBone("Right Little Intermediate", "LittleFinger2_R"),
            new HumanRigBone("Right Little Distal", "LittleFinger3_R"),
        };

        public static HumanBone GetHumanBone(string humanBoneName, Transform transformToSearch)
        {
            HumanRigBone hrb = Bones.Find(o => o.humanBoneName.ToLower() == humanBoneName.ToLower());
            if(hrb)
            {
                for(int i = 0; i < hrb.boneNames.Length; i++)
                {
                    string s = hrb.boneNames[i];
                    Transform t = transformToSearch.FindDeepChild(s);
                    if(t)
                    {
                        HumanBone hb = new HumanBone()
                        {
                            boneName = t.name,
                            humanName = hrb.humanBoneName,
                            limit = hrb.humanLimit,
                        };
                        return hb;
                    }
                }
            }
            return default(HumanBone);
        }
    }

    public class HumanRigBone
    {
        public string humanBoneName;
        public string[] boneNames;
        public HumanLimit humanLimit = new HumanLimit()
        {
            useDefaultValues = true
        };

        public HumanRigBone(string humanBoneName, params string[] boneNames)
        {
            this.humanBoneName = humanBoneName;
            this.boneNames = boneNames;
        }

        public HumanRigBone(string humanBoneName, HumanLimit humanLimit, params string[] boneNames) : this(humanBoneName, boneNames)
        {
            this.humanLimit = humanLimit;
        }

        public static implicit operator bool(HumanRigBone hrb)
        {
            if(hrb == null)
                return false;
            return true;
        }
    }

    public class PumkinsMuscleDefinitions
    {
        public static readonly Vector2Int bodyRange = new Vector2Int(0, 8);
        public static readonly Vector2Int headRange = new Vector2Int(9, 20);

        public static readonly Vector2Int leftLegRange = new Vector2Int(21, 28);
        public static readonly Vector2Int rightLegRange = new Vector2Int(29, 36);
        public static readonly Vector2Int legsRange = new Vector2Int(leftLegRange.x, rightLegRange.y);

        public static readonly Vector2Int leftArmRange = new Vector2Int(37, 45);
        public static readonly Vector2Int rightArmRange = new Vector2Int(46, 54);
        public static readonly Vector2Int armsRange = new Vector2Int(leftArmRange.x, rightArmRange.y);

        public static readonly Vector2Int leftFingersRange = new Vector2Int(55, 74);
        public static readonly Vector2Int rightFingersRange = new Vector2Int(75, 94);
        public static readonly Vector2Int fingersRange = new Vector2Int(leftFingersRange.x, rightFingersRange.y);

        public static string[] Body
        {
            get { return HumanTrait.MuscleName.SubArray(bodyRange.x, bodyRange.y - bodyRange.x); }
        }
        public static string[] Head
        {
            get { return HumanTrait.MuscleName.SubArray(headRange.x, headRange.y - headRange.x); }
        }
        public static string[] LeftLeg
        {
            get { return HumanTrait.MuscleName.SubArray(leftLegRange.x, leftLegRange.y - leftLegRange.x); }
        }
        public static string[] RightLeg
        {
            get { return HumanTrait.MuscleName.SubArray(rightLegRange.x, rightLegRange.y - rightLegRange.x); }
        }
        public static string[] LeftArm
        {
            get { return HumanTrait.MuscleName.SubArray(leftArmRange.x, leftArmRange.y - leftArmRange.x); }
        }
        public static string[] RightArm
        {
            get { return HumanTrait.MuscleName.SubArray(rightArmRange.x, rightArmRange.y - rightArmRange.x); }
        }
        public static string[] LeftFingers
        {
            get { return HumanTrait.MuscleName.SubArray(leftFingersRange.x, leftFingersRange.y - leftFingersRange.x); }
        }
        public static string[] RightFingers
        {
            get { return HumanTrait.MuscleName.SubArray(rightFingersRange.x, rightFingersRange.y - rightFingersRange.x); }
        }
    }

    [Serializable]
    class ThryModuleRequirement
    {
        public string type = "VRC_SDK_VERSION";
        public string data = ">=0.0";
    }

    [Serializable]
    class ThryModuleManifest
    {
        public string name;
        public string description;
        public string classname;
        public double version;
        public ThryModuleRequirement requirement;
        public string[] files;

        ThryModuleManifest()
        {
            name = Strings.Main.title;
            description = "A set of tools to help you setup avatars easier.\nIncludes a component copier and tools to make thumbnails.\nTo launch the tools go to Tools > Pumkin > Avatar Tools.";
            classname = typeof(PumkinsAvatarTools).FullName.ToString();
            requirement = new ThryModuleRequirement();
            version = Strings.toolsVersion;

            var fileArray = Directory.GetFiles(PumkinsAvatarTools.MainFolderPath, "*", SearchOption.AllDirectories);
            var finalFileList = new List<string>();
            for(int i = 0; i < fileArray.Length; i++)
            {
                var file = fileArray[i].Substring(PumkinsAvatarTools.MainFolderPath.Length + 1);
                if(file.EndsWith(".meta") || file.EndsWith("thry_module_manifest.json") || file.StartsWith(".git"))
                    continue;

                file = file.Replace('\\', '/');
                finalFileList.Add(file);
            }
            files = finalFileList.ToArray();
        }

        static public void Generate()
        {
            ThryModuleManifest m = new ThryModuleManifest();
            string json = EditorJsonUtility.ToJson(m, true);
            string path = PumkinsAvatarTools.MainFolderPath + "/thry_module_manifest.json";
            File.WriteAllText(path, json);
        }
    }
}