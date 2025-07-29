namespace FastNotes.Api.Models;

public class TaskDraft
{
    public long ChatId { get; set; }
    public string RawText { get; set; }
    public string Title { get; set; }
    public DateTime? DueDate { get; set; }
    public bool isConfirmed { get; set; } = false;
    public string AssignedTo { get; set; }
}