using Associate.Domain.Activates;
using Moq;

namespace UnitTests.Application.Activates
{
    public class UpdateActivateCommandHandlerTests
    {
        private readonly Mock<IActivateRepository> _repositoryMock;
        
        public UpdateActivateCommandHandlerTests()
        {
            _repositoryMock = new Mock<IActivateRepository>();
        }

        [Fact]
        public async Task UpdateActivate_ShouldWorkThroughRepository()
        {
            // Arrange
            var existingActivate = Activate.Create(new Guid(), "123", false, true, DateTime.UtcNow).Value;
            var updatedPensionerStatus = true;

            _repositoryMock.Setup(x => x.GetByIdTypeAndNumber(new Guid(), "123", It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingActivate);

            // Act
            var activate = await _repositoryMock.Object.GetByIdTypeAndNumber(new Guid(), "123");
            activate.UpdateDetails(updatedPensionerStatus);
            _repositoryMock.Object.Update(activate, CancellationToken.None);

            // Assert
            Assert.Equal(updatedPensionerStatus, activate.Pensioner);
            _repositoryMock.Verify(x => x.Update(
                It.Is<Activate>(a => a.Pensioner == updatedPensionerStatus),
                It.IsAny<CancellationToken>()), 
                Times.Once);
        }

        [Fact]
        public async Task UpdateActivate_ShouldFail_WhenNotFound()
        {
            // Arrange
            _repositoryMock.Setup(x => x.GetByIdTypeAndNumber(new Guid(), "999", It.IsAny<CancellationToken>()))
                .ReturnsAsync((Activate)null);

            // Act
            var activate = await _repositoryMock.Object.GetByIdTypeAndNumber(new Guid(), "999");

            // Assert
            Assert.Null(activate);
        }        
    }
}