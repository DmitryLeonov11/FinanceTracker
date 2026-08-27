using FinanceTracker.Application.Common.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace FinanceTracker.Api.Realtime;

public sealed class SignalRRealtimeNotifier : IRealtimeNotifier
{
    private readonly IHubContext<UserHub> _hub;
    private readonly ILogger<SignalRRealtimeNotifier> _logger;

    public SignalRRealtimeNotifier(IHubContext<UserHub> hub, ILogger<SignalRRealtimeNotifier> logger)
    {
        _hub = hub;
        _logger = logger;
    }

    public async Task NotifyUserAsync(Guid userId, string eventName, object payload, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(eventName))
            throw new ArgumentException("Event name cannot be null or whitespace.", nameof(eventName));

        try
        {
            await _hub.Clients
                .Group(UserHub.GroupForUser(userId))
                .SendAsync(UserHub.ClientMethod, new { eventName, payload }, cancellationToken);
        }
        catch (Exception ex)
        {
            // Сбой realtime-пуша не должен откатывать уже закоммиченную команду: данные сохранены,
            // клиент досинхронизируется при следующем запросе или переподключении.
            _logger.LogWarning(ex, "Не удалось отправить realtime-уведомление '{EventName}' пользователю {UserId}", eventName, userId);
        }
    }
}
