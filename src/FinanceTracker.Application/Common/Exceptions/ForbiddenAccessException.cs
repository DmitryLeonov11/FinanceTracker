namespace FinanceTracker.Application.Common.Exceptions;

public sealed class ForbiddenAccessException : Exception
{
    public bool RequiresAuthentication { get; init; }

    public ForbiddenAccessException() : base("Доступ к запрошенному ресурсу запрещён.") { }

    public ForbiddenAccessException(string message) : base(message) { }

    public ForbiddenAccessException(string message, bool requiresAuthentication) : base(message)
    {
        RequiresAuthentication = requiresAuthentication;
    }
}
