using Serilog;
using Telegram.Bot;
using VoiceRecognitionBot;
using VoiceRecognitionBot.CognitiveService;
using VoiceRecognitionBot.Common;
using VoiceRecognitionBot.Infrastructure;
using VoiceRecognitionBot.Utils;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting up");
try
{
    builder.Host.UseSerilog((ctx, lc) => lc
        .WriteTo.Console()
        .ReadFrom.Configuration(ctx.Configuration));

    builder.ConfigureServices();

    var app = builder.Build();
    //var logger = app.Services.GetRequiredService<ILogger<Program>>();
    app.UseSerilogRequestLogging();


    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        Log.Information("Starting application is Development mode");

        app.UseSwagger();
        app.UseSwaggerUI();
    }
    else
    {
        Log.Information("Starting application is Prod mode");
    }

    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();

    app.Run();

}
catch (Exception ex)
{
    Log.Fatal(ex, "Unhandled exception");
}
finally
{
    Log.Information("Shut down complete");
    Log.CloseAndFlush();
}
