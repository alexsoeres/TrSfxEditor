using System.IO;

namespace TrSfxLib
{
    public class Sample
    {
        public short[] Channels { get; }

        public Sample(params short[] channels)
        {
            Channels = channels;
        }

        public byte[] Serialize()
        {
            using MemoryStream stream = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(stream);
            
            foreach (short c in Channels)
            {
                writer.Write(c);
            }
            return stream.ToArray();
        }
    }
}