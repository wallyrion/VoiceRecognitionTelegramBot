using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using VoiceRecognitionBot.Common;
using Xunit;

namespace VoiceRecognitionBot.CognitiveService.Tests
{
    public class VoiceRecognizerTests
    {
        private readonly ILogger<VoiceRecognizer> _logger;
        private readonly IOptions<CognitiveServiceOptions> _options;
        public VoiceRecognizerTests()
        {
            _logger = Substitute.For<ILogger<VoiceRecognizer>>();
            _options = Options.Create<CognitiveServiceOptions>(new CognitiveServiceOptions
            {
                AzureCognitiveRegion = "westeurope",
                AzureCognitiveServiceId = "9fef83c82e5d42b3bf36aaa0af6555b9"
            });

        }

        [Fact]
        public async Task VoiceRecognizer_Should_Correctly_Recognize_English_Sample_File()
        {
            var recognizer = new VoiceRecognizer(_logger, _options);

            var text = await recognizer.RecognizeTextFromWavFile("nice-work.wav");

            Assert.Equal("Nice work.", text);
        }

        [Fact]
        public async Task VoiceRecognizer_Should_Correctly_Recognize_Russian_Sample_File()
        {
            var recognizer = new VoiceRecognizer(_logger, _options);

            var text = await recognizer.RecognizeTextFromWavFile("russian-sample.wav");

            Assert.Equal("Привет, как у тебя дела?", text);
        }
    }
}