
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace Backend.Tibia11
{
    public class Texture
    {
        public Bitmap Bitmap { get; set; }
        public byte[] Pixels { get; set; }
        byte[] BmpHeader { get; } = new byte[122];

        uint Width { get; }
        uint Height { get; }

        public Texture(uint width, uint height, byte[] pixels)
        {
            Array.Copy(pixels, BmpHeader, 122);
            this.Pixels = pixels;
            this.Bitmap = new Bitmap(new MemoryStream(pixels));
            this.Width = width;
            this.Height = height;
        }

        public byte[] SaveToByteArray()
        {
            using var stream = new MemoryStream();
            Bitmap.Save(stream, ImageFormat.Bmp);
            return stream.ToArray();
        }

        public byte[] RawTibiaBmp()
        {
            var rect = new Rectangle(0, 0, (int)Bitmap.Width, (int)Bitmap.Height);
            var newBitmap = Bitmap.Clone(rect, Bitmap.PixelFormat);

            var headerData = Util.Util.GetBITMAPV4HEADER((int)Width, (int)Height);
            int headerLength = headerData.Length;

            // = channels (Usually Alpha, Red, Green, Blue)
            int bytesPerPixel = Bitmap.GetPixelFormatSize(newBitmap.PixelFormat) / 8;

            int bitmapSize = (int)(Width * Height * bytesPerPixel);

            var result = new byte[headerLength + bitmapSize];

            // Add header
            Array.Copy(headerData, result, headerLength);

            newBitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
            var bitmapData = newBitmap.LockBits(rect, ImageLockMode.ReadWrite, newBitmap.PixelFormat);

            // Add bitmap data
            Marshal.Copy(bitmapData.Scan0, result, headerLength, bitmapSize);
            newBitmap.UnlockBits(bitmapData);

            return result;
        }
    }
}