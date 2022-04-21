using System;
using System.Collections.Generic;
using System.Linq;
using Pumkin.DataStructures;
using Pumkin.HelperFunctions;
using UnityEditor;
using UnityEngine;

namespace Pumkin.TexturePacker
{
    public class PumkinsTexturePackerEditor : EditorWindow
    {
        public const string DefaultResourcesPath = "Library/unity default resources/";

        const string LOG_PREFIX = "<color=blue>Texture Packer:</color> "; //color is hex or name
        static readonly Vector2 MIN_WINDOW_SIZE = new Vector2(400, 500);
        const int AUTO_SELECT_CEILING = 2048;

        const short texturesRequiredToPack = 1; // How many textures required to allow packing

        const string INVERT_LABEL = "Invert";
        const string PACKED_TEXTURE_LABEL = "Texture";
        const string RED_TEXTURE_LABEL = "Red";
        const string GREEN_TEXTURE_LABEL = "Green";
        const string BLUE_TEXTURE_LABEL = "Blue";
        const string ALPHA_TEXTURE_LABEL = "Alpha";
        
        const string COMPRESSION_WARNING_LABEL = "Input texture is crunch compressed. This can lead to a loss of quality.";

        float InvertToggleWidth
        {
            get
            {
                if(_invertLabelWidth == default)
                    _invertLabelWidth = EditorStyles.toggle.CalcSize(new GUIContent(INVERT_LABEL)).x;
                return _invertLabelWidth;
            }
        }

        // Default values
        string savePath = "Assets/_TexturePacker";
        string packedName = "packed";
        string unpackedName = "unpacked";

        // Version
        Version version = new Version(1, 3, 1);
        string SubTitle
        {
            get
            {
                if(string.IsNullOrWhiteSpace(_subTitle))
                    _subTitle = $"v{version}";
                return _subTitle;
            }
        }

        static EditorWindow Window
        {
            get
            {
                if(!_window)
                    _window = GetWindow<PumkinsTexturePackerEditor>();
                return _window;
            }
        }

        // Texture stuff
        static int[] SizePresets { get; } = { 128, 256, 512, 1024, 2048, 4096 };
        string[] SizePresetNames
        {
            get
            {
                if(_sizeNames == null)
                    _sizeNames = SizePresets.Select(i => i + " x " + i).ToArray();
                return _sizeNames;
            }
        }

        Vector2Int PackSize { get; set; } = new Vector2Int(1024, 1024);
        Vector2Int UnpackSize { get; set; } = new Vector2Int(1024, 1024);

        bool packSizeIsLinked = true;
        bool unpackSizeIsLinked = true;

        bool packSizeAutoSelect = true;
        bool unpackSizeAutoSelect = true;

        bool showChannelPicker = false;

        TexturePackerData.PumkinsTextureChannel redTexChan, blueTexChan, greenTexChan, alphaTexChan, unpackChan;
        Texture2D packRed, packGreen, packBlue, packAlpha, unpackSource;
        bool redInvert, greenInvert, blueInvert, alphaInvert, unpackInvert;
        bool redWarning, greenWarning, blueWarning, alphaWarning, unpackWarning;

        string[] ChannelLabels { get; } = { "All", "Red", "Green", "Blue", "Alpha" };

        bool PackerShadersExist
        {
            get
            {
                bool everythingIsAlwaysFine = true;

                if(!TexturePackerData.UnpackerShader)
                {
                    Debug.LogWarning(LOG_PREFIX + "Unpacker shader is missing or invalid. Can't unpack textures.");
                    everythingIsAlwaysFine = false;
                }

                if(!TexturePackerData.PackerShader)
                {
                    Debug.LogWarning(LOG_PREFIX + "Packer shader is missing or invalid. Can't pack textures.");
                    everythingIsAlwaysFine = false;
                }

                return everythingIsAlwaysFine;
            }
        }

        // UI
        enum Tab { Pack, Unpack }
        int selectedTab = 0;
        string[] TabNames
        {
            get
            {
                if(_tabNames == null)
                    _tabNames = Enum.GetNames(typeof(Tab));
                return _tabNames;
            }
        }


        [MenuItem("Pumkin/Tools/Texture Packer", priority = 31)]
        public static void ShowWindow()
        {
            //Show existing window instance. If one doesn't exist, make one.
            Window.autoRepaintOnSceneChange = true;
            Window.minSize = MIN_WINDOW_SIZE;

            Window.Show();
            Window.titleContent = new GUIContent("Texture Packer");
        }

        #region Drawing GUI

        void OnGUI()
        {
            EditorGUILayout.LabelField("Pumkins Texture Packer", Styles.Label_mainTitle);
            EditorGUILayout.LabelField(SubTitle);

            Helpers.DrawGUILine();

            selectedTab = GUILayout.Toolbar(selectedTab, TabNames);

            if(selectedTab == (int)Tab.Pack)
                DrawPackUI();
            else
                DrawUnpackUI();
        }

        void DrawPackUI()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            {
                bool textureChanged = false;
                EditorGUI.BeginChangeCheck();
                DrawTextureSelector(RED_TEXTURE_LABEL, ref packRed, ref redTexChan, ref redInvert, redWarning);
                if(EditorGUI.EndChangeCheck())
                {
                    textureChanged = true;
                    redWarning = PumkinsTexturePackerHelpers.IsCrunchCompressedTexture(packRed);
                }

                EditorGUI.BeginChangeCheck();
                DrawTextureSelector(GREEN_TEXTURE_LABEL, ref packGreen, ref greenTexChan, ref greenInvert, greenWarning);
                if(EditorGUI.EndChangeCheck())
                {
                    textureChanged = true;
                    greenWarning = PumkinsTexturePackerHelpers.IsCrunchCompressedTexture(packGreen);
                }

                EditorGUI.BeginChangeCheck();
                DrawTextureSelector(BLUE_TEXTURE_LABEL, ref packBlue, ref blueTexChan, ref blueInvert, blueWarning);
                if(EditorGUI.EndChangeCheck())
                {
                    textureChanged = true;
                    blueWarning = PumkinsTexturePackerHelpers.IsCrunchCompressedTexture(packBlue);
                }

                EditorGUI.BeginChangeCheck();
                DrawTextureSelector(ALPHA_TEXTURE_LABEL, ref packAlpha, ref alphaTexChan, ref alphaInvert, alphaWarning);
                if(EditorGUI.EndChangeCheck())
                {
                    textureChanged = true;
                    alphaWarning = PumkinsTexturePackerHelpers.IsCrunchCompressedTexture(packAlpha);
                }

                if(textureChanged && packSizeAutoSelect)
                {
                    // Get biggest texture size from selections and make a selection in our sizes list
                    var tempSize = PumkinsTexturePackerHelpers.GetMaxSizeFromTextures(packRed, packGreen, packBlue, packAlpha);
                    if(tempSize != default)
                        PackSize = tempSize.ClosestPowerOfTwo(AUTO_SELECT_CEILING);
                }
            }
            EditorGUILayout.EndScrollView();

            DrawShowChannelPicker(ref showChannelPicker);
            
            bool disabled = new bool[] { packRed, packGreen, packBlue, packAlpha }.Count(b => b) < texturesRequiredToPack;
            EditorGUI.BeginDisabledGroup(disabled);
            {
                PackSize = DrawTextureSizeSettings(PackSize, ref packedName, ref packSizeIsLinked, ref packSizeAutoSelect);

                if(GUILayout.Button("Pack", Styles.BigButton))
                    DoPack();

                EditorGUILayout.Space();
            }
            EditorGUI.EndDisabledGroup();
        }

        private void DrawShowChannelPicker(ref bool pickerValue)
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            pickerValue = EditorGUILayout.ToggleLeft("Pick source channel", pickerValue);
            EditorGUILayout.EndHorizontal();
        }

        void DrawUnpackUI()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            {
                EditorGUI.BeginChangeCheck();
                {
                    DrawTextureSelector(PACKED_TEXTURE_LABEL, ref unpackSource, ref unpackChan, ref unpackInvert, unpackWarning);
                }
                if(EditorGUI.EndChangeCheck() && unpackSizeAutoSelect)
                {
                    // Get biggest texture size from selections and make a selection in our sizes list
                    var tempSize = PumkinsTexturePackerHelpers.GetMaxSizeFromTextures(unpackSource);
                    if(tempSize != default)
                        UnpackSize = tempSize.ClosestPowerOfTwo(AUTO_SELECT_CEILING);

                    unpackWarning = PumkinsTexturePackerHelpers.IsCrunchCompressedTexture(unpackSource);
                }
            }
            EditorGUILayout.EndScrollView();
            
            DrawShowChannelPicker(ref showChannelPicker);

            EditorGUI.BeginDisabledGroup(!unpackSource);
            {
                UnpackSize = DrawTextureSizeSettings(UnpackSize, ref unpackedName, ref unpackSizeIsLinked, ref unpackSizeAutoSelect);

                if(GUILayout.Button("Unpack", Styles.BigButton))
                    DoUnpack(unpackChan);
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space();
        }

        #endregion

        #region Packing and Unpacking

        void DoPack()
        {
            if(!PackerShadersExist)
                return;

            Texture2D red = packRed;
            Texture2D green = packGreen;
            Texture2D blue = packBlue;
            Texture2D alpha = packAlpha;

            if(showChannelPicker)
            {
                red = packRed.GetChannelAsTexture(redTexChan, false);
                green = packGreen.GetChannelAsTexture(greenTexChan, false);
                blue = packBlue.GetChannelAsTexture(blueTexChan, false);
                alpha = packAlpha.GetChannelAsTexture(alphaTexChan, false);
            }

            Texture2D packResult = PumkinsTexturePackerHelpers.PackTextures(PackSize, red, green, blue, alpha, redInvert, greenInvert, blueInvert, alphaInvert);
            if(packResult)
            {
                string path = $"{savePath}/Packed/{packedName}.png";
                packResult.SaveTextureAsset(path, true);
                Debug.Log(LOG_PREFIX + "Finished packing texture at " + path);
                Helpers.SelectAndPing(path);
            }

        }

        void DoUnpack(TexturePackerData.PumkinsTextureChannel singleChannel = TexturePackerData.PumkinsTextureChannel.RGBA)
        {
            if(!PackerShadersExist)
                return;

            var channelTextures = new Dictionary<string, Texture2D>();
            if(singleChannel == TexturePackerData.PumkinsTextureChannel.RGBA)
                channelTextures = PumkinsTexturePackerHelpers.UnpackTextureToChannels(unpackSource, unpackInvert, UnpackSize);
            else
                channelTextures[singleChannel.ToString().ToLower()] = unpackSource.GetChannelAsTexture(singleChannel, unpackInvert, UnpackSize);


            string pingPath = null;
            pingPath = SaveTextures(channelTextures, pingPath);

            Debug.Log(LOG_PREFIX + "Finished unpacking texture at " + pingPath);
            Helpers.SelectAndPing(pingPath);
        }

        #endregion

        #region Helpers

        string SaveTextures(Dictionary<string, Texture2D> output, string pingPath)
        {
            try
            {
                AssetDatabase.StartAssetEditing();
                foreach(var kv in output)
                {
                    if(string.IsNullOrWhiteSpace(pingPath))
                        pingPath = $"{savePath}/Unpacked/{unpackedName}_{kv.Key}.png";
                    kv.Value?.SaveTextureAsset($"{savePath}/Unpacked/{unpackedName}_{kv.Key}.png", true);
                }
            }
            catch { }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            return pingPath;
        }

        void DrawTextureSelector(string label, ref Texture2D tex, bool showCompressionWarning)
        {
            EditorGUILayout.BeginVertical(Styles.HelpBox);
            {
                tex = EditorGUILayout.ObjectField(label, tex, typeof(Texture2D), true, GUILayout.ExpandHeight(true)) as Texture2D;
                if(showCompressionWarning)
                    EditorGUILayout.HelpBox(COMPRESSION_WARNING_LABEL, MessageType.Warning, true);
            }
            EditorGUILayout.EndHorizontal();
        }

        void DrawTextureSelector(string label, ref Texture2D tex, ref TexturePackerData.PumkinsTextureChannel selectedChannel, ref bool invert, bool showCompressionWarning)
        {
            EditorGUILayout.BeginVertical(Styles.HelpBox);
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.BeginVertical();
                    {
                        var labelContent = new GUIContent(label);
                        var size = EditorStyles.boldLabel.CalcSize(labelContent);

                        EditorGUILayout.LabelField(labelContent, EditorStyles.boldLabel, GUILayout.MaxWidth(size.x));

                        GUILayout.Space(15 * EditorGUIUtility.pixelsPerPoint);

                        GUILayout.FlexibleSpace();

                        invert = EditorGUILayout.ToggleLeft(INVERT_LABEL, invert, GUILayout.MaxWidth(InvertToggleWidth));
                    }
                    EditorGUILayout.EndVertical();

                    tex = EditorGUILayout.ObjectField(GUIContent.none, tex, typeof(Texture2D), true, GUILayout.ExpandHeight(true)) as Texture2D;
                }
                EditorGUILayout.EndHorizontal();

                if(showChannelPicker)
                {
                    EditorGUI.BeginDisabledGroup(!tex);
                    selectedChannel = PumkinsTexturePackerHelpers.DrawChannelSelector(selectedChannel, ChannelLabels);
                    EditorGUI.EndDisabledGroup();
                }
                
                if(showCompressionWarning)
                    EditorGUILayout.HelpBox(COMPRESSION_WARNING_LABEL, MessageType.Warning, true);
            }
            EditorGUILayout.EndVertical();
        }

        Vector2Int DrawTextureSizeSettings(Vector2Int size, ref string fileName, ref bool sizeIsLinked, ref bool sizeAutoSelect)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                fileName = EditorGUILayout.TextField("File name", fileName);
                EditorGUILayout.Space();
                size = PumkinsTexturePackerHelpers.DrawResolutionPicker(size, ref sizeIsLinked, ref sizeAutoSelect, SizePresets, SizePresetNames);
            }
            EditorGUILayout.EndVertical();
            return size;
        }

        #endregion

        string[] _tabNames;
        string[] _sizeNames;
        static EditorWindow _window;
        string _subTitle;
        Vector2 _scroll;
        float _invertLabelWidth;
    }
}