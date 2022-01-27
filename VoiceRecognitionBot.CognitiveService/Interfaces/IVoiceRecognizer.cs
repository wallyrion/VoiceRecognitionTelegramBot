namespace VoiceRecognitionBot.CognitiveService.Interfaces;

public interface IVoiceRecognizer
{
    Task<string> RecognizeTextFromWavFile(string fileName);
}