using Common.SharedKernel.Core.Primitives;

using FluentAssertions;

using Moq;

using Products.Application.Objectives.Services;
using Products.Domain.ConfigurationParameters;
using Products.Domain.Objectives;
using Products.Integrations.Objectives.GetObjectives;

namespace Products.test.UnitTests.Application.Services;

public class ObjectiveReaderTests
{
    [Fact]
    public async Task BuildValidationContextAsync_Should_Not_Query_When_Affiliate_Not_Found()
    {
        // arrange
        var repoMock = new Mock<IObjectiveRepository>(MockBehavior.Strict);
        var configRepo = Mock.Of<IConfigurationParameterRepository>();
        var reader = new ObjectiveReader(repoMock.Object, configRepo);

        // act
        var ctx = await reader.BuildValidationContextAsync(
            false,
            null,
            StatusType.A,
            false,
            CancellationToken.None);

        // assert
        ctx.AffiliateExists.Should().BeFalse();
        ctx.DocumentTypeExists.Should().BeFalse();

        repoMock.Verify(r => r.AnyAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        repoMock.Verify(r => r.AnyWithStatusAsync(It.IsAny<int>(), It.IsAny<Status>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}