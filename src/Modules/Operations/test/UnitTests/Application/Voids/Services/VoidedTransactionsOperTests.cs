using System.Reflection;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using Operations.Application.Abstractions.Data;
using Operations.Application.Abstractions.External;
using Operations.Application.Abstractions.Services.OperationCompleted;
using Operations.Application.Abstractions.Services.Voids;
using Operations.Application.Voids.Services;
using Operations.Integrations.Voids.RegisterVoidedTransactions;
using Operations.Domain.ClientOperations;

namespace Operations.test.UnitTests.Application.Voids.Services;

public class VoidedTransactionsOperTests
{
    [Fact]
    public async Task ExecuteAsync_WhenTrustUpdaterFails_ReturnsFailure()
    {
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var transactionMock = CreateTransactionMock();
        unitOfWorkMock
            .Setup(work => work.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(transactionMock.Object);
        unitOfWorkMock
            .Setup(work => work.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var clientOperationRepositoryMock = new Mock<IClientOperationRepository>();
        clientOperationRepositoryMock.Setup(repository => repository.Update(It.IsAny<ClientOperation>()));

        var trustUpdaterMock = new Mock<ITrustUpdater>();
        var trustError = Error.Validation("TRUST.UPDATE", "Error al actualizar fideicomiso");
        trustUpdaterMock
            .Setup(updater => updater.UpdateAsync(It.IsAny<TrustUpdate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(trustError));

        var operationCompletedMock = new Mock<IOperationCompleted>(MockBehavior.Strict);

        var service = new VoidedTransactionsOper(
            unitOfWorkMock.Object,
            clientOperationRepositoryMock.Object,
            trustUpdaterMock.Object,
            operationCompletedMock.Object);

        var operation = CreateClientOperation(1, causeId: 5, trustId: 10);
        var readyOperation = new VoidedTransactionReady(
            operation,
            Amount: 100m,
            PortfolioCurrentDate: DateTime.UtcNow,
            TrustId: 99);

        var validationResult = new VoidedTransactionsValidationResult(
            new[] { readyOperation },
            Array.Empty<VoidedTransactionFailure>(),
            CauseConfigurationParameterId: 20);

        var result = await service.ExecuteAsync(
            new VoidedTransactionsOperRequest(AffiliateId: 1, ObjectiveId: 2),
            validationResult,
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(trustError.Code);
        result.Error.Description.Should().Be(trustError.Description);
        trustUpdaterMock.Verify(updater => updater.UpdateAsync(
                It.Is<TrustUpdate>(update => update.ClientOperationId == operation.ClientOperationId && update.Status == LifecycleStatus.Annulled),
                It.IsAny<CancellationToken>()),
            Times.Once);
        operationCompletedMock.Verify(completed => completed.ExecuteAsync(It.IsAny<ClientOperation>(), It.IsAny<CancellationToken>()), Times.Never);
        transactionMock.Verify(transaction => transaction.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenOperationsAreProcessed_ReturnsSuccessWithMessage()
    {
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var transactionMock = CreateTransactionMock();
        unitOfWorkMock
            .Setup(work => work.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(transactionMock.Object);
        unitOfWorkMock
            .Setup(work => work.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var clientOperationRepositoryMock = new Mock<IClientOperationRepository>();
        clientOperationRepositoryMock.Setup(repository => repository.Update(It.IsAny<ClientOperation>()));

        var trustUpdaterMock = new Mock<ITrustUpdater>();
        trustUpdaterMock
            .Setup(updater => updater.UpdateAsync(It.IsAny<TrustUpdate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var operationCompletedMock = new Mock<IOperationCompleted>();
        operationCompletedMock
            .Setup(completed => completed.ExecuteAsync(It.IsAny<ClientOperation>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = new VoidedTransactionsOper(
            unitOfWorkMock.Object,
            clientOperationRepositoryMock.Object,
            trustUpdaterMock.Object,
            operationCompletedMock.Object);

        var firstOperation = CreateClientOperation(1, causeId: 3, trustId: null);
        var secondOperation = CreateClientOperation(2, causeId: 4, trustId: 50);

        var validationResult = new VoidedTransactionsValidationResult(
            new[]
            {
                new VoidedTransactionReady(firstOperation, 120m, DateTime.UtcNow, 70),
                new VoidedTransactionReady(secondOperation, 80m, DateTime.UtcNow, 60)
            },
            Array.Empty<VoidedTransactionFailure>(),
            CauseConfigurationParameterId: 25);

        var result = await service.ExecuteAsync(
            new VoidedTransactionsOperRequest(AffiliateId: 5, ObjectiveId: 6),
            validationResult,
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.VoidIds.Should().BeEquivalentTo(new long[] { 1, 2 });
        result.Value.Message.Should().Be("Anulación registrada. 2 operación(es) anulada(s). Operación(es) en proceso de cierre. Operación(es) anulada(s): 1,2.");
        firstOperation.Status.Should().Be(LifecycleStatus.Annulled);
        firstOperation.CauseId.Should().Be(25);
        firstOperation.TrustId.Should().Be(70);
        secondOperation.Status.Should().Be(LifecycleStatus.Annulled);
        secondOperation.CauseId.Should().Be(25);
        secondOperation.TrustId.Should().Be(60);
        trustUpdaterMock.Verify(updater => updater.UpdateAsync(It.IsAny<TrustUpdate>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        operationCompletedMock.Verify(completed => completed.ExecuteAsync(It.IsAny<ClientOperation>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        transactionMock.Verify(transaction => transaction.CommitAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    private static Mock<IDbContextTransaction> CreateTransactionMock()
    {
        var transactionMock = new Mock<IDbContextTransaction>();
        transactionMock
            .Setup(transaction => transaction.CommitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        transactionMock
            .Setup(transaction => transaction.DisposeAsync())
            .Returns(ValueTask.CompletedTask);
        return transactionMock;
    }

    private static ClientOperation CreateClientOperation(long id, int causeId, long? trustId)
    {
        var operation = ClientOperation
            .Create(
                DateTime.UtcNow,
                affiliateId: 1,
                objectiveId: 2,
                portfolioId: 3,
                amount: 100m,
                processDate: DateTime.UtcNow,
                operationTypeId: 10,
                applicationDate: DateTime.UtcNow,
                status: LifecycleStatus.Active,
                causeId: causeId,
                trustId: trustId)
            .Value;

        typeof(ClientOperation)
            .GetProperty(nameof(ClientOperation.ClientOperationId), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
            .SetValue(operation, id);

        return operation;
    }
}
