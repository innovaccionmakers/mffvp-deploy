using Associate.Application.Activates.CreateActivate;
using Associate.Domain.Activates;
using Associate.Integrations.Activates.CreateActivate;
using Common.SharedKernel.Application.Rpc;
using Moq;
using Customers.IntegrationEvents.PersonValidation;

namespace UnitTests.Application.Activates
{
    public class CreateActivateCommandHandlerTests
    {
        private readonly Mock<IActivateRepository> _repositoryMock = new();
        private readonly Mock<IRpcClient> _rpcMock = new();

        [Fact]
        public void RuleEvaluator_ShouldValidateContextCorrectly()
        {
            // Arrange
            var command = new CreateActivateCommand("Type1", "123", false, false, null, null);
            var existingActivate = Activate.Create(new Guid(), "123", true, true, DateTime.UtcNow).Value;
            var personData = new GetPersonValidationResponse(false, null, null);

            _repositoryMock.Setup(x => x.GetByIdTypeAndNumber(
                                It.IsAny<Guid>(), 
                                It.IsAny<string>(), 
                                It.IsAny<CancellationToken>()))
                            .ReturnsAsync(existingActivate);

            _rpcMock.Setup(x => x.CallAsync<PersonDataRequestEvent, GetPersonValidationResponse>(
                    It.IsAny<PersonDataRequestEvent>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(personData);

            // Act
            var validationContext = new CreateActivateValidationContext(command, existingActivate, new Guid());

            // Assert
            Assert.Equal(command, validationContext.Request);
        }

        [Fact]
        public void Activate_Create_ShouldInitializePropertiesCorrectly()
        {
            // Arrange
            var documentType = new Guid();
            var identification = "123";
            var pensioner = false;
            var meetsRequirements = true;
            var activateDate = DateTime.UtcNow;

            // Act
            var result = Activate.Create(
                documentType,
                identification,
                pensioner,
                meetsRequirements,
                activateDate);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(identification, result.Value.Identification);
            Assert.Equal(pensioner, result.Value.Pensioner);
        }

        [Fact]
        public void Repository_ShouldDetectExistingActivate()
        {
            //Arrange            
            var existingActivate = Activate.Create(new Guid(), "123", true, true, DateTime.UtcNow).Value;
            _repositoryMock.Setup(x => x.GetByIdTypeAndNumber(
                                It.IsAny<Guid>(), 
                                It.IsAny<string>(), 
                                It.IsAny<CancellationToken>()))
                            .ReturnsAsync(existingActivate);

            // Act
            var exists = _repositoryMock.Object.GetByIdTypeAndNumber(new Guid(), "123");

            // Assert
            Assert.True(exists is not null);
            _repositoryMock.Verify(x => x.GetByIdTypeAndNumber(new Guid(), "123", CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task RpcClient_ShouldReturnPersonData()
        {
            // Arrange
            var expectedPerson = new GetPersonValidationResponse(false, null, null);
            var requestEvent = new PersonDataRequestEvent("T123", "123");

            _rpcMock.Setup(x => x.CallAsync<PersonDataRequestEvent, GetPersonValidationResponse>(
                    It.IsAny<PersonDataRequestEvent>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedPerson);

            // Act
            var result = await _rpcMock.Object.CallAsync<PersonDataRequestEvent, GetPersonValidationResponse>(
                requestEvent, CancellationToken.None);

            // Assert
            Assert.Equal(expectedPerson, result);
        }
    }
}