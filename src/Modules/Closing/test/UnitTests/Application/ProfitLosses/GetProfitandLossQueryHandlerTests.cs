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
            new ProfitLossSummary("Ingreso", IncomeExpenseNature.Income, 100m),
            new ProfitLossSummary("Gasto", IncomeExpenseNature.Expense, 40m)
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

    [Fact]
    public async Task Handle_Should_Return_Error_When_Portfolio_Does_Not_Exist()
    {
        // arrange
        var date = DateTime.Today;
        var portfolioError = Error.NotFound("Portfolio.NotFound", "Portfolio not found");

        _validator.Setup(v => v.EnsureExistsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(portfolioError));

        var handler = Build();
        var query = new GetProfitandLossQuery(1, date);

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be(portfolioError.Code);
        result.Error.Description.Should().Be(portfolioError.Description);
        _profitLossRepo.Verify(r => r.GetSummaryAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Calculate_Correct_Net_Yield_With_Multiple_Incomes_And_Expenses()
    {
        // arrange
        var date = DateTime.Today;
        var summaries = new[]
        {
            new ProfitLossSummary("Rendimientos Brutos", IncomeExpenseNature.Income, 1000m),
            new ProfitLossSummary("Dividendos", IncomeExpenseNature.Income, 500m),
            new ProfitLossSummary("Gastos Administrativos", IncomeExpenseNature.Expense, 200m),
            new ProfitLossSummary("Comisiones", IncomeExpenseNature.Expense, 150m)
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
        result.Value.Values.Should().HaveCount(4);
        result.Value.Values["Rendimientos Brutos"].Should().Be(1000m);
        result.Value.Values["Dividendos"].Should().Be(500m);
        result.Value.Values["Gastos Administrativos"].Should().Be(200m);
        result.Value.Values["Comisiones"].Should().Be(150m);
        result.Value.NetYield.Should().Be(1150m);
    }

    [Fact]
    public async Task Handle_Should_Return_Empty_Values_When_No_Data_Found()
    {
        // arrange
        var date = DateTime.Today;
        var summaries = Array.Empty<ProfitLossSummary>();

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
        result.Value.Values.Should().BeEmpty();
        result.Value.NetYield.Should().Be(0m);
    }

    [Fact]
    public async Task Handle_Should_Handle_Negative_Net_Yield()
    {
        // arrange
        var date = DateTime.Today;
        var summaries = new[]
        {
            new ProfitLossSummary("Rendimientos Brutos", IncomeExpenseNature.Income, 300m),
            new ProfitLossSummary("Gastos Administrativos", IncomeExpenseNature.Expense, 500m)
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
        result.Value.NetYield.Should().Be(-200m);
    }
}