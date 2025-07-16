using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Domain;
using FluentAssertions;
using Moq;
using Operations.Infrastructure.External.Customers;
using Customers.IntegrationEvents.ClientValidation;

namespace Operations.test.UnitTests.Application.Services;

public class PersonValidatorTests
{
    private readonly Mock<IRpcClient> _rpc = new();

    [Fact]
    public async Task ValidateAsync_Should_Return_Failure_When_Rpc_Fails()
    {
        var rsp = new ValidatePersonByIdentificationResponse(false, "E", "bad");
        _rpc.Setup(r => r.CallAsync<ValidatePersonByIdentificationRequest, ValidatePersonByIdentificationResponse>(
                It.IsAny<ValidatePersonByIdentificationRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(rsp);
        var validator = new PersonValidator(_rpc.Object);

        var result = await validator.ValidateAsync("CC", "123", CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        AssertionExtensions.Should(result.Error).BeEquivalentTo(Error.Validation("E", "bad"));
    }

    [Fact]
    public async Task ValidateAsync_Should_Return_Success_When_Rpc_Succeeds()
    {
        var rsp = new ValidatePersonByIdentificationResponse(true, null, null);
        ValidatePersonByIdentificationRequest? captured = null;
        _rpc.Setup(r => r.CallAsync<ValidatePersonByIdentificationRequest, ValidatePersonByIdentificationResponse>(
                It.IsAny<ValidatePersonByIdentificationRequest>(),
                It.IsAny<CancellationToken>()))
            .Callback<ValidatePersonByIdentificationRequest, CancellationToken>((req, _) => captured = req)
            .ReturnsAsync(rsp);
        var validator = new PersonValidator(_rpc.Object);

        var result = await validator.ValidateAsync("CC", "123", CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        captured!.DocumentTypeHomologatedCode.Should().Be("CC");
        captured.IdentificationNumber.Should().Be("123");
    }
}