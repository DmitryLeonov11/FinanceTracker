using FinanceTracker.Domain.Budgets;

namespace FinanceTracker.Application.Budgets.Models;

public record BudgetDto(
    Guid Id,
    string Name,
    Guid? CategoryId,
    BudgetPeriod Period,
    string Currency,
    decimal Limit,
    DateOnly StartDate,
    DateOnly? EndDate,
    bool Rollover,
    bool IsClosed,
    DateTimeOffset CreatedAt);
