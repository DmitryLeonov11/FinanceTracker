using FinanceTracker.Domain.ValueObjects;
using FluentValidation;

namespace FinanceTracker.Application.Fx.Queries.ConvertMoney;

public sealed class ConvertMoneyQueryValidator : AbstractValidator<ConvertMoneyQuery>
{
    public ConvertMoneyQueryValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Сумма должна быть положительной.");
        RuleFor(x => x.From)
            .NotEmpty().WithMessage("Исходная валюта обязательна.")
            .Must(Currency.IsSupported)
            .WithMessage("Поддерживаются только валюты: BYN, USD, EUR, RUB.");
        RuleFor(x => x.To)
            .NotEmpty().WithMessage("Целевая валюта обязательна.")
            .Must(Currency.IsSupported)
            .WithMessage("Поддерживаются только валюты: BYN, USD, EUR, RUB.");
    }
}
