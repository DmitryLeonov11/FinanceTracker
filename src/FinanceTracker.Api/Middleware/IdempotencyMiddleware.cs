using System.Security.Claims;
using System.Text;
using FinanceTracker.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace FinanceTracker.Api.Middleware;

public sealed class IdempotencyMiddleware
{
    private static readonly TimeSpan Ttl = TimeSpan.FromHours(24);
    private const int MaxStoredBodyBytes = 256 * 1024;
    private const int MaxKeyLength = 128;
    private const string HeaderName = "Idempotency-Key";
    private const string UniqueViolation = "23505";

    private readonly RequestDelegate _next;
    private readonly ILogger<IdempotencyMiddleware> _logger;

    public IdempotencyMiddleware(RequestDelegate next, ILogger<IdempotencyMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IServiceScopeFactory scopeFactory)
    {
        if (!IsMutation(context.Request.Method)
            || !context.Request.Headers.TryGetValue(HeaderName, out var headerValue))
        {
            await _next(context);
            return;
        }

        var key = headerValue.ToString();
        var userIdRaw = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (key.Length is 0 or > MaxKeyLength || !Guid.TryParse(userIdRaw, out var userId))
        {
            await _next(context);
            return;
        }

        var path = context.Request.Path.ToString();
        var method = context.Request.Method;

        var claim = await TryClaimAsync(scopeFactory, userId, key, method, path, context.RequestAborted);
        switch (claim.Outcome)
        {
            case ClaimOutcome.Replay:
                await WriteStoredResponseAsync(context, claim.Existing!);
                return;
            case ClaimOutcome.InFlight:
                await WriteInFlightProblemAsync(context);
                return;
            case ClaimOutcome.MethodPathMismatch:
                await WriteMismatchProblemAsync(context);
                return;
        }

        var originalBody = context.Response.Body;
        await using var buffer = new MemoryStream();
        context.Response.Body = buffer;

        try
        {
            await _next(context);
        }
        catch
        {
            context.Response.Body = originalBody;
            // Команда не дала ответа — освобождаем ключ, чтобы ретрай мог выполниться.
            await ReleaseAsync(scopeFactory, userId, key);
            throw;
        }

        context.Response.Body = originalBody;

        var status = context.Response.StatusCode;
        if (status is >= 200 and < 300 && buffer.Length <= MaxStoredBodyBytes)
        {
            await CompleteAsync(scopeFactory, userId, key, status, context.Response.ContentType, buffer);
        }
        else
        {
            await ReleaseAsync(scopeFactory, userId, key);
        }

        buffer.Position = 0;
        await buffer.CopyToAsync(originalBody, context.RequestAborted);
    }

    private static bool IsMutation(string method)
        => HttpMethods.IsPost(method) || HttpMethods.IsPut(method)
        || HttpMethods.IsPatch(method) || HttpMethods.IsDelete(method);

    private enum ClaimOutcome { Claimed, Replay, InFlight, MethodPathMismatch }

    private readonly record struct ClaimResult(ClaimOutcome Outcome, IdempotencyRecord? Existing);

    private async Task<ClaimResult> TryClaimAsync(
        IServiceScopeFactory scopeFactory,
        Guid userId,
        string key,
        string method,
        string path,
        CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var cutoff = DateTimeOffset.UtcNow - Ttl;
        var existing = await db.IdempotencyRecords
            .SingleOrDefaultAsync(r => r.UserId == userId && r.Key == key, cancellationToken);

        if (existing is not null && existing.CreatedAt < cutoff)
        {
            db.IdempotencyRecords.Remove(existing);
            await db.SaveChangesAsync(cancellationToken);
            existing = null;
        }

        if (existing is not null)
            return Classify(existing, method, path);

        db.IdempotencyRecords.Add(new IdempotencyRecord
        {
            UserId = userId,
            Key = key,
            Method = method,
            Path = path,
            StatusCode = 0,
            CreatedAt = DateTimeOffset.UtcNow
        });

        try
        {
            await db.SaveChangesAsync(cancellationToken);
            return new ClaimResult(ClaimOutcome.Claimed, null);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: UniqueViolation })
        {
            // Параллельный запрос захватил ключ первым — перечитываем его состояние.
            db.ChangeTracker.Clear();
            var winner = await db.IdempotencyRecords
                .AsNoTracking()
                .SingleAsync(r => r.UserId == userId && r.Key == key, cancellationToken);
            return Classify(winner, method, path);
        }
    }

    private static ClaimResult Classify(IdempotencyRecord existing, string method, string path)
    {
        if (existing.Method != method || existing.Path != path)
            return new ClaimResult(ClaimOutcome.MethodPathMismatch, existing);
        return existing.StatusCode == 0
            ? new ClaimResult(ClaimOutcome.InFlight, existing)
            : new ClaimResult(ClaimOutcome.Replay, existing);
    }

    private async Task CompleteAsync(
        IServiceScopeFactory scopeFactory,
        Guid userId,
        string key,
        int statusCode,
        string? contentType,
        MemoryStream buffer)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var record = await db.IdempotencyRecords
                .SingleOrDefaultAsync(r => r.UserId == userId && r.Key == key, CancellationToken.None);
            if (record is null) return;

            record.StatusCode = statusCode;
            record.ContentType = contentType;
            record.ResponseBody = Encoding.UTF8.GetString(buffer.GetBuffer(), 0, (int)buffer.Length);
            await db.SaveChangesAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Не удалось сохранить идемпотентный ответ для ключа {Key}", key);
        }
    }

    private async Task ReleaseAsync(IServiceScopeFactory scopeFactory, Guid userId, string key)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await db.IdempotencyRecords
                .Where(r => r.UserId == userId && r.Key == key && r.StatusCode == 0)
                .ExecuteDeleteAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Не удалось освободить идемпотентный ключ {Key}", key);
        }
    }

    private static async Task WriteStoredResponseAsync(HttpContext context, IdempotencyRecord record)
    {
        context.Response.StatusCode = record.StatusCode;
        if (!string.IsNullOrEmpty(record.ContentType))
            context.Response.ContentType = record.ContentType;
        if (!string.IsNullOrEmpty(record.ResponseBody))
            await context.Response.WriteAsync(record.ResponseBody, context.RequestAborted);
    }

    private static Task WriteInFlightProblemAsync(HttpContext context)
        => WriteProblemAsync(
            context,
            StatusCodes.Status409Conflict,
            "Запрос уже выполняется",
            "Запрос с этим Idempotency-Key ещё обрабатывается. Повторите позже.");

    private static Task WriteMismatchProblemAsync(HttpContext context)
        => WriteProblemAsync(
            context,
            StatusCodes.Status422UnprocessableEntity,
            "Конфликт Idempotency-Key",
            "Этот Idempotency-Key уже использован для другого запроса.");

    private static async Task WriteProblemAsync(HttpContext context, int status, string title, string detail)
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
        await context.Response.WriteAsJsonAsync(problem);
    }
}
