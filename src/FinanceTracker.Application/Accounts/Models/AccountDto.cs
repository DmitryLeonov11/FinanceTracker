using FinanceTracker.Domain.Accounts;

namespace FinanceTracker.Application.Accounts.Models;

public sealed record AccountDto(
    Guid Id,
    string Name,
    AccountType Type,
    string Currency,
    decimal Balance,
    bool IsArchived,
    DateTimeOffset CreatedAt);
