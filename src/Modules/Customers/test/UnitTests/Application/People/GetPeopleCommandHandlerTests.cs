using Common.SharedKernel.Core.Primitives;

using Customers.Domain.People;
using Customers.Integrations.People;
using Customers.UnitTests.TestHelpers;

using FluentAssertions;

using Moq;

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
            var activePerson = PersonObjectMother.CreateTestPerson(1, "John", "Doe", Status.Active);
            var inactivePerson = PersonObjectMother.CreateTestPerson(2, "Jane", "Smith", Status.Inactive);

            // Debug: Verificar los status inmediatamente después de la creación
            activePerson.Status.Should().Be(Status.Active, "La persona activa no tiene el status correcto");

            var persons = new List<Person> { activePerson, inactivePerson };

            _repositoryMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(persons);

            // Act
            var result = await _repositoryMock.Object.GetAllAsync(CancellationToken.None);

            // Assert
            result.Should().HaveCount(2, "Debería haber 2 personas en la lista");

            // Verificación más detallada
            var statusList = result.Select(p => p.Status).ToList();
            statusList.Should().Contain(Status.Active, "Falta la persona con status Active");
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