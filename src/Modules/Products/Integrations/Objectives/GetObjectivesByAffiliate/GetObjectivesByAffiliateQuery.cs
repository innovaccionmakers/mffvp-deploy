using Common.SharedKernel.Application.Messaging;

namespace Products.Integrations.Objectives.GetObjectivesByAffiliate;

public sealed record GetObjectivesByAffiliateQuery(
    int AffiliateId
) : IQuery<IReadOnlyCollection<AffiliateObjectiveQueryResponse>>;