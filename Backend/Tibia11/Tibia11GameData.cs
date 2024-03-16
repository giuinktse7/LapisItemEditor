using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using Backend.Util;

namespace Backend.Tibia11
{
    public class AppearanceArray
    {
        // Indexed by ClientID - offset
        private float GrowthFactor = 1.5f;
        private Appearance?[] appearances = new Appearance[0];
        private int offset;

        public AppearanceArray(uint offset = 0)
        {
            this.offset = (int)offset;
        }

        public void Add(uint clientId, Appearance itemType)
        {
            if (clientId < offset)
            {
                throw new ArgumentException("clientId must be >= 100.");
            }

            if (clientId - offset >= appearances.Length)
            {
                SetItemCount((uint)Math.Floor(appearances.Length * GrowthFactor));
            }

            appearances[clientId - offset] = itemType;
        }

        public Appearance? Get(uint clientId)
        {
            int index = (int)clientId - offset;
            if (index >= appearances.Length)
            {
                return null;
            }

            return appearances[index];
        }

        public bool Has(uint clientId)
        {
            int index = (int)clientId - offset;

            if (index < 0 || index > appearances.Length)
            {
                return false;
            }

            return appearances[index] != null;
        }

        public IEnumerable<Appearance> Iterator
        {
            get
            {
                foreach (Appearance? appearance in appearances)
                {
                    if (appearance != null)
                    {
                        yield return appearance;
                    }
                }
            }
        }

        public int Length { get => appearances.Length; }

        public uint Count { get => (uint)appearances.Length; }

        public void SetItemCount(uint count)
        {
            Array.Resize(ref appearances, (int)count + 1);
        }
    }


    ///<summary>
    /// Game data using appearances.dat, catalog-content.json, and LZMA-files for sprites
    /// </summary>
    public class Tibia11GameData : GameData
    {
        // 0x11 is used by https://github.com/ottools/ItemEditor
        // We use it here to remain compatible with old sprite hashes.
        private const byte TransparencyColorForRGBData = 0x11;

        private RGBPixel transparencyPixel = new RGBPixel()
        {
            red = TransparencyColorForRGBData,
            green = TransparencyColorForRGBData,
            blue = TransparencyColorForRGBData,
        };

        // private SprData sprData;
        // private DatIO datData;
        private OtbData? otbData;

        public VersionData Version { get; private set; }
        public string AssetDirectory { get; set; }
        public OtbData? OtbData { get => otbData; private set { otbData = value; } }

        private Appearances appearances;


        public Tibia11GameData(VersionData version, string assetDirectory, ClientFeatures features = ClientFeatures.None)
        {
            this.Version = version;
            this.AssetDirectory = assetDirectory;

            appearances = new Appearances(assetDirectory);
            appearances.Load();
        }

        public void CreateNewOtb()
        {
            int clientVersion = 1330;
            uint majorVersion = 3;
            uint minorVersion = 64;
            uint buildNumber = 62;
            otbData = OtbData.Create(clientVersion, majorVersion, minorVersion, buildNumber);
        }

        public void LoadOtb(string path)
        {
            otbData = OtbData.Load(path, this);
        }

        public Appearance GetOrCreateObjectByClientId(uint clientId)
        {
            Appearance? itemType = appearances.AppearanceData.Objects.Get(clientId);
            if (itemType != null)
            {
                return itemType;
            }
            else
            {
                return CreateObject(clientId);
            }
        }

        public Appearance? GetObjectByClientId(uint clientId)
        {
            return appearances.AppearanceData.Objects.Get(clientId);
        }

        public Appearance? GetObjectByServerId(uint serverId)
        {
            if (otbData == null)
            {
                throw new NullReferenceException("OTB is not loaded.");
            }

            if (otbData.ServerIdToClientId.TryGetValue(serverId, out uint clientId))
            {
                return GetObjectByClientId(clientId);
            }
            else
            {
                return null;
            }
        }

        public Bitmap? GetSpriteBitmap(uint spriteId)
        {
            return appearances.TextureAtlases.GetSpriteBitmap(spriteId);
        }


        public Appearance CreateObject(uint clientId)
        {
#if DEBUG
            if (appearances.AppearanceData.Objects.Has(clientId))
            {
                throw new ArgumentException($"An ItemType with client ID {clientId} already exists.");
            }
#endif

            var apperance = new Appearance(clientId);
            appearances.AppearanceData.Objects.Add(clientId, apperance);

            return apperance;
        }

        public void WriteOtb(string path)
        {
            if (otbData == null)
            {
                throw new NullReferenceException("There is no loaded OTB data.");
            }

            otbData.Write(path, this);
        }

        public void WriteClientData(string path)
        {
            appearances.WriteToDisk(path);
        }


        public void CreateMissingItems()
        {
            if (otbData == null)
            {
                throw new NullReferenceException("There is no loaded/created OTB.");
            }

            otbData.CreateMissingItems(this);
        }


        public byte[] ComputeSpriteHashAndReturnRgbaData(Appearance appearance)
        {
            MD5 md5 = MD5.Create();
            MemoryStream stream = new MemoryStream();

            // All sprites are 32x32 in the Tibia7 version of ComputeSpriteHash. We have to crop larger sprites into
            // 32x32 pieces and write each piece separately in order to maintain compatibility.
            ushort PixelsDataSize = 4096; // 32 * 32 * 4
            byte[] rgbaData = new byte[PixelsDataSize];

            var fg = appearance.Data.FrameGroup[0];
            var spriteInfo = fg.SpriteInfo;

            uint patternWidth = spriteInfo.PatternWidth;
            uint patternHeight = spriteInfo.PatternHeight;
            uint patternDepth = spriteInfo.PatternDepth;
            uint layers = spriteInfo.Layers;

            uint spritesPerLayer = patternWidth * patternHeight * patternDepth;

            for (uint layer = 1; layer <= layers; ++layer)
            {
                // First sprite in every layer
                int index = (int)((layer - 1) * spritesPerLayer);
                uint spriteId = appearance.Data.FrameGroup[0].SpriteInfo.SpriteId[index];

                var bitmap = appearances.TextureAtlases.GetSpriteBitmap(spriteId);
                if (bitmap != null)
                {
                    bitmap.GetSpriteHashData(stream);
                }
                else
                {
                    stream.Write(Util.ImageExtensions.BlankARGBSprite, 0, Util.ImageExtensions.BlankARGBSprite.Length);
                }
            }

            stream.Position = 0;
            var data2 = stream.ToArray();

            appearance.SpriteHash = md5.ComputeHash(stream);

            return data2;
        }

        public Appearance? GetItemTypeByClientId(uint clientId)
        {
            return appearances.AppearanceData.Objects.Get(clientId);
        }

        public void ComputeSpriteHash(Appearance appearance)
        {
            if (appearance.SpriteHash != null)
            {
                return;
            }

            MD5 md5 = MD5.Create();
            MemoryStream stream = new MemoryStream();

            // All sprites are 32x32 in the Tibia7 version of ComputeSpriteHash. We have to crop larger sprites into
            // 32x32 pieces and write each piece separately in order to maintain compatibility.
            ushort PixelsDataSize = 4096; // 32 * 32 * 4
            byte[] rgbaData = new byte[PixelsDataSize];

            var fg = appearance.Data.FrameGroup[0];
            var spriteInfo = fg.SpriteInfo;

            uint patternWidth = spriteInfo.PatternWidth;
            uint patternHeight = spriteInfo.PatternHeight;
            uint patternDepth = spriteInfo.PatternDepth;
            uint layers = spriteInfo.Layers;

            uint spritesPerLayer = patternWidth * patternHeight * patternDepth;

            for (uint layer = 1; layer <= layers; ++layer)
            {
                // First sprite in every layer
                int index = (int)((layer - 1) * spritesPerLayer);
                uint spriteId = appearance.Data.FrameGroup[0].SpriteInfo.SpriteId[index];

                var bitmap = appearances.TextureAtlases.GetSpriteBitmap(spriteId);
                if (bitmap != null)
                {
                    bitmap.GetSpriteHashData(stream);
                }
                else
                {
                    stream.Write(Util.ImageExtensions.BlankARGBSprite, 0, Util.ImageExtensions.BlankARGBSprite.Length);
                }
            }

            stream.Position = 0;
            appearance.SpriteHash = md5.ComputeHash(stream);
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
            Appearance? appearance = appearances.AppearanceData.Objects.Get(clientId);
            if (appearance == null)
            {
                appearance = new Appearance(clientId);
                appearances.AppearanceData.Objects.Add(clientId, appearance);
            }

            return appearance;
        }

        public Bitmap? GetItemTypeBitmap(Appearance appearance)
        {
            uint spriteId = appearance.GetDefaultSpriteId();
            return GetSpriteBitmap(spriteId);
        }

        public void Dispose()
        {
            // Empty
        }

        public uint LastItemTypeClientId
        {
            get => throw new NotImplementedException();
        }

        public IEnumerable<Appearance> Objects => appearances.AppearanceData.Objects.Iterator;

        public uint LastItemTypeServerId => otbData?.LastServerId ?? 0;
    }
}