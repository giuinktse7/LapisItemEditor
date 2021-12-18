using System;

namespace Backend.Tibia11
{
    public class Tibia11VersionData : VersionData
    {
        public Tibia11VersionData(ClientVersion version) : base(version)
        {
            if ((uint)version < 1100)
            {
                throw new ArgumentException("The Tibia11 namespace only accepts client versions at or after 11.00 (1100). Use the Tibia7 namespace instead.");
            }
        }

        public override string ToString()
        {
            return this.Description;
        }

        protected override DatFormat VersionValueToDatFormat()
        {
            return DatFormat.Format_1100;
        }
    }
}
