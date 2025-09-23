using Common.SharedKernel.Application.Messaging;

namespace Associate.Integrations.Activates.GetActivateIds;

public sealed record GetIdentificationByActivateIdsRequestQuery(IEnumerable<int> ActivateIds) : IQuery<IReadOnlyCollection<GetIdentificationByActivateIdsResponse>>;