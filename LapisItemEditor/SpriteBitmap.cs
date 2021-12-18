
using System.Collections.Generic;

namespace LapisItemEditor
{
    public static class ImageStore
    {
        private static Dictionary<uint, Avalonia.Media.Imaging.Bitmap> spriteBitmaps = new Dictionary<uint, Avalonia.Media.Imaging.Bitmap>();

        public static Avalonia.Media.Imaging.Bitmap? GetBitmap(uint spriteId)
        {
            if (spriteBitmaps.TryGetValue(spriteId, out var bitmap))
            {
                return bitmap;
            }

            var spriteBitmap = Backend.Backend.GetSpriteBitmap(spriteId);
            if (spriteBitmap == null)
            {
                return null;
            }

            var newBitmap = ImageExtensions.ConvertToAvaloniaBitmap(spriteBitmap);
            spriteBitmaps.Add(spriteId, newBitmap);
            return newBitmap;
        }
    }
}