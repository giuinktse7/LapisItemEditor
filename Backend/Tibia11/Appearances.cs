using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Google.Protobuf;

namespace Backend.Tibia11
{
    using Proto = Tibia.Protobuf;

    public class Appearances
    {

        private class CatalogEntry
        {
            [JsonPropertyName("type")]
            public string Type { get; set; } = "";
            [JsonPropertyName("file")]
            public string File { get; set; } = "";
            [JsonPropertyName("spritetype")]
            public uint SpriteType { get; set; }
            [JsonPropertyName("firstspriteid")]
            public uint Firstspriteid { get; set; }
            [JsonPropertyName("lastspriteid")]
            public uint Lastspriteid { get; set; }
            [JsonPropertyName("area")]
            public uint Area { get; set; }
        }


        public TextureAtlasStore TextureAtlases { get; private set; }

        Proto.Appearances.Appearances appearances;
        string appearancesFileName;

        public AppearanceData AppearanceData { get; } = new AppearanceData();

        public string AssetDirectory { get; set; }

        uint TextureAtlasWidth { get; set; } = 384;
        uint TextureAtlasHeight { get; set; } = 384;

        uint TileSizePx { get; set; } = 32;

        public Appearances(string assetDirectory)
        {
            this.AssetDirectory = assetDirectory;
        }

        public void Load(System.Progress<int>? reporter)
        {
            string catalogPath = Path.Combine(AssetDirectory, "catalog-content.json");

            string jsonString = File.ReadAllText(catalogPath);
            var catalogEntries = JsonSerializer.Deserialize<List<CatalogEntry>>(jsonString);

            int total = catalogEntries!.Count;

            if (catalogEntries != null)
            {
                Trace.WriteLine($"Count: {catalogEntries.Count}");

                // -1 because we always have at least one non-sprite entry type (type "appearances")
                TextureAtlases = new TextureAtlasStore((uint)catalogEntries.Count - 1);
                int progress = 0;

                int current = 0;
                foreach (var entry in catalogEntries)
                {
                    switch (entry.Type)
                    {
                        case "appearances":
                            appearancesFileName = entry.File;
                            LoadAppearances(entry.File);
                            break;
                        case "sprite":
                            {
                                var atlas = LoadTextureAtlasFromLzma(entry);
                                TextureAtlases.AddTextureAtlas(atlas);
                            }
                            break;
                        default:
                            break;
                    }
                    current += 1;

                    int prevProgress = progress;
                    progress = (int)((float)current / total * 100);

                    if (progress != prevProgress)
                    {
                        (reporter as IProgress<int>)?.Report(progress);
                    }
                }

                // Must be here! Having it sorted makes it possible to use binary search for texture atlas retrieval by sprite ID.
                TextureAtlases.Sort();
                (reporter as IProgress<int>)?.Report(100);
            }
        }

        private TextureAtlas LoadTextureAtlasFromLzma(CatalogEntry entry)
        {
            var buffer = new LZMACompressedBuffer() { buffer = File.ReadAllBytes(Path.Combine(AssetDirectory, entry.File)) };

            return new TextureAtlas(
                buffer,
                TextureAtlasWidth,
                TextureAtlasHeight,
                TileSizePx,
                entry.Firstspriteid,
                entry.Lastspriteid,
                (SpriteType)entry.SpriteType,
                entry.File);
        }

        public void WriteToDisk(string path)
        {
            var filePath = Path.Combine(path, appearancesFileName);

            using (var output = File.Create(filePath))
            {
                appearances.WriteTo(output);
            }
        }


        private void LoadAppearances(string filename)
        {
            string appearancesPath = Path.Combine(AssetDirectory, filename);

            using var input = File.OpenRead(appearancesPath);

            appearances = Proto.Appearances.Appearances.Parser.ParseFrom(input);


            AppearanceData.Objects.SetItemCount((uint)appearances.Object.Count);

            foreach (var objectAppearance in appearances.Object)
            {
                uint clientId = objectAppearance.Id;
                var appearance = new Appearance(objectAppearance);
                AppearanceData.Objects.Add(clientId, appearance);
            }
        }

    }
}