using Common.SharedKernel.Application.Messaging;

namespace Trusts.Integrations.CustomerDeals.GetCustomerDeals;

public sealed record GetCustomerDealsQuery : IQuery<IReadOnlyCollection<CustomerDealResponse>>;