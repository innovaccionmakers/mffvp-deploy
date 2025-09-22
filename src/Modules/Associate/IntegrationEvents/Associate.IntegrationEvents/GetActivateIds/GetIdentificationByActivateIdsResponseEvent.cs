using Associate.Integrations.Activates.GetActivateIds;
using Common.SharedKernel.Application.Attributes;

namespace Associate.IntegrationsEvents.GetActivateIds;

public sealed record GetIdentificationByActivateIdsResponseEvent(
        bool IsValid,
        string? Code,
        string? Message,
        IReadOnlyCollection<GetIdentificationByActivateIdsResponse> Indentifications
    );
