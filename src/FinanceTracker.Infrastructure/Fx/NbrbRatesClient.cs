using System.Net.Http.Json;
using System.Text.Json;
using FinanceTracker.Application.Common.Interfaces;
using FinanceTracker.Application.Fx.Helpers;

namespace FinanceTracker.Infrastructure.Fx;

internal sealed class NbrbRatesClient : INbrbRatesClient
{
    private const string DailyEndpoint = "exrates/rates?periodicity=0";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _http;

    public NbrbRatesClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<IReadOnlyCollection<NbrbRateEntry>> FetchTodayAsync(CancellationToken cancellationToken)
    {
        var payload = await _http.GetFromJsonAsync<List<NbrbRateDto>>(DailyEndpoint, JsonOptions, cancellationToken);
        if (payload is null || payload.Count == 0)
            return Array.Empty<NbrbRateEntry>();

        return payload
            .Where(dto => !string.IsNullOrWhiteSpace(dto.CurAbbreviation))
            .Select(dto => new NbrbRateEntry(
                dto.CurAbbreviation.Trim().ToUpperInvariant(),
                dto.CurScale,
                dto.CurOfficialRate,
                DateOnly.FromDateTime(dto.Date)))
            .ToArray();
    }
}
