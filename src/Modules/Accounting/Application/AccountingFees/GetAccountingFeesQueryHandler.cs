using Accounting.Application.Abstractions.Data;
using Accounting.Application.Abstractions.External;
using Accounting.Domain.PassiveTransactions;
using Accounting.Integrations.AccountingFees;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;

namespace Accounting.Application.AccountingFees;

internal sealed class GetAccountingFeesQueryHandler(
    ILogger<GetAccountingFeesQueryHandler> logger,
    IPassiveTransactionRepository passiveTransactionRepository,
    IUnitOfWork unitOfWork,
    IYieldLocator yieldLocator,
    IPortfolioLocator portfolioLocator) : IQueryHandler<GetAccountingFeesQuery, bool>
{
    public async Task<Result<bool>> Handle(GetAccountingFeesQuery request, CancellationToken cancellationToken)
    {
        var yieldsResult = await yieldLocator.GetYieldsPortfolioIdsAndClosingDate(request.PortfolioIds, request.ClosingDate, cancellationToken);
        var portfoliosResult = await portfolioLocator.GetPortfolioInformationAsync(5, cancellationToken);
        return true;
    }
}
