namespace FinanceTracker.Application.Accounts.Models;

public sealed record AccountBalanceHistoryDto(
    Guid AccountId,
    string Currency,
    IReadOnlyList<BalancePointDto> Points);

public sealed record BalancePointDto(DateOnly Date, decimal Balance);
