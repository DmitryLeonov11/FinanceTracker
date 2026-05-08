using FinanceTracker.Application.Common.Interfaces;
using FinanceTracker.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace FinanceTracker.Infrastructure.Persistence.Interceptors;

public sealed class DispatchDomainEventsInterceptor : SaveChangesInterceptor
{
    private readonly IRealtimeNotifier _notifier;
    private readonly ILogger<DispatchDomainEventsInterceptor> _logger;

    public DispatchDomainEventsInterceptor(IRealtimeNotifier notifier, ILogger<DispatchDomainEventsInterceptor> logger)
    {
        _notifier = notifier;
        _logger = logger;
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
            await DispatchAsync(eventData.Context, cancellationToken);

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    private async Task DispatchAsync(DbContext context, CancellationToken cancellationToken)
    {
        var aggregates = context.ChangeTracker
            .Entries<AggregateRoot>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        var events = aggregates.SelectMany(a => a.DomainEvents).ToList();
        foreach (var aggregate in aggregates)
            aggregate.ClearDomainEvents();

        foreach (var domainEvent in events)
        {
            try
            {
                var userId = ResolveUserId(domainEvent);
                if (userId is null) continue;
                await _notifier.NotifyUserAsync(userId.Value, domainEvent.GetType().Name, domainEvent, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Не удалось доставить доменное событие {EventType}", domainEvent.GetType().Name);
            }
        }
    }

    private static Guid? ResolveUserId(IDomainEvent domainEvent)
    {
        var prop = domainEvent.GetType().GetProperty("UserId");
        return prop?.GetValue(domainEvent) as Guid?;
    }
}
