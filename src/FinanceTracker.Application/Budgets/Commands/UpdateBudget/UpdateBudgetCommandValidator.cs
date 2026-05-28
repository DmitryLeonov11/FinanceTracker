using FluentValidation;

namespace FinanceTracker.Application.Budgets.Commands.UpdateBudget;

public sealed class UpdateBudgetCommandValidator : AbstractValidator<UpdateBudgetCommand>
{
    public UpdateBudgetCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Идентификатор бюджета обязателен.");
        When(x => x.Name is not null, () =>
        {
            RuleFor(x => x.Name!)
                .NotEmpty().WithMessage("Название не может быть пустым.")
                .MaximumLength(100).WithMessage("Название не должно превышать 100 символов.");
        });
        When(x => x.Limit.HasValue, () =>
        {
            RuleFor(x => x.Limit!.Value)
                .GreaterThan(0).WithMessage("Лимит должен быть больше нуля.");
        });
        RuleFor(x => x)
            .Must(x => x.Name is not null || x.Limit.HasValue)
            .WithMessage("Укажите хотя бы одно поле для изменения.");
    }
}
