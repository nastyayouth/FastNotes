namespace FastNotes.Api.Infrastructure.Config;

public class DatabaseSettings
{
    public string DBProvider { get; set; } = "postgresql";
    public string ConnectionString { get; set; } = string.Empty;
    public string ProtectionKeysConnectionString { get; set; } = string.Empty;
}