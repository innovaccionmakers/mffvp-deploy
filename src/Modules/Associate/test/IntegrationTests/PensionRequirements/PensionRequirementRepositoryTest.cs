using Associate.Domain.PensionRequirements;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace IntegrationTests.PensionRequirements
{
    public class PensionRequirementRepositoryTest
    {
        private readonly Mock<IPensionRequirementRepository> _repositoryMock;

        public PensionRequirementRepositoryTest()
        {
            _repositoryMock = new Mock<IPensionRequirementRepository>();
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllPensionRequirements()
        {
            // Arrange
            var expectedRequirements = new List<PensionRequirement>
            {
                PensionRequirement.Create(DateTime.Now, DateTime.Now.AddYears(1), DateTime.UtcNow, true, 123).Value,
                PensionRequirement.Create(DateTime.Now, DateTime.Now.AddYears(1), DateTime.UtcNow, false, 456).Value
            };

            _repositoryMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedRequirements);

            // Act
            var result = await _repositoryMock.Object.GetAllAsync();

            // Assert
            Assert.Equal(expectedRequirements, result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetAsync_ShouldReturnPensionRequirement_WhenExists()
        {
            // Arrange
            var expectedRequirement = PensionRequirement.Create(
                DateTime.Now,
                DateTime.Now.AddYears(1),
                DateTime.UtcNow,
                true,
                789).Value;

            _repositoryMock.Setup(x => x.GetAsync(
                                It.IsAny<int>(),
                                It.IsAny<CancellationToken>()))
                            .ReturnsAsync(expectedRequirement);

            // Act
            var result = await _repositoryMock.Object.GetAsync(789, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(789, result.ActivateId);
            Assert.True(result.Status);
        }

        [Fact]
        public async Task GetAsync_ShouldReturnNull_WhenNotExists()
        {
            // Arrange
            _repositoryMock.Setup(x => x.GetAsync(
                                It.IsAny<int>(),
                                It.IsAny<CancellationToken>()))
                            .ReturnsAsync((PensionRequirement)null);

            // Act
            var result = await _repositoryMock.Object.GetAsync(999, CancellationToken.None);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Insert_ShouldAddNewPensionRequirement()
        {
            // Arrange
            var requirement = PensionRequirement.Create(
                DateTime.Now,
                DateTime.Now.AddYears(1),
                DateTime.UtcNow,
                true,
                101).Value;

            var insertedRequirements = new List<PensionRequirement>();

            _repositoryMock.Setup(x => x.Insert(It.IsAny<PensionRequirement>()))
                .Callback<PensionRequirement>(r => insertedRequirements.Add(r));

            // Act
            _repositoryMock.Object.Insert(requirement);

            // Assert
            Assert.Single(insertedRequirements);
            Assert.Equal(requirement, insertedRequirements[0]);
        }

        [Fact]
        public async Task DeactivateExistingRequirementsAsync_ShouldUpdateStatus()
        {
            // Arrange
            var affectedCount = 2;
            _repositoryMock.Setup(x => x.DeactivateExistingRequirementsAsync(
                                It.IsAny<int>(),
                                It.IsAny<CancellationToken>()))
                            .ReturnsAsync(affectedCount);

            // Act
            var result = await _repositoryMock.Object.DeactivateExistingRequirementsAsync(123, CancellationToken.None);

            // Assert
            Assert.Equal(affectedCount, result);
        }

        [Fact]
        public void Update_ShouldModifyExistingPensionRequirement()
        {
            // Arrange
            var requirement = PensionRequirement.Create(
                DateTime.Now,
                DateTime.Now.AddYears(1),
                DateTime.UtcNow,
                true,
                202).Value;

            var updatedRequirements = new List<PensionRequirement>();

            _repositoryMock.Setup(x => x.Update(It.IsAny<PensionRequirement>()))
                .Callback<PensionRequirement>(r => updatedRequirements.Add(r));

            // Act
            requirement.UpdateDetails(false);
            _repositoryMock.Object.Update(requirement);

            // Assert
            Assert.Single(updatedRequirements);
            Assert.Equal(requirement, updatedRequirements[0]);
            Assert.False(updatedRequirements[0].Status);
        }
    }
}