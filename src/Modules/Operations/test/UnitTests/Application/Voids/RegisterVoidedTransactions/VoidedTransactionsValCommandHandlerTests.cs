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
            processDate: new DateTime(2024, 01, 02, 0, 0, 0, DateTimeKind.Utc),
            affiliateId,
            objectiveId);

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
        result.Value.Message.Should().Be(
            "Lo sentimos, ninguna de las operaciones seleccionadas pudo ser anulada. Revisa el detalle e inténtalo nuevamente.");
        result.Value.FailedOperations.Should().ContainSingle(failure =>
            failure.ClientOperationId == clientOperationId &&
            failure.Code == trustError.Code &&
            failure.Message == trustError.Description);
        result.Value.TotalProcessed.Should().Be(command.Items.Count);
        result.Value.SuccessCount.Should().Be(0);
        result.Value.ErrorCount.Should().Be(result.Value.FailedOperations.Count);
        voidsOperMock.Verify(
            oper => oper.ExecuteAsync(It.IsAny<VoidedTransactionsOperRequest>(), It.IsAny<VoidedTransactionsValidationResult>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenOperationDoesNotMatchAffiliateOrObjective_TreatsOperationAsMissing()
    {
        const long clientOperationId = 1101;
        const decimal amount = 750m;
        const int causeId = 14;
        const int affiliateId = 18;
        const int objectiveId = 28;
        const int portfolioId = 38;
        const long contributionTypeId = 8;

        var command = new VoidedTransactionsValCommand(
            new[] { new VoidedTransactionItem(clientOperationId, amount) },
            causeId,
            affiliateId,
            objectiveId);

        var ruleEvaluatorMock = new Mock<IInternalRuleEvaluator<OperationsModuleMarker>>();
        ruleEvaluatorMock
            .Setup(evaluator => evaluator.EvaluateAsync(
                "Operations.Voids.RequiredFields",
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(SuccessfulRules);

        var validationError = new RuleValidationError(
            "OPS.VOIDS.OPERATION.NOT_FOUND",
            "La operación no pertenece al afiliado u objetivo indicado.");

        ruleEvaluatorMock
            .Setup(evaluator => evaluator.EvaluateAsync(
                "Operations.Voids.Validation",
                It.Is<object>(context => HasBooleanPropertyWithValue(context, "OperationExists", false)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, Array.Empty<RuleResultTree>(), new[] { validationError }));

        ruleEvaluatorMock
            .Setup(evaluator => evaluator.EvaluateAsync(
                "Operations.Voids.Validation",
                It.Is<object>(context => HasBooleanPropertyWithValue(context, "OperationExists", true)),
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

        var mismatchedOperation = CreateClientOperation(
            clientOperationId,
            portfolioId,
            contributionTypeId,
            amount,
            processDate: new DateTime(2024, 02, 15, 0, 0, 0, DateTimeKind.Utc),
            affiliateId: affiliateId + 1,
            objectiveId: objectiveId + 1);

        var clientOperationRepositoryMock = new Mock<IClientOperationRepository>();
        clientOperationRepositoryMock
            .Setup(repository => repository.GetAsync(clientOperationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mismatchedOperation);

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

        result.IsSuccess.Should().BeTrue();
        result.Value.VoidIds.Should().BeEmpty();
        result.Value.Message.Should().Be(
            "Lo sentimos, ninguna de las operaciones seleccionadas pudo ser anulada. Revisa el detalle e inténtalo nuevamente.");
        result.Value.FailedOperations.Should().ContainSingle(failure =>
            failure.ClientOperationId == clientOperationId &&
            failure.Code == validationError.Code &&
            failure.Message == validationError.Message);
        result.Value.TotalProcessed.Should().Be(command.Items.Count);
        result.Value.SuccessCount.Should().Be(0);
        result.Value.ErrorCount.Should().Be(result.Value.FailedOperations.Count);
    }

    [Fact]
    public async Task Handle_WhenVoidsOperSucceeds_ReturnsSummaryWithCounts()
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
            processDate: new DateTime(2024, 02, 02, 0, 0, 0, DateTimeKind.Utc),
            affiliateId,
            objectiveId);

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

        var expectedVoidIds = new[] { clientOperationId };
        var voidsOperMock = new Mock<IVoidsOper>();
        voidsOperMock
            .Setup(oper => oper.ExecuteAsync(
                It.IsAny<VoidedTransactionsOperRequest>(),
                It.IsAny<VoidedTransactionsValidationResult>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new VoidedTransactionsOperResult(expectedVoidIds, "OK")));

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
        result.Value.VoidIds.Should().BeEquivalentTo(expectedVoidIds);
        result.Value.Message.Should().Be(
            "Genial!, Se ha realizado exitosamente la anulación de todas las operaciones seleccionadas.");
        result.Value.TotalProcessed.Should().Be(command.Items.Count);
        result.Value.SuccessCount.Should().Be(expectedVoidIds.Length);
        result.Value.ErrorCount.Should().Be(result.Value.FailedOperations.Count);
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

    [Fact]
    public async Task Handle_WhenSomeOperationsAreAnnulled_ReturnsPartialSuccessMessage()
    {
        const long firstOperationId = 3001;
        const long secondOperationId = 3002;
        const decimal firstAmount = 2_000m;
        const decimal secondAmount = 3_000m;
        const int causeId = 12;
        const int affiliateId = 22;
        const int objectiveId = 32;
        const int portfolioId = 42;
        const long contributionTypeId = 7;
        const long trustId = 601;

        var command = new VoidedTransactionsValCommand(
            new[]
            {
                new VoidedTransactionItem(firstOperationId, firstAmount),
                new VoidedTransactionItem(secondOperationId, secondAmount)
            },
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

        var firstOperation = CreateClientOperation(
            firstOperationId,
            portfolioId,
            contributionTypeId,
            firstAmount,
            processDate: new DateTime(2024, 03, 02, 0, 0, 0, DateTimeKind.Utc),
            affiliateId,
            objectiveId);

        var secondOperation = CreateClientOperation(
            secondOperationId,
            portfolioId,
            contributionTypeId,
            secondAmount,
            processDate: new DateTime(2024, 03, 02, 0, 0, 0, DateTimeKind.Utc),
            affiliateId,
            objectiveId);

        var clientOperationRepositoryMock = new Mock<IClientOperationRepository>();
        clientOperationRepositoryMock
            .Setup(repository => repository.GetAsync(firstOperationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(firstOperation);
        clientOperationRepositoryMock
            .Setup(repository => repository.GetAsync(secondOperationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(secondOperation);

        var portfolioCurrentDate = new DateTime(2024, 03, 01, 0, 0, 0, DateTimeKind.Utc);
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
            .Setup(provider => provider.GetAsync(firstOperationId, firstAmount, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new TrustInfoResult(trustId)));

        var trustError = Error.Validation("TRUST.ERROR", "La validación de fideicomiso falló");
        trustInfoProviderMock
            .Setup(provider => provider.GetAsync(secondOperationId, secondAmount, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<TrustInfoResult>(trustError));

        var expectedVoidIds = new[] { firstOperationId };
        var voidsOperMock = new Mock<IVoidsOper>();
        voidsOperMock
            .Setup(oper => oper.ExecuteAsync(
                It.Is<VoidedTransactionsOperRequest>(request =>
                    request.AffiliateId == affiliateId && request.ObjectiveId == objectiveId),
                It.Is<VoidedTransactionsValidationResult>(summary =>
                    summary.ValidOperations.Count == 1 &&
                    summary.ValidOperations.Single().OriginalOperation.ClientOperationId == firstOperationId &&
                    summary.FailedOperations.Count == 1 &&
                    summary.FailedOperations.Single().ClientOperationId == secondOperationId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new VoidedTransactionsOperResult(expectedVoidIds, "OK")));

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
        result.Value.Message.Should().Be(
            "Atención: Solo algunas de las operaciones seleccionadas fueron anuladas. Revisa el detalle para conocer cuáles siguen pendientes.");
        result.Value.SuccessCount.Should().Be(1);
        result.Value.ErrorCount.Should().Be(1);
        result.Value.FailedOperations.Should().ContainSingle(failure =>
            failure.ClientOperationId == secondOperationId &&
            failure.Code == trustError.Code &&
            failure.Message == trustError.Description);
        result.Value.VoidIds.Should().BeEquivalentTo(expectedVoidIds);
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
            processDate: new DateTime(2024, 02, 02, 0, 0, 0, DateTimeKind.Utc),
            affiliateId,
            objectiveId);

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

    private static bool HasBooleanPropertyWithValue(object context, string propertyName, bool expectedValue)
    {
        var property = context
            .GetType()
            .GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (property is null || property.PropertyType != typeof(bool))
        {
            return false;
        }

        return (bool)property.GetValue(context)! == expectedValue;
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
        DateTime processDate,
        int affiliateId,
        int objectiveId)
    {
        var operation = ClientOperation
            .Create(
                DateTime.UtcNow,
                affiliateId,
                objectiveId,
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
