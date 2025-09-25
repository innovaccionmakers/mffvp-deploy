using Accounting.Application.Abstractions.External;
using Accounting.Domain.AccountingAssistants;
using Accounting.Domain.Constants;
using Accounting.Domain.PassiveTransactions;
using Accounting.Integrations.AccountingAssistants.Commands;
using Accounting.Integrations.AccountingFees;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Accounting.Application.AccountingFees;

internal sealed class GetAccountingFeesCommandHandler(
    ILogger<GetAccountingFeesCommandHandler> logger,
    IPassiveTransactionRepository passiveTransactionRepository,
    IYieldLocator yieldLocator,
    IPortfolioLocator portfolioLocator,
    IOperationLocator operationLocator,
    IMediator mediator) : ICommandHandler<GetAccountingFeesCommand, bool>
{
    public async Task<Result<bool>> Handle(GetAccountingFeesCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var yields = await yieldLocator.GetYieldsPortfolioIdsAndClosingDate(command.PortfolioIds, command.ProcessDate, cancellationToken);

            if (yields.IsFailure)
            {
                logger.LogError("No se pudieron obtener los yields para los portafolios: {Error}", yields.Error);
                return Result.Failure<bool>(yields.Error);
            }

            var accountingFeesResult = await CreateRange(yields.Value, command.ProcessDate, cancellationToken);

            if (!accountingFeesResult.IsSuccess)
                logger.LogError("Error al crear las entidades contables: {Error}", accountingFeesResult.Errors);                

            if (!accountingFeesResult.SuccessItems.Any())
            {
                logger.LogInformation("No accounting fees to process");
                return false;
            }

            return await mediator.Send(new AddAccountingEntitiesCommand(accountingFeesResult.SuccessItems), cancellationToken);

        } catch (Exception ex)
        {
            logger.LogError(ex, "Error handling GetAccountingFeesQuery");
            return false;
        }
    }

    private async Task<ProcessingResult<AccountingAssistant>> CreateRange(IEnumerable<YieldResponse> yields, DateTime processDate,
                                                                             CancellationToken cancellationToken)
    {
        var accountingAssistants = new List<AccountingAssistant>();
        var errors = new List<Error>();

        var operationType = await operationLocator.GetOperationTypeByNameAsync(OperationTypeNames.Commission, cancellationToken);

        if (operationType.IsFailure)
        {
            logger.LogWarning("No se pudo obtener el tipo de operación 'Comisión': {Error}", operationType.Error);
            errors.Add(operationType.Error);
            return new ProcessingResult<AccountingAssistant>(accountingAssistants, errors);
        }
       
        foreach (var yield in yields)
        {

            var passiveTransaction = await passiveTransactionRepository
                .GetByPortfolioIdAsync(yield.PortfolioId, cancellationToken);
            
            if (passiveTransaction == null)
            {
                logger.LogWarning("No se encontró una transacción pasiva para el portafolio {PortfolioId}", yield.PortfolioId);
                errors.Add(Error.NotFound(
                    "PassiveTransaction.NotFound",
                    $"No se encontró una transacción pasiva para el portafolio {yield.PortfolioId}"));
                continue;
            }

            var portfolioResult = await portfolioLocator
                .GetPortfolioInformationAsync(yield.PortfolioId, cancellationToken);

            if (portfolioResult.IsFailure)
            {
                logger.LogError("No se pudo obtener la información del portafolio {PortfolioId}: {Error}",
                    yield.PortfolioId, portfolioResult.Error);
                errors.Add(portfolioResult.Error);
                continue;
            }

            var accountingAssistant = AccountingAssistant.Create(
                portfolioResult.Value.NitApprovedPortfolio,
                portfolioResult.Value.VerificationDigit,
                portfolioResult.Value.Name,
                processDate.ToString("yyyyMM"),
                passiveTransaction?.ContraCreditAccount,
                processDate,
                operationType.Value.Name,
                yield.Commissions,
                operationType.Value.Nature                
            );

            if (accountingAssistant.IsFailure)
            {
                logger.LogError("Error al crear el AccountingAssistant para el portafolio {PortfolioId}: {Error}",
                    yield.PortfolioId, accountingAssistant.Error);
                errors.Add(accountingAssistant.Error);
                continue;
            }
            
            accountingAssistants.AddRange(accountingAssistant.Value.ToDebitAndCredit());
        }

        return new ProcessingResult<AccountingAssistant>(accountingAssistants, errors);
    }
}