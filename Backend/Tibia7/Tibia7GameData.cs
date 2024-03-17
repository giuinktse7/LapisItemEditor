using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Security.Cryptography;

namespace Backend.Tibia7
{
    ///<summary> Game data using tibia.dat and tibia.spr</summary>
    public class Tibia7GameData : GameData
    {
        private AppearanceData appearanceData = new AppearanceData();

        private SprData sprData;
        private DatIO datData;
        private OtbData? otbData;

        private string datPath;
        private string sprPath;
        private ClientFeatures features;


        public VersionData Version { get; private set; }
        public OtbData? OtbData { get => otbData; private set { otbData = value; } }

        public void Load(Progress<int>? reporter = null)
        {
            sprData = SprData.Load(sprPath, (Tibia7VersionData)Version, features);
            datData = DatIO.Load(datPath, (Tibia7VersionData)Version, features, appearanceData);
        }

        public Tibia7GameData(Tibia7VersionData version, string datPath, string sprPath, ClientFeatures features = ClientFeatures.None)
        {
            Version = version;
            this.datPath = datPath;
            this.sprPath = sprPath;
            this.features = features;

            if (features == ClientFeatures.None || features == ClientFeatures.Transparency)
            {
                features |= version.Format >= DatFormat.Format_755 ? ClientFeatures.PatternZ : features;
                features |= version.Format >= DatFormat.Format_960 ? ClientFeatures.Extended : features;
                features |= version.Format >= DatFormat.Format_1050 ? ClientFeatures.FrameDurations : features;
                features |= version.Format >= DatFormat.Format_1057 ? ClientFeatures.FrameGroups : features;
            }

            if (blankRGBSprite == null)
            {
                CreateBlankRgbSprite();
            }
        }

        public void CreateNewOtb()
        {
            int clientVersion = 760; // TODO What should the default be?
            uint majorVersion = 1; // TODO What should the default be?
            uint minorVersion = 1; // 1 TODO What should the default be?
            uint buildNumber = 1; // 1 TODO What should the default be?
            otbData = OtbData.Create(clientVersion, majorVersion, minorVersion, buildNumber);
        }

        public void LoadOtb(string path)
        {
            otbData = OtbData.Load(path, this);
        }

        public Appearance CreateObject(uint clientId)
        {
#if DEBUG
            if (appearanceData.Objects.Has(clientId))
            {
                throw new ArgumentException($"An ItemType with client ID {clientId} already exists.");
            }
#endif

            var apperance = new Appearance(clientId);
            appearanceData.Objects.Add(clientId, apperance);

            return apperance;
        }

        public Bitmap? GetSpriteBitmap(uint spriteId)
        {
            return sprData.getSpriteBitmap(spriteId);
        }

        public void dumpSpriteBitmaps(Appearance appearance, string path)
        {
            var clientId = appearance.ClientId;
            var fg = appearance.Data.FrameGroup[0];
            if (fg == null)
            {
                return;
            }

            var spriteIds = fg.SpriteInfo.SpriteId;

            var count = 1;
            if (appearance.SpriteTileWidth == 2 && appearance.SpriteTileHeight == 2)
            {
                count = 4;
            }
            else if (appearance.SpriteTileWidth == 2 || appearance.SpriteTileHeight == 2)
            {
                count = 2;
            }

            for (int i = 0; i < count; ++i)
            {
                var bitmap = GetSpriteBitmap(spriteIds[i]);
                bitmap?.Save(Path.Combine(path, $"tibia7_{clientId}_{i}.png"));
            }
        }

        public Sprite? GetSprite(uint spriteId)
        {
            return sprData.GetSprite(spriteId);
        }

        public void WriteOtb(string path)
        {
            if (otbData == null)
            {
                throw new NullReferenceException("There is no loaded OTB data.");
            }

            otbData.Write(path, this);
        }

        public uint CreateMissingItems()
        {
            throw new NotImplementedException();
            if (otbData == null)
            {
                throw new NullReferenceException("There is no loaded/created OTB.");
            }

            return 0;

            // otbData.CreateMissingItems(itemData.GetItemTypes);
        }

        public uint LastItemTypeClientId
        {
            get => datData.LastItemTypeClientId;
        }

        public uint LastItemTypeServerId => otbData?.LastServerId ?? 0;


        private string emptyHash = Convert.ToHexString(new byte[16]);

        private static byte[] blankRGBSprite = null;
        private void CreateBlankRgbSprite()
        {
            blankRGBSprite = new byte[Sprite.RGBPixelsDataSize];
            for (int i = 0; i < Sprite.RGBPixelsDataSize; i++)
            {
                blankRGBSprite[i] = 0x11;
            }
        }

        public byte[] ComputeSpriteHashAndReturnRgbaData(ThingType thingType)
        {
            string? storedSpriteHash = thingType.SpriteHash != null ? Convert.ToHexString(thingType.SpriteHash) : null;

            MD5 md5 = MD5.Create();
            int spriteBase = 0;
            MemoryStream stream = new MemoryStream();
            byte[] rgbaData = new byte[Sprite.PixelsDataSize];

            var fg = thingType.DefaultFrameGroup;

            for (byte l = 0; l < fg.Layers; l++)
            {
                for (byte h = 0; h < fg.Height; h++)
                {
                    for (byte w = 0; w < fg.Width; w++)
                    {
                        int index = spriteBase + w + h * fg.Width + l * fg.Width * fg.Height;
                        Sprite? sprite = GetSprite(fg.SpriteIDs[index]);

                        byte[]? rgbData = sprite?.GetRGBData();
                        if (rgbData == null)
                        {
                            rgbData = blankRGBSprite;
                        }

                        // reverse rgb
                        for (int y = 0; y < Sprite.DefaultSize; ++y)
                        {
                            for (int x = 0; x < Sprite.DefaultSize; ++x)
                            {
                                rgbaData[128 * y + x * 4 + 0] = rgbData[(32 - y - 1) * 96 + x * 3 + 2]; // blue
                                rgbaData[128 * y + x * 4 + 1] = rgbData[(32 - y - 1) * 96 + x * 3 + 1]; // green
                                rgbaData[128 * y + x * 4 + 2] = rgbData[(32 - y - 1) * 96 + x * 3 + 0]; // red
                                rgbaData[128 * y + x * 4 + 3] = 0;
                            }
                        }

                        stream.Write(rgbaData, 0, Sprite.PixelsDataSize);
                    }
                }
            }

            stream.Position = 0;
            var result = stream.ToArray();

            thingType.SpriteHash = md5.ComputeHash(stream);
            if (storedSpriteHash != null)
            {
                var newHash = Convert.ToHexString(thingType.SpriteHash);
                if (!(storedSpriteHash.Equals(newHash) || storedSpriteHash.Equals(emptyHash)))
                {
                    Trace.WriteLine($"Different hash for clientID: {thingType.ClientId}");
                }
            }
            // var k = Convert.ToHexString(thingType.SpriteHash);
            return result;
        }


        public byte[] ComputeSpriteHashAndReturnRgbaData(Appearance apperance)
        {
            string? storedSpriteHash = apperance.SpriteHash != null ? Convert.ToHexString(apperance.SpriteHash) : null;

            MD5 md5 = MD5.Create();
            int spriteBase = 0;
            MemoryStream stream = new MemoryStream();
            byte[] rgbaData = new byte[Sprite.PixelsDataSize];

            var fg = apperance.Data.FrameGroup[0];
            var spriteInfo = fg.SpriteInfo;

            byte width = apperance.SpriteTileWidth;
            byte height = apperance.SpriteTileHeight;

            for (byte l = 0; l < spriteInfo.Layers; l++)
            {
                for (byte h = 0; h < apperance.SpriteTileHeight; h++)
                {
                    for (byte w = 0; w < width; w++)
                    {
                        int index = spriteBase + w + h * width + l * width * height;
                        Sprite? sprite = GetSprite(spriteInfo.SpriteId[index]);

                        byte[]? rgbData = sprite?.GetRGBData();
                        if (rgbData == null)
                        {
                            rgbData = blankRGBSprite;
                        }

                        // reverse rgb
                        for (int y = 0; y < Sprite.DefaultSize; ++y)
                        {
                            for (int x = 0; x < Sprite.DefaultSize; ++x)
                            {
                                rgbaData[128 * y + x * 4 + 0] = rgbData[(32 - y - 1) * 96 + x * 3 + 2]; // blue
                                rgbaData[128 * y + x * 4 + 1] = rgbData[(32 - y - 1) * 96 + x * 3 + 1]; // green
                                rgbaData[128 * y + x * 4 + 2] = rgbData[(32 - y - 1) * 96 + x * 3 + 0]; // red
                                rgbaData[128 * y + x * 4 + 3] = 0;
                            }
                        }

                        stream.Write(rgbaData, 0, Sprite.PixelsDataSize);
                    }
                }
            }

            stream.Position = 0;
            var result = stream.ToArray();

            apperance.SpriteHash = md5.ComputeHash(stream);
            if (storedSpriteHash != null)
            {
                var newHash = Convert.ToHexString(apperance.SpriteHash);
                if (!(storedSpriteHash.Equals(newHash) || storedSpriteHash.Equals(emptyHash)))
                {
                    Trace.WriteLine($"Different hash for clientID: {apperance.ClientId}");
                }
            }
            // var k = Convert.ToHexString(thingType.SpriteHash);
            return result;
        }

        public void ComputeSpriteHash(Appearance appearance)
        {
            ComputeSpriteHashAndReturnRgbaData(appearance);
        }

        public Appearance? GetItemTypeByClientId(uint clientId)
        {
            return appearanceData.Objects.Get(clientId);
        }

        public Appearance? GetItemTypeByServerId(uint serverId)
        {
            if (otbData == null)
            {
                throw new NullReferenceException("OTB is not loaded.");
            }

            if (otbData.ServerIdToClientId.TryGetValue(serverId, out uint clientId))
            {
                return GetItemTypeByClientId(clientId);
            }
            else
            {
                return null;
            }
        }

        public Appearance GetOrCreateItemTypeByClientId(uint clientId)
        {
            Appearance? appearance = appearanceData.Objects.Get(clientId);
            if (appearance == null)
            {
                appearance = new Appearance(clientId);
                appearanceData.Objects.Add(clientId, appearance);
            }

            return appearance;
        }

        private Bitmap? GetFirstSpriteBitmap(Appearance appearance)
        {
            var bitmap = GetSpriteBitmap(appearance.GetDefaultSpriteId());
            if (bitmap == null)
            {
                return null;
            }

            return bitmap;
        }

        public Bitmap? GetItemTypeBitmap(Appearance appearance)
        {
            var fg = appearance.Data.FrameGroup[0];
            int size = Sprite.DefaultSize;

            var width = appearance.SpriteTileWidth;
            var height = appearance.SpriteTileHeight;

            int pixelWidth = (int)width * size;
            int pixelHeight = (int)height * size;

            Bitmap? bitmap;

            // Take the default sprite for 32x32 ItemType and Wrappable ItemType
            if ((width == 1 && height == 1) || appearance.Data.Flags.Wrap)
            {
                return GetFirstSpriteBitmap(appearance);
            }

            bitmap = new Bitmap(pixelWidth, pixelHeight, PixelFormat.Format32bppArgb);
            var rect = new Rectangle(0, 0, pixelWidth, pixelHeight);

            for (byte layer = 0; layer < fg.SpriteInfo.Layers; layer++)
            {
                for (byte widthIndex = 0; widthIndex < width; widthIndex++)
                {
                    for (byte heightIndex = 0; heightIndex < height; heightIndex++)
                    {
                        int index = width * (heightIndex + layer * height) + widthIndex;
                        int px = (width - widthIndex - 1) * size;
                        int py = (height - heightIndex - 1) * size;

                        var spriteId = fg.SpriteInfo.SpriteId[index];

                        var spriteBitmap = GetSpriteBitmap(spriteId);
                        if (spriteBitmap == null)
                        {
                            if (width != height)
                            {
                                return GetFirstSpriteBitmap(appearance);
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

        public void Dispose()
        {
            sprData.Dispose();
        }

        public void WriteClientData(string path)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Appearance> Objects => throw new NotImplementedException();
    }
}