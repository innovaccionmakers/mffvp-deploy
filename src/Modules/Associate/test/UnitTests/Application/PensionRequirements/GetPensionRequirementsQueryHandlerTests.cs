using Associate.Integrations.PensionRequirements;
using Associate.Integrations.PensionRequirements.GetPensionRequirements;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;

namespace UnitTests.Application.PensionRequirements
{
    public class GetPensionRequirementsQueryHandlerTests
    {
        [Fact]
        public void Query_ShouldImplementIQueryInterface()
        {
            // Arrange & Act
            var query = new GetPensionRequirementsQuery();

            // Assert
            Assert.IsAssignableFrom<IQuery<IReadOnlyCollection<PensionRequirementResponse>>>(query);
        }

        [Fact]
        public void Query_ShouldBeSealedRecord()
        {
            // Arrange & Act
            var query = new GetPensionRequirementsQuery();

            // Assert
            Assert.True(query.GetType().IsSealed);
        }    

        [Fact]
        public void Constructor_ShouldSetPropertiesCorrectly()
        {
            // Arrange
            var id = 1;
            var activateId = 123;
            var startDate = DateTime.Now;
            var expirationDate = DateTime.Now.AddYears(1);
            var creationDate = DateTime.UtcNow;
            var status = Status.Active;

            // Act
            var response = new PensionRequirementResponse(
                id, activateId, startDate, expirationDate, creationDate, status);

            // Assert
            Assert.Equal(id, response.PensionRequirementId);
            Assert.Equal(activateId, response.ActivateId);
            Assert.Equal(startDate, response.StartDate);
            Assert.Equal(expirationDate, response.ExpirationDate);
            Assert.Equal(creationDate, response.CreationDate);
            Assert.Equal(status, response.Status);
        }
    }
}