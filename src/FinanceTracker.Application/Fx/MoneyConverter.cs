using FinanceTracker.Application.Common.Interfaces;
using FinanceTracker.Domain.Exceptions;
using FinanceTracker.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Application.Fx;

public sealed class MoneyConverter : IMoneyConverter
{
    private readonly IApplicationDbContext _db;
    private readonly IDateTime _clock;

    public MoneyConverter(IApplicationDbContext db, IDateTime clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<MoneyConversion> ConvertAsync(
        Money source,
        Currency target,
        DateOnly? asOf = null,
        CancellationToken cancellationToken = default)
    {
        if (source.Currency.Equals(target))
            return new MoneyConversion(source, 1m, asOf ?? DateOnly.FromDateTime(_clock.UtcNow.UtcDateTime));

        var cutoff = asOf ?? DateOnly.FromDateTime(_clock.UtcNow.UtcDateTime);

        var rateSource = await _db.FxRates
            .AsNoTracking()
            .Where(r => r.Currency == source.Currency.Code && r.Date <= cutoff)
            .OrderByDescending(r => r.Date)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new DomainException($"Курс для валюты {source.Currency.Code} на {cutoff:yyyy-MM-dd} не найден.");

        var rateTarget = await _db.FxRates
            .AsNoTracking()
            .Where(r => r.Currency == target.Code && r.Date <= cutoff)
            .OrderByDescending(r => r.Date)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new DomainException($"Курс для валюты {target.Code} на {cutoff:yyyy-MM-dd} не найден.");

        // amount_target = amount_source * (rateSource_to_usd / rateTarget_to_usd)
        var crossRate = rateSource.RateToUsd / rateTarget.RateToUsd;
        var resultAmount = decimal.Round(source.Amount * crossRate, 4, MidpointRounding.AwayFromZero);

        var rateDate = rateSource.Date < rateTarget.Date ? rateSource.Date : rateTarget.Date;
        return new MoneyConversion(Money.Of(resultAmount, target), crossRate, rateDate);
    }
}
