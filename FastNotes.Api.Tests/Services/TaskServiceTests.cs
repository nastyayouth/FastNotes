using FastNotes.Api.Data;
using FastNotes.Api.Services;
using FastNotes.Shared;
using Microsoft.EntityFrameworkCore;

namespace FastNotes.Api.Tests.Services;

public class TaskServiceTests
{
    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task CreateAndGetAll_ReturnsCreatedTask()
    {
        using var context = CreateContext();
        var service = new TaskService(context);

        var dto = new TaskDto
        {
            Title = "Test",
            Description = "Desc",
            AssignedTo = "User",
            DueDate = DateTime.UtcNow,
            IsConfirmed = false
        };

        await service.CreateAsync(dto);
        var all = await service.GetAllAsync();

        var task = Assert.Single(all);
        Assert.Equal("Test", task.Title);
    }
}
