using FastNotes.Api.Controllers;
using FastNotes.Api.Data;
using FastNotes.Api.Services;
using FastNotes.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastNotes.Api.Tests.Controllers;

public class TasksControllerTests
{
    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task Create_ReturnsCreatedTask()
    {
        using var context = CreateContext();
        var service = new TaskService(context);
        var controller = new TasksController(service);

        var dto = new TaskDto
        {
            Title = "Test",
            Description = "Desc",
            AssignedTo = "User",
            DueDate = DateTime.UtcNow,
            IsConfirmed = false
        };

        var result = await controller.Create(dto);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        var returned = Assert.IsType<TaskDto>(created.Value);
        Assert.NotEqual(0, returned.Id);
    }

    [Fact]
    public async Task GetAll_ReturnsTasks()
    {
        using var context = CreateContext();
        var service = new TaskService(context);
        var controller = new TasksController(service);

        await controller.Create(new TaskDto
        {
            Title = "A",
            Description = "B",
            AssignedTo = "C",
            DueDate = DateTime.UtcNow,
            IsConfirmed = true
        });

        var result = await controller.GetAll();
        var ok = Assert.IsType<OkObjectResult>(result);
        var tasks = Assert.IsAssignableFrom<IEnumerable<TaskDto>>(ok.Value);
        Assert.Single(tasks);
    }
}
