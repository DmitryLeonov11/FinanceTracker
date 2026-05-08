using System.Text.RegularExpressions;
using FinanceTracker.Domain.Common;
using FinanceTracker.Domain.Exceptions;

namespace FinanceTracker.Domain.ValueObjects;

public sealed partial class Email : ValueObject
{
    public string Value { get; }

    private Email(string value) => Value = value;

    public static Email Of(string value)
    {
        var trimmed = Guard.NotNullOrWhiteSpace(value, nameof(value)).Trim().ToLowerInvariant();
        if (trimmed.Length > 254 || !EmailRegex().IsMatch(trimmed))
            throw new DomainException($"'{value}' не является корректным адресом электронной почты.");
        return new Email(trimmed);
    }

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase)]
    private static partial Regex EmailRegex();

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
