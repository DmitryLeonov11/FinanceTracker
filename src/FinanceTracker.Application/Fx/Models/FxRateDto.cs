namespace FinanceTracker.Application.Fx.Models;

public sealed record FxRateDto(
    string Currency,
    decimal RateToUsd,
    DateOnly Date);
