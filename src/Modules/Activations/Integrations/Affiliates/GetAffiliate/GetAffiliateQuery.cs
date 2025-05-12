using Common.SharedKernel.Application.Messaging;

namespace Activations.Integrations.Affiliates.GetAffiliate;

public sealed record GetAffiliateQuery(
    int AffiliateId
) : IQuery<AffiliateResponse>;