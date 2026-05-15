namespace FinanceTracker.Application.Common.Models;

public sealed record PagedResult<T>(
    IReadOnlyCollection<T> Items,
    int Total,
    int Page,
    int PageSize)
{
    public bool HasMore => Page * PageSize < Total;
}
