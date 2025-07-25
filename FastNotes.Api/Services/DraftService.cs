using FastNotes.Api.Models;

namespace FastNotes.Api.Services;


public class DraftService
{
    private readonly Dictionary<long, TaskDraft> _drafts = new();

    public void SaveDraft(TaskDraft draft)
    {
        _drafts[draft.ChatId] = draft;
    }

    public TaskDraft? GetDraft(long chatId)
    {
        return _drafts.TryGetValue(chatId, out var draft) ? draft : null;
    }

    public void RemoveDraft(long chatId)
    {
        _drafts.Remove(chatId);
    }
}
