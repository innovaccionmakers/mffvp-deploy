using Accounting.Application.Abstractions;
using Accounting.Application.Abstractions.External;
using Accounting.Domain.AccountingAssistants;
using Accounting.Domain.AccountingInconsistencies;
using Accounting.Domain.Constants;
using Accounting.Domain.PassiveTransactions;
using Accounting.Integrations.AccountingAssistants.Commands;
using Accounting.Integrations.AccountingReturns;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Accounting.Application.AccountingReturns;

public sealed class AccountingReturnsCommandHandler(
    ILogger<AccountingReturnsCommandHandler> logger,
    IPassiveTransactionRepository passiveTransactionRepository,
    IYieldLocator yieldLocator,
    IPortfolioLocator portfolioLocator,
    IOperationLocator operationLocator,
    IInconsistencyHandler inconsistencyHandler,
    IMediator mediator) : ICommandHandler<AccountingReturnsCommand, bool>
{
    public async Task<Result<bool>> Handle(AccountingReturnsCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var yields = await yieldLocator.GetAllReturnsPortfolioIdsAndClosingDate(command.PortfolioIds, command.ProcessDate, cancellationToken);

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

            var accountingReturnsResult = await CreateRange(yields.Value, command.ProcessDate, operationType.Value.OperationTypeId, operationType.Value.Name, operationType.Value.Nature, cancellationToken);

            if (!accountingReturnsResult.IsSuccess)
            {
                await inconsistencyHandler.HandleInconsistenciesAsync(accountingReturnsResult.Errors, command.ProcessDate, ProcessTypes.AccountingReturns, cancellationToken);
                return Result.Failure<bool>(Error.Problem("Accounting.Returns", "Se encontraron inconsistencias"));
            }

            var accountingReturnsSave = await mediator.Send(new AddAccountingEntitiesCommand(accountingReturnsResult.SuccessItems), cancellationToken);
            if (accountingReturnsSave.IsFailure)
            {
                logger.LogWarning("No se pudieron guardar los rendimientos contables: {Error}", accountingReturnsSave.Error);
                return Result.Failure<bool>(Error.Problem("Accounting.Returns", "No se pudieron guardar las rendimientos contables"));
            }

            return Result.Success(true);

        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error al procesar los rendimientos contables para la fecha {ProcessDate} y los Portafolios [{Portfolios}]",
                command.ProcessDate,
                string.Join(",", command.PortfolioIds)
            );
            return Result.Failure<bool>(Error.Problem("Exception", "Ocurrio un error inesperado al procesar los rendimientos contables"));
        }
    }

    private async Task<ProcessingResult<AccountingAssistant, AccountingInconsistency>> CreateRange(IEnumerable<YieldResponse> yields,
                                                                                                 DateTime processDate,
                                                                                                 long operationTypeId,
                                                                                                 string operationTypeName,
                                                                                                 string operationTypeNature,
                                                                                                 CancellationToken cancellationToken)
    {
        var accountingAssistants = new List<AccountingAssistant>();
        var errors = new List<AccountingInconsistency>();

        foreach (var yield in yields)
        {

            var passiveTransaction = await passiveTransactionRepository
                .GetByPortfolioIdAndOperationTypeAsync(yield.PortfolioId, operationTypeId, cancellationToken);

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
                operationTypeNature
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
            if (accountingAssistant.Value.Value > 0)
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
