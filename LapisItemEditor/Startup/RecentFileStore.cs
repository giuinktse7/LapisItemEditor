// For persisting data to file
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

public class RecentFileStore
{
    public class RecentItem
    {
        [JsonPropertyName("recentAssetFolders")]
        public DateTime Timestamp { get; set; }

        [JsonPropertyName("path")]
        public string Path { get; set; } = "";
    }

    private static readonly int MaxRecentItems = 4;

    [JsonPropertyName("recentAssetFolders")]
    public List<RecentItem> RecentAssetFolders { get; set; } = new List<RecentItem>();

    [JsonPropertyName("recentOtbFiles")]
    public List<RecentItem> RecentOtbFiles { get; set; } = new List<RecentItem>();

    public RecentFileStore() { }

    public void AddAssetFolder(DateTime accessedAt, string path)
    {
        if (Directory.Exists(path))
        {
            var found = RecentAssetFolders.Find(x => x.Path.Equals(path));
            if (found != null)
            {
                found.Timestamp = accessedAt;
            }
            else
            {
                RecentAssetFolders.Add(new RecentItem() { Timestamp = accessedAt, Path = path });
            }
        }

        RecentAssetFolders = RecentAssetFolders.OrderByDescending(x => x.Timestamp).Take(MaxRecentItems).ToList();
    }

    public void AddOtbFile(DateTime accessedAt, string path)
    {
        if (File.Exists(path))
        {
            var found = RecentOtbFiles.Find(x => x.Path.Equals(path));
            if (found != null)
            {
                found.Timestamp = accessedAt;
            }
            else
            {
                RecentOtbFiles.Add(new RecentItem() { Timestamp = accessedAt, Path = path });
            }
        }

        RecentOtbFiles = RecentOtbFiles.OrderByDescending(x => x.Timestamp).Take(MaxRecentItems).ToList();
    }

    public void SaveToFile(string path)
    {
        string json = JsonSerializer.Serialize(this);
        File.WriteAllText(path, json);
    }

    public static RecentFileStore LoadFromFile(string path)
    {
        if (!File.Exists(path))
        {
            return new RecentFileStore();
        }

        string recentDataString = File.ReadAllText(path);
        return JsonSerializer.Deserialize<RecentFileStore>(recentDataString) ?? new RecentFileStore();
    }
}