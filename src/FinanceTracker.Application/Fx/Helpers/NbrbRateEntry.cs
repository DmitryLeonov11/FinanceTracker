namespace FinanceTracker.Application.Fx.Helpers;

public sealed record NbrbRateEntry(string Currency, int Scale, decimal OfficialRate, DateOnly Date);
