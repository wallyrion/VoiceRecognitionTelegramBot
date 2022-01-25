using System.Net;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using VoiceRecognitionBot.Common;

namespace VoiceRecognitionBot;

public class TelegramHelper
{
    private readonly TelegramBotClient _botClient;
    private readonly ILogger<TelegramHelper> _logger;
    private readonly string _botToken;
    public TelegramHelper(TelegramBotClient botClient, ILogger<TelegramHelper> logger, IOptions<TelegramBotOptions> options)
    {
        _botClient = botClient;
        _logger = logger;
        _botToken = options.Value.Token;
    }

    public async Task<byte[]> GetFile(string fileId)
    {
        var resultFile = await _botClient.GetFileAsync(fileId);
        string pathFile = @$"https://api.telegram.org/file/bot{_botToken}/{resultFile.FilePath}";

        using WebClient wc = new ();
        var downloadedData = wc.DownloadData(pathFile);

        return downloadedData;
    }
}