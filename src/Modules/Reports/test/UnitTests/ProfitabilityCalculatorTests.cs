using System;
using Common.SharedKernel.Application.Helpers.Finance;
using FluentAssertions;
using Xunit;

namespace MFFVP.Modules.Reports.test.UnitTests;

public class ProfitabilityCalculatorTests
{
    private readonly IProfitabilityCalculator calc = new ProfitabilityCalculator();

    [Theory]
    [InlineData(1.05, 1.00, 30)]
    [InlineData(1.10, 1.00, 180)]
    [InlineData(0.98, 1.00, 365)]
    public void AnnualizedPercentage_ShouldMatchFormula(double finalD, double initialD, int days)
    {
        var expected = (decimal)((Math.Pow(finalD / initialD, 365d / days) - 1d) * 100d);
        var actual = calc.AnnualizedPercentage((decimal)finalD, (decimal)initialD, days);

        // Compare with rounding to avoid tiny floating errors on casting
        Math.Round(actual, 8).Should().Be(Math.Round(expected, 8));
    }

    [Fact]
    public void AnnualizedPercentage_ShouldReturnZero_WhenInitialIsZero()
    {
        var actual = calc.AnnualizedPercentage(1.05m, 0m, 30);
        actual.Should().Be(0m);
    }

    [Fact]
    public void AnnualizedPercentage_ShouldReturnZero_WhenInitialIsNegative()
    {
        var actual = calc.AnnualizedPercentage(1.05m, -1m, 30);
        actual.Should().Be(0m);
    }

    [Fact]
    public void AnnualizedPercentage_ShouldReturnZero_WhenDaysIsZeroOrNegative()
    {
        calc.AnnualizedPercentage(1.05m, 1m, 0).Should().Be(0m);
        calc.AnnualizedPercentage(1.05m, 1m, -30).Should().Be(0m);
    }
}

