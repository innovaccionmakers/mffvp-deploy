using Associate.IntegrationEvents.ActivateValidation;
using Associate.Integrations.Activates;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using FluentAssertions;
using Moq;
using Operations.Infrastructure.External.Activate;

namespace Operations.test.UnitTests.Application.Services;

public class ActivateLocatorTests
{
    private readonly Mock<ICapRpcClient> _rpc = new();

    [Fact]
    public async Task FindAsync_Should_Return_Failure_When_Rpc_Fails()
    {
        var rsp = new GetActivateIdByIdentificationResponse(false, null, "E", "bad");
        _rpc.Setup(r => r.CallAsync<GetActivateIdByIdentificationRequest, GetActivateIdByIdentificationResponse>(
                nameof(GetActivateIdByIdentificationRequest),
                It.IsAny<GetActivateIdByIdentificationRequest>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(rsp);
        var locator = new ActivateLocator(_rpc.Object);

        var result = await locator.FindAsync("CC", "123", CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        AssertionExtensions.Should(result.Error).BeEquivalentTo(Error.Validation("E", "bad"));
    }

    [Fact]
    public async Task FindAsync_Should_Return_Data_When_Rpc_Succeeds()
    {
        var activate = new ActivateResponse(1, "CC", "321", false, false, DateTime.Today);
        var rsp = new GetActivateIdByIdentificationResponse(true, activate, null, null);
        GetActivateIdByIdentificationRequest? captured = null;
        _rpc.Setup(r => r.CallAsync<GetActivateIdByIdentificationRequest, GetActivateIdByIdentificationResponse>(
                nameof(GetActivateIdByIdentificationRequest),
                It.IsAny<GetActivateIdByIdentificationRequest>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, GetActivateIdByIdentificationRequest, TimeSpan, CancellationToken>((_, req, _, _) => captured = req)
            .ReturnsAsync(rsp);
        var locator = new ActivateLocator(_rpc.Object);

        var result = await locator.FindAsync("CC", "321", CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be((true, 1, false));
        captured!.IdentificationType.Should().Be("CC");
        captured.Identification.Should().Be("321");
    }
}