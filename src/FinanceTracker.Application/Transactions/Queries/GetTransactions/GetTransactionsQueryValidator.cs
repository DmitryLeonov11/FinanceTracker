using FluentValidation;

namespace FinanceTracker.Application.Transactions.Queries.GetTransactions;

public sealed class GetTransactionsQueryValidator : AbstractValidator<GetTransactionsQuery>
{
    public GetTransactionsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1).WithMessage("Номер страницы должен быть не меньше 1.");
        RuleFor(x => x.PageSize).InclusiveBetween(1, 200).WithMessage("Размер страницы — от 1 до 200.");
        RuleFor(x => x.Search).MaximumLength(200).WithMessage("Поисковая строка не должна превышать 200 символов.");
        When(x => x.From.HasValue && x.To.HasValue, () =>
        {
            RuleFor(x => x).Must(x => x.From <= x.To)
                .WithMessage("Дата начала должна быть не позже даты окончания.");
        });
    }
}
