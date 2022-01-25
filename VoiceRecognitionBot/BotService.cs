namespace VoiceRecognitionBot;

public class BotService : IHostedService
{
    private readonly ILogger<BotService> _logger;
    private readonly BotFramework _bot;

    public BotService(BotFramework bot, ILogger<BotService> logger)
    {
        _bot = bot;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await CheckStatusTask(cancellationToken);
        await _bot.SubscribeToBot();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _bot.UnSubscribeFromBot();
    }

    private async Task CheckStatusTask(CancellationToken cancellationToken)
    {
        var _ = Task.Run(async () =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation($"Current date : {DateTime.UtcNow.ToShortDateString()}");
                var a = TimeSpan.FromMinutes(1).TotalMilliseconds;
                await Task.Delay((int)a);
            }
        }, cancellationToken);
    }
}