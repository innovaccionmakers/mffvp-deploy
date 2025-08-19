using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Domain;
using FluentAssertions;
using Moq;
using Operations.Infrastructure.External.CollectionBankValidation;
using Treasury.IntegrationEvents.Issuers.ValidateCollectionBank;

namespace Operations.test.UnitTests.Application.Services;

public class CollectionBankValidatorTests
{
    private readonly Mock<IRpcClient> _rpc = new();

    [Fact]
    public async Task ValidateAsync_Should_Return_Error_When_Bank_Invalid()
    {
        var response = new ValidateCollectionBankResponse(false, "code", "message", null);
        _rpc.Setup(r => r.CallAsync<ValidateCollectionBankRequest, ValidateCollectionBankResponse>(
                It.IsAny<ValidateCollectionBankRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
        var validator = new CollectionBankValidator(_rpc.Object);

        var result = await validator.ValidateAsync("123", CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("code");
        result.Error.Description.Should().Be("message");
    }

    [Fact]
    public async Task ValidateAsync_Should_Send_Request_With_Homologated_Code()
    {
        var response = new ValidateCollectionBankResponse(true, null, null, 10);
        ValidateCollectionBankRequest? captured = null;
        _rpc.Setup(r => r.CallAsync<ValidateCollectionBankRequest, ValidateCollectionBankResponse>(
                It.IsAny<ValidateCollectionBankRequest>(),
                It.IsAny<CancellationToken>()))
            .Callback<ValidateCollectionBankRequest, CancellationToken>((req, _) => captured = req)
            .ReturnsAsync(response);
        var validator = new CollectionBankValidator(_rpc.Object);

        var result = await validator.ValidateAsync("B-001", CancellationToken.None);

        result.Value.Should().Be(10);
        captured!.HomologatedCode.Should().Be("B-001");
    }
}