using FinanceTracker.Domain.Transactions;

namespace FinanceTracker.Application.Transactions.Models;

public sealed record TransactionDto(
    Guid Id,
    Guid AccountId,
    Guid? CounterAccountId,
    Guid? CategoryId,
    TransactionType Type,
    decimal Amount,
    string Currency,
    DateTimeOffset OccurredAt,
    string? Note);
