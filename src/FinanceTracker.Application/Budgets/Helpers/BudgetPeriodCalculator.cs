using FinanceTracker.Domain.Budgets;

namespace FinanceTracker.Application.Budgets.Helpers;

/// <summary>
/// Calculates the current window of a recurring budget period anchored at <c>startDate</c>.
/// Windows are aligned to calendar boundaries (Mon–Sun for week, 1st of month for month, etc.).
/// </summary>
public static class BudgetPeriodCalculator
{
    public static (DateTimeOffset From, DateTimeOffset To) GetCurrentWindow(
        BudgetPeriod period,
        DateOnly startDate,
        DateTimeOffset now)
    {
        var today = DateOnly.FromDateTime(now.UtcDateTime);
        if (today < startDate) today = startDate;

        DateOnly windowStart;
        DateOnly windowEnd;

        switch (period)
        {
            case BudgetPeriod.Week:
            {
                // ISO week: Monday is start of week
                var diff = ((int)today.DayOfWeek + 6) % 7; // Monday=0, Sunday=6
                windowStart = today.AddDays(-diff);
                if (windowStart < startDate) windowStart = startDate;
                windowEnd = windowStart.AddDays(7);
                break;
            }
            case BudgetPeriod.Month:
            {
                windowStart = new DateOnly(today.Year, today.Month, 1);
                if (windowStart < startDate) windowStart = startDate;
                windowEnd = windowStart.AddMonths(1);
                break;
            }
            case BudgetPeriod.Quarter:
            {
                var quarterStartMonth = ((today.Month - 1) / 3) * 3 + 1;
                windowStart = new DateOnly(today.Year, quarterStartMonth, 1);
                if (windowStart < startDate) windowStart = startDate;
                windowEnd = windowStart.AddMonths(3);
                break;
            }
            case BudgetPeriod.Year:
            {
                windowStart = new DateOnly(today.Year, 1, 1);
                if (windowStart < startDate) windowStart = startDate;
                windowEnd = windowStart.AddYears(1);
                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(period), period, "Unsupported budget period.");
        }

        var from = new DateTimeOffset(windowStart.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var to = new DateTimeOffset(windowEnd.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero).AddTicks(-1);
        return (from, to);
    }
}
