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

internal sealed class YieldProcessor(
    ILogger<YieldProcessor> logger,
    IPassiveTransactionRepository passiveTransactionRepository,
    IYieldLocator yieldLocator,
    IPortfolioLocator portfolioLocator,
    IOperationLocator operationLocator,
    IInconsistencyHandler inconsistencyHandler,
    IMediator mediator)
{
    public async Task<Result<bool>> ProcessAsync(IEnumerable<int> portfolioIds, DateTime processDate, CancellationToken cancellationToken)
    {
        var yields = await yieldLocator.GetAllReturnsPortfolioIdsAndClosingDate(portfolioIds, processDate, cancellationToken);

        if (yields.IsFailure)
        {
            logger.LogError("No se pudieron obtener los rendimientos para los portafolios: {Error}", yields.Error);
            return Result.Success(true);
        }

        var operationType = await operationLocator.GetOperationTypeByNameAsync(OperationTypeNames.Yield, cancellationToken);

        if (operationType.IsFailure)
        {
            logger.LogWarning("No se pudo obtener el tipo de operación '{OperationType}': {Error}", OperationTypeNames.Yield, operationType.Error);
            return Result.Failure<bool>(Error.Problem("Accounting.Returns", "No se pudo obtener el tipo de operación"));
        }

        var accountingReturnsResult = await CreateRange(yields.Value, processDate, operationType.Value.OperationTypeId, operationType.Value.Name, cancellationToken);

        if (!accountingReturnsResult.IsSuccess)
        {
            await inconsistencyHandler.HandleInconsistenciesAsync(accountingReturnsResult.Errors, processDate, ProcessTypes.AccountingReturns, cancellationToken);
            return Result.Failure<bool>(Error.Problem("Accounting.Returns", "Se encontraron inconsistencias"));
        }

        var accountingReturnsSave = await mediator.Send(new AddAccountingEntitiesCommand(accountingReturnsResult.SuccessItems), cancellationToken);
        if (accountingReturnsSave.IsFailure)
        {
            logger.LogWarning("No se pudieron guardar los rendimientos contables: {Error}", accountingReturnsSave.Error);
            return Result.Failure<bool>(Error.Problem("Accounting.Returns", "No se pudieron guardar los rendimientos contables"));
        }

        return Result.Success(true);
    }

    private async Task<ProcessingResult<AccountingAssistant, AccountingInconsistency>> CreateRange(IEnumerable<YieldResponse> yields,
                                                                                                     DateTime processDate,
                                                                                                     long operationTypeId,
                                                                                                     string operationTypeName,
                                                                                                     CancellationToken cancellationToken)
    {
        var accountingAssistants = new List<AccountingAssistant>();
        var errors = new List<AccountingInconsistency>();

        var portfolioIds = yields.Select(y => y.PortfolioId).Distinct().ToList();
        var passiveTransactions = await passiveTransactionRepository
            .GetByPortfolioIdsAndOperationTypeAsync(portfolioIds, operationTypeId, cancellationToken);

        var passiveTransactionsDict = passiveTransactions
            .GroupBy(pt => pt.PortfolioId)
            .ToDictionary(g => g.Key, g => g.First());

        foreach (var yield in yields)
        {
            passiveTransactionsDict.TryGetValue(yield.PortfolioId, out var passiveTransaction);

            if (passiveTransaction == null)
            {
                logger.LogWarning("No se encontró una transacción pasiva para el portafolio {PortfolioId} y el tipo operación {OperationType}", yield.PortfolioId, operationTypeId);
                errors.Add(AccountingInconsistency.Create(yield.PortfolioId, OperationTypeNames.Yield, "No existe parametrización contable", AccountingActivity.Credit));
                errors.Add(AccountingInconsistency.Create(yield.PortfolioId, OperationTypeNames.Yield, "No existe parametrización contable", AccountingActivity.Debit));
                continue;
            }

            if (passiveTransaction.CreditAccount.IsNullOrEmpty() || passiveTransaction.ContraCreditAccount.IsNullOrEmpty())
            {
                logger.LogWarning("La transacción pasiva para el portafolio {PortfolioId} y el tipo operación {OperationType} no tiene cuenta de crédito", yield.PortfolioId, operationTypeId);
                errors.Add(AccountingInconsistency.Create(yield.PortfolioId, OperationTypeNames.Yield, "No existe cuenta de crédito", AccountingActivity.Credit));
                continue;
            }

            if (passiveTransaction.DebitAccount.IsNullOrEmpty() || passiveTransaction.ContraDebitAccount.IsNullOrEmpty())
            {
                logger.LogWarning("La transacción pasiva para el portafolio {PortfolioId} y el tipo operación {OperationType} no tiene cuenta de débito", yield.PortfolioId, operationTypeId);
                errors.Add(AccountingInconsistency.Create(yield.PortfolioId, OperationTypeNames.Yield, "No existe cuenta de débito", AccountingActivity.Debit));
                continue;
            }

            var portfolioResult = await portfolioLocator.GetPortfolioInformationAsync(yield.PortfolioId, cancellationToken);

            if (portfolioResult.IsFailure)
            {
                logger.LogError("No se pudo obtener la información del portafolio {PortfolioId}: {Error}",
                    yield.PortfolioId, portfolioResult.Error);
                errors.Add(AccountingInconsistency.Create(yield.PortfolioId, OperationTypeNames.Yield, portfolioResult.Error.Description));
                continue;
            }

            var accountingAssistant = AccountingAssistant.Create(
                yield.PortfolioId,
                portfolioResult.Value.NitApprovedPortfolio,
                portfolioResult.Value.VerificationDigit,
                portfolioResult.Value.Name,
                processDate.ToString("yyyyMM"),
                processDate,
                operationTypeName,
                yield.YieldToCredit,
                NatureTypes.Yields
            );

            if (accountingAssistant.IsFailure)
            {
                logger.LogError("Error al crear el AccountingAssistant para el portafolio {PortfolioId}: {Error}",
                    yield.PortfolioId, accountingAssistant.Error);
                errors.Add(AccountingInconsistency.Create(yield.PortfolioId, OperationTypeNames.Yield, accountingAssistant.Error.Description));
                continue;
            }

            string? cuentaDebit;
            string? cuentaCredit;
            if (yield.YieldToCredit > 0)
            {
                cuentaDebit = passiveTransaction.DebitAccount!;
                cuentaCredit = passiveTransaction.CreditAccount!;
            }
            else
            {
                cuentaDebit = passiveTransaction.ContraCreditAccount!;
                cuentaCredit = passiveTransaction.ContraDebitAccount!;
            }

            accountingAssistants.AddRange(accountingAssistant.Value.ToDebitAndCredit(cuentaDebit, cuentaCredit));
        }

        return new ProcessingResult<AccountingAssistant, AccountingInconsistency>(accountingAssistants, errors);
    }
}

