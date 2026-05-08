using FinanceTracker.Application.Transactions.Commands.AddTransaction;
using FinanceTracker.Application.Transactions.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceTracker.Api.Controllers;

[Authorize]
[Route("api/transactions")]
public sealed class TransactionsController : ApiControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<TransactionDto>> Add([FromBody] AddTransactionCommand command, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);
        return Created(string.Empty, result);
    }
}
