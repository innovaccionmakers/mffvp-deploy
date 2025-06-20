using Customers.Domain.People;
using Common.SharedKernel.Domain;
using FluentAssertions;

namespace Customers.UnitTests.Domain.People
{
    public class PeopleTests
    {
        [Fact]
        public void CreatePerson_WithValidParameters_ShouldReturnSuccess()
        {
            // Arrange
            var uuid = Guid.NewGuid();
            var birthDate = new DateTime(1990, 1, 1);

            // Act
            var result = Person.Create(
                "HOM001",
                uuid,
                "123456789",
                "John",
                "Middle",
                "Doe",
                "Second",
                birthDate,
                "1234567890",
                1, // GenderId
                1, // CountryId
                1, // DepartmentId
                1, // MunicipalityId
                "john.doe@example.com",
                1, // EconomicActivityId
                Status.Active,
                "123 Main St",
                false,
                1, // InvestorTypeId
                1  // RiskProfileId
            );

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.FirstName.Should().Be("John");
            result.Value.LastName.Should().Be("Doe");
            result.Value.FullName.Should().Be("John Middle Doe Second");
            result.Value.BirthDate.Should().Be(birthDate);
            result.Value.Status.Should().Be(Status.Active);
        }

        [Fact]
        public void CreatePerson_WithInvalidParameters_ShouldFail()
        {
            // Arrange
            var uuid = Guid.NewGuid();

            // Act
            var result = Person.Create(
                null, // homologatedCode (opcional)
                uuid,
                null, // identification (requerido)
                null, // firstName (requerido)
                "Middle", // middleName (opcional)
                null, // lastName (requerido)
                "Second", // secondLastName (opcional)
                DateTime.UtcNow,
                "1234567890",
                1, // GenderId
                1, // CountryId
                1, // DepartmentId
                1, // MunicipalityId
                "john.doe@example.com",
                1, // EconomicActivityId
                Status.Active,
                "123 Main St",
                false,
                1, // InvestorTypeId
                1  // RiskProfileId
            );

            // Assert
            result.IsFailure.Should().BeFalse("Debió fallar por parámetros requeridos nulos");
        }
    }
}