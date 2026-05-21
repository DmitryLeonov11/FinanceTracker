using FinanceTracker.Domain.ValueObjects;
using FluentValidation;

namespace FinanceTracker.Application.Dashboard.Queries.GetCashflow;

public sealed class GetCashflowQueryValidator : AbstractValidator<GetCashflowQuery>
{
    public GetCashflowQueryValidator()
    {
        RuleFor(x => x.Days)
            .InclusiveBetween(1, 730)
            .WithMessage("Период должен быть от 1 до 730 дней.");

        When(x => !string.IsNullOrWhiteSpace(x.Currency), () =>
        {
            RuleFor(x => x.Currency!)
                .Must(Currency.IsSupported)
                .WithMessage("Поддерживаются только валюты: BYN, USD, EUR, RUB.");
        });
    }
}
