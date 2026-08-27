using FinanceTracker.Application.Common.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace FinanceTracker.Api.Realtime;

public sealed class SignalRRealtimeNotifier : IRealtimeNotifier
{
    private readonly IHubContext<UserHub> _hub;

    public SignalRRealtimeNotifier(IHubContext<UserHub> hub) => _hub = hub;

    public Task NotifyUserAsync(Guid userId, string eventName, object payload, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(eventName))
            throw new ArgumentException("Event name cannot be null or whitespace.", nameof(eventName));
        return _hub.Clients
            .Group(UserHub.GroupForUser(userId))
            .SendAsync(UserHub.ClientMethod, new { eventName, payload }, cancellationToken);
    }
}
