using System;

using SevenZip;
using System.IO;
using SevenZip.Compression.LZMA;

namespace Backend.Tibia11
{
    /// <summary>
    /// LZMA file format spec for reference:
    /// https://svn.python.org/projects/external/xz-5.0.3/doc/lzma-file-format.txt
    /// </summary>
    public static class LZMA
    {
        private const int TibiaBmpHeaderSizeBytes = 122;

        // CIP asset LZMA files start with a 32 byte header before the LZMA-compressed data.
        private static byte[] TibiaHeaderRegion = new byte[32];
        private static int TibiaHeaderRegionSize = TibiaHeaderRegion.Length;
        private static readonly byte[] TibiaHeaderConstant = { 0x70, 0x0A, 0xFA, 0x80, 0x24 };

        #region LZMA Config
        /// For more info on lc, lp, pb: https://www.manpagez.com/man/1/lzma/

        // (1 << 25) = 2^25 = 33554432 (33 MB)
        private const int dictionarySize = 1 << 25;

        // Also called lc (literal context) 
        private const int literalContextBits = 3;

        // Also called lp (literal position)
        private const int literalPositionBits = 0;

        // Also called pb (position bits)
        private const int positionStateBits = 2;

        private const int algorithm = 2;

        /// Controls compression ratio.
        /// 32 - moderate compression
        /// 128 - extreme compression
        private const int numFastBytes = 32;

        // BinTree 4
        private const string matchFinder = "bt4";

        private const bool writeEndMarker = false;

        private static readonly CoderPropID[] propIDs =
        {
            CoderPropID.DictionarySize,
            CoderPropID.LitContextBits,
            CoderPropID.LitPosBits,
            CoderPropID.PosStateBits,
            CoderPropID.Algorithm,
            CoderPropID.NumFastBytes,
            CoderPropID.MatchFinder,
            CoderPropID.EndMarker
        };

        private static readonly object[] properties = {
                dictionarySize,
                literalContextBits,
                literalPositionBits,
                positionStateBits,
                algorithm,
                numFastBytes,
                matchFinder,
                writeEndMarker
            };

        #endregion


        public static byte[] Decompress(byte[] bytes, int bytesForPixelData)
        {
            // BMP header size for a Tibia texture atlas, plus the bytes for the texture atlas pixels
            int outSize = TibiaBmpHeaderSizeBytes + bytesForPixelData;

            using var input = new MemoryStream(bytes);
            using var output = new MemoryStream(outSize);

            /*
                CIP's header, always 32 (0x20) bytes.

                Header format:
                [0x00, X):          A variable number of NULL (0x00) bytes. The amount of pad-bytes can vary depending on how many
                                    bytes the "7-bit integer encoded LZMA file size" take.

                [X, X + 0x05):      The constant byte sequence [0x70 0x0A 0xFA 0x80 0x24]
                [X + 0x05, 0x20]:   LZMA file size (Note: excluding the 32 bytes of this header) encoded as a 7-bit integer
            */

            // Since there may be a variable number of NULL bytes just loop until we get through them all
            while (input.ReadByte() == 0) ;

            // Skip past the remaining 4 bytes of the constant (0x0A 0xFA 0x80 0x24)
            input.Position += 4;

            /*
                LZMA file size encoded as a 7-bit integer.
                The MSB is used as a flag. If MSB is set, then the next byte is included in the LZMA file size.
            */
            while (((byte)input.ReadByte() & 0x80) == 0x80) ;

            var decoder = new Decoder();

            // Read LZMA decoder properties
            byte[] decoderProperties = new byte[5];
            input.Read(decoderProperties, 0, 5);
            decoder.SetDecoderProperties(decoderProperties);

            // These 8 bytes should contain the decompressed size but CIP writes the compressed size.
            // We can skip it because we already know the outSize.
            input.Position += 8;

            decoder.Code(input, output, input.Length - input.Position, outSize, null);

            return output.GetBuffer();
        }

        // From: https://github.com/Arch-Mina/Assets-Editor/blob/main/Assets%20Editor/LZMA.cs
        public static byte[] Compress(byte[] bmpTextureAtlas)
        {
            using var inStream = new MemoryStream(bmpTextureAtlas);
            using var outStream = new MemoryStream();

            // Write the Tibia header region at start of file (filled in after LZMA compression is done)
            outStream.Write(TibiaHeaderRegion);

            var encoder = new SevenZip.Compression.LZMA.Encoder();

            encoder.SetCoderProperties(propIDs, properties);
            encoder.WriteCoderProperties(outStream);

            // CIP Writes the compressed size instead of the uncompressed size.
            // We write these 8 bytes after encoding
            var compressedSizePos = outStream.Position;
            outStream.Position += 8;

            encoder.Code(inStream, outStream, -1, -1, null);

            /*
                Uncompressed Size is stored as unsigned 64-bit little endian integer. A special value of
                0xFFFF_FFFF_FFFF_FFFF indicates that Uncompressed Size is unknown. End of Stream Marker (*)
                is used if and only if Uncompressed Size is unknown.

                (*) Some tools use the term End of Payload (EOP) marker
                instead of End of Stream Marker.

                Note: CIP instead writes the size of the LZMA-compressed bytes here. We subtract 45 bytes for the data that
                is not LZMA-compressed:
                - 32 bytes from Tibia header
                - 13 bytes from LZMA header (5 bytes for properties, 8 bytes for this value)
            */
            var compressedSize = writeEndMarker ? -1 : outStream.Length - 45;

            var pos = outStream.Position;
            outStream.Position = compressedSizePos;

            for (int i = 0; i < 8; i++)
            {
                byte value = (byte)(compressedSize >> (i * 8));
                outStream.WriteByte(value);
            }
            outStream.Position = pos;

            var tempStream = new MemoryStream();
            var writer = new BinaryWriter(tempStream);

            writer.Write(TibiaHeaderConstant);
            writer.Write7BitEncodedInt((int)outStream.Length - TibiaHeaderRegionSize);

            outStream.Position = TibiaHeaderRegionSize - tempStream.Length;

            outStream.Write(tempStream.ToArray(), 0, (int)tempStream.Length);

            return outStream.ToArray();
        }
    }
}
