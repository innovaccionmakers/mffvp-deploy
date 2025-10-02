using Customers.Domain.People;
using Customers.Application.Abstractions.Data;
using Moq;
using FluentAssertions;
using Common.SharedKernel.Domain;
using Common.SharedKernel.Core.Primitives;

namespace Customers.IntegrationTests.People
{
    public class PeopleRepositoryTests
    {
        private readonly Mock<IPersonRepository> _repositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;

        public PeopleRepositoryTests()
        {
            _repositoryMock = new Mock<IPersonRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllPersons()
        {
            // Arrange
            var expectedPersons = new List<Person>
            {
                CreateTestPerson(1),
                CreateTestPerson(2)
            };

            _repositoryMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedPersons);

            // Act
            var result = await _repositoryMock.Object.GetAllAsync(CancellationToken.None);

            // Assert
            result.Should().BeEquivalentTo(expectedPersons);
            _repositoryMock.Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task InsertAsync_ShouldAddPersonToRepository()
        {
            // Arrange
            var person = CreateTestPerson(1);

            _repositoryMock.Setup(x => x.Insert(person));
            _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1); 

            // Act
            _repositoryMock.Object.Insert(person);
            var result = await _unitOfWorkMock.Object.SaveChangesAsync(CancellationToken.None);

            // Assert
            _repositoryMock.Verify(x => x.Insert(person), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetForIdentificationAsync_ShouldReturnPerson_WhenExists()
        {
            // Arrange
            var expectedPerson = CreateTestPerson(1);
            var documentType = Guid.NewGuid();
            var identification = expectedPerson.Identification;

            _repositoryMock.Setup(x => x.GetForIdentificationAsync(
                documentType,
                identification,
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedPerson);

            // Act
            var result = await _repositoryMock.Object.GetForIdentificationAsync(
                documentType,
                identification,
                CancellationToken.None);

            // Assert
            result.Should().BeEquivalentTo(expectedPerson);
        }
        [Fact]
        public async Task GetAsync_WhenPersonExists_ShouldReturnPerson()
        {
            // Arrange
            var personId = 1L;
            var expectedPerson = CreateTestPerson(personId);

            _repositoryMock.Setup(x => x.GetAsync(
                It.Is<long>(id => id == personId),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedPerson);

            // Act
            var result = await _repositoryMock.Object.GetAsync(personId, CancellationToken.None);

            // Assert
            result.Should().BeEquivalentTo(expectedPerson);
            _repositoryMock.Verify(x => x.GetAsync(personId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetAsync_WhenPersonDoesNotExist_ShouldReturnNull()
        {
            // Arrange
            var personId = 999L;

            _repositoryMock.Setup(x => x.GetAsync(
                It.Is<long>(id => id == personId),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync((Person?)null);

            // Act
            var result = await _repositoryMock.Object.GetAsync(personId, CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void Update_ShouldModifyExistingPerson()
        {
            // Arrange
            var person = CreateTestPerson(1);
            var updatedPersons = new List<Person>();

            _repositoryMock.Setup(x => x.Update(It.IsAny<Person>()))
                .Callback<Person>(p => updatedPersons.Add(p));

            // Act
            _repositoryMock.Object.Update(person);

            // Assert
            updatedPersons.Should().ContainSingle();
            updatedPersons[0].Should().BeEquivalentTo(person);
            _repositoryMock.Verify(x => x.Update(person), Times.Once);
        }

        [Fact]
        public void Delete_ShouldRemovePerson()
        {
            // Arrange
            var person = CreateTestPerson(1);
            var deletedPersons = new List<Person>();

            _repositoryMock.Setup(x => x.Delete(It.IsAny<Person>()))
                .Callback<Person>(p => deletedPersons.Add(p));

            // Act
            _repositoryMock.Object.Delete(person);

            // Assert
            deletedPersons.Should().ContainSingle();
            deletedPersons[0].Should().BeEquivalentTo(person);
            _repositoryMock.Verify(x => x.Delete(person), Times.Once);
        }

        [Fact]
        public async Task GetByIdentificationAsync_WhenPersonExists_ShouldReturnPerson()
        {
            // Arrange
            var identification = "123456789";
            var documentTypeCode = "CC";
            var expectedPerson = CreateTestPerson(1);

            _repositoryMock.Setup(x => x.GetByIdentificationAsync(
                It.Is<string>(id => id == identification),
                It.Is<string>(code => code == documentTypeCode),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedPerson);

            // Act
            var result = await _repositoryMock.Object.GetByIdentificationAsync(
                identification, documentTypeCode, CancellationToken.None);

            // Assert
            result.Should().BeEquivalentTo(expectedPerson);
        }

        [Fact]
        public async Task GetByIdentificationAsync_WhenDocumentTypeCodeIsEmpty_ShouldReturnNull()
        {
            // Arrange
            var identification = "123456789";
            var emptyDocumentTypeCode = "";

            _repositoryMock.Setup(x => x.GetByIdentificationAsync(
                It.IsAny<string>(),
                It.Is<string>(code => string.IsNullOrWhiteSpace(code)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync((Person?)null);

            // Act
            var result = await _repositoryMock.Object.GetByIdentificationAsync(
                identification, emptyDocumentTypeCode, CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetExistingHomologatedCode_WhenCodeExists_ShouldReturnTrue()
        {
            // Arrange
            var homologatedCode = "HOM123";

            _repositoryMock.Setup(x => x.GetExistingHomologatedCode(
                It.Is<string>(code => code == homologatedCode),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _repositoryMock.Object.GetExistingHomologatedCode(
                homologatedCode, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task GetExistingHomologatedCode_WhenCodeDoesNotExist_ShouldReturnFalse()
        {
            // Arrange
            var homologatedCode = "NONEXISTENT";

            _repositoryMock.Setup(x => x.GetExistingHomologatedCode(
                It.Is<string>(code => code == homologatedCode),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _repositoryMock.Object.GetExistingHomologatedCode(
                homologatedCode, CancellationToken.None);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetExistingHomologatedCode_WhenCodeIsEmpty_ShouldReturnNull()
        {
            // Arrange
            var emptyCode = "";

            _repositoryMock.Setup(x => x.GetExistingHomologatedCode(
                It.Is<string>(code => string.IsNullOrWhiteSpace(code)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync((bool?)null);

            // Act
            var result = await _repositoryMock.Object.GetExistingHomologatedCode(
                emptyCode, CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetActivePersonsByFilterAsync_WithIdentificationType_ShouldReturnFilteredPersons()
        {
            // Arrange
            var identificationType = Guid.NewGuid().ToString();
            var expectedPersonInfos = new List<PersonInformation>
            {
                new PersonInformation(1, Guid.NewGuid(), "CC", "123456789", "John Doe", Status.Active),
                new PersonInformation(2, Guid.NewGuid(), "CE", "987654321", "Jane Smith", Status.Active)
            };

            _repositoryMock.Setup(x => x.GetActivePersonsByFilterAsync(
                It.Is<string>(type => type == identificationType),
                It.IsAny<SearchByType?>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedPersonInfos);

            // Act
            var result = await _repositoryMock.Object.GetActivePersonsByFilterAsync(
                identificationType, null, null, CancellationToken.None);

            // Assert
            result.Should().BeEquivalentTo(expectedPersonInfos);
        }

        [Fact]
        public async Task GetPersonsByDocumentsAsync_WhenDocumentsProvided_ShouldReturnMatchingPersons()
        {
            // Arrange
            var documents = new List<PersonDocumentKey>
            {
                new PersonDocumentKey(Guid.NewGuid(), "123456789"),
                new PersonDocumentKey(Guid.NewGuid(), "987654321")
            };
            var expectedPersons = new List<Person>
            {
                CreateTestPerson(1),
                CreateTestPerson(2)
            };

            _repositoryMock.Setup(x => x.GetPersonsByDocumentsAsync(
                It.Is<IReadOnlyCollection<PersonDocumentKey>>(docs => docs.SequenceEqual(documents)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedPersons);

            // Act
            var result = await _repositoryMock.Object.GetPersonsByDocumentsAsync(
                documents, CancellationToken.None);

            // Assert
            result.Should().BeEquivalentTo(expectedPersons);
        }

        [Fact]
        public async Task GetPersonsByDocumentsAsync_WhenDocumentsEmpty_ShouldReturnEmptyCollection()
        {
            // Arrange
            var emptyDocuments = new List<PersonDocumentKey>();
            var expectedEmptyCollection = new List<Person>();

            _repositoryMock.Setup(x => x.GetPersonsByDocumentsAsync(
                It.Is<IReadOnlyCollection<PersonDocumentKey>>(docs => docs.Count == 0),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedEmptyCollection);

            // Act
            var result = await _repositoryMock.Object.GetPersonsByDocumentsAsync(
                emptyDocuments, CancellationToken.None);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetPeoplebyIdentificationsAsync_WhenIdentificationsProvided_ShouldReturnPeople()
        {
            // Arrange
            var identifications = new List<string> { "123456789", "987654321" };
            var expectedPeople = new List<PeopleByIdentifications?>
            {
                new PeopleByIdentifications("123456789", "CC", "John Doe"),
                new PeopleByIdentifications("987654321", "CE", "Jane Smith")
            };

            _repositoryMock.Setup(x => x.GetPeoplebyIdentificationsAsync(
                It.Is<IEnumerable<string>>(ids => ids.SequenceEqual(identifications)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedPeople);

            // Act
            var result = await _repositoryMock.Object.GetPeoplebyIdentificationsAsync(
                identifications, CancellationToken.None);

            // Assert
            result.Should().BeEquivalentTo(expectedPeople);
        }


        [Fact]
        public async Task GetPeoplebyIdentificationsAsync_WhenIdentificationsEmpty_ShouldReturnEmptyCollection()
        {
            // Arrange
            var emptyIdentifications = Enumerable.Empty<string>();
            var expectedEmptyCollection = Enumerable.Empty<PeopleByIdentifications?>();

            _repositoryMock.Setup(x => x.GetPeoplebyIdentificationsAsync(
                It.Is<IEnumerable<string>>(ids => !ids.Any()),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedEmptyCollection);

            // Act
            var result = await _repositoryMock.Object.GetPeoplebyIdentificationsAsync(
                emptyIdentifications, CancellationToken.None);

            // Assert
            result.Should().BeEmpty();
        }

        public static Person CreateTestPerson(long id)
        {
            var result = Person.Create(
                $"HOM{id}",
                Guid.NewGuid(),
                $"{id}23456789",
                $"FirstName{id}",
                "Middle",
                $"LastName{id}",
                "Second",
                new DateTime(1990, 1, 1).AddDays(id),
                $"{id}234567890",
                1, 1, 1, 1,
                $"user{id}@example.com",
                1,
                Status.Active,
                $"{id} Main St",
                false,
                1, 1);

            return result.IsSuccess ? result.Value : throw new Exception("Failed to create test person");
        }
    }
}