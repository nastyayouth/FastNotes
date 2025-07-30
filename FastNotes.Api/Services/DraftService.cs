using FastNotes.Api.Models;

namespace FastNotes.Api.Services;


public class DraftService
{
    private readonly Dictionary<Guid, TaskDraft> _drafts = new();

    public void SaveDraft(TaskDraft draft)
    {
        _drafts[draft.Id] = draft;
    }

    public TaskDraft? GetDraftById(Guid id)
    {
        return _drafts.TryGetValue(id, out var draft) ? draft : null;
    }

    public TaskDraft? GetDraft(long chatId)
    {
        return _drafts.Values.FirstOrDefault(d => d.ChatId == chatId);
    }

    public void RemoveDraft(long chatId)
    {
        var draft = GetDraft(chatId);
        if (draft != null)
            _drafts.Remove(draft.Id);
    }

    public void RemoveDraft(Guid draftId)
    {
        _drafts.Remove(draftId);
    }
}
