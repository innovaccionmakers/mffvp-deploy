using Common.SharedKernel.Application.Messaging;

namespace Trusts.Integrations.CustomerDeals.GetCustomerDeal;

public sealed record GetCustomerDealQuery(
    Guid CustomerDealId
) : IQuery<CustomerDealResponse>;