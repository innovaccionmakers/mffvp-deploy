using Associate.Integrations.PensionRequirements.CreatePensionRequirement;

namespace UnitTests.Application.PensionRequirements.CreatePensionRequirement
{
    public class CreatePensionRequirementCommandTests
    {
        [Fact]
        public void Command_ShouldHaveCorrectPropertyValues()
        {
            // Arrange
            var startDate = DateTime.Now;
            var endDate = DateTime.Now.AddYears(1);
            var idType = "CC";
            var idNumber = "123456";

            // Act
            var command = new CreatePensionRequirementCommand(idType, idNumber, startDate, endDate);

            // Assert
            Assert.Equal(idType, command.IdentificationType);
            Assert.Equal(idNumber, command.Identification);
            Assert.Equal(startDate, command.StartDateReqPen);
            Assert.Equal(endDate, command.EndDateReqPen);
        }
    }
}