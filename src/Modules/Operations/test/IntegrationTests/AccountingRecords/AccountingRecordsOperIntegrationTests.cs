using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Microsoft.EntityFrameworkCore.Storage;
using Operations.Application.Abstractions.Data;
using Operations.Application.Abstractions.External;
using Operations.Application.Abstractions.Services.AccountingRecords;
using Operations.Application.Abstractions.Services.OperationCompleted;
using Operations.Application.AccountingRecords.Services;
using Operations.Domain.ClientOperations;
using Operations.Domain.TrustOperations;
using Xunit;

namespace Operations.test.IntegrationTests.AccountingRecords;

public class AccountingRecordsOperIntegrationTests
{
    [Fact]
    public async Task ExecuteAsync_WhenDependenciesSucceed_PersistsDebitNoteAndTrustOperation()
    {
        // Arrange
        var fixture = AccountingRecordsOperIntegrationTestFixture.Create();
        var sut = fixture.CreateSut();
        var beforeExecution = DateTime.UtcNow;

        // Act
        var result = await sut.ExecuteAsync(
            fixture.Request,
            fixture.ValidationResult,
            CancellationToken.None);

        var afterExecution = DateTime.UtcNow;

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(fixture.GeneratedDebitNoteId, result.Value.DebitNoteId);
        Assert.Equal(fixture.ExpectedSuccessMessage, result.Value.Message);
        Assert.Equal(3, fixture.UnitOfWork.SaveChangesCallCount);
        Assert.True(fixture.UnitOfWork.Transaction?.HasCommitted);
        Assert.True(fixture.UnitOfWork.Transaction?.IsDisposed);

        var debitNote = fixture.ClientOperationRepository.InsertedDebitNote;
        Assert.NotNull(debitNote);
        Assert.Equal(fixture.Request.AffiliateId, debitNote!.AffiliateId);
        Assert.Equal(fixture.Request.ObjectiveId, debitNote.ObjectiveId);
        Assert.Equal(fixture.OriginalOperation.PortfolioId, debitNote.PortfolioId);
        Assert.Equal(fixture.Request.Amount, debitNote.Amount);
        Assert.Equal(fixture.ValidationResult.DebitNoteOperationTypeId, debitNote.OperationTypeId);
        Assert.Equal(LifecycleStatus.Active, debitNote.Status);
        Assert.Equal(fixture.Request.CauseId, debitNote.CauseId);
        Assert.Equal(fixture.ValidationResult.TrustId, debitNote.TrustId);
        Assert.Equal(fixture.OriginalOperation.ClientOperationId, debitNote.LinkedClientOperationId);
        Assert.Equal(fixture.ExpectedProcessDate, debitNote.ProcessDate);

        var expectedUnits = decimal.Round(
            fixture.Request.Amount / fixture.UnitValue,
            16,
            MidpointRounding.AwayFromZero);
        Assert.Equal(expectedUnits, debitNote.Units);
        Assert.True(debitNote.RegistrationDate >= beforeExecution);
        Assert.True(debitNote.RegistrationDate <= afterExecution);
        Assert.True(debitNote.ApplicationDate >= beforeExecution);
        Assert.True(debitNote.ApplicationDate <= afterExecution);

        var updatedOperation = fixture.ClientOperationRepository.UpdatedOperation;
        Assert.NotNull(updatedOperation);
        Assert.Equal(LifecycleStatus.AnnulledByDebitNote, updatedOperation!.Status);

        var addedTrustOperation = fixture.TrustOperationRepository.AddedOperations.Single();
        Assert.Equal(fixture.GeneratedDebitNoteId, addedTrustOperation.ClientOperationId);
        Assert.Equal(fixture.ValidationResult.TrustId, addedTrustOperation.TrustId);
        Assert.Equal(-fixture.TrustEarnings, addedTrustOperation.Amount);
        Assert.Equal(fixture.ValidationResult.TrustAdjustmentOperationTypeId, addedTrustOperation.OperationTypeId);
        Assert.Equal(fixture.OriginalOperation.PortfolioId, addedTrustOperation.PortfolioId);
        Assert.Equal(fixture.ExpectedProcessDate, addedTrustOperation.ProcessDate);

        var expectedTrustUnits = decimal.Round(
            fixture.TrustEarnings / fixture.UnitValue,
            16,
            MidpointRounding.AwayFromZero);
        Assert.Equal(expectedTrustUnits, addedTrustOperation.Units);

        var sentUpdate = fixture.TrustUpdater.LastUpdate;
        Assert.NotNull(sentUpdate);
        Assert.Equal(fixture.OriginalOperation.ClientOperationId, sentUpdate!.ClientOperationId);
        Assert.Equal(LifecycleStatus.AnnulledByDebitNote, sentUpdate.Status);
        Assert.Equal(fixture.ExpectedProcessDate, sentUpdate.UpdateDate);

        Assert.Equal(2, fixture.OperationCompleted.ExecutedOperations.Count);
        Assert.Same(fixture.OriginalOperation, fixture.OperationCompleted.ExecutedOperations[0]);
        Assert.Same(debitNote, fixture.OperationCompleted.ExecutedOperations[1]);
        Assert.Same(debitNote, fixture.OperationCompleted.ExecutedOperation);
    }

    [Fact]
    public async Task ExecuteAsync_WhenTrustDetailsFail_RollsBackTransaction()
    {
        // Arrange
        var fixture = AccountingRecordsOperIntegrationTestFixture.Create()
            .WithTrustDetailsFailure(Error.Validation("OPS_ACC_TRUST", "No fue posible obtener la información del fideicomiso."));
        var sut = fixture.CreateSut();

        // Act
        var result = await sut.ExecuteAsync(
            fixture.Request,
            fixture.ValidationResult,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("OPS_ACC_TRUST", result.Error.Code);
        Assert.Equal(2, fixture.UnitOfWork.SaveChangesCallCount);
        Assert.False(fixture.UnitOfWork.Transaction?.HasCommitted);
        Assert.True(fixture.UnitOfWork.Transaction?.IsDisposed);
        Assert.Empty(fixture.TrustOperationRepository.AddedOperations);
        Assert.Null(fixture.TrustUpdater.LastUpdate);
        Assert.Single(fixture.OperationCompleted.ExecutedOperations);
        Assert.Same(fixture.OriginalOperation, fixture.OperationCompleted.ExecutedOperations[0]);
        Assert.NotNull(fixture.ClientOperationRepository.InsertedDebitNote);
        Assert.NotSame(fixture.ClientOperationRepository.InsertedDebitNote, fixture.OperationCompleted.ExecutedOperation);
    }

    [Fact]
    public async Task ExecuteAsync_WhenTrustUpdaterFails_DoesNotCompleteOperation()
    {
        // Arrange
        var fixture = AccountingRecordsOperIntegrationTestFixture.Create()
            .WithTrustUpdaterFailure(Error.Validation("OPS_ACC_UPDATE", "No fue posible actualizar el fideicomiso."));
        var sut = fixture.CreateSut();

        // Act
        var result = await sut.ExecuteAsync(
            fixture.Request,
            fixture.ValidationResult,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("OPS_ACC_UPDATE", result.Error.Code);
        Assert.Equal(3, fixture.UnitOfWork.SaveChangesCallCount);
        Assert.True(fixture.UnitOfWork.Transaction?.HasCommitted);
        Assert.True(fixture.UnitOfWork.Transaction?.IsDisposed);
        Assert.NotNull(fixture.TrustUpdater.LastUpdate);
        Assert.Single(fixture.OperationCompleted.ExecutedOperations);
        Assert.Same(fixture.OriginalOperation, fixture.OperationCompleted.ExecutedOperations[0]);
        Assert.NotNull(fixture.ClientOperationRepository.InsertedDebitNote);
        Assert.NotSame(fixture.ClientOperationRepository.InsertedDebitNote, fixture.OperationCompleted.ExecutedOperation);
    }

    private sealed class AccountingRecordsOperIntegrationTestFixture
    {
        private AccountingRecordsOperIntegrationTestFixture()
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

            PortfolioCurrentDate = new DateTime(2024, 5, 1, 0, 0, 0, DateTimeKind.Utc);
            ValidationResult = new AccountingRecordsValidationResult(
                OriginalOperation,
                DebitNoteOperationTypeId: 901,
                TrustAdjustmentOperationTypeId: 902,
                PortfolioCurrentDate,
                TrustId: 555,
                CauseConfigurationParameterId: 77);

            UnitOfWork = new FakeUnitOfWork();
            ClientOperationRepository = new FakeClientOperationRepository(GeneratedDebitNoteId);
            TrustOperationRepository = new FakeTrustOperationRepository();
            OperationCompleted = new FakeOperationCompleted();
            TrustUpdater = new FakeTrustUpdater();
            PortfolioValuationProvider = new FakePortfolioValuationProvider(UnitValue);
            TrustDetailsProvider = new FakeTrustDetailsProvider(Result.Success(new TrustDetailsResult(TrustEarnings)));
        }

        public static AccountingRecordsOperIntegrationTestFixture Create() => new();

        public AccountingRecordsOperRequest Request { get; }

        public AccountingRecordsValidationResult ValidationResult { get; }

        public ClientOperation OriginalOperation { get; }

        public FakeUnitOfWork UnitOfWork { get; }

        public FakeClientOperationRepository ClientOperationRepository { get; }

        public FakeTrustOperationRepository TrustOperationRepository { get; }

        public FakeOperationCompleted OperationCompleted { get; }

        public FakeTrustUpdater TrustUpdater { get; }

        public FakePortfolioValuationProvider PortfolioValuationProvider { get; }

        public FakeTrustDetailsProvider TrustDetailsProvider { get; private set; }

        public decimal UnitValue { get; }

        public decimal TrustEarnings { get; private set; }

        public DateTime PortfolioCurrentDate { get; }

        public long GeneratedDebitNoteId { get; }

        public long OriginalOperationId { get; }

        public DateTime ExpectedProcessDate => DateTime.SpecifyKind(
            PortfolioCurrentDate.AddDays(1),
            DateTimeKind.Utc);

        public string ExpectedSuccessMessage =>
            $"La nota débito se creó correctamente. ID de la operación: {GeneratedDebitNoteId}.";

        public AccountingRecordsOperIntegrationTestFixture WithTrustDetailsFailure(Error error)
        {
            TrustDetailsProvider = new FakeTrustDetailsProvider(Result.Failure<TrustDetailsResult>(error));
            return this;
        }

        public AccountingRecordsOperIntegrationTestFixture WithTrustUpdaterFailure(Error error)
        {
            TrustUpdater.NextResult = Result.Failure(error);
            return this;
        }

        public AccountingRecordsOper CreateSut()
        {
            return new AccountingRecordsOper(
                UnitOfWork,
                ClientOperationRepository,
                OperationCompleted,
                TrustUpdater,
                PortfolioValuationProvider,
                TrustOperationRepository,
                TrustDetailsProvider);
        }

        private static void SetClientOperationId(ClientOperation operation, long id)
        {
            typeof(ClientOperation)
                .GetProperty(
                    nameof(ClientOperation.ClientOperationId),
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
                .SetValue(operation, id);
        }
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int SaveChangesCallCount { get; private set; }

        public FakeDbContextTransaction? Transaction { get; private set; }

        public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            Transaction = new FakeDbContextTransaction();
            return Task.FromResult<IDbContextTransaction>(Transaction);
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesCallCount++;
            return Task.FromResult(1);
        }

        public IDbConnection GetDbConnection() => throw new NotSupportedException();
    }

    private sealed class FakeDbContextTransaction : IDbContextTransaction
    {
        public Guid TransactionId { get; } = Guid.NewGuid();

        public bool HasCommitted { get; private set; }

        public bool HasRolledBack { get; private set; }

        public bool IsDisposed { get; private set; }

        public void Commit()
        {
            HasCommitted = true;
        }

        public Task CommitAsync(CancellationToken cancellationToken = default)
        {
            HasCommitted = true;
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            IsDisposed = true;
        }

        public ValueTask DisposeAsync()
        {
            IsDisposed = true;
            return ValueTask.CompletedTask;
        }

        public IDbContextTransaction GetDbTransaction() => this;

        public void Rollback()
        {
            HasRolledBack = true;
        }

        public Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            HasRolledBack = true;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeClientOperationRepository : IClientOperationRepository
    {
        private readonly long _generatedDebitNoteId;

        public FakeClientOperationRepository(long generatedDebitNoteId)
        {
            _generatedDebitNoteId = generatedDebitNoteId;
        }

        public ClientOperation? InsertedDebitNote { get; private set; }

        public ClientOperation? UpdatedOperation { get; private set; }

        public void Insert(ClientOperation clientoperation)
        {
            InsertedDebitNote = clientoperation;
            SetClientOperationId(clientoperation, _generatedDebitNoteId);
        }

        public void Update(ClientOperation clientoperation)
        {
            UpdatedOperation = clientoperation;
        }

        public Task<IReadOnlyCollection<ClientOperation>> GetAllAsync(CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<ClientOperation?> GetAsync(long clientoperationId, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public void Delete(ClientOperation clientoperation) => throw new NotSupportedException();

        public Task<bool> ExistsContributionAsync(int affiliateId, int objectiveId, int portfolioId, CancellationToken ct) =>
            throw new NotSupportedException();

        public Task<IEnumerable<ClientOperation>> GetClientOperationsByProcessDateAsync(
            DateTime processDate,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<IEnumerable<ClientOperation>> GetAccountingOperationsAsync(
            IEnumerable<int> PortfolioId,
            DateTime processDate,
            string clientOperationTypeName,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<bool> HasActiveLinkedOperationAsync(
            long clientOperationId,
            long operationTypeId,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<IReadOnlyCollection<ClientOperation>> GetContributionOperationsInRangeAsync(
            IReadOnlyCollection<long> contributionOperationTypeIds,
            int affiliateId,
            int objectiveId,
            DateTime startDate,
            DateTime endDate,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<IReadOnlyCollection<ClientOperation>> GetContributionOperationsAsync(
            IReadOnlyCollection<long> contributionOperationTypeIds,
            int affiliateId,
            int objectiveId,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        private static void SetClientOperationId(ClientOperation operation, long id)
        {
            typeof(ClientOperation)
                .GetProperty(
                    nameof(ClientOperation.ClientOperationId),
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
                .SetValue(operation, id);
        }
    }

    private sealed class FakeTrustOperationRepository : ITrustOperationRepository
    {
        public List<TrustOperation> AddedOperations { get; } = new();

        public Task AddAsync(TrustOperation operation, CancellationToken cancellationToken)
        {
            AddedOperations.Add(operation);
            return Task.CompletedTask;
        }

        public Task<TrustOperation?> GetForUpdateByPortfolioTrustAndDateAsync(
            int portfolioId,
            long trustId,
            DateTime closingDate,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public void Update(TrustOperation operation) => throw new NotSupportedException();

        public Task<TrustOperation?> GetByPortfolioAndTrustAsync(
            int portfolioId,
            long trustId,
            DateTime closingDate,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<IReadOnlyCollection<TrustOperation>> GetByPortfolioProcessDateAndOperationTypeAsync(
            int portfolioId,
            DateTime processDate,
            long operationTypeId,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();
    }

    private sealed class FakeOperationCompleted : IOperationCompleted
    {
        public ClientOperation? ExecutedOperation => ExecutedOperations.LastOrDefault();

        public List<ClientOperation> ExecutedOperations { get; } = new();

        public Task ExecuteAsync(ClientOperation operation, CancellationToken cancellationToken)
        {
            ExecutedOperations.Add(operation);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeTrustUpdater : ITrustUpdater
    {
        public Result NextResult { get; set; } = Result.Success();

        public TrustUpdate? LastUpdate { get; private set; }

        public Task<Result> UpdateAsync(TrustUpdate update, CancellationToken cancellationToken)
        {
            LastUpdate = update;
            return Task.FromResult(NextResult);
        }
    }

    private sealed class FakePortfolioValuationProvider : IPortfolioValuationProvider
    {
        public FakePortfolioValuationProvider(decimal unitValue)
        {
            NextResult = Result.Success(unitValue);
        }

        public Result<decimal> NextResult { get; set; }

        public Task<Result<decimal>> GetUnitValueAsync(int portfolioId, DateTime closingDate, CancellationToken cancellationToken)
        {
            return Task.FromResult(NextResult);
        }
    }

    private sealed class FakeTrustDetailsProvider : ITrustDetailsProvider
    {
        public FakeTrustDetailsProvider(Result<TrustDetailsResult> nextResult)
        {
            NextResult = nextResult;
        }

        public Result<TrustDetailsResult> NextResult { get; }

        public Task<Result<TrustDetailsResult>> GetAsync(long trustId, CancellationToken cancellationToken)
        {
            return Task.FromResult(NextResult);
        }
    }
}
