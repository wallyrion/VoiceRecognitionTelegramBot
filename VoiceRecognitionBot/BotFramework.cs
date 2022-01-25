using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using VoiceRecognitionBot.CognitiveService;
using VoiceRecognitionBot.MessageHandlerWorker;
using VoiceRecognitionBot.Utils;
using File = System.IO.File;

namespace VoiceRecognitionBot;

public class BotFramework
{
    private readonly ILogger _logger;
    private readonly TelegramBotClient _botClient;
    private CancellationTokenSource _botCancellationTokenSource = null!;

    private readonly TelegramHelper _telegramHelper;
    private readonly VoiceRecognizer _voiceRecognizer;
    private readonly FileConverter _fileConverter;
    private MessageHandlerWorker<(ITelegramBotClient, Update, CancellationToken)> _messageHandlerWorker;

    public BotFramework(
        ILogger<BotFramework> logger,
        TelegramHelper telegramHelper,
        VoiceRecognizer voiceRecognizer,
        FileConverter fileConverter,
        TelegramBotClient botClient)
    {
        _logger = logger;
        _telegramHelper = telegramHelper;
        _voiceRecognizer = voiceRecognizer;
        _fileConverter = fileConverter;
        _botClient = botClient;
    }

    public async Task SubscribeToBot()
    {
        var me = await _botClient.GetMeAsync();
        _botCancellationTokenSource = new CancellationTokenSource();

        _logger.LogInformation($"Start listening for @{me.Username}");

        var receiverOptions = new ReceiverOptions();

        _messageHandlerWorker = new MessageHandlerWorker<(ITelegramBotClient, Update update, CancellationToken)>(
            MessageHandler,
            e => e.update.Message!.Chat.Id.ToString() + e.update.Message?.From?.Id.ToString(),
            _botCancellationTokenSource.Token, 
            8,
            _logger);

        _botClient.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions, cancellationToken: _botCancellationTokenSource.Token);
    }

    public async Task UnSubscribeFromBot()
    {
        var me = await _botClient.GetMeAsync();

        _logger.LogInformation($"Stop listening for @{me.Username}");

        _botCancellationTokenSource.Cancel();
    }


    Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Message == null)
        {
            _logger.LogError("Unknown Error. Message is null");
            return Task.CompletedTask;
        }

        _messageHandlerWorker.Process((botClient, update, cancellationToken));
        return Task.CompletedTask;
    }

    private async Task MessageHandler((ITelegramBotClient botClient, Update update, CancellationToken token) data)
    {
        var (botClient, update, token) = data;
        if (update.Type != UpdateType.Message)
            return;

        _logger.LogDebug($"New message came {update.Message!.Type}");

        var chatId = update.Message.Chat.Id;

        if (update.Message!.Type == MessageType.Voice)
        {
            var sentInProgressMessage = await botClient.SendTextMessageAsync(
                chatId: chatId, replyToMessageId: update.Message.MessageId,
                text: "\U0001f9bb",
                cancellationToken: token);

            await HandleVoiceAsync(update, sentInProgressMessage);
        }

        return;

        // Only process text messages
        if (update.Message!.Type != MessageType.Text)
            return;

        var messageText = update.Message.Text;

        Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");

        // Echo received message text
        await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "You said:\n" + messageText,
            cancellationToken: token);
    }

    async Task HandleVoiceAsync(Update update, Message sentInProgressMessage)
    {
        try
        {
            var message = update.Message!;
            string fileName = update.Message!.Voice!.FileId;
            var downloadedData = await _telegramHelper.GetFile(fileName);

            var filePath = $"{fileName}.wav";
            await _fileConverter.SaveOgaFileAsWav(downloadedData, filePath);

            var recognizedText = await _voiceRecognizer.RecognizeTextFromWavFile(filePath);

            var chatId = update!.Message!.Chat.Id;

            if (string.IsNullOrEmpty(recognizedText))
            {
                recognizedText = "I can't hear you";
            }
            await _botClient.EditMessageTextAsync(chatId, sentInProgressMessage.MessageId, recognizedText);

            File.Delete($"{fileName}.wav");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Server error while handling voice message");
        }
    }

    // https://api.telegram.org/file/bot5183588482:AAGixx6BM3XyEsOuYOrL_VOJ7FAiNkUMoVc/voice/file_2.oga

    Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        Console.WriteLine(ErrorMessage);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}