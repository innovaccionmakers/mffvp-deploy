using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Application.Rules;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;
using Operations.Application.Abstractions;
using Operations.Application.Abstractions.External;
using Operations.Application.Abstractions.Services.Closing;
using Operations.Application.Abstractions.Services.Voids;
using Operations.Domain.ClientOperations;
using Operations.Domain.ConfigurationParameters;
using Operations.Domain.OperationTypes;
using Operations.Integrations.Voids.RegisterVoidedTransactions;

namespace Operations.Application.Voids.RegisterVoidedTransactions;

internal sealed class VoidedTransactionsValCommandHandler(
    IInternalRuleEvaluator<OperationsModuleMarker> ruleEvaluator,
    IClientOperationRepository clientOperationRepository,
    IConfigurationParameterRepository configurationParameterRepository,
    IOperationTypeRepository operationTypeRepository,
    IPortfolioLocator portfolioLocator,
    IClosingValidator closingValidator,
    ITrustInfoProvider trustInfoProvider,
    IVoidsOper voidsOper,
    ILogger<VoidedTransactionsValCommandHandler> logger)
    : ICommandHandler<VoidedTransactionsValCommand, VoidedTransactionsValResult>
{
    private const string RequiredWorkflow = "Operations.Voids.RequiredFields";
    private const string ValidationWorkflow = "Operations.Voids.Validation";
    private const string ContributionOperationName = "Aporte";

    public async Task<Result<VoidedTransactionsValResult>> Handle(
        VoidedTransactionsValCommand command,
        CancellationToken cancellationToken)
    {
        var validationResult = await ValidateAsync(command, cancellationToken);

        if (validationResult.IsFailure)
        {
            return Result.Failure<VoidedTransactionsValResult>(validationResult.Error);
        }

        var summary = validationResult.Value;

        if (summary.ValidOperations.Count == 0)
        {
            return Result.Success(new VoidedTransactionsValResult(
                Array.Empty<long>(),
                string.Empty,
                summary.FailedOperations));
        }

        var request = new VoidedTransactionsOperRequest(command.AffiliateId, command.ObjectiveId);

        var operationResult = await voidsOper.ExecuteAsync(request, summary, cancellationToken);

        if (operationResult.IsFailure)
        {
            return Result.Failure<VoidedTransactionsValResult>(operationResult.Error);
        }

        var response = operationResult.Value;

        return Result.Success(new VoidedTransactionsValResult(
            response.VoidIds,
            response.Message,
            summary.FailedOperations));
    }

    private async Task<Result<VoidedTransactionsValidationResult>> ValidateAsync(
        VoidedTransactionsValCommand command,
        CancellationToken cancellationToken)
    {
        var items = command.Items.ToArray();

        var operationsAreUnique = items
            .Select(item => item.ClientOperationId)
            .Distinct()
            .Count() == items.Length;

        var requiredContext = new
        {
            command.CauseId,
            command.AffiliateId,
            command.ObjectiveId,
            HasItems = items.Length > 0,
            OperationsProvided = items.All(item => item.ClientOperationId > 0),
            AmountsProvided = items.All(item => item.Amount > 0),
            OperationsAreUnique = operationsAreUnique
        };

        var (requiredValid, _, requiredErrors) = await ruleEvaluator
            .EvaluateAsync(RequiredWorkflow, requiredContext, cancellationToken);

        if (!requiredValid)
        {
            var requiredError = requiredErrors.First();

            logger.LogWarning(
                "{Service} - Validación de campos requerida falló: {Code} - {Message}",
                nameof(VoidedTransactionsValCommandHandler),
                requiredError.Code,
                requiredError.Message);

            return Result.Failure<VoidedTransactionsValidationResult>(
                Error.Validation(requiredError.Code, requiredError.Message));
        }

        var causeTask = configurationParameterRepository
            .GetByIdAsync(command.CauseId, cancellationToken);

        var contributionTypeTask = await operationTypeRepository
            .GetByNameAsync(ContributionOperationName, cancellationToken);

        var operationTasks = items
            .Select(item => clientOperationRepository.GetAsync(item.ClientOperationId, cancellationToken))
            .ToArray();

        var operations = await Task.WhenAll(operationTasks);
        var causeConfigurationParameter = await causeTask;
        var contributionType = contributionTypeTask.FirstOrDefault();

        var causeExists = causeConfigurationParameter is not null;
        var contributionTypeExists = contributionType is not null;

        var operationTypesTasks = new Dictionary<long, Task<OperationType?>>();

        foreach (var operation in operations)
        {
            if (operation is null)
            {
                continue;
            }

            if (operationTypesTasks.ContainsKey(operation.OperationTypeId))
            {
                continue;
            }

            operationTypesTasks[operation.OperationTypeId] = operationTypeRepository
                .GetByIdAsync(operation.OperationTypeId, cancellationToken);
        }

        await Task.WhenAll(operationTypesTasks.Values);

        var operationTypes = operationTypesTasks.ToDictionary(
            pair => pair.Key,
            pair => pair.Value.Result);

        var portfolioSnapshots = new Dictionary<int, PortfolioSnapshot>();

        async Task<PortfolioSnapshot> GetPortfolioSnapshotAsync(int portfolioId)
        {
            if (portfolioSnapshots.TryGetValue(portfolioId, out var cachedSnapshot))
            {
                return cachedSnapshot;
            }

            var portfolioResult = await portfolioLocator
                .FindByPortfolioIdAsync(portfolioId, cancellationToken);

            if (!portfolioResult.IsSuccess)
            {
                var notFound = PortfolioSnapshot.NotFound();
                portfolioSnapshots[portfolioId] = notFound;
                return notFound;
            }

            var currentDate = DateTime.SpecifyKind(
                portfolioResult.Value.CurrentDate,
                DateTimeKind.Utc);

            var closingAvailable = !await closingValidator
                .IsClosingActiveAsync(portfolioId, cancellationToken);

            var snapshot = PortfolioSnapshot.Create(currentDate, closingAvailable);
            portfolioSnapshots[portfolioId] = snapshot;
            return snapshot;
        }

        var failedOperations = new List<VoidedTransactionFailure>();
        var validOperations = new List<VoidedTransactionReady>();

        for (var index = 0; index < items.Length; index++)
        {
            var item = items[index];
            var operation = operations[index];

            var operationExists = operation is not null;
            var operationType = operationExists &&
                                operationTypes.TryGetValue(operation!.OperationTypeId, out var type)
                                    ? type
                                    : null;

            var operationIsContribution = operationExists &&
                operationType is not null &&
                contributionTypeExists &&
                (operationType!.OperationTypeId == contributionType!.OperationTypeId ||
                 (operationType.CategoryId.HasValue &&
                  contributionType.OperationTypeId == operationType.CategoryId.Value));

            var shouldEvaluateAdditionalChecks = operationExists &&
                operationType is not null &&
                contributionTypeExists &&
                operationIsContribution;

            var portfolioFound = false;
            var closingAvailable = false;
            var processDateIsValid = false;
            DateTime portfolioCurrentDate = default;

            if (shouldEvaluateAdditionalChecks)
            {
                var snapshot = await GetPortfolioSnapshotAsync(operation!.PortfolioId);

                portfolioFound = snapshot.Exists;
                closingAvailable = snapshot.ClosingAvailable;

                if (snapshot.Exists)
                {
                    portfolioCurrentDate = snapshot.CurrentDate;
                    processDateIsValid = operation.ProcessDate.Date == snapshot.ProcessDate.Date;
                }
            }

            var validationContext = new
            {
                OperationExists = operationExists,
                ContributionTypeExists = contributionTypeExists,
                OperationIsContribution = operationIsContribution,
                PortfolioFound = portfolioFound,
                ClosingAvailable = closingAvailable,
                ProcessDateIsValid = processDateIsValid,
                OperationIsActive = operation?.Status == LifecycleStatus.Active,
                CauseExists = causeExists
            };

            var (isValid, _, validationErrors) = await ruleEvaluator
                .EvaluateAsync(ValidationWorkflow, validationContext, cancellationToken);

            if (!isValid)
            {
                var validationError = validationErrors.First();

                logger.LogWarning(
                    "{Service} - Validación fallida para operación {OperationId}: {Code} - {Message}",
                    nameof(VoidedTransactionsValCommandHandler),
                    item.ClientOperationId,
                    validationError.Code,
                    validationError.Message);

                failedOperations.Add(new VoidedTransactionFailure(
                    item.ClientOperationId,
                    validationError.Code,
                    validationError.Message));

                continue;
            }

            var trustInfoResult = await trustInfoProvider
                .GetAsync(operation!.ClientOperationId, item.Amount, cancellationToken);

            if (trustInfoResult.IsFailure)
            {
                var trustError = trustInfoResult.Error;

                logger.LogWarning(
                    "{Service} - Validación de fideicomiso falló para operación {OperationId}: {Code} - {Message}",
                    nameof(VoidedTransactionsValCommandHandler),
                    item.ClientOperationId,
                    trustError.Code,
                    trustError.Description);

                failedOperations.Add(new VoidedTransactionFailure(
                    item.ClientOperationId,
                    trustError.Code,
                    trustError.Description));

                continue;
            }

            var trustId = operation.TrustId ?? trustInfoResult.Value.TrustId;

            validOperations.Add(new VoidedTransactionReady(
                operation,
                item.Amount,
                portfolioCurrentDate,
                trustId));
        }

        return Result.Success(new VoidedTransactionsValidationResult(
            validOperations,
            failedOperations,
            causeConfigurationParameter?.ConfigurationParameterId ?? 0));
    }

    private sealed record PortfolioSnapshot(bool Exists, DateTime CurrentDate, bool ClosingAvailable)
    {
        public DateTime ProcessDate => DateTime.SpecifyKind(CurrentDate.AddDays(1), DateTimeKind.Utc);

        public static PortfolioSnapshot Create(DateTime currentDate, bool closingAvailable) =>
            new(true, currentDate, closingAvailable);

        public static PortfolioSnapshot NotFound() => new(false, default, false);
    }
}
