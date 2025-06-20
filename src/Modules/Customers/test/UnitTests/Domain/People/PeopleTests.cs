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

        [Theory]
        [InlineData(null, "123456789", "John", "Doe")] // Missing homologated code
        [InlineData("HOM001", null, "John", "Doe")] // Missing identification
        [InlineData("HOM001", "123456789", null, "Doe")] // Missing first name
        [InlineData("HOM001", "123456789", "John", null)] // Missing last name
        public void CreatePerson_WithInvalidParameters_ShouldFail(
            string homologatedCode,
            string identification,
            string firstName,
            string lastName)
        {
            // Arrange
            var uuid = Guid.NewGuid();

            // Act
            var result = Person.Create(
                homologatedCode,
                uuid,
                identification,
                firstName,
                "Middle",
                lastName,
                "Second",
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
            result.IsFailure.Should().BeTrue();
        }
    }
}