using FluentValidation;

namespace FinanceTracker.Application.Transactions.Commands.RecordTransfer;

public sealed class RecordTransferCommandValidator : AbstractValidator<RecordTransferCommand>
{
    public RecordTransferCommandValidator()
    {
        RuleFor(x => x.SourceAccountId).NotEmpty().WithMessage("Счёт-источник обязателен.");
        RuleFor(x => x.DestinationAccountId).NotEmpty().WithMessage("Счёт-получатель обязателен.");
        RuleFor(x => x)
            .Must(x => x.SourceAccountId != x.DestinationAccountId)
            .WithMessage("Счёт-источник и счёт-получатель должны различаться.");
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Сумма должна быть положительной.");
        RuleFor(x => x.OccurredAt).NotEmpty().WithMessage("Дата перевода обязательна.");
        RuleFor(x => x.Note).MaximumLength(500).WithMessage("Комментарий не должен превышать 500 символов.");
    }
}
