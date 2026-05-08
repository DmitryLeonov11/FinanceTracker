using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace FinanceTracker.Api.Realtime;

[Authorize]
public sealed class UserHub : Hub
{
    public const string Path = "/hubs/user";
    public const string ClientMethod = "ReceiveEvent";

    public static string GroupForUser(Guid userId) => $"user:{userId}";

    public override async Task OnConnectedAsync()
    {
        var userId = ResolveUserId();
        if (userId is null)
        {
            Context.Abort();
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, GroupForUser(userId.Value));
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = ResolveUserId();
        if (userId is not null)
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupForUser(userId.Value));

        await base.OnDisconnectedAsync(exception);
    }

    private Guid? ResolveUserId()
    {
        var raw = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(raw, out var id) ? id : null;
    }
}
