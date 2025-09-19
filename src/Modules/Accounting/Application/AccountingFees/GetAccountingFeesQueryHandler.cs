using Accounting.Application.Abstractions.Data;
using Accounting.Application.Abstractions.External;
using Accounting.Domain.AccountingAssistants;
using Accounting.Domain.PassiveTransactions;
using Accounting.Integrations.AccountingFees;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;

namespace Accounting.Application.AccountingFees;

internal sealed class GetAccountingFeesQueryHandler(
    ILogger<GetAccountingFeesQueryHandler> logger,
    IAccountingAssistantRepository accountingAssistantRepository,
    IPassiveTransactionRepository passiveTransactionRepository,
    IUnitOfWork unitOfWork,
    IYieldLocator yieldLocator,
    IPortfolioLocator portfolioLocator) : IQueryHandler<GetAccountingFeesQuery, bool>
{
    public async Task<Result<bool>> Handle(GetAccountingFeesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var yields = (await yieldLocator.GetYieldsPortfolioIdsAndClosingDate(request.PortfolioIds, request.ClosingDate, cancellationToken)).Value;            
            var accountingAssintantList = new List<AccountingAssistant>();

            foreach (var yield in yields)
            {

                var portfolio = (await portfolioLocator.GetPortfolioInformationAsync(yield.PortfolioId, cancellationToken)).Value;
                var passiveTransaction = await passiveTransactionRepository.GetByPortfolioIdAsync(yield.PortfolioId, cancellationToken);
                var accountAssitant = AccountingAssistant.Create(
                    portfolio.NitApprovedPortfolio,
                    portfolio.VerificationDigit,
                    portfolio.Name,
                    passiveTransaction.DebitAccount,
                    new DateTime(),
                    portfolio.IdentificationType,
                    "",
                    "",
                    "D",
                    "",
                    1
                ); 
                accountingAssintantList.Add(accountAssitant.Value);
            }
            return true;
        } catch (Exception ex)
        {
            logger.LogError(ex, "Error handling GetAccountingFeesQuery");
            return false;
        }
    }
}