using FluentValidation;

namespace FinanceTracker.Application.Accounts.Queries.GetAccountBalanceHistory;

public sealed class GetAccountBalanceHistoryQueryValidator : AbstractValidator<GetAccountBalanceHistoryQuery>
{
    public GetAccountBalanceHistoryQueryValidator()
    {
        RuleFor(x => x.Days)
            .InclusiveBetween(7, 365)
            .WithMessage("Период должен быть от 7 до 365 дней.");
    }
}
