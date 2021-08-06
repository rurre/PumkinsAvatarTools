using System;
using System.Linq;
using UnityEngine;

namespace Pumkin.AvatarTools.MaterialPreview
{
    internal static class MaterialManager
    {
        public static Texture2D MatCapTex
            => _matcapTexture = _matcapTexture ?? Resources.Load<Texture2D>("FallbackShaders/FallbackMatcap");

        static class Shaders
        {
            public static Shader SpriteDefault => _fbSpriteDefault = _fbSpriteDefault ?? Shader.Find("Sprites/Default");
            public static Shader Particle => _fbParticle = _fbParticle ?? Shader.Find("Particles/~Additive-Multiply");
            public static Shader ToonLit => _fbToon = _fbToon ?? Shader.Find("Fallback/Toon/Opaque");
            public static Shader ToonLitDouble => _fbToonDouble = _fbToonDouble ?? Shader.Find("Fallback/Toon/Opaque Double");
            public static Shader LegacyVertexLit => _fbVertexLit = _fbVertexLit ?? Shader.Find("Legacy Shaders/VertexLit");
            public static Shader ToonCutout => _fbToonCutout = _fbToonCutout ?? Shader.Find("Fallback/Toon/Cutout");
            public static Shader ToonCutoutDouble => _fbToonCutoutDouble = _fbToonCutoutDouble ?? Shader.Find("Fallback/Toon/Cutout Double");
            public static Shader UnlitColor => _fbUnlitColor = _fbUnlitColor ?? Shader.Find("Unlit/Color");
            public static Shader UnlitTransparentCutout => _fbUnlitTransparentCutout = _fbUnlitTransparentCutout ?? Shader.Find("Unlit/Transparent Cutout");
            public static Shader UnlitTexture => _fbUnlitTexture = _fbUnlitTexture ?? Shader.Find("Fallback/Unlit_Texture");
            public static Shader MatCapShader => _fbMatcap = _fbMatcap ?? Shader.Find("Fallback/Matcap");
            public static Shader Standard => _fbStandard = _fbStandard ?? Shader.Find("Standard");

            public static Shader UnlitTransparent => _fbUnlitTransparent = _fbUnlitTransparent ?? Shader.Find("Unlit/Transparent");

            static Shader _fbSpriteDefault;
            static Shader _fbMatcap;
            static Shader _fbStandard;
            static Shader _fbUnlitTexture;
            static Shader _fbUnlitTransparentCutout;
            static Shader _fbUnlitColor;
            static Shader _fbToonCutoutDouble;
            static Shader _fbToonCutout;
            static Shader _fbVertexLit;
            static Shader _fbToonDouble;
            static Shader _fbToon;
            static Shader _fbParticle;
            static Shader _fbUnlitTransparent;
        }
        
        private static readonly string[] ShaderWhitelist = 
        {
            "Standard",
            "Standard (Specular setup)",
            "Effects/Rim",
            "Effects/GlowAdditiveSimple",
            "Legacy Shaders/Bumped Diffuse",
            "Legacy Shaders/Bumped Specular",
            "Legacy Shaders/Decal",
            "Legacy Shaders/Diffuse",
            "Legacy Shaders/Diffuse Detail",
            "Legacy Shaders/Diffuse Fast",
            "Legacy Shaders/Lightmapped/Diffuse",
            "Legacy Shaders/Lightmapped/Specular",
            "Legacy Shaders/Lightmapped/VertexLit",
            "Legacy Shaders/Parallax Diffuse",
            "Legacy Shaders/Parallax Specular",
            "Legacy Shaders/Reflective/Bumped Diffuse",
            "Legacy Shaders/Reflective/Bumped Specular",
            "Legacy Shaders/Reflective/Bumped Unlit",
            "Legacy Shaders/Reflective/Bumped VertexLit",
            "Legacy Shaders/Reflective/Diffuse",
            "Legacy Shaders/Reflective/Parallax Diffuse",
            "Legacy Shaders/Reflective/Parallax Specular",
            "Legacy Shaders/Reflective/Specular",
            "Legacy Shaders/Reflective/VertexLit",
            "Legacy Shaders/Self-Illum/Bumped Diffuse",
            "Legacy Shaders/Self-Illum/Bumped Specular",
            "Legacy Shaders/Self-Illum/Diffuse",
            "Legacy Shaders/Self-Illum/Parallax Diffuse",
            "Legacy Shaders/Self-Illum/Parallax Specular",
            "Legacy Shaders/Self-Illum/Specular",
            "Legacy Shaders/Self-Illum/VertexLit",
            "Legacy Shaders/Specular",
            "Legacy Shaders/Transparent/Bumped Diffuse",
            "Legacy Shaders/Transparent/Bumped Specular",
            "Legacy Shaders/Transparent/Cutout/Bumped Diffuse",
            "Legacy Shaders/Transparent/Cutout/Bumped Specular",
            "Legacy Shaders/Transparent/Cutout/Diffuse",
            "Legacy Shaders/Transparent/Cutout/Soft Edge Unlit",
            "Legacy Shaders/Transparent/Cutout/Specular",
            "Legacy Shaders/Transparent/Cutout/VertexLit",
            "Legacy Shaders/Transparent/Diffuse",
            "Legacy Shaders/Transparent/Parallax Diffuse",
            "Legacy Shaders/Transparent/Parallax Specular",
            "Legacy Shaders/Transparent/Specular",
            "Legacy Shaders/Transparent/VertexLit",
            "Legacy Shaders/VertexLit",
            "MatCap/Vertex/Textured Lit",
            "Mobile/Bumped Diffuse",
            "Mobile/Bumped Specular",
            "Mobile/Bumped Specular (1 Directional Light)",
            "Mobile/Diffuse",
            "Mobile/Unlit (Supports Lightmap)",
            "Mobile/Particles/Additive",
            "Mobile/Particles/Alpha Blended",
            "Mobile/Particles/Multiply",
            "Mobile/Particles/VertexLit Blended",
            "Particles/~Additive-Multiply",
            "Particles/Additive",
            "Particles/Additive (Soft)",
            "Particles/Alpha Blended",
            "Particles/Alpha Blended Premultiply",
            "Particles/Anim Alpha Blended",
            "Particles/Multiply",
            "Particles/Multiply (Double)",
            "Particles/VertexLit Blended",
            "Sprites/Default",
            "Sprites/Diffuse",
            "Toon/Lit",
            "Toon/Lit (Double)",
            "Toon/Lit Cutout",
            "Toon/Lit Cutout (Double)",
            "Toon/Lit Outline",
            "UI/Default",
            "Unlit/FailShader",
            "VRChat/UI/Default"
        };


        static class MaterialProperties
        {
            public static readonly int SpecularColor = Shader.PropertyToID("_SpecColor");
            public static readonly int Emission = Shader.PropertyToID("_EmissionColor");
            public static readonly int Shininess = Shader.PropertyToID("_Shininess");
            public static readonly int Color = Shader.PropertyToID("_Color");
            public static readonly int MatCap = Shader.PropertyToID("_MatCap");
            public static readonly int Mode = Shader.PropertyToID("_Mode");
            public static readonly int OutlineWidth = Shader.PropertyToID("_outline_width");
            public static readonly int MainTex = Shader.PropertyToID("_MainTex");
        }

        public static Material CreateMatCapMaterial(Color baseColor)
        {
            Material material = new Material(Shaders.MatCapShader);

            if(material.HasProperty(MaterialProperties.MatCap) && MatCapTex != null)
                material.SetTexture(MaterialProperties.MatCap, MatCapTex);

            if(material.HasProperty(MaterialProperties.Color))
                material.SetColor(MaterialProperties.Color, baseColor);

            return material;
        }

        public static bool CopyMaterialProperty<T>(string prop, Material oldMat, Material newMat)
        {
            if(!oldMat.HasProperty(prop))
                return false;

            if(!newMat.HasProperty(prop))
                return false;

            if(typeof(T) == typeof(int))
                newMat.SetInt(prop, oldMat.GetInt(prop));

            if(typeof(T) == typeof(float))
                newMat.SetFloat(prop, oldMat.GetFloat(prop));
            else if(typeof(T) == typeof(Texture2D))
                newMat.SetTexture(prop, oldMat.GetTexture(prop));
            else
            {
                if(typeof(T) != typeof(Color))
                    return false;
                newMat.SetColor(prop, oldMat.GetColor(prop));
            }
            return true;
        }

        public static bool TransferMaterialProperty<T>(string oldProp, Material oldMat, string newProp, Material newMat)
        {
            if(!oldMat.HasProperty(oldProp))
                return false;
            if(!newMat.HasProperty(newProp))
                return false;

            if(typeof(T) == typeof(float))
                newMat.SetFloat(newProp, oldMat.GetFloat(oldProp));
            else if(typeof(T) == typeof(Texture2D))
                newMat.SetTexture(newProp, oldMat.GetTexture(oldProp));
            else
            {
                if(typeof(T) != typeof(Color))
                    return false;
                newMat.SetColor(newProp, oldMat.GetColor(oldProp));
            }
            return true;
        }

        static void SetupBlendMode(Material material)
        {
            if(material.HasProperty(MaterialProperties.Mode))
            {
                StandardShaderManager.BlendMode mode = (StandardShaderManager.BlendMode) material.GetInt(MaterialProperties.Mode); 
                StandardShaderManager.SetupMaterialWithBlendMode(material, mode);
            }
        }

        public static Material CreateFallbackMaterial(Material oldMat, Color rankColor)
        {
            if(oldMat == null || oldMat.shader == null)
                return CreateMatCapMaterial(rankColor);

            Material material = null;
            if(ShaderWhitelist.Contains(oldMat.shader.name))
            {
                if(oldMat.shader.name == "Standard")
                    material = CreateStandardFallback(oldMat);
                else
                    material = CreateWhitelistFallback(oldMat);
            }

            if(material != null)
                return material;
            return CreateUnknownFallbackMaterial(oldMat, rankColor);
        }

        private static Material CreateUnknownFallbackMaterial(Material oldMat, Color rankColor)
        {
            Material material;
            string shaderName = oldMat.shader.name;
            bool isUnlit = shaderName.Contains("Unlit");
            bool isVertexLit = shaderName.Contains("VertexLit");
            bool isToon = shaderName.Contains("Toon") || oldMat.HasProperty("_Ramp");
            bool isTransparent = shaderName.Contains("Transparent") || oldMat.IsKeywordEnabled("_ALPHABLEND_ON");
            bool isCutout = shaderName.Contains("Cutout") || oldMat.IsKeywordEnabled("_ALPHATEST_ON");
            bool isFade = shaderName.Contains("Fade");
            bool isParticle = shaderName.Contains("Particle");
            bool isSprite = shaderName.Contains("Sprite");
            bool isMatcap = shaderName.Contains("MatCap");
            bool hasOutline = oldMat.IsKeywordEnabled("TINTED_OUTLINE") && oldMat.HasProperty("_outline_width") &&
                              oldMat.GetFloat(MaterialProperties.OutlineWidth) == 0f;

            bool isMobileToonLitShader = oldMat.shader.name == "VRChat/Mobile/Toon Lit";

            if(isSprite)
                material = new Material(Shaders.SpriteDefault);
            else if(isParticle)
                material = new Material(Shaders.Particle);
            else if(isMatcap)
            {
                material = CreateMatCapMaterial(Color.white);
                CopyMaterialProperty<Texture2D>("_MatCap", oldMat, material);
            }
            else if(isMobileToonLitShader)
            {
                material = new Material(Shaders.ToonLit);
            }
            else if(isToon && !isTransparent && !isFade)
            {
                if(isVertexLit)
                {
                    material = new Material(Shaders.LegacyVertexLit);
                    if(oldMat.HasProperty(MaterialProperties.Color))
                    {
                        material.SetColor(MaterialProperties.SpecularColor, Color.black);
                        material.SetColor(MaterialProperties.Emission, oldMat.GetColor(MaterialProperties.Color) * 0.2f);
                        material.SetFloat(MaterialProperties.Shininess, 0f);
                    }
                }
                else if(isCutout)
                {
                    material = new Material(!hasOutline ? Shaders.ToonCutout : Shaders.ToonCutoutDouble);
                    CopyMaterialProperty<float>("_Cutoff", oldMat, material);
                }
                else
                    material = new Material(!hasOutline ? Shaders.ToonLit : Shaders.ToonLitDouble);
            }
            else if(isToon || isUnlit)
            {
                if(!oldMat.HasProperty("_MainTex"))
                    material = new Material(Shaders.UnlitColor);
                else if(isCutout)
                {
                    material = new Material(Shaders.UnlitTransparentCutout);
                    CopyMaterialProperty<float>("_Cutoff", oldMat, material);
                }
                else if(isTransparent || isFade)
                    material = new Material(Shaders.UnlitTransparent);
                else
                    material = new Material(Shaders.UnlitTexture);
            }
            else if(isVertexLit)
            {
                material = new Material(Shaders.LegacyVertexLit);
            }
            else
            {
                material = CreateStandardFallback(oldMat);
            }

            if(material.HasProperty(MaterialProperties.MainTex)
               && !CopyMaterialProperty<Texture2D>("_MainTex", oldMat, material)
               && !TransferMaterialProperty<Texture2D>("_Diffuse", oldMat, "_MainTex", material)
               && !TransferMaterialProperty<Texture2D>("_Texture", oldMat, "_MainTex", material)
               && material.GetTexture(MaterialProperties.MainTex) == null && !oldMat.HasProperty("_Color"))
            {
                material = CreateMatCapMaterial(rankColor);
            }

            CopyMaterialProperty<Color>("_Color", oldMat, material);
            material.name = oldMat.name;
            return material;
        }

        private static Material CreateWhitelistFallback(Material oldMat)
        {
            Material material;
            Shader shader = Shader.Find(oldMat.shader.name);
            if(shader == null)
                return null;

            material = new Material(shader);
            material.CopyPropertiesFromMaterial(oldMat);
            material.name = oldMat.name;
            return material;
        }

        private static Material CreateStandardFallback(Material oldMat)
        {
            Material material;
            material = new Material(Shaders.Standard);
            SetupBlendMode(material);
            CopyMaterialProperty<float>("_Cutoff", oldMat, material);
            CopyMaterialProperty<float>("_Glossiness", oldMat, material);
            CopyMaterialProperty<float>("_Metallic", oldMat, material);
            CopyMaterialProperty<Texture2D>("_BumpMap", oldMat, material);
            CopyMaterialProperty<Color>("_EmissionColor", oldMat, material);
            CopyMaterialProperty<Texture2D>("_EmissionMap", oldMat, material);
            material.name = oldMat.name;
            
            if(material.HasProperty(MaterialProperties.MainTex)
               && !CopyMaterialProperty<Texture2D>("_MainTex", oldMat, material)
               && !TransferMaterialProperty<Texture2D>("_Diffuse", oldMat, "_MainTex", material)
               && !TransferMaterialProperty<Texture2D>("_Texture", oldMat, "_MainTex", material))
            {
                // Good job
            }

            CopyMaterialProperty<Color>("_Color", oldMat, material);
            return material;
        }

        static Texture2D _matcapTexture;
    }

    internal static class StandardShaderManager
    {
        static ReflectionType DetermineWorkflow(Material mat)
        {
            if(mat.HasProperty("_SpecGlossMap") && mat.HasProperty("_SpecColor"))
                return ReflectionType.Specular;
            if(mat.HasProperty("_MetallicGlossMap") && mat.HasProperty("_Metallic"))
                return ReflectionType.Metallic;
            return ReflectionType.Dielectric;
        }

        public static void SetupMaterialWithBlendMode(Material material, BlendMode blendMode)
        {
            switch(blendMode)
            {
                case BlendMode.Opaque:
                    material.SetOverrideTag("RenderType", string.Empty);
                    material.SetInt("_SrcBlend", 1);
                    material.SetInt("_DstBlend", 0);
                    material.SetInt("_ZWrite", 1);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.renderQueue = -1;
                    break;
                case BlendMode.Cutout:
                    material.SetOverrideTag("RenderType", "TransparentCutout");
                    material.SetInt("_SrcBlend", 1);
                    material.SetInt("_DstBlend", 0);
                    material.SetInt("_ZWrite", 1);
                    material.EnableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.renderQueue = 2450;
                    break;
                case BlendMode.Fade:
                    material.SetOverrideTag("RenderType", "Transparent");
                    material.SetInt("_SrcBlend", 5);
                    material.SetInt("_DstBlend", 10);
                    material.SetInt("_ZWrite", 0);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.EnableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.renderQueue = 3000;
                    break;
                case BlendMode.Transparent:
                    material.SetOverrideTag("RenderType", "Transparent");
                    material.SetInt("_SrcBlend", 1);
                    material.SetInt("_DstBlend", 10);
                    material.SetInt("_ZWrite", 0);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.renderQueue = 3000;
                    break;
            }
        }

        static AlphaType GetSmoothnessMapChannel(Material material)
        {
            int smoothnessChannel = (int)material.GetFloat("_SmoothnessTextureChannel");
            if(smoothnessChannel == 1)
                return AlphaType.AlbedoAlpha;
            return AlphaType.SpecularMetallicAlpha;
        }

        static void SetMaterialKeywords(Material material, StandardShaderManager.ReflectionType workflowType)
        {
            Texture texture = null;
            if(material.HasProperty("_BumpMap"))
                texture = material.GetTexture("_BumpMap");
            else if(material.HasProperty("_DetailNormalMap"))
                texture = material.GetTexture("_DetailNormalMap");

            if(texture != null)
                SetKeyword(material, "_NORMALMAP", texture);

            if(workflowType == ReflectionType.Specular)
                SetKeyword(material, "_SPECGLOSSMAP", material.GetTexture("_SpecGlossMap"));
            else if(workflowType == ReflectionType.Metallic)
                SetKeyword(material, "_METALLICGLOSSMAP", material.GetTexture("_MetallicGlossMap"));

            if(material.HasProperty("_ParallaxMap"))
                SetKeyword(material, "_PARALLAXMAP", material.GetTexture("_ParallaxMap"));

            texture = null;
            if(material.HasProperty("_DetailAlbedoMap"))
                texture = material.GetTexture("_DetailAlbedoMap");
            else if(material.HasProperty("_DetailNormalMap"))
                texture = material.GetTexture("_DetailNormalMap");
            if(texture != null)
                SetKeyword(material, "_DETAIL_MULX2", texture);

            bool state = (material.globalIlluminationFlags & MaterialGlobalIlluminationFlags.EmissiveIsBlack) == MaterialGlobalIlluminationFlags.None;

            SetKeyword(material, "_EMISSION", state);
            if(material.HasProperty("_SmoothnessTextureChannel"))
            {
                SetKeyword(material, "_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A", GetSmoothnessMapChannel(material) == AlphaType.AlbedoAlpha);
            }
        }

        public static void MaterialChanged(Material material)
        {
            if(material.HasProperty("_Mode"))
                SetupMaterialWithBlendMode(material, (BlendMode)material.GetInt("_Mode"));
            SetMaterialKeywords(material, DetermineWorkflow(material));
        }

        static void SetKeyword(Material m, string keyword, bool state)
        {
            if(state)
                m.EnableKeyword(keyword);
            else
                m.DisableKeyword(keyword);

        }

        public enum ReflectionType
        {
            Specular,
            Metallic,
            Dielectric
        }

        public enum BlendMode
        {
            Opaque,
            Cutout,
            Fade,
            Transparent
        }

        public enum AlphaType
        {
            SpecularMetallicAlpha,
            AlbedoAlpha
        }
    }
}