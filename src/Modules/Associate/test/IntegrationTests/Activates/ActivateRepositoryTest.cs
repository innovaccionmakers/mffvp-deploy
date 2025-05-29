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
                Activate.Create("Type1", "123", false, false, DateTime.UtcNow).Value,
                Activate.Create("Type2", "456", true, true, DateTime.UtcNow).Value
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
            
            var existingActivate = Activate.Create("Type1", "123", true, true, DateTime.UtcNow).Value;
            _repositoryMock.Setup(x => x.GetByIdTypeAndNumber(
                                It.IsAny<string>(), 
                                It.IsAny<string>(), 
                                It.IsAny<CancellationToken>()))
                            .ReturnsAsync(existingActivate);

            // Act
            var result = _repositoryMock.Object.GetByIdTypeAndNumber("Unknown", "999");

            // Assert
            Assert.False(result is null);
        }

        [Fact]
        public void Insert_ShouldAddNewActivate()
        {
            // Arrange
            var activate = Activate.Create("Type1", "123", false, false, DateTime.UtcNow).Value;
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
    }
}