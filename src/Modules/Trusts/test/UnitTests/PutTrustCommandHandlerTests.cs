using Common.SharedKernel.Application.Rules;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using RulesEngine.Models;
using Trusts.Application.Abstractions;
using Trusts.Application.Abstractions.Data;
using Trusts.Application.Trusts.PutTrust;
using Trusts.Domain.Trusts;
using Trusts.Integrations.Trusts.PutTrust;

namespace Trusts.test.UnitTests;

public class PutTrustCommandHandlerTests
{
    private static readonly (bool Success, IReadOnlyCollection<RuleResultTree> Results, IReadOnlyCollection<RuleValidationError> Errors) SuccessfulRules =
        (true, Array.Empty<RuleResultTree>(), Array.Empty<RuleValidationError>());

    [Fact]
    public async Task Handle_WhenRequiredFieldsFail_ReturnsFailure()
    {
        var command = new PutTrustCommand(
            ClientOperationId: 10,
            Status: LifecycleStatus.Active,
            TotalBalance: 100m,
            TotalUnits: 40m,
            Principal: 50m,
            Earnings: 25m,
            ContingentWithholding: 10m,
            EarningsWithholding: 5m,
            AvailableAmount: 15m,
            UpdateDate: DateTime.UtcNow);

        var ruleError = new RuleValidationError("TRUST.REQUIRED", "Los campos requeridos son obligatorios");
        var ruleEvaluatorMock = new Mock<IInternalRuleEvaluator<TrustsModuleMarker>>();
        ruleEvaluatorMock
            .Setup(evaluator => evaluator.EvaluateAsync(
                "Trusts.PutTrust.RequiredFields",
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, Array.Empty<RuleResultTree>(), new[] { ruleError }));

        var trustRepositoryMock = new Mock<ITrustRepository>(MockBehavior.Strict);
        var unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);

        var handler = new PutTrustCommandHandler(
            trustRepositoryMock.Object,
            unitOfWorkMock.Object,
            ruleEvaluatorMock.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(ruleError.Code);
        result.Error.Description.Should().Be(ruleError.Message);
        trustRepositoryMock.Verify(repository => repository.GetByClientOperationIdAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenTrustValidationFails_ReturnsFailure()
    {
        var command = new PutTrustCommand(
            ClientOperationId: 20,
            Status: LifecycleStatus.Annulled,
            TotalBalance: 200m,
            TotalUnits: 80m,
            Principal: 100m,
            Earnings: 40m,
            ContingentWithholding: 20m,
            EarningsWithholding: 10m,
            AvailableAmount: 60m,
            UpdateDate: DateTime.UtcNow);

        var ruleEvaluatorMock = new Mock<IInternalRuleEvaluator<TrustsModuleMarker>>();
        ruleEvaluatorMock
            .Setup(evaluator => evaluator.EvaluateAsync(
                "Trusts.PutTrust.RequiredFields",
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(SuccessfulRules);

        var validationError = new RuleValidationError("TRUST.NOT_FOUND", "El fideicomiso no existe");
        ruleEvaluatorMock
            .Setup(evaluator => evaluator.EvaluateAsync(
                "Trusts.PutTrust.Validation",
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, Array.Empty<RuleResultTree>(), new[] { validationError }));

        var trustRepositoryMock = new Mock<ITrustRepository>();
        trustRepositoryMock
            .Setup(repository => repository.GetByClientOperationIdAsync(command.ClientOperationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Trust?)null);

        var unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);

        var handler = new PutTrustCommandHandler(
            trustRepositoryMock.Object,
            unitOfWorkMock.Object,
            ruleEvaluatorMock.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(validationError.Code);
        result.Error.Description.Should().Be(validationError.Message);
        unitOfWorkMock.Verify(work => work.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenRequestIsValid_UpdatesTrust()
    {
        var command = new PutTrustCommand(
            ClientOperationId: 30,
            Status: LifecycleStatus.Annulled,
            TotalBalance: 500m,
            TotalUnits: 250m,
            Principal: 250m,
            Earnings: 125m,
            ContingentWithholding: 70m,
            EarningsWithholding: 35m,
            AvailableAmount: 300m,
            UpdateDate: new DateTime(2024, 01, 10, 12, 0, 0, DateTimeKind.Utc));

        var ruleEvaluatorMock = new Mock<IInternalRuleEvaluator<TrustsModuleMarker>>();
        ruleEvaluatorMock
            .Setup(evaluator => evaluator.EvaluateAsync(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(SuccessfulRules);

        var trust = Trust
            .Create(
                affiliateId: 1,
                clientOperationId: command.ClientOperationId,
                creationDate: DateTime.UtcNow.AddMonths(-1),
                objectiveId: 2,
                portfolioId: 3,
                totalBalance: 100m,
                totalUnits: 50m,
                principal: 40m,
                earnings: 10m,
                taxCondition: 1,
                contingentWithholding: 5m,
                earningsWithholding: 3m,
                availableAmount: 12m,
                status: LifecycleStatus.Active)
            .Value;

        var trustRepositoryMock = new Mock<ITrustRepository>();
        trustRepositoryMock
            .Setup(repository => repository.GetByClientOperationIdAsync(command.ClientOperationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(trust);
        trustRepositoryMock.Setup(repository => repository.Update(trust));

        var transactionMock = new Mock<IDbContextTransaction>();
        transactionMock
            .Setup(transaction => transaction.CommitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        transactionMock
            .Setup(transaction => transaction.DisposeAsync())
            .Returns(ValueTask.CompletedTask);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock
            .Setup(work => work.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(transactionMock.Object);
        unitOfWorkMock
            .Setup(work => work.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new PutTrustCommandHandler(
            trustRepositoryMock.Object,
            unitOfWorkMock.Object,
            ruleEvaluatorMock.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        trust.TotalBalance.Should().Be(command.TotalBalance);
        trust.TotalUnits.Should().Be(command.TotalUnits);
        trust.Principal.Should().Be(command.Principal);
        trust.Earnings.Should().Be(command.Earnings);
        trust.ContingentWithholding.Should().Be(command.ContingentWithholding);
        trust.EarningsWithholding.Should().Be(command.EarningsWithholding);
        trust.AvailableAmount.Should().Be(command.AvailableAmount);
        trust.Status.Should().Be(command.Status);
        trust.UpdateDate.Should().Be(command.UpdateDate);
        unitOfWorkMock.Verify(work => work.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        transactionMock.Verify(transaction => transaction.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        trustRepositoryMock.Verify(repository => repository.Update(trust), Times.Once);
    }
}
