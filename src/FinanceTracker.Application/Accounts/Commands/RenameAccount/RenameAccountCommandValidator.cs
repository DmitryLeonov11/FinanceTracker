using FluentValidation;

namespace FinanceTracker.Application.Accounts.Commands.RenameAccount;

public sealed class RenameAccountCommandValidator : AbstractValidator<RenameAccountCommand>
{
    public RenameAccountCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Идентификатор счёта обязателен.");
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Название обязательно.")
            .MaximumLength(100).WithMessage("Название не должно превышать 100 символов.");
    }
}
