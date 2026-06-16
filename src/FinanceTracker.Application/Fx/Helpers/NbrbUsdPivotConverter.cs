using FinanceTracker.Domain.ValueObjects;

namespace FinanceTracker.Application.Fx.Helpers;

public static class NbrbUsdPivotConverter
{
    public static IReadOnlyDictionary<string, decimal> ToUsdPivot(IReadOnlyCollection<NbrbRateEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);

        var supported = entries
            .Where(e => Currency.IsSupported(e.Currency) && e.Scale > 0 && e.OfficialRate > 0m)
            .GroupBy(e => e.Currency.ToUpperInvariant())
            .ToDictionary(g => g.Key, g => g.First());

        if (!supported.TryGetValue("USD", out var usd))
            throw new InvalidOperationException("NBRB snapshot does not contain USD rate.");

        var bynPerUsd = usd.OfficialRate / usd.Scale;

        var result = new Dictionary<string, decimal>(supported.Count + 1)
        {
            ["USD"] = 1m,
            ["BYN"] = 1m / bynPerUsd
        };

        foreach (var (code, entry) in supported)
        {
            if (code is "USD" or "BYN") continue;
            var bynPerUnit = entry.OfficialRate / entry.Scale;
            result[code] = bynPerUnit / bynPerUsd;
        }

        return result;
    }
}
