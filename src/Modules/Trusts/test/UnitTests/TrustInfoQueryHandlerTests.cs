using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Common.SharedKernel.Application.Rules;
using Common.SharedKernel.Core.Primitives;
using FluentAssertions;
using Moq;
using RulesEngine.Models;
using Trusts.Application.Abstractions;
using Trusts.Application.TrustInfo;
using Trusts.Domain.Trusts;
using Trusts.Integrations.Trusts.TrustInfo;

namespace Trusts.test.UnitTests;

public sealed class TrustInfoQueryHandlerTests
{
    [Fact]
    public async Task Handle_Should_ReturnSuccess_WhenRulesPass()
    {
        // Arrange
        const long clientOperationId = 1234;
        const decimal contributionValue = 1500.50m;
        const long trustId = 9876;

        var trustRepositoryMock = new Mock<ITrustRepository>();
        var ruleEvaluatorMock = new Mock<IInternalRuleEvaluator<TrustsModuleMarker>>();
        var trust = CreateTrust(trustId, clientOperationId, contributionValue, LifecycleStatus.Active);

        trustRepositoryMock
            .Setup(repo => repo.GetByClientOperationIdAsync(clientOperationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(trust);

        ruleEvaluatorMock
            .Setup(evaluator => evaluator.EvaluateAsync(
                "Trusts.TrustInfo.Validation",
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, Array.Empty<RuleResultTree>(), Array.Empty<RuleValidationError>()));

        var handler = new TrustInfoQueryHandler(trustRepositoryMock.Object, ruleEvaluatorMock.Object);
        var query = new TrustInfoQuery(clientOperationId, contributionValue);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TrustId.Should().Be(trustId);

        trustRepositoryMock.Verify(
            repo => repo.GetByClientOperationIdAsync(clientOperationId, It.IsAny<CancellationToken>()),
            Times.Once);

        ruleEvaluatorMock.Verify(
            evaluator => evaluator.EvaluateAsync(
                "Trusts.TrustInfo.Validation",
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenRulesFail()
    {
        // Arrange
        const long clientOperationId = 1234;
        const decimal contributionValue = 1500.50m;
        const string errorCode = "TRUST-001";
        const string errorMessage = "Mensaje de error";

        var trustRepositoryMock = new Mock<ITrustRepository>();
        var ruleEvaluatorMock = new Mock<IInternalRuleEvaluator<TrustsModuleMarker>>();
        var trust = CreateTrust(42, clientOperationId, contributionValue, LifecycleStatus.Active);

        trustRepositoryMock
            .Setup(repo => repo.GetByClientOperationIdAsync(clientOperationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(trust);

        var validationError = new RuleValidationError(errorCode, errorMessage);

        ruleEvaluatorMock
            .Setup(evaluator => evaluator.EvaluateAsync(
                "Trusts.TrustInfo.Validation",
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, Array.Empty<RuleResultTree>(), new[] { validationError }));

        var handler = new TrustInfoQueryHandler(trustRepositoryMock.Object, ruleEvaluatorMock.Object);
        var query = new TrustInfoQuery(clientOperationId, contributionValue);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(errorCode);
        result.Error.Description.Should().Be(errorMessage);
    }

    private static Trust CreateTrust(long trustId, long clientOperationId, decimal totalBalance, LifecycleStatus status)
    {
        var creationResult = Trust.Create(
            affiliateId: 1,
            clientOperationId: clientOperationId,
            creationDate: DateTime.UtcNow,
            objectiveId: 1,
            portfolioId: 1,
            totalBalance: totalBalance,
            totalUnits: 1,
            principal: totalBalance,
            earnings: 0,
            taxCondition: 1,
            contingentWithholding: 0,
            earningsWithholding: 0,
            availableAmount: totalBalance,
            status: status);

        var trust = creationResult.Value;
        typeof(Trust)
            .GetProperty(nameof(Trust.TrustId), BindingFlags.Instance | BindingFlags.Public)!
            .SetValue(trust, trustId);

        return trust;
    }
}
