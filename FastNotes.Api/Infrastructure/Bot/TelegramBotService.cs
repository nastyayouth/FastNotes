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
    private readonly  string _token;

    public TelegramBotService(
        IOptions<TelegramBotSettings> options,
        IServiceProvider services)
    {
        _services = services;

        _token = options.Value.Token;

        if (string.IsNullOrWhiteSpace(_token))
            throw new InvalidOperationException("Telegram bot token is missing");

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

        Console.WriteLine("TelegramBot started");
    }

    private async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken cancellationToken)
    {
        if (update.Type == UpdateType.CallbackQuery)
        {
            await HandleCallbackAsync(bot, update.CallbackQuery, cancellationToken);
        }
        if (update.Type == UpdateType.Message && update.Message?.Text != null)
        {
            using var scope = _services.CreateScope();
            var editStateService = scope.ServiceProvider.GetRequiredService<EditStateService>();
            var session = editStateService.GetState(update.Message.Chat.Id);

            if (session is { Field: not EditField.None })
            {
                await HandleEditResponseAsync(update.Message, session.Field, session.DraftId, scope);
                editStateService.ClearState(update.Message.Chat.Id);
                return;
            }
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

        var stream = await DownloadVoiceStreamAsync(bot, message.Voice.FileId, cancellationToken);
        using var scope = _services.CreateScope();
        var recognizedText = await TranscribeVoiceAsync(scope, stream);
        var draft = ParseToDraft(message.Chat.Id, recognizedText);
        SaveDraft(scope, draft);

        await SendDraftPreviewAsync(bot, message.Chat.Id, draft, cancellationToken);
    }

    private async Task<Stream> DownloadVoiceStreamAsync(ITelegramBotClient bot, string fileId, CancellationToken token)
    {
        var file = await bot.GetFileAsync(fileId, token);
        var stream = new MemoryStream();
        var url = $"https://api.telegram.org/file/bot{_token}/{file.FilePath}";

        using var http = new HttpClient();
        using var voiceStream = await http.GetStreamAsync(url, token);
        await voiceStream.CopyToAsync(stream, token);
        stream.Position = 0;

        return stream;
    }

    private async Task<string> TranscribeVoiceAsync(IServiceScope scope, Stream stream)
    {
        var whisper = scope.ServiceProvider.GetRequiredService<WhisperService>();
        return await whisper.TranscribeAsync(stream);
    }
    
    private  TaskDraft ParseToDraft(long chatId, string text)
    {
        var (title, dueDate, assignedTo) = VoiceParser.Parse(text);
        return  new TaskDraft
        {
            Id = Guid.NewGuid(),
            ChatId = chatId,
            RawText = text,
            Title = title,
            DueDate = dueDate,
            AssignedTo = assignedTo
        };
    }

    private void SaveDraft(IServiceScope scope, TaskDraft draft)
    {
        var drafts = scope.ServiceProvider.GetRequiredService<DraftService>();
        drafts.SaveDraft(draft);
    }

    private async Task SendDraftPreviewAsync(
        ITelegramBotClient bot, long chatId, TaskDraft draft, CancellationToken token)
    {
        var responseText =
            $"Task draft\n" +
            $"Title: {draft.Title}\n" +
            $"Due date: {(draft.DueDate.HasValue ? draft.DueDate.Value.ToString("f") : "not specified")}\n\n" +
            $"Assignee: {draft.AssignedTo ?? "not specified"}\n" +
            $"Edit or save?";

        await bot.SendTextMessageAsync(
            chatId,
            responseText,
            replyMarkup: new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("✅ Keep as is", "confirm_draft"),
                    InlineKeyboardButton.WithCallbackData("✏️ Edit", $"edit_draft:{draft.Id}")
                }
            }),
            cancellationToken: token
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
                    text: " Available commands:\n" +
                          "/list – show all tasks\n" +
                          "/today – tasks for today\n" +
                          "/help – help",
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
                        $"{t.Title}\n Until:{t.DueDate:d}"))
                    : "No tasks yet.";
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
                        $"{t.Title}\n Until:{t.DueDate:d}"))
                    : "No tasks for today.";
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
                    text: " Unknown command. Type /help",
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

        if (callback.Data.StartsWith("confirm_draft"))
        {
            var taskService = scope.ServiceProvider.GetRequiredService<TaskService>();
            await taskService.CreateAsync(new TaskDto()
            {
                Title =  draft.Title,
                Description = draft.RawText,
                AssignedTo = draft.AssignedTo ?? "not specified",
                DueDate =  draft.DueDate!.Value.ToUniversalTime(),
                IsConfirmed = true
            });
            drafts.RemoveDraft(draft.ChatId);

            await bot.SendTextMessageAsync(callback.Message.Chat.Id, "Task has been saved", cancellationToken: token);
        }
        else if (callback.Data.StartsWith("edit_draft:"))
        {
            var parts = callback.Data.Split(':');
            if (parts.Length == 2 && Guid.TryParse(parts[1], out var draftId))
            {
                var editService = scope.ServiceProvider.GetRequiredService<EditStateService>();
                editService.SetState(callback.Message.Chat.Id, EditField.None, draftId); // Пока поле неизвестно

                await bot.SendTextMessageAsync(
                    callback.Message.Chat.Id,
                    "What would you like to edit?",
                    replyMarkup: new InlineKeyboardMarkup(new[]
                    {
                        new[] { InlineKeyboardButton.WithCallbackData("✏️ Title", $"edit_title:{draftId}") },
                        new[] { InlineKeyboardButton.WithCallbackData("📅 Due Date", $"edit_due:{draftId}") },
                        new[] { InlineKeyboardButton.WithCallbackData("👤 Assignee", $"edit_assigned:{draftId}") },
                    }),
                    cancellationToken: token
                );
                return;
            }
        }
        else if (callback.Data.StartsWith("edit_"))
        {
            var parts = callback.Data.Split(':');
            if (parts.Length == 2 && Guid.TryParse(parts[1], out var draftId))
            {
                var editService = scope.ServiceProvider.GetRequiredService<EditStateService>();

                var field = parts[0] switch
                {
                    "edit_title" => EditField.Title,
                    "edit_due" => EditField.DueDate,
                    "edit_assigned" => EditField.AssignedTo,
                    _ => EditField.None
                };

                editService.SetState(callback.Message.Chat.Id, field, draftId);

                var prompt = field switch
                {
                    EditField.Title => "Enter a new task title:",
                    EditField.DueDate => "Enter a new date and time (e.g. 2025-08-01 14:00):",
                    EditField.AssignedTo => "Enter a new assignee:",
                    _ => "Enter a value:"
                };

                await bot.SendTextMessageAsync(callback.Message.Chat.Id, prompt, cancellationToken: token);
                return;
            }
        }

    }
    
    private async Task HandleEditResponseAsync(Message message, EditField field, Guid draftId, IServiceScope scope)
    {
        var drafts = scope.ServiceProvider.GetRequiredService<DraftService>();
        var editStateService = scope.ServiceProvider.GetRequiredService<EditStateService>();
        var session = editStateService.GetState(message.Chat.Id);

        if (session == null)
        {
            await _botClient.SendTextMessageAsync(message.Chat.Id, "Edit state not found.");
            return;
        }

        var draft = drafts.GetDraftById(session.DraftId); 
        if (draft == null)
        {
            await _botClient.SendTextMessageAsync(message.Chat.Id, "Draft not found.");
            return;
        }

        switch (field)
        {
            case EditField.Title:
                draft.Title = message.Text!;
                break;
            case EditField.DueDate:
                if (DateTime.TryParse(message.Text, out var dueDate))
                    draft.DueDate = dueDate.ToUniversalTime();
                else
                {
                    await _botClient.SendTextMessageAsync(message.Chat.Id, "Invalid date format. Example: 2025-08-01 14:00");
                    return;
                }
                break;
            case EditField.AssignedTo:
                draft.AssignedTo = message.Text!;
                break;
        }

        drafts.SaveDraft(draft);

        await _botClient.SendTextMessageAsync(
            message.Chat.Id,
            "Field updated. Draft:\n" +
            $"Title: {draft.Title}\n" +
            $"Due date: {(draft.DueDate.HasValue ? draft.DueDate.Value.ToString("f") : "not specified")}\n" +
            $"Assignee: {draft.AssignedTo ?? "not specified"}\n\n" +
            "Edit or save?",
            replyMarkup: new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("✅ Save", $"confirm_draft:{draft.Id}"),
                    InlineKeyboardButton.WithCallbackData("✏️ Edit title", $"edit_title:{draft.Id}"),
                    InlineKeyboardButton.WithCallbackData("📅 Edit due date", $"edit_due:{draft.Id}"),
                    InlineKeyboardButton.WithCallbackData("👤 Change assignee", $"edit_assigned:{draft.Id}")
                }
            }));
    }


    private Task HandleErrorAsync(ITelegramBotClient bot, Exception exception, CancellationToken token)
    {
        Console.WriteLine($"Telegram error: {exception.Message}");
        return Task.CompletedTask;
    }
}