using FastNotes.Api.Infrastructure.Config;
using FastNotes.Api.Services;

namespace FastNotes.Api.Infrastructure;

using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Microsoft.Extensions.Options;

public class TelegramBotService
{
    private readonly ITelegramBotClient _botClient;
    private readonly IServiceProvider _services;
    private readonly string _token;

    public TelegramBotService(IOptions<DatabaseSettings> options, IServiceProvider services)
    {
        _services = services;
        _token = options.Value.ProtectionKeysConnectionString;
        _botClient = new TelegramBotClient(_token);
    }

    public void Start()
    {
        var cts = new CancellationTokenSource();
        _botClient.StartReceiving(
            HandleUpdateAsync,
            HandleErrorAsync,
            new ReceiverOptions { AllowedUpdates = Array.Empty<UpdateType>() },
            cancellationToken: cts.Token
        );

        Console.WriteLine("TelegramBot запущен");
    }

    private async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken cancellationToken)
    {
        if (update.Type != UpdateType.Message || update.Message?.Voice == null)
            return;

        var message = update.Message;

        var file = await bot.GetFileAsync(message.Voice.FileId, cancellationToken);
        var stream = new MemoryStream();
        var url = $"https://api.telegram.org/file/bot{_token}/{file.FilePath}";

        using var http = new HttpClient();
        using var voiceStream = await http.GetStreamAsync(url, cancellationToken);
        await voiceStream.CopyToAsync(stream, cancellationToken);
        stream.Position = 0;

        using var scope = _services.CreateScope();
        var taskProcessor = scope.ServiceProvider.GetRequiredService<TelegramTaskProcessor>();

        string recognizedText = "[распознанный текст из голосового]";
        

        var task = await taskProcessor.CreateTaskFromTextAsync(recognizedText,
            message.Chat.Username ?? "telegram-user");

        await bot.SendTextMessageAsync(
            message.Chat.Id,
            $"✅ Задача сохранена: {task.Title}",
            cancellationToken: cancellationToken
        );
    }

    private Task HandleErrorAsync(ITelegramBotClient bot, Exception exception, CancellationToken token)
    {
        Console.WriteLine($"❌ Telegram error: {exception.Message}");
        return Task.CompletedTask;
    }
}