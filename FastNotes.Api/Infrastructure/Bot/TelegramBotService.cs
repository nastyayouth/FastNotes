using FastNotes.Api.Infrastructure.Config;
using FastNotes.Api.Infrastructure.Whisper;
using FastNotes.Api.Services;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Microsoft.Extensions.Options;

namespace FastNotes.Api.Infrastructure.Bot;

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
        if (message.Voice != null)
        {
            await HandleVoiceMessageAsync(bot, update,cancellationToken);
        }
        else if (message.Text != null && message.Text.StartsWith("/"))
        {
            await HandleCommandAsync(bot, message, cancellationToken);
        }
    }

    private async Task HandleVoiceMessageAsync(ITelegramBotClient bot, Update update, CancellationToken cancellationToken)
    {
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

        var whisper = scope.ServiceProvider.GetRequiredService<WhisperService>();
        var recognizedText = await whisper.TranscribeAsync(stream);
        

        var task = await taskProcessor.CreateTaskFromTextAsync(recognizedText,
            message.Chat.Username ?? "telegram-user");

        await bot.SendTextMessageAsync(
            message.Chat.Id,
            $"Задача сохранена: {task.Title}",
            cancellationToken: cancellationToken
        );
    }

    private async Task HandleCommandAsync(ITelegramBotClient bot, Message message, CancellationToken token)
    {
        var command = message.Text!.Trim().ToLower();

        switch (command)
        {
            case "/start":
            case "/help":
                await bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: " Доступные команды:\n" +
                          "/list – показать все задачи\n" +
                          "/today – задачи на сегодня\n" +
                          "/help – справка",
                    cancellationToken: token
                );
                break;

            case "/list":
            {
                using var scope = _services.CreateScope();
                var taskService = scope.ServiceProvider.GetRequiredService<TaskService>();
                var tasks = await taskService.GetAllAsync();

                var messageText = tasks.Any()
                    ? string.Join("\n\n", tasks.Select(t =>
                        $"{t.Title}\n До:{t.DueDate:d}"))
                    : "Пока нет задач.";
                await bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: messageText,
                    cancellationToken: token
                );
                break;
            }
            case "/today":
            {
                using var scope = _services.CreateScope();
                var taskService = scope.ServiceProvider.GetRequiredService<TaskService>();
                var tasks = await taskService.GetTodayAsync();

                var messageText = tasks.Any()
                    ? string.Join("\n\n", tasks.Select(t =>
                        $"{t.Title}\n До:{t.DueDate:d}"))
                    : "На сегодня нет задач.";
                await bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: messageText,
                    cancellationToken: token
                );
                break;
            }
            default:
                await bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: " Неизвестная команда. Напиши /help",
                    cancellationToken: token
                );
                break;
        }
    }

    private Task HandleErrorAsync(ITelegramBotClient bot, Exception exception, CancellationToken token)
    {
        Console.WriteLine($"Telegram error: {exception.Message}");
        return Task.CompletedTask;
    }
}