using FluentValidation;

namespace FinanceTracker.Application.Transactions.Commands.DeleteTransaction;

public sealed class DeleteTransactionCommandValidator : AbstractValidator<DeleteTransactionCommand>
{
    public DeleteTransactionCommandValidator()
    {
        RuleFor(x => x.TransactionId).NotEmpty().WithMessage("Идентификатор операции обязателен.");
    }
}
