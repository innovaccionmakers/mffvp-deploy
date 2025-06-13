using Associate.Domain.PensionRequirements;
using Common.SharedKernel.Domain;

namespace UnitTests.Domain.PensionRequirements
{
    public class PensionRequirementTests
    {
        [Fact]
        public void Create_WithValidData_ShouldReturnSuccessResult()
        {
            // Arrange
            var startDate = DateTime.Now;
            var expirationDate = DateTime.Now.AddYears(1);
            var creationDate = DateTime.UtcNow;
            var activateId = 123;

            // Act
            var result = PensionRequirement.Create(startDate, expirationDate, creationDate, Status.Active, activateId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(activateId, result.Value.ActivateId);
            Assert.Equal(startDate, result.Value.StartDate);
            Assert.Equal(expirationDate, result.Value.ExpirationDate);
            Assert.Equal(creationDate, result.Value.CreationDate);
            Assert.Equal(Status.Active, result.Value.Status);
        }

        [Fact]
        public void UpdateDetails_ShouldChangeStatus()
        {
            // Arrange
            var pensionRequirement = PensionRequirement.Create(
                DateTime.Now,
                DateTime.Now.AddYears(1),
                DateTime.UtcNow,
                Status.Active,
                123).Value;

            // Act
            pensionRequirement.UpdateDetails(Status.Inactive);

            // Assert
            Assert.Equal(Status.Inactive, pensionRequirement.Status);
        }
        
        [Fact]
        public void GetProperties_AfterCreation_ShouldReturnCorrectValues()
        {
            // Arrange
            var startDate = DateTime.Now;
            var expirationDate = DateTime.Now.AddYears(1);
            var creationDate = DateTime.UtcNow;
            var activateId = 123;
            var status = Status.Active;

            // Act
            var result = PensionRequirement.Create(startDate, expirationDate, creationDate, status, activateId);
            var requirement = result.Value;

            // Assert
            Assert.Equal(0, requirement.PensionRequirementId); // Debería ser 0 hasta que se persista
            Assert.Equal(activateId, requirement.ActivateId);
            Assert.Equal(startDate, requirement.StartDate);
            Assert.Equal(expirationDate, requirement.ExpirationDate);
            Assert.Equal(creationDate, requirement.CreationDate);
            Assert.Equal(status, requirement.Status);
        }

        [Fact]
        public void GetProperties_AfterUpdate_ShouldReflectChanges()
        {
            // Arrange
            var requirement = PensionRequirement.Create(
                DateTime.Now,
                DateTime.Now.AddYears(1),
                DateTime.UtcNow,
                Status.Active,
                123).Value;

            var newStatus = Status.Inactive;

            // Act
            requirement.UpdateDetails(newStatus);

            // Assert
            Assert.Equal(newStatus, requirement.Status);
            // Verificar que otras propiedades no cambian
            Assert.NotEqual(0, requirement.ActivateId);
            Assert.NotNull(requirement.StartDate);
            Assert.NotNull(requirement.ExpirationDate);
            Assert.NotNull(requirement.CreationDate);
        }

        [Fact]
        public void GetDomainEvents_AfterCreation_ShouldContainEvent()
        {
            // Arrange
            var requirement = PensionRequirement.Create(
                DateTime.Now,
                DateTime.Now.AddYears(1),
                DateTime.UtcNow,
                Status.Active,
                123).Value;

            // Act
            var domainEvents = requirement.DomainEvents;

            // Assert
            Assert.Single(domainEvents);
            var domainEvent = domainEvents.First() as PensionRequirementCreatedDomainEvent;
            Assert.NotNull(domainEvent);
            Assert.Equal(requirement.PensionRequirementId, domainEvent.PensionRequirementId);
        }
    }
}