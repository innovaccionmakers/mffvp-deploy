using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Trusts.Domain.CustomerDeals;
using Trusts.Integrations.CustomerDeals;
using Trusts.Integrations.CustomerDeals.GetCustomerDeal;

namespace Trusts.Application.CustomerDeals.GetCustomerDeal;

internal sealed class GetCustomerDealQueryHandler(
    ICustomerDealRepository customerdealRepository)
    : IQueryHandler<GetCustomerDealQuery, CustomerDealResponse>
{
    public async Task<Result<CustomerDealResponse>> Handle(GetCustomerDealQuery request,
        CancellationToken cancellationToken)
    {
        var customerdeal = await customerdealRepository.GetAsync(request.CustomerDealId, cancellationToken);
        if (customerdeal is null)
            return Result.Failure<CustomerDealResponse>(CustomerDealErrors.NotFound(request.CustomerDealId));
        var response = new CustomerDealResponse(
            customerdeal.CustomerDealId,
            customerdeal.Date,
            customerdeal.AffiliateId,
            customerdeal.ObjectiveId,
            customerdeal.PortfolioId,
            customerdeal.ConfigurationParamId,
            customerdeal.Amount
        );
        return response;
    }
}