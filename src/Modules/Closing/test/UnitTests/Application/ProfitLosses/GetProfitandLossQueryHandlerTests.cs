using Closing.Application.Abstractions.External;
using Closing.Application.ProfitLosses.GetProfitandLoss;
using Closing.Domain.ProfitLossConcepts;
using Closing.Domain.ProfitLosses;
using Closing.Integrations.ProfitLosses.GetProfitandLoss;
using Common.SharedKernel.Domain;
using FluentAssertions;
using Moq;

namespace Closing.test.UnitTests.Application.ProfitLosses;

public class GetProfitandLossQueryHandlerTests
{
    private readonly Mock<IProfitLossRepository> _profitLossRepo = new();
    private readonly Mock<IPortfolioValidator> _validator = new();

    private GetProfitandLossQueryHandler Build()
    {
        return new GetProfitandLossQueryHandler(
            _profitLossRepo.Object,
            _validator.Object);
    }


    [Fact]
    public async Task Handle_Should_Return_Summary_With_Net_Value()
    {
        // arrange
        var date = DateTime.Today;
        var summaries = new[]
        {
            new ProfitLossSummary("Ingreso", ProfitLossNature.Income, 100m),
            new ProfitLossSummary("Gasto", ProfitLossNature.Expense, 40m)
        };
        _profitLossRepo.Setup(r => r.GetSummaryAsync(1, date, It.IsAny<CancellationToken>()))
            .ReturnsAsync(summaries);

        _validator.Setup(v => v.EnsureExistsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var handler = Build();
        var query = new GetProfitandLossQuery(1, date);

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Values["Ingreso"].Should().Be(100m);
        result.Value.Values["Gasto"].Should().Be(40m);
        result.Value.NetYield.Should().Be(60m);
    }
}