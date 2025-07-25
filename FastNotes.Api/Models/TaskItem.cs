namespace FastNotes.Api.Models;

public class TaskItem
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string AssignedTo { get; set; } = null!;
    public DateTime? DueDate { get; set; }
    public bool IsConfirmed { get; set; }
}