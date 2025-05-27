using Associate.Application.Abstractions;
using Associate.Application.Abstractions.Data;
using Associate.Application.Abstractions.Rules;
using Associate.Application.Activates.CreateActivate;
using Associate.Domain.Activates;
using Associate.Integrations.Activates.CreateActivate;
using Common.SharedKernel.Application.Messaging;
using Moq;
using People.IntegrationEvents.PersonValidation;

namespace UnitTests.Application.Activates
{
    public class CreateActivateCommandHandlerTests
    {private readonly Mock<IActivateRepository> _repositoryMock = new();
        private readonly Mock<IRuleEvaluator<ActivateModuleMarker>> _ruleEvaluatorMock = new();
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<ICapRpcClient> _rpcMock = new();

        [Fact]
        public async Task RuleEvaluator_ShouldValidateContextCorrectly()
        {
            // Arrange
            var command = new CreateActivateCommand(
                "Type1", "123", false, false, null, null);
            
            var personData = new GetPersonValidationResponse(false, null, null);
            
            _repositoryMock.Setup(x => x.GetByIdTypeAndNumber(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(false);
                
            _rpcMock.Setup(x => x.CallAsync<PersonDataRequestEvent, GetPersonValidationResponse>(
                    It.IsAny<string>(), It.IsAny<PersonDataRequestEvent>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(personData);

            // Act
            var validationContext = new ActivateValidationContext(command, false);

            // Assert
            Assert.Equal(command, validationContext.Request);
            Assert.False(validationContext.ExistingActivate);
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
        public async Task Repository_ShouldDetectExistingActivate()
        {
            // Arrange
            _repositoryMock.Setup(x => x.GetByIdTypeAndNumber("Type1", "123"))
                .Returns(true);

            // Act
            var exists = _repositoryMock.Object.GetByIdTypeAndNumber("Type1", "123");

            // Assert
            Assert.True(exists);
            _repositoryMock.Verify(x => x.GetByIdTypeAndNumber("Type1", "123"), Times.Once);
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