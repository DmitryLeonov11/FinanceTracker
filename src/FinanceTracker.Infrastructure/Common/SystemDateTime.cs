using FinanceTracker.Application.Common.Interfaces;

namespace FinanceTracker.Infrastructure.Common;

public sealed class SystemDateTime : IDateTime
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
