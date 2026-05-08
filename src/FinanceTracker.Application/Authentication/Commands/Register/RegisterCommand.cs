using FinanceTracker.Application.Authentication.Models;
using MediatR;

namespace FinanceTracker.Application.Authentication.Commands.Register;

public sealed record RegisterCommand(
    string Email,
    string Password,
    string DisplayName,
    string DisplayCurrency) : IRequest<AuthResult>;
