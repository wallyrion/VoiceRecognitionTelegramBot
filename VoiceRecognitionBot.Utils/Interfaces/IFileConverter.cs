namespace VoiceRecognitionBot.Utils.Interfaces;

public interface IFileConverter
{
    Task SaveOgaFileAsWav(byte[] data, string filePath);
}