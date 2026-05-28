using FinanceTracker.Domain.ValueObjects;
using FluentValidation;

namespace FinanceTracker.Application.Budgets.Commands.CreateBudget;

public sealed class CreateBudgetCommandValidator : AbstractValidator<CreateBudgetCommand>
{
    public CreateBudgetCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Название бюджета обязательно.")
            .MaximumLength(100).WithMessage("Название не должно превышать 100 символов.");

        RuleFor(x => x.Period)
            .IsInEnum().WithMessage("Период должен быть Week, Month, Quarter или Year.");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Валюта обязательна.")
            .Must(Currency.IsSupported)
            .WithMessage("Поддерживаются только валюты: BYN, USD, EUR, RUB.");

        RuleFor(x => x.Limit)
            .GreaterThan(0).WithMessage("Лимит должен быть больше нуля.");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Дата старта обязательна.");
    }
}
