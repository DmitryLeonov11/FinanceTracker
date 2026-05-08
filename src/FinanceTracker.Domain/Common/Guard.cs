using FinanceTracker.Domain.Exceptions;

namespace FinanceTracker.Domain.Common;

public static class Guard
{
    public static T NotNull<T>(T? value, string parameterName) where T : class
        => value ?? throw new DomainException($"Параметр '{parameterName}' не может быть пустым.");

    public static string NotNullOrWhiteSpace(string? value, string parameterName)
        => string.IsNullOrWhiteSpace(value)
            ? throw new DomainException($"Параметр '{parameterName}' не может быть пустым.")
            : value;

    public static string MaxLength(string value, int max, string parameterName)
        => value.Length > max
            ? throw new DomainException($"Параметр '{parameterName}' не должен превышать {max} символов.")
            : value;

    public static decimal Positive(decimal value, string parameterName)
        => value <= 0
            ? throw new DomainException($"Параметр '{parameterName}' должен быть положительным.")
            : value;

    public static decimal NonNegative(decimal value, string parameterName)
        => value < 0
            ? throw new DomainException($"Параметр '{parameterName}' не может быть отрицательным.")
            : value;
}
