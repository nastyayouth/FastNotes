using FastNotes.Api.Data;
using FastNotes.Api.Infrastructure.Config;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

namespace FastNotes.Api.Controllers;

[ApiController]
[Route("api/health")]
[AllowAnonymous]
public class HealthController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly TelegramBotSettings _telegramSettings;
    private readonly OpenAISettings _openAiSettings;
    private readonly JwtSettings _jwtSettings;

    public HealthController(
        AppDbContext dbContext,
        IHttpClientFactory httpClientFactory,
        IOptions<TelegramBotSettings> telegramOptions,
        IOptions<OpenAISettings> openAiOptions,
        IOptions<JwtSettings> jwtOptions)
    {
        _dbContext = dbContext;
        _httpClientFactory = httpClientFactory;
        _telegramSettings = telegramOptions.Value;
        _openAiSettings = openAiOptions.Value;
        _jwtSettings = jwtOptions.Value;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var database = await CheckDatabaseAsync(cancellationToken);
        var telegram = await CheckTelegramAsync(cancellationToken);
        var openAi = await CheckOpenAiAsync(cancellationToken);

        var jwt = IsConfigured(_jwtSettings.SigningKey)
            ? ServiceHealth.Healthy()
            : ServiceHealth.NotConfigured("Jwt:SigningKey is not configured.");

        var allHealthy =
            database.Status == "Healthy" &&
            telegram.Status == "Healthy" &&
            openAi.Status == "Healthy" &&
            jwt.Status == "Healthy";

        var response = new
        {
            status = allHealthy ? "Healthy" : "Degraded",
            services = new
            {
                database,
                telegram,
                openAi,
                jwt
            }
        };

        return allHealthy
            ? Ok(response)
            : StatusCode(StatusCodes.Status503ServiceUnavailable, response);
    }

    private async Task<ServiceHealth> CheckDatabaseAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.Database.OpenConnectionAsync(cancellationToken);

            return ServiceHealth.Healthy();
        }
        catch (Exception ex)
        {
            return ServiceHealth.Unhealthy(
                $"{ex.GetType().Name}: {ex.Message}"
            );
        }
        finally
        {
            await _dbContext.Database.CloseConnectionAsync();
        }
    }

    private async Task<ServiceHealth> CheckTelegramAsync(
        CancellationToken cancellationToken)
    {
        if (!IsConfigured(_telegramSettings.Token))
        {
            return ServiceHealth.NotConfigured(
                "TelegramBot:Token is not configured.");
        }

        try
        {
            var client = _httpClientFactory.CreateClient();

            var response = await client.GetAsync(
                $"https://api.telegram.org/bot{_telegramSettings.Token}/getMe",
                cancellationToken);

            return response.IsSuccessStatusCode
                ? ServiceHealth.Healthy()
                : ServiceHealth.Unhealthy(
                    $"Telegram returned {(int)response.StatusCode}.");
        }
        catch (Exception ex)
        {
            return ServiceHealth.Unhealthy(ex.Message);
        }
    }

    private async Task<ServiceHealth> CheckOpenAiAsync(
        CancellationToken cancellationToken)
    {
        if (!IsConfigured(_openAiSettings.ApiKey))
        {
            return ServiceHealth.NotConfigured(
                "OpenAI:ApiKey is not configured.");
        }

        try
        {
            var client = _httpClientFactory.CreateClient();

            using var request =
                new HttpRequestMessage(
                    HttpMethod.Get,
                    "https://api.openai.com/v1/models");

            request.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    _openAiSettings.ApiKey);

            var response =
                await client.SendAsync(request, cancellationToken);

            return response.IsSuccessStatusCode
                ? ServiceHealth.Healthy()
                : ServiceHealth.Unhealthy(
                    $"OpenAI returned {(int)response.StatusCode}.");
        }
        catch (Exception ex)
        {
            return ServiceHealth.Unhealthy(ex.Message);
        }
    }

    private static bool IsConfigured(string? value)
    {
        return !string.IsNullOrWhiteSpace(value) &&
               !value.StartsWith("<");
    }

    private record ServiceHealth(
        string Status,
        string? Message = null)
    {
        public static ServiceHealth Healthy() =>
            new("Healthy");

        public static ServiceHealth Unhealthy(string message) =>
            new("Unhealthy", message);

        public static ServiceHealth NotConfigured(string message) =>
            new("NotConfigured", message);
    }
}