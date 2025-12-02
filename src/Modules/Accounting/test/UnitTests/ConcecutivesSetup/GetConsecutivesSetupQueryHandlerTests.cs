using Accounting.Application.ConcecutivesSetup;
using Accounting.Domain.Consecutives;
using Accounting.Infrastructure;
using Accounting.Integrations.ConsecutivesSetup;
using FluentAssertions;
using Moq;

namespace Accounting.test.UnitTests.ConcecutivesSetup;

public class GetConsecutivesSetupQueryHandlerTests
{
    private readonly Mock<IConsecutiveRepository> _consecutiveRepositoryMock = new();
    private readonly GetConsecutivesSetupQueryHandler _handler;

    public GetConsecutivesSetupQueryHandlerTests()
    {
        _handler = new GetConsecutivesSetupQueryHandler(_consecutiveRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_All_Consecutives()
    {
        // Arrange
        var consecutives = new List<Consecutive>
        {
            CreateConsecutive(1, "ING", "DOC-001", 100),
            CreateConsecutive(2, "EGR", "DOC-002", 200)
        };

        _consecutiveRepositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(consecutives);

        // Act
        var result = await _handler.Handle(new GetConsecutivesSetupQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);

        result.Value.Should().ContainEquivalentOf(new
        {
            Id = 1L,
            Nature = "ING",
            SourceDocument = "DOC-001",
            Consecutive = 100
        });

        result.Value.Should().ContainEquivalentOf(new
        {
            Id = 2L,
            Nature = "EGR",
            SourceDocument = "DOC-002",
            Consecutive = 200
        });

        _consecutiveRepositoryMock.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private static Consecutive CreateConsecutive(long id, string nature, string sourceDocument, int number)
    {
        var consecutive = Consecutive.Create(nature, sourceDocument, number).Value;
        typeof(Consecutive).GetProperty(nameof(Consecutive.ConsecutiveId))!
            .SetValue(consecutive, id);
        return consecutive;
    }
}
