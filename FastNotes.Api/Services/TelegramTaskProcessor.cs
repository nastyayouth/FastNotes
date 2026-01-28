using FastNotes.Api.Data;
using FastNotes.Api.Models;

namespace FastNotes.Api.Services;

public class TelegramTaskProcessor
{
    private readonly AppDbContext _db;

    public TelegramTaskProcessor(AppDbContext db)
    {
        _db = db;
    }

    public async Task<TaskItem> CreateTaskFromTextAsync(string text, string username)
    {
        var task = new TaskItem
        {
            Title = $"Voice message from @{username}",
            Description = text,
            AssignedTo = username,
            DueDate = DateTime.UtcNow.AddDays(1),
            IsConfirmed = false
        };

        _db.Tasks.Add(task);
        await _db.SaveChangesAsync();

        return task;
    }
}