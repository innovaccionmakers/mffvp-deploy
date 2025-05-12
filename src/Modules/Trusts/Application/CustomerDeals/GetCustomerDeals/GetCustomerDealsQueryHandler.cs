using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Trusts.Domain.CustomerDeals;
using Trusts.Integrations.CustomerDeals;
using Trusts.Integrations.CustomerDeals.GetCustomerDeals;

namespace Trusts.Application.CustomerDeals.GetCustomerDeals;

internal sealed class GetCustomerDealsQueryHandler(
    ICustomerDealRepository customerdealRepository)
    : IQueryHandler<GetCustomerDealsQuery, IReadOnlyCollection<CustomerDealResponse>>
{
    public async Task<Result<IReadOnlyCollection<CustomerDealResponse>>> Handle(GetCustomerDealsQuery request,
        CancellationToken cancellationToken)
    {
        var entities = await customerdealRepository.GetAllAsync(cancellationToken);

        var response = entities
            .Select(e => new CustomerDealResponse(
                e.CustomerDealId,
                e.Date,
                e.AffiliateId,
                e.ObjectiveId,
                e.PortfolioId,
                e.ConfigurationParamId,
                e.Amount))
            .ToList();

        return Result.Success<IReadOnlyCollection<CustomerDealResponse>>(response);
    }
}