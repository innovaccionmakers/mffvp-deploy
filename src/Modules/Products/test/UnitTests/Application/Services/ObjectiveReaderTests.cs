using FluentAssertions;
using Moq;
using Products.Application.Objectives.Services;
using Products.Domain.Objectives;
using Products.Domain.ConfigurationParameters;
using Products.Integrations.Objectives.GetObjectives;

namespace Products.test.UnitTests.Application.Services;

public class ObjectiveReaderTests
{
    [Fact]
    public async Task BuildValidationContextAsync_Should_Not_Query_When_Affiliate_Not_Found()
    {
        var repoMock = new Mock<IObjectiveRepository>(MockBehavior.Strict);
        var reader = new ObjectiveReader(repoMock.Object, Mock.Of<IConfigurationParameterRepository>());

        var ctx = await reader.BuildValidationContextAsync(false, null, StatusType.A, CancellationToken.None);

        ctx.AffiliateExists.Should().BeFalse();
        repoMock.Verify(r => r.AnyAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}