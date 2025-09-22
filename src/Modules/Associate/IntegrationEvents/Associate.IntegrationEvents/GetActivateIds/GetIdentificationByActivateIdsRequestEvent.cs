using Common.SharedKernel.Application.Messaging;

namespace Associate.IntegrationsEvents.GetActivateIds;

public sealed record GetIdentificationByActivateIdsRequestEvent(
    IEnumerable<int> AffiliateIds
    );