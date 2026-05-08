using FinanceTracker.Domain.Common;
using FinanceTracker.Domain.Exceptions;

namespace FinanceTracker.Domain.ValueObjects;

public sealed class Money : ValueObject
{
    public decimal Amount { get; }
    public Currency Currency { get; }

    private Money(decimal amount, Currency currency)
    {
        Amount = decimal.Round(amount, 4, MidpointRounding.ToEven);
        Currency = currency;
    }

    public static Money Of(decimal amount, Currency currency)
        => new(amount, Guard.NotNull(currency, nameof(currency)));

    public static Money Of(decimal amount, string currencyCode)
        => new(amount, Currency.Of(currencyCode));

    public static Money Zero(Currency currency) => new(0m, currency);

    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount - other.Amount, Currency);
    }

    public Money Negate() => new(-Amount, Currency);

    public bool IsPositive => Amount > 0;
    public bool IsNegative => Amount < 0;
    public bool IsZero => Amount == 0;

    public static Money operator +(Money a, Money b) => a.Add(b);
    public static Money operator -(Money a, Money b) => a.Subtract(b);
    public static Money operator -(Money m) => m.Negate();

    private void EnsureSameCurrency(Money other)
    {
        if (!Currency.Equals(other.Currency))
            throw new CurrencyMismatchException(Currency.Code, other.Currency.Code);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString() => $"{Amount:0.####} {Currency.Code}";
}
