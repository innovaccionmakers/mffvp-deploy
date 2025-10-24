using System.Globalization;
using System.Reflection;
using System.Text.Json;
using Common.SharedKernel.Application.Attributes;
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
using Operations.Application.Abstractions.Services.AccountingRecords;
using Operations.Application.Abstractions.Services.Closing;
using Operations.Application.AccountingRecords.CreateDebitNote;
using Operations.Domain.ClientOperations;
using Operations.Domain.ConfigurationParameters;
using Operations.Domain.OperationTypes;
using Operations.Integrations.AccountingRecords.CreateDebitNote;
using RulesEngine.Models;

namespace Operations.test.UnitTests.Application.AccountingRecords.CreateDebitNote;

public class AccountingRecordsValCommandHandlerTests
{
    private static readonly (bool Success, IReadOnlyCollection<RuleResultTree> Results, IReadOnlyCollection<RuleValidationError> Errors) SuccessfulRules =
        (true, Array.Empty<RuleResultTree>(), Array.Empty<RuleValidationError>());

    [Fact]
    public async Task Handle_WithExistingTrustId_WhenTrustValidationFails_ReturnsFailure()
    {
        const long clientOperationId = 12;
        const decimal amount = 1_000m;
        const int causeHomologationCode = 1;
        const int causeConfigurationParameterId = 10;
        const long contributionTypeId = 1;
        const long contributionSubtypeId = 2;
        const long debitNoteTypeId = 3;

        var contributionType = CreateOperationType(contributionTypeId, "Aporte", null);
        var contributionsSubtype = CreateOperationType(contributionSubtypeId, "Aporte Débito Automático", (int)contributionTypeId);
        var contributionSubtype = contributionsSubtype.FirstOrDefault();
        var debitNoteType = CreateOperationType(debitNoteTypeId, "Nota Débito", null, IncomeEgressNature.Egress);

        var ruleEvaluatorMock = new Mock<IInternalRuleEvaluator<OperationsModuleMarker>>();
        ruleEvaluatorMock
            .Setup(evaluator => evaluator.EvaluateAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(SuccessfulRules);

        var causeScope = HomologScope.Of<AccountingRecordsValCommand>(c => c.CauseId);
        var causeConfigurationParameter = ConfigurationParameter.Create(
            name: "Causal Nota Débito",
            homologationCode: causeHomologationCode.ToString(CultureInfo.InvariantCulture),
            type: causeScope);

        typeof(ConfigurationParameter)
            .GetProperty(nameof(ConfigurationParameter.ConfigurationParameterId), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
            .SetValue(causeConfigurationParameter, causeConfigurationParameterId);

        var configurationParameterRepositoryMock = new Mock<IConfigurationParameterRepository>();
        configurationParameterRepositoryMock
            .Setup(repository => repository.GetByCodeAndScopeAsync(
                causeHomologationCode.ToString(CultureInfo.InvariantCulture),
                causeScope,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(causeConfigurationParameter);

        var clientOperation = ClientOperation
            .Create(
                DateTime.UtcNow,
                affiliateId: 10,
                objectiveId: 20,
                portfolioId: 30,
                amount,
                DateTime.UtcNow.AddDays(-2),
                operationTypeId: contributionSubtype.OperationTypeId,
                DateTime.UtcNow.AddDays(-1),
                LifecycleStatus.Active,
                trustId: 100)
            .Value;

        typeof(ClientOperation)
            .GetProperty(nameof(ClientOperation.ClientOperationId), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
            .SetValue(clientOperation, clientOperationId);

        var clientOperationRepositoryMock = new Mock<IClientOperationRepository>();
        clientOperationRepositoryMock
            .Setup(repository => repository.GetAsync(clientOperationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(clientOperation);

        var operationTypeRepositoryMock = new Mock<IOperationTypeRepository>();
        operationTypeRepositoryMock
            .Setup(repository => repository.GetByIdAsync(contributionSubtype.OperationTypeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contributionSubtype);
        operationTypeRepositoryMock
            .Setup(repository => repository.GetByNameAsync("Aporte", It.IsAny<CancellationToken>()))
            .ReturnsAsync(contributionType);
        operationTypeRepositoryMock
            .Setup(repository => repository.GetByNameAsync("Nota Débito", It.IsAny<CancellationToken>()))
            .ReturnsAsync(debitNoteType);

        var portfolioLocatorMock = new Mock<IPortfolioLocator>();
        var portfolioCurrentDate = DateTime.UtcNow.Date;
        portfolioLocatorMock
            .Setup(locator => locator.FindByPortfolioIdAsync(clientOperation.PortfolioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success((PortfolioId: (long)clientOperation.PortfolioId, Name: "Portfolio", CurrentDate: portfolioCurrentDate)));

        var closingValidatorMock = new Mock<IClosingValidator>();
        closingValidatorMock
            .Setup(validator => validator.IsClosingActiveAsync(clientOperation.PortfolioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var trustValidationError = Error.Validation("Trust.Mismatch", "Valor diferente al saldo del fideicomiso");
        var trustInfoProviderMock = new Mock<ITrustInfoProvider>();
        trustInfoProviderMock
            .Setup(provider => provider.GetAsync(clientOperationId, amount, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<TrustInfoResult>(trustValidationError));

        var accountingRecordsOperMock = new Mock<IAccountingRecordsOper>(MockBehavior.Strict);
        var loggerMock = new Mock<ILogger<AccountingRecordsValCommandHandler>>();

        var handler = new AccountingRecordsValCommandHandler(
            ruleEvaluatorMock.Object,
            clientOperationRepositoryMock.Object,
            configurationParameterRepositoryMock.Object,
            operationTypeRepositoryMock.Object,
            portfolioLocatorMock.Object,
            closingValidatorMock.Object,
            trustInfoProviderMock.Object,
            accountingRecordsOperMock.Object,
            loggerMock.Object);

        var command = new AccountingRecordsValCommand(
            clientOperationId,
            amount,
            causeHomologationCode,
            2,
            3);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        ((object)result.Error).Should().BeEquivalentTo(trustValidationError);
        trustInfoProviderMock.Verify(provider => provider.GetAsync(clientOperationId, amount, It.IsAny<CancellationToken>()), Times.Once);
        accountingRecordsOperMock.Verify(
            service => service.ExecuteAsync(It.IsAny<AccountingRecordsOperRequest>(), It.IsAny<AccountingRecordsValidationResult>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenCauseConfigurationParameterNotFound_ReturnsFailure()
    {
        const long clientOperationId = 25;
        const decimal amount = 500m;
        const int causeHomologationCode = 99;
        const long contributionTypeId = 10;
        const long contributionSubtypeId = 11;
        const long debitNoteTypeId = 12;

        var contributionType = CreateOperationType(contributionTypeId, "Aporte", null);
        var contributionsSubtype = CreateOperationType(contributionSubtypeId, "Aporte Nómina", (int)contributionTypeId);
        var contributionSubtype = contributionsSubtype.FirstOrDefault();
        var debitNoteType = CreateOperationType(debitNoteTypeId, "Nota Débito", null, IncomeEgressNature.Egress);

        var causeScope = HomologScope.Of<AccountingRecordsValCommand>(c => c.CauseId);

        var ruleEvaluatorMock = new Mock<IInternalRuleEvaluator<OperationsModuleMarker>>();
        ruleEvaluatorMock
            .Setup(evaluator => evaluator.EvaluateAsync(
                "Operations.DebitNote.RequiredFields",
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(SuccessfulRules);

        var validationError = new RuleValidationError(
            "OPS_ACC_CAUSE_NOT_FOUND",
            "No es posible registrar la nota débito: la causal no está configurada.");

        ruleEvaluatorMock
            .Setup(evaluator => evaluator.EvaluateAsync(
                "Operations.DebitNote.Validation",
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, Array.Empty<RuleResultTree>(), new[] { validationError }));

        var configurationParameterRepositoryMock = new Mock<IConfigurationParameterRepository>();
        configurationParameterRepositoryMock
            .Setup(repository => repository.GetByCodeAndScopeAsync(
                causeHomologationCode.ToString(CultureInfo.InvariantCulture),
                causeScope,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((ConfigurationParameter?)null);

        var clientOperation = ClientOperation
            .Create(
                DateTime.UtcNow,
                affiliateId: 30,
                objectiveId: 40,
                portfolioId: 50,
                amount,
                DateTime.UtcNow.AddDays(-2),
                operationTypeId: contributionSubtype.OperationTypeId,
                DateTime.UtcNow.AddDays(-1),
                LifecycleStatus.Active,
                trustId: null)
            .Value;

        typeof(ClientOperation)
            .GetProperty(nameof(ClientOperation.ClientOperationId), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
            .SetValue(clientOperation, clientOperationId);

        var clientOperationRepositoryMock = new Mock<IClientOperationRepository>();
        clientOperationRepositoryMock
            .Setup(repository => repository.GetAsync(clientOperationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(clientOperation);

        var operationTypeRepositoryMock = new Mock<IOperationTypeRepository>();
        operationTypeRepositoryMock
            .Setup(repository => repository.GetByIdAsync(contributionSubtype.OperationTypeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contributionSubtype);
        operationTypeRepositoryMock
            .Setup(repository => repository.GetByNameAsync("Aporte", It.IsAny<CancellationToken>()))
            .ReturnsAsync(contributionType);
        operationTypeRepositoryMock
            .Setup(repository => repository.GetByNameAsync("Nota Débito", It.IsAny<CancellationToken>()))
            .ReturnsAsync(debitNoteType);

        var portfolioLocatorMock = new Mock<IPortfolioLocator>();
        var portfolioCurrentDate = DateTime.UtcNow.Date;
        portfolioLocatorMock
            .Setup(locator => locator.FindByPortfolioIdAsync(clientOperation.PortfolioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success((PortfolioId: (long)clientOperation.PortfolioId, Name: "Portfolio", CurrentDate: portfolioCurrentDate)));

        var closingValidatorMock = new Mock<IClosingValidator>();
        closingValidatorMock
            .Setup(validator => validator.IsClosingActiveAsync(clientOperation.PortfolioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var trustInfoProviderMock = new Mock<ITrustInfoProvider>();
        trustInfoProviderMock
            .Setup(provider => provider.GetAsync(clientOperationId, amount, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new TrustInfoResult(1000)));

        var accountingRecordsOperMock = new Mock<IAccountingRecordsOper>(MockBehavior.Strict);
        var loggerMock = new Mock<ILogger<AccountingRecordsValCommandHandler>>();

        var handler = new AccountingRecordsValCommandHandler(
            ruleEvaluatorMock.Object,
            clientOperationRepositoryMock.Object,
            configurationParameterRepositoryMock.Object,
            operationTypeRepositoryMock.Object,
            portfolioLocatorMock.Object,
            closingValidatorMock.Object,
            trustInfoProviderMock.Object,
            accountingRecordsOperMock.Object,
            loggerMock.Object);

        var command = new AccountingRecordsValCommand(
            clientOperationId,
            amount,
            causeHomologationCode,
            70,
            80);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(validationError.Code);
        result.Error.Description.Should().Be(validationError.Message);
        result.Error.Type.Should().Be(Common.SharedKernel.Core.Primitives.ErrorType.Validation);
        configurationParameterRepositoryMock.Verify(repository => repository.GetByCodeAndScopeAsync(
                causeHomologationCode.ToString(CultureInfo.InvariantCulture),
                causeScope,
                It.IsAny<CancellationToken>()),
            Times.Once);
        accountingRecordsOperMock.Verify(
            service => service.ExecuteAsync(It.IsAny<AccountingRecordsOperRequest>(), It.IsAny<AccountingRecordsValidationResult>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static List<OperationType> CreateOperationType(
        long id,
        string name,
        int? categoryId,
        IncomeEgressNature nature = IncomeEgressNature.Income,
        Status status = Status.Active,
        bool visible = true)
    {
        var listOfOperationType = new List<OperationType>();
        var operationType = OperationType.Create(
            name,
            categoryId,
            nature,
            status,
            external: string.Empty,
            visible,
            additionalAttributes: JsonDocument.Parse("{}"),
            homologatedCode: name).Value;

        typeof(OperationType)
            .GetProperty(nameof(OperationType.OperationTypeId), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
            .SetValue(operationType, id);
        listOfOperationType.Add(operationType);

        return listOfOperationType;
    }

    private static T GetPropertyValue<T>(object target, string propertyName)
    {
        return (T)target.GetType().GetProperty(propertyName)!.GetValue(target)!;
    }
}
