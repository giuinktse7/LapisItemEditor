using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using Backend.Tibia7;

namespace Backend
{
    public static class Backend
    {
        public static GameData? GameData { get; set; }

        public static void Reset()
        {
            GameData?.Dispose();
            GameData = null;
        }


        public static Appearance? GetItemTypeByClientId(uint clientId)
        {
            Debug.Assert(GameData != null);
            return GameData.GetItemTypeByClientId(clientId);
        }

        public static Appearance? GetItemTypeByServerId(uint serverId)
        {
            return GameData?.GetItemTypeByServerId(serverId);
        }

        public static Bitmap? GetSpriteBitmap(uint spriteId)
        {
            Debug.Assert(GameData != null);
            return GameData.GetSpriteBitmap(spriteId);
        }

        public static uint LastItemTypeClientId
        {
            get
            {
                Debug.Assert(GameData != null);
                return GameData.LastItemTypeClientId;
            }
        }

        public static uint LastItemTypeServerId
        {
            get
            {
                Debug.Assert(GameData != null);
                return GameData.LastItemTypeServerId;
            }
        }

        public static Bitmap? GetFirstSpriteBitmap(ThingType itemType)
        {
            var bitmap = GetSpriteBitmap(itemType.GetDefaultSpriteId());
            if (bitmap == null)
            {
                return null;
            }

            return bitmap;
        }

        public static Bitmap? GetItemTypeBitmap(Appearance appearance)
        {
            return GameData?.GetItemTypeBitmap(appearance);
        }

        public static Bitmap? GetItemTypeBitmap(ThingType itemType)
        {
            var fg = itemType.DefaultFrameGroup;
            int size = Sprite.DefaultSize;

            int pixelWidth = (int)fg.Width * size;
            int pixelHeight = (int)fg.Height * size;

            Bitmap? bitmap;

            // Take the default sprite for 32x32 ItemType and Wrappable ItemType
            if ((fg.Width == 1 && fg.Height == 1) || itemType.Wrappable)
            {
                return GetFirstSpriteBitmap(itemType);
            }

            bitmap = new Bitmap(pixelWidth, pixelHeight, PixelFormat.Format32bppArgb);
            var rect = new Rectangle(0, 0, pixelWidth, pixelHeight);

            for (byte layer = 0; layer < fg.Layers; layer++)
            {
                for (byte widthIndex = 0; widthIndex < fg.Width; widthIndex++)
                {
                    for (byte heightIndex = 0; heightIndex < fg.Height; heightIndex++)
                    {
                        int index = fg.Width * (heightIndex + layer * fg.Height) + widthIndex;
                        int px = (fg.Width - widthIndex - 1) * size;
                        int py = (fg.Height - heightIndex - 1) * size;

                        var spriteId = fg.SpriteIDs[index];

                        var spriteBitmap = GetSpriteBitmap(spriteId);
                        if (spriteBitmap == null)
                        {
                            if (fg.Width != fg.Height)
                            {
                                return GetFirstSpriteBitmap(itemType);
                            }
                        }
                        else
                        {
                            using (var graphics = System.Drawing.Graphics.FromImage(bitmap))
                            {
                                graphics.DrawImage(spriteBitmap, px, py);
                            }
                        }
                    }
                }
            }
            // bitmap.MakeTransparent(Color.FromArgb(0x11, 0x11, 0x11));

            return bitmap;
        }

    }

}