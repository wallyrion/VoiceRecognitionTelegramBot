using Concentus.Oggfile;
using Concentus.Structs;
using NAudio.Wave;
using VoiceRecognitionBot.Utils.Interfaces;

namespace VoiceRecognitionBot.Utils;

public class FileConverter : IFileConverter
{
    public async Task SaveOgaFileAsWav(byte[] data, string filePath)
    {
        await using MemoryStream stream = new MemoryStream(data);
        OpusDecoder decoder = OpusDecoder.Create(48000, 1);
        OpusOggReadStream oggIn = new OpusOggReadStream(decoder, stream);
        await using MemoryStream pcmStream = new MemoryStream();

        while (oggIn.HasNextPacket)
        {
            short[] packet = oggIn.DecodeNextPacket();
            if (packet == null)
            {
                continue;
            }

            foreach (var t in packet)
            {
                var bytes = BitConverter.GetBytes(t);
                pcmStream.Write(bytes, 0, bytes.Length);
            }
        }

        pcmStream.Position = 0;
        var wavStream = new RawSourceWaveStream(pcmStream, new WaveFormat(48000, 1));
        var sampleProvider = wavStream.ToSampleProvider();
        WaveFileWriter.CreateWaveFile16(filePath, sampleProvider);
    }
}