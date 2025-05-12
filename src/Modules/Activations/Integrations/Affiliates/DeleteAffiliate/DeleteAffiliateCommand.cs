using Common.SharedKernel.Application.Messaging;

namespace Activations.Integrations.Affiliates.DeleteAffiliate;

public sealed record DeleteAffiliateCommand(
    int AffiliateId
) : ICommand;