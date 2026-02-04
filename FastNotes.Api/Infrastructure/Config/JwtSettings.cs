namespace FastNotes.Api.Infrastructure.Config;

public class JwtSettings
{
    public string Issuer { get; set; } = "FastNotes";
    public string Audience { get; set; } = "FastNotes.Api";
    public string SigningKey { get; set; } = string.Empty;
    public int TokenLifetimeMinutes { get; set; } = 60;
}