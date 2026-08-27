using FinanceTracker.Application.Authentication.Models;
using MediatR;

namespace FinanceTracker.Application.Authentication.Commands.RefreshToken;

public sealed record RefreshTokenCommand(string RefreshToken) : IRequest<AuthResult>;
