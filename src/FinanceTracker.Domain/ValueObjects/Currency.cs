using FinanceTracker.Domain.Common;
using FinanceTracker.Domain.Exceptions;

namespace FinanceTracker.Domain.ValueObjects;

public sealed class Currency : ValueObject
{
    public static readonly IReadOnlyList<string> Supported = new[] { "BYN", "USD", "EUR", "RUB" };

    public static Currency Byn => Of("BYN");
    public static Currency Usd => Of("USD");
    public static Currency Eur => Of("EUR");
    public static Currency Rub => Of("RUB");

    public string Code { get; }

    private Currency(string code) => Code = code;

    public static Currency Of(string code)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Length != 3)
            throw new DomainException("Код валюты должен состоять из 3 букв (ISO 4217).");

        var upper = code.ToUpperInvariant();
        if (!upper.All(char.IsLetter))
            throw new DomainException("Код валюты должен содержать только буквы.");

        if (!Supported.Contains(upper))
            throw new DomainException($"Валюта '{upper}' не поддерживается. Доступные валюты: {string.Join(", ", Supported)}.");

        return new Currency(upper);
    }

    public static bool IsSupported(string code)
        => !string.IsNullOrWhiteSpace(code) && Supported.Contains(code.ToUpperInvariant());

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Code;
    }

    public override string ToString() => Code;
}
