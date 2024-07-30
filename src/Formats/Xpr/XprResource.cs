using System.Runtime.CompilerServices;
using System.Text;
using UIXTool.Extensions;
using UIXTool.IO;
using UIXTool.Utilities;

// TODO: source references, add any licenses/attributions
// https://xboxdevwiki.net/XPR
// https://github.com/xemu-project/xemu/blob/master/hw/xbox/nv2a/swizzle.c
// https://github.com/mafaca/Dxt/blob/master/Dxt/DxtDecoder.cs
// https://learn.microsoft.com/en-us/windows/win32/directshow/working-with-16-bit-rgb

namespace UIXTool.Formats.Xpr
{
    public class XprResource : IDisposable
    {
        public const int StructSize = 20;

        public string Name
        {
            get
            {
                return "[" + Index + "] - " + Type.ToString();
            }
        }

        public string ToolTip
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("Type: {0}\r\n", Type.ToString());
                if (Type == XprResourceType.Texture)
                {
                    sb.AppendFormat("Format: {0}\r\n", TextureFormat.ToString());
                    sb.AppendFormat("Width: {0}\r\n", Width);
                    sb.AppendFormat("Height: {0}\r\n", Height);
                    sb.AppendFormat("Depth: {0}\r\n", Depth);
                    sb.AppendFormat("Mips: {0}\r\n", MipMapLevels);
                    sb.AppendFormat("Stream Position: 0x{0}\r\n", StreamPosition.ToString("X"));
                    sb.AppendFormat("Texture Data Position: 0x{0}\r\n", DataPosition.ToString("X"));
                    sb.AppendFormat("Texture Data Size: 0x{0}\r\n", CalculateTextureDataSize(TextureFormat, (int)Width, (int)Height).ToString("X"));
                }
                return sb.ToString();
            }
        }

        public Xpr Parent { get; private set; }

        public long StreamPosition { get; private set; }

        public int Index => (int)(StreamPosition - Parent.StreamPosition - Xpr.StructSize) / StructSize;

        public long EntryInfoOffset => Index * StructSize;   // relative to Parent.StreamPosition + Xpr.StructSize

        /// <summary>
        /// Common ownership info.
        /// </summary>
        private uint Common { get; set; }

        /// <summary>
        /// The resource data offset relative to the end of the header.
        /// </summary>
        public uint DataOffset { get; private set; }

        /// <summary>
        /// The absolute data position.
        /// </summary>
        public long DataPosition => Parent.StreamPosition + Parent.HeaderSize + DataOffset;

        /// <summary>
        /// TODO: description
        /// </summary>
        public uint Lock { get; private set; }

        /// <summary>
        /// Describes the resource gpu format.
        /// </summary>
        private uint GpuFormat { get; set; }

        /// <summary>
        /// Describes the size of a texture whose dimensions aren't a power of 2.
        /// </summary>
        private uint AlternateSize { get; set; }

        /// <summary>
        /// The resource reference count. Likely always 0 in file form.
        /// </summary>
        public uint RefCount => Common.BitField(0, 15);

        /// <summary>
        /// The resource type.
        /// </summary>
        public XprResourceType Type => (XprResourceType)Common.BitField(16, 18);

        /// <summary>
        /// TODO: description
        /// </summary>
        public uint CommonUnk => Common.BitField(19, 31);

        /// <summary>
        /// TODO: description
        /// </summary>
        public uint Dma => GpuFormat.BitField(0, 3);

        /// <summary>
        /// The texture dimensions; 1D, 2D, or 3D.
        /// </summary>
        public uint TextureDimensions => GpuFormat.BitField(4, 7);

        /// <summary>
        /// The texture format.
        /// </summary>
        public XprTextureFormat TextureFormat => (XprTextureFormat)GpuFormat.BitField(8, 15);

        /// <summary>
        /// The texture mip map levels.
        /// </summary>
        public uint MipMapLevels => GpuFormat.BitField(16, 19);
        
        // TODO: some textures appear to have dimensions set in both fields? unclear which takes precedence

        /// <summary>
        /// The texture width.
        /// </summary>
        public uint Width
        {
            get
            {
                var w1 = (int)GpuFormat.BitField(20, 23);
                Assert.Throw(((uint)(1 << w1)).IsPow2(), "Width must be a power of 2 when set via the {0} field.", nameof(GpuFormat));
                var w2 = AlternateSize.BitField(0, 11);
                if (w2 != 0) w2++;
                Assert.Throw((w1 == 0) ^ (w2 == 0), "Width cannot be set in both the {0} and {1} fields.", nameof(GpuFormat), nameof(AlternateSize));
                Assert.Throw(w1 != 0 || w2 != 0, "Width must be set in either the {0} or {1} fields.", nameof(GpuFormat), nameof(AlternateSize));
                return w1 > 0 ? ((uint)(1 << w1)) : w2;
            }
        }

        /// <summary>
        /// The texture height.
        /// </summary>
        public uint Height
        {
            get
            {
                var h1 = (int)GpuFormat.BitField(24, 27);
                Assert.Throw(((uint)(1 << h1)).IsPow2(), "Height must be a power of 2 when set via the {0} field.", nameof(GpuFormat));
                var h2 = AlternateSize.BitField(12, 23);
                if (h2 != 0) h2++;
                Assert.Throw((h1 == 0) ^ (h2 == 0), "Height cannot be set in both the {0} and {1} fields.", nameof(GpuFormat), nameof(AlternateSize));
                Assert.Throw(h1 != 0 || h2 != 0, "Height must be set in either the {0} or {1} fields.", nameof(GpuFormat), nameof(AlternateSize));
                return h1 > 0 ? ((uint)(1 << h1)) : h2;
            }
        }

        /// <summary>
        /// The texture depth.
        /// </summary>
        public uint Depth
        {
            get
            {
                if (TextureDimensions != 3)
                    return 0;

                var d1 = (int)GpuFormat.BitField(28, 31);
                Assert.Throw(((uint)(1 << d1)).IsPow2(), "Depth must be a power of 2 when set via the {0} field.", nameof(GpuFormat));
                var d2 = AlternateSize.BitField(24, 31);
                if (d2 != 0) d2++;
                Assert.Throw((d1 == 0) ^ (d2 == 0), "Depth cannot be set in both the {0} and {1} fields.", nameof(GpuFormat), nameof(AlternateSize));
                Assert.Throw(d1 != 0 || d2 != 0, "Depth must be set in either the {0} or {1} fields.", nameof(GpuFormat), nameof(AlternateSize));
                return d1 > 0 ? ((uint)(1 << d1)) : d2;
            }
        }

        public Image? Image { get; private set; }

        public XprResource(Xpr parent, EndianStream stream, long? position = null)
        {
            Parent = parent;
            if (position != null) stream.Position = position.Value;
            StreamPosition = stream.Position;
            Common = stream.Read<uint>();

            // skip vertex buffer stuff for now (half life 2 loader xpr)
            if (Type == XprResourceType.VertexBuffer)
            {
                uint vertexBufferSize = stream.Read<uint>();
                uint unkType = stream.Read<uint>();
                stream.Position += vertexBufferSize - 4; // skip the data (the 4 is probably the type size, the rest are type-specific headers)
                return;
            }
            // TODO: other non-texture types might also have inlined header data

            // specific to texture resource types most likely
            DataOffset = stream.Read<uint>();
            Lock = stream.Read<uint>();
            GpuFormat = stream.Read<uint>();
            AlternateSize = stream.Read<uint>();

            // only support 2D textures for now
            if (Type != XprResourceType.Texture || TextureDimensions != 2)
                return;

            int texSize = CalculateTextureDataSize(TextureFormat, (int)Width, (int)Height);
            long maxReadAvailable = Math.Min(stream.Length - DataPosition, texSize);
            if (maxReadAvailable < texSize)
            {
                // TODO: warn 
            }
            var resourceData = stream.PeekBytes(DataPosition, (int)maxReadAvailable);

            // ARGB pixel data
            byte[] bmp = new byte[Width * Height * 4];

            // convert texture to A8R8G8B8 bitmap data
            bool unusedAlpha = false;   // set for formats that disregard the alpha channel so it can be masked out in the bitmap
            switch (TextureFormat)
            {
                case XprTextureFormat.L_DXT1_A1R5G5B5:
                    DecompressDXT1(resourceData, (int)Width, (int)Height, bmp);
                    break;
                case XprTextureFormat.L_DXT23_A8R8G8B8:
                    DecompressDXT3(resourceData, (int)Width, (int)Height, bmp);
                    break;
                case XprTextureFormat.L_DXT45_A8R8G8B8:
                    DecompressDXT5(resourceData, (int)Width, (int)Height, bmp);
                    break;
                case XprTextureFormat.SZ_X8R8G8B8:
                    unusedAlpha = true;
                    goto case XprTextureFormat.SZ_A8R8G8B8;
                case XprTextureFormat.SZ_A8R8G8B8:
                    UnswizzleRect(resourceData, Width, Height, bmp, Width * 4, 4);
                    break;
                case XprTextureFormat.LU_IMAGE_X8R8G8B8:
                    unusedAlpha = true;
                    goto case XprTextureFormat.LU_IMAGE_A8R8G8B8;
                case XprTextureFormat.LU_IMAGE_A8R8G8B8:
                    Buffer.BlockCopy(resourceData, 0, bmp, 0, resourceData.Length);
                    break;
                case XprTextureFormat.SZ_R8G8B8A8:
                    {
                        // unswizzle
                        UnswizzleRect(resourceData, Width, Height, bmp, Width * 4, 4);

                        // convert RGBA to ARGB
                        for (int i = 0; i < resourceData.Length; i += 4)
                        {
                            (bmp[i], bmp[i + 1], bmp[i + 2], bmp[i + 3]) =
                                (bmp[i + 3], bmp[i], bmp[i + 1], bmp[i + 2]);
                        }
                    }
                    break;
                case XprTextureFormat.SZ_A8B8G8R8:
                    {
                        // unswizzle
                        UnswizzleRect(resourceData, Width, Height, bmp, Width * 4, 4);

                        // swap red and blue channels
                        uint pitch = Width * 4;
                        for (int y = 0; y < Height; y++)
                        {
                            for (int x = 0; x < Width; x++)
                            {
                                int dstOffset = (int)(y * pitch + x * 4);
                                var c = bmp[dstOffset + 0];
                                bmp[dstOffset + 0] = bmp[dstOffset + 2];
                                bmp[dstOffset + 2] = c;
                            }
                        }
                    }
                    break;
                case XprTextureFormat.LU_IMAGE_A8B8G8R8:
                    {
                        Buffer.BlockCopy(resourceData, 0, bmp, 0, resourceData.Length);

                        // swap red and blue channels
                        uint pitch = Width * 4;
                        for (int y = 0; y < Height; y++)
                        {
                            for (int x = 0; x < Width; x++)
                            {
                                int dstOffset = (int)(y * pitch + x * 4);
                                var c = bmp[dstOffset + 0];
                                bmp[dstOffset + 0] = bmp[dstOffset + 2];
                                bmp[dstOffset + 2] = c;
                            }
                        }
                    }
                    break;
                case XprTextureFormat.SZ_R5G6B5:
                    {
                        unusedAlpha = true;

                        // unswizzle into temp buffer
                        byte[] tmp = new byte[Width * Height * 2];
                        UnswizzleRect(resourceData, Width, Height, tmp, Width * 2, 2);

                        // convert to 32-bit
                        uint srcPitch = Width * 2;
                        uint dstPitch = Width * 4;
                        for (int y = 0; y < Height; y++)
                        {
                            for (int x = 0; x < Width; x++)
                            {
                                ushort pixel = BitConverter.ToUInt16(tmp, (int)(y * srcPitch + x * 2));
                                int dstOffset = (int)(y * dstPitch + x * 4);
                                bmp[dstOffset + 0] = (byte)((pixel & 0x001F) << 3);
                                bmp[dstOffset + 1] = (byte)((pixel & 0x07E0) >> 3);
                                bmp[dstOffset + 2] = (byte)((pixel & 0xF800) >> 8);
                                bmp[dstOffset + 3] = 0xFF;
                            }
                        }

                        break;
                    }
                case XprTextureFormat.LU_IMAGE_R5G6B5:
                    {
                        unusedAlpha = true;

                        // convert to 32-bit
                        uint srcPitch = Width * 2;
                        uint dstPitch = Width * 4;
                        for (int y = 0; y < Height; y++)
                        {
                            for (int x = 0; x < Width; x++)
                            {
                                ushort pixel = BitConverter.ToUInt16(resourceData, (int)(y * srcPitch + x * 2));
                                int dstOffset = (int)(y * dstPitch + x * 4);
                                bmp[dstOffset + 0] = (byte)((pixel & 0x001F) << 3);
                                bmp[dstOffset + 1] = (byte)((pixel & 0x07E0) >> 3);
                                bmp[dstOffset + 2] = (byte)((pixel & 0xF800) >> 8);
                                bmp[dstOffset + 3] = 0xFF;
                            }
                        }

                        break;
                    }
                case XprTextureFormat.SZ_X1R5G5B5:
                    unusedAlpha = true;
                    goto case XprTextureFormat.SZ_A1R5G5B5;
                case XprTextureFormat.SZ_A1R5G5B5:
                    {
                        // unswizzle into temp buffer
                        byte[] tmp = new byte[Width * Height * 2];
                        UnswizzleRect(resourceData, Width, Height, tmp, Width * 2, 2);

                        // convert to 32-bit
                        uint srcPitch = Width * 2;
                        uint dstPitch = Width * 4;
                        for (int y = 0; y < Height; y++)
                        {
                            for (int x = 0; x < Width; x++)
                            {
                                ushort pixel = BitConverter.ToUInt16(tmp, (int)(y * srcPitch + x * 2));
                                int dstOffset = (int)(y * dstPitch + x * 4);
                                bmp[dstOffset + 0] = (byte)((pixel & 0x001F) << 3);
                                bmp[dstOffset + 1] = (byte)((pixel & 0x03E0) >> 2);
                                bmp[dstOffset + 2] = (byte)((pixel & 0x7C00) >> 7);
                                bmp[dstOffset + 3] = (byte)((pixel & 0x8000) > 0 ? 0xFF : 0);
                            }
                        }

                        break;
                    }

                case XprTextureFormat.SZ_A4R4G4B4:
                    {
                        // unswizzle into temp buffer
                        byte[] tmp = new byte[Width * Height * 2];
                        UnswizzleRect(resourceData, Width, Height, tmp, Width * 2, 2);

                        // convert to 32-bit
                        uint srcPitch = Width * 2;
                        uint dstPitch = Width * 4;
                        for (int y = 0; y < Height; y++)
                        {
                            for (int x = 0; x < Width; x++)
                            {
                                ushort pixel = BitConverter.ToUInt16(tmp, (int)(y * srcPitch + x * 2));
                                int dstOffset = (int)(y * dstPitch + x * 4);
                                bmp[dstOffset + 0] = (byte)((pixel & 0x000F) << 4);
                                bmp[dstOffset + 1] = (byte)((pixel & 0x00F0) << 0);
                                bmp[dstOffset + 2] = (byte)((pixel & 0x0F00) >> 4);
                                bmp[dstOffset + 3] = (byte)((pixel & 0xF000) >> 8);
                            }
                        }

                        break;
                    }
                case XprTextureFormat.SZ_A8:
                case XprTextureFormat.SZ_Y8:
                    {
                        unusedAlpha = true;

                        // unswizzle into temp buffer
                        byte[] tmp = new byte[Width * Height];
                        UnswizzleRect(resourceData, Width, Height, tmp, Width, 1);

                        // convert to 32-bit grayscale
                        uint srcPitch = Width;
                        uint dstPitch = Width * 4;
                        for (int y = 0; y < Height; y++)
                        {
                            for (int x = 0; x < Width; x++)
                            {
                                byte value = tmp[y * srcPitch + x];
                                int dstOffset = (int)(y * dstPitch + x * 4);
                                bmp[dstOffset + 0] = value;
                                bmp[dstOffset + 1] = value;
                                bmp[dstOffset + 2] = value;
                                bmp[dstOffset + 3] = 0xFF;
                            }
                        }
                    }
                    break;
                default:
                    // TODO:
                    //SZ_I8_A8R8G8B8
                    //LU_IMAGE_DEPTH_Y16_FIXED
                    break;
            }

            // don't dispose the stream, it's required to be alive for saving the image; MemoryStream doesn't really need disposing anyways
            var ms = new MemoryStream();
            var es = new EndianStream(ms);

            // 32-bit ARGB - https://en.wikipedia.org/wiki/BMP_file_format#Example_2
            es.Write<ushort>(0x4D42);                       // "BM"
            es.Write<int>(122 + bmp.Length);                // file size
            es.Write<ushort>(0);                            // unused
            es.Write<ushort>(0);                            // unused
            es.Write<uint>(122);                            // pixel data offset
            es.Write<uint>(108);                            // number of bytes in the DIB header
            es.Write<uint>(Width);                          // image pixel width
            es.Write<int>(-(int)Height);                    // image pixel height; negative to indicate top-down orientation in memory
            es.Write<ushort>(1);                            // number of color planes
            es.Write<ushort>(32);                           // number of bits per pixel
            es.Write<uint>(3);                              // BI_BITFIELDS
            es.Write<int>(bmp.Length);                      // size of raw data
            es.Write<uint>(2835);                           // horizontal print dpi
            es.Write<uint>(2835);                           // vertical print dpi
            es.Write<uint>(0);                              // number of colors in the palette
            es.Write<uint>(0);                              // all important colors
            es.Write<uint>(0x00FF0000);                     // red channel mask
            es.Write<uint>(0x0000FF00);                     // green channel mask
            es.Write<uint>(0x000000FF);                     // blue channel mask
            es.Write<uint>(unusedAlpha ? 0 : 0xFF000000);   // alpha channel mask
            es.Write<uint>(0x57696E20);                     // "WIN " - Windows color space
            es.Write(new byte[0x24]);                       // unused when Windows color space specified
            es.Write<uint>(0);                              // unused when Windows color space specified
            es.Write<uint>(0);                              // unused when Windows color space specified
            es.Write<uint>(0);                              // unused when Windows color space specified
            es.Write(bmp, 0, bmp.Length);                   // pixel data
            es.Flush();

            // store the image for later
            Image = new Bitmap(ms);
        }

        private static int CalculateTextureDataSize(XprTextureFormat format, int width, int height)
        {
            switch (format)
            {
                case XprTextureFormat.L_DXT1_A1R5G5B5:
                    return (width * height) / 2;
                case XprTextureFormat.L_DXT23_A8R8G8B8:
                case XprTextureFormat.L_DXT45_A8R8G8B8:
                case XprTextureFormat.SZ_A8:
                case XprTextureFormat.SZ_I8_A8R8G8B8:   // TODO: confirm, palletized, might be 4bpp?
                case XprTextureFormat.SZ_Y8:
                    return width * height;
                case XprTextureFormat.SZ_A1R5G5B5:
                case XprTextureFormat.SZ_X1R5G5B5:
                case XprTextureFormat.SZ_A4R4G4B4:
                case XprTextureFormat.SZ_R5G6B5:
                case XprTextureFormat.LU_IMAGE_R5G6B5:
                case XprTextureFormat.LU_IMAGE_DEPTH_Y16_FIXED:
                    return width * height * 2;
                case XprTextureFormat.SZ_A8R8G8B8:
                case XprTextureFormat.SZ_X8R8G8B8:
                case XprTextureFormat.LU_IMAGE_A8R8G8B8:
                case XprTextureFormat.LU_IMAGE_X8R8G8B8:
                case XprTextureFormat.SZ_A8B8G8R8:
                case XprTextureFormat.SZ_R8G8B8A8:
                case XprTextureFormat.LU_IMAGE_A8B8G8R8:
                    return width * height * 4;
                default: throw new NotSupportedException();
            }
        }

        private static void DecompressDXT1(byte[] input, int width, int height, byte[] output)
        {
            int offset = 0;
            int bcw = (width + 3) / 4;
            int bch = (height + 3) / 4;
            int clen_last = (width + 3) % 4 + 1;
            uint[] buffer = new uint[16];
            int[] colors = new int[4];
            for (int t = 0; t < bch; t++)
            {
                for (int s = 0; s < bcw; s++, offset += 8)
                {
                    int r0, g0, b0, r1, g1, b1;
                    int q0 = input[offset + 0] | input[offset + 1] << 8;
                    int q1 = input[offset + 2] | input[offset + 3] << 8;
                    Rgb565(q0, out r0, out g0, out b0);
                    Rgb565(q1, out r1, out g1, out b1);
                    colors[0] = Color(r0, g0, b0, 255);
                    colors[1] = Color(r1, g1, b1, 255);
                    if (q0 > q1)
                    {
                        colors[2] = Color((r0 * 2 + r1) / 3, (g0 * 2 + g1) / 3, (b0 * 2 + b1) / 3, 255);
                        colors[3] = Color((r0 + r1 * 2) / 3, (g0 + g1 * 2) / 3, (b0 + b1 * 2) / 3, 255);
                    }
                    else
                    {
                        colors[2] = Color((r0 + r1) / 2, (g0 + g1) / 2, (b0 + b1) / 2, 255);
                    }

                    uint d = BitConverter.ToUInt32(input, offset + 4);
                    for (int i = 0; i < 16; i++, d >>= 2)
                    {
                        buffer[i] = unchecked((uint)colors[d & 3]);
                    }

                    int clen = (s < bcw - 1 ? 4 : clen_last) * 4;
                    for (int i = 0, y = t * 4; i < 4 && y < height; i++, y++)
                    {
                        Buffer.BlockCopy(buffer, i * 4 * 4, output, (y * width + s * 4) * 4, clen);
                    }
                }
            }
        }

        private static void DecompressDXT3(byte[] input, int width, int height, byte[] output)
        {
            int offset = 0;
            int bcw = (width + 3) / 4;
            int bch = (height + 3) / 4;
            int clen_last = (width + 3) % 4 + 1;
            uint[] buffer = new uint[16];
            int[] colors = new int[4];
            int[] alphas = new int[16];
            for (int t = 0; t < bch; t++)
            {
                for (int s = 0; s < bcw; s++, offset += 16)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        int alpha = input[offset + i * 2] | input[offset + i * 2 + 1] << 8;
                        alphas[i * 4 + 0] = (((alpha >> 0) & 0xF) * 0x11) << 24;
                        alphas[i * 4 + 1] = (((alpha >> 4) & 0xF) * 0x11) << 24;
                        alphas[i * 4 + 2] = (((alpha >> 8) & 0xF) * 0x11) << 24;
                        alphas[i * 4 + 3] = (((alpha >> 12) & 0xF) * 0x11) << 24;
                    }

                    int r0, g0, b0, r1, g1, b1;
                    int q0 = input[offset + 8] | input[offset + 9] << 8;
                    int q1 = input[offset + 10] | input[offset + 11] << 8;
                    Rgb565(q0, out r0, out g0, out b0);
                    Rgb565(q1, out r1, out g1, out b1);
                    colors[0] = Color(r0, g0, b0, 0);
                    colors[1] = Color(r1, g1, b1, 0);
                    if (q0 > q1)
                    {
                        colors[2] = Color((r0 * 2 + r1) / 3, (g0 * 2 + g1) / 3, (b0 * 2 + b1) / 3, 0);
                        colors[3] = Color((r0 + r1 * 2) / 3, (g0 + g1 * 2) / 3, (b0 + b1 * 2) / 3, 0);
                    }
                    else
                    {
                        colors[2] = Color((r0 + r1) / 2, (g0 + g1) / 2, (b0 + b1) / 2, 0);
                    }

                    uint d = BitConverter.ToUInt32(input, offset + 12);
                    for (int i = 0; i < 16; i++, d >>= 2)
                    {
                        buffer[i] = unchecked((uint)(colors[d & 3] | alphas[i]));
                    }

                    int clen = (s < bcw - 1 ? 4 : clen_last) * 4;
                    for (int i = 0, y = t * 4; i < 4 && y < height; i++, y++)
                    {
                        Buffer.BlockCopy(buffer, i * 4 * 4, output, (y * width + s * 4) * 4, clen);
                    }
                }
            }
        }

        private static void DecompressDXT5(byte[] input, int width, int height, byte[] output)
        {
            int offset = 0;
            int bcw = (width + 3) / 4;
            int bch = (height + 3) / 4;
            int clen_last = (width + 3) % 4 + 1;
            uint[] buffer = new uint[16];
            int[] colors = new int[4];
            int[] alphas = new int[8];
            for (int t = 0; t < bch; t++)
            {
                for (int s = 0; s < bcw; s++, offset += 16)
                {
                    alphas[0] = input[offset + 0];
                    alphas[1] = input[offset + 1];
                    if (alphas[0] > alphas[1])
                    {
                        alphas[2] = (alphas[0] * 6 + alphas[1]) / 7;
                        alphas[3] = (alphas[0] * 5 + alphas[1] * 2) / 7;
                        alphas[4] = (alphas[0] * 4 + alphas[1] * 3) / 7;
                        alphas[5] = (alphas[0] * 3 + alphas[1] * 4) / 7;
                        alphas[6] = (alphas[0] * 2 + alphas[1] * 5) / 7;
                        alphas[7] = (alphas[0] + alphas[1] * 6) / 7;
                    }
                    else
                    {
                        alphas[2] = (alphas[0] * 4 + alphas[1]) / 5;
                        alphas[3] = (alphas[0] * 3 + alphas[1] * 2) / 5;
                        alphas[4] = (alphas[0] * 2 + alphas[1] * 3) / 5;
                        alphas[5] = (alphas[0] + alphas[1] * 4) / 5;
                        alphas[7] = 255;
                    }
                    for (int i = 0; i < 8; i++)
                    {
                        alphas[i] <<= 24;
                    }

                    int r0, g0, b0, r1, g1, b1;
                    int q0 = input[offset + 8] | input[offset + 9] << 8;
                    int q1 = input[offset + 10] | input[offset + 11] << 8;
                    Rgb565(q0, out r0, out g0, out b0);
                    Rgb565(q1, out r1, out g1, out b1);
                    colors[0] = Color(r0, g0, b0, 0);
                    colors[1] = Color(r1, g1, b1, 0);
                    if (q0 > q1)
                    {
                        colors[2] = Color((r0 * 2 + r1) / 3, (g0 * 2 + g1) / 3, (b0 * 2 + b1) / 3, 0);
                        colors[3] = Color((r0 + r1 * 2) / 3, (g0 + g1 * 2) / 3, (b0 + b1 * 2) / 3, 0);
                    }
                    else
                    {
                        colors[2] = Color((r0 + r1) / 2, (g0 + g1) / 2, (b0 + b1) / 2, 0);
                    }

                    ulong da = BitConverter.ToUInt64(input, offset) >> 16;
                    uint dc = BitConverter.ToUInt32(input, offset + 12);
                    for (int i = 0; i < 16; i++, da >>= 3, dc >>= 2)
                    {
                        buffer[i] = unchecked((uint)(alphas[da & 7] | colors[dc & 3]));
                    }

                    int clen = (s < bcw - 1 ? 4 : clen_last) * 4;
                    for (int i = 0, y = t * 4; i < 4 && y < height; i++, y++)
                    {
                        Buffer.BlockCopy(buffer, i * 4 * 4, output, (y * width + s * 4) * 4, clen);
                    }
                }
            }
        }

        /* This should be pretty straightforward.
         * It creates a bit pattern like ..zyxzyxzyx from ..xxx, ..yyy and ..zzz
         * If there are no bits left from any component it will pack the other masks
         * more tighly (Example: zzxzxzyx = Fewer x than z and even fewer y)
         */
        private static void GenerateSwizzleMasks(uint width, uint height, uint depth, out uint mask_x, out uint mask_y, out uint mask_z)
        {
            uint x = 0, y = 0, z = 0;
            uint bit = 1;
            uint mask_bit = 1;
            bool done;
            do
            {
                done = true;
                if (bit < width) { x |= mask_bit; mask_bit <<= 1; done = false; }
                if (bit < height) { y |= mask_bit; mask_bit <<= 1; done = false; }
                if (bit < depth) { z |= mask_bit; mask_bit <<= 1; done = false; }
                bit <<= 1;
            } while (!done);
            Assert.Throw((x ^ y ^ z) == (mask_bit - 1), "Invalid swizzle mask.");
            mask_x = x;
            mask_y = y;
            mask_z = z;
        }

        /* This fills a pattern with a value if your value has bits abcd and your
         * pattern is 11010100100 this will return: 0a0b0c00d00
         */
        private static uint FillPattern(uint pattern, uint value)
        {
            uint result = 0;
            uint bit = 1;
            while (value != 0)
            {
                if ((pattern & bit) != 0)
                {
                    /* Copy bit to result */
                    result |= ((value & 1) != 0) ? bit : 0;
                    value >>= 1;
                }
                bit <<= 1;
            }
            return result;
        }

        private static uint GetSwizzledOffset(uint x, uint y, uint z, uint mask_x, uint mask_y, uint mask_z, uint bytes_per_pixel)
        {
            return bytes_per_pixel * (FillPattern(mask_x, x) | FillPattern(mask_y, y) | FillPattern(mask_z, z));
        }

        private static void SwizzleBox(byte[] src_buf, uint width, uint height, uint depth, byte[] dst_buf, uint row_pitch, uint slice_pitch, uint bytes_per_pixel)
        {
            uint srcBaseOffset = 0;
            GenerateSwizzleMasks(width, height, depth, out uint mask_x, out uint mask_y, out uint mask_z);
            for (uint z = 0; z < depth; z++)
            {
                for (uint y = 0; y < height; y++)
                {
                    for (uint x = 0; x < width; x++)
                    {
                        uint srcOffset = srcBaseOffset + y * row_pitch + x * bytes_per_pixel;
                        uint dstOffset = GetSwizzledOffset(x, y, z, mask_x, mask_y, mask_z, bytes_per_pixel);
                        Buffer.BlockCopy(src_buf, (int)srcOffset, dst_buf, (int)dstOffset, (int)bytes_per_pixel);
                    }
                }
                srcBaseOffset += slice_pitch;
            }
        }

        private static void UnswizzleBox(byte[] src_buf, uint width, uint height, uint depth, byte[] dst_buf, uint row_pitch, uint slice_pitch, uint bytes_per_pixel)
        {
            uint dstBaseOffset = 0;
            GenerateSwizzleMasks(width, height, depth, out uint mask_x, out uint mask_y, out uint mask_z);
            for (uint z = 0; z < depth; z++)
            {
                for (uint y = 0; y < height; y++)
                {
                    for (uint x = 0; x < width; x++)
                    {
                        uint srcOffset = GetSwizzledOffset(x, y, z, mask_x, mask_y, mask_z, bytes_per_pixel);
                        uint dstOffset = dstBaseOffset + y * row_pitch + x * bytes_per_pixel;
                        Buffer.BlockCopy(src_buf, (int)srcOffset, dst_buf, (int)dstOffset, (int)bytes_per_pixel);
                    }
                }
                dstBaseOffset += slice_pitch;
            }
        }

        private static void UnswizzleRect(byte[] src_buf, uint width, uint height, byte[] dst_buf, uint pitch, uint bytes_per_pixel)
        {
            UnswizzleBox(src_buf, width, height, 1, dst_buf, pitch, 0, bytes_per_pixel);
        }

        private static void SwizzleRect(byte[] src_buf, uint width, uint height, byte[] dst_buf, uint pitch, uint bytes_per_pixel)
        {
            SwizzleBox(src_buf, width, height, 1, dst_buf, pitch, 0, bytes_per_pixel);
        }

        // TODO: test what Xbox does, possibly not a "true" 565 conversion, there's some additional weighted bias fuckery with different implementations?
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Rgb565(int c, out int r, out int g, out int b)
        {
            // true rgb565
            //r = (c & 0xF800) >> 8;
            //g = (c & 0x07E0) >> 3;
            //b = (c & 0x001F) << 3;

            // seems to be more popular and possibly proper
            var rt = (c >> 11) * 0xFF + 16;
            var gt = ((c & 0x07e0) >> 5) * 0xFF + 32;
            var bt = (c & 0x001F) * 0xFF + 16;
            r = (rt / 32 + rt) / 32;
            g = (gt / 64 + gt) / 64;
            b = (bt / 32 + bt) / 32;

            // modified weighted bias that's close to the middle one and more efficient but slightly off
            //r = (c & 0xf800) >> 8;
            //r |= r >> 5;
            //g = (c & 0x07e0) >> 3;
            //g |= g >> 6;
            //b = (c & 0x001f) << 3;
            //b |= b >> 5;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Color(int r, int g, int b, int a)
        {
            return r << 16 | g << 8 | b | a << 24;
        }

        public void Dispose()
        {
            if (Image != null)
            {
                Image.Dispose();
            }
        }
    }
}
