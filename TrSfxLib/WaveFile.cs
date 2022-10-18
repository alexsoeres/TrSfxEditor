using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrSfxLib
{
    public class WaveFile
    {
        private static readonly byte[] RiffChunkId = new byte[] { 0x52, 0x49, 0x46, 0x46 };
        private static readonly byte[] WaveId = new byte[] { 0x57, 0x41, 0x56, 0x45 };
        public WaveFormat Format { get; private set; }
        public WaveData Data { get; private set; }

        public WaveFile(WaveFormat format, WaveData data)
        {
            Format = format;
            Data = data;
        }

        public byte[] Serialize()
        {
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                byte[] formatChunk = Format.Serialize();
                byte[] dataChunk = Data.Serialize();
                uint riffChunkSize = (uint)WaveId.Length + (uint)formatChunk.Length + (uint)dataChunk.Length;

                writer.Write(RiffChunkId);
                writer.Write(riffChunkSize);
                writer.Write(WaveId);
                writer.Write(formatChunk);
                writer.Write(dataChunk);
                return stream.ToArray();
            }
        }

        public class WaveFormat
        {
            private sealed class WaveFormatEqualityComparer : IEqualityComparer<WaveFormat>
            {
                public bool Equals(WaveFormat x, WaveFormat y)
                {
                    if (ReferenceEquals(x, y)) return true;
                    if (ReferenceEquals(x, null)) return false;
                    if (ReferenceEquals(y, null)) return false;
                    if (x.GetType() != y.GetType()) return false;
                    return x.Channels == y.Channels && x.SampleRate == y.SampleRate && x.BitsPerSample == y.BitsPerSample;
                }

                public int GetHashCode(WaveFormat obj)
                {
                    return HashCode.Combine(obj.Channels, obj.SampleRate, obj.BitsPerSample);
                }
            }

            public static IEqualityComparer<WaveFormat> WaveFormatComparer { get; } = new WaveFormatEqualityComparer();

            public static readonly WaveFormat Tr2Format = new WaveFormat(11025, 16, 1);
            public static readonly WaveFormat Tr3Format = new WaveFormat(22050, 16, 1);

            public static readonly byte[] FmtChunkId = new byte[] { 0x66, 0x6D, 0x74, 0x20 };
            public const uint FmtChunkSize = 16;
            public const ushort FormatCode = 1;                                               //2 (10)

            public ushort Channels { get; private set; }                                        //4 (12)

            public uint SampleRate { get; private set; }                                        //8 (16)
            public uint AvgBytesPerSecond => (uint)(Channels * (BitsPerSample / 8) * SampleRate); //12 (20)

            public ushort BlockAlign => (ushort)((ushort)(BitsPerSample / 8) * Channels);       //14 (22)

            public ushort BitsPerSample { get; private set; }                                   //16 (24)

            public WaveFormat(uint sampleRate, ushort bitsPerSample, ushort channels)
            {
                Channels = channels;
                BitsPerSample = bitsPerSample;
                SampleRate = sampleRate;
            }

            public byte[] Serialize()
            {
                using MemoryStream stream = new MemoryStream();
                using BinaryWriter writer = new BinaryWriter(stream);
                
                writer.Write(FmtChunkId);
                writer.Write(FmtChunkSize);
                writer.Write(FormatCode);
                writer.Write(Channels);
                writer.Write(SampleRate);
                writer.Write(AvgBytesPerSecond);
                writer.Write(BlockAlign);
                writer.Write(BitsPerSample);
                return stream.ToArray();
            }
        }
        public class WaveData
        {
            private static readonly byte[] DATA_CHUNK_ID = new byte[] { 0x64, 0x61, 0x74, 0x61 };

            public uint DataChunkSize => (uint)Data.Length;

            public byte[] Data { get; set; }

            public WaveData(byte[] data)
            {
                Data = data;
            }

            public byte[] Serialize()
            {
                using (MemoryStream stream = new MemoryStream())
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(DATA_CHUNK_ID);
                    writer.Write(DataChunkSize);
                    writer.Write(Data);
                    if (DataChunkSize % 2 == 1) writer.Write((byte)0x0); //add padding byte if chunkSize odd
                    return stream.ToArray();
                }
            }
        }
    }
}
