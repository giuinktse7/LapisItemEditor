
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LapisItemEditor
{
    public class ClientVersion
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("version")]
        public int Version { get; set; }
    }

    public class Config
    {
        public static Config instance = new Config();

        [JsonPropertyName("clientVersions")]
        public List<ClientVersion> ClientVersions { get; set; } = new List<ClientVersion>();

        [JsonPropertyName("majorOtbVersions")]
        public List<uint> MajorOtbVersions { get; set; } = new List<uint>();

        public static void LoadFromFile(string path)
        {
            string rawJson = File.ReadAllText(path);
            var config = JsonSerializer.Deserialize<Config>(rawJson);
            if (config != null)
            {
                Config.instance = config;
            }
            else
            {
                Trace.WriteLine("Could not load config.");
            }
        }

        public void AddClientVersion(int version)
        {
            ClientVersion? found = Config.instance.ClientVersions.Find(x => x.Version == version);
            if (found != null) { return; }

            if (found == null)
            {
                int majorClientVersion = (int)version / 100;
                int minorClientVersion = (int)version - majorClientVersion * 100;
                string name = $"{majorClientVersion}.{minorClientVersion}";

                var otbClientVersion = new ClientVersion() { Name = name, Version = (int)version };
                Config.instance.ClientVersions.Add(otbClientVersion);
                Config.instance.ClientVersions = Config.instance.ClientVersions.OrderBy(x => x.Version).ToList();

            }
        }

        public void addMajorOtbVersion(uint version)
        {
            var found = Config.instance.MajorOtbVersions.Contains(version);
            if (found) { return; }

            Config.instance.MajorOtbVersions.Add(version);
        }
    }
}
