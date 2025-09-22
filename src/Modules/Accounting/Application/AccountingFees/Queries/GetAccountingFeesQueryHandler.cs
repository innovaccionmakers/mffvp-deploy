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
            
            var accountingFees = await CreateRange(yields.Value, cancellationToken);
            
            if (!accountingFees.Any())
            {
                logger.LogInformation("No accounting fees to process");
                return false;
            }

            return await mediator.Send(new AddAccountingEntitiesCommand(accountingFees), cancellationToken);
            
        } catch (Exception ex)
        {
            logger.LogError(ex, "Error handling GetAccountingFeesQuery");
            return false;
        }
    }

    private async Task<IEnumerable<AccountingAssistant>> CreateRange(
    IEnumerable<YieldResponse> yields,
    CancellationToken cancellationToken)
    {
        var accountingAssistants = new List<AccountingAssistant>();

        foreach (var yield in yields)
        {
            var passiveTransaction = await passiveTransactionRepository
                .GetByPortfolioIdAsync(yield.PortfolioId, cancellationToken);

            var portfolioResult = (await portfolioLocator
                .GetPortfolioInformationAsync(yield.PortfolioId, cancellationToken))?.Value;

            var accountingAssistant = AccountingAssistant.Create(
                portfolioResult.NitApprovedPortfolio,
                portfolioResult.VerificationDigit,
                portfolioResult.Name,
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