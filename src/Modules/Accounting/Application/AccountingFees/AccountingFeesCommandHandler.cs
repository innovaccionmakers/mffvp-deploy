using Accounting.Application.Abstractions;
using Accounting.Application.Abstractions.External;
using Accounting.Domain.AccountingAssistants;
using Accounting.Domain.AccountingInconsistencies;
using Accounting.Domain.Constants;
using Accounting.Domain.PassiveTransactions;
using Accounting.Integrations.AccountingAssistants.Commands;
using Accounting.Integrations.AccountingFees;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Accounting.Application.AccountingFees;

public sealed class AccountingFeesCommandHandler(
    ILogger<AccountingFeesCommandHandler> logger,
    IPassiveTransactionRepository passiveTransactionRepository,
    IYieldLocator yieldLocator,
    IPortfolioLocator portfolioLocator,
    IOperationLocator operationLocator,
    IInconsistencyHandler inconsistencyHandler,
    IMediator mediator) : ICommandHandler<AccountingFeesCommand, bool>
{
    public async Task<Result<bool>> Handle(AccountingFeesCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var yields = await yieldLocator.GetAllComissionsPortfolioIdsAndClosingDate(command.PortfolioIds, command.ProcessDate, cancellationToken);

            if (yields.IsFailure)
            {
                logger.LogError("No se pudieron obtener los yields para los portafolios: {Error}", yields.Error);
                return Result.Failure<bool>(Error.Problem("Accounting.Fees", "No se pudieron obtener los yields para los portafolios"));
            }

            var operationType = await operationLocator.GetOperationTypeByNameAsync(OperationTypeNames.Commission, cancellationToken);

            if (operationType.IsFailure)
            {
                logger.LogWarning("No se pudo obtener el tipo de operación '{OperationType}': {Error}", OperationTypeNames.Commission, operationType.Error);
                return Result.Failure<bool>(Error.Problem("Accounting.Fees", "No se pudo obtener el tipo de operación"));
            }

            var accountingFeesResult = await CreateRange(yields.Value, command.ProcessDate, operationType.Value.OperationTypeId, operationType.Value.Name, operationType.Value.Nature, cancellationToken);

            if (!accountingFeesResult.IsSuccess)
            {
                await inconsistencyHandler.HandleInconsistenciesAsync(accountingFeesResult.Errors, command.ProcessDate, ProcessTypes.AccountingFees, cancellationToken);
                return Result.Failure<bool>(Error.Problem("Accounting.Fees", "Se encontraron inconsistencias"));
            }

            if (!accountingFeesResult.SuccessItems.Any())
            {
                logger.LogInformation("Sin comisiones contables para procesar");
                return Result.Failure<bool>(Error.Problem("Accounting.Fees", "Sin comisiones contables para procesar"));
            }

            var accountingFeesSave = await mediator.Send(new AddAccountingEntitiesCommand(accountingFeesResult.SuccessItems), cancellationToken);
            if (accountingFeesSave.IsFailure)
            {
                logger.LogWarning("No se pudieron guardar las comisiones contables: {Error}", accountingFeesSave.Error);
                return Result.Failure<bool>(Error.Problem("Accounting.Fees", "No se pudieron guardar las comisiones contables"));
            }

            return Result.Success(true);

        } catch (Exception ex)
        {
            logger.LogError(ex, "Error al procesar las comisiones contables para la fecha {ProcessDate} y los Portafolios [{Portfolios}]",
                command.ProcessDate,
                string.Join(",", command.PortfolioIds)
            );
            return Result.Failure<bool>(Error.Problem("Exception", "Ocurrio un error inesperado al procesar las comisiones contables"));
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
                errors.Add(AccountingInconsistency.Create(yield.PortfolioId, OperationTypeNames.Commission, "No existe parametrización contable", AccountingActivity.Credit));
                errors.Add(AccountingInconsistency.Create(yield.PortfolioId, OperationTypeNames.Commission, "No existe parametrización contable", AccountingActivity.Debit));
                continue;
            }

            if(passiveTransaction.CreditAccount.IsNullOrEmpty())
            {
                logger.LogWarning("La transacción pasiva para el portafolio {PortfolioId} y el tipo operación {OperationType} no tiene cuenta de crédito", yield.PortfolioId, operationTypeId);
                errors.Add(AccountingInconsistency.Create(yield.PortfolioId, OperationTypeNames.Commission, "No existe cuenta de crédito", AccountingActivity.Credit));
                continue;
            }

            if (passiveTransaction.DebitAccount.IsNullOrEmpty())
            {
                logger.LogWarning("La transacción pasiva para el portafolio {PortfolioId} y el tipo operación {OperationType} no tiene cuenta de débito", yield.PortfolioId, operationTypeId);
                errors.Add(AccountingInconsistency.Create(yield.PortfolioId, OperationTypeNames.Commission, "No existe cuenta de débito", AccountingActivity.Debit));
                continue;
            }

            var portfolioResult = await portfolioLocator.GetPortfolioInformationAsync(yield.PortfolioId, cancellationToken);

            if (portfolioResult.IsFailure)
            {
                logger.LogError("No se pudo obtener la información del portafolio {PortfolioId}: {Error}",
                    yield.PortfolioId, portfolioResult.Error);
                errors.Add(AccountingInconsistency.Create(yield.PortfolioId, OperationTypeNames.Commission, portfolioResult.Error.Description));
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
                yield.Commissions,
                operationTypeNature
            );

            if (accountingAssistant.IsFailure)
            {
                logger.LogError("Error al crear el AccountingAssistant para el portafolio {PortfolioId}: {Error}",
                    yield.PortfolioId, accountingAssistant.Error);
                errors.Add(AccountingInconsistency.Create(yield.PortfolioId, OperationTypeNames.Commission, accountingAssistant.Error.Description));
                continue;
            }

            accountingAssistants.AddRange(accountingAssistant.Value.ToDebitAndCredit(passiveTransaction.DebitAccount, passiveTransaction.CreditAccount));
        }

        return new ProcessingResult<AccountingAssistant, AccountingInconsistency>(accountingAssistants, errors);
    }
}