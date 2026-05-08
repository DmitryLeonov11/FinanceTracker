namespace FinanceTracker.Application.Common.Interfaces;

public interface IDateTime
{
    DateTimeOffset UtcNow { get; }
}
