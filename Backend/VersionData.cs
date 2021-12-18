using System;

namespace Backend
{
    public enum DatFormat : uint
    {
        InvalidFormat = 0,
        Format_710 = 710,
        Format_740 = 740,
        Format_755 = 755,
        Format_780 = 780,
        Format_860 = 860,
        Format_960 = 960,
        Format_1010 = 1010,
        Format_1050 = 1050,
        Format_1057 = 1057,
        Format_1092 = 1092,
        Format_1093 = 1093,
        Format_1100 = 1100,
    }

    public abstract class VersionData
    {
        public VersionData(ClientVersion version, string? description = null)
        {
            this.Version = version;
            this.Description = string.IsNullOrEmpty(description) ? $"Client {(uint)version / 100}.{(uint)version % 100}" : description;
            this.Format = VersionValueToDatFormat();
        }

        public ClientVersion Version { get; set; }

        public string Description { get; private set; }

        public DatFormat Format { get; private set; }

        public override string ToString()
        {
            return this.Description;
        }

        protected abstract DatFormat VersionValueToDatFormat();
    }
}
