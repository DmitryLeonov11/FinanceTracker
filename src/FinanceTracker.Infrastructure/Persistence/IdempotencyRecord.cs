namespace FinanceTracker.Infrastructure.Persistence;

public sealed class IdempotencyRecord
{
    public Guid UserId { get; set; }
    public string Key { get; set; } = null!;
    public string Method { get; set; } = null!;
    public string Path { get; set; } = null!;
    public int StatusCode { get; set; }
    public string? ContentType { get; set; }
    public string? ResponseBody { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
