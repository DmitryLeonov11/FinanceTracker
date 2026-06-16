namespace FinanceTracker.Infrastructure.Settings;

public sealed class FxRefreshSettings
{
    public const string SectionName = "Fx:Refresh";

    public bool Enabled { get; init; } = true;
    public string BaseUrl { get; init; } = "https://api.nbrb.by/";
    public TimeSpan Interval { get; init; } = TimeSpan.FromHours(24);
    public TimeSpan InitialDelay { get; init; } = TimeSpan.FromSeconds(30);
    public TimeSpan HttpTimeout { get; init; } = TimeSpan.FromSeconds(10);
}
