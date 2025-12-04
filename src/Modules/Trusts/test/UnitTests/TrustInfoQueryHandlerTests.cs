using System;
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
        const decimal totalBalance = 2000.75m;
        const decimal principal = 1500.567m;
        const decimal contributionValue = 1500.567m;
        const long trustId = 9876;

        var trustRepositoryMock = new Mock<ITrustRepository>();
        var ruleEvaluatorMock = new Mock<IInternalRuleEvaluator<TrustsModuleMarker>>();
        var trust = CreateTrust(trustId, clientOperationId, totalBalance, principal, LifecycleStatus.Active);

        trustRepositoryMock
            .Setup(repo => repo.GetByClientOperationIdAsync(clientOperationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(trust);

        ruleEvaluatorMock
            .Setup(evaluator => evaluator.EvaluateAsync(
                "Trusts.TrustInfo.Validation",
                It.Is<object>(context =>
                    ContextPropertyEquals(context, "TrustPrincipal", decimal.Round(principal, 2, MidpointRounding.AwayFromZero)) &&
                    ContextPropertyEquals(context, "ContributionValue", decimal.Round(contributionValue, 2, MidpointRounding.AwayFromZero)) &&
                    ContextPropertyEquals(context, "TrustExists", true) &&
                    ContextPropertyEquals(context, "TrustIsActive", true)),
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
        const decimal totalBalance = 2000.75m;
        const decimal principal = 1750.215m;
        const decimal contributionValue = 1500.50m;
        const string errorCode = "TRUST-001";
        const string errorMessage = "Mensaje de error";

        var trustRepositoryMock = new Mock<ITrustRepository>();
        var ruleEvaluatorMock = new Mock<IInternalRuleEvaluator<TrustsModuleMarker>>();
        var trust = CreateTrust(42, clientOperationId, totalBalance, principal, LifecycleStatus.Active);

        trustRepositoryMock
            .Setup(repo => repo.GetByClientOperationIdAsync(clientOperationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(trust);

        var validationError = new RuleValidationError(errorCode, errorMessage);

        ruleEvaluatorMock
            .Setup(evaluator => evaluator.EvaluateAsync(
                "Trusts.TrustInfo.Validation",
                It.Is<object>(context =>
                    ContextPropertyEquals(context, "TrustPrincipal", decimal.Round(principal, 2, MidpointRounding.AwayFromZero)) &&
                    ContextPropertyEquals(context, "ContributionValue", decimal.Round(contributionValue, 2, MidpointRounding.AwayFromZero)) &&
                    ContextPropertyEquals(context, "TrustExists", true) &&
                    ContextPropertyEquals(context, "TrustIsActive", true)),
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

    private static Trust CreateTrust(long trustId, long clientOperationId, decimal totalBalance, decimal principal, LifecycleStatus status)
    {
        var creationResult = Trust.Create(
            affiliateId: 1,
            clientOperationId: clientOperationId,
            creationDate: DateTime.UtcNow,
            objectiveId: 1,
            portfolioId: 1,
            totalBalance: totalBalance,
            totalUnits: 1,
            principal: principal,
            earnings: 0,
            taxCondition: 1,
            contingentWithholding: 0,
            earningsWithholding: 0,
            availableAmount: totalBalance,
            protectedBalance: 0,
            agileWithdrawalAvailable: 0,
            status: status);

        var trust = creationResult.Value;
        typeof(Trust)
            .GetProperty(nameof(Trust.TrustId), BindingFlags.Instance | BindingFlags.Public)!
            .SetValue(trust, trustId);

        return trust;
    }

    private static bool ContextPropertyEquals<TValue>(object context, string propertyName, TValue expectedValue)
    {
        var property = context.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);

        if (property is null)
        {
            return false;
        }

        var value = property.GetValue(context);
        return Equals(value, expectedValue);
    }
}
