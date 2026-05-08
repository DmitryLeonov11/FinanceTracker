using FinanceTracker.Application.Authentication.Commands.Login;
using FinanceTracker.Application.Authentication.Commands.RefreshToken;
using FinanceTracker.Application.Authentication.Commands.Register;
using FinanceTracker.Application.Authentication.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceTracker.Api.Controllers;

[AllowAnonymous]
[Route("api/auth")]
public sealed class AuthController : ApiControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AuthResult>> Register([FromBody] RegisterCommand command, CancellationToken cancellationToken)
        => Ok(await Sender.Send(command, cancellationToken));

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AuthResult>> Login([FromBody] LoginCommand command, CancellationToken cancellationToken)
        => Ok(await Sender.Send(command, cancellationToken));

    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AuthResult>> Refresh([FromBody] RefreshTokenCommand command, CancellationToken cancellationToken)
        => Ok(await Sender.Send(command, cancellationToken));
}
