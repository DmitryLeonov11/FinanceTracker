using FinanceTracker.Application.Common.Models;
using FinanceTracker.Application.Transactions.Commands.AddTransaction;
using FinanceTracker.Application.Transactions.Commands.DeleteTransaction;
using FinanceTracker.Application.Transactions.Commands.EditTransaction;
using FinanceTracker.Application.Transactions.Commands.RecordTransfer;
using FinanceTracker.Application.Transactions.Models;
using FinanceTracker.Application.Transactions.Queries.GetTransactions;
using FinanceTracker.Domain.Transactions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceTracker.Api.Controllers;

[Authorize]
[Route("api/transactions")]
public sealed class TransactionsController : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<TransactionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<TransactionDto>>> List(
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        [FromQuery(Name = "accountId")] Guid[]? accountIds,
        [FromQuery(Name = "categoryId")] Guid[]? categoryIds,
        [FromQuery(Name = "type")] TransactionType[]? types,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var query = new GetTransactionsQuery(
            from,
            to,
            accountIds is { Length: > 0 } ? accountIds : null,
            categoryIds is { Length: > 0 } ? categoryIds : null,
            types is { Length: > 0 } ? types : null,
            search,
            page,
            pageSize);

        return Ok(await Sender.Send(query, cancellationToken));
    }

    [HttpPost]
    [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<TransactionDto>> Add([FromBody] AddTransactionCommand command, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);
        return Created(string.Empty, result);
    }

    public sealed record EditTransactionRequest(
        decimal Amount,
        DateTimeOffset OccurredAt,
        Guid? CategoryId,
        string? Note);

    [HttpPatch("{id:guid}")]
    [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TransactionDto>> Edit(
        [FromRoute] Guid id,
        [FromBody] EditTransactionRequest body,
        CancellationToken cancellationToken)
    {
        var command = new EditTransactionCommand(id, body.Amount, body.OccurredAt, body.CategoryId, body.Note);
        return Ok(await Sender.Send(command, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        await Sender.Send(new DeleteTransactionCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("transfer")]
    [ProducesResponseType(typeof(TransferDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TransferDto>> Transfer(
        [FromBody] RecordTransferCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);
        return Created(string.Empty, result);
    }
}
