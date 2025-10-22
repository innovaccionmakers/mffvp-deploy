using System.Linq;
using System.Reflection;
using System.Text.Json;
using Common.SharedKernel.Application.Rules;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Common.SharedKernel.Domain.ConfigurationParameters;
using Common.SharedKernel.Domain.OperationTypes;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Operations.Application.Abstractions;
using Operations.Application.Abstractions.External;
using Operations.Application.Abstractions.Services.Closing;
using Operations.Application.Abstractions.Services.Voids;
using Operations.Application.Voids.RegisterVoidedTransactions;
using Operations.Domain.ClientOperations;
using Operations.Domain.ConfigurationParameters;
using Operations.Domain.OperationTypes;
using Operations.Integrations.Voids.RegisterVoidedTransactions;
using RulesEngine.Models;

namespace Operations.test.UnitTests.Application.Voids.RegisterVoidedTransactions;

public class VoidedTransactionsValCommandHandlerTests
{
    private static readonly (bool Success, IReadOnlyCollection<RuleResultTree> Results, IReadOnlyCollection<RuleValidationError> Errors) SuccessfulRules =
        (true, Array.Empty<RuleResultTree>(), Array.Empty<RuleValidationError>());

    [Fact]
    public async Task Handle_WhenRequiredFieldsValidationFails_ReturnsFailure()
    {
        var items = new[] { new VoidedTransactionItem(10, 1_000m) };
        var command = new VoidedTransactionsValCommand(items, CauseId: 1, AffiliateId: 2, ObjectiveId: 3);

        var ruleError = new RuleValidationError("OPS_VOID_REQUIRED", "Los campos requeridos son obligatorios.");
        var ruleEvaluatorMock = new Mock<IInternalRuleEvaluator<OperationsModuleMarker>>();
        ruleEvaluatorMock
            .Setup(evaluator => evaluator.EvaluateAsync(
                "Operations.Voids.RequiredFields",
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, Array.Empty<RuleResultTree>(), new[] { ruleError }));

        var clientOperationRepositoryMock = new Mock<IClientOperationRepository>(MockBehavior.Strict);
        var configurationParameterRepositoryMock = new Mock<IConfigurationParameterRepository>(MockBehavior.Strict);
        var operationTypeRepositoryMock = new Mock<IOperationTypeRepository>(MockBehavior.Strict);
        var portfolioLocatorMock = new Mock<IPortfolioLocator>(MockBehavior.Strict);
        var closingValidatorMock = new Mock<IClosingValidator>(MockBehavior.Strict);
        var trustInfoProviderMock = new Mock<ITrustInfoProvider>(MockBehavior.Strict);
        var voidsOperMock = new Mock<IVoidsOper>(MockBehavior.Strict);
        var loggerMock = new Mock<ILogger<VoidedTransactionsValCommandHandler>>();

        var handler = new VoidedTransactionsValCommandHandler(
            ruleEvaluatorMock.Object,
            clientOperationRepositoryMock.Object,
            configurationParameterRepositoryMock.Object,
            operationTypeRepositoryMock.Object,
            portfolioLocatorMock.Object,
            closingValidatorMock.Object,
            trustInfoProviderMock.Object,
            voidsOperMock.Object,
            loggerMock.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(ruleError.Code);
        result.Error.Description.Should().Be(ruleError.Message);
    }

    [Fact]
    public async Task Handle_WhenNoValidOperationsFound_ReturnsSummaryWithoutExecutingOper()
    {
        const long clientOperationId = 1001;
        const decimal amount = 500m;
        const int causeId = 10;
        const int affiliateId = 15;
        const int objectiveId = 20;
        const int portfolioId = 30;
        const long contributionTypeId = 5;

        var command = new VoidedTransactionsValCommand(
            new[] { new VoidedTransactionItem(clientOperationId, amount) },
            causeId,
            affiliateId,
            objectiveId);

        var ruleEvaluatorMock = new Mock<IInternalRuleEvaluator<OperationsModuleMarker>>();
        ruleEvaluatorMock
            .Setup(evaluator => evaluator.EvaluateAsync(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(SuccessfulRules);

        const string causeScope = "Operations.Voids.Cause";
        var configurationParameter = ConfigurationParameter.Create("Causal", causeId.ToString(), causeScope);
        SetProperty(configurationParameter, nameof(ConfigurationParameter.ConfigurationParameterId), causeId);

        var configurationParameterRepositoryMock = new Mock<IConfigurationParameterRepository>();
        configurationParameterRepositoryMock
            .Setup(repository => repository.GetByIdAsync(
                causeId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(configurationParameter);

        var contributionType = CreateOperationType(contributionTypeId, "Aporte", null);
        var operationTypeRepositoryMock = new Mock<IOperationTypeRepository>();
        operationTypeRepositoryMock
            .Setup(repository => repository.GetByNameAsync("Aporte", It.IsAny<CancellationToken>()))
            .ReturnsAsync(contributionType);
        operationTypeRepositoryMock
            .Setup(repository => repository.GetByIdAsync(contributionTypeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contributionType);

        var clientOperation = CreateClientOperation(
            clientOperationId,
            portfolioId,
            contributionTypeId,
            amount,
            processDate: new DateTime(2024, 01, 02, 0, 0, 0, DateTimeKind.Utc));

        var clientOperationRepositoryMock = new Mock<IClientOperationRepository>();
        clientOperationRepositoryMock
            .Setup(repository => repository.GetAsync(clientOperationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(clientOperation);

        var portfolioCurrentDate = new DateTime(2024, 01, 01, 0, 0, 0, DateTimeKind.Utc);
        var portfolioLocatorMock = new Mock<IPortfolioLocator>();
        portfolioLocatorMock
            .Setup(locator => locator.FindByPortfolioIdAsync(portfolioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success((PortfolioId: (long)portfolioId, Name: "P", CurrentDate: portfolioCurrentDate)));

        var closingValidatorMock = new Mock<IClosingValidator>();
        closingValidatorMock
            .Setup(validator => validator.IsClosingActiveAsync(portfolioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var trustError = Error.Validation("TRUST.INVALID", "La validación de fideicomiso falló");
        var trustInfoProviderMock = new Mock<ITrustInfoProvider>();
        trustInfoProviderMock
            .Setup(provider => provider.GetAsync(clientOperationId, amount, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<TrustInfoResult>(trustError));

        var voidsOperMock = new Mock<IVoidsOper>(MockBehavior.Strict);
        var loggerMock = new Mock<ILogger<VoidedTransactionsValCommandHandler>>();

        var handler = new VoidedTransactionsValCommandHandler(
            ruleEvaluatorMock.Object,
            clientOperationRepositoryMock.Object,
            configurationParameterRepositoryMock.Object,
            operationTypeRepositoryMock.Object,
            portfolioLocatorMock.Object,
            closingValidatorMock.Object,
            trustInfoProviderMock.Object,
            voidsOperMock.Object,
            loggerMock.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.VoidIds.Should().BeEmpty();
        result.Value.Message.Should().BeEmpty();
        result.Value.FailedOperations.Should().ContainSingle(failure =>
            failure.ClientOperationId == clientOperationId &&
            failure.Code == trustError.Code &&
            failure.Message == trustError.Description);
        voidsOperMock.Verify(
            oper => oper.ExecuteAsync(It.IsAny<VoidedTransactionsOperRequest>(), It.IsAny<VoidedTransactionsValidationResult>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenVoidsOperFails_ReturnsFailure()
    {
        const long clientOperationId = 2001;
        const decimal amount = 1_500m;
        const int causeId = 11;
        const int affiliateId = 21;
        const int objectiveId = 31;
        const int portfolioId = 41;
        const long contributionTypeId = 6;
        const long trustId = 501;

        var command = new VoidedTransactionsValCommand(
            new[] { new VoidedTransactionItem(clientOperationId, amount) },
            causeId,
            affiliateId,
            objectiveId);

        var ruleEvaluatorMock = new Mock<IInternalRuleEvaluator<OperationsModuleMarker>>();
        ruleEvaluatorMock
            .Setup(evaluator => evaluator.EvaluateAsync(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(SuccessfulRules);

        const string causeScope = "Operations.Voids.Cause";
        var configurationParameter = ConfigurationParameter.Create("Causal", causeId.ToString(), causeScope);
        SetProperty(configurationParameter, nameof(ConfigurationParameter.ConfigurationParameterId), causeId);

        var configurationParameterRepositoryMock = new Mock<IConfigurationParameterRepository>();
        configurationParameterRepositoryMock
            .Setup(repository => repository.GetByIdAsync(
                causeId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(configurationParameter);

        var contributionType = CreateOperationType(contributionTypeId, "Aporte", null);
        var operationTypeRepositoryMock = new Mock<IOperationTypeRepository>();
        operationTypeRepositoryMock
            .Setup(repository => repository.GetByNameAsync("Aporte", It.IsAny<CancellationToken>()))
            .ReturnsAsync(contributionType);
        operationTypeRepositoryMock
            .Setup(repository => repository.GetByIdAsync(contributionTypeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contributionType);

        var clientOperation = CreateClientOperation(
            clientOperationId,
            portfolioId,
            contributionTypeId,
            amount,
            processDate: new DateTime(2024, 02, 02, 0, 0, 0, DateTimeKind.Utc));

        var clientOperationRepositoryMock = new Mock<IClientOperationRepository>();
        clientOperationRepositoryMock
            .Setup(repository => repository.GetAsync(clientOperationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(clientOperation);

        var portfolioCurrentDate = new DateTime(2024, 02, 01, 0, 0, 0, DateTimeKind.Utc);
        var portfolioLocatorMock = new Mock<IPortfolioLocator>();
        portfolioLocatorMock
            .Setup(locator => locator.FindByPortfolioIdAsync(portfolioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success((PortfolioId: (long)portfolioId, Name: "Portfolio", CurrentDate: portfolioCurrentDate)));

        var closingValidatorMock = new Mock<IClosingValidator>();
        closingValidatorMock
            .Setup(validator => validator.IsClosingActiveAsync(portfolioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var trustInfoProviderMock = new Mock<ITrustInfoProvider>();
        trustInfoProviderMock
            .Setup(provider => provider.GetAsync(clientOperationId, amount, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new TrustInfoResult(trustId)));

        var voidsError = Error.Validation("OPS.VOID.ERROR", "No fue posible anular las operaciones");
        var voidsOperMock = new Mock<IVoidsOper>();
        voidsOperMock
            .Setup(oper => oper.ExecuteAsync(
                It.IsAny<VoidedTransactionsOperRequest>(),
                It.IsAny<VoidedTransactionsValidationResult>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<VoidedTransactionsOperResult>(voidsError));

        var loggerMock = new Mock<ILogger<VoidedTransactionsValCommandHandler>>();

        var handler = new VoidedTransactionsValCommandHandler(
            ruleEvaluatorMock.Object,
            clientOperationRepositoryMock.Object,
            configurationParameterRepositoryMock.Object,
            operationTypeRepositoryMock.Object,
            portfolioLocatorMock.Object,
            closingValidatorMock.Object,
            trustInfoProviderMock.Object,
            voidsOperMock.Object,
            loggerMock.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(voidsError.Code);
        result.Error.Description.Should().Be(voidsError.Description);
        voidsOperMock.Verify(oper => oper.ExecuteAsync(
                It.Is<VoidedTransactionsOperRequest>(request =>
                    request.AffiliateId == affiliateId && request.ObjectiveId == objectiveId),
                It.Is<VoidedTransactionsValidationResult>(summary =>
                    summary.ValidOperations.Count == 1 &&
                    summary.ValidOperations.Single().OriginalOperation.ClientOperationId == clientOperationId &&
                    summary.ValidOperations.Single().TrustId == trustId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static OperationType CreateOperationType(long id, string name, int? categoryId)
    {
        var operationType = OperationType
            .Create(
                name,
                categoryId,
                IncomeEgressNature.Income,
                Status.Active,
                string.Empty,
                true,
                JsonDocument.Parse("{}"),
                string.Empty)
            .Value;

        SetProperty(operationType, nameof(OperationType.OperationTypeId), id);
        return operationType;
    }

    private static ClientOperation CreateClientOperation(
        long id,
        int portfolioId,
        long operationTypeId,
        decimal amount,
        DateTime processDate)
    {
        var operation = ClientOperation
            .Create(
                DateTime.UtcNow,
                affiliateId: 1,
                objectiveId: 1,
                portfolioId,
                amount,
                processDate,
                operationTypeId,
                DateTime.UtcNow,
                LifecycleStatus.Active)
            .Value;

        SetProperty(operation, nameof(ClientOperation.ClientOperationId), id);
        return operation;
    }

    private static void SetProperty<T, TValue>(T target, string propertyName, TValue value)
    {
        typeof(T)
            .GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
            .SetValue(target, value);
    }
}
