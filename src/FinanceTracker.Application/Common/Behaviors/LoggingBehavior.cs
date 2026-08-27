using FinanceTracker.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FinanceTracker.Application.Common.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
    private readonly ICurrentUser _currentUser;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger, ICurrentUser currentUser)
    {
        _logger = logger;
        _currentUser = currentUser;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var userId = _currentUser.UserId?.ToString() ?? "anonymous";

        _logger.LogInformation("Handling {RequestName} for user {UserId}", requestName, userId);

        try
        {
            var response = await next();
            _logger.LogInformation("Handled {RequestName} for user {UserId}", requestName, userId);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failure handling {RequestName} for user {UserId}", requestName, userId);
            throw;
        }
    }
}
