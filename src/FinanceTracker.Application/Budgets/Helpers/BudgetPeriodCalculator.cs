using FinanceTracker.Domain.Budgets;

namespace FinanceTracker.Application.Budgets.Helpers;

public static class BudgetPeriodCalculator
{
    public static (DateTimeOffset From, DateTimeOffset To) GetCurrentWindow(
        BudgetPeriod period,
        DateOnly startDate,
        DateTimeOffset now)
    {
        var today = DateOnly.FromDateTime(now.UtcDateTime);
        if (today < startDate) today = startDate;

        DateOnly alignedStart;
        DateOnly windowEnd;

        switch (period)
        {
            case BudgetPeriod.Week:
            {
                // неделя бюджета всегда начинается с понедельника, а не с воскресенья
                var diff = ((int)today.DayOfWeek + 6) % 7;
                alignedStart = today.AddDays(-diff);
                windowEnd = alignedStart.AddDays(7);
                break;
            }
            case BudgetPeriod.Month:
            {
                alignedStart = new DateOnly(today.Year, today.Month, 1);
                windowEnd = alignedStart.AddMonths(1);
                break;
            }
            case BudgetPeriod.Quarter:
            {
                var quarterStartMonth = ((today.Month - 1) / 3) * 3 + 1;
                alignedStart = new DateOnly(today.Year, quarterStartMonth, 1);
                windowEnd = alignedStart.AddMonths(3);
                break;
            }
            case BudgetPeriod.Year:
            {
                alignedStart = new DateOnly(today.Year, 1, 1);
                windowEnd = alignedStart.AddYears(1);
                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(period), period, "Unsupported budget period.");
        }

        var windowStart = alignedStart < startDate ? startDate : alignedStart;

        var from = new DateTimeOffset(windowStart.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var to = new DateTimeOffset(windowEnd.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero).AddTicks(-1);
        return (from, to);
    }
}
