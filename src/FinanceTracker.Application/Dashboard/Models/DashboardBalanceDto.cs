namespace FinanceTracker.Application.Dashboard.Models;

public sealed record DashboardBalanceDto(
    string DisplayCurrency,
    IReadOnlyCollection<CurrencyBalance> BalancesByCurrency,
    IReadOnlyCollection<AccountBalance> Accounts);

public sealed record CurrencyBalance(string Currency, decimal Total, int AccountCount);

public sealed record AccountBalance(
    Guid AccountId,
    string Name,
    string Currency,
    decimal Balance);
