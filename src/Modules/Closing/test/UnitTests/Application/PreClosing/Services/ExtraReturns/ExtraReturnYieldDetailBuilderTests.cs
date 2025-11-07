using System.Text.Json;

using Closing.Application.PreClosing.Services.ExtraReturns.Dto;
using Closing.Application.PreClosing.Services.Yield.Builders;
using Closing.Application.PreClosing.Services.Yield.Constants;
using Closing.Application.PreClosing.Services.Yield.Dto;
using Closing.Integrations.PreClosing.RunSimulation;

using FluentAssertions;

namespace Closing.test.UnitTests.Application.PreClosing.Services.ExtraReturns;

public class ExtraReturnYieldDetailBuilderTests
{
    private readonly ExtraReturnYieldDetailBuilder builder = new();

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Build_ShouldCreateYieldDetailWithExpectedConcept(bool isClosing)
    {
        // Arrange
        var summary = new ExtraReturnSummary(
            PortfolioId: 10,
            ProcessDateUtc: new DateTime(2024, 5, 10, 0, 0, 0, DateTimeKind.Utc),
            OperationTypeId: 77,
            OperationTypeName: "Rendimientos",
            Amount: -1500.25m,
            Concept: JsonSerializer.Serialize(new StringEntityDto("77", "Rendimientos")));

        var parameters = new RunSimulationParameters(
            PortfolioId: 10,
            ClosingDate: new DateTime(2024, 5, 31),
            IsClosing: isClosing);

        // Act
        var yieldDetail = builder.Build(summary, parameters);

        // Assert
        yieldDetail.Should().NotBeNull();
        yieldDetail.Source.Should().Be(YieldsSources.ExtraReturn);
        yieldDetail.IsClosed.Should().Be(isClosing);
        yieldDetail.Income.Should().Be(1500.25m);
        yieldDetail.Expenses.Should().Be(0);
        yieldDetail.Commissions.Should().Be(0);
        yieldDetail.ProcessDate.Should().Be(summary.ProcessDateUtc);

        yieldDetail.Concept.RootElement.GetProperty("EntityId").GetString().Should().Be("77");
        yieldDetail.Concept.RootElement.GetProperty("EntityValue").GetString().Should().Be("Rendimientos");
    }

    [Fact]
    public void CanHandle_ShouldReturnTrueForExtraReturnSummary()
    {
        builder.CanHandle(typeof(ExtraReturnSummary)).Should().BeTrue();
    }
}
