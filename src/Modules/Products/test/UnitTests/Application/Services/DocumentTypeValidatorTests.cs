using System.Threading;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using FluentAssertions;
using Moq;
using People.IntegrationEvents.DocumentTypeValidation;
using Products.Infrastructure.External.DocumentTypes;

namespace Products.test.UnitTests.Application.Services;

public class DocumentTypeValidatorTests
{
    private readonly Mock<ICapRpcClient> _rpc = new();

    [Fact]
    public async Task EnsureExistsAsync_Should_Return_Failure_When_Rpc_Fails()
    {
        var response = new GetDocumentTypeIdByCodeResponse(false, null, "E02", "err");
        _rpc.Setup(r => r.CallAsync<GetDocumentTypeIdByCodeRequest, GetDocumentTypeIdByCodeResponse>(
                nameof(GetDocumentTypeIdByCodeRequest),
                It.IsAny<GetDocumentTypeIdByCodeRequest>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
        var validator = new DocumentTypeValidator(_rpc.Object);

        var result = await validator.EnsureExistsAsync("CC", CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        AssertionExtensions.Should(result.Error).BeEquivalentTo(Error.Validation("E02", "err"));
    }

    [Fact]
    public async Task EnsureExistsAsync_Should_Return_Success_When_Rpc_Succeeds()
    {
        var response = new GetDocumentTypeIdByCodeResponse(true, 1, null, null);
        GetDocumentTypeIdByCodeRequest? captured = null;
        _rpc.Setup(r => r.CallAsync<GetDocumentTypeIdByCodeRequest, GetDocumentTypeIdByCodeResponse>(
                nameof(GetDocumentTypeIdByCodeRequest),
                It.IsAny<GetDocumentTypeIdByCodeRequest>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, GetDocumentTypeIdByCodeRequest, TimeSpan, CancellationToken>((_, req, _, _) =>
                captured = req)
            .ReturnsAsync(response);
        var validator = new DocumentTypeValidator(_rpc.Object);

        var result = await validator.EnsureExistsAsync("CC", CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        captured!.TypeIdHomologationCode.Should().Be("CC");
    }
}