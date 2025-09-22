using Common.SharedKernel.Application.Attributes;

namespace Associate.Integrations.Activates.GetActivateIds;

public sealed record GetIdentificationByActivateIdsResponse(
    int ActivateIds,
    string Identification
    );
