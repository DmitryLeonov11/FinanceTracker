using FinanceTracker.Application.Budgets.Helpers;
using FinanceTracker.Domain.Budgets;

namespace FinanceTracker.Domain.UnitTests.Application;

public class BudgetPeriodCalculatorTests
{
    [Fact]
    public void Month_window_should_start_on_first_day()
    {
        var (from, to) = BudgetPeriodCalculator.GetCurrentWindow(
            BudgetPeriod.Month,
            startDate: new DateOnly(2026, 1, 1),
            now: new DateTimeOffset(2026, 5, 15, 14, 0, 0, TimeSpan.Zero));

        from.UtcDateTime.Should().Be(new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc));
        to.UtcDateTime.Date.Should().Be(new DateTime(2026, 5, 31));
    }

    [Fact]
    public void Week_window_should_start_on_monday()
    {
        var (from, to) = BudgetPeriodCalculator.GetCurrentWindow(
            BudgetPeriod.Week,
            startDate: new DateOnly(2026, 1, 1),
            now: new DateTimeOffset(2026, 5, 13, 10, 0, 0, TimeSpan.Zero));

        from.UtcDateTime.DayOfWeek.Should().Be(DayOfWeek.Monday);
        from.UtcDateTime.Day.Should().Be(11);
        to.UtcDateTime.Date.Should().Be(new DateTime(2026, 5, 17));
    }

    [Fact]
    public void Week_window_when_today_is_sunday()
    {
        var (from, _) = BudgetPeriodCalculator.GetCurrentWindow(
            BudgetPeriod.Week,
            startDate: new DateOnly(2026, 1, 1),
            now: new DateTimeOffset(2026, 5, 17, 23, 0, 0, TimeSpan.Zero));

        from.UtcDateTime.DayOfWeek.Should().Be(DayOfWeek.Monday);
        from.UtcDateTime.Day.Should().Be(11);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(2, 1)]
    [InlineData(3, 1)]
    [InlineData(4, 4)]
    [InlineData(7, 7)]
    [InlineData(10, 10)]
    [InlineData(12, 10)]
    public void Quarter_window_should_align_to_calendar_quarters(int currentMonth, int expectedStartMonth)
    {
        var (from, _) = BudgetPeriodCalculator.GetCurrentWindow(
            BudgetPeriod.Quarter,
            startDate: new DateOnly(2026, 1, 1),
            now: new DateTimeOffset(2026, currentMonth, 15, 12, 0, 0, TimeSpan.Zero));

        from.UtcDateTime.Month.Should().Be(expectedStartMonth);
        from.UtcDateTime.Day.Should().Be(1);
    }

    [Fact]
    public void Year_window_should_start_on_january_first()
    {
        var (from, to) = BudgetPeriodCalculator.GetCurrentWindow(
            BudgetPeriod.Year,
            startDate: new DateOnly(2026, 1, 1),
            now: new DateTimeOffset(2026, 8, 20, 9, 0, 0, TimeSpan.Zero));

        from.UtcDateTime.Should().Be(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        to.UtcDateTime.Date.Should().Be(new DateTime(2026, 12, 31));
    }

    [Fact]
    public void First_partial_window_should_end_at_calendar_boundary()
    {
        var (from, to) = BudgetPeriodCalculator.GetCurrentWindow(
            BudgetPeriod.Month,
            startDate: new DateOnly(2026, 6, 15),
            now: new DateTimeOffset(2026, 6, 20, 12, 0, 0, TimeSpan.Zero));

        from.UtcDateTime.Should().Be(new DateTime(2026, 6, 15, 0, 0, 0, DateTimeKind.Utc));
        to.UtcDateTime.Date.Should().Be(new DateTime(2026, 6, 30));
    }

    [Fact]
    public void StartDate_in_future_should_clamp_today_up_to_start()
    {
        var (from, _) = BudgetPeriodCalculator.GetCurrentWindow(
            BudgetPeriod.Month,
            startDate: new DateOnly(2027, 1, 1),
            now: new DateTimeOffset(2026, 5, 15, 0, 0, 0, TimeSpan.Zero));

        from.UtcDateTime.Should().BeOnOrAfter(new DateTime(2027, 1, 1, 0, 0, 0, DateTimeKind.Utc));
    }
}
