namespace FastNotes.Api.Infrastructure.Errors;

public sealed class ValidationException : ApiException
{
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public ValidationException(
        string message,
        IReadOnlyDictionary<string, string[]> errors)
        : base(message)
    {
        Errors = errors;
    }

    public override int StatusCode => StatusCodes.Status400BadRequest;
    public override string Title => "Validation error";
}
 