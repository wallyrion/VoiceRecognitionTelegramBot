using Microsoft.AspNetCore.Mvc;

namespace VoiceRecognitionBot.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class VoiceRecognitionBotController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly ILogger<VoiceRecognitionBotController> _logger;
        private readonly BotFramework _botFramework;
        public VoiceRecognitionBotController(ILogger<VoiceRecognitionBotController> logger, BotFramework botFramework)
        {
            _logger = logger;
            _botFramework = botFramework;
        }

        [HttpGet("Subscribe")]
        public async Task<ActionResult> SubscribeToBot()
        {
            try
            {
                await _botFramework.SubscribeToBot();
                return Ok();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Can not subscribe to bot");
                return StatusCode(500);
            }
        }

        [HttpGet("Unsubscribe")]
        public async Task<ActionResult> UnSubscribeFromBot()
        {
            try
            {
                await _botFramework.UnSubscribeFromBot();
                return Ok();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Can not unsubscribe from bot");
                return StatusCode(500);
            }
        }
    }
}