namespace FinanceTracker.Application.Dashboard.Models;

public sealed record CashflowDto(
    string Currency,
    DateTimeOffset From,
    DateTimeOffset To,
    decimal TotalIncome,
    decimal TotalExpense,
    decimal Net,
    IReadOnlyList<CashflowPoint> Points);

public sealed record CashflowPoint(
    DateOnly Date,
    decimal Income,
    decimal Expense);
