using System.IO;
using Backend.Tibia7;

namespace Backend.IO
{
    public class BinaryDataWriter : BinaryWriter
    {
        #region | Constructor |

        public BinaryDataWriter(Stream output) : base(output)
        {
            ////
        }

        #endregion

        #region | Public Methods |

        public void WriteU32(uint value)
        {
            base.Write(value);
        }

        public void Write32(int value)
        {
            base.Write(value);
        }

        public void WriteU16(ushort value)
        {
            base.Write(value);
        }

        public void WriteByte(byte value)
        {
            base.Write(value);
        }

        public void WriteSignedByte(sbyte value)
        {
            base.Write(value);
        }

        public void WriteByte(ThingTypeAttribute attribute)
        {
            base.Write((byte)attribute);
        }

        public void WriteBytes(byte[] buffer)
        {
            base.Write(buffer);
        }

        #endregion
    }
}
