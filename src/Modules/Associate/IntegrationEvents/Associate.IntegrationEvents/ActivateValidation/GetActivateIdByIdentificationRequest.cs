namespace Associate.IntegrationEvents.ActivateValidation;

public record GetActivateIdByIdentificationRequest(
    string DocumentType,
    string Identification);
