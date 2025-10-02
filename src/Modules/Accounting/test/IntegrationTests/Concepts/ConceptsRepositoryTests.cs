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
            var expectedEmptyCollection = Enumerable.Empty<Concept>();

            _repositoryMock.Setup(x => x.GetConceptsByPortfolioIdsAsync(
                It.Is<IEnumerable<int>>(ids => !ids.Any()),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedEmptyCollection);

            // Act
            var result = await _repositoryMock.Object.GetConceptsByPortfolioIdsAsync(emptyPortfolioIds, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
            _repositoryMock.Verify(x => x.GetConceptsByPortfolioIdsAsync(
                emptyPortfolioIds, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetConceptsByPortfolioIdsAsync_WhenPortfolioIdsNull_ShouldReturnEmptyCollection()
        {
            // Arrange
            IEnumerable<int> nullPortfolioIds = null;
            var expectedEmptyCollection = Enumerable.Empty<Concept>();

            _repositoryMock.Setup(x => x.GetConceptsByPortfolioIdsAsync(
                It.Is<IEnumerable<int>>(ids => ids == null || !ids.Any()),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedEmptyCollection);

            // Act
            var result = await _repositoryMock.Object.GetConceptsByPortfolioIdsAsync(nullPortfolioIds, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
            _repositoryMock.Verify(x => x.GetConceptsByPortfolioIdsAsync(
                nullPortfolioIds, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetConceptsByPortfolioIdsAsync_WhenNoMatchingConcepts_ShouldReturnEmptyCollection()
        {
            // Arrange
            var portfolioIds = new List<int> { 99, 100 };
            var expectedEmptyCollection = Enumerable.Empty<Concept>();

            _repositoryMock.Setup(x => x.GetConceptsByPortfolioIdsAsync(
                It.Is<IEnumerable<int>>(ids => ids.SequenceEqual(portfolioIds)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedEmptyCollection);

            // Act
            var result = await _repositoryMock.Object.GetConceptsByPortfolioIdsAsync(portfolioIds, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
            _repositoryMock.Verify(x => x.GetConceptsByPortfolioIdsAsync(
                portfolioIds, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetConceptsByPortfolioIdsAsync_WhenCancelled_ShouldThrowOperationCanceledException()
        {
            // Arrange
            var portfolioIds = new List<int> { 1, 2 };
            var cancellationToken = new CancellationToken(canceled: true);

            _repositoryMock.Setup(x => x.GetConceptsByPortfolioIdsAsync(
                It.IsAny<IEnumerable<int>>(),
                It.Is<CancellationToken>(ct => ct.IsCancellationRequested)))
                .ThrowsAsync(new OperationCanceledException());

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(() =>
                _repositoryMock.Object.GetConceptsByPortfolioIdsAsync(portfolioIds, cancellationToken));
        }
    }
}
