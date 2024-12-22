using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace XiPHiA.OptMapTextureHelper
{
    public static class TextureUtility
    {
        public static Texture2D[] SeparateTexturesByChannel(Texture2D texture)
        {
            var filePath = AssetDatabase.GetAssetPath(texture);
            var is32Bit = HasAlphaChannel(texture);
            var bufferTexture = LoadTexture(filePath, is32Bit);
            var colorData = bufferTexture.GetPixels32(0);
            var separateTextures = new Texture2D[is32Bit ? 4 : 3];
            for (var i = 0; i < separateTextures.Length; i++)
            {
                separateTextures[i] = new Texture2D(bufferTexture.width, bufferTexture.height, TextureFormat.RGB24, false);
                var channelColorData = i switch
                {
                    0 => colorData.Select(c => new Color32(c.r, c.r, c.r, 255)).ToArray(),
                    1 => colorData.Select(c => new Color32(c.g, c.g, c.g, 255)).ToArray(),
                    2 => colorData.Select(c => new Color32(c.b, c.b, c.b, 255)).ToArray(),
                    3 => colorData.Select(c => new Color32(c.a, c.a, c.a, 255)).ToArray(),
                    _ => throw new Exception("Invalid channel number")
                };
                separateTextures[i].SetPixels32(channelColorData);
            }
            
            UnityEngine.Object.DestroyImmediate(bufferTexture);
            
            return separateTextures;
        }

        public static Texture2D CombineTextures(Texture2D redChannel, Texture2D greenChannel, Texture2D blueChannel)
        {
            var redChannelFilePath = AssetDatabase.GetAssetPath(redChannel);
            var greenChannelFilePath = AssetDatabase.GetAssetPath(greenChannel);
            var blueChannelFilePath = AssetDatabase.GetAssetPath(blueChannel);
            var redChannelTexture = LoadTexture(redChannelFilePath, false);
            var greenChannelTexture = LoadTexture(greenChannelFilePath, false);
            var blueChannelTexture = LoadTexture(blueChannelFilePath, false);
            if (redChannelTexture.width != greenChannelTexture.width || redChannelTexture.width != blueChannelTexture.width || greenChannelTexture.width != blueChannelTexture.width ||
                redChannelTexture.height != greenChannelTexture.height || redChannelTexture.height != blueChannelTexture.height || greenChannelTexture.height != blueChannelTexture.height)
            {
                throw new Exception("Texture sizes don't match");
            }
            var combinedTexture = new Texture2D(redChannelTexture.width, redChannelTexture.height, TextureFormat.RGB24, false);
            var combinedColorData = new Color32[redChannelTexture.width * redChannelTexture.height];
            var redData = redChannelTexture.GetPixels32(0);
            var greenData = greenChannelTexture.GetPixels32(0);
            var blueData = blueChannelTexture.GetPixels32(0);
            for (var i = 0; i < combinedColorData.Length; i++)
            {
                combinedColorData[i] = new Color32(redData[i].r, greenData[i].g, blueData[i].b, 255);
            }
            combinedTexture.SetPixels32(combinedColorData);
            UnityEngine.Object.DestroyImmediate(redChannelTexture);
            UnityEngine.Object.DestroyImmediate(greenChannelTexture);
            UnityEngine.Object.DestroyImmediate(blueChannelTexture);
            return combinedTexture;
        }

        public static Texture2D CombineTextures(Texture2D redChannel, Texture2D greenChannel, Texture2D blueChannel, Texture2D alphaChannel)
        {
            var redChannelFilePath = AssetDatabase.GetAssetPath(redChannel);
            var greenChannelFilePath = AssetDatabase.GetAssetPath(greenChannel);
            var blueChannelFilePath = AssetDatabase.GetAssetPath(blueChannel);
            var alphaChannelFilePath = AssetDatabase.GetAssetPath(alphaChannel);
            var redChannelTexture = LoadTexture(redChannelFilePath, false);
            var greenChannelTexture = LoadTexture(greenChannelFilePath, false);
            var blueChannelTexture = LoadTexture(blueChannelFilePath, false);
            var alphaChannelTexture = LoadTexture(alphaChannelFilePath, false);
            if (redChannel.width != greenChannel.width || redChannel.width != blueChannel.width || redChannel.width != alphaChannel.width ||
                greenChannel.width != blueChannel.width || greenChannel.width != alphaChannel.width || blueChannel.width != alphaChannel.width ||
                redChannel.height != greenChannel.height || redChannel.height != blueChannel.height || redChannel.height != alphaChannel.height ||
                greenChannel.height != blueChannel.height || greenChannel.height != alphaChannel.height || blueChannel.height != alphaChannel.height)
            {
                throw new Exception("Texture sizes don't match");
            }
            var combinedTexture = new Texture2D(redChannelTexture.width, redChannelTexture.height, TextureFormat.RGBA32, false);
            var combinedColorData = new Color32[redChannelTexture.width * redChannelTexture.height];
            var redData = redChannelTexture.GetPixels32(0);
            var greenData = greenChannelTexture.GetPixels32(0);
            var blueData = blueChannelTexture.GetPixels32(0);
            var alphaData = alphaChannelTexture.GetPixels32(0);
            for (var i = 0; i < combinedColorData.Length; i++)
            {
                combinedColorData[i] = new Color32(redData[i].r, greenData[i].g, blueData[i].b, alphaData[i].r);
            }
            combinedTexture.SetPixels32(combinedColorData);
            UnityEngine.Object.DestroyImmediate(redChannelTexture);
            UnityEngine.Object.DestroyImmediate(greenChannelTexture);
            UnityEngine.Object.DestroyImmediate(blueChannelTexture);
            UnityEngine.Object.DestroyImmediate(alphaChannelTexture);
            return combinedTexture;
        }

        private static bool HasAlphaChannel(Texture2D texture)
        {
            return texture.format switch
            {
                TextureFormat.RGBA32 or TextureFormat.ARGB32 or TextureFormat.DXT5 or TextureFormat.DXT5Crunched => true,
                _ => false
            };
        }

        private static Texture2D LoadTexture(string filePath, bool is32Bit)
        {
            var extension = Path.GetExtension(filePath).ToLower();
            var isPngOrJpg = extension is ".png" or ".jpg" or ".jpeg";
            var isTga = extension is ".tga";
            var isSupported = isPngOrJpg || isTga;
            if (!isSupported)
            {
                throw new Exception("Texture format not supported");
            }
            return isPngOrJpg ? LoadPngOrJpgTexture(filePath, is32Bit) : LoadTga(filePath, is32Bit);
        }

        private static Texture2D LoadPngOrJpgTexture(string filePath, bool is32Bit)
        {
            var imageData = File.ReadAllBytes(filePath);
            var bufferTexture = new Texture2D(2, 2, is32Bit ? TextureFormat.RGBA32 : TextureFormat.RGB24, false);
            bufferTexture.LoadImage(imageData);
            return bufferTexture;
        }

        private static Texture2D LoadTga(string filePath, bool is32Bit)
        {
            var tgaImageData = TGALoader.Load(filePath);
            var bufferTexture = new Texture2D(tgaImageData.Header.Width, tgaImageData.Header.Height, is32Bit ? TextureFormat.RGBA32 : TextureFormat.RGB24, false);
            bufferTexture.SetPixels32(tgaImageData.PixelData, 0);
            return bufferTexture;
        }
    }
}