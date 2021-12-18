using System.IO;
using System.Text;
using Backend.Tibia7;

namespace Backend.Otb
{

    public class OtbWriter
    {
        private BinaryWriter writer;
        public OtbWriter(BinaryWriter writer)
        {
            this.writer = writer;
        }

        public void CloseNode()
        {
            writer.Write((byte)NodeType.End);
        }
        public void StartNode(ServerItemGroup group)
        {
            writer.Write((byte)NodeType.Start);
            WriteU8((byte)group);
        }

        public void WriteOTBMRootV1()
        {
            writer.Write((byte)NodeType.OTBM_RootV1);
        }

        public void WriteAttributeType(OtbItemAttribute attributeType, ushort attributeSize)
        {
            WriteU8((byte)attributeType);
            WriteU16(attributeSize);
        }

        public void WriteU8(byte value)
        {
            if (value == (byte)NodeType.Start || value == (byte)NodeType.End || value == (byte)NodeType.Escape)
            {
                writer.Write((byte)NodeType.Escape);
            }

            writer.Write(value);
        }

        public void WriteU16(ushort value)
        {
            WriteU8((byte)(value));
            WriteU8((byte)(value >> 8));
        }

        public void WriteU32(uint value)
        {
            WriteU8((byte)(value));
            WriteU8((byte)(value >> 8));
            WriteU8((byte)(value >> 16));
            WriteU8((byte)(value >> 24));
        }

        public void WriteRawByte(byte value)
        {
            writer.Write(value);
        }

        public void WriteRawU16(ushort value)
        {
            writer.Write(value);
        }

        public void WriteRawU32(uint value)
        {
            writer.Write(value);
        }

        public void WriteString(string value)
        {
            WriteU16((ushort)value.Length);
            foreach (var b in value)
            {
                WriteU8((byte)b);
            }
        }


        ///<summary>
        ///Dumps the array without a size hint.
        ///</summary>
        public void WriteBytesWithoutSizeHint(byte[] buffer)
        {
            foreach (var b in buffer)
            {
                WriteU8((byte)b);
            }
        }

        ///<summary>
        ///Dumps the array without a size hint.
        ///</summary>
        public void WriteBytesWithoutSizeHint(char[] buffer)
        {
            foreach (var b in buffer)
            {
                WriteU8((byte)b);
            }
        }
    }

}