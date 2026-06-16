using FinanceTracker.Application.Accounts.Models;

namespace FinanceTracker.Application.Accounts.Helpers;

public static class BalanceHistoryCalculator
{
    public static IReadOnlyList<BalancePointDto> Compute(
        decimal currentBalance,
        DateOnly today,
        int days,
        IReadOnlyDictionary<DateOnly, decimal> deltasByDay)
    {
        ArgumentNullException.ThrowIfNull(deltasByDay);
        if (days < 1)
            throw new ArgumentOutOfRangeException(nameof(days), "Период должен быть не менее одного дня.");

        var points = new BalancePointDto[days];
        var balance = currentBalance;

        for (var i = days - 1; i >= 0; i--)
        {
            var date = today.AddDays(-(days - 1 - i));
            points[i] = new BalancePointDto(date, balance);
            if (deltasByDay.TryGetValue(date, out var delta))
                balance -= delta;
        }

        return points;
    }
}
