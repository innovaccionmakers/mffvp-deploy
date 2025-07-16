using Associate.IntegrationEvents.ActivateValidation;
using Associate.Integrations.Activates;
using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Domain;
using FluentAssertions;
using Moq;
using Products.Infrastructure.External.Affiliates;

namespace Products.test.UnitTests.Application.Services;

public class AffiliateLocatorTests
{
    private readonly Mock<IRpcClient> _rpc = new();

    [Fact]
    public async Task FindAsync_Should_Return_Failure_When_Rpc_Fails()
    {
        var response = new GetActivateIdByIdentificationResponse(false, null, "E01", "bad");
        _rpc.Setup(r => r.CallAsync<GetActivateIdByIdentificationRequest, GetActivateIdByIdentificationResponse>(
                It.IsAny<GetActivateIdByIdentificationRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
        var locator = new AffiliateLocator(_rpc.Object);

        var result = await locator.FindAsync("CC", "123", CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        AssertionExtensions.Should(result.Error).BeEquivalentTo(Error.Validation("E01", "bad"));
    }

    [Fact]
    public async Task FindAsync_Should_Return_Id_When_Rpc_Succeeds()
    {
        var activate = new ActivateResponse(
            55,
            "CC",
            Guid.NewGuid(),
            "321",
            false,
            false,
            DateTime.UtcNow);

        var response = new GetActivateIdByIdentificationResponse(true, activate, null, null);

        GetActivateIdByIdentificationRequest? captured = null;
        _rpc.Setup(r => r.CallAsync<GetActivateIdByIdentificationRequest, GetActivateIdByIdentificationResponse>(
                It.IsAny<GetActivateIdByIdentificationRequest>(),
                It.IsAny<CancellationToken>()))
            .Callback<GetActivateIdByIdentificationRequest, CancellationToken>((req, _) =>
                captured = req)
            .ReturnsAsync(response);

        var locator = new AffiliateLocator(_rpc.Object);

        var result = await locator.FindAsync("CC", "321", CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(55);
        captured!.DocumentType.Should().Be("CC");
        captured.Identification.Should().Be("321");
    }
}