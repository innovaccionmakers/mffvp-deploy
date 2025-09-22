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
        await using var tx = await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var yields = await yieldLocator.GetYieldsPortfolioIdsAndClosingDate(request.PortfolioIds, request.ClosingDate, cancellationToken);            
            
            var accountingFees = await CreateRange(yields.Value, request.ClosingDate, cancellationToken);
            
            if (!accountingFees.Any())
            {
                logger.LogInformation("No accounting fees to process for closing date {ClosingDate}", request.ClosingDate);
                return false;
            }

            await accountingAssistantRepository.AddRangeAsync(accountingFees, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);
            return true;

        } catch (Exception ex)
        {
            logger.LogError(ex, "Error handling GetAccountingFeesQuery");
            return false;
        }
    }

    private async Task<IEnumerable<AccountingAssistant>> CreateRange(
        IEnumerable<YieldResponse> yields,
        DateTime closingDate,
        CancellationToken cancellationToken)
    {
        var accountingAssistants = new List<AccountingAssistant>();
        foreach (var yield in yields)
        {
            var accountingAssistant = AccountingAssistant.Create(
                "",
                0,
                "",
                "",
                "",
                new DateTime(),
                "",
                "",  
                "D",
                1,
                "",
                1
            );
            if(accountingAssistant.IsSuccess)
                accountingAssistants.Add(accountingAssistant.Value);
        }
        return accountingAssistants;
    }
}