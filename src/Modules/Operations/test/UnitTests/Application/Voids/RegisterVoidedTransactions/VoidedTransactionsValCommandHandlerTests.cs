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
}
