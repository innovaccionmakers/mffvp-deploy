using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using Operations.Application.Abstractions.Data;
using Operations.Application.Abstractions.External;
using Operations.Application.Abstractions.Services.AccountingRecords;
using Operations.Application.Abstractions.Services.OperationCompleted;
using Operations.Application.AccountingRecords.Services;
using Operations.Domain.ClientOperations;
using Operations.Domain.TrustOperations;
using Xunit;

namespace Operations.test.UnitTests.Application.AccountingRecords.Services;

public class AccountingRecordsOperTests
{
    [Fact]
    public async Task ExecuteAsync_WhenAllDependenciesSucceed_ReturnsSuccessAndPersistsChanges()
    {
        var fixture = TestFixture.Create();
        var sut = fixture.CreateSut();

        var result = await sut.ExecuteAsync(
            fixture.Request,
            fixture.ValidationResult,
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.DebitNoteId.Should().Be(fixture.GeneratedDebitNoteId);
        result.Value.Message.Should().Be(
            $"La nota débito se creó correctamente. ID de la operación: {fixture.GeneratedDebitNoteId}.");

        fixture.ClientOperationRepositoryMock.Verify(
            repository => repository.Insert(It.IsAny<ClientOperation>()),
            Times.Once);
        fixture.ClientOperationRepositoryMock.Verify(
            repository => repository.Update(fixture.OriginalOperation),
            Times.Once);
        fixture.TrustOperationRepositoryMock.Verify(
            repository => repository.AddAsync(It.IsAny<TrustOperation>(), It.IsAny<CancellationToken>()),
            Times.Once);
        fixture.UnitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Exactly(3));
        fixture.TransactionMock.Verify(
            transaction => transaction.CommitAsync(It.IsAny<CancellationToken>()),
            Times.Once);
        fixture.OperationCompletedMock.Verify(
            operationCompleted => operationCompleted.ExecuteAsync(
                fixture.InsertedDebitNote!,
                It.IsAny<CancellationToken>()),
            Times.Once);

        var expectedUnits = decimal.Round(
            fixture.Request.Amount / fixture.UnitValue,
            16,
            MidpointRounding.AwayFromZero);
        fixture.InsertedDebitNote.Should().NotBeNull();
        fixture.InsertedDebitNote!.AffiliateId.Should().Be(fixture.Request.AffiliateId);
        fixture.InsertedDebitNote.ObjectiveId.Should().Be(fixture.Request.ObjectiveId);
        fixture.InsertedDebitNote.PortfolioId.Should().Be(fixture.OriginalOperation.PortfolioId);
        fixture.InsertedDebitNote.Amount.Should().Be(fixture.Request.Amount);
        fixture.InsertedDebitNote.OperationTypeId.Should().Be(fixture.ValidationResult.DebitNoteOperationTypeId);
        fixture.InsertedDebitNote.Status.Should().Be(LifecycleStatus.Active);
        fixture.InsertedDebitNote.CauseId.Should().Be(fixture.Request.CauseId);
        fixture.InsertedDebitNote.TrustId.Should().Be(fixture.ValidationResult.TrustId);
        fixture.InsertedDebitNote.LinkedClientOperationId.Should().Be(fixture.OriginalOperation.ClientOperationId);
        fixture.InsertedDebitNote.Units.Should().Be(expectedUnits);
        fixture.InsertedDebitNote.ProcessDate.Should().Be(fixture.ExpectedProcessDate);

        fixture.OriginalOperation.Status.Should().Be(LifecycleStatus.AnnulledByDebitNote);

        var expectedTrustOperationUnits = decimal.Round(
            Math.Abs(fixture.TrustEarnings) / fixture.UnitValue,
            16,
            MidpointRounding.AwayFromZero);
        fixture.AddedTrustOperation.Should().NotBeNull();
        fixture.AddedTrustOperation!.ClientOperationId.Should().Be(fixture.GeneratedDebitNoteId);
        fixture.AddedTrustOperation.TrustId.Should().Be(fixture.ValidationResult.TrustId);
        fixture.AddedTrustOperation.Amount.Should().Be(-fixture.TrustEarnings);
        fixture.AddedTrustOperation.Units.Should().Be(expectedTrustOperationUnits);
        fixture.AddedTrustOperation.OperationTypeId.Should().Be(fixture.ValidationResult.TrustAdjustmentOperationTypeId);
        fixture.AddedTrustOperation.PortfolioId.Should().Be(fixture.OriginalOperation.PortfolioId);
        fixture.AddedTrustOperation.ProcessDate.Should().Be(fixture.ExpectedProcessDate);

        fixture.SentTrustUpdate.Should().NotBeNull();
        fixture.SentTrustUpdate!.ClientOperationId.Should().Be(fixture.OriginalOperation.ClientOperationId);
        fixture.SentTrustUpdate.Status.Should().Be(LifecycleStatus.AnnulledByDebitNote);
        fixture.SentTrustUpdate.UpdateDate.Should().Be(fixture.ExpectedProcessDate);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPortfolioValuationFails_ReturnsFailure()
    {
        var valuationError = Error.Validation("OPS_ACC_VAL", "No fue posible obtener la valoración del portafolio.");
        var fixture = TestFixture.Create()
            .WithPortfolioValuationFailure(valuationError);
        var sut = fixture.CreateSut();

        var result = await sut.ExecuteAsync(
            fixture.Request,
            fixture.ValidationResult,
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        FluentAssertions.AssertionExtensions.Should(result.Error)
            .BeEquivalentTo(valuationError);
        fixture.PortfolioValuationProviderMock.Verify(
            provider => provider.GetUnitValueAsync(
                fixture.OriginalOperation.PortfolioId,
                fixture.ValidationResult.PortfolioCurrentDate,
                It.IsAny<CancellationToken>()),
            Times.Once);
        fixture.UnitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
        fixture.ClientOperationRepositoryMock.Verify(
            repository => repository.Insert(It.IsAny<ClientOperation>()),
            Times.Never);
        fixture.TransactionMock.Verify(
            transaction => transaction.CommitAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenTrustDetailsFail_ReturnsFailureWithoutCommittingTransaction()
    {
        var trustDetailsError = Error.Validation("OPS_ACC_TRUST", "No fue posible obtener la información del fideicomiso.");
        var fixture = TestFixture.Create()
            .WithTrustDetailsFailure(trustDetailsError);
        var sut = fixture.CreateSut();

        var result = await sut.ExecuteAsync(
            fixture.Request,
            fixture.ValidationResult,
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        FluentAssertions.AssertionExtensions.Should(result.Error)
            .BeEquivalentTo(trustDetailsError);
        fixture.TrustDetailsProviderMock.Verify(
            provider => provider.GetAsync(
                fixture.ValidationResult.TrustId,
                It.IsAny<CancellationToken>()),
            Times.Once);
        fixture.TrustOperationRepositoryMock.Verify(
            repository => repository.AddAsync(It.IsAny<TrustOperation>(), It.IsAny<CancellationToken>()),
            Times.Never);
        fixture.TransactionMock.Verify(
            transaction => transaction.CommitAsync(It.IsAny<CancellationToken>()),
            Times.Never);
        fixture.UnitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Exactly(2));
        fixture.TrustUpdaterMock.Verify(
            updater => updater.UpdateAsync(It.IsAny<TrustUpdate>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenTrustUpdaterFails_ReturnsFailureAndDoesNotCompleteOperation()
    {
        var trustUpdateError = Error.Validation("OPS_ACC_UPDATE", "No fue posible actualizar la información del fideicomiso.");
        var fixture = TestFixture.Create()
            .WithTrustUpdaterFailure(trustUpdateError);
        var sut = fixture.CreateSut();

        var result = await sut.ExecuteAsync(
            fixture.Request,
            fixture.ValidationResult,
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        FluentAssertions.AssertionExtensions.Should(result.Error)
            .BeEquivalentTo(trustUpdateError);
        fixture.TrustUpdaterMock.Verify(
            updater => updater.UpdateAsync(
                It.Is<TrustUpdate>(update => update.Status == LifecycleStatus.AnnulledByDebitNote),
                It.IsAny<CancellationToken>()),
            Times.Once);
        fixture.OperationCompletedMock.Verify(
            operationCompleted => operationCompleted.ExecuteAsync(It.IsAny<ClientOperation>(), It.IsAny<CancellationToken>()),
            Times.Never);
        fixture.TransactionMock.Verify(
            transaction => transaction.CommitAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private sealed class TestFixture
    {
        private TestFixture()
        {
            GeneratedDebitNoteId = 99887766;
            UnitValue = 2.5m;
            TrustEarnings = 125m;

            OriginalOperation = ClientOperation
                .Create(
                    DateTime.UtcNow.AddDays(-5),
                    affiliateId: 25,
                    objectiveId: 30,
                    portfolioId: 35,
                    amount: 750m,
                    processDate: DateTime.UtcNow.AddDays(-3),
                    operationTypeId: 45,
                    applicationDate: DateTime.UtcNow.AddDays(-4),
                    status: LifecycleStatus.Active,
                    causeId: 14,
                    trustId: 555,
                    linkedClientOperationId: null,
                    units: 10m)
                .Value;

            OriginalOperationId = 44332211;
            SetClientOperationId(OriginalOperation, OriginalOperationId);

            Request = new AccountingRecordsOperRequest(
                Amount: 1000m,
                CauseId: 77,
                AffiliateId: 25,
                ObjectiveId: 30);

            PortfolioCurrentDate = new DateTime(2024, 05, 01, 0, 0, 0, DateTimeKind.Utc);
            ValidationResult = new AccountingRecordsValidationResult(
                OriginalOperation,
                DebitNoteOperationTypeId: 901,
                TrustAdjustmentOperationTypeId: 902,
                PortfolioCurrentDate,
                TrustId: 555,
                CauseConfigurationParameterId: 77);

            TransactionMock = new Mock<IDbContextTransaction>();
            TransactionMock
                .Setup(transaction => transaction.CommitAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            TransactionMock
                .Setup(transaction => transaction.DisposeAsync())
                .Returns(ValueTask.CompletedTask);

            UnitOfWorkMock = new Mock<IUnitOfWork>();
            UnitOfWorkMock
                .Setup(unitOfWork => unitOfWork.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(TransactionMock.Object);
            UnitOfWorkMock
                .Setup(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            ClientOperationRepositoryMock = new Mock<IClientOperationRepository>();
            ClientOperationRepositoryMock
                .Setup(repository => repository.Insert(It.IsAny<ClientOperation>()))
                .Callback<ClientOperation>(operation =>
                {
                    InsertedDebitNote = operation;
                    SetClientOperationId(operation, GeneratedDebitNoteId);
                });
            ClientOperationRepositoryMock
                .Setup(repository => repository.Update(It.IsAny<ClientOperation>()));

            TrustOperationRepositoryMock = new Mock<ITrustOperationRepository>();
            TrustOperationRepositoryMock
                .Setup(repository => repository.AddAsync(It.IsAny<TrustOperation>(), It.IsAny<CancellationToken>()))
                .Callback<TrustOperation, CancellationToken>((operation, _) => AddedTrustOperation = operation)
                .Returns(Task.CompletedTask);

            OperationCompletedMock = new Mock<IOperationCompleted>();
            OperationCompletedMock
                .Setup(operationCompleted => operationCompleted.ExecuteAsync(
                    It.IsAny<ClientOperation>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            PortfolioValuationProviderMock = new Mock<IPortfolioValuationProvider>();
            TrustDetailsProviderMock = new Mock<ITrustDetailsProvider>();
            TrustUpdaterMock = new Mock<ITrustUpdater>();

            _portfolioValuationResult = Result.Success(UnitValue);
            _trustDetailsResult = Result.Success(new TrustDetailsResult(TrustEarnings));
            _trustUpdaterResult = Result.Success();

            PortfolioValuationProviderMock
                .Setup(provider => provider.GetUnitValueAsync(
                    It.IsAny<int>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()))
                .Returns((int _, DateTime _, CancellationToken _) =>
                    Task.FromResult(_portfolioValuationResult ?? Result.Success(UnitValue)));

            TrustDetailsProviderMock
                .Setup(provider => provider.GetAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
                .Returns((long _, CancellationToken _) =>
                    Task.FromResult(_trustDetailsResult ?? Result.Success(new TrustDetailsResult(TrustEarnings))));

            TrustUpdaterMock
                .Setup(updater => updater.UpdateAsync(It.IsAny<TrustUpdate>(), It.IsAny<CancellationToken>()))
                .Returns((TrustUpdate update, CancellationToken _) =>
                {
                    SentTrustUpdate = update;
                    return Task.FromResult(_trustUpdaterResult ?? Result.Success());
                });
        }

        public static TestFixture Create() => new();

        public AccountingRecordsOperRequest Request { get; }

        public AccountingRecordsValidationResult ValidationResult { get; }

        public ClientOperation OriginalOperation { get; }

        public ClientOperation? InsertedDebitNote { get; private set; }

        public TrustOperation? AddedTrustOperation { get; private set; }

        public TrustUpdate? SentTrustUpdate { get; private set; }

        public decimal UnitValue { get; private set; }

        public decimal TrustEarnings { get; private set; }

        public DateTime PortfolioCurrentDate { get; }

        public DateTime ExpectedProcessDate => DateTime.SpecifyKind(
            PortfolioCurrentDate.AddDays(1),
            DateTimeKind.Utc);

        public long GeneratedDebitNoteId { get; }

        public long OriginalOperationId { get; }

        public Mock<IUnitOfWork> UnitOfWorkMock { get; }

        public Mock<IClientOperationRepository> ClientOperationRepositoryMock { get; }

        public Mock<IOperationCompleted> OperationCompletedMock { get; }

        public Mock<ITrustUpdater> TrustUpdaterMock { get; }

        public Mock<IPortfolioValuationProvider> PortfolioValuationProviderMock { get; }

        public Mock<ITrustOperationRepository> TrustOperationRepositoryMock { get; }

        public Mock<ITrustDetailsProvider> TrustDetailsProviderMock { get; }

        public Mock<IDbContextTransaction> TransactionMock { get; }

        public AccountingRecordsOper CreateSut()
        {
            return new AccountingRecordsOper(
                UnitOfWorkMock.Object,
                ClientOperationRepositoryMock.Object,
                OperationCompletedMock.Object,
                TrustUpdaterMock.Object,
                PortfolioValuationProviderMock.Object,
                TrustOperationRepositoryMock.Object,
                TrustDetailsProviderMock.Object);
        }

        public TestFixture WithPortfolioValuationFailure(Error error)
        {
            _portfolioValuationResult = Result.Failure<decimal>(error);
            return this;
        }

        public TestFixture WithUnitValue(decimal unitValue)
        {
            UnitValue = unitValue;
            _portfolioValuationResult = Result.Success(unitValue);
            return this;
        }

        public TestFixture WithTrustDetailsFailure(Error error)
        {
            _trustDetailsResult = Result.Failure<TrustDetailsResult>(error);
            return this;
        }

        public TestFixture WithTrustEarnings(decimal earnings)
        {
            TrustEarnings = earnings;
            _trustDetailsResult = Result.Success(new TrustDetailsResult(earnings));
            return this;
        }

        public TestFixture WithTrustUpdaterFailure(Error error)
        {
            _trustUpdaterResult = Result.Failure(error);
            return this;
        }

        private static void SetClientOperationId(ClientOperation operation, long id)
        {
            typeof(ClientOperation)
                .GetProperty(
                    nameof(ClientOperation.ClientOperationId),
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
                .SetValue(operation, id);
        }

        private Result<decimal>? _portfolioValuationResult;
        private Result<TrustDetailsResult>? _trustDetailsResult;
        private Result? _trustUpdaterResult;
    }
}
