using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Pumkin.HelperFunctions;
using UnityEditor;
using UnityEngine;
using Pumkin.DataStructures;

namespace Pumkin.TexturePacker
{
    static class PumkinsTexturePackerHelpers
    {
        internal static Vector2Int DrawResolutionPicker(Vector2Int size, ref bool linked, ref bool autoDetect, int[] presets = null, string[] presetNames = null)
        {
            EditorGUI.BeginDisabledGroup(autoDetect);
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.PrefixLabel("Size");

                EditorGUI.BeginChangeCheck();
                size.x = EditorGUILayout.IntField(size.x);
                if(linked && EditorGUI.EndChangeCheck())
                    size.y = size.x;

                EditorGUILayout.LabelField("x", GUILayout.MaxWidth(12));

                EditorGUI.BeginChangeCheck();
                size.y = EditorGUILayout.IntField(size.y);
                if(linked && EditorGUI.EndChangeCheck())
                    size.x = size.y;

                if(presets != null && presetNames != null)
                {
                    EditorGUI.BeginChangeCheck();
                    int selectedPresetIndex = EditorGUILayout.Popup(GUIContent.none, -1, presetNames, GUILayout.MaxWidth(16));
                    if(EditorGUI.EndChangeCheck() && selectedPresetIndex != -1)
                        size = new Vector2Int(presets[selectedPresetIndex], presets[selectedPresetIndex]);
                }

                linked = GUILayout.Toggle(linked, Icons.LinkIcon, EditorStyles.miniButton, GUILayout.MaxWidth(22), GUILayout.MaxHeight(16));
            }
            EditorGUILayout.EndHorizontal();

            EditorGUI.EndDisabledGroup();

            autoDetect = EditorGUILayout.Toggle("Auto detect", autoDetect);

            return size;
        }

        /// <summary>
        /// Gets the combined maximum width and height of the passed in textures
        /// </summary>
        /// <param name="textures"></param>
        /// <returns></returns>
        internal static Vector2Int GetMaxSizeFromTextures(params Texture2D[] textures)
        {
            var sizes = textures.Where(tex => tex).Select(tex => new Vector2Int(tex.width, tex.height)).ToArray();
            if(sizes.Length == 0)
                return default;

            int maxW = sizes.Max(wh => wh.x);
            int maxH = sizes.Max(wh => wh.y);
            return new Vector2Int(maxW, maxH);
        }

        internal static Texture2D PackTextures(Vector2Int resolution, Texture2D red, Texture2D green, Texture2D blue, Texture2D alpha, bool invertRed, bool invertGreen, bool invertBlue, bool invertAlpha)
        {
            // Setup Material
            var mat = new Material(TexturePackerData.PackerShader);

            mat.SetTexture("_Red", red);
            mat.SetTexture("_Green", green);
            mat.SetTexture("_Blue", blue);
            mat.SetTexture("_Alpha", alpha);

            mat.SetInt("_Invert_Red", Convert.ToInt32(invertRed));
            mat.SetInt("_Invert_Green", Convert.ToInt32(invertGreen));
            mat.SetInt("_Invert_Blue", Convert.ToInt32(invertBlue));
            mat.SetInt("_Invert_Alpha", Convert.ToInt32(invertAlpha));

            // Create texture and render to it
            var tex = new Texture2D(resolution.x, resolution.y);
            tex.BakeMaterialToTexture(mat);

            // Cleanup
            Helpers.DestroyAppropriate(mat);

            return tex;
        }

        internal static Dictionary<string, Texture2D> UnpackTextureToChannels(Texture2D packedTexture, bool invert, Vector2Int resolution)
        {
            var channels = new Dictionary<string, Texture2D>
            {
                {TexturePackerData.PumkinsTextureChannel.Red.ToString().ToLower(),
                    new Texture2D(resolution.x, resolution.y, TextureFormat.RGB24, true)},
                {TexturePackerData.PumkinsTextureChannel.Green.ToString().ToLower(),
                    new Texture2D(resolution.x, resolution.y, TextureFormat.RGB24, true)},
                {TexturePackerData.PumkinsTextureChannel.Blue.ToString().ToLower(),
                    new Texture2D(resolution.x, resolution.y, TextureFormat.RGB24, true)},
                {TexturePackerData.PumkinsTextureChannel.Alpha.ToString().ToLower(),
                    new Texture2D(resolution.x, resolution.y, TextureFormat.RGB24, true)}
            };

            var mat = new Material(TexturePackerData.UnpackerShader);
            mat.SetTexture("_MainTex", packedTexture);
            mat.SetInt("_Invert", Convert.ToInt32(invert));

            for(int i = 0; i < 4; i++)
            {
                mat.SetFloat("_Mode", i);
                channels.ElementAt(i).Value.BakeMaterialToTexture(mat);
            }

            return channels;
        }

        internal static void DrawWithLabelWidth(float width, Action action)
        {
            if(action == null)
                return;
            float old = EditorGUIUtility.labelWidth;
            action.Invoke();
            EditorGUIUtility.labelWidth = old;
        }

        internal static TexturePackerData.PumkinsTextureChannel DrawChannelSelector(TexturePackerData.PumkinsTextureChannel currentSelection, params string[] labels)
        {
            if(labels == null)
                return TexturePackerData.PumkinsTextureChannel.RGBA;
            return (TexturePackerData.PumkinsTextureChannel)GUILayout.SelectionGrid((int)currentSelection, labels, labels.Length);
        }

        internal static bool IsCrunchCompressedTexture(Texture2D tex)
        {
            if(!tex)
                return false;

            string path = AssetDatabase.GetAssetPath(tex);
            if(string.IsNullOrWhiteSpace(path))
                return false;

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            return importer != null && importer.crunchedCompression;
        }
    }

    internal static class PumkinsTexturePackerExtensions
    {
        internal static Texture2D GetChannelAsTexture(this Texture2D tex, TexturePackerData.PumkinsTextureChannel chan, bool invert = false, Vector2Int sizeOverride = default)
        {
            if(chan == TexturePackerData.PumkinsTextureChannel.RGBA)
                return tex;

            if(sizeOverride == default)
                sizeOverride = new Vector2Int(tex.width, tex.height);

            Material mat = new Material(TexturePackerData.UnpackerShader);
            mat.SetFloat("_Mode", (int)chan - 1);
            mat.SetInt("_Invert", Convert.ToInt32(invert));
            mat.SetTexture("_MainTex", tex);

            var newTex = new Texture2D(sizeOverride.x, sizeOverride.y, TextureFormat.RGB24, true);
            newTex.name = chan.ToString();
            newTex.BakeMaterialToTexture(mat);
            newTex.Apply(false, false);

            return newTex;
        }

        /// <summary>
        /// Extension method that bakes a material to <paramref name="tex"/>
        /// </summary>
        /// <param name="tex">Texture to bake <paramref name="materialToBake"/> to</param>
        /// <param name="materialToBake">Material to bake to <paramref name="tex"/></param>
        internal static void BakeMaterialToTexture(this Texture2D tex, Material materialToBake)
        {
            var res = new Vector2Int(tex.width, tex.height);

            RenderTexture renderTexture = RenderTexture.GetTemporary(res.x, res.y);
            Graphics.Blit(null, renderTexture, materialToBake);

            //transfer image from rendertexture to texture
            RenderTexture.active = renderTexture;
            tex.ReadPixels(new Rect(Vector2.zero, res), 0, 0);
            tex.Apply(false, false);

            //clean up variables
            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(renderTexture);
        }

        internal static void SaveTextureAsset(this Texture2D tex, string assetPath, bool overwrite)
        {
            var bytes = tex.EncodeToPNG();


            // Ensure directory exists then convert path to local asset path
            if(!assetPath.StartsWith("Assets", StringComparison.OrdinalIgnoreCase))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(assetPath));
                assetPath = Helpers.AbsolutePathToLocalAssetsPath(assetPath);
            }
            else
            {
                string absolutePath = Helpers.LocalAssetsPathToAbsolutePath(assetPath);
                Directory.CreateDirectory(Path.GetDirectoryName(absolutePath));
            }

            if(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath) && !overwrite)
                assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);

            File.WriteAllBytes(assetPath, bytes);
            AssetDatabase.Refresh();
        }

        internal static Texture2D GetReadableTextureCopy(this Texture2D tex)
        {
            byte[] pix = tex.GetRawTextureData();
            Texture2D finalTex = new Texture2D(tex.width, tex.height, tex.format, false);
            finalTex.LoadRawTextureData(pix);
            finalTex.Apply();
            return finalTex;
        }

        /// <summary>
        /// Rounds vector to closest power of two. Optionally, if above ceiling, square root down by one power of two
        /// </summary>
        /// <param name="vec"></param>
        /// <param name="ceiling">Power of two ceiling. Will be rounded to power of two if not power of two already</param>
        /// <returns></returns>
        internal static Vector2Int ClosestPowerOfTwo(this Vector2Int vec, int? ceiling = null)
        {
            int x = Mathf.ClosestPowerOfTwo(vec.x);
            int y = Mathf.ClosestPowerOfTwo(vec.y);

            if(ceiling != null)
            {
                int ceil = Mathf.ClosestPowerOfTwo((int) ceiling);

                x = Mathf.Clamp(x, x, ceil);
                y = Mathf.Clamp(y, y, ceil);
            }

            return new Vector2Int(x, y);
        }
    }
}