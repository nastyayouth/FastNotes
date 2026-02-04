namespace FastNotes.Api.Infrastructure.Errors;

public sealed class NotFoundException : ApiException
{
    public NotFoundException(string message) : base(message) { }

    public override int StatusCode => StatusCodes.Status404NotFound;
    public override string Title => "Resource not found";
}
