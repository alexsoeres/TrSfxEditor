using System;
using System.Collections.Generic;
using System.Linq;

namespace TrSfxLib
{
    public static class TrSfxConverter
    {
        public static WaveFile Convert(WaveFile waveFile, WaveFile.WaveFormat desiredFormat)
        {
            if (WaveFile.WaveFormat.WaveFormatComparer.Equals(waveFile.Format, desiredFormat))
            {
                return waveFile; //already same format
            }
            
            List<Sample> samples = GetSamples(waveFile);
            WaveFile.WaveFormat currentFormat = waveFile.Format;
            
            if (desiredFormat.SampleRate < currentFormat.SampleRate)
            {
                samples = ReduceSampleRate(samples, currentFormat, desiredFormat);
            }
            else if (desiredFormat.SampleRate > currentFormat.SampleRate)
            {
                samples = ExpandSampleRate(samples, currentFormat, desiredFormat);
            }

            if (desiredFormat.Channels < waveFile.Format.Channels)
            {
                samples = ReduceChannels(samples, desiredFormat);
            } 
            else if (desiredFormat.Channels > waveFile.Format.Channels)
            {
                samples = ExpandChannels(desiredFormat, samples);
            }

            byte[] newDataPayload = samples.SelectMany(x => x.Serialize()).ToArray();

            return new WaveFile(desiredFormat, new WaveFile.WaveData(newDataPayload));
        }

        private static List<Sample> ExpandChannels(WaveFile.WaveFormat desiredFormat, List<Sample> samples)
        {
            List<Sample> result = samples
                .Select(x => new Sample(x.Channels.Expand(desiredFormat.Channels, x.Channels[0])))
                .ToList();
            return result;
        }

        private static List<Sample> ReduceChannels(List<Sample> samples, WaveFile.WaveFormat desiredFormat)
        {
            List<Sample> result = samples
                .Select(x => new Sample(x.Channels.SubArray(0, desiredFormat.Channels)))
                .ToList();
            return result;
        }

        private static List<Sample> ReduceSampleRate(List<Sample> samples, WaveFile.WaveFormat currentFormat, WaveFile.WaveFormat desiredFormat)
        {
            int samplesPerSample = (int) (currentFormat.SampleRate / desiredFormat.SampleRate);
            List<Sample> reduced = new List<Sample>();
            for (int i = 0; i < samples.Count; i += samplesPerSample)
            {
                reduced.Add(samples[i]);
            }

            return reduced;
        }

        private static List<Sample> ExpandSampleRate(List<Sample> samples, WaveFile.WaveFormat currentFormat, WaveFile.WaveFormat desiredFormat)
        {
            int repeatsPerSample = (int) (desiredFormat.SampleRate / currentFormat.SampleRate);
            List<Sample> expanded = new List<Sample>();
            foreach (Sample sample in samples)
            {
                for (int repeats = 0; repeats < repeatsPerSample; repeats++)
                {
                    expanded.Add(sample);
                }
            }

            return expanded;
        }

        private static List<Sample> GetSamples(WaveFile waveFile)
        {
            if (waveFile.Format.BitsPerSample != 16)
            {
                throw new NotSupportedException("BitsPerSample must be 16");
            }
            
            int bytesPerSample = 2 * waveFile.Format.Channels;
                
            List<Sample> samples = new List<Sample>();
            for (int offset = 0; offset + (bytesPerSample - 1) < waveFile.Data.Data.Length; offset += bytesPerSample)
            {
                byte[] input = waveFile.Data.Data;
                
                byte[] sampleData = input.SubArray(offset, bytesPerSample);

                List<short> channels = new List<short>();
                for (int c = 0; c < waveFile.Format.Channels; c++)
                {
                    byte[] channelData = sampleData.SubArray(c * 2, 2);
                    
                    channels.Add(BitConverter.ToInt16(channelData));
                }
                samples.Add(new Sample(channels.ToArray()));
            }

            return samples;
        }

        private static T[] SubArray<T>(this T[] input, int offset, int length)
        {
            T[] o = new T[length];
            Array.Copy(input, offset, o, 0, length);
            return o;
        }
        
        
        private static T[] Expand<T>(this T[] input, int length, T emptyValue)
        {
            T[] o = Enumerable.Repeat(emptyValue, length).ToArray();
            Array.Copy(input, 0, o, 0, input.Length);
            return o;
        }
    }
}