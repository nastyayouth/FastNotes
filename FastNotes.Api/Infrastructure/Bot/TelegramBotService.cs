using FastNotes.Api.Infrastructure.Config;
using FastNotes.Api.Infrastructure.Whisper;
using FastNotes.Api.Models;
using FastNotes.Api.Services;
using FastNotes.Shared;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types.ReplyMarkups;

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
        if (update.Type == UpdateType.CallbackQuery)
        {
            await HandleCallbackAsync(bot, update.CallbackQuery, cancellationToken);
        }
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

        var (title, dueDate) = VoiceParser.Parse(recognizedText);
        // Сохраняем черновик
        var drafts = scope.ServiceProvider.GetRequiredService<DraftService>();

        var draft = new TaskDraft
        {
            ChatId = message.Chat.Id,
            RawText = recognizedText,
            Title = title,
            DueDate = dueDate
        };
        drafts.SaveDraft(draft);

        var responseText =
            $"Черновик задачи:\n" +
            $" Название: {draft.Title}\n" +
            $" Срок: {(draft.DueDate.HasValue ? draft.DueDate.Value.ToString("f") : "не указан")}\n\n" +
            $"Изменить или сохранить?";

        await bot.SendTextMessageAsync(
            message.Chat.Id,
            responseText,
            replyMarkup: new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("✅ Оставить как есть", "confirm_draft"),
                    InlineKeyboardButton.WithCallbackData("✏️ Изменить", "edit_draft")
                }
            }),
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
    private async Task HandleCallbackAsync(ITelegramBotClient bot, CallbackQuery callback, CancellationToken token)
    {
        using var scope = _services.CreateScope();
        var drafts = scope.ServiceProvider.GetRequiredService<DraftService>();
        var draft = drafts.GetDraft(callback.Message.Chat.Id);

        if (callback.Data == "confirm_draft" && draft != null)
        {
            var taskService = scope.ServiceProvider.GetRequiredService<TaskService>();
            await taskService.CreateAsync(new TaskDto()
            {
                Title =  draft.Title,
                Description = draft.RawText,
                AssignedTo = null,
                DueDate =  draft.DueDate,
                IsConfirmed = true
            });
            drafts.RemoveDraft(draft.ChatId);

            await bot.SendTextMessageAsync(callback.Message.Chat.Id, "Задача сохранена", cancellationToken: token);
        }
        else if (callback.Data == "edit_draft")
        {
            await bot.SendTextMessageAsync(callback.Message.Chat.Id, 
                "Задача сохранена и доступна в /list", cancellationToken: token);
        }
    }

    private Task HandleErrorAsync(ITelegramBotClient bot, Exception exception, CancellationToken token)
    {
        Console.WriteLine($"Telegram error: {exception.Message}");
        return Task.CompletedTask;
    }
}