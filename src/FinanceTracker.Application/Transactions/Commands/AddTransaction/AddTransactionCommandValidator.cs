using FinanceTracker.Domain.Transactions;
using FluentValidation;

namespace FinanceTracker.Application.Transactions.Commands.AddTransaction;

public sealed class AddTransactionCommandValidator : AbstractValidator<AddTransactionCommand>
{
    public AddTransactionCommandValidator()
    {
        RuleFor(x => x.AccountId).NotEmpty().WithMessage("Счёт обязателен.");
        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Тип операции должен быть Income или Expense.")
            .Must(t => t is TransactionType.Income or TransactionType.Expense)
            .WithMessage("Для переводов между счетами используйте отдельный endpoint.");
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Сумма должна быть положительной.");
        RuleFor(x => x.OccurredAt).NotEmpty().WithMessage("Дата операции обязательна.");
        RuleFor(x => x.Note).MaximumLength(500).WithMessage("Комментарий не должен превышать 500 символов.");
    }
}
