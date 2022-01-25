using Telegram.Bot;
using VoiceRecognitionBot;
using VoiceRecognitionBot.CognitiveService;
using VoiceRecognitionBot.Common;
using VoiceRecognitionBot.Utils;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
ConfigurationManager configuration = builder.Configuration;

builder.Services.Configure<CognitiveServiceOptions>(configuration.GetSection("CognitiveServiceOptions"));
builder.Services.Configure<TelegramBotOptions>(configuration.GetSection("TelegramBot"));
builder.Services.AddApplicationInsightsTelemetry();

builder.Services.AddSingleton<TelegramBotClient>(p => new TelegramBotClient(configuration.GetSection("TelegramBot:Token").Value));

builder.Services.AddHostedService<BotService>();

builder.Services.AddSingleton<BotFramework>();
builder.Services.AddSingleton<FileConverter>();
builder.Services.AddSingleton<TelegramHelper>();
builder.Services.AddSingleton<VoiceRecognizer>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
