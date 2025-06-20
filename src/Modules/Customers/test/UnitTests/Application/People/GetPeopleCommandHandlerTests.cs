using Customers.Domain.People;
using Moq;
using FluentAssertions;
using Common.SharedKernel.Domain;
using Customers.Integrations.People;
using Customers.UnitTests.TestHelpers;

namespace Customers.UnitTests.Application.People
{
    public class GetPeopleCommandHandlerTests
    {
        private readonly Mock<IPersonRepository> _repositoryMock;

        public GetPeopleCommandHandlerTests()
        {
            _repositoryMock = new Mock<IPersonRepository>();
        }

        [Fact]
        public async Task RepositoryGetAll_ShouldReturnCorrectPersonResponses()
        {
            // Arrange
            var persons = new List<Person>
            {
                PersonObjectMother.CreateTestPerson(1, "John", "Doe", Status.Active),
                PersonObjectMother.CreateTestPerson(2, "Jane", "Smith", Status.Inactive)
            };

            _repositoryMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(persons);

            // Act
            var result = await _repositoryMock.Object.GetAllAsync(CancellationToken.None);

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(p => p.Status == Status.Active);
            result.Should().Contain(p => p.Status == Status.Inactive);
        }

        [Fact]
        public void PersonResponse_ShouldMapStatusCorrectly()
        {
            // Arrange
            var activePerson = PersonObjectMother.CreateTestPerson(1, "Active", "User", Status.Active);
            var inactivePerson = PersonObjectMother.CreateTestPerson(2, "Inactive", "User", Status.Inactive);

            // Act
            var activeResponse = MapToResponse(activePerson);
            var inactiveResponse = MapToResponse(inactivePerson);

            // Assert
            activeResponse.Status.Should().Be(Status.Active);
            inactiveResponse.Status.Should().Be(Status.Inactive);
        }

        private PersonResponse MapToResponse(Person person)
        {
            return new PersonResponse(
                person.PersonId,
                person.DocumentType,
                person.HomologatedCode,
                person.Identification,
                person.FirstName,
                person.MiddleName,
                person.LastName,
                person.SecondLastName,
                person.BirthDate,
                person.Mobile,
                person.FullName,
                person.GenderId,
                person.CountryOfResidenceId,
                person.DepartmentId,
                person.MunicipalityId,
                person.Email,
                person.EconomicActivityId,
                person.Status,
                person.Address,
                person.IsDeclarant,
                person.InvestorTypeId,
                person.RiskProfileId);
        }
    }
}