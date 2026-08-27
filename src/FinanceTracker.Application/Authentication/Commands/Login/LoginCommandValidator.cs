using FluentValidation;

namespace FinanceTracker.Application.Authentication.Commands.Login;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email обязателен.")
            .EmailAddress().WithMessage("Некорректный формат email.")
            .MaximumLength(254).WithMessage("Email не должен превышать 254 символа.");
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Пароль обязателен.")
            .MaximumLength(128).WithMessage("Пароль не должен превышать 128 символов.");
    }
}
