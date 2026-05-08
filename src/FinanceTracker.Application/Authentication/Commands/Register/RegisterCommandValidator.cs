using FluentValidation;

namespace FinanceTracker.Application.Authentication.Commands.Register;

public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email обязателен.")
            .EmailAddress().WithMessage("Некорректный формат email.")
            .MaximumLength(254).WithMessage("Email не должен превышать 254 символа.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Пароль обязателен.")
            .MinimumLength(8).WithMessage("Пароль должен содержать не менее 8 символов.")
            .MaximumLength(128).WithMessage("Пароль не должен превышать 128 символов.")
            .Matches(@"[A-Z]").WithMessage("Пароль должен содержать хотя бы одну заглавную букву.")
            .Matches(@"[a-z]").WithMessage("Пароль должен содержать хотя бы одну строчную букву.")
            .Matches(@"\d").WithMessage("Пароль должен содержать хотя бы одну цифру.");

        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("Имя пользователя обязательно.")
            .MaximumLength(100).WithMessage("Имя пользователя не должно превышать 100 символов.");

        RuleFor(x => x.DisplayCurrency)
            .NotEmpty().WithMessage("Валюта по умолчанию обязательна.")
            .Must(FinanceTracker.Domain.ValueObjects.Currency.IsSupported)
            .WithMessage("Поддерживаются только валюты: BYN, USD, EUR, RUB.");
    }
}
