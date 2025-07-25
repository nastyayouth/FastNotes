namespace FastNotes.Shared;

public class TaskDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string AssignedTo { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public bool IsConfirmed { get; set; }
}