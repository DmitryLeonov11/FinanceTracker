namespace FinanceTracker.Application.Common.Interfaces;

public interface IRealtimeNotifier
{
    Task NotifyUserAsync(Guid userId, string eventName, object payload, CancellationToken cancellationToken = default);
}
