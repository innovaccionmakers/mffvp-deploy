
using Accounting.Integrations.AccountingFees;
using MediatR;

namespace Accounting.Presentation.GraphQL;

public class AccountingExperienceQueries(IMediator mediator) : IAccountingExperienceQueries
{
    public async Task<string> GetAccountingFeesAsync(List<int> portfolioIds, DateTime closingDate, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetAccountingFeesQuery(portfolioIds, closingDate), cancellationToken);

        return "";
    }
}
