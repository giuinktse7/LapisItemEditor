using System;
using System.Drawing;
using System.Drawing.Imaging;


namespace LapisItemEditor
{
    public static class ImageExtensions
    {
        public static Avalonia.Media.Imaging.Bitmap ConvertToAvaloniaBitmap(System.Drawing.Bitmap bitmap)
        {
            if (bitmap == null)
            {
                throw new ArgumentException("bitmap must be non-null.");
            }

            var bitmapdata = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadWrite,
                PixelFormat.Format32bppArgb);

            var avaloniaBitmap = new Avalonia.Media.Imaging.Bitmap(
                Avalonia.Platform.PixelFormat.Bgra8888,
                Avalonia.Platform.AlphaFormat.Premul,
                bitmapdata.Scan0,
                new Avalonia.PixelSize(bitmapdata.Width, bitmapdata.Height),
                new Avalonia.Vector(96, 96),
                bitmapdata.Stride);

            bitmap.UnlockBits(bitmapdata);
            bitmap.Dispose();

            return avaloniaBitmap;
        }
    }

}