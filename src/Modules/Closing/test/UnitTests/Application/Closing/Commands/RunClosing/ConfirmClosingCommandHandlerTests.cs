using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
using Closing.Application.Abstractions.Data;
using Closing.Application.Abstractions.External;
using Closing.Application.Closing.RunClosing;
using Closing.Application.Closing.Services.Orchestation.Interfaces;
using Closing.Application.PostClosing.Services.Orchestation;
using Closing.Domain.PortfolioValuations;
using Closing.Integrations.Closing.RunClosing;
using Common.SharedKernel.Application.Caching.Closing.Interfaces;
using Common.SharedKernel.Domain;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Moq;

namespace Closing.test.UnitTests.Application.Closing.Commands.RunClosing;

public class AutoMoqDataAttribute : AutoDataAttribute
{
    public AutoMoqDataAttribute() : base(() =>
    {
        var fixture = new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });
        return fixture;
    })
    { }
}

public class ConfirmClosingCommandHandlerTests
{
    [Theory, AutoMoqData]
    public async Task HandleSuccessCommitsAndRunsPostClosing(
        [Frozen] Mock<IConfirmClosingOrchestrator> orchestratorMock,
        [Frozen] Mock<IPostClosingServicesOrchestation> postClosingMock,
        [Frozen] Mock<IUnitOfWork> unitOfWorkMock,
        [Frozen] Mock<IDbContextTransaction> transactionMock,
        [Frozen] Mock<IPortfolioValidator> portfolioValidatorMock,
        [Frozen] Mock<IPortfolioValuationRepository> portfolioValuationRepositoryMock,
        [Frozen] Mock<IClosingExecutionStore> closingExecutionStoreMock,
        Fixture fixture)
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ConfirmClosingCommandHandler>>();

        var portfolioId = 2;
        var closingDate = new DateTime(2025, 10, 06);
        var command = new ConfirmClosingCommand(portfolioId, closingDate);

        unitOfWorkMock
            .Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(transactionMock.Object);

        // Setup para IsFirstClosingDay = false
        portfolioValidatorMock
            .Setup(x => x.GetPortfolioDataAsync(portfolioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new PortfolioData(portfolioId, closingDate)));

        portfolioValuationRepositoryMock
            .Setup(x => x.ExistsByPortfolioAndDateAsync(portfolioId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var confirmResult = fixture.Create<ConfirmClosingResult>();

        orchestratorMock
            .Setup(x => x.ConfirmAsync(portfolioId, closingDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(confirmResult));

        unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        transactionMock
            .Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        postClosingMock
            .Setup(x => x.ExecuteAsync(portfolioId, closingDate, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // En el happy path asumimos que ClosingEnd ya limpió el lock → IsClosingActiveAsync = false
        closingExecutionStoreMock
            .Setup(x => x.IsClosingActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = new ConfirmClosingCommandHandler(
            orchestratorMock.Object,
            postClosingMock.Object,
            unitOfWorkMock.Object,
            closingExecutionStoreMock.Object,
            loggerMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(confirmResult, result.Value);

        orchestratorMock.Verify(
            x => x.ConfirmAsync(portfolioId, closingDate, It.IsAny<CancellationToken>()),
            Times.Once);

        unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        transactionMock.Verify(
            x => x.CommitAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        transactionMock.Verify(
            x => x.RollbackAsync(It.IsAny<CancellationToken>()),
            Times.Never);

        postClosingMock.Verify(
            x => x.ExecuteAsync(portfolioId, closingDate, It.IsAny<CancellationToken>()),
            Times.Once);

        transactionMock.Verify(x => x.DisposeAsync(), Times.Once);

        // Consulta el estado pero NO intenta liberar si no está activo
        closingExecutionStoreMock.Verify(
            x => x.IsClosingActiveAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        closingExecutionStoreMock.Verify(
            x => x.EndAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory, AutoMoqData]
    public async Task HandlePhase1OrchestratorFailureRollsBackAndRethrows(
        [Frozen] Mock<IConfirmClosingOrchestrator> orchestratorMock,
        [Frozen] Mock<IPostClosingServicesOrchestation> postClosingMock,
        [Frozen] Mock<IUnitOfWork> unitOfWorkMock,
        [Frozen] Mock<IDbContextTransaction> transactionMock,
        [Frozen] Mock<IPortfolioValidator> portfolioValidatorMock,
        [Frozen] Mock<IPortfolioValuationRepository> portfolioValuationRepositoryMock,
        [Frozen] Mock<IClosingExecutionStore> closingExecutionStoreMock)
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ConfirmClosingCommandHandler>>();

        var portfolioId = 2;
        var closingDate = new DateTime(2025, 10, 06);
        var command = new ConfirmClosingCommand(portfolioId, closingDate);

        unitOfWorkMock
            .Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(transactionMock.Object);

        portfolioValidatorMock
            .Setup(x => x.GetPortfolioDataAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new PortfolioData(portfolioId, closingDate)));

        portfolioValuationRepositoryMock
            .Setup(x => x.ExistsByPortfolioAndDateAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        orchestratorMock
            .Setup(x => x.ConfirmAsync(portfolioId, closingDate, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("orchestrator failed"));

        var handler = new ConfirmClosingCommandHandler(
            orchestratorMock.Object,
            postClosingMock.Object,
            unitOfWorkMock.Object,
            closingExecutionStoreMock.Object,
            loggerMock.Object);

        // Act
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(command, CancellationToken.None));

        // Assert
        transactionMock.Verify(
            x => x.RollbackAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        transactionMock.Verify(
            x => x.CommitAsync(It.IsAny<CancellationToken>()),
            Times.Never);

        postClosingMock.Verify(
            x => x.ExecuteAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()),
            Times.Never);

        loggerMock.VerifyLog(LogLevel.Error, Times.Once());
    }

    [Theory, AutoMoqData]
    public async Task HandlePhase1SaveChangesFailureRollsBackAndRethrows(
        [Frozen] Mock<IConfirmClosingOrchestrator> orchestratorMock,
        [Frozen] Mock<IPostClosingServicesOrchestation> postClosingMock,
        [Frozen] Mock<IUnitOfWork> unitOfWorkMock,
        [Frozen] Mock<IDbContextTransaction> transactionMock,
        [Frozen] Mock<IPortfolioValidator> portfolioValidatorMock,
        [Frozen] Mock<IPortfolioValuationRepository> portfolioValuationRepositoryMock,
        [Frozen] Mock<IClosingExecutionStore> closingExecutionStoreMock,
        Fixture fixture)
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ConfirmClosingCommandHandler>>();

        var portfolioId = 9;
        var closingDate = new DateTime(2025, 10, 06);
        var command = new ConfirmClosingCommand(portfolioId, closingDate);

        unitOfWorkMock
            .Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(transactionMock.Object);

        portfolioValidatorMock
            .Setup(x => x.GetPortfolioDataAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new PortfolioData(portfolioId, closingDate)));

        portfolioValuationRepositoryMock
            .Setup(x => x.ExistsByPortfolioAndDateAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        orchestratorMock
            .Setup(x => x.ConfirmAsync(portfolioId, closingDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(fixture.Create<ConfirmClosingResult>()));

        unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("save failed"));

        var handler = new ConfirmClosingCommandHandler(
            orchestratorMock.Object,
            postClosingMock.Object,
            unitOfWorkMock.Object,
            closingExecutionStoreMock.Object,
            loggerMock.Object);

        // Act
        await Assert.ThrowsAsync<Exception>(
            () => handler.Handle(command, CancellationToken.None));

        // Assert
        transactionMock.Verify(
            x => x.RollbackAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        transactionMock.Verify(
            x => x.CommitAsync(It.IsAny<CancellationToken>()),
            Times.Never);

        postClosingMock.Verify(
            x => x.ExecuteAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()),
            Times.Never);

        loggerMock.VerifyLog(LogLevel.Error, Times.Once());
    }

    [Theory, AutoMoqData]
    public async Task HandlePhase1CommitFailureRollsBackAndRethrows(
        [Frozen] Mock<IConfirmClosingOrchestrator> orchestratorMock,
        [Frozen] Mock<IPostClosingServicesOrchestation> postClosingMock,
        [Frozen] Mock<IUnitOfWork> unitOfWorkMock,
        [Frozen] Mock<IDbContextTransaction> transactionMock,
        [Frozen] Mock<IPortfolioValidator> portfolioValidatorMock,
        [Frozen] Mock<IPortfolioValuationRepository> portfolioValuationRepositoryMock,
        [Frozen] Mock<IClosingExecutionStore> closingExecutionStoreMock,
        Fixture fixture)
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ConfirmClosingCommandHandler>>();

        var portfolioId = 5;
        var closingDate = new DateTime(2025, 10, 06);
        var command = new ConfirmClosingCommand(portfolioId, closingDate);

        unitOfWorkMock
            .Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(transactionMock.Object);

        portfolioValidatorMock
            .Setup(x => x.GetPortfolioDataAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new PortfolioData(portfolioId, closingDate)));

        portfolioValuationRepositoryMock
            .Setup(x => x.ExistsByPortfolioAndDateAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        orchestratorMock
            .Setup(x => x.ConfirmAsync(portfolioId, closingDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(fixture.Create<ConfirmClosingResult>()));

        unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        transactionMock
            .Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("commit failed"));

        var handler = new ConfirmClosingCommandHandler(
            orchestratorMock.Object,
            postClosingMock.Object,
            unitOfWorkMock.Object,
            closingExecutionStoreMock.Object,
            loggerMock.Object);

        // Act
        await Assert.ThrowsAsync<Exception>(
            () => handler.Handle(command, CancellationToken.None));

        // Assert
        transactionMock.Verify(
            x => x.RollbackAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        postClosingMock.Verify(
            x => x.ExecuteAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()),
            Times.Never);

        loggerMock.VerifyLog(LogLevel.Error, Times.Once());
    }

    [Theory, AutoMoqData]
    public async Task HandlePhase1RollbackAlsoFailsLogsWarningAndRethrowsOriginal(
        [Frozen] Mock<IConfirmClosingOrchestrator> orchestratorMock,
        [Frozen] Mock<IPostClosingServicesOrchestation> postClosingMock,
        [Frozen] Mock<IUnitOfWork> unitOfWorkMock,
        [Frozen] Mock<IDbContextTransaction> transactionMock,
        [Frozen] Mock<IPortfolioValidator> portfolioValidatorMock,
        [Frozen] Mock<IPortfolioValuationRepository> portfolioValuationRepositoryMock,
        [Frozen] Mock<IClosingExecutionStore> closingExecutionStoreMock,
        Fixture fixture)
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ConfirmClosingCommandHandler>>();

        var portfolioId = 7;
        var closingDate = new DateTime(2025, 10, 06);
        var command = new ConfirmClosingCommand(portfolioId, closingDate);

        unitOfWorkMock
            .Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(transactionMock.Object);

        portfolioValidatorMock
            .Setup(x => x.GetPortfolioDataAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new PortfolioData(portfolioId, closingDate)));

        portfolioValuationRepositoryMock
            .Setup(x => x.ExistsByPortfolioAndDateAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        orchestratorMock
            .Setup(x => x.ConfirmAsync(portfolioId, closingDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(fixture.Create<ConfirmClosingResult>()));

        unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("save boom"));

        transactionMock
            .Setup(x => x.RollbackAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("rollback boom"));

        var handler = new ConfirmClosingCommandHandler(
            orchestratorMock.Object,
            postClosingMock.Object,
            unitOfWorkMock.Object,
            closingExecutionStoreMock.Object,
            loggerMock.Object);

        // Act
        var ex = await Assert.ThrowsAsync<Exception>(
            () => handler.Handle(command, CancellationToken.None));

        // Assert 
        Assert.Equal("save boom", ex.Message);

        loggerMock.VerifyLog(LogLevel.Warning, Times.Once());
        loggerMock.VerifyLog(LogLevel.Error, Times.Once());

        postClosingMock.Verify(
            x => x.ExecuteAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory, AutoMoqData]
    public async Task HandlePostClosingFailureKeepsPhase1CommittedAndRethrows(
        [Frozen] Mock<IConfirmClosingOrchestrator> orchestratorMock,
        [Frozen] Mock<IPostClosingServicesOrchestation> postClosingMock,
        [Frozen] Mock<IUnitOfWork> unitOfWorkMock,
        [Frozen] Mock<IDbContextTransaction> transactionMock,
        [Frozen] Mock<IPortfolioValidator> portfolioValidatorMock,
        [Frozen] Mock<IPortfolioValuationRepository> portfolioValuationRepositoryMock,
        [Frozen] Mock<IClosingExecutionStore> closingExecutionStoreMock,
        Fixture fixture)
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ConfirmClosingCommandHandler>>();

        var portfolioId = 3;
        var closingDate = new DateTime(2025, 10, 06);
        var command = new ConfirmClosingCommand(portfolioId, closingDate);

        unitOfWorkMock
            .Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(transactionMock.Object);

        portfolioValidatorMock
            .Setup(x => x.GetPortfolioDataAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new PortfolioData(portfolioId, closingDate)));

        portfolioValuationRepositoryMock
            .Setup(x => x.ExistsByPortfolioAndDateAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        orchestratorMock
            .Setup(x => x.ConfirmAsync(portfolioId, closingDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(fixture.Create<ConfirmClosingResult>()));

        unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        transactionMock
            .Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        postClosingMock
            .Setup(x => x.ExecuteAsync(portfolioId, closingDate, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("post-closing failed"));

        // Escenario: simular que el lock SIGUE activo al final → se debe llamar a EndAsync
        closingExecutionStoreMock
            .Setup(x => x.IsClosingActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        closingExecutionStoreMock
            .Setup(x => x.EndAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new ConfirmClosingCommandHandler(
            orchestratorMock.Object,
            postClosingMock.Object,
            unitOfWorkMock.Object,
            closingExecutionStoreMock.Object,
            loggerMock.Object);

        // Act
        await Assert.ThrowsAsync<Exception>(
            () => handler.Handle(command, CancellationToken.None));

        // Assert
        unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        transactionMock.Verify(
            x => x.CommitAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        transactionMock.Verify(
            x => x.RollbackAsync(It.IsAny<CancellationToken>()),
            Times.Never);

        loggerMock.VerifyLog(LogLevel.Error, Times.Once());

        closingExecutionStoreMock.Verify(
            x => x.IsClosingActiveAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        closingExecutionStoreMock.Verify(
            x => x.EndAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory, AutoMoqData]
    public async Task HandleCancellationBeforeStartThrowsOperationCanceledAndDoesNothing(
        [Frozen] Mock<IConfirmClosingOrchestrator> orchestratorMock,
        [Frozen] Mock<IPostClosingServicesOrchestation> postClosingMock,
        [Frozen] Mock<IUnitOfWork> unitOfWorkMock,
        [Frozen] Mock<IPortfolioValidator> portfolioValidatorMock,
        [Frozen] Mock<IPortfolioValuationRepository> portfolioValuationRepositoryMock,
        [Frozen] Mock<IClosingExecutionStore> closingExecutionStoreMock)
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ConfirmClosingCommandHandler>>();

        var portfolioId = 11;
        var closingDate = new DateTime(2025, 10, 06);
        var command = new ConfirmClosingCommand(portfolioId, closingDate);

        var cancellationSource = new CancellationTokenSource();
        cancellationSource.Cancel();

        var handler = new ConfirmClosingCommandHandler(
            orchestratorMock.Object,
            postClosingMock.Object,
            unitOfWorkMock.Object,
            closingExecutionStoreMock.Object,
            loggerMock.Object);

        // Act
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => handler.Handle(command, cancellationSource.Token));

        // Assert
        unitOfWorkMock.Verify(
            x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()),
            Times.Never);

        orchestratorMock.Verify(
            x => x.ConfirmAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()),
            Times.Never);

        postClosingMock.Verify(
            x => x.ExecuteAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory, AutoMoqData]
    public async Task HandleCancellationBetweenOrchestratorAndSaveChangesRollsBack(
        [Frozen] Mock<IConfirmClosingOrchestrator> orchestratorMock,
        [Frozen] Mock<IPostClosingServicesOrchestation> postClosingMock,
        [Frozen] Mock<IUnitOfWork> unitOfWorkMock,
        [Frozen] Mock<IDbContextTransaction> transactionMock,
        [Frozen] Mock<IPortfolioValidator> portfolioValidatorMock,
        [Frozen] Mock<IPortfolioValuationRepository> portfolioValuationRepositoryMock,
        [Frozen] Mock<IClosingExecutionStore> closingExecutionStoreMock,
        Fixture fixture)
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ConfirmClosingCommandHandler>>();

        var portfolioId = 13;
        var closingDate = new DateTime(2025, 10, 06);
        var command = new ConfirmClosingCommand(portfolioId, closingDate);

        var cancellationSource = new CancellationTokenSource();

        unitOfWorkMock
            .Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(transactionMock.Object);

        portfolioValidatorMock
            .Setup(x => x.GetPortfolioDataAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new PortfolioData(portfolioId, closingDate)));

        portfolioValuationRepositoryMock
            .Setup(x => x.ExistsByPortfolioAndDateAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        orchestratorMock
            .Setup(x => x.ConfirmAsync(portfolioId, closingDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                cancellationSource.Cancel();
                return Result.Success(fixture.Create<ConfirmClosingResult>());
            });

        var handler = new ConfirmClosingCommandHandler(
            orchestratorMock.Object,
            postClosingMock.Object,
            unitOfWorkMock.Object,
            closingExecutionStoreMock.Object,
            loggerMock.Object);

        // Act
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => handler.Handle(command, cancellationSource.Token));

        // Assert
        transactionMock.Verify(
            x => x.RollbackAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        transactionMock.Verify(
            x => x.CommitAsync(It.IsAny<CancellationToken>()),
            Times.Never);

        postClosingMock.Verify(
            x => x.ExecuteAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
