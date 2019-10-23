using Newtonsoft.Json;
using Pumkin.AvatarTools;
using Pumkin.HelperFunctions;
using Pumkin.PoseEditor;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using VRC.Core;
using VRCSDK2;
using VRCSDK2.Validation.Performance;
using VRCSDK2.Validation.Performance.Stats;

namespace Pumkin.DataStructures
{
    public static class Colors
    {
        public static Color SceneGUIWindow { get; internal set; }
        public static Color DefaultCameraBackground { get; internal set; }
        public static Color DarkCameraBackground { get; internal set; }
        public static Color BallHandle { get; internal set; }
        public static Color LightLabelText { get; internal set; }

        static Colors()
        {
            SceneGUIWindow = new Color(0.3804f, 0.3804f, 0.3804f, 0.7f);
            DefaultCameraBackground = new Color(0.192f, 0.302f, 0.475f);
            DarkCameraBackground = new Color(0.235f, 0.22f, 0.22f);
            BallHandle = new Color(1, 0.92f, 0.016f, 0.5f);
            LightLabelText = Color.white;
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

        static Styles()
        {
            BigButton = new GUIStyle("Button")
            {
                fixedHeight = 28f,
                stretchHeight = false,
            };
            Foldout_title = new GUIStyle("Foldout")
            {
                fontStyle = FontStyle.Bold,
                fontSize = 12,
            };

            Label_mainTitle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontStyle = FontStyle.Bold,
                fontSize = 14,
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
                fixedHeight = 17,
                stretchHeight = false,
            };

            LightTextField = new GUIStyle("TextField");
            LightTextField.normal.textColor = Color.white;

            Foldout = new GUIStyle("Foldout");
            HelpBox = new GUIStyle("HelpBox");
            Box = new GUIStyle("box");
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

        public static Texture2D DiscordIcon { get; internal set; }
        public static Texture2D GithubIcon { get; internal set; }
        public static Texture2D KofiIcon { get; internal set; }

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

            BoneIcon = Resources.Load("icons/bone-icon") as Texture2D ?? CsScript;
            BoneColliderIcon = Resources.Load("icons/bonecollider-icon") as Texture2D ?? DefaultAsset;
            DiscordIcon = Resources.Load("icons/discord-logo") as Texture2D ?? Star;
            GithubIcon = Resources.Load("icons/github-logo") as Texture2D ?? Star;
            KofiIcon = Resources.Load("icons/kofi-logo") as Texture2D ?? Star;        
#endif
        }

    }

    /// <summary>
    /// Strings. Need to move these to files eventually
    /// </summary>
    public static class Strings
    {
        public static readonly string version = "0.7b - Work in Progress";
        public static readonly string toolsPage = "https://github.com/rurre/PumkinsAvatarTools/";
        public static readonly string donationLink = "https://ko-fi.com/notpumkin";
        public static readonly string discordLink = "https://discord.gg/7vyekJv";
        readonly static Dictionary<string, string> dictionary_english, dictionary_uwu;
        static Dictionary<string, string> stringDictionary;
        static DictionaryLanguage language;

        public enum DictionaryLanguage { English, uwu = 100 };

        static public DictionaryLanguage Language
        {
            get { return language; }
            set
            {
                if(value != language)
                {
                    switch(value)
                    {
                        case DictionaryLanguage.English:
                            stringDictionary = dictionary_english;
                            break;
                        case DictionaryLanguage.uwu:
                            stringDictionary = dictionary_uwu;
                            break;
                        default:
                            stringDictionary = dictionary_english;
                            break;
                    }
                    language = value;
                    ReloadStrings();
                }
            }
        }

        public static class AvatarInfo
        {
            public static string SelectAvatarFirst { get; internal set; }

            public static string Name { get; internal set; }
            public static string GameObjects { get; internal set; }
            public static string Bones { get; internal set; }
            public static string SkinnedMeshRenderers { get; internal set; }
            public static string MeshRenderers { get; internal set; }
            public static string Polygons { get; internal set; }
            public static string UsedMaterialSlots { get; internal set; }
            public static string UniqueMaterials { get; internal set; }
            public static string Shaders { get; internal set; }
            public static string DynamicBoneTransforms { get; internal set; }
            public static string DynamicBoneColliders { get; internal set; }
            public static string DynamicBoneColliderTransforms { get; internal set; }
            public static string ParticleSystems { get; internal set; }
            public static string MaxParticles { get; internal set; }
            public static string OverallPerformance { get; internal set; }
            public static string Line { get; internal set; }

            static AvatarInfo()
            {
                Reload();
            }

            public static void Reload()
            {
                Name = GetString("ui_avatarInfo_name") ?? "_{0}";
                Line = GetString("ui_avatarInfo_line") ?? "---------------------";
                GameObjects = GetString("ui_avatarInfo_gameobjects") ?? "_GameObjects: {0} ({1})";
                Bones = GetString("ui_avatarInfo_bones") ?? "_Bones: {0} - {1}";
                SkinnedMeshRenderers = GetString("ui_avatarInfo_skinnedMeshRenderers") ?? "_Skinned Mesh Renderers: {0} ({1}) - {2}";
                MeshRenderers = GetString("ui_avatarInfo_meshRenderers") ?? "_Mesh Renderers: {0} ({1}) - {2}";
                Polygons = GetString("ui_avatarInfo_polygons") ?? "_Polygons: {0} ({1}) - {2}";
                UsedMaterialSlots = GetString("ui_avatarInfo_usedMaterialSlots") ?? "_Used Material Slots: {0} ({1}) - {2}";
                UniqueMaterials = GetString("ui_avatarInfo_uniqueMaterials") ?? "_Unique Materials: {0} ({1})";
                Shaders = GetString("ui_avatarInfo_shaders") ?? "_Shaders: {0}";
                DynamicBoneTransforms = GetString("ui_avatarInfo_dynamicBoneTransforms") ?? "_Dynamic Bone Transforms: {0} ({1}) - {2}";
                DynamicBoneColliders = GetString("ui_avatarInfo_dynamicBoneColliders") ?? "_Dynamic Bone Colliders: {0} ({1}) - {2}";
                DynamicBoneColliderTransforms = GetString("ui_avatarInfo_dynamicBoneColliderTransforms") ?? "_Collider Affected Transforms: {0} ({1}) - {2}";
                ParticleSystems = GetString("ui_avatarInfo_particleSystems") ?? "_Particle Systems: {0} ({1}) - {2}";
                MaxParticles = GetString("ui_avatarInfo_maxParticles") ?? "_Max Particles: {0} ({1}) - {2}";
                OverallPerformance = GetString("ui_avatarInfo_overallPerformance") ?? "_Overall Performance: {0}";
                SelectAvatarFirst = GetString("ui_avatarInfo_selectAvatarFirst") ?? "_Select an Avatar first";
            }
        }

        public static class Main
        {
            public static string Title { get; internal set; }
            public static string WindowName { get; internal set; }
            public static string Version { get; internal set; }
            public static string Avatar { get; internal set; }
            public static string Tools { get; internal set; }
            public static string Copier { get; internal set; }
            public static string RemoveAll { get; internal set; }
            public static string AvatarInfo { get; internal set; }
            public static string Thumbnails { get; internal set; }

            public static string Misc { get; internal set; }
            public static string UseSceneSelection { get; internal set; }

            static Main()
            {
                Reload();
            }

            public static void Reload()
            {
                Avatar = GetString("ui_main_avatar") ?? "_Avatar";
                Title = GetString("ui_main_title") ?? "_Pumkin's Avatar Tools";
                Version = GetString("ui_main_version") ?? "_Version";
                WindowName = GetString("ui_main_windowName") ?? "_Pumkin Tools";
                Tools = GetString("ui_tools") ?? "_Tools";
                Copier = GetString("ui_copier") ?? "_Copy Components";
                AvatarInfo = GetString("ui_avatarInfo") ?? "_Avatar Info";
                RemoveAll = GetString("ui_removeAll") ?? "_Remove All";
                Misc = GetString("ui_misc") ?? "_Misc";
                Thumbnails = GetString("ui_thumbnails") ?? "_Thumbnails";
                UseSceneSelection = GetString("ui_useSceneSelection") ?? "_Use Scene Selection";
            }

        };
        public static class Buttons
        {
            public static string SelectFromScene { get; internal set; }
            public static string CopySelected { get; internal set; }
            public static string Cancel { get; internal set; }
            public static string Apply { get; internal set; }
            public static string Refresh { get; internal set; }
            public static string Copy { get; internal set; }
            public static string OpenHelpPage { get; internal set; }
            public static string OpenGithubPage { get; internal set; }
            public static string OpenDonationPage { get; internal set; }
            public static string OpenPoseEditor { get; internal set; }
            public static string JoinDiscordServer { get; internal set; }
            public static string SelectNone { get; internal set; }
            public static string SelectAll { get; internal set; }
            public static string Browse { get; internal set; }

            static Buttons()
            {
                Reload();
            }

            public static void Reload()
            {
                SelectFromScene = GetString("buttons_selectFromScene") ?? "_Select from Scene";
                CopySelected = GetString("buttons_copySelected") ?? "_Copy Selected";
                Refresh = GetString("buttons_refresh") ?? "_Refresh";
                Cancel = GetString("buttons_cancel") ?? "_Cancel";
                Apply = GetString("buttons_apply") ?? "_Apply";
                Copy = GetString("buttons_copyText") ?? "_Copy Text";
                OpenHelpPage = GetString("buttons_openHelpPage") ?? "_Open Help Page";
                OpenGithubPage = GetString("buttons_openGithubPage") ?? "_Open Github Page";
                OpenDonationPage = GetString("buttons_openDonationPage") ?? "_Buy me a Ko-Fi~";
                OpenPoseEditor = GetString("buttons_openPoseEditor") ?? "_Open Pose Editor";
                JoinDiscordServer = GetString("buttons_joinDiscordServer") ?? "_Join Discord Server!";
                SelectNone = GetString("buttons_selectNone") ?? "_Select None";
                SelectAll = GetString("buttons_selectAll") ?? "_Select All";
                Browse = GetString("buttons_browse") ?? "_Browse";
            }
        };
        public static class Tools
        {
            public static string FillVisemes { get; internal set; }
            public static string EditViewpoint { get; internal set; }
            public static string RevertBlendshapes { get; internal set; }
            public static string ZeroBlendshapes { get; internal set; }
            public static string ResetPose { get; internal set; }
            public static string ResetToTPose { get; internal set; }
            public static string EditScale { get; internal set; }

            static Tools()
            {
                Reload();
            }

            public static void Reload()
            {
                FillVisemes = GetString("ui_tools_fillVisemes") ?? "_Fill Visemes";
                EditViewpoint = GetString("ui_tools_editViewpoint") ?? "_Edit Viewpoint";
                RevertBlendshapes = GetString("ui_tools_revertBlendShapes") ?? "_Revert Blendshapes";
                ZeroBlendshapes = GetString("ui_tools_zeroBlendShapes") ?? "_Zero Blendshapes";
                ResetPose = GetString("ui_tools_resetPose") ?? "_Reset Pose";
                ResetToTPose = GetString("ui_tools_resetToTPose") ?? "_Reset to T-Pose";
                EditScale = GetString("ui_tools_editScale") ?? "_Edit Scale";
            }
        };
        public static class Thumbnails
        {
            public static string OverlayCameraImage { get; internal set; }
            public static string OverlayTexture { get; internal set; }
            public static string StartUploadingFirst { get; internal set; }
            public static string CenterCameraOnViewpoint { get; internal set; }
            public static string BackgroundType { get; internal set; }
            public static string BackgroundType_None { get; internal set; }
            public static string BackgroundType_Material { get; internal set; }
            public static string BackgroundType_Color { get; internal set; }
            public static string HideOtherAvatars { get; internal set; }
            public static string BackgroundType_Image { get; internal set; }

            static Thumbnails()
            {
                Reload();
            }

            public static void Reload()
            {
                OverlayCameraImage = GetString("ui_thumbnails_overlayCameraImage") ?? "_Overlay Image";
                OverlayTexture = GetString("ui_thumbnails_overlayTexture") ?? "_Overlay Texture";
                StartUploadingFirst = GetString("ui_thumbnails_startUploadingFirst") ?? "_Start uploading an Avatar first";
                CenterCameraOnViewpoint = GetString("ui_thumbnails_centerCameraOnViewpoint") ?? "_Center Camera on Viewpoint";

                BackgroundType = GetString("ui_thumbnails_backgroundType") ?? "_Background Type";

                BackgroundType_None = GetString("ui_thumbnails_backgroundType_none") ?? "_None";
                BackgroundType_Material = GetString("ui_thumbnails_backgroundType_material") ?? "_Material";
                BackgroundType_Color = GetString("ui_thumbnails_backgroundType_color") ?? "_Color";
                BackgroundType_Image = GetString("ui_thumbnails_backgroundType_image") ?? "_Image";

                HideOtherAvatars = GetString("ui_thumbnails_hideOtherAvatars") ?? "_Hide Other Avatars when Uploading";
            }
        }
        public static class Copier
        {
            public static string CopyFrom { get; internal set; }
            public static string CopyGameObjects { get; internal set; }
            public static string CopyColliderObjects { get; internal set; }
            public static string CopySettings { get; internal set; }
            public static string CreateMissing { get; internal set; }

            public static string Transforms { get; internal set; }
            public static string Transforms_position { get; internal set; }
            public static string Transforms_rotation { get; internal set; }
            public static string Transforms_scale { get; internal set; }
            public static string Transforms_avatarScale { get; internal set; }
            public static string DynamicBones { get; internal set; }
            public static string DynamicBones_colliders { get; internal set; }
            public static string DynamicBones_removeOldBones { get; internal set; }
            public static string DynamicBones_removeOldColliders { get; internal set; }
            public static string DynamicBones_createMissing { get; internal set; }
            public static string Colliders { get; internal set; }
            public static string Colliders_box { get; internal set; }
            public static string Colliders_capsule { get; internal set; }
            public static string Colliders_sphere { get; internal set; }
            public static string Colliders_mesh { get; internal set; }
            public static string Colliders_removeOld { get; internal set; }
            public static string Descriptor { get; internal set; }
            public static string Descriptor_pipelineId { get; internal set; }
            public static string Descriptor_animationOverrides { get; internal set; }
            public static string SkinMeshRender { get; internal set; }
            public static string SkinMeshRender_materials { get; internal set; }
            public static string SkinMeshRender_blendShapeValues { get; internal set; }
            public static string ParticleSystems { get; internal set; }
            public static string RigidBodies { get; internal set; }
            public static string TrailRenderers { get; internal set; }
            public static string EmptyGameObjects { get; internal set; }
            public static string MeshRenderers { get; internal set; }
            public static string Lights { get; internal set; }
            public static string Animators { get; internal set; }
            public static string CopyMainAnimator { get; internal set; }
            public static string ReplaceOld { get; internal set; }
            public static string Animators_inChildren { get; internal set; }
            public static string AudioSources { get; internal set; }
            public static string Joints { get; internal set; }
            public static string Exclusions { get; internal set; }
            public static string IncludeChildren { get; private set; }
            public static string Size { get; private set; }
            public static string Descriptor_copyViewpoint { get; internal set; }

            static Copier()
            {
                Reload();
            }

            public static void Reload()
            {
                CopyFrom = GetString("ui_copier_copyFrom") ?? "_Copy From";

                CopySettings = GetString("ui_copier_copySettings") ?? "_Settings";
                CreateMissing = GetString("ui_copier_createMissing") ?? "_Create Missing";
                EmptyGameObjects = GetString("ui_copier_emptyGameObjects") ?? "_Empty GameObjects";
                ReplaceOld = GetString("ui_copier_replaceOld") ?? "_Replace Old";

                Transforms = GetString("ui_copier_transforms") ?? "_Transforms";
                Transforms_position = GetString("ui_copier_transforms_position") ?? "_Position";
                Transforms_rotation = GetString("ui_copier_transforms_rotation") ?? "_Rotation";
                Transforms_scale = GetString("ui_copier_transforms_scale") ?? "_Scale";
                Transforms_avatarScale = GetString("ui_copier_transforms_avatarScale") ?? "_Avatar Scale";
                DynamicBones = GetString("ui_copier_dynamicBones") ?? "_Dynamic Bones";
                DynamicBones_colliders = GetString("ui_copier_dynamicBones_colliders") ?? "_Colliders";
                DynamicBones_removeOldBones = GetString("ui_copier_dynamicBones_removeOld") ?? "_Remove Old Bones";
                DynamicBones_removeOldColliders = GetString("ui_copier_dynamicBones_removeOldColliders") ?? "_Remove Old Colliders";
                DynamicBones_createMissing = GetString("ui_copier_dynamicBones_createMissing") ?? "_Create Missing Bones";
                Colliders = GetString("ui_copier_colliders") ?? "_Colliders";
                Colliders_box = GetString("ui_copier_colliders_box") ?? "_Box Colliders";
                Colliders_capsule = GetString("ui_copier_colliders_capsule") ?? "_Capsule Colliders";
                Colliders_sphere = GetString("ui_copier_colliders_sphere") ?? "_Sphere Colliders";
                Colliders_mesh = GetString("ui_copier_colliders_mesh") ?? "_Mesh Colliders";
                Colliders_removeOld = GetString("ui_copier_colliders_removeOld") ?? "_Remove Old Colliders";
                Descriptor = GetString("ui_copier_descriptor") ?? "_Avatar Descriptor";
                Descriptor_pipelineId = GetString("ui_copier_descriptor_pipelineId") ?? "_Pipeline Id";
                Descriptor_animationOverrides = GetString("ui_copier_descriptor_animationOverrides") ?? "_Animation Overrides";
                Descriptor_copyViewpoint = GetString("ui_copier_descriptor_copyViewpoint") ?? "_Viewpoint";
                SkinMeshRender = GetString("ui_copier_skinMeshRender") ?? "_Skinned Mesh Renderers";
                SkinMeshRender_materials = GetString("ui_copier_skinMeshRender_materials") ?? "_Materials";
                SkinMeshRender_blendShapeValues = GetString("ui_copier_skinMeshRender_blendShapeValues") ?? "_BlendShape Values";
                ParticleSystems = GetString("ui_copier_particleSystem") ?? "_Particle Systems";
                RigidBodies = GetString("ui_copier_rigidBodies") ?? "_Rigid Bodies";
                TrailRenderers = GetString("ui_copier_trailRenderers") ?? "_Trail Renderers";
                MeshRenderers = GetString("ui_copier_meshRenderers") ?? "_Mesh Renderers";
                CopyGameObjects = GetString("ui_copier_copyGameObjects") ?? "_Copy GameObjects";
                CopyColliderObjects = GetString("ui_copier_dynamicBones_copyColliderObjects") ?? "_Copy Collider Objects";
                Lights = GetString("ui_copier_lights") ?? "_Lights";
                Animators = GetString("ui_copier_animators") ?? "_Animators";
                CopyMainAnimator = GetString("ui_copier_animators_copyMain") ?? "_Copy Main Animator";
                Animators_inChildren = GetString("ui_copier_animatorsInChildren") ?? "_Child Animators";
                AudioSources = GetString("ui_copier_audioSources") ?? "_Audio Sources";
                Joints = GetString("ui_copier_joints") ?? "_Joints";
                Exclusions = GetString("ui_copier_exclusions") ?? "_Exclusions";
                IncludeChildren = GetString("ui_copier_includeChildren") ?? "_Include Children";
                Size = GetString("ui_copier_size") ?? "_Size";
            }
        };
        public static class Log
        {
            public static string CopyAttempt { get; internal set; }
            public static string RemoveAttempt { get; internal set; }
            public static string CopyFromInvalid { get; internal set; }
            public static string Done { get; internal set; }
            public static string Failed { get; internal set; }
            public static string CantCopyToSelf { get; internal set; }
            public static string ViewpointApplied { get; internal set; }
            public static string ViewpointCancelled { get; internal set; }
            public static string Cancelled { get; internal set; }
            public static string NoSkinnedMeshFound { get; internal set; }
            public static string DescriptorIsNull { get; internal set; }
            public static string Success { get; internal set; }
            public static string TryFillVisemes { get; internal set; }
            public static string TryRemoveUnsupportedComponent { get; internal set; }
            public static string MeshHasNoVisemes { get; internal set; }
            public static string FailedIsNull { get; internal set; }
            public static string NameIsEmpty { get; internal set; }
            public static string LoadedPose { get; internal set; }
            public static string LoadedBlendshapePreset { get; internal set; }
            public static string NothingSelected { get; internal set; }
            public static string FailedDoesntHave { get; internal set; }
            public static string FailedAlreadyHas { get; internal set; }
            public static string LoadedCameraOverlay { get; internal set; }
            public static string FailedHasNo { get; internal set; }

            static Log()
            {
                Reload();
            }

            public static void Reload()
            {
                Done = GetString("log_done") ?? "_Done";
                Cancelled = GetString("log_cancelled") ?? "_Cancelled";
                NothingSelected = GetString("log_nothingSelected") ?? "_Select something first";
                CantCopyToSelf = GetString("log_cantCopyToSelf") ?? "_Can't copy Components from an object to itself. What are you doing?";
                CopyAttempt = GetString("log_copyAttempt") ?? "_Attempting to copy {0} from {1} to {2}";
                RemoveAttempt = GetString("log_removeAttempt") ?? "_Attempting to remove {0} from {1}";
                CopyFromInvalid = GetString("log_copyFromInvalid") ?? "_Can't copy Components because 'Copy From' is invalid";
                ViewpointApplied = GetString("log_viewpointApplied") ?? "_Set Viewposition to {0}";
                ViewpointCancelled = GetString("log_viewpointCancelled") ?? "_Cancelled Viewposition changes";
                TryFillVisemes = GetString("log_tryFillVisemes") ?? "_Attempting to fill visemes on {0}";
                NoSkinnedMeshFound = GetString("log_noSkinnedMeshFound") ?? "_Failed: No skinned mesh found";
                DescriptorIsNull = GetString("log_descriptorIsNull") ?? "_Avatar descriptor is null";
                Success = GetString("log_success") ?? "_Success";
                MeshHasNoVisemes = GetString("log_meshHasNoVisemes") ?? "_Failed. Mesh has no Visemes. Set to Default";
                TryRemoveUnsupportedComponent = GetString("log_tryRemoveUnsupportedComponent") ?? "_Attempting to remove unsupported component {0} from {1}";
                Failed = GetString("log_failed") ?? "_Failed";
                FailedIsNull = GetString("log_failedIsNull") ?? "_Failed: {1} is null";
                NameIsEmpty = GetString("log_nameIsEmpty") ?? "_Name is Empty";
                LoadedPose = GetString("log_loadedPose") ?? "_Loaded Pose: {0}";
                LoadedBlendshapePreset = GetString("log_loadedBlendshapePreset") ?? "_Loaded Blendshapes: {0}";
                FailedDoesntHave = GetString("log_failedDoesntHave") ?? "_Failed: {0} doesn't have a {1}";
                FailedAlreadyHas = GetString("log_failedAlreadyHas") ?? "_Failed: {1} already has {0}";
                LoadedCameraOverlay = GetString("log_loadedCameraOverlay") ?? "_Loaded {0} as Camera Overlay";
                FailedHasNo = GetString("log_failedHasNo") ?? "_{0} has no {1}, Ignoring.";
            }
        };
        public static class Warning
        {
            public static string Warn { get; internal set; }
            public static string NotFound { get; internal set; }
            public static string SelectSceneObject { get; internal set; }
            public static string OldVersion { get; internal set; }
            public static string VRCCamNotFound { get; internal set; }

            static Warning()
            {
                Reload();
            }

            public static void Reload()
            {
                Warn = GetString("warn_warning") ?? "_Warning";
                NotFound = GetString("warn_notFound") ?? "_(Not Found)";
                OldVersion = GetString("warn_oldVersion") ?? "_(Old Version)";
                SelectSceneObject = GetString("warn_selectSceneObject") ?? "_Please select an object from the scene";
                VRCCamNotFound = GetString("warn_vrcCamNotFound") ?? "_VRCCam not found";
            }
        };
        public static class Credits
        {
            public static string Title { get; internal set; }
            public static string Version { get; internal set; }
            public static string RedundantStrings { get; internal set; }
            public static string JsonDotNetCredit { get; internal set; }
            public static string AddMoreStuff { get; internal set; }
            public static string PokeOnDiscord { get; internal set; }

            static Credits()
            {
                Reload();
            }

            public static void Reload()
            {
                Title = GetString("credits_title") ?? "_Pumkin's Avatar Tools";
                Version = GetString("credits_version") ?? "_Version" + " " + version;
                RedundantStrings = GetString("credits_redundantStrings") ?? "_Now with 100% more redundant strings";
                JsonDotNetCredit = GetString("credits_jsonDotNetCredit") ?? "_JsonDotNet by Newtonsoft";
                AddMoreStuff = GetString("credits_addMoreStuff") ?? "_I'll add more stuff to this eventually";
                PokeOnDiscord = GetString("credits_pokeOnDiscord") ?? "_Poke me on Discord at Pumkin#2020";
            }
        };
        public static class Misc
        {
            public static string uwu { get; internal set; }
            public static string SearchForBones { get; internal set; }
            public static string SuperExperimental { get; internal set; }

            private static string searchForBones;

            static Misc()
            {
                Reload();
            }

            public static void Reload()
            {
                uwu = GetString("misc_uwu") ?? "_uwu";
                SearchForBones = GetString("misc_searchForBones") ?? "_Search for DynamicBones";
                SuperExperimental = GetString("misc_superExperimental") ?? "_Super Experimental Stuff:";
            }
        }

        public static class PoseEditor
        {
            public static string Title { get; internal set; }

            public static string Scene { get; internal set; }
            public static string SceneLoadAdditive { get; internal set; }
            public static string SceneOverrideLights { get; internal set; }

            public static string AvatarPosition { get; internal set; }
            public static string AvatarPositionOverridePose { get; internal set; }
            public static string AvatarPositionOverrideBlendshapes { get; internal set; }

            public static string SceneSaveChanges { get; internal set; }
            public static string UnloadScene { get; internal set; }
            public static string ResetPosition { get; internal set; }

            public static string Pose { get; internal set; }
            public static string NewPose { get; internal set; }
            public static string OnlySavePoseChanges { get; internal set; }
            public static string LoadPose { get; internal set; }

            public static string Blendshapes { get; internal set; }
            public static string NewPreset { get; internal set; }
            public static string LoadPreset { get; internal set; }

            public static string SaveButton { get; internal set; }
            public static string ReloadButton { get; internal set; }

            public static string BodyPositionYTooSmall { get; internal set; }

            static PoseEditor()
            {
                Reload();
            }

            public static void Reload()
            {
                Title = GetString("ui_poseEditor") ?? "_Pose Editor (Very Beta)";
                Scene = GetString("ui_poseEditor_scene") ?? "_Scene";
                SceneLoadAdditive = GetString("ui_poseEditor_scene_loadAdditive") ?? "_Load Additive";
                SceneOverrideLights = GetString("ui_poseEditor_scene_overrideLights") ?? "_Override Lights";
                AvatarPosition = GetString("ui_poseEditor_avatarPosition") ?? "_Avatar Position";
                AvatarPositionOverridePose = GetString("ui_poseEditor_avatarPosition_overridePose") ?? "_Override Pose";
                AvatarPositionOverrideBlendshapes = GetString("ui_poseEditor_avatarPositionOverrideBlendshapes") ?? "_Override Blendshapes";
                SceneSaveChanges = GetString("ui_poseEditor_scene_saveChanges") ?? "_Save Scene Changes";
                UnloadScene = GetString("ui_poseEditor_scene_unload") ?? "_Unload Scene";
                ResetPosition = GetString("ui_poseEditor_resetPosition") ?? "_Reset Position";
                Pose = GetString("ui_poseEditor_pose") ?? "_Pose";
                NewPose = GetString("ui_poseEditor_newPose") ?? "_New Pose";
                OnlySavePoseChanges = GetString("ui_poseEditor_onlySavePoseChanges") ?? "_Only Save Pose Changes";
                LoadPose = GetString("ui_poseEditor_loadPose") ?? "_Load Pose";
                Blendshapes = GetString("ui_poseEditor_blendshapes") ?? "_Blendshapes";
                NewPreset = GetString("ui_poseEditor_newPreset") ?? "_New Preset";
                LoadPreset = GetString("ui_poseEditor_loadPreset") ?? "_Load Preset";
                SaveButton = GetString("ui_poseEditor_save") ?? "_Save";
                ReloadButton = GetString("ui_poseEditor_reload") ?? "_Reload";
                BodyPositionYTooSmall = GetString("warn_poseEditor_bodyPositionYTooSmall") ?? "_humanPose.bodyPosition.y is {0}, you probably don't want that. Setting humanPose.bodyPosition.y to 1";
            }
        }

        static Strings()
        {
            //Language Dictionaries
            dictionary_english = new Dictionary<string, string>
            {
#region Main
                //Main
                {"ui_main_title", "Pumkin's Avatar Tools" },
                {"ui_main_windowName", "Pumkin Tools" },
                {"ui_main_version", "Version" },
                {"ui_main_avatar", "Avatar" },
                {"ui_tools", "Tools" },
                {"ui_copier", "Copy Components" },
                {"ui_avatarInfo", "Avatar Info" },
                {"ui_thumbnails", "Thumbnails" },
                {"ui_misc", "Misc" },
                {"ui_removeAll", "Remove All" },
                {"ui_useSceneSelection", "Use Scene Selection" },

#region AvatarInfo
                { "ui_avatarInfo_name", "Name: {0}"},
                { "ui_avatarInfo_line", "---------------------"},
                { "ui_avatarInfo_gameobjects", "GameObjects: {0} ({1})"},
                { "ui_avatarInfo_bones", "Bones: {0} - {1}"},
                { "ui_avatarInfo_skinnedMeshRenderers", "Skinned Mesh Renderers: {0} ({1}) - {2}"},
                { "ui_avatarInfo_meshRenderers", "Mesh Renderers: {0} ({1}) - {2}"},
                { "ui_avatarInfo_polygons", "Polygons: {0} ({1}) - {2}"},
                { "ui_avatarInfo_usedMaterialSlots", "Used Material Slots: {0} ({1}) - {2}"},
                { "ui_avatarInfo_uniqueMaterials", "Unique Materials: {0} ({1})"},
                { "ui_avatarInfo_shaders", "Shaders: {0}"},
                { "ui_avatarInfo_dynamicBoneTransforms", "Dynamic Bone Transforms: {0} ({1}) - {2}"},
                { "ui_avatarInfo_dynamicBoneColliders", "Dynamic Bone Colliders: {0} ({1}) - {2}"},
                { "ui_avatarInfo_dynamicBoneColliderTransforms", "Collider Affected Transforms: {0} ({1}) - {2}"},
                { "ui_avatarInfo_particleSystems", "Particle Systems: {0} ({1}) - {2}"},
                { "ui_avatarInfo_maxParticles", "Max Particles: {0} ({1}) - {2}"},
                { "ui_avatarInfo_overallPerformance", "Overall Performance: {0}"},
                { "ui_avatarInfo_selectAvatarFirst", "Select an Avatar first"},                
#endregion

#region Buttons
                {"buttons_selectFromScene", "Select from Scene" },
                {"buttons_copySelected" , "Copy Selected" },
                {"buttons_refresh", "Refresh" },
                {"buttons_apply", "Apply" },
                {"buttons_cancel", "Cancel" },
                {"buttons_copyText", "Copy Text" },
                {"buttons_openHelpPage", "Open Help Page" },
                {"buttons_openGithubPage", "Open Github Page" },
                {"buttons_openDonationPage", "Buy me a Ko-Fi~" },
                {"buttons_openPoseEditor", "Open Pose Editor" },
                {"buttons_joinDiscordServer", "Join Discord Server!" },
                {"buttons_selectAll", "Select All" },
                {"buttons_selectNone", "Select None" },
                {"buttons_browse", "Browse" },
#endregion

#endregion
#region Tools
            //UI Tools                
                {"ui_tools_fillVisemes", "Fill Visemes" },
                {"ui_tools_editViewpoint", "Edit Viewpoint" },
                {"ui_tools_revertBlendShapes", "Revert Blendshapes" },
                {"ui_tools_zeroBlendShapes", "Zero Blendshapes" },
                {"ui_tools_resetPose", "Reset Pose" },
                {"ui_tools_resetToTPose", "Reset to T-Pose" },
                {"ui_tools_editScale", "Scale Avatar" },

#endregion
#region Copier
                //UI Copier
                {"ui_copier_copyFrom", "Copy from" },
                {"ui_copier_exclusions", "Exclusions"},
                {"ui_copier_includeChildren", "Include Children"},
                {"ui_copier_size", "Size"},
                //UI Copier Misc
                {"ui_copier_copyGameObjects", "Copy GameObjects" },
                {"ui_copier_emptyGameObjects", "Empty GameObjects" },
                {"ui_copier_copySettings", "Settings" },
                {"ui_copier_createMissing", "Create Missing" },
                {"ui_copier_replaceOld", "Replace Old" },

                //UI Copier Transforms
                {"ui_copier_transforms", "Transforms" },
                {"ui_copier_transforms_position", "Position" },
                {"ui_copier_transforms_rotation", "Rotation" },
                {"ui_copier_transforms_scale", "Scale" },
                {"ui_copier_transforms_avatarScale", "Avatar Scale" },
            
                //UI Copier Dynamic Bones
                {"ui_copier_dynamicBones", "Dynamic Bones" },
                {"ui_copier_dynamicBones_colliders", "Dynamic Bone Colliders" },
                {"ui_copier_dynamicBones_removeOld", "Remove Old Bones" },
                {"ui_copier_dynamicBones_removeOldColliders", "Remove Old Colliders" },
                {"ui_copier_dynamicBones_createMissing", "Create Missing Bones" },
                {"ui_copier_dynamicBones_copyColliderObjects" , "Copy Collider Objects" },


                //UI Copier Colliders
                {"ui_copier_colliders", "Colliders" },
                {"ui_copier_colliders_box", "Box Colliders" },
                {"ui_copier_colliders_capsule", "Capsule Colliders" },
                {"ui_copier_colliders_sphere", "Sphere Colliders" },
                {"ui_copier_colliders_mesh", "Mesh Colliders" },
                {"ui_copier_colliders_removeOld", "Remove Old Colliders" },                

                //UI Copier Avatar Descriptor
                {"ui_copier_descriptor", "Avatar Descriptor" },
                {"ui_copier_descriptor_pipelineId", "Pipeline Id" },
                {"ui_copier_descriptor_animationOverrides", "Animation Overrides" },
                {"ui_copier_descriptor_copyViewpoint", "Viewpoint" },

                //UI Copier Skinned Mesh Renderer
                {"ui_copier_skinMeshRender", "Skinned Mesh Renderers" },
                {"ui_copier_skinMeshRender_materials", "Materials" },
                {"ui_copier_skinMeshRender_blendShapeValues", "BlendShape Values" },

                //UI Copier Particle System
                {"ui_copier_particleSystem", "Particle Systems" },

                //UI Copier Rigid Bodies
                {"ui_copier_rigidBodies", "Rigid Bodies" },

                //UI Copier Trail Renderers
                {"ui_copier_trailRenderers", "Trail Renderers" },

                //UI Copier MeshRenderers
                {"ui_copier_meshRenderers",  "Mesh Renderers"},

                //UI Copier Lights
                {"ui_copier_lights",  "Lights"},
                
                //UI Copier Animators
                {"ui_copier_animators",  "Animators"},
                {"ui_copier_animators_copyMain", "Copy Main Animator" },
                {"ui_copier_animatorsInChildren", "Child Animators" },

                //UI Copier Audio Sources
                {"ui_copier_audioSources", "Audio Sources" },

                //UI Copier Joints
                {"ui_copier_joints", "Joints"},

#endregion

#region Thumbnails
                //Thumbnails                
                {"ui_thumbnails_overlayCameraImage", "Overlay Image" },
                {"ui_thumbnails_overlayTexture",  "Overlay Texture"},
                {"ui_thumbnails_startUploadingFirst", "Begin uploading an Avatar first" },
                {"ui_thumbnails_backgroundColor", "Background Color" },
                {"ui_thumbnails_centerCameraOnViewpoint", "Center Camera on Viewpoint" },

                {"ui_thumbnails_backgroundType", "Background Type" },
                {"ui_thumbnails_backgroundType_none", "None" },
                {"ui_thumbnails_backgroundType_material", "Material" },
                {"ui_thumbnails_backgroundType_color", "Color" },
                {"ui_thumbnails_backgroundType_image", "Image" },
                {"ui_thumbnails_hideOtherAvatars" , "Hide Other Avatars when Uploading"},
#endregion
#region PoseEditor
                //Pose Editor
                {"ui_poseEditor", "Pose Editor (Very Beta)"},
                {"ui_poseEditor_scene", "Scene"},
                {"ui_poseEditor_scene_loadAdditive", "Load Additive"},
                {"ui_poseEditor_scene_overrideLights", "Override Lights"},
                {"ui_poseEditor_avatarPosition", "Avatar Position"},
                {"ui_poseEditor_avatarPosition_overridePose", "Override Pose"},
                {"ui_poseEditor_scene_saveChanges", "Save Scene Changes"},
                {"ui_poseEditor_scene_unload", "Unload Scene"},
                {"ui_poseEditor_resetPosition", "Reset Position"},
                {"ui_poseEditor_pose", "Pose"},
                {"ui_poseEditor_newPose", "New Pose"},
                {"ui_poseEditor_onlySavePoseChanges", "Only Save Pose Changes"},
                {"ui_poseEditor_loadPose", "Load Pose"},
                {"ui_poseEditor_blendshapes", "Blendshapes"},
                {"ui_poseEditor_newPreset", "New Preset"},
                {"ui_poseEditor_loadPreset", "Load Preset"},
                {"ui_poseEditor_save", "Save"},
                {"ui_poseEditor_reload", "Reload"},
#endregion

#region Log
                //Log
                {"log_failed", "Failed" },
                {"log_cancelled", "Cancelled" },
                {"log_success", "Success" },
                {"log_nothingSelected" , "Select something first" },
                {"log_done", "Done. Check Unity Console for full Output Log" },
                {"log_copyAttempt", "Attempting to copy {0} from {1} to {2}" },
                {"log_removeAttempt", "Attempting to remove {0} from {1}" },
                {"log_copyFromInvalid", "Can't copy Components because 'Copy From' is invalid" },
                {"log_cantCopyToSelf", "Can't copy Components from an object to itself. What are you doing?" },
                {"log_viewpointApplied", "Set Viewposition to {0}" },
                {"log_viewpointCancelled", "Cancelled Viewposition changes" },
                {"log_tryFillVisemes", "Attempting to fill visemes on {0}" },
                {"log_noSkinnedMeshFound", "Failed: No skinned mesh found" },
                {"log_descriptorIsNull", "Avatar descriptor is null"},
                {"log_meshHasNoVisemes", "Failed. Mesh has no Visemes. Set to Default" },
                {"log_failedIsNull" , "Failed. {1} is null. Ignoring" },
                {"log_nameIsEmpty", "Name is empty" },
                {"log_loadedPose", "Loaded Pose: {0}"},
                {"log_loadedBlendshapePreset", "Loaded Blendshapes: {0}"},
                {"log_failedDoesntHave", "Failed: {0} doesn't have a {1}" },
                {"log_failedAlreadyHas", "Failed: {1} already has {0}" },
                {"log_loadedCameraOverlay", "Loaded {0} as Camera Overlay" },
                {"log_failedHasNo", "{0} has no {1}, Ignoring."},
#endregion

#region Warnings
                //Warnings
                { "log_warning", "Warning" },
                { "warn_selectSceneObject" , "Please select an object from the scene" },
                { "warn_notFound", "(Not Found)" },
                { "warn_oldVersion", "(Old Version)" },
                { "warn_poseEditor_bodyPositionYTooSmall", "humanPose.bodyPosition.y is {0}, you probably don't want that. Setting humanPose.bodyPosition.y to 1" },
                { "warn_vrcCamNotFound" , "VRCCam not found" },
#endregion

#region Credits
                //Credits
                { "credits_title", "Pumkin's Avatar Tools"},
                { "credits_version", "Version" + " " + version },
                { "credits_redundantStrings", "Now with 100% more redundant strings"},
                { "credits_jsonDotNetCredit", "JsonDotNet by Newtonsoft"},
                { "credits_addMoreStuff", "I'll add more stuff to this eventually" },
                { "credits_pokeOnDiscord", "Poke me on Discord at Pumkin#2020" },
#endregion

                //Misc                
                { "misc_uwu", "uwu" },
                { "misc_searchForBones", "Search for DynamicBones" },
                { "misc_superExperimental", "Super Experimental stuff" },
            };

            //Mistakes
            dictionary_uwu = new Dictionary<string, string>
            {
#region Main
                //Main
                {"ui_main_title", "Pumkin's Avataw Awoos! ÒwÓ" },
                {"ui_main_windowName", "Pumkin Awoos" },
                {"ui_main_version", "Vewsion~" },
                {"ui_main_avatar", "Avataw :o" },
                {"ui_thumbnails", "Thumbnyaiws >:3" },
                {"ui_tools", "Toows òwó" },
                {"ui_copier", "Copy Componyents uwu" },
                {"ui_avatarInfo", "Avataw Info 0w0" },
                {"ui_misc", "Misc ;o" },
                {"ui_removeAll", "Wemuv Aww (⁰д⁰ )" },

#region AvatarInfo
                { "ui_avatarInfo_name", "Nyame: {0}"},
                { "ui_avatarInfo_line", "---------------------"},
                { "ui_avatarInfo_gameobjects", "GamyeObwects: {0} ({1})"},
                { "ui_avatarInfo_bones", "Bonyes: {0} - {1}"},
                { "ui_avatarInfo_skinnedMeshRenderers", "Skinnyed Mesh Wendewews: {0} ({1}) - {2}"},
                { "ui_avatarInfo_meshRenderers", "Mesh Wendewews: {0} ({1}) - {2}"},
                { "ui_avatarInfo_polygons", "Powygons: {0} ({1}) - {2}"},
                { "ui_avatarInfo_usedMaterialSlots", "Used Matewiaw Swots: {0} ({1}) - {2}"},
                { "ui_avatarInfo_uniqueMaterials", "Unyique Matewiaws: {0} ({1})"},
                { "ui_avatarInfo_shaders", "Shadews: {0}"},
                { "ui_avatarInfo_dynamicBoneTransforms", "Dynyamic Bonye Twansfowms: {0} ({1}) - {2}"},
                { "ui_avatarInfo_dynamicBoneColliders", "Dynyamic Bonye Cowwidews: {0} ({1}) - {2}"},
                { "ui_avatarInfo_dynamicBoneColliderTransforms", "Cowwidew Affected Twansfowms: {0} ({1}) - {2}"},
                { "ui_avatarInfo_particleSystems", "Pawticwe Systems: {0} ({1}) - {2}"},
                { "ui_avatarInfo_maxParticles", "Max Pawticwes: {0} ({1}) - {2}"},
                { "ui_avatarInfo_overallPerformance", "Ovewaww Pewfowmance: {0}"},
                { "ui_avatarInfo_selectAvatarFirst", "Sewect an Avataw furst ewe" },
#endregion

#region Buttons
                {"buttons_selectFromScene", "Sewect fwom Scenye x3" },
                {"buttons_copySelected" , "Copy Sewected (´• ω •`)" },
                {"buttons_refresh", "Wefwesh (ﾟωﾟ;)" },
                {"buttons_apply", "Appwy （>﹏<）" },
                {"buttons_cancel", "Cancew ; o;" },
                {"buttons_copyText", "Cowpy OwO" },
                {"buttons_openHelpPage", "Opyen Hewp Paws uwu" },
                {"buttons_openGithubPage", "Opyen Gitwub Paws :o" },
                {"buttons_openDonationPage", "Buy Pumkin a Ko-Fi~ OwO" },
                {"buttons_openPoseEditor", "Open Paws Editow" },
#endregion

#endregion
#region Tools
                //UI Toows                
                {"ui_tools_fillVisemes", "Fiww Visemes ;~;" },
                {"ui_tools_editViewpoint", "Edit Viewpoint o-o" },
                {"ui_tools_revertBlendShapes", "Weset Bwendshapyes uwu" },
                {"ui_tools_zeroBlendShapes", "Zewo Bwendshapyes 0~0" },
                {"ui_tools_resetPose", "Weset Pose ;3" },
                {"ui_tools_resetToTPose", "Weset to T-Pose" },

#endregion
#region Copier
                //UI Copier
                {"ui_copier_copyFrom", "Copy fwom~" },                

                //UI Copier Transforms
                {"ui_copier_transforms", "Twansfowms!" },
                {"ui_copier_transforms_position", "Position~" },
                {"ui_copier_transforms_wotation", "Wotation @~@" },
                {"ui_copier_transforms_scawe", "Scawe www" },
                {"ui_copier_transforms_avatarScale", "Avatar Scale o-o" },

                //UI Copier Dynamic Bones
                {"ui_copier_dynamicBones", "Dynyamic Bonyes~" },
                {"ui_copier_dynamicBones_settings", "Settings º ^º" },
                {"ui_copier_dynamicBones_colliders", "Dynyamic Bonye Cowwidews~" },
                {"ui_copier_dynamicBones_removeOld", "Wemuv Owd Bonyes uwu" },
                {"ui_copier_dynamicBones_removeOldColliders", "Wemuv Owd Cowwidews ;w;" },
                {"ui_copier_dynamicBones_createMissing", "Cweate Missing Bonyes!" },

                //UI Copier Colliders
                {"ui_copier_colliders", "Cowwidews ;o;" },
                {"ui_copier_colliders_box", "Box Cowwidews!" },
                {"ui_copier_colliders_capsule", "Capsuwe Cowwidews o-o" },
                {"ui_copier_colliders_sphere", "Sphewe Cowwidews O~O" },
                {"ui_copier_colliders_mesh", "Mesh Cowwidews zzz" },
                {"ui_copier_colliders_removeOld", "Wemuv Owd Cowwidews uwu" },

                //UI Copier Avatar Descriptor
                {"ui_copier_descriptor", "Avataw Descwiptow~" },
                {"ui_copier_descriptor_settings", "Settings agen" },
                {"ui_copier_descriptor_pipelineId", "Pipewinye Id!" },
                {"ui_copier_descriptor_animationOverrides", "Anyimation Ovewwides :o" },

                //UI Copier Skinned Mesh Renderer
                {"ui_copier_skinMeshRender", "Skinnyed Mesh Wendewews ;w;" },
                {"ui_copier_skinMeshRender_settings", "Settings ageeen" },
                {"ui_copier_skinMeshRender_materials", "Matewiaws uwu" },
                {"ui_copier_skinMeshRender_blendShapeValues", "BwendShape Vawues ùwú" },

                //UI Copier Particle System
                {"ui_copier_particleSystem", "Pawtickle Systums zzz" },

                //UI Copier Rigid Bodies
                {"ui_copier_rigidBodies", "Wigid Bodyes" },                

                //UI Copier Trail Renderers
                {"ui_copier_trailRenderers", "Twail Wendewuws" },

                //UI Copier MeshRenderers
                {"ui_copier_meshRenderers",  "Mesh Wenderur"},

                //UI Copier Lights
                {"ui_copier_lights",  "Wighties"},
                
                //UI Copier Animators
                {"ui_copier_animators",  "Anyanmaytows"},
                {"ui_copier_animators_copyMain", "Copy main Anyanmaytow" },
                {"ui_copier_animatorsInChildren", "Smol Anyanmaytows" },

                //UI Copier Audio Sources
                { "ui_copier_audioSources", "Awwdio Sauces" },

                //UI Copier Joints
                {"ui_copier_joints", "Joints"},

#endregion

#region Thumbnails
                //Thumbnails                
                { "ui_thumbnails_overlayCameraImage", "Ovewwide Camewa Image" },
                { "ui_thumbnails_overlayTexture",  "Ovewwide Textuwe"},
                { "ui_thumbnails_startUploadingFirst", "Stawt upwoading Avataw fiwst!!" },
                { "ui_thumbnails_backgroundColor", "Background Color" },
                { "ui_thumbnails_centerCameraOnViewpoint", "Center Camera on Viewpoint" },
#endregion
#region PoseEditor
                //Pose Editor
                {"ui_poseEditor", "Pose Editow (Vewy Beta)"},
                {"ui_poseEditor_scene", "Scenye"},
                {"ui_poseEditor_scene_loadAdditive", "Woad Addititive"},
                {"ui_poseEditor_scene_overrideLights", "Ovewwide Wights"},
                {"ui_poseEditor_avatarPosition", "Avataw Position"},
                {"ui_poseEditor_avatarPosition_overridePose", "Ovewwide Pose"},
                {"ui_poseEditor_scene_saveChanges", "Save Scenye Changes"},
                {"ui_poseEditor_scene_unload", "Unwoad Scenye"},
                {"ui_poseEditor_resetPosition", "Weset Position"},
                {"ui_poseEditor_pose", "Paws"},
                {"ui_poseEditor_newPose", "Nyew Paws"},
                {"ui_poseEditor_onlySavePoseChanges", "Onwy Save Paws Changes"},
                {"ui_poseEditor_loadPose", "Woad Paws"},
                {"ui_poseEditor_blendshapes", "Bwendshapes"},
                {"ui_poseEditor_newPreset", "Nyew Pweset"},
                {"ui_poseEditor_loadPreset", "Woad Pweset"},
                {"ui_poseEditor_save", "Save"},
                {"ui_poseEditor_reload", "Wewoad"},
#endregion

#region Log
                //Log
                {"log_failed", "Faiwed ùwú" },
                {"log_cancelled", "Cancewwed .-." },
                {"log_success", "Success OWO" },
                {"log_nothingSelected" , "Sewect sumstuf furst uwu" },
                {"log_done", "Donye. Check Unyity Consowe fow fuww Output Wog uwus" },
                {"log_copyAttempt", "Attempting to copy {0} fwom {1} to {2} o-o" },
                {"log_remuveAttempt", "Attempting to wemuv {0} fwom {1} ;-;" },
                {"log_copyFromInvalid", "Can't copy Componyents because 'Copy Fwom' is invawid ; o ;" },
                {"log_cantCopyToSelf", "Can't copy Componyents fwom an object to itsewf. What awe you doing? ;     w     ;" },
                {"log_viewpointApplied", "Set Viewposition to {0}!" },
                {"log_viewpointCancelled", "Cancewwed Viewposition changes uwu" },
                {"log_tryFixVisemes", "Attempting to fiww visemes on {0}!" },
                {"log_noSkinnedMeshFound", "Faiwed: Nyo skinnyed mesh found ;o;" },
                {"log_descriptorIsNull", "Avataw descwiptow is nyuww humpf"},
                {"log_meshHasNoVisemes", "Faiwed. Mesh has nyo Visemes. Set to Defauwt ;w;" },
                {"log_failedIsNull" , "Faiwed {1} is nyull /w\\. Ignyowing uwu" },
                {"log_nameIsEmpty", "Nyame ish emptyyy ;w;" },
                {"log_loadedPose", "Woaded Paws: {0}"},
                {"log_loadedBlendshapePreset", "Woaded Bwendshapyes: {0}"},
                {"log_failedDoesntHave", "Faiwed: {0} dun have a {1} ;o;" },
                {"log_failedAlreadyHas", "Faiwed: {0} alredy has {1} ;w;" },
                {"log_loadedCameraOverlay", "Loaded {0} as Camera Overlay" },
                {"log_failedHasNo", "{0} has no {1}, Ignoring."},
#endregion

#region Warnings
                //Warnings
                { "log_warning", "Wawnying! unu" },
                { "warn_selectSceneObject" , "Pwease sewect an object fwom the scenye!!" },
                { "warn_notFound", "(Nyot Fownd ;~;)" },
                { "warn_oldVersion", "(Old Version)" },
                { "warn_poseEditor_bodyPositionYTooSmall", "humanPose.bodyPosition.y is {0}, you pwobabwy don't want that. Setting humanPose.bodyPosition.y to 1 uwu" },
#endregion

#region Credits
                //Credits
                { "credits_title", "Pumkin's Avataw Awoos~ :3"},
                { "credits_version", "Vewsion" + " " + version },
                { "credits_redundantStrings", "Nyow with 0W0% mowe noticin things~"},
                { "credits_jsonDotNetCredit", "JsonDotNet by Newtonsoft" },
                { "credits_addMoreStuff", "I'ww add mowe stuff to this eventuawwy >w<" },
                { "credits_pokeOnDiscord", "Poke me! But on Discowd at Pumkin#2020~ uwus" },
#endregion

                //Misc                
                { "misc_uwu", "OwO" },
                { "misc_searchForBones", "Seawch fow DynyamicBonyes" },
                { "misc_superExperimental", "Supew Scawy stuffs ヾ(´囗｀｡)ﾉ" },
            };

            stringDictionary = dictionary_english;
            language = DictionaryLanguage.English;
            ReloadStrings();
        }

        static void ReloadStrings()
        {
            Main.Reload();
            Buttons.Reload();
            Tools.Reload();
            Copier.Reload();
            Log.Reload();
            Warning.Reload();
            Credits.Reload();
            Misc.Reload();
            Thumbnails.Reload();
        }

        static string GetString(string stringName)
        {
            if(string.IsNullOrEmpty(stringName))
                return stringName;

            string s = string.Empty;
            stringDictionary.TryGetValue(stringName, out s);

            return s;
        }
    };

    public class AvatarInfo
    {
        AvatarPerformanceStats perfStats = new AvatarPerformanceStats();

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

        AvatarInfo()
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

        }

        public AvatarInfo(GameObject o) : base()
        {
            if(o == null)
                return;

            AvatarPerformance.CalculatePerformanceStats(o.name, o, perfStats);

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
            foreach(var r in sRenders)
            {
                SkinnedMeshRenderers_Total += 1;
                Polygons_Total += r.sharedMesh.triangles.Length;
                //Triangles_Total += r.sharedMesh.triangles.Length / 3;

                if(r.gameObject.activeInHierarchy && r.enabled)
                {
                    SkinnedMeshRenderers += 1;
                    Polygons += r.sharedMesh.triangles.Length;
                    //Triangles += r.sharedMesh.triangles.Length/3;
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

            var renders = o.GetComponentsInChildren<MeshRenderer>(true);
            foreach(var r in renders)
            {
                var filter = r.GetComponent<MeshFilter>();

                if(filter != null && filter.sharedMesh != null)
                {
                    MeshRenderers_Total += 1;
                    Polygons_Total += filter.sharedMesh.triangles.Length;

                    if(r.gameObject.activeInHierarchy && r.enabled)
                    {
                        MeshRenderers += 1;
                        Polygons += filter.sharedMesh.triangles.Length;
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

#if BONES || OLD_BONES

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
                    CachedInfo =
                    string.Format(Strings.AvatarInfo.Name, Name) + "\n" +
                    string.Format(Strings.AvatarInfo.Line) + "\n" +
                    string.Format(Strings.AvatarInfo.GameObjects, GameObjects, GameObjects_Total) + "\n" +
                    string.Format(Strings.AvatarInfo.Bones, perfStats.boneCount, perfStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.BoneCount)) + "\n\n" +
                    string.Format(Strings.AvatarInfo.SkinnedMeshRenderers, SkinnedMeshRenderers, SkinnedMeshRenderers_Total, perfStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.SkinnedMeshCount)) + "\n" +
                    string.Format(Strings.AvatarInfo.MeshRenderers, MeshRenderers, MeshRenderers_Total, perfStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.MeshCount)) + "\n" +
                    string.Format(Strings.AvatarInfo.Polygons, Polygons, Polygons_Total, perfStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.PolyCount)) + "\n\n" +
                    string.Format(Strings.AvatarInfo.UsedMaterialSlots, MaterialSlots, MaterialSlots_Total, perfStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.MaterialCount)) + "\n" +
                    string.Format(Strings.AvatarInfo.UniqueMaterials, UniqueMaterials, UniqueMaterials_Total) + "\n" +
                    string.Format(Strings.AvatarInfo.Shaders, ShaderCount) + "\n\n" +
                    string.Format(Strings.AvatarInfo.DynamicBoneTransforms, DynamicBoneTransforms, DynamicBoneTransforms_Total, perfStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.DynamicBoneSimulatedBoneCount)) + "\n" +
                    string.Format(Strings.AvatarInfo.DynamicBoneColliders, DynamicBoneColliders, DynamicBoneColliders_Total, perfStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.DynamicBoneColliderCount)) + "\n" +
                    string.Format(Strings.AvatarInfo.DynamicBoneColliderTransforms, DynamicBoneColliderTransforms, DynamicBoneColliderTransforms_Total, perfStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.DynamicBoneCollisionCheckCount)) + "\n\n" +
                    string.Format(Strings.AvatarInfo.ParticleSystems, ParticleSystems, ParticleSystems_Total, perfStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.ParticleSystemCount)) + "\n" +
                    string.Format(Strings.AvatarInfo.MaxParticles, MaxParticles, MaxParticles_Total, perfStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.ParticleTotalCount)) + "\n" +
                    Strings.AvatarInfo.Line + "\n" +
                    string.Format(Strings.AvatarInfo.OverallPerformance, perfStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.Overall));
                }
                catch(Exception)
                {
                    CachedInfo = null;
                }
                return CachedInfo;
            }
        }

        public static bool operator true(AvatarInfo x) { return x != null; }
        public static bool operator false(AvatarInfo x) { return !(x == null); }
    }

    [Serializable]
    public class CameraPreset
    {
        string presetName;
        SerialVector3 positionOffset;
        SerialQuaternion rotationOffset;
        SerialVector3 anglesOffset;

        string transformPath;

        Color backgroundColor;
        string overlayPath;
        Material skyboxMaterial;

        public CameraPreset(string name, GameObject focusObject, Camera cam)
        {
            Transform tFocus = focusObject.transform;
            Transform tCam = cam.transform;

            Transform tDummy = new GameObject("_FocusDummy").transform;

            tDummy.SetPositionAndRotation(tFocus.position, tFocus.rotation);



        }

        public CameraPreset(string name, Vector3 posOffset, Quaternion rotOffset, Vector3 angleOffset, string transPath, Color bgColor, string overlayPath = null)
        {
            this.positionOffset = posOffset;
            this.rotationOffset = rotOffset;
            this.anglesOffset = angleOffset;
            this.backgroundColor = bgColor;
            this.overlayPath = overlayPath;
            this.presetName = name;
        }

        void ApplyPreset(Camera camera)
        {
            GameObject avatar = null;
            VRC_AvatarDescriptor desc = null;
            Camera cam = PumkinsAvatarTools.VRCCam;

            if(EditorApplication.isPlaying)
            {
                var pm = GameObject.FindObjectOfType<RuntimeBlueprintCreation>();
                if(pm == null)
                {
                    Debug.Log("_RuntimeBlueprintCreation script not found. Start uploading an avatar to use this");
                    return;
                }
                else if(pm.pipelineManager.contentType == PipelineManager.ContentType.world)
                {
                    Debug.Log("_You must be uploading an avatar, not a world to use this.");
                    return;
                }
                desc = pm.pipelineManager.GetComponent<VRC_AvatarDescriptor>();
                avatar = desc.transform.root.gameObject;
            }
            else if((avatar = PumkinsAvatarTools.selectedAvatar) == null)
            {
                Debug.Log("_Begin uploading an avatar or select one manually before using this.");
                return;
            }

            Transform focusObj = avatar.transform.Find(transformPath);
            if(focusObj == null)
            {
                PumkinsAvatarTools.Log("_GameObject {0} not found in {1}. Ignoring", LogType.Warning, Helpers.GetNameFromPath(transformPath), avatar.name);
                return;
            }

            cam.transform.parent = focusObj;
            cam.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

            cam.transform.position = positionOffset;
            cam.transform.rotation = rotationOffset;
            cam.transform.eulerAngles = anglesOffset;

            cam.transform.parent = null;
        }

        bool SavePreset(Camera cam, string path)
        {
            return true;
        }
    }

    /// <summary>
    /// Serializable Transform class
    /// </summary>
    [Serializable]
    public class SerialTransform
    {
        public SerialVector3 position, localPosition, scale, localScale, eulerAngles, localEulerAngles;
        public SerialQuaternion rotation, localRotation;

        SerialTransform()
        {
            this.position = Vector3.zero;
            this.rotation = Quaternion.identity;
            this.scale = Vector3.one;
            this.localEulerAngles = Vector3.zero;
            this.localPosition = Vector3.zero;
            this.localRotation = Quaternion.identity;
            this.localScale = Vector3.one;
        }

        [JsonConstructor]
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

            if(localEulerAngles != null)
                localEulerAngles = t.localEulerAngles;
        }

        public static implicit operator SerialTransform(Transform t)
        {
            return new SerialTransform(t);
        }
    }

    /// <summary>
    /// Serializable Quaternion class
    /// </summary>
    [Serializable]
    public class SerialQuaternion
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
    public class SerialVector3
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
    }

    /// <summary>
    /// This might be much better than the transform-based PosePreset
    /// </summary>
    [Serializable]
    public class HumanPosePreset
    {
        public string poseName;
        public Vector3 bodyPosition;
        public Quaternion bodyRotation;
        public Vector3 rootPosition;
        public float[] muscles;

        public HumanPosePreset(string poseName, Vector3 rootPosition, HumanPose p)
        {
            this.poseName = poseName;
            bodyPosition = p.bodyPosition;
            bodyRotation = p.bodyRotation;
            muscles = p.muscles;
            this.rootPosition = rootPosition;
        }

        public HumanPosePreset(string poseName, Vector3 bodyPosition, Quaternion bodyRotation, Vector3 rootPosition, float[] muscles)
        {
            this.poseName = poseName;
            this.rootPosition = rootPosition;
            this.bodyPosition = bodyPosition;
            this.bodyRotation = bodyRotation;
            this.muscles = muscles;
        }

        internal bool ApplyPose(GameObject avatar)
        {
            Animator anim = avatar.GetComponent<Animator>();
            if(anim == null || !anim.isHuman)
                return false;

            HumanPoseHandler hph = new HumanPoseHandler(anim.avatar, avatar.transform);
            HumanPose hp = this;
            hph.SetHumanPose(ref hp);

            return true;
        }

        public static implicit operator HumanPose(HumanPosePreset v)
        {
            HumanPose hp = new HumanPose();
            hp.bodyPosition = v.bodyPosition;
            hp.bodyRotation = v.bodyRotation;
            hp.muscles = v.muscles;

            return hp;
        }
    }

    /// <summary>
    /// Serializable Pose preset, used to store avatar transforms associated to a name
    /// </summary>
    [Serializable]
    public class PosePreset
    {
        public string poseName;
        public Dictionary<string, SerialTransform> transforms;

        public PosePreset(string poseName, Dictionary<string, SerialTransform> transformSettings = null)
        {
            this.poseName = poseName;
            this.transforms = transformSettings;
        }

        /// <summary>
        /// Adds or overwrites a transform to the pose dictionary.
        /// </summary>
        /// <param name="path">Path of the transform</param>
        /// <param name="transform">Transform for positoin and rotation settings</param>
        /// <param name="allowOverwrite">Whether to overwrite settings if transform exists or not</param>
        /// <returns>Returns true if successfully added or overwritten</returns>
        public bool AddTransform(string path, Transform transform, bool allowOverwrite = true)
        {
            if(transforms.ContainsKey(path))
            {
                if(allowOverwrite)
                    transforms[path] = transform;
                else
                    return false;
            }
            else
            {
                transforms.Add(path, transform);
            }
            return true;
        }

        /// <summary>
        /// Serialize and save pose to file
        /// </summary>
        /// <param name="filePath">Path to folder where to save the file to, excluding filename</param>
        /// <param name="overwriteExisting">Overwrite file if one with the same name already exists, if not will add a number to the end and create new file</param>
        /// <returns></returns>
        public bool SaveToFile(string filePath, bool overwriteExisting)
        {
            if(transforms == null)
                transforms = new Dictionary<string, SerialTransform>();

            string path = filePath + "/" + poseName + '.' + PumkinsPoseEditor.poseExtension;

            if(!overwriteExisting)
                path = Helpers.NextAvailableFilename(filePath + "/" + poseName + '.' + PumkinsPoseEditor.poseExtension);

            string json = JsonConvert.SerializeObject(this, Formatting.Indented);

            if(!string.IsNullOrEmpty(json))
            {
                File.WriteAllText(path, json);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Don't hurt me, I need this for a default failsafe or two.
        /// </summary>
        /// <returns>C# code to declare and initialize this object</returns>
        public string ToHardcodedString()
        {
            if(transforms == null)
                transforms = new Dictionary<string, SerialTransform>();

            string s = string.Format("new PosePreset(\"{0}\", new Dictionary<string, SerialTransform>\n\t", poseName);
            s += "{\n";
            foreach(var t in transforms)
            {
                s += "\t\t{";
                s += string.Format("\"{0}\", new SerialTransform(new Quaternion({1}f, {2}f, {3}f, {4}f))", t.Key, t.Value.localRotation.x, t.Value.localRotation.y, t.Value.localRotation.z, t.Value.localRotation.w);
                s += "},\n";
            }
            s += "}),\n";

            return s;
        }

        /// <summary>
        /// Apply this pose to avatar
        /// </summary>        
        public void ApplyPose(GameObject avatar)//, bool childrenFirst = false)
        {
            if(!avatar)
                return;

            foreach(var kv in transforms)
            {
                var t = avatar.transform.Find(kv.Key);

                if(t != null)
                {
                    t.localEulerAngles = kv.Value.localEulerAngles;
                    t.localRotation = kv.Value.localRotation;
                }
            }
            //}
        }
    }

    [Serializable]
    public class BlendshapePreset
    {
        public string presetName;
        public Dictionary<string, List<PoseBlendshape>> blendshapes;

        public BlendshapePreset(string presetName, Dictionary<string, List<PoseBlendshape>> blendshapes = null)
        {
            this.presetName = presetName;
            this.blendshapes = blendshapes;

            if(blendshapes == null)
                blendshapes = new Dictionary<string, List<PoseBlendshape>>();
        }

        public bool AddBlendshape(string meshRendererPath, PoseBlendshape blend)
        {
            var d = blendshapes[meshRendererPath];

            if(d != null && d.Count > 0 && !d.Exists(x => x.Name.ToLower() == blend.Name.ToLower()))
            {
                d.Add(blend);
                return true;
            }
            return false;
        }

        public bool RemoveBlendshape(string meshRendererPath, string shapeName)
        {
            var d = blendshapes[meshRendererPath];
            var b = d.Find(x => x.Name.ToLower() == shapeName.ToLower());

            if(d != null && d.Count > 0 && b != null)
            {
                d.Remove(b);
                return true;
            }
            return false;
        }

        public bool SaveToFile(string filePath, bool overwriteExisting)
        {
            string path = filePath + "/" + presetName + '.' + PumkinsPoseEditor.blendshapeExtension;

            if(!overwriteExisting)
                path = Helpers.NextAvailableFilename(filePath + "/" + presetName + '.' + PumkinsPoseEditor.blendshapeExtension);

            string json = JsonConvert.SerializeObject(this, Formatting.Indented);

            if(!string.IsNullOrEmpty(json))
            {
                File.WriteAllText(path, json);
                return true;
            }
            return false;
        }

        public void ApplyBlendshapes(GameObject avatar)
        {
            if(!avatar)
                return;

            foreach(var b in blendshapes)
            {
                var t = avatar.transform.Find(b.Key);
                if(t)
                {
                    var sr = t.GetComponent<SkinnedMeshRenderer>();
                    if(sr)
                    {
                        foreach(var shape in b.Value)
                        {
                            int index = sr.sharedMesh.GetBlendShapeIndex(shape.Name);

                            if(shape.AlternateNames.Count > 0)
                            {
                                for(int i = 0; index == -1 && i < shape.AlternateNames.Count; i++)
                                {
                                    index = sr.sharedMesh.GetBlendShapeIndex(shape.AlternateNames[i]);
                                }
                            }

                            if(index != -1)
                            {
                                sr.SetBlendShapeWeight(index, shape.Weight);
                            }
                        }
                    }
                }
            }
        }
    }

    [Serializable]
    public class PoseBlendshape
    {
        public string FriendlyName
        {
            get
            {
                return friendlyName;
            }
            set
            {
                if(string.IsNullOrEmpty(value))
                    friendlyName = Name;
            }
        }

        public string Name
        {
            get; set;
        }

        public float Weight
        {
            get; set;
        }

        public List<string> AlternateNames
        {
            get; set;
        }

        string friendlyName;

        public PoseBlendshape(string name, float weight = 0, string friendlyName = null, List<string> alternateNames = null)
        {
            Name = name;
            Weight = weight;

            FriendlyName = friendlyName;
            if(alternateNames != null)
                AlternateNames = alternateNames;
            else
                AlternateNames = new List<string>();
        }
    }
}
