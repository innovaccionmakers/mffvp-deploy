using Common.SharedKernel.Application.Messaging;

namespace Trusts.Integrations.CustomerDeals.UpdateCustomerDeal;

public sealed record UpdateCustomerDealCommand(
    Guid CustomerDealId,
    DateTime NewDate,
    int NewAffiliateId,
    int NewObjectiveId,
    int NewPortfolioId,
    int NewConfigurationParamId,
    decimal NewAmount
) : ICommand<CustomerDealResponse>;