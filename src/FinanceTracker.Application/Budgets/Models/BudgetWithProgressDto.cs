using FinanceTracker.Domain.Budgets;

namespace FinanceTracker.Application.Budgets.Models;

public sealed record BudgetWithProgressDto(
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
    DateTimeOffset CreatedAt,
    decimal Spent,
    decimal Remaining,
    decimal ProgressPercent,
    bool IsOverLimit,
    int? ThresholdReached,
    DateTimeOffset CurrentWindowFrom,
    DateTimeOffset CurrentWindowTo)
    : BudgetDto(Id, Name, CategoryId, Period, Currency, Limit, StartDate, EndDate, Rollover, IsClosed, CreatedAt);
