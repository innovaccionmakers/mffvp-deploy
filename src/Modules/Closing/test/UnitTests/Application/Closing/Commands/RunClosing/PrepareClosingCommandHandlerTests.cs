
using AutoFixture;

using AutoFixture.Xunit2;
using Closing.Application.Abstractions.Data;
using Closing.Application.Closing.Commands.RunClosing;
using Closing.Application.Closing.Services.Orchestation.Interfaces;
using Closing.Integrations.Closing.RunClosing;
using Common.SharedKernel.Domain;
using Microsoft.EntityFrameworkCore.Storage; 
using Microsoft.Extensions.Logging;
using Moq;

namespace Closing.test.UnitTests.Application.Closing.Commands.RunClosing;


public class PrepareClosingCommandHandlerTests
{
    [Theory, AutoMoqData]
    public async Task HandleSuccessCommitsAndDoesNotCancel(
        [Frozen] Mock<IPrepareClosingOrchestrator> prepareMock,
        [Frozen] Mock<ICancelClosingOrchestrator> cancelMock,
        [Frozen] Mock<IUnitOfWork> uowMock,
        [Frozen] Mock<IDbContextTransaction> txMock,
        Fixture fixture)
    {
        // Arrange
        var loggerMock = new Mock<ILogger<PrepareClosingCommandHandler>>();
        var command = new PrepareClosingCommand(
            PortfolioId: 2,
            ClosingDate: new DateTime(2025, 10, 06)
        );

        uowMock.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
               .ReturnsAsync(txMock.Object);
        uowMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
               .ReturnsAsync(1);

        var expected = fixture.Create<PrepareClosingResult>();
        prepareMock.Setup(x => x.PrepareAsync(command, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(Result.Success(expected));

        var sut = new PrepareClosingCommandHandler(
            prepareMock.Object, cancelMock.Object, uowMock.Object, loggerMock.Object);

        // Act
        var result = await sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value);

        prepareMock.Verify(x => x.PrepareAsync(command, It.IsAny<CancellationToken>()), Times.Once);
        uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        txMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        txMock.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        cancelMock.Verify(x => x.CancelAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);

        txMock.Verify(x => x.DisposeAsync(), Times.Once);
    }

    [Theory, AutoMoqData]
    public async Task HandlePrepareFailureRollsBackCancelsAndRethrows(
       [Frozen] Mock<IPrepareClosingOrchestrator> prepareMock,
       [Frozen] Mock<ICancelClosingOrchestrator> cancelMock,
       [Frozen] Mock<IUnitOfWork> uowMock,
       [Frozen] Mock<IDbContextTransaction> txMock,
       Fixture fixture) 
    {
        var loggerMock = new Mock<ILogger<PrepareClosingCommandHandler>>();
        var command = new PrepareClosingCommand(3, new DateTime(2025, 10, 06));

        uowMock.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
               .ReturnsAsync(txMock.Object);

        prepareMock.Setup(x => x.PrepareAsync(command, It.IsAny<CancellationToken>()))
                   .ThrowsAsync(new InvalidOperationException("prepare failed"));

        cancelMock.Setup(x => x.CancelAsync(command.PortfolioId, command.ClosingDate, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(Result.Success(fixture.Create<CancelClosingResult>()));

        var sut = new PrepareClosingCommandHandler(
            prepareMock.Object, cancelMock.Object, uowMock.Object, loggerMock.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.Handle(command, CancellationToken.None));

        txMock.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        cancelMock.Verify(x => x.CancelAsync(command.PortfolioId, command.ClosingDate, It.IsAny<CancellationToken>()), Times.Once);
        loggerMock.VerifyLog(LogLevel.Error, Times.Once());
    }

    [Theory, AutoMoqData]
    public async Task HandleSaveChangesFailureRollsBackCancelsAndRethrows(
        [Frozen] Mock<IPrepareClosingOrchestrator> prepareMock,
        [Frozen] Mock<ICancelClosingOrchestrator> cancelMock,
        [Frozen] Mock<IUnitOfWork> uowMock,
        [Frozen] Mock<IDbContextTransaction> txMock,
        Fixture fixture)
    {
        // Arrange
        var loggerMock = new Mock<ILogger<PrepareClosingCommandHandler>>();
        var command = new PrepareClosingCommand(5, new DateTime(2025, 10, 06));

        uowMock.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
               .ReturnsAsync(txMock.Object);

        prepareMock.Setup(x => x.PrepareAsync(command, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(Result.Success(fixture.Create<PrepareClosingResult>()));

        uowMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
               .ThrowsAsync(new Exception("save failed"));

        var sut = new PrepareClosingCommandHandler(
            prepareMock.Object, cancelMock.Object, uowMock.Object, loggerMock.Object);

        // Act
        await Assert.ThrowsAsync<Exception>(() => sut.Handle(command, CancellationToken.None));

        // Assert
        txMock.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        txMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        cancelMock.Verify(x => x.CancelAsync(command.PortfolioId, command.ClosingDate, It.IsAny<CancellationToken>()), Times.Once);
        loggerMock.VerifyLog(LogLevel.Error, Times.Once());
    }

    [Theory, AutoMoqData]
    public async Task HandleCommitFailureRollsBackCancelsAndRethrows(
        [Frozen] Mock<IPrepareClosingOrchestrator> prepareMock,
        [Frozen] Mock<ICancelClosingOrchestrator> cancelMock,
        [Frozen] Mock<IUnitOfWork> uowMock,
        [Frozen] Mock<IDbContextTransaction> txMock,
        Fixture fixture)
    {
        // Arrange
        var loggerMock = new Mock<ILogger<PrepareClosingCommandHandler>>();
        var command = new PrepareClosingCommand(7, new DateTime(2025, 10, 06));

        uowMock.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
               .ReturnsAsync(txMock.Object);

        prepareMock.Setup(x => x.PrepareAsync(command, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(Result.Success(fixture.Create<PrepareClosingResult>()));

        uowMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
               .ReturnsAsync(1);

        txMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
              .ThrowsAsync(new Exception("commit failed"));

        var sut = new PrepareClosingCommandHandler(
            prepareMock.Object, cancelMock.Object, uowMock.Object, loggerMock.Object);

        // Act
        await Assert.ThrowsAsync<Exception>(() => sut.Handle(command, CancellationToken.None));

        // Assert
        txMock.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        cancelMock.Verify(x => x.CancelAsync(command.PortfolioId, command.ClosingDate, It.IsAny<CancellationToken>()), Times.Once);
        loggerMock.VerifyLog(LogLevel.Error, Times.Once());
    }

    [Theory, AutoMoqData]
    public async Task HandleCancellationBeforeStartThrowsAndDoesNothing(
        [Frozen] Mock<IPrepareClosingOrchestrator> prepareMock,
        [Frozen] Mock<ICancelClosingOrchestrator> cancelMock,
        [Frozen] Mock<IUnitOfWork> uowMock)
    {
        // Arrange
        var loggerMock = new Mock<ILogger<PrepareClosingCommandHandler>>();
        var command = new PrepareClosingCommand(11, new DateTime(2025, 10, 06));
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var sut = new PrepareClosingCommandHandler(
            prepareMock.Object, cancelMock.Object, uowMock.Object, loggerMock.Object);

        // Act
        await Assert.ThrowsAsync<OperationCanceledException>(() => sut.Handle(command, cts.Token));

        // Assert
        uowMock.Verify(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        prepareMock.Verify(x => x.PrepareAsync(It.IsAny<PrepareClosingCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        cancelMock.Verify(x => x.CancelAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory, AutoMoqData]
    public async Task HandleCancellationAfterPrepareBeforeSaveChangesRollsBackAndCancels(
        [Frozen] Mock<IPrepareClosingOrchestrator> prepareMock,
        [Frozen] Mock<ICancelClosingOrchestrator> cancelMock,
        [Frozen] Mock<IUnitOfWork> uowMock,
        [Frozen] Mock<IDbContextTransaction> txMock,
        Fixture fixture)
    {
        // Arrange
        var loggerMock = new Mock<ILogger<PrepareClosingCommandHandler>>();
        var command = new PrepareClosingCommand(13, new DateTime(2025, 10, 06));
        var cts = new CancellationTokenSource();

        uowMock.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
               .ReturnsAsync(txMock.Object);

        prepareMock.Setup(x => x.PrepareAsync(command, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(() =>
                   {
                       cts.Cancel();
                       return Result.Success(fixture.Create<PrepareClosingResult>());
                   });

        var sut = new PrepareClosingCommandHandler(
            prepareMock.Object, cancelMock.Object, uowMock.Object, loggerMock.Object);

        // Act
        await Assert.ThrowsAsync<OperationCanceledException>(() => sut.Handle(command, cts.Token));

        // Assert
        txMock.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        cancelMock.Verify(x => x.CancelAsync(command.PortfolioId, command.ClosingDate, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory, AutoMoqData]
    public async Task HandleRollbackFailureLogsWarningCallsCancelAndRethrowsOriginal(
        [Frozen] Mock<IPrepareClosingOrchestrator> prepareMock,
        [Frozen] Mock<ICancelClosingOrchestrator> cancelMock,
        [Frozen] Mock<IUnitOfWork> uowMock,
        [Frozen] Mock<IDbContextTransaction> txMock,
        Fixture fixture)
    {
        // Arrange
        var loggerMock = new Mock<ILogger<PrepareClosingCommandHandler>>();
        var command = new PrepareClosingCommand(17, new DateTime(2025, 10, 06));

        uowMock.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
               .ReturnsAsync(txMock.Object);

        prepareMock.Setup(x => x.PrepareAsync(command, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(Result.Success(fixture.Create<PrepareClosingResult>()));

        uowMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
               .ThrowsAsync(new Exception("save boom"));

        txMock.Setup(x => x.RollbackAsync(It.IsAny<CancellationToken>()))
              .ThrowsAsync(new Exception("rollback boom"));

        cancelMock.Setup(x => x.CancelAsync(command.PortfolioId, command.ClosingDate, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(Result.Success(fixture.Create<CancelClosingResult>()));

        var sut = new PrepareClosingCommandHandler(
            prepareMock.Object, cancelMock.Object, uowMock.Object, loggerMock.Object);

        // Act
        var ex = await Assert.ThrowsAsync<Exception>(() => sut.Handle(command, CancellationToken.None));

        // Assert
        Assert.Equal("save boom", ex.Message);

        txMock.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        txMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        cancelMock.Verify(x => x.CancelAsync(command.PortfolioId, command.ClosingDate, It.IsAny<CancellationToken>()), Times.Once);

        loggerMock.VerifyLog(LogLevel.Warning, Times.AtLeastOnce());
        loggerMock.VerifyLog(LogLevel.Error, Times.Once());
    }
}

