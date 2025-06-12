namespace Customers.IntegrationEvents.DocumentTypeValidation;

public sealed record GetDocumentTypeIdByCodeResponse(
    bool Succeeded,
    int? DocumentTypeId,
    string? Code,
    string? Message);