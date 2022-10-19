using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TrSfxLib
{
    public static class TrSfxReader
    {
        public static void Main(string[] args)
        {
            Random rand = new Random();
            List<WaveFile> shit = ReadSfx(@"D:\SteamLibrary\steamapps\common\Tomb Raider (II)\data\MAIN.SFX");

            using (FileStream outSfx = File.OpenWrite(@"D:\SteamLibrary\steamapps\common\Tomb Raider (II)\data\DESPACITO.SFX"))
            using (BinaryWriter writer = new BinaryWriter(outSfx))
            {
                foreach (WaveFile bushit in shit.Select(x => new { Shit = x, Rand = rand.Next() }).OrderBy(x => x.Rand).Select(x => x.Shit))
                {
                    writer.Write(bushit.Serialize());
                }
            }
        }

        public static List<WaveFile> ReadSfx(string path)
        {
            List<WaveFile> waveFiles = new List<WaveFile>();

            if (!File.Exists(path)) throw new IOException($"SFX file not found (\"{path}\")");
            using (FileStream file = File.OpenRead(path))
            using (BinaryReader reader = new BinaryReader(file))
            {
                while (ReadNext(reader, out WaveFile waveFile))
                {
                    waveFiles.Add(waveFile);
                }
            }
            return waveFiles;
        }

        public static WaveFile ReadWav(string path)
        {
            if (!File.Exists(path)) throw new IOException($"WAV file not found (\"{path}\")");
            
            using FileStream file = File.OpenRead(path);
            using BinaryReader reader = new BinaryReader(file);
            if (ReadNext(reader, out WaveFile waveFile))
            {
                return waveFile;
            } else
            {
                throw new Exception("Empty WAV file!");
            }
        }

        private static bool ReadNext(BinaryReader reader, out WaveFile waveFile)
        {
            try
            {
                //check chunk id RIFF
                if (reader.ReadByte() != 0x52 ||
                    reader.ReadByte() != 0x49 ||
                    reader.ReadByte() != 0x46 ||
                    reader.ReadByte() != 0x46)
                    throw new Exception("Failure to read next sound effect: \"RIFF\" expected at beginning of file.");

                //get chunksize
                uint chunkSize = reader.ReadUInt32();

                //check waveid
                if (reader.ReadByte() != 0x57 ||
                    reader.ReadByte() != 0x41 ||
                    reader.ReadByte() != 0x56 ||
                    reader.ReadByte() != 0x45)
                    throw new Exception("Failure to read next sound effect: \"WAVE\" expected at position 4 of current file.");

                //check fmt chunkid
                if (reader.ReadByte() != 0x66 ||
                    reader.ReadByte() != 0x6D ||
                    reader.ReadByte() != 0x74 ||
                    reader.ReadByte() != 0x20)
                    throw new Exception("Failure to read next sound effect: \"fmt \" expected at position 8 of current file.");

                //check fmt chunksize
                if (reader.ReadUInt32() != 16)
                    throw new Exception("Failure to read next sound effect: fmt chunksize of 16 expected at position 12 of current file.");

                //check pcm tag
                if (reader.ReadUInt16() != 1)
                    throw new Exception("Failure to read next sound effect: format tag <> pcm at position 16");

                //get format data
                ushort channels = reader.ReadUInt16();
                uint samplesPerSec = reader.ReadUInt32();
                reader.ReadUInt32(); //skipped: avg bytes per second
                reader.ReadUInt16(); //skipped: block align
                ushort bitsPerSample = reader.ReadUInt16();

                WaveFile.WaveFormat format = new WaveFile.WaveFormat(samplesPerSec, bitsPerSample, channels);

                // handle LIST
                byte firstByteList = reader.ReadByte();
                byte firstByteData;
                bool hasList = firstByteList == 0x4c;
                uint listChunkSize = 0;
                if (hasList)
                {
                    if (reader.ReadByte() != 0x49 ||
                        reader.ReadByte() != 0x53 ||
                        reader.ReadByte() != 0x54)
                        throw new Exception("Failure to read next sound effect: \"LIST\" expected at position 36.");
                    listChunkSize = reader.ReadUInt32();
                    reader.ReadBytes((int)listChunkSize); //skip
                    
                    firstByteData = reader.ReadByte();
                }
                else
                {
                    firstByteData = firstByteList;
                }
                    
                //check data chunkid
                if (firstByteData != 0x64 ||
                    reader.ReadByte() != 0x61 ||
                    reader.ReadByte() != 0x74 ||
                    reader.ReadByte() != 0x61)
                {
                    int pos = hasList ? (36 + 8 + (int) listChunkSize) : 36;
                    throw new Exception($"Failure to read next sound effect: \"data\" expected at position {pos}.");
                }

                //read data
                uint dataChunkSize = reader.ReadUInt32();
                WaveFile.WaveData data = new WaveFile.WaveData(reader.ReadBytes((int)dataChunkSize));

                waveFile = new WaveFile(format, data);
                return true;
            }
            catch(EndOfStreamException)
            {
                waveFile = null;
                return false;
            }
        }
    }
}
