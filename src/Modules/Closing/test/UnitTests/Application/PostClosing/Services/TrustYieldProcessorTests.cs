using Closing.Application.Abstractions.External.Operations.OperationTypes;
using Closing.Application.Abstractions.External.Operations.TrustOperations;
using Closing.Application.Abstractions.External.Trusts.Trusts;
using Closing.Application.PostClosing.Services.TrustYield;
using Closing.Domain.TrustYields;
using Common.SharedKernel.Core.Primitives; 
using Common.SharedKernel.Domain;
using Common.SharedKernel.Domain.OperationTypes;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Collections.Concurrent;


namespace Closing.test.UnitTests.Application.PostClosing.Services
{
    public class TrustYieldProcessorTests
    {
        private readonly Mock<ITrustYieldRepository> trustYieldRepository = new();
        private readonly Mock<IUpsertTrustYieldOperationsRemote> operationsRemote = new();
        private readonly Mock<IUpdateTrustRemote> trustsRemote = new();
        private readonly Mock<IOperationTypesLocator> operationTypesLocator = new();
        private readonly Mock<ILogger<TrustYieldProcessor>> logger = new();

        private static IOptions<TrustYieldOptions> Options(int bulkBatchSize = 3, bool useEmitFilter = false)
            => Microsoft.Extensions.Options.Options.Create(new TrustYieldOptions
            {
                BulkBatchSize = bulkBatchSize,
                UseEmitFilter = useEmitFilter
            });

        private TrustYieldProcessor CreateSut(IOptions<TrustYieldOptions>? options = null)
            => new(
                trustYieldRepository.Object,
                operationsRemote.Object,
                trustsRemote.Object,
                logger.Object,
                operationTypesLocator.Object,
                options ?? Options()
            );

        private static DateTime Utc(int y, int m, int d) =>
            DateTime.SpecifyKind(new DateTime(y, m, d), DateTimeKind.Utc);

        private static TrustYield BuildTrustYield(
            long trustId,
            int portfolioId,
            DateTime closingDateUtc,
            decimal yieldAmount,
            decimal preClosingBalance,
            decimal closingBalance,
            decimal capital,
            decimal participation = 0m,
            decimal units = 0m,
            decimal income = 0m,
            decimal expenses = 0m,
            decimal commissions = 0m,
            decimal cost = 0m,
            decimal contingentRetention = 0m,
            decimal yieldRetention = 0m,
            DateTime? processDateUtc = null)
        {
            var result = TrustYield.Create(
                trustId: trustId,
                portfolioId: portfolioId,
                closingDate: closingDateUtc,
                participation: participation,
                units: units,
                yieldAmount: yieldAmount,
                preClosingBalance: preClosingBalance,
                closingBalance: closingBalance,
                income: income,
                expenses: expenses,
                commissions: commissions,
                cost: cost,
                capital: capital,
                processDate: processDateUtc ?? closingDateUtc,
                contingentRetention: contingentRetention,
                yieldRetention: yieldRetention
            );

            result.IsSuccess.Should().BeTrue("TrustYield.Create debe ser exitoso para test data");
            return result.Value;
        }

        private static Result<T> Ok<T>(T value) => Result.Success(value);
        private static Result<T> Fail<T>(string code, string description) => Result.Failure<T>(Error.Validation(code, description));

        private void SetupOperationTypeSuccess(long operationTypeId = 10)
        {
            operationTypesLocator
                .Setup(x => x.GetOperationTypeByNameAsync("Rendimientos", It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(new OperationTypeInfo(
                    OperationTypeId: operationTypeId,
                    Name: "Rendimientos",
                    Category: null,
                    Nature: IncomeEgressNature.Income,
                    Status: Status.Active,
                    External: "EXT",
                    HomologatedCode: "REN"
                )));
        }

        private void SetupOperationTypeFailure()
        {
            operationTypesLocator
                .Setup(x => x.GetOperationTypeByNameAsync("Rendimientos", It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure<OperationTypeInfo>(Error.Validation("OPTYPE_NOT_FOUND", "Not found")));
        }

        [Fact]
        public async Task ReturnsEarlyWhenOperationTypeIsNotFound()
        {
            SetupOperationTypeFailure();

            var sut = CreateSut();

            await sut.ProcessAsync(portfolioId: 2, closingDateUtc: Utc(2025, 10, 20), CancellationToken.None);

            trustYieldRepository.Verify(r => r.GetReadOnlyByPortfolioAndDateAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
            operationsRemote.Verify(r => r.UpsertYieldOperationsBulkAsync(It.IsAny<UpsertTrustYieldOperationsBulkRemoteRequest>(), It.IsAny<CancellationToken>()), Times.Never);
            trustsRemote.Verify(r => r.UpdateFromYieldAsync(It.IsAny<UpdateTrustFromYieldBulkRemoteRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task SkipsWhenNoTrustYields()
        {
            SetupOperationTypeSuccess();
            trustYieldRepository
                .Setup(r => r.GetReadOnlyByPortfolioAndDateAsync(2, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<TrustYield>());

            var sut = CreateSut();

            await sut.ProcessAsync(2, Utc(2025, 10, 20), CancellationToken.None);

            operationsRemote.Verify(r => r.UpsertYieldOperationsBulkAsync(It.IsAny<UpsertTrustYieldOperationsBulkRemoteRequest>(), It.IsAny<CancellationToken>()), Times.Never);
            trustsRemote.Verify(r => r.UpdateFromYieldAsync(It.IsAny<UpdateTrustFromYieldBulkRemoteRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task SkipsWhenEmitFilterRemovesAll()
        {
            SetupOperationTypeSuccess();

            var date = Utc(2025, 10, 20);
            var portfolioId = 2;

            var yields = new List<TrustYield>
            {
                BuildTrustYield(1, portfolioId, date, 0m, 100m, 100m, capital: 50m),
                BuildTrustYield(2, portfolioId, date, 0m, 200m, 200m, capital: 0m),
            };

            trustYieldRepository
                .Setup(r => r.GetReadOnlyByPortfolioAndDateAsync(portfolioId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(yields);

            var sut = CreateSut(Options(bulkBatchSize: 3, useEmitFilter: true));

            await sut.ProcessAsync(portfolioId, date, CancellationToken.None);

            operationsRemote.Verify(r => r.UpsertYieldOperationsBulkAsync(It.IsAny<UpsertTrustYieldOperationsBulkRemoteRequest>(), It.IsAny<CancellationToken>()), Times.Never);
            trustsRemote.Verify(r => r.UpdateFromYieldAsync(It.IsAny<UpdateTrustFromYieldBulkRemoteRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CallsOperationsInChunksAndAggregatesChangedTrusts()
        {

            SetupOperationTypeSuccess(operationTypeId: 77);

            var date = Utc(2025, 10, 20);
            var portfolioId = 2;

            var yields = Enumerable.Range(1, 7)
                .Select(i => BuildTrustYield(
                    trustId: i,
                    portfolioId: portfolioId,
                    closingDateUtc: date,
                    yieldAmount: 10m * i,
                    preClosingBalance: 100m * i,
                    closingBalance: 100m * i + 10m,
                    capital: 50m))
                .ToList();

            trustYieldRepository
                .Setup(r => r.GetReadOnlyByPortfolioAndDateAsync(portfolioId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(yields);

            var opRequests = new ConcurrentBag<UpsertTrustYieldOperationsBulkRemoteRequest>();
            operationsRemote
                .Setup(r => r.UpsertYieldOperationsBulkAsync(
                    It.IsAny<UpsertTrustYieldOperationsBulkRemoteRequest>(),
                    It.IsAny<CancellationToken>()))
                .Callback<UpsertTrustYieldOperationsBulkRemoteRequest, CancellationToken>((req, _) => opRequests.Add(req))
                .ReturnsAsync((UpsertTrustYieldOperationsBulkRemoteRequest req, CancellationToken _) =>
                {
                    if (req.IdempotencyKey.EndsWith("-ops-b0"))
                        return Result.Success(new UpsertTrustYieldOperationsBulkRemoteResponse(
                            Inserted: 3, Updated: 0, ChangedTrustIds: new List<long> { 1, 2 }));

                    if (req.IdempotencyKey.EndsWith("-ops-b1"))
                        return Result.Success(new UpsertTrustYieldOperationsBulkRemoteResponse(
                            Inserted: 1, Updated: 2, ChangedTrustIds: new List<long> { 5 }));

                    return Result.Success(new UpsertTrustYieldOperationsBulkRemoteResponse(
                        Inserted: 1, Updated: 0, ChangedTrustIds: new List<long>()));
                });

            var trustRequests = new ConcurrentBag<UpdateTrustFromYieldBulkRemoteRequest>();
            trustsRemote
                .Setup(r => r.UpdateFromYieldAsync(
                    It.IsAny<UpdateTrustFromYieldBulkRemoteRequest>(),
                    It.IsAny<CancellationToken>()))
                .Callback<UpdateTrustFromYieldBulkRemoteRequest, CancellationToken>((req, _) => trustRequests.Add(req))
                .ReturnsAsync((UpdateTrustFromYieldBulkRemoteRequest req, CancellationToken _) =>
                    Result.Success(new UpdateTrustFromYieldBulkRemoteResponse(
                        BatchIndex: req.BatchIndex,
                        Updated: req.TrustsToUpdate?.Count ?? 0,
                        MissingTrustIds: new List<long>(),
                        ValidationMismatchTrustIds: new List<long>())));

            var sut = CreateSut(Options(bulkBatchSize: 3));

            await sut.ProcessAsync(portfolioId, date, CancellationToken.None);

            operationsRemote.Verify(r => r.UpsertYieldOperationsBulkAsync(
                It.IsAny<UpsertTrustYieldOperationsBulkRemoteRequest>(), It.IsAny<CancellationToken>()), Times.Exactly(3));

            var ops = opRequests.ToList();
            ops.Should().HaveCount(3);

            var b0 = ops.Single(r => r.IdempotencyKey.EndsWith("-ops-b0"));
            var b1 = ops.Single(r => r.IdempotencyKey.EndsWith("-ops-b1"));
            var b2 = ops.Single(r => r.IdempotencyKey.EndsWith("-ops-b2"));

            b0.TrustYieldOperations.Should().HaveCount(3);
            b1.TrustYieldOperations.Should().HaveCount(3);
            b2.TrustYieldOperations.Should().HaveCount(1);

            ops.Should().OnlyContain(r => r.OperationTypeId == 77);

            trustsRemote.Verify(r => r.UpdateFromYieldAsync(
                It.IsAny<UpdateTrustFromYieldBulkRemoteRequest>(), It.IsAny<CancellationToken>()), Times.Once);

            var trs = trustRequests.ToList();
            trs.Should().HaveCount(1);

            var tr0 = trs.Single(r => r.IdempotencyKey.EndsWith("-tr-b0"));
            tr0.TrustsToUpdate.Select(t => t.TrustId).OrderBy(x => x)
                .Should().Equal(new[] { 1L, 2L, 5L });
            tr0.BatchIndex.Should().Be(0);
        }


        [Fact]
        public async Task SkipsTrustsWhenNoChangedTrustsInOperations()
        {
            SetupOperationTypeSuccess();

            var date = Utc(2025, 10, 20);
            var portfolioId = 2;

            var yields = Enumerable.Range(1, 5)
                .Select(i => BuildTrustYield(i, portfolioId, date, 10m * i, 100m * i, 100m * i + 10m, capital: 50m))
                .ToList();

            trustYieldRepository
                .Setup(r => r.GetReadOnlyByPortfolioAndDateAsync(portfolioId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(yields);

            operationsRemote
                .Setup(r => r.UpsertYieldOperationsBulkAsync(It.IsAny<UpsertTrustYieldOperationsBulkRemoteRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Ok(new UpsertTrustYieldOperationsBulkRemoteResponse(Inserted: 5, Updated: 0, ChangedTrustIds: new List<long>())));

            var sut = CreateSut(Options(bulkBatchSize: 10));

            await sut.ProcessAsync(portfolioId, date, CancellationToken.None);

            trustsRemote.Verify(r => r.UpdateFromYieldAsync(It.IsAny<UpdateTrustFromYieldBulkRemoteRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CallsTrustsInChunksForOnlyChangedTrusts()
        {

            SetupOperationTypeSuccess();

            var date = Utc(2025, 10, 20);
            var portfolioId = 2;

            var yields = Enumerable.Range(1, 8)
                .Select(i => BuildTrustYield(i, portfolioId, date, 1m, 100m, 101m, capital: 50m))
                .ToList();

            trustYieldRepository
                .Setup(r => r.GetReadOnlyByPortfolioAndDateAsync(portfolioId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(yields);

            var changed = new HashSet<long> { 2, 4, 6, 8 };

            operationsRemote
                .Setup(r => r.UpsertYieldOperationsBulkAsync(It.IsAny<UpsertTrustYieldOperationsBulkRemoteRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((UpsertTrustYieldOperationsBulkRemoteRequest req, CancellationToken _) =>
                {
                    var ids = req.TrustYieldOperations.Select(o => o.TrustId).Where(id => changed.Contains(id)).ToList();
                    return Result.Success(new UpsertTrustYieldOperationsBulkRemoteResponse(
                        Inserted: req.TrustYieldOperations.Count,
                        Updated: 0,
                        ChangedTrustIds: ids));
                });

            var trustBatches = new ConcurrentBag<UpdateTrustFromYieldBulkRemoteRequest>();
            trustsRemote
                .Setup(r => r.UpdateFromYieldAsync(It.IsAny<UpdateTrustFromYieldBulkRemoteRequest>(), It.IsAny<CancellationToken>()))
                .Callback<UpdateTrustFromYieldBulkRemoteRequest, CancellationToken>((req, _) => trustBatches.Add(req))
                .ReturnsAsync((UpdateTrustFromYieldBulkRemoteRequest req, CancellationToken _) =>
                    Result.Success(new UpdateTrustFromYieldBulkRemoteResponse(
                        BatchIndex: req.BatchIndex,
                        Updated: req.TrustsToUpdate?.Count ?? 0,
                        MissingTrustIds: new List<long>(),
                        ValidationMismatchTrustIds: new List<long>())));

            var sut = CreateSut(Options(bulkBatchSize: 3));

            await sut.ProcessAsync(portfolioId, date, CancellationToken.None);

            trustsRemote.Verify(r => r.UpdateFromYieldAsync(
                It.IsAny<UpdateTrustFromYieldBulkRemoteRequest>(), It.IsAny<CancellationToken>()), Times.Exactly(2));

            var trs = trustBatches.ToList();
            trs.Should().HaveCount(2);

            var tr0 = trs.Single(r => r.IdempotencyKey.EndsWith("-tr-b0"));
            var tr1 = trs.Single(r => r.IdempotencyKey.EndsWith("-tr-b1"));

            tr0.TrustsToUpdate.Select(t => t.TrustId).OrderBy(x => x)
               .Should().Equal(new[] { 2L, 4L, 6L });

            tr1.TrustsToUpdate.Select(t => t.TrustId).OrderBy(x => x)
               .Should().Equal(new[] { 8L });
        }

    }
}
