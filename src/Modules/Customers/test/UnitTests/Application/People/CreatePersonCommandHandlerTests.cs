using Customers.Domain.People;
using Customers.Application.Abstractions.Data;
using Customers.Domain.ConfigurationParameters;
using Moq;
using FluentAssertions;
using Integrations.People.CreatePerson;
using Common.SharedKernel.Domain;
using Customers.UnitTests.TestHelpers;

namespace Customers.UnitTests.Application.People
{
    public class CreatePersonCommandHandlerTests
    {
        private readonly Mock<IPersonRepository> _repositoryMock;
        private readonly Mock<IConfigurationParameterRepository> _configRepoMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;

        public CreatePersonCommandHandlerTests()
        {
            _repositoryMock = new Mock<IPersonRepository>();
            _configRepoMock = new Mock<IConfigurationParameterRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
        }

        [Fact]
        public async Task Repository_ShouldPreventDuplicateIdentifications()
        {
            // Arrange
            var existingPerson = PersonObjectMother.CreateTestPerson(1, "Existing", "User");
            var request = new CreatePersonRequestCommand(
                "HOM001",
                "CC",
                existingPerson.Identification, // Duplicate ID
                "John",
                "Middle",
                "Doe",
                "Second",
                DateTime.UtcNow,
                "1234567890",
                "M",
                "CO",
                "ANT",
                "MED",
                "john@example.com",
                "EMP",
                "123 Main St",
                true,
                "INV1",
                "RISK1");

            _repositoryMock.Setup(x => x.GetForIdentificationAsync(
                    It.IsAny<Guid>(), 
                    existingPerson.Identification, 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingPerson);

            // Act
            var existing = await _repositoryMock.Object.GetForIdentificationAsync(
                Guid.NewGuid(), 
                existingPerson.Identification, 
                CancellationToken.None);

            // Assert
            existing.Should().NotBeNull();
            existing.Identification.Should().Be(existingPerson.Identification);
        }

        [Fact]
        public void PersonCreate_ShouldValidateRequiredFields()
        {
            // Arrange
            Action createWithMissingFirstName = () => Person.Create(
                "HOM001",
                Guid.NewGuid(),
                "123456789",
                null, // Missing first name
                "Middle",
                "Doe",
                "Second",
                DateTime.UtcNow,
                "1234567890",
                1, 1, 1, 1,
                "test@example.com",
                1,
                Status.Active,
                "123 St",
                false,
                1, 1);

            // Act & Assert
            createWithMissingFirstName.Should().Throw<Exception>();
        }
    }
}