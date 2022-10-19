using System;
using System.IO;
using System.Linq;
using TrSfxLib;

namespace TrSfx
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Commands: \n" +
                                  "unpack <main.sfx> <folder>\n" +
                                  "pack <folder> <main.sfx>\n" +
                                  "convert <Tr2/Tr3> <in.wav> <out.wav>\n" +
                                  "convert-all <Tr2/Tr3> <folder>\n" +
                                  "check <in.wav>");
            }
            switch (args[0])
            {
                case "unpack":
                    Unpack(args[1], args[2]);
                    break;
                case "pack":
                    Pack(args[1], args[2]);
                    break;
                case "convert":
                    Convert(Enum.Parse<TrVersion>(args[1]), args[2], args[3]);
                    break;
                case "convert-all":
                    ConvertAll(Enum.Parse<TrVersion>(args[1]), args[2]);
                    break;
                case "check":
                    Check(args[1]);
                    break;
            }
        }

        private static void Unpack(string mainSfxPath, string targetFolderPath)
        {
            Console.WriteLine("Reading SFX file...");
            var waveFiles = TrSfxReader.ReadSfx(mainSfxPath);

            Console.WriteLine("Creating target folder...");
            DirectoryInfo targetDirectory = Directory.CreateDirectory(targetFolderPath);

            Console.WriteLine("Exporting wave files...");
            int soundId = 1;
            foreach (WaveFile waveFile in waveFiles)
            {
                Console.WriteLine($"> {soundId}.wav");
                WriteWaveFile(waveFile, targetDirectory, soundId++);
            }
            
            Console.WriteLine("Done.");
        }

        private static void Pack(string sourceFolderPath, string mainSfxPath)
        {
            Console.WriteLine("Getting wav files...");
            var wavFilePaths = Directory.GetFiles(sourceFolderPath)
                .Where(x => x.ToLower().EndsWith(".wav"))
                .Select(x => new { FileName = x, SoundId = GetSoundId(x)})
                .Where(x => x.SoundId > -1)
                .OrderBy(x => x.SoundId)
                .Select(x => x.FileName).ToList();
            Console.WriteLine($"Found {wavFilePaths.Count} files");

            Console.WriteLine("Importing sounds...");
            var files = wavFilePaths.Select(TrSfxReader.ReadWav).ToList();
            
            Console.WriteLine("Writing sfx file");
            using FileStream outStream = File.Create(mainSfxPath);
            foreach (WaveFile wavFile in files)
            {
                outStream.Write(wavFile.Serialize());
            }
            
            Console.WriteLine("Done.");
        }

        private static void Convert(TrVersion trVersion, string inPath, string outPath)
        {
            WaveFile.WaveFormat format = trVersion switch
            {
                TrVersion.Tr2 => WaveFile.WaveFormat.Tr2Format,
                TrVersion.Tr3 => WaveFile.WaveFormat.Tr3Format,
                _ => throw new NotImplementedException($"{trVersion} not implemented")
            };
            
            Console.WriteLine("Reading wave file...");
            WaveFile file = TrSfxReader.ReadWav(inPath);

            Console.WriteLine($"Converting to {trVersion} format...");
            WaveFile converted = TrSfxConverter.Convert(file, format);
            
            Console.WriteLine($"Exporting File...");
            WriteWaveFile(converted, outPath);
            
            Console.WriteLine("Done.");
        }
        
        private static void ConvertAll(TrVersion trVersion, string folderPath)
        {
            WaveFile.WaveFormat format = trVersion switch
            {
                TrVersion.Tr2 => WaveFile.WaveFormat.Tr2Format,
                TrVersion.Tr3 => WaveFile.WaveFormat.Tr3Format,
                _ => throw new NotImplementedException($"{trVersion} not implemented")
            };
            
            Console.WriteLine("Getting wav files...");
            var wavFilePaths = Directory.GetFiles(folderPath)
                .Where(x => x.ToLower().EndsWith(".wav"))
                .Select(x => new { FileName = x, SoundId = GetSoundId(x)})
                .Where(x => x.SoundId > -1)
                .OrderBy(x => x.SoundId).ToList();
            Console.WriteLine($"Found {wavFilePaths.Count} files");
            
            Console.WriteLine("Importing sounds...");
            var files = wavFilePaths
                .Select(x => x.FileName)
                .Select(x => new { FileName = x, File = TrSfxReader.ReadWav(x) })
                .Where(x => !WaveFile.WaveFormat.WaveFormatComparer.Equals(x.File.Format, format)).ToList();
            
            Console.WriteLine($"Converting {files.Count} files...");
            foreach (var wavFile in files.Select(x => new { x.FileName, File = TrSfxConverter.Convert(x.File, format)}))
            {
                using FileStream outStream = File.Create(wavFile.FileName);
                outStream.Write(wavFile.File.Serialize());
            }
            
            Console.WriteLine("Done.");
        }

        private static void Check(string inPath)
        {
            WaveFile waveFile = TrSfxReader.ReadWav(inPath);
            
            Console.WriteLine($"FormatTag:      PCM");
            Console.WriteLine($"Channels:       {waveFile.Format.Channels}");
            Console.WriteLine($"SampleRate:     {waveFile.Format.SampleRate}");
            Console.WriteLine($"AvgBytesPerSec: {waveFile.Format.AvgBytesPerSecond}");
            Console.WriteLine($"BlockAlign:     {waveFile.Format.BlockAlign}");
            Console.WriteLine($"BitsPerSample:  {waveFile.Format.BitsPerSample}");
        }

        private static void WriteWaveFile(WaveFile waveFile, DirectoryInfo targetDirectory, int soundId)
        {
            WriteWaveFile(waveFile, Path.Combine(targetDirectory.FullName, $"{soundId}.wav"));
        }
        
        private static void WriteWaveFile(WaveFile waveFile, String path)
        {
            using FileStream file = File.Create(path);
            file.Write(waveFile.Serialize());
        }

        private static int GetSoundId(string fileName)
        {
            string namePart = new FileInfo(fileName).Name.Split(".")[0];
            if (Int32.TryParse(namePart, out int number))
            {
                return number;
            }
            else
            {
                return -1;
            }
        }
    }
}