namespace FastNotes.Api.Infrastructure.Errors;

public abstract class ApiException : Exception
{
    protected ApiException(string message) : base(message) { }

    public abstract int StatusCode { get; }
    public abstract string Title { get; }
}
