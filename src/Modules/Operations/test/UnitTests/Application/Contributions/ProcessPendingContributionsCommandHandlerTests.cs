using Common.SharedKernel.Application.EventBus;
using Common.SharedKernel.Core.Primitives;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Operations.Application.Abstractions.Data;
using Operations.Application.Abstractions.Services.Cleanup;
using Operations.Application.Abstractions.Services.OperationCompleted;
using Operations.Application.Abstractions.Services.TransactionControl;
using Operations.Application.Contributions.ProcessPendingContributions;
using Operations.Domain.AuxiliaryInformations;
using Operations.Domain.ClientOperations;
using Operations.Domain.TemporaryClientOperations;
using Operations.Integrations.Contributions.ProcessPendingContributions;
using System.Text.Json;
using Trusts.IntegrationEvents.CreateTrustRequested;
using IUnitOfWorkTransaction = Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction;
using PendingRow = Operations.Domain.TemporaryClientOperations.PendingContributionRow;

namespace Operations.Application.Tests.Contributions.ProcessPendingContributions;

public sealed class ProcessPendingContributionsCommandHandlerTests
{
    private readonly Mock<IPendingTransactionsReaderRepository> optimizedReader = new();
    private readonly Mock<ITemporaryClientOperationRepository> tempOpRepo = new();
    private readonly Mock<ITempClientOperationsCleanupService> cleanupService = new();
    private readonly Mock<ITransactionControl> transactionControl = new();
    private readonly Mock<IOperationCompleted> operationCompleted = new();
    private readonly Mock<IEventBus> eventBus = new();
    private readonly Mock<IUnitOfWork> unitOfWork = new();
    private readonly Mock<ILogger<ProcessPendingContributionsCommandHandler>> logger = new();

    private ProcessPendingContributionsCommandHandler CreateSut()
        => new(
            optimizedReader.Object,
            tempOpRepo.Object,
            cleanupService.Object,
            transactionControl.Object,
            operationCompleted.Object,
            eventBus.Object,
            unitOfWork.Object,
            logger.Object);

    [Fact]
    public async Task ReturnsSuccessWhenNoPendingRows()
    {
        // Arrange
        var portfolioId = 7;
        var processDate = new DateTime(2025, 10, 27, 10, 0, 0, DateTimeKind.Utc);

        optimizedReader
            .Setup(r => r.TakePendingBatchWithAuxAsync(portfolioId, It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<PendingRow>());

        var sut = CreateSut();
        var command = new ProcessPendingContributionsCommand(portfolioId, processDate);

        // Act
        var result = await sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        unitOfWork.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        tempOpRepo.Verify(r => r.MarkProcessedBulkIfPendingAsync(It.IsAny<long[]>(), It.IsAny<CancellationToken>()), Times.Never);
        operationCompleted.Verify(o => o.ExecuteAsync(It.IsAny<ClientOperation>(), It.IsAny<CancellationToken>()), Times.Never);
        eventBus.Verify(b => b.PublishAsync(It.IsAny<CreateTrustRequestedIntegrationEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        cleanupService.Verify(c => c.ScheduleCleanupAsync(It.IsAny<IReadOnlyCollection<long>>(), It.IsAny<IReadOnlyCollection<long>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessesSingleBatchPublishesEventsAndSchedulesCleanup()
    {
        // Arrange
        var portfolioId = 1;
        var processDateLocal = new DateTime(2025, 10, 27, 10, 0, 0, DateTimeKind.Local);

        var rows = new[]
        {
            TestRowBuilder.Create(portfolioId: portfolioId, tempId: 101, auxTempId: 201, affiliateId: 444, amount: 1000m),
            TestRowBuilder.Create(portfolioId: portfolioId, tempId: 102, auxTempId: 202, affiliateId: 445, amount: 2000m)
        };

        optimizedReader
            .SetupSequence(r => r.TakePendingBatchWithAuxAsync(portfolioId, It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(rows)
            .ReturnsAsync(Array.Empty<PendingRow>());

        // Dummy transaction
        var tx = new Mock<IUnitOfWorkTransaction>();
        unitOfWork
            .Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(tx.Object);
        unitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        tempOpRepo
            .Setup(r => r.MarkProcessedBulkIfPendingAsync(It.IsAny<long[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((long[] ids, CancellationToken _) => ids.Length);

        transactionControl
            .Setup(t => t.ExecuteAsync(It.IsAny<ClientOperation>(), It.IsAny<AuxiliaryInformation>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var publishedEvents = new List<CreateTrustRequestedIntegrationEvent>();
        eventBus
            .Setup(b => b.PublishAsync(It.IsAny<CreateTrustRequestedIntegrationEvent>(), It.IsAny<CancellationToken>()))
            .Callback<CreateTrustRequestedIntegrationEvent, CancellationToken>((e, _) => publishedEvents.Add(e))
            .Returns(Task.CompletedTask);

        var scheduledTempIds = Array.Empty<long>();
        var scheduledAuxIds = Array.Empty<long>();
        cleanupService
            .Setup(c => c.ScheduleCleanupAsync(It.IsAny<IReadOnlyCollection<long>>(), It.IsAny<IReadOnlyCollection<long>>(), It.IsAny<CancellationToken>()))
            .Callback<IReadOnlyCollection<long>, IReadOnlyCollection<long>, CancellationToken>((t, a, _) =>
            {
                scheduledTempIds = t.ToArray();
                scheduledAuxIds = a.ToArray();
            })
            .Returns(Task.CompletedTask);

        var sut = CreateSut();
        var command = new ProcessPendingContributionsCommand(portfolioId, processDateLocal);

        // Act
        var result = await sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Se ejecuta una sola transacción (un batch) y luego termina el loop
        unitOfWork.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2)); // una para insertar ops+aux, otra tras marcar temporales
        tx.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);

        // Se insertan 2 pares (op + aux)
        transactionControl.Verify(t => t.ExecuteAsync(It.IsAny<ClientOperation>(), It.IsAny<AuxiliaryInformation>(), It.IsAny<CancellationToken>()), Times.Exactly(rows.Length));

        // Se publican 2 eventos de fideicomiso y se replica la op a Closing 2 veces
        eventBus.Verify(b => b.PublishAsync(It.IsAny<CreateTrustRequestedIntegrationEvent>(), It.IsAny<CancellationToken>()), Times.Exactly(rows.Length));
        operationCompleted.Verify(o => o.ExecuteAsync(It.IsAny<ClientOperation>(), It.IsAny<CancellationToken>()), Times.Exactly(rows.Length));

        // Validar que los IDs de temporales se programan para borrado
        scheduledTempIds.Should().BeEquivalentTo(rows.Select(r => r.TemporaryClientOperationId));
        scheduledAuxIds.Should().BeEquivalentTo(rows.Select(r => r.TemporaryAuxiliaryInformationId));

        publishedEvents.Should().HaveCount(2);
        publishedEvents.Select(e => e.AffiliateId).Should().BeEquivalentTo(rows.Select(r => r.AffiliateId));
        publishedEvents.Select(e => e.PortfolioId).Should().AllBeEquivalentTo(portfolioId);
    }

    [Fact]
    public async Task ContinuesWhenSomeRowsAlreadyProcessedAndLogsWarning()
    {
        // Arrange
        var portfolioId = 2;
        var rows = new[]
        {
            TestRowBuilder.Create(portfolioId, 201, 301, affiliateId: 500, amount: 100m),
            TestRowBuilder.Create(portfolioId, 202, 302, affiliateId: 501, amount: 200m),
            TestRowBuilder.Create(portfolioId, 203, 303, affiliateId: 502, amount: 300m)
        };

        optimizedReader
            .SetupSequence(r => r.TakePendingBatchWithAuxAsync(portfolioId, It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(rows)
            .ReturnsAsync(Array.Empty<PendingRow>());

        var tx = new Mock<IUnitOfWorkTransaction>();
        unitOfWork.Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(tx.Object);
        unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Sólo 2 de 3 quedan marcadas (simula carrera con otra instancia)
        tempOpRepo
            .Setup(r => r.MarkProcessedBulkIfPendingAsync(It.IsAny<long[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        transactionControl
            .Setup(t => t.ExecuteAsync(It.IsAny<ClientOperation>(), It.IsAny<AuxiliaryInformation>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        eventBus
            .Setup(b => b.PublishAsync(It.IsAny<CreateTrustRequestedIntegrationEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var sut = CreateSut();
        var command = new ProcessPendingContributionsCommand(portfolioId, DateTime.UtcNow);

        // Act
        var result = await sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        // Se procede normalmente aunque affected < total
        tempOpRepo.Verify(r => r.MarkProcessedBulkIfPendingAsync(It.IsAny<long[]>(), It.IsAny<CancellationToken>()), Times.Once);
        // warning fue loggeado 
        logger.Verify(
            l => l.Log(
                It.Is<LogLevel>(lv => lv == LogLevel.Warning),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((_, __) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((_, __) => true)),
            Times.Once);
    }

    [Fact]
    public async Task RollsBackAndReturnsFailureWhenTransactionStepThrows()
    {
        // Arrange
        var portfolioId = 3;
        var rows = new[] { TestRowBuilder.Create(portfolioId, 301, 401, affiliateId: 700, amount: 1500m) };

        optimizedReader
            .SetupSequence(r => r.TakePendingBatchWithAuxAsync(portfolioId, It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(rows)
            .ReturnsAsync(Array.Empty<PendingRow>());

        var tx = new Mock<IUnitOfWorkTransaction>();
        unitOfWork.Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(tx.Object);
        unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        transactionControl
            .Setup(t => t.ExecuteAsync(It.IsAny<ClientOperation>(), It.IsAny<AuxiliaryInformation>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("DB failure"));

        var sut = CreateSut();
        var command = new ProcessPendingContributionsCommand(portfolioId, DateTime.UtcNow);

        // Act
        var result = await sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        tx.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        // No publicaciones ni cleanup
        eventBus.Verify(b => b.PublishAsync(It.IsAny<CreateTrustRequestedIntegrationEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        cleanupService.Verify(c => c.ScheduleCleanupAsync(It.IsAny<IReadOnlyCollection<long>>(), It.IsAny<IReadOnlyCollection<long>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ReturnsFailureIfPublishingAfterCommitThrows()
    {
        // Arrange
        var portfolioId = 4;
        var rows = new[] { TestRowBuilder.Create(portfolioId, 401, 501, affiliateId: 800, amount: 999m) };

        optimizedReader
            .SetupSequence(r => r.TakePendingBatchWithAuxAsync(portfolioId, It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(rows)
            .ReturnsAsync(Array.Empty<PendingRow>());

        var tx = new Mock<IUnitOfWorkTransaction>();
        unitOfWork.Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(tx.Object);
        unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        tempOpRepo.Setup(r => r.MarkProcessedBulkIfPendingAsync(It.IsAny<long[]>(), It.IsAny<CancellationToken>())).ReturnsAsync(1);
        transactionControl.Setup(t => t.ExecuteAsync(It.IsAny<ClientOperation>(), It.IsAny<AuxiliaryInformation>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Falla publicación después del commit
        eventBus
            .Setup(b => b.PublishAsync(It.IsAny<CreateTrustRequestedIntegrationEvent>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Bus down"));

        var sut = CreateSut();
        var command = new ProcessPendingContributionsCommand(portfolioId, DateTime.UtcNow);

        // Act
        var result = await sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();

        cleanupService.Verify(c => c.ScheduleCleanupAsync(It.IsAny<IReadOnlyCollection<long>>(), It.IsAny<IReadOnlyCollection<long>>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    [Fact]
    public async Task ProcessesMultipleBatchesAccumulatesCleanupAndPublishesAll()
    {
        // Arrange
        var portfolioId = 5;
        var batch1 = new[]
        {
            TestRowBuilder.Create(portfolioId, 501, 601, affiliateId: 900, amount: 111m),
            TestRowBuilder.Create(portfolioId, 502, 602, affiliateId: 901, amount: 222m)
        };
        var batch2 = new[]
        {
            TestRowBuilder.Create(portfolioId, 503, 603, affiliateId: 902, amount: 333m)
        };

        optimizedReader
            .SetupSequence(r => r.TakePendingBatchWithAuxAsync(portfolioId, It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(batch1)
            .ReturnsAsync(batch2)
            .ReturnsAsync(Array.Empty<PendingRow>());

        var tx = new Mock<IUnitOfWorkTransaction>();
        unitOfWork.Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                  .ReturnsAsync(tx.Object);
        unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                  .ReturnsAsync(1);

        tempOpRepo
            .Setup(r => r.MarkProcessedBulkIfPendingAsync(It.IsAny<long[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((long[] ids, CancellationToken _) => ids.Length);

        transactionControl
            .Setup(t => t.ExecuteAsync(It.IsAny<ClientOperation>(), It.IsAny<AuxiliaryInformation>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var publishedEvents = new List<CreateTrustRequestedIntegrationEvent>();
        eventBus
            .Setup(b => b.PublishAsync(It.IsAny<CreateTrustRequestedIntegrationEvent>(), It.IsAny<CancellationToken>()))
            .Callback<CreateTrustRequestedIntegrationEvent, CancellationToken>((e, _) => publishedEvents.Add(e))
            .Returns(Task.CompletedTask);

        long[] scheduledTempIds = Array.Empty<long>();
        long[] scheduledAuxIds = Array.Empty<long>();
        cleanupService
            .Setup(c => c.ScheduleCleanupAsync(It.IsAny<IReadOnlyCollection<long>>(), It.IsAny<IReadOnlyCollection<long>>(), It.IsAny<CancellationToken>()))
            .Callback<IReadOnlyCollection<long>, IReadOnlyCollection<long>, CancellationToken>((t, a, _) =>
            {
                scheduledTempIds = t.ToArray();
                scheduledAuxIds = a.ToArray();
            })
            .Returns(Task.CompletedTask);

        var sut = CreateSut();
        var command = new ProcessPendingContributionsCommand(portfolioId, DateTime.UtcNow);

        // Act
        var result = await sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        unitOfWork.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(4)); // 2 por batch
        tx.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));

        transactionControl.Verify(t => t.ExecuteAsync(It.IsAny<ClientOperation>(), It.IsAny<AuxiliaryInformation>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
        operationCompleted.Verify(o => o.ExecuteAsync(It.IsAny<ClientOperation>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
        eventBus.Verify(b => b.PublishAsync(It.IsAny<CreateTrustRequestedIntegrationEvent>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
        publishedEvents.Should().HaveCount(3);

        scheduledTempIds.Should().BeEquivalentTo(new long[] { 501, 502, 503 });
        scheduledAuxIds.Should().BeEquivalentTo(new long[] { 601, 602, 603 });
    }

    private static class TestRowBuilder
    {
        public static PendingRow Create(
            int portfolioId,
            long tempId,
            long auxTempId,
            int affiliateId,
            decimal amount)
        {
            return new PendingRow
            {
                TemporaryClientOperationId = tempId,
                TemporaryAuxiliaryInformationId = auxTempId,
                RegistrationDate = new DateTime(2025, 10, 20, 8, 0, 0, DateTimeKind.Utc),
                AffiliateId = affiliateId,
                ObjectiveId = 1,
                PortfolioId = portfolioId,
                Amount = amount,
                ProcessDate = new DateTime(2025, 10, 27, 10, 0, 0, DateTimeKind.Utc),
                OperationTypeId = 4,
                ApplicationDate = new DateTime(2025, 10, 27, 10, 0, 0, DateTimeKind.Utc),
                Processed = false,
                TrustId = 123,
                LinkedClientOperationId = null,
                Status = LifecycleStatus.Active,
                Units = null,
                CauseId = null,

                // Auxiliar
                OriginId = 1,
                CollectionMethodId = 1,
                PaymentMethodId = 1,
                CollectionAccount = "001-000-000",
                PaymentMethodDetail = JsonDocument.Parse("\"DEP-123\""),
                CertificationStatusId = 1,
                TaxConditionId = 1,
                ContingentWithholding = 0m,
                VerifiableMedium = JsonDocument.Parse("\"\""),
                CollectionBankId = 10,
                DepositDate = new DateTime(2025, 10, 27, 10, 0, 0, DateTimeKind.Utc),
                SalesUser = "tester",
                OriginModalityId = 1,
                CityId = 1,
                ChannelId = 1,
                UserId = "99"
            };
        }
    }
}