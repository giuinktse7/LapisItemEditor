using System;

namespace Backend.Tibia7
{
    public class Tibia7VersionData : VersionData
    {
        public uint DatSignature { get; private set; }
        public uint SprSignature { get; private set; }

        public Tibia7VersionData(ClientVersion version, string? description, uint datSignature, uint sprSignature) : base(version)
        {
            this.DatSignature = datSignature;
            this.SprSignature = sprSignature;
        }

        public override string ToString()
        {
            return this.Description;
        }


        protected override DatFormat VersionValueToDatFormat()
        {
            uint rawVersion = (uint)Version;

            if (Version == ClientVersion.Invalid)
            {
                return DatFormat.InvalidFormat;
            }

            if (rawVersion < 740)
            {
                return DatFormat.Format_710;
            }
            else if (rawVersion < 755)
            {
                return DatFormat.Format_740;
            }
            else if (rawVersion < 780)
            {
                return DatFormat.Format_755;
            }
            else if (rawVersion < 860)
            {
                return DatFormat.Format_780;
            }
            else if (rawVersion < 960)
            {
                return DatFormat.Format_860;
            }
            else if (rawVersion < 1010)
            {
                return DatFormat.Format_960;
            }
            else if (rawVersion < 1050)
            {
                return DatFormat.Format_1010;
            }
            else if (rawVersion < 1057)
            {
                return DatFormat.Format_1050;
            }
            else if (rawVersion < 1092)
            {
                return DatFormat.Format_1057;
            }
            else if (rawVersion < 1093)
            {
                return DatFormat.Format_1092;
            }
            else if (rawVersion >= 1093)
            {
                return DatFormat.Format_1093;
            }

            return DatFormat.InvalidFormat;
        }
    }
}
