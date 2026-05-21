using FluentValidation;

namespace FinanceTracker.Application.Transactions.Commands.EditTransaction;

public sealed class EditTransactionCommandValidator : AbstractValidator<EditTransactionCommand>
{
    public EditTransactionCommandValidator()
    {
        RuleFor(x => x.TransactionId).NotEmpty().WithMessage("Идентификатор операции обязателен.");
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Сумма должна быть положительной.");
        RuleFor(x => x.OccurredAt).NotEmpty().WithMessage("Дата операции обязательна.");
        RuleFor(x => x.Note).MaximumLength(500).WithMessage("Комментарий не должен превышать 500 символов.");
    }
}
