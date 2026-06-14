namespace FinanceTracker.Application.Budgets.Helpers;

/// <summary>
/// Pure logic for detecting which budget thresholds (50% / 80% / 100%) were
/// crossed upward in a single transaction (previousSpent → newSpent).
/// </summary>
public static class BudgetThresholdEvaluator
{
    private static readonly int[] Thresholds = [50, 80, 100];

    public static IReadOnlyList<int> CrossedThresholds(decimal previousSpent, decimal newSpent, decimal limit)
    {
        if (limit <= 0m) return Array.Empty<int>();
        if (newSpent <= previousSpent) return Array.Empty<int>();

        var crossed = new List<int>(Thresholds.Length);
        foreach (var threshold in Thresholds)
        {
            var bound = limit * threshold / 100m;
            if (previousSpent < bound && newSpent >= bound)
                crossed.Add(threshold);
        }
        return crossed;
    }
}
