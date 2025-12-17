using FluentAssertions;
using Moq;
using Treasury.Domain.TreasuryConcepts;

namespace Treasury.test.IntegrationTests.TreasuryConcepts
{
    public class TreasuryConceptRepositoryTests
    {
        private readonly Mock<ITreasuryConceptRepository> _mockRepository;

        public TreasuryConceptRepositoryTests()
        {
            _mockRepository = new Mock<ITreasuryConceptRepository>();
        }

        [Fact]
        public async Task GetByIdAsync_WithNonExistingId_ShouldReturnNull()
        {
            // Arrange
            var id = 999L;
            var cancellationToken = CancellationToken.None;

            _mockRepository.Setup(x => x.GetByIdAsync(id, cancellationToken))
                .ReturnsAsync((TreasuryConcept?)null);

            // Act
            var result = await _mockRepository.Object.GetByIdAsync(id, cancellationToken);

            // Assert
            result.Should().BeNull();
            _mockRepository.Verify(x => x.GetByIdAsync(id, cancellationToken), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_WithZeroId_ShouldReturnNull()
        {
            // Arrange
            var id = 0L;
            var cancellationToken = CancellationToken.None;

            _mockRepository.Setup(x => x.GetByIdAsync(id, cancellationToken))
                .ReturnsAsync((TreasuryConcept?)null);

            // Act
            var result = await _mockRepository.Object.GetByIdAsync(id, cancellationToken);

            // Assert
            result.Should().BeNull();
            _mockRepository.Verify(x => x.GetByIdAsync(id, cancellationToken), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_WithNegativeId_ShouldReturnNull()
        {
            // Arrange
            var id = -1L;
            var cancellationToken = CancellationToken.None;

            _mockRepository.Setup(x => x.GetByIdAsync(id, cancellationToken))
                .ReturnsAsync((TreasuryConcept?)null);

            // Act
            var result = await _mockRepository.Object.GetByIdAsync(id, cancellationToken);

            // Assert
            result.Should().BeNull();
            _mockRepository.Verify(x => x.GetByIdAsync(id, cancellationToken), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_WhenCancelled_ShouldThrowOperationCanceledException()
        {
            // Arrange
            var id = 1L;
            var cancellationToken = new CancellationToken(canceled: true);

            _mockRepository.Setup(x => x.GetByIdAsync(
                It.IsAny<long>(),
                It.Is<CancellationToken>(ct => ct.IsCancellationRequested)))
                .ThrowsAsync(new OperationCanceledException());

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(() =>
                _mockRepository.Object.GetByIdAsync(id, cancellationToken));
        }

        [Fact]
        public async Task GetByConceptAsync_WithExistingConcept_ShouldReturnTrue()
        {
            // Arrange
            var concept = "Test Concept";
            var cancellationToken = CancellationToken.None;

            _mockRepository.Setup(x => x.GetByConceptAsync(concept, cancellationToken))
                .ReturnsAsync(true);

            // Act
            var result = await _mockRepository.Object.GetByConceptAsync(concept, cancellationToken);

            // Assert
            result.Should().BeTrue();
            _mockRepository.Verify(x => x.GetByConceptAsync(concept, cancellationToken), Times.Once);
        }

        [Fact]
        public async Task GetByConceptAsync_WithNonExistingConcept_ShouldReturnFalse()
        {
            // Arrange
            var concept = "Non Existing Concept";
            var cancellationToken = CancellationToken.None;

            _mockRepository.Setup(x => x.GetByConceptAsync(concept, cancellationToken))
                .ReturnsAsync(false);

            // Act
            var result = await _mockRepository.Object.GetByConceptAsync(concept, cancellationToken);

            // Assert
            result.Should().BeFalse();
            _mockRepository.Verify(x => x.GetByConceptAsync(concept, cancellationToken), Times.Once);
        }

        [Fact]
        public async Task GetByConceptAsync_WithEmptyString_ShouldReturnFalse()
        {
            // Arrange
            var concept = string.Empty;
            var cancellationToken = CancellationToken.None;

            _mockRepository.Setup(x => x.GetByConceptAsync(concept, cancellationToken))
                .ReturnsAsync(false);

            // Act
            var result = await _mockRepository.Object.GetByConceptAsync(concept, cancellationToken);

            // Assert
            result.Should().BeFalse();
            _mockRepository.Verify(x => x.GetByConceptAsync(concept, cancellationToken), Times.Once);
        }

        [Fact]
        public async Task GetByConceptAsync_WithNullConcept_ShouldReturnFalse()
        {
            // Arrange
            string concept = null;
            var cancellationToken = CancellationToken.None;

            _mockRepository.Setup(x => x.GetByConceptAsync(concept, cancellationToken))
                .ReturnsAsync(false);

            // Act
            var result = await _mockRepository.Object.GetByConceptAsync(concept, cancellationToken);

            // Assert
            result.Should().BeFalse();
            _mockRepository.Verify(x => x.GetByConceptAsync(concept, cancellationToken), Times.Once);
        }

        [Fact]
        public async Task GetByConceptAsync_WithWhitespaceConcept_ShouldReturnFalse()
        {
            // Arrange
            var concept = "   ";
            var cancellationToken = CancellationToken.None;

            _mockRepository.Setup(x => x.GetByConceptAsync(concept, cancellationToken))
                .ReturnsAsync(false);

            // Act
            var result = await _mockRepository.Object.GetByConceptAsync(concept, cancellationToken);

            // Assert
            result.Should().BeFalse();
            _mockRepository.Verify(x => x.GetByConceptAsync(concept, cancellationToken), Times.Once);
        }

        [Fact]
        public async Task GetByConceptAsync_WhenCancelled_ShouldThrowOperationCanceledException()
        {
            // Arrange
            var concept = "Test Concept";
            var cancellationToken = new CancellationToken(canceled: true);

            _mockRepository.Setup(x => x.GetByConceptAsync(
                It.IsAny<string>(),
                It.Is<CancellationToken>(ct => ct.IsCancellationRequested)))
                .ThrowsAsync(new OperationCanceledException());

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(() =>
                _mockRepository.Object.GetByConceptAsync(concept, cancellationToken));
        }
    }
}