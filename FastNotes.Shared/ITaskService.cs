namespace FastNotes.Shared;

public interface ITaskService
{
    Task<IEnumerable<TaskDto>> GetAllAsync();
    Task<TaskDto> CreateAsync(TaskDto task);
}