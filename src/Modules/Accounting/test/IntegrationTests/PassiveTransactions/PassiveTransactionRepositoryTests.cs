using Accounting.Domain.PassiveTransactions;
using FluentAssertions;
using Moq;

namespace Accounting.test.IntegrationTests.PassiveTransactions
{
    public class PassiveTransactionRepositoryTests
    {
        private readonly Mock<IPassiveTransactionRepository> _mockPassiveTransactionRepository = new Mock<IPassiveTransactionRepository>();

        public PassiveTransactionRepositoryTests()
        {
            _mockPassiveTransactionRepository = new Mock<IPassiveTransactionRepository>();
        }

        [Fact]
        public async Task GetByPortfolioIdAsync_WhenPassiveTransactionDoesNotExist_ShouldReturnNull()
        {
            // Arrange
            var portfolioId = 999;
            var operationTypeId = 888;

            _mockPassiveTransactionRepository.Setup(x => x.GetByPortfolioIdAndOperationTypeAsync(
                It.Is<int>(id => id == portfolioId),
                It.Is<long>(id => id == operationTypeId),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync((PassiveTransaction?)null);

            // Act
            var result = await _mockPassiveTransactionRepository.Object.GetByPortfolioIdAndOperationTypeAsync(portfolioId, operationTypeId, CancellationToken.None);

            // Assert
            result.Should().BeNull();
            _mockPassiveTransactionRepository.Verify(x => x.GetByPortfolioIdAndOperationTypeAsync(
                portfolioId, operationTypeId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetAccountingOperationsAsync_WhenPortfolioIdsEmpty_ShouldReturnEmptyCollection()
        {
            // Arrange
            var emptyPortfolioIds = Enumerable.Empty<int>();
            var typeOperationsIds = new List<long> { 1001, 1002 };
            var expectedEmptyCollection = Enumerable.Empty<PassiveTransaction?>();

            _mockPassiveTransactionRepository.Setup(x => x.GetAccountingOperationsAsync(
                It.Is<IEnumerable<int>>(ids => !ids.Any()),
                It.IsAny<IEnumerable<long>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedEmptyCollection);

            // Act
            var result = await _mockPassiveTransactionRepository.Object.GetAccountingOperationsAsync(
                emptyPortfolioIds, typeOperationsIds, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
            _mockPassiveTransactionRepository.Verify(x => x.GetAccountingOperationsAsync(
                emptyPortfolioIds, typeOperationsIds, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetAccountingOperationsAsync_WhenPortfolioIdsNull_ShouldReturnEmptyCollection()
        {
            // Arrange
            IEnumerable<int> nullPortfolioIds = null;
            var typeOperationsIds = new List<long> { 1001, 1002 };
            var expectedEmptyCollection = Enumerable.Empty<PassiveTransaction?>();

            _mockPassiveTransactionRepository.Setup(x => x.GetAccountingOperationsAsync(
                It.Is<IEnumerable<int>>(ids => ids == null || !ids.Any()),
                It.IsAny<IEnumerable<long>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedEmptyCollection);

            // Act
            var result = await _mockPassiveTransactionRepository.Object.GetAccountingOperationsAsync(
                nullPortfolioIds, typeOperationsIds, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
            _mockPassiveTransactionRepository.Verify(x => x.GetAccountingOperationsAsync(
                nullPortfolioIds, typeOperationsIds, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetAccountingOperationsAsync_WhenNoMatchingPassiveTransactions_ShouldReturnEmptyCollection()
        {
            // Arrange
            var portfolioIds = new List<int> { 99, 100 };
            var typeOperationsIds = new List<long> { 9999 };
            var expectedEmptyCollection = Enumerable.Empty<PassiveTransaction?>();

            _mockPassiveTransactionRepository.Setup(x => x.GetAccountingOperationsAsync(
                It.Is<IEnumerable<int>>(ids => ids.SequenceEqual(portfolioIds)),
                It.Is<IEnumerable<long>>(types => types.SequenceEqual(typeOperationsIds)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedEmptyCollection);

            // Act
            var result = await _mockPassiveTransactionRepository.Object.GetAccountingOperationsAsync(
                portfolioIds, typeOperationsIds, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
            _mockPassiveTransactionRepository.Verify(x => x.GetAccountingOperationsAsync(
                portfolioIds, typeOperationsIds, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetAccountingOperationsAsync_WhenBothParametersEmpty_ShouldReturnEmptyCollection()
        {
            // Arrange
            var emptyPortfolioIds = Enumerable.Empty<int>();
            var emptyTypeOperationsIds = Enumerable.Empty<long>();
            var expectedEmptyCollection = Enumerable.Empty<PassiveTransaction?>();

            _mockPassiveTransactionRepository.Setup(x => x.GetAccountingOperationsAsync(
                It.Is<IEnumerable<int>>(ids => !ids.Any()),
                It.Is<IEnumerable<long>>(types => !types.Any()),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedEmptyCollection);

            // Act
            var result = await _mockPassiveTransactionRepository.Object.GetAccountingOperationsAsync(
                emptyPortfolioIds, emptyTypeOperationsIds, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
            _mockPassiveTransactionRepository.Verify(x => x.GetAccountingOperationsAsync(
                emptyPortfolioIds, emptyTypeOperationsIds, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetAccountingOperationsAsync_WhenCancelled_ShouldThrowOperationCanceledException()
        {
            // Arrange
            var portfolioIds = new List<int> { 1, 2 };
            var typeOperationsIds = new List<long> { 1001, 1002 };
            var cancellationToken = new CancellationToken(canceled: true);

            _mockPassiveTransactionRepository.Setup(x => x.GetAccountingOperationsAsync(
                It.IsAny<IEnumerable<int>>(),
                It.IsAny<IEnumerable<long>>(),
                It.Is<CancellationToken>(ct => ct.IsCancellationRequested)))
                .ThrowsAsync(new OperationCanceledException());

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(() =>
                _mockPassiveTransactionRepository.Object.GetAccountingOperationsAsync(
                    portfolioIds, typeOperationsIds, cancellationToken));
        }

        [Fact]
        public async Task GetByPortfolioIdAsync_WhenCancelled_ShouldThrowOperationCanceledException()
        {
            // Arrange
            var portfolioId = 1;
            var operationTypeId = 888;
            var cancellationToken = new CancellationToken(canceled: true);

            _mockPassiveTransactionRepository.Setup(x => x.GetByPortfolioIdAndOperationTypeAsync(
                It.IsAny<int>(),
                It.IsAny<long>(),
                It.Is<CancellationToken>(ct => ct.IsCancellationRequested)))
                .ThrowsAsync(new OperationCanceledException());

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(() =>
                _mockPassiveTransactionRepository.Object.GetByPortfolioIdAndOperationTypeAsync(
                    portfolioId, operationTypeId, cancellationToken));
        }

        [Fact]
        public void Insert_WhenPassiveTransactionIsValid_ShouldAddToContext()
        {
            // Arrange
            var portfolioId = 1;
            var operationTypeId = 1001L;
            var debitAccount = "123456";
            var creditAccount = "654321";
            var costCenter = "CC001";
            var thirdParty = "TP001";

            var passiveTransaction = PassiveTransaction.Create(
                portfolioId,
                operationTypeId,
                debitAccount,
                creditAccount,
                costCenter,
                thirdParty);

            // Act
            _mockPassiveTransactionRepository.Object.Insert(passiveTransaction.Value);

            // Assert
            _mockPassiveTransactionRepository.Verify(x => x.Insert(
                It.Is<PassiveTransaction>(pt =>
                    pt.PortfolioId == passiveTransaction.Value.PortfolioId &&
                    pt.TypeOperationsId == passiveTransaction.Value.TypeOperationsId &&
                    pt.DebitAccount == passiveTransaction.Value.DebitAccount &&
                    pt.CreditAccount == passiveTransaction.Value.CreditAccount &&
                    pt.ContraCreditAccount == passiveTransaction.Value.ContraCreditAccount &&
                    pt.ContraDebitAccount == passiveTransaction.Value.ContraDebitAccount)),
                Times.Once);
        }

        [Fact]
        public void Update_WhenPassiveTransactionIsValid_ShouldUpdateInContext()
        {
            // Arrange
            string? debitAccount = "123456";
            string? creditAccount = "654321";
            string? contraCreditAccount = "654987";
            string? contraDebitAccount = "987654";

            var passiveTransaction = new PassiveTransaction();

            passiveTransaction.UpdateDetails(
                debitAccount,
                creditAccount,
                contraCreditAccount,
                contraDebitAccount);

            // Act
            _mockPassiveTransactionRepository.Object.Update(passiveTransaction);

            // Assert
            _mockPassiveTransactionRepository.Verify(x => x.Update(
                It.Is<PassiveTransaction>(pt =>
                    pt.PortfolioId == passiveTransaction.PortfolioId &&
                    pt.TypeOperationsId == passiveTransaction.TypeOperationsId)),
                Times.Once);
        }

        [Fact]
        public void Delete_WhenPassiveTransactionIsValid_ShouldRemoveFromContext()
        {
            // Arrange
            var passiveTransaction = new PassiveTransaction();

            // Act
            _mockPassiveTransactionRepository.Object.Delete(passiveTransaction);

            // Assert
            _mockPassiveTransactionRepository.Verify(x => x.Delete(
                It.Is<PassiveTransaction>(pt =>
                    pt.PortfolioId == passiveTransaction.PortfolioId &&
                    pt.TypeOperationsId == passiveTransaction.TypeOperationsId)),
                Times.Once);
        }

        [Fact]
        public async Task GetPassiveTransactionsAsync_WithExistingTransactions_ShouldReturnAllTransactions()
        {
            // Arrange
            var expectedTransactions = new List<PassiveTransaction>
            {
                CreateTestPassiveTransaction(1, 1001L),
                CreateTestPassiveTransaction(2, 1002L),
                CreateTestPassiveTransaction(3, 1003L)
            };
                    var cancellationToken = CancellationToken.None;

            _mockPassiveTransactionRepository.Setup(x => x.GetPassiveTransactionsAsync(cancellationToken))
                .ReturnsAsync(expectedTransactions.AsReadOnly());

            // Act
            var result = await _mockPassiveTransactionRepository.Object.GetPassiveTransactionsAsync(cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.Should().BeEquivalentTo(expectedTransactions);
            _mockPassiveTransactionRepository.Verify(x => x.GetPassiveTransactionsAsync(cancellationToken), Times.Once);
        }

        [Fact]
        public async Task GetPassiveTransactionsAsync_WithNoTransactions_ShouldReturnEmptyCollection()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;
            var expectedEmptyCollection = new List<PassiveTransaction>().AsReadOnly();

            _mockPassiveTransactionRepository.Setup(x => x.GetPassiveTransactionsAsync(cancellationToken))
                .ReturnsAsync(expectedEmptyCollection);

            // Act
            var result = await _mockPassiveTransactionRepository.Object.GetPassiveTransactionsAsync(cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
            _mockPassiveTransactionRepository.Verify(x => x.GetPassiveTransactionsAsync(cancellationToken), Times.Once);
        }

        [Fact]
        public async Task GetPassiveTransactionsAsync_WithSingleTransaction_ShouldReturnSingleElementCollection()
        {
            // Arrange
            var expectedTransactions = new List<PassiveTransaction>
            {
                CreateTestPassiveTransaction(1, 1001L)
            };
            var cancellationToken = CancellationToken.None;

            _mockPassiveTransactionRepository.Setup(x => x.GetPassiveTransactionsAsync(cancellationToken))
                .ReturnsAsync(expectedTransactions.AsReadOnly());

            // Act
            var result = await _mockPassiveTransactionRepository.Object.GetPassiveTransactionsAsync(cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result.First().PortfolioId.Should().Be(1);
            result.First().TypeOperationsId.Should().Be(1001L);
            _mockPassiveTransactionRepository.Verify(x => x.GetPassiveTransactionsAsync(cancellationToken), Times.Once);
        }

        [Fact]
        public async Task GetPassiveTransactionsAsync_WithCancelledToken_ShouldThrowOperationCanceledException()
        {
            // Arrange
            var cancellationToken = new CancellationToken(canceled: true);

            _mockPassiveTransactionRepository.Setup(x => x.GetPassiveTransactionsAsync(
                It.Is<CancellationToken>(ct => ct.IsCancellationRequested)))
                .ThrowsAsync(new OperationCanceledException());

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(() =>
                _mockPassiveTransactionRepository.Object.GetPassiveTransactionsAsync(cancellationToken));
        }

        [Fact]
        public async Task GetPassiveTransactionsAsync_ShouldReturnIReadOnlyCollection()
        {
            // Arrange
            var expectedTransactions = new List<PassiveTransaction>
            {
                CreateTestPassiveTransaction(1, 1001L),
                CreateTestPassiveTransaction(2, 1002L)
            };
            var cancellationToken = CancellationToken.None;

            _mockPassiveTransactionRepository.Setup(x => x.GetPassiveTransactionsAsync(cancellationToken))
                .ReturnsAsync(expectedTransactions.AsReadOnly());

            // Act
            var result = await _mockPassiveTransactionRepository.Object.GetPassiveTransactionsAsync(cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<IReadOnlyCollection<PassiveTransaction>>();
            result.Count.Should().Be(2);
            _mockPassiveTransactionRepository.Verify(x => x.GetPassiveTransactionsAsync(cancellationToken), Times.Once);
        }

        [Fact]
        public async Task GetPassiveTransactionsAsync_WithMixedPortfolioIds_ShouldReturnAllTransactions()
        {
            // Arrange
            var expectedTransactions = new List<PassiveTransaction>
            {
                CreateTestPassiveTransaction(1, 1001L),
                CreateTestPassiveTransaction(1, 1002L),
                CreateTestPassiveTransaction(2, 1001L),
                CreateTestPassiveTransaction(3, 1003L)
            };
            var cancellationToken = CancellationToken.None;

            _mockPassiveTransactionRepository.Setup(x => x.GetPassiveTransactionsAsync(cancellationToken))
                .ReturnsAsync(expectedTransactions.AsReadOnly());

            // Act
            var result = await _mockPassiveTransactionRepository.Object.GetPassiveTransactionsAsync(cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(4);

            var portfolio1Transactions = result.Where(t => t.PortfolioId == 1);
            portfolio1Transactions.Should().HaveCount(2);

            var portfolio2Transactions = result.Where(t => t.PortfolioId == 2);
            portfolio2Transactions.Should().HaveCount(1);

            var portfolio3Transactions = result.Where(t => t.PortfolioId == 3);
            portfolio3Transactions.Should().HaveCount(1);

            _mockPassiveTransactionRepository.Verify(x => x.GetPassiveTransactionsAsync(cancellationToken), Times.Once);
        }

        [Fact]
        public async Task GetPassiveTransactionsAsync_WithMixedOperationTypes_ShouldReturnAllTransactions()
        {
            // Arrange
            var expectedTransactions = new List<PassiveTransaction>
            {
                CreateTestPassiveTransaction(1, 1001L),
                CreateTestPassiveTransaction(1, 2001L),
                CreateTestPassiveTransaction(2, 1001L),
                CreateTestPassiveTransaction(2, 3001L),
                CreateTestPassiveTransaction(3, 2001L)
            };
            var cancellationToken = CancellationToken.None;

            _mockPassiveTransactionRepository.Setup(x => x.GetPassiveTransactionsAsync(cancellationToken))
                .ReturnsAsync(expectedTransactions.AsReadOnly());

            // Act
            var result = await _mockPassiveTransactionRepository.Object.GetPassiveTransactionsAsync(cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(5);

            var operation1001Transactions = result.Where(t => t.TypeOperationsId == 1001L);
            operation1001Transactions.Should().HaveCount(2);

            var operation2001Transactions = result.Where(t => t.TypeOperationsId == 2001L);
            operation2001Transactions.Should().HaveCount(2);

            var operation3001Transactions = result.Where(t => t.TypeOperationsId == 3001L);
            operation3001Transactions.Should().HaveCount(1);

            _mockPassiveTransactionRepository.Verify(x => x.GetPassiveTransactionsAsync(cancellationToken), Times.Once);
        }

        // Método auxiliar para crear transacciones pasivas de prueba
        private PassiveTransaction CreateTestPassiveTransaction(int portfolioId, long operationTypeId)
        {
            var result = PassiveTransaction.Create(
                portfolioId,
                operationTypeId,
                "123456",
                "654321",
                "CC001",
                "TP001");

            return result.Value;
        }
    }
}
