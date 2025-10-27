using System.Globalization;
using System.Linq;
using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Application.Rules;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;
using Operations.Application.Abstractions;
using Operations.Application.Abstractions.External;
using Operations.Application.Abstractions.Services.AccountingRecords;
using Operations.Application.Abstractions.Services.Closing;
using Operations.Domain.ClientOperations;
using Operations.Domain.ConfigurationParameters;
using Operations.Domain.OperationTypes;
using Operations.Integrations.AccountingRecords.CreateDebitNote;

namespace Operations.Application.AccountingRecords.CreateDebitNote;

internal sealed class AccountingRecordsValCommandHandler(
    IInternalRuleEvaluator<OperationsModuleMarker> ruleEvaluator,
    IClientOperationRepository clientOperationRepository,
    IConfigurationParameterRepository configurationParameterRepository,
    IOperationTypeRepository operationTypeRepository,
    IPortfolioLocator portfolioLocator,
    IClosingValidator closingValidator,
    ITrustInfoProvider trustInfoProvider,
    IAccountingRecordsOper accountingRecordsOper,
    ILogger<AccountingRecordsValCommandHandler> logger)
    : ICommandHandler<AccountingRecordsValCommand, AccountingRecordsValResult>
{
    private const string RequiredWorkflow = "Operations.DebitNote.RequiredFields";
    private const string ValidationWorkflow = "Operations.DebitNote.Validation";
    private const string DebitNoteOperationName = "Nota Débito";
    private const string ContributionOperationName = "Aporte";

    public async Task<Result<AccountingRecordsValResult>> Handle(
        AccountingRecordsValCommand command,
        CancellationToken cancellationToken)
    {
        var validationResult = await ValidateAsync(command, cancellationToken);

        if (validationResult.IsFailure)
        {
            return Result.Failure<AccountingRecordsValResult>(validationResult.Error);
        }

        var accountingRecordsOperRequest = new AccountingRecordsOperRequest(
            command.Amount,
            validationResult.Value.CauseConfigurationParameterId,
            command.AffiliateId,
            command.ObjectiveId);

        var operationResult = await accountingRecordsOper.ExecuteAsync(
            accountingRecordsOperRequest,
            validationResult.Value,
            cancellationToken);

        if (operationResult.IsFailure)
        {
            return Result.Failure<AccountingRecordsValResult>(operationResult.Error);
        }

        var accountingRecordsOperResult = operationResult.Value;

        return Result.Success(new AccountingRecordsValResult(
            accountingRecordsOperResult.DebitNoteId,
            accountingRecordsOperResult.Message));
    }

    private async Task<Result<AccountingRecordsValidationResult>> ValidateAsync(
        AccountingRecordsValCommand command,
        CancellationToken cancellationToken)
    {
        var requiredContext = new
        {
            command.ClientOperationId,
            command.Amount,
            command.CauseId,
            command.AffiliateId,
            command.ObjectiveId
        };

        var (requiredValid, _, requiredErrors) = await ruleEvaluator
            .EvaluateAsync(RequiredWorkflow, requiredContext, cancellationToken);

        if (!requiredValid)
        {
            var requiredError = requiredErrors.First();
            logger.LogWarning(
                "{Service} - Validación de campos requerida falló: {Code} - {Message}",
                nameof(AccountingRecordsValCommandHandler),
                requiredError.Code,
                requiredError.Message);

            return Result.Failure<AccountingRecordsValidationResult>(
                Error.Validation(requiredError.Code, requiredError.Message));
        }

        var causeScope = HomologScope.Of<AccountingRecordsValCommand>(c => c.CauseId);
        var causeHomologationCode = command.CauseId.ToString(CultureInfo.InvariantCulture);

        var causeConfigurationParameter = await configurationParameterRepository
            .GetByCodeAndScopeAsync(causeHomologationCode, causeScope, cancellationToken);

        var operation = await clientOperationRepository
            .GetAsync(command.ClientOperationId, cancellationToken);

        var debitNotesType = await operationTypeRepository
            .GetByNameAsync(DebitNoteOperationName, cancellationToken);

        var debitNoteType = debitNotesType.FirstOrDefault();

        var (operationTypeExists, contributionTypeExists, operationIsContribution) =
            await EvaluateContributionOperationAsync(operation, cancellationToken);

        var shouldEvaluateAdditionalChecks =
            operation is not null &&
            operationTypeExists &&
            contributionTypeExists &&
            operationIsContribution;

        PortfolioEvaluation portfolioEvaluation;

        if (shouldEvaluateAdditionalChecks)
        {
            var portfolioEvaluationResult = await EvaluatePortfolioAsync(
                operation,
                command.Amount,
                cancellationToken);

            if (portfolioEvaluationResult.IsFailure)
            {
                return Result.Failure<AccountingRecordsValidationResult>(portfolioEvaluationResult.Error);
            }

            portfolioEvaluation = portfolioEvaluationResult.Value;
        }
        else
        {
            portfolioEvaluation = new PortfolioEvaluation(true, default, true, true, null);
        }

        var noPendingAnnulment = shouldEvaluateAdditionalChecks
            ? await HasNoPendingAnnulmentAsync(operation, debitNoteType, cancellationToken)
            : true;

        var validationContext = new
        {
            OperationExists = operation is not null,
            OperationTypeExists = operationTypeExists,
            ContributionTypeExists = contributionTypeExists,
            OperationIsContribution = operationIsContribution,

            PortfolioFound = portfolioEvaluation.PortfolioFound,
            ClosingAvailable = portfolioEvaluation.ClosingAvailable,
            ProcessDateIsValid = portfolioEvaluation.ProcessDateIsValid,

            OperationIsActive = operation?.Status == LifecycleStatus.Active,
            
            DebitNoteTypeExists = shouldEvaluateAdditionalChecks ? debitNoteType is not null : true,

            NoPendingAnnulment = noPendingAnnulment,
            CauseExists = causeConfigurationParameter is not null
        };

        var (isValid, _, validationErrors) = await ruleEvaluator
            .EvaluateAsync(ValidationWorkflow, validationContext, cancellationToken);

        if (!isValid)
        {
            var validationError = validationErrors.First();

            logger.LogWarning(
                "{Service} - Validación fallida: {Code} - {Message}",
                nameof(AccountingRecordsValCommandHandler),
                validationError.Code,
                validationError.Message);

            return Result.Failure<AccountingRecordsValidationResult>(
                Error.Validation(validationError.Code, validationError.Message));
        }
        
        var trustId = operation!.TrustId ?? portfolioEvaluation.TrustInfo!.TrustId;

        return Result.Success(new AccountingRecordsValidationResult(
            operation,
            debitNoteType!.OperationTypeId,
            portfolioEvaluation.PortfolioCurrentDate,
            trustId,
            causeConfigurationParameter!.ConfigurationParameterId));
    }

    private async Task<Result<PortfolioEvaluation>> EvaluatePortfolioAsync(
        ClientOperation? operation,
        decimal amount,
        CancellationToken cancellationToken)
    {
        if (operation is null)
        {
            return Result.Success(new PortfolioEvaluation(false, default, false, false, null));
        }

        var portfolioResult = await portfolioLocator
            .FindByPortfolioIdAsync(operation.PortfolioId, cancellationToken);

        if (!portfolioResult.IsSuccess)
        {
            return Result.Success(new PortfolioEvaluation(false, default, false, false, null));
        }

        var portfolioCurrentDate = DateTime.SpecifyKind(
            portfolioResult.Value.CurrentDate,
            DateTimeKind.Utc);

        var closingAvailable = !await closingValidator
            .IsClosingActiveAsync(operation.PortfolioId, cancellationToken);

        var processDateIsValid = operation.ProcessDate.Date < portfolioCurrentDate.AddDays(1).Date;

        var trustInfoResult = await trustInfoProvider
            .GetAsync(operation.ClientOperationId, amount, cancellationToken);

        if (trustInfoResult.IsFailure)
        {
            var trustError = trustInfoResult.Error;

            logger.LogWarning(
                "{Service} - Validación de fideicomiso falló: {Code} - {Message}",
                nameof(AccountingRecordsValCommandHandler),
                trustError.Code,
                trustError.Description);

            return Result.Failure<PortfolioEvaluation>(trustError);
        }

        return Result.Success(new PortfolioEvaluation(
            true,
            portfolioCurrentDate,
            closingAvailable,
            processDateIsValid,
            trustInfoResult.Value));
    }

    private async Task<bool> HasNoPendingAnnulmentAsync(
        ClientOperation? operation,
        OperationType? debitNoteType,
        CancellationToken cancellationToken)
    {
        if (operation is null || debitNoteType is null)
        {
            return false;
        }

        var hasActiveLinkedOperation = await clientOperationRepository
            .HasActiveLinkedOperationAsync(
                operation.ClientOperationId,
                debitNoteType.OperationTypeId,
                cancellationToken);

        return !hasActiveLinkedOperation;
    }

    private sealed record PortfolioEvaluation(
        bool PortfolioFound,
        DateTime PortfolioCurrentDate,
        bool ClosingAvailable,
        bool ProcessDateIsValid,
        TrustInfoResult? TrustInfo);

    private async Task<(bool OperationTypeExists, bool ContributionTypeExists, bool OperationIsContribution)>
        EvaluateContributionOperationAsync(ClientOperation? operation, CancellationToken cancellationToken)
    {
        if (operation is null)
        {
            var contribution = await operationTypeRepository
                .GetByNameAsync(ContributionOperationName, cancellationToken);

            return (OperationTypeExists: false,
                    ContributionTypeExists: contribution is not null,
                    OperationIsContribution: false);
        }

        var operationType = await operationTypeRepository
            .GetByIdAsync(operation.OperationTypeId, cancellationToken);

        var contributionsType = await operationTypeRepository
            .GetByNameAsync(ContributionOperationName, cancellationToken);
        
        var contributionType = contributionsType.FirstOrDefault();

        var operationTypeExists = operationType is not null;
        var contributionTypeExists = contributionType is not null;

        var isContributionOperation = operationTypeExists &&
            contributionTypeExists &&
            (operationType!.OperationTypeId == contributionType!.OperationTypeId ||
             (operationType.CategoryId.HasValue &&
              contributionType.OperationTypeId == operationType.CategoryId.Value));

        return (operationTypeExists, contributionTypeExists, isContributionOperation);
    }
}
