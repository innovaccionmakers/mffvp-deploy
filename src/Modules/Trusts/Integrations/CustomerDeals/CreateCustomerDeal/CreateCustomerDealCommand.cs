using Common.SharedKernel.Application.Messaging;

namespace Trusts.Integrations.CustomerDeals.CreateCustomerDeal;

public sealed record CreateCustomerDealCommand(
    DateTime Date,
    int AffiliateId,
    int ObjectiveId,
    int PortfolioId,
    int ConfigurationParamId,
    decimal Amount
) : ICommand<CustomerDealResponse>;