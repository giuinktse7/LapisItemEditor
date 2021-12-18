using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace Backend.Util
{
    public class RGBPixel
    {
        public byte red;
        public byte green;
        public byte blue;
    }

    public static class ImageExtensions
    {
        public static Bitmap Crop(this Bitmap bitmap, Rectangle section)
        {
            // Create the new bitmap and associated graphics object
            var result = new Bitmap(section.Width, section.Height, bitmap.PixelFormat);

            using var graphics = Graphics.FromImage(result);
            graphics.DrawImage(bitmap, 0, 0, section, GraphicsUnit.Pixel);

            return result;
        }

        public static byte[] ToByteArray(this Bitmap bitmap, ImageFormat format)
        {
            using var ms = new MemoryStream();
            // bitmap.MakeTransparent(Color.Magenta);

            bitmap.Save(ms, format);

            return ms.ToArray();
        }

        public static byte[] BitmapData(this Bitmap bitmap)
        {
            var bitmapData = bitmap.LockBits(new Rectangle(0, 0, (int)bitmap.Width, (int)bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            var length = bitmapData.Stride * bitmapData.Height;

            byte[] bytes = new byte[length];

            Marshal.Copy(bitmapData.Scan0, bytes, 0, length);
            bitmap.UnlockBits(bitmapData);

            return bytes;
        }

        public static byte[] ToRawBmpArray(this Bitmap bitmap)
        {
            bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);

            // Lock bitmap and return bitmap data
            var bitmapData = bitmap.LockBits(new Rectangle(0, 0, (int)bitmap.Width, (int)bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);

            // create byte array to copy pixel values
            var pixels = new byte[bitmap.Width * bitmap.Height * 4];

            // Copy data from pointer to array
            Marshal.Copy(bitmapData.Scan0, pixels, 0, pixels.Length);
            bitmap.UnlockBits(bitmapData);

            return pixels;
        }

        private static RGBPixel DefaultTransparencyColor = new RGBPixel()
        {
            red = 255,
            green = 0,
            blue = 255
        };

        public static byte[] GetRGBData(this Bitmap bitmap)
        {
            return GetRGBData(bitmap, DefaultTransparencyColor);
        }

        public static byte[] GetRGBData(this Bitmap bitmap, RGBPixel transparencyColor)
        {
            if (bitmap.PixelFormat != PixelFormat.Format32bppArgb)
            {
                throw new ArgumentException("Only PixelFormat.Format32bppArgb is supported.");
            }

            int pixelsInBitmap = bitmap.Width * bitmap.Height;

            // 3 for R, G, B
            byte[] dest = new byte[pixelsInBitmap * 3];

            var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            BitmapData bitmapData = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            try
            {
                // 4 for A, R, G, B
                int bitmapSizeInBytes = pixelsInBitmap * 4;

                byte[] src = new byte[bitmapSizeInBytes];
                Marshal.Copy(bitmapData.Scan0, src, 0, bitmapSizeInBytes);

                int srcCursor = 0;
                int destCursor = 0;

                while (destCursor < dest.Length)
                {
                    byte alpha = src[srcCursor + 3];

                    // Transparent, write transparency color
                    if (alpha == 0)
                    {
                        dest[destCursor] = transparencyColor.blue;
                        dest[destCursor + 1] = transparencyColor.green;
                        dest[destCursor + 2] = transparencyColor.red;
                    }
                    else
                    {
                        // The format is 32bppArgb, but written in little endian, so the bytes are in order B, G, R, A
                        // This copies the bytes B, G, R, in that order, into the output buffer.
                        System.Buffer.BlockCopy(src, srcCursor, dest, destCursor, 3);
                    }

                    destCursor += 3;
                    srcCursor += 4;
                }
            }
            finally
            {
                bitmap.UnlockBits(bitmapData);
            }

            return dest;
        }



        private static byte[]? GetSpriteHashDataForRegion(byte[] rgbaData, int dataWidthInPixels, Rectangle region)
        {
            byte[] dest = new byte[region.Width * region.Height * 4];

            // 4 bytes per pixel
            int startOffset = (region.Y * dataWidthInPixels + region.X) * 4;

            int srcCursor = startOffset;
            int destCursor = 0;

            // We must check whether the sprite region contains any non-empty pixels. If it does not, we do not include it in the sprite hash due to legacy reasons.
            bool hasData = false;

            int writtenOnRow = 0;
            while (destCursor < dest.Length)
            {
                byte alpha = rgbaData[srcCursor + 3];

                // Transparent, write transparency color
                if (alpha == 0)
                {
                    // 0x11 to maintain compatibility with the hashes of the old ItemEditor
                    dest[destCursor++] = 0x11;
                    dest[destCursor++] = 0x11;
                    dest[destCursor++] = 0x11;
                    srcCursor += 3;
                }
                else
                {
                    // We have data if there is any non-transparent pixel
                    hasData = true;

                    byte b = rgbaData[srcCursor++];
                    byte g = rgbaData[srcCursor++];
                    byte r = rgbaData[srcCursor++];

                    dest[destCursor++] = b;
                    dest[destCursor++] = g;
                    dest[destCursor++] = r;
                }

                // Zero out alpha
                // dest[destCursor++] = 0;
                ++srcCursor;
                ++destCursor;

                ++writtenOnRow;
                if (writtenOnRow == region.Width)
                {
                    srcCursor += (dataWidthInPixels - region.Width) * 4;
                    writtenOnRow = 0;
                }
            }

            return hasData ? dest : null;
        }

        public static byte[]? blankARGBSprite = null;
        public static byte[] BlankARGBSprite
        {
            get
            {
                if (blankARGBSprite == null)
                {
                    blankARGBSprite = new byte[4096];
                    for (int i = 0; i < 4096; i += 4)
                    {
                        blankARGBSprite[i] = 0x11;
                        blankARGBSprite[i + 1] = 0x11;
                        blankARGBSprite[i + 2] = 0x11;
                    }
                }

                return blankARGBSprite;
            }
        }

        public static void GetSpriteHashData(this Bitmap bitmap, Stream stream)
        {
            if (bitmap.PixelFormat != PixelFormat.Format32bppArgb)
            {
                throw new ArgumentException("Only PixelFormat.Format32bppArgb is supported.");
            }

            int spriteWidth = bitmap.Width;
            int spriteHeight = bitmap.Height;

            int pixelsInBitmap = spriteWidth * spriteHeight;

            Bitmap copy = new Bitmap(bitmap);
            copy.RotateFlip(RotateFlipType.RotateNoneFlipY);

            /// Subdivide the sprite into 32x32 chunks to get hashes that match the old ItemEditor
            /// We must do this because all sprites in the old format (tibia.spr) were 32x32.
            int piecesX = spriteWidth / 32;
            int piecesY = spriteHeight / 32;

            int bitmapSizeInBytes = pixelsInBitmap * 4;
            byte[] buffer = new byte[bitmapSizeInBytes];

            var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            BitmapData bitmapData = copy.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            try
            {
                Marshal.Copy(bitmapData.Scan0, buffer, 0, bitmapSizeInBytes);

                // Increasing in y because the bit data is y-flipped (to match legacy sprite hashes)
                for (int yOffset = 0; yOffset < piecesY; ++yOffset)
                {
                    for (int xOffset = piecesX - 1; xOffset >= 0; --xOffset)
                    {
                        var data = GetSpriteHashDataForRegion(buffer, spriteWidth, new Rectangle(xOffset * 32, yOffset * 32, 32, 32));
                        if (data != null)
                        {
                            stream.Write(data, 0, data.Length);
                        }
                        else
                        {
                            stream.Write(BlankARGBSprite, 0, BlankARGBSprite.Length);
                        }
                    }
                }
            }
            finally
            {
                copy.UnlockBits(bitmapData);
            }
        }

        /// <summary>
        /// Returns data as 32bppArgb.
        /// </summary>
        public static byte[] GetSpriteHashData(this Bitmap bitmap)
        {
            if (bitmap.PixelFormat != PixelFormat.Format32bppArgb)
            {
                throw new ArgumentException("Only PixelFormat.Format32bppArgb is supported.");
            }

            // 0x11 to maintain compatibility with the hashes of the old ItemEditor
            byte transparencyByte = 0x11;

            int pixelsInBitmap = bitmap.Width * bitmap.Height;

            // 4 for A, R, G, B
            int bitmapSizeInBytes = pixelsInBitmap * 4;


            Bitmap copy = new Bitmap(bitmap);
            copy.RotateFlip(RotateFlipType.RotateNoneFlipY);

            byte[] buffer = new byte[bitmapSizeInBytes];

            var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            BitmapData bitmapData = copy.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            try
            {
                Marshal.Copy(bitmapData.Scan0, buffer, 0, bitmapSizeInBytes);

                int cursor = 0;
                int row = bitmap.Height - 1;

                while (cursor < buffer.Length)
                {
                    byte alpha = buffer[cursor + 3];

                    // Transparent, write transparency color
                    if (alpha == 0)
                    {
                        buffer[cursor] = transparencyByte;
                        buffer[cursor + 1] = transparencyByte;
                        buffer[cursor + 2] = transparencyByte;
                    }

                    // Zero out alpha
                    buffer[cursor + 3] = 0;
                    cursor += 4;
                }
            }
            finally
            {
                copy.UnlockBits(bitmapData);
            }

            return buffer;
        }
    }

    public static class Util
    {

        ///<summary>
        /// Get the BMP header in the BITMAPV4HEADER format (the format that Tibia uses).
        ///</summary>
        public static byte[] GetBITMAPV4HEADER(int width, int height)
        {
            int bmpHeaderSize = 14;
            int dibHeaderSize = 108;
            int headerSize = bmpHeaderSize + dibHeaderSize;
            byte[] buffer = new byte[headerSize];

            // ID field (42h, 4Dh)
            buffer[0x00] = 0x42;
            buffer[0x01] = 0x4D;

            // Size of the BMP file
            WriteU32LE(buffer, 2, headerSize + width * height * 4);

            // [0x06, 0x07] Reserved (Application specific, unused here)
            // [0x08, 0x09] Reserved (Application specific, unused here)

            buffer[0x0A] = (byte)headerSize; // Offset where pixel data can be found
            buffer[0x0E] = (byte)dibHeaderSize; // DIB Header size

            WriteU32LE(buffer, 0x12, width); // Width of the bitmap in pixels
            WriteU32LE(buffer, 0x16, height); // Height of the bitmap in pixels

            WriteU16LE(buffer, 0x1A, 1); // Number of color planes being used
            WriteU16LE(buffer, 0x1C, 32); // Number of bits per pixel
            WriteU32LE(buffer, 0x1E, 3); // BI_BITFIELDS, no pixel array compression used

            // [0x22, 0x26) Size of the raw bitmap data (including padding) (Unused by Tibia)
            // [0x26, 0x2A) Print resolution of the image (Horizontal) (Unused by Tibia)
            // [0x2A, 0x2E) Print resolution of the image (Vertical) (Unused by Tibia)
            // [0x2E, 0x32) Number of colors in the palette (Unused by Tibia)
            // [0x32, 0x36) 0 means all colors are important (Unused by Tibia)

            // Red channel bit mask
            buffer[0x36] = 0x00;
            buffer[0x37] = 0x00;
            buffer[0x38] = 0xFF;
            buffer[0x39] = 0x00;

            // Green channel bit mask
            buffer[0x3A] = 0x00;
            buffer[0x3B] = 0xFF;
            buffer[0x3C] = 0x00;
            buffer[0x3D] = 0x00;

            // Green channel bit mask
            buffer[0x3E] = 0xFF;
            buffer[0x3F] = 0x00;
            buffer[0x40] = 0x00;
            buffer[0x41] = 0x00;

            // Alpha channel bit mask
            buffer[0x42] = 0x00;
            buffer[0x43] = 0x00;
            buffer[0x44] = 0x00;
            buffer[0x45] = 0xFF;

            // LCS_WINDOWS_COLOR_SPACE (bV4CSType) "Win "
            WriteU32LE(buffer, 0x46, 0x206E6957);

            // [0x4A, 0x6E) CIEXYZTRIPLE Color Space endpoints (Unused for LCS "Win ")
            // [0x6E, 0x72) Red Gamma (Unused for LCS "Win ")
            // [0x72, 0x76) Green Gamma (Unused for LCS "Win ")
            // [0x76, 0x7A) Blue Gamma (Unused for LCS "Win ")

            return buffer;
        }

        private static void WriteU32LE(byte[] buffer, int offset, int value)
        {
            buffer[offset] = (byte)value;
            buffer[offset + 1] = (byte)(value >> 8);
            buffer[offset + 2] = (byte)(value >> 16);
            buffer[offset + 3] = (byte)(value >> 24);
        }

        private static void WriteU16LE(byte[] buffer, int offset, ushort value)
        {
            buffer[offset] = (byte)value;
            buffer[offset + 1] = (byte)(value >> 8);
        }


        public static int Get7BitEncoded(byte b1, byte b2)
        {
            using var ss = new MemoryStream();
            var first = b1.ToString("X2");
            var second = b2.ToString("X2");

            ss.WriteByte(b1);
            ss.WriteByte(b2);

            ss.Position = 0;
            using var reader = new BinaryReader(ss);
            return reader.Read7BitEncodedInt();
        }
    }
}