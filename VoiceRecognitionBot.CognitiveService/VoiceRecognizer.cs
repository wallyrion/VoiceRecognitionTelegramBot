using System.Text;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VoiceRecognitionBot.Common;

namespace VoiceRecognitionBot.CognitiveService;

public class VoiceRecognizer
{
    private readonly ILogger<VoiceRecognizer> _logger;
    private readonly SpeechConfig _speechConfig;
    private readonly AutoDetectSourceLanguageConfig autoDetectSourceLanguageConfig;

    public VoiceRecognizer(ILogger<VoiceRecognizer> logger, IOptions<CognitiveServiceOptions> options)
    {
        _logger = logger;
        _speechConfig = SpeechConfig.FromSubscription(options.Value.AzureCognitiveServiceId, options.Value.AzureCognitiveRegion);
        autoDetectSourceLanguageConfig = AutoDetectSourceLanguageConfig.FromLanguages(new string[] { "ru-RU", "en-US" });
}

    public async Task<string> RecognizeTextFromWavFile(string fileName)
    {
        using var audioConfig = AudioConfig.FromWavFileInput(fileName);
        using var recognizer = new SpeechRecognizer(_speechConfig, autoDetectSourceLanguageConfig, audioConfig);
        var stopRecognition = new TaskCompletionSource<int>();

        var textBuilder = new StringBuilder();
        recognizer.Recognizing += (s, e) =>
        {
            if (e.Result.Reason == ResultReason.RecognizingSpeech)
            {
                //
            }
        };
        recognizer.Recognized += (s, e) =>
        {
            if (e.Result.Reason == ResultReason.RecognizedSpeech)
            {
                textBuilder.Append(" " + e.Result.Text);
            }
        };

        recognizer.Canceled += (s, e) =>
        {
            if (e.Reason == CancellationReason.Error)
            {
                _logger.LogError($"CANCELED: ErrorCode={e.ErrorCode} ErrorDetails={e.ErrorDetails}");
            }

            stopRecognition.TrySetResult(0);
        };

        recognizer.SessionStopped += (s, e) =>
        {
            stopRecognition.TrySetResult(0);
        };

        await recognizer.StartContinuousRecognitionAsync();
        Task.WaitAny(new Task[] { stopRecognition.Task });
        await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);

        return textBuilder.ToString().Trim();
    }
}

