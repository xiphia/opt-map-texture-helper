using System;
using System.IO;
using UnityEngine;

namespace XiPHiA.OptMapTextureHelper
{
    internal enum TGAImageType : byte
    {
        NoImageData = 0,
        UncompressedColorMappedImage = 1,
        UncompressedRGBImage = 2,
        UncompressedBlackWhiteImage = 3,
        RLEColorMappedImage = 9,
        RLERGBImage = 10,
        RLEBlackWhiteImage = 11,
    }

    public struct TGAHeader
    {
        public byte IDLength;
        public byte ColorMap;
        public byte DataTypeCode;
        public short ColorMapOrigin;
        public short ColorMapLength;
        public byte ColorMapDepth;
        public short XOrigin;
        public short YOrigin;
        public short Width;
        public short Height;
        public byte BitsPerPixel;
        public byte ImageDescriptor;
    }

    public struct TGAImage
    {
        public TGAHeader Header;
        public Color32[] PixelData;
    }
    
    public static class TGALoader
    {
        public static TGAImage Load(string fileName)
        {
            var data = File.ReadAllBytes(fileName);
            var header = ParseHeader(data);
            if (header.DataTypeCode != (byte)TGAImageType.UncompressedRGBImage &&
                header.DataTypeCode != (byte)TGAImageType.RLERGBImage)
            {
                throw new Exception($"Unsupported TGA image type: {header.DataTypeCode}");
            }
            if (header.BitsPerPixel != 24 && header.BitsPerPixel != 32)
            {
                throw new Exception($"Unsupported TGA bits per pixel: {header.BitsPerPixel}");
            }
            var pixelData = ParseData(header, data);
            return new TGAImage
            {
                Header = header,
                PixelData = pixelData
            };
        }

        private static TGAHeader ParseHeader(byte[] data)
        {
            return new TGAHeader
            {
                IDLength = data[0],
                ColorMap = data[1],
                DataTypeCode = data[2],
                ColorMapOrigin = BitConverter.ToInt16(data, 3),
                ColorMapLength = BitConverter.ToInt16(data, 5),
                ColorMapDepth = data[7],
                XOrigin = BitConverter.ToInt16(data, 8),
                YOrigin = BitConverter.ToInt16(data, 10),
                Width = BitConverter.ToInt16(data, 12),
                Height = BitConverter.ToInt16(data, 14),
                BitsPerPixel = data[16],
                ImageDescriptor = data[17]
            };
        }

        private static Color32[] ParseData(TGAHeader header, byte[] data)
        {
            var pixelData = new Color32[header.Width * header.Height];
            var cursor = 18;
            
            if (header.IDLength != 0)
            {
                cursor += header.IDLength;
            }

            if (header.ColorMap != 0)
            {
                cursor += header.ColorMapDepth * header.ColorMapLength * 3;
            }

            switch (header.DataTypeCode)
            {
                case (byte)TGAImageType.UncompressedRGBImage:
                {
                    for (var i = 0; i < pixelData.Length; i++)
                    {
                        pixelData[i] = ParseColor(data, cursor, header.BitsPerPixel);
                        cursor += header.BitsPerPixel / 8;
                    }
                    break;
                }
                case (byte)TGAImageType.RLERGBImage:
                    for (var i = 0; i < pixelData.Length;)
                    {
                        var d = data[cursor++];
                        if (d >= 0x80)
                        {
                            var color = ParseColor(data, cursor, header.BitsPerPixel);
                            cursor += header.BitsPerPixel / 8;
                            for (var j = 0; j <= d - 0x80; j++)
                            {
                                pixelData[i++] = color;
                            }
                        }
                        else
                        {
                            for (var j = 0; j <= d; j++)
                            {
                                pixelData[i++] = ParseColor(data, cursor, header.BitsPerPixel);
                                cursor += header.BitsPerPixel / 8;
                            }
                        }
                    }
                    break;
            }
            
            return pixelData;
        }

        private static Color32 ParseColor(byte[] data, int offset, int bitsPerPixel)
        {
            return bitsPerPixel switch
            {
                24 => new Color32(data[offset + 2], data[offset + 1], data[offset], 255),
                32 => new Color32(data[offset + 2], data[offset + 1], data[offset], data[offset + 3]),
                _ => throw new Exception($"Unsupported bits per pixel: {bitsPerPixel}")
            };
        }
    }
}