using FastNotes.Api.Services;
using FastNotes.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FastNotes.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _service;

    public TasksController(ITaskService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Policy = "TasksRead")]
    public async Task<IActionResult> GetAll()
    {
        var tasks = await _service.GetAllAsync();
        return Ok(tasks);
    }

    [HttpPost]
    [Authorize(Policy = "TasksWrite")]
    public async Task<IActionResult> Create(TaskDto task)
    {
        var created = await _service.CreateAsync(task);
        return CreatedAtAction(nameof(GetAll), new { id = created.Id }, created);
    }
}