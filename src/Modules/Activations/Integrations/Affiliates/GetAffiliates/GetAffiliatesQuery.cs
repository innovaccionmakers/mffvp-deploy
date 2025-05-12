using Common.SharedKernel.Application.Messaging;

namespace Activations.Integrations.Affiliates.GetAffiliates;

public sealed record GetAffiliatesQuery : IQuery<IReadOnlyCollection<AffiliateResponse>>;