using MediatR;

namespace FinanceTracker.Application.Transactions.Commands.DeleteTransaction;

public sealed record DeleteTransactionCommand(Guid TransactionId) : IRequest;
