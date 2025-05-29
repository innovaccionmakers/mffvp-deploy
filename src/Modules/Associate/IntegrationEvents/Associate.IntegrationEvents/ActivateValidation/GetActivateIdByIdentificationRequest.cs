namespace Associate.IntegrationEvents.ActivateValidation;

public record GetActivateIdByIdentificationRequest(
    string IdentificationType,
    string Identification);
