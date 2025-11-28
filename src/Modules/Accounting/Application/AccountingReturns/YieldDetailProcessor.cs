using Accounting.Application.Abstractions;
using Accounting.Application.Abstractions.External;
using Accounting.Domain.AccountingAssistants;
using Accounting.Domain.AccountingInconsistencies;
using Accounting.Domain.Constants;
using Accounting.Domain.PassiveTransactions;
using Accounting.Integrations.AccountingAssistants.Commands;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Accounting.Application.AccountingReturns;

internal sealed class YieldDetailProcessor(
    ILogger<YieldDetailProcessor> logger,
    IPassiveTransactionRepository passiveTransactionRepository,
    IYieldDetailsLocator yieldDetailsLocator,
    IPortfolioLocator portfolioLocator,
    IOperationLocator operationLocator,
    IInconsistencyHandler inconsistencyHandler,
    IMediator mediator)
{
    public async Task<Result<bool>> ProcessAsync(IEnumerable<int> portfolioIds, DateTime processDate, CancellationToken cancellationToken)
    {
        var yieldDetails = await yieldDetailsLocator.GetYieldsDetailsByPortfolioIdsClosingDateSourceAndConceptAsync(portfolioIds, processDate, SourceTypes.ExtraYield, null, cancellationToken);

        if (yieldDetails.IsFailure)
        {
            logger.LogError("No se pudieron obtener los detalles de rendimiento para los portafolios: {Error}", yieldDetails.Error);
            return Result.Success(true);
        }

        var operationType = await operationLocator.GetOperationTypeByNameAsync(OperationTypeNames.AdjustYields, cancellationToken);

        if (operationType.IsFailure)
        {
            logger.LogWarning("No se pudo obtener el tipo de operación '{OperationType}': {Error}", OperationTypeNames.AdjustYields, operationType.Error);
            return Result.Failure<bool>(Error.Problem("Accounting.Returns", "No se pudo obtener el tipo de operación"));
        }

        var accountingReturnsResult = await CreateRangeFromYieldDetails(yieldDetails.Value, processDate, operationType.Value.OperationTypeId, operationType.Value.Name, cancellationToken);

        if (!accountingReturnsResult.IsSuccess)
        {
            await inconsistencyHandler.HandleInconsistenciesAsync(accountingReturnsResult.Errors, processDate, ProcessTypes.AccountingReturns, cancellationToken);
            return Result.Failure<bool>(Error.Problem("Accounting.Returns", "Se encontraron inconsistencias"));
        }

        var accountingReturnsSave = await mediator.Send(new AddAccountingEntitiesCommand(accountingReturnsResult.SuccessItems), cancellationToken);
        if (accountingReturnsSave.IsFailure)
        {
            logger.LogWarning("No se pudieron guardar los ajustes de rendimientos contables: {Error}", accountingReturnsSave.Error);
            return Result.Failure<bool>(Error.Problem("Accounting.Returns", "No se pudieron guardar los ajustes de rendimientos contables"));
        }

        return Result.Success(true);
    }

    private async Task<ProcessingResult<AccountingAssistant, AccountingInconsistency>> CreateRangeFromYieldDetails(IEnumerable<YieldDetailResponse> yieldDetails,
                                                                                                                        DateTime processDate,
                                                                                                                        long operationTypeId,
                                                                                                                        string operationTypeName,
                                                                                                                        CancellationToken cancellationToken)
    {
        var accountingAssistants = new List<AccountingAssistant>();
        var errors = new List<AccountingInconsistency>();

        var portfolioIds = yieldDetails.Select(yd => yd.PortfolioId).Distinct().ToList();
        var passiveTransactions = await passiveTransactionRepository
            .GetByPortfolioIdsAndOperationTypeAsync(portfolioIds, operationTypeId, cancellationToken);

        var passiveTransactionsDict = passiveTransactions
            .GroupBy(pt => pt.PortfolioId)
            .ToDictionary(g => g.Key, g => g.First());

        foreach (var yieldDetail in yieldDetails)
        {
            passiveTransactionsDict.TryGetValue(yieldDetail.PortfolioId, out var passiveTransaction);

            if (passiveTransaction == null)
            {
                logger.LogWarning("No se encontró una transacción pasiva para el portafolio {PortfolioId} y el tipo operación {OperationType}", yieldDetail.PortfolioId, operationTypeId);
                errors.Add(AccountingInconsistency.Create(yieldDetail.PortfolioId, OperationTypeNames.AdjustYields, "No existe parametrización contable", AccountingActivity.Credit));
                errors.Add(AccountingInconsistency.Create(yieldDetail.PortfolioId, OperationTypeNames.AdjustYields, "No existe parametrización contable", AccountingActivity.Debit));
                continue;
            }

            string? cuentaDebit;
            string? cuentaCredit;

            if (yieldDetail.Income > 0)
            {
                // Si el valor es positivo: cuenta_debito y cuenta_credito
                if (passiveTransaction.DebitAccount.IsNullOrEmpty())
                {
                    logger.LogWarning("La transacción pasiva para el portafolio {PortfolioId} y el tipo operación {OperationType} no tiene cuenta de débito", yieldDetail.PortfolioId, operationTypeId);
                    errors.Add(AccountingInconsistency.Create(yieldDetail.PortfolioId, OperationTypeNames.AdjustYields, "No existe cuenta de débito", AccountingActivity.Debit));
                    continue;
                }

                if (passiveTransaction.CreditAccount.IsNullOrEmpty())
                {
                    logger.LogWarning("La transacción pasiva para el portafolio {PortfolioId} y el tipo operación {OperationType} no tiene cuenta de crédito", yieldDetail.PortfolioId, operationTypeId);
                    errors.Add(AccountingInconsistency.Create(yieldDetail.PortfolioId, OperationTypeNames.AdjustYields, "No existe cuenta de crédito", AccountingActivity.Credit));
                    continue;
                }

                cuentaDebit = passiveTransaction.DebitAccount!;
                cuentaCredit = passiveTransaction.CreditAccount!;
            }
            else
            {
                // Si el valor es negativo: cuenta_contra_credito y cuenta_contra_debito
                if (passiveTransaction.ContraCreditAccount.IsNullOrEmpty())
                {
                    logger.LogWarning("La transacción pasiva para el portafolio {PortfolioId} y el tipo operación {OperationType} no tiene cuenta contra crédito", yieldDetail.PortfolioId, operationTypeId);
                    errors.Add(AccountingInconsistency.Create(yieldDetail.PortfolioId, OperationTypeNames.AdjustYields, "No existe cuenta contra crédito", AccountingActivity.Debit));
                    continue;
                }

                if (passiveTransaction.ContraDebitAccount.IsNullOrEmpty())
                {
                    logger.LogWarning("La transacción pasiva para el portafolio {PortfolioId} y el tipo operación {OperationType} no tiene cuenta contra débito", yieldDetail.PortfolioId, operationTypeId);
                    errors.Add(AccountingInconsistency.Create(yieldDetail.PortfolioId, OperationTypeNames.AdjustYields, "No existe cuenta contra débito", AccountingActivity.Credit));
                    continue;
                }

                cuentaDebit = passiveTransaction.ContraCreditAccount!;
                cuentaCredit = passiveTransaction.ContraDebitAccount!;
            }

            var portfolioResult = await portfolioLocator.GetPortfolioInformationAsync(yieldDetail.PortfolioId, cancellationToken);

            if (portfolioResult.IsFailure)
            {
                logger.LogError("No se pudo obtener la información del portafolio {PortfolioId}: {Error}",
                    yieldDetail.PortfolioId, portfolioResult.Error);
                errors.Add(AccountingInconsistency.Create(yieldDetail.PortfolioId, OperationTypeNames.AdjustYields, portfolioResult.Error.Description));
                continue;
            }

            var accountingAssistant = AccountingAssistant.Create(
                yieldDetail.PortfolioId,
                portfolioResult.Value.NitApprovedPortfolio,
                portfolioResult.Value.VerificationDigit,
                portfolioResult.Value.Name,
                processDate.ToString("yyyyMM"),
                processDate,
                operationTypeName,
                yieldDetail.Income,
                yieldDetail.Income > 0 ? NatureTypes.Income : NatureTypes.Egress
            );

            if (accountingAssistant.IsFailure)
            {
                logger.LogError("Error al crear el AccountingAssistant para el portafolio {PortfolioId}: {Error}",
                    yieldDetail.PortfolioId, accountingAssistant.Error);
                errors.Add(AccountingInconsistency.Create(yieldDetail.PortfolioId, OperationTypeNames.AdjustYields, accountingAssistant.Error.Description));
                continue;
            }

            accountingAssistants.AddRange(accountingAssistant.Value.ToDebitAndCredit(cuentaDebit, cuentaCredit));
        }

        return new ProcessingResult<AccountingAssistant, AccountingInconsistency>(accountingAssistants, errors);
    }
}

