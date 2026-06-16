using FinanceTracker.Application.Fx.Helpers;

namespace FinanceTracker.Application.Common.Interfaces;

public interface INbrbRatesClient
{
    Task<IReadOnlyCollection<NbrbRateEntry>> FetchTodayAsync(CancellationToken cancellationToken);
}
