using Common.SharedKernel.Application.Messaging;

namespace Trusts.Integrations.CustomerDeals.DeleteCustomerDeal;

public sealed record DeleteCustomerDealCommand(
    Guid CustomerDealId
) : ICommand;