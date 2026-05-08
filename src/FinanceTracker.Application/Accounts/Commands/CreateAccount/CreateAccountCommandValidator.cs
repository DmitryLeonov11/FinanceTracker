using FluentValidation;

namespace FinanceTracker.Application.Accounts.Commands.CreateAccount;

public sealed class CreateAccountCommandValidator : AbstractValidator<CreateAccountCommand>
{
    public CreateAccountCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Название счёта обязательно.")
            .MaximumLength(100).WithMessage("Название счёта не должно превышать 100 символов.");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Тип счёта должен быть одним из: Cash, Bank, Card, Crypto, Other.");

        RuleFor(x => x.Currency)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Валюта обязательна.")
            .Must(FinanceTracker.Domain.ValueObjects.Currency.IsSupported)
            .WithMessage("Поддерживаются только валюты: BYN, USD, EUR, RUB.");

        RuleFor(x => x.InitialBalance)
            .GreaterThanOrEqualTo(0).WithMessage("Начальный баланс не может быть отрицательным.");

        RuleFor(x => x.InitialBalance)
            .GreaterThanOrEqualTo(0).WithMessage("Начальный баланс не может быть отрицательным.");
    }
}
