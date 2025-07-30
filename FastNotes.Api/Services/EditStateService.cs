using FastNotes.Api.Models;

namespace FastNotes.Api.Services;

public enum EditField
{
    Title,
    DueDate,
    AssignedTo,
    None
}

public class EditStateService
{
    public record EditSession(EditField Field, Guid DraftId);

    private readonly Dictionary<long, EditSession> _states = new();

    public void SetState(long chatId, EditField field, Guid draftId)
    {
        _states[chatId] = new EditSession(field, draftId);
    }

    public EditSession? GetState(long chatId)
    {
        return _states.TryGetValue(chatId, out var session) ? session : null;
    }

    public void ClearState(long chatId)
    {
        _states.Remove(chatId);
    }
}
