using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using FluentAssertions;
using Moq;
using Operations.Application.Abstractions.External;
using Operations.Infrastructure.External.Trusts;
using Trusts.IntegrationEvents.CreateTrust;
using Trusts.Integrations.Trusts;

namespace Operations.test.UnitTests.Application.Services;

public class TrustCreatorTests
{
    private readonly Mock<ICapRpcClient> _rpc = new();

    private static TrustCreationDto Dto() => new(1, 10, DateTime.Today, 20, 30, 100m, 1, 80m, 20m, 1, 2m, 3m, 50m);

    [Fact]
    public async Task CreateAsync_Should_Return_Failure_When_Rpc_Fails()
    {
        var rsp = new CreateTrustResponse(false, null, "E", "bad");
        _rpc.Setup(r => r.CallAsync<CreateTrustRequest, CreateTrustResponse>(
                nameof(CreateTrustRequest),
                It.IsAny<CreateTrustRequest>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(rsp);
        var creator = new TrustCreator(_rpc.Object);

        var result = await creator.CreateAsync(Dto(), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        AssertionExtensions.Should(result.Error).BeEquivalentTo(Error.Validation("E", "bad"));
    }

    [Fact]
    public async Task CreateAsync_Should_Return_Success_When_Rpc_Succeeds()
    {
        var trust = new TrustResponse(5, 1, 10, DateTime.Today, 20, 30, 100m, 1, 80m, 20m, 1, 2m, 3m, 50m);
        var rsp = new CreateTrustResponse(true, trust, null, null);
        CreateTrustRequest? captured = null;
        _rpc.Setup(r => r.CallAsync<CreateTrustRequest, CreateTrustResponse>(
                nameof(CreateTrustRequest),
                It.IsAny<CreateTrustRequest>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, CreateTrustRequest, TimeSpan, CancellationToken>((_, req, _, _) => captured = req)
            .ReturnsAsync(rsp);
        var creator = new TrustCreator(_rpc.Object);
        var dto = Dto();

        var result = await creator.CreateAsync(dto, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        captured!.AffiliateId.Should().Be(dto.AffiliateId);
        captured.ClientOperationId.Should().Be(dto.ClientOperationId);
        captured.ObjectiveId.Should().Be(dto.ObjectiveId);
    }
}