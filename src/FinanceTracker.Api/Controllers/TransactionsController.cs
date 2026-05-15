using FinanceTracker.Application.Common.Models;
using FinanceTracker.Application.Transactions.Commands.AddTransaction;
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
}
