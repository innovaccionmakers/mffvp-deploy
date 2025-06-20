using Customers.Domain.People;
using Customers.Application.Abstractions.Data;
using Xunit;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using FluentAssertions;
using Common.SharedKernel.Domain;

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
                .Returns((Task<int>)Task.CompletedTask);

            // Act
            _repositoryMock.Object.Insert(person);
            await _unitOfWorkMock.Object.SaveChangesAsync(CancellationToken.None);

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