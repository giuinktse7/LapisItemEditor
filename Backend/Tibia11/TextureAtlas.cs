using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Backend.Util;

namespace Backend.Tibia11
{
    ///<summary>
    /// Sprites larger than one tile need to be offset when drawn to be drawn in the correct position
    ///</summary>
    public struct TileDrawOffset
    {
        public int x;
        public int y;
    };

    public enum SpriteType
    {
        Width1_Height1 = 0,
        Width1_Height2 = 1,
        Width2_Height1 = 2,
        Width2_Height2 = 3
    };

    public struct LZMACompressedBuffer
    {
        public byte[] buffer;
    }


    public class TextureAtlas
    {
        // ARGB (alpha, red, green, blue)
        private const int ColorChannels = 4;

        readonly private int BytesForPixelData;

        public TextureAtlas(LZMACompressedBuffer buffer, uint width, uint height, uint tileSizePx, uint firstSpriteId, uint lastSpriteId, SpriteType spriteType, string sourceFile)
        {
            this.texture = null;
            this.compressedBytes = buffer;
            this.SourceFile = sourceFile;
            this.Width = width;
            this.Height = height;
            this.FirstSpriteId = firstSpriteId;
            this.LastSpriteId = lastSpriteId;
            this.TileSizePx = tileSizePx;

            uint tilesInRow = (width / tileSizePx);
            uint tilesInColumn = (height / tileSizePx);

            this.BytesForPixelData = (int)(width * height * ColorChannels);

            switch (spriteType)
            {
                case SpriteType.Width1_Height1:
                    this.Rows = tilesInRow;
                    this.Columns = tilesInColumn;
                    this.SpriteWidth = tileSizePx;
                    this.SpriteHeight = tileSizePx;
                    drawOffset.x = 0;
                    drawOffset.y = 0;
                    break;
                case SpriteType.Width1_Height2:
                    this.Rows = tilesInRow / 2;
                    this.Columns = tilesInColumn;
                    this.SpriteWidth = tileSizePx;
                    this.SpriteHeight = tileSizePx * 2;
                    drawOffset.x = 0;
                    drawOffset.y = -1;
                    break;
                case SpriteType.Width2_Height1:
                    this.Rows = tilesInRow;
                    this.Columns = tilesInColumn / 2;
                    this.SpriteWidth = tileSizePx * 2;
                    this.SpriteHeight = tileSizePx;
                    drawOffset.x = -1;
                    drawOffset.y = 0;
                    break;
                case SpriteType.Width2_Height2:
                    this.Rows = tilesInRow / 2;
                    this.Columns = tilesInColumn / 2;
                    this.SpriteWidth = tileSizePx * 2;
                    this.SpriteHeight = tileSizePx * 2;
                    drawOffset.x = -1;
                    drawOffset.y = -1;
                    break;
                default:
                    this.Rows = tilesInRow;
                    this.Columns = tilesInColumn;
                    this.SpriteWidth = tileSizePx;
                    this.SpriteHeight = tileSizePx;
                    drawOffset.x = 0;
                    drawOffset.y = 0;
                    break;
            }
        }

        public static TextureAtlas FromFile(string path, uint width, uint height, uint tileSizePx, uint firstSpriteId, uint lastSpriteId, SpriteType spriteType)
        {
            var buffer = new LZMACompressedBuffer() { buffer = File.ReadAllBytes(path) };
            return new TextureAtlas(buffer, width, height, tileSizePx, firstSpriteId, lastSpriteId, spriteType, path);
        }

        public void Decompress()
        {
            var compressedData = compressedBytes?.buffer;
            if (compressedData == null)
            {
                throw new NullReferenceException("No compressed bytes.");
            }

            var result = LZMA.Decompress(compressedData, BytesForPixelData);
            texture = new Texture(Width, Height, result);

            // Don't keep the compressed bytes in memory
            compressedBytes = null;
        }

        public void CompressToFile(string path)
        {
            var compressed = Compress();

            File.WriteAllBytes(path, compressed.buffer);
        }

        public LZMACompressedBuffer Compress()
        {
            var compressedData = compressedBytes?.buffer;
            if (compressedBytes != null)
            {
                return (LZMACompressedBuffer)compressedBytes;
            }

            // Texture should never be able to be null here. We always have either a compressed texture or a texture.
            Debug.Assert(texture != null);

            return new LZMACompressedBuffer() { buffer = LZMA.Compress(texture.RawTibiaBmp()) };
        }

        public void WriteToFile(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            File.WriteAllBytes(path, Texture.RawTibiaBmp());
        }

        public void WriteAsPng(string path)
        {
            Texture.Bitmap.Save(path, ImageFormat.Png);
        }

        public Bitmap? GetSprite(uint spriteId)
        {
            if (spriteId < FirstSpriteId || spriteId > LastSpriteId)
            {
                throw new ArgumentOutOfRangeException($"sprite ID {spriteId} is out of range (Range: [{FirstSpriteId},{LastSpriteId}]).");
            }

            int index = (int)(spriteId - FirstSpriteId);
            int x = (int)((index % Columns) * SpriteWidth);
            int y = (int)((index / Columns) * SpriteHeight);

            // Very slow
            // var bitmap = Texture.Bitmap.Clone(new Rectangle(x, y, (int)SpriteWidth, (int)SpriteHeight), PixelFormat.Format32bppArgb);

            return Texture.Bitmap.Crop(new Rectangle(x, y, (int)SpriteWidth, (int)SpriteHeight));
        }

        public string SourceFile { get; }

        public uint TileSizePx { get; }

        public uint Width { get; }
        public uint Height { get; }

        public uint FirstSpriteId { get; }
        public uint LastSpriteId { get; }

        public uint Rows { get; }
        public uint Columns { get; }

        public uint SpriteWidth { get; }
        public uint SpriteHeight { get; }

        private TileDrawOffset drawOffset;
        public TileDrawOffset DrawOffset { get => drawOffset; }

        public Texture Texture
        {
            get
            {
                if (texture == null)
                {
                    Decompress();
                }
                return texture;
            }
        }

        public byte[]? CompressedBytes
        {
            get => compressedBytes?.buffer;
        }

        private LZMACompressedBuffer? compressedBytes;
        private Texture? texture;
    }

    public class TextureAtlasStore
    {
        private int size = 0;
        private TextureAtlas[] textureAtlases;
        public uint FirstSpriteId { get; private set; } = uint.MaxValue;
        public uint LastSpriteId { get; private set; } = uint.MinValue;

        private bool sorted = false;

        public TextureAtlasStore(uint initialCount)
        {
            this.textureAtlases = new TextureAtlas[initialCount];
        }

        public void AddTextureAtlas(TextureAtlas atlas)
        {
            textureAtlases[size++] = atlas;

            FirstSpriteId = Math.Min(FirstSpriteId, atlas.FirstSpriteId);
            LastSpriteId = Math.Max(LastSpriteId, atlas.LastSpriteId);
        }

        public void Sort()
        {
            Array.Resize(ref textureAtlases, size);
            Array.Sort(textureAtlases, (a, b) => a.LastSpriteId.CompareTo(b.LastSpriteId));
            sorted = true;
        }

        public TextureAtlas? GetTextureAtlas(uint spriteId)
        {
            if (spriteId < FirstSpriteId || spriteId > LastSpriteId)
            {
                throw new ArgumentOutOfRangeException($"Sprite id out of range: '{spriteId}', range: [{FirstSpriteId}, {LastSpriteId}]");
            }

            Debug.Assert(sorted);

            int start = 0;
            int end = size - 1;

            while (start <= end)
            {
                int mid = (start + end) / 2;

                if (textureAtlases[mid].LastSpriteId < spriteId)
                {
                    start = mid + 1;
                }
                else
                {
                    end = mid - 1;
                }
            }

            return textureAtlases[start];
        }

        public Bitmap? GetSpriteBitmap(uint spriteId)
        {
            var atlas = GetTextureAtlas(spriteId);
            return atlas?.GetSprite(spriteId);
        }
    }
}