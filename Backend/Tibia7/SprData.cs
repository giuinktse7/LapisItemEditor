
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;

namespace Backend.Tibia7
{
    public class SprData : IDisposable
    {
        public ClientFeatures ClientFeatures { get; private set; }
        public Tibia7VersionData Version { get; private set; }
        public string Path { get; private set; }

        public bool IsBusy { get; private set; }

        private Dictionary<uint, Sprite> sprites;

        private const byte HeaderU16 = 6;
        private const byte HeaderU32 = 8;

        private uint rawSpriteCount = 0;
        private uint spriteOffset = 0;


        private FileStream? stream;
        private BinaryReader? reader;

        private SprData(string path, Tibia7VersionData version, ClientFeatures features)
        {
            this.Path = path;
            this.ClientFeatures = features;
            this.Version = version;
            this.sprites = new Dictionary<uint, Sprite>();
        }

        public static SprData Load(string path, Tibia7VersionData version, ClientFeatures features = ClientFeatures.None)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"File not found: {path}", path);
            }

            var data = new SprData(path, version, features);
            data._Load();
            return data;
        }

        public Bitmap? getSpriteBitmap(uint spriteId)
        {
            var sprite = GetSprite(spriteId);
            if (sprite != null)
            {
                return sprite.CreateBitmap();
            }

            return null;
        }

        public Sprite? GetSprite(uint id)
        {
            Debug.Assert(stream != null && reader != null);

            if (sprites.TryGetValue(id, out var sprite))
            {
                return sprite;
            }

            if (id > rawSpriteCount)
            {
                return null;
            }

            if (id == 0)
            {
                return null;
            }

            // Subtract 1 because sprite ids start at 1. A sprite address is 4 bytes
            stream.Position = spriteOffset + ((id - 1) * 4);
            uint spriteAddress = reader.ReadUInt32();

            if (spriteAddress == 0)
            {
                return null;
            }

            stream.Position = spriteAddress;

            // Skip R, G, B that represent the transparency color (usually if not always Magenta)
            reader.ReadByte();
            reader.ReadByte();
            reader.ReadByte();

            ushort compressedPixelBytes = reader.ReadUInt16();
            var compressedPixels = reader.ReadBytes(compressedPixelBytes);

            sprite = new Sprite(id, compressedPixels, ClientFeatures.HasFlag(ClientFeatures.Transparency));
            sprites.Add(id, sprite);
            return sprite;

            //     if (id == 0 || !m_spritesFile)
            //         return nullptr;

            //     m_spritesFile->seek(((id - 1) * 4) + m_spritesOffset);

            //     const uint32 spriteAddress = m_spritesFile->getU32();

            //     // no sprite? return an empty texture
            //     if (spriteAddress == 0)
            //         return nullptr;

            //     m_spritesFile->seek(spriteAddress);

            //     // skip color key
            //     m_spritesFile->getU8();
            //     m_spritesFile->getU8();
            //     m_spritesFile->getU8();

            //     const uint16 pixelDataSize = m_spritesFile->getU16();

            //     ImagePtr image(new Image(Size(SPRITE_SIZE, SPRITE_SIZE)));

            //     uint8* pixels = image->getPixelData();
            //     int writePos = 0;
            //     int read = 0;
            //     const bool useAlpha = g_game.getFeature(Otc::GameSpritesAlphaChannel);
            //     const uint8 channels = useAlpha ? 4 : 3;
            //     // decompress pixels
            //     while (read < pixelDataSize && writePos < SPRITE_DATA_SIZE)
            //     {
            //         const uint16 transparentPixels = m_spritesFile->getU16();
            //         const uint16 coloredPixels = m_spritesFile->getU16();
            //         if (!image->hasTransparentPixel())
            //             image->setTransparentPixel(transparentPixels > 0);
            //         for (int i = 0; i < transparentPixels && writePos < SPRITE_DATA_SIZE; ++i)
            //         {
            //             pixels[writePos + 0] = 0x00;
            //             pixels[writePos + 1] = 0x00;
            //             pixels[writePos + 2] = 0x00;
            //             pixels[writePos + 3] = 0x00;
            //             writePos += 4;
            //         }

            //         for (int i = 0; i < coloredPixels && writePos < SPRITE_DATA_SIZE; ++i)
            //         {
            //             pixels[writePos + 0] = m_spritesFile->getU8();
            //             pixels[writePos + 1] = m_spritesFile->getU8();
            //             pixels[writePos + 2] = m_spritesFile->getU8();
            //             pixels[writePos + 3] = useAlpha ? m_spritesFile->getU8() : 0xFF;
            //             writePos += 4;
            //         }

            //         read += 4 + (channels * coloredPixels);
            //     }

            //     // Error margin for 4 pixel transparent
            //     if (writePos + 4 < SPRITE_DATA_SIZE && !image->hasTransparentPixel())
            //         image->setTransparentPixel(true);

            //     // fill remaining pixels with alpha
            //     while (writePos < SPRITE_DATA_SIZE)
            //     {
            //         pixels[writePos + 0] = 0x00;
            //         pixels[writePos + 1] = 0x00;
            //         pixels[writePos + 2] = 0x00;
            //         pixels[writePos + 3] = 0x00;
            //         writePos += 4;
            //     }

            //     return image;
            // }
            // catch (stdext::exception&e) {
            //     g_logger.error(stdext::format("Failed to get sprite id %d: %s", id, e.what()));
            //     return nullptr;
            // }
        }

        private bool _Load()
        {
            if (this.IsBusy)
            {
                Trace.WriteLine("_Load: IsBusy, can not load.");
                return false;
            }

            stream = new FileStream(Path, FileMode.Open);
            reader = new BinaryReader(stream);

            uint signature = reader.ReadUInt32();
            if (signature != Version.SprSignature)
            {
                throw new Exception($"Invalid SPR signature. Expected signature is {Version.SprSignature} and loaded signature is {signature}.");
            }

            rawSpriteCount = ClientFeatures.HasFlag(ClientFeatures.Extended) ? reader.ReadUInt32() : reader.ReadUInt16();
            // rawSpriteCount = reader.ReadUInt16();
            spriteOffset = (uint)stream.Position;

            return true;
        }

        public void Dispose()
        {
            stream?.Close();
            reader?.Close();
        }
    }
}