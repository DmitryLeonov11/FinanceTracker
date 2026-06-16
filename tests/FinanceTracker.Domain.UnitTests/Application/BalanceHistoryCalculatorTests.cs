using FinanceTracker.Application.Accounts.Helpers;

namespace FinanceTracker.Domain.UnitTests.Application;

public class BalanceHistoryCalculatorTests
{
    private static readonly DateOnly Today = new(2026, 6, 17);

    [Fact]
    public void No_deltas_should_carry_current_balance_through_all_days()
    {
        var result = BalanceHistoryCalculator.Compute(1000m, Today, 30, new Dictionary<DateOnly, decimal>());

        result.Should().HaveCount(30);
        result.Should().OnlyContain(p => p.Balance == 1000m);
        result[0].Date.Should().Be(Today.AddDays(-29));
        result[^1].Date.Should().Be(Today);
    }

    [Fact]
    public void Single_income_today_should_lower_yesterdays_balance()
    {
        var deltas = new Dictionary<DateOnly, decimal> { [Today] = 200m };

        var result = BalanceHistoryCalculator.Compute(1000m, Today, 30, deltas);

        result[^1].Balance.Should().Be(1000m);
        result[^2].Balance.Should().Be(800m);
        result[0].Balance.Should().Be(800m);
    }

    [Fact]
    public void Single_expense_today_should_raise_yesterdays_balance()
    {
        var deltas = new Dictionary<DateOnly, decimal> { [Today] = -300m };

        var result = BalanceHistoryCalculator.Compute(1000m, Today, 30, deltas);

        result[^1].Balance.Should().Be(1000m);
        result[^2].Balance.Should().Be(1300m);
    }

    [Fact]
    public void Multiple_deltas_should_invert_sum_correctly()
    {
        var deltas = new Dictionary<DateOnly, decimal>
        {
            [Today] = 100m,
            [Today.AddDays(-2)] = -50m,
            [Today.AddDays(-5)] = 200m
        };

        var result = BalanceHistoryCalculator.Compute(1000m, Today, 30, deltas);

        result[^1].Balance.Should().Be(1000m);
        result[^2].Balance.Should().Be(900m);
        result[^3].Balance.Should().Be(900m);
        result[^4].Balance.Should().Be(950m);
        result[^5].Balance.Should().Be(950m);
        result[^6].Balance.Should().Be(950m);
        result[^7].Balance.Should().Be(750m);
    }

    [Fact]
    public void Custom_days_should_return_exact_count_oldest_first()
    {
        var result = BalanceHistoryCalculator.Compute(500m, Today, 7, new Dictionary<DateOnly, decimal>());

        result.Should().HaveCount(7);
        result[0].Date.Should().Be(Today.AddDays(-6));
        result[^1].Date.Should().Be(Today);
    }
}
