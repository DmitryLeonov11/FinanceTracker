namespace FinanceTracker.Application.Fx.Models;

public sealed record FxConversionDto(
    decimal SourceAmount,
    string SourceCurrency,
    decimal TargetAmount,
    string TargetCurrency,
    decimal RateApplied,
    DateOnly RateDate);
