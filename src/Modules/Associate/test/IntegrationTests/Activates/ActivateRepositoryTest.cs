using Associate.Domain.Activates;
using Moq;

namespace IntegrationTests.Activates
{
    public class ActivateRepositoryTest
    {
        private readonly Mock<IActivateRepository> _repositoryMock;

        public ActivateRepositoryTest()
        {
            _repositoryMock = new Mock<IActivateRepository>();
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllActivates()
        {
            // Arrange
            var expectedActivates = new List<Activate>
            {
                Activate.Create(new Guid(), "123", false, false, DateTime.UtcNow).Value,
                Activate.Create(new Guid(), "456", true, true, DateTime.UtcNow).Value
            };

            _repositoryMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedActivates);

            // Act
            var result = await _repositoryMock.Object.GetAllAsync();

            // Assert
            Assert.Equal(expectedActivates, result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void GetByIdTypeAndNumber_ShouldReturnExpectedResult()
        {
            // Arrange

            var existingActivate = Activate.Create(new Guid(), "123", true, true, DateTime.UtcNow).Value;
            _repositoryMock.Setup(x => x.GetByIdTypeAndNumber(
                                It.IsAny<Guid>(),
                                It.IsAny<string>(),
                                It.IsAny<CancellationToken>()))
                            .ReturnsAsync(existingActivate);

            // Act
            var result = _repositoryMock.Object.GetByIdTypeAndNumber(new Guid(), "999");

            // Assert
            Assert.False(result is null);
        }

        [Fact]
        public void Insert_ShouldAddNewActivate()
        {
            // Arrange
            var activate = Activate.Create(new Guid(), "123", false, false, DateTime.UtcNow).Value;
            var insertedActivates = new List<Activate>();

            _repositoryMock.Setup(x => x.Insert(
                                It.IsAny<Activate>(),
                                It.IsAny<CancellationToken>()))
                            .Callback<Activate, CancellationToken>((a, _) => insertedActivates.Add(a));

            // Act
            _repositoryMock.Object.Insert(activate);

            // Assert
            Assert.Single(insertedActivates);
            Assert.Equal(activate, insertedActivates[0]);
        }

        [Fact]
        public async Task Update_ShouldModifyExistingActivate()
        {
            // Arrange
            var existingActivate = Activate.Create(new Guid(), "123", false, true, DateTime.UtcNow).Value;
            var updatedActivate = Activate.Create(new Guid(), "123", true, true, DateTime.UtcNow).Value;
            var updatedActivates = new List<Activate>();

            _repositoryMock.Setup(x => x.Update(It.IsAny<Activate>(), It.IsAny<CancellationToken>()))
                .Callback<Activate, CancellationToken>((a, _) => updatedActivates.Add(a));

            // Act
            _repositoryMock.Object.Update(updatedActivate, CancellationToken.None);

            // Assert
            Assert.Single(updatedActivates);
            Assert.Equal(updatedActivate, updatedActivates[0]);
        }

        [Fact]
        public async Task GetByIdAsync_WhenActivateExists_ShouldReturnActivate()
        {
            // Arrange
            var activateId = 1L;
            var expectedActivate = Activate.Create(new Guid(), "123", true, true, DateTime.UtcNow).Value;

            _repositoryMock.Setup(x => x.GetByIdAsync(
                                It.Is<long>(id => id == activateId),
                                It.IsAny<CancellationToken>()))
                            .ReturnsAsync(expectedActivate);

            // Act
            var result = await _repositoryMock.Object.GetByIdAsync(activateId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedActivate, result);
        }

        [Fact]
        public async Task GetByIdAsync_WhenActivateDoesNotExist_ShouldReturnNull()
        {
            // Arrange
            var activateId = 999L;

            _repositoryMock.Setup(x => x.GetByIdAsync(
                                It.Is<long>(id => id == activateId),
                                It.IsAny<CancellationToken>()))
                            .ReturnsAsync((Activate?)null);

            // Act
            var result = await _repositoryMock.Object.GetByIdAsync(activateId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetActivateByIdsAsync_WhenIdsProvided_ShouldReturnMatchingActivates()
        {
            // Arrange
            var activateIds = new List<int> { 1, 2, 3 };
            var expectedActivates = new List<Activate?>
            {
                Activate.Create(new Guid(), "001", false, false, DateTime.UtcNow).Value,
                Activate.Create(new Guid(), "002", true, false, DateTime.UtcNow).Value,
                Activate.Create(new Guid(), "003", false, true, DateTime.UtcNow).Value
            };

            _repositoryMock.Setup(x => x.GetActivateByIdsAsync(
                                It.Is<IEnumerable<int>>(ids => ids.SequenceEqual(activateIds)),
                                It.IsAny<CancellationToken>()))
                            .ReturnsAsync(expectedActivates);

            // Act
            var result = await _repositoryMock.Object.GetActivateByIdsAsync(activateIds);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count());
            Assert.Equal(expectedActivates, result);
        }

        [Fact]
        public async Task GetActivateByIdsAsync_WhenIdsAreEmpty_ShouldReturnEmptyCollection()
        {
            // Arrange
            var emptyIds = Enumerable.Empty<int>();
            var expectedEmptyCollection = Enumerable.Empty<Activate?>();

            _repositoryMock.Setup(x => x.GetActivateByIdsAsync(
                                It.Is<IEnumerable<int>>(ids => !ids.Any()),
                                It.IsAny<CancellationToken>()))
                            .ReturnsAsync(expectedEmptyCollection);

            // Act
            var result = await _repositoryMock.Object.GetActivateByIdsAsync(emptyIds);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetActivateByIdsAsync_WhenNoMatchingActivates_ShouldReturnEmptyCollection()
        {
            // Arrange
            var activateIds = new List<int> { 99, 100 };
            var expectedEmptyCollection = Enumerable.Empty<Activate?>();

            _repositoryMock.Setup(x => x.GetActivateByIdsAsync(
                                It.Is<IEnumerable<int>>(ids => ids.SequenceEqual(activateIds)),
                                It.IsAny<CancellationToken>()))
                            .ReturnsAsync(expectedEmptyCollection);

            // Act
            var result = await _repositoryMock.Object.GetActivateByIdsAsync(activateIds);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetActivateByIdsAsync_WhenIdsAreNull_ShouldReturnEmptyCollection()
        {
            // Arrange
            IEnumerable<int> nullIds = null;
            var expectedEmptyCollection = Enumerable.Empty<Activate?>();

            _repositoryMock.Setup(x => x.GetActivateByIdsAsync(
                                It.Is<IEnumerable<int>>(ids => ids == null || !ids.Any()),
                                It.IsAny<CancellationToken>()))
                            .ReturnsAsync(expectedEmptyCollection);

            // Act
            var result = await _repositoryMock.Object.GetActivateByIdsAsync(nullIds);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }
    }
}