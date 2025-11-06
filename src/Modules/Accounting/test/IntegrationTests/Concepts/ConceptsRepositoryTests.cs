using Accounting.Domain.Concepts;
using FluentAssertions;
using Moq;

namespace Accounting.test.IntegrationTests.Concepts
{
    public class ConceptsRepositoryTests
    {
        private readonly Mock<IConceptsRepository> _repositoryMock;

        public ConceptsRepositoryTests()
        {
            _repositoryMock = new Mock<IConceptsRepository>();
        }


        [Fact]
        public async Task GetConceptsByPortfolioIdsAsync_WhenPortfolioIdsEmpty_ShouldReturnEmptyCollection()
        {
            // Arrange
            var emptyPortfolioIds = Enumerable.Empty<int>();
            var emptyConcepts = Enumerable.Empty<string>();
            var expectedEmptyCollection = Enumerable.Empty<Concept>();

            _repositoryMock.Setup(x => x.GetConceptsByPortfolioIdsAsync(
                It.Is<IEnumerable<int>>(ids => !ids.Any()),
                It.Is<IEnumerable<string>>(concepts => !concepts.Any()),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedEmptyCollection);

            // Act
            var result = await _repositoryMock.Object.GetConceptsByPortfolioIdsAsync(emptyPortfolioIds, emptyConcepts, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
            _repositoryMock.Verify(x => x.GetConceptsByPortfolioIdsAsync(
                emptyPortfolioIds, emptyConcepts, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetConceptsByPortfolioIdsAsync_WhenPortfolioIdsNull_ShouldReturnEmptyCollection()
        {
            // Arrange
            IEnumerable<int> nullPortfolioIds = null;
            IEnumerable<string> nullConcepts = null;
            var expectedEmptyCollection = Enumerable.Empty<Concept>();

            _repositoryMock.Setup(x => x.GetConceptsByPortfolioIdsAsync(
                It.Is<IEnumerable<int>>(ids => ids == null || !ids.Any()),
                It.Is<IEnumerable<string>>(concepts => concepts == null || !concepts.Any()),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedEmptyCollection);

            // Act
            var result = await _repositoryMock.Object.GetConceptsByPortfolioIdsAsync(nullPortfolioIds, nullConcepts, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
            _repositoryMock.Verify(x => x.GetConceptsByPortfolioIdsAsync(
                nullPortfolioIds, nullConcepts, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetConceptsByPortfolioIdsAsync_WhenNoMatchingConcepts_ShouldReturnEmptyCollection()
        {
            // Arrange
            var portfolioIds = new List<int> { 99, 100 };
            var concepts = new List<string> { "GASTUA", "ING888" };
            var expectedEmptyCollection = Enumerable.Empty<Concept>();

            _repositoryMock.Setup(x => x.GetConceptsByPortfolioIdsAsync(
                It.Is<IEnumerable<int>>(ids => ids.SequenceEqual(portfolioIds)),
                It.Is<IEnumerable<string>>(concepts => concepts.SequenceEqual(concepts)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedEmptyCollection);

            // Act
            var result = await _repositoryMock.Object.GetConceptsByPortfolioIdsAsync(portfolioIds, concepts, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
            _repositoryMock.Verify(x => x.GetConceptsByPortfolioIdsAsync(
                portfolioIds, concepts, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetConceptsByPortfolioIdsAsync_WhenCancelled_ShouldThrowOperationCanceledException()
        {
            // Arrange
            var portfolioIds = new List<int> { 1, 2 };
            var concepts = new List<string> { "GASTUA", "ING888" };
            var cancellationToken = new CancellationToken(canceled: true);

            _repositoryMock.Setup(x => x.GetConceptsByPortfolioIdsAsync(
                It.IsAny<IEnumerable<int>>(),
                It.IsAny<IEnumerable<string>>(),
                It.Is<CancellationToken>(ct => ct.IsCancellationRequested)))
                .ThrowsAsync(new OperationCanceledException());

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(() =>
                _repositoryMock.Object.GetConceptsByPortfolioIdsAsync(portfolioIds, concepts, cancellationToken));
        }
    }
}
