namespace Products.IntegrationEvents.ObjectiveValidation;

public sealed record CheckObjectivesResponse(bool Succeeded, string? Code, string? Message);
