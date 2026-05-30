using FinanceTracker.Domain.Exceptions;
using FinanceTracker.Domain.ValueObjects;

namespace FinanceTracker.Domain.UnitTests.ValueObjects;

public class CurrencyTests
{
    [Theory]
    [InlineData("BYN")]
    [InlineData("USD")]
    [InlineData("EUR")]
    [InlineData("RUB")]
    public void Of_should_accept_supported_currencies(string code)
    {
        var c = Currency.Of(code);
        c.Code.Should().Be(code);
    }

    [Fact]
    public void Of_should_uppercase_input()
    {
        Currency.Of("usd").Code.Should().Be("USD");
    }

    [Theory]
    [InlineData("JPY")]
    [InlineData("GBP")]
    [InlineData("CHF")]
    public void Of_should_reject_unsupported_currencies(string code)
    {
        var act = () => Currency.Of(code);
        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("US")]
    [InlineData("USDX")]
    [InlineData("US$")]
    public void Of_should_reject_malformed_codes(string code)
    {
        var act = () => Currency.Of(code);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void IsSupported_should_match_Of_behaviour()
    {
        Currency.IsSupported("BYN").Should().BeTrue();
        Currency.IsSupported("usd").Should().BeTrue();
        Currency.IsSupported("JPY").Should().BeFalse();
        Currency.IsSupported("").Should().BeFalse();
    }
}
