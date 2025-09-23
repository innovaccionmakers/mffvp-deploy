using Accounting.Application.Abstractions.Data;
using Accounting.Application.Abstractions.External;
using Accounting.Domain.AccountingAssistants;
using Accounting.Domain.PassiveTransactions;
using Accounting.Integrations.AccountingAssistants.Commands;
using Accounting.Integrations.AccountingFees;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Accounting.Application.AccountingFees.Queries;

internal sealed class GetAccountingFeesQueryHandler(
    ILogger<GetAccountingFeesQueryHandler> logger,
    IPassiveTransactionRepository passiveTransactionRepository,
    IYieldLocator yieldLocator,
    IPortfolioLocator portfolioLocator,
    IMediator mediator) : IQueryHandler<GetAccountingFeesQuery, bool>
{
    public async Task<Result<bool>> Handle(GetAccountingFeesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var yields = await yieldLocator.GetYieldsPortfolioIdsAndClosingDate(request.PortfolioIds, request.ClosingDate, cancellationToken);

            if (yields.IsFailure)
            {
                logger.LogError("No se pudieron obtener los yields para los portafolios: {Error}", yields.Error);
                return Result.Failure<bool>(yields.Error);
            }

            var accountingFeesResult = await CreateRange(yields.Value, cancellationToken);

            if (accountingFeesResult.IsFailure)
            {
                logger.LogError("Error al crear las entidades contables: {Error}", accountingFeesResult.Error);
                return Result.Failure<bool>(accountingFeesResult.Error);
            }

            if (!accountingFeesResult.Value.Any())
            {
                logger.LogInformation("No accounting fees to process");
                return false;
            }

            return await mediator.Send(new AddAccountingEntitiesCommand(accountingFeesResult.Value), cancellationToken);

        } catch (Exception ex)
        {
            logger.LogError(ex, "Error handling GetAccountingFeesQuery");
            return false;
        }
    }

    private async Task<Result<IEnumerable<AccountingAssistant>>> CreateRange(
    IEnumerable<YieldResponse> yields,
    CancellationToken cancellationToken)
    {
        var accountingAssistants = new List<AccountingAssistant>();

        foreach (var yield in yields)
        {
            var passiveTransaction = await passiveTransactionRepository
                .GetByPortfolioIdAsync(yield.PortfolioId, cancellationToken);

            var portfolioResult = await portfolioLocator
                .GetPortfolioInformationAsync(yield.PortfolioId, cancellationToken);

            if (portfolioResult.IsFailure)
            {
                logger.LogError("No se pudo obtener la información del portafolio {PortfolioId}: {Error}",
                    yield.PortfolioId, portfolioResult.Error);
                return Result.Failure<IEnumerable<AccountingAssistant>>(portfolioResult.Error);
            }

            var accountingAssistant = AccountingAssistant.Create(
                portfolioResult.Value.NitApprovedPortfolio,
                portfolioResult.Value.VerificationDigit,
                portfolioResult.Value.Name,
                DateTime.UtcNow.ToString("yyyyMM"),
                passiveTransaction.ContraCreditAccount,
                DateTime.UtcNow,
                "",
                "",
                "",
                1,
                "",
                1
            );

            if (accountingAssistant.IsSuccess)
                accountingAssistants.AddRange(accountingAssistant.Value.ToDebitAndCredit());
        }

        return accountingAssistants;
    }
}