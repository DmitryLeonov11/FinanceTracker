using FinanceTracker.Application.Common.Exceptions;
using FinanceTracker.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace FinanceTracker.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            if (context.Response.HasStarted)
            {
                _logger.LogWarning(ex, "Response already started, cannot write validation error");
                return;
            }
            await WriteValidationProblem(context, ex);
        }
        catch (NotFoundException ex)
        {
            if (context.Response.HasStarted)
            {
                _logger.LogWarning(ex, "Response already started, cannot write not found error");
                return;
            }
            await WriteProblem(context, StatusCodes.Status404NotFound, "Не найдено", ex.Message);
        }
        catch (ForbiddenAccessException ex)
        {
            if (context.Response.HasStarted)
            {
                _logger.LogWarning(ex, "Response already started, cannot write forbidden error");
                return;
            }
            var status = ex.RequiresAuthentication
                ? StatusCodes.Status401Unauthorized
                : StatusCodes.Status403Forbidden;
            await WriteProblem(context, status, ex.RequiresAuthentication ? "Требуется авторизация" : "Доступ запрещён", ex.Message);
        }
        catch (DomainException ex)
        {
            if (context.Response.HasStarted)
            {
                _logger.LogWarning(ex, "Response already started, cannot write domain error");
                return;
            }
            await WriteProblem(context, StatusCodes.Status409Conflict, "Ошибка бизнес-правила", ex.Message);
        }
        catch (Exception ex)
        {
            if (context.Response.HasStarted)
            {
                _logger.LogWarning(ex, "Response already started, cannot write internal error");
                return;
            }
            _logger.LogError(ex, "Необработанное исключение");
            await WriteProblem(context, StatusCodes.Status500InternalServerError, "Внутренняя ошибка сервера", "Произошла непредвиденная ошибка.");
        }
    }

    private static Task WriteProblem(HttpContext context, int status, string title, string detail)
    {
        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";
        return context.Response.WriteAsJsonAsync(problem);
    }

    private static Task WriteValidationProblem(HttpContext context, ValidationException ex)
    {
        var problem = new ValidationProblemDetails(ex.Errors.ToDictionary(kv => kv.Key, kv => kv.Value))
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Ошибка валидации",
            Instance = context.Request.Path
        };
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        context.Response.ContentType = "application/problem+json";
        return context.Response.WriteAsJsonAsync(problem);
    }
}
