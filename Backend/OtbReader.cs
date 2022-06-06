using System.Text;

namespace Backend.Otb
{

    public class OtbReader
    {
        private long cursor = 0;
        readonly private byte[] buffer;
        public OtbReader(byte[] buffer)
        {
            this.buffer = buffer;
        }

        public bool NodeEnd()
        {
            return buffer[cursor] == (byte)NodeType.End;
        }

        public byte PeekByte()
        {
            if (buffer[cursor] == (byte)NodeType.Escape)
            {
                return buffer[cursor + 1];
            }
            return buffer[cursor];
        }

        public byte NextU8()
        {
            if (buffer[cursor] == (byte)NodeType.Escape)
            {
                ++cursor;
            }

            byte result = buffer[cursor];
            ++cursor;
            return result;
        }

        public ushort NextU16()
        {
            ushort result = 0;
            int shift = 0;
            while (shift < 2)
            {
                if (buffer[cursor] == (byte)NodeType.Escape)
                {
                    ++cursor;
                }

                result += (ushort)(((uint)buffer[cursor]) << (8 * shift));
                ++cursor;
                ++shift;
            }

            return result;
        }

        public uint NextU32()
        {
            uint result = 0;
            int shift = 0;
            while (shift < 4)
            {
                if (buffer[cursor] == (byte)NodeType.Escape)
                {
                    ++cursor;
                }

                result += (uint)(((uint)buffer[cursor]) << (8 * shift));
                ++cursor;
                ++shift;
            }

            return result;
        }

        public string NextString(long length)
        {
            byte[] stringBuffer = new byte[length];
            uint current = 0;
            while (current < length)
            {
                if (buffer[cursor] == (byte)NodeType.Escape)
                {
                    ++cursor;
                }

                stringBuffer[current] = buffer[cursor];
                ++cursor;
                ++current;
            }

            return Encoding.Default.GetString(stringBuffer);
        }


        public byte[] NextBytes(uint bytes)
        {
            byte[] byteBuffer = new byte[bytes];
            uint current = 0;
            while (current < bytes)
            {
                if (buffer[cursor] == (byte)NodeType.Escape)
                {
                    ++cursor;
                }
                byteBuffer[current] = buffer[cursor];
                ++cursor;
                ++current;
            }

            return byteBuffer;
        }

        public void SkipBytes(uint bytes)
        {
            while (bytes > 0)
            {
                if (buffer[cursor] == (uint)NodeType.Escape)
                {
                    ++cursor;
                }
                ++cursor;
                --bytes;
            }
        }
    }

}