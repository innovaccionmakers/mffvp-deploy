using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using FluentAssertions;
using Moq;
using Trusts.Application.Queries;
using Trusts.Domain.Trusts;
using Trusts.Integrations.Trusts.Queries;

namespace Trusts.test.UnitTests;

public sealed class GetTrustByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnTrustDetails_WhenTrustExists()
    {
        // Arrange
        const long trustId = 1001;
        var trust = CreateTrust(trustId);
        trust.UpdateState(
            trust.TotalBalance,
            trust.TotalUnits,
            trust.Principal,
            trust.Earnings,
            trust.ContingentWithholding,
            trust.EarningsWithholding,
            trust.AvailableAmount,
            trust.Status,
            new DateTime(2024, 01, 01, 12, 0, 0, DateTimeKind.Utc));

        var trustRepositoryMock = new Mock<ITrustRepository>();
        trustRepositoryMock
            .Setup(repository => repository.GetAsync(trustId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(trust);

        var handler = new GetTrustByIdQueryHandler(trustRepositoryMock.Object);
        var query = new GetTrustByIdQuery(trustId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.TrustId.Should().Be(trustId);
        result.Value.UpdateDate.Should().Be(trust.UpdateDate);

        trustRepositoryMock.Verify(repository => repository.GetAsync(trustId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenTrustDoesNotExist()
    {
        // Arrange
        const long trustId = 9999;
        var trustRepositoryMock = new Mock<ITrustRepository>();
        trustRepositoryMock
            .Setup(repository => repository.GetAsync(trustId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Trust?)null);

        var handler = new GetTrustByIdQueryHandler(trustRepositoryMock.Object);
        var query = new GetTrustByIdQuery(trustId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    private static Trust CreateTrust(long trustId)
    {
        var creationResult = Trust.Create(
            affiliateId: 10,
            clientOperationId: 2000,
            creationDate: new DateTime(2023, 12, 31, 0, 0, 0, DateTimeKind.Utc),
            objectiveId: 3,
            portfolioId: 4,
            totalBalance: 5000m,
            totalUnits: 100m,
            principal: 4000m,
            earnings: 1000m,
            taxCondition: 2,
            contingentWithholding: 50m,
            earningsWithholding: 20m,
            availableAmount: 4800m,
            status: LifecycleStatus.Active);

        creationResult.IsSuccess.Should().BeTrue();

        var trust = creationResult.Value;
        typeof(Trust)
            .GetProperty(nameof(Trust.TrustId), BindingFlags.Instance | BindingFlags.Public)!
            .SetValue(trust, trustId);

        return trust;
    }
}
