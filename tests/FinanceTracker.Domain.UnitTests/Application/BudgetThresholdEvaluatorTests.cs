using FinanceTracker.Application.Budgets.Helpers;

namespace FinanceTracker.Domain.UnitTests.Application;

public class BudgetThresholdEvaluatorTests
{
    [Fact]
    public void Crossing_50_should_return_50()
    {
        var result = BudgetThresholdEvaluator.CrossedThresholds(40m, 60m, 100m);
        result.Should().Equal(50);
    }

    [Fact]
    public void Crossing_50_and_80_in_one_jump_should_return_both()
    {
        var result = BudgetThresholdEvaluator.CrossedThresholds(40m, 85m, 100m);
        result.Should().Equal(50, 80);
    }

    [Fact]
    public void Crossing_all_three_in_one_jump_should_return_all()
    {
        var result = BudgetThresholdEvaluator.CrossedThresholds(40m, 110m, 100m);
        result.Should().Equal(50, 80, 100);
    }

    [Fact]
    public void Movement_already_above_a_threshold_should_not_re_emit()
    {
        var result = BudgetThresholdEvaluator.CrossedThresholds(60m, 70m, 100m);
        result.Should().BeEmpty();
    }

    [Fact]
    public void Landing_exactly_on_threshold_should_emit()
    {
        var result = BudgetThresholdEvaluator.CrossedThresholds(0m, 50m, 100m);
        result.Should().Equal(50);
    }

    [Fact]
    public void No_movement_should_emit_nothing()
    {
        var result = BudgetThresholdEvaluator.CrossedThresholds(80m, 80m, 100m);
        result.Should().BeEmpty();
    }

    [Fact]
    public void Downward_movement_should_emit_nothing()
    {
        var result = BudgetThresholdEvaluator.CrossedThresholds(90m, 60m, 100m);
        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public void Non_positive_limit_should_emit_nothing(decimal limit)
    {
        var result = BudgetThresholdEvaluator.CrossedThresholds(0m, 100m, limit);
        result.Should().BeEmpty();
    }

    [Fact]
    public void Realistic_example_400_to_600_on_1000_limit_emits_50()
    {
        var result = BudgetThresholdEvaluator.CrossedThresholds(400m, 600m, 1000m);
        result.Should().Equal(50);
    }

    [Fact]
    public void Realistic_example_600_to_900_on_1000_limit_emits_80()
    {
        var result = BudgetThresholdEvaluator.CrossedThresholds(600m, 900m, 1000m);
        result.Should().Equal(80);
    }

    [Fact]
    public void Realistic_example_900_to_1100_on_1000_limit_emits_100()
    {
        var result = BudgetThresholdEvaluator.CrossedThresholds(900m, 1100m, 1000m);
        result.Should().Equal(100);
    }
}
