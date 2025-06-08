using Associate.Integrations.Activates;

namespace Associate.IntegrationEvents.ActivateValidation;

public sealed record GetActivateIdByIdentificationResponse(
    bool Succeeded,
    ActivateResponse? Activate,
    string? Code,
    string? Message);