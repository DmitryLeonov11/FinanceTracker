using System.Text;
using FinanceTracker.Application.Authentication.Models;
using MediatR;

namespace FinanceTracker.Application.Authentication.Commands.Login;

public sealed record LoginCommand(string Email, string Password) : IRequest<AuthResult>
{
    private bool PrintMembers(StringBuilder builder)
    {
        builder.Append("Email = REDACTED, Password = REDACTED");
        return true;
    }
}
