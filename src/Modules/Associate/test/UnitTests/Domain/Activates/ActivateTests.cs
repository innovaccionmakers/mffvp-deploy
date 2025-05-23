using Associate.Domain.Activates;

namespace UnitTests.Domain.Activates
{
    public class ActivateTests
    {
        [Fact]
        public void Create_ShouldRaiseActivateCreatedDomainEvent()
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
            var activate = result.Value;

            Assert.Single(activate.DomainEvents);
        }

        [Fact]
        public void ActivateCreatedDomainEvent_ShouldContainCorrectActivateId()
        {
            // Arrange
            var activateId = 123;

            // Act
            var domainEvent = new ActivateCreatedDomainEvent(activateId);

            // Assert
            Assert.Equal(activateId, domainEvent.ActivateId);
        }

    }
}