using System.Runtime.Serialization;
using System.Text.Json;
using Closing.Application.Abstractions.External.Products.AccumulatedCommissions;
using Closing.Application.PostClosing.Services.PortfolioCommission;
using Closing.Application.PreClosing.Services.Yield.Constants;
using Closing.Domain.YieldDetails;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain; 
using Moq;

namespace Closing.test.UnitTests.Application.PostClosing.Services
{
    public sealed class PortfolioCommissionServiceTests
    {
        private readonly Mock<IYieldDetailRepository> repositoryMock;
        private readonly Mock<IUpdateAccumulatedCommissionRemote> remoteMock;

        public PortfolioCommissionServiceTests()
        {
            repositoryMock = new(MockBehavior.Strict);
            remoteMock = new(MockBehavior.Strict);
        }

        [Fact]
        public async Task ExecuteAsyncAggregatesByCommissionAndCallsRemoteOncePerCommission()
        {
            // Arrange
            var portfolioId = 7;
            var closingDateLocal = new DateTime(2025, 10, 07, 0, 0, 0, DateTimeKind.Utc); 
            var cancellationToken = CancellationToken.None;

            var details = new List<YieldDetail>
            {
                // Comisión válida id 1 (dos filas => se suman)
                CreateYieldDetail(YieldsSources.Commission, 10m, commissionId: 1),
                CreateYieldDetail(YieldsSources.Commission, 15m, commissionId: 1),

                // Comisión válida id 2
                CreateYieldDetail(YieldsSources.Commission, 5m, commissionId: 2),

                // Comisión con valor 0 => descartada
                CreateYieldDetail(YieldsSources.Commission, 0m, commissionId: 3),

                // Fuente no Comisión => descartada
                CreateYieldDetail("Tesoreria", 999m, commissionId: 4),

                // Concept malformado o sin EntityId => descartada
                CreateYieldDetailWithInvalidConcept(YieldsSources.Commission, 7m)
            };

            repositoryMock
                .Setup(r => r.GetReadOnlyByPortfolioAndDateAsync(
                    portfolioId, closingDateLocal, true, cancellationToken))
                .ReturnsAsync(details);

            remoteMock
                .Setup(x => x.ExecuteAsync(
                    It.Is<UpdateAccumulatedCommissionRemoteRequest>(req =>
                        req.PortfolioId == portfolioId &&
                        req.CommissionId == 1 &&
                        req.AccumulatedValue == 25m &&
                        req.ClosingDateUtc == closingDateLocal.ToUniversalTime() &&
                        req.Origin == "Closing.PortfolioCommission"),
                    cancellationToken))
                .ReturnsAsync(Success(new UpdateAccumulatedCommissionRemoteResponse(true, "OK")));

            remoteMock
                .Setup(x => x.ExecuteAsync(
                    It.Is<UpdateAccumulatedCommissionRemoteRequest>(req =>
                        req.PortfolioId == portfolioId &&
                        req.CommissionId == 2 &&
                        req.AccumulatedValue == 5m &&
                        req.ClosingDateUtc == closingDateLocal.ToUniversalTime() &&
                        req.Origin == "Closing.PortfolioCommission"),
                    cancellationToken))
                .ReturnsAsync(Success(new UpdateAccumulatedCommissionRemoteResponse(true, "OK")));

            var sut = new PortfolioCommissionService(repositoryMock.Object, remoteMock.Object);

            // Act
            await sut.ExecuteAsync(portfolioId, closingDateLocal, cancellationToken);

            // Assert
            repositoryMock.VerifyAll();
            remoteMock.Verify(x => x.ExecuteAsync(It.IsAny<UpdateAccumulatedCommissionRemoteRequest>(), cancellationToken), Times.Exactly(2));
        }

        [Fact]
        public async Task ExecuteAsyncWhenRemoteFailureThrowsAndStopsProcessing()
        {
            // Arrange
            var portfolioId = 8;
            var closingDateUtc = new DateTime(2025, 10, 07, 0, 0, 0, DateTimeKind.Utc);
            var cancellationToken = CancellationToken.None;

            var details = new List<YieldDetail>
            {
                CreateYieldDetail(YieldsSources.Commission, 12m, commissionId: 10)
            };

            repositoryMock
                .Setup(r => r.GetReadOnlyByPortfolioAndDateAsync(portfolioId, closingDateUtc, true, cancellationToken))
                .ReturnsAsync(details);

            remoteMock
                .Setup(x => x.ExecuteAsync(
                    It.Is<UpdateAccumulatedCommissionRemoteRequest>(r =>
                        r.PortfolioId == portfolioId &&
                        r.CommissionId == 10 &&
                        r.AccumulatedValue == 12m &&
                        r.ClosingDateUtc == closingDateUtc),
                    cancellationToken))
                .ReturnsAsync(Failure<UpdateAccumulatedCommissionRemoteResponse>(new Error("RPC-001", "Remote failure", ErrorType.Validation)));

            var sut = new PortfolioCommissionService(repositoryMock.Object, remoteMock.Object);

            // Act + Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                sut.ExecuteAsync(portfolioId, closingDateUtc, cancellationToken));

            remoteMock.Verify(x => x.ExecuteAsync(It.IsAny<UpdateAccumulatedCommissionRemoteRequest>(), cancellationToken), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsyncWhenRemoteRespondsNotSucceededThrows()
        {
            // Arrange
            var portfolioId = 9;
            var closingDateUtc = new DateTime(2025, 10, 07, 0, 0, 0, DateTimeKind.Utc);
            var cancellationToken = CancellationToken.None;

            var details = new List<YieldDetail>
            {
                CreateYieldDetail(YieldsSources.Commission, 3m, commissionId: 20)
            };

            repositoryMock
                .Setup(r => r.GetReadOnlyByPortfolioAndDateAsync(portfolioId, closingDateUtc, true, cancellationToken))
                .ReturnsAsync(details);

            remoteMock
                .Setup(x => x.ExecuteAsync(It.IsAny<UpdateAccumulatedCommissionRemoteRequest>(), cancellationToken))
                .ReturnsAsync(Success(new UpdateAccumulatedCommissionRemoteResponse(false, "Rejected")));

            var sut = new PortfolioCommissionService(repositoryMock.Object, remoteMock.Object);

            // Act + Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                sut.ExecuteAsync(portfolioId, closingDateUtc, cancellationToken));

            remoteMock.Verify(x => x.ExecuteAsync(It.IsAny<UpdateAccumulatedCommissionRemoteRequest>(), cancellationToken), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsyncWhenNoMatchingDetailsDoesNotCallRemote()
        {
            // Arrange
            var portfolioId = 10;
            var closingDateUtc = new DateTime(2025, 10, 07, 0, 0, 0, DateTimeKind.Utc);
            var cancellationToken = CancellationToken.None;

            var details = new List<YieldDetail>
            {
                CreateYieldDetail("Otro", 100m, commissionId: 1),   
                CreateYieldDetail(YieldsSources.Commission, 0m, commissionId: 2), 
                CreateYieldDetailWithInvalidConcept(YieldsSources.Commission, 7m)  
            };

            repositoryMock
                .Setup(r => r.GetReadOnlyByPortfolioAndDateAsync(portfolioId, closingDateUtc, true, cancellationToken))
                .ReturnsAsync(details);

            var sut = new PortfolioCommissionService(repositoryMock.Object, remoteMock.Object);

            // Act
            await sut.ExecuteAsync(portfolioId, closingDateUtc, cancellationToken);

            // Assert
            remoteMock.Verify(x => x.ExecuteAsync(It.IsAny<UpdateAccumulatedCommissionRemoteRequest>(), It.IsAny<CancellationToken>()), Times.Never);
            repositoryMock.VerifyAll();
        }


        private static Result<T> Success<T>(T value) => Result.Success(value);
        private static Result<T> Failure<T>(Error error) => Result.Failure<T>(error);

        private static YieldDetail CreateYieldDetail(string source, decimal commissions, int commissionId)
        {
            var concept = JsonDocument.Parse($"{{\"EntityId\":\"{commissionId}\"}}");
            return CreateYieldDetailCore(source, commissions, concept);
        }

        private static YieldDetail CreateYieldDetailWithInvalidConcept(string source, decimal commissions)
        {
            var concept = JsonDocument.Parse("{\"Other\":\"x\"}"); 
            return CreateYieldDetailCore(source, commissions, concept);
        }

        private static YieldDetail CreateYieldDetailCore(string source, decimal commissions, JsonDocument concept)
        {
            var instance = (YieldDetail)FormatterServices.GetUninitializedObject(typeof(YieldDetail));

            SetBackingField(instance, nameof(YieldDetail.Source), source);
            SetBackingField(instance, nameof(YieldDetail.Commissions), commissions);
            SetBackingField(instance, nameof(YieldDetail.Concept), concept);

            return instance;
        }

        private static void SetBackingField<TObject, TValue>(TObject obj, string propertyName, TValue value)
        {
            var backingFieldName = $"<{propertyName}>k__BackingField";
            var field = typeof(TObject).GetField(backingFieldName,
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic);

            if (field == null)
                throw new MissingFieldException(typeof(TObject).FullName, backingFieldName);

            field.SetValue(obj, value);
        }
    }
}
