using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using FluentAssertions;
using Moq;
using Operations.Application.Abstractions.External;
using Operations.Infrastructure.External.ContributionValidation;
using Products.IntegrationEvents.ContributionValidation;

namespace Operations.test.UnitTests.Application.Services;

public class ContributionRemoteValidatorTests
{
    private readonly Mock<IRpcClient> _rpc = new();

    [Fact]
    public async Task ValidateAsync_Should_Return_Failure_When_Rpc_Fails()
    {
        var rsp = new ContributionValidationResponse(false, "E", "bad");
        _rpc.Setup(r => r.CallAsync<ContributionValidationRequest, ContributionValidationResponse>(
                It.IsAny<ContributionValidationRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(rsp);
        var validator = new ContributionRemoteValidator(_rpc.Object);

        var result = await validator.ValidateAsync(1, 2, "P", DateTime.Today, DateTime.Today, 100m, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        AssertionExtensions.Should(result.Error).BeEquivalentTo(Error.Validation("E", "bad"));
    }

    [Fact]
    public async Task ValidateAsync_Should_Return_Data_When_Rpc_Succeeds()
    {
        var rsp = new ContributionValidationResponse(true, null, null, 10, 20, 30, 50m, 25m, "name");
        ContributionValidationRequest? captured = null;
        _rpc.Setup(r => r.CallAsync<ContributionValidationRequest, ContributionValidationResponse>(
                It.IsAny<ContributionValidationRequest>(),
                It.IsAny<CancellationToken>()))
            .Callback<ContributionValidationRequest, CancellationToken>((req, _) => captured = req)
            .ReturnsAsync(rsp);
        var validator = new ContributionRemoteValidator(_rpc.Object);
        var deposit = new DateTime(2024, 1, 1);
        var exec = new DateTime(2024, 1, 2);

        var result = await validator.ValidateAsync(1, 20, "P", deposit, exec, 100m, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(new ContributionRemoteData(10, 20, 30, "name", 50m, 25m));
        captured!.ActivateId.Should().Be(1);
        captured.ObjectiveId.Should().Be(20);
        captured.PortfolioHomologatedCode.Should().Be("P");
        captured.DepositDate.Should().Be(deposit);
        captured.ExecutionDate.Should().Be(exec);
        captured.Amount.Should().Be(100m);
    }
}