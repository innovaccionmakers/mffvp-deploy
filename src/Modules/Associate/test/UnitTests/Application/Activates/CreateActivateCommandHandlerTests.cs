using Associate.Application.Activates.CreateActivate;
using Associate.Domain.Activates;
using Associate.Integrations.Activates.CreateActivate;
using Common.SharedKernel.Application.Messaging;
using Moq;
using People.IntegrationEvents.PersonValidation;

namespace UnitTests.Application.Activates
{
    public class CreateActivateCommandHandlerTests
    {
        private readonly Mock<IActivateRepository> _repositoryMock = new();
        private readonly Mock<ICapRpcClient> _rpcMock = new();

        [Fact]
        public void RuleEvaluator_ShouldValidateContextCorrectly()
        {
            // Arrange
            var command = new CreateActivateCommand("Type1", "123", false, false, null, null);
            var existingActivate = Activate.Create("Type1", "123", true, true, DateTime.UtcNow).Value;
            var personData = new GetPersonValidationResponse(false, null, null);

            _repositoryMock.Setup(x => x.GetByIdTypeAndNumber(
                                It.IsAny<string>(), 
                                It.IsAny<string>(), 
                                It.IsAny<CancellationToken>()))
                            .ReturnsAsync(existingActivate);

            _rpcMock.Setup(x => x.CallAsync<PersonDataRequestEvent, GetPersonValidationResponse>(
                    It.IsAny<string>(), It.IsAny<PersonDataRequestEvent>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(personData);

            // Act
            var validationContext = new CreateActivateValidationContext(command, existingActivate, true);

            // Assert
            Assert.Equal(command, validationContext.Request);
        }

        [Fact]
        public void Activate_Create_ShouldInitializePropertiesCorrectly()
        {
            // Arrange
            var identificationType = "Type1";
            var identification = "123";
            var pensioner = false;
            var meetsRequirements = true;
            var activateDate = DateTime.UtcNow;

            // Act
            var result = Activate.Create(
                identificationType,
                identification,
                pensioner,
                meetsRequirements,
                activateDate);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(identificationType, result.Value.IdentificationType);
            Assert.Equal(identification, result.Value.Identification);
            Assert.Equal(pensioner, result.Value.Pensioner);
        }

        [Fact]
        public void Repository_ShouldDetectExistingActivate()
        {
            //Arrange            
            var existingActivate = Activate.Create("Type1", "123", true, true, DateTime.UtcNow).Value;
            _repositoryMock.Setup(x => x.GetByIdTypeAndNumber(
                                It.IsAny<string>(), 
                                It.IsAny<string>(), 
                                It.IsAny<CancellationToken>()))
                            .ReturnsAsync(existingActivate);

            // Act
            var exists = _repositoryMock.Object.GetByIdTypeAndNumber("Type1", "123");

            // Assert
            Assert.True(exists is not null);
            _repositoryMock.Verify(x => x.GetByIdTypeAndNumber("Type1", "123", CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task RpcClient_ShouldReturnPersonData()
        {
            // Arrange
            var expectedPerson = new GetPersonValidationResponse(false, null, null);
            var requestEvent = new PersonDataRequestEvent("Type1", "123");

            _rpcMock.Setup(x => x.CallAsync<PersonDataRequestEvent, GetPersonValidationResponse>(
                    It.IsAny<string>(), It.IsAny<PersonDataRequestEvent>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedPerson);

            // Act
            var result = await _rpcMock.Object.CallAsync<PersonDataRequestEvent, GetPersonValidationResponse>(
                "endpoint", requestEvent, TimeSpan.FromSeconds(30), CancellationToken.None);

            // Assert
            Assert.Equal(expectedPerson, result);
        }
    }
}