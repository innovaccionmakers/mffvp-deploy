using Accounting.Application.Abstractions;
using Accounting.Application.Abstractions.External;
using Accounting.Domain.AccountingAssistants;
using Accounting.Domain.AccountingInconsistencies;
using Accounting.Domain.Constants;
using Accounting.Domain.PassiveTransactions;
using Accounting.Integrations.AccountingAssistants.Commands;
using Closing.IntegrationEvents.Yields;
using Common.SharedKernel.Application.Helpers.Serialization;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Common.SharedKernel.Domain.OperationTypes;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Operations.Integrations.OperationTypes;

namespace Accounting.Application.AutomaticConcepts.Processors;

internal sealed class AutomaticConceptsProcessor(ILogger<AutomaticConceptsProcessor> logger,
                                                  ISender sender,
                                                  IPortfolioLocator portfolioLocator,
                                                  IOperationLocator operationLocator,
                                                  IPassiveTransactionRepository passiveTransactionRepository,
                                                  IInconsistencyHandler inconsistencyHandler)
{
    public async Task<Result<bool>> ProcessAsync(
        GetAllAutConceptsByPortfolioIdsAndClosingDateConsumerResponse yieldResult,
        DateTime processDate,
        CancellationToken cancellationToken)
    {
        var operationTypes = await operationLocator.GetOperationTypesByNameAsync(OperationTypeNames.AutomaticConcept, cancellationToken);

        if (operationTypes.IsFailure)
        {
            logger.LogError("No se pudieron obtener los tipos de operación para los conceptos automáticos: {Error}", operationTypes.Error);
            return Result.Failure<bool>(Error.Validation(operationTypes.Error.Code ?? string.Empty, operationTypes.Error.Description ?? string.Empty));
        }

        var automaticConcepts = await ProcessAutomaticConceptsAsync(yieldResult, processDate, operationTypes.Value, OperationTypeNames.AutomaticConcept, cancellationToken);

        if (!automaticConcepts.IsSuccess)
        {
            logger.LogInformation("Insertar errores en Redis");
            await inconsistencyHandler.HandleInconsistenciesAsync(automaticConcepts.Errors, processDate, ProcessTypes.AutomaticConcepts, cancellationToken);
            return Result.Failure<bool>(Error.Problem("Automatic.Concepts", "Se encontraron inconsistencias"));
        }

        if (!automaticConcepts.SuccessItems.Any())
        {
            logger.LogInformation("No hay conceptos automáticos que procesar");
            return Result.Success(true);
        }

        var automaticConceptsSave = await sender.Send(new AddAccountingEntitiesCommand(automaticConcepts.SuccessItems), cancellationToken);

        if (automaticConceptsSave.IsFailure)
        {
            logger.LogWarning("No se pudieron guardar los conceptos automáticos: {Error}", automaticConceptsSave.Error);
            return Result.Failure<bool>(Error.Problem("Automatic.Concepts", "No se pudieron guardar los conceptos automáticos"));
        }

        return Result.Success(true);
    }

    private async Task<ProcessingResult<AccountingAssistant, AccountingInconsistency>> ProcessAutomaticConceptsAsync(
        GetAllAutConceptsByPortfolioIdsAndClosingDateConsumerResponse yieldResult,
        DateTime processDate,
        IReadOnlyCollection<OperationTypeResponse> operationsType,
        string automaticConcept,
        CancellationToken cancellationToken)
    {
        var accountingAssistants = new List<AccountingAssistant>();
        var errors = new List<AccountingInconsistency>();

        foreach (var yield in yieldResult.YieldAutConcepts.Yields)
        {
            if (yield.CreditedYields == yield.YieldToCredit)
                continue;

            var value = yield.YieldToCredit - yield.CreditedYields;
            var portfolioResult = await portfolioLocator.GetPortfolioInformationAsync(yield.PortfolioId, cancellationToken);

            if (portfolioResult.IsFailure)
            {
                logger.LogError("No se pudo obtener la información del portafolio {PortfolioId}: {Error}", yield.PortfolioId, portfolioResult.Error);
                errors.Add(AccountingInconsistency.Create(yield.PortfolioId, OperationTypeNames.AutomaticConcepts, portfolioResult.Error.Description));
                continue;
            }

            IncomeEgressNature naturalezaFiltro = value < 0 ? IncomeEgressNature.Income : IncomeEgressNature.Egress;
            var operationType = operationsType.FirstOrDefault(ot => ot.Name == automaticConcept && ot.Nature == naturalezaFiltro);

            if (operationType == null)
            {
                logger.LogWarning("No se encontró el tipo de operación para el concepto automático {AutomaticConcept} con naturaleza {Nature} para el portafolio {PortfolioId}",
                    automaticConcept, naturalezaFiltro, yield.PortfolioId);
                errors.Add(AccountingInconsistency.Create(yield.PortfolioId, OperationTypeNames.AutomaticConcepts, "No existe parametrización contable", AccountingActivity.Credit));
                errors.Add(AccountingInconsistency.Create(yield.PortfolioId, OperationTypeNames.AutomaticConcepts, "No existe parametrización contable", AccountingActivity.Debit));
                continue;
            }

            var passiveTransaction = await passiveTransactionRepository.GetByPortfolioIdAndOperationTypeAsync(yield.PortfolioId, operationType.OperationTypeId, cancellationToken);

            if (passiveTransaction == null)
            {
                logger.LogWarning("No se encontraron conceptos automáticos para el portafolio {PortfolioId} y el tipo operación {OperationType}", yield.PortfolioId, operationType.OperationTypeId);
                errors.Add(AccountingInconsistency.Create(yield.PortfolioId, OperationTypeNames.AutomaticConcepts, "No existe parametrización contable", AccountingActivity.Credit));
                errors.Add(AccountingInconsistency.Create(yield.PortfolioId, OperationTypeNames.AutomaticConcepts, "No existe parametrización contable", AccountingActivity.Debit));
                continue;
            }

            if (passiveTransaction.CreditAccount.IsNullOrEmpty())
            {
                logger.LogWarning("El concepto automático para el portafolio {PortfolioId} y el tipo operación {OperationType} no tiene cuenta de crédito", yield.PortfolioId, operationType.OperationTypeId);
                errors.Add(AccountingInconsistency.Create(yield.PortfolioId, OperationTypeNames.AutomaticConcepts, "No existe parametrización contable", AccountingActivity.Credit));
                continue;
            }

            if (passiveTransaction.DebitAccount.IsNullOrEmpty())
            {
                logger.LogWarning("El concepto automático para el portafolio {PortfolioId} y el tipo operación {OperationType} no tiene cuenta de débito", yield.PortfolioId, operationType.OperationTypeId);
                errors.Add(AccountingInconsistency.Create(yield.PortfolioId, OperationTypeNames.AutomaticConcepts, "No existe parametrización contable", AccountingActivity.Debit));
                continue;
            }

            var natureValue = EnumHelper.GetEnumMemberValue(operationType.Nature);
            var accountingAssistant = AccountingAssistant.Create(
                yield.PortfolioId,
                portfolioResult.Value.NitApprovedPortfolio,
                portfolioResult.Value.VerificationDigit,
                portfolioResult.Value.Name,
                processDate.ToString("yyyyMM"),
                processDate,
                operationType.Name,
                Math.Abs(value),
                natureValue
            );

            if (accountingAssistant.IsFailure)
            {
                logger.LogError("Error procesando los conceptos automáticos para portfolio {PortfolioId}: {Error}", yield.PortfolioId, accountingAssistant.Error);
                errors.Add(AccountingInconsistency.Create(yield.PortfolioId, OperationTypeNames.AutomaticConcepts, accountingAssistant.Error.Description));
                continue;
            }

            accountingAssistants.AddRange(accountingAssistant.Value.ToDebitAndCredit(passiveTransaction.DebitAccount, passiveTransaction.CreditAccount));
        }

        foreach (var yield in yieldResult.YieldAutConcepts.YieldDetails)
        {
            if (yield.Income == 0)
                continue;

            var value = yield.Income;
            var portfolioResult = await portfolioLocator.GetPortfolioInformationAsync(yield.PortfolioId, cancellationToken);

            if (portfolioResult.IsFailure)
            {
                logger.LogError("No se pudo obtener la información del portafolio {PortfolioId}: {Error}", yield.PortfolioId, portfolioResult.Error);
                errors.Add(AccountingInconsistency.Create(yield.PortfolioId, OperationTypeNames.AutomaticConcepts, portfolioResult.Error.Description));
                continue;
            }

            IncomeEgressNature naturalezaFiltro = value > 0 ? IncomeEgressNature.Income : IncomeEgressNature.Egress;
            var operationType = operationsType.FirstOrDefault(ot => ot.Name == automaticConcept && ot.Nature == naturalezaFiltro);

            if (operationType == null)
            {
                logger.LogWarning("No se encontró el tipo de operación para el concepto automático {AutomaticConcept} con naturaleza {Nature} para el portafolio {PortfolioId}",
                    automaticConcept, naturalezaFiltro, yield.PortfolioId);
                errors.Add(AccountingInconsistency.Create(yield.PortfolioId, OperationTypeNames.AutomaticConcepts, "No existe parametrización contable", AccountingActivity.ContraCreditAccount));
                errors.Add(AccountingInconsistency.Create(yield.PortfolioId, OperationTypeNames.AutomaticConcepts, "No existe parametrización contable", AccountingActivity.ContraDebitAccount));
                continue;
            }

            var passiveTransaction = await passiveTransactionRepository.GetByPortfolioIdAndOperationTypeAsync(yield.PortfolioId, operationType.OperationTypeId, cancellationToken);

            if (passiveTransaction == null)
            {
                logger.LogWarning("No se encontraron conceptos automáticos para el portafolio {PortfolioId} y el tipo operación {OperationType}", yield.PortfolioId, operationType.OperationTypeId);
                errors.Add(AccountingInconsistency.Create(yield.PortfolioId, OperationTypeNames.AutomaticConcepts, "No existe parametrización contable", AccountingActivity.ContraCreditAccount));
                errors.Add(AccountingInconsistency.Create(yield.PortfolioId, OperationTypeNames.AutomaticConcepts, "No existe parametrización contable", AccountingActivity.ContraDebitAccount));
                continue;
            }

            if (passiveTransaction.ContraCreditAccount.IsNullOrEmpty())
            {
                logger.LogWarning("El concepto automático para el portafolio {PortfolioId} y el tipo operación {OperationType} no tiene cuenta contra crédito", yield.PortfolioId, operationType.OperationTypeId);
                errors.Add(AccountingInconsistency.Create(yield.PortfolioId, OperationTypeNames.AutomaticConcepts, "No existe parametrización contable", AccountingActivity.ContraCreditAccount));
                continue;
            }

            if (passiveTransaction.ContraDebitAccount.IsNullOrEmpty())
            {
                logger.LogWarning("El concepto automático para el portafolio {PortfolioId} y el tipo operación {OperationType} no tiene cuenta contra débito", yield.PortfolioId, operationType.OperationTypeId);
                errors.Add(AccountingInconsistency.Create(yield.PortfolioId, OperationTypeNames.AutomaticConcepts, "No existe parametrización contable", AccountingActivity.ContraDebitAccount));
                continue;
            }

            var natureValue = EnumHelper.GetEnumMemberValue(value < 0 ? IncomeEgressNature.Income : IncomeEgressNature.Egress);
            var accountingAssistant = AccountingAssistant.Create(
                yield.PortfolioId,
                portfolioResult.Value.NitApprovedPortfolio,
                portfolioResult.Value.VerificationDigit,
                portfolioResult.Value.Name,
                processDate.ToString("yyyyMM"),
                processDate,
                operationType.Name,
                Math.Abs(value),
                natureValue
            );

            if (accountingAssistant.IsFailure)
            {
                logger.LogError("Error procesando los conceptos automáticos para portfolio {PortfolioId}: {Error}", yield.PortfolioId, accountingAssistant.Error);
                errors.Add(AccountingInconsistency.Create(yield.PortfolioId, OperationTypeNames.AutomaticConcepts, accountingAssistant.Error.Description));
                continue;
            }

            accountingAssistants.AddRange(accountingAssistant.Value.ToDebitAndCredit(passiveTransaction.ContraDebitAccount, passiveTransaction.ContraCreditAccount));
        }

        return new ProcessingResult<AccountingAssistant, AccountingInconsistency>(accountingAssistants, errors);
    }
}

