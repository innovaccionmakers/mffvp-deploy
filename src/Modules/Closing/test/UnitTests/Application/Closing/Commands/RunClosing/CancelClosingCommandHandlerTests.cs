

using AutoFixture;
using AutoFixture.AutoMoq;
using Closing.Application.Closing.Commands.RunClosing;
using Closing.Application.Closing.Services.Orchestation.Interfaces;
using Closing.Integrations.Closing.RunClosing;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;
using Moq;

namespace Closing.test.UnitTests.Application.Closing.Commands.RunClosing;

public class CancelClosingCommandHandlerTests
{
    private readonly IFixture fixture;
    private readonly Mock<ICancelClosingOrchestrator> orchestratorMock;
    private readonly Mock<ILogger<CancelClosingCommandHandler>> loggerMock;

    public CancelClosingCommandHandlerTests()
    {
        fixture = new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });
        orchestratorMock = fixture.Freeze<Mock<ICancelClosingOrchestrator>>();
        loggerMock = fixture.Freeze<Mock<ILogger<CancelClosingCommandHandler>>>();
    }

    [Fact]
    public async Task HandleReturnsSameResultInstanceAndSuccessProperties()
    {
        // Arrange
        var portfolioId = fixture.Create<int>();
        var closingDate = fixture.Create<DateTime>();
        var command = new CancelClosingCommand(portfolioId, closingDate);

        var payload = fixture.Create<CancelClosingResult>();
        var expected = Result.Success(payload); 

        orchestratorMock
            .Setup(o => o.CancelAsync(portfolioId, closingDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var sut = new CancelClosingCommandHandler(orchestratorMock.Object, loggerMock.Object);

        // Act
        var result = await sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.Same(expected, result);
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(Error.None, result.Error);
        Assert.Same(payload, result.Value); 

        orchestratorMock.Verify(o => o.CancelAsync(portfolioId, closingDate, It.IsAny<CancellationToken>()), Times.Once);
        loggerMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HandleDoesNotCallOrchestratorWhenCancellationAlreadyRequested()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var command = new CancelClosingCommand(fixture.Create<int>(), fixture.Create<DateTime>());
        var sut = new CancelClosingCommandHandler(orchestratorMock.Object, loggerMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => sut.Handle(command, cts.Token));

        orchestratorMock.Verify(o => o.CancelAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);

        loggerMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HandlePassesCorrectParametersToOrchestrator()
    {
        // Arrange
        var portfolioId = 123;
        var closingDate = new DateTime(2025, 10, 2, 0, 0, 0, DateTimeKind.Utc);
        var command = new CancelClosingCommand(portfolioId, closingDate);

        var expected = Result.Success(fixture.Create<CancelClosingResult>());
        var cts = new CancellationTokenSource();

        orchestratorMock
            .Setup(o => o.CancelAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var sut = new CancelClosingCommandHandler(orchestratorMock.Object, loggerMock.Object);

        // Act
        var _ = await sut.Handle(command, cts.Token);

        // Assert
        orchestratorMock.Verify(o => o.CancelAsync(
            It.Is<int>(p => p == portfolioId),
            It.Is<DateTime>(d => d == closingDate),
            It.Is<CancellationToken>(t => t.Equals(cts.Token))
        ), Times.Once);
    }

    [Fact]
    public async Task HandlePropagatesFailureResultFromOrchestrator()
    {
        // Arrange
        var command = new CancelClosingCommand(fixture.Create<int>(), fixture.Create<DateTime>());
        var expectedFailure = Result.Failure<CancelClosingResult>(Error.NullValue);

        orchestratorMock
            .Setup(o => o.CancelAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedFailure);

        var sut = new CancelClosingCommandHandler(orchestratorMock.Object, loggerMock.Object);

        // Act
        var result = await sut.Handle(command, CancellationToken.None);

        // Assert 
        Assert.Same(expectedFailure, result);
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal(Error.NullValue, result.Error);

        loggerMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HandleLogsErrorAndRethrowsWhenOrchestratorThrows()
    {
        // Arrange
        var command = new CancelClosingCommand(fixture.Create<int>(), fixture.Create<DateTime>());
        var exception = new InvalidOperationException("boom");

        orchestratorMock
            .Setup(o => o.CancelAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        var sut = new CancelClosingCommandHandler(orchestratorMock.Object, loggerMock.Object);

        // Act
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => sut.Handle(command, CancellationToken.None));

        // Assert
        Assert.Same(exception, ex);
        loggerMock.VerifyLog(LogLevel.Error, Times.Once());
    }

    [Fact]
    public async Task HandleLogsInformationAndRethrowsWhenCancellationIsRequestedAfterOrchestratorCompletes()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var command = new CancelClosingCommand(fixture.Create<int>(), fixture.Create<DateTime>());

        orchestratorMock
            .Setup(o => o.CancelAsync(
                command.PortfolioId,
                command.ClosingDate,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                cts.Cancel();
                return Result.Success(fixture.Create<CancelClosingResult>());
            });

        var sut = new CancelClosingCommandHandler(orchestratorMock.Object, loggerMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => sut.Handle(command, cts.Token));

        loggerMock.VerifyLog(LogLevel.Information, Times.Once());
        loggerMock.VerifyLog(LogLevel.Error, Times.Never());
    }

}

public static class LoggerMoqExtensions
{
    public static void VerifyLog<T>(this Mock<ILogger<T>> logger, LogLevel level, Times times)
    {
        logger.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == level),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((_, __) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            times);
    }
}