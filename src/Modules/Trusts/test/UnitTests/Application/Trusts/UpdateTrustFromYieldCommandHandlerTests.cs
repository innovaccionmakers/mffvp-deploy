
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Moq;
using Trusts.Application.Abstractions.Data;
using Trusts.Application.Trusts.Commands;
using Trusts.Domain.Trusts;
using Trusts.Domain.Trusts.TrustYield;
using Trusts.Integrations.TrustYields.Commands;

namespace Trusts.test.UnitTests.Application.Trusts.Commands
{
    public class UpdateTrustFromYieldCommandHandlerTests
    {
        private readonly Mock<ITrustBulkRepository> trustBulkRepository = new();
        private readonly Mock<IUnitOfWork> unitOfWork = new();
        private readonly Mock<ILogger<UpdateTrustFromYieldCommandHandler>> logger = new();

        private UpdateTrustFromYieldCommandHandler CreateSut()
            => new(trustBulkRepository.Object, unitOfWork.Object, logger.Object);

        [Fact]
        public async Task ReturnsSuccessWithZeroCountersWhenRowsIsNull()
        {
            var handler = CreateSut();
            var request = new UpdateTrustFromYieldCommand(
                BatchIndex: 1,
                Rows: new List<ApplyYieldRow>()
            );

            var result = await handler.Handle(request, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value.BatchIndex.Should().Be(1);
            result.Value.Updated.Should().Be(0);
            result.Value.MissingTrustIds.Should().BeEmpty();
            result.Value.ValidationMismatchTrustIds.Should().BeEmpty();

            trustBulkRepository.Verify(
                r => r.ApplyYieldToBalanceBulkAsync(It.IsAny<IReadOnlyList<ApplyYieldRow>>(), It.IsAny<CancellationToken>()),
                Times.Never);
            unitOfWork.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ReturnsSuccessWithZeroCountersWhenRowsIsEmpty()
        {
            var handler = CreateSut();
            var request = new UpdateTrustFromYieldCommand(
                BatchIndex: 7,
                Rows: new List<ApplyYieldRow>()
            );

            var result = await handler.Handle(request, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value.BatchIndex.Should().Be(7);
            result.Value.Updated.Should().Be(0);
            result.Value.MissingTrustIds.Should().BeEmpty();
            result.Value.ValidationMismatchTrustIds.Should().BeEmpty();

            trustBulkRepository.Verify(
                r => r.ApplyYieldToBalanceBulkAsync(It.IsAny<IReadOnlyList<ApplyYieldRow>>(), It.IsAny<CancellationToken>()),
                Times.Never);
            unitOfWork.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CallsRepositoryWithRoundedValuesAndCommitsTransaction()
        {
            // Arrange
            var handler = CreateSut();

            var inputRows = new List<ApplyYieldRow>
            {
                new(TrustId: 101, YieldAmount: 10.234m, YieldRetentionRate: 1.236m, ClosingBalance: 200.234m),
                new(TrustId: 202, YieldAmount: 99.236m, YieldRetentionRate: 0.234m, ClosingBalance: 1000.236m)
            };
            var request = new UpdateTrustFromYieldCommand(BatchIndex: 3, Rows: inputRows);

            var transactionMock = new Mock<IDbContextTransaction>();
            unitOfWork
                .Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(transactionMock.Object);

            IReadOnlyList<ApplyYieldRow>? captured = null;

            trustBulkRepository
                .Setup(r => r.ApplyYieldToBalanceBulkAsync(
                    It.IsAny<IReadOnlyList<ApplyYieldRow>>(), 
                    It.IsAny<CancellationToken>()))
                .Callback((IReadOnlyList<ApplyYieldRow> rows, CancellationToken _) => captured = rows) 
                .ReturnsAsync(new ApplyYieldBulkResult(
                    Updated: 2,
                    MissingTrustIds: Array.Empty<long>(),
                    ValidationMismatchTrustIds: Array.Empty<long>()));


            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert: éxito y mapeo del repo
            result.IsSuccess.Should().BeTrue();
            result.Value.BatchIndex.Should().Be(3);
            result.Value.Updated.Should().Be(2);
            result.Value.MissingTrustIds.Should().BeEmpty();
            result.Value.ValidationMismatchTrustIds.Should().BeEmpty();

            // Assert: redondeo aplicado antes de llamar al repo
            captured.Should().NotBeNull();
            captured!.Count.Should().Be(2);

            var r1 = captured[0];
            r1.TrustId.Should().Be(101);
            r1.YieldAmount.Should().Be(10.23m);
            r1.YieldRetentionRate.Should().Be(1.24m);
            r1.ClosingBalance.Should().Be(200.23m);

            var r2 = captured[1];
            r2.TrustId.Should().Be(202);
            r2.YieldAmount.Should().Be(99.24m);
            r2.YieldRetentionRate.Should().Be(0.23m);
            r2.ClosingBalance.Should().Be(1000.24m);

            // Assert: transacción iniciada y confirmada
            unitOfWork.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
            transactionMock.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DoesNotStartTransactionWhenNoRows()
        {
            var handler = CreateSut();
            var request = new UpdateTrustFromYieldCommand(BatchIndex: 9, Rows: new List<ApplyYieldRow>());

            var result = await handler.Handle(request, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            unitOfWork.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ReturnsFailureWhenRepositoryThrows()
        {
            // Arrange
            var handler = CreateSut();

            var inputRows = new List<ApplyYieldRow>
            {
                new(TrustId: 1, YieldAmount: 1.11m, YieldRetentionRate: 0.11m, ClosingBalance: 10.11m)
            };
            var request = new UpdateTrustFromYieldCommand(BatchIndex: 5, Rows: inputRows);

            var transactionMock = new Mock<IDbContextTransaction>();
            unitOfWork
                .Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(transactionMock.Object);

            trustBulkRepository
                .Setup(r => r.ApplyYieldToBalanceBulkAsync(It.IsAny<IReadOnlyList<ApplyYieldRow>>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("boom"));

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error!.Code.Should().Be("TRU-BULK-001");

            // Se inició la transacción pero no debería confirmar
            unitOfWork.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
            transactionMock.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
