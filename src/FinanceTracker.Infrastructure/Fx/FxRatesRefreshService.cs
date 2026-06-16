using FinanceTracker.Application.Common.Interfaces;
using FinanceTracker.Application.Fx.Helpers;
using FinanceTracker.Domain.Fx;
using FinanceTracker.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FinanceTracker.Infrastructure.Fx;

internal sealed class FxRatesRefreshService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptions<FxRefreshSettings> _options;
    private readonly ILogger<FxRatesRefreshService> _logger;

    public FxRatesRefreshService(
        IServiceScopeFactory scopeFactory,
        IOptions<FxRefreshSettings> options,
        ILogger<FxRatesRefreshService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = _options.Value;
        if (!settings.Enabled)
        {
            _logger.LogInformation("FX refresh disabled via configuration.");
            return;
        }

        try
        {
            await Task.Delay(settings.InitialDelay, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        using var timer = new PeriodicTimer(settings.Interval);
        do
        {
            await RefreshOnceAsync(stoppingToken);
        }
        while (await WaitForNextTickAsync(timer, stoppingToken));
    }

    private static async Task<bool> WaitForNextTickAsync(PeriodicTimer timer, CancellationToken ct)
    {
        try
        {
            return await timer.WaitForNextTickAsync(ct);
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }

    private async Task RefreshOnceAsync(CancellationToken ct)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var client = scope.ServiceProvider.GetRequiredService<INbrbRatesClient>();
            var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
            var clock = scope.ServiceProvider.GetRequiredService<IDateTime>();

            var entries = await client.FetchTodayAsync(ct);
            if (entries.Count == 0)
            {
                _logger.LogWarning("NBRB returned an empty rates snapshot.");
                return;
            }

            var pivot = NbrbUsdPivotConverter.ToUsdPivot(entries);
            var date = entries.Select(e => e.Date).DefaultIfEmpty(DateOnly.FromDateTime(clock.UtcNow.UtcDateTime)).Max();
            var now = clock.UtcNow;

            foreach (var (currency, rateToUsd) in pivot)
            {
                var existing = await db.FxRates
                    .FirstOrDefaultAsync(r => r.Currency == currency && r.Date == date, ct);

                if (existing is null)
                {
                    db.FxRates.Add(FxRate.Of(date, currency, rateToUsd));
                }
                else if (existing.RateToUsd != rateToUsd)
                {
                    existing.UpdateRate(rateToUsd, now);
                }
            }

            var written = await db.SaveChangesAsync(ct);
            _logger.LogInformation(
                "FX rates refreshed for {Date}: {Count} rows touched. USD=1, BYN={Byn:0.######}, EUR={Eur:0.######}, RUB={Rub:0.######}",
                date,
                written,
                pivot.GetValueOrDefault("BYN"),
                pivot.GetValueOrDefault("EUR"),
                pivot.GetValueOrDefault("RUB"));
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            // shutdown
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "FX refresh failed; will retry on next tick.");
        }
    }
}
