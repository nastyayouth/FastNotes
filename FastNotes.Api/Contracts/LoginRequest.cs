namespace FastNotes.Api.Contracts;

public class LoginRequest
{
    public string UserId { get; set; } = "dev-user";
    public bool CanWrite { get; set; } = true;
}