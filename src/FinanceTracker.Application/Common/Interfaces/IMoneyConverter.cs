using FinanceTracker.Domain.ValueObjects;

namespace FinanceTracker.Application.Common.Interfaces;

public interface IMoneyConverter
{
    Task<MoneyConversion> ConvertAsync(
        Money source,
        Currency target,
        DateOnly? asOf = null,
        CancellationToken cancellationToken = default);
}

public sealed record MoneyConversion(Money Result, decimal RateApplied, DateOnly RateDate);
