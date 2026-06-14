using FluentValidation;

namespace FinanceTracker.Application.Categories.Commands.CreateCategory;

public sealed class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Название категории обязательно.")
            .MaximumLength(100).WithMessage("Название категории не должно превышать 100 символов.");

        RuleFor(x => x.Kind)
            .IsInEnum().WithMessage("Тип категории должен быть Income или Expense.");

        RuleFor(x => x.Icon)
            .MaximumLength(50).WithMessage("Имя иконки не должно превышать 50 символов.");

        RuleFor(x => x.Color)
            .MaximumLength(20).WithMessage("Код цвета не должен превышать 20 символов.");
    }
}
