using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace XiPHiA.OptMapTextureHelper
{
    public class OptMapTextureHelper : EditorWindow
    {
        #region Constants

        private const string OptionRimLightMaskPrefix = "_rim";
        private const string OptionOutlineMaskPrefix = "_out";
        private const string OptionSSSMaskPrefix = "_sss";
        private const string SpecularOptionNoiseTexturePrefix = "_nse";
        private const string SpecularOptionNoiseMaskPrefix = "_nse_msk";
        private const string SpecularOptionFeatherTexturePrefix = "_fth";
        private const string OtherTextureRPrefix = "_red";
        private const string OtherTextureGPrefix = "_grn";
        private const string OtherTextureBPrefix = "_blu";
        private const string OtherTextureAPrefix = "_alp";
        private const string CombinedPrefix = "_mod";

        private const string OutputFormatKey = "XiPHiAOptMapTextureHelperOutputFormat";
        private const string CombineModeKey = "XiPHiAOptMapTextureHelperCombineMode";
        
        private const string PrefixPattern = "_(rim|out|sss|nse|nse_msk|fth|red|blu|grn|alp)$";
        private const string BaseOptMapPrefixPattern = "_(rim|out|sss)$";
        private const string SpecularOptMapPrefixPattern = "_(nse|nse_msk|fth)$";
        private const string GeneralPrefixPattern = "_(red|blu|grn|alp)$";

        private const string ToolName = "Opt Map Texture Helper";

        #endregion
        
        #region Internal Variables
        
        private Vector2 _scrollPosition = Vector2.zero;
        private Texture2D _texture;
        private Texture2D _textureR;
        private Texture2D _textureG;
        private Texture2D _textureB;
        private Texture2D _textureA;
        private OutputFormat _outputFormat;
        private CombineMode _combineMode;
        private bool _useAlphaChannel;
        
        #endregion

        private enum OutputFormat
        {
            Auto,
            TGA,
            PNG,
            JPG
        }

        private enum CombineMode
        {
            BaseOptMap,
            SpecularOptMap,
            General
        }

        private enum Channel
        {
            Red,
            Blue,
            Green,
            Alpha
        }
        
        [MenuItem("Tools/" + ToolName)]
        public static void ShowWindow()
        {
            var window = GetWindow<OptMapTextureHelper>(ToolName);
            window._outputFormat = GetSavedParameter<OutputFormat>(OutputFormatKey);
            window._combineMode = GetSavedParameter<CombineMode>(CombineModeKey);
        }

        [MenuItem("Assets/" + ToolName + "/Separate Channels")]
        public static void SeparateChannels()
        {
            var selected = Selection.activeObject;
            if (selected is not Texture2D)
            {
                if (EditorUtility.DisplayDialog(ToolName, "Selected file is not texture.", "OK")) return;
            }
            SeparateTextures(selected as Texture2D, GetSavedParameter<OutputFormat>(OutputFormatKey));
        }

        [MenuItem("Assets/" + ToolName + "/Combine Channels")]
        public static void CombineChannels()
        {
            var selected = Selection.activeObject;
            if (selected is not Texture2D)
            {
                if (EditorUtility.DisplayDialog(ToolName, "Selected file is not texture.", "OK")) return;
            }
            var selectedTexture = selected as Texture2D;
            var filePath = AssetDatabase.GetAssetPath(selectedTexture);
            var baseName = Path.GetFileNameWithoutExtension(filePath);
            if (!Regex.IsMatch(baseName, PrefixPattern))
            {
                if (EditorUtility.DisplayDialog(ToolName, "Selected file is not following naming rules.", "OK")) return;
            }
            var optBaseName = Regex.Replace(baseName, PrefixPattern, string.Empty);
            var directory = Path.GetDirectoryName(filePath) ?? "";
            var extension = Path.GetExtension(filePath).ToLower();
            var isBaseOptMap = Regex.IsMatch(baseName, BaseOptMapPrefixPattern);
            var isSpecularOptMap = Regex.IsMatch(baseName, SpecularOptMapPrefixPattern);
            var isGeneral = Regex.IsMatch(baseName, GeneralPrefixPattern);
            if (!isGeneral)
            {
                var redChannelPath = Path.Combine(directory, $"{optBaseName}{(isBaseOptMap ? OptionRimLightMaskPrefix : SpecularOptionNoiseTexturePrefix)}{extension}");
                var greenChannelPath = Path.Combine(directory, $"{optBaseName}{(isBaseOptMap ? OptionOutlineMaskPrefix : SpecularOptionNoiseMaskPrefix)}{extension}");
                var blueChannelPath = Path.Combine(directory, $"{optBaseName}{(isBaseOptMap ? OptionSSSMaskPrefix : SpecularOptionFeatherTexturePrefix)}{extension}");
                var isRedChannelTextureExist = File.Exists(redChannelPath);
                var isGreenChannelTextureExist = File.Exists(greenChannelPath);
                var isBlueChannelTextureExist = File.Exists(blueChannelPath);
                if (!isRedChannelTextureExist || !isGreenChannelTextureExist || !isBlueChannelTextureExist)
                {
                    if (EditorUtility.DisplayDialog(ToolName, "Failed to find correspond textures", "OK")) return;
                }
                var redChannelTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(redChannelPath);
                var greenChannelTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(greenChannelPath);
                var blueChannelTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(blueChannelPath);
                CombineTextures(redChannelTexture, greenChannelTexture, blueChannelTexture, null, GetSavedParameter<OutputFormat>(OutputFormatKey), isBaseOptMap ? CombineMode.BaseOptMap : CombineMode.SpecularOptMap);
            }
            else
            {
                var redChannelPath = Path.Combine(directory, $"{optBaseName}{OtherTextureRPrefix}{extension}");
                var greenChannelPath = Path.Combine(directory, $"{optBaseName}{OtherTextureGPrefix}{extension}");
                var blueChannelPath = Path.Combine(directory, $"{optBaseName}{OtherTextureBPrefix}{extension}");
                var alphaChannelPath = Path.Combine(directory, $"{optBaseName}{OtherTextureAPrefix}{extension}");
                var isRedChannelTextureExist = File.Exists(redChannelPath);
                var isGreenChannelTextureExist = File.Exists(greenChannelPath);
                var isBlueChannelTextureExist = File.Exists(blueChannelPath);
                var isAlphaChannelTextureExist = File.Exists(alphaChannelPath);
                if (!isRedChannelTextureExist || !isGreenChannelTextureExist || !isBlueChannelTextureExist)
                {
                    if (EditorUtility.DisplayDialog(ToolName, "Failed to find correspond textures", "OK")) return;
                }
                var redChannelTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(redChannelPath);
                var greenChannelTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(greenChannelPath);
                var blueChannelTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(blueChannelPath);
                var alphaChannelTexture = isAlphaChannelTextureExist ? AssetDatabase.LoadAssetAtPath<Texture2D>(alphaChannelPath) : null;
                CombineTextures(redChannelTexture, greenChannelTexture, blueChannelTexture, alphaChannelTexture, GetSavedParameter<OutputFormat>(OutputFormatKey), CombineMode.General);
            }
        }

        private void OnGUI()
        {
            using var scrollView = new EditorGUILayout.ScrollViewScope(_scrollPosition);
            _scrollPosition = scrollView.scrollPosition;
            EditorGUILayout.LabelField("Output Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space(8);
            var outputFormat = (OutputFormat)EditorGUILayout.EnumPopup("Output Format", _outputFormat);
            if (outputFormat != _outputFormat)
            {
                _outputFormat = outputFormat;
                EditorUserSettings.SetConfigValue(OutputFormatKey, outputFormat.ToString());
            }
            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Texture Separator", EditorStyles.boldLabel);
            EditorGUILayout.Space(8);
            _texture = (Texture2D)EditorGUILayout.ObjectField("Texture", _texture, typeof(Texture2D), false);
            EditorGUILayout.Space(4);
            if (GUILayout.Button("Separate") && _texture is not null)
            {
                SeparateTextures(_texture, _outputFormat);
            }
            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Texture Combiner", EditorStyles.boldLabel);
            EditorGUILayout.Space(8);
            var combineMode = (CombineMode)EditorGUILayout.EnumPopup("Combine Mode", _combineMode);
            if (combineMode != _combineMode)
            {
                _combineMode = combineMode;
                EditorUserSettings.SetConfigValue(CombineModeKey, combineMode.ToString());
            }
            EditorGUILayout.Space(4);
            if (_combineMode == CombineMode.General)
            {
                _useAlphaChannel = EditorGUILayout.Toggle("Use Alpha Channel", _useAlphaChannel);
                EditorGUILayout.Space(4);
            }
            _textureR = (Texture2D)EditorGUILayout.ObjectField(GetCombineTextureLabel(_combineMode, Channel.Red), _textureR, typeof(Texture2D), false);
            EditorGUILayout.Space(4);
            _textureG = (Texture2D)EditorGUILayout.ObjectField(GetCombineTextureLabel(_combineMode, Channel.Green), _textureG, typeof(Texture2D), false);
            EditorGUILayout.Space(4);
            _textureB = (Texture2D)EditorGUILayout.ObjectField(GetCombineTextureLabel(_combineMode, Channel.Blue), _textureB, typeof(Texture2D), false);
            EditorGUILayout.Space(4);
            if (_combineMode == CombineMode.General && _useAlphaChannel)
            {
                _textureA = (Texture2D)EditorGUILayout.ObjectField(GetCombineTextureLabel(_combineMode, Channel.Alpha), _textureA, typeof(Texture2D), false);
                EditorGUILayout.Space(4);
            }
            if (GUILayout.Button("Combine") &&
                ((_combineMode != CombineMode.General && _textureR is not null && _textureG is not null && _textureB is not null) ||
                 (_combineMode == CombineMode.General && _textureR is not null && _textureG is not null && _textureB is not null && _textureA is not null)))
            {
                CombineTextures(_textureR, _textureG, _textureB, _combineMode != CombineMode.General ? null : _textureA,
                    GetSavedParameter<OutputFormat>(OutputFormatKey), _combineMode);
            }
            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.LabelField("ver 2024.12.21", EditorStyles.centeredGreyMiniLabel);
        }

        private static void SeparateTextures(Texture2D baseTexture, OutputFormat format = OutputFormat.Auto)
        {
            var filePath = AssetDatabase.GetAssetPath(baseTexture);
            var baseName = Path.GetFileNameWithoutExtension(filePath);
            var directory = Path.GetDirectoryName(filePath) ?? "";
            var extension = Path.GetExtension(filePath).ToLower();
            var outputExtension = DetermineOutputExtension(format, extension);
            if (AssetImporter.GetAtPath(filePath) is not TextureImporter originalTextureImporter)
            {
                throw new Exception($"failed to get texture importer: {filePath}");
            }
            var isSpecularOptMap = baseName.EndsWith("_spe_opt");
            var isBaseOptMap = !isSpecularOptMap && baseName.EndsWith("_opt");
            var textures = TextureUtility.SeparateTexturesByChannel(baseTexture);
            for (var i = 0; i < textures.Length; i++)
            {
                var prefix = i switch
                {
                    0 => isBaseOptMap ? OptionRimLightMaskPrefix : isSpecularOptMap ? SpecularOptionNoiseTexturePrefix : OtherTextureRPrefix,
                    1 => isBaseOptMap ? OptionOutlineMaskPrefix : isSpecularOptMap ? SpecularOptionNoiseMaskPrefix : OtherTextureGPrefix,
                    2 => isBaseOptMap ? OptionSSSMaskPrefix : isSpecularOptMap ? SpecularOptionFeatherTexturePrefix : OtherTextureBPrefix,
                    3 => OtherTextureAPrefix,
                    _ => throw new Exception($"Invalid texture channel: {i}")
                };
                var imageData = EncodeImage(textures[i], outputExtension);
                var texturePath = Path.Combine(directory, $"{baseName}{prefix}{outputExtension}");
                File.WriteAllBytes(texturePath, imageData);
                AssetDatabase.Refresh();
                if (AssetImporter.GetAtPath(texturePath) is not TextureImporter importer)
                {
                    throw new Exception($"failed to get texture importer: {texturePath}");
                }
                importer.crunchedCompression = originalTextureImporter.crunchedCompression;
                importer.compressionQuality = originalTextureImporter.compressionQuality;
                importer.mipmapEnabled = originalTextureImporter.mipmapEnabled;
                importer.streamingMipmaps = originalTextureImporter.streamingMipmaps;
                importer.maxTextureSize = originalTextureImporter.maxTextureSize;
                importer.SaveAndReimport();
            }
        }

        private static void CombineTextures(Texture2D redChannel, Texture2D greenChannel, Texture2D blueChannel, Texture2D alphaChannel = null, OutputFormat format = OutputFormat.Auto, CombineMode combineMode = CombineMode.BaseOptMap)
        {
            var filePath = AssetDatabase.GetAssetPath(redChannel);
            var baseName = Path.GetFileNameWithoutExtension(filePath);
            var optBaseName = Regex.Replace(baseName, PrefixPattern, string.Empty, RegexOptions.IgnoreCase);
            var directory = Path.GetDirectoryName(filePath) ?? "";
            var extension = Path.GetExtension(filePath).ToLower();
            var outputExtension = DetermineOutputExtension(format, extension);
            if (AssetImporter.GetAtPath(filePath) is not TextureImporter originalTextureImporter)
            {
                throw new Exception($"failed to get texture importer: {filePath}");
            }
            try
            {
                var combinedTexture = combineMode != CombineMode.General
                    ? TextureUtility.CombineTextures(redChannel, greenChannel, blueChannel)
                    : TextureUtility.CombineTextures(redChannel, greenChannel, blueChannel, alphaChannel);
                var imageData = EncodeImage(combinedTexture, outputExtension);
                var texturePath = Path.Combine(directory, $"{optBaseName}{CombinedPrefix}{outputExtension}");
                File.WriteAllBytes(texturePath, imageData);
                AssetDatabase.Refresh();
                if (AssetImporter.GetAtPath(texturePath) is not TextureImporter importer)
                {
                    throw new Exception($"failed to get texture importer: {texturePath}");
                }
                importer.crunchedCompression = originalTextureImporter.crunchedCompression;
                importer.compressionQuality = originalTextureImporter.compressionQuality;
                importer.mipmapEnabled = originalTextureImporter.mipmapEnabled;
                importer.streamingMipmaps = originalTextureImporter.streamingMipmaps;
                importer.maxTextureSize = originalTextureImporter.maxTextureSize;
                importer.SaveAndReimport();
            }
            catch (Exception e)
            {
                if (EditorUtility.DisplayDialog(ToolName, e.Message, "OK")) return;
            }
        }

        private static string DetermineOutputExtension(OutputFormat format, string originalExtension)
        {
            return format switch
            {
                OutputFormat.Auto => originalExtension,
                OutputFormat.TGA => ".tga",
                OutputFormat.PNG => ".png",
                OutputFormat.JPG => ".jpg",
                _ => originalExtension
            };
        }

        private static byte[] EncodeImage(Texture2D texture, string extension)
        {
            return extension switch
            {
                ".png" => texture.EncodeToPNG(),
                ".jpg" => texture.EncodeToJPG(),
                ".tga" => texture.EncodeToTGA(),
                _ => throw new Exception($"Invalid image format: {extension}")
            };
        }

        private static T GetSavedParameter<T>(string key)
        {
            var savedParameters = EditorUserSettings.GetConfigValue(key);
            return string.IsNullOrEmpty(savedParameters) ? default : (T)Enum.Parse(typeof(T), savedParameters, true);
        }

        private static string GetCombineTextureLabel(CombineMode combineMode, Channel channel)
        {
            return combineMode switch
            {
                CombineMode.BaseOptMap => channel switch
                {
                    Channel.Red => "RimLight Mask",
                    Channel.Green => "Outline Mask",
                    Channel.Blue => "SSS Mask",
                    Channel.Alpha => "Alpha Channel",
                    _ => throw new Exception($"Invalid texture channel: {combineMode},{channel}")
                },
                CombineMode.SpecularOptMap => channel switch
                {
                    Channel.Red => "Noise",
                    Channel.Green => "Noise Mask",
                    Channel.Blue => "Feather",
                    Channel.Alpha => "Alpha Channel",
                    _ => throw new Exception($"Invalid texture channel: {combineMode},{channel}")
                },
                CombineMode.General => channel switch
                {
                    Channel.Red => "Red Channel",
                    Channel.Green => "Green Channel",
                    Channel.Blue => "Blue Channel",
                    Channel.Alpha => "Alpha Channel",
                    _ => throw new Exception($"Invalid texture channel: {combineMode},{channel}")
                },
                _ => throw new Exception($"Invalid combine mode: {combineMode}")
            };
        }
    }
}
