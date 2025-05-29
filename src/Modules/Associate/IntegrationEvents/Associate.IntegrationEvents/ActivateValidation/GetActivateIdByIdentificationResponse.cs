namespace Associate.IntegrationEvents.ActivateValidation;

public sealed record GetActivateIdByIdentificationResponse(
    bool   Succeeded,
    int?   ActivateId,
    string? Code,
    string? Message);