
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Moq;
using Products.Application.Portfolios.QueriesHandlers;
using Products.Domain.Portfolios;
using Products.Integrations.Portfolios;
using Products.Integrations.Portfolios.Queries;


namespace Products.test.UnitTests.Application.Portfolios.QueriesHandlers;

public sealed class GetPortfolioInfoForClosingQueryHandlerTests
{
    private readonly Mock<IPortfolioRepository> portfolioRepositoryMock;
    private readonly GetPortfolioInfoForClosingQueryHandler handler;

    public GetPortfolioInfoForClosingQueryHandlerTests()
    {
        portfolioRepositoryMock = new Mock<IPortfolioRepository>();
        handler = new GetPortfolioInfoForClosingQueryHandler(portfolioRepositoryMock.Object);
    }

    [Fact]
    public async Task HandleReturnsFailureWhenRequestIsNull()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        Result<PortfolioInfoForClosingResponse> result =
            await handler.Handle(null!, cancellationToken);

        // Assert
        Assert.True(result.IsFailure);
        Assert.False(result.IsSuccess);

        Assert.Equal("GetPortfolioInfoForClosing.NullRequest", result.Error.Code);
        Assert.Equal("La petición no puede ser null.", result.Error.Description);

        Assert.Equal(string.Empty, result.Description);

        portfolioRepositoryMock.Verify(
            r => r.GetAgileWithdrawalPercentageProtectedBalanceAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleReturnsSuccessWhenRequestIsValid()
    {
        // Arrange
        int portfolioId = 7;
        int agileWithdrawalPercentageProtectedBalance = 12;

        using var cancellationTokenSource = new CancellationTokenSource();
        CancellationToken cancellationToken = cancellationTokenSource.Token;

        portfolioRepositoryMock
            .Setup(r => r.GetAgileWithdrawalPercentageProtectedBalanceAsync(
                portfolioId,
                cancellationToken))
            .ReturnsAsync(agileWithdrawalPercentageProtectedBalance);

        var query = new GetPortfolioInfoForClosingQuery(portfolioId);

        // Act
        Result<PortfolioInfoForClosingResponse> result =
            await handler.Handle(query, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(Error.None, result.Error);
        Assert.Equal(string.Empty, result.Description);

        var expectedResponse = new PortfolioInfoForClosingResponse(
            portfolioId,
            agileWithdrawalPercentageProtectedBalance);

        Assert.Equal(expectedResponse, result.Value);

        portfolioRepositoryMock.Verify(
            r => r.GetAgileWithdrawalPercentageProtectedBalanceAsync(
                portfolioId,
                cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task HandleHonorsCancellationToken()
    {
        // Arrange
        int portfolioId = 10;

        using var cancellationTokenSource = new CancellationTokenSource();
        CancellationToken cancellationToken = cancellationTokenSource.Token;

        portfolioRepositoryMock
            .Setup(r => r.GetAgileWithdrawalPercentageProtectedBalanceAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .Returns<long, CancellationToken>((_, token) =>
            {
                token.ThrowIfCancellationRequested();
                return Task.FromResult(0);
            });

        var query = new GetPortfolioInfoForClosingQuery(portfolioId);

        cancellationTokenSource.Cancel();

        // Act + Assert
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
        {
            _ = await handler.Handle(query, cancellationToken);
        });

        portfolioRepositoryMock.Verify(
            r => r.GetAgileWithdrawalPercentageProtectedBalanceAsync(
                portfolioId,
                It.Is<CancellationToken>(t => t.IsCancellationRequested)),
            Times.Once);
    }
}

