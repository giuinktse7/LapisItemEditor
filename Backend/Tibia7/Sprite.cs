using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace Backend.Tibia7
{
    public class Sprite
    {
        public const byte DefaultSize = 32;
        public const ushort PixelsDataSize = 4096; // 32 * 32 * 4
        public const ushort RGBPixelsDataSize = 3072; // 32 * 32 * 3

        // Why 0x11? Copied over from https://github.com/ottools/ItemEditor
        private const byte TransparencyColorForRGBData = 0x11;

        private bool useAlpha = false;


        public byte[] CompressedPixels { get; internal set; }

        public uint ID { get; set; }
        public int Length
        {
            get { return this.CompressedPixels == null ? 0 : this.CompressedPixels.Length; }
        }

        public Sprite(uint id, byte[] compressedPixels, bool useAlpha)
        {
            this.ID = id;
            this.useAlpha = useAlpha;
            this.CompressedPixels = compressedPixels;
        }

        public override string ToString()
        {
            return this.ID.ToString();
        }


        public Bitmap CreateBitmap()
        {
            var bitmap = new Bitmap(DefaultSize, DefaultSize, PixelFormat.Format32bppArgb);

            byte[] pixels = UncompressPixelsBGRA();
            if (pixels != null)
            {
                BitmapData bitmapData = bitmap.LockBits(bitmapRectangle, ImageLockMode.ReadWrite, bitmap.PixelFormat);
                Marshal.Copy(pixels, 0, bitmapData.Scan0, pixels.Length);
                bitmap.UnlockBits(bitmapData);
            }

            return bitmap;
        }

        // public Sprite Clone()
        // {
        //     return new Sprite(this.ID, (byte[])CompressedPixels.Clone(), this.useAlpha);
        // }

        private static readonly byte[] EmptyArray = new byte[0];


        public static byte[] CompressPixelsBGRA(byte[] pixels, bool transparent)
        {
            if (pixels == null)
            {
                throw new ArgumentNullException(nameof(pixels));
            }

            if (pixels.Length != PixelsDataSize)
            {
                throw new Exception("Invalid pixels data size");
            }

            byte[] compressedPixels;

            using (BinaryWriter writer = new BinaryWriter(new MemoryStream()))
            {
                int read = 0;
                int alphaCount = 0;
                ushort chunkSize = 0;
                long coloredPos = 0;
                long finishOffset = 0;
                int length = pixels.Length / 4;
                int index = 0;

                while (index < length)
                {
                    chunkSize = 0;

                    // Read transparent pixels
                    while (index < length)
                    {
                        read = (index * 4) + 3;

                        // alpha
                        if (pixels[read++] != 0)
                        {
                            break;
                        }

                        alphaCount++;
                        chunkSize++;
                        index++;
                    }

                    // Read colored pixels
                    if (alphaCount < length && index < length)
                    {
                        writer.Write(chunkSize); // Writes the length of the transparent pixels
                        coloredPos = writer.BaseStream.Position; // Save colored position
                        writer.BaseStream.Seek(2, SeekOrigin.Current); // Skip colored position
                        chunkSize = 0;

                        while (index < length)
                        {
                            read = index * 4;

                            byte blue = pixels[read++];
                            byte green = pixels[read++];
                            byte red = pixels[read++];
                            byte alpha = pixels[read++];

                            if (alpha == 0)
                            {
                                break;
                            }

                            writer.Write(red);
                            writer.Write(green);
                            writer.Write(blue);

                            if (transparent)
                            {
                                writer.Write(alpha);
                            }

                            chunkSize++;
                            index++;
                        }

                        finishOffset = writer.BaseStream.Position;
                        writer.BaseStream.Seek(coloredPos, SeekOrigin.Begin); // Go back to chunksize indicator
                        writer.Write(chunkSize); // Writes the length of he colored pixels
                        writer.BaseStream.Seek(finishOffset, SeekOrigin.Begin);
                    }
                }

                compressedPixels = ((MemoryStream)writer.BaseStream).ToArray();
            }

            return compressedPixels;
        }

        public static byte[] CompressPixelsBGRA(byte[] pixels)
        {
            return CompressPixelsBGRA(pixels, false);
        }

        public static byte[] CompressPixelsARGB(byte[] pixels, bool transparent)
        {
            if (pixels == null)
            {
                throw new ArgumentNullException(nameof(pixels));
            }

            if (pixels.Length != PixelsDataSize)
            {
                throw new Exception("Invalid pixels data size");
            }

            byte[] compressedPixels;

            using (BinaryWriter writer = new BinaryWriter(new MemoryStream()))
            {
                int read = 0;
                int alphaCount = 0;
                ushort chunkSize = 0;
                long coloredPos = 0;
                long finishOffset = 0;
                int length = pixels.Length / 4;
                int index = 0;

                while (index < length)
                {
                    chunkSize = 0;

                    // Read transparent pixels
                    while (index < length)
                    {
                        read = index * 4;

                        byte alpha = pixels[read++];
                        read += 3;

                        if (alpha != 0)
                        {
                            break;
                        }

                        alphaCount++;
                        chunkSize++;
                        index++;
                    }

                    // Read colored pixels
                    if (alphaCount < length && index < length)
                    {
                        writer.Write(chunkSize); // Writes the length of the transparent pixels
                        coloredPos = writer.BaseStream.Position; // Save colored position
                        writer.BaseStream.Seek(2, SeekOrigin.Current); // Skip colored position
                        chunkSize = 0;

                        while (index < length)
                        {
                            read = index * 4;

                            byte alpha = pixels[read++];
                            byte red = pixels[read++];
                            byte green = pixels[read++];
                            byte blue = pixels[read++];

                            if (alpha == 0)
                            {
                                break;
                            }

                            writer.Write(red);
                            writer.Write(green);
                            writer.Write(blue);

                            if (transparent)
                            {
                                writer.Write(alpha);
                            }

                            chunkSize++;
                            index++;
                        }

                        finishOffset = writer.BaseStream.Position;
                        writer.BaseStream.Seek(coloredPos, SeekOrigin.Begin); // Go back to chunksize indicator
                        writer.Write(chunkSize); // Writes the length of he colored pixels
                        writer.BaseStream.Seek(finishOffset, SeekOrigin.Begin);
                    }
                }

                compressedPixels = ((MemoryStream)writer.BaseStream).ToArray();
            }

            return compressedPixels;
        }

        public static byte[] CompressPixelsARGB(byte[] pixels)
        {
            return CompressPixelsARGB(pixels, false);
        }


        // private Bitmap UncompressBitmap()
        // {
        //     if (CompressedPixels == null)
        //     {
        //         throw new ArgumentNullException(nameof(compressedPixels));
        //     }

        //     Bitmap bitmap = new Bitmap(DefaultSize, DefaultSize, PixelFormat.Format32bppArgb);
        //     byte[] pixels = UncompressPixelsBGRA();
        //     BitmapData bitmapData = bitmap.LockBits(Rectangle, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
        //     Marshal.Copy(pixels, 0, bitmapData.Scan0, PixelsDataSize);
        //     bitmap.UnlockBits(bitmapData);
        //     return bitmap;
        // }

        private ushort readUint16(byte[] bytes, ref uint cursor)
        {
            return (ushort)(bytes[cursor++] | bytes[cursor++] << 8);
        }

        private byte readUint8(byte[] bytes, ref uint cursor)
        {
            return (byte)bytes[cursor++];
        }

        private byte[] UncompressPixelsBGRA()
        {
            if (CompressedPixels == null)
            {
                throw new ArgumentNullException(nameof(CompressedPixels));
            }

            byte[] pixels = new byte[PixelsDataSize];

            int compressedBytesLength = CompressedPixels.Length;
            byte channels = (byte)(useAlpha ? 4 : 3);

            uint readCursor = 0;
            uint writeCursor = 0;
            while (readCursor < CompressedPixels.Length && writeCursor < PixelsDataSize)
            {
                ushort transparentPixels = readUint16(CompressedPixels, ref readCursor);
                ushort colorPixels = readUint16(CompressedPixels, ref readCursor);

                writeCursor += transparentPixels * (uint)4;
                // for (int i = 0; i < transparentPixels; ++i)
                // {
                //     pixels[writeCursor++] = 0x00; // blue
                //     pixels[writeCursor++] = 0x00; // green
                //     pixels[writeCursor++] = 0x00; // red
                //     pixels[writeCursor++] = 0x00; // alpha
                // }

                for (int i = 0; i < colorPixels; i++)
                {
                    byte red = readUint8(CompressedPixels, ref readCursor);
                    byte green = readUint8(CompressedPixels, ref readCursor);
                    byte blue = readUint8(CompressedPixels, ref readCursor);
                    byte alpha = useAlpha ? readUint8(CompressedPixels, ref readCursor) : (byte)0xFF;

                    pixels[writeCursor++] = blue;
                    pixels[writeCursor++] = green;
                    pixels[writeCursor++] = red;
                    pixels[writeCursor++] = alpha;
                }
            }

            return pixels;
        }


        public byte[]? GetRGBData(byte transparentColor = TransparencyColorForRGBData)
        {
            if (CompressedPixels == null)
            {
                return null;
            }

            byte[] rgb32x32x3 = new byte[RGBPixelsDataSize];
            uint bytes = 0;
            uint x = 0;
            uint y = 0;
            int chunkSize;
            byte bitPerPixel = (byte)(useAlpha ? 4 : 3);

            while (bytes < CompressedPixels.Length)
            {
                chunkSize = CompressedPixels[bytes] | CompressedPixels[bytes + 1] << 8;
                bytes += 2;

                for (int i = 0; i < chunkSize; ++i)
                {
                    rgb32x32x3[96 * y + x * 3 + 0] = transparentColor;
                    rgb32x32x3[96 * y + x * 3 + 1] = transparentColor;
                    rgb32x32x3[96 * y + x * 3 + 2] = transparentColor;

                    ++x;
                    if (x >= 32)
                    {
                        x = 0;
                        ++y;
                    }
                }

                if (bytes >= CompressedPixels.Length)
                {
                    break;
                }

                chunkSize = CompressedPixels[bytes] | CompressedPixels[bytes + 1] << 8;
                bytes += 2;

                for (int i = 0; i < chunkSize; ++i)
                {
                    byte red = CompressedPixels[bytes + 0];
                    byte green = CompressedPixels[bytes + 1];
                    byte blue = CompressedPixels[bytes + 2];

                    rgb32x32x3[96 * y + x * 3 + 0] = red;
                    rgb32x32x3[96 * y + x * 3 + 1] = green;
                    rgb32x32x3[96 * y + x * 3 + 2] = blue;

                    bytes += bitPerPixel;

                    ++x;
                    if (x >= 32)
                    {
                        x = 0;
                        ++y;
                    }
                }
            }

            // Fill up any trailing pixels
            while (y < DefaultSize && x < DefaultSize)
            {
                rgb32x32x3[96 * y + x * 3 + 0] = transparentColor;
                rgb32x32x3[96 * y + x * 3 + 1] = transparentColor;
                rgb32x32x3[96 * y + x * 3 + 2] = transparentColor;

                ++x;
                if (x >= DefaultSize)
                {
                    x = 0;
                    ++y;
                }
            }

            return rgb32x32x3;
        }



        private static readonly Rectangle bitmapRectangle = new Rectangle(0, 0, DefaultSize, DefaultSize);
    }
}
