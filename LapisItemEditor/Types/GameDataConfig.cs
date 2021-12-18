using Backend;

namespace LapisItemEditor
{
    public abstract class GameDataConfig
    {
        public GameDataConfig(VersionData version, ClientFeatures features = ClientFeatures.None)
        {
            this.Version = version;
            this.Features = features;
        }

        public abstract GameData CreateGameData();

        public VersionData Version { get; }
        public ClientFeatures Features { get; }
    }

    public class Tibia7GameDataConfig : GameDataConfig
    {
        public string TibiaDatPath { get; }
        public string TibiaSprPath { get; }

        public Tibia7GameDataConfig(Backend.Tibia7.Tibia7VersionData version, string datPath, string sprPath, ClientFeatures features = ClientFeatures.None)
            : base(version, features)
        {
            this.TibiaDatPath = datPath;
            this.TibiaSprPath = sprPath;
        }

        public override GameData CreateGameData()
        {
            return new Backend.Tibia7.Tibia7GameData((Backend.Tibia7.Tibia7VersionData)Version, TibiaDatPath, TibiaSprPath, Features);
        }
    }

    public class Tibia11GameDataConfig : GameDataConfig
    {
        public string AssetDirectory { get; }

        public Tibia11GameDataConfig(Backend.Tibia11.Tibia11VersionData version, string assetDirectory, ClientFeatures features = ClientFeatures.None)
            : base(version, features)
        {
            this.AssetDirectory = assetDirectory;
        }

        public override GameData CreateGameData()
        {
            return new Backend.Tibia11.Tibia11GameData((Backend.Tibia11.Tibia11VersionData)Version, AssetDirectory, Features);
        }
    }
}