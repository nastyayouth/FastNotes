using FastNotes.Api.Data;
using FastNotes.Api.Models;
using FastNotes.Shared;
using Microsoft.EntityFrameworkCore;

namespace FastNotes.Api.Services;

public class TaskService : ITaskService
{
    private readonly AppDbContext _context;

    public TaskService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TaskDto>> GetAllAsync()
    {
        var tasks = await _context.Tasks.ToListAsync();

        return tasks.Select(t => new TaskDto
        {
            Id = t.Id,
            Title = t.Title,
            Description = t.Description,
            AssignedTo = t.AssignedTo,
            DueDate = t.DueDate,
            IsConfirmed = t.IsConfirmed
        });
    }

    public async Task<TaskDto> CreateAsync(TaskDto task)
    {
        var entity = new TaskItem
        {
            Title = task.Title,
            Description = task.Description,
            AssignedTo = task.AssignedTo,
            DueDate = task.DueDate,
            IsConfirmed = task.IsConfirmed
        };

        _context.Tasks.Add(entity);
        await _context.SaveChangesAsync();

        task.Id = entity.Id;
        return task;
    }

    public async Task<List<TaskDto>> GetTodayAsync()
    {
        var today = DateTime.Today;

        var tasks = await _context.Tasks
            .Where(t => t.DueDate.Value== today)
            .ToListAsync();

        return tasks.Select(t => new TaskDto()
        {
            Id = t.Id,
            Title = t.Title,
            Description = t.Description,
            AssignedTo = t.AssignedTo,
            DueDate = t.DueDate,
            IsConfirmed = t.IsConfirmed
        }).ToList();
    }
}