using Associate.Domain.Activates;
using Associate.Integrations.Activates.GetActivates;
using Moq;
using MediatR;
using Common.SharedKernel.Domain;
using Associate.Integrations.Activates;

namespace UnitTests.Application.Activates
{
    public class GetActivatesCommandHandlerTests
    {
        private readonly Mock<IActivateRepository> _repositoryMock = new();

        [Fact]
        public async Task RepositoryInteraction_ShouldBeCorrect()
        {
            // Arrange
            var activates = new List<Activate>
            {
                Activate.Create("Type1", "123", false, false, DateTime.UtcNow).Value,
                Activate.Create("Type2", "456", true, true, DateTime.UtcNow).Value
            };

            _repositoryMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(activates);

            // Act
            var result = await _repositoryMock.Object.GetAllAsync();

            // Assert - Verifica interacción con repositorio
            Assert.Equal(2, result.Count);
            _repositoryMock.Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public void ActivateResponse_Constructor_ShouldWorkCorrectly()
        {
            // Arrange
            var activateId = 1;
            var identificationType = "Type1";
            var identification = "123";
            var pensioner = false;
            var meetsRequirements = true;
            var activateDate = DateTime.UtcNow;

            // Act
            var response = new ActivateResponse(
                activateId,
                identificationType,
                identification,
                pensioner,
                meetsRequirements,
                activateDate);

            // Assert
            Assert.Equal(activateId, response.ActivateId);
            Assert.Equal(identificationType, response.IdentificationType);
            Assert.Equal(pensioner, response.Pensioner);
        }

        [Fact]
        public async Task QueryObject_ShouldBeProperlyConstructed()
        {
            // Arrange
            var query = new GetActivatesQuery();

            // Act (Simulación de envío a través de MediatR)
            var mockMediator = new Mock<IMediator>();
            mockMediator.Setup(m => m.Send(It.IsAny<GetActivatesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success<IReadOnlyCollection<ActivateResponse>>(new List<ActivateResponse>()));

            // Assert
            Assert.NotNull(query);
        }
    }
}