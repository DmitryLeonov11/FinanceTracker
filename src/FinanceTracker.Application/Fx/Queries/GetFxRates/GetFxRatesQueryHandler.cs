using FinanceTracker.Application.Common.Interfaces;
using FinanceTracker.Application.Fx.Models;
using FinanceTracker.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Application.Fx.Queries.GetFxRates;

public sealed class GetFxRatesQueryHandler : IRequestHandler<GetFxRatesQuery, IReadOnlyCollection<FxRateDto>>
{
    private readonly IApplicationDbContext _db;

    public GetFxRatesQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyCollection<FxRateDto>> Handle(GetFxRatesQuery request, CancellationToken cancellationToken)
    {
        // Latest rate per supported currency.
        var latest = await _db.FxRates
            .AsNoTracking()
            .GroupBy(r => r.Currency)
            .Select(g => g.OrderByDescending(r => r.Date).First())
            .ToListAsync(cancellationToken);

        var dict = latest.ToDictionary(r => r.Currency);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var result = new List<FxRateDto>(Currency.Supported.Count);
        foreach (var code in Currency.Supported)
        {
            if (dict.TryGetValue(code, out var rate))
                result.Add(new FxRateDto(rate.Currency, rate.RateToUsd, rate.Date));
            else if (code == "USD")
                result.Add(new FxRateDto("USD", 1m, today));
        }

        return result.OrderBy(r => r.Currency).ToList();
    }
}
