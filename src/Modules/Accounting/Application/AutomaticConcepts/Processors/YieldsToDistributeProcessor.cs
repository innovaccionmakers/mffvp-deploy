using Accounting.Application.Abstractions;
using Accounting.Application.Abstractions.External;
using Accounting.Domain.AccountingAssistants;
using Accounting.Domain.AccountingInconsistencies;
using Accounting.Domain.Constants;
using Accounting.Domain.PassiveTransactions;
using Accounting.Integrations.AccountingAssistants.Commands;
using Closing.Domain.YieldDetails;
using Common.SharedKernel.Application.Helpers.Serialization;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Common.SharedKernel.Domain.OperationTypes;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Operations.Integrations.OperationTypes;

namespace Accounting.Application.AutomaticConcepts.Processors;

internal sealed class YieldsToDistributeProcessor(ILogger<YieldsToDistributeProcessor> logger,
                                                  ISender sender,
                                                  IYieldToDistributeLocator yieldToDistributeLocator,
                                                  IYieldDetailsLocator yieldDetailsLocator,
                                                  IPortfolioLocator portfolioLocator,
                                                  IOperationLocator operationLocator,
                                                  IPassiveTransactionRepository passiveTransactionRepository,
                                                  IInconsistencyHandler inconsistencyHandler)
{
    public async Task<Result<bool>> ProcessAsync(IEnumerable<int> portfolioIds, DateTime processDate, CancellationToken cancellationToken)
    {
        var distributedYields = await yieldToDistributeLocator.GetDistributedYieldGroupResponse(portfolioIds, processDate, ConceptsTypeNames.AutomaticAccountingNote, cancellationToken);
        var yieldDetails = await yieldDetailsLocator.GetYieldsDetailsByPortfolioIdsClosingDateSourceAndConceptAsync(portfolioIds, processDate, SourceTypes.AutomaticConcept, ConceptsTypeNames.AdjustYieldsIncome,  cancellationToken);

        if (distributedYields.IsFailure)
        {
            logger.LogError("No se pudieron obtener los rendimientos distribuidos para los portafolios: {Error}", distributedYields.Error);
            return Result.Success(true);
        }

        if (yieldDetails.IsFailure)
        {
            logger.LogError("No se pudieron obtener los detalles de rendimiento para los portafolios: {Error}", yieldDetails.Error);
            return Result.Success(true);
        }

        var operationTypes = await operationLocator.GetOperationTypesByNameAsync(OperationTypeNames.AutomaticConceptAccountingNote, cancellationToken);

        if (operationTypes.IsFailure)
        {
            logger.LogError("No se pudieron obtener los tipos de operación para los conceptos automáticos: {Error}", operationTypes.Error);
            return Result.Failure<bool>(Error.Problem("Automatic.Concepts", "No se pudieron obtener los tipos de operación para los conceptos automáticos"));
        }

        var distributedYieldsResult = await CreateRangeFromDistributedYields(distributedYields.Value, yieldDetails.Value, processDate, operationTypes.Value, OperationTypeNames.AutomaticConceptAccountingNote, cancellationToken);

        if (!distributedYieldsResult.IsSuccess)
        {
            logger.LogInformation("Insertar errores en Redis");
            await inconsistencyHandler.HandleInconsistenciesAsync(distributedYieldsResult.Errors, processDate, ProcessTypes.AutomaticConcepts, cancellationToken);
            return Result.Failure<bool>(Error.Problem("Automatic.Concepts", "Se encontraron inconsistencias"));
        }

        if (!distributedYieldsResult.SuccessItems.Any())
        {
            logger.LogInformation("No hay conceptos automáticos que procesar");
            return Result.Success(true);
        }

        var distributedYieldsSave = await sender.Send(new AddAccountingEntitiesCommand(distributedYieldsResult.SuccessItems), cancellationToken);

        if (distributedYieldsSave.IsFailure)
        {
            logger.LogWarning("No se pudieron guardar los rendimientos distribuidos: {Error}", distributedYieldsSave.Error);
            return Result.Failure<bool>(Error.Problem("Automatic.Concepts", "No se pudieron guardar los rendimientos distribuidos"));
        }

        return Result.Success(true);

    }

    private async Task<ProcessingResult<AccountingAssistant, AccountingInconsistency>> CreateRangeFromDistributedYields(IReadOnlyCollection<GenericDebitNoteResponse> distributedYields,
                                                                                                                        IReadOnlyCollection<YieldDetailResponse> yieldDetails,
                                                                                                                        DateTime processDate,
                                                                                                                        IReadOnlyCollection<OperationTypeResponse> operationTypes,
                                                                                                                        string automaticConcept,
                                                                                                                        CancellationToken cancellationToken)
    {
        var accountingAssistants = new List<AccountingAssistant>();
        var errors = new List<AccountingInconsistency>();

        var portfolioIds = distributedYields.Select(yd => yd.PortfolioId).Distinct().ToList();
        var operationTypeIds = operationTypes.Select(ot => ot.OperationTypeId).ToList();
        var passiveTransactions = await passiveTransactionRepository
            .GetByPortfolioIdsAndOperationTypesAsync(portfolioIds, operationTypeIds, cancellationToken);

        var passiveTransactionsDict = passiveTransactions
            .GroupBy(pt => (pt.PortfolioId, pt.TypeOperationsId))
            .ToDictionary(g => g.Key, g => g.First());

        //T0 Automatic Concepts Debit Note
        foreach (var distributedYield in distributedYields)
        {
            IncomeEgressNature naturalezaFiltro = distributedYield.Value < 0 ? IncomeEgressNature.Income : IncomeEgressNature.Egress;
            var detail = distributedYield.Value < 0 ? IncomeExpenseNature.Income : IncomeExpenseNature.Expense;


            var operationType = operationTypes.FirstOrDefault(ot => ot.Name == automaticConcept && ot.Nature == naturalezaFiltro);

            if (operationType == null)
            {
                var errorMessage = $"No se encontró el tipo de operación para el concepto automático {automaticConcept} con naturaleza {naturalezaFiltro} para el portafolio {distributedYield.PortfolioId}";
                logger.LogWarning(errorMessage);
                errors.Add(AccountingInconsistency.Create(distributedYield.PortfolioId, OperationTypeNames.AutomaticConceptAccountingNote, errorMessage, AccountingActivity.Debit));
                errors.Add(AccountingInconsistency.Create(distributedYield.PortfolioId, OperationTypeNames.AutomaticConceptAccountingNote, errorMessage, AccountingActivity.Credit));
                continue;
            }

            passiveTransactionsDict.TryGetValue((distributedYield.PortfolioId, operationType.OperationTypeId), out var passiveTransaction);

            if (passiveTransaction == null)
            {
                logger.LogWarning("No se encontró una transacción pasiva para el portafolio {PortfolioId} y el tipo operación {OperationType}", distributedYield.PortfolioId, operationType.OperationTypeId);
                errors.Add(AccountingInconsistency.Create(distributedYield.PortfolioId, OperationTypeNames.AutomaticConceptAccountingNote, "No existe parametrización contable", AccountingActivity.Credit));
                errors.Add(AccountingInconsistency.Create(distributedYield.PortfolioId, OperationTypeNames.AutomaticConceptAccountingNote, "No existe parametrización contable", AccountingActivity.Debit));
                continue;
            }

            if (passiveTransaction.CreditAccount.IsNullOrEmpty())
            {
                logger.LogWarning("La transacción pasiva para el portafolio {PortfolioId} y el tipo operación {OperationType} no tiene cuenta de crédito", distributedYield.PortfolioId, operationType.OperationTypeId);
                errors.Add(AccountingInconsistency.Create(distributedYield.PortfolioId, OperationTypeNames.AutomaticConceptAccountingNote, "No existe cuenta de crédito", AccountingActivity.Credit));
                continue;
            }

            if (passiveTransaction.DebitAccount.IsNullOrEmpty())
            {
                logger.LogWarning("La transacción pasiva para el portafolio {PortfolioId} y el tipo operación {OperationType} no tiene cuenta de débito", distributedYield.PortfolioId, operationType.OperationTypeId);
                errors.Add(AccountingInconsistency.Create(distributedYield.PortfolioId, OperationTypeNames.AutomaticConceptAccountingNote, "No existe cuenta de débito", AccountingActivity.Debit));
                continue;
            }

            var portfolioResult = await portfolioLocator.GetPortfolioInformationAsync(distributedYield.PortfolioId, cancellationToken);

            if (portfolioResult.IsFailure)
            {
                logger.LogError("No se pudo obtener la información del portafolio {PortfolioId}: {Error}",
                    distributedYield.PortfolioId, portfolioResult.Error);
                errors.Add(AccountingInconsistency.Create(distributedYield.PortfolioId, OperationTypeNames.AutomaticConceptAccountingNote, portfolioResult.Error.Description));
                continue;
            }

            var distributedYieldCreate = AccountingAssistant.Create(
               distributedYield.PortfolioId,
               portfolioResult.Value.NitApprovedPortfolio,
               portfolioResult.Value.VerificationDigit,
               portfolioResult.Value.Name,
               processDate.ToString("yyyyMM"),
               processDate,
               $"{operationType.Name} {EnumHelper.GetEnumMemberValue(detail)}",
               distributedYield.Value,
               EnumHelper.GetEnumMemberValue(operationType.Nature) 
           );


            if (distributedYieldCreate.IsFailure)
            {
                logger.LogError("Error al crear el AccountingAssistant para el portafolio {PortfolioId}: {Error}",
                    distributedYield.PortfolioId, distributedYieldCreate.Error);
                errors.Add(AccountingInconsistency.Create(distributedYield.PortfolioId, OperationTypeNames.AutomaticConceptAccountingNote, distributedYieldCreate.Error.Description));
                continue;
            }

            accountingAssistants.AddRange(distributedYieldCreate.Value.ToDebitAndCredit(passiveTransaction.DebitAccount, passiveTransaction.CreditAccount));
        }


        //T1 Automatic Concepts Debit Note
        foreach (var yieldDetail in yieldDetails)
        {
            IncomeEgressNature naturalezaFiltro = yieldDetail.Income < 0 ? IncomeEgressNature.Income : IncomeEgressNature.Egress;
            var detail = yieldDetail.Income < 0 ? IncomeExpenseNature.Income : IncomeExpenseNature.Expense;

            var operationType = operationTypes.FirstOrDefault(ot => ot.Name == automaticConcept && ot.Nature == naturalezaFiltro);
            if (operationType == null)
            {
                var errorMessage = $"No se encontró el tipo de operación para el concepto automático {automaticConcept} con naturaleza {naturalezaFiltro} para el portafolio {yieldDetail.PortfolioId}";
                logger.LogWarning(errorMessage);
                errors.Add(AccountingInconsistency.Create(yieldDetail.PortfolioId, OperationTypeNames.AutomaticConceptAccountingNote, errorMessage, AccountingActivity.Debit));
                errors.Add(AccountingInconsistency.Create(yieldDetail.PortfolioId, OperationTypeNames.AutomaticConceptAccountingNote, errorMessage, AccountingActivity.Credit));
                continue;
            }

            passiveTransactionsDict.TryGetValue((yieldDetail.PortfolioId, operationType.OperationTypeId), out var passiveTransaction);

            if (passiveTransaction == null)
            {
                logger.LogWarning("No se encontró una transacción pasiva para el portafolio {PortfolioId} y el tipo operación {OperationType}", yieldDetail.PortfolioId, operationType.OperationTypeId);
                errors.Add(AccountingInconsistency.Create(yieldDetail.PortfolioId, OperationTypeNames.AutomaticConceptAccountingNote, "No existe parametrización contable", AccountingActivity.Credit));
                errors.Add(AccountingInconsistency.Create(yieldDetail.PortfolioId, OperationTypeNames.AutomaticConceptAccountingNote, "No existe parametrización contable", AccountingActivity.Debit));
                continue;
            }

            if (passiveTransaction.ContraCreditAccount.IsNullOrEmpty())
            {
                logger.LogWarning("La transacción pasiva para el portafolio {PortfolioId} y el tipo operación {OperationType} no tiene cuenta de crédito", yieldDetail.PortfolioId, operationType.OperationTypeId);
                errors.Add(AccountingInconsistency.Create(yieldDetail.PortfolioId, OperationTypeNames.AutomaticConceptAccountingNote, "No existe cuenta de crédito", AccountingActivity.Credit));
                continue;
            }

            if(passiveTransaction.ContraDebitAccount.IsNullOrEmpty())
            {
                logger.LogWarning("La transacción pasiva para el portafolio {PortfolioId} y el tipo operación {OperationType} no tiene cuenta de débito", yieldDetail.PortfolioId, operationType.OperationTypeId);
                errors.Add(AccountingInconsistency.Create(yieldDetail.PortfolioId, OperationTypeNames.AutomaticConceptAccountingNote, "No existe cuenta de débito", AccountingActivity.Debit));
                continue;
            }

            var portfolioResult = await portfolioLocator.GetPortfolioInformationAsync(yieldDetail.PortfolioId, cancellationToken);
            if (portfolioResult.IsFailure)
            {
                logger.LogError("No se pudo obtener la información del portafolio {PortfolioId}: {Error}",
                    yieldDetail.PortfolioId, portfolioResult.Error);
                errors.Add(AccountingInconsistency.Create(yieldDetail.PortfolioId, OperationTypeNames.AutomaticConceptAccountingNote, portfolioResult.Error.Description));
                continue;
            }

            var accountingAssistant = AccountingAssistant.Create(
                yieldDetail.PortfolioId,
                portfolioResult.Value.NitApprovedPortfolio,
                portfolioResult.Value.VerificationDigit,
                portfolioResult.Value.Name,
                processDate.ToString("yyyyMM"),
                processDate,
                $"{operationType.Name} {EnumHelper.GetEnumMemberValue(detail)}",
                yieldDetail.Income,
                EnumHelper.GetEnumMemberValue(operationType.Nature)

            );

            if (accountingAssistant.IsFailure)
            {
                logger.LogError("Error al crear el AccountingAssistant para el portafolio {PortfolioId}: {Error}",
                    yieldDetail.PortfolioId, accountingAssistant.Error);
                errors.Add(AccountingInconsistency.Create(yieldDetail.PortfolioId, OperationTypeNames.AutomaticConceptAccountingNote, accountingAssistant.Error.Description));
                continue;
            }

            accountingAssistants.AddRange(accountingAssistant.Value.ToDebitAndCredit(passiveTransaction.ContraDebitAccount, passiveTransaction.ContraCreditAccount));
        }
        return new ProcessingResult<AccountingAssistant, AccountingInconsistency>(accountingAssistants, errors);
    }
}
