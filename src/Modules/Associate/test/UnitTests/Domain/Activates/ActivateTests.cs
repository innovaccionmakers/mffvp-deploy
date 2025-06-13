using Associate.Domain.Activates;

namespace UnitTests.Domain.Activates
{
    public class ActivateTests
    {
        [Fact]
        public void Create_ShouldRaiseActivateCreatedDomainEvent()
        {
            // Arrange
            var identificationType = new Guid();
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

        

        [Fact]
        public void UpdateDetails_ShouldChangeOnlyPensionerStatus()
        {
            // Arrange
            var original = Activate.Create(new Guid(), "123", false, true, DateTime.UtcNow).Value;
            var originalIdentificationType = original.DocumentType;
            var originalIdentification = original.Identification;
            var originalMeetsRequirements = original.MeetsPensionRequirements;
            var originalDate = original.ActivateDate;

            // Act
            original.UpdateDetails(true);

            // Assert
            Assert.True(original.Pensioner);
            Assert.Equal(originalIdentificationType, original.DocumentType);
            Assert.Equal(originalIdentification, original.Identification);
            Assert.Equal(originalMeetsRequirements, original.MeetsPensionRequirements);
            Assert.Equal(originalDate, original.ActivateDate);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void UpdateDetails_ShouldSetCorrectPensionerValue(bool newValue)
        {
            // Arrange
            var activate = Activate.Create(new Guid(), "123", !newValue, true, DateTime.UtcNow).Value;

            // Act
            activate.UpdateDetails(newValue);

            // Assert
            Assert.Equal(newValue, activate.Pensioner);
        }
    }
}