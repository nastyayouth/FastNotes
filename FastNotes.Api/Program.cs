using FastNotes.Api.Data;
using FastNotes.Api.Infrastructure;
using FastNotes.Api.Services;
using FastNotes.Shared;
using Microsoft.EntityFrameworkCore;
using FastNotes.Api.Infrastructure.Config;
using FastNotes.Api.Infrastructure.Whisper;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<AppDbContext>((sp, options) =>
{
    var config = sp.GetRequiredService<IOptions<DatabaseSettings>>().Value;
    options.UseNpgsql(config.ConnectionString);
});
builder.Services.Configure<DatabaseSettings>(
    builder.Configuration.GetSection("DatabaseSettings"));

builder.Services.Configure<OpenAISettings>(
    builder.Configuration.GetSection("OpenAI"));


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<TelegramBotService>();

builder.Services.AddScoped<TelegramTaskProcessor>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<WhisperService>();
builder.Services.AddSingleton<DraftService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

var bot = app.Services.GetRequiredService<TelegramBotService>();
bot.Start();
app.Run();