using FinanceTracker.Domain.Exceptions;
using FinanceTracker.Domain.ValueObjects;

namespace FinanceTracker.Domain.UnitTests.ValueObjects;

public class MoneyTests
{
    private static readonly Currency Byn = Currency.Of("BYN");
    private static readonly Currency Usd = Currency.Of("USD");

    [Fact]
    public void Of_should_round_to_4_decimals()
    {
        var m = Money.Of(1.123456m, Byn);
        m.Amount.Should().Be(1.1235m);
    }

    [Fact]
    public void Add_same_currency_should_sum_amounts()
    {
        var a = Money.Of(10.50m, Byn);
        var b = Money.Of(2.25m, Byn);
        (a + b).Amount.Should().Be(12.75m);
    }

    [Fact]
    public void Add_different_currency_should_throw()
    {
        var a = Money.Of(10m, Byn);
        var b = Money.Of(5m, Usd);
        var act = () => a.Add(b);
        act.Should().Throw<CurrencyMismatchException>();
    }

    [Fact]
    public void Subtract_should_produce_negative_when_b_greater()
    {
        var a = Money.Of(5m, Byn);
        var b = Money.Of(20m, Byn);
        var result = a - b;
        result.Amount.Should().Be(-15m);
        result.IsNegative.Should().BeTrue();
    }

    [Fact]
    public void Negate_should_flip_sign_and_keep_currency()
    {
        var m = Money.Of(7m, Byn);
        var n = -m;
        n.Amount.Should().Be(-7m);
        n.Currency.Should().Be(Byn);
    }

    [Fact]
    public void IsPositive_IsNegative_IsZero_should_be_mutually_exclusive()
    {
        Money.Of(1m, Byn).IsPositive.Should().BeTrue();
        Money.Of(-1m, Byn).IsNegative.Should().BeTrue();
        Money.Zero(Byn).IsZero.Should().BeTrue();
    }

    [Fact]
    public void Equality_should_consider_amount_and_currency()
    {
        var a = Money.Of(10m, Byn);
        var b = Money.Of(10m, Byn);
        var c = Money.Of(10m, Usd);
        a.Should().Be(b);
        a.Should().NotBe(c);
    }
}
