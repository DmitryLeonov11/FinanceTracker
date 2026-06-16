using FinanceTracker.Application.Fx.Helpers;

namespace FinanceTracker.Domain.UnitTests.Application;

public class NbrbUsdPivotConverterTests
{
    private static readonly DateOnly Today = new(2026, 6, 17);

    [Fact]
    public void Happy_path_should_produce_USD_BYN_EUR_RUB_pivot()
    {
        var entries = new[]
        {
            new NbrbRateEntry("USD", 1, 3.27m, Today),
            new NbrbRateEntry("EUR", 1, 3.54m, Today),
            new NbrbRateEntry("RUB", 100, 3.52m, Today)
        };

        var result = NbrbUsdPivotConverter.ToUsdPivot(entries);

        result.Should().ContainKey("USD").WhoseValue.Should().Be(1m);
        result["BYN"].Should().BeApproximately(1m / 3.27m, 0.0001m);
        result["EUR"].Should().BeApproximately(3.54m / 3.27m, 0.0001m);
        result["RUB"].Should().BeApproximately(0.0352m / 3.27m, 0.0001m);
    }

    [Fact]
    public void Missing_USD_should_throw()
    {
        var entries = new[]
        {
            new NbrbRateEntry("EUR", 1, 3.54m, Today),
            new NbrbRateEntry("RUB", 100, 3.52m, Today)
        };

        var act = () => NbrbUsdPivotConverter.ToUsdPivot(entries);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Unsupported_currency_should_be_silently_ignored()
    {
        var entries = new[]
        {
            new NbrbRateEntry("USD", 1, 3.27m, Today),
            new NbrbRateEntry("GBP", 1, 4.10m, Today),
            new NbrbRateEntry("JPY", 100, 2.05m, Today)
        };

        var result = NbrbUsdPivotConverter.ToUsdPivot(entries);

        result.Should().ContainKey("USD");
        result.Should().ContainKey("BYN");
        result.Should().NotContainKey("GBP");
        result.Should().NotContainKey("JPY");
    }

    [Fact]
    public void Scale_greater_than_one_should_be_normalized_per_unit()
    {
        var entries = new[]
        {
            new NbrbRateEntry("USD", 1, 3.00m, Today),
            new NbrbRateEntry("RUB", 100, 3.00m, Today)
        };

        var result = NbrbUsdPivotConverter.ToUsdPivot(entries);

        result["RUB"].Should().BeApproximately(0.03m / 3m, 0.0001m);
    }

    [Fact]
    public void Non_positive_rates_or_scales_should_be_filtered()
    {
        var entries = new[]
        {
            new NbrbRateEntry("USD", 1, 3.27m, Today),
            new NbrbRateEntry("EUR", 0, 3.54m, Today),
            new NbrbRateEntry("RUB", 100, -3.50m, Today)
        };

        var result = NbrbUsdPivotConverter.ToUsdPivot(entries);

        result.Should().ContainKey("USD");
        result.Should().ContainKey("BYN");
        result.Should().NotContainKey("EUR");
        result.Should().NotContainKey("RUB");
    }
}
