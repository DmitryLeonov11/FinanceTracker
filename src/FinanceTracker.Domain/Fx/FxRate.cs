using FinanceTracker.Domain.Common;
using FinanceTracker.Domain.Exceptions;

namespace FinanceTracker.Domain.Fx;

/// <summary>
/// 1 unit of <see cref="Currency"/> equals <see cref="RateToUsd"/> USD.
/// USD anchor: <c>RateToUsd = 1.0</c>.
/// </summary>
public sealed class FxRate : AggregateRoot
{
    public DateOnly Date { get; private set; }
    public string Currency { get; private set; } = null!;
    public decimal RateToUsd { get; private set; }

    private FxRate() { }

    private FxRate(Guid id, DateOnly date, string currency, decimal rateToUsd) : base(id)
    {
        Date = date;
        Currency = currency;
        RateToUsd = rateToUsd;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public static FxRate Of(DateOnly date, string currency, decimal rateToUsd)
    {
        var code = (currency ?? string.Empty).Trim().ToUpperInvariant();
        if (!ValueObjects.Currency.IsSupported(code))
            throw new DomainException($"Валюта '{currency}' не поддерживается.");
        if (rateToUsd <= 0)
            throw new DomainException("Курс должен быть положительным.");
        return new FxRate(Guid.NewGuid(), date, code, rateToUsd);
    }

    public void UpdateRate(decimal newRate, DateTimeOffset asOf)
    {
        if (newRate <= 0)
            throw new DomainException("Курс должен быть положительным.");
        RateToUsd = newRate;
        UpdatedAt = asOf;
        Version++;
    }
}
